using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class taiDLC : MonoBehaviour
{
    string uri,cC, urlVersion;
    private string KeyTxt = "Language";
    private string urlTxt = ".";
    public Handling handling;
    public List<string> ListNameDLC = new List<string>();
    public List<string> ListUrlDownLoadDLC = new List<string>();


    private string[] quickMessStr;//= { "Xin chào", "Đánh nhanh đê", "Cứ từ từ", "Vãi xoài", "Có hàng nè", "Tha cho em", "Chạy đâu", "Tưởng thế nào" };
    private string[] xocDiaMessStr; //= { "Chẵn nè!", "Xin lỗi đời quá đen", "Mua nhanh nào", "Hôm nay hên quá", "Lẻ lẻ lẻ...", "Đỏ quá đi :v", "Yếu đừng ra gió", "Chia buồn với các chú" };
    private string[] mauBinhMessStr; //= { "Vãi xoài", "Cứ từ từ", "Ôi thôi chết rồi", "Mậu binh rồi", "Nhanh còn đọ bài", "Sập 3 chi", "Xin chào", "Đời quá đen" };
    private string[] phomMessStr; //= { "Ù này", "Xin cây chốt", "Ăn đê...", "Cứ từ từ", "Mình xin", "Cho thì ăn", "Xin chào", "Đánh nhanh đê" };
    private string[] xitoMessStr; //= { "Úp nhanh còn kịp", "Tất tay đê", "Tố to vào", "Sợ rồi à", "Mình xin", "Ngon thì zô", "Xin chào", "Đen đừng hỏi" };
    private string[] xiDachMessStr; //= { "Còn non lắm", "Lại có tiền", "Vãi xoài", "Quắc mất rồi", "Bốc nhanh đê", "Đời quá đen", "Xin chào", "Đủ 21, hốt tiền rồi" };
    private string[] chanMessStr; //= { "Bình tĩnh", "Ôi thôi chết rồi", "Chờ ù, hehe", "Ăn đê", "Xin chào", "Nhất đi nhì ù", "Mình xin", "Đánh nhanh đê" };
    private string[] pokerMessStr; //= { "Úp nhanh còn kịp", "Tất tay đê", "Tố to vào", "Sợ rồi à", "Mình xin", "Ngon thì zdô", "Xin chào", "Đen đừng hỏi" };

    public static taiDLC instance;
    public long filesize_loading_inDisk, filesize_lobby_inDisk, filesize_tablelist_inDisk;
    public long filesize_loading_inserver,filesize_lobby_inserver,filesize_tablelist_inserver;

    public AssetBundleCreateRequest listScene;
    public AssetBundle[] asbs = new AssetBundle[10];
    public string[] scenes;
    public Slider slider;
    public string countryCode = "VN";
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        Application.targetFrameRate = 60;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif

        if (!App.needStartSocket)
        {
            Debug.Log("Start");
            StartCoroutine(GetDomain(() =>
            {
                if (App.ws != null)
                {
                    App.ws.getWS().Close();
                }
                App.start(uri);
                StartCoroutine(App.ws.getWS().Connect(() =>
                {
                    Debug.Log("Connect Complete..");
                    getCountryCodeURL((countryCode) =>
                    {
                        //LoadingControl.instance.GetOneSignalID();

                        getLanguageCode(() =>
                        {
                            GetDataServer(() =>
                            {
                                 handling.OnDownLoadTxt(urlTxt , ()=> {
                                   getfileVersion_Server();
                                 });
                            });

                        });
                    });

                }));
            }));
        }

    }
    private void Start()
    {
        if(App.listKeyText.Count > 0)
        {
            App.listKeyText.Clear();
        }
       // Debug.Log("TIME_LOADING : " + PlayerPrefs.GetString("TIME_LOADING"));
         //PlayerPrefs.DeleteAll();
    }
    void start_download()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(dowload_loading());
        //getfileVersion_Server();
        getFileSize_Disk();
    }
    #region Download 3 scene
    IEnumerator dowload_loading()
    {
        string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "loading_android_vn.dlc";

        Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
          + "/DLC/");
        string url = /*"https://tungahihi.imfast.io/loading.dlc" */ListUrlDownLoadDLC[6];

        //string url = ListUrlDownLoadDLC[6];

        //if (File.Exists(path) && filesize_loading_inDisk == filesize_loading_inserver)
        //{
        //    Debug.LogError("File loading da ton tai");
        //    StartCoroutine(dowload_lobby());
        //}
        //else
        //{
            WWW www = new WWW(url);
            while (!www.isDone)
            {
                Debug.Log("Download loading : " + (www.bytesDownloaded / 1000000));
                slider.value = (www.bytesDownloaded / 1000000);
                yield return null;
            }
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                yield break;
            }
#if !UNITY_WEBPLAYER
            if (www.isDone)
            {
                Debug.LogError("tai xong loading");
                File.WriteAllBytes(path, www.bytes);
                StartCoroutine(dowload_lobby());
                slider.value = 100;
            }
#endif
        //}
        //=======================================

    }
    IEnumerator dowload_lobby()
    {

        string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "lobby_android_vn.dlc";

        Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
          + "/DLC/");


        // string url = "https://tungahihi.imfast.io/lobby.dlc" /*ListUrlDownLoadDLC[1]*/;
        string url = ListUrlDownLoadDLC[3];
        //string url = App.mainUrl + App.GetDLCName(DLCName);
        //Debug.LogError(url);


        //if (File.Exists(path) && filesize_lobby_inDisk == filesize_lobby_inserver)// file da to tai
        //{
        //    StartCoroutine(dowload_Tablelist());
        //}
        //else
        //{
            WWW www = new WWW(url);
            while (!www.isDone)
            {
                Debug.Log("Download lobby : " + (www.bytesDownloaded / 1000000));
                slider.value = 35 + (www.bytesDownloaded / 1000000);
            yield return null;
            }
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                yield break;
            }

#if !UNITY_WEBPLAYER

            if (www.isDone)
            {
                Debug.LogError("tai xong lobby");
                File.WriteAllBytes(path, www.bytes);
                StartCoroutine(dowload_Tablelist());
            }
#endif
        //}
    }
    IEnumerator dowload_Tablelist()
    {

        string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "tablelist.dlc";

        Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
          + "/DLC/");


        //string url = "https://tungahihi.imfast.io/tablelist.dlc" /*ListUrlDownLoadDLC[1]*/;
        string url = ListUrlDownLoadDLC[9];
        //Debug.LogError(url);


        //if (File.Exists(path) && filesize_lobby_inDisk == filesize_lobby_inserver)// file da to tai
        //{
        //    //StartCoroutine(_EnterGameByLoadData("loading", "Loading"));
        //    //startgame();
        //}
        //else
        //{
            WWW www = new WWW(url);
            while (!www.isDone)
            {
                Debug.Log("Download tablelist : " + (www.bytesDownloaded / 1000000));
                slider.value = 57+ (www.bytesDownloaded / 1000000);
            yield return null;
            }
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                yield break;
            }

#if !UNITY_WEBPLAYER

            if (www.isDone)
            {
                Debug.LogError("tai xong tablelist");
                File.WriteAllBytes(path, www.bytes);
                //StartCoroutine(_EnterGameByLoadData("loading", "Loading"));
#endif       
                startgame();
            //}

        }
    }

    private IEnumerator _EnterGameByLoadData(string gameName, string sceneName)
    {
        //AssetBundle[] bundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
        //for (int i = 0; i < bundles.Length; i++)
        //{
        //    bundles[i].Unload(true);
        //}
        yield return new WaitForSeconds(0.5f);
        string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
            + "/DLC/" + App.GetDLCName(gameName);
        AssetBundleCreateRequest rqAB = new AssetBundleCreateRequest();
        
        rqAB = AssetBundle.LoadFromFileAsync(path);
        yield return rqAB;
        asbs[0] = rqAB.assetBundle;
        SceneManager.LoadScene(sceneName);
        Debug.LogError(asbs[0]);
    }
    // size file
    IEnumerator GetFileSize(string url, Action<long> resut)
    {
        UnityWebRequest uwr = UnityWebRequest.Head(url);
        yield return uwr.SendWebRequest();
        string size = uwr.GetResponseHeader("Content-Length");

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log("Error While Getting Length: " + uwr.error);
            if (resut != null)
                resut(-1);
        }
        else
        {
            if (resut != null)
                resut(Convert.ToInt64(size));
        }
    }
    void getFileSize_Disk()
    {
        string path_loading = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "loading_android_vn.dlc";
        string path_lobby = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "lobby_android_vn.dlc";
        string path_tablelist = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                       + "/DLC/" + "tablelist.dlc";
        var fileInfo_loading = new FileInfo(path_loading);
        var fileInfo_lobby = new FileInfo(path_lobby);
        var fileInfo_tablelist = new FileInfo(path_tablelist);

        filesize_loading_inDisk = fileInfo_loading.Length;
        filesize_lobby_inDisk = fileInfo_lobby.Length;
        filesize_tablelist_inDisk = fileInfo_tablelist.Length;
        
    }

    void getfileVersion_Server()
    {

        WebRequest req = HttpWebRequest.Create(ListUrlDownLoadDLC[6]);
        req.Method = "HEAD";
        using (System.Net.WebResponse resp = req.GetResponse())
        {
            DateTime time_Loading = Convert.ToDateTime(resp.Headers.Get(4).ToString());
            if (time_Loading.Ticks.ToString() != PlayerPrefs.GetString("TIME_LOADING", ""))
            {
                PlayerPrefs.SetString("TIME_LOADING", time_Loading.Ticks.ToString());
                Debug.Log("TIME_LOADING : " + PlayerPrefs.GetString("TIME_LOADING"));
                StartCoroutine(dowload_loading());
                slider.gameObject.SetActive(true);
            }
            else
            {
                WebRequest req1 = HttpWebRequest.Create(ListUrlDownLoadDLC[3]);
                req1.Method = "HEAD";
                using (System.Net.WebResponse resp1 = req1.GetResponse())
                {
                    DateTime time_lobby = Convert.ToDateTime(resp1.Headers.Get(4).ToString());
                    if (time_lobby.Ticks.ToString() != PlayerPrefs.GetString("TIME_LOBBY", ""))
                    {
                        PlayerPrefs.SetString("TIME_LOBBY", time_lobby.Ticks.ToString());
                        Debug.Log("TIME_LOBBY : " + PlayerPrefs.GetString("TIME_LOBBY"));
                        StartCoroutine(dowload_loading());
                    }
                    else
                    {
                        WebRequest req2 = HttpWebRequest.Create(ListUrlDownLoadDLC[9]);
                        req2.Method = "HEAD";
                        using (System.Net.WebResponse resp2 = req2.GetResponse())
                        {
                            DateTime time_tablelist = Convert.ToDateTime(resp2.Headers.Get(4).ToString());
                            if (time_tablelist.Ticks.ToString() != PlayerPrefs.GetString("TIME_TABLELIST", ""))
                            {
                                PlayerPrefs.SetString("TIME_TABLELIST", time_tablelist.Ticks.ToString());
                                Debug.Log("TIME_TABLELIST : " + PlayerPrefs.GetString("TIME_TABLELIST"));
                                StartCoroutine(dowload_loading());
                            }
                            else
                            {
                                startgame();
                            }
                        }
                    }

                }
            }
        }
    }
    
    #endregion

    //===========================================================================================================

    #region Connect Server

    public void GetDataServer(System.Action callback = null)
    {
        var outB = new OutBounMessage("ASSET_DATA.LOAD");
        outB.addHead();
        outB.writeString(App.getDevicePlatform());
        outB.writeString(countryCode);
        outB.writeString(App.getProvider());
        Debug.Log(App.getDevicePlatform() + "--" + App.getProvider() + "--" + countryCode);

        App.ws.send(outB.getReq(), delegate (InBoundMessage inB)
        {
            string connectDownLoadDataInServer = inB.readString();
            Debug.Log(connectDownLoadDataInServer + "connect");
            //string connectDownLoadDataInServer = "0";
            if (string.Compare(connectDownLoadDataInServer, "1") == 0)
            {
                // dong mini game
            }
            else
            {
                // mo mini game
            }

            urlVersion = inB.readString();
            int countDlc = inB.readInt();
            for (int i = 0; i < countDlc; i++)
            {
                string nameDLC = inB.readString();

                if (string.Compare(nameDLC, KeyTxt) == 0)
                {
                    urlTxt = inB.readString();
                }
                else
                {
                    ListNameDLC.Add(nameDLC);

                    string urlDownLoadDlc = inB.readString() + "?" + DateTime.Now;
                    ListUrlDownLoadDLC.Add(urlDownLoadDlc);
                }
            }
            callback.Invoke();
        });
    }


    private IEnumerator GetDomain(Action callback = null)
    {

        WWW www = new WWW("https://docs.google.com/document/d/e/2PACX-1vTX_6w0jNftt7tE0OLo7g4fCw1orTkpPqlqRZ_2a6uXk1BwEwbxhFNhTuipnZMCtsQftiU47U7i1GZ9/pub");

        while (!www.isDone && string.IsNullOrEmpty(www.error))
            yield return null;

        if (www.isDone)
        {
            Debug.Log("Bytes : " + www.text);
            var strSource = www.text;

            string strStart = "wss://";
            string strEnd = "/ws/gameServer";
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                uri = strStart + strSource.Substring(Start, End - Start) + strEnd;
                Debug.Log(uri);
                callback.Invoke();
            }


        }

    }
    public void getCountryCodeURL(System.Action<string> callback = null)
    {
        var req_ccurl = new OutBounMessage("IP_LOOKUP.URL");
        req_ccurl.addHead();
        req_ccurl.writeString(App.getProvider());
        App.ws.send(req_ccurl.getReq(), delegate (InBoundMessage res_ccurl)
        {
            string url = res_ccurl.readString();

            string urlIpUser = res_ccurl.readString();
            StartCoroutine(GetIPUser(urlIpUser, (_success, _text) => {

                if (_success)
                {
                    string ipuser = _text.Trim();
                }
                else
                {
                    StartCoroutine(GetIPUser("", (_successs, _textt) =>
                    {
                        string IpUser = _textt.Trim();
                    }));
                }
            }));
            Debug.Log("ilu: " + url);
            StartCoroutine(GetCountryCode(url, (_success, _text) =>
            {
                if (_success)
                {
                    string[] listText = cC.Trim().Split(' ');

                    countryCode = listText[0].Trim();
                    Debug.Log(countryCode);
                    string CountryC = countryCode;
                    if (callback != null)
                    {
                        callback.Invoke(countryCode);
                    }
                }
                else
                {
                    StartCoroutine(GetCountryCode("https://ipinfo.io/country", (_successs, _textt) =>
                    {
                        string CountryC;
                        if (cC.Trim().Length < 1)
                        {
                             CountryC = countryCode;
                        }

                        countryCode = cC.Trim();
                         CountryC = countryCode;
                        callback.Invoke(countryCode);
                    }));
                }

            }
            ));

        });
    }
    IEnumerator GetCountryCode(string url, System.Action<bool, string> callback = null)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        Debug.Log(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error + "xxx");
            if (callback != null)
            {
                callback.Invoke(false, " bi loi, khong download dc text");
            }
        }
        else
        {
            cC = www.downloadHandler.text;
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
            if (callback != null)
            {
                callback.Invoke(true, www.downloadHandler.text);
            }
        }
    }
    IEnumerator GetIPUser(string url, System.Action<bool, string> callback = null)
    {
        Debug.LogError("EnterGetIPUser");
        UnityWebRequest www = UnityWebRequest.Get(url);
        Debug.Log(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error + "xxx");
            if (callback != null)
            {
                callback.Invoke(false, " bi loi, khong download dc text");
            }
        }
        else
        {
            cC = www.downloadHandler.text;
            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
            if (callback != null)
            {
                callback.Invoke(true, www.downloadHandler.text);
            }
        }

    }
    public void getLanguageCode(Action callBack)
    {
        OutBounMessage req = new OutBounMessage("CLIENT.INFO");
        req.addHead();
        req.writeString(App.getDevicePlatform());
        req.writeString(countryCode);
        req.writeString(App.getProvider());
        App.ws.send(req.getReq(), (InBoundMessage res) =>
        {
            App.appCode = res.readString();
            App.languageCode = res.readString();

            callBack.Invoke();
        });
    }


    #endregion


    public void GetQuickChat()
    {
        string tempquickMessStr = App.listKeyText["BOTCHAT_COMMON"];
        quickMessStr = tempquickMessStr.Split('<');

        string tempxocDiaMessStr = App.listKeyText["BOTCHAT_XOCDIA"];
        xocDiaMessStr = tempxocDiaMessStr.Split('<');

        string tempmauBinhMessStr = App.listKeyText["BOTCHAT_MAUBINH"];
        mauBinhMessStr = tempmauBinhMessStr.Split('<');

        string tempphomMessStr = App.listKeyText["BOTCHAT_PHOM"];
        phomMessStr = tempphomMessStr.Split('<');

        string tempxitoMessStr = App.listKeyText["BOTCHAT_XITO"];
        xitoMessStr = tempxitoMessStr.Split('<');

        string tempxiDachMessStr = App.listKeyText["BOTCHAT_XIJACH"];
        xiDachMessStr = tempxiDachMessStr.Split('<');

        string tempchanMessStr = App.listKeyText["BOTCHAT_CHAN"];
        chanMessStr = tempchanMessStr.Split('<');

        string temppokerMessStr = App.listKeyText["BOTCHAT_POKER"];
        pokerMessStr = temppokerMessStr.Split('<');
    }

    public void startgame()
    {
        App.ws.getWS().Close();
        App.needStartSocket = false;
        StartCoroutine(_EnterGameByLoadData("loading", "Loading"));
    }
}
