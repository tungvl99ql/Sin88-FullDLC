using Casino.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Bird
{
    public class BirdRowHistory : UILine
    {
        public Text txtMaPhien;
        public Text txtThoiGian;
        public Text txtMucCuoc;
        public Text txtPhatSinh;
        public Text txtSoDu;
        void OnDisable()
        {
            Destroy(gameObject);
        }

        public override void DrawLine(PanelLineData lineData)
        {
            HistoryRowData _lineData = (HistoryRowData)lineData;
            txtMaPhien.text = _lineData.maPhien;
            txtThoiGian.text = _lineData.thoiGian;
            txtMucCuoc.text = _lineData.mucCuoc;
            txtPhatSinh.text = _lineData.phatSinh.ToString();
            txtSoDu.text = _lineData.soDu.ToString();
        }

    }


}
