using System.Collections.Generic;
using UnityEngine;

public class ShipSelectionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask selectableMask;

    private readonly List<SelectableShip> selectedShips = new();

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectUnderMouse();
        }
    }

    private void SelectUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, selectableMask))
        {
            SelectableShip ship = hit.collider.GetComponentInParent<SelectableShip>();

            if (ship != null)
            {
                bool additive = Input.GetKey(KeyCode.LeftShift);

                if (!additive)
                {
                    ClearSelection();
                }

                Select(ship);
            }
        }
        else
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                ClearSelection();
            }
        }
    }

    private void Select(SelectableShip ship)
    {
        if (selectedShips.Contains(ship)) return;

        selectedShips.Add(ship);
        ship.SetSelected(true);
    }

    private void ClearSelection()
    {
        foreach (SelectableShip ship in selectedShips)
        {
            ship.SetSelected(false);
        }

        selectedShips.Clear();
    }
}
