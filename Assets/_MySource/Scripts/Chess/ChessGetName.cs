using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessGetName : MonoBehaviour {
    public ChessControll chessControll;
    void Awake()
    {
        chessControll = GameObject.FindGameObjectWithTag("WaitChess").GetComponent<ChessControll>();
    }
    public void chessSendNameTable()
    {
        chessControll.chessGetTableData(gameObject.name.ToString());
    }
}
