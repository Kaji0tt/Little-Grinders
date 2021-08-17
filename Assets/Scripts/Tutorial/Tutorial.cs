using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    private int tutorialTextCount = 0;
    public GameObject[] tutorialTxts;

    [SerializeField]
    private CanvasGroup tutorialBox;

    public bool actionBarUsed = false;

    public void NextText()
    {
        tutorialTxts[tutorialTextCount].SetActive(false);
        
        if (tutorialTextCount < 8)
        {
            tutorialTextCount++;
            if(tutorialTxts[tutorialTextCount] != null)
            tutorialTxts[tutorialTextCount].SetActive(true);

        }


        if (tutorialTextCount >= 2)
        {
            tutorialBox.alpha = tutorialBox.alpha > 0 ? 0 : 1;
            tutorialBox.blocksRaycasts = tutorialBox.blocksRaycasts == true ? false : true;
        }



    }

    public void ShowTutorial(int tutCount)
    {
        /*
        if (tutorialTextCount == tutCount)
        {
            tutorialBox.alpha = tutorialBox.alpha > 0 ? 0 : 1;
            tutorialBox.blocksRaycasts = tutorialBox.blocksRaycasts == true ? false : true;
        }
        else
        {
            tutorialTxts[tutorialTextCount].SetActive(false);
            //print("Listen-Element " + tutCount + " wurde manuel abgerufen.");
            tutorialTextCount = tutCount;
            tutorialTxts[tutCount].SetActive(true);

        }
        */
    }

}
