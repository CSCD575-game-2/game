using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

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
    

    public  bool playerFighterPriority = true;

    //public GridPosition CurrentPlayerGoal { get; private set; }
    //public GridPosition CurrentEnemyGoal { get; private set; }
    //
    [Range(0f, 1f)]
    public float playerAggression = 0.5f;

    [Range(0f, 1f)]
    public float enemyAggression = 0.1f;


    [SerializeField] private LineRenderer resupplyLine;
    [SerializeField] private float resupplyEffectTime = 0.25f;
    private Coroutine resupplyEffectCoroutine;

    [SerializeField] private GameObject attackBeamPrefab;
    [SerializeField] private float beamLifetime = 0.5f;

    public static class RadioChatter
    {
        private static readonly string[] fighterLines =
        {
            "Contact!",
            "Weapons hot!",
            "Target acquired.",
            "Scratch one drone.",
            "KILL KILL KILL!",
            "Die muthafucka!",
            "Boom goes the dynamite!",
            "Another one bites the dust!",
            "Bullseye!",
            "That one's finished!",
            "Send another!",
            "Target obliterated!",
            "He's done!",
            "Back to the scrapyard!",
            "Nothing but debris now!",
            "Burn, you toaster!",
            "Take that, tin can!",
            "Enemy fighter neutralized!",
            "That was too easy!",
            "Another drone bites the dust!",
            "Right through the cockpit!",
            "Enemy fighter disintegrating!",
            "Target reduced to atoms!",
            "Got him dead center!",
            "Target is history!",
            "That's a confirmed kill!",
            "Drone turned into confetti!",
            "I barely had to aim!",
            "Enemy fighter shattered!",
            "That's what you get!",
            "Target terminated!",
            "Another victory for the fleet!",
            "Enemy ship destroyed!",
            "Drone eradicated!",
            "One shot, one kill!",
            "That's going on my scorecard!",
            "I got another one!",
            "Target annihilated!",
            "Enemy fighter is breaking up!",
            "Nothing left but wreckage!",
            "That drone never saw it coming!",
            "Send me a real challenge!",
            "Easy kill!",
            "Target extinguished!",
            "Another machine bites the vacuum!",
        };
        private static readonly string[] fighterDistressLines =
        {
            "I'm hit!",
            "Taking heavy fire!",
            "I can't shake him!",
            "Mayday! Mayday!",
            "Need assistance!",
            "I'm losing control!",
            "Systems failing!",
            "I'm taking damage!",
            "I've got multiple contacts on me!",
            "Where's my wingman?!",
            "Shields are gone!",
            "Hull breach!",
            "I've lost engine power!",
            "My controls are locked up!",
            "They're all over me!",
            "I need support now!",
            "I can't hold them off!",
            "I'm not gonna make it!",
            "This is it for me!",
            "Tell my wife I love her!",
            "Tell my kids I fought well!",
            "Good hunting, boys.",
            "It's been an honor flying with you.",
            "I regret nothing!",
            "At least take one with me!",
            "Not like this!",
            "I'm venting atmosphere!",
            "I'm losing altitude!",
            "I can't recover!",
            "They got me!",
            "I've taken critical damage!",
            "I've lost weapons!",
            "Everything's on fire!",
            "This bird's finished!",
            "Somebody get these drones off me!",
            "I need a resupply ship!",
            "I'm out of options!",
            "Commander, we've got a problem!",
            "I can see the stars through the hull!",
            "I think they got my reactor!",
            "We're done for!",
            "See you on the other side!",
            "For the fleet!",
            "For humanity!"
        };
        private static readonly string[] fighterDefeatedLines =
        {
            "NOOOOOOOOOOOOO...",
            "Tell my family I love them...",
            "I can't go on...",
            "Welp, this is awkward...",
            "Guys, I just want to say I'm the one who clogged the toilet...",
            "I've made a terrible mistake.",
            "This wasn't in the flight manual!",
            "I knew I should have stayed in the simulator.",
            "At least I won't have to do paperwork.",
            "Remember me as a hero!",
            "I'm becoming one with the stars.",
            "Tell Commander Adama this is entirely his fault.",
            "I regret nothing!",
            "Actually, I regret several things.",
            "I can see my house from here!",
            "My ship appears to be on fire.",
            "Good luck, you're gonna need it.",
            "Well, that's not ideal.",
            "The drones are cheating!",
            "I was having such a good day, too.",
            "I should have called in sick.",
            "Tell my wingman he still owes me twenty credits.",
            "I've got a bad feeling about this...",
            "I don't think that's supposed to happen.",
            "One day from retirement...",
            "I never liked this ship anyway.",
            "If anybody asks, I got at least three of them.",
            "I blame the engineers.",
            "I blame the commander.",
            "I blame myself.",
            "Mostly the commander, though.",
            "May my kill count live on!",
            "This is not how I imagined my promotion.",
            "Well... frak.",
            "The stars are beautiful tonight.",
            "Goodbye, cruel vacuum.",
            "I think I just hit something important.",
            "I've lost everything except my sense of humor.",
            "Don't let them turn my bunk into storage.",
            "See you all on the other side."
        };

        private static readonly string[] fighterBoastLines =
        {
            "Is that all you've got?",
            "Too easy!",
            "Another toaster for the pile!",
            "You'll have to do better than that!",
            "My grandma hits harder than you!",
            "Come on, fight back!",
            "You're making this too easy!",
            "I almost feel bad about that one.",
            "Almost.",
            "Did they train you in a scrapyard?",
            "I've seen better flying from cargo haulers.",
            "You're outmatched, drone.",
            "I could do this all day.",
            "Target practice complete.",
            "That barely counts as a kill.",
            "Send me a real pilot!",
            "I hope they built more of you.",
            "That's another mark on my hull.",
            "You picked the wrong fleet.",
            "Who's next?",
            "I'm just getting started.",
            "You call that evasive maneuvering?",
            "My targeting computer is getting bored.",
            "I didn't even break formation.",
            "One shot. One kill.",
            "Too slow!",
            "Try harder!",
            "Back to the factory with you.",
            "I expected more resistance.",
            "Another victory for the good guys.",
            "You flew right into that one.",
            "That was embarrassing.",
            "I think that drone was already broken.",
            "That kill belongs in the academy textbooks.",
            "I'm making this look easy.",
            "Commander better be writing this down.",
            "That's going on my scorecard.",
            "The fleet owes me a medal.",
            "I'm carrying this battle.",
            "Just another day in the cockpit.",
            "I haven't even warmed up yet.",
            "You're flying like a recruit.",
            "You should've stayed docked.",
            "Looks like I'm buying drinks tonight.",
            "King of the skies, reporting in."
        };
        public static string GetRandomFighterLine()
        {
            return fighterLines[Random.Range(0, fighterLines.Length)];
        }
        public static string GetRandomFighterDistressLine()
        {
            return fighterDistressLines[Random.Range(0, fighterDistressLines.Length)];
        }

        public static string GetRandomFighterDefeatedLine()
        {
            return fighterDefeatedLines[Random.Range(0, fighterDefeatedLines.Length)];
        }

        public static string GetRandomFighterBoastLine()
        {
            return fighterBoastLines[Random.Range(0, fighterBoastLines.Length)];
        }
    }
    private float nextRadioMessageTime = 0f;
    [SerializeField] private float radioMessageCooldown = 0.5f;

    [SerializeField] private TextMeshProUGUI battleStatusText;


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
    public void PlayResupplyEffect(Vector3 supplierPos, Vector3 targetWorldPos)
    {
        if (resupplyEffectCoroutine != null)
        {
            StopCoroutine(resupplyEffectCoroutine);
            resupplyLine.enabled = false;
        }

        resupplyEffectCoroutine =
            StartCoroutine(ResupplyEffect(supplierPos, targetWorldPos));
    }

    private IEnumerator ResupplyEffect(Vector3 supplierPos, Vector3 targetWorldPos)
    {
        resupplyLine.enabled = true;
        resupplyLine.SetPosition(0, supplierPos);
        resupplyLine.SetPosition(1, targetWorldPos);

        yield return new WaitForSeconds(resupplyEffectTime);

        resupplyLine.enabled = false;
        resupplyEffectCoroutine = null;
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

    public void ResetEnvironmentLevel1()
    {
        battleStatusText.text = "";
        for (int i = allShips.Count - 1; i >= 0; i--)
        {
            SpaceshipAgent ship = allShips[i];
            if (ship != null && ship.status == ShipStatus.Destroyed)
            {
                Destroy(ship.gameObject);
                allShips.RemoveAt(i);
            }
            else
            {
                DockShip(ship);
            }
        }
        for (int i = dockedFighters.Count - 1; i >= 0; i--)
        {
            SpaceshipAgent ship = dockedFighters[i];
            if (ship != null && ship.status == ShipStatus.Destroyed)
            {
                Destroy(ship.gameObject);
                dockedFighters.RemoveAt(i);
            }
        }
        for (int i = dockedEnemyFighters.Count - 1; i >= 0; i--)
        {            SpaceshipAgent ship = dockedEnemyFighters[i];
            if (ship != null && ship.status == ShipStatus.Destroyed)
            {                
                Destroy(ship.gameObject);
                dockedEnemyFighters.RemoveAt(i);

            }
        }
        for (int i = dockedResupplyShips.Count - 1; i >= 0; i--)
        {
            SpaceshipAgent ship = dockedResupplyShips[i];
            if (ship != null && ship.status == ShipStatus.Destroyed)
            {
                Destroy(ship.gameObject);
                dockedResupplyShips.RemoveAt(i);
            }
        }
        dockedFighters.Clear();
        dockedEnemyFighters.Clear();
        dockedResupplyShips.Clear();
        allShips.Clear();


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
            Debug.Log($"*** Resolving attack for {attacker.name}-{attacker.ID}" + 
                    $"({attacker.status}), at {attackerPos}, Action: {action}, TargetPos: {targetPos}");
        }

        if (target == null && mothership == null)
            return -0.5f; // missed shot

        attacker.PlayAttackEffect(GridToWorld(targetPos));
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLaserShot();
        }
        
        float damage = 25f;

        if (attacker.team == ShipTeam.Player) 
        {
           if (playerFighterPriority)
           {
                if (target != null)
                {
                    target.TakeDamage(damage);
                    TryAppendBattleStatus($"{attacker.Callsign}: {RadioChatter.GetRandomFighterLine()} Attacked {target.Callsign} for {damage}!"); 

                    if (target.status == ShipStatus.Destroyed && target.role == ShipRole.Fighter)
                        return 10f;

                    return 5f;

                }
                if (mothership != null)
                {
                    if (mothership.currentHealth - damage <= 0)
                    {
                        for (int i = 0; i < allShips.Count; i++)
                        {
                            SpaceshipAgent ship = allShips[i];
                            if (ship.team == ShipTeam.Enemy && ship.status != ShipStatus.Destroyed)
                            {
                                AppendBattleStatus($"{ship.Callsign}: {RadioChatter.GetRandomFighterBoastLine()}");
                                //ship.TakeDamage(100f);
                            }
                        }
                    }
                    mothership.TakeDamage(damage);
                    if (mothership.team == ShipTeam.Enemy)
                        TryAppendBattleStatus($"{attacker.Callsign} attacked the Basestar for {damage}!");

                    if (mothership.status == ShipStatus.Destroyed)
                        return 50f;
                }
           } else {

                if (mothership != null)
                {
                    mothership.TakeDamage(damage);
                    if (mothership.team == ShipTeam.Enemy)
                        TryAppendBattleStatus($"{attacker.Callsign} attacked the Basestar for {damage}!");

                    if (mothership.status == ShipStatus.Destroyed)
                        return 50f;
                }
                if (target != null)
                {
                    target.TakeDamage(damage);
                    TryAppendBattleStatus($"{attacker.Callsign}: {RadioChatter.GetRandomFighterLine()} Attacked {target.Callsign} for {damage}!"); 

                    if (target.status == ShipStatus.Destroyed && target.role == ShipRole.Fighter)
                        return 10f;

                    return 5f;
                }
           }
        }
        else
        {
            if (target != null)
            {
                target.TakeDamage(damage);
                TryAppendBattleStatus($"{target.Callsign}: {RadioChatter.GetRandomFighterDistressLine()} Attacked {target.Callsign} for {damage}!"); 
                if (target.status == ShipStatus.Destroyed && target.role == ShipRole.Fighter)
                    return 10f;
                return 5f;
            }
            if (mothership != null)
                if (mothership.currentHealth - damage <= 0)
                {
                    for (int i = 0; i < allShips.Count; i++)
                    {
                        SpaceshipAgent ship = allShips[i];
                        if (ship.team == ShipTeam.Player && ship.status != ShipStatus.Destroyed)
                        {
                            AppendBattleStatus($"{ship.Callsign}: {RadioChatter.GetRandomFighterDefeatedLine()}");
                            //ship.TakeDamage(100f);
                        }
                    }
                }

                mothership.TakeDamage(damage);
            if (mothership.team == ShipTeam.Player)
                TryAppendBattleStatus($"{attacker.Callsign} attacked the Galactica for {damage}!");
            if (mothership.status == ShipStatus.Destroyed) {
                return 50f;
            }

            return 20f;
        } 
        return 3f;
    }

    public void TogglePlayerPriority() {
        playerFighterPriority = !playerFighterPriority;
    }

    public void TryAppendBattleStatus(string message)
    {
        if (Time.time < nextRadioMessageTime)
            return;

        nextRadioMessageTime =
            Time.time + radioMessageCooldown;

        AppendBattleStatus(message);
    }

    public void AppendBattleStatus(string message)
    {
        string existingText = battleStatusText.text;
        battleStatusText.text = $"{existingText}\n{message}";

        // take last 5 lines
        string[] lines = battleStatusText.text.Split('\n');
        if (lines.Length > 5)
        {
            battleStatusText.text = string.Join("\n", lines[^5..]);
        }
    }
    
    public float ResolveResupply(SpaceshipAgent supplier, string action)
    {
        if (action != "Resupply")
            return 0f;

        SpaceshipAgent target = GetBestAdjacentResupplyTarget(supplier);

        if (target == null)
            return -1f;

        target.Refuel(0.5f);
        target.Repair(0.75f);
        PlayResupplyEffect(supplier.transform.position, target.transform.position);

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
    public void PlayerMothershipFire()
    {
        if (!playerMothership.HasAmmo)
        {
            AppendBattleStatus("Mothership batteries depleted.");
            return;
        }

        GridPosition pos = playerMothershipPosition;

        GridPosition[] targets =
        {
            new GridPosition(pos.x - 1, pos.y, pos.z),
            new GridPosition(pos.x + 1, pos.y, pos.z),
            new GridPosition(pos.x, pos.y, pos.z + 1),
            new GridPosition(pos.x, pos.y, pos.z - 1),
        };

        bool hitSomething = false;

        foreach (GridPosition targetPos in targets)
        {
            SpaceshipAgent target = GetShipAtPosition(targetPos, ShipTeam.Player);

            if (target == null)
                continue;

            PlayMothershipAttackEffect(GridToWorld(targetPos));

            target.TakeDamage(50f);
            hitSomething = true;

            AppendBattleStatus($"Mothership fired on {target.Callsign}.");
        }

        playerMothership.UseAmmo(1);

        if (!hitSomething)
            AppendBattleStatus("Mothership fired. No targets in range.");
    }


    public void PlayMothershipAttackEffect(Vector3 targetWorldPos)
    {
        //StartCoroutine(AttackEffect(targetWorldPos));

        Vector3 start = transform.position;
        Vector3 end = targetWorldPos;

        Vector3 midpoint = (start + end) * 0.5f;
        Vector3 direction = end - start;

        GameObject beam = Instantiate(
            attackBeamPrefab,
            midpoint,
            Quaternion.LookRotation(direction.normalized)
        );

        beam.transform.localScale = new Vector3(
            0.2f,
            0.2f,
            direction.magnitude
        );

        Destroy(beam, beamLifetime);
    }
}
