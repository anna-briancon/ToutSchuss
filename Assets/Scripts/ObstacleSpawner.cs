using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacles Side")]
    public GameObject[] neutralObstacles; // Snowman, Tree → n'importe quel couloir
    public GameObject gateBlue;           // Toujours gauche ou centre
    public GameObject gateRed;            // Toujours droite ou centre

    [Header("Obstacles Jump")]
    public GameObject[] jumpObstacles;

    [Header("Obstacles Duck")]
    public GameObject[] duckObstacles;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnY = -10f;
    public float laneWidth = 1f;
    [Range(0f, 1f)] public float sideSpawnChance = 0.65f;
    [Range(0f, 1f)] public float safeLaneJumpChance = 0.2f;
    [Range(0f, 1f)] public float neutralSpawnChance = 0.6f;

    [Header("References")]
    public WorldScroller worldScroller;

    private float timer = 0f;
    private int lastSafeLane = 1;

    // Positions des couloirs
    // 0 = gauche (x:-1), 1 = centre (x:0), 2 = droite (x:+1)

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnObstacle();
        }
    }

    void SpawnObstacle()
    {
        float roll = Random.value;
        if (roll < sideSpawnChance)
            SpawnSideLine();
        else if (roll < sideSpawnChance + (1f - sideSpawnChance) * 0.5f)
            SpawnJumpLine();
        else
            SpawnDuckObstacle();
    }

    void SpawnSideLine()
    {
        GameObject[] line = new GameObject[3];
        int safeLane = PickNextSafeLane();
        lastSafeLane = safeLane;

        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == safeLane) continue;
            line[lane] = PickBlockerForLane(lane, safeLane);
        }

        EnsureAtLeastOneNeutral(line, safeLane);

        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * laneWidth;

            if (line[i] != null)
            {
                GameObject obs = Instantiate(line[i], new Vector3(x, spawnY, 0), Quaternion.identity);
                worldScroller.AddObstacle(obs);
            }
            else if (Random.value < safeLaneJumpChance && jumpObstacles.Length > 0)
            {
                int idx = Random.Range(0, jumpObstacles.Length);
                GameObject obs = Instantiate(jumpObstacles[idx], new Vector3(x, spawnY, 0), Quaternion.identity);
                worldScroller.AddObstacle(obs);
            }
        }
    }

    int PickNextSafeLane()
    {
        int[] candidates = new int[2];
        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            if (i != lastSafeLane)
                candidates[count++] = i;
        }

        return candidates[Random.Range(0, count)];
    }

    GameObject PickBlockerForLane(int lane, int safeLane)
    {
        if (neutralObstacles.Length > 0 && Random.value < neutralSpawnChance)
            return neutralObstacles[Random.Range(0, neutralObstacles.Length)];

        if (lane == 0 && gateBlue != null)
            return gateBlue;

        if (lane == 2 && gateRed != null)
            return gateRed;

        if (lane == 1)
        {
            if (safeLane == 0 && gateRed != null)
                return gateRed;
            if (safeLane == 2 && gateBlue != null)
                return gateBlue;
        }

        if (neutralObstacles.Length > 0)
            return neutralObstacles[Random.Range(0, neutralObstacles.Length)];

        return gateBlue != null ? gateBlue : gateRed;
    }

    void EnsureAtLeastOneNeutral(GameObject[] line, int safeLane)
    {
        if (neutralObstacles.Length == 0) return;

        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == safeLane) continue;
            if (!IsGate(line[lane]))
                return;
        }

        int[] blockedLanes = new int[2];
        int count = 0;
        for (int lane = 0; lane < 3; lane++)
        {
            if (lane != safeLane)
                blockedLanes[count++] = lane;
        }

        int replaceLane = blockedLanes[Random.Range(0, count)];
        line[replaceLane] = neutralObstacles[Random.Range(0, neutralObstacles.Length)];
    }

    bool IsGate(GameObject prefab)
    {
        return prefab == gateBlue || prefab == gateRed;
    }

    void SpawnJumpLine()
    {
        if (jumpObstacles.Length == 0) return;

        int safeLane = PickNextSafeLane();
        lastSafeLane = safeLane;

        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == safeLane) continue;

            float x = (lane - 1) * laneWidth;
            int idx = Random.Range(0, jumpObstacles.Length);
            GameObject obs = Instantiate(jumpObstacles[idx], new Vector3(x, spawnY, 0), Quaternion.identity);
            worldScroller.AddObstacle(obs);
        }
    }

    void SpawnDuckObstacle()
    {
        if (duckObstacles.Length == 0) return;
        int idx = Random.Range(0, duckObstacles.Length);
        GameObject obs = Instantiate(duckObstacles[idx], new Vector3(0, spawnY, 0), Quaternion.identity);
        worldScroller.AddObstacle(obs);
    }
}