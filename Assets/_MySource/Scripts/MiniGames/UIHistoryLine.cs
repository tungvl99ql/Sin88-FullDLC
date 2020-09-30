using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Casino.Core;


namespace Casino.Games.BxB {
	
	public class UIHistoryLine : UILine {
		
		public Text accountTxt;
		public Text timeStampTxt;
		public Text roomTxt;
		public Text winTxt;
		public Text bigWinTxt;

		void OnDisable() {
			Destroy (gameObject);
		}

		public override void DrawLine (PanelLineData lineData)
		{
			HistoryLineData _lineData = (HistoryLineData)lineData;
			accountTxt.text = _lineData.account;
			timeStampTxt.text = _lineData.timeStamp;
			roomTxt.text = _lineData.room;
			winTxt.text = _lineData.win.ToString ();
			bigWinTxt.text = _lineData.bigWin.ToString ();			
		}
				
		
	}

	public class HistoryLineData : PanelLineData {
		public string account;
		public string timeStamp;
		public string room;
		public string win;
		public string bigWin;
		
		public HistoryLineData(string account, string timeStamp, string room, string win, string bigWin) {
			this.account = account;
			this.timeStamp = timeStamp;
			this.room = room;
			this.win = win;
			this.bigWin = bigWin;
		}
	}
}
