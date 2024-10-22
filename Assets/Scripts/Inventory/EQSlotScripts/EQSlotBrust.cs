
public class EQSlotBrust : EQSlotBase
{
    private void Awake()
    {
        slotType = EquipmentSlotType.Brust; // Setzt den Slot-Typ auf Schuhe
    }

    protected override void BindToGameEvent()
    {
        GameEvents.Instance.OnEquipBrust += Equip;
    }

    protected override void UnbindFromGameEvent()
    {
        GameEvents.Instance.OnEquipBrust -= Equip;
    }
}
