using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VectorGraphics.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace UnityReactIcons
{
    public class IconWindow : EditorWindow
    {
        public static Dictionary<string, object> cache = new();
        private static readonly string directoryPath = Path.Combine("Library/unity-tailwindcss");
        private static readonly string filePath = Path.Combine(directoryPath, "icons_cache.json");
        private const string endpointUrl = "https://unity-react-icons-backend.vercel.app";
        private const string developmentEndpointUrl = "http://localhost:3000";
        private string iconsListUrl = endpointUrl + "/list-icons";
        private string iconPackUrl = endpointUrl + "/icon";
        private ListView iconPacksListView;
        // private VisualElement iconsListView;
        private ListView iconsListView;
        private VisualElement detailPane;
        private Image image;
        private Button importButton;
        private Label iconDetailsLabel;
        private TextField searchField;
        private TextField searchFieldPack;
        private DropdownField svgType, assetType;
        private ToolbarMenu menu;
        private List<List<string>> iconsListGrid = new();
        private List<string> iconsList = new List<string>();
        private List<IconPack> allIconPack = new();
        private IconPack currentIconPack = null;

        [MenuItem("Window/Icon Window")]
        public static void ShowExample()
        {
            IconWindow wnd = GetWindow<IconWindow>();
            wnd.titleContent = new GUIContent("Icon Window");
        }

        private void UpdateEndPointsTarget(string endpointUrl)
        {
            iconsListUrl = endpointUrl + "/list-icons";
            iconPackUrl = endpointUrl + "/icon";
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

            // Debug.Log(projectPath + filePath);

            File.WriteAllText(projectPath + filePath, json);
        }

        private void OnDisable()
        {
            SaveCache();
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

            var csScriptPath = AssetDatabase.GUIDToAssetPath("fe8f7a2ffd8b54d3691c5e5503861f1e");
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
            svgType = root.Q<DropdownField>("svgType");
            assetType = root.Q<DropdownField>("assetType");
            menu = root.Q<ToolbarMenu>("menu");

            menu.menu.AppendAction("Icon Server/Default", a =>
            {
                UpdateEndPointsTarget(endpointUrl);
            }, a => iconsListUrl.StartsWith(endpointUrl) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            menu.menu.AppendAction("Icon Server/Development", a =>
            {
                UpdateEndPointsTarget(developmentEndpointUrl);
            }, a => iconsListUrl.StartsWith(developmentEndpointUrl) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            // Configure ListViews and search field
            iconPacksListView.makeItem = MakeIconPackItem;
            iconPacksListView.bindItem = BindIconPackItem;
            iconPacksListView.onSelectionChange += IconPacksSelectionChange;

            iconDetailsLabel = root.Q<Label>("iconDetailsLabel");
            importButton = root.Q<Button>("importButton");

            importButton.RegisterCallback<ClickEvent>(
                (evt) =>
                {
                    if (lastSelectIconId == null) return;

                    var requestItem = JsonUtility.FromJson<IconDetailResponse>(cache[lastSelectIconId] as string);

                    // create an svg file under Assets/Icons
                    string path = Application.dataPath + "/Icons";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path = path + "/" + requestItem.iconId;

                    if (assetType.value == "SVG")
                    {
                        path += ".svg";

                        File.WriteAllText(path, requestItem
                            .svg.Replace("stroke=\"currentColor\"", "stroke=\"white\"")
                            .Replace("fill=\"currentColor\"", "fill=\"white\"")
                        );

                        AssetDatabase.ImportAsset(path.Replace(Application.dataPath, "Assets"), ImportAssetOptions.ForceSynchronousImport);

                        SVGImporter svgImporter = (SVGImporter)SVGImporter.GetAtPath(path.Replace(Application.dataPath, "Assets"));
                        svgImporter.SvgType = (SVGType)svgType.index;
                        EditorUtility.SetDirty(svgImporter);
                        svgImporter.SaveAndReimport();

                        if (svgImporter.SvgType == SVGType.UIToolkit)
                        {
                            var size = "100%";
                            var code = @$"
<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements""
    xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements""
    editor=""UnityEditor.UIElements"">
    <ui:Image
        style=""--unity-image: url(&apos;{requestItem.iconId}.svg?&amp;guid={AssetDatabase.AssetPathToGUID(path.Replace(Application.dataPath, "Assets"))}&amp;type=3&apos;); width: {size}; height: {size};"" />
</ui:UXML>
                            ";

                            File.WriteAllText(
                                path.Substring(0, path.LastIndexOf('/')) + '/' + requestItem.iconId + ".uxml"
                                , code);
                        }
                    }
                    else
                    {
                        path += ".png";
                        // convert svg to png
                        byte[] imageBytes = Convert.FromBase64String(requestItem.base64ImagePreview);
                        File.WriteAllBytes(path, imageBytes);

                        AssetDatabase.ImportAsset(path.Replace(Application.dataPath, "Assets"), ImportAssetOptions.ForceSynchronousImport);

                        TextureImporter textureImporter = TextureImporter.GetAtPath(path.Replace(Application.dataPath, "Assets")) as TextureImporter;
                        textureImporter.textureType = TextureImporterType.Sprite;
                        EditorUtility.SetDirty(textureImporter);
                        textureImporter.SaveAndReimport();
                    }

                    UnityEditor.AssetDatabase.Refresh();
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
                allIconPack = task.Result.packs;
                iconPacksListView.itemsSource = task.Result.packs;
                iconPacksListView.selectedIndex = 0;
                iconPacksListView.RefreshItems();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void IconSelectionChange(IEnumerable<object> selectedItems)
        {
            var iconId = selectedItems.First() as string;
            var iconPackId = currentIconPack.id;
            var iconResponse = await GetIconAsync(iconPackId, iconId);
            SetupDetailPane(iconResponse);
        }

        private void LoadIconIntoImage(Image image, IconDetailResponse iconResponse)
        {
            if (image == null) return;
            if (iconResponse == null) return;
            if (iconResponse.base64ImagePreview == null) return;

            if (cache.TryGetValue(iconResponse.iconId + "_texture", out var ttex))
            {
                image.image = ttex as Texture2D;
                return;
            }

            byte[] imageBytes = Convert.FromBase64String(iconResponse.base64ImagePreview);
            Texture2D tex = new Texture2D(2, 2);
            tex.hideFlags = HideFlags.DontSaveInEditor;
            if (tex.LoadImage(imageBytes))
                image.image = tex;

            cache[iconResponse.iconId + "_texture"] = tex;
        }

        private void SetupDetailPane(IconDetailResponse iconDetail)
        {
            LoadIconIntoImage(image, iconDetail);
            iconDetailsLabel.text = $"<b><size=120%><voffset=0.5em>{iconDetail.iconId}</voffset=0.5em></size=11></b><br>Pack: {currentIconPack.name}<br>License: {currentIconPack.license}<br>Project Url: {GetProjectUrl()}";
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
                if (i % 4 == 0)
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
            // , List<string> iconList = null
            // if (iconList == null) iconList = iconsList;

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
            // for each of the items in current selected pack, add to the list

            // iconsListView.Clear();
            // for (int i = 0; i < iconsList.Count; i++)
            // {
            //     string item = iconsList[i];
            //     var view = MakeIconItemGrid();
            //     BindIconItemGrid(view, i);
            //     iconsListView.Add(view);
            // }

            // iconsListView.onSelectionChange += IconSelectionChange;
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
            var pack = iconPacksListView.itemsSource[index] as IconPack;
            (element as Label).text = $"{pack.name}";
        }

        private async void IconPacksSelectionChange(IEnumerable<object> selectedItems)
        {
            iconsListView.itemsSource = new object[0];
            iconsListView.RefreshItems();

            var item = selectedItems.First();

            currentIconPack = item as IconPack;
            var iconResponse = await GetIconsAsync(currentIconPack.id);

            iconsList = new List<string>(iconResponse.icons);
            MakeGrid(iconsList);
            // SetGridMode(true, iconsList);

            SetGridMode(true);
        }

        private VisualElement lastSelectedItem = null;
        private string lastSelectIconId = null;

        // List Item Creation and Binding for Icons
        private VisualElement MakeIconItemGrid()
        {
            // create an icon item with an image and a label
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

            // build 4

            var container = new VisualElement();
            container.AddToClassList("iconItemContainer");

            for (int i = 0; i < 4; i++)
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
            for (int i = 0; i < 4; i++)
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

                    LoadIconIntoImage(image, response.Result);
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
            (element as Label).text = iconsListGrid[index / 4][index % 4];
        }

        // Search field value change callback
        private void SearchValueChangedPack(ChangeEvent<string> evt)
        {
            iconPacksListView.itemsSource = allIconPack.FindAll(pack => pack.name.ToLower().Contains(evt.newValue.ToLower()));
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
        private async Task<IconPackResponse> GetIconPacksAsync()
        {
            if (cache.ContainsKey("iconPacks"))
                return JsonUtility.FromJson<IconPackResponse>(cache["iconPacks"] as string);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(iconsListUrl))
            {
                // Request and wait for the desired page.
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    IconPackResponse iconPackResponse = JsonUtility.FromJson<IconPackResponse>(webRequest.downloadHandler.text);
                    cache["iconPacks"] = webRequest.downloadHandler.text;
                    return iconPackResponse;
                }
                else
                {
                    Debug.Log(": Error: " + webRequest.error);
                    return null;
                }
            }

        }

        // Task for Getting Icons
        private async Task<IconResponse> GetIconsAsync(string iconPackId)
        {
            IconPackRequest iconPackRequest = new IconPackRequest() { iconPackId = iconPackId };

            string json = JsonUtility.ToJson(iconPackRequest);

            if (cache.ContainsKey(iconPackId))
                return JsonUtility.FromJson<IconResponse>(cache[iconPackId] as string);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            using (UnityWebRequest webRequest = UnityWebRequest.Put(iconPackUrl, jsonToSend))
            {
                webRequest.method = "POST";
                webRequest.SetRequestHeader("Content-Type", "application/json");
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    IconResponse iconResponse = JsonUtility.FromJson<IconResponse>(webRequest.downloadHandler.text);
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

        private async Task<IconDetailResponse> GetIconAsync(string iconPackId, string iconId)
        {
            IconPackRequest iconPackRequest = new IconPackRequest()
            {
                iconPackId = iconPackId,
                iconId = iconId
            };

            if (cache.TryGetValue(iconId, out var res))
                return JsonUtility.FromJson<IconDetailResponse>(res as string);

            string json = JsonUtility.ToJson(iconPackRequest);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            using (UnityWebRequest webRequest = UnityWebRequest.Put(iconPackUrl, jsonToSend))
            {
                webRequest.method = "POST";
                webRequest.SetRequestHeader("Content-Type", "application/json");
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    IconDetailResponse iconResponse = JsonUtility.FromJson<IconDetailResponse>(webRequest.downloadHandler.text);
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

    [System.Serializable]
    public class IconPack
    {
        public string id;
        public string name;
        public string projectUrl;
        public string license;
        public string licenseUrl;
    }

    [System.Serializable]
    public class IconPackResponse
    {
        public List<IconPack> packs;
    }

    [System.Serializable]
    public class IconResponse
    {
        public List<string> icons;
    }

    [System.Serializable]
    public class IconDetailResponse
    {
        public string iconId;
        public string svg;
        public string base64ImagePreview;
    }

    [System.Serializable]
    public class IconPackRequest
    {
        public string iconPackId;
        public string iconId;
    }

    [Serializable]
    public class SerializableDictionaryWrapper
    {
        // JsonUtility only works with fields, not properties
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public void TryAdd(string key, string value)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            for (int i = 0; i < keys.Count; i++)
            {
                result.Add(keys[i], values[i]);
            }
            return result;
        }
    }

    public static class UnityWebRequestExtension
    {
        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
        {
            TaskCompletionSource<UnityWebRequest.Result> tsc = new();
            reqOp.completed += asyncOp => tsc.TrySetResult(reqOp.webRequest.result);

            if (reqOp.isDone)
                tsc.TrySetResult(reqOp.webRequest.result);

            return tsc.Task.GetAwaiter();
        }
    }
}
