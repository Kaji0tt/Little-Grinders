
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed = 50f; // Grad pro Sekunde

    void Update()
    {
        transform.Rotate(new Vector3(0f,1f,0f), rotationSpeed);
    }
}
