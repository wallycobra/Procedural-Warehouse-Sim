public class DropoffData
{
    public GridNode FrontCell { get; }
    public GridNode BackCell { get; }

    public DropoffData(GridNode frontCell, GridNode backCell)
    {
        FrontCell = frontCell;
        BackCell = backCell;
    }
}