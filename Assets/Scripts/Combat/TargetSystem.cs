using UnityEngine;
using System.Linq;

public class TargetSystem : MonoBehaviour
{
    [Header("Ziel-Parameter")]
    [Tooltip("Der Radius um den Mauszeiger, in dem nach Gegnern gesucht wird.")]
    public float targetingRadius = 1.5f;
    public GameObject targetIndicator;

    private CharacterCombat characterCombat;
    private int floorLayerMask; // NEU: LayerMask für den Boden

    private void Start()
    {
        characterCombat = PlayerManager.instance.player.GetComponent<CharacterCombat>();
        // NEU: Bereite die LayerMask vor.
        floorLayerMask = LayerMask.GetMask("Floor");

        if (targetIndicator != null)
        {
            targetIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        EnemyController selectedTarget = null;

        // --- Priorität 1: Gegner im Radius um den Mauszeiger ---
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Benutze die LayerMask, um sicherzustellen, dass der Raycast nur den Boden trifft.
        if (Physics.Raycast(ray, out RaycastHit floorHit, 200f, floorLayerMask))
        {
            // Die Prüfung auf den Tag ist jetzt redundant, aber schadet nicht.
            // if (floorHit.collider.CompareTag("Floor"))
            // {
                Vector3 mouseWorldPos = floorHit.point;
                
                // OverlapSphere sucht weiterhin nach allen Colliders, die wir dann nach Tag filtern.
                Collider[] collidersInRadius = Physics.OverlapSphere(mouseWorldPos, targetingRadius);

                EnemyController closestEnemyInRadius = null;
                float minDistance = Mathf.Infinity;

                foreach (var col in collidersInRadius)
                {
                    // Prüfe, ob der Collider ein Gegner ist
                    if (col.CompareTag("Enemy"))
                    {
                        EnemyController enemy = col.GetComponentInParent<EnemyController>();
                        if (enemy != null && !enemy.mobStats.isDead)
                        {
                            float distance = Vector3.Distance(mouseWorldPos, enemy.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestEnemyInRadius = enemy;
                            }
                        }
                    }
                }
                selectedTarget = closestEnemyInRadius;
        }
        

        // --- Priorität 2 (Fallback): Gegner im Nahbereich ---
        // Wenn im Maus-Radius kein Gegner gefunden wurde, nutze die Nahbereichslogik.
        if (selectedTarget == null)
        {
            var mouseTargetPoint = DirectionCollider.instance.transform.position;
            var enemiesInSwing = DirectionCollider.instance.collidingEnemyControllers;
            float closestToMouse = Mathf.Infinity;

            foreach (EnemyController enemy in enemiesInSwing)
            {
                if (enemy == null || enemy.mobStats.isDead) continue;

                float distToMouse = Vector3.Distance(enemy.transform.position, mouseTargetPoint);
                if (distToMouse < closestToMouse)
                {
                    closestToMouse = distToMouse;
                    selectedTarget = enemy;
                }
            }
        }

        // --- Finale Zuweisung und UI-Update ---
        characterCombat.SetCurrentTarget(selectedTarget);

        if (selectedTarget != null && !selectedTarget.mobStats.isDead)
        {
            targetIndicator.SetActive(true);
            targetIndicator.transform.position = selectedTarget.transform.position;
        }
        else
        {
            targetIndicator.SetActive(false);
        }
    }
}