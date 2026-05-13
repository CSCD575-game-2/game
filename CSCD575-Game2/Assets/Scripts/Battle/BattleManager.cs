using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private BattleEnvironment battleEnvironment;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button deployFighterButton;

    [Header("Prefabs")]
    [SerializeField] private SpaceshipAgent fighterPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private float spacing = 1.2f;

    [SerializeField] private GameObject playerMothershipPrefab;
    [SerializeField] private GameObject enemyMothershipPrefab;

    [Header("Commander Dials")]
    [SerializeField] private Slider exploreExploitSlider;

    private BattlePhase phase = BattlePhase.Deployment;

    private void Start()
    {
        deployFighterButton.interactable = false;

        exploreExploitSlider.onValueChanged.AddListener(UpdateFleetEpsilon);

        startButton.onClick.AddListener(StartEpisode);
        deployFighterButton.onClick.AddListener(DeployFighter);

        SpawnMotherships();
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

        startButton.interactable = false;
        deployFighterButton.interactable = true;

        Debug.Log("Episode started");
    }

    public void DeployFighter()
    {
        if (phase != BattlePhase.ActiveBattle)
            return;


        SpaceshipAgent ship = Instantiate(
            fighterPrefab,
            playerSpawnPoint.position,
            playerSpawnPoint.rotation
        );

        ship.role = ShipRole.Fighter;
        ship.status = ShipStatus.Active;
        ship.directive = ShipDirective.Attack;

        battleEnvironment.RegisterShip(ship);

        float epsilon = exploreExploitSlider.value;

        IRLPolicy fighterPolicy = new TDPolicy(
            alpha: 0.2f,
            gamma: 0.9f,
            epsilon: epsilon
        );

        //IRLPolicy fighterPolicy = new TDPolicy(
            //alpha: 0.2f,
            //gamma: 0.9f,
            //epsilon: 0.5f
        //);

        //GridPosition spawnState = new GridPosition(0, 0, 0);
        GridPosition spawnState = battleEnvironment.playerMothershipPosition;

        ship.Initialize(battleEnvironment, fighterPolicy, spawnState);

        Debug.Log("Fighter deployed and registered");
    }
}
