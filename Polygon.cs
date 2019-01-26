using System;
using System.Data;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text;
using System.Collections.Generic;
using System.Linq;

[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native,
    IsByteOrdered = true,
    ValidationMethodName = "ValidatePoint")]
public class Polygon : INullable
{
    /*********************************************
     * Private variables
     *********************************************/

    public bool isNull;

    //The number of lines of the shape
    private SqlInt32 lines;

    //Array of points
    private List<Point2D> points;

    private Point2D center;

    /*********************************************
     * Public accessors
     *********************************************/

    public List<Point2D> Points
    {
        get => points;
    }
    
    public SqlInt32 Lines
    {
        get => lines;
        set => lines = value;
    }
    public bool IsNull
    {
        get => isNull;
    }

    public static Polygon Null
    {
        get
        {
            Polygon poly = new Polygon();
            poly.isNull = true;
            return poly;
        }
    }

    public Point2D Center { get => center; set => center = value; }

    /*********************************************
     * Public functions
     *********************************************/

    public Polygon()
    {

    }

    public Polygon(List<Point2D> pts)
    {
        isNull = false;
        points = new List<Point2D>(pts);
        lines = points.Count;
        center = GetCenter();
    }

    //default constructor
    public Polygon(SqlString coordinates)
    {
        points = new List<Point2D>();

        List<Char> str = coordinates.Value.ToList();
        Predicate<Char> predicate = new Predicate<char>( (char x) => { return x == '(' || x == ')'; });
        str.RemoveAll(predicate);
        SqlString coords = new String(str.ToArray());
        string[] strcoords = coords.ToString().Split(',');
        lines = strcoords.Length;
        foreach(string coord in strcoords)
        {
            points.Add(new Point2D(coord));
        }
        center = GetCenter();
    }

    [SqlMethod(OnNullCall = false)]
    public static Polygon Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        Polygon pt = new Polygon(s);
        
        return pt;
    }

    //field
    [SqlMethod(OnNullCall = false)]
    public SqlDouble GetArea()
    {
        // Initialze area 
        SqlDouble area = 0.0;

        // Calculate value of shoelace formula 
        int j = lines.Value - 1;

        for (int i = 0; i < lines; i++)
        {
            area += (points[j].X + points[i].X) * (points[j].Y - points[i].Y);
        }
        return area;
    }

    //Circuit
    [SqlMethod(OnNullCall = false)]
    public SqlDouble GetCircuit()
    {
        SqlDouble circuit = 0.0;
        for (int i = 1; i < lines; i++)
        {
            circuit += points[i - 1].DistanceFrom(points[i]);
        }
        circuit += points.Last().DistanceFrom(points.First());
        return circuit;
    }

    //Circuit
    [SqlMethod(OnNullCall = false)]
    public Point2D GetCenter()
    {
        SqlInt32 x=0, y=0;
        SqlDouble area = GetArea();
        /*for(int i = 0; i < lines.Value-1; i++)
        {
            x += ((points[i].X + points[i + 1].X) * (points[i].X*points[i+1].Y - points[i+1].X * points[i].Y)).Value;
            y += ((points[i].Y + points[i + 1].Y) * (points[i].X * points[i + 1].Y - points[i + 1].X * points[i].Y)).Value;
        }*/
        foreach (Point2D p in points)
        {
            x += p.X;
            y += p.Y;
        }
        x /= points.Count;
        y /= points.Count;

        return new Point2D(x, y);
    }


    [SqlMethod(OnNullCall = false)]
    public Polygon OR(Polygon p2)
    {
        List<Point2D> points = new List<Point2D>();

        foreach (Point2D p in p2.Points)
        {
            bool res = IntersectPoly(p);
            if (!res)
            {
                points.Add(new Point2D(p));
            }
        }

        foreach (Point2D p in this.points)
        {
            bool res = p2.IntersectPoly(p);
            if (!res)
            {
                points.Add(new Point2D(p));
            }
        }

        List<Point2D> intersections = ReturnIntersection(p2);

        points.AddRange(intersections);

        Polygon result = new Polygon(points.Distinct(new Point2DEqualityComparer() ).ToList());

        result.Sort();

        return result;
    }

    [SqlMethod(OnNullCall = false)]
    public Polygon AND(Polygon p2)
    {
        List<Point2D> points = new List<Point2D>();

        foreach (Point2D p in p2.Points)
        {
            bool res = IntersectPoly(p);
            if (res)
            {
                points.Add(new Point2D(p));
            }
        }

        foreach (Point2D p in this.points)
        {
            bool res = p2.IntersectPoly(p);
            if (res)
            {
                points.Add(new Point2D(p));
            }
        }

        List<Point2D> intersections = ReturnIntersection(p2);

        points.AddRange(intersections);

        Polygon result = new Polygon(points.Distinct(new Point2DEqualityComparer()).ToList());

        result.Sort();

        return result;
    }

    [SqlMethod(OnNullCall = false)]
    public Polygon NOT(Polygon p2)
    {
        List<Point2D> points = new List<Point2D>();

        foreach(Point2D p in  p2.Points)
        {
            bool res = IntersectPoly(p);
            if(res)
            {
                points.Add(new Point2D(p));
            }
        }

        foreach (Point2D p in this.points)
        {
            bool res = p2.IntersectPoly(p);
            if (!res)
            {
                points.Add(new Point2D(p));
            }
        }

        List<Point2D> intersections = ReturnIntersection(p2);

        points.AddRange(intersections);

        Polygon result = new Polygon(points.Distinct(new Point2DEqualityComparer()).ToList());

        result.Sort();

        return result;
    }

    //not implemented
    [SqlMethod(OnNullCall = false)]
    public Polygon XOR(Polygon p2)
    {
        return Null;
    }

    /*********************************************
     * Private utility functions
     *********************************************/

    // check if point intersects a polygon
    public bool IntersectPoly(Point2D pq)
    {
        Point2D tp = new Point2D(pq.X, pq.Y);
        bool result = true;
        //get range to check
        int xmax=0, xmin=0;
        foreach(Point2D p in points)
        {
            if (p.X.Value > xmax)
                xmax = p.X.Value+1;
        }

        int intersections = 0;
        for (xmin=0; xmin<=xmax; xmin++)
        {
            if (points.Contains(tp, new Point2DEqualityComparer()))
                intersections++;
            else for(int i = 1; i<lines; i++)
            {
                if(tp.Intersect(points[i], points[i-1]))
                {
                    if(orientation(pq, points[i], points[i-1])==0)
                    {
                            return pq.Intersect(points[i], points[i - 1]);
                    }
                    intersections++;
                }

            }
            tp.X=tp.X+1;
        }
        if (intersections % 2 == 0)
            result = false;
        return result;
    }

    SqlInt32 orientation(Point2D p, Point2D q, Point2D r)
    {
        SqlInt32 val = (q.Y - p.Y) * (r.X - q.X) -
                  (q.X - p.X) * (r.Y - q.Y);

        if (val == 0) return 0;  // colinear 
        return (val > 0) ? 1 : 2; // clock or counterclock wise 
    }

    private List<Point2D> ReturnIntersection(Polygon second)
    {
        List<Point2D> result = new List<Point2D>();
        for(int i=1; i<lines; i++)
        {
            for(int j=1; j<second.Lines; j++)
            {
                Point2D inter = GetIntersectionPoint(points[i-1], points[i], second.Points[j-1], second.Points[j]);
                if(inter!=null)
                {
                    result.Add(inter);
                }
            }
        }
        return result;
    }

    private Point2D GetIntersectionPoint(Point2D l1p1, Point2D l1p2, Point2D l2p1, Point2D l2p2)
    {
        SqlInt32 A1 = l1p2.Y - l1p1.Y;
        SqlInt32 B1 = l1p1.X - l1p2.X;
        SqlInt32 C1 = A1 * l1p1.X + B1 * l1p1.Y;

        SqlInt32 A2 = l2p2.Y - l2p1.Y;
        SqlInt32 B2 = l2p1.X - l2p2.X;
        SqlInt32 C2 = A2 * l2p1.X + B2 * l2p1.Y;

        //lines are parallel
        SqlInt32 det = A1 * B2 - A2 * B1;
        if (det == 0)
        {
            return null; //parallel lines
        }
        else
        {
            SqlInt32 x = (B2 * C1 - B1 * C2) / det;
            SqlInt32 y = (A1 * C2 - A2 * C1) / det;
            SqlBoolean online1 = (Math.Min(l1p1.X.Value, l1p2.X.Value) < x || (Math.Min(l1p1.X.Value, l1p2.X.Value) == x))
                && (Math.Max(l1p1.X.Value, l1p2.X.Value) > x || (Math.Max(l1p1.X.Value, l1p2.X.Value)== x))
                && (Math.Min(l1p1.Y.Value, l1p2.Y.Value) < y || (Math.Min(l1p1.Y.Value, l1p2.Y.Value) == y))
                && (Math.Max(l1p1.Y.Value, l1p2.Y.Value) > y || (Math.Max(l1p1.Y.Value, l1p2.Y.Value) == y))
                ;
            SqlBoolean online2 = (Math.Min(l2p1.X.Value, l2p2.X.Value) < x || (Math.Min(l2p1.X.Value, l2p2.X.Value) == x))
                && (Math.Max(l2p1.X.Value, l2p2.X.Value) > x || (Math.Max(l2p1.X.Value, l2p2.X.Value) == x))
                && (Math.Min(l2p1.Y.Value, l2p2.Y.Value) < y || (Math.Min(l2p1.Y.Value, l2p2.Y.Value) == y))
                && (Math.Max(l2p1.Y.Value, l2p2.Y.Value) > y || (Math.Max(l2p1.Y.Value, l2p2.Y.Value) == y))
                ;

            if (online1 && online2)
                return new Point2D(x, y);
        }
        return null; //intersection is at out of at least one segment.
    }

    //sorts the polygon points to be clockwise
    private void Sort()
    {
        SqlInt32 mX = 0;
        SqlInt32 my = 0;
        foreach (Point2D p in points)
        {
            mX += p.X;
            my += p.Y;
        }
        mX /= points.Count;
        my /= points.Count;

        points.OrderBy(v => Math.Atan2(v.Y.Value - my.Value, v.X.Value - mX.Value));
    }


    /*********************************************
     * Override functions
     *********************************************/

    // Use StringBuilder to provide string representation of UDT.  
    public override string ToString()
    {
        string text = "(";
        foreach(Point2D point in points)
        {
            text += point.ToString();
            if(!point.Equals(points.Last()))
                text += ",";
        }
        return text+')';
    }

}

