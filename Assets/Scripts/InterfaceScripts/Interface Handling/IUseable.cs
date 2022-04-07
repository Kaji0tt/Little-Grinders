
public interface IUseable
{
    void Use();

    bool IsOnCooldown();

    float GetCooldown();

    float CooldownTimer();

    bool IsActive();
}
