using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldType { plains, jungle };

//Schritt 1: What Type of Map is this? Array [1 - x]
//Schritt 2: Choose Objects to Spawn (Small Env, Big Env, Border, Path) of Type [1 - x]
//Schritt 3: Spawn Random Objects of according Arrays [1 - z] and "safe [1 - z]" - wie?
public class SpawnObject : MonoBehaviour
{
    private EnvCamNoRot layerScripts;

    //Die Anzahl an Welttypen
    //private WorldType worldType;

    //Später: Gewisse Welttypen sind erst ab einem bestimmten Spielerlevel verfügbar {Wüste ab level 15, Bosslayer ab level 20, [... oder, analog gestaltete Story Maps]

    //Die Prefabs, welche spawnen dürfen.

    [SerializeField]
    public GameObject[] ObjectsPlains { get; private set; }

    [SerializeField]
    public GameObject[] ObjectsJungle { get; private set; }


    //Values, zum Speichern
    private int thisWorldType;

    private void Start()
    {
        /*
        System.Array values = System.Enum.GetValues(typeof(WorldType));

        //worldType = (WorldType)values.GetValue(Random.Range(0, values.Length)); //Out of Array Fehler

        //print(worldType);


        int rnd = Random.Range(0, ObjectsPlains.Length);

        var myTransform = Instantiate(ObjectsPlains[rnd], transform.position, Quaternion.identity);

        //rnd sollte in einer Variable gespeichert werden, damit diese serialized werden kann. 
        
        //Weiter sollte es ein großes Array / List geben, in welcher alle Spawnpunkt liegen. Diese sollten durch nummeriert werden + entsprechend dieser nummerierung sollten die Objekte, welche gespawned
        //sind als int gesichert werden. 
        //Wenn die Map dann erneut geladen werden soll, sollte dieses Array durch gegangen werden und für jedes i das entsprechende Prefab geladen werden.

        myTransform.transform.parent = gameObject.transform.parent;



        layerScripts = FindObjectOfType<EnvCamNoRot>();

        Destroy(this.gameObject);

        //layerScripts.LayerSprites();
        */
    }
}
