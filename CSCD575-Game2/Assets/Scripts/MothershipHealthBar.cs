using UnityEngine;
using UnityEngine.UI;

public class MothershipHealthBar : MonoBehaviour
{
    [SerializeField] private Mothership ship;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (ship == null)
            ship = GetComponentInParent<Mothership>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (ship == null)
            return;

        if (healthFillImage != null)
            healthFillImage.fillAmount = ship.HealthPercent;

        if (targetCamera != null)
            transform.forward = targetCamera.transform.forward;
    }
}

