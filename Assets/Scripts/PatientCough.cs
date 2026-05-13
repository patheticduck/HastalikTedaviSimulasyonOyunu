using UnityEngine;

public class PatientCough : MonoBehaviour
{
    [Header("Cough Settings")]
    public Transform targetPoint;
    public float triggerDistance = 0.5f;

    private AudioSource audioSource;
    private Animator animator;
    private bool hasCoughed = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Animator bu objede yoksa child objelerde arar
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (hasCoughed || targetPoint == null)
            return;

        Vector3 patientPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 targetPos = new Vector3(targetPoint.position.x, 0f, targetPoint.position.z);

        float distance = Vector3.Distance(patientPos, targetPos);

        if (distance <= triggerDistance)
        {
            hasCoughed = true;

            if (audioSource != null)
            {
                audioSource.Play();
            }

            if (animator != null)
            {
                animator.ResetTrigger("Cough");
                animator.SetTrigger("Cough");
            }
        }
    }
}