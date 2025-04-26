using UnityEngine;
using UnityEditor;

public class SectorVisualizer : MonoBehaviour
{
    public float radius = 5f; // Größe des Kreises

    private void OnDrawGizmos()
    {
        TalentType[] types = (TalentType[])System.Enum.GetValues(typeof(TalentType));
        int typeCount = types.Length;
        float sectorSize = 360f / typeCount; // 60° pro Sektor

        Vector3 center = transform.position;

        // Farben pro TalentType für bessere Sichtbarkeit
        Color[] sectorColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan };

        for (int i = 0; i < typeCount; i++)
        {
            float angleStart = i * sectorSize;
            float angleEnd = (i + 1) * sectorSize;

            // Berechne zwei Eckpunkte des Sektors
            Vector3 startPos = center + Quaternion.Euler(0, 0, -angleStart) * Vector3.right * radius;
            Vector3 endPos = center + Quaternion.Euler(0, 0, -angleEnd) * Vector3.right * radius;

            // Zeichne Linien für die Sektoren
            Gizmos.color = sectorColors[i];
            Gizmos.DrawLine(center, startPos);
            Gizmos.DrawLine(center, endPos);

            // Berechne die Position für das Label (Mitte des Sektors)
            float midAngle = angleStart + (sectorSize / 2);
            Vector3 labelPos = center + Quaternion.Euler(0, 0, -midAngle) * Vector3.right * (radius * 1.2f);

            // Zeichne das Label
            Handles.Label(labelPos, types[i].ToString());
        }

        // Kreis zeichnen (Begrenzung)
        Handles.color = Color.white;
        Handles.DrawWireArc(center, Vector3.forward, Vector3.right, 360, radius);
    }
}