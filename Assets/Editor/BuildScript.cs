using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GoldAndGoblins.EditorTools
{
    /// <summary>
    /// Menu items (and CI-friendly static entry points) for producing the Xcode project /
    /// Android App Bundle that you hand to Xcode / Google Play Console. See
    /// docs/STORE_SUBMISSION_CHECKLIST.md for the manual signing and store-listing steps this
    /// script does NOT do (Apple/Google won't let a headless script do those for you).
    /// </summary>
    public static class BuildScript
    {
        private const string IosBuildPath = "Builds/iOS";
        private const string AndroidBuildPath = "Builds/Android/GoldAndGoblins.aab";

        [MenuItem("Gold And Goblins/Build/iOS Xcode Project")]
        public static void BuildIos()
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildScript] No scenes enabled in Build Settings.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = IosBuildPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            LogSummary("iOS", report);
        }

        [MenuItem("Gold And Goblins/Build/Android App Bundle (.aab)")]
        public static void BuildAndroid()
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildScript] No scenes enabled in Build Settings.");
                return;
            }

            EditorUserBuildSettings.buildAppBundle = true;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = AndroidBuildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            LogSummary("Android", report);
        }

        /// <summary>
        /// CI entry point, e.g.:
        /// Unity -batchmode -quit -projectPath . -executeMethod GoldAndGoblins.EditorTools.BuildScript.CIBuildAndroid
        /// Signing (keystore path/password) must already be configured in
        /// PlayerSettings/EditorUserBuildSettings or passed via -keystorePass etc. on the command line.
        /// </summary>
        public static void CIBuildAndroid()
        {
            BuildAndroid();
        }

        public static void CIBuildIos()
        {
            BuildIos();
        }

        private static void LogSummary(string platform, UnityEditor.Build.Reporting.BuildReport report)
        {
            var summary = report.summary;
            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] {platform} build succeeded: {summary.totalSize} bytes at {summary.outputPath}");
            }
            else
            {
                Debug.LogError($"[BuildScript] {platform} build {summary.result}. See errors above.");
            }
        }
    }
}
