using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HaKien;

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
        [SerializeField] private Button trainingButton;
 
        [Header("Tutorial Configuration")]
        [SerializeField] private TutorialScenarioData[] availableScenarios;
        [Tooltip("Exact name of the dedicated scene for tutorial battles.")]
        [SerializeField] private string tutorialSceneName = "TutorialBattle";
 
        [Header("Scene Configuration")]
        [Tooltip("Exact name of the scene to load when clicking Play.")]
        [SerializeField] private string nextSceneName = "InGame";
 
        private void Start()
        {
            MessageManager.Instance.SendMessage(new Message(MessageType.OnMainMenuEnter));

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (achievementButton != null)
                achievementButton.onClick.AddListener(OnAchievementClicked);

            if (trainingButton != null)
                trainingButton.onClick.AddListener(OnTrainingClicked);
        }

        private void OnDestroy()
        {
            // Unregister listeners to avoid memory leaks or issues with scene reloads
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitClicked);
            if (achievementButton != null) achievementButton.onClick.RemoveListener(OnAchievementClicked);
            if (trainingButton != null) trainingButton.onClick.RemoveListener(OnTrainingClicked);
        }

        public void OnPlayClicked()
        {
            Debug.Log("[Menu] Loading: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }

        public void OnTrainingClicked()
        {
            if (availableScenarios != null && availableScenarios.Length > 0)
            {
                Debug.Log("[Menu] Starting Tutorial: " + availableScenarios[0].scenarioTitle);
                TutorialBattleBootstrapper.PendingScenario = availableScenarios[0];
                SceneManager.LoadScene(tutorialSceneName);
            }
            else
            {
                Debug.LogError("[Menu] No tutorial scenarios assigned to MainMenuController!");
            }
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
