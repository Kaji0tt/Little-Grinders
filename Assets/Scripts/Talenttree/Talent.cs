using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Talent : MonoBehaviour, IMoveable, IPointerClickHandler
{

    public TalentType talentType;

    //Description of the Spell
    [SerializeField]
    [TextArea]
    private string description;

    public string GetDescription
    {
        get
        {
            return description;
        }
    }
    
    public virtual void SetDescription(string newDes)
    {
        this.description = newDes;
    }
    
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

    //[SerializeField]
    public Talent[] childTalent;

    //Wie viele Punkte in TalentType sind nötig, für dieses Talent?
    public int requiredTypePoints;

    private int collectedTypePoints;

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

        //UpdateTalentTypePoins();
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

    public void IncreaseTalentTypePoins(Talent talent)
    {
        switch (talent.talentType)
        {
            case TalentType.Life:

                TalentTree.instance.lifePoints++;
                foreach (Talent lT in TalentTree.instance.lifeTalents)
                {
                        lT.collectedTypePoints = TalentTree.instance.lifePoints;

                    if (lT.collectedTypePoints == lT.requiredTypePoints && lT is Spell)
                        lT.Unlock();
                }



                break;

            case TalentType.Combat:

                TalentTree.instance.combatPoints++;
                foreach (Talent cT in TalentTree.instance.combatTalents)
                {
                        cT.collectedTypePoints = TalentTree.instance.combatPoints;

                    if (cT.collectedTypePoints == cT.requiredTypePoints && cT is Spell)
                        cT.Unlock();
                }

                break;

            case TalentType.Void:

                TalentTree.instance.voidPoints++;
                foreach(Talent vT in TalentTree.instance.voidTalents)
                {
                        vT.collectedTypePoints = TalentTree.instance.voidPoints;

                    if (vT.collectedTypePoints == vT.requiredTypePoints && vT is Spell)
                        vT.Unlock();
                }


                break;


        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        TalentTree.instance.TryUseTalent(this);
        

    }
}
