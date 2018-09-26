// Adds window like behaviour to UI panels, so that they can be moved and closed
// by the user.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class InvUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    // cache
    Transform window;

    void Awake()
    {
        // cache the parent window
        window = transform.parent;
    }

    public void HandleDrag(PointerEventData d)
    {
        // send message in case the parent needs to know about it
        window.SendMessage("OnWindowDrag", d, SendMessageOptions.DontRequireReceiver);

        // move the parent
        window.Translate(d.delta);
    }

    public void OnBeginDrag(PointerEventData d)
    {
        HandleDrag(d);
    }

    public void OnDrag(PointerEventData d)
    {
        HandleDrag(d);
    }

    public void OnEndDrag(PointerEventData d)
    {
        HandleDrag(d);
    }


}
