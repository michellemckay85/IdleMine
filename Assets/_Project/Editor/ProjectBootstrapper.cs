using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoldAndGoblins.Core;
using GoldAndGoblins.Economy;
using GoldAndGoblins.Gameplay;
using GoldAndGoblins.Goblins;
using GoldAndGoblins.LiveOps;
using GoldAndGoblins.Ads;
using GoldAndGoblins.Analytics;
using GoldAndGoblins.UI;

namespace GoldAndGoblins.EditorTools
{
    // One-click starter scene so the project isn't a blank canvas on first open.
    // Wires up every manager singleton and a minimal Canvas hierarchy; you still need
    // to assign your art prefabs / ScriptableObject data / GritLine materials in the
    // Inspector afterward, and build out real visuals in place of the placeholder UI.
    public static class ProjectBootstrapper
    {
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";

        [MenuItem("Gold And Goblins/Bootstrap Starter Scene")]
        public static void BootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateManagers();
            CreateEventSystem();
            CreateCanvas();

            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[ProjectBootstrapper] Starter scene created at {ScenePath}");
        }

        private static void CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(0, 0, -10);
        }

        private static void CreateManagers()
        {
            var root = new GameObject("Managers");

            root.AddComponent<SaveManager>();
            root.AddComponent<CurrencyManager>();
            var upgradeSystem = root.AddComponent<UpgradeSystem>();

            var gridGo = new GameObject("MineGrid");
            gridGo.transform.SetParent(root.transform);
            var mineGrid = gridGo.AddComponent<MineGrid>();

            var gridRootGo = new GameObject("GridRoot");
            gridRootGo.transform.SetParent(gridGo.transform);
            gridRootGo.transform.localPosition = Vector3.zero;
            AssignSerializedField(mineGrid, "gridRoot", gridRootGo.transform);

            var goblinCombat = gridGo.AddComponent<GoblinCombatManager>();
            AssignSerializedField(mineGrid, "goblinCombatManager", goblinCombat);

            var drillInput = gridGo.AddComponent<DrillInputController>();
            AssignSerializedField(drillInput, "worldCamera", Camera.main);

            var idleEarnings = root.AddComponent<IdleEarningsManager>();
            root.AddComponent<PrestigeManager>();
            var eventManager = root.AddComponent<EventManager>();
            var dailyReward = root.AddComponent<DailyRewardManager>();
            root.AddComponent<IAPManager>();

            var adsGo = new GameObject("AdsManager");
            adsGo.transform.SetParent(root.transform);
            var mockProvider = adsGo.AddComponent<MockAdsProvider>();
            var adsManager = adsGo.AddComponent<AdsManager>();
            AssignSerializedField(adsManager, "providerBehaviour", mockProvider);

            root.AddComponent<AnalyticsManager>();

            var gameManager = root.AddComponent<GameManager>();
            AssignSerializedField(gameManager, "mineGrid", mineGrid);
            AssignSerializedField(gameManager, "upgradeSystem", upgradeSystem);
            AssignSerializedField(gameManager, "idleEarningsManager", idleEarnings);
            AssignSerializedField(gameManager, "eventManager", eventManager);
            AssignSerializedField(gameManager, "dailyRewardManager", dailyReward);
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            var hudGo = CreateUIChild(canvasGo.transform, "HUD");
            hudGo.AddComponent<HUDController>();

            var bannerGo = CreateUIChild(canvasGo.transform, "EventBanner");
            bannerGo.AddComponent<EventBannerController>();

            var welcomeBackGo = CreateUIChild(canvasGo.transform, "WelcomeBackPopup");
            welcomeBackGo.AddComponent<WelcomeBackPopupController>();

            var dailyRewardGo = CreateUIChild(canvasGo.transform, "DailyRewardPopup");
            dailyRewardGo.AddComponent<DailyRewardPopupController>();

            var goblinBarGo = CreateUIChild(canvasGo.transform, "GoblinHealthBar");
            goblinBarGo.AddComponent<GoblinHealthBarController>();

            Debug.Log("[ProjectBootstrapper] Placeholder UI created -- build real layouts on these GameObjects and assign their serialized fields in the Inspector.");
        }

        private static GameObject CreateUIChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[ProjectBootstrapper] Could not find serialized field '{fieldName}' on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
