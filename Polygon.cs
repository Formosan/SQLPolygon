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
public class Polygon
{
    /*********************************************
     * Private variables
     *********************************************/
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
    /*********************************************
     * Public functions
     *********************************************/
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


    /*********************************************
     * Private utility functions
     *********************************************/
    //field
    public SqlDouble getField()
    {
          // Initialze area 
        SqlDouble area = 0.0; 
  
        // Calculate value of shoelace formula 
        int j = (int)lines - 1;

        for (int i = 0; i < lines; i++)
        {
            area += (points[j].X + points[i].X) * (points[j].Y - points[i].Y);
        }
            return area;
    }

    //Circuit
    public SqlDouble getCircuit()
    {
        SqlDouble circuit = 0.0;
        for (int i = 1; i<lines; i++)
        {
            circuit +=Point2D.getDistance(points[i-1], points[i]);
        }
        return circuit;
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

