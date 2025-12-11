#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class QuickAndroidSign
{
    // ==== HARD-CODE YOURS HERE ====
    private const string KeystorePath = "Assets/Keystore/akheerstudio.keystore"; // or "../UserData/keystore.keystore"
    private const string KeystorePass = "akheerstudio";
    private const string KeyAlias = "akheerstudio";
    private const string KeyAliasPass = "akheerstudio";
    // ===============================

    // Shortcut: Ctrl/Cmd + Alt + S
    [MenuItem("Tools/Android/Quick: Apply Signing %&s", priority = 1)]
    public static void ApplySigning()
    {
        if (!File.Exists(Path.GetFullPath(KeystorePath)))
        {
            EditorUtility.DisplayDialog("Quick Android Sign",
                $"Keystore not found:\n{Path.GetFullPath(KeystorePath)}", "OK");
            return;
        }

        // Ensure Android is the active target (optional: comment out if you switch manually)
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                EditorUtility.DisplayDialog("Quick Android Sign", "Failed to switch to Android build target.", "OK");
                return;
            }
        }

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = KeystorePath;
        PlayerSettings.Android.keystorePass = KeystorePass;
        PlayerSettings.Android.keyaliasName = KeyAlias;
        PlayerSettings.Android.keyaliasPass = KeyAliasPass;

        Debug.Log($"[QuickAndroidSign] Applied signing: {KeyAlias} @ {PlayerSettings.Android.keystoreName}");
        EditorUtility.DisplayDialog("Quick Android Sign", "Signing settings applied.", "Done");
    }

    // Optional: quick clear (no shortcut)
    [MenuItem("Tools/Android/Quick: Clear Signing", priority = 2)]
    public static void ClearSigning()
    {
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = "";
        PlayerSettings.Android.keystorePass = "";
        PlayerSettings.Android.keyaliasName = "";
        PlayerSettings.Android.keyaliasPass = "";
        Debug.Log("[QuickAndroidSign] Cleared signing settings.");
        EditorUtility.DisplayDialog("Quick Android Sign", "Signing settings cleared.", "OK");
    }
}
#endif
