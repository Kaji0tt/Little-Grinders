

//Useable sollten dringend so überarbeitet werden, dass eine Entitie mitgegeben wird.
public interface IUseable
{
    string GetName();

    void Use();

    bool IsOnCooldown();

    float GetCooldown();

    float CooldownTimer();

    bool IsActive();
}
