using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;

namespace Slot.Games.Fish
{
    public class FishGameGloryPanel : UIPanel
    {
        public GameObject gloryPerfab;
        public Transform gloryParent;
        public Button btnNoHu, btnThangLon;
        public override void Show()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            btnThangLon.image.color = new Color32(146, 57, 57, 255);
            btnNoHu.image.color = new Color32(255, 255, 255, 255);
            for (int i = gloryParent.childCount - 1; i > 0; i--)
            {
                Destroy(gloryParent.GetChild(i).gameObject);
            }
            for (int i = 0; i < panelLines.Length; i++)
            {
                if (((GrolyRowData)panelLines[i]).isJackpost == true)
                {
                    GameObject goj = Instantiate(gloryPerfab, gloryParent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();
                    txtArr[0].text = ((GrolyRowData)panelLines[i]).taiKhoan;
                    txtArr[1].text = ((GrolyRowData)panelLines[i]).thoiGian;
                    txtArr[2].text = ((GrolyRowData)panelLines[i]).mucCuoc;
                    txtArr[3].text = ((GrolyRowData)panelLines[i]).thang;
                    goj.SetActive(true);
                }

            }
        }
        public void ChangeType(bool isBigWin)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            for (int i = gloryParent.childCount - 1; i > 0; i--)
            {
                Destroy(gloryParent.GetChild(i).gameObject);
            }
            if (isBigWin)
            {
                btnNoHu.image.color = new Color32(146, 57, 57, 255);
                btnThangLon.image.color =new  Color32(255, 255, 255, 255);
            }
            else
            {
                btnNoHu.image.color = new Color32(255, 255, 255, 255);
                btnThangLon.image.color = new Color32(146, 57, 57, 255);
            }
            for (int i = 0; i < panelLines.Length; i++)
            {
                if (((GrolyRowData)panelLines[i]).isBigWin == isBigWin && ((GrolyRowData)panelLines[i]).isJackpost != isBigWin)
                {
                    GameObject goj = Instantiate(gloryPerfab, gloryParent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();
                    txtArr[0].text = ((GrolyRowData)panelLines[i]).taiKhoan;
                    txtArr[1].text = ((GrolyRowData)panelLines[i]).thoiGian;
                    txtArr[2].text = ((GrolyRowData)panelLines[i]).mucCuoc;
                    txtArr[3].text = ((GrolyRowData)panelLines[i]).thang;
                    goj.SetActive(true);
                }
                else if (((GrolyRowData)panelLines[i]).isJackpost != isBigWin)
                {
                    GameObject goj = Instantiate(gloryPerfab, gloryParent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();
                    txtArr[0].text = ((GrolyRowData)panelLines[i]).taiKhoan;
                    txtArr[1].text = ((GrolyRowData)panelLines[i]).thoiGian;
                    txtArr[2].text = ((GrolyRowData)panelLines[i]).mucCuoc;
                    txtArr[3].text = ((GrolyRowData)panelLines[i]).thang;
                    goj.SetActive(true);
                }

            }
        }


    }
    public class GrolyRowData : PanelLineData
    {
        public string taiKhoan;
        public string thoiGian;
        public string mucCuoc;
        public string thang;
        public bool isBigWin;
        public bool isJackpost;

        public GrolyRowData(string taiKhoan, string thoiGian, string mucCuoc, string thang, bool isBigWin, bool isJackpost)
        {
            this.taiKhoan = taiKhoan;
            this.thoiGian = thoiGian;
            this.mucCuoc = mucCuoc;
            this.thang = thang;
            this.isBigWin = isBigWin;
            this.isJackpost = isJackpost;
        }
    }
}
