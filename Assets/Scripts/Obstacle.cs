using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public enum ObstacleType
    {
        Side,   // Eviter sur le côté
        Jump,   // Sauter
        Duck    // Se baisser
    }

    public ObstacleType type;

    private bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        triggered = true;

        bool survived = false;

        switch (type)
        {
            case ObstacleType.Jump:
                survived = player.IsJumping();
                break;
            case ObstacleType.Duck:
                survived = player.IsDucking();
                break;
            case ObstacleType.Side:
                survived = false; // On ne peut qu'éviter sur le côté
                break;
        }

        if (!survived)
        {
            player.HitObstacle();
        }
    }
}