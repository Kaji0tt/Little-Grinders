using System.Collections.Generic;
using UnityEngine;

public static class MovementHelper
{
    private static readonly Vector3[] directions = new Vector3[]
    {
        new Vector3(1, 0, 0), new Vector3(1, 0, 1).normalized, new Vector3(0, 0, 1),
        new Vector3(-1, 0, 1).normalized, new Vector3(-1, 0, 0), new Vector3(-1, 0, -1).normalized,
        new Vector3(0, 0, -1), new Vector3(1, 0, -1).normalized
    };

    public static Vector3 GetWeightedDirection(Vector3 preferredDir)
    {
        preferredDir.y = 0f;
        preferredDir.Normalize();

        List<float> weights = new List<float>();
        float sum = 0f;

        foreach (var dir in directions)
        {
            float dot = Vector3.Dot(preferredDir, dir);
            float weight = Mathf.Max(0, dot);
            weight = Mathf.Pow(weight, 3); // optional justierbar
            weights.Add(weight);
            sum += weight;
        }

        for (int i = 0; i < weights.Count; i++) weights[i] /= sum;

        float rand = Random.value, cumulative = 0f;
        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative) return directions[i];
        }

        return directions[0];
    }
}