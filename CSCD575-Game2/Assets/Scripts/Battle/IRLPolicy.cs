
public interface IRLPolicy
{
    string ChooseAction(SpaceshipAgent ship, BattleEnvironment env);

    void Learn(
        ShipState state,
        string action,
        float reward,
        ShipState nextState
    );
}
