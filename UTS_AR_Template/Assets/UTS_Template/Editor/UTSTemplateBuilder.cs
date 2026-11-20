using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace UTSTemplate.EditorTools
{
    public static class UTSTemplateBuilder
    {
        private const string RootFolder = "Assets/UTS_Template";
        private const string ScenesFolder = RootFolder + "/Scenes";
        private const string MaterialsFolder = RootFolder + "/Materials";
        private const string MainMenuPath = ScenesFolder + "/MainMenu.unity";
        private const string ARScenePath = ScenesFolder + "/ARScene.unity";

        private const string DataSetPath = "Vuforia/DataRafi.xml";
        private const string TargetName = "cover-sui";
        private const float TargetWidth = 20f;
        private const float TargetHeight = 31.846153f;

        private static readonly Color BackgroundColor = new Color(0.043f, 0.071f, 0.125f, 1f);
        private static readonly Color CardColor = new Color(0.067f, 0.110f, 0.180f, 0.97f);
        private static readonly Color AccentColor = new Color(0.133f, 0.827f, 0.933f, 1f);
        private static readonly Color SuccessColor = new Color(0.200f, 0.850f, 0.580f, 1f);
        private static readonly Color LightTextColor = new Color(0.900f, 0.930f, 0.970f, 1f);
        private static readonly Color MutedTextColor = new Color(0.580f, 0.650f, 0.750f, 1f);
        private static readonly Color ButtonDarkColor = new Color(0.118f, 0.161f, 0.231f, 1f);

        private static Sprite panelSprite;
        private static Sprite knobSprite;
        private static Font uiFont;

        [MenuItem("UTS AR/Build Template Scenes", false, 0)]
        public static void BuildTemplateScenes()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("UTS AR", "Keluar dari Play Mode terlebih dahulu.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolders();
            LoadEditorResources();

            bool mainMenuExists = File.Exists(MainMenuPath);
            bool arSceneExists = File.Exists(ARScenePath);

            if (mainMenuExists || arSceneExists)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Timpa Scene Template?",
                    "Scene MainMenu atau ARScene sudah ada. Timpa dengan versi baru?",
                    "Ya, timpa",
                    "Batal"
                );

                if (!overwrite)
                {
                    return;
                }
            }

            CreateMainMenuScene();
            CreateARScene();
            RegisterBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(MainMenuPath);

            EditorUtility.DisplayDialog(
                "UTS AR",
                "Template berhasil dibuat.\n\n" +
                "1. MainMenu.unity sudah dibuka.\n" +
                "2. Tekan Play untuk mencoba menu.\n" +
                "3. Klik MULAI AR untuk masuk ke scene AR.\n\n" +
                "Panduan lengkap ada di Assets/UTS_Template/README.md",
                "Siap"
            );
        }

        [MenuItem("UTS AR/Validate Setup", false, 20)]
        public static void ValidateSetup()
        {
            var lines = new List<string>();

            lines.Add(Resources.Load("VuforiaConfiguration") != null
                ? "[OK] VuforiaConfiguration ditemukan."
                : "[X] VuforiaConfiguration tidak ditemukan di Assets/Resources.");

            lines.Add(File.Exists("Assets/StreamingAssets/" + DataSetPath)
                ? "[OK] Dataset ditemukan: Assets/StreamingAssets/" + DataSetPath
                : "[X] Dataset tidak ditemukan: Assets/StreamingAssets/" + DataSetPath);

            lines.Add(File.Exists(MainMenuPath)
                ? "[OK] MainMenu.unity ada."
                : "[X] MainMenu.unity belum dibuat.");

            lines.Add(File.Exists(ARScenePath)
                ? "[OK] ARScene.unity ada."
                : "[X] ARScene.unity belum dibuat.");

            lines.Add(IsSceneInBuild(MainMenuPath)
                ? "[OK] MainMenu terdaftar di Build Settings."
                : "[X] MainMenu belum terdaftar di Build Settings.");

            lines.Add(IsSceneInBuild(ARScenePath)
                ? "[OK] ARScene terdaftar di Build Settings."
                : "[X] ARScene belum terdaftar di Build Settings.");

            Debug.Log("[UTS AR] Validasi:\n" + string.Join("\n", lines));
            EditorUtility.DisplayDialog("UTS AR - Validasi", string.Join("\n", lines), "OK");
        }

        [MenuItem("UTS AR/Open Main Menu Scene", false, 21)]
        public static void OpenMainMenuScene()
        {
            if (!File.Exists(MainMenuPath))
            {
                EditorUtility.DisplayDialog(
                    "UTS AR",
                    "MainMenu.unity belum dibuat. Jalankan UTS AR > Build Template Scenes.",
                    "OK"
                );
                return;
            }

            EditorSceneManager.OpenScene(MainMenuPath);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "UTS_Template");
            EnsureFolder(RootFolder, "Scenes");
            EnsureFolder(RootFolder, "Materials");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void LoadEditorResources()
        {
            panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (uiFont == null)
            {
                uiFont = Font.CreateDynamicFontFromOSFont("Arial", 24);
            }
        }

        private static void CreateMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Canvas canvas = CreateCanvas("Canvas", 1080f, 1920f);

            Image background = CreateImage("Background", canvas.transform, BackgroundColor);
            Stretch(background.rectTransform);

            Image accentTop = CreateImage(
                "AksenAtas",
                canvas.transform,
                new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.07f)
            );
            accentTop.sprite = knobSprite;
            accentTop.type = Image.Type.Simple;
            SetRect(
                accentTop.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(920f, 920f),
                new Vector2(230f, 230f)
            );

            Image accentBottom = CreateImage(
                "AksenBawah",
                canvas.transform,
                new Color(SuccessColor.r, SuccessColor.g, SuccessColor.b, 0.05f)
            );
            accentBottom.sprite = knobSprite;
            accentBottom.type = Image.Type.Simple;
            SetRect(
                accentBottom.rectTransform,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(0.5f, 0.5f),
                new Vector2(720f, 720f),
                new Vector2(-190f, -190f)
            );

            Image card = CreateImage("KartuMenu", canvas.transform, CardColor);
            card.sprite = panelSprite;
            card.type = Image.Type.Sliced;
            SetRect(
                card.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(880f, 1180f),
                new Vector2(0f, -20f)
            );

            Image accentLine = CreateImage("GarisAksen", card.transform, AccentColor);
            SetRect(
                accentLine.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(140f, 8f),
                new Vector2(0f, 500f)
            );

            Text title = CreateText(
                "JudulUtama",
                card.transform,
                "AR UTS",
                104,
                LightTextColor,
                TextAnchor.MiddleCenter,
                true
            );
            SetRect(
                title.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(780f, 140f),
                new Vector2(0f, 400f)
            );
            AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.45f), new Vector2(3f, -3f));

            Text subtitle = CreateText(
                "SubJudul",
                card.transform,
                "MARKER-BASED AUGMENTED REALITY",
                30,
                AccentColor,
                TextAnchor.MiddleCenter,
                true
            );
            SetRect(
                subtitle.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(760f, 50f),
                new Vector2(0f, 300f)
            );

            Image divider = CreateImage("Pemisah", card.transform, new Color(1f, 1f, 1f, 0.08f));
            SetRect(
                divider.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(720f, 2f),
                new Vector2(0f, 240f)
            );

            Text studentInfo = CreateText(
                "InfoMahasiswa",
                card.transform,
                "Nama       : ____________________________\n" +
                "NIM        : ____________________________\n" +
                "Kelas      : ____________________________\n" +
                "Mata Kuliah: Augmented Reality",
                30,
                MutedTextColor,
                TextAnchor.UpperLeft,
                false
            );
            studentInfo.lineSpacing = 1.5f;
            SetRect(
                studentInfo.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(700f, 260f),
                new Vector2(0f, 80f)
            );

            Button startButton = CreateButton(
                "TombolMulaiAR",
                card.transform,
                "MULAI AR",
                new Vector2(680f, 130f),
                new Vector2(0f, -200f),
                AccentColor,
                BackgroundColor,
                40
            );

            Button quitButton = CreateButton(
                "TombolKeluar",
                card.transform,
                "KELUAR",
                new Vector2(680f, 90f),
                new Vector2(0f, -350f),
                new Color(1f, 1f, 1f, 0.06f),
                MutedTextColor,
                28
            );

            Text hint = CreateText(
                "Petunjuk",
                card.transform,
                "Tekan MULAI AR, lalu arahkan kamera ke gambar marker.",
                24,
                MutedTextColor,
                TextAnchor.MiddleCenter,
                false
            );
            SetRect(
                hint.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(720f, 60f),
                new Vector2(0f, -480f)
            );

            GameObject controllerObject = new GameObject("MainMenuController");
            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();
            controller.startButton = startButton;
            controller.quitButton = quitButton;
            controller.arSceneName = "ARScene";
            controller.forcePortrait = true;

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, MainMenuPath);
        }

        private static void CreateARScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("ARCamera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 1000f;
            camera.fieldOfView = 60f;

            cameraObject.AddComponent<VuforiaBehaviour>();
            cameraObject.AddComponent<DefaultInitializationErrorHandler>();

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject targetObject = new GameObject("ImageTarget");
            ImageTargetBehaviour imageTarget = targetObject.AddComponent<ImageTargetBehaviour>();
            DefaultObserverEventHandler observerHandler = targetObject.AddComponent<DefaultObserverEventHandler>();
            observerHandler.StatusFilter = DefaultObserverEventHandler.TrackingStatusFilter.Tracked_ExtendedTracked;
            ConfigureImageTarget(imageTarget);

            Material contentMaterial = CreateContentMaterial();
            GameObject content = GameObject.CreatePrimitive(PrimitiveType.Cube);
            content.name = "Konten3D";
            content.transform.SetParent(targetObject.transform, false);
            content.transform.localPosition = new Vector3(0f, 4f, 0f);
            content.transform.localScale = Vector3.one * 8f;

            Renderer contentRenderer = content.GetComponent<Renderer>();
            if (contentMaterial != null)
            {
                contentRenderer.sharedMaterial = contentMaterial;
            }

            Canvas canvas = CreateCanvas("CanvasAR", 1080f, 1920f);

            Image statusBackground = CreateImage(
                "StatusBackground",
                canvas.transform,
                new Color(0.04f, 0.07f, 0.12f, 0.82f)
            );
            statusBackground.sprite = panelSprite;
            statusBackground.type = Image.Type.Sliced;
            SetRect(
                statusBackground.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(540f, 76f),
                new Vector2(0f, -40f)
            );

            Text statusText = CreateText(
                "TeksStatus",
                statusBackground.transform,
                "Arahkan kamera ke marker",
                28,
                MutedTextColor,
                TextAnchor.MiddleCenter,
                false
            );
            Stretch(statusText.rectTransform, 24f, 24f, 0f, 0f);

            Button backButton = CreateButton(
                "TombolKembali",
                canvas.transform,
                "KEMBALI",
                new Vector2(220f, 76f),
                Vector2.zero,
                new Color(0.04f, 0.07f, 0.12f, 0.82f),
                LightTextColor,
                26
            );
            SetRect(
                backButton.GetComponent<RectTransform>(),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(220f, 76f),
                new Vector2(40f, -40f)
            );

            Image controlPanel = CreateImage(
                "PanelKontrol",
                canvas.transform,
                new Color(0.04f, 0.07f, 0.12f, 0.90f)
            );
            controlPanel.sprite = panelSprite;
            controlPanel.type = Image.Type.Sliced;
            RectTransform controlPanelRect = controlPanel.rectTransform;
            controlPanelRect.anchorMin = new Vector2(0f, 0f);
            controlPanelRect.anchorMax = new Vector2(1f, 0f);
            controlPanelRect.pivot = new Vector2(0.5f, 0f);
            controlPanelRect.anchoredPosition = new Vector2(0f, 40f);
            controlPanelRect.sizeDelta = new Vector2(-80f, 430f);

            Text rotationLabel = CreateText(
                "LabelRotasi",
                controlPanel.transform,
                "ROTASI OBJEK",
                28,
                MutedTextColor,
                TextAnchor.MiddleLeft,
                true
            );
            SetRect(
                rotationLabel.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(400f, 40f),
                new Vector2(40f, -30f)
            );

            Text rotationValue = CreateText(
                "TeksNilaiRotasi",
                controlPanel.transform,
                "0 deg",
                28,
                AccentColor,
                TextAnchor.MiddleRight,
                true
            );
            SetRect(
                rotationValue.rectTransform,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(160f, 40f),
                new Vector2(-40f, -30f)
            );

            Slider rotationSlider = CreateSlider(
                "SliderRotasi",
                controlPanel.transform,
                new Vector2(0f, -80f),
                new Vector2(-80f, 60f),
                0f,
                360f,
                0f
            );

            Text zoomLabel = CreateText(
                "LabelZoom",
                controlPanel.transform,
                "ZOOM",
                28,
                MutedTextColor,
                TextAnchor.MiddleLeft,
                true
            );
            SetRect(
                zoomLabel.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(200f, 40f),
                new Vector2(40f, -170f)
            );

            Button zoomOutButton = CreateButton(
                "TombolZoomOut",
                controlPanel.transform,
                "-",
                new Vector2(110f, 80f),
                Vector2.zero,
                ButtonDarkColor,
                LightTextColor,
                40
            );
            SetRect(
                zoomOutButton.GetComponent<RectTransform>(),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(110f, 80f),
                new Vector2(-140f, -240f)
            );

            Button zoomInButton = CreateButton(
                "TombolZoomIn",
                controlPanel.transform,
                "+",
                new Vector2(110f, 80f),
                Vector2.zero,
                ButtonDarkColor,
                LightTextColor,
                40
            );
            SetRect(
                zoomInButton.GetComponent<RectTransform>(),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(110f, 80f),
                new Vector2(0f, -240f)
            );

            Button resetButton = CreateButton(
                "TombolReset",
                controlPanel.transform,
                "RESET",
                new Vector2(220f, 80f),
                Vector2.zero,
                ButtonDarkColor,
                LightTextColor,
                26
            );
            SetRect(
                resetButton.GetComponent<RectTransform>(),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(220f, 80f),
                new Vector2(250f, -240f)
            );

            Text hint = CreateText(
                "PetunjukAR",
                controlPanel.transform,
                "Cubit layar untuk zoom  |  Scroll mouse untuk zoom di Editor  |  Geser slider untuk rotasi",
                22,
                MutedTextColor,
                TextAnchor.MiddleCenter,
                false
            );
            SetRect(
                hint.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(940f, 40f),
                new Vector2(0f, -350f)
            );

            GameObject controllerObject = new GameObject("ARController");
            ARInteractionController controller = controllerObject.AddComponent<ARInteractionController>();
            controller.imageTarget = imageTarget;
            controller.targetModel = content.transform;
            controller.rotationSlider = rotationSlider;
            controller.rotationValueText = rotationValue;
            controller.zoomInButton = zoomInButton;
            controller.zoomOutButton = zoomOutButton;
            controller.resetButton = resetButton;
            controller.backButton = backButton;
            controller.statusText = statusText;
            controller.mainMenuScene = "MainMenu";
            controller.forcePortrait = true;

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ARScenePath);
        }

        private static void ConfigureImageTarget(ImageTargetBehaviour imageTarget)
        {
            SerializedObject serializedObject = new SerializedObject(imageTarget);

            SetString(serializedObject, "mTrackableName", TargetName);
            SetString(serializedObject, "mDataSetPath", DataSetPath);
            SetFloat(serializedObject, "mWidth", TargetWidth);
            SetFloat(serializedObject, "mHeight", TargetHeight);
            SetFloat(serializedObject, "mAspectRatio", TargetHeight / TargetWidth);

            SerializedProperty targetType = serializedObject.FindProperty("mImageTargetType");
            if (targetType != null)
            {
                targetType.enumValueIndex = (int)ImageTargetType.PREDEFINED;
            }

            SerializedProperty initialized = serializedObject.FindProperty("mInitializedInEditor");
            if (initialized != null)
            {
                initialized.boolValue = true;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static Material CreateContentMaterial()
        {
            string path = MaterialsFolder + "/Konten3D.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (existing != null)
            {
                return existing;
            }

            Shader shader = Shader.Find("Standard");

            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                Debug.LogWarning("[UTS AR] Shader Standard tidak ditemukan. Cube akan memakai material default.");
                return null;
            }

            Material material = new Material(shader);
            material.color = AccentColor;

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.25f);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0.65f);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Canvas CreateCanvas(string name, float referenceWidth, float referenceHeight)
        {
            GameObject canvasObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            return canvas;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return image;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string content,
            int fontSize,
            Color color,
            TextAnchor alignment,
            bool bold
        )
        {
            GameObject textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text)
            );

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.font = uiFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = true;

            return text;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 size,
            Vector2 position,
            Color backgroundColor,
            Color textColor,
            int fontSize
        )
        {
            GameObject buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button)
            );

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;

            Image image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;
            image.sprite = panelSprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.90f, 0.96f, 1f, 1f);
            colors.pressedColor = new Color(0.72f, 0.82f, 0.92f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            Text labelText = CreateText(
                "Label",
                buttonObject.transform,
                label,
                fontSize,
                textColor,
                TextAnchor.MiddleCenter,
                true
            );
            Stretch(labelText.rectTransform, 12f, 12f, 0f, 0f);

            return button;
        }

        private static Slider CreateSlider(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 sizeDelta,
            float minValue,
            float maxValue,
            float value
        )
        {
            GameObject sliderObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Slider)
            );

            RectTransform rectTransform = sliderObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = position;

            Image background = CreateImage("Background", sliderObject.transform, new Color(1f, 1f, 1f, 0.10f));
            background.sprite = panelSprite;
            background.type = Image.Type.Sliced;
            background.raycastTarget = true;
            SetRect(
                background.rectTransform,
                new Vector2(0f, 0.25f),
                new Vector2(1f, 0.75f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero
            );

            GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
            RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
            fillAreaRect.SetParent(sliderObject.transform, false);
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRect.offsetMin = new Vector2(8f, 0f);
            fillAreaRect.offsetMax = new Vector2(-18f, 0f);

            Image fill = CreateImage("Fill", fillAreaObject.transform, AccentColor);
            fill.sprite = panelSprite;
            fill.type = Image.Type.Sliced;
            Stretch(fill.rectTransform);

            GameObject handleAreaObject = new GameObject("Handle Slide Area", typeof(RectTransform));
            RectTransform handleAreaRect = handleAreaObject.GetComponent<RectTransform>();
            handleAreaRect.SetParent(sliderObject.transform, false);
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.pivot = new Vector2(0.5f, 0.5f);
            handleAreaRect.offsetMin = new Vector2(12f, 0f);
            handleAreaRect.offsetMax = new Vector2(-12f, 0f);

            Image handle = CreateImage("Handle", handleAreaObject.transform, Color.white);
            handle.sprite = knobSprite;
            handle.type = Image.Type.Simple;
            handle.raycastTarget = true;

            RectTransform handleRect = handle.rectTransform;
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(44f, 44f);
            handleRect.anchoredPosition = Vector2.zero;

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = value;
            slider.wholeNumbers = false;

            return slider;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));

#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static void Stretch(RectTransform rectTransform)
        {
            Stretch(rectTransform, 0f, 0f, 0f, 0f);
        }

        private static void Stretch(RectTransform rectTransform, float left, float right, float top, float bottom)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        private static void SetRect(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition
        )
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        private static void AddShadow(GameObject gameObject, Color color, Vector2 distance)
        {
            Shadow shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
        }

        private static void RegisterBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            scenes.Add(new EditorBuildSettingsScene(MainMenuPath, true));
            scenes.Add(new EditorBuildSettingsScene(ARScenePath, true));

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path != MainMenuPath && scene.path != ARScenePath)
                {
                    scenes.Add(scene);
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static bool IsSceneInBuild(string path)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == path && scene.enabled)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
