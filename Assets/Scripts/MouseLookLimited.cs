using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookLimited : MonoBehaviour
{
    [Header("References")]
    public Transform cameraHolder;

    [Header("Sensitivity")]
    public float mouseSensitivity = 0.05f;

    [Header("Horizontal Limits")]
    public float horizontalLimit = 80f;

    [Header("Vertical Limits")]
    public float minPitch = -20f;
    public float maxPitch = 20f;

    [Header("State")]
    public bool startEnabled = false;

    private float yawOffset;
    private float pitch;
    private float baseYaw;
    private bool canLook;

    private void Start()
    {
        baseYaw = NormalizeAngle(transform.localEulerAngles.y);

        if (cameraHolder != null)
            pitch = NormalizeAngle(cameraHolder.localEulerAngles.x);

        SetLookEnabled(startEnabled);
        ApplyRotation();
    }

    private void Update()
    {
        if (!canLook)
            return;

        if (Mouse.current == null)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        yawOffset += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;

        yawOffset = Mathf.Clamp(yawOffset, -horizontalLimit, horizontalLimit);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, baseYaw + yawOffset, 0f);

        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void SetLookEnabled(bool enabled)
    {
        canLook = enabled;

        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}