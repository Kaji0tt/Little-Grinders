using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAim : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit[] hits;

        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

        for (int i = 0; i < hits.Length; i++)
        {

            RaycastHit hit = hits[i];
            //print(hits[i].transform.name);
            if (hit.transform.tag == "Enemy")
                transform.position = hit.point;

            else if (hit.transform.tag == "Floor")
                transform.position = hit.point;
        }

    }
}
