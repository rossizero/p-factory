using Map.Tile;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    public int X { get; private set; }
    public int Z { get; private set; }

    //"third" hex coordinate axis sum(X,Y,Z) = 0
    public int Y
    {
        get
        {
            return -X - Z;
        }
    }

    public HexCoordinates(int x, int z)
    {
        X = x;
        Z = z;
    }

    /// <summary>
    /// Converts a "Sqaure" HexCoordinate to a "Hex" HexCoordinate
    /// </summary>
    /// <param name="x">square x coordinate</param>
    /// <param name="z">square z coordinate</param>
    /// <returns>hex HexCoordinate</returns>
    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    /// <summary>
    /// Converts a "Hex" HexCoordinate to a "Square" HexCoordinate
    /// </summary>
    /// <param name="x">hex x coordinate</param>
    /// <param name="z">hex z coordinate</param>
    /// <returns>Sqaure HexCoordinate</returns>
    public static HexCoordinates ToOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x + z / 2, z);
    }

    /// <summary>
    /// Converts a "Hex" HexCoordinate to a "Square" HexCoordinate
    /// </summary>
    /// <param name="coordinates">hex coordinates</param>
    /// <returns>Sqaure HexCoordinate</returns>
    public static HexCoordinates ToOffsetCoordinates(HexCoordinates coordinates)
    {
        return new HexCoordinates(coordinates.X + coordinates.Z / 2, coordinates.Z);
    }

    /// <summary>
    /// Converts a scene position to "Hex" HexCoordinate
    /// </summary>
    /// <param name="position">position in scene</param>
    /// <returns>"Hex" HexCoordinate</returns>
    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;

        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }


    /// <summary>
    /// String representation of this Coordinate
    /// </summary>
    /// <returns></returns>
    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Z.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>HexCoordinate of neighbour in HexDirection W</returns>
    public HexCoordinates W()
    {
        return new HexCoordinates(X - 1, Z);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>HexCoordinate of neighbour in HexDirection NW</returns>
    public HexCoordinates NW()
    {
        return new HexCoordinates(X - 1, Z + 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>HexCoordinate of neighbour in HexDirection NE</returns>
    public HexCoordinates NE()
    {
        return new HexCoordinates(X, Z + 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>HexCoordinate of neighbour in HexDirection E</returns>
    public HexCoordinates E()
    {
        return new HexCoordinates(X + 1, Z);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>HexCoordinate of neighbour in HexDirection SE</returns>
    public HexCoordinates SE()
    {
        return new HexCoordinates(X + 1, Z - 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>HexCoordinate of neighbour in HexDirection SW</returns>
    public HexCoordinates SW()
    {
        return new HexCoordinates(X, Z - 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>HexCoordinate of neighbour in HexDirection direction</returns>
    public HexCoordinates GetNeighbour(HexDirection direction)
    {
        switch (direction)
        {
            case HexDirection.NW:
                return NW();
            case HexDirection.NE:
                return NE();
            case HexDirection.E:
                return E();
            case HexDirection.SE:
                return SE();
            case HexDirection.SW:
                return SW();
            default:
                return W();
        }
    }
}
