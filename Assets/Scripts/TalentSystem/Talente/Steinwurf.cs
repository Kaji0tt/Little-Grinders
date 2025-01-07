using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Steinwurf : MonoBehaviour, IUseable
{


    IsometricPlayer player;

    [SerializeField]
    private GameObject projectile;

    Steinwurf_Bullet bullet;

    [SerializeField]
    public float speed;

    [SerializeField]
    public float range;

    [SerializeField]
    public float damage;

    // Scaling of AP
    //[SerilizeField]
    //private float scaling;

    //Does it need Line of Sight?
    [SerializeField]
    public bool los;

    private Transform target;
    private Vector3 spell_destination;

    private void Awake()
    {
        damage = 5;

        SetDescription("Du wirfst einen Stein auf deinen Gegner und fügst " + damage + " Schaden zu");


    }

    public override void Use()
    {

        player = PlayerManager.instance.player.GetComponent<IsometricPlayer>();
        if (!onCoolDown && currentCount >= 1)
        {


            RaycastHit[] hits;

            hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

            for (int i = 0; i < hits.Length; i++)
            {

                RaycastHit hit = hits[i];
                //print(hits[i].transform.name);
                if (hit.transform.tag == "Enemy")
                {
                    float dist = Vector3.Distance(spell_destination, PlayerManager.instance.player.transform.position);

                    if (dist <= range)
                    {
                        Steinwurf_Bullet bullet = projectile.GetComponent<Steinwurf_Bullet>();

                        bullet.InstantiateMe(this, hit.point);

                        Instantiate(projectile, PlayerManager.instance.player.transform.position, Quaternion.identity);

                        //Play Bullet Sound
                        string[] bulletSound = { "Wurf1", "Wurf2", "Wurf3" };
                        AudioManager.instance.Play(bulletSound[Random.Range(0, 3)]);

                    }

                }


                else if (hit.transform.tag == "Floor")
                {
                    Steinwurf_Bullet bullet = projectile.GetComponent<Steinwurf_Bullet>();

                    bullet.InstantiateMe(this, hit.point);

                    Instantiate(projectile, PlayerManager.instance.player.transform.position, Quaternion.identity);
                }

            }


            onCoolDown = true;
        }
    }

    public override bool IsOnCooldown()
    {
        return onCoolDown;
    }

    public override float GetCooldown()
    {
        return GetSpellCoolDown;
    }

    public override float CooldownTimer()
    {
        return coolDownTimer;
    }
}
*/