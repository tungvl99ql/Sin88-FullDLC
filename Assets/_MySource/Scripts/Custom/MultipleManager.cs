using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleManager : LayerManager {

    public List<Touchable> draggableMiniGameList;
    public List<Touchable> touchingGO = new List<Touchable>();

    private void Start()
    {
        foreach (var item in draggableMiniGameList)
        {
            item.addTouchingEvent.AddListener(AddTouchingGO);
            item.removeTouchingEvent.AddListener(RemoveTouchingGO);
        }
    }

    private void OnDestroy()
    {
        foreach (var item in draggableMiniGameList)
        {
            item.addTouchingEvent.RemoveListener(AddTouchingGO);
            item.removeTouchingEvent.RemoveListener(RemoveTouchingGO);
        }
    }



    public void AddTouchingGO(Touchable touching) {
        resetID();
        touchingGO.Add(touching);
        UpdateTouchingID();
    }

    public void RemoveTouchingGO(Touchable touching) {
        touchingGO.Remove(touching);
        //touching.touchID = -1;
        resetID();
        UpdateTouchingID();
    }

    private void resetID() {
        foreach (var item in draggableMiniGameList)
        {
            item.touchID = -1;
        }
    }


    private void UpdateTouchingID() {
        var touchingCount = touchingGO.Count;
        for (int i = 0; i < touchingCount; i++)
        {
            touchingGO[i].touchID = i;
        }
    }

    private void Update()
    {
        var touchingCount = touchingGO.Count;
        for (int i = 0; i < touchingCount; i++)
        {
            touchingGO[i].UpdatePostion();
        }
    }

}
