using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string myName;
    public int myAttackDamage, myAbilityPower, myMovementSpeed, myAttackSpeed, myArmor, myHealth, myRegeneration;

    public Sprite mySpriteSheet;
    public RuntimeAnimatorController myAnimatorController;
    public IsometricRenderer myRenderer { get; private set; }

    //Weitere Felder wie Loot, Verhaltenstyp (als enum, gute Idee!), etc.

}
