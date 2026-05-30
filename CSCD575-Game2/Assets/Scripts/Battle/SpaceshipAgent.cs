using UnityEngine;
using System.Collections;

public class SpaceshipAgent : MonoBehaviour
{
    public BattleEnvironment env;

    public TDPolicy fighterPolicy;
    public ResupplyTDPolicy resupplyPolicy;

    public TDPolicy FighterPolicy => fighterPolicy; 
    public ResupplyTDPolicy ResupplyPolicy => resupplyPolicy;

    public ShipDirective directive;
    public ShipRole role;
    public ShipStatus status;

    public ShipTeam team;

    private int id;
    public int ID => id;

    [SerializeField] private string callsign;
    public string Callsign
    {
        get
        {
            if (team == ShipTeam.Enemy)
                return $"Drone{ID % 1000}";

            return callsign;
        }
        private set => callsign = value;
    }

    [SerializeField] private GameObject attackBeamPrefab;
    [SerializeField] private float beamLifetime = 0.1f;

    [SerializeField] private float lowFuelHelpThreshold = 0.3f;

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


    [SerializeField] private float accelerationTime = 0.25f;
    [SerializeField] private float maxMoveSpeed = 8f;
    private Vector3 moveVelocity;

    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float destroyDelay = 0.4f;

    private Coroutine episodeCoroutine;

    public static class Callsigns
    {
        private static readonly string[] names =
        {
            "Apollo",
            "Starbuck",
            "Boomer",
            "Hotdog",
            "Racetrack",
            "Kat",
            "Helo",
            "Bulldog",
            "Ghost",
            "Viper",
            "Mako",
            "Falcon",
            "Nova",
            "Phoenix",
            "Raven",
            "Specter",
            "Maverick",
            "Bandit",
            "Valkyrie",
            "Hammer",
            "Reaper",
            "Titan",
            "Blaze",
            "Shadow",
            "Comet",
            "Orion",
            "Wolf",
            "Kodiak",
            "Jaguar",
            "Thunder"
        };

        public static string GetRandom()
        {
            return names[Random.Range(0, names.Length)];
        }
    }

    public void PlayAttackEffect(Vector3 targetWorldPos)
    {
        StartCoroutine(AttackEffect(targetWorldPos));

        Vector3 start = transform.position;
        Vector3 end = targetWorldPos;

        Vector3 midpoint = (start + end) * 0.5f;
        Vector3 direction = end - start;

        GameObject beam = Instantiate(
            attackBeamPrefab,
            midpoint,
            Quaternion.LookRotation(direction.normalized)
        );

        beam.transform.localScale = new Vector3(
            0.2f,
            0.2f,
            direction.magnitude
        );

        Destroy(beam, beamLifetime);
    }


    private IEnumerator AttackEffect(Vector3 targetWorldPos)
    {
        attackLine.enabled = true;
        attackLine.SetPosition(0, transform.position);
        attackLine.SetPosition(1, targetWorldPos);
        //attackLine.startWidth = 0.35f;
        //attackLine.endWidth = 0.05f;

        yield return new WaitForSeconds(attackEffectTime);

        attackLine.enabled = false;
    }

    private void Initialize(BattleEnvironment env, GridPosition startState)
    {
        this.id = GetInstanceID();
        currentHealth = maxHealth;
        this.env = env; 

        // get spacing from GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) {
            spacing = gm.GetSpacing();
        }

        currentState = startState;
        transform.position = env.GridToWorld(currentState);
        //Callsign = Callsigns.GetRandom();
        if (string.IsNullOrEmpty(callsign) && team == ShipTeam.Player)
            Callsign = Callsigns.GetRandom();

        episodeCoroutine = StartCoroutine(RunEpisode());
    }

    public void InitializeFighter(
        BattleEnvironment env,
        GridPosition startState,
        TDPolicy policy)
    {
        this.fighterPolicy = policy;
        this.role = ShipRole.Fighter;
        Initialize(env, startState);
    }
    public void InitializeResupply(
            BattleEnvironment env,
            GridPosition startState,
            ResupplyTDPolicy policy)
    {
        this.resupplyPolicy = policy;
        this.role = ShipRole.Resupply;
        Initialize(env, startState);
    }

    public void TakeDamage(float amount)
    {
        if (status == ShipStatus.Destroyed || status == ShipStatus.Dying)
            return;
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log($"{role} took {amount} damage, health now {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            status = ShipStatus.Dying;
            ExplodeAndDisappear();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosion();
            }

            //gameObject.SetActive(false);
        }

    }

    private void ExplodeAndDisappear()
    {
        
        if (explosionPrefab != null)
        {
            Debug.Log("BOOOOM!");
            
            Instantiate(
                    explosionPrefab,
                    transform.position,
                    Quaternion.identity
                    );
        }

        StartCoroutine(DisappearAfterExplosion());
    }

    private IEnumerator DisappearAfterExplosion()
    {
        yield return new WaitForSeconds(destroyDelay);
        status = ShipStatus.Destroyed;

        gameObject.SetActive(false);

        // stop all coroutines to end episode
        StopAllCoroutines();
    }

    public void StopAllActions()
    {
        StopAllCoroutines();
    }

    IEnumerator RunEpisode()
    {
        if (role == ShipRole.Resupply)
            yield return RunResupplyEpisode();
        else
            yield return RunFighterEpisode();
    }

    IEnumerator RunFighterEpisode()
    {
        stepsUsed = 0; 

        while (status == ShipStatus.Active || status == ShipStatus.ReturningHome)
        {
            ShipState state = env.GetShipState(this);

            string action = fighterPolicy.ChooseAction(this, env);

            float reward = 0f;

            if (env.IsAttackAction(action))
            {
                reward = env.ResolveAttack(this, action);

                ShipState nextState = env.GetShipState(this);

                fighterPolicy.Learn(state, action, reward, nextState);

                yield return new WaitForSeconds(stepDelay);
                stepsUsed++;
                continue;
            }

            GridPosition nextGridState = env.GetNextState(currentState, action);
            reward = env.GetMovementReward(this, currentState, action, nextGridState);

            yield return MoveTo(env.GridToWorldWithNoise(nextGridState));

            currentState = nextGridState;

            ShipState nextShipState = env.GetShipState(this);

            fighterPolicy.Learn(state, action, reward, nextShipState);

            stepsUsed++;

            //if (status == ShipStatus.ReturningHome &&
                    //env.IsAtHomeMothership(this))
            //{
                //env.DockShip(this);
                //gameObject.SetActive(false); 
                //Debug.Log($"{role} docked at mothership");
                //break;
            //}

            if (stepsUsed > maxSteps)
            {
                status = ShipStatus.Disabled;;
                Debug.Log($"{role} disabled: exceeded max steps");
                break;
            }

            if (ShouldEndEpisode())
            {
                break;
            }
            yield return new WaitForSeconds(stepDelay);

        }

        Debug.Log($"{role} episode finished");
    }

    IEnumerator RunResupplyEpisode()
    {
        stepsUsed = 0;


        while (status == ShipStatus.Active || status == ShipStatus.ReturningHome)
        {
            ResupplyState state = env.GetResupplyState(this);

            string action = resupplyPolicy.ChooseAction(this, env);

            float reward = 0f;

            if (env.IsResupplyAction(action))
            {
                reward = env.ResolveResupply(this, action);

                ResupplyState nextState = env.GetResupplyState(this);

                resupplyPolicy.Learn(state, action, reward, nextState);

                yield return new WaitForSeconds(stepDelay);
                stepsUsed++;
                continue;
            }

            GridPosition nextGridState = env.GetNextState(currentState, action);
            reward = env.GetMovementReward(this, currentState, action, nextGridState);

            yield return MoveTo(env.GridToWorldWithNoise(nextGridState));

            currentState = nextGridState;

            ResupplyState nextResupplyState = env.GetResupplyState(this);

            resupplyPolicy.Learn(state, action, reward, nextResupplyState);

            stepsUsed++;

            if (stepsUsed > maxSteps)
            {
                status = ShipStatus.Disabled;;
                Debug.Log($"{role} disabled: exceeded max steps");
                break;
            }


            if (ShouldEndEpisode())
            {
                break;
            }
            yield return new WaitForSeconds(stepDelay);
        }

        Debug.Log($"{role} episode finished");
    }


    private bool ShouldEndEpisode()
    {
        if (status == ShipStatus.ReturningHome &&
                env.IsAtHomeMothership(this))
        {
            env.DockShip(this);
            gameObject.SetActive(false);
            Debug.Log($"{role} docked at mothership");
            return true;
        }

        //if (stepsUsed > maxSteps)
        //{
            //status = ShipStatus.Destroyed;
            //Debug.Log($"{role} destroyed: exceeded max steps");
            //return true;
        //}

        return false;
    }

    //IEnumerator MoveTo(Vector3 target)
    //{
        //Vector3 start = transform.position;

        //Vector3 mid = (start + target) * 0.5f;

        //Vector3 sideways = Vector3.Cross(
                //(target - start).normalized,
                //Vector3.up
                //);

        //mid += sideways * Random.Range(-2.5f, 2.5f);
        //mid += Vector3.up * Random.Range(0.1f, 0.4f);

        //float t = 0f;

        //float duration = moveDuration; 

        //while (t < 1.0f)
        //{
            //t += Time.deltaTime / duration;

            //float smoothT =
                //Mathf.SmoothStep(
                        //0f,
                        //1f,
                        //Mathf.SmoothStep(0f, 1f, t)
                        //);

            //Vector3 a = Vector3.Lerp(start, mid, smoothT);
            //Vector3 b = Vector3.Lerp(mid, target, smoothT);

            //Vector3 pos = Vector3.Lerp(a, b, smoothT);

            //Vector3 direction = pos - transform.position;

            //if (direction.sqrMagnitude > 0.001f)
            //{
                //Quaternion targetRotation =
                    //Quaternion.LookRotation(direction.normalized, Vector3.up)
                    //* Quaternion.Euler(rotationOffsetEuler);

                //transform.rotation = Quaternion.Slerp(
                        //transform.rotation,
                        //targetRotation,
                        //rotateSpeed * Time.deltaTime
                        //);
            //}

            //transform.position = pos;

            //yield return null;
        //}

        //transform.position = target;
    //}


    IEnumerator MoveTo(Vector3 target)
    {
        float stopDistance = 0.05f;

        while (Vector3.Distance(transform.position, target) > stopDistance)
        {
            Vector3 previousPosition = transform.position;

            transform.position = Vector3.SmoothDamp(
                    transform.position,
                    target,
                    ref moveVelocity,
                    accelerationTime,
                    maxMoveSpeed
                    );

            Vector3 direction = transform.position - previousPosition;

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

            yield return null;
        }

        transform.position = target;
    }


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
        return status == ShipStatus.Destroyed || status == ShipStatus.Docked || status == ShipStatus.Disabled;
    }

    public bool NeedsHelp()
    {
        return (status == ShipStatus.Active || status == ShipStatus.Disabled || status == ShipStatus.ReturningHome)
            && (
                FuelPercent < lowFuelHelpThreshold ||
                HealthPercent < 0.4f 
            );
    }

    public void Refuel(float amount)
    {
        ShipStatus prevStatus = status;

        stepsUsed = Mathf.Max(
            0,
            stepsUsed - Mathf.RoundToInt(amount * maxSteps)
        );

        if (prevStatus == ShipStatus.Disabled && stepsUsed < maxSteps)
        {
            status = ShipStatus.Active;
            Debug.Log($"{role} refueled and reactivated");
            episodeCoroutine = StartCoroutine(RunEpisode());
        }
    }    
    public void Repair(float amount)
    {
        currentHealth += amount * maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

    }

    public void DockForNextLevel(
            BattleEnvironment battleEnv,
            GridPosition dockPosition)
    {
        if (attackLine != null)
            attackLine.enabled = false;
        StopAllCoroutines();

        env = battleEnv;
        moveVelocity = Vector3.zero;

        currentState = dockPosition;
        transform.position = battleEnv.GridToWorld(dockPosition);

        status = ShipStatus.Docked;
        gameObject.SetActive(false);
    }
}
