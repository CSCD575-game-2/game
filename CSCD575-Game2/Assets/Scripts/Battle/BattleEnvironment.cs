using System.Collections.Generic;
using UnityEngine;

public class BattleEnvironment : MonoBehaviour
{
    public readonly List<SpaceshipAgent> allShips = new();
    public readonly List<SpaceshipAgent> dockedFighters = new();
    public readonly List<SpaceshipAgent> dockedEnemyFighters = new();

    public int sizeX { get; private set; }
    public int sizeY { get; private set; }
    public int sizeZ { get; private set; }
    public float tileSpacing { get; private set; }

    public GridPosition playerMothershipPosition;
    public GridPosition enemyMothershipPosition;

    //public GridPosition CurrentPlayerGoal { get; private set; }
    //public GridPosition CurrentEnemyGoal { get; private set; }
    //
    [Range(0f, 1f)]
    public float playerAggression = 0.5f;

    [Range(0f, 1f)]
    public float enemyAggression = 0.1f;

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

    public void SetPlayerAggression(float value)
    {
        playerAggression = Mathf.Clamp01(value);
    }

    public void SetEnemyAggression(float value)
    {
        enemyAggression = Mathf.Clamp01(value);
    }

    public float GetAggressionForShip(SpaceshipAgent ship)
    {
        return ship.team == ShipTeam.Player
            ? playerAggression
            : enemyAggression;
    }

    private Vector3 GetCommanderGoalWorldPosition(SpaceshipAgent ship)
    {
        Vector3 home;
        Vector3 enemy;

        if (ship.team == ShipTeam.Player)
        {
            home = GridToWorld(playerMothershipPosition);
            enemy = GridToWorld(enemyMothershipPosition);
        }
        else
        {
            home = GridToWorld(enemyMothershipPosition);
            enemy = GridToWorld(playerMothershipPosition);
        }

        if (ship.status == ShipStatus.ReturningHome)
        {
            return ship.team == ShipTeam.Player
                ? GridToWorld(playerMothershipPosition)
                : GridToWorld(enemyMothershipPosition);
        }

       float aggression = GetAggressionForShip(ship);
       return Vector3.Lerp(home, enemy, aggression); 

    }

    public void RegisterShip(SpaceshipAgent ship)
    {
        if (!allShips.Contains(ship))
        {
            allShips.Add(ship);
        }
        if (ship.role == ShipRole.Fighter &&
            ship.status == ShipStatus.Docked &&
            ship.team == ShipTeam.Player &&
            !dockedFighters.Contains(ship))
        {
            dockedFighters.Add(ship);
        }

        if (ship.role == ShipRole.Fighter &&
            ship.team == ShipTeam.Enemy &&
            ship.status == ShipStatus.Docked &&
            !dockedEnemyFighters.Contains(ship))
        {
            dockedEnemyFighters.Add(ship);
        }
    }

    public void UnregisterShip(SpaceshipAgent ship)
    {
        allShips.Remove(ship);
    }

    public SpaceshipAgent GetDockedEnemyFighter()
    {
        if (dockedEnemyFighters.Count == 0)
            return null;

        SpaceshipAgent ship = dockedEnemyFighters[0];

        dockedEnemyFighters.RemoveAt(0);

        return ship;
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
            ship.team == ShipTeam.Player &&
            !dockedFighters.Contains(ship))
        {
            dockedFighters.Add(ship);
        }

        if (ship.role == ShipRole.Fighter &&
            ship.team == ShipTeam.Enemy &&
            !dockedEnemyFighters.Contains(ship))
        {
            dockedEnemyFighters.Add(ship);
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

            case "HoldPosition":
                next = state;
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
        
        Vector3 goalWorld = GetCommanderGoalWorldPosition(ship);

        float oldDistance = Vector3.Distance(
                GridToWorld(state),
                goalWorld
                );

        float newDistance = Vector3.Distance(
                GridToWorld(nextState),
                goalWorld
                );

        if (newDistance < oldDistance)
            reward += 1f;
        else if (newDistance > oldDistance)
            reward -= 1f;
        else
            reward -= 0.2f;

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

    //public void SetAttackGoal()
    //{
        //CurrentEnemyGoal = playerMothershipPosition;
        //CurrentPlayerGoal = enemyMothershipPosition;
    //}

    //public void SetReturnHomeGoal()
    //{
        //CurrentEnemyGoal = enemyMothershipPosition;
        //CurrentPlayerGoal = playerMothershipPosition;
    //}

    public bool IsAtHomeMothership(SpaceshipAgent ship)
    {
        if (ship.team == ShipTeam.Player)
        {
            return ship.CurrentState.Equals(
                    playerMothershipPosition
                    );
        }

        return ship.CurrentState.Equals(
                enemyMothershipPosition
                );
    }

    public bool IsAttackAction(string action)
    {
        return action.StartsWith("Attack");
    }

    public GridPosition GetAttackTarget(GridPosition state, string action)
    {
        return action switch
        {
            "AttackLeft" => new GridPosition(state.x - 1, state.y, state.z),
            "AttackRight" => new GridPosition(state.x + 1, state.y, state.z),
            "AttackForward" => new GridPosition(state.x, state.y, state.z + 1),
            "AttackBack" => new GridPosition(state.x, state.y, state.z - 1),
            _ => state
        };
    }

    public float ResolveAttack(SpaceshipAgent attacker, string action)
    {
        GridPosition targetPos = GetAttackTarget(attacker.CurrentState, action);

        if (!IsWithinBounds(targetPos))
            return -2f;

        SpaceshipAgent target = GetShipAtPosition(targetPos, attacker.team);

        attacker.PlayAttackEffect(GridToWorld(targetPos));

        Debug.Log($"Attacker: {attacker.name}, Action: {action}, TargetPos: {targetPos}, Target: {(target != null ? target.name : "None")}");

        if (target == null)
            return -0.5f; // missed shot

        target.TakeDamage(25f);

        if (target.status == ShipStatus.Destroyed)
            return 10f;

        return 3f;
    }

    private SpaceshipAgent GetShipAtPosition(GridPosition pos, ShipTeam attackerTeam)
    {
        foreach (SpaceshipAgent ship in allShips)
        {
            if (ship.status == ShipStatus.Destroyed)
                continue;

            if (ship.team == attackerTeam)
                continue;

            if (ship.CurrentState.Equals(pos))
                return ship;
        }

        return null;
    }

    public ShipState GetShipState(SpaceshipAgent ship)
    {
        GridPosition pos = ship.CurrentState;

        return new ShipState(
                pos,
                HasEnemyAt(ship, new GridPosition(pos.x - 1, pos.y, pos.z)),
                HasEnemyAt(ship, new GridPosition(pos.x + 1, pos.y, pos.z)),
                HasEnemyAt(ship, new GridPosition(pos.x, pos.y, pos.z + 1)),
                HasEnemyAt(ship, new GridPosition(pos.x, pos.y, pos.z - 1))
                );
    }

    public bool HasEnemyAt(SpaceshipAgent ship, GridPosition pos)
    {
        foreach (SpaceshipAgent other in allShips)
        {
            if (other == ship)
                continue;

            if (other.status == ShipStatus.Destroyed ||
                    other.status == ShipStatus.Docked)
                continue;

            if (other.team == ship.team)
                continue;

            if (other.CurrentState.Equals(pos))
                return true;
        }

        return false;
    }
}
