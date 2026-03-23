using UnityEngine;

public class PatientMovement : MonoBehaviour
{
    public Transform targetPoint;
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f;

    private bool hasArrived = false;
    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager bulunamadı.");
        }
    }

    void Update()
    {
        if (hasArrived || targetPoint == null)
            return;

        Vector3 targetPosition = new Vector3(
            targetPoint.position.x,
            transform.position.y,
            targetPoint.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            hasArrived = true;
            Debug.Log("Hasta hedef noktaya ulaştı.");

            if (dialogueManager != null)
            {
                Debug.Log("Diyalog başlatılıyor.");
                dialogueManager.StartDialogue();
            }
        }
    }
}