using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Threading.Tasks;

class Point2D
{
    /*********************************************
    * Private variables
    *********************************************/
    //Two coordinates of the point
    private SqlDouble x;
    private SqlDouble y;


    /*********************************************
     * Public accessors
     *********************************************/
    public SqlDouble X
    {
        get => x;
    }
    public SqlDouble Y
    {
        get => y;
    }

    /*********************************************
     * Public functions
     *********************************************/
    // main constructor
    public Point2D(SqlDouble x, SqlDouble y)
    {
        this.x = x;
        this.y = y;
    }

    //secondary constructor with single input
    public Point2D(SqlDouble coor)
    {
        x = coor;
        y = coor;
    }

    //secondary constructor with single input
    public Point2D(SqlString coords)
    {
        string[] strcords = coords.ToString().Split(',');

        x = SqlDouble.Parse(strcords[0]);
        y = SqlDouble.Parse(strcords[1]);
    }

    //get distance between two points
    public static SqlDouble getDistance(Point2D p1, Point2D p2)
    {
        return Math.Sqrt(Math.Pow((double)(p2.Y-p1.Y), 2)+Math.Pow((double)(p2.X-p1.X), 2));
    }

    /*********************************************
     * Private utility functions
     *********************************************/


    /*********************************************
     * Override functions
     *********************************************/
     // Use StringBuilder to provide string representation of UDT.  
    public override string ToString()
    {
        return "("+X+';'+Y+") ";
    }
}

