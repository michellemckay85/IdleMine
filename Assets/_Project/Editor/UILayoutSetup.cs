using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.Events;
using TMPro;
using GoldAndGoblins.Core;
using GoldAndGoblins.UI;
using GoldAndGoblins.LiveOps;
using GoldAndGoblins.Social;

namespace GoldAndGoblins.EditorTools
{
    // Builds real visuals (backgrounds, text, buttons) onto the placeholder UI
    // GameObjects ProjectBootstrapper creates, using the Kenney/KayKit sprites under
    // Art/UI, and adds the Upgrade/Shop panels + nav bar that bootstrap never created.
    // Operates on whatever scene is currently open -- run after Bootstrap Starter Scene.
    public static class UILayoutSetup
    {
        private const string UIArtPath = "Assets/_Project/Art/UI";
        private const string RowPrefabPath = "Assets/_Project/Prefabs";

        [MenuItem("Gold And Goblins/Build UI Layout")]
        public static void BuildUILayout()
        {
            var canvas = Object.FindObjectOfType<Canvas>(true);
            if (canvas == null)
            {
                Debug.LogError("[UILayoutSetup] No Canvas in the open scene -- run 'Bootstrap Starter Scene' first.");
                return;
            }

            var panelSprite = LoadSprite("panel_brown.png");
            var buttonSprite = LoadSprite("button_brown.png");
            var closeButtonSprite = LoadSprite("button_brown_close.png");
            var bannerSprite = LoadSprite("banner_hanging.png");
            var barFillSprite = LoadSprite("progress_red.png");
            var barBorderSprite = LoadSprite("progress_red_border.png");

            EnsureLeaderboardManagerExists();

            BuildHUD(canvas);
            var upgradePanel = BuildUpgradePanel(canvas, panelSprite, closeButtonSprite, buttonSprite);
            var shopPanel = BuildShopPanel(canvas, panelSprite, closeButtonSprite, buttonSprite);
            var leaderboardPanel = BuildLeaderboardPanel(canvas, panelSprite, closeButtonSprite, buttonSprite);
            BuildNavBar(canvas, panelSprite, buttonSprite, upgradePanel, shopPanel, leaderboardPanel);
            BuildEventBanner(canvas, bannerSprite);
            BuildWelcomeBackPopup(canvas, panelSprite, buttonSprite);
            BuildDailyRewardPopup(canvas, panelSprite, buttonSprite);
            BuildGoblinHealthBar(canvas, barFillSprite, barBorderSprite);

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            EditorSceneManager.SaveScene(canvas.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[UILayoutSetup] Built HUD, Upgrade/Shop/Leaderboard panels + row prefabs, nav bar, event " +
                      "banner, welcome-back/daily-reward popups, and goblin health bar. Upgrade/Shop panels will be " +
                      "empty until you create UpgradeDataSO / IAPProductSO+ProductCatalogSO assets. The leaderboard " +
                      "will show 'unavailable' until Unity Gaming Services is linked (Edit > Project Settings > " +
                      "Services) and a 'total_gold_earned' leaderboard exists in the Unity Cloud Dashboard.");
        }

        // LeaderboardManager is only added to fresh scenes by ProjectBootstrapper -- an
        // already-existing saved scene (like this project's Main.unity) never picks up
        // additions to that tool retroactively, so add it here if it's missing.
        private static void EnsureLeaderboardManagerExists()
        {
            if (Object.FindObjectOfType<LeaderboardManager>(true) != null) return;

            var managersRoot = Object.FindObjectOfType<GameManager>(true);
            if (managersRoot == null)
            {
                Debug.LogWarning("[UILayoutSetup] No GameManager found -- can't attach LeaderboardManager. Run 'Bootstrap Starter Scene' first.");
                return;
            }

            managersRoot.gameObject.AddComponent<LeaderboardManager>();
        }

        private static Sprite LoadSprite(string fileName)
        {
            var path = $"{UIArtPath}/{fileName}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            // These UI PNGs were imported as plain Default textures, not Sprites, so there's
            // no Sprite sub-asset to load yet -- reimport as a Sprite and try again.
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[UILayoutSetup] No texture importer found for {path}.");
                return null;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---------- HUD ----------

        private static void BuildHUD(Canvas canvas)
        {
            var hud = Object.FindObjectOfType<HUDController>(true);
            if (hud == null) return;
            ClearChildren(hud.transform);

            var rt = hud.GetComponent<RectTransform>();
            AnchorStretchTop(rt, 160);

            var bg = CreateImage("Background", hud.transform, LoadSprite("panel_brown.png"), new Color(1, 1, 1, 0.9f));
            AnchorStretchAll(bg.GetComponent<RectTransform>());

            var goldText = CreateText(hud.transform, "GoldText", "Gold: 0", 42, TextAlignmentOptions.MidlineLeft);
            AnchorLeft(goldText.rectTransform, 40, 320, 100);

            var gemsText = CreateText(hud.transform, "GemsText", "Gems: 0", 42, TextAlignmentOptions.MidlineLeft);
            AnchorLeft(gemsText.rectTransform, 380, 280, 100);

            var depthText = CreateText(hud.transform, "DepthText", "Depth 1", 42, TextAlignmentOptions.MidlineRight);
            AnchorRight(depthText.rectTransform, 40, 280, 100);

            AssignSerializedField(hud, "goldText", goldText);
            AssignSerializedField(hud, "gemsText", gemsText);
            AssignSerializedField(hud, "depthText", depthText);
        }

        // ---------- Upgrade panel ----------

        private static GameObject BuildUpgradePanel(Canvas canvas, Sprite panelSprite, Sprite closeSprite, Sprite buttonSprite)
        {
            var existing = Object.FindObjectOfType<UpgradePanelController>(true);
            var panelGo = existing != null ? existing.gameObject : CreateUIChild(canvas.transform, "UpgradePanel");
            ClearChildren(panelGo.transform);
            var controller = existing != null ? existing : panelGo.AddComponent<UpgradePanelController>();

            AnchorModal(panelGo.GetComponent<RectTransform>());
            var bg = CreateImage("Background", panelGo.transform, panelSprite, Color.white);
            AnchorStretchAll(bg.GetComponent<RectTransform>());

            CreateTitle(panelGo.transform, "Upgrades");
            CreateCloseButton(panelGo.transform, closeSprite, panelGo);

            var rowContainer = BuildScrollingContent(panelGo.transform);
            var rowPrefab = BuildUpgradeRowPrefab(buttonSprite);

            AssignSerializedField(controller, "rowPrefab", rowPrefab.GetComponent<UpgradeRowController>());
            AssignSerializedField(controller, "rowContainer", rowContainer);

            panelGo.SetActive(false);
            return panelGo;
        }

        private static GameObject BuildUpgradeRowPrefab(Sprite buttonSprite)
        {
            var row = new GameObject("UpgradeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            var rt = row.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 140);

            var layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 140;
            layoutElement.flexibleWidth = 1;

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;

            var nameText = CreateText(row.transform, "NameText", "Upgrade", 34, TextAlignmentOptions.MidlineLeft);
            AddFlexibleWidth(nameText.gameObject, 2f);

            var levelText = CreateText(row.transform, "LevelText", "Lv. 0", 30, TextAlignmentOptions.Midline);
            AddFlexibleWidth(levelText.gameObject, 1f);

            var costText = CreateText(row.transform, "CostText", "0", 30, TextAlignmentOptions.Midline);
            AddFlexibleWidth(costText.gameObject, 1f);

            var button = CreateButton(row.transform, "BuyButton", buttonSprite, "Buy", 28);
            var buttonLayout = button.gameObject.AddComponent<LayoutElement>();
            buttonLayout.preferredWidth = 180;
            buttonLayout.preferredHeight = 100;

            var controller = row.AddComponent<UpgradeRowController>();
            AssignSerializedField(controller, "nameText", nameText);
            AssignSerializedField(controller, "levelText", levelText);
            AssignSerializedField(controller, "costText", costText);
            AssignSerializedField(controller, "buyButton", button);

            Directory.CreateDirectory(RowPrefabPath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(row, $"{RowPrefabPath}/UpgradeRow.prefab");
            Object.DestroyImmediate(row);
            return prefab;
        }

        // ---------- Shop panel ----------

        private static GameObject BuildShopPanel(Canvas canvas, Sprite panelSprite, Sprite closeSprite, Sprite buttonSprite)
        {
            var existing = Object.FindObjectOfType<ShopUIController>(true);
            var panelGo = existing != null ? existing.gameObject : CreateUIChild(canvas.transform, "ShopPanel");
            ClearChildren(panelGo.transform);
            var controller = existing != null ? existing : panelGo.AddComponent<ShopUIController>();

            AnchorModal(panelGo.GetComponent<RectTransform>());
            var bg = CreateImage("Background", panelGo.transform, panelSprite, Color.white);
            AnchorStretchAll(bg.GetComponent<RectTransform>());

            CreateTitle(panelGo.transform, "Shop");
            CreateCloseButton(panelGo.transform, closeSprite, panelGo);

            var rowContainer = BuildScrollingContent(panelGo.transform);
            var rowPrefab = BuildShopRowPrefab(buttonSprite);

            AssignSerializedField(controller, "rowPrefab", rowPrefab.GetComponent<ShopRowController>());
            AssignSerializedField(controller, "rowContainer", rowContainer);

            panelGo.SetActive(false);
            return panelGo;
        }

        private static GameObject BuildShopRowPrefab(Sprite buttonSprite)
        {
            var row = new GameObject("ShopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            var layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 160;
            layoutElement.flexibleWidth = 1;

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;

            var textColumn = new GameObject("TextColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
            textColumn.transform.SetParent(row.transform, false);
            var vlg = textColumn.GetComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            AddFlexibleWidth(textColumn, 2f);

            var nameText = CreateText(textColumn.transform, "NameText", "Product", 34, TextAlignmentOptions.MidlineLeft);
            var descText = CreateText(textColumn.transform, "DescriptionText", "Description", 24, TextAlignmentOptions.MidlineLeft);
            descText.color = new Color(0.3f, 0.3f, 0.3f);

            var priceText = CreateText(row.transform, "PriceText", "$0.00", 30, TextAlignmentOptions.Midline);
            AddFlexibleWidth(priceText.gameObject, 1f);

            var button = CreateButton(row.transform, "BuyButton", buttonSprite, "Buy", 28);
            var buttonLayout = button.gameObject.AddComponent<LayoutElement>();
            buttonLayout.preferredWidth = 180;
            buttonLayout.preferredHeight = 100;

            var controller = row.AddComponent<ShopRowController>();
            AssignSerializedField(controller, "nameText", nameText);
            AssignSerializedField(controller, "descriptionText", descText);
            AssignSerializedField(controller, "priceText", priceText);
            AssignSerializedField(controller, "buyButton", button);

            Directory.CreateDirectory(RowPrefabPath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(row, $"{RowPrefabPath}/ShopRow.prefab");
            Object.DestroyImmediate(row);
            return prefab;
        }

        // ---------- Leaderboard panel ----------

        private static GameObject BuildLeaderboardPanel(Canvas canvas, Sprite panelSprite, Sprite closeSprite, Sprite buttonSprite)
        {
            var existing = Object.FindObjectOfType<LeaderboardPanelController>(true);
            var panelGo = existing != null ? existing.gameObject : CreateUIChild(canvas.transform, "LeaderboardPanel");
            ClearChildren(panelGo.transform);
            var controller = existing != null ? existing : panelGo.AddComponent<LeaderboardPanelController>();

            AnchorModal(panelGo.GetComponent<RectTransform>());
            var bg = CreateImage("Background", panelGo.transform, panelSprite, Color.white);
            AnchorStretchAll(bg.GetComponent<RectTransform>());

            CreateTitle(panelGo.transform, "Leaderboard");
            CreateCloseButton(panelGo.transform, closeSprite, panelGo);

            var statusText = CreateText(panelGo.transform, "StatusText", "Loading...", 32, TextAlignmentOptions.Midline);
            AnchorCenterBox(statusText.rectTransform, 800, 200, 0, -60);

            var rowContainer = BuildScrollingContent(panelGo.transform);
            var rowPrefab = BuildLeaderboardRowPrefab();

            AssignSerializedField(controller, "rowPrefab", rowPrefab.GetComponent<LeaderboardRowController>());
            AssignSerializedField(controller, "rowContainer", rowContainer);
            AssignSerializedField(controller, "statusText", statusText);

            panelGo.SetActive(false);
            return panelGo;
        }

        private static GameObject BuildLeaderboardRowPrefab()
        {
            var row = new GameObject("LeaderboardRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            var layoutElement = row.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 110;
            layoutElement.flexibleWidth = 1;

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 5, 5);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;

            var rankText = CreateText(row.transform, "RankText", "#0", 30, TextAlignmentOptions.MidlineLeft);
            AddFlexibleWidth(rankText.gameObject, 0.6f);

            var nameText = CreateText(row.transform, "NameText", "Player", 30, TextAlignmentOptions.MidlineLeft);
            AddFlexibleWidth(nameText.gameObject, 2f);

            var scoreText = CreateText(row.transform, "ScoreText", "0", 30, TextAlignmentOptions.MidlineRight);
            AddFlexibleWidth(scoreText.gameObject, 1f);

            var controller = row.AddComponent<LeaderboardRowController>();
            AssignSerializedField(controller, "rankText", rankText);
            AssignSerializedField(controller, "nameText", nameText);
            AssignSerializedField(controller, "scoreText", scoreText);

            Directory.CreateDirectory(RowPrefabPath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(row, $"{RowPrefabPath}/LeaderboardRow.prefab");
            Object.DestroyImmediate(row);
            return prefab;
        }

        // ---------- Nav bar ----------

        private static void BuildNavBar(Canvas canvas, Sprite panelSprite, Sprite buttonSprite, GameObject upgradePanel, GameObject shopPanel, GameObject leaderboardPanel)
        {
            var existingNav = Object.FindObjectOfType<NavBarController>(true);
            var navGo = existingNav != null ? existingNav.gameObject : CreateUIChild(canvas.transform, "NavBar");
            ClearChildren(navGo.transform);
            var nav = existingNav != null ? existingNav : navGo.AddComponent<NavBarController>();

            AnchorStretchBottom(navGo.GetComponent<RectTransform>(), 200);
            var bg = CreateImage("Background", navGo.transform, panelSprite, new Color(1, 1, 1, 0.9f));
            AnchorStretchAll(bg.GetComponent<RectTransform>());

            var upgradesButton = CreateButton(navGo.transform, "UpgradesButton", buttonSprite, "Upgrades", 28);
            AnchorHorizontalSlot(upgradesButton.GetComponent<RectTransform>(), 1f / 6f, 320, 140);

            var shopButton = CreateButton(navGo.transform, "ShopButton", buttonSprite, "Shop", 28);
            AnchorHorizontalSlot(shopButton.GetComponent<RectTransform>(), 0.5f, 320, 140);

            var leaderboardButton = CreateButton(navGo.transform, "LeaderboardButton", buttonSprite, "Ranks", 28);
            AnchorHorizontalSlot(leaderboardButton.GetComponent<RectTransform>(), 5f / 6f, 320, 140);

            AssignSerializedField(nav, "upgradePanel", upgradePanel);
            AssignSerializedField(nav, "shopPanel", shopPanel);
            AssignSerializedField(nav, "leaderboardPanel", leaderboardPanel);

            UnityEventTools.AddPersistentListener(upgradesButton.onClick, nav.ShowUpgrades);
            UnityEventTools.AddPersistentListener(shopButton.onClick, nav.ShowShop);
            UnityEventTools.AddPersistentListener(leaderboardButton.onClick, nav.ShowLeaderboard);
        }

        // ---------- Event banner ----------

        private static void BuildEventBanner(Canvas canvas, Sprite bannerSprite)
        {
            var controller = Object.FindObjectOfType<EventBannerController>(true);
            if (controller == null) return;
            ClearChildren(controller.transform);

            AnchorStretchTop(controller.GetComponent<RectTransform>(), 400);
            controller.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -180);

            var bannerRoot = CreateUIChild(controller.transform, "BannerRoot");
            AnchorStretchAll(bannerRoot.GetComponent<RectTransform>());

            var image = CreateImage("Image", bannerRoot.transform, bannerSprite, Color.white);
            AnchorStretchAll(image.GetComponent<RectTransform>());

            var titleText = CreateText(bannerRoot.transform, "TitleText", "Event", 36, TextAlignmentOptions.Midline);
            AnchorStretchAll(titleText.rectTransform);

            AssignSerializedField(controller, "bannerRoot", bannerRoot);
            AssignSerializedField(controller, "titleText", titleText);
            AssignSerializedField(controller, "bannerImage", image);

            var eventManager = Object.FindObjectOfType<EventManager>(true);
            if (eventManager != null) AssignSerializedField(controller, "eventManager", eventManager);

            bannerRoot.SetActive(false);
        }

        // ---------- Welcome back popup ----------

        private static void BuildWelcomeBackPopup(Canvas canvas, Sprite panelSprite, Sprite buttonSprite)
        {
            var controller = Object.FindObjectOfType<WelcomeBackPopupController>(true);
            if (controller == null) return;
            ClearChildren(controller.transform);

            AnchorModal(controller.GetComponent<RectTransform>());

            var popupRoot = CreateUIChild(controller.transform, "PopupRoot");
            AnchorStretchAll(popupRoot.GetComponent<RectTransform>());

            var bg = CreateImage("Background", popupRoot.transform, panelSprite, Color.white);
            AnchorStretchAll(bg.GetComponent<RectTransform>());

            CreateTitle(popupRoot.transform, "Welcome Back!");

            var messageText = CreateText(popupRoot.transform, "MessageText", "You earned gold while away.", 32, TextAlignmentOptions.Midline);
            AnchorCenterBox(messageText.rectTransform, 700, 300, 0, -40);

            var watchAdButton = CreateButton(popupRoot.transform, "WatchAdButton", buttonSprite, "Watch Ad to Double", 28);
            AnchorCenterBox(watchAdButton.GetComponent<RectTransform>(), 500, 120, 0, -220);

            var closeButton = CreateButton(popupRoot.transform, "CloseButton", buttonSprite, "Close", 28);
            AnchorCenterBox(closeButton.GetComponent<RectTransform>(), 500, 120, 0, -370);

            AssignSerializedField(controller, "popupRoot", popupRoot);
            AssignSerializedField(controller, "messageText", messageText);
            AssignSerializedField(controller, "watchAdToDoubleButton", watchAdButton);
            AssignSerializedField(controller, "closeButton", closeButton);
        }

        // ---------- Daily reward popup ----------

        private static void BuildDailyRewardPopup(Canvas canvas, Sprite panelSprite, Sprite buttonSprite)
        {
            var controller = Object.FindObjectOfType<DailyRewardPopupController>(true);
            if (controller == null) return;
            ClearChildren(controller.transform);

            AnchorModal(controller.GetComponent<RectTransform>());

            var popupRoot = CreateUIChild(controller.transform, "PopupRoot");
            AnchorStretchAll(popupRoot.GetComponent<RectTransform>());

            var bg = CreateImage("Background", popupRoot.transform, panelSprite, Color.white);
            AnchorStretchAll(bg.GetComponent<RectTransform>());

            CreateTitle(popupRoot.transform, "Daily Reward");

            var streakText = CreateText(popupRoot.transform, "StreakText", "Day 1 login reward!", 32, TextAlignmentOptions.Midline);
            AnchorCenterBox(streakText.rectTransform, 700, 200, 0, -60);

            var claimButton = CreateButton(popupRoot.transform, "ClaimButton", buttonSprite, "Claim", 30);
            AnchorCenterBox(claimButton.GetComponent<RectTransform>(), 500, 120, 0, -280);

            AssignSerializedField(controller, "popupRoot", popupRoot);
            AssignSerializedField(controller, "streakText", streakText);
            AssignSerializedField(controller, "claimButton", claimButton);
        }

        // ---------- Goblin health bar ----------

        private static void BuildGoblinHealthBar(Canvas canvas, Sprite fillSprite, Sprite borderSprite)
        {
            var controller = Object.FindObjectOfType<GoblinHealthBarController>(true);
            if (controller == null) return;
            ClearChildren(controller.transform);

            var rt = controller.GetComponent<RectTransform>();
            AnchorTopCenterBox(rt, 500, 60, 0, -340);

            var barRoot = CreateUIChild(controller.transform, "BarRoot");
            AnchorStretchAll(barRoot.GetComponent<RectTransform>());

            var border = CreateImage("Border", barRoot.transform, borderSprite, Color.white);
            AnchorStretchAll(border.GetComponent<RectTransform>());

            var fillGo = CreateImage("Fill", barRoot.transform, fillSprite, Color.white);
            AnchorStretchAll(fillGo.GetComponent<RectTransform>());
            var fillImage = fillGo.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 1f;

            AssignSerializedField(controller, "barRoot", barRoot);
            AssignSerializedField(controller, "fillImage", fillImage);

            barRoot.SetActive(false);
        }

        // ---------- Shared building blocks ----------

        private static RectTransform BuildScrollingContent(Transform parent)
        {
            var scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, false);
            AnchorCenterBox(scrollGo.GetComponent<RectTransform>(), 900, 1200, 0, -60);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            AnchorStretchAll(viewportGo.GetComponent<RectTransform>());
            viewportGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f); // near-transparent, needed for raycast/mask

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0, 0);

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportGo.GetComponent<RectTransform>();
            scrollRect.content = contentRt;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return contentRt;
        }

        private static void CreateTitle(Transform parent, string text)
        {
            var title = CreateText(parent, "TitleText", text, 48, TextAlignmentOptions.Top);
            AnchorStretchTop(title.rectTransform, 120);
        }

        private static void CreateCloseButton(Transform parent, Sprite closeSprite, GameObject panelToClose)
        {
            var button = CreateButton(parent, "CloseButton", closeSprite, "", 0);
            AnchorTopRightBox(button.GetComponent<RectTransform>(), 90, 90, -20, -20);
            UnityEventTools.AddBoolPersistentListener(button.onClick, panelToClose.SetActive, false);
        }

        private static GameObject CreateUIChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static GameObject CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Sliced;
            return go;
        }

        private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = new Color(0.25f, 0.15f, 0.05f);
            var rt = go.GetComponent<RectTransform>();
            AnchorStretchAll(rt);
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, Sprite sprite, string label, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;

            if (!string.IsNullOrEmpty(label))
            {
                var text = CreateText(go.transform, "Label", label, fontSize, TextAlignmentOptions.Midline);
                text.color = Color.white;
            }

            return go.GetComponent<Button>();
        }

        private static void AddFlexibleWidth(GameObject go, float flex)
        {
            var layout = go.AddComponent<LayoutElement>();
            layout.flexibleWidth = flex;
        }

        // ---------- Anchoring helpers (1080x1920 reference resolution) ----------

        private static void AnchorStretchAll(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AnchorStretchTop(RectTransform rt, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = Vector2.zero;
        }

        private static void AnchorStretchBottom(RectTransform rt, float height)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = Vector2.zero;
        }

        private static void AnchorLeft(RectTransform rt, float x, float width, float height)
        {
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(x, 0);
        }

        private static void AnchorRight(RectTransform rt, float x, float width, float height)
        {
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(-x, 0);
        }

        private static void AnchorHorizontalSlot(RectTransform rt, float xFraction, float width, float height)
        {
            rt.anchorMin = new Vector2(xFraction, 0.5f);
            rt.anchorMax = new Vector2(xFraction, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = Vector2.zero;
        }

        private static void AnchorTopRightBox(RectTransform rt, float width, float height, float x, float y)
        {
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(x, y);
        }

        private static void AnchorTopCenterBox(RectTransform rt, float width, float height, float x, float y)
        {
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(x, y);
        }

        private static void AnchorCenterBox(RectTransform rt, float width, float height, float x, float y)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(x, y);
        }

        private static void AnchorModal(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0.05f, 0.15f);
            rt.anchorMax = new Vector2(0.95f, 0.85f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AssignSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[UILayoutSetup] Could not find serialized field '{fieldName}' on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
