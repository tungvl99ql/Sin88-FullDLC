using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// User interface panel.
/// the core panel that presents the linear data
/// </summary>
namespace Casino.Core {	
	public class UIPanel : MonoBehaviour {

		public PanelLineData[] panelLines {
			get;
			set;
		}

		public UILine linePrefab;

		void OnEnable() {
			Show ();
		}

		public virtual void Show() {
        }

		public virtual void Close() {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            gameObject.SetActive (false);
		}

	}

	public class PanelLineData {
		
	}
}