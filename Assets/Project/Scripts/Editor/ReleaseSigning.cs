using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Signs Android builds with the release upload key. Unity never saves keystore passwords in the
// project, so they live in ReleaseSigning.local.json at the project root (git-ignored, never commit
// it) and are applied on every Editor load and again just before each Android build. Without that
// file the build falls back to Unity's debug key, which Google Play rejects.
[InitializeOnLoad]
public class ReleaseSigning : IPreprocessBuildWithReport
{
    const string FileName = "ReleaseSigning.local.json";

    [System.Serializable]
    class SigningFile
    {
        public string keystore;
        public string alias;
        public string keystorePass;
        public string keyaliasPass;
    }

    static ReleaseSigning()
    {
        Apply(false);
    }

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
        {
            Apply(true);
        }
    }

    /// <summary>Applies the release keystore to PlayerSettings. Returns false when the signing file is missing or broken.</summary>
    public static bool Apply(bool warnIfMissing)
    {
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, FileName);
        if (!File.Exists(path))
        {
            if (warnIfMissing)
            {
                Debug.LogWarning("[ReleaseSigning] " + FileName + " not found — this Android build is debug-signed "
                                 + "and Google Play will reject it.");
            }

            return false;
        }

        SigningFile file = JsonUtility.FromJson<SigningFile>(File.ReadAllText(path));
        if (file == null || string.IsNullOrEmpty(file.keystore) || !File.Exists(file.keystore))
        {
            Debug.LogError("[ReleaseSigning] " + FileName + " does not point at an existing keystore.");
            return false;
        }

        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = file.keystore;
        PlayerSettings.Android.keystorePass = file.keystorePass;
        PlayerSettings.Android.keyaliasName = file.alias;
        PlayerSettings.Android.keyaliasPass = file.keyaliasPass;
        return true;
    }
}
