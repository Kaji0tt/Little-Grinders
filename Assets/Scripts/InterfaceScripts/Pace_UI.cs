using UnityEngine;
using UnityEngine.UI;

public class Pace_UI : MonoBehaviour
{
    [SerializeField] private Sprite spriteNoCharge;
    [SerializeField] private Sprite spriteOneCharge;
    [SerializeField] private Sprite spriteTwoCharge;
    [SerializeField] private Sprite spriteFullCharged;

    private Image imageComponent;

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
    }

    public void UpdateChargeUI(int chargeCount)
    {
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
}

