using UnityEngine;

[CreateAssetMenu(menuName = "Active/Ability")]
public class AbilityData : ScriptableObject
{
    public string abilityName;

    public string description;

    public float cooldownTime;

    public float activeTime;

    public float tickInterval;

    public float tickTimer;

    public bool isPersistent;

    public Sprite icon;

    public GameObject myAbilityPrefab; // ← Prefab mit AbilityRunner-Komponente!

    public virtual void UseAbility(PlayerStats target)
    {
        if (myAbilityPrefab == null)
        {
            Debug.LogWarning($"No AbilityRunnerPrefab assigned in {abilityName}");
            return;
        }

        GameObject runnerGO = Instantiate(myAbilityPrefab);
        Ability runner = runnerGO.GetComponent<Ability>();

        runner.Initialize(this, target); // übergebe Daten und Ziel
    }
}