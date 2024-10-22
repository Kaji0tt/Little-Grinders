using UnityEngine;
using UnityEngine.UI;

public class EQSlotWeapon : EQSlotBase
{
    private GameObject weaponAnim;

    private void Awake()
    {
        slotType = EquipmentSlotType.Weapon; // Setzt den Slot-Typ auf Weapon
        weaponAnim = GameObject.Find("WeaponAnimParent");
    }

    protected override void BindToGameEvent()
    {
        GameEvents.Instance.OnEquipWeapon += Equip;
    }

    protected override void UnbindFromGameEvent()
    {
        GameEvents.Instance.OnEquipWeapon -= Equip;
    }

    public override void Equip(ItemInstance item)
    {
        base.Equip(item); // Nutzt die allgemeine Logik für das Ausrüsten von Items

        // Spezifische Logik für die Waffe: Setzt das Sprite der Waffe im WeaponAnim GameObject
        if (weaponAnim != null)
        {
            weaponAnim.GetComponent<SpriteRenderer>().sprite = item.icon;
        }

        // Logik für Fernkampfwaffen
        if (item.RangedWeapon)
        {
            PlayerManager.instance.player.rangedWeapon = true;
        }

        // Spezielle Tutorial-Logik
        if (item.ItemID == "WP0001" && GameObject.FindGameObjectWithTag("TutorialScript") != null)
        {
            Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
            tutorialScript.ShowTutorial(5);
        }
    }

    public override void Dequip()
    {
        base.Dequip(); // Nutzt die allgemeine Logik für das Ablegen von Items

        // Entfernt das Waffensprite vom WeaponAnim GameObject
        if (weaponAnim != null)
        {
            weaponAnim.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Blank_Icon");
        }

        /* Derzeit nicht vorhanden.
        // Logik für Fernkampfwaffen
        if (weapon_Item != null && weapon_Item.RangedWeapon)
        {
            PlayerManager.Instance.Player.GetComponent<IsometricPlayer>().rangedWeapon = false;
        }
        */
    }

    public override void LoadItem(ItemInstance item)
    {
        base.LoadItem(item); // Nutzt die allgemeine Logik zum Laden eines Items

        // Setzt das Waffensprite im WeaponAnim GameObject
        if (weaponAnim != null)
        {
            weaponAnim.GetComponent<SpriteRenderer>().sprite = item.icon;
        }
    }
}