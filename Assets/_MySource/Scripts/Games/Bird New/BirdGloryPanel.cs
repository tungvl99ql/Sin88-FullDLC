using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;

namespace  Casino.Games.BxB {
	
	public class BirdGloryPanel : UIPanel {

		public GameObject page;
        public GameObject page2;
		public RectTransform virtualMash;
        public CustomToggleGroup toggleGroup;
		public ScrollRect scrollRect;

        public override void Show()
        {
            toggleGroup.OnChangeSelect = changeTab;
            for (int i = 0; i < panelLines.Length; i++)
            {
              //  Debug.Log(((GloryLineData)panelLines[i]).isPotWin +" "+ ((GloryLineData)panelLines[i]).isBigWin);

                if (((BirdGloryLineData)panelLines[i]).isPotWin && ((BirdGloryLineData)panelLines[i]).isBigWin == false)
                {
                    UILine l = GameObject.Instantiate(linePrefab) as UILine;
                    l.DrawLine(panelLines[i]);
                    l.transform.parent = page.transform;
                    l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    l.gameObject.SetActive(true);
                }
                else if (((GloryLineData)panelLines[i]).isBigWin==true)
                {
                    UILine l = GameObject.Instantiate(linePrefab) as UILine;
                    l.DrawLine(panelLines[i]);
                    l.transform.parent = page2.transform;
                    l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    l.gameObject.SetActive(true);
                }

            }
         //   scrollRect.verticalNormalizedPosition = 1f;

          StartCoroutine(wait());
   
        }


    IEnumerator wait()
    {

        yield return new WaitForSeconds(0.1f);
            scrollRect.verticalNormalizedPosition = 1f;

    }
    private void changeTab(int tabIdx)
        {
            switch (tabIdx)
            {

                case 0:
                    // TODO: try to set as last sibbling.
                    page.transform.SetAsLastSibling();
                    virtualMash.SetSiblingIndex(1);
                    page2.SetActive(false);
                    page.SetActive(true);
                    for (int i = 0; i < panelLines.Length; i++)
                    {

                        if (((GloryLineData)panelLines[i]).isPotWin && ((GloryLineData)panelLines[i]).isBigWin == false)
                        {
                            UILine l = GameObject.Instantiate(linePrefab) as UILine;
                            l.DrawLine(panelLines[i]);
                            l.transform.parent = page.transform;
                            l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                            l.gameObject.SetActive(true);
                        }

                    }
                    scrollRect.verticalNormalizedPosition = 1f;
                    break;

                case 1:
                    page2.transform.SetAsLastSibling();
                    virtualMash.SetSiblingIndex(1);
                    page2.SetActive(true);
                    page.SetActive(false);

                    for (int i = 0; i < panelLines.Length; i++)
                    {

                        if (((GloryLineData)panelLines[i]).isBigWin == true)
                        {
                            UILine l = GameObject.Instantiate(linePrefab) as UILine;
                            l.DrawLine(panelLines[i]);
                            l.transform.parent = page2.transform;
                            l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                            l.gameObject.SetActive(true);
                        }

                    }
                    StartCoroutine(wait());
                    break;
                default:
                    break;
            }

        }

		public override void Close ()
		{
            toggleGroup.OnChangeSelect -= changeTab;
			base.Close ();
		}


	}

	public class GloryLineData : PanelLineData {
		public string account;
		public int idx;
		public string timeStamp;
		public string betLevel;
		public string win;
		public bool isBigWin;
		public bool isPotWin;

		public GloryLineData(string account, string timeStamp, string betLevel, string win,  bool isPotWin, bool isBigWin) {
			this.account = account;
			this.timeStamp = timeStamp;
			this.betLevel = betLevel;
			this.win = win;
			this.isBigWin = isBigWin;
			this.isPotWin = isPotWin;
		}
	}
}
