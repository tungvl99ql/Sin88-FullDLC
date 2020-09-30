using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;

namespace Casino.Games.BxB {
	public class UIGloryLine : UILine {

		public Text accountTxt;
		public Text timeStampTxt;
		public Text betLevel;
		public Text winTxt;


		public override void DrawLine (PanelLineData lineData)
		{
			GloryLineData _lineData = (GloryLineData)lineData;
			accountTxt.text = _lineData.account;
			timeStampTxt.text = _lineData.timeStamp;
			betLevel.text = _lineData.betLevel;
			winTxt.text = _lineData.win.ToString ();	
		}

		void OnDisable() {
			Destroy (gameObject);
		}

	}
}

