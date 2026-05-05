using UnityEngine;

public class SelectableShip : MonoBehaviour
{
    [SerializeField] private GameObject selectionRing;

    public bool IsSelected { get; private set; }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (selectionRing != null)
        {
            selectionRing.SetActive(selected);
        }
    }
}
