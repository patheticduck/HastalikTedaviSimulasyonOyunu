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

    [Header("Patient Visuals")]
    public GameObject[] patientVisuals;
    private int currentPatientIndex = 0;

    private Animator animator;
    private bool canMove = false;
    private bool walkingIn = true;
    private Vector3 currentTarget;

    void OnEnable()
    {
        SetPatientVisual(currentPatientIndex);
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
        Debug.Log("Hasta hedefe ulaştı.");

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
            currentPatientIndex++;

            gameObject.SetActive(false);

            if (gameFlowManager != null)
                gameFlowManager.OnPatientExited();
        }
    }

    public void BeginWalk()
    {
        if (patientVisuals == null || patientVisuals.Length == 0)
        {
            Debug.LogWarning("Patient Visuals boş!");
            return;
        }

        gameObject.SetActive(true);

        SetPatientVisual(currentPatientIndex);

        if (startPoint != null && targetPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;
            currentTarget = targetPoint.position;
        }

        walkingIn = true;
        canMove = true;

        PlayWalk();

        Debug.Log("Hasta içeri yürümeye başladı: " + GetCurrentPatientName());
    }

    public void BeginExit()
    {
        if (patientVisuals == null || patientVisuals.Length == 0)
        {
            Debug.LogWarning("Patient Visuals boş!");
            return;
        }

        SetPatientVisual(currentPatientIndex);

        if (startPoint == null)
        {
            HidePatient();

            currentPatientIndex++;

            if (gameFlowManager != null)
                gameFlowManager.OnPatientExited();

            return;
        }

        walkingIn = false;
        canMove = true;
        currentTarget = startPoint.position;

        PlayWalk();

        Debug.Log("Hasta dışarı çıkmaya başladı: " + GetCurrentPatientName());
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

    public void SetPatientVisual(int index)
    {
        if (patientVisuals == null || patientVisuals.Length == 0)
        {
            animator = GetComponentInChildren<Animator>();
            return;
        }

        int visualIndex = Mathf.Abs(index) % patientVisuals.Length;

        for (int i = 0; i < patientVisuals.Length; i++)
        {
            if (patientVisuals[i] != null)
                patientVisuals[i].SetActive(i == visualIndex);
        }

        animator = GetComponentInChildren<Animator>();
    }

    private string GetCurrentPatientName()
    {
        if (patientVisuals == null || patientVisuals.Length == 0)
            return "Hasta yok";

        int visualIndex = Mathf.Abs(currentPatientIndex) % patientVisuals.Length;

        if (patientVisuals[visualIndex] == null)
            return "Boş hasta elementi";

        return patientVisuals[visualIndex].name;
    }

    private void PlayWalk()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.Play("walk", 0, 0f);
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
        }
        else
        {
            Debug.LogWarning("Animator bulunamadı!");
        }
    }
}