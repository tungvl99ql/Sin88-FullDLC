using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessInvitePlayerVua : MonoBehaviour {

    public ChessTableVuaControll chessTableControll;
    void Awake()
    {
        chessTableControll = GameObject.FindGameObjectWithTag("TableChessVua").GetComponent<ChessTableVuaControll>();
    }
    public void chessInvitePlayer()
    {
        chessTableControll.chessInvitePlayer(this.transform.parent.name);
        Destroy(this.transform.parent.gameObject, 0.2f);
    }
}
