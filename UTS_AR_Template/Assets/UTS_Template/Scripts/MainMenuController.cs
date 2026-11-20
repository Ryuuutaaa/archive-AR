using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UTSTemplate
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Referensi Tombol")]
        public Button startButton;
        public Button quitButton;

        [Header("Pengaturan Scene")]
        public string arSceneName = "ARScene";

        [Header("Tampilan")]
        public bool forcePortrait = true;

        void Awake()
        {
            if (forcePortrait)
            {
                Screen.orientation = ScreenOrientation.Portrait;
            }

            if (startButton == null)
            {
                startButton = FindButton("TombolMulaiAR");
            }

            if (quitButton == null)
            {
                quitButton = FindButton("TombolKeluar");
            }

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartAR);
                startButton.onClick.AddListener(StartAR);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
                quitButton.onClick.AddListener(QuitGame);
            }
        }

        public void StartAR()
        {
            if (!Application.CanStreamedLevelBeLoaded(arSceneName))
            {
                Debug.LogError(
                    "[UTS AR] Scene '" + arSceneName + "' belum terdaftar di Build Settings. " +
                    "Jalankan menu UTS AR > Build Template Scenes atau tambahkan ARScene secara manual."
                );
                return;
            }

            SceneManager.LoadScene(arSceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
    }
}
