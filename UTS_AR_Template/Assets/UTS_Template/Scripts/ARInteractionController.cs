using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UTSTemplate
{
    [DisallowMultipleComponent]
    public class ARInteractionController : MonoBehaviour
    {
        [Header("Referensi Vuforia")]
        [Tooltip("ImageTarget yang ada di scene. Jika kosong, dicari otomatis.")]
        public ImageTargetBehaviour imageTarget;

        [Tooltip("Root objek 3D yang diputar dan di-zoom. Jika kosong, dicari otomatis.")]
        public Transform targetModel;

        [Header("Referensi UI")]
        public Slider rotationSlider;
        public Text rotationValueText;
        public Button zoomInButton;
        public Button zoomOutButton;
        public Button resetButton;
        public Button backButton;
        public Text statusText;

        [Header("Pengaturan Interaksi")]
        [Range(0.05f, 0.95f)]
        public float minScaleMultiplier = 0.25f;

        [Range(1.05f, 10f)]
        public float maxScaleMultiplier = 4f;

        [Range(1.01f, 2f)]
        public float buttonZoomStep = 1.15f;

        [Range(0.1f, 3f)]
        public float pinchSensitivity = 1f;

        [Range(0.01f, 0.5f)]
        public float scrollZoomStep = 0.1f;

        [Header("Pengaturan Scene")]
        public string mainMenuScene = "MainMenu";
        public bool forcePortrait = true;

        private Vector3 baseScale;
        private Quaternion baseRotation;
        private Vector3 basePosition;
        private bool hasBaseTransform;
        private bool pinching;
        private float lastPinchDistance;
        private bool uiWired;

        void Awake()
        {
            if (forcePortrait)
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }

            if (imageTarget == null)
            {
                imageTarget = Object.FindFirstObjectByType<ImageTargetBehaviour>(FindObjectsInactive.Exclude);
            }

            if (targetModel == null)
            {
                targetModel = FindTargetModel();
            }

            if (rotationSlider == null) rotationSlider = FindSlider("SliderRotasi");
            if (rotationValueText == null) rotationValueText = FindText("TeksNilaiRotasi");
            if (zoomInButton == null) zoomInButton = FindButton("TombolZoomIn");
            if (zoomOutButton == null) zoomOutButton = FindButton("TombolZoomOut");
            if (resetButton == null) resetButton = FindButton("TombolReset");
            if (backButton == null) backButton = FindButton("TombolKembali");
            if (statusText == null) statusText = FindText("TeksStatus");

            if (targetModel != null)
            {
                baseScale = targetModel.localScale;
                baseRotation = targetModel.localRotation;
                basePosition = targetModel.localPosition;
                hasBaseTransform = true;
            }
            else
            {
                Debug.LogWarning(
                    "[UTS AR] targetModel tidak ditemukan. Pastikan ada objek 'Konten3D' di bawah ImageTarget."
                );
            }

            WireUI();

            if (imageTarget != null)
            {
                imageTarget.OnTargetStatusChanged += HandleTargetStatusChanged;
            }
            else
            {
                Debug.LogWarning("[UTS AR] ImageTargetBehaviour tidak ditemukan di scene.");
            }

            UpdateStatus(imageTarget != null && IsTracked(imageTarget.TargetStatus));
        }

        void Start()
        {
            if (rotationSlider != null)
            {
                OnRotationChanged(rotationSlider.value);
            }
            else
            {
                UpdateRotationValueText(GetCurrentRotation());
            }
        }

        void OnDestroy()
        {
            if (imageTarget != null)
            {
                imageTarget.OnTargetStatusChanged -= HandleTargetStatusChanged;
            }

            UnwireUI();
        }

        void Update()
        {
            HandlePinchAndScroll();
        }

        public void OnRotationChanged(float value)
        {
            if (targetModel == null)
            {
                return;
            }

            targetModel.localRotation = baseRotation * Quaternion.Euler(0f, value, 0f);
            UpdateRotationValueText(value);
        }

        public void ZoomIn()
        {
            ZoomBy(buttonZoomStep);
        }

        public void ZoomOut()
        {
            ZoomBy(1f / buttonZoomStep);
        }

        public void ZoomBy(float factor)
        {
            if (targetModel == null || !hasBaseTransform)
            {
                return;
            }

            if (factor <= 0f || Mathf.Approximately(factor, 1f))
            {
                return;
            }

            float baseSize = Mathf.Max(baseScale.x, 0.0001f);
            float minSize = baseSize * minScaleMultiplier;
            float maxSize = baseSize * maxScaleMultiplier;

            Vector3 current = targetModel.localScale;
            float currentSize = Mathf.Max(current.x, 0.0001f);
            float nextSize = Mathf.Clamp(currentSize * factor, minSize, maxSize);
            float ratio = nextSize / currentSize;

            if (!Mathf.Approximately(ratio, 1f))
            {
                targetModel.localScale = current * ratio;
            }
        }

        public void ResetTransform()
        {
            if (targetModel == null || !hasBaseTransform)
            {
                return;
            }

            targetModel.localScale = baseScale;
            targetModel.localRotation = baseRotation;
            targetModel.localPosition = basePosition;

            if (rotationSlider != null)
            {
                rotationSlider.SetValueWithoutNotify(0f);
            }

            UpdateRotationValueText(0f);
        }

        public void BackToMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(mainMenuScene))
            {
                Debug.LogError(
                    "[UTS AR] Scene '" + mainMenuScene + "' belum terdaftar di Build Settings. " +
                    "Jalankan menu UTS AR > Build Template Scenes atau tambahkan MainMenu secara manual."
                );
                return;
            }

            SceneManager.LoadScene(mainMenuScene);
        }

        void HandlePinchAndScroll()
        {
#if ENABLE_INPUT_SYSTEM
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                var first = touchscreen.touches[0];
                var second = touchscreen.touches[1];

                if (first.press.isPressed && second.press.isPressed)
                {
                    Vector2 firstPosition = first.position.ReadValue();
                    Vector2 secondPosition = second.position.ReadValue();
                    float distance = Vector2.Distance(firstPosition, secondPosition);

                    if (!pinching)
                    {
                        pinching = true;
                        lastPinchDistance = distance;
                    }
                    else if (distance > 1f && lastPinchDistance > 1f)
                    {
                        float delta = distance / lastPinchDistance;

                        if (!Mathf.Approximately(delta, 1f))
                        {
                            ZoomBy(Mathf.Pow(delta, pinchSensitivity));
                            lastPinchDistance = distance;
                        }
                    }
                }
                else
                {
                    pinching = false;
                }
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;

                if (Mathf.Abs(scroll) > 0.01f)
                {
                    ZoomBy(1f + Mathf.Sign(scroll) * scrollZoomStep);
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount >= 2)
            {
                Touch first = Input.GetTouch(0);
                Touch second = Input.GetTouch(1);
                float distance = Vector2.Distance(first.position, second.position);

                if (!pinching)
                {
                    pinching = true;
                    lastPinchDistance = distance;
                }
                else if (distance > 1f && lastPinchDistance > 1f)
                {
                    float delta = distance / lastPinchDistance;

                    if (!Mathf.Approximately(delta, 1f))
                    {
                        ZoomBy(Mathf.Pow(delta, pinchSensitivity));
                        lastPinchDistance = distance;
                    }
                }
            }
            else
            {
                pinching = false;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ZoomBy(1f + Mathf.Sign(scroll) * scrollZoomStep);
            }
#endif
        }

        void HandleTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
        {
            UpdateStatus(IsTracked(status));
        }

        static bool IsTracked(TargetStatus status)
        {
            return status.Status == Status.TRACKED ||
                   status.Status == Status.EXTENDED_TRACKED;
        }

        void UpdateStatus(bool tracked)
        {
            if (statusText == null)
            {
                return;
            }

            if (tracked)
            {
                statusText.text = "Marker terdeteksi";
                statusText.color = new Color(0.20f, 0.85f, 0.58f, 1f);
            }
            else
            {
                statusText.text = "Arahkan kamera ke marker";
                statusText.color = new Color(0.78f, 0.85f, 0.93f, 1f);
            }
        }

        void UpdateRotationValueText(float value)
        {
            if (rotationValueText != null)
            {
                rotationValueText.text = Mathf.RoundToInt(value) + " deg";
            }
        }

        float GetCurrentRotation()
        {
            if (!hasBaseTransform || targetModel == null)
            {
                return 0f;
            }

            Quaternion delta = Quaternion.Inverse(baseRotation) * targetModel.localRotation;
            return Mathf.DeltaAngle(0f, delta.eulerAngles.y);
        }

        Transform FindTargetModel()
        {
            if (imageTarget == null)
            {
                return null;
            }

            Transform direct = imageTarget.transform.Find("Konten3D");
            if (direct != null)
            {
                return direct;
            }

            foreach (Transform child in imageTarget.transform)
            {
                if (child.GetComponentInChildren<Renderer>(true) != null)
                {
                    return child;
                }
            }

            return null;
        }

        static Slider FindSlider(string objectName)
        {
            var sliders = Object.FindObjectsByType<Slider>(FindObjectsSortMode.None);

            foreach (var slider in sliders)
            {
                if (slider.name == objectName)
                {
                    return slider;
                }
            }

            return null;
        }

        static Button FindButton(string objectName)
        {
            var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (var button in buttons)
            {
                if (button.name == objectName)
                {
                    return button;
                }
            }

            return null;
        }

        static Text FindText(string objectName)
        {
            var texts = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);

            foreach (var text in texts)
            {
                if (text.name == objectName)
                {
                    return text;
                }
            }

            return null;
        }

        void WireUI()
        {
            if (uiWired)
            {
                return;
            }

            uiWired = true;

            if (rotationSlider != null)
            {
                rotationSlider.minValue = 0f;
                rotationSlider.maxValue = 360f;
                rotationSlider.wholeNumbers = false;
                rotationSlider.onValueChanged.AddListener(OnRotationChanged);
            }

            if (zoomInButton != null) zoomInButton.onClick.AddListener(ZoomIn);
            if (zoomOutButton != null) zoomOutButton.onClick.AddListener(ZoomOut);
            if (resetButton != null) resetButton.onClick.AddListener(ResetTransform);
            if (backButton != null) backButton.onClick.AddListener(BackToMenu);
        }

        void UnwireUI()
        {
            if (!uiWired)
            {
                return;
            }

            uiWired = false;

            if (rotationSlider != null) rotationSlider.onValueChanged.RemoveListener(OnRotationChanged);
            if (zoomInButton != null) zoomInButton.onClick.RemoveListener(ZoomIn);
            if (zoomOutButton != null) zoomOutButton.onClick.RemoveListener(ZoomOut);
            if (resetButton != null) resetButton.onClick.RemoveListener(ResetTransform);
            if (backButton != null) backButton.onClick.RemoveListener(BackToMenu);
        }
    }
}
