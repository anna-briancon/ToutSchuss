using UnityEngine;

public class RowOfftrack : MonoBehaviour
{
    [Header("Tile Renderers")]
    public SpriteRenderer offtrackLeft;
    public SpriteRenderer offtrackRight;

    [Header("Sprites")]
    public Sprite[] offtrackSprites;

    void Start()
    {
        offtrackLeft.sprite = GetRandomSprite();
        offtrackRight.sprite = GetRandomSprite();
    }

    Sprite GetRandomSprite()
    {
        return offtrackSprites[Random.Range(0, offtrackSprites.Length)];
    }
}
