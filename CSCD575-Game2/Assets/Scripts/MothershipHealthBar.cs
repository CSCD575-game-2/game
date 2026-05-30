using UnityEngine;
using UnityEngine.UI;

public class MothershipHealthBar : MonoBehaviour
{
    [SerializeField] private Mothership ship;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image backgroundImage;
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

        if (healthFillImage != null) {

            //if (ship.HealthPercent < 1f) {
                //Debug.Log("Updating health bar for " + ship.name + " with health percent: " + ship.HealthPercent);
            //}
            healthFillImage.fillAmount = ship.HealthPercent;
        }

        if (targetCamera != null && healthFillImage != null && backgroundImage != null) {
            healthFillImage.transform.forward = targetCamera.transform.forward;
            backgroundImage.transform.forward = targetCamera.transform.forward;
        }
    }
}

