using UnityEngine;

public static class MapLayoutGenerator
{
    public static int[][] GenerateLayout()
    {
        // Array initialisieren
        int[][] layoutArray = new int[9][];
        for (int i = 0; i < 9; i++)
        {
            layoutArray[i] = new int[9];
        }
        
        // 1. ERST Outside Fields
        SetOutsideFields(layoutArray);
        
        // 2. DANN garantierte Exits an allen 4 Seiten
        var exitPoints = SetGuaranteedExitPoints(layoutArray);
        
        // 3. DANN Road Layout mit generischen Roads
        CreateRoadLayout(layoutArray, exitPoints);
        
        // 4. DANN leere Felder füllen
        FillEmptyFields(layoutArray);
        
        // 5. NEU: Konvertiere Roads zu richtungsbasierten Types
        ConvertRoadsToDirectional(layoutArray);
        
        return layoutArray;
    }

    private static void SetOutsideFields(int[][] fieldsArray)
    {
        for (int x = 0; x < 9; x++)
        {
            for (int z = 0; z < 9; z++)
            {
                if ((x == 0 || x == 8) && (z == 0 || z == 8))
                {
                    fieldsArray[z][x] = 19; // Corner
                }
                else if (x == 0)
                {
                    fieldsArray[z][x] = 17; // Left border
                }
                else if (x == 8)
                {
                    fieldsArray[z][x] = 18; // Right border
                }
                else if (z == 0)
                {
                    fieldsArray[z][x] = 16; // Bottom border
                }
                else if (z == 8)
                {
                    fieldsArray[z][x] = 15; // Top border
                }
            }
        }
    }

    /// <summary>
    /// Garantiert Exits an ALLEN 4 Seiten - KORRIGIERTE Zuweisungen für [z][x] Array
    /// </summary>
    private static ExitPoints SetGuaranteedExitPoints(int[][] fieldsArray)
    {
        var exits = new ExitPoints();
        
        // ✅ Left Exit (X=0/1) - Array Position [z][0] und [z][1]
        exits.leftY = Random.Range(2, 7); 
        fieldsArray[exits.leftY][1] = 9;  // InnerExitLeft
        fieldsArray[exits.leftY][0] = 13; // OutsideExitLeft

        // ✅ Right Exit (X=7/8) - Array Position [z][7] und [z][8]
        exits.rightY = Random.Range(2, 7); 
        fieldsArray[exits.rightY][7] = 10; // InnerExitRight
        fieldsArray[exits.rightY][8] = 14; // OutsideExitRight

        // ✅ Bottom Exit (Z=0/1) - Array Position [0][x] und [1][x]
        exits.botX = Random.Range(2, 7); 
        fieldsArray[1][exits.botX] = 8;  // InnerExitBot
        fieldsArray[0][exits.botX] = 12; // OutsideExitBot

        // ✅ Top Exit (Z=7/8) - Array Position [7][x] und [8][x]
        exits.topX = Random.Range(2, 7); 
        fieldsArray[7][exits.topX] = 7;  // InnerExitTop
        fieldsArray[8][exits.topX] = 11; // OutsideExitTop

        Debug.Log($"[MapLayoutGenerator] Exits generiert - Left:Y{exits.leftY}, Right:Y{exits.rightY}, Bottom:X{exits.botX}, Top:X{exits.topX}");
        
        return exits;
    }

    private static void CreateRoadLayout(int[][] fieldsArray, ExitPoints exits)
    {
        // Hauptstraße von unten nach oben (botX zu topX)
        CreateMainRoad(fieldsArray, exits.botX, exits.topX);
        
        // Seitenstraßen zu den Seiten-Exits (leftY und rightY)
        ConnectSideRoads(fieldsArray, exits.leftY, exits.rightY);
    }

    private static void CreateMainRoad(int[][] fieldsArray, int botExitX, int topExitX)
    {
        Debug.Log($"[MapLayoutGenerator] Erstelle Hauptstraße von Bot-Exit X{botExitX} zu Top-Exit X{topExitX}");
        
        int currentX = botExitX;
        int currentZ = 1;
        
        while (currentZ < 7)
        {
            fieldsArray[currentZ][currentX] = 1; // Road (Beachte: [Z][X]!)
            
            if (currentZ < 6)
            {
                if (currentX != topExitX && Random.Range(0, 3) == 0)
                {
                    if (currentX < topExitX) currentX++;
                    else if (currentX > topExitX) currentX--;
                }
                else
                {
                    currentZ++;
                }
            }
            else
            {
                if (currentX < topExitX) currentX++;
                else if (currentX > topExitX) currentX--;
                else currentZ++;
            }
        }
        
        fieldsArray[currentZ][topExitX] = 1;
    }

    private static void ConnectSideRoads(int[][] fieldsArray, int leftExitY, int rightExitY)
    {
        Debug.Log($"[MapLayoutGenerator] Seitenstraßen: Left Y{leftExitY}, Right Y{rightExitY}");
        
        // Linke Seitenstraße (von X=1 nach innen)
        for (int x = 1; x < 8; x++)
        {
            if (fieldsArray[leftExitY][x] == 0) // [Z][X] = [Y][X]
            {
                fieldsArray[leftExitY][x] = 1;
            }
            if (x < 7 && fieldsArray[leftExitY][x + 1] == 1)
                break;
        }
        
        // Rechte Seitenstraße (von X=7 nach innen)
        for (int x = 7; x > 0; x--)
        {
            if (fieldsArray[rightExitY][x] == 0) // [Z][X] = [Y][X]
            {
                fieldsArray[rightExitY][x] = 1;
            }
            if (x > 1 && fieldsArray[rightExitY][x - 1] == 1)
                break;
        }
    }

    private static void FillEmptyFields(int[][] fieldsArray)
    {
        int noVegCount = 0;
        int minNoVegRequired = 3; // Guarantee at least 3 NoVeg fields for Totem/Altar
        
        // First pass: Count existing NoVeg fields
        for (int x = 1; x < 8; x++)
        {
            for (int z = 1; z < 8; z++)
            {
                if (fieldsArray[z][x] == 4) // NoVeg
                {
                    noVegCount++;
                }
            }
        }
        
        Debug.Log($"[MapLayoutGenerator] FillEmptyFields - Existing NoVeg: {noVegCount}");
        
        // Fill empty fields
        for (int x = 1; x < 8; x++)
        {
            for (int z = 1; z < 8; z++)
            {
                if (fieldsArray[z][x] == 0)
                {
                    // If we still need NoVeg fields, prioritize them
                    if (noVegCount < minNoVegRequired)
                    {
                        // 40% chance for NoVeg when below minimum
                        if (Random.Range(0f, 1f) < 0.4f)
                        {
                            fieldsArray[z][x] = 4; // NoVeg
                            noVegCount++;
                        }
                        else
                        {
                            fieldsArray[z][x] = Random.Range(4, 7); // Random vegetation
                        }
                    }
                    else
                    {
                        // Normal random distribution
                        fieldsArray[z][x] = Random.Range(4, 7); // Random vegetation
                    }
                }
            }
        }
        
        // Final guarantee: Force NoVeg if still below minimum
        if (noVegCount < minNoVegRequired)
        {
            Debug.LogWarning($"[MapLayoutGenerator] Only {noVegCount} NoVeg fields! Forcing {minNoVegRequired - noVegCount} more...");
            
            int fieldsToConvert = minNoVegRequired - noVegCount;
            int converted = 0;
            
            // Find non-road fields to convert
            for (int z = 1; z < 8 && converted < fieldsToConvert; z++)
            {
                for (int x = 1; x < 8 && converted < fieldsToConvert; x++)
                {
                    int fieldType = fieldsArray[z][x];
                    // Convert HighVeg (6) or LowVeg (5) to NoVeg
                    if (fieldType == 5 || fieldType == 6)
                    {
                        fieldsArray[z][x] = 4; // NoVeg
                        converted++;
                        Debug.Log($"[MapLayoutGenerator] Converted field [{z}][{x}] from type {fieldType} to NoVeg");
                    }
                }
            }
            
            noVegCount += converted;
        }
        
        Debug.Log($"[MapLayoutGenerator] FillEmptyFields complete - Total NoVeg: {noVegCount}");
    }

    /// <summary>
    /// Converts generic Road tiles (1) to directional types based on neighbor analysis
    /// </summary>
    private static void ConvertRoadsToDirectional(int[][] fieldsArray)
    {
        for (int z = 1; z < 8; z++) // Skip outside borders
        {
            for (int x = 1; x < 8; x++)
            {
                if (fieldsArray[z][x] == 1) // Generic road
                {
                    // Count road neighbors in each direction
                    bool hasTop = (z < 7 && IsRoadOrExit(fieldsArray[z + 1][x]));
                    bool hasBot = (z > 1 && IsRoadOrExit(fieldsArray[z - 1][x]));
                    bool hasLeft = (x > 1 && IsRoadOrExit(fieldsArray[z][x - 1]));
                    bool hasRight = (x < 7 && IsRoadOrExit(fieldsArray[z][x + 1]));

                    int roadCount = (hasTop ? 1 : 0) + (hasBot ? 1 : 0) + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);

                    if (roadCount >= 4)
                    {
                        // Crossroad - 4 connections
                        fieldsArray[z][x] = 25; // RoadCrossroad
                    }
                    else if (roadCount == 3)
                    {
                        // T-Junction - determine which way is open
                        if (!hasTop) fieldsArray[z][x] = 21; // RoadTJunctionTop (open to top)
                        else if (!hasBot) fieldsArray[z][x] = 22; // RoadTJunctionBot (open to bottom)
                        else if (!hasLeft) fieldsArray[z][x] = 23; // RoadTJunctionLeft (open to left)
                        else if (!hasRight) fieldsArray[z][x] = 24; // RoadTJunctionRight (open to right)
                    }
                    else if (roadCount == 2)
                    {
                        // Distinguish between straight roads and curves
                        if (hasTop && hasBot)
                        {
                            // Vertical road - connections opposite each other
                            fieldsArray[z][x] = 2; // RoadVertical
                        }
                        else if (hasLeft && hasRight)
                        {
                            // Horizontal road - connections opposite each other
                            fieldsArray[z][x] = 3; // RoadHorizontal
                        }
                        else
                        {
                            // Curve - connections at 90 degrees
                            if (hasTop && hasLeft)
                            {
                                fieldsArray[z][x] = 26; // RoadCurveTopLeft
                            }
                            else if (hasTop && hasRight)
                            {
                                fieldsArray[z][x] = 27; // RoadCurveTopRight
                            }
                            else if (hasBot && hasLeft)
                            {
                                fieldsArray[z][x] = 28; // RoadCurveBottomLeft
                            }
                            else if (hasBot && hasRight)
                            {
                                fieldsArray[z][x] = 29; // RoadCurveBottomRight
                            }
                        }
                    }
                    else if (roadCount == 1)
                    {
                        // Dead end - determine direction
                        if (hasTop || hasBot) fieldsArray[z][x] = 2; // RoadVertical
                        else fieldsArray[z][x] = 3; // RoadHorizontal
                    }
                    // If roadCount == 0, leave as generic road (shouldn't happen)
                }
            }
        }
        
        Debug.Log("[MapLayoutGenerator] Road conversion complete - directional types assigned");
    }

    /// <summary>
    /// Checks if a field type is a road or exit (should be treated as road connection)
    /// </summary>
    private static bool IsRoadOrExit(int fieldType)
    {
        return fieldType == 1 || fieldType == 2 || fieldType == 3 || // Roads
               fieldType == 7 || fieldType == 8 || fieldType == 9 || fieldType == 10 || // Inner exits
               fieldType == 21 || fieldType == 22 || fieldType == 23 || fieldType == 24 || fieldType == 25 || // Junctions
               fieldType == 26 || fieldType == 27 || fieldType == 28 || fieldType == 29; // Curves
    }

    public struct ExitPoints
    {
        public int leftY, rightY;  // Y-Koordinaten (Z-Index) für linke/rechte Exits
        public int botX, topX;     // X-Koordinaten für untere/obere Exits
    }
}