using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;
namespace Slot.Games.Fish
{
    public class FishGameHistoryPanel : UIPanel
    {
        public GameObject historyPerfab;
        public Transform historyParent;
        public override void Show()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            for (int i = historyParent.childCount - 1; i > 0; i--)
            {
                Destroy(historyParent.GetChild(i).gameObject);
            }
            // Debug.Log("lenght = "+panelLines.Length);
            for (int i = 0; i < panelLines.Length; i++)
            {
                GameObject goj = Instantiate(historyPerfab, historyParent, false);
                Text[] txtArr = goj.GetComponentsInChildren<Text>();
                txtArr[0].text = ((HistoryRowData)panelLines[i]).maPhien;
                txtArr[1].text = ((HistoryRowData)panelLines[i]).thoiGian;
                txtArr[2].text = ((HistoryRowData)panelLines[i]).mucCuoc;
                txtArr[3].text = ((HistoryRowData)panelLines[i]).phatSinh;
                txtArr[4].text = ((HistoryRowData)panelLines[i]).soDu;
                goj.SetActive(true);
            }
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
