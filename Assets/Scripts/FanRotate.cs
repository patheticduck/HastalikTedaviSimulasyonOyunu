using UnityEngine;

public class FanRotate : MonoBehaviour
{
    public float speed = 500f;

    void Update()
    {
        transform.localRotation *= Quaternion.Euler(0f, speed * Time.deltaTime, 0f);
    }
}