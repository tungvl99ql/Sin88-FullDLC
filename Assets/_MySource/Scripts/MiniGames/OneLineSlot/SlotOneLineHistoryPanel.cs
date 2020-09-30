using Casino.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Casino.Games.OneLineSlot {
    public class SlotOneLineHistoryPanel : UIPanel
    {
        public GameObject line;
        public ScrollRect scr;
        public override void Show()
        {
          //  scr.verticalNormalizedPosition = 1;
            for (int i = 0; i < panelLines.Length; i++)
            {
                UILine l = GameObject.Instantiate(linePrefab) as UILine;
                l.DrawLine(panelLines[i]);
                l.transform.parent = line.transform;
                l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                l.gameObject.SetActive(true);
            }
            StartCoroutine(wait());
        }

        IEnumerator wait()
        {

            yield return new WaitForSeconds(0.1f);
            scr.verticalNormalizedPosition = 1f;

        }
    }
    public class HistoryRowData : PanelLineData
    {
        public string maPhien;
        public string thoiGian;
        public string mucCuoc;
        public string phatSinh;
        public string soDu;

        public HistoryRowData(string maPhien, string thoiGian, string mucCuoc, string phatSinh, string soDu)
        {
            this.maPhien = maPhien;
            this.thoiGian = thoiGian;
            this.mucCuoc = mucCuoc;
            this.phatSinh = phatSinh;
            this.soDu = soDu;
        }
    }

}
