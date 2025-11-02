using UnityEngine;
using UnityEngine.Events;

public class InteractiveButton : MonoBehaviour
{
    public GameObject buttonTop;
    public float pressDistance = 0.1f;
    public UnityEvent OnClick;

    private Vector3 originalPosition;
    private bool isPressed = false;

    void Start()
    {
        if (buttonTop != null)
        {
            originalPosition = buttonTop.transform.localPosition;
            Debug.Log("ButtonTop reference: " + buttonTop.name);
        }
        else
        {
            Debug.LogError("BUTTONTOP REFERENCE IS NULL!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckForClick();
        }

        if (Input.GetMouseButtonUp(0) && isPressed)
        {
            ReleaseButton();
        }
    }

    void CheckForClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.gameObject == buttonTop)
            {
                if (!isPressed)
                {
                    PressButton();
                }
            }
        }
    }

    void PressButton()
    {
        Debug.Log("=== PRESS BUTTON CALLED ===");

        isPressed = true;

        // Check/ move button
        if (buttonTop != null)
        {
            buttonTop.transform.localPosition = originalPosition - new Vector3(0, pressDistance, 0);
            Debug.Log("Button should be moving. New position: " + buttonTop.transform.localPosition);
        }
        else
        {
            Debug.LogError("Cannot move - buttonTop is null!");
        }

        // Check/ start event
        if (OnClick != null)
        {
            Debug.Log("OnClick event exists with " + OnClick.GetPersistentEventCount() + " listeners");
            OnClick.Invoke();
        }
        else
        {
            Debug.LogError("OnClick event is null!");
        }
    }

    void ReleaseButton()
    {
        Debug.Log("Releasing button");
        isPressed = false;

        if (buttonTop != null)
        {
            buttonTop.transform.localPosition = originalPosition;
        }
    }
}