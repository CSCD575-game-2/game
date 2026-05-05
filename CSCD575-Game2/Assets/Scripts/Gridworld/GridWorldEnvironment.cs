using System.Collections.Generic;
using UnityEngine;

public class GridWorldEnvironment
{
    public int sizeX, sizeY, sizeZ;

    public GridPosition start;
    public GridPosition goal;

    private int scale = 1;

    private HashSet<GridPosition> traps;
    private HashSet<GridPosition> hazards;

    public Dictionary<string, GridPosition> actions;

    public GridWorldEnvironment(
        int x, int y, int z,
        GridPosition start,
        GridPosition goal,
        IEnumerable<GridPosition> traps,
        IEnumerable<GridPosition> hazards)
    {
        sizeX = x;
        sizeY = y;
        sizeZ = z;

        actions = new Dictionary<string, GridPosition>
        {
            { "U", new GridPosition(0, 1 * scale, 0) },
            { "D", new GridPosition(0, -1 * scale, 0) },
            { "L", new GridPosition(-1 * scale, 0, 0) },
            { "R", new GridPosition(1 * scale, 0, 0) },
            { "F", new GridPosition(0, 0, 1 * scale) },
            { "B", new GridPosition(0, 0, -1 * scale) },
        };

        this.start = start;
        this.goal = goal;

        this.traps = new HashSet<GridPosition>(traps);
        this.hazards = new HashSet<GridPosition>(hazards);
    }

    public bool IsTerminal(GridPosition s)
    {
        return s == goal || traps.Contains(s);
    }

    public bool IsHazard(GridPosition s)
    {
        return hazards.Contains(s);
    }

    public bool IsTrap(GridPosition s)
    {
        return traps.Contains(s);
    }

    public int GetReward(GridPosition s)
    {
        if (s == goal) return 20;
        if (traps.Contains(s)) return -20;
        if (hazards.Contains(s)) return -10;
        return -1;
    }

    public (GridPosition next, int reward) Step(GridPosition s, string action)
    {
        if (IsTerminal(s)) return (s, 0);

        var d = actions[action];

        int nx = s.x + d.x;
        int ny = s.y + d.y;
        int nz = s.z + d.z;

        GridPosition next;

        if (nx < 0 || nx >= sizeX ||
            ny < 0 || ny >= sizeY ||
            nz < 0 || nz >= sizeZ)
        {
            next = s; // wall
        }
        else
        {
            next = new GridPosition(nx, ny, nz);
        }

        return (next, GetReward(next));
    }
}
