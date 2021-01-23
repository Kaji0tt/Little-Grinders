using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootbox : MonoBehaviour
{




    private void OnTriggerStay(Collider collider)
    {
        if (Input.GetKeyDown(KeyCode.Q) && collider.gameObject.tag == "Player")
        {
            print("got triggered");
            ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);
        }
    }

}
