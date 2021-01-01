using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : Spell, IUseable
{
    public float travelDistance;

    IsometricPlayer player;


    public void Use()
    {

        player = PlayerManager.instance.player.GetComponent<IsometricPlayer>();
        if (!onCoolDown)
        {

            RaycastHit[] hits;

            hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

            for (int i = 0; i < hits.Length; i++)
            {

                RaycastHit hit = hits[i];

                if (hit.transform.tag == "Floor")
                {
                    Ray ray = new Ray(player.transform.position, hit.point-player.transform.position);

                    Vector3 destination = ray.GetPoint(travelDistance);

                    if (Vector3.Distance(hit.point, player.transform.position) <= travelDistance)
                    {
                        player.transform.position = hit.point;
                    }
                    else player.transform.position = destination;
                }

            }

            onCoolDown = true;
        }
    }

}
