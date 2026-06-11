using UnityEngine;

public enum GameState { Menu, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public PlayerController player;
    public YetiChaser yeti;

    [Header("UI")]
    public GameObject startPanel;
    public GameObject gameOverPanel;

    private Animator playerAnimator;
    private UIManager uiManager;
    public GameState State { get; private set; } = GameState.Menu;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        uiManager = GetComponent<UIManager>();
    }

    void Start()
    {
        if (player != null)
            playerAnimator = player.GetComponent<Animator>();

        SetState(GameState.Menu);
    }

    public void SetState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 1f;
                SetGameplayActive(false);
                if (startPanel != null) startPanel.SetActive(true);
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMenuMusic();
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                if (startPanel != null) startPanel.SetActive(false);
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                SetGameplayActive(true);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayGameMusic();
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                SetGameplayActive(false);
                if (startPanel != null) startPanel.SetActive(false);
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                if (uiManager != null)
                    uiManager.OnGameOver();
                break;
        }
    }

    void SetGameplayActive(bool active)
    {
        if (player != null)
            player.enabled = active;

        if (playerAnimator != null)
            playerAnimator.enabled = active;

        if (yeti != null)
            yeti.enabled = active;

        if (player != null && player.skiTrails != null)
            player.skiTrails.SetActive(active);
    }

    public bool IsPlaying => State == GameState.Playing;
}
