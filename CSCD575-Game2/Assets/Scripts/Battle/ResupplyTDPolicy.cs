
using System.Collections.Generic;
using UnityEngine;

public class ResupplyTDPolicy : IRLPolicy<ResupplyState>
{
    private readonly Dictionary<(ResupplyState, string), float> qValues = new();

    private string[] actions =
    {
        "Up",
        "Down",
        "Left",
        "Right",
        "Forward",
        "Back",
        "HoldPosition",
        "Resupply"
    };

    public float Epsilon { get; set; }
    public float Alpha { get; set; }
    public float Gamma { get; set; }

    public ResupplyTDPolicy(float alpha = 0.2f, float gamma = 0.9f, float epsilon = 0.2f)
    {
        Alpha = alpha;
        Gamma = gamma;
        Epsilon = epsilon;
    }

    public string ChooseAction(SpaceshipAgent ship, BattleEnvironment env)
    {
        if (env.sizeY == 1)
        {
            // 2D environment, remove "Up" and "Down" actions
            this.actions = new string[]
            {
                "Left",
                "Right",
                "Forward",
                "Back",
                "HoldPosition",
                "Resupply",
            };
        }

        ResupplyState state = env.GetResupplyState(ship);

        // explore
        if (Random.value < Epsilon)
        {
            return actions[Random.Range(0, actions.Length)];
        }

        // exploit
        string bestAction = actions[Random.Range(0, actions.Length)];
        float bestValue = float.NegativeInfinity;

        foreach (string action in actions)
        {
            float q = GetQ(state, action);

            if (q > bestValue)
            {
                bestValue = q;
                bestAction = action;
            }
        }

        return bestAction;
    }

    public void Learn(
            ResupplyState state,
            string action,
            float reward,
            ResupplyState nextState)
    {
        float oldQ = GetQ(state, action);

        float bestNextQ = float.NegativeInfinity;

        foreach (string nextAction in actions)
        {
            bestNextQ = Mathf.Max(bestNextQ, GetQ(nextState, nextAction));
        }

        float newQ = oldQ + Alpha * (reward + Gamma * bestNextQ - oldQ);

        qValues[(state, action)] = newQ;
    }

    private float GetQ(ResupplyState state, string action)
    {
        var key = (state, action);

        if (!qValues.ContainsKey(key))
            qValues[key] = 0f;

        return qValues[key];
    }
}
