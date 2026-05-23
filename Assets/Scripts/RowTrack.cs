using UnityEngine;

public class RowTrack : MonoBehaviour
{
    [Header("Tile Renderers")]
    public SpriteRenderer tileLeft;
    public SpriteRenderer tileCenter;
    public SpriteRenderer tileRight;

    [Header("Sprites")]
    public Sprite[] trackSprites;

    void Start()
    {
        tileLeft.sprite = GetRandomSprite();
        tileCenter.sprite = GetRandomSprite();
        tileRight.sprite = GetRandomSprite();
    }

    Sprite GetRandomSprite()
    {
        return trackSprites[Random.Range(0, trackSprites.Length)];
    }
}
