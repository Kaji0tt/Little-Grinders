using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Talent : MonoBehaviour
{

    private Image sprite; //Verwendet er, damit ausgewählte Talente / noch nicht skillbare Talente ausgegraut werden.

    [SerializeField]
    private Text countText;

    [SerializeField]
    private int maxCount;
    private int currentCount;

    public bool unlocked;

    [SerializeField]
    private Talent childTalent;

    //Die Weiterleitung zu anderen Talenten macht er in Abhängig von "Pfeilen", also wenn ein PfeilSprite auf eine andere Fähigkeit zeigt, bzw. sie einen Pfeil besitzt, dann wird das Folgetalent freigeschaltet.
    //Kp ob ich das so machen wollen würde, entsprechendes Video hier:
    // https://youtu.be/NEqaBBnAFfM?t=406


    private void Awake()
    {
        sprite = GetComponent<Image>();
        countText.text = $"{currentCount}/{maxCount}";


        if (unlocked)
            Unlock();
    }

    public bool Click()
    {
        if (currentCount < maxCount && unlocked)
        {
            currentCount++;
            countText.text = $"{currentCount}/{maxCount}";

            if (currentCount == maxCount)
            {
                if (childTalent != null)
                {
                    childTalent.Unlock();
                }
            }
            return true;
        }
        return false;
    }

    public void LockTalents()
    {
        sprite.color = Color.grey;
        countText.color = Color.grey;
    }

    public void Unlock()
    {
        sprite.color = Color.white;
        countText.color = Color.white;

        unlocked = true;
    }
}
