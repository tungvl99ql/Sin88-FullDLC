using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessInvitePlayer : MonoBehaviour {

    public ChessTableControll chessTableControll;
    void Awake()
    {
        chessTableControll = GameObject.FindGameObjectWithTag("TableChess").GetComponent<ChessTableControll>();
    }
    public void chessInvitePlayer()
    {
        chessTableControll.chessInvitePlayer(this.transform.parent.name);
        Destroy(this.transform.parent.gameObject, 0.2f);
    }
}
