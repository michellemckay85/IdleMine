using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using GoldAndGoblins.Gameplay;

namespace GoldAndGoblins.EditorTools
{
    // Phone games are portrait. Unity's Game view defaults to a wide desktop aspect,
    // which makes this project look "landscape" even when Player Settings are correct.
    // This menu also frames the mine camera and adds dungeon lighting so KayKit art reads.
    public static class PortraitCameraSetup
    {
        [MenuItem("Gold And Goblins/Use Phone Portrait View (1080x1920)")]
        public static void ApplyPortraitViewAndCamera()
        {
            SetGameViewPortrait();
            ConfigureMainCamera();
            EnsureDungeonLighting();
            FrameMineGridIfPresent();

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("[PortraitCameraSetup] Game view set to 1080x1920 portrait, camera framed on the mine, dungeon lighting added. Press Play to check.");
        }

        private static void SetGameViewPortrait()
        {
            // Unity doesn't expose Game View sizes publicly — use the editor API via reflection.
            try
            {
                var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                var singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instanceProp = singletonType.GetProperty("instance");
                var gameViewSizes = instanceProp.GetValue(null, null);

                var currentGroupProp = sizesType.GetProperty("currentGroup");
                var group = currentGroupProp.GetValue(gameViewSizes, null);
                var groupType = group.GetType();

                var getTotalCount = groupType.GetMethod("GetTotalCount");
                var getGameViewSize = groupType.GetMethod("GetGameViewSize");
                var addCustomSize = groupType.GetMethod("AddCustomSize");

                const string label = "Phone Portrait 1080x1920";
                var total = (int)getTotalCount.Invoke(group, null);
                var foundIndex = -1;
                for (var i = 0; i < total; i++)
                {
                    var size = getGameViewSize.Invoke(group, new object[] { i });
                    var baseText = size.GetType().GetProperty("baseText").GetValue(size, null) as string;
                    if (baseText == label)
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex < 0)
                {
                    var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
                    var gameViewSizeTypeEnum = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                    var fixedResolution = Enum.Parse(gameViewSizeTypeEnum, "FixedResolution");
                    var ctor = gameViewSizeType.GetConstructor(new[] { gameViewSizeTypeEnum, typeof(int), typeof(int), typeof(string) });
                    var newSize = ctor.Invoke(new object[] { fixedResolution, 1080, 1920, label });
                    addCustomSize.Invoke(group, new[] { newSize });
                    foundIndex = (int)getTotalCount.Invoke(group, null) - 1;
                }

                var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
                var gameView = EditorWindow.GetWindow(gameViewType);
                var sizeSelectionCallback = gameViewType.GetMethod("SizeSelectionCallback",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                sizeSelectionCallback.Invoke(gameView, new object[] { foundIndex, null });
                gameView.Repaint();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[PortraitCameraSetup] Could not set Game view size automatically (" + e.Message +
                                 "). In the Game tab, open the aspect dropdown (says Free Aspect) and pick 1080x1920 or add Phone Portrait.");
            }
        }

        private static void ConfigureMainCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[PortraitCameraSetup] No Main Camera found.");
                return;
            }

            // MineGrid centers blocks on local origin after the portrait framing fix.
            cam.orthographic = true;
            cam.orthographicSize = 5.2f; // tall enough for HUD + 5-row shaft in portrait
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.06f, 0.05f, 1f); // near-black mine shaft
            EditorUtility.SetDirty(cam);
            EditorUtility.SetDirty(cam.gameObject);
        }

        private static void EnsureDungeonLighting()
        {
            var existing = GameObject.Find("Dungeon Light");
            if (existing == null)
            {
                existing = new GameObject("Dungeon Light");
                var light = existing.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.92f, 0.75f);
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                existing.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.25f, 0.22f, 0.2f);
            RenderSettings.subtractiveShadowColor = new Color(0.15f, 0.12f, 0.1f);
        }

        private static void FrameMineGridIfPresent()
        {
            var grid = Object.FindObjectOfType<MineGrid>(true);
            if (grid == null) return;
            // Nudge the grid object to world origin under the camera framing.
            grid.transform.localPosition = Vector3.zero;
            EditorUtility.SetDirty(grid);
        }
    }
}
