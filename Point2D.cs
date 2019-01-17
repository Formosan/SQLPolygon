using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Threading.Tasks;

[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native,
 IsByteOrdered = true, ValidationMethodName = "SprawdzPunkt")]
public class Point2D : INullable
{
    /********************************************
    * Private variables
    *********************************************/
    //Two coordinates of the point
    private bool is_Null;
    private SqlInt32 __x__;
    private SqlInt32 __y__;


    /*********************************************
     * Public accessors
     *********************************************/
    public SqlInt32 X
    {
        get
        { return this.__x__; }
        set
        {
            SqlInt32 temp = __x__;
            __x__ = value;
            if (!checkPoint())
            {
                __x__ = temp;
                throw new ArgumentException("Zła współrzędna X.");
            }
        }
    }
    public SqlInt32 Y
    {
        get
        { return this.__y__; }
        set
        {
            SqlInt32 temp = __y__;
            __y__ = value;
            if (!checkPoint())
            {
                __y__ = temp;
                throw new ArgumentException("Zła współrzędna X.");
            }
        }
    }
    public bool IsNull
    {
        get
        { return (is_Null); }
    }

    /*********************************************
     * Public functions
     *********************************************/

    public static Point2D Null
    {
        get
        {
            Point2D pt = new Point2D();
            pt.is_Null = true;
            return pt;
        }
    }

    public Point2D()
    {

    }
    // main constructor
    public Point2D(SqlInt32 x, SqlInt32 y)
    {
        this.__x__ = x;
        this.__y__ = y;
    }

    //secondary constructor with single input
    public Point2D(SqlInt32 coor)
    {
        __x__ = coor;
        __y__ = coor;
    }

    //secondary constructor with single input
    public Point2D(SqlString coords)
    {
        string[] strcords = coords.ToString().Split(',');

        __x__ = SqlInt32.Parse(strcords[0]);
        __y__ = SqlInt32.Parse(strcords[1]);
    }


    // Odległość od 0,0.
    [SqlMethod(OnNullCall = false)]
    public SqlDouble Distance()
    {
        return DistanceBetween(0, 0);
    }

    // Odległość od wskazanego punktu
    [SqlMethod(OnNullCall = false)]
    public SqlDouble DistanceFrom(Point2D pFrom)
    {
        return DistanceBetween(pFrom.X, pFrom.Y);
    }

    // Odległość od wskazanego punktu.
    [SqlMethod(OnNullCall = false)]
    public SqlDouble DistanceBetween(SqlInt32 iX, SqlInt32 iY)
    {
        return Math.Sqrt(Math.Pow((iX - __x__).Value, 2.0) + Math.Pow((iY - __y__).Value, 2.0));
    }

    /*********************************************
     * Private utility functions
     *********************************************/
    // metoda walidująca współrzędne X Y
    private bool checkPoint()
    {
        if ((__x__ >= 0) && (__y__ >= 0))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //check if point pq intersects line between p1 and p2
    public bool Intersect(Point2D p1, Point2D p2)
    {
        return (p1.DistanceFrom(p2) == DistanceFrom(p1) + DistanceFrom(p2)).Value;
    }

    /*********************************************
     * Override functions
     *********************************************/

    [SqlMethod(OnNullCall = false)]
    public static Point2D Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        Point2D pt = new Point2D();
        string[] xy = s.Value.Split(" ".ToCharArray());
        pt.X = Int32.Parse(xy[0]);
        pt.Y = Int32.Parse(xy[1]);

        if (!pt.checkPoint())
            throw new ArgumentException("Invalid XY coordinate values.");
        return pt;
    }

    // Use StringBuilder to provide string representation of UDT.  
    public override string ToString()
    {
        if (this.IsNull)
            return "NULL";
        else
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(__x__);
            builder.Append(",");
            builder.Append(__y__);
            return builder.ToString();
        }
    }
}

