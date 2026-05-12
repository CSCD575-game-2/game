using System.Collections.Generic;
using UnityEngine;

public class DPAgent : RLAgent
{

    public readonly Dictionary<GridPosition, float> values = new();

    public DPAgent(GridWorldEnvironment env, float gamma = 0.9f, float theta = 0.000001f) : base(env, gamma, theta)
    {
    }


    public override void Train()
    {
        for (int x = 0; x < env.sizeX; x++)
        {
            for (int z = 0; z < env.sizeZ; z++)
            {
                for (int y = 0; y < env.sizeZ; y++)
                {
                    values[new GridPosition(x, y, z)] = 0f;
                }
            }
        }

        while (true)
        {
            float delta = 0f;

            for (int x = 0; x < env.sizeX; x++)
            {
                for (int z = 0; z < env.sizeZ; z++)
                {
                    for (int y = 0; y < env.sizeZ; y++)
                    {
                        GridPosition state = new GridPosition(x, y, z);

                        if (env.IsTerminal(state))
                        {
                            continue;
                        }

                        float oldValue = values[state];
                        float bestValue = float.NegativeInfinity;

                        foreach (string action in env.actions.Keys)
                        {
                            var result = env.Step(state, action);
                            float value = result.reward + gamma * values[result.next];

                            if (value > bestValue)
                            {
                                bestValue = value;
                            }
                        }

                        values[state] = bestValue;
                        delta = Mathf.Max(delta, Mathf.Abs(oldValue - values[state]));
                    }
                }
            }

            if (delta < theta)
            {
                break;
            }
        }
    }

    public override void ExtractPolicy()
    {
        for (int x = 0; x < env.sizeX; x++)
        {
            for (int z = 0; z < env.sizeZ; z++)
            {
                for (int y = 0; y < env.sizeZ; y++)
                {
                    GridPosition state = new GridPosition(x, y, z);


                    if (state == env.goal)
                    {
                        policy[state] = "G";
                        continue;
                    }

                    if (env.IsTrap(state))
                    {
                        policy[state] = "X";
                        continue;
                    }

                    string bestAction = "?";
                    float bestValue = float.NegativeInfinity;

                    foreach (string action in env.actions.Keys)
                    {
                        var result = env.Step(state, action);
                        float value = result.reward + gamma * values[result.next];

                        if (value > bestValue)
                        {
                            bestValue = value;
                            bestAction = action;
                        }
                    }

                    policy[state] = bestAction;

                }
            }
        }
    }

}
