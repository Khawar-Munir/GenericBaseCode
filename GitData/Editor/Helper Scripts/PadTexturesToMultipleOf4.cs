// Assets/Editor/PadTexturesToMultipleOf4.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PadTexturesToMultipleOf4
{
    // Set true if you want to overwrite the original source file (be careful!)
    private const bool OVERWRITE_SOURCE = false;
    private const string SUFFIX = "_m4"; // used if not overwriting

    [MenuItem("Tools/Textures/Pad to multiple of 4 (Selection)")]
    public static void FixSelection()
    {
        var guids = Selection.assetGUIDs;
        if (guids == null || guids.Length == 0) { Debug.Log("Nothing selected."); return; }
        var paths = guids.Select(AssetDatabase.GUIDToAssetPath)
                         .Where(p => IsTexturePath(p)).ToArray();
        Process(paths);
    }

    [MenuItem("Tools/Textures/Pad to multiple of 4 (Folder Recursively)")]
    public static void FixFolderRecursive()
    {
        var guids = Selection.assetGUIDs;
        if (guids == null || guids.Length == 0) { Debug.Log("Select a folder."); return; }
        var all = guids.Select(AssetDatabase.GUIDToAssetPath)
                       .SelectMany(p => AssetDatabase.FindAssets("t:Texture2D", new[] { p }))
                       .Select(AssetDatabase.GUIDToAssetPath)
                       .Where(p => IsTexturePath(p))
                       .Distinct()
                       .ToArray();
        Process(all);
    }

    private static bool IsTexturePath(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".png" || ext == ".tga" || ext == ".jpg" || ext == ".jpeg" || ext == ".psd";
    }

    private static void Process(string[] paths)
    {
        int changed = 0, skipped = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var path in paths)
            {
                if (!PadOne(path)) skipped++;
                else changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        Debug.Log($"Pad to multiple of 4: changed {changed}, skipped {skipped}.");
    }

    private static bool PadOne(string assetPath)
    {
        // Try to load the raw image file to avoid isReadable issues
        string projectRoot = Path.GetDirectoryName(Application.dataPath)!.Replace("\\", "/");
        string fullPath = Path.Combine(projectRoot, assetPath).Replace("\\", "/");

        Texture2D src = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        byte[] fileBytes = null;
        bool loadedFromBytes = false;

        try
        {
            fileBytes = File.ReadAllBytes(fullPath);
            // Decode PNG/JPG (and most TGAs) without relying on importer flags
            loadedFromBytes = ImageConversion.LoadImage(src, fileBytes, markNonReadable: false);
        }
        catch { /* fall back below */ }

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        bool touchedImporter = false;
        bool originalReadable = false;
        var origNPOT = TextureImporterNPOTScale.ToNearest;

        // Fallback path for formats LoadImage can't handle (e.g., PSD)
        if (!loadedFromBytes)
        {
            if (importer == null) return false;

            originalReadable = importer.isReadable;
            origNPOT = importer.npotScale;

            // Ensure readable, don’t let Unity rescale NPOT (we only need multiple-of-4)
            importer.isReadable = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
            touchedImporter = true;

            src = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (src == null)
            {
                Debug.LogWarning($"Could not load Texture2D at {assetPath}");
                return false;
            }
        }

        int w = src.width, h = src.height;
        int w4 = (w + 3) & ~3;
        int h4 = (h + 3) & ~3;

        if (w4 == w && h4 == h)
        {
            // Restore importer if we changed it
            if (touchedImporter)
            {
                importer.isReadable = originalReadable;
                importer.npotScale = origNPOT;
                importer.SaveAndReimport();
            }
            return false; // no change needed
        }

        // Build padded texture (edge-repeat padding)
        var padded = new Texture2D(w4, h4, TextureFormat.RGBA32, false, false);

        // Copy original area
        if (loadedFromBytes)
            padded.SetPixels(0, 0, w, h, src.GetPixels());
        else
            padded.SetPixels(0, 0, w, h, src.GetPixels(0, 0, w, h));

        // Right border: repeat last column
        var lastCol = new Color[h];
        for (int y = 0; y < h; y++) lastCol[y] = src.GetPixel(w - 1, y);
        for (int x = w; x < w4; x++) padded.SetPixels(x, 0, 1, h, lastCol);

        // Top border: repeat last row (after right pad so we can read from padded)
        var lastRow = new Color[w4];
        for (int x = 0; x < w4; x++)
            lastRow[x] = x < w ? src.GetPixel(x, h - 1) : padded.GetPixel(x, h - 1);
        for (int y = h; y < h4; y++) padded.SetPixels(0, y, w4, 1, lastRow);

        padded.Apply(false, false);

        // Write out PNG next to the source (non-destructive)
        byte[] bytes = padded.EncodeToPNG();
        string dir = Path.GetDirectoryName(assetPath)!;
        string name = Path.GetFileNameWithoutExtension(assetPath);
        string ext = Path.GetExtension(assetPath); // keep original ext in name only
        string newPath = Path.Combine(dir, name + "_m4.png").Replace("\\", "/");

        File.WriteAllBytes(newPath, bytes);
        AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceSynchronousImport);

        // Restore importer flags if we modified them
        if (touchedImporter)
        {
            importer.isReadable = originalReadable;
            importer.npotScale = origNPOT;
            importer.SaveAndReimport();
        }

        return true;
    }

}
