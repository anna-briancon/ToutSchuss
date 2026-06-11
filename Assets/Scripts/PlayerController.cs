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

    [Header("Effets")]
    public GameObject puffPrefab;
    public GameObject duckPuffPrefab;
    public float landingPuffDuration = 0.75f;
    public float duckPuffDuration = 1.1f;
    private float trailHideTimer;
    private GameObject activeLandingPuff;
    private GameObject activeDuckPuff;
    
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
            TiltTrails(-15f); // Incline à gauche quand on va à gauche
        }
        if (input.x > 0 && currentLane < 2)
        {
            currentLane++;
            targetX = (currentLane - 1) * laneWidth;
            TiltTrails(15f); // Incline à droite quand on va à droite
        }
    }

    void TiltTrails(float angle)
    {
        if (skiTrails == null) return;
        skiTrails.transform.localRotation = Quaternion.Euler(0, 0, angle);
    }

    void MoveLane()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * laneSwitchSpeed);
        transform.position = pos;

        // Remet la trainée droite quand on est arrivé
        if (Mathf.Abs(pos.x - targetX) < 0.05f && skiTrails != null)
        {
            skiTrails.transform.localRotation = Quaternion.Lerp(
                skiTrails.transform.localRotation,
                Quaternion.identity,
                Time.deltaTime * 10f
            );
        }
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

            if (!isDead)
                SpawnLandingPuff();
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

        if (duckTimer == 0f)
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerDucking");
            if (!isDead)
                SpawnDuckPuff();
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
             
            gameObject.layer = LayerMask.NameToLayer("PlayerNormal");
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
        isJumping = false;
        isDucking = false;
        jumpTimer = 0f;
        trailHideTimer = 0f;
        isSlowed = false;
        slowTimer = 0f;
        transform.localScale = baseScale;
        gameObject.layer = LayerMask.NameToLayer("PlayerNormal");

        Vector3 pos = transform.position;
        pos.y = baseY;
        transform.position = pos;

        DestroyActivePuffs();

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
    
    void SpawnLandingPuff()
    {
        if (puffPrefab == null || isDead) return;

        if (activeLandingPuff != null)
            Destroy(activeLandingPuff);

        activeLandingPuff = Instantiate(puffPrefab, transform);
        Destroy(activeLandingPuff, landingPuffDuration);
        trailHideTimer = Mathf.Max(trailHideTimer, landingPuffDuration);
    }

    void SpawnDuckPuff()
    {
        if (duckPuffPrefab == null || isDead) return;

        if (activeDuckPuff != null)
            Destroy(activeDuckPuff);

        activeDuckPuff = Instantiate(duckPuffPrefab, transform);
        Destroy(activeDuckPuff, duckPuffDuration);
        trailHideTimer = Mathf.Max(trailHideTimer, duckPuffDuration);
    }

    void DestroyActivePuffs()
    {
        if (activeLandingPuff != null)
        {
            Destroy(activeLandingPuff);
            activeLandingPuff = null;
        }

        if (activeDuckPuff != null)
        {
            Destroy(activeDuckPuff);
            activeDuckPuff = null;
        }
    }

    void UpdateTrails()
    {
        if (skiTrails == null) return;

        if (trailHideTimer > 0f)
            trailHideTimer -= Time.deltaTime;

        skiTrails.SetActive(!isJumping && !isDead && !isDucking && trailHideTimer <= 0f);
    }

    // ── GETTERS ─────────────────────────────────────────────────
    public bool IsJumping() { return isJumping; }
    public bool IsDucking() { return isDucking; }
    public bool IsDead() { return isDead; }
    public float GetGroundY() { return baseY; }
}