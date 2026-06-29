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
    public float jumpScaleBoost = 0.3f;
    private bool isJumping = false;
    private float jumpTimer = 0f;

    [Header("Baisse")]
    public float duckScale = 0.6f;
    public float duckDuration = 0.5f;
    private bool isDucking = false;
    private float duckTimer = 0f;

    [Header("Rendu")]
    public string duckSortingLayer = "Props";

    [Header("Collision Layers")]
    public int jumpingLayer;
    public int duckingLayer;

    [Header("Trainées")]
    public float trailTiltAngle = 15f;
    public float trailUprightSpeed = 10f;

    [Header("Mort")]
    public float deathSpinSpeed = 360f;

    [Header("Hit System")]
    public float slowMultiplier = 0.4f;
    public float slowDuration = 3f;
    public float hitShakeDuration = 0.45f;
    public float hitShakeIntensity = 0.25f;
    public float hitShakeFrequency = 28f;
    private float slowTimer = 0f;
    private float hitShakeTimer = 0f;
    private float hitShakeElapsed;
    private Vector3 lastShakeOffset;
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
    private int normalLayer;
    private int normalSortingLayerId;
    private int normalSortingOrder;

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

        normalLayer = gameObject.layer;
        normalSortingLayerId = sr.sortingLayerID;
        normalSortingOrder = sr.sortingOrder;
    }

    void Update()
    {
        if (!isDead)
        {
            MoveLane();
            UpdateJump();
            UpdateDuck();
            UpdateSlow();
        }
        UpdateTrails();
        UpdateHitShake();
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
            TiltTrails(-trailTiltAngle);
        }
        if (input.x > 0 && currentLane < 2)
        {
            currentLane++;
            targetX = (currentLane - 1) * laneWidth;
            TiltTrails(trailTiltAngle);
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
                Time.deltaTime * trailUprightSpeed
            );
        }
    }

    public void OnJump(InputValue value)
    {
        if (isDead || !value.isPressed || isJumping || isDucking) return;

        isJumping = true;
        jumpTimer = 0f;
    }

    public void OnCrouch(InputValue value)
    {
        if (isDead || !value.isPressed || isJumping || isDucking) return;

        isDucking = true;
        duckTimer = 0f;
    }

    // ── SAUT ────────────────────────────────────────────────────
    void UpdateJump()
    {
        if (!isJumping) return;

        if (jumpTimer == 0f)
            gameObject.layer = jumpingLayer;

        jumpTimer += Time.deltaTime;
        float ratio = jumpTimer / jumpDuration;
        float arc = Mathf.Sin(ratio * Mathf.PI);

        Vector3 pos = transform.position;
        pos.y = baseY + arc * jumpHeight;
        transform.position = pos;

        float scaleBoost = 1f + arc * jumpScaleBoost;
        transform.localScale = baseScale * scaleBoost;

        if (jumpTimer >= jumpDuration)
        {
            isJumping = false;
            jumpTimer = 0f;
            pos.y = baseY;
            transform.position = pos;
            transform.localScale = baseScale;
            gameObject.layer = normalLayer;

            if (!isDead)
                SpawnLandingPuff();
        }
    }

    // ── BAISSE ──────────────────────────────────────────────────
    void UpdateDuck()
    {
        if (!isDucking) return;

        if (duckTimer == 0f)
        {
            gameObject.layer = duckingLayer;
            ApplyDuckSorting();
            if (!isDead)
                SpawnDuckPuff();
        }

        duckTimer += Time.deltaTime;
        float ratio = duckTimer / duckDuration;
        float scale = ratio < 0.5f
            ? Mathf.Lerp(1f, duckScale, ratio * 2f)
            : Mathf.Lerp(duckScale, 1f, (ratio - 0.5f) * 2f);

        transform.localScale = baseScale * scale;

        if (duckTimer >= duckDuration)
        {
            isDucking = false;
            duckTimer = 0f;
            transform.localScale = baseScale;
            RestoreNormalSorting();
            gameObject.layer = normalLayer;
        }
    }

    void ApplyDuckSorting()
    {
        sr.sortingLayerName = duckSortingLayer;
    }

    void RestoreNormalSorting()
    {
        sr.sortingLayerID = normalSortingLayerId;
        sr.sortingOrder = normalSortingOrder;
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

        hitShakeTimer = hitShakeDuration;
        hitShakeElapsed = 0f;

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
        hitShakeTimer = 0f;
        transform.position -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;
        transform.localScale = baseScale;
        gameObject.layer = normalLayer;
        RestoreNormalSorting();

        Vector3 pos = transform.position;
        pos.y = baseY;
        transform.position = pos;

        DestroyActivePuffs();
        Debug.Log("Mort !");
    }

    void UpdateDeath()
    {
        if (!isDead) return;
    
        deathTimer += Time.deltaTime;
    
        if (deathTimer < deathDuration)
        {
            transform.Rotate(0, 0, -deathSpinSpeed * Time.deltaTime);
        }
        else if (!gamePaused)
        {
            transform.rotation = Quaternion.identity;
            gamePaused = true;
            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.GameOver);
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

    void UpdateHitShake()
    {
        transform.position -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;

        if (hitShakeTimer <= 0f) return;

        hitShakeTimer -= Time.deltaTime;
        hitShakeElapsed += Time.deltaTime;
        float damper = hitShakeTimer / hitShakeDuration;
        float wave = hitShakeElapsed * hitShakeFrequency * Mathf.PI * 2f;
        lastShakeOffset = new Vector3(
            Mathf.Sin(wave),
            Mathf.Sin(wave * 1.37f),
            0f) * hitShakeIntensity * damper;
        transform.position += lastShakeOffset;
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