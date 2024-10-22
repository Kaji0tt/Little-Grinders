
public class EQSlotHose : EQSlotBase
{
    private void Awake()
    {
        slotType = EquipmentSlotType.Hose; // Setzt den Slot-Typ auf Schuhe
    }

    protected override void BindToGameEvent()
    {
        GameEvents.Instance.OnEquipHose += Equip;
    }

    protected override void UnbindFromGameEvent()
    {
        GameEvents.Instance.OnEquipHose -= Equip;
    }
}