using UnityEngine;
using UnityEngine.UI;

public class ShipHealthBar : MonoBehaviour
{
    [SerializeField] private SpaceshipAgent ship;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (ship == null)
            ship = GetComponentInParent<SpaceshipAgent>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (ship == null)
            return;

        if (healthFillImage != null)
            healthFillImage.fillAmount = ship.HealthPercent;

        if (fuelFillImage != null)
            fuelFillImage.fillAmount = ship.FuelPercent;

        if (targetCamera != null)
            transform.forward = targetCamera.transform.forward;
    }
}
