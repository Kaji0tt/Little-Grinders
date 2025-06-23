
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 90f;
    private float currentYRotation = 0f;

    void Update()
    {
        currentYRotation += speed * Time.deltaTime;
        currentYRotation %= 360f; // optional, um es schön sauber zu halten

        transform.localEulerAngles = new Vector3(90f, currentYRotation, 0f);
    }
}