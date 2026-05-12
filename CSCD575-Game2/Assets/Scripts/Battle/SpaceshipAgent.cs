using UnityEngine;
using System.Collections;

public class SpaceshipAgent : MonoBehaviour
{
    public BattleEnvironment env;
    public IRLPolicy policy;
    public ShipDirective directive;
    public ShipRole role;
    public ShipStatus status;
    public GridPosition currentState;

    public GridPosition CurrentState => currentState;

    public void Initialize(BattleEnvironment env)
    {
        this.env = env;
    }
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stepDelay = 0.5f;
    [SerializeField] private float spacing = 1.2f;

    [SerializeField] private float rotateSpeed = 8f;
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    public void Initialize(BattleEnvironment env, IRLPolicy policy, GridPosition startState)
    {
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

    IEnumerator RunEpisode()
    {
        int steps = 0;

        while (status == ShipStatus.Active)
        {
            string action = policy.ChooseAction(this, env);

            GridPosition nextState = env.GetNextState(currentState, action);
            float reward = env.GetReward(this, currentState, action, nextState);

            policy.Learn(currentState, action, reward, nextState);

            Debug.Log($"{role} | State {currentState.x},{currentState.y},{currentState.z} | Action {action} | Reward {reward}");

            yield return MoveTo(env.GridToWorld(nextState));

            currentState = nextState;
            steps++;

            yield return new WaitForSeconds(stepDelay);

            if (steps > 100)
            {
                Debug.Log("Stopped: too many steps");
                break;
            }
        }

        Debug.Log($"{role} episode finished");
    }

    IEnumerator MoveTo(Vector3 target)
    {
        Vector3 start = transform.position;
        Vector3 direction = target - start;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction.normalized, Vector3.up)
                * Quaternion.Euler(rotationOffsetEuler);

            while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime
                );

                yield return null;
            }
        }

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            transform.position = Vector3.Lerp(start, target, t);

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
        return status == ShipStatus.Destroyed || status == ShipStatus.Docked;
    }
}
