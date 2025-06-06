using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    //private Vector3 right, forward;
    public Vector3 dirVector;

    //IsometricPlayer player;

    public List<EnemyController> collidingEnemyControllers = new List<EnemyController>();
    //public List<GameObject> collidingEnemyControllers = new List<GameObject>();



    void Update()
    {
        int floorLayerMask = LayerMask.GetMask("Floor"); // Erstelle eine Layer-Maske nur für die Ebene mit dem Tag "Floor"
        
        //Direction of Directioncollider = Input.Mouse World Position - Charakter Position 
        Ray ray = CameraManager.instance.mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, floorLayerMask))
        {
            //
            Vector3 rayhitDir = raycastHit.point - PlayerManager.instance.player.transform.position;
            dirVector = rayhitDir.normalized;
        }

        Vector3 newPosition = PlayerManager.instance.player.transform.position + dirVector;
        newPosition.y = transform.position.y; // Setze die Y-Position auf die aktuelle Y-Position des DirectionColliders
        transform.position = newPosition;    

        //Das is noch nicht perfekt, aber OnTriggerExit() funktioniert nicht richtig, 
        //um die entsprechenden Listeneinträge zu entfernen. Deshalb dafür noch WorkAround finden.
        for (int i = 0; i < collidingEnemyControllers.Count; i++)
        {
            if(collidingEnemyControllers.ElementAt(i) == null)
            {
                EnemyController enemy = collidingEnemyControllers.ElementAt(i);
                collidingEnemyControllers.Remove(enemy);
            }

        }

    }

}


