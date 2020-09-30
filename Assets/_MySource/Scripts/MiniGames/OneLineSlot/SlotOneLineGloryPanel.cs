using Casino.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Casino.Games.OneLineSlot
{

    public class SlotOneLineGloryPanel : UIPanel
    {
        public Button btnNoHu, btnThangLon;
        public GameObject line;
        public ScrollRect rect;
        private bool jackpost = true;
        public override void Show()
        {
            jackpost = true;
            btnNoHu.image.color = new Color32(178, 3, 119, 255);
            btnThangLon.image.color = new Color32(141, 79, 120, 255);
            for (int i = 0; i < panelLines.Length; i++)
            {
                GrolyRowData row = (GrolyRowData)(panelLines[i]);
                if (row.isJackpost)
                {
                    UILine l = GameObject.Instantiate(linePrefab) as UILine;
                    l.DrawLine(panelLines[i]);
                    l.transform.parent = line.transform;
                    l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    l.gameObject.SetActive(true);
                }

            }
            StartCoroutine(wait());
        }
        IEnumerator wait()
        {

            yield return new WaitForSeconds(0.1f);
            rect.verticalNormalizedPosition = 1f;

        }
        public void Change_NoHu()
        {

            for (int i = linePrefab.transform.parent.childCount - 1; i > 0; i--)
            {
                Destroy(linePrefab.transform.parent.GetChild(i).gameObject);
            }
            jackpost = true;
            btnNoHu.image.color = new Color32(178, 3, 119, 255);
            btnThangLon.image.color = new Color32(141, 79, 120, 255);

        
            for (int i = 0; i < panelLines.Length; i++)
            {
                GrolyRowData row = (GrolyRowData)(panelLines[i]);
                if (row.isJackpost)
                {
                    UILine l = GameObject.Instantiate(linePrefab) as UILine;
                    l.DrawLine(panelLines[i]);
                    l.transform.parent = line.transform;
                    l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    l.gameObject.SetActive(true);
                }

            }
            StartCoroutine(wait());
        }
        public void Change_ThangLon()
        {
            for (int i = linePrefab.transform.parent.childCount - 1; i > 0; i--)
            {
                Destroy(linePrefab.transform.parent.GetChild(i).gameObject);
            }
            jackpost = false;
            btnNoHu.image.color = new Color32(141, 79, 120, 255);
            btnThangLon.image.color = new Color32(178, 3, 119, 255);

            for (int i = 0; i < panelLines.Length; i++)
            {
                GrolyRowData row = (GrolyRowData)(panelLines[i]);
                if (row.isBigWin&&row.isJackpost==false)
                {
                    UILine l = GameObject.Instantiate(linePrefab) as UILine;
                    l.DrawLine(panelLines[i]);
                    l.transform.parent = line.transform;
                    l.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    l.gameObject.SetActive(true);
                }

            }
            StartCoroutine(wait());
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

