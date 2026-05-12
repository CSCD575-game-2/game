
public interface IRLPolicy
{
    string ChooseAction(SpaceshipAgent ship, BattleEnvironment env);
    void Learn(GridPosition state, string action, float reward, GridPosition nextState);
}
