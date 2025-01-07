using UnityEngine;
using UnityEngine.UI;

public class Pace_UI : MonoBehaviour
{
    [SerializeField] private Sprite spriteNoCharge;
    [SerializeField] private Sprite spriteOneCharge;
    [SerializeField] private Sprite spriteTwoCharge;
    [SerializeField] private Sprite spriteFullCharged;

    private Image imageComponent;



    private void Start()
    {
        Debug.Log("I woke up");
        imageComponent = GetComponent<Image>();

        Debug.Log("Going to Check UI Colors ");
        SetPaceUIColors();

        Debug.Log("UI Colors good");
    }

    public void UpdateChargeUI(int chargeCount)
    {
        SetPaceUIColors();

        // Aktualisiert das angezeigte Sprite basierend auf der Anzahl der Ladungen
        switch (chargeCount)
        {
            case 0:
                imageComponent.sprite = spriteNoCharge;
                break;
            case 1:
                imageComponent.sprite = spriteOneCharge;
                break;
            case 2:
                imageComponent.sprite = spriteTwoCharge;
                break;
            case 3:
                imageComponent.sprite = spriteFullCharged;
                break;
            default:
                Debug.LogWarning("Ungültige Anzahl von Ladungen");
                break;
        }
    }

    private void Update()
    {
        IsPaceSkilled();
    }

    public bool IsPaceSkilled()
    {
        if (TalentTree.instance.defaultTalent.currentCount >= 1)
        {
            return true;
        }
        else return false;

    }

    public void SetPaceUIColors()
    {
        if (IsPaceSkilled())
        {
            // Setze normale Farben
            imageComponent.color = Color.white; // Standardfarbe
        }
        else
        {
            // Setze auf Schwarz-Weiß (Graustufen)
            imageComponent.color = Color.black; // Grautöne
        }
    }

}

