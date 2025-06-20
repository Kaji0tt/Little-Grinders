using UnityEngine;

public class TargetSystem : MonoBehaviour
{
    public float maxMouseDistance = 1.0f;

    private void Update()
    {
        var playerPos = PlayerManager.instance.player.transform.position;
        var mouseTargetPoint = DirectionCollider.instance.transform.position;
        var enemiesInRange = DirectionCollider.instance.collidingEnemyControllers;
        float attackRange = PlayerManager.instance.player.playerStats.Range;

        EnemyController selectedTarget = null;
        float closestToMouse = Mathf.Infinity;

        Debug.Log(enemiesInRange.Count);

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

        // Zielindikator aktualisieren
        foreach (EnemyController enemy in enemiesInRange)
        {
            if (enemy == null) continue;
            enemy.SetTargetIndicatorActive(enemy == selectedTarget);
        }

        // 👉 Hier: Ziel im Kampfsystem setzen
        var combat = PlayerManager.instance.player.GetComponent<CharacterCombat>();
        combat.SetCurrentTarget(selectedTarget);
    }
}