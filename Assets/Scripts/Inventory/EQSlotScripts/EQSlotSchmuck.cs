
public class EQSlotSchmuck : EQSlotBase
{
    private void Awake()
    {
        slotType = EquipmentSlotType.Schmuck; // Setzt den Slot-Typ auf Schuhe
    }

    protected override void BindToGameEvent()
    {
        GameEvents.Instance.OnEquipSchmuck += Equip;
    }

    protected override void UnbindFromGameEvent()
    {
        GameEvents.Instance.OnEquipSchmuck -= Equip;
    }
}
