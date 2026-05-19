using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private BattleEnvironment battleEnvironment;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button deployFighterButton;
    [SerializeField] private Button endBattleButton;

    [Header("Prefabs")]
    [SerializeField] private SpaceshipAgent fighterPrefab;
    [SerializeField] private SpaceshipAgent enemyFighterPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private int startingFighters = 5;
    [SerializeField] private int startingEnemyFighters = 5;
    [SerializeField] private Camera battleCamera;
    [SerializeField] private float enemyDeployInterval = 5f;
    private Transform playerMothership;
    private Transform enemyMothership;

    [SerializeField] private float enemyStartingAggression = 0.1f;
    [SerializeField] private float enemyAggressionIncreaseRate = 0.02f;

    [SerializeField] private GameObject playerMothershipPrefab;
    [SerializeField] private GameObject enemyMothershipPrefab;

    [Header("Commander Dials")]
    [SerializeField] private Slider exploreExploitSlider;
    [SerializeField] private Slider aggressionSlider;
    [SerializeField] private Slider strategicSlider;
    [SerializeField] private Slider adaptiveSlider;


    private float enemyDeployTimer;

    private BattlePhase phase = BattlePhase.Deployment;

    private void Start()
    {
        CreateStartingFleet();
        CreateEnemyFleet();

        deployFighterButton.interactable = false;
        endBattleButton.interactable = false;

        startButton.onClick.AddListener(StartEpisode);
        endBattleButton.onClick.AddListener(EndEpisode);
        deployFighterButton.onClick.AddListener(DeployFighter);

        aggressionSlider.onValueChanged.AddListener(UpdateAggression);
        exploreExploitSlider.onValueChanged.AddListener(UpdateFleetEpsilon);
        strategicSlider.onValueChanged.AddListener(UpdateFleetGamma);
        adaptiveSlider.onValueChanged.AddListener(UpdateFleetAlpha);

        battleEnvironment.SetPlayerAggression(aggressionSlider.value);

        SpawnMotherships();
    }

    private void Update()
    {
        if (phase == BattlePhase.Recall && AllShipsTerminal())
        {
            FinishEpisode();
        }
        if (phase == BattlePhase.ActiveBattle)
        {
            float nextEnemyAggression =
                battleEnvironment.enemyAggression +
                enemyAggressionIncreaseRate * Time.deltaTime;

            battleEnvironment.SetEnemyAggression(nextEnemyAggression);

            enemyDeployTimer += Time.deltaTime;

            if (enemyDeployTimer >= enemyDeployInterval)
            {
                enemyDeployTimer = 0f;

                DeployEnemyFighter();
                Debug.Log("Enemy aggression: " + battleEnvironment.enemyAggression);
            }
        }
    }

    private void InitializeBattleCamera()
    {
        if (battleCamera == null ||
            playerMothership == null ||
            enemyMothership == null)
        {
            return;
        }

        Vector3 direction =
            (enemyMothership.position - playerMothership.position).normalized;

        Vector3 cameraPosition =
            playerMothership.position
            - direction * 20f
            + Vector3.up * 100f;

        battleCamera.transform.position = cameraPosition;

        Vector3 lookTarget =
            Vector3.Lerp(
                playerMothership.position,
                enemyMothership.position,
                0.5f
            );

        battleCamera.transform.LookAt(lookTarget);
    }

    private void CreateStartingFleet()
    {
        for (int i = 0; i < startingFighters; i++)
        {
            SpaceshipAgent ship = Instantiate(
                fighterPrefab,
                battleEnvironment.GridToWorld(battleEnvironment.playerMothershipPosition),
                Quaternion.identity
            );

            ship.role = ShipRole.Fighter;
            ship.status = ShipStatus.Docked;
            ship.team = ShipTeam.Player;
            ship.directive = ShipDirective.ReturnHome;

            ship.gameObject.SetActive(false);

            battleEnvironment.RegisterShip(ship);
        }
    }

    private void CreateEnemyFleet()
    {
        for (int i = 0; i < startingEnemyFighters; i++)
        {
            SpaceshipAgent ship = Instantiate(
                enemyFighterPrefab,
                battleEnvironment.GridToWorld(
                    battleEnvironment.enemyMothershipPosition
                ),
                Quaternion.identity
            );

            ship.role = ShipRole.Fighter;
            ship.team = ShipTeam.Enemy;
            ship.status = ShipStatus.Docked;

            ship.gameObject.SetActive(false);

            battleEnvironment.RegisterShip(ship);
        }
    }

    private bool AllShipsTerminal()
    {
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            if (ship.status != ShipStatus.Docked &&
                ship.status != ShipStatus.Destroyed)
            {
                return false;
            }
        }

        return battleEnvironment.allShips.Count > 0;
    }

    private void FinishEpisode()
    {
        phase = BattlePhase.Finished;

        startButton.interactable = true;
        deployFighterButton.interactable = false;
        endBattleButton.interactable = false;

        Debug.Log("Episode finished. Buttons reset.");
    }

    private void UpdateFleetEpsilon(float value)
    {
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            // Only update player ships
            if (ship.team != ShipTeam.Player)
                continue;

            if (ship.Policy is TDPolicy tdPolicy)
            {
                tdPolicy.Epsilon = -value;
            }
        }
    }

    private void UpdateFleetGamma(float value)
    {
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            // Only update player ships
            if (ship.team != ShipTeam.Player)
                continue;

            if (ship.Policy is TDPolicy tdPolicy)
            {
                tdPolicy.Gamma = value;
            }
        }
    }

    private void UpdateFleetAlpha(float value)
    {
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            // Only update player ships
            if (ship.team != ShipTeam.Player)
                continue;

            if (ship.Policy is TDPolicy tdPolicy)
            {
                tdPolicy.Alpha = value;
            }
        }
    }

    private void SpawnMotherships()
    {
        GameObject playerObj = Instantiate(
            playerMothershipPrefab,
            battleEnvironment.GridToWorld(
                battleEnvironment.playerMothershipPosition
            ),
            Quaternion.identity
        );

        GameObject enemyObj = Instantiate(
            enemyMothershipPrefab,
            battleEnvironment.GridToWorld(
                battleEnvironment.enemyMothershipPosition
            ),
            Quaternion.identity
        );

        playerMothership = playerObj.transform;
        enemyMothership = enemyObj.transform;
        InitializeBattleCamera();
    }

    public void StartEpisode()
    {
        phase = BattlePhase.ActiveBattle;

        //battleEnvironment.SetAttackGoal();

        startButton.interactable = false;
        endBattleButton.interactable = true;
        deployFighterButton.interactable = true;

        battleEnvironment.SetEnemyAggression(enemyStartingAggression);

        Debug.Log("Episode started: goal is enemy mothership");
        //Debug.Log($"CurrentPlayerGoal: {battleEnvironment.CurrentPlayerGoal}");
        //Debug.Log($"CurrentEnemyGoal: {battleEnvironment.CurrentEnemyGoal}");
    }

    public void EndEpisode()
    {
        phase = BattlePhase.Recall;

        //battleEnvironment.SetReturnHomeGoal();

        deployFighterButton.interactable = false;
        endBattleButton.interactable = false;

        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            if (ship.status == ShipStatus.Active)
            {
                ship.directive = ShipDirective.ReturnHome;
                ship.status = ShipStatus.ReturningHome;
            }
        }

        Debug.Log("Recall ordered: goal is player mothership");
    }

    public void DeployFighter()
    {
        if (phase != BattlePhase.ActiveBattle)
            return;

        SpaceshipAgent ship = battleEnvironment.GetDockedFighter();

        if (ship == null)
        {
            Debug.Log("No docked fighters available");
            deployFighterButton.interactable = false;
            return;
        }

        TDPolicy fighterPolicy = new TDPolicy(
            alpha: adaptiveSlider.value,
            gamma: strategicSlider.value,
            epsilon: exploreExploitSlider.value
        );

        ship.gameObject.SetActive(true);

        ship.role = ShipRole.Fighter;
        ship.status = ShipStatus.Active;
        ship.directive = ShipDirective.Attack;

        ship.Initialize(
            battleEnvironment,
            fighterPolicy,
            battleEnvironment.playerMothershipPosition
        );

        Debug.Log("Docked fighter deployed");
    }

    private void UpdateAggression(float value)
    {
        battleEnvironment.SetPlayerAggression(value); 
    }

    private void DeployEnemyFighter()
    {
        SpaceshipAgent ship =
            battleEnvironment.GetDockedEnemyFighter();

        if (ship == null)
            return;

        TDPolicy enemyPolicy = new TDPolicy(
            alpha: 0.2f,
            gamma: 0.9f,
            epsilon: 0.1f
        );

        ship.gameObject.SetActive(true);

        ship.status = ShipStatus.Active;
        ship.directive = ShipDirective.Attack;

        ship.Initialize(
            battleEnvironment,
            enemyPolicy,
            battleEnvironment.enemyMothershipPosition
        );

        Debug.Log("Enemy fighter deployed");
    }
}
