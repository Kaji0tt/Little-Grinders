//Useable sollten dringend so überarbeitet werden, dass eine Entitie mitgegeben wird.
public interface IUseable
{
    string GetName();
    string GetDescription(); // NEU

    void Use();

    bool IsOnCooldown();

    float GetCooldown();

    bool IsActive();

    float GetActiveTime();

    int GetCurrentCharges();
    int GetMaxCharges();
}
