using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private BattleEnvironment battleEnvironment;
    [SerializeField] private Material[] levelSkyboxes;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button playerMothershipFireButton;
    [SerializeField] private Button deployFighterButton;
    [SerializeField] private Button deployResupplyButton;
    [SerializeField] private Button togglePriorityButton;
    [SerializeField] private TextMeshProUGUI togglePriorityButtonText;
    [SerializeField] private float togglePriorityCooldown = 2f;
    [SerializeField] private Image togglePriorityCooldownFill;

    [SerializeField] private Button allInButton;
    [SerializeField] private Button balancedButton;
    [SerializeField] private Button cautiousButton;
    [SerializeField] private Button adaptiveButton;
    [SerializeField] private TextMeshProUGUI allInStatusText;
    [SerializeField] private TextMeshProUGUI balancedStatusText;
    [SerializeField] private TextMeshProUGUI cautiousStatusText;
    [SerializeField] private TextMeshProUGUI adaptiveStatusText;
    private Color originalTextColor;
    public Color selectedTextColor = Color.black;

    [SerializeField] private TextMeshProUGUI fleetStatusText;
    [SerializeField] private TextMeshProUGUI enemyFleetStatusText;
    [SerializeField] private TextMeshProUGUI gameOverStatusText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image deployFighterCooldownFill;
    [SerializeField] private Image deployResupplyCooldownFill;

    [Header("Prefabs")]
    [SerializeField] private SpaceshipAgent fighterPrefab;
    [SerializeField] private SpaceshipAgent enemyFighterPrefab;
    [SerializeField] private SpaceshipAgent resupplyPrefab;
    [SerializeField] private GameObject playerMothershipPrefab;
    [SerializeField] private GameObject enemyMothershipPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private int startingFighters = 5;
    [SerializeField] private int startingResupplyShips = 2;
    [SerializeField] private int startingEnemyFightersBase = 5;
    [SerializeField] private int startingEnemyFighters = 5;
    [SerializeField] private Camera battleCamera;
    [SerializeField] private float enemyDeployInterval = 5f;
    [SerializeField] private float deployFighterCooldown = 5f;
    [SerializeField] private float deployResupplyCooldown = 5f;
    [SerializeField] private float enemyStartingAggression = 0.1f;
    [SerializeField] private float enemyAggressionIncreaseRate = 0.02f;

    [SerializeField] private Mothership playerMothership;
    [SerializeField] private Mothership enemyMothership;

    [Header("Commander Dials")]
    [SerializeField] private Slider exploreExploitSlider;
    [SerializeField] private Slider aggressionSlider;
    [SerializeField] private Slider strategicSlider;
    [SerializeField] private Slider adaptiveSlider;

    [Header("Level Progression")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int enemyFightersPerLevel = 3;


    private bool deployResupplyOnCooldown;
    private bool deployFighterOnCooldown;
    private bool togglePriorityOnCooldown;
    private float enemyDeployTimer;
    private BattlePhase phase = BattlePhase.Deployment;
    private float gameOverDelaySeconds = 5f;


    private void Start()
    {
        phase = BattlePhase.Finished;
        InitializeBattleCamera();

        togglePriorityButton.onClick.AddListener(OnTogglePriorityButton);
        deployFighterButton.onClick.AddListener(DeployFighter);
        deployResupplyButton.onClick.AddListener(DeployResupply);

        aggressionSlider.onValueChanged.AddListener(UpdateAggression);
        exploreExploitSlider.onValueChanged.AddListener(UpdateFleetEpsilon);
        strategicSlider.onValueChanged.AddListener(UpdateFleetGamma);
        adaptiveSlider.onValueChanged.AddListener(UpdateFleetAlpha);


        allInButton.onClick.AddListener(SetAllInDoctrine);
        balancedButton.onClick.AddListener(SetBalancedDoctrine);
        cautiousButton.onClick.AddListener(SetCautiousDoctrine);
        adaptiveButton.onClick.AddListener(SetAdaptiveDoctrine);
        playerMothershipFireButton.onClick.AddListener(OnMothershipFireButton);
        startButton.onClick.AddListener(BeginLevel);

        originalTextColor = allInStatusText.color;
        Debug.Log($"Original text color: {originalTextColor}");

       

    }

    private void ResetPresetTextColors()
    {
        allInStatusText.color = originalTextColor;
        balancedStatusText.color = originalTextColor;
        cautiousStatusText.color = originalTextColor;
        adaptiveStatusText.color = originalTextColor;
    }

    public void OnMothershipFireButton()
    {
        if (phase != BattlePhase.ActiveBattle)
            return;

        battleEnvironment.PlayerMothershipFire();
    }

    private void SetDoctrine(
            float aggression,
            float epsilon,
            float gamma,
            float alpha)
    {
        aggressionSlider.value = aggression;
        exploreExploitSlider.value = epsilon;
        strategicSlider.value = gamma;
        adaptiveSlider.value = alpha;

    }
    public void SetAllInDoctrine()
    {
        ResetPresetTextColors();
        Debug.Log($"All-In doctrine selected. Setting text color to: {allInStatusText.color}");
        SetDoctrine(
                aggression: 1.0f,
                epsilon: 0.05f,
                gamma: 0.95f,
                alpha: 0.05f);
        allInStatusText.color = selectedTextColor;
    }
    public void SetBalancedDoctrine()
    {
        ResetPresetTextColors();
        SetDoctrine(
            aggression: 0.5f,
            epsilon: 0.15f,
            gamma: 0.80f,
            alpha: 0.20f);
        balancedStatusText.color = selectedTextColor;
    }
    public void SetCautiousDoctrine()
    {
        ResetPresetTextColors();
        SetDoctrine(
                aggression: 0.1f,
                epsilon: 0.10f,
                gamma: 0.90f,
                alpha: 0.10f);
        cautiousStatusText.color = selectedTextColor;
    }
    public void SetAdaptiveDoctrine()
    {
        ResetPresetTextColors();
        SetDoctrine(
                aggression: 0.7f,
                epsilon: 0.40f,
                gamma: 0.70f,
                alpha: 0.40f);
        adaptiveStatusText.color = selectedTextColor;
    }

    private void UpdateFleetStatusText()
    {
        if (phase == BattlePhase.Finished)
            return;

        int fighterDocked = 0;
        int fighterDestroyed = 0;
        int fighterDisabled = 0;
        int fighterActive = 0;

        int resupplyDocked = 0;
        int resupplyDestroyed = 0;
        int resupplyActive = 0;

        int enemyFightersDocked = 0;
        int enemyFightersDestroyed = 0;
        int enemyFightersDisabled = 0;
        int enemyFightersActive = 0;

        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            if (ship.team != ShipTeam.Player)
                continue;
            if (ship.role == ShipRole.Fighter)
            {
                if (ship.status == ShipStatus.Docked)
                    fighterDocked++;
                else if (ship.status == ShipStatus.Destroyed)
                    fighterDestroyed++;
                else if (ship.status == ShipStatus.Disabled)
                    fighterDisabled++;
                else
                    fighterActive++;
            }
            else if (ship.role == ShipRole.Resupply)
            {
                if (ship.status == ShipStatus.Docked)
                    resupplyDocked++;
                else if (ship.status == ShipStatus.Destroyed)
                    resupplyDestroyed++;
                else
                    resupplyActive++;
            }
        }
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            if (ship.team != ShipTeam.Enemy)
                continue;
            if (ship.role == ShipRole.Fighter)
            {
                if (ship.status == ShipStatus.Docked)
                    enemyFightersDocked++;
                else if (ship.status == ShipStatus.Destroyed)
                    enemyFightersDestroyed++;
                else if (ship.status == ShipStatus.Disabled)
                    enemyFightersDisabled++;
                else
                    enemyFightersActive++;
            }
        }
        if (fighterActive == 0 && fighterDocked == 0
            && resupplyActive == 0 && resupplyDocked == 0)
        {
            GameOver(false);
        }
        if (enemyFightersActive == 0 && enemyFightersDocked == 0)
        {
            GameOver(true);
        }

        if (resupplyActive == 0 && resupplyDocked == 0)
        {
            battleEnvironment.TryAppendBattleStatus("All resupply ships are lost!");
        }

        fleetStatusText.text =
            $"PLAYER FIGHTERS\n\n{fighterDocked}  DOCKED\n{fighterActive}  ACTIVE" +
            $"\n{fighterDisabled}  DISABLED" +
            $"\n{fighterDestroyed}  DESTROYED\n\n" +
            $"RESUPPLY\n\n{resupplyActive}  ACTIVE\n{resupplyDocked}  DOCKED" + 
            $"\n{resupplyDestroyed}  DESTROYED";

        enemyFleetStatusText.text =
            $"ENEMY FIGHTERS\n\nDOCKED  {enemyFightersDocked}\nACTIVE  {enemyFightersActive}" + 
            $"\nDISABLED  {enemyFightersDisabled}" +
            $"\nDESTROYED  {enemyFightersDestroyed}";
    }

    private void UpdateLevelDisplay()
    {
        levelText.text = $"LEVEL {currentLevel}";
    }

    private void ApplySkyboxForLevel()
    {
        if (levelSkyboxes == null || levelSkyboxes.Length == 0)
            return;

        int index = (currentLevel - 1) % levelSkyboxes.Length;

        RenderSettings.skybox = levelSkyboxes[index];
    }
    private IEnumerator TogglePriorityCooldown()
    {
        togglePriorityOnCooldown = true;
        togglePriorityButton.interactable = false;

        float timer = 0f;

        while (timer < togglePriorityCooldown)
        {
            timer += Time.deltaTime;

            float progress = timer / togglePriorityCooldown;
            togglePriorityCooldownFill.fillAmount = 1f - progress;

            yield return null;
        }

        togglePriorityCooldownFill.fillAmount = 0f;
        togglePriorityButton.interactable = true;
        togglePriorityOnCooldown = false;
    }

    private void OnTogglePriorityButton()
    {
        if (phase != BattlePhase.ActiveBattle)
            return;

        if (togglePriorityOnCooldown)
            return;

        battleEnvironment.TogglePlayerPriority();

        if (battleEnvironment.playerFighterPriority)
        {
            battleEnvironment.AppendBattleStatus("This is Commander Adama. You must neutralize the enemy fighters.");
            togglePriorityButtonText.text = "PRIORITY: FIGHTERS";

        }
        else
        {
            battleEnvironment.AppendBattleStatus("This is Commander Adama. KILL THE ENEMY MOTHERSHIP AT ALL COSTS!");
            togglePriorityButtonText.text = "PRIORITY: MOTHERSHIP";
        }
        StartCoroutine(TogglePriorityCooldown());

    }

    private void BeginLevel() {


        UpdateLevelDisplay();

        ApplySkyboxForLevel();

        CreateFleets();

        deployFighterButton.interactable = false;
        deployResupplyButton.interactable = false;


        battleEnvironment.SetPlayerAggression(aggressionSlider.value);

        gameOverStatusText.text = "";
        phase = BattlePhase.ActiveBattle;
        startButton.gameObject.SetActive(false);
        deployFighterButton.interactable = true;
        deployResupplyButton.interactable = true;
        battleEnvironment.SetEnemyAggression(enemyStartingAggression);


    }


    private void Update()
    {
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
        
        GridPosition pos = battleEnvironment.playerMothershipPosition;
        GridPosition[] targets =
        {
            new GridPosition(pos.x - 1, pos.y, pos.z),
            new GridPosition(pos.x + 1, pos.y, pos.z),
            new GridPosition(pos.x, pos.y, pos.z + 1),
            new GridPosition(pos.x, pos.y, pos.z - 1),
        };
        bool hasValidTarget = false;
        foreach (GridPosition targetPos in targets)
        {
            SpaceshipAgent target = battleEnvironment.GetShipAtPosition(targetPos, ShipTeam.Player);
            if (target != null) {
                playerMothershipFireButton.interactable = true;
                hasValidTarget = true;
                break;
            }
        }
        if (!hasValidTarget)
        {
            playerMothershipFireButton.interactable = false;
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

        UpdateFleetStatusText();
    }
    private int GetEnemyFighterCount()
    {
        return startingEnemyFighters + ((currentLevel - 1) * enemyFightersPerLevel);
    }

    private void GameOver(bool playerWon)
    {

        if (phase == BattlePhase.Finished)
            return;
        phase = BattlePhase.Finished;

        enemyDeployTimer = 0f;

        startButton.gameObject.SetActive(true);
        deployFighterButton.interactable = false;
        deployResupplyButton.interactable = false;

        Debug.Log(playerWon ? "Victory!" : "Defeat!");

        if (playerWon)
        {
            gameOverStatusText.text = "VICTORY";
            currentLevel++;
            enemyDeployInterval = Mathf.Max(0.5f, enemyDeployInterval - 0.5f); // Increase difficulty by deploying enemies more frequently
        }
        else
        {
            gameOverStatusText.text = "GAME OVER";
            currentLevel = 1;
        }

        FinishEpisode();
        
    }


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
        int numDockedFighters = battleEnvironment.GetDockedFighterCount(ShipTeam.Player);
        int numDockedResupply = battleEnvironment.GetDockedResupplyShipCount();
        for (int i = numDockedFighters; i < startingFighters; i++)
        {
            SpaceshipAgent ship = Instantiate(
                fighterPrefab,
                battleEnvironment.GridToWorld(battleEnvironment.playerMothershipPosition),
                Quaternion.identity
            );

            TDPolicy fighterPolicy = new TDPolicy(
                alpha: adaptiveSlider.value,
                gamma: strategicSlider.value,
                epsilon: exploreExploitSlider.value
            );
            ship.fighterPolicy = fighterPolicy;

            ship.role = ShipRole.Fighter;
            ship.status = ShipStatus.Docked;
            ship.team = ShipTeam.Player;
            ship.directive = ShipDirective.ReturnHome;

            ship.gameObject.SetActive(false);

            battleEnvironment.RegisterShip(ship);
        }

        for (int i = numDockedResupply; i < startingResupplyShips; i++)
        {
            SpaceshipAgent ship = Instantiate(
                resupplyPrefab,
                battleEnvironment.GridToWorld(battleEnvironment.playerMothershipPosition),
                Quaternion.identity
            );

            ResupplyTDPolicy resupplyPolicy = new ResupplyTDPolicy(
                alpha: adaptiveSlider.value,
                gamma: strategicSlider.value,
                epsilon: exploreExploitSlider.value
            );
            ship.resupplyPolicy = resupplyPolicy;


            ship.role = ShipRole.Resupply;
            ship.status = ShipStatus.Docked;
            ship.team = ShipTeam.Player;
            ship.directive = ShipDirective.ReturnHome;

            ship.gameObject.SetActive(false);

            battleEnvironment.RegisterShip(ship);
        }
    }



    private void CreateFleets() {
        // first delete any existing enemy fighters from previous levels or all if player lost
        
        if (currentLevel == 1) {
            battleEnvironment.ResetEnvironmentLevel1();
            startingEnemyFighters = startingEnemyFightersBase;
        } else {
            battleEnvironment.ResetEnvironment();
        }

        CreateStartingFleet();
        CreateEnemyFleet();
        SpawnMotherships();
    }

    private void CreateEnemyFleet()
    {
        int numDockedEnemyFighters = battleEnvironment.GetDockedFighterCount(ShipTeam.Enemy);
        for (int i = numDockedEnemyFighters; i < GetEnemyFighterCount(); i++)
        {
            SpaceshipAgent ship = Instantiate(
                enemyFighterPrefab,
                battleEnvironment.GridToWorld(
                    battleEnvironment.enemyMothershipPosition
                ),
                Quaternion.identity
            );

            TDPolicy enemyPolicy = new TDPolicy(
                    alpha: 0.2f,
                    gamma: 0.9f,
                    epsilon: 0.1f
                    );
            ship.fighterPolicy = enemyPolicy;


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

        deployFighterButton.interactable = false;
        deployResupplyButton.interactable = false;
        
        
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            ship.StopAllActions();
        }

        Debug.Log("Episode finished. Buttons reset.");

    }


    private void UpdateFleetEpsilon(float value)
    {
        ResetPresetTextColors();
        foreach (SpaceshipAgent ship in battleEnvironment.allShips)
        {
            // Only update player ships
            if (ship.team != ShipTeam.Player)
                continue;

            if (ship.FighterPolicy is TDPolicy tdPolicy)
            {
                tdPolicy.Epsilon = value;
            }
            if (ship.ResupplyPolicy is ResupplyTDPolicy resupplyPolicy)
            {
                resupplyPolicy.Epsilon = value;
            }
        }
    }

    private void UpdateFleetGamma(float value)
    {
        ResetPresetTextColors();
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
        ResetPresetTextColors();
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
        playerMothership.SetVisible(true);
        enemyMothership.ResetHealth();
        enemyMothership.SetVisible(true);

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


    }

    public void StartEpisode()
    {
        gameOverStatusText.text = "";
        phase = BattlePhase.ActiveBattle;
        startButton.gameObject.SetActive(false);
        deployFighterButton.interactable = true;
        deployResupplyButton.interactable = true;
        battleEnvironment.SetEnemyAggression(enemyStartingAggression);

    }


    public void DeployFighter()
    {

        if (deployFighterOnCooldown)
            return;

        if (phase != BattlePhase.ActiveBattle)
            return;

        SpaceshipAgent ship = battleEnvironment.GetDockedFighter();

        if (ship == null)
        {
            battleEnvironment.TryAppendBattleStatus("No docked fighters available");
            deployFighterButton.interactable = false;
            return;
        }


        TDPolicy fighterPolicy = ship.FighterPolicy;

        ship.gameObject.SetActive(true);

        ship.role = ShipRole.Fighter;
        ship.status = ShipStatus.Active;
        ship.directive = ShipDirective.Attack;
        
        GridPosition initialGridPosition = battleEnvironment.playerMothershipPosition;
        initialGridPosition.x += 0; // next to the mothership

        ship.InitializeFighter(
            battleEnvironment,
            initialGridPosition,
            fighterPolicy
        );

        battleEnvironment.TryAppendBattleStatus($"{ship.Callsign} deployed.");
        Debug.Log("Docked fighter deployed");


        StartCoroutine(DeployFighterCooldown());
    }

    private IEnumerator DeployFighterCooldown()
    {
        deployFighterOnCooldown = true;
        deployFighterButton.interactable = false;

        float timer = 0f;

        while (timer < deployFighterCooldown)
        {
            timer += Time.deltaTime;

            float progress = timer / deployFighterCooldown;

            deployFighterCooldownFill.fillAmount = 1f - progress;

            yield return null;
        }

        deployFighterCooldownFill.fillAmount = 0f;
        deployFighterButton.interactable = true;
        deployFighterOnCooldown = false;
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

        ResupplyTDPolicy resupplyPolicy = ship.ResupplyPolicy;


        ship.gameObject.SetActive(true);

        ship.role = ShipRole.Resupply;
        ship.status = ShipStatus.Active;
        ship.directive = ShipDirective.Resupply;

        GridPosition initialGridPosition = battleEnvironment.playerMothershipPosition;
        initialGridPosition.x += 0; // next to the mothership


        ship.InitializeResupply(
            battleEnvironment,
            initialGridPosition,
            resupplyPolicy
        );

        Debug.Log("Docked resupply ship deployed");
        StartCoroutine(DeployResupplyCooldown());
    }
    private IEnumerator DeployResupplyCooldown()
    {
        deployResupplyOnCooldown = true;
        deployResupplyButton.interactable = false;

        float timer = 0f;

        while (timer < deployResupplyCooldown)
        {
            timer += Time.deltaTime;

            float progress = timer / deployResupplyCooldown;

            deployResupplyCooldownFill.fillAmount = 1f - progress;

            yield return null;
        }

        deployResupplyCooldownFill.fillAmount = 0f;
        deployResupplyButton.interactable = true;
        deployResupplyOnCooldown = false;
    }


    private void UpdateAggression(float value)
    {
        battleEnvironment.SetPlayerAggression(value); 
        ResetPresetTextColors();
    }

    private void DeployEnemyFighter()
    {
        SpaceshipAgent ship =
            battleEnvironment.GetDockedEnemyFighter();

        if (ship == null)
            return;

        TDPolicy enemyPolicy = ship.FighterPolicy;

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
