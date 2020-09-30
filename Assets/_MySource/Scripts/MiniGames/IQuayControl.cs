using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IQuayControl : MonoBehaviour {

    private void OnEnable()
    {
        var req_INFO = new OutBounMessage("XENG.GET_INFO");
        req_INFO.addHead();
        App.ws.send(req_INFO.getReq(), delegate (InBoundMessage res_INFO)
        {
            App.trace("[RECV] IQUAY.GET_INFO");
            int count = res_INFO.readByte();
            for (int i = 0; i < count; i++)
            {
                int bet = res_INFO.readInt();
                App.trace("BET = " + bet);
            }

            bool isEnd = res_INFO.readByte() == 0;
            if (!isEnd)
            {
                int bet = res_INFO.readInt();
                int currRound = res_INFO.readInt();
                string items = res_INFO.readAscii();
                string prize = res_INFO.readAscii();
                
            }
        });
    }

    public void Spin()
    {
        var req_START = new OutBounMessage("XENG.START");
        req_START.addHead();
        req_START.writeInt(2000);           //bet amount
        req_START.writeAcii("chip");         //balance type: "man"|"chip"
        App.ws.send(req_START.getReq(), delegate (InBoundMessage res_START)
        {
            string items = res_START.readAscii();
            bool allowSpin = res_START.readByte() == 1;
            bool allowEnd = res_START.readByte() == 1;
            string prize = res_START.readAscii();

            App.trace("[RECV] XENG.START item = " + items + "|allowSpin = " + allowSpin + "|allowEnd = " + allowEnd + "|prize = " + prize);
            //[RECV] XENG.START item = r1|allowSpin = True|allowEnd = Trueprize = 0.5
        });
    }
}
