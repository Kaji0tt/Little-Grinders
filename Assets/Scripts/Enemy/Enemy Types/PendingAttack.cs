using UnityEngine;

public class PendingAttack : IAttackBehavior
{
    public float pauseTime;
    public float timer;

    public void Enter(EnemyController controller)
    {
        timer = pauseTime;
        Debug.Log($"[PendingAttack] Enter: Timer gesetzt auf {timer}");
        controller.StopMoving();
    }

    public void Update(EnemyController controller)
    {
        timer -= Time.deltaTime;
        Debug.Log($"[PendingAttack] Update: Timer = {timer}");

        if (timer <= 0)
        {
            Debug.Log("[PendingAttack] Timer abgelaufen, MoveToPlayer wird aufgerufen.");
            controller.MoveToPlayer();

            if (controller.IsPlayerInAttackRange())
            {
                Debug.Log("[PendingAttack] Spieler in Angriffsreichweite, Angriff wird ausgeführt.");
                controller.PerformAttack();
                timer = pauseTime; // Optional: für wiederholte Angriffe
                Debug.Log($"[PendingAttack] Timer nach Angriff zurückgesetzt auf {timer}");
            }
            else
            {
                Debug.Log("[PendingAttack] Spieler NICHT in Angriffsreichweite.");
            }
        }
    }

    public void Exit(EnemyController controller)
    {
        Debug.Log("[PendingAttack] Exit aufgerufen.");
    }

    public bool IsAttackReady(EnemyController controller)
    {
        bool ready = timer <= 0 && controller.IsPlayerInAttackRange();
        Debug.Log($"[PendingAttack] IsAttackReady: {ready}");
        return ready;
    }
}