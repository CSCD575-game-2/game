
public struct ShipState
{
    public GridPosition position;

    public bool enemyLeft;
    public bool enemyRight;
    public bool enemyForward;
    public bool enemyBack;

    public ShipState(
        GridPosition position,
        bool enemyLeft,
        bool enemyRight,
        bool enemyForward,
        bool enemyBack)
    {
        this.position = position;
        this.enemyLeft = enemyLeft;
        this.enemyRight = enemyRight;
        this.enemyForward = enemyForward;
        this.enemyBack = enemyBack;
    }
}
