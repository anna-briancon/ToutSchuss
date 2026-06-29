using UnityEngine;

public class YetiChaser : MonoBehaviour
{
    [Header("Distance")]
    public float normalOffsetY = 5f;   // Distance au dessus du joueur en temps normal
    public float catchDistance = 0.8f; // Distance à laquelle il attrape le joueur

    [Header("Vitesse")]
    public float approachSpeed = 2f;   // Vitesse de rapprochement quand joueur ralenti
    public float retreatSpeed = 1f;    // Vitesse de recul quand joueur récupère
    
    [Header("Audio")]
    public AudioClip[] approachSounds;   // Sons quand il se rapproche
    public AudioClip catchSound;         // Son quand il attrape le joueur
    private AudioSource audioSource;

    private float approachSoundTimer = 0f;
    public float approachSoundInterval = 2f; // Toutes les 2 secondes

    [Header("References")]
    public PlayerController player;
    private Animator animator;
    private bool hasCaught = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (player == null) return;

        transform.position = new Vector3(
            player.transform.position.x,
            player.transform.position.y + normalOffsetY,
            0
        );
    }

    void Update()
    {
        if (player == null) return;

        if (player.IsDead())
        {
            CatchPlayer();
            FollowPlayerX();
            return;
        }
        
        UpdateApproachSound();
        ChaseOrRetreat();
        FollowPlayerX();
        CheckCatch();
        UpdateAnimation();
    }
    
    void CatchPlayer()
    {
        if (!hasCaught)
        {
            hasCaught = true;
            PlayCatchSound();
        }

        // Fonce sur le joueur
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.transform.position,
            approachSpeed * 3f * Time.deltaTime
        );
    }

    void ChaseOrRetreat()
    {
        float targetY;

        if (player.IsSlowed)
        {
            // Se rapproche du joueur
            targetY = player.transform.position.y + 1.5f;
            transform.position = new Vector3(
                transform.position.x,
                Mathf.MoveTowards(transform.position.y, targetY, approachSpeed * Time.deltaTime),
                0
            );
        }
        else
        {
            // Retourne à sa distance normale au dessus du joueur
            targetY = player.transform.position.y + normalOffsetY;
            transform.position = new Vector3(
                transform.position.x,
                Mathf.MoveTowards(transform.position.y, targetY, retreatSpeed * Time.deltaTime),
                0
            );
        }
    }

    void FollowPlayerX()
    {
        float newX = Mathf.Lerp(transform.position.x, player.transform.position.x, Time.deltaTime * 3f);
        transform.position = new Vector3(newX, transform.position.y, 0);
    }

    void CheckCatch()
    {
        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist <= catchDistance)
        {
            player.HitObstacle();
        }
    }

    void UpdateAnimation()
    {
        animator.SetBool("isSlowing", player.IsSlowed);
    }
    
    void UpdateApproachSound()
    {
        if (!player.IsSlowed) 
        {
            approachSoundTimer = 0f;
            return;
        }

        approachSoundTimer += Time.deltaTime;
        if (approachSoundTimer >= approachSoundInterval)
        {
            approachSoundTimer = 0f;
            if (approachSounds.Length > 0)
            {
                AudioClip clip = approachSounds[Random.Range(0, approachSounds.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
    }

    void PlayCatchSound()
    {
        if (catchSound != null)
            audioSource.PlayOneShot(catchSound);
    }
}