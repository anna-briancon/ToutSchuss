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
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreGameOverText;
    public Button restartButton;
    
    [Header("Start")]
    public GameObject startPanel;
    public Button startButton;

    private PlayerController player;
    private YetiChaser yeti;
    private float distance = 0f;
    private int bestScore = 0;
    private bool gameOver = false;
    private bool gameStarted = false;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        yeti = FindObjectOfType<YetiChaser>();

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = "BEST: " + bestScore + " m";

        gameOverPanel.SetActive(false);
        yetiDangerBar.value = 0f;

        restartButton.onClick.AddListener(RestartGame);
        
        startPanel.SetActive(true);
        startButton.onClick.AddListener(StartGame);
        
        // Désactive le input du joueur au départ
        player.enabled = false;
    }
    
    void StartGame()
    {
        startPanel.SetActive(false);
        player.enabled = true;
        FindObjectOfType<WorldScroller>().isRunning = true;
        gameStarted = true;
        AudioManager.Instance.PlayGameMusic();
    }

    void Update()
    {
        UpdateYetiBar();

        if (gameOver) return;

        UpdateScore();
        CheckGameOver();
    }

    void UpdateScore()
    {
        if (!gameStarted) return;
        if (player.IsDead()) return;

        // La distance augmente avec le temps
        distance += Time.deltaTime * 5f; // 5 mètres par seconde
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
        else
        {
            float groundY = player.GetGroundY();
            Vector2 playerPos = new Vector2(player.transform.position.x, groundY);
            float dist = Vector2.Distance(yeti.transform.position, playerPos);
            targetDanger = 1f - Mathf.Clamp01(dist / yeti.normalOffsetY);
        }

        // Monte vite quand le yéti approche, redescend doucement quand il s'éloigne
        float speed = targetDanger > displayedDanger ? dangerRiseSpeed : dangerFallSpeed;
        displayedDanger = Mathf.MoveTowards(displayedDanger, targetDanger, Time.deltaTime * speed);

        yetiDangerBar.value = displayedDanger;
    }

    void CheckGameOver()
    {
        if (player.IsDead() && Time.timeScale == 0f)
        {
            gameOver = true;
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        int meters = Mathf.FloorToInt(distance);

        // Sauvegarde le meilleur score
        if (meters > bestScore)
        {
            bestScore = meters;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }
        AudioManager.Instance.PlayGameOver();

        finalScoreText.text = meters + " m";
        bestScoreGameOverText.text = "BEST: " + bestScore + " m";
        gameOverPanel.SetActive(true);
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}