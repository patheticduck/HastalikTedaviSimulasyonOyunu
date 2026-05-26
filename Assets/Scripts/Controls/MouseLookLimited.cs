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

    // =====================================================
    // BAŞLANGIÇ ROTASYON VE DURUM AYARLARI
    // =====================================================

    private void Start()
    {
        // Başlangıç yatay rotasyonu kaydedilir.
        baseYaw = NormalizeAngle(transform.localEulerAngles.y);

        // Kamera holder başlangıç pitch değeri alınır.
        if (cameraHolder != null)
        {
            pitch =
                NormalizeAngle(cameraHolder.localEulerAngles.x);
        }

        SetLookEnabled(startEnabled);

        ApplyRotation();
    }

    // =====================================================
    // MOUSE LOOK INPUT KONTROLLERİ
    // =====================================================

    private void Update()
    {
        if (!canLook)
            return;

        if (Mouse.current == null)
            return;

        Vector2 delta =
            Mouse.current.delta.ReadValue();

        // Mouse hareketine göre yaw ve pitch hesaplanır.
        yawOffset += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;

        // Kamera limitleri uygulanır.
        yawOffset = Mathf.Clamp(
            yawOffset,
            -horizontalLimit,
            horizontalLimit
        );

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        ApplyRotation();
    }

    // =====================================================
    // KARAKTER VE KAMERA ROTASYONUNU UYGULAMA
    // =====================================================

    private void ApplyRotation()
    {
        // Karakter yatay rotasyonu uygulanır.
        transform.localRotation =
            Quaternion.Euler(
                0f,
                baseYaw + yawOffset,
                0f
            );

        // Kamera dikey rotasyonu uygulanır.
        if (cameraHolder != null)
        {
            cameraHolder.localRotation =
                Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    // =====================================================
    // MOUSE LOOK AKTİF / PASİF DURUMU
    // =====================================================

    public void SetLookEnabled(bool enabled)
    {
        canLook = enabled;

        // Gameplay modunda cursor kilitlenir.
        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // UI modunda cursor serbest bırakılır.
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // =====================================================
    // AÇIYI -180 / 180 ARALIĞINA NORMALLEŞTİRME
    // =====================================================

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }
}