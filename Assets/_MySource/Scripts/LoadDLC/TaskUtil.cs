using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskUtil
{
    public static Coroutine Delay(MonoBehaviour mon, Action action,float time)
    {
        return mon.StartCoroutine(IEDelay(time, action));
    }

    public static IEnumerator IEDelay(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        if (action != null)
        {
            action.Invoke();
        }
    }
}
