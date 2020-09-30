using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

public class ChessDragZone : MonoBehaviour,IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ChessTableControll chessTableControll;
    void Awake()
    {
        chessTableControll = GameObject.FindGameObjectWithTag("TableChess").GetComponent<ChessTableControll>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        ChessDragger d = eventData.pointerDrag.GetComponent<ChessDragger>();
        if (d != null && d.gameObject.name.Contains(chessTableControll.chessMyColor))
        {
            d.placeholderParent = this.transform.parent;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        ChessDragger d = eventData.pointerDrag.GetComponent<ChessDragger>();
        if (d != null)
        {
            d.placeholderParent = d.parentToReturnTo;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        //Debug.Log(eventData.pointerDrag.name + " was dropped on " + gameObject.name);

        ChessDragger d = eventData.pointerDrag.GetComponent<ChessDragger>();
        if (d != null && d.gameObject.name.Contains(chessTableControll.chessMyColor))
        {            
            string getSource = Regex.Match(chessTableControll.chessSourcePointDrag, @"\d+").Value;
            int sourcePoint = Int32.Parse(getSource);
            string getDir = Regex.Match(gameObject.name, @"\d+").Value;
            int dirPoint = Int32.Parse(getDir);
            chessTableControll.chessMovePerPieceDrag(sourcePoint, dirPoint);
            //d.parentToReturnTo = this.transform.parent;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        chessTableControll.chessTableControllPieceClick(this.transform.parent.gameObject);
    }
}
