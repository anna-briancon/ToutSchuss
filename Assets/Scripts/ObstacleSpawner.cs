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

    [Header("References")]
    public WorldScroller worldScroller;

    private float timer = 0f;

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
        int typeRoll = Random.Range(0, 3);
        switch (typeRoll)
        {
            case 0: SpawnSideLine(); break;
            case 1: SpawnJumpLine(); break;
            case 2: SpawnDuckObstacle(); break;
        }
    }

    void SpawnSideLine()
    {
        // Tableau qui stocke ce qu'on met dans chaque couloir
        // null = libre, sinon = prefab à spawner
        GameObject[] line = new GameObject[3]; // [0]=gauche [1]=centre [2]=droite

        // Décide si on place GateBlue et/ou GateRed
        bool placeGateBlue = Random.value < 0.5f;
        bool placeGateRed = Random.value < 0.5f;

        // Place GateBlue → préfère gauche, sinon centre
        if (placeGateBlue)
        {
            if (line[0] == null)
                line[0] = gateBlue;
            else if (line[1] == null)
                line[1] = gateBlue;
        }

        // Place GateRed → préfère droite, sinon centre
        if (placeGateRed)
        {
            if (line[2] == null)
                line[2] = gateRed;
            else if (line[1] == null)
                line[1] = gateRed;
        }

        // Compte les couloirs libres
        int freeLanes = 0;
        for (int i = 0; i < 3; i++)
            if (line[i] == null) freeLanes++;

        // S'assure qu'au moins un couloir reste libre (sans obstacle Side)
        // Place des neutres sur certains couloirs libres
        for (int i = 0; i < 3; i++)
        {
            if (line[i] != null) continue;

            // Garde au moins un couloir vraiment libre
            bool isLastFree = (freeLanes == 1);
            if (!isLastFree && Random.value < 0.5f)
            {
                if (neutralObstacles.Length > 0)
                {
                    line[i] = neutralObstacles[Random.Range(0, neutralObstacles.Length)];
                    freeLanes--;
                }
            }
        }

        // Instancie tout
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * laneWidth;

            if (line[i] != null)
            {
                // Obstacle Side
                GameObject obs = Instantiate(line[i], new Vector3(x, spawnY, 0), Quaternion.identity);
                worldScroller.AddObstacle(obs);
            }
            else
            {
                // Couloir libre → rien ou Jump
                if (Random.value < 0.4f && jumpObstacles.Length > 0)
                {
                    int idx = Random.Range(0, jumpObstacles.Length);
                    GameObject obs = Instantiate(jumpObstacles[idx], new Vector3(x, spawnY, 0), Quaternion.identity);
                    worldScroller.AddObstacle(obs);
                }
            }
        }
    }

    void SpawnJumpLine()
    {
        if (jumpObstacles.Length == 0) return;

        int count = Random.Range(1, 4);
        int[] lanes = { 0, 1, 2 };
        ShuffleLanes(lanes);

        for (int i = 0; i < count; i++)
        {
            float x = (lanes[i] - 1) * laneWidth;
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

    void ShuffleLanes(int[] lanes)
    {
        for (int i = lanes.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = lanes[i];
            lanes[i] = lanes[j];
            lanes[j] = tmp;
        }
    }
}