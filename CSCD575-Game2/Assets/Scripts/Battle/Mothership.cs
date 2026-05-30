using UnityEngine;
using System.Collections;

public class Mothership : MonoBehaviour
{
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private GameObject mothershipExplosionPrefab;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private GameObject finalMegaExplosionPrefab;
    public ShipStatus status;

    private float currentHealth;

    public ShipTeam team;
    public float HealthPercent => currentHealth / maxHealth;
    public bool IsDestroyed => status == ShipStatus.Destroyed;
    private void Awake()
    {
        currentHealth = maxHealth;
        status = ShipStatus.Active;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        status = ShipStatus.Active;
        print($"{team} mothership health reset to {maxHealth}");
    }

    public void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = visible;
        }
    }

    public void TakeDamage(float amount)
    {
        if (status == ShipStatus.Dying || status == ShipStatus.Destroyed)
        {
            return;
        }
        float newCurrentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        print($"Mothership took {amount} damage" + $" (current health: {currentHealth}/{maxHealth})");

        if (newCurrentHealth <= 0f)
        {
            SetVisible(false);
            status = ShipStatus.Dying;
            Debug.Log($"{team} mothership destroyed");
            ExplodeAndDisappear();
        }
        else
        {
            currentHealth = newCurrentHealth;
        }
    }

    private void ExplodeAndDisappear()
    {
        StartCoroutine(MothershipDeathSequence());
    }

    IEnumerator MothershipDeathSequence()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 offset =
                Random.insideUnitSphere * 8f;

            Instantiate(
                    mothershipExplosionPrefab,
                    transform.position + offset * 4f,
                    Quaternion.identity
                    );

            yield return new WaitForSeconds(0.1f);
        }

        Instantiate(
                finalMegaExplosionPrefab,
                transform.position,
                Quaternion.identity
                );

        yield return new WaitForSeconds(2f);
        status = ShipStatus.Destroyed;
        currentHealth = 0f;
    }
}
