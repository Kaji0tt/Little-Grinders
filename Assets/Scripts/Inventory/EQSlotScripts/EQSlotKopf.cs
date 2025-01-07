
public class EQSlotKopf : EQSlotBase
{
    private void Awake()
    {
        slotType = EquipmentSlotType.Kopf; // Setzt den Slot-Typ auf Kopf
    }

    protected override void BindToGameEvent()
    {
        GameEvents.Instance.OnEquipKopf += Equip;
    }

    protected override void UnbindFromGameEvent()
    {
        GameEvents.Instance.OnEquipKopf -= Equip;
    }
}