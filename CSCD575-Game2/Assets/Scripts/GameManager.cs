using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int sizeX = 10;
    [SerializeField] private int sizeY = 1;
    [SerializeField] private int sizeZ = 10;
    [SerializeField] private float tileSpacing = 1.2f;

    [Header("Prefabs")]
    [SerializeField] private GridTileView tilePrefab;

    [Header("Battle")]
    [SerializeField] private BattleEnvironment battleEnvironment;

    private void Start()
    {
        battleEnvironment.Initialize(sizeX, sizeY, sizeZ, tileSpacing);

        GenerateGrid();
    }

    public float GetSpacing()
    {
        return tileSpacing;
    }

    public int GetSizeX() => sizeX;
    public int GetSizeY() => sizeY;
    public int GetSizeZ() => sizeZ;

    public Vector3 GridToWorld(GridPosition pos)
    {
        return new Vector3(
            pos.x * tileSpacing,
            pos.y * tileSpacing,
            pos.z * tileSpacing
        );
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

    private void CreateTile(int x, int y, int z)
    {
        GridPosition state = new GridPosition(x, y, z);

        Vector3 worldPos = GridToWorld(state);

        GridTileView tile = Instantiate(
            tilePrefab,
            worldPos,
            Quaternion.identity
        );

        tile.transform.localScale = Vector3.one * tileSpacing * 0.9f;

        Color color = Color.black;
        string label = $"{x},{y},{z}";

        tile.Setup(color, label);
    }
}
