using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Score")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;

    [Header("Yeti Danger")]
    public Slider yetiDangerBar;
    public float dangerRiseSpeed = 6f;
    public float dangerFallSpeed = 1.2f;

    private float displayedDanger = 0f;

    [Header("Game Over")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreGameOverText;
    public Button restartButton;
    
    [Header("Start")]
    public Button startButton;

    private PlayerController player;
    private YetiChaser yeti;
    private float distance = 0f;
    private int bestScore = 0;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            player = GameManager.Instance.player;
            yeti = GameManager.Instance.yeti;
        }

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = "BEST: " + bestScore + " m";
        yetiDangerBar.value = 0f;

        restartButton.onClick.AddListener(RestartGame);
        startButton.onClick.AddListener(StartGame);
    }
    
    void StartGame()
    {
        distance = 0f;
        GameManager.Instance.SetState(GameState.Playing);
    }

    void Update()
    {
        UpdateYetiBar();
        UpdateScore();
    }

    void UpdateScore()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (player.IsDead()) return;

        distance += Time.deltaTime * 5f;
        int meters = Mathf.FloorToInt(distance);

        scoreText.text = meters + " m";
        bestScoreText.text = "BEST: " + Mathf.Max(meters, bestScore) + " m";
    }

    void UpdateYetiBar()
    {
        if (yeti == null || player == null) return;

        float targetDanger;

        if (player.IsDead())
        {
            displayedDanger = 1f;
            yetiDangerBar.value = 1f;
            return;
        }

        float groundY = player.GetGroundY();
        Vector2 playerPos = new Vector2(player.transform.position.x, groundY);
        float dist = Vector2.Distance(yeti.transform.position, playerPos);
        targetDanger = 1f - Mathf.Clamp01(dist / yeti.normalOffsetY);

        float speed = targetDanger > displayedDanger ? dangerRiseSpeed : dangerFallSpeed;
        displayedDanger = Mathf.MoveTowards(displayedDanger, targetDanger, Time.deltaTime * speed);

        yetiDangerBar.value = displayedDanger;
    }

    public void OnGameOver()
    {
        int meters = Mathf.FloorToInt(distance);

        if (meters > bestScore)
        {
            bestScore = meters;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOver();

        finalScoreText.text = meters + " m";
        bestScoreGameOverText.text = "BEST: " + bestScore + " m";
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
