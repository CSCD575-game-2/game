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

    [Header("Spawn Settings")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private int startingFighters = 5;

    [SerializeField] private GameObject playerMothershipPrefab;
    [SerializeField] private GameObject enemyMothershipPrefab;

    [Header("Commander Dials")]
    [SerializeField] private Slider exploreExploitSlider;

    private BattlePhase phase = BattlePhase.Deployment;

    private void Start()
    {
        CreateStartingFleet();

        deployFighterButton.interactable = false;
        endBattleButton.interactable = false;

        exploreExploitSlider.onValueChanged.AddListener(UpdateFleetEpsilon);

        startButton.onClick.AddListener(StartEpisode);
        endBattleButton.onClick.AddListener(EndEpisode);
        deployFighterButton.onClick.AddListener(DeployFighter);

        SpawnMotherships();
    }

    private void Update()
    {
        if (phase == BattlePhase.Recall && AllShipsTerminal())
        {
            FinishEpisode();
        }
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
            ship.directive = ShipDirective.ReturnHome;

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
            if (ship.Policy is TDPolicy tdPolicy)
            {
                tdPolicy.Epsilon = value;
            }
        }
    }

    private void SpawnMotherships()
    {
        Instantiate(
            playerMothershipPrefab,
            battleEnvironment.GridToWorld(
                battleEnvironment.playerMothershipPosition
            ),
            Quaternion.identity
        );

        Instantiate(
            enemyMothershipPrefab,
            battleEnvironment.GridToWorld(
                battleEnvironment.enemyMothershipPosition
            ),
            Quaternion.identity
        );
    }

    public void StartEpisode()
    {
        phase = BattlePhase.ActiveBattle;

        battleEnvironment.SetAttackGoal();

        startButton.interactable = false;
        endBattleButton.interactable = true;
        deployFighterButton.interactable = true;

        Debug.Log("Episode started: goal is enemy mothership");
        Debug.Log($"CurrentGoal: {battleEnvironment.CurrentGoal}");
    }

    public void EndEpisode()
    {
        phase = BattlePhase.Recall;

        battleEnvironment.SetReturnHomeGoal();

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
            alpha: 0.2f,
            gamma: 0.9f,
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
}
