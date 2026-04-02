using UnityEngine;

public class PatientMovement : MonoBehaviour
{
    [Header("Movement")]
    public Transform startPoint;
    public Transform targetPoint;
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f;

    [Header("Flow")]
    public GameFlowManager gameFlowManager;

    private bool hasArrived = false;
    private bool canMove = false;

    void Update()
    {
        if (!canMove || hasArrived || targetPoint == null)
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
            canMove = false;

            Debug.Log("Hasta hedef noktaya ulaştı.");

            if (gameFlowManager != null)
            {
                Debug.Log("GameFlowManager'a haber veriliyor: hasta geldi.");
                gameFlowManager.OnPatientArrived();
            }
            else
            {
                Debug.LogWarning("PatientMovement: gameFlowManager bağlı değil.");
            }
        }
    }

    public void BeginWalk()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
        }

        hasArrived = false;
        canMove = true;

        gameObject.SetActive(true);

        Debug.Log("Hasta yürümeye başladı.");
    }

    public void HidePatient()
    {
        canMove = false;
        hasArrived = false;
        gameObject.SetActive(false);
    }
}