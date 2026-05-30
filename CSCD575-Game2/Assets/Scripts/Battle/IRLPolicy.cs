
public interface IRLPolicy<T>
{
    string ChooseAction(SpaceshipAgent ship, BattleEnvironment env);

    void Learn(
        T state,
        string action,
        float reward,
        T nextState
    );
}
