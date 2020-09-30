using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System;
public class ChessDraggerVua : MonoBehaviour, IPointerClickHandler
{

    public Transform parentToReturnTo = null;
    public Transform placeholderParent = null;
    public ChessTableVuaControll chessTableControll;
    private string type;
    GameObject placeholder = null;
    void Awake()
    {
        chessTableControll = GameObject.FindGameObjectWithTag("TableChessVua").GetComponent<ChessTableVuaControll>();
    }
    /*
    public void OnBeginDrag(PointerEventData eventData)
    {
        chessTableControll.chessTableClearPointMove();
        chessTableControll.chessPieceMove = this.transform.parent.gameObject;
        chessTableControll.dragMode = true;
        if (gameObject.name.Contains("red"))
        {
            type = "red";
        }
        else
        {
            type = "black";
        }
        chessTableControll.chessTableControllPiece(gameObject.name, gameObject.transform.parent.name, type);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);
        if(gameObject.transform.GetChild(0).gameObject.activeSelf)
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
        //chessTableControll.chessPointToSlot[chessTableControll.chessCurrDirPoint].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
        //App.trace("||||||| " + gameObject.name);
        placeholder = new GameObject();
        placeholder.transform.SetParent(this.transform.parent);
        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        parentToReturnTo = this.transform.parent;
        placeholderParent = parentToReturnTo;
        this.transform.SetParent(this.transform.parent.parent);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log ("OnDrag");

        this.transform.position = eventData.position;

        if (placeholder.transform.parent != placeholderParent)
            placeholder.transform.SetParent(placeholderParent);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(parentToReturnTo);
        transform.position = parentToReturnTo.position;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        Destroy(placeholder);
        //chessTableControll.chessTableClearPointMove();
    }
    */
    public void OnPointerClick(PointerEventData eventData)
    {
        chessTableControll.dragMode = false;
        if (gameObject.name.Contains("red"))
        {
            type = "red";
        }
        else
        {
            type = "black";
        }
        chessTableControll.chessTableClearPointMove();
        chessTableControll.chessTableControllPiece(gameObject.name, gameObject.transform.parent.name, type);
        chessTableControll.chessPieceMove = this.transform.parent.gameObject;
        //App.trace("Clicked");
    }
}
