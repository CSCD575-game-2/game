using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int sizeX = 3;
    [SerializeField] private int sizeY = 3;
    [SerializeField] private int sizeZ = 3;
    [SerializeField] private float tileSpacing = 1.2f;

    [Header("Positions")]
    [SerializeField] private GridPosition start = new GridPosition(0, 0, 0);
    [SerializeField] private GridPosition goal = new GridPosition(2, 2, 2);
    [SerializeField] private List<GridPosition> traps = new()
    {
        new GridPosition(0, 1, 1),
        new GridPosition(1, 1, 1)
    };
    [SerializeField] private List<GridPosition> hazards = new();

    [Header("Prefabs")]
    [SerializeField] private GridTileView tilePrefab;
    [SerializeField] private SpaceshipAgent spaceshipPrefab;

    private GridWorldEnvironment env;
    private DPAgent agent;

    private void Start()
    {
        env = new GridWorldEnvironment(
                    sizeX,
                    sizeY,
                    sizeZ,
                    start,
                    goal,
                    traps,
                    hazards
                );

        agent = new DPAgent(env);
        agent.Train();
        agent.ExtractPolicy();

        GenerateGrid();

        SpawnAgent();
    }

    public float GetSpacing() {
        return tileSpacing;
    }

    void SpawnAgent()
    {
        var ship = Instantiate(spaceshipPrefab);
        ship.Initialize(env, agent, tileSpacing);
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                     CreateTile(x, y, z);
                }
            }
        }
    }

    public int getSizeX() => sizeX;
    public int getSizeY() => sizeY;
    public int getSizeZ() => sizeZ;

    private void CreateTile(int x, int y, int z)
    {
        
        GridPosition state = new GridPosition(x, y, z);

        if (!env.IsTrap(state) && !env.IsHazard(state) && state != start && state != goal)
        {
            //return;
        }

        Vector3 worldPos = new Vector3(
            x * tileSpacing,
            y * tileSpacing,
            z * tileSpacing
        );

        GridTileView tile = Instantiate(tilePrefab, worldPos, Quaternion.identity);

        // scale the tile based on the spacing
        tile.transform.localScale = Vector3.one * tileSpacing * 0.9f;
        Debug.Log($"Creating tile at:  {worldPos}");

        string policyLabel = agent.policy[state];
        float value = agent.values[state];

        string label = $"{policyLabel}\n{value:F1}";

        Color color = Color.black;        
        

        if (state == start) color = Color.cyan;
        if (state == goal) color = Color.green;
        if (env.IsTrap(state)) color = Color.red;
        if (env.IsHazard(state)) color = Color.yellow;

        tile.Setup(color, label);
    }
}
