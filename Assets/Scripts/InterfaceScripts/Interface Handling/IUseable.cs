
public interface IUseable
{
    string GetName();

    void Use();

    bool IsOnCooldown();

    float GetCooldown();

    float CooldownTimer();

    bool IsActive();
}
