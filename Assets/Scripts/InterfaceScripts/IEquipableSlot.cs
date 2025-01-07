public enum EquipmentSlotType
{
    Kopf,
    Brust,
    Hose,
    Schuhe,
    Schmuck,
    Weapon
}


public interface IEquipableSlot
{
    void Equip(ItemInstance item);
    void Dequip();
    void TaskOnClick();
    void LoadItem(ItemInstance item);
}
