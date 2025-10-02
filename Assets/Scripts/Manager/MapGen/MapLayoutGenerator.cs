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
        
        // 3. DANN Road Layout
        CreateRoadLayout(layoutArray, exitPoints);
        
        // 4. ZULETZT leere Felder füllen
        FillEmptyFields(layoutArray);
        
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
        for (int x = 0; x < 9; x++)
        {
            for (int z = 0; z < 9; z++)
            {
                if (fieldsArray[x][z] == 0)
                {
                    fieldsArray[x][z] = Random.Range(4, 7); // Random vegetation
                }
            }
        }
    }

    public struct ExitPoints
    {
        public int leftY, rightY;  // Y-Koordinaten (Z-Index) für linke/rechte Exits
        public int botX, topX;     // X-Koordinaten für untere/obere Exits
    }
}