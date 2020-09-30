using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.Server.Api
{

    public class MyWebsocketClient
    {
        private WebSocketClient ws;
        private Dictionary<string, Action<InBoundMessage>> callBackList = new Dictionary<string, Action<InBoundMessage>>();
        public MyWebsocketClient(string tempUri)
        {
            //this.ws = new WebSocketClient(new Uri("ws://192.168.100.5:8008/ws/gameServer"));    //test sv
            //this.ws = new WebSocketClient(new Uri("wss://sungaming.win/ws/gameServer"));
            this.ws = new WebSocketClient(new Uri(tempUri));
        }
        
        public WebSocketClient getWS()
        {
            return this.ws;
        }

        public void send(byte[] data, Action<InBoundMessage> callback, bool add = true, int showLoad = 1)
        {
            this.ws.Send(data, callback, add, showLoad);

        }

        public void sendHandler(byte[] data, Action<InBoundMessage> callback, string name = "")
        {
            this.ws.SendHandler(data, callback, name);
        }

        public void delHandler(byte[] data)
        {
            this.ws.delHandler(data);
        }

        public byte[] recv()
        {
            return this.ws.Recv();
        }

        public bool endOfData()
        {
            return this.ws.endOfData();
        }
    }
}
