using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Point2DEqualityComparer : IEqualityComparer<Point2D>
{
    public bool Equals(Point2D b1, Point2D b2)
    {
        if (b2 == null && b1 == null)
            return true;
        else if (b1 == null || b2 == null)
            return false;
        else if (b1.X == b2.X && b1.Y == b2.Y)
            return true;
        else
            return false;
    }

    public int GetHashCode(Point2D obj)
    {
        int hCode = (obj.X ^ obj.Y).Value;
        return hCode.GetHashCode();
    }
}

public class PolygonEqualityComparer : IEqualityComparer<Polygon>
{
    public bool Equals(Polygon b1, Polygon b2)
    {
        if (b2 == null && b1 == null)
            return true;
        else if (b1 == null || b2 == null)
            return false;
        else if (b1.Points.Except(b2.Points, new Point2DEqualityComparer()).Count()==0)
            return true;
        else
            return false;
    }

    public int GetHashCode(Polygon obj)
    {
        int hCode = obj.GetArea().ToSqlInt32().Value^obj.GetCircuit().ToSqlInt32().Value;
        return hCode.GetHashCode();
    }
}