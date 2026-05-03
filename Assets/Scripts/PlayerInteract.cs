using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 20f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                Debug.Log("Vurulan obje: " + hit.collider.name);

                WindowClickToggle window = hit.collider.GetComponentInParent<WindowClickToggle>();

                if (window != null)
                    window.ToggleWindow();
            }
            else
            {
                Debug.Log("Hiçbir şeye vurmadı.");
            }
        }
    }
}