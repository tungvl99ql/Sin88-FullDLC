using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

public class ChessDragZoneVua : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ChessTableVuaControll chessTableControll;
    void Awake()
    {
        chessTableControll = GameObject.FindGameObjectWithTag("TableChessVua").GetComponent<ChessTableVuaControll>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        ChessDraggerVua d = eventData.pointerDrag.GetComponent<ChessDraggerVua>();
        if (d != null && d.gameObject.name.Contains(chessTableControll.chessMyColor))
        {
            d.placeholderParent = this.transform.parent;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        ChessDraggerVua d = eventData.pointerDrag.GetComponent<ChessDraggerVua>();
        if (d != null)
        {
            d.placeholderParent = d.parentToReturnTo;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        //Debug.Log(eventData.pointerDrag.name + " was dropped on " + gameObject.name);
        ChessDraggerVua d = eventData.pointerDrag.GetComponent<ChessDraggerVua>();
        if (d != null && d.gameObject.name.Contains(chessTableControll.chessMyColor))
        {
            string getSource = Regex.Match(chessTableControll.chessSourcePointDrag, @"\d+").Value;
            int sourcePoint = Int32.Parse(getSource);
            string getDir = Regex.Match(gameObject.name, @"\d+").Value;
            int dirPoint = Int32.Parse(getDir);
            if (dirPoint <= 7 && d.gameObject.name.Contains("tot")  || dirPoint >= 56 && d.gameObject.name.Contains("tot"))
            {
                chessTableControll.chessClick = false;
                chessTableControll.chessUpdateNewFacePopUp(true);
                chessTableControll.chessCurrSourePoint = sourcePoint;
                chessTableControll.chessCurrDirPoint = dirPoint;
            }
            else
            {
                chessTableControll.chessMovePerPieceDrag(sourcePoint, dirPoint,0);
            }
            //d.parentToReturnTo = this.transform.parent;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        chessTableControll.chessTableControllPieceClick(this.transform.parent.gameObject);
    }
}
