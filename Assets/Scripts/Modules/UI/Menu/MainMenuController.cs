using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ProjectAlpha.UI.Menu
{
    /// <summary>
    /// Controls the main menu UI.
    /// Handles Play, Quit, and Achievement button interactions.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button achievementButton;

        [Header("Scene Configuration")]
        [Tooltip("Exact name of the scene to load when clicking Play.")]
        [SerializeField] private string nextSceneName = "InGame";

        private void Start()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (achievementButton != null)
                achievementButton.onClick.AddListener(OnAchievementClicked);
        }

        private void OnDestroy()
        {
            // Unregister listeners to avoid memory leaks or issues with scene reloads
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
            if (achievementButton != null) achievementButton.onClick.RemoveListener(OnAchievementClicked);
        }

        public void OnPlayClicked()
        {
            Debug.Log("[Menu] Loading: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }

        public void OnQuitClicked()
        {
            Debug.Log("[Menu] Quitting application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnAchievementClicked()
        {
            Debug.Log("[Menu] Achievement clicked. (Coming soon!)");
            // Placeholder for future implementation
        }
    }
}
