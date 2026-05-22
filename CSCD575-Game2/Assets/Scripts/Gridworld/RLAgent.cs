using System.Collections;
using System.Collections.Generic;

public abstract class RLAgent
{
    protected readonly GridWorldEnvironment env;
    public readonly Dictionary<GridPosition, string> policy;

    protected readonly float gamma;
    protected readonly float theta = 0.01f; // Learning rate


    public RLAgent(GridWorldEnvironment env, float gamma = 0.9f, float theta = 0.01f)
    {
        this.env = env;
        this.gamma = gamma;
        this.theta = theta;
        this.policy = new Dictionary<GridPosition, string>();
    }

    public abstract void Train();

    public abstract void ExtractPolicy();

}
