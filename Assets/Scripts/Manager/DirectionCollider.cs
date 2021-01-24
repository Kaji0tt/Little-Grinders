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

    public IsometricPlayer player;
    private Vector3 right, forward;


    void Start()
    {
        player = GetComponent<IsometricPlayer>();

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

        if (direction.magnitude > .5f)
            transform.position = PlayerManager.instance.player.transform.position + direction;

    }

    /* Vielleicht das ganze im Enemy abfragen?
    private void OnTriggerStay(Collider collider)
    {

        //Falls im DirectionCollider eine Instanz von Enemy steckt, führe Angriff aus.
        GameObject collidedGO = collider.gameObject;
        //Enemy enemy = collidedGO.GetComponent<Enemy>();
        //print(enemy.Hp.Value);
        print(collidedGO.name);
        if (collidedGO.CompareTag("Enemy"))
        {
            Enemy enemy = collidedGO.GetComponent<Enemy>();
            print(enemy.Hp.Value);
            player.attackCD -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (player.attackCD <= 0)
                {
                    enemy.GetComponent<Enemy>().TakeDamage(player.AttackPower.Value, player.Range);
                    player.attackCD = 1f / player.AttackSpeed.Value;
                }
            }
            
        }
    }
    */

}
