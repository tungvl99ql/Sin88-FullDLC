using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;

namespace  Casino.Games.BxB {
    public class HistoryPanel : UIPanel
    {

        public GameObject page;
        public ScrollRect scrollRect;

        public override void Show()
        {

            for (int i = 0; i < panelLines.Length; i++)
            {
                UILine l = GameObject.Instantiate(linePrefab) as UILine;
                l.DrawLine(panelLines[i]);
                l.transform.parent = page.transform;
                l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                l.gameObject.SetActive(true);
            }


            StartCoroutine(wait());
        }
        IEnumerator wait()
        {

            yield return new WaitForSeconds(0.1f);
            scrollRect.verticalNormalizedPosition = 1f;

        }

    }

	public class HistorLineData : PanelLineData {
		public string account;
		public int idx;
		public string timeStamp;
		public string room;
		public string win;
		public string bigWin;

		public HistorLineData(string account, string timeStamp, string room, string win, string bigWin) {
			this.account = account;
			this.timeStamp = timeStamp;
			this.room = room;
			this.win = win;
			this.bigWin = bigWin;
		}
	}
}

