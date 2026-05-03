using UnityEngine;

public class WindowClickToggle : MonoBehaviour
{
    public Animation windowAnimation;
    private bool isOpen = false;

    public void ToggleWindow()
    {
        if (isOpen)
            windowAnimation.Play("Close");
        else
            windowAnimation.Play("Open");

        isOpen = !isOpen;
    }

    void Start()
    {
        if (windowAnimation == null)
            windowAnimation = GetComponent<Animation>();
    }
}