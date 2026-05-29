using System.Collections.Generic;
using UnityEngine;

public class BattleEnvironment : MonoBehaviour
{
    public readonly List<SpaceshipAgent> allShips = new();
    public readonly List<SpaceshipAgent> dockedFighters = new();
    public readonly List<SpaceshipAgent> dockedResupplyShips = new();
    public readonly List<SpaceshipAgent> dockedEnemyFighters = new();

    public int sizeX { get; private set; }
    public int sizeY { get; private set; }
    public int sizeZ { get; private set; }
    public float tileSpacing { get; private set; }

    public GridPosition playerMothershipPosition;
    public GridPosition enemyMothershipPosition;

    public Mothership playerMothership { get; set; }
    public Mothership enemyMothership { get; set; }

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

    public int GetDockedFighterCount(ShipTeam team)
    {
        if (team == ShipTeam.Player)
            return dockedFighters.Count;
        else
            return dockedEnemyFighters.Count;
    }
    public int GetDockedResupplyShipCount()
    {
        return dockedResupplyShips.Count;
    }

    public void ResetEnvironment()
    {
        for (int i = allShips.Count - 1; i >= 0; i--)
        {
            SpaceshipAgent ship = allShips[i];
            if (ship != null && ship.status == ShipStatus.Destroyed || ship.status == ShipStatus.Disabled)
            {
                Destroy(ship.gameObject);
                allShips.RemoveAt(i);
            }
            else {
                DockShip(ship);
            }
        }
    }

    public void DockShip(SpaceshipAgent ship) {
        if (ship == null)
            return;

        GridPosition dockPos =
            ship.team == ShipTeam.Player
                ? playerMothershipPosition
                : enemyMothershipPosition;

        ship.DockForNextLevel(this, dockPos); 

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
        if (ship.role == ShipRole.Resupply &&
            ship.team == ShipTeam.Player &&
            !dockedResupplyShips.Contains(ship))
        {
            dockedResupplyShips.Add(ship);
        }

        ship.transform.position = GridToWorld(
                ship.team == ShipTeam.Player
                    ? playerMothershipPosition
                    : enemyMothershipPosition
                );
        ship.currentState = ship.team == ShipTeam.Player
            ? playerMothershipPosition
            : enemyMothershipPosition;
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

    public void SetMotherships(
        Mothership player,
        Mothership enemy)
    {
        playerMothership = player;
        enemyMothership = enemy;
    }

    public void RegisterMothership(Mothership mothership)
    {
        if (mothership.team == ShipTeam.Player)
            playerMothership = mothership;
        else
            enemyMothership = mothership;
    }

    public void RegisterShip(SpaceshipAgent ship)
    {
        Debug.Log($"Registering ship: {ship.name}-{ship.ID} ({ship.status}), Role: {ship.role}, Team: {ship.team}");
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

        if (ship.role == ShipRole.Resupply &&
            ship.status == ShipStatus.Docked &&
            ship.team == ShipTeam.Player &&
            !dockedResupplyShips.Contains(ship))
        {
            dockedResupplyShips.Add(ship);
        }
    }

    public void UnregisterShip(SpaceshipAgent ship)
    {
        allShips.Remove(ship);
    }

    public SpaceshipAgent GetDockedResupplyShip()
    {
        if (dockedResupplyShips.Count == 0)
            return null;

        SpaceshipAgent ship = dockedResupplyShips[0];
        dockedResupplyShips.RemoveAt(0);
        return ship;
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


    public Vector3 GridToWorld(GridPosition pos)
    {
        return new Vector3(
            pos.x * tileSpacing,
            pos.y * tileSpacing,
            pos.z * tileSpacing
        );
    }

    public Vector3 GridToWorldWithNoise(GridPosition pos)
    {
        Vector3 center = GridToWorld(pos);

        float noiseRadius = tileSpacing * 0.35f;

        Vector3 noise = new Vector3(
                Random.Range(-noiseRadius, noiseRadius),
                Random.Range(-noiseRadius * 0.2f, noiseRadius * 0.2f),
                Random.Range(-noiseRadius, noiseRadius)
                );

        return center + noise;
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

    public float GetMovementReward(
            SpaceshipAgent ship,
            GridPosition state,
            string action,
            GridPosition nextState)
    {
        if (ship.role == ShipRole.Resupply) {
            return GetResupplyMovementReward(ship, state, action, nextState);
        } 
        else if (ship.role == ShipRole.Fighter)
        {
            return GetFighterMovementReward(ship, state, action, nextState);
        }
        else {
            return -0.1f;
        }
    }

    private float GetFighterMovementReward(
        SpaceshipAgent ship,
        GridPosition state,
        string action,
        GridPosition nextState)
    {
        float reward = -0.1f;

        if (HasAttackTargetAdjacent(ship, state))
            reward -= 2f;
        
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


        return reward;

    }

    private bool HasAttackTargetAdjacent(SpaceshipAgent ship, GridPosition pos)
    {
        GridPosition[] neighbors =
        {
            new GridPosition(pos.x - 1, pos.y, pos.z),
            new GridPosition(pos.x + 1, pos.y, pos.z),
            new GridPosition(pos.x, pos.y, pos.z + 1),
            new GridPosition(pos.x, pos.y, pos.z - 1),
            new GridPosition(pos.x, pos.y + 1, pos.z),
            new GridPosition(pos.x, pos.y - 1, pos.z)
        };

        foreach (GridPosition neighbor in neighbors)
        {
            if (GetShipAtPosition(neighbor, ship.team) != null)
                return true;

            if (GetEnemyMothershipAtPosition(ship, neighbor) != null)
                return true;
        }

        return false;
    }

    private float GetResupplyMovementReward(
            SpaceshipAgent ship,
            GridPosition state,
            string action,
            GridPosition nextState)
    {
        float reward = -0.1f;

        SpaceshipAgent oldTarget = GetBestResupplyTarget(ship, state);
        SpaceshipAgent newTarget = GetBestResupplyTarget(ship, nextState);

        if (oldTarget == null && newTarget == null)
            return reward - 0.2f;

        if (newTarget != null)
        {
            float oldDistance = oldTarget == null
                ? float.MaxValue
                : GridDistance(state, oldTarget.CurrentState);

            float newDistance = GridDistance(nextState, newTarget.CurrentState);

            if (newDistance < oldDistance)
                reward += 1f;
            else if (newDistance > oldDistance)
                reward -= 1f;
        }

        reward += CountNearbyFriendlyShips(nextState, ship.team, radius: 2) * 0.2f;

        return reward;
    }

    private SpaceshipAgent GetBestResupplyTarget(SpaceshipAgent supplier, GridPosition pos)
    {
        SpaceshipAgent best = null;
        float bestScore = float.NegativeInfinity;

        foreach (SpaceshipAgent ship in allShips)
        {
            if (ship == supplier)
                continue;

            if (ship.team != supplier.team)
                continue;

            if (!ship.NeedsHelp())
                continue;

            float distance = GridDistance(pos, ship.CurrentState);
            if (distance > 3)
                continue;

            float score = GetResupplyTargetScore(ship) - distance * 0.5f;

            if (score > bestScore)
            {
                bestScore = score;
                best = ship;
            }
        }

        return best;
    }

    private int ManhattanDistance(GridPosition a, GridPosition b)
    {
        return Mathf.Abs(a.x - b.x)
             + Mathf.Abs(a.y - b.y)
             + Mathf.Abs(a.z - b.z);
    }

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

    public bool IsResupplyAction(string action)
    {
        return action == "Resupply";
    }

    public GridPosition GetAttackTarget(GridPosition state, string action)
    {
        return action switch
        {
            "AttackLeft" => new GridPosition(state.x - 1, state.y, state.z),
            "AttackRight" => new GridPosition(state.x + 1, state.y, state.z),
            "AttackForward" => new GridPosition(state.x, state.y, state.z + 1),
            "AttackBack" => new GridPosition(state.x, state.y, state.z - 1),
            "AttackUp" => new GridPosition(state.x, state.y + 1, state.z),
            "AttackDown" => new GridPosition(state.x, state.y - 1, state.z),
            _ => state
        };
    }

    public float ResolveAttack(SpaceshipAgent attacker, string action)
    {

        GridPosition targetPos = GetAttackTarget(attacker.CurrentState, action);
        GridPosition attackerPos = attacker.CurrentState;


        if (!IsWithinBounds(targetPos))
            return -2f;

        SpaceshipAgent target = GetShipAtPosition(targetPos, attacker.team);
        Mothership mothership = GetEnemyMothershipAtPosition(attacker, targetPos);

        Debug.Log("Mothership at target: " + (mothership != null ? mothership.name : "None"));

        if (mothership != null) {
            Debug.Log($"*** Resolving attack for {attacker.name}-{attacker.ID} ({attacker.status}), at {attackerPos}, Action: {action}, TargetPos: {targetPos}");
        }


        if (target == null && mothership == null)
            return -0.5f; // missed shot
        

        attacker.PlayAttackEffect(GridToWorld(targetPos));
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLaserShot();
        }

        //Debug.Log($"Attacker: {attacker.name}-{attacker.ID} ({attacker.status}), at {attackerPos}" +
                //$", Action: {action}, TargetPos: {targetPos}" +
                //$", Target: {(target != null ? target.name + "-" + target.ID + "(" + target.status + ")": "None")}" +
                //$", Mothership: {(mothership != null ? mothership.name : "None")}");


        if (mothership != null)
        {
            mothership.TakeDamage(25f);

            if (mothership.status == ShipStatus.Destroyed)
                return 50f;

            return 50f;
        } 
        else if (target != null)
        {
            target.TakeDamage(25f);

            if (target.status == ShipStatus.Destroyed && target.role == ShipRole.Fighter)
                return 20f;
            if (target.status == ShipStatus.Destroyed && target.role == ShipRole.Mothership)
                return 50f;

            return 10f;

        }

        return 3f;
    }
    
    public float ResolveResupply(SpaceshipAgent supplier, string action)
    {
        if (action != "Resupply")
            return 0f;

        SpaceshipAgent target = GetBestAdjacentResupplyTarget(supplier);

        if (target == null)
            return -1f;

        target.Refuel(25f);

        if (target.role == ShipRole.Fighter)
            return 8f;

        if (target.role == ShipRole.Resupply)
            return 2f;

        return 1f;
    }

    private SpaceshipAgent GetBestAdjacentResupplyTarget(SpaceshipAgent supplier)
    {
        SpaceshipAgent best = null;
        float bestScore = float.NegativeInfinity;

        foreach (SpaceshipAgent ship in allShips)
        {
            if (ship == supplier)
                continue;

            if (ship.team != supplier.team)
                continue;

            if (!ship.NeedsHelp())
                continue;

            if (GridDistance(supplier.CurrentState, ship.CurrentState) > 1)
                continue;

            float score = GetResupplyTargetScore(ship);

            if (score > bestScore)
            {
                bestScore = score;
                best = ship;
            }
        }

        return best;
    }

    private float GetResupplyTargetScore(SpaceshipAgent target)
    {
        float score = 0f;

        if (target.role == ShipRole.Fighter)
            score += 10f;
        else if (target.role == ShipRole.Resupply)
            score += 3f;

        score += (1f - target.FuelPercent) * 5f;

        score += CountNearbyFriendlyShips(target.CurrentState, target.team, radius: 2) * 0.5f;

        return score;
    }

    private int CountNearbyFriendlyShips(GridPosition pos, ShipTeam team, int radius)
    {
        int count = 0;

        foreach (SpaceshipAgent ship in allShips)
        {
            if (ship.team != team)
                continue;

            if (ship.status == ShipStatus.Destroyed)
                continue;

            if (GridDistance(pos, ship.CurrentState) <= radius)
                count++;
        }

        return count;
    }

    private int GridDistance(GridPosition a, GridPosition b)
    {
        return Mathf.Abs(a.x - b.x)
            + Mathf.Abs(a.y - b.y)
            + Mathf.Abs(a.z - b.z);
    }

    private Mothership GetEnemyMothershipAtPosition(
            SpaceshipAgent attacker,
            GridPosition pos)
    {
        //Debug.Log($"*** Attacker: {attacker.name}-{attacker.ID} ({attacker.status}) attacking {pos}" +
                //$", Player Mothership at {playerMothershipPosition} at ({playerMothershipPosition})" +
                //$", Enemy Mothership at {enemyMothershipPosition} at ({enemyMothershipPosition})");

        if (attacker.team == ShipTeam.Player &&
                pos.Equals(enemyMothershipPosition))
        {
            Debug.Log("Returning enemy mothership");
            return enemyMothership;
        }

        if (attacker.team == ShipTeam.Enemy &&
                pos.Equals(playerMothershipPosition))
        {
            Debug.Log("Returning player mothership");
            return playerMothership;
        }

        return null;
    }

    private SpaceshipAgent GetShipAtPosition(GridPosition pos, ShipTeam attackerTeam)
    {
        foreach (SpaceshipAgent ship in allShips)
        {
            if (ship.status == ShipStatus.Destroyed)
                continue;

            if (ship.team == attackerTeam)
                continue;

            if (ship.status == ShipStatus.Docked)
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

    public ResupplyState GetResupplyState(SpaceshipAgent ship)
    {
        GridPosition pos = ship.CurrentState;

        return new ResupplyState(
            pos,
            HasAllyNeedingHelpAt(ship, new GridPosition(pos.x - 1, pos.y, pos.z)),
            HasAllyNeedingHelpAt(ship, new GridPosition(pos.x + 1, pos.y, pos.z)),
            HasAllyNeedingHelpAt(ship, new GridPosition(pos.x, pos.y, pos.z + 1)),
            HasAllyNeedingHelpAt(ship, new GridPosition(pos.x, pos.y, pos.z - 1)),
            HasAllyNeedingHelpAt(ship, new GridPosition(pos.x, pos.y + 1, pos.z)),
            HasAllyNeedingHelpAt(ship, new GridPosition(pos.x, pos.y - 1, pos.z))
           
        );
    }

    private bool HasAllyNeedingHelpAt(SpaceshipAgent ship, GridPosition pos)
    {
        foreach (SpaceshipAgent other in allShips)
        {
            if (other == ship)
                continue;

            if (other.team != ship.team)
                continue;

            if (other.CurrentState.Equals(pos) && other.NeedsHelp())
                return true;
        }

        return false;
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

        if (ship.team == ShipTeam.Player &&
                pos.Equals(enemyMothershipPosition) &&
                enemyMothership.status != ShipStatus.Destroyed)
        {
            return true;
        }

        if (ship.team == ShipTeam.Enemy &&
                pos.Equals(playerMothershipPosition) &&
                playerMothership.status != ShipStatus.Destroyed)
        {
            return true;
        }

        return false;
    }
}
