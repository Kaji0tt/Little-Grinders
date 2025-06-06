using UnityEngine;
using UnityEngine.UI;

public class EQSlotWeapon : EQSlotBase
{
    private Image weaponImage;

    private void Awake()
    {
        slotType = EquipmentSlotType.Weapon; // Setzt den Slot-Typ auf Weapon
        weaponImage = GetComponent<Image>();
        //Bad Mannor.
        //weaponAnim = GameObject.Find("WeaponAnimParent");
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

        //Greife auf den IsometricRenderer des Spielers zu, um das Waffen-Sprite zu setzen
        IsometricRenderer isoRenderer = PlayerManager.instance.player.GetComponent<IsometricRenderer>();

        //Greife auf das Sprite-Renderer der RootAnimation zu:
        SpriteRenderer weaponSprite = isoRenderer.weaponAnimator.GetComponent<SpriteRenderer>();

        weaponSprite.sprite = item.icon; //Null?

        // Spezifische Logik für die Waffe: Setzt das Sprite der Waffe im WeaponAnim GameObject
        /*
        if (weaponImage.sprite == null)
        {
            weaponImage.sprite = GetComponent<SpriteRenderer>().sprite;
            weaponImage.sprite = item.icon; //Null?
        }
        else
        {
            weaponImage.sprite = item.icon; //Null?
        }

        */

        // Logik für Fernkampfwaffen
        if (item.RangedWeapon)
        {
            PlayerManager.instance.player.rangedWeapon = true;
        }



    }

    public override void Dequip()
    {
        base.Dequip(); // Nutzt die allgemeine Logik für das Ablegen von Items

        // Entfernt das Waffensprite vom WeaponAnim GameObject
        weaponImage.sprite = Resources.Load<Sprite>("Blank_Icon");

    }

    public override void LoadItem(ItemInstance item)
    {
        base.LoadItem(item); // Nutzt die allgemeine Logik zum Laden eines Items

        // Setzt das Waffensprite im WeaponAnim GameObject
        if (weaponImage != null)
        {
            weaponImage.GetComponent<SpriteRenderer>().sprite = item.icon;
        }
    }
}