using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace UnityReactIcons
{
    public class FontsWindow : EditorWindow
    {
        private int rowCount = 2;
        public static Dictionary<string, object> cache = new();
        private static readonly string directoryPath = Path.Combine("Library/unity-tailwindcss");
        private static readonly string filePath = Path.Combine(directoryPath, "fonts_cache.json");
        private const string endpointUrl = "https://unity-react-icons-backend.vercel.app";
        private const string developmentEndpointUrl = "http://localhost:3000";
        private string fontListUrl = endpointUrl + "/font";
        private string fontPreviewUrl = endpointUrl + "/font-preview";
        private ListView iconPacksListView;
        // private VisualElement iconsListView;
        private ListView iconsListView;
        private VisualElement detailPane;
        private Image image;
        private Button importButton, viewFontButton, updateTMPButton;
        private Label iconDetailsLabel;
        private TextField searchField;
        private TextField searchFieldPack;
        private Toggle createTMPFont;
        // private DropdownField svgType, assetType;
        private ToolbarMenu menu;
        private List<List<string>> iconsListGrid = new();
        private List<FontPack> allIconPack = new();
        private List<string> iconsList = new();
        private List<WebFont> allFonts = new();
        private FontPack currentIconPack = null;

        [MenuItem("Window/Fonts Window")]
        public static void ShowExample()
        {
            FontsWindow wnd = GetWindow<FontsWindow>();
            wnd.titleContent = new GUIContent("Fonts Window");
            // set the window size
            wnd.minSize = new Vector2(800, 600);
        }

        private void UpdateEndPointsTarget(string endpointUrl)
        {
            fontListUrl = endpointUrl + "/list-icons";
            fontPreviewUrl = endpointUrl + "/icon";
        }

        private void LoadCache()
        {
            var projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + '/';

            if (File.Exists(projectPath + filePath))
            {
                string json = File.ReadAllText(projectPath + filePath);
                SerializableDictionaryWrapper wrapper = JsonUtility.FromJson<SerializableDictionaryWrapper>(json);
                cache = wrapper.ToDictionary();
            }
            else
            {
                cache = new Dictionary<string, object>();
            }
        }

        private void SaveCache()
        {
            if (cache.Count == 0) return;

            SerializableDictionaryWrapper wrapper = new SerializableDictionaryWrapper();
            foreach (KeyValuePair<string, object> kvp in cache)
            {
                if (kvp.Value is not string) continue;
                wrapper.TryAdd(kvp.Key, kvp.Value.ToString()); // Assuming that the object can be represented as string
            }

            string json = JsonUtility.ToJson(wrapper);

            var projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + '/';

            // Create directory if it does not exist
            Directory.CreateDirectory(projectPath + directoryPath);
            File.WriteAllText(projectPath + filePath, json);
        }

        private void OnDisable()
        {
            SaveCache();
        }

        private void OnDestroy()
        {
            // Debug.Log("Cleaning up...");
            // find all the object in the cache that have keys ends with _texture and destory it
            foreach (KeyValuePair<string, object> kvp in cache)
            {
                if (kvp.Key.EndsWith("_texture"))
                {
                    DestroyImmediate(kvp.Value as Texture2D);
                }
            }

            // clean up the cache 
            cache.Clear();
        }


        public void OnEnable()
        {
            // cache.Clear();
            LoadCache();

            // Reference to the root of the window.
            var root = rootVisualElement;

            // Load the UXML and USS
            // get relative path of this current c# file
            // var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            // // get directory path
            // path = path.Substring(0, path.LastIndexOf('/'));
            // Debug.Log(path);

            var csScriptPath = AssetDatabase.GUIDToAssetPath("41fa3e1e0abed411a87ebd84485ffb56");
            var csFileName = Path.GetFileNameWithoutExtension(csScriptPath);
            var csDirectory = Path.GetDirectoryName(csScriptPath);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(csDirectory + "/IconWindow.uxml");
            var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(csDirectory + "/IconWindow.uss");

            // Clone the visual tree and apply styles
            VisualElement labelFromUXML = visualTree.CloneTree().Children().First();
            labelFromUXML.styleSheets.Add(styles);
            root.Add(labelFromUXML);

            // Get elements
            iconPacksListView = root.Q<ListView>("iconPacksListView");
            // iconsListView = root.Q<VisualElement>("iconsListView");
            iconsListView = root.Q<ListView>("iconsListView");
            searchField = root.Q<ToolbarSearchField>("searchField").Q<TextField>();
            searchFieldPack = root.Q<ToolbarSearchField>("searchFieldPack").Q<TextField>();
            detailPane = root.Q<VisualElement>("detailPane");
            image = root.Q<Image>("image");
            // svgType = root.Q<DropdownField>("svgType");
            // assetType = root.Q<DropdownField>("assetType");
            menu = root.Q<ToolbarMenu>("menu");
            createTMPFont = root.Q<Toggle>("createTMPFont");

            menu.menu.AppendAction("Icon Server/Default", a =>
            {
                UpdateEndPointsTarget(endpointUrl);
            }, a => fontListUrl.StartsWith(endpointUrl) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            menu.menu.AppendAction("Icon Server/Development", a =>
            {
                UpdateEndPointsTarget(developmentEndpointUrl);
            }, a => fontListUrl.StartsWith(developmentEndpointUrl) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            // Configure ListViews and search field
            iconPacksListView.makeItem = MakeIconPackItem;
            iconPacksListView.bindItem = BindIconPackItem;
            iconPacksListView.onSelectionChange += IconPacksSelectionChange;

            iconDetailsLabel = root.Q<Label>("iconDetailsLabel");
            importButton = root.Q<Button>(nameof(importButton));
            viewFontButton = root.Q<Button>(nameof(viewFontButton));
            updateTMPButton = root.Q<Button>(nameof(updateTMPButton));

            viewFontButton.RegisterCallback<ClickEvent>(
                (evt) =>
                {
                    if (currentIconPack == null) return;
                    Application.OpenURL("https://fonts.google.com/specimen/" + Uri.EscapeDataString(lastSelectIconId));
                }
            );

            // Find all TextMeshPro components in the scene and update them with the new font
            updateTMPButton.RegisterCallback<ClickEvent>(
                (evt) =>
                {
                    if (currentIconPack == null) return;
                    var tmps = FindObjectsOfType<TextMeshProUGUI>();
                    var path = "Assets/Fonts/" + lastSelectIconId + " SDF.asset";
                    // var font = Resources.Load<TMP_FontAsset>(path);
                    var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);

                    if (font == null)
                    {
                        Debug.LogError(lastSelectIconId + " font not found in Fonts folder, please click import first. Path: " + path);
                        return;
                    }

                    Undo.RecordObjects(
                        tmps,
                        "Update TMP Font"
                    );

                    Debug.Log("Updating " + tmps.Length + " TMP components with " + lastSelectIconId + " font.");
                    foreach (var tmp in tmps)
                        tmp.font = font;
                }
            );

            importButton.RegisterCallback<ClickEvent>(
                async (evt) =>
                {
                    if (lastSelectIconId == null) return;

                    string path = Application.dataPath + "/Fonts";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path = path + "/" + lastSelectIconId;

                    var webFont = allFonts.Find(x => x.family == lastSelectIconId);

                    var regularFont = webFont.files.Find(x => x.fontType == "regular");

                    importButton.text = "Loading...";
                    importButton.SetEnabled(false);

                    var fontFile = await GetFontFile(regularFont.url);

                    path += ".ttf";
                    File.WriteAllBytes(path, fontFile);

                    var finalPath = path.Replace(Application.dataPath, "Assets");

                    AssetDatabase.ImportAsset(finalPath, ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.Refresh();

                    if (createTMPFont.value)
                    {
                        Selection.objects = new[] {
                            AssetDatabase.LoadAssetAtPath<Font>(finalPath)
                        };
                        EditorApplication.ExecuteMenuItem("Assets/Create/TextMeshPro/Font Asset");
                    }

                    importButton.text = "Import";
                    importButton.SetEnabled(true);

                }
            );

            SetGridMode(true);

            searchField.RegisterValueChangedCallback(SearchValueChanged);
            searchFieldPack.RegisterValueChangedCallback(SearchValueChangedPack);

            // Start the process
            GetIconPacksAsync().ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception);
                    return;
                }
                allIconPack = task.Result;
                iconPacksListView.itemsSource = task.Result;
                iconPacksListView.selectedIndex = 0;
                iconPacksListView.RefreshItems();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void IconSelectionChange(IEnumerable<object> selectedItems)
        {
            var iconId = selectedItems.First() as string;
            var iconPackId = currentIconPack.id;
            var iconResponse = await GetIconAsync(iconPackId, iconId);
            LoadIconIntoImage(image, iconId, iconResponse);

            var webFont = allFonts.Find(x => x.family == lastSelectIconId);
            iconDetailsLabel.text = $"<b><size=120%><voffset=0.5em>{webFont.family}</voffset=0.5em></size=11></b><br>Pack: {currentIconPack.name}<br>Font Weight: {webFont.variants.OrderBy(q => q).Aggregate("", (x, y) => x.Length == 0 ? y : (x + " | " + y))}<br>Project Url: {GetProjectUrl()}";
        }

        private void LoadIconIntoImage(Image image, string iconId, FontDetailResponse iconResponse)
        {
            if (image == null) return;
            if (iconResponse == null) return;
            // if (iconResponse.base64ImagePreview == null) return;

            if (cache.TryGetValue(iconId + "_texture", out var ttex))
            {
                image.image = ttex as Texture2D;
                return;
            }

            byte[] imageBytes = Convert.FromBase64String(iconResponse.previews[0]);
            Texture2D tex = new Texture2D(2, 2);
            tex.hideFlags = HideFlags.DontSaveInEditor;
            if (tex.LoadImage(imageBytes))
                image.image = tex;

            image.scaleMode = ScaleMode.ScaleToFit;

            cache[iconId + "_texture"] = tex;
        }


        private string GetProjectUrl()
        {
#if UNITY_2022_2_OR_NEWER
            return $"<a href=\"{currentIconPack.projectUrl}\">{currentIconPack.projectUrl}</a>";
#endif
            return currentIconPack.projectUrl;
        }

        private void MakeGrid(List<string> iconsList)
        {
            iconsListGrid = new List<List<string>>();

            List<string> currentList = new List<string>();
            for (int i = 0; i < iconsList.Count; i++)
            {
                if (i % rowCount == 0)
                {
                    currentList = new List<string>();
                    iconsListGrid.Add(currentList);
                }
                currentList.Add(iconsList[i]);
            }
            iconsListView.itemsSource = iconsListGrid;
            iconsListView.RefreshItems();
        }

        private void SetGridMode(bool on)
        {
            if (!on)
            {
                iconsListView.makeItem = MakeIconItem;
                iconsListView.bindItem = BindIconItem;
            }
            else
            {
                iconsListView.makeItem = MakeIconItemGrid;
                iconsListView.bindItem = BindIconItemGrid;
            }
        }

        // List Item Creation and Binding for Icon Packs
        private VisualElement MakeIconPackItem()
        {
            var label = new Label();
            label.AddToClassList("iconPackLabel");
            return label;
        }

        private void BindIconPackItem(VisualElement element, int index)
        {
            var pack = iconPacksListView.itemsSource[index] as FontPack;
            (element as Label).text = $"{pack.name}";
        }

        private async void IconPacksSelectionChange(IEnumerable<object> selectedItems)
        {
            iconsListView.itemsSource = new object[0];
            iconsListView.RefreshItems();

            var item = selectedItems.First();

            currentIconPack = item as FontPack;
            var iconResponse = await GetIconsAsync(currentIconPack.id);

            iconsList = new List<string>(iconResponse.items.Where(x => !x.family.Contains("Material Icons")).Select(x => x.family));
            allFonts = iconResponse.items;
            MakeGrid(iconsList);
            SetGridMode(true);
        }

        private VisualElement lastSelectedItem = null;
        private string lastSelectIconId = null;

        private VisualElement MakeIconItemGrid()
        {
            VisualElement Build()
            {
                var iconItem = new VisualElement();
                iconItem.AddToClassList("iconItem");
                iconItem.AddToClassList("unity-list-view__item");
                iconItem.AddToClassList("unity-collection-view__item");

                var iconImage = new Image();

                var iconLabel = new Label();

                iconItem.Add(iconImage);
                iconItem.Add(iconLabel);

                iconItem.RegisterCallback<ClickEvent>(evt =>
                {
                    lastSelectedItem?.RemoveFromClassList("checked");
                    lastSelectedItem = iconItem;
                    lastSelectIconId = iconLabel.text;
                    IconSelectionChange(new List<object> { iconLabel.text });
                    iconItem.AddToClassList("checked");
                });

                return iconItem;
            }

            // build rowCount

            var container = new VisualElement();
            container.AddToClassList("iconItemContainer");

            for (int i = 0; i < rowCount; i++)
            {
                var item = Build();
                container.Add(item);
            }

            return container;
        }

        // List Item Creation and Binding for Icons
        private VisualElement MakeIconItem()
        {
            var label = new Label();
            label.AddToClassList("iconPackLabel");
            return label;
        }

        private void BindIconItemGrid(VisualElement e, int index)
        {
            for (int i = 0; i < rowCount; i++)
            {
                var element = e.Children().ElementAt(i);
                var label = element.Q<Label>();
                var image = element.Q<Image>();


                if (index >= iconsListGrid.Count || i >= iconsListGrid[index].Count) continue;

                var target = iconsListGrid[index][i];
                label.text = target;

                element.style.opacity = 0;
                GetIconAsync(currentIconPack.id, label.text).ContinueWith(response =>
                {
                    if (label.text != target) return;

                    LoadIconIntoImage(image, label.text, response.Result);
                    element.style.opacity = 1;

                    if (lastSelectIconId == label.text)
                    {
                        lastSelectedItem = element;
                        element.AddToClassList("checked");
                    }
                    else
                    {
                        element.RemoveFromClassList("checked");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void BindIconItem(VisualElement element, int index)
        {
            // (element as Label).text = iconsList[index];
            (element as Label).text = iconsListGrid[index / rowCount][index % rowCount];
        }

        // Search field value change callback
        private void SearchValueChangedPack(ChangeEvent<string> evt)
        {
            // iconPacksListView.itemsSource = allIconPack.FindAll(pack => pack.name.ToLower().Contains(evt.newValue.ToLower()));
            iconPacksListView.itemsSource = allIconPack.FindAll(pack => pack.id.ToLower().Contains(evt.newValue.ToLower()));
            iconPacksListView.RefreshItems();
        }

        // Search field value change callback
        private void SearchValueChanged(ChangeEvent<string> evt)
        {
            // Implement fuzzy search algorithm here
            // For example, using a basic string.Contains search
            // if (iconsList != null)
            // {
            //     iconsListView.itemsSource = iconsList.FindAll(icon => icon.Contains(evt.newValue));
            //     // var list = iconsList.FindAll(icon => icon.Contains(evt.newValue));
            //     // SetGridMode(true, list);
            // }

            if (iconsListGrid != null)
            {
                MakeGrid(iconsList.FindAll(icon => icon.ToLower().Contains(evt.newValue.ToLower())));
                // var list = iconsList.FindAll(icon => icon.Contains(evt.newValue));
                // SetGridMode(true, list);
            }
        }

        // Task for Getting Icon Packs
        private async Task<List<FontPack>> GetIconPacksAsync()
        {
            return await Task.Run(() =>
            {
                List<FontPack> iconPacks = new() {
                    new FontPack()
                    {
                        id = "google",
                        name = "Google Fonts",
                        projectUrl = "https://fonts.google.com"
                    }
                };
                return iconPacks;
            });
        }

        private async Task<byte[]> GetFontFile(string downloadUrl)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(downloadUrl))
            {
                webRequest.method = "GET";
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    return webRequest.downloadHandler.data;
                }
                else
                {
                    Debug.Log(": Error: " + webRequest.error);
                    return null;
                }
            }
        }

        // Task for Getting Icons
        private async Task<FontResponse> GetIconsAsync(string iconPackId)
        {
            // IconPackRequest iconPackRequest = new IconPackRequest() { iconPackId = iconPackId };

            // string json = JsonUtility.ToJson(iconPackRequest);

            if (cache.ContainsKey(iconPackId))
                return JsonUtility.FromJson<FontResponse>(cache[iconPackId] as string);

            // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(fontListUrl))
            {
                webRequest.method = "GET";
                webRequest.SetRequestHeader("Content-Type", "application/json");
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    FontResponse iconResponse = JsonUtility.FromJson<FontResponse>(webRequest.downloadHandler.text);
                    cache[iconPackId] = webRequest.downloadHandler.text;
                    return iconResponse;
                }
                else
                {
                    Debug.Log(": Error: " + webRequest.error);
                    return null;
                }
            }
        }

        private async Task<FontDetailResponse> GetIconAsync(string iconPackId, string iconId)
        {
            // IconPackRequest iconPackRequest = new IconPackRequest()
            // {
            //     iconPackId = iconPackId,
            //     iconId = iconId
            // };

            if (cache.TryGetValue(iconId, out var res))
                return JsonUtility.FromJson<FontDetailResponse>(res as string);

            // string json = JsonUtility.ToJson(iconPackRequest);
            // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(fontPreviewUrl + "?family=" + iconId))
            {
                webRequest.method = "GET";
                webRequest.SetRequestHeader("Content-Type", "application/json");
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    FontDetailResponse iconResponse = JsonUtility.FromJson<FontDetailResponse>(webRequest.downloadHandler.text);
                    cache[iconId] = webRequest.downloadHandler.text;
                    return iconResponse;
                }
                else
                {
                    // Debug.Log(": Error: " + webRequest.error);
                    return null;
                }
            }
        }
    }

    [Serializable]
    public class WebFont
    {
        public string family;
        public List<string> variants;
        public List<string> subsets;
        public string version;
        public string lastModified;
        public List<FontFile> files;
        public string category;
        public string kind;
        public string menu;
    }

    [Serializable]
    public class FontResponse
    {
        public string kind;
        public List<WebFont> items;
    }

}

[System.Serializable]
public class FontPack
{
    public string id;
    public string name;
    public string projectUrl;
    public string license;
    public string licenseUrl;
}

[System.Serializable]
public class FontDetailResponse
{
    public string[] previews;
}


[System.Serializable]
public class FontFile
{
    public string fontType;

    public string url;
}
