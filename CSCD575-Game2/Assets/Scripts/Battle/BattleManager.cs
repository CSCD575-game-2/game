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

    private BattlePhase phase = BattlePhase.Deployment;

    private void Start()
    {
        deployFighterButton.interactable = false;

        startButton.onClick.AddListener(StartEpisode);
        deployFighterButton.onClick.AddListener(DeployFighter);
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

        IRLPolicy fighterPolicy = new TDPolicy(
            alpha: 0.2f,
            gamma: 0.9f,
            epsilon: 0.5f
        );

        GridPosition spawnState = new GridPosition(0, 0, 0);

        ship.Initialize(battleEnvironment, fighterPolicy, spawnState);

        Debug.Log("Fighter deployed and registered");
    }
}
