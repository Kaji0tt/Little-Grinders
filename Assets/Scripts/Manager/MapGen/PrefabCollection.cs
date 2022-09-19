using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldType
{
    Forest = 0,
    Jungle = 1,
    Desert = 2,
}
/*
public enum LevelObjectType
{
    Env,
    Enemy,
    Interactable,
    Tile,
}
*/

public class PrefabCollection : MonoBehaviour
{
    #region Singleton
    public static PrefabCollection instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    [SerializeField]
    private GameObject[] smalGreenPF;
    [SerializeField]
    private GameObject[] midGreenPF;
    [SerializeField]
    private GameObject[] highGreenPF;
    [SerializeField]
    private GameObject[] horizntalFencePF;
    [SerializeField]
    private GameObject[] verticalFencePF;
    [SerializeField]
    private GameObject[] enemiesPF;
    [SerializeField]
    public GameObject[] groundTexture;
    [SerializeField]
    public GameObject[] exitColliders;
    [SerializeField]
    private GameObject[] interactablesPF;

    //Vorgebaute Tiles für Low-Veg-Fields
    public GameObject[] preBuildTiles;

    public WorldType worldType;



    List<GameObject> possibleMobs = new List<GameObject>();

    //Collection of all possible themes.
    public List<PrefabTheme> themeCollection = new List<PrefabTheme>();

    //Private collection of themes
    private List<PrefabTheme> possibleThemes = new List<PrefabTheme>();

    //Create a List of PrefabCollections,
    //in dependency of MapRoll, populate the prefabs 
    public void PopulatePrefabCollection()
    {
        possibleThemes.Clear();

        //possibleThemes.Add(themeCollection[0]);
        //Prüfe in der Liste der Themes, welche anhand des Global-Map-Levels zur Verfügung stehen und füge sie einer tempListe hinzu.
        foreach (PrefabTheme theme in themeCollection)
        {
            if(GlobalMap.instance.currentMap != null)
            if (theme.requiredLevel <= GlobalMap.instance.currentMap.mapLevel)
            {
                //Im Editor dient die Prefab-Collection als Datenbank für alle Verfügbaren Themes. Verfügar sollen aber nur jene sein, 
                //welche im Levelbereich des Spielers liegen.
                possibleThemes.Add(theme);
            }

        }

        //Würfel ein zufälliges Theme aus
        worldType = (WorldType)Random.Range(0, possibleThemes.Count);



        //Populate the PrefabCollection with the theme that has been rolled.
        //Problem - if the themeInt rolls for themeType = Forest, the Prefabs are not recalculated to the Forest prefabs.
        if(possibleThemes.Find(x => x.themeType == worldType))
        {
            print("Found a Theme in possibleThemes with according worldType, should be resetting the Prefabs.");

            smalGreenPF = null;
            smalGreenPF = possibleThemes.Find(x => x.themeType == worldType).smalGreenPF;



            midGreenPF = null;
            midGreenPF = possibleThemes.Find(x => x.themeType == worldType).midGreenPF;



            highGreenPF = null;
            highGreenPF = possibleThemes.Find(x => x.themeType == worldType).highGreenPF;


            horizntalFencePF = null;
            horizntalFencePF = possibleThemes.Find(x => x.themeType == worldType).horizntalFencePF;


            verticalFencePF = null;
            verticalFencePF = possibleThemes.Find(x => x.themeType == worldType).verticalFencePF;


            enemiesPF = null;
            enemiesPF = possibleThemes.Find(x => x.themeType == worldType).enemiesPF;
            //interactablesPF = possibleThemes[themeInt].interactablesPF;

            if (possibleThemes.Find(x => x.themeType == worldType).preBuildTiles.Length > 0)
            {
                preBuildTiles = null;
                preBuildTiles = themeCollection.Find(x => x.themeType == worldType).preBuildTiles;
            }
        }


        //MapGenHandler.instance.CreateANewMap2(playerSpawn);


    }

    public void PopulatePrefabCollection(MapSave map)
    {

        if (themeCollection.Find(x => x.themeType == map.mapTheme))
        {
            print("Found a Theme in possibleThemes with according worldType, should be resetting the Prefabs.");

            smalGreenPF = null;
            smalGreenPF = themeCollection.Find(x => x.themeType == map.mapTheme).smalGreenPF;

            midGreenPF = null;
            midGreenPF = themeCollection.Find(x => x.themeType == map.mapTheme).midGreenPF;

            highGreenPF = null;
            highGreenPF = themeCollection.Find(x => x.themeType == map.mapTheme).highGreenPF;

            horizntalFencePF = null;
            horizntalFencePF = themeCollection.Find(x => x.themeType == map.mapTheme).horizntalFencePF;

            verticalFencePF = null;
            verticalFencePF = themeCollection.Find(x => x.themeType == map.mapTheme).verticalFencePF;

            enemiesPF = null;
            enemiesPF = themeCollection.Find(x => x.themeType == map.mapTheme).enemiesPF;
            //interactablesPF = possibleThemes[themeInt].interactablesPF;


            //Falls es vorgebaute Tiles gibt, lade diese.
            if(themeCollection.Find(x => x.themeType == map.mapTheme).preBuildTiles.Length > 0)
            {
                preBuildTiles = null;
                preBuildTiles = themeCollection.Find(x => x.themeType == map.mapTheme).preBuildTiles;
            }
            else
            {
                preBuildTiles = null;
            }

        }

    }

    public GameObject GetRandomSmalGreenPF()
    {
        return smalGreenPF[Random.Range(0, smalGreenPF.Length)];

    }
    public GameObject GetRandomMidGreenPF()
    {
        return midGreenPF[Random.Range(0, midGreenPF.Length)];

    }
    public GameObject GetRandomHighGreenPF()
    {
        return highGreenPF[Random.Range(0, highGreenPF.Length)];

    }
    public GameObject GetRandomHFencePFPF()
    {
        return horizntalFencePF[Random.Range(0, horizntalFencePF.Length)];

    }
    public GameObject GetRandomVFencePFPF()
    {
        return verticalFencePF[Random.Range(0, verticalFencePF.Length)];


    }
    public GameObject GetRandomEnemie()
    {
        possibleMobs.Clear();

        //If-Statement, der prüft, ob entsprechende Mobs überhaupt ein ausreichendes Level haben, um in diesem Szenario zu spawnen.
        foreach(GameObject mob in enemiesPF)
        {
            if (GlobalMap.instance.currentMap != null)
            {
                //print("current map is not null");
                if (mob.GetComponent<MobStats>().level -1 <= GlobalMap.instance.currentMap.mapLevel)
                {
                    //print("adding mob: " + mob.name);
                    possibleMobs.Add(mob);
                }
            }
            else
                possibleMobs.Add(enemiesPF[1]);

        }

        return possibleMobs[Random.Range(0, possibleMobs.Count)];
    }
    public GameObject GetRandomGroundTexture()
    {
        return groundTexture[Random.Range(0, groundTexture.Length)];
    }

    public GameObject GetRandomInteractable()
    {
        return interactablesPF[Random.Range(0, interactablesPF.Length)];

    }

    public GameObject GetRandomPreBuildTile()
    {
        print("The length of the current Pre-Build Tiles is: " + preBuildTiles.Length);

        return preBuildTiles[Random.Range(0, preBuildTiles.Length)];


    }

}
