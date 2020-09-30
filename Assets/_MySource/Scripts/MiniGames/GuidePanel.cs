using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;

namespace  Casino.Games.BxB {
	public class GuidePanel : UIPanel {
		public GameObject page;
		public ScrollRect scrollRect;
		// Use image for guide :).
		private bool hasShowed = true;

		public override void Show ()
		{
			scrollRect.verticalNormalizedPosition = 1f;
			if (!hasShowed) {
				for (int i = 0; i < panelLines.Length; i++) {
					UILine l = GameObject.Instantiate (linePrefab) as UILine;
					l.DrawLine (panelLines [i]);
					l.transform.parent = page.transform;
					l.gameObject.SetActive (true);
				}
				hasShowed = true;
			}

		}

	}

	public class GuideLineData : PanelLineData {
		public int iconIdx;
		public int iconNum;
		public string award;
		public GuideLineData(int iconIdx, int iconNum, string award) {
			this.iconIdx = iconIdx;
			this.iconNum = iconNum;
			this.award = award;
		}
	}

}

