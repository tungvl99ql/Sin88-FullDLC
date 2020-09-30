using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using BestHTTP.WebSocket;
using Core.Server.Api;
using UnityEngine.SceneManagement;//Using IOS
using BestHTTP;

namespace Core.Server.Api
{


    public class WebSocketClient
    {
#if UNITY_IOS || UNITY_ANDROID
        private Uri mUrl;

        public WebSocketClient(Uri url)
        {
            mUrl = url;

            string protocol = mUrl.Scheme;
            if (!protocol.Equals("ws") && !protocol.Equals("wss"))
                throw new ArgumentException("Unsupported protocol: " + protocol);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern int SocketCreate (string url);

	[DllImport("__Internal")]
	private static extern int SocketState (int socketInstance);

	[DllImport("__Internal")]
	private static extern void SocketSend (int socketInstance, byte[] ptr, int length);

	[DllImport("__Internal")]
	private static extern void SocketRecv (int socketInstance, byte[] ptr, int length);

	[DllImport("__Internal")]
	private static extern int SocketRecvLength (int socketInstance);

	[DllImport("__Internal")]
	private static extern void SocketClose (int socketInstance);

	[DllImport("__Internal")]
	private static extern int SocketError (int socketInstance, byte[] ptr, int length);

	int m_NativeRef = 0;

	public void Send(byte[] buffer)
	{
		SocketSend (m_NativeRef, buffer, buffer.Length);
	}

	public byte[] Recv()
	{
		int length = SocketRecvLength (m_NativeRef);
		if (length == 0)
			return null;
		byte[] buffer = new byte[length];
		SocketRecv (m_NativeRef, buffer, length);
		return buffer;
	}

	public IEnumerator Connect()
	{
		m_NativeRef = SocketCreate (mUrl.ToString());

		while (SocketState(m_NativeRef) == 0)
			yield return 0;
	}
 
	public void Close()
	{
		SocketClose(m_NativeRef);
	}

	public string error
	{
		get {
			const int bufsize = 1024;
			byte[] buffer = new byte[bufsize];
			int result = SocketError (m_NativeRef, buffer, bufsize);

			if (result == 0)
				return null;

			return Encoding.UTF8.GetString (buffer);				
		}
	}
#else
        WebSocket m_Socket;
        Queue<byte[]> m_Messages = new Queue<byte[]>();
        bool m_IsConnected = false;
        string m_Error = null;
        private Dictionary<string, Action<InBoundMessage>> callBackList = new Dictionary<string, Action<InBoundMessage>>();
        private Dictionary<string, Action<InBoundMessage>> callBackHandlerList = new Dictionary<string, Action<InBoundMessage>>();
        public IEnumerator Connect(System.Action onConnectSucces = null)
        {
            m_Socket = new WebSocket(mUrl);
            
            m_Socket.OnClosed += (sender, code, e) =>
            {
                Loom.QueueOnMainThread(() =>
                {
                    //if (CPlayer.lobbyBtnBackIsPressed == false)
                    //{
                    //LoadingControl.instance.currMessTime = -1;
                   var currMessTime = -1;
                    if (LoadingControl.instance != null && logout == false)
                    {
                        m_Socket.Close();
                        logout = true;
                        App.needStartSocket = false;
                        App.trace("CONNECT FAILED! FROM WS CL", "red");
                        //App.showErr("Mất kết nối, vui lòng đăng nhập lại", true);
                        App.showErr(App.listKeyText["WARN_SERVER_CONNECTION"],true);
                    }
                    //}

                });

            };

            m_Socket.OnBinary += (sender, data) =>
            {

                try
                {
                    byte[] reply = new byte[1024];
                    reply = data;
                    if (reply != null)
                    {


                        string callBackId = "" + reply[0] + reply[1];

                        //    Debug.Log("callBackId  " + callBackId);
                        InBoundMessage inb = new InBoundMessage(reply);




                        if (callBackList.ContainsKey(callBackId))
                        {

                            App.trace("[RECV MESS = " + callBackId + "]");
                            int first = inb.readByte();
                            if (first != 0)
                            {
                                //if (LoadingControl.instance.loadingGojList[32].activeSelf)
                                //LoadingControl.instance.loadingGojList[32].SetActive(false);
                                App.printBytesArray(reply);
                                App.trace("ERRORRRRRRRRRRRR!");
                                string t = inb.readStrings();
                                App.trace(t);
                                if (t == "NotEnoughAmount")
                                {
                                    t = "Bạn không đủ Gold.";
                                }
                                if (t == "NoSpinAvailable")
                                {
                                    t = "Số lượt quay của bạn đã hết.";
                                }

                                Loom.QueueOnMainThread(() =>
                                {
                                    if (CPlayer.currMess == callBackId)
                                    {
                                        LoadingControl.instance.currMessTime = -1;
                                        //LoadingControl.instance.loadingScene.SetActive(false);
                                        LoadingUIPanel.Hide();
                                        App.showLoading(false);
                                    }


                                    if (t == "NotInRoom")
                                    {
                                        //TableList.instance.loadData();
                                        //App.showErr("Đã xảy ra lỗi");
                                        var req = new OutBounMessage("GET_CURRENT_PATH");
                                        req.addHead();
                                        App.ws.send(req.getReq(), delegate (InBoundMessage res)
                                        {
                                            var count = res.readByte();
                                            for (var i = 0; i < count; i++)
                                            {
                                                var type = res.readAscii();
                                                var id = res.readAscii();
                                                var name = res.readString();
                                                //App.trace("type = " + type + "|id= " + id + "|name = " + name);
                                                App.showErr("[NotInRoom] type = " + type + "|id= " + id + "|name = " + name);
                                            }


                                            var avatar = res.readAscii();
                                            var avatarId = res.readInt();
                                            var chipBalance = res.readLong();
                                            var starBalance = res.readLong();
                                            var score = res.readLong();
                                            var level = res.readByte();
                                        });
                                    }
                                    else if (t.Contains("cần xác thực tài khoản"))
                                    {
                                        App.trace("RCV [CASH_OUT.EXCHANGE_ITEM]");
                                        //LoadingControl.instance.loadingScene.SetActive(false);
                                        LoadingControl.instance.btnDoConfirm.onClick.RemoveAllListeners();

                                        LoadingControl.instance.btnDoConfirm.onClick.AddListener(() =>
                                        {
                                            LoadingControl.instance.closeConfirmDialog();
                                            string url = "sms:7069?body=CGV XT " + CPlayer.id;
#if UNITY_IOS
                                        url = string.Format("sms:{0}?&body={1}", 7069, System.Uri.EscapeDataString("CGV XT " + CPlayer.id));
#endif
                                            Application.OpenURL(url);
                                        });
                                        //LoadingControl.instance.confirmText.text = t;
                                        //LoadingControl.instance.confirmText.text = PlayerPrefs.GetString("confirmText3"); //"- Xác thực số điện thoại để bảo mật tài khoản và kích hoạt VIP\n- Tặng ngay 2000 Chip sau khi xác thực thành công\n- Nhận miễn phí VQMM hàng ngày\n(*)Phí: 1000 vnđ";
                                        LoadingControl.instance.blackPanel.SetActive(true);
                                        LoadingControl.instance.confirmDialogAnim.Play("DialogAnim");
                                    }
                                    else if (t.Contains("chưa đăng nhập vào game") || t.Contains("đủ chip để vào bàn"))
                                    {
                                        //App.showErr("Gửi lời mời thành công");
                                        App.showErr(App.listKeyText["INFO_INVITE_SUCCESS"]);
                                    }
                                    else if (t.Contains("đã bị đóng bởi chủ bàn"))
                                    {
                                        if (TableList.instance != null)
                                        {
                                            //App.showErr("Bàn chơi đã bị đóng bởi chủ bàn");
                                            App.showErr(App.listKeyText["INFO_CLOSED_OWNER"]);
                                            /*
                                            var req = new OutBounMessage("GET_CURRENT_PATH");
                                            req.addHead();
                                            App.ws.send(req.getReq(), delegate (InBoundMessage res)
                                            {
                                                var count = res.readByte();
                                                for (var i = 0; i < count; i++)
                                                {
                                                    var type = res.readAscii();
                                                    var id = res.readAscii();
                                                    var name = res.readString();
                                                    //App.trace("type = " + type + "|id= " + id + "|name = " + name);
                                                    App.showErr("[Table Closed] type = " + type + "|id= " + id + "|name = " + name);
                                                }


                                                var avatar = res.readAscii();
                                                var avatarId = res.readInt();
                                                var chipBalance = res.readLong();
                                                var starBalance = res.readLong();
                                                var score = res.readLong();
                                                var level = res.readByte();
                                            });*/
                                        }
                                    }
                                    else if (t.Contains("không được vào chơi do không đủ Gold"))
                                    {
                                        LoadingControl.instance.notEnoughChip(App.formatToUserContent(t));
                                    }
                                    else if (t.Contains("không đủ"))
                                    {

                                        if (CPlayer.hidePayment)
                                        {
                                            App.showErr(App.formatToUserContent(t));
                                        }
                                        else
                                        {
                                            //LoadingControl.instance._showNotEnoughMoney(App.formatToUserContent(t));
                                            App.showErr(App.formatToUserContent(t));
                                        }


                                    }
                                    else if (t.Contains("không ở bàn chơi"))
                                    {
#if UNITY_IOS
                                switch (SceneManager.GetActiveScene().name)
                                    {
                                        case "Phom":
                                            SceneManager.LoadScene("LobbyScene");
                                            App.showErr("Ván chơi đã kết thúc!");
                                            break;
                                    }
#endif
                                    }
                                    else
                                    {
                                        //CPlayer.erroShowing = true;
                                        //if (LoadingControl.instance.loadingScene.activeSelf)
                                        //LoadingControl.instance.loadingScene.SetActive(false);

                                        LoadingUIPanel.Hide();

                                        App.showErr(App.formatToUserContent(t));
                                    }

                                });


                            }
                            else
                            {

                                //App.trace("GET CALLBACK ID = " + callBackId);
                                if (callBackList.ContainsKey(callBackId))
                                {
                                    var callback1 = callBackList[callBackId];
                                    if (callback1 != null)
                                        //callBackList.Remove(callBackId);
                                        Loom.QueueOnMainThread(() =>
                                            {
                                            // Use the Unity API here.
                                            callback1(inb);
                                                if (CPlayer.currMess == callBackId)
                                                {
                                                    App.showLoading(false);
                                                    //LoadingControl.instance.currMessTime = -1;
                                                    var currMessTime = -1;
                                                }
                                            //callBackList.Remove(callBackId);
                                        });

                                }

                            }

                            callBackList.Remove(callBackId);
                            m_Messages.Enqueue(data);
                            return;


                        }
                        if (callBackHandlerList.ContainsKey(callBackId))
                        {
                            //int first = inb.readByte();

                            var callback = callBackHandlerList[callBackId];

                            Loom.QueueOnMainThread(() =>
                            {
                                // Use the Unity API here.
                                callback(inb);
                                //LoadingControl.instance.loadingScene.SetActive(false);
                                //callBackHandlerList.Remove(callBackId);
                                LoadingControl.instance.currMessTime = 0;
                            });
                            m_Messages.Enqueue(data);
                            return;
                        }

                        Loom.QueueOnMainThread(() =>
                        {
                            // Use the Unity API here.
                            //callback1(inb);
                            App.trace("No Handler or Mess with cmd = " + callBackId);
                            // Debug.Log("No Handler or Mess with cmd = " + callBackId);
                            //LoadingControl.instance.loadingScene.SetActive(false);
                            //callBackList.Remove(callBackId);
                        });

                    }

                    m_Messages.Enqueue(data);
                }
                catch
                {
                    m_Socket.Close();
                    App.needStartSocket = false;
                    //App.showErr("Kết nối bị ngắt. Vui lòng đăng nhập lại", true);
                    App.showErr(App.listKeyText["WARN_SERVER_CONNECTION"],true);
                }
            };
            // m_Socket.OnMessage += (sender, e) => Debug.Log("111111111111");
            m_Socket.OnOpen += (sender) => m_IsConnected = true;
            // m_Socket.OnError += (sender, e) => m_Error = e.Message;
            m_Socket.OnError += (sender, e) =>
            {
                m_Socket.Close();
                App.needStartSocket = false;
                //App.showErr("Bị mất nối bị ngắt. Vui lòng đăng nhập lại", true);
                App.showErr(App.listKeyText["WARN_SERVER_CONNECTION"],true);
            };
            m_Socket.Open();
            while (!m_IsConnected && m_Error == null)
                //LobbyControl.instance.getNewUpdate();
                //LobbyControl.instance.getMaintain();
            yield return 0;
            
            if(onConnectSucces != null)
            {
                onConnectSucces();
            }
        }

        public void Send(byte[] buffer, Action<InBoundMessage> callback, bool add, int showLoad = 1)
        {
            string callBackId = "" + buffer[0] + buffer[1];

            if (add)
            {
                if (!callBackList.ContainsKey(callBackId))
                {
                    //App.trace("SAVE CALLBACK ID = " + callBackId);

                    callBackList.Add(callBackId, callback);
                }
                else
                {
                    callBackList.Remove(callBackId);

                    callBackList.Add(callBackId, callback);
                    //callBackList[callBackId] = callback;
                }
            }
            m_Socket.Send(buffer);

            //App.trace("sented");
            Loom.QueueOnMainThread(() =>
            {
                // Use the Unity API here.
                if (showLoad == 1)
                {
                    //LoadingControl.instance.currMessTime = 0;
                    var  currMessTime = 0;
                    CPlayer.currMess = callBackId;
                    App.trace("[SEND MESS = " + callBackId + "]");
                    App.showLoading();
                }

            });
        }

        public void SendHandler(byte[] buffer, Action<InBoundMessage> callback, string name = "")
        {
            string callBackId = "" + buffer[0] + buffer[1];

            if (!callBackHandlerList.ContainsKey(callBackId))
            {
                //App.trace("SAVE CALLBACK ID = " + callBackId);
                callBackHandlerList.Add(callBackId, callback);
            }
            else
            {
                callBackHandlerList.Remove(callBackId);
                callBackHandlerList.Add(callBackId, callback);
            }
            //m_Socket.Send(buffer);
            if (name != "")
                App.trace("[SEND] " + name, "red");
        }

        public void delHandler(byte[] buff)
        {
            string callBackId = "" + buff[0] + buff[1];
            if (callBackHandlerList.ContainsKey(callBackId))
            {
                callBackHandlerList.Remove(callBackId);
            }
        }

        public byte[] Recv()
        {
            if (m_Messages.Count == 0)
                return null;
            return m_Messages.Dequeue();
        }

        public void Close()
        {
            m_Socket.Close();
            m_IsConnected = false;
            logout = false;
        }

        private bool logout = false;
        public void Logout()
        {
            m_Socket.Close();
            App.needStartSocket = false;
            m_IsConnected = false;
            logout = true;
        }

        public string error
        {
            get
            {
                return m_Error;
            }
        }

        public bool isConnecting()
        {
            return m_IsConnected;
        }

        public bool endOfData()
        {
            return m_Messages.Count == 0;
        }
#endif
#else
    private Uri mUrl;

    public WebSocketClient(Uri url)
    {
        mUrl = url;

        string protocol = mUrl.Scheme;
        if (!protocol.Equals("ws") && !protocol.Equals("wss"))
            throw new ArgumentException("Unsupported protocol: " + protocol);
    }


    WebSocket m_Socket;
    Queue<byte[]> m_Messages = new Queue<byte[]>();
    bool m_IsConnected = false;
    string m_Error = null;
    private Dictionary<string, Action<InBoundMessage>> callBackList = new Dictionary<string, Action<InBoundMessage>>();
    private Dictionary<string, Action<InBoundMessage>> callBackHandlerList = new Dictionary<string, Action<InBoundMessage>>();
    public IEnumerator Connect()
    {
        m_Socket = new WebSocket(new Uri(mUrl.ToString()));
#if !BESTHTTP_DISABLE_PROXY && !UNITY_WEBGL
                    if (HTTPManager.Proxy != null)
                        m_Socket.InternalRequest.Proxy = new HTTPProxy(HTTPManager.Proxy.Address, HTTPManager.Proxy.Credentials, false);
#endif
        m_Socket.OnClosed += OnClosed;

        m_Socket.OnMessage += OnMessageReceived;
        //m_Socket.OnMessage += (sender, e) => Debug.Log();
        m_Socket.OnOpen += OnOpen;
        m_Socket.OnError += (sender, e) => {
            m_Socket.Close();
           App.showErr("Bị mất nối bị ngắt. Vui lòng đăng nhập lại", true);
        
        };
        m_Socket.OnBinary += OnBinaryMessageReceived;
        m_Socket.Open();
        while (!m_IsConnected && m_Error == null)
            yield return 0;
    }



    public void Send(byte[] buffer, Action<InBoundMessage> callback, bool add, int showLoad = 1)
    {
        string callBackId = "" + buffer[0] + buffer[1];

        if (add)
        {
            if (!callBackList.ContainsKey(callBackId))
            {
                //App.trace("SAVE CALLBACK ID = " + callBackId);

                callBackList.Add(callBackId, callback);
            }
            else
            {
                callBackList.Remove(callBackId);

                callBackList.Add(callBackId, callback);
                //callBackList[callBackId] = callback;
            }
        }
        m_Socket.Send(buffer);

        //App.trace("sented");
        Loom.QueueOnMainThread(() =>
        {
            // Use the Unity API here.
            if (showLoad == 1)
            {
                LoadingControl.instance.currMessTime = 0;
                CPlayer.currMess = callBackId;
                App.trace("[SEND MESS = " + callBackId + "]");
                App.showLoading();
            }

        });
    }

    public void SendHandler(byte[] buffer, Action<InBoundMessage> callback, string name = "")
    {
        string callBackId = "" + buffer[0] + buffer[1];

        if (!callBackHandlerList.ContainsKey(callBackId))
        {
            //App.trace("SAVE CALLBACK ID = " + callBackId);
            callBackHandlerList.Add(callBackId, callback);
        }
        else
        {
            callBackHandlerList.Remove(callBackId);
            callBackHandlerList.Add(callBackId, callback);
        }
        //m_Socket.Send(buffer);
        if (name != "")
            App.trace("[SEND] " + name, "red");
    }

    public void delHandler(byte[] buff)
    {
        string callBackId = "" + buff[0] + buff[1];
        if (callBackHandlerList.ContainsKey(callBackId))
        {
            callBackHandlerList.Remove(callBackId);
        }
    }

    public byte[] Recv()
    {
        if (m_Messages.Count == 0)
            return null;
        return m_Messages.Dequeue();
    }

    public void Close()
    {
        //m_Socket.Close();
        m_IsConnected = false;
        logout = false;
    }

    private bool logout = false;
    public void Logout()
    {
        m_IsConnected = false;
        logout = true;
        m_Socket.Close();
    }

    public string error
    {
        get
        {
            return m_Error;
        }
    }

    public bool isConnecting()
    {
        return m_IsConnected;
    }

    public bool endOfData()
    {
        return m_Messages.Count == 0;
    }

    private void OnBinaryMessageReceived(WebSocket webSocket, byte[] message)
    {
        try
        {
            byte[] reply = new byte[1024];
            reply = message;
            if (reply != null)
            {

                string callBackId = "" + reply[0] + reply[1];

                InBoundMessage inb = new InBoundMessage(reply);




                if (callBackList.ContainsKey(callBackId))
                {

                    App.trace("[RECV MESS = " + callBackId + "]");
                    int first = inb.readByte();
                    if (first != 0)
                    {

                        App.printBytesArray(reply);
                        App.trace("ERRORRRRRRRRRRRR!");
                        string t = inb.readStrings();
                        App.trace(t);
                        if (t == "NotEnoughAmount")
                        {
                            t = "Bạn không đủ Gold.";
                        }
                        if (t == "NoSpinAvailable")
                        {
                            t = "Số lượt quay của bạn đã hết.";
                        }

                        Loom.QueueOnMainThread(() =>
                        {
                            if (CPlayer.currMess == callBackId)
                            {
                                LoadingControl.instance.currMessTime = -1;
                                App.showLoading(false);
                            }

                            if (t == "NotInRoom")
                            {

                            }
                            else if (t.Contains("cần xác thực tài khoản"))
                            {
                               
                                LoadingControl.instance.showAuthen(true);
                            }
                            else if (t.Contains("chưa đăng nhập vào game") || t.Contains("đủ chip để vào bàn"))
                            {
                                App.showErr("Gửi lời mời thành công");
                            }
                            else if (t.Contains("đã bị đóng bởi chủ bàn"))
                            {
                            }
                           
                            else
                            {
                                CPlayer.errorShowing = true;
                                App.showErr(App.formatToUserContent(t));
                            }

                        });


                    }
                    else
                    {

                        //App.trace("GET CALLBACK ID = " + callBackId);
                        if (callBackList.ContainsKey(callBackId))
                        {
                            var callback1 = callBackList[callBackId];
                            if (callback1 != null)
                                //callBackList.Remove(callBackId);
                                Loom.QueueOnMainThread(() =>
                                {
                                    // Use the Unity API here.
                                    callback1(inb);
                                    if (CPlayer.currMess == callBackId)
                                    {
                                        App.showLoading(false);
                                        LoadingControl.instance.currMessTime = -1;
                                    }
                                    //callBackList.Remove(callBackId);
                                });

                        }

                    }

                    callBackList.Remove(callBackId);
                    m_Messages.Enqueue(message);
                    return;


                }
                if (callBackHandlerList.ContainsKey(callBackId))
                {
                    //int first = inb.readByte();

                    var callback = callBackHandlerList[callBackId];

                    Loom.QueueOnMainThread(() =>
                    {
                        // Use the Unity API here.
                        callback(inb);
                        //LoadingControl.instance.loadingScene.SetActive(false);
                        //callBackHandlerList.Remove(callBackId);
                        LoadingControl.instance.currMessTime = 0;
                    });
                    m_Messages.Enqueue(message);
                    return;
                }

                Loom.QueueOnMainThread(() =>
                {
                    // Use the Unity API here.
                    //callback1(inb);
                    App.trace("No Handler or Mess with cmd = " + callBackId);
                    //LoadingControl.instance.loadingScene.SetActive(false);
                    //callBackList.Remove(callBackId);
                });

            }

            m_Messages.Enqueue(message);
        }
        catch
        {
            App.showErr("Kết nối bị ngắt. Vui lòng đăng nhập lại", true);
        }
    }

    /// <summary>
    /// Called when the web socket is open, and we are ready to send and receive data
    /// </summary>
    void OnOpen(WebSocket ws)
    {
        logout = false;
        m_IsConnected = true;
        App.trace("Connected ");
    }

    /// <summary>
    /// Called when we received a text message from the server
    /// </summary>
    void OnMessageReceived(WebSocket ws, string e)
    {
        App.trace("Message String Revice");
    }

    /// <summary>
    /// Called when the web socket closed
    /// </summary>
    void OnClosed(WebSocket ws, UInt16 code, string message)
    {
        Loom.QueueOnMainThread(() =>
        {
            //if (CPlayer.lobbyBtnBackIsPressed == false)
            //{
            LoadingControl.instance.currMessTime = -1;
            App.trace("Logout " + logout, "red");
            if (LoadingControl.instance != null && logout == false)
            {
                App.trace("CONNECT FAILED! FROM WS CL", "red");
                App.showErr("Mất kết nối, vui lòng đăng nhập lại", true);
                App.LogOut();
            }
            //}

        });
        m_Socket = null;
    }

    /// <summary>
    /// Called when an error occured on client side
    /// </summary>
    void OnError(WebSocket ws, Exception ex)
    {
        string errorMsg = string.Empty;
#if !UNITY_WEBGL || UNITY_EDITOR
        if (ws.InternalRequest.Response != null)
            errorMsg = string.Format("Status Code from Server: {0} and Message: {1}", ws.InternalRequest.Response.StatusCode, ws.InternalRequest.Response.Message);
#endif

        m_Socket = null;
    }
#endif
    }

}