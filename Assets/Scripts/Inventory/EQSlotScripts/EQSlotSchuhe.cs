
public class EQSlotSchuhe : EQSlotBase
{
    private void Awake()
    {
        slotType = EquipmentSlotType.Schuhe; // Setzt den Slot-Typ auf Schuhe
    }

    protected override void BindToGameEvent()
    {
        GameEvents.Instance.OnEquipSchuhe += Equip;
    }

    protected override void UnbindFromGameEvent()
    {
        GameEvents.Instance.OnEquipSchuhe -= Equip;
    }
}