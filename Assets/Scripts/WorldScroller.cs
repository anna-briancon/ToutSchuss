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

    [Header("Props")]
    public float propSpawnChance = 0.4f;

    [Header("References")]
    public Transform staticWorldRoot;
    public Transform staticPropsRoot;

    [Header("Difficulté")]
    public float difficultyInterval = 15f;
    public float speedIncrease = 0.3f;
    private float difficultyTimer = 0f;

    private List<GameObject> rows = new List<GameObject>();
    private List<GameObject> props = new List<GameObject>();
    private List<GameObject> obstacles = new List<GameObject>();

    private float topY;
    private float bottomY;

    void Start()
    {
        topY = rowCount / 2f * rowHeight;
        bottomY = -topY;

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

        // Si pas assez de rangées, complète avec des nouvelles
        if (rows.Count < rowCount)
        {
            int missing = rowCount - rows.Count;
            for (int i = 0; i < missing; i++)
            {
                float yPos = bottomY - i * rowHeight;
                SpawnRow(yPos);
            }
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
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].transform.position.y > topY + rowHeight)
            {
                float newY = rows[i].transform.position.y - rowCount * rowHeight;
                rows[i].transform.position = new Vector3(0, newY, 0);
                TrySpawnProps(newY);
            }
        }
    }

    void RecycleProps()
    {
        props.RemoveAll(p =>
        {
            if (p == null) return true;
            if (p.transform.position.y > topY + rowHeight)
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
        obstacles.RemoveAll(o =>
        {
            if (o == null) return true;
            if (o.transform.position.y > topY + 5f)
            {
                Destroy(o);
                return true;
            }
            return false;
        });
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
