using System.Collections.Generic;
using System;


public class MCAgent : RLAgent
{
    private readonly float epsilon = 0.1f;

    // (stage, action), list of returns key value pairs:
    Dictionary<Tuple<GridPosition, string>, float> qValues = new();
    Dictionary<Tuple<GridPosition, string>, List<float>> returns = new();

    // episode is a list of (state, action, reward) tuples
    List<Tuple<GridPosition, string, float>> episode = new();

    public MCAgent(GridWorldEnvironment env, float gamma = 0.9f, float theta = 0.000001f) : base(env, gamma, theta)
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
                    var state = new GridPosition(x, y, z);

                    foreach (string action in env.actions.Keys)
                    {
                        //qValues[new Tuple<GridPosition, string>(state, env.actions[action])] = 0f;
                        //returns[new Tuple<GridPosition, string>(state, env.actions[action])] = new List<float>();
                    }
                }
            }
        }
    }


    public override void ExtractPolicy()
    {
    }

}
