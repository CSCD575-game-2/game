public struct ResupplyState
{
    public GridPosition position;

    public bool allyNeedsHelpLeft;
    public bool allyNeedsHelpRight;
    public bool allyNeedsHelpForward;
    public bool allyNeedsHelpBack;
    public bool allyNeedsHelpUp;
    public bool allyNeedsHelpDown;

    //public bool adjacentToAllyNeedsHelp;
    //public bool carryingSupplies;

    public ResupplyState(
        GridPosition position,
        bool allyNeedsHelpLeft,
        bool allyNeedsHelpRight,
        bool allyNeedsHelpForward,
        bool allyNeedsHelpBack,
        bool allyNeedsHelpUp,
        bool allyNeedsHelpDown
        )
    {
        this.position = position;
        this.allyNeedsHelpLeft = allyNeedsHelpLeft;
        this.allyNeedsHelpRight = allyNeedsHelpRight;
        this.allyNeedsHelpForward = allyNeedsHelpForward;
        this.allyNeedsHelpBack = allyNeedsHelpBack;
        this.allyNeedsHelpUp = allyNeedsHelpUp;
        this.allyNeedsHelpDown = allyNeedsHelpDown;
    }
}
