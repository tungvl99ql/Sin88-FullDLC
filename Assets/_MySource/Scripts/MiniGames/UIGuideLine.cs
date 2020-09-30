using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Casino.Core;

namespace Casino.Games.BxB {
	public class UIGuideLine : UILine {
		
		public Image[] iconImgs;
		public Text awardTxt;
		public Sprite[] refIconList;

		public override void DrawLine (PanelLineData lineData)
		{
			GuideLineData _lineData = (GuideLineData)lineData;
			for (int i = 0; i < _lineData.iconNum; i++) {
				iconImgs [i].sprite = refIconList [_lineData.iconIdx - 1];
				iconImgs [i].gameObject.SetActive (true);
			}
			awardTxt.text =  _lineData.award;
		}
		
	}
}

