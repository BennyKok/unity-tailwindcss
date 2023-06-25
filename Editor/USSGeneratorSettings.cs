using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "USSGeneratorSettings", menuName = "USSGeneratorSettings")]
public class USSGeneratorSettings : ScriptableObject
{
    public string ussFilePath = "Assets/Styles/generated.uss";
}

#if UNITY_EDITOR
[CustomEditor(typeof(USSGeneratorSettings))]
public class USSGeneratorSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        USSGeneratorSettings settings = (USSGeneratorSettings)target;

        if (!string.IsNullOrWhiteSpace(settings.ussFilePath))
        {
            var directoryName = Path.GetDirectoryName(settings.ussFilePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
                Debug.Log("Directory created: " + directoryName);
            }
        }

        if (GUILayout.Button("Generate USS"))
        {
            USSGenerator.Generate(settings.ussFilePath);
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Create New UXML"))
        {
            CreateNewUXML(settings.ussFilePath);
        }
    }

    public void CreateNewUXML(string ussPath)
    {
        var path = EditorUtility.SaveFilePanelInProject("New UXML", "NewUXML", "uxml", "Please enter a file name to save the UXML to");

        if (path.Length != 0)
        {
            var directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
                Debug.Log("Directory created: " + directoryName);
            }

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<UXML xmlns=\"UnityEngine.UIElements\" xmlns:u=\"UnityEditor.UIElements\" xmlns:uss=\"UnityStyleSheet\">");
                sw.WriteLine("<Style src=\"project://database/"+ussPath+"\" />");
                sw.WriteLine("</UXML>");
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif

