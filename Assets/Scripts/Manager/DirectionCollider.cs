using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionCollider : MonoBehaviour
{
    #region Singleton;
    public static DirectionCollider instance;
    private void Awake()
    {
        instance = this;
    }
    public GameObject dirCollider;
    #endregion

    private float attackCD = 0;
    
    private Vector3 right, forward;


    void Start()
    {
        IsometricPlayer player = PlayerManager.instance.player.GetComponent<IsometricPlayer>();

        //Übernehmen der Kamera Achsen
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }


    void Update()
    {

        if (Input.anyKey)
        {
            Move();

        }
        
    }

    private void Move()
    {
        Vector3 direction = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        direction = Vector3.ClampMagnitude(direction, 1);
        direction = direction.normalized;

        if (direction.magnitude > .3f)
            transform.position = new Vector3(PlayerManager.instance.player.transform.position.x, PlayerManager.instance.player.transform.position.y, PlayerManager.instance.player.transform.position.z)
                                 + direction;

    }

    //Bleibt die Frage, ob es schlauer ist im Enemy.cs oder im DirCollider die TakeDamage abfrage zu callen.
    /*
    private void OnTriggerStay(Collider enemy)
    {

        if (enemy.gameObject.tag == "Enemy")
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

            attackCD -= Time.deltaTime;

            if (attackCD <= 0)
            {

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    //print(enemy.gameObject.name);
                    //print(enemy.gameObject.GetComponent<EnemyController>().Hp.Value);
                    enemy.gameObject.GetComponentInParent<EnemyController>().TakeDamage(playerStats.AttackPower.Value, playerStats.Range);

                    attackCD = 1f / playerStats.AttackSpeed.Value;
                }

            }


        }
    }
    */

}
