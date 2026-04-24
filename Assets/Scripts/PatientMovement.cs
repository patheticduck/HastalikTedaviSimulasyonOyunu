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
    private bool walkingIn = true;
    private Vector3 currentTarget;

    void Update()
    {
        if (!canMove)
            return;

        Vector3 targetPosition = new Vector3(
            currentTarget.x,
            transform.position.y,
            currentTarget.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
            transform.forward = direction.normalized;

        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            canMove = false;
            hasArrived = true;

            if (walkingIn)
            {
                Debug.Log("Hasta hedef noktaya ulaştı.");

                if (gameFlowManager != null)
                    gameFlowManager.OnPatientArrived();
            }
            else
            {
                Debug.Log("Hasta odadan çıktı.");

                gameObject.SetActive(false);

                if (gameFlowManager != null)
                    gameFlowManager.OnPatientExited();
            }
        }
    }

    // 🔥 OYUN BAŞLARKEN ÇAĞRILACAK ANA FONKSİYON
    public void BeginWalk()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
            currentTarget = targetPoint.position;
        }

        walkingIn = true;
        hasArrived = false;
        canMove = true;
        gameObject.SetActive(true);

        Debug.Log("Hasta içeri yürümeye başladı.");
    }

    public void BeginExit()
    {
        if (startPoint == null)
        {
            HidePatient();

            if (gameFlowManager != null)
                gameFlowManager.OnPatientExited();

            return;
        }

        walkingIn = false;
        hasArrived = false;
        canMove = true;
        currentTarget = startPoint.position;

        Debug.Log("Hasta dışarı çıkmaya başladı.");
    }

    public void HidePatient()
    {
        canMove = false;
        hasArrived = false;
        gameObject.SetActive(false);
    }
    public void StopMovementImmediately()
{
    canMove = false;
    hasArrived = false;
}
}