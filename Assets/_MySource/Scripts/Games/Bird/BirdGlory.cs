using Casino.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Bird
{
    public class BirdGlory : UIPanel
    {
        public Button btnNoHu, btnThangLon;
        public GameObject line;
        public ScrollRect screct;
        public override void Show()
        {
            btnNoHu.GetComponentsInChildren<Image>()[1].enabled = true;
            btnThangLon.GetComponentsInChildren<Image>()[1].enabled = false;
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
            //screct.normalizedPosition = new Vector2(1, 1);
            //     screct.GetComponent<ScrollRect>().verticalNormalizedPosition = 1.0f;
            //  screct.verticalScrollbar.value = 1;


        }


        IEnumerator wait() {

            yield return new WaitForSeconds(0.1f);
             screct.verticalNormalizedPosition = 1f;

        }

        private void ClearRow()
        {
            for (int i = 0; i < line.transform.childCount ; i++)
            {
                Destroy(line.transform.GetChild(i).gameObject);
            }
        }
        public void Change_NoHu()
        {

            ClearRow();
            
            btnNoHu.GetComponentsInChildren<Image>()[1].enabled = true;
            btnThangLon.GetComponentsInChildren<Image>()[1].enabled = false;
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
            ClearRow();
            btnNoHu.GetComponentsInChildren<Image>()[1].enabled = false;
            btnThangLon.GetComponentsInChildren<Image>()[1].enabled = true;
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