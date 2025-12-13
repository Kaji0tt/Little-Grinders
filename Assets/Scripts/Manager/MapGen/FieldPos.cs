using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum FieldType
{
    Empty = 0,
    Road = 1,
    RoadVertical = 2,
    RoadHorizontal = 3,
    NoVeg = 4,
    LowVeg = 5,
    HighVeg = 6,
    ExitTop = 7,
    ExitBot = 8,
    ExitLeft = 9,
    ExitRight = 10,
    OutsideExitTop = 11,
    OutsideExitBot = 12,
    OutsideExitLeft = 13,
    OutsideExitRight = 14,
    OutsideTop = 15,
    OutsideBot = 16,
    OutsideLeft = 17,
    OutsideRight = 18,
    OutsideCorner = 19,
    PreBuildTile = 20,
    RoadTJunctionTop = 21,
    RoadTJunctionBot = 22,
    RoadTJunctionLeft = 23,
    RoadTJunctionRight = 24,
    RoadCrossroad = 25,
    RoadCurveTopLeft = 26,
    RoadCurveTopRight = 27,
    RoadCurveBottomLeft = 28,
    RoadCurveBottomRight = 29,
}

public class FieldPos : MonoBehaviour
{


    public FieldType Type = FieldType.Empty;
    
    public int ArrayPosX { get; set; } 
    public int ArrayPosZ { get; set; }
    
    void Start()
    {
        if ((int)transform.position.x - 5 != 0)
        {
            ArrayPosX = ((int)transform.position.x - 5)/10;
        }
        else
        {
            ArrayPosX = 0;
        }
        if ((int)transform.position.z - 5 != 0)
        {
            ArrayPosZ = ((int)transform.position.z - 5)/10;
        }
        else
        {
            ArrayPosZ = 0;
        }
    }
}
