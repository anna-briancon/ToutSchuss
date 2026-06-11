using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Couloirs")]
    public float laneWidth = 1f;
    public float laneSwitchSpeed = 10f;
    private int currentLane = 1;
    private float targetX;

    [Header("Saut")]
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.3f;
    private bool isJumping = false;
    private float jumpTimer = 0f;

    [Header("Baisse")]
    public float duckScale = 0.6f;
    public float duckDuration = 0.5f;
    private bool isDucking = false;
    private float duckTimer = 0f;

    [Header("Sprites")]
    public Sprite spriteIdle;
    public Sprite spriteMoving;

    [Header("Hit System")]
    public float slowMultiplier = 0.4f;
    public float slowDuration = 3f;
    private float slowTimer = 0f;
    private int hitCount = 0;
    public bool isSlowed = false;
    private bool isDead = false;
    
    [Header("Ski Trails")]
    public GameObject skiTrails;
    
    [Header("Audio")]
    public AudioClip[] hitSounds;
    private AudioSource audioSource;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private float baseY;
    
    private float deathTimer = 0f;
    private float deathDuration = 2f;
    private bool gamePaused = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseY = transform.position.y;
        targetX = 0f;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isDead)
        {
            HandleJumpInput();
            HandleDuckInput();
            MoveLane();
            UpdateJump();
            UpdateDuck();
            UpdateSprite();
            UpdateSlow();
        }
        UpdateTrails();
        UpdateDeath();
    }

    // ── COULOIRS ────────────────────────────────────────────────
    public void OnMove(InputValue value)
    {
        if (isDead) return;

        Vector2 input = value.Get<Vector2>();

        if (input.x < 0 && currentLane > 0)
        {
            currentLane--;
            targetX = (currentLane - 1) * laneWidth;
        }
        if (input.x > 0 && currentLane < 2)
        {
            currentLane++;
            targetX = (currentLane - 1) * laneWidth;
        }
    }

    void MoveLane()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * laneSwitchSpeed);
        transform.position = pos;
    }

    // ── SAUT ────────────────────────────────────────────────────
    void HandleJumpInput()
    {
        if ((Keyboard.current.upArrowKey.wasPressedThisFrame ||
             Keyboard.current.wKey.wasPressedThisFrame ||
             Keyboard.current.spaceKey.wasPressedThisFrame)
            && !isJumping && !isDucking)
        {
            isJumping = true;
            jumpTimer = 0f;
        }
    }

    void UpdateJump()
    {
        if (!isJumping) return;

        if (jumpTimer == 0f)
            gameObject.layer = LayerMask.NameToLayer("PlayerJumping");

        jumpTimer += Time.deltaTime;
        float ratio = jumpTimer / jumpDuration;
        float arc = Mathf.Sin(ratio * Mathf.PI);

        Vector3 pos = transform.position;
        pos.y = baseY + arc * jumpHeight;
        transform.position = pos;

        float scaleBoost = 1f + arc * 0.3f;
        transform.localScale = baseScale * scaleBoost;

        if (jumpTimer >= jumpDuration)
        {
            isJumping = false;
            jumpTimer = 0f;
            pos.y = baseY;
            transform.position = pos;
            transform.localScale = baseScale;
            gameObject.layer = LayerMask.NameToLayer("PlayerNormal");
        }
    }

    // ── BAISSE ──────────────────────────────────────────────────
    void HandleDuckInput()
    {
        if ((Keyboard.current.downArrowKey.wasPressedThisFrame ||
             Keyboard.current.sKey.wasPressedThisFrame)
            && !isJumping && !isDucking)
        {
            isDucking = true;
            duckTimer = 0f;
        }
    }

    void UpdateDuck()
    {
        if (!isDucking) return;

        // Change le layer au début du duck
        if (duckTimer == 0f)
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerDucking");
            if (skiTrails != null)
                foreach (SpriteRenderer trailSr in skiTrails.GetComponentsInChildren<SpriteRenderer>())
                    trailSr.sortingLayerName = "Background";
        }

        duckTimer += Time.deltaTime;
        float ratio = duckTimer / duckDuration;
        float scale = ratio < 0.5f
            ? Mathf.Lerp(1f, duckScale, ratio * 2f)
            : Mathf.Lerp(duckScale, 1f, (ratio - 0.5f) * 2f);

        transform.localScale = baseScale * scale;
        sr.sortingLayerName = "Props";

        if (duckTimer >= duckDuration)
        {
            isDucking = false;
            duckTimer = 0f;
            transform.localScale = baseScale;
            sr.sortingLayerName = "Characters";
             
            // Remet le layer normal
            gameObject.layer = LayerMask.NameToLayer("PlayerNormal");
            
            if (skiTrails != null)
                foreach (SpriteRenderer trailSr in skiTrails.GetComponentsInChildren<SpriteRenderer>())
                    trailSr.sortingLayerName = "Characters";
        }
    }

    // ── SPRITE ──────────────────────────────────────────────────
    void UpdateSprite()
    {
        sr.sprite = spriteMoving;
    }

    // ── HIT SYSTEM ──────────────────────────────────────────────
    public void HitObstacle()
    {
        if (isDead) return;
        if (hitSounds.Length > 0)
        {
            AudioClip randomHit = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomHit);
        }
        hitCount++;

        if (hitCount >= 2)
        {
            Die();
        }
        else
        {
            ApplySlow();
        }
    }

    void ApplySlow()
    {
        isSlowed = true;
        slowTimer = slowDuration;
        Debug.Log("Ralenti !");
    }

    void Die()
    {
        isDead = true;
        isSlowed = false;
        slowTimer = 0f;
        sr.sprite = spriteIdle;
        Debug.Log("Mort !");
    }

    void UpdateDeath()
    {
        if (!isDead) return;
    
        deathTimer += Time.deltaTime;
    
        if (deathTimer < deathDuration)
        {
            transform.Rotate(0, 0, -360 * Time.deltaTime);
        }
        else if (!gamePaused)
        {
            transform.rotation = Quaternion.identity;
            gamePaused = true;
            Time.timeScale = 0f;
        }
    }

    void UpdateSlow()
    {
        if (!isSlowed || isDead) return;

        slowTimer -= Time.deltaTime;
        if (slowTimer <= 0f)
        {
            isSlowed = false;
            slowTimer = 0f;
            hitCount = 0;
            Debug.Log("Récupéré !");
        }
    }
    
    void UpdateTrails()
    {
        if (skiTrails == null) return;
        skiTrails.SetActive(!isJumping && !isDead);
    }

    // ── GETTERS ─────────────────────────────────────────────────
    public bool IsJumping() { return isJumping; }
    public bool IsDucking() { return isDucking; }
    public bool IsDead() { return isDead; }
    public float GetGroundY() { return baseY; }
}