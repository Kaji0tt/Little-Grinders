using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Talent : MonoBehaviour
{

    public Image image;
    
    public Sprite icon
    {
        get
        {
            return image.sprite;
        }
    }
    

    [SerializeField]
    private Text countText;

    [SerializeField]
    private int maxCount;
    public int currentCount { get; private set; }

    public void Set_currentCount(int newCount)
    {
        currentCount = newCount;
    }

    public bool unlocked;

    [SerializeField]
    private Talent[] childTalent;



    //Die Weiterleitung zu anderen Talenten macht er in Abhängig von "Pfeilen", also wenn ein PfeilSprite auf eine andere Fähigkeit zeigt, bzw. sie einen Pfeil besitzt, dann wird das Folgetalent freigeschaltet.
    //Kp ob ich das so machen wollen würde, entsprechendes Video hier:
    // https://youtu.be/NEqaBBnAFfM?t=406

    //Drag and Drop Magie
    //https://youtu.be/ILaDr3CE7QY?t=615



    private void Awake()
    {

        image = GetComponent<Image>();

        countText.text = $"{currentCount}/{maxCount}";

        if (unlocked)
            Unlock();

        //CheckIfUnlocked();

    }


    public bool Click()
    {
        if (currentCount < maxCount && unlocked)
        {
            currentCount++;

            countText.text = $"{currentCount}/{maxCount}";

            if (currentCount == maxCount)
            {
                for (int i = 0; i < childTalent.Length; i++)
                {
                    if (childTalent[i] != null)
                        childTalent[i].Unlock();

                }
            }
            return true;
        }
        return false;
    }

    public void LockTalents()
    {
        image.color = Color.grey;

        countText.color = Color.grey;
    }

    public void Unlock()
    {
        image.color = Color.white;

        countText.color = Color.white;

        unlocked = true;
    }

    public void UpdateTalent()
    {
        image = GetComponent<Image>();

        countText.text = $"{currentCount}/{maxCount}";

        if (unlocked)
            Unlock();
    }


}
