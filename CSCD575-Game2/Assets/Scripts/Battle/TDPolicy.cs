using System.Collections.Generic;
using UnityEngine;

public class TDPolicy : IRLPolicy
{
    //private readonly Dictionary<(GridPosition, string), float> qValues = new();
    private readonly Dictionary<(ShipState, string), float> qValues = new();

    private string[] actions =
    {
        "Up",
        "Down",
        "Left",
        "Right",
        "Forward",
        "Back"
    };

    //private readonly float alpha;
    //private readonly float gamma;

    public float Epsilon { get; set; }
    public float Alpha { get; set; }
    public float Gamma { get; set; }

    public TDPolicy(float alpha = 0.2f, float gamma = 0.9f, float epsilon = 0.2f)
    {
        Epsilon = epsilon;
        Alpha = alpha;
        Gamma = gamma;
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
                "AttackLeft",
                "AttackRight",
                "AttackForward",
                "AttackBack"
            };
        }

        //GridPosition state = ship.CurrentState;
        ShipState state = env.GetShipState(ship);

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
            ShipState state,
            string action,
            float reward,
            ShipState nextState)
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

    private float GetQ(ShipState state, string action)
    {
        var key = (state, action);

        if (!qValues.ContainsKey(key))
            qValues[key] = 0f;

        return qValues[key];
    }

    
    //private float GetQ(GridPosition state, string action)
    //{
        //var key = (state, action);

        //if (!qValues.ContainsKey(key))
        //{
            //qValues[key] = 0f;
        //}

        //return qValues[key];
    //}
}
