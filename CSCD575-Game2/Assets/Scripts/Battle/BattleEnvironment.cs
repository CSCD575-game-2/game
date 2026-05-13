using System.Collections.Generic;
using UnityEngine;

public class BattleEnvironment : MonoBehaviour
{
    public readonly List<SpaceshipAgent> allShips = new();
    public readonly List<SpaceshipAgent> dockedFighters = new();

    public int sizeX { get; private set; }
    public int sizeY { get; private set; }
    public int sizeZ { get; private set; }
    public float tileSpacing { get; private set; }

    public GridPosition playerMothershipPosition;
    public GridPosition enemyMothershipPosition;

    public GridPosition CurrentGoal { get; private set; }

    public void Initialize(int sizeX, int sizeY, int sizeZ, float tileSpacing)
    {
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.sizeZ = sizeZ;
        this.tileSpacing = tileSpacing;

        playerMothershipPosition = new GridPosition(0, 0, 0);
        enemyMothershipPosition = new GridPosition(
            sizeX - 1,
            sizeY - 1,
            sizeZ - 1
        );
    }

    public void RegisterShip(SpaceshipAgent ship)
    {
        if (!allShips.Contains(ship))
        {
            allShips.Add(ship);
        }
        if (ship.role == ShipRole.Fighter &&
            ship.status == ShipStatus.Docked &&
            !dockedFighters.Contains(ship))
        {
            dockedFighters.Add(ship);
        }
    }

    public void UnregisterShip(SpaceshipAgent ship)
    {
        allShips.Remove(ship);
    }

    public SpaceshipAgent GetDockedFighter()
    {
        if (dockedFighters.Count == 0)
            return null;

        SpaceshipAgent ship = dockedFighters[0];
        dockedFighters.RemoveAt(0);
        return ship;
    }

    public void DockShip(SpaceshipAgent ship)
    {
        ship.status = ShipStatus.Docked;

        if (ship.role == ShipRole.Fighter &&
            !dockedFighters.Contains(ship))
        {
            dockedFighters.Add(ship);
        }
    }

    public Vector3 GridToWorld(GridPosition pos)
    {
        return new Vector3(
            pos.x * tileSpacing,
            pos.y * tileSpacing,
            pos.z * tileSpacing
        );
    }

    public GridPosition GetNextState(GridPosition state, string action)
    {
        GridPosition next = state;

        switch (action)
        {
            case "Up":
                next = new GridPosition(state.x, state.y + 1, state.z);
                break;

            case "Down":
                next = new GridPosition(state.x, state.y - 1, state.z);
                break;

            case "Left":
                next = new GridPosition(state.x - 1, state.y, state.z);
                break;

            case "Right":
                next = new GridPosition(state.x + 1, state.y, state.z);
                break;

            case "Forward":
                next = new GridPosition(state.x, state.y, state.z + 1);
                break;

            case "Back":
                next = new GridPosition(state.x, state.y, state.z - 1);
                break;
        }

        //next.x = Mathf.Clamp(next.x, 0, sizeX - 1);
        //next.y = Mathf.Clamp(next.y, 0, sizeY - 1);
        //next.z = Mathf.Clamp(next.z, 0, sizeZ - 1);
        
        if (!IsWithinBounds(next))
        {
            return state;
        }

        return next;
    }

    public bool IsWithinBounds(GridPosition pos)
    {
        return
            pos.x >= 0 &&
            pos.x < sizeX &&
            pos.y >= 0 &&
            pos.y < sizeY &&
            pos.z >= 0 &&
            pos.z < sizeZ;
    }

    public float GetReward(
        SpaceshipAgent ship,
        GridPosition state,
        string action,
        GridPosition nextState)
    {
        float reward = -0.1f;

        int oldDistance = ManhattanDistance(state, CurrentGoal);
        int newDistance = ManhattanDistance(nextState, CurrentGoal);

        if (nextState.Equals(state))
        {
            reward -= 2f;
            return reward;
        }

        if (newDistance < oldDistance)
            reward += 1f;

        if (newDistance > oldDistance)
            reward -= 1f;

        if (nextState.Equals(CurrentGoal))
            reward += 20f;

        //if (ship.role == ShipRole.Fighter)
        //{
            //reward += 0.2f;
        //}

        //if (ship.directive == ShipDirective.Attack)
        //{
            //reward += nextState.z * 0.05f;
        //}

        return reward;

    }

    private int ManhattanDistance(GridPosition a, GridPosition b)
    {
        return Mathf.Abs(a.x - b.x)
             + Mathf.Abs(a.y - b.y)
             + Mathf.Abs(a.z - b.z);
    }

    public void SetAttackGoal()
    {
        CurrentGoal = enemyMothershipPosition;
    }

    public void SetReturnHomeGoal()
    {
        CurrentGoal = playerMothershipPosition;
    }

    public bool IsAtPlayerMothership(GridPosition pos)
    {
        return pos.Equals(playerMothershipPosition);
    }

    //public bool IsTerminal()
    //{
        //return false;
    //}

}
