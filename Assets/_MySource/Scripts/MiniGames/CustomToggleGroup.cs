using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Casino.Core {

	public delegate void OnChangeSelect(int toggleOnIdx);

	public class CustomToggleGroup : ToggleGroup {

		public OnChangeSelect OnChangeSelect;

		public Toggle[] toggles;


		void OnEnable() {

			OnChangeSelect += testChangeSelect;

			for (int i = 0; i < toggles.Length; i++) {
				toggles [i].onValueChanged.AddListener (onChangeValue);
			}
			//onChangeValue (true);

		}

		void OnDisable() {
			OnChangeSelect -= testChangeSelect;

			for (int i = 0; i < toggles.Length; i++) {
				toggles [i].onValueChanged.RemoveListener (onChangeValue);
			}

		}


		private void onChangeValue(bool isOn) {

			for (int i = 0; i < toggles.Length; i++) {
				if (toggles[i].isOn) {
					if (OnChangeSelect != null) {
						OnChangeSelect (i);
					}
				}
			}


		}

		private void testChangeSelect(int id) {
			//Debug.Log ("============================ :" + id.ToString());
		}
	}
}

