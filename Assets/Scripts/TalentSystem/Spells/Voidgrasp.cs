using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Voidgrasp : Ability
{
    private const float searchRadius = 0.5f;
    private const int maxTargets = 5;

    public override void UseBase(IEntitie player)
    {
        StartCoroutine(VoidgraspRoutine());
    }

    private IEnumerator VoidgraspRoutine()
    {
        // 1. Mausposition auf dem Boden bestimmen
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mouseWorldPos = Vector3.zero;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Floor")))
        {
            mouseWorldPos = hit.point;
        }
        else
        {
            Debug.Log("Voidgrasp: Kein gültiger Boden unter dem Cursor.");
            yield break;
        }

        // 2. Finde alle Gegner im Radius
        Collider[] hits = Physics.OverlapSphere(mouseWorldPos, searchRadius);
        var targets = new List<EnemyController>();

        foreach (var col in hits)
        {
            if (targets.Count >= maxTargets) break;
            if (col == null || col.gameObject == null) continue;
            if (!col.CompareTag("Enemy")) continue;

            EnemyController enemy = col.GetComponentInParent<EnemyController>();
            if (enemy == null) continue;
            if (enemy.mobStats != null && !enemy.mobStats.isDead)
            {
                targets.Add(enemy);
                enemy.gameObject.SetActive(false); // Gegner "entfernen"
            }
        }

        if (targets.Count == 0)
        {
            Debug.Log("Voidgrasp: Keine Gegner im Bereich gefunden.");
            yield break;
        }
        else
        {
            if (AudioManager.instance != null)
            {
                // Optional: Sound abspielen
                AudioManager.instance.PlaySound("Voidgrasp_Cast");
                AudioManager.instance.PlaySound("Voidgrasp_Use");
            }
        }

        // 3. Warte 1 Sekunde
        yield return new WaitForSeconds(1f);

        // 4. Neue Mausposition holen
        Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 newMouseWorldPos = mouseWorldPos;
        if (Physics.Raycast(ray2, out RaycastHit hit2, Mathf.Infinity, LayerMask.GetMask("Floor")))
        {
            newMouseWorldPos = hit2.point;
        }

        // 5. Spawne Gegner an neuer Position (leicht versetzt)
        float angleStep = 360f / targets.Count;
        float spawnRadius = 1.0f;
        for (int i = 0; i < targets.Count; i++)
        {
            var enemy = targets[i];
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnRadius;
            enemy.transform.position = newMouseWorldPos + offset;
            enemy.gameObject.SetActive(true);
        }

        // 6. Optional: VFX/Sound abspielen
        //Debug.Log("Voidgrasp: Gegner teleportiert!");
    }

    public override void OnTick(IEntitie entitie) { }
    public override void OnCooldown(IEntitie entitie) { }
    
    protected override void ApplyRarityScaling(float rarityScaling)
    {

        Debug.Log($"[Voidgrasp] rarityScaling angewendet: {rarityScaling}");

    }
}
