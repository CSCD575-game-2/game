using TMPro;
using UnityEngine;

public class GridTileView : MonoBehaviour
{
    [SerializeField] private Renderer tileRenderer;
    [SerializeField] private TMP_Text labelText;

    private void Awake()
    {
        if (tileRenderer == null)
        {
            tileRenderer = GetComponentInChildren<Renderer>();
        }

        if (labelText == null)
        {
            labelText = GetComponentInChildren<TMP_Text>();
        }
    }

    public void Setup(Color color, string label)
    {
        if (tileRenderer != null)
        {
            color.a = 0.20f; // alpha value
            tileRenderer.material.color = color;
        }

        if (labelText != null)
        {
            labelText.text = label;
        }
    }
}
