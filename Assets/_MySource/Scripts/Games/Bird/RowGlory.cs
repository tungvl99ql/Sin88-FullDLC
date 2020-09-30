using Casino.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Bird
{
    public class RowGlory : UILine
    {

        public Text txtTaiKhoan;
        public Text txtThoiGian;
        public Text txtMucCuoc;
        public Text txtThang;

        void OnDisable()
        {
            Destroy(gameObject);
        }

        public override void DrawLine(PanelLineData lineData)
        {
            GrolyRowData _lineData = (GrolyRowData)lineData;
            txtTaiKhoan.text = _lineData.taiKhoan;
            txtThoiGian.text = _lineData.thoiGian;
            txtMucCuoc.text = _lineData.mucCuoc;
            txtThang.text = _lineData.thang.ToString();
        }

    }
}