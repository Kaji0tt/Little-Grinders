using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldType
{
    Forest = 0,
    Jungle = 1,
    Desert = 2,
}

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

    public WorldType worldType;



    List<GameObject> possibleMobs = new List<GameObject>();

    //Collection of all possible themes.
    public List<PrefabTheme> themeCollection = new List<PrefabTheme>();

    //Private collection of themes
    private List<PrefabTheme> possibleThemes = new List<PrefabTheme>();

    //Create a List of PrefabCollections,
    //in dependency of MapRoll, populate the prefabs 
    public void PopulatePrefabCollection(string playerSpawn)
    {
        possibleThemes.Clear();

        //possibleThemes.Add(themeCollection[0]);
        //Prüfe in der Liste der Themes, welche anhand des Global-Map-Levels zur Verfügung stehen und füge sie einer tempListe hinzu.
        foreach (PrefabTheme theme in themeCollection)
        {
            if(GlobalMap.instance.currentMap != null)
            if (theme.requiredLevel <= GlobalMap.instance.currentMap.mapLevel)
            {
                possibleThemes.Add(theme);
            }

        }

        //Würfel ein zufälliges Theme aus
        int themeInt = Random.Range(0, possibleThemes.Count);



        //Write WorldType to public Enum for saving purposes.
        if(themeInt == 0) { worldType = WorldType.Forest; }
        if(themeInt == 1) { worldType = WorldType.Jungle; }
        if(themeInt == 2) { worldType = WorldType.Desert; }


        //possible themes count seems to be buggy, theme int + worldType seem to be working.
        print("The count of possible MapThemes: " + possibleThemes.Count + ". The themeInt that has been rolled: " + themeInt + ", resulting in WorldType: " + worldType.ToString());



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
        }
        /*
        if (possibleThemes.Count >= 1)
        {
            smalGreenPF = null;
            smalGreenPF = possibleThemes[themeInt].smalGreenPF;

            midGreenPF = null;
            midGreenPF = possibleThemes[themeInt].midGreenPF;

            highGreenPF = null;
            highGreenPF = possibleThemes[themeInt].highGreenPF;

            horizntalFencePF = null;
            horizntalFencePF = possibleThemes[themeInt].horizntalFencePF;

            verticalFencePF = null;
            verticalFencePF = possibleThemes[themeInt].verticalFencePF;

            enemiesPF = null;
            enemiesPF = possibleThemes[themeInt].enemiesPF;
            //interactablesPF = possibleThemes[themeInt].interactablesPF;
        }
        */

        MapGenHandler.instance.CreateANewMap2(playerSpawn);


    }

    public void LoadPrefabCollection(MapSave map)
    {
        
        //Populate the PrefabCollection with the theme that was saved on Map.
        smalGreenPF = themeCollection[(int)map.mapTheme].smalGreenPF;
        midGreenPF = themeCollection[(int)map.mapTheme].midGreenPF;
        highGreenPF = themeCollection[(int)map.mapTheme].highGreenPF;
        horizntalFencePF = themeCollection[(int)map.mapTheme].horizntalFencePF;
        verticalFencePF = themeCollection[(int)map.mapTheme].verticalFencePF;
        enemiesPF = themeCollection[(int)map.mapTheme].enemiesPF;
        interactablesPF = themeCollection[(int)map.mapTheme].interactablesPF;

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

        foreach(GameObject mob in enemiesPF)
        {
            if (GlobalMap.instance.currentMap != null)
            {
                //print("current map is not null");
                if (mob.GetComponent<EnemyController>().level -1 <= GlobalMap.instance.currentMap.mapLevel)
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

}
