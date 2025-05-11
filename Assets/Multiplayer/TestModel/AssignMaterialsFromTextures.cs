using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class MaterialAssigner : MonoBehaviour
{

    #if UNITY_EDITOR
    public string textureFolder = "Assets/Multiplayer/TestModel/Textures";
    public string materialFolder = "Assets/Multiplayer/TestModel/Materials";

    public List<Texture> textures = new List<Texture>();
    public List<string> extractedNames = new List<string>();

    [ContextMenu("Step 1: Get Textures From Folder")]
    public void GetTexturesFromFolder()
    {
        textures.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { textureFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex != null) textures.Add(tex);
        }

        Debug.Log($"Found {textures.Count} textures.");
    }

    [ContextMenu("Step 2: Extract Clean Names from Textures")]
    public void ExtractNames()
    {
        extractedNames.Clear();

        foreach (var tex in textures)
        {
            string name = tex.name;
            Match match = Regex.Match(name, @"RGB_(.*?)\s*-\s*Color");
            if (match.Success)
            {
                string clean = CleanName(match.Groups[1].Value);
                if (!extractedNames.Contains(clean))
                    extractedNames.Add(clean);
            }
        }

        Debug.Log($"Extracted {extractedNames.Count} unique names.");
    }

    [ContextMenu("Step 3: Create Materials and Assign to Objects")]
    public void CreateMaterialsAndAssign()
    {
        if (!AssetDatabase.IsValidFolder(materialFolder))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(materialFolder), Path.GetFileName(materialFolder));

        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (var child in allChildren)
        {
            string cleanObjName = CleanObjectName(child.name);

            Texture matchedTexture = textures.FirstOrDefault(tex =>
            {
                Match match = Regex.Match(tex.name, @"RGB_(.*?)\s*-\s*Color");
                if (match.Success)
                {
                    string texClean = CleanName(match.Groups[1].Value);
                    return texClean == cleanObjName;
                }
                return false;
            });

            if (matchedTexture == null)
            {
                Debug.LogWarning($" No texture found for: {child.name}");
                continue;
            }

            string matPath = Path.Combine(materialFolder, cleanObjName + ".mat");
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = matchedTexture;
                AssetDatabase.CreateAsset(mat, matPath);
                Debug.Log($"Created material: {cleanObjName}");
            }

            Renderer rend = child.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sharedMaterial = mat;
                Debug.Log($"Assigned material to: {child.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Material creation and assignment complete.");
    }

    // Normalize texture names
    string CleanName(string input)
    {
        return input.Replace(" ", "").Replace("+", "").ToLower();
    }

    // Normalize object names
    string CleanObjectName(string input)
    {
        string[] parts = input.Split('_');
        if (parts.Length >= 2)
            input = parts[1]; // Get the middle part

        return CleanName(input);
    }

#endif
}
