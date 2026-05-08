using UnityEngine;

public class DoorAutoController : MonoBehaviour
{
    public Transform doorObject;
    public Vector3 closedRotation = Vector3.zero;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float speed = 10f;

    private Quaternion targetRotation;

    void Start()
    {
        CloseDoorInstant();
    }

    void Update()
    {
        if (doorObject == null)
            return;

        doorObject.localRotation = Quaternion.Lerp(
            doorObject.localRotation,
            targetRotation,
            Time.deltaTime * speed
        );
    }

    public void OpenDoor()
    {
        targetRotation = Quaternion.Euler(openRotation);
    }

    public void CloseDoor()
    {
        targetRotation = Quaternion.Euler(closedRotation);
    }

    public void CloseDoorInstant()
    {
        targetRotation = Quaternion.Euler(closedRotation);

        if (doorObject != null)
            doorObject.localRotation = targetRotation;
    }
}