using System.Collections;
using UnityEngine;

public class SpaceshipAgent : MonoBehaviour
{
    private GridWorldEnvironment env;
    private DPAgent agent;

    private GridPosition currentState;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stepDelay = 0.5f;
    [SerializeField] private float spacing = 1.2f;

    [SerializeField] private float rotateSpeed = 8f;
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    public void Initialize(GridWorldEnvironment env, DPAgent agent, float spacing)
    {
        this.env = env; 
        this.agent = agent;
        this.spacing = spacing;

        // get spacing from GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) {
            spacing = gm.GetSpacing();
        }

        currentState = env.start;

        transform.position = GridToWorld(currentState);

        StartCoroutine(RunEpisode());
    }

    IEnumerator RunEpisode()
    {
        int steps = 0;

        while (!env.IsTerminal(currentState))
        {
            string action = agent.policy[currentState];

            var (nextState, reward) = env.Step(currentState, action);

            Debug.Log($"State: {currentState.x},{currentState.y},{currentState.z} | Action: {action} | Reward: {reward}");

            yield return MoveTo(GridToWorld(nextState));

            currentState = nextState;
            steps++;

            yield return new WaitForSeconds(stepDelay);

            if (steps > 100)
            {
                Debug.Log("Stopped: too many steps");
                break;
            }
        }

        Debug.Log("Episode finished");
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

    Vector3 GridToWorld(GridPosition pos)
    {
        return new Vector3(
            pos.x * spacing,
            pos.y * spacing,
            pos.z * spacing
        );
    }
}
