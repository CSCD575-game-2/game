using UnityEngine;

public class Mothership : MonoBehaviour
{
    [SerializeField] private float maxHealth = 500f;
    public ShipStatus status;

    private float currentHealth;

    public ShipTeam team;
    public float HealthPercent => currentHealth / maxHealth;
    public bool IsDestroyed => currentHealth <= 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
        status = ShipStatus.Active;
    }


    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            status = ShipStatus.Destroyed;
            Debug.Log($"{team} mothership destroyed");
        }
    }
}
