using UnityEngine;

public class Enigma : Ability
{
    public override void UseBase(IEntitie entitie)
    {
        Transform playerTransform = entitie.GetTransform();
        if (playerTransform == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int floorLayerMask = LayerMask.GetMask("Floor");

        if (floorLayerMask == 0)
        {
            Debug.LogError("Die LayerMask für 'Floor' konnte nicht erstellt werden. Überprüfe den Layernamen auf Tippfehler!");
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayerMask))
        {
            float distanceToTarget = Vector3.Distance(playerTransform.position, hit.point);

            if (distanceToTarget <= range)
            {
                // Fall 1: Ziel ist innerhalb der Reichweite. Teleportiere direkt zum angeklickten Punkt.
                playerTransform.position = hit.point;
            }
            else
            {
                // Fall 2: Ziel ist außerhalb der Reichweite. Teleportiere zur maximalen Distanz in diese Richtung.

                // Berechne die Richtung vom Spieler zum angeklickten Punkt.
                // .normalized macht den Vektor genau 1 Einheit lang.
                Vector3 direction = (hit.point - playerTransform.position).normalized;

                // Berechne die neue Zielposition: Startpunkt + Richtung * maximale Reichweite.
                Vector3 targetPosition = playerTransform.position + direction * range;

                // Teleportiere den Spieler an diese Position.
                playerTransform.position = targetPosition;
            }
        }
        else
        {
            Debug.Log("Enigma Raycast hat nichts auf dem 'Floor'-Layer getroffen.");
        }
    }

    // Diese Methoden werden für eine sofortige Fähigkeit nicht benötigt, müssen aber existieren.
    public override void OnTick(IEntitie entitie) { }
    public override void OnCooldown(IEntitie entitie) { }
}
