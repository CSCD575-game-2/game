using UnityEngine;
using System.Collections;

public class SpaceshipAgent : MonoBehaviour
{
    public BattleEnvironment env;
    private IRLPolicy policy;
    public IRLPolicy Policy => policy; 
    public ShipDirective directive;
    public ShipRole role;
    public ShipStatus status;

    public ShipTeam team;


    [SerializeField] private int maxSteps = 100;
    private int stepsUsed = 0;

    public float FuelPercent => 1f - ((float)stepsUsed / maxSteps);

    public GridPosition currentState;

    public GridPosition CurrentState => currentState;

    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public float HealthPercent => currentHealth / maxHealth;

    [SerializeField] private float moveDuration = 0.25f;
    //[SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stepDelay = 0.3f;
    [SerializeField] private float spacing = 1.2f;

    [SerializeField] private float rotateSpeed = 8f;
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    [SerializeField] private LineRenderer attackLine;
    [SerializeField] private float attackEffectTime = 0.08f;

    public void PlayAttackEffect(Vector3 targetWorldPos)
    {
        StartCoroutine(AttackEffect(targetWorldPos));
    }

    private IEnumerator AttackEffect(Vector3 targetWorldPos)
    {
        attackLine.enabled = true;
        attackLine.SetPosition(0, transform.position);
        attackLine.SetPosition(1, targetWorldPos);

        yield return new WaitForSeconds(attackEffectTime);

        attackLine.enabled = false;
    }

    public void Initialize(BattleEnvironment env, IRLPolicy policy, GridPosition startState)
    {
        currentHealth = maxHealth;
        this.env = env; 
        this.policy = policy;

        // get spacing from GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) {
            spacing = gm.GetSpacing();
        }

        currentState = startState;
        transform.position = env.GridToWorld(currentState);

        StartCoroutine(RunEpisode());
        //RunEpisode();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

    if (currentHealth <= 0f)
    {
        status = ShipStatus.Destroyed;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion();
        }

        gameObject.SetActive(false);
    }

    }


    IEnumerator RunEpisode()
    {
        stepsUsed = 0; 

        while (status == ShipStatus.Active || status == ShipStatus.ReturningHome)
        {
            ShipState state = env.GetShipState(this);

            string action = policy.ChooseAction(this, env);

            float reward = 0f;

            if (env.IsAttackAction(action))
            {
                reward = env.ResolveAttack(this, action);

                ShipState nextState = env.GetShipState(this);

                policy.Learn(state, action, reward, nextState);

                yield return new WaitForSeconds(stepDelay);
                stepsUsed++;
                continue;
            }

            GridPosition nextGridState = env.GetNextState(currentState, action);
            reward = env.GetReward(this, currentState, action, nextGridState);

            yield return MoveTo(env.GridToWorldWithNoise(nextGridState));

            currentState = nextGridState;

            ShipState nextShipState = env.GetShipState(this);

            policy.Learn(state, action, reward, nextShipState);

            stepsUsed++;

            if (status == ShipStatus.ReturningHome &&
                    env.IsAtHomeMothership(this))
            {
                env.DockShip(this);
                gameObject.SetActive(false); 
                Debug.Log($"{role} docked at mothership");
                break;
            }

            if (stepsUsed > maxSteps)
            {
                status = ShipStatus.Destroyed;
                Debug.Log($"{role} destroyed: exceeded max steps");
                break;
            }

            yield return new WaitForSeconds(stepDelay);

        }

        Debug.Log($"{role} episode finished");
    }

    IEnumerator MoveTo(Vector3 target)
    {
        Vector3 start = transform.position;

        Vector3 mid = (start + target) * 0.5f;

        Vector3 sideways = Vector3.Cross(
                (target - start).normalized,
                Vector3.up
                );

        mid += sideways * Random.Range(-2.5f, 2.5f);
        mid += Vector3.up * Random.Range(0.1f, 0.4f);

        float t = 0f;

        float duration = moveDuration; 

        while (t < 1.0f)
        {
            t += Time.deltaTime / duration;

            float smoothT =
                Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.SmoothStep(0f, 1f, t)
                        );

            Vector3 a = Vector3.Lerp(start, mid, smoothT);
            Vector3 b = Vector3.Lerp(mid, target, smoothT);

            Vector3 pos = Vector3.Lerp(a, b, smoothT);

            Vector3 direction = pos - transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction.normalized, Vector3.up)
                    * Quaternion.Euler(rotationOffsetEuler);

                transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotateSpeed * Time.deltaTime
                        );
            }

            transform.position = pos;

            yield return null;
        }

        transform.position = target;}

    //Vector3 GridToWorld(GridPosition pos)
    //{
    //return new Vector3(
    //pos.x * spacing,
    //pos.y * spacing,
    //pos.z * spacing
    //);
    //}

    public void SetDirective(ShipDirective newDirective)
    {
        directive = newDirective;
    }

    public bool IsTerminal()
    {
        return status == ShipStatus.Destroyed || status == ShipStatus.Docked;
    }
}
