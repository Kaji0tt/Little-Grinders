using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialLootBoxCollider : MonoBehaviour
{
    public Tutorial tutorialScript;

    private void OnTriggerEnter(Collider collider)
    {
        /*
        if(collider.gameObject.tag == "Player" && gotTriggered == false)
        {
            gotTriggered = true;
            tutorialScript.ShowTutorial(2);
        }
        */
    }
}
