using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Schritt 1: What Type of Map is this? Array [1 - x]
//Schritt 2: Choose Objects to Spawn (Small Env, Big Env, Border) of Type [1 - x]
//Schritt 3: Spawn Random Objects of according Arrays [1 - z] and "safe [1 - z]" - wie?
public class SpawnObject : MonoBehaviour
{
    private EnvCamNoRot layerScripts;

    //Die Anzahl an Welttypen
    public enum WorldType { plains, jungle };
    
    //Die Prefabs, welche spawnen dürfen.

    public GameObject[] objectsPlains;

    public GameObject[] objectsJungle;


    //Values, zum Speichern
    private int thisWorldType;

    private void Start()
    {
        System.Array values = System.Enum.GetValues(typeof(WorldType));
        WorldType randomWorldType = (WorldType)values.GetValue(Random.Range(0, values.Length)); //Out of Array Fehler

        print(randomWorldType);


        int rnd = Random.Range(0, objectsPlains.Length);

        var myTransform = Instantiate(objectsPlains[rnd], transform.position, Quaternion.identity);

        //rnd sollte in einer Variable gespeichert werden, damit diese serialized werden kann. 
        
        //Weiter sollte es ein großes Array / List geben, in welcher alle Spawnpunkt liegen. Diese sollten durch nummeriert werden + entsprechend dieser nummerierung sollten die Objekte, welche gespawned
        //sind als int gesichert werden. 
        //Wenn die Map dann erneut geladen werden soll, sollte dieses Array durch gegangen werden und für jedes i das entsprechende Prefab geladen werden.

        myTransform.transform.parent = gameObject.transform.parent;



        layerScripts = FindObjectOfType<EnvCamNoRot>();

        Destroy(this.gameObject);

        //layerScripts.LayerSprites();
    }
}
