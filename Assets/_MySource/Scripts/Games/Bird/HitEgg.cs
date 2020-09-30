using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Core.Bird
{
    public class HitEgg : MonoBehaviour
    {
        public UnityEvent OpenHitEggEvent;
        public UnityEvent CloseHitEggEvent;

        void OnEnable()
        {
            if (OpenHitEggEvent != null && CloseHitEggEvent != null)
            {
                OpenHitEggEvent.Invoke();
                CloseHitEggEvent.Invoke();
            }
        }
    }
}