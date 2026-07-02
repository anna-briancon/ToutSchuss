using UnityEngine;
using System.Collections.Generic;

public class WorldScroller : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject rowPrefab;
    public GameObject[] propPrefabs;

    [Header("Settings")]
    public float scrollSpeed = 3f;
    public int rowCount = 18;
    public float rowHeight = 1f;
    public float preloadMargin = 4f;

    [Header("Props")]
    public float propSpawnChance = 0.4f;

    [Header("References")]
    public Transform staticWorldRoot;
    public Transform staticPropsRoot;
    public Camera mainCamera;

    [Header("Difficulté")]
    public float difficultyInterval = 15f;
    public float speedIncrease = 0.3f;
    private float difficultyTimer = 0f;

    private List<GameObject> rows = new List<GameObject>();
    private List<GameObject> props = new List<GameObject>();
    private List<GameObject> obstacles = new List<GameObject>();

    private float topY;
    private float bottomY;
    private float visibleBottomY;
    private float visibleTopY;
    private float recycleTopY;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        UpdateVisibleBounds();
        topY = visibleTopY;
        bottomY = visibleBottomY - preloadMargin;

        if (staticWorldRoot != null)
        {
            foreach (Transform child in staticWorldRoot)
                rows.Add(child.gameObject);
        }

        if (staticPropsRoot != null)
        {
            foreach (Transform child in staticPropsRoot)
                props.Add(child.gameObject);
        }

        EnsureRowsCoverView();

        if (rows.Count < rowCount)
        {
            int missing = rowCount - rows.Count;
            float lowestY = GetLowestRowY();
            for (int i = 0; i < missing; i++)
                SpawnRow(lowestY - (i + 1) * rowHeight);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        ScrollRows();
        ScrollProps();
        RecycleRows();
        RecycleProps();
        ScrollObstacles();
        RecycleObstacles();
        UpdateDifficulty();
    }

    void ScrollRows()
    {
        foreach (GameObject row in rows)
        {
            row.transform.position += Vector3.up * scrollSpeed * Time.deltaTime;
        }
    }

    void ScrollProps()
    {
        foreach (GameObject prop in props)
        {
            if (prop != null)
                prop.transform.position += Vector3.up * scrollSpeed * Time.deltaTime;
        }
    }

    void RecycleRows()
    {
        UpdateVisibleBounds();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].transform.position.y > recycleTopY)
            {
                float newY = GetLowestRowY() - rowHeight;
                rows[i].transform.position = new Vector3(0, newY, 0);
                TrySpawnProps(newY);
            }
        }
    }

    void RecycleProps()
    {
        UpdateVisibleBounds();

        props.RemoveAll(p =>
        {
            if (p == null) return true;
            if (p.transform.position.y > recycleTopY)
            {
                Destroy(p);
                return true;
            }
            return false;
        });
    }

    void SpawnRow(float yPos)
    {
        GameObject row = Instantiate(rowPrefab, new Vector3(0, yPos, 0), Quaternion.identity);
        rows.Add(row);
    }

    void TrySpawnProps(float yPos)
    {
        if (propPrefabs.Length == 0) return;

        if (Random.value < propSpawnChance)
        {
            float xLeft = Random.Range(-14f, -3f);
            SpawnProp(xLeft, yPos);
        }

        if (Random.value < propSpawnChance)
        {
            float xRight = Random.Range(3f, 14f);
            SpawnProp(xRight, yPos);
        }
    }

    void SpawnProp(float x, float y)
    {
        int idx = Random.Range(0, propPrefabs.Length);
        GameObject prop = Instantiate(propPrefabs[idx], new Vector3(x, y, 0), Quaternion.identity);
        props.Add(prop);
    }

    public void AddObstacle(GameObject obs)
    {
        obstacles.Add(obs);
    }

    void ScrollObstacles()
    {
        foreach (GameObject obs in obstacles)
        {
            if (obs != null)
                obs.transform.position += Vector3.up * scrollSpeed * Time.deltaTime;
        }
    }

    void RecycleObstacles()
    {
        UpdateVisibleBounds();

        obstacles.RemoveAll(o =>
        {
            if (o == null) return true;
            if (o.transform.position.y > recycleTopY + 2f)
            {
                Destroy(o);
                return true;
            }
            return false;
        });
    }

    public float GetObstacleSpawnY()
    {
        UpdateVisibleBounds();
        return visibleBottomY - preloadMargin;
    }

    void UpdateVisibleBounds()
    {
        if (mainCamera == null)
        {
            visibleBottomY = -rowCount / 2f * rowHeight;
            visibleTopY = rowCount / 2f * rowHeight;
            recycleTopY = visibleTopY + rowHeight;
            return;
        }

        float camY = mainCamera.transform.position.y;
        float halfHeight = mainCamera.orthographicSize;
        visibleBottomY = camY - halfHeight;
        visibleTopY = camY + halfHeight;
        recycleTopY = visibleTopY + rowHeight;
    }

    void EnsureRowsCoverView()
    {
        if (rows.Count == 0) return;

        UpdateVisibleBounds();

        float targetBottom = visibleBottomY - preloadMargin;
        float targetTop = visibleTopY + rowHeight;

        float lowestY = GetLowestRowY();
        while (lowestY > targetBottom)
        {
            lowestY -= rowHeight;
            SpawnRow(lowestY);
        }

        float highestY = GetHighestRowY();
        while (highestY < targetTop)
        {
            highestY += rowHeight;
            SpawnRow(highestY);
        }
    }

    float GetLowestRowY()
    {
        float lowestY = float.MaxValue;

        foreach (GameObject row in rows)
        {
            if (row == null) continue;
            float y = row.transform.position.y;
            if (y < lowestY)
                lowestY = y;
        }

        return lowestY == float.MaxValue ? 0f : lowestY;
    }

    float GetHighestRowY()
    {
        float highestY = float.MinValue;

        foreach (GameObject row in rows)
        {
            if (row == null) continue;
            float y = row.transform.position.y;
            if (y > highestY)
                highestY = y;
        }

        return highestY == float.MinValue ? 0f : highestY;
    }
    
    void UpdateDifficulty()
    {
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyInterval)
        {
            difficultyTimer = 0f;
            scrollSpeed += speedIncrease;
            Debug.Log("Vitesse : " + scrollSpeed);
        }
    }
}
