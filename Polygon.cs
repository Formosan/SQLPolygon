using System;
using System.Data;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Text;
using System.Collections.Generic;

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

    /*********************************************
     * Public accessors
     *********************************************/
    public SqlInt32 Lines
    {
        get => lines;
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

    /*********************************************
     * Public functions
     *********************************************/

    public Polygon()
    {

    }

    //default constructor
    public Polygon(SqlString coordinates)
    {
        points = new List<Point2D>();
        string[] strcoords = coordinates.ToString().Split(';');
        lines = strcoords.Length;
        foreach(string coord in strcoords)
        {
            points.Add(new Point2D(coord));
        }
    }

    [SqlMethod(OnNullCall = false)]
    public static Polygon Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        Polygon pt = new Polygon();
        string[] coordinates = s.Value.Split(',');
        foreach (string coord in coordinates)
        {
            pt.points.Add(Point2D.Parse(coord));
        }
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
        return circuit;
    }

    //Circuit
    [SqlMethod(OnNullCall = false)]
    public Point2D GetCenter()
    {
        int x=0, y=0;
        SqlDouble area = GetArea();
        for(int i = 0; i < lines.Value-1; i++)
        {
            x += ((points[i].X + points[i + 1].X) * (points[i].X*points[i+1].Y - points[i+1].X * points[i].Y)).Value;
            y += ((points[i].Y + points[i + 1].Y) * (points[i].X * points[i + 1].Y - points[i + 1].X * points[i].Y)).Value;
        }
        
        return new Point2D(x, y);
    }


    [SqlMethod(OnNullCall = false)]
    public Polygon OR(Polygon p2)
    {
        return Null;
    }

    [SqlMethod(OnNullCall = false)]
    public Polygon AND(Polygon p2)
    {
        return Null;
    }

    [SqlMethod(OnNullCall = false)]
    public Polygon NOT(Polygon p2)
    {
        Polygon result = new Polygon();


        return result;
    }

    [SqlMethod(OnNullCall = false)]
    public Polygon XOR(Polygon p2)
    {
        return Null;
    }

    /*********************************************
     * Private utility functions
     *********************************************/

    // check if point intersects a polygon
    private bool InstersectPoly(Point2D pq)
    {
        Point2D tp = new Point2D(pq.X, pq.Y);
        bool result = false;
        //get range to check
        int xmax=0, xmin=0;
        foreach(Point2D p in points)
        {
            if (p.X.Value > xmax)
                xmax = p.X.Value;
        }

        int intersections = 0;
        for (xmin=0; xmin<=xmax; xmin++)
        {
            for(int i = 1; i<lines; i++)
            {
                if(tp.Intersect(points[i], points[i-1]))
                {
                    intersections++;
                    break;
                }

            }
            tp.X=tp.X+1;
        }
        if (intersections % 2 == 0)
            result = true;
        return result;
    }


    /*********************************************
     * Override functions
     *********************************************/

    // Use StringBuilder to provide string representation of UDT.  
    public override string ToString()
    {
        string text = "Polygon of "+lines+" lines, with points: ";
        foreach(Point2D points in points)
        {
            text += points.ToString();
        }
        return text+'\n';
    }

}

