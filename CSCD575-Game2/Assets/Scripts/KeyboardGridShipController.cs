using UnityEngine;

public class KeyboardGridShipController : MonoBehaviour
{
    [SerializeField] private float spacing = 100f;
    [SerializeField] private float moveSpeed = 6f;

    [Header("Grid Bounds")]
    private int sizeX;
    private int sizeY;
    private int sizeZ;

    private GridPosition gridPos;
    private Vector3 targetWorldPos;

    private void Start()
    {
        // get grid bounds from GameManager
        GameManager gm  = FindObjectOfType<GameManager>();
        sizeX = gm.GetSizeX();
        sizeY = gm.GetSizeY();
        sizeZ = gm.GetSizeZ();


        gridPos = new GridPosition(0, 0, 0);
        targetWorldPos = GridToWorld(gridPos);
        transform.position = targetWorldPos;
    }

    private void Update()
    {
        //HandleInput();

        //Vector3 direction = targetWorldPos - transform.position;

        //// rotate toward movement direction
        //if (direction.sqrMagnitude > 0.001f)
        //{
            //Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            //transform.rotation = Quaternion.Slerp(
                //transform.rotation,
                //targetRotation,
                //10f * Time.deltaTime
            //);

            //float roll = Input.GetKey(KeyCode.H) ? 20f :
            //Input.GetKey(KeyCode.L) ? -20f : 0f;

            //Quaternion rollRot = Quaternion.Euler(0, 0, roll);

            //transform.rotation = Quaternion.Slerp(
                //transform.rotation,
                //targetRotation * rollRot,
                //5f * Time.deltaTime
            //);

        //}

        //// move
        //transform.position = Vector3.MoveTowards(
            //transform.position,
            //targetWorldPos,
            //moveSpeed * Time.deltaTime
        //);
    }

    private void HandleInput()
    {
        if (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            return;

        GridPosition next = gridPos;

        if (Input.GetKeyDown(KeyCode.H)) next.x -= 1;
        if (Input.GetKeyDown(KeyCode.L)) next.x += 1;

        if (Input.GetKeyDown(KeyCode.J)) next.z -= 1;
        if (Input.GetKeyDown(KeyCode.K)) next.z += 1;

        if (Input.GetKeyDown(KeyCode.U)) next.y -= 1;
        if (Input.GetKeyDown(KeyCode.I)) next.y += 1;

        next.x = Mathf.Clamp(next.x, 0, sizeX - 1);
        next.y = Mathf.Clamp(next.y, 0, sizeY - 1);
        next.z = Mathf.Clamp(next.z, 0, sizeZ - 1);

        if (next != gridPos)
        {
            // scale to grid spacing

            gridPos = next;
            targetWorldPos = GridToWorld(gridPos);
        }
    }

    private Vector3 GridToWorld(GridPosition pos)
    {
        return new Vector3(
            pos.x * spacing,
            pos.y * spacing,
            pos.z * spacing
        );
    }
}
