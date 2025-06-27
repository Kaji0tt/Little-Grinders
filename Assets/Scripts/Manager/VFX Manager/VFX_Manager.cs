using UnityEngine;

public class VFX_Manager : MonoBehaviour
{
    public ParticleSystem levelUpEffectPrefab;

    void OnEnable()
    {
        PlayerStats.eventLevelUp += PlayLevelUpEffect;
    }

    void OnDisable()
    {
        PlayerStats.eventLevelUp -= PlayLevelUpEffect;
    }

    void PlayLevelUpEffect()
    {
        if (levelUpEffectPrefab == null)
            return;

        if (PlayerManager.instance.player == null)
            return;

        ParticleSystem effect = Instantiate(
            levelUpEffectPrefab);
        effect.transform.position = PlayerManager.instance.player.transform.position;
        effect.transform.SetParent(PlayerManager.instance.player.transform.GetComponentInParent<Transform>());
        effect.transform.localRotation = levelUpEffectPrefab.transform.localRotation;
        effect.transform.localScale = Vector3.one;

        effect.Play();
        Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
    }
}