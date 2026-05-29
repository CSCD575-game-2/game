using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private BattleEnvironment battleEnvironment;
    [SerializeField] private Material[] levelSkyboxes;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button deployFighterButton;
    [SerializeField] private Button deployResupplyButton;
    [SerializeField] private Button endBattleButton;

    [Header("Prefabs")]
    [SerializeField] private SpaceshipAgent fighterPrefab;
    [SerializeField] private SpaceshipAgent enemyFighterPrefab;

    [SerializeField] private SpaceshipAgent resupplyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private int startingFighters = 5;
    [SerializeField] private int startingResupplyShips = 2;
    [SerializeField] private int startingEnemyFighters = 5;
    [SerializeField] private Camera battleCamera;
    [SerializeField] private float enemyDeployInterval = 5f;
    //private Transform playerMothership;
    //private Transform enemyMothership;

    [SerializeField] private float enemyStartingAggression = 0.1f;
    [SerializeField] private float enemyAggressionIncreaseRate = 0.02f;

    [SerializeField] private GameObject playerMothershipPrefab;
    [SerializeField] private GameObject enemyMothershipPrefab;

    [SerializeField] private Mothership playerMothership;
    [SerializeField] private Mothership enemyMothership;

    [Header("Commander Dials")]
    [SerializeField] private Slider exploreExploitSlider;
    [SerializeField] private Slider aggressionSlider;
    [SerializeField] private Slider strategicSlider;
    [SerializeField] private Slider adaptiveSlider;

    [SerializeField] private int currentLevel = 1;
    //[SerializeField] private int baseEnemyFighters = 2;
    [SerializeField] private int enemyFightersPerLevel = 2;

    private float enemyDeployTimer;

    private BattlePhase phase = BattlePhase.Deployment;

    private void Start()
    {
        BeginLevel();
        InitializeBattleCamera();
    }

    private void ApplySkyboxForLevel()
    {
        if (levelSkyboxes == null || levelSkyboxes.Length == 0)
            return;

        int index = (currentLevel - 1) % levelSkyboxes.Length;

        RenderSettings.skybox = levelSkyboxes[index];
    }

    private void BeginLevel() {

        ApplySkyboxForLevel();

        CreateFleets();

        deployFighterButton.interactable = false;
        deployResupplyButton.interactable = false;
        endBattleButton.interactable = false;

        startButton.onClick.AddListener(StartEpisode);
        endBattleButton.onClick.AddListener(EndEpisode);
        deployFighterButton.onClick.AddListener(DeployFighter);
        deployResupplyButton.onClick.AddListener(DeployResupply);

        aggressionSlider.onValueChanged.AddListener(UpdateAggression);
        exploreExploitSlider.onValueChanged.AddListener(UpdateFleetEpsilon);
        strategicSlider.onValueChanged.AddListener(UpdateFleetGamma);
        adaptiveSlider.onValueChanged.AddListener(UpdateFleetAlpha);

        battleEnvironment.SetPlayerAggression(aggressionSlider.value);

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
        if (battleEnvironment.playerMothership != null &&
                battleEnvironment.playerMothership.IsDestroyed)
        {
            GameOver(false);
        }

        if (battleEnvironment.enemyMothership != null &&
                battleEnvironment.enemyMothership.IsDestroyed)
        {
            GameOver(true);
        }
    }
    private int GetEnemyFighterCount()
    {
        return startingEnemyFighters + ((currentLevel - 1) * enemyFightersPerLevel);
    }

    private void GameOver(bool playerWon)
    {
        phase = BattlePhase.Finished;

        startButton.interactable = true;
        deployFighterButton.interactable = false;
        deployResupplyButton.interactable = false;
        endBattleButton.interactable = false;

        Debug.Log(playerWon ? "Victory!" : "Defeat!");

        if (playerWon)
        {
            currentLevel++;
            BeginLevel();
        }
        else
        {
            currentLevel = 1;
            BeginLevel();
        }
        
    }

    //private void InitializeBattleCamera()
    //{
        //if (battleCamera == null ||
            //playerMothership == null ||
            //enemyMothership == null)
        //{
            //return;
        //}

        //Vector3 direction =
            //(enemyMothership.position - playerMothership.position).normalized;

        //Vector3 cameraPosition =
            //playerMothership.position
            //- direction * 120f
            //+ Vector3.up * 100f;

        //battleCamera.transform.position = cameraPosition;

        //Vector3 lookTarget =
            //Vector3.Lerp(
                //playerMothership.position,
                //enemyMothership.position,
                //0.5f
            //);

        //battleCamera.transform.LookAt(lookTarget);
    //}
    private void InitializeBattleCamera()
    {
        if (battleCamera == null || battleEnvironment == null)
            return;

        GameManager gm = FindObjectOfType<GameManager>();
        Vector3 center = gm.GetGridWorldCenter();

        Vector3 cameraPosition =
            center
            + new Vector3(0f, 0f, 0f);

        battleCamera.transform.position = cameraPosition;
        battleCamera.transform.LookAt(center);
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

        for (int i = 0; i < startingResupplyShips; i++)
        {
            SpaceshipAgent ship = Instantiate(
                resupplyPrefab,
                battleEnvironment.GridToWorld(battleEnvironment.playerMothershipPosition),
                Quaternion.identity
            );

            ship.role = ShipRole.Resupply;
            ship.status = ShipStatus.Docked;
            ship.team = ShipTeam.Player;
            ship.directive = ShipDirective.ReturnHome;

            ship.gameObject.SetActive(false);

            battleEnvironment.RegisterShip(ship);
        }
    }

    private void ClearFleets() {
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            Destroy(ship.gameObject);
        }
        battleEnvironment.allShips.Clear();


        //if (battleEnvironment.playerMothership != null)
        //{
            //Destroy(battleEnvironment.playerMothership.gameObject);
            //battleEnvironment.playerMothership = null;
        //}
        //if (battleEnvironment.enemyMothership != null)
        //{
            //Destroy(battleEnvironment.enemyMothership.gameObject);
            //battleEnvironment.enemyMothership = null;
        //}
    }

    private void CreateFleets() {
        // first delete any existing enemy fighters from previous levels
        ClearFleets();

        CreateStartingFleet();
        CreateEnemyFleet();
        SpawnMotherships();
    }

    private void CreateEnemyFleet()
    {
        for (int i = 0; i < GetEnemyFighterCount(); i++)
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
                ship.status != ShipStatus.Destroyed &&
                ship.status != ShipStatus.Disabled)
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
        deployResupplyButton.interactable = false;
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

            if (ship.FighterPolicy is TDPolicy tdPolicy)
            {
                tdPolicy.Epsilon = -value;
            }
            if (ship.ResupplyPolicy is ResupplyTDPolicy resupplyPolicy)
            {
                resupplyPolicy.Epsilon = -value;
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

            if (ship.FighterPolicy is TDPolicy tdPolicy)
            {
                tdPolicy.Gamma = value;
            }
            if (ship.ResupplyPolicy is ResupplyTDPolicy resupplyPolicy)
            {
                resupplyPolicy.Gamma = value;
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

            if (ship.FighterPolicy is TDPolicy tdPolicy)
            {
                tdPolicy.Alpha = value;
            }
            if (ship.ResupplyPolicy is ResupplyTDPolicy resupplyPolicy)
            {
                resupplyPolicy.Alpha = value;
            }
        }
    }

    private void SpawnMotherships()
    {

        playerMothership.team = ShipTeam.Player;
        enemyMothership.team = ShipTeam.Enemy;
        
        playerMothership.ResetHealth();
        enemyMothership.ResetHealth();

        playerMothership.status = ShipStatus.Active;
        enemyMothership.status = ShipStatus.Active;

        battleEnvironment.SetMotherships(
                playerMothership,
                enemyMothership
                );
        playerMothership.transform.position = battleEnvironment.GridToWorld(
                battleEnvironment.playerMothershipPosition
            );
        enemyMothership.transform.position = battleEnvironment.GridToWorld(
                battleEnvironment.enemyMothershipPosition
            );

        //GameObject playerObj = Instantiate(
            //playerMothershipPrefab,
            //battleEnvironment.GridToWorld(
                //battleEnvironment.playerMothershipPosition
            //),
            //Quaternion.identity
        //);

        //GameObject enemyObj = Instantiate(
            //enemyMothershipPrefab,
            //battleEnvironment.GridToWorld(
                //battleEnvironment.enemyMothershipPosition
            //),
            //Quaternion.identity
        //);

        //Mothership playerMs = playerObj.GetComponent<Mothership>();
        //playerMs.team = ShipTeam.Player;
        //battleEnvironment.RegisterMothership(playerMs);

        //Mothership enemyMs = enemyObj.GetComponent<Mothership>();
        //enemyMs.team = ShipTeam.Enemy;
        //battleEnvironment.RegisterMothership(enemyMs);

        //playerMothership = playerObj.transform;
        //enemyMothership = enemyObj.transform;
    }

    public void StartEpisode()
    {
        phase = BattlePhase.ActiveBattle;

        //battleEnvironment.SetAttackGoal();

        startButton.interactable = false;
        endBattleButton.interactable = true;
        deployFighterButton.interactable = true;
        deployResupplyButton.interactable = true;

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
        deployResupplyButton.interactable = false;
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

        ship.InitializeFighter(
            battleEnvironment,
            battleEnvironment.playerMothershipPosition,
            fighterPolicy
        );

        Debug.Log("Docked fighter deployed");
    }

    public void DeployResupply()
    {
        if (phase != BattlePhase.ActiveBattle)
            return;

        SpaceshipAgent ship = battleEnvironment.GetDockedResupplyShip();

        if (ship == null)
        {
            Debug.Log("No docked resupply ships available");
            deployResupplyButton.interactable = false;
            return;
        }

        ResupplyTDPolicy resupplyPolicy = new ResupplyTDPolicy(
            alpha: adaptiveSlider.value,
            gamma: strategicSlider.value,
            epsilon: exploreExploitSlider.value
        );

        ship.gameObject.SetActive(true);

        ship.role = ShipRole.Resupply;
        ship.status = ShipStatus.Active;
        ship.directive = ShipDirective.Resupply;

        ship.InitializeResupply(
            battleEnvironment,
            battleEnvironment.playerMothershipPosition,
            resupplyPolicy
        );

        Debug.Log("Docked resupply ship deployed");
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

        ship.InitializeFighter(
            battleEnvironment,
            battleEnvironment.enemyMothershipPosition,
            enemyPolicy
        );

        Debug.Log("Enemy fighter deployed");
    }
}
