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
                    fieldsArray[x][z] = 19; // Corner
                }
                else if (x == 0)
                {
                    fieldsArray[x][z] = 17; // Left border
                }
                else if (x == 8)
                {
                    fieldsArray[x][z] = 18; // Right border
                }
                else if (z == 0)
                {
                    fieldsArray[x][z] = 16; // Bottom border
                }
                else if (z == 8)
                {
                    fieldsArray[x][z] = 15; // Top border
                }
            }
        }
    }

    /// <summary>
    /// Garantiert Exits an ALLEN 4 Seiten - nicht nur zufällig
    /// </summary>
    private static ExitPoints SetGuaranteedExitPoints(int[][] fieldsArray)
    {
        var exits = new ExitPoints();
        
        // Bot Exit (garantiert)
        exits.botX = Random.Range(2, 7); // Mehr zentriert
        fieldsArray[exits.botX][1] = 8;  // InnerExitBot
        fieldsArray[exits.botX][0] = 12; // OutsideExitBot

        // Top Exit (garantiert)  
        exits.topX = Random.Range(2, 7); // Mehr zentriert
        fieldsArray[exits.topX][7] = 7;  // InnerExitTop
        fieldsArray[exits.topX][8] = 11; // OutsideExitTop

        // Left Exit (garantiert)
        exits.leftY = Random.Range(2, 7); // Mehr zentriert
        fieldsArray[1][exits.leftY] = 9;  // InnerExitLeft
        fieldsArray[0][exits.leftY] = 13; // OutsideExitLeft

        // Right Exit (garantiert)
        exits.rightY = Random.Range(2, 7); // Mehr zentriert
        fieldsArray[7][exits.rightY] = 10; // InnerExitRight
        fieldsArray[8][exits.rightY] = 14; // OutsideExitRight

        Debug.Log($"[MapLayoutGenerator] Exits generiert - Bot:{exits.botX}, Top:{exits.topX}, Left:Y{exits.leftY}, Right:Y{exits.rightY}");
        
        return exits;
    }

    private static void CreateRoadLayout(int[][] fieldsArray, ExitPoints exits)
    {
        // Hauptstraße von unten nach oben
        CreateMainRoad(fieldsArray, exits.botX, exits.topX);
        
        // Seitenstraßen zu den Seiten-Exits
        ConnectSideRoads(fieldsArray, exits.leftY, exits.rightY);
    }

    private static void CreateMainRoad(int[][] fieldsArray, int startX, int endX)
    {
        Debug.Log($"[MapLayoutGenerator] Erstelle Hauptstraße von X{startX} zu X{endX}");
        
        int currentX = startX;
        int currentZ = 1;
        
        // Direkte Verbindung mit zufälligen Abweichungen
        while (currentZ < 7)
        {
            fieldsArray[currentX][currentZ] = 1; // Road
            
            // Bewege dich hauptsächlich nach oben, aber manchmal zur Seite
            if (currentZ < 6)
            {
                if (currentX != endX && Random.Range(0, 3) == 0) // 33% Chance für Seitenbewegung
                {
                    if (currentX < endX) currentX++;
                    else if (currentX > endX) currentX--;
                }
                else
                {
                    currentZ++;
                }
            }
            else
            {
                // Letzte Reihe: Direkt zum Exit
                if (currentX < endX) currentX++;
                else if (currentX > endX) currentX--;
                else currentZ++;
            }
        }
        
        // Stelle sicher, dass der Top-Exit erreicht wird
        fieldsArray[endX][currentZ] = 1;
    }

    private static void ConnectSideRoads(int[][] fieldsArray, int leftY, int rightY)
    {
        Debug.Log($"[MapLayoutGenerator] Verbinde Seitenstraßen bei Y{leftY} und Y{rightY}");
        
        // Linke Seitenstraße
        for (int x = 1; x < 8; x++)
        {
            if (fieldsArray[x][leftY] == 0) // Nur wenn noch leer
            {
                fieldsArray[x][leftY] = 1; // Road
            }
            // Stoppe wenn wir eine bestehende Straße treffen
            if (x < 7 && fieldsArray[x + 1][leftY] == 1)
                break;
        }
        
        // Rechte Seitenstraße
        for (int x = 7; x > 0; x--)
        {
            if (fieldsArray[x][rightY] == 0) // Nur wenn noch leer
            {
                fieldsArray[x][rightY] = 1; // Road
            }
            // Stoppe wenn wir eine bestehende Straße treffen
            if (x > 1 && fieldsArray[x - 1][rightY] == 1)
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
        public int botX, topX, leftY, rightY;
    }
}