using UnityEngine;

public class ClickDebug : MonoBehaviour
{
    void OnMouseEnter()
    {
        Debug.Log("Mouse ENTERED: " + gameObject.name);
    }

    void OnMouseExit()
    {
        Debug.Log("Mouse EXITED: " + gameObject.name);
    }

    void OnMouseDown()
    {
        Debug.Log("Mouse DOWN: " + gameObject.name);
    }

    void OnMouseUp()
    {
        Debug.Log("Mouse UP: " + gameObject.name);
    }

    void OnMouseOver()
    {
        Debug.Log("Mouse OVER: " + gameObject.name);
    }
}