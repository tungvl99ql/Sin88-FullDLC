using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Casino.Core {

    public class LineToggle : Toggle {
     
        public ChangeValueEvent changeValueEvent = new ChangeValueEvent();
        public int id;

		protected override void Start()
		{
            onValueChanged.AddListener(OnValueChangedHandler);
		}

        private void OnValueChangedHandler(bool _isOn){
            if (changeValueEvent != null)
            {
                changeValueEvent.Invoke(id, _isOn);
            }
        }

	}

    public class ChangeValueEvent : UnityEvent<int, bool> {
        
    }

}

