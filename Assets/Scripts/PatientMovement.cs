using UnityEngine;

public class PatientMovement : MonoBehaviour
{
    [Header("Movement")]
    public Transform startPoint;
    public Transform targetPoint;
    public float moveSpeed = 2f;
    public float stopDistance = 1f;

    [Header("Flow")]
    public GameFlowManager gameFlowManager;

    private Animator animator;
    private bool canMove = false;
    private bool walkingIn = true;
    private Vector3 currentTarget;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

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

        if (direction.sqrMagnitude > 0.001f)
            transform.forward = direction.normalized;

        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            ArriveAtTarget();
        }
    }

    private void ArriveAtTarget()
    {
        Debug.Log("BURAYA GİRDİ: Hasta hedefe ulaştı.");

        canMove = false;
        PlayIdle();

        if (walkingIn)
        {
            if (targetPoint != null)
                transform.rotation = targetPoint.rotation;

            if (gameFlowManager != null)
                gameFlowManager.OnPatientArrived();
        }
        else
        {
            gameObject.SetActive(false);

            if (gameFlowManager != null)
                gameFlowManager.OnPatientExited();
        }
    }

    public void BeginWalk()
    {
        gameObject.SetActive(true);

        animator = GetComponentInChildren<Animator>();

        if (startPoint != null && targetPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
            currentTarget = targetPoint.position;
        }

        walkingIn = true;
        canMove = true;

        PlayWalk();

        Debug.Log("Hasta içeri yürümeye başladı.");
    }

    public void BeginExit()
    {
        animator = GetComponentInChildren<Animator>();

        if (startPoint == null)
        {
            HidePatient();

            if (gameFlowManager != null)
                gameFlowManager.OnPatientExited();

            return;
        }

        walkingIn = false;
        canMove = true;
        currentTarget = startPoint.position;

        PlayWalk();

        Debug.Log("Hasta dışarı çıkmaya başladı.");
    }

    public void HidePatient()
    {
        canMove = false;
        PlayIdle();
        gameObject.SetActive(false);
    }

    public void StopMovementImmediately()
    {
        canMove = false;
        PlayIdle();
    }

    private void PlayWalk()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.Play("walk", 0, 0f);
            Debug.Log("WALK: isWalking TRUE");
        }
        else
        {
            Debug.LogWarning("Animator bulunamadı!");
        }
    }

    private void PlayIdle()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.Play("Idle 0", 0, 0f);
            Debug.Log("IDLE: isWalking FALSE");
        }
        else
        {
            Debug.LogWarning("Animator bulunamadı!");
        }
    }
}