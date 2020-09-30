using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Touchable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AddTouchingEvent addTouchingEvent = new AddTouchingEvent();
    public RemoveTouchingEvent removeTouchingEvent = new RemoveTouchingEvent();
    public int touchID = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        //touchID = Input.touchCount - 1;
        if (addTouchingEvent != null)
        {
            addTouchingEvent.Invoke(this);

        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (removeTouchingEvent != null)
        {
            
            removeTouchingEvent.Invoke(this);

        }
        //touchID = -1;
    }


    //private void Update()
    //{

    //    if (touchID >= 0)
    //    {
    //       transform.position = Input.GetTouch(touchID).position;
    //    }
    //}

    public void UpdatePostion() {
        if (touchID >= 0)
        {
            transform.position = Input.GetTouch(touchID).position;
        }
    }


}

public class AddTouchingEvent: UnityEvent<Touchable> {

}

public class RemoveTouchingEvent : UnityEvent<Touchable>
{

}

