using UnityEngine;

public class TargetSystem : MonoBehaviour
{
    public float maxMouseDistance = 1.0f;

    [Header("Ziel-Indikator")]
    public GameObject targetIndicator; // Verweis auf das eine Zielobjekt

    private void Update()
    {
        var playerPos = PlayerManager.instance.player.transform.position;
        var mouseTargetPoint = DirectionCollider.instance.transform.position;
        var enemiesInRange = DirectionCollider.instance.collidingEnemyControllers;
        float attackRange = PlayerManager.instance.player.playerStats.Range;

        EnemyController selectedTarget = null;
        float closestToMouse = Mathf.Infinity;

        foreach (EnemyController enemy in enemiesInRange)
        {
            if (enemy == null) continue;

            float distToPlayer = Vector3.Distance(playerPos, enemy.transform.position);
            if (distToPlayer > attackRange) continue;

            float distToMouse = Vector3.Distance(enemy.transform.position, mouseTargetPoint);
            if (distToMouse < closestToMouse)
            {
                closestToMouse = distToMouse;
                selectedTarget = enemy;
            }
        }

        // Ziel-Indikator setzen
        if (selectedTarget != null && !selectedTarget.mobStats.isDead)
        {
            targetIndicator.SetActive(true);
            Vector3 indicatorPos = selectedTarget.transform.position;
            indicatorPos.y += 0.1f; // Optional: leicht über Boden
            targetIndicator.transform.position = indicatorPos;
        }
        else
        {
            targetIndicator.SetActive(false);
        }

        // Ziel im Kampfsystem setzen
        var combat = PlayerManager.instance.player.GetComponent<CharacterCombat>();
        combat.SetCurrentTarget(selectedTarget);
    }
}