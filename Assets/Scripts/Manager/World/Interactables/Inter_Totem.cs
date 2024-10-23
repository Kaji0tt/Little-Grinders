using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inter_Totem : Interactable
{
    public static Inter_Totem instance;

    [SerializeField]
    private GameObject[] spawnPoints;

    PrefabCollection prefabCollection;

    bool mobsSpawned = false;

    public List<GameObject> spawnedMobs = new List<GameObject>();

    [SerializeField]
    private Light lightBulb;

    private GameObject mobParent;

    private float timeStamp;

    private float intervall = 0.5f;

    private bool soundPlayed = false;


    private void Start()
    {
        mobParent = GameObject.Find("MobParent");
    }

    public override void Use()
    {

        prefabCollection = FindObjectOfType<PrefabCollection>();

        foreach (GameObject spawnPoint in spawnPoints)
        {
            int rnd = Random.Range(0, 2);
            if(rnd == 1)
            {
                GameObject mob = prefabCollection.GetRandomEnemie();

                GameObject instanceMob;
                instanceMob = Instantiate(mob, spawnPoint.transform.position, Quaternion.identity);

                instanceMob.transform.SetParent(mobParent.transform);

                instanceMob.AddComponent<SummonedMob>();

                spawnedMobs.Add(instanceMob);


            }

            

        }

        lightBulb.intensity = 0;

        if (AudioManager.instance != null)
            AudioManager.instance.Play("TotemCall");

        mobsSpawned = true;
    }

    private void Update()
    {

        if(mobsSpawned)
        {
            SummonedMob[] summonedMobs = FindObjectsOfType<SummonedMob>();
            //print(summonedMobs.Length);

            if (summonedMobs.Length == 0)
            {
                if (AudioManager.instance != null && !soundPlayed)
                {
                    AudioManager.instance.Play("TotemClear");
                    soundPlayed = true;
                }


                PlayerManager.instance.player.GetComponent<PlayerStats>().Gain_xp(20);

                timeStamp += Time.deltaTime;

                lightBulb.color = Color.white;
                lightBulb.intensity = 0.7f;

                if(timeStamp > intervall)
                {
                    ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);

                    intervall += intervall;

                    if(intervall >= 5)
                        mobsSpawned = false;
                }

            }
        }
    }
}
