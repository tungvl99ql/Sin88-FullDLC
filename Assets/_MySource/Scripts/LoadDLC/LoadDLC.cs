using Casino.Core;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net;

namespace Core.Server.Api
{
    public class LoadDLC : MonoBehaviour
    {

        public List<string> timeList;
        public static LoadDLC instance;

        //Contry Code
        public string countryCode = "VN";
        private string cC;

        //LoaddingBar
        public GameObject loading;
        public UnityEngine.UI.Slider slider;
        //MiniGameOffline
        public GameObject miniGame;

        private float valueBytesDownLoad = 0;
        private float SUMBytesDownLoad = 0;

        //Txt
        public Handling handling;
        private string urlTxt = ".";
        private string KeyTxt = "Language";

        //Field in file Version .txt
        private string urlVersion = ".";
        private int versionChest = 0;
        private int versionFish = 0;
        private int versionBird = 0;
        private int versionLobby = 0;

        private int sizeChest = 0;
        private int sizeFish = 0;
        private int sizeBird = 0;
        private int sizeLobby = 0;

        //Path to Delete DLC
        private int checkGetDataServer = 0;
        private string pathChest = "";
        private string pathBird = "";
        private string pathFish = "";
        private string pathLobby = "";
        private List<string> listPathDLC = new List<string>();

        //List name DLC to DownLoad

        public List<string> ListNameDLC = new List<string>();
        public List<string> ListUrlDownLoadDLC = new List<string>();

        private List<string> TempListNameDLC = new List<string>();

        //Coroutine download hien tai
        private Coroutine downloadDLC;

        //Domain
        private string uri = "";


        private int countListDLC = 0;

        public void MakeInstance()
        {
            if (instance == null)
            {
                instance = this;
            }
        }


        private void Awake()
        {
            //PlayerPrefs.DeleteAll();
            Application.targetFrameRate = 60;

#if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
            MakeInstance();
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
                            LoadingControl.instance.GetOneSignalID();

                            getLanguageCode(() =>
                            {

                                //Set path to Delete DLC file
                                #region Path DLC
                                pathChest = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                                + "/DLC/" + App.GetDLCName("chest" + App.languageCode.ToLower());
                                listPathDLC.Add(pathChest);
                                pathFish = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                                + "/DLC/" + App.GetDLCName("fish" + App.languageCode.ToLower());
                                listPathDLC.Add(pathFish);
                                pathBird = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                                + "/DLC/" + App.GetDLCName("bird" + App.languageCode.ToLower());
                                listPathDLC.Add(pathBird);
                                pathLobby = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                                + "/DLC/" + App.GetDLCName("lobby" + App.languageCode.ToLower());
                                listPathDLC.Add(pathLobby);
                                #endregion

                                GetDataServer(() =>
                                {

                                    if (loading.activeInHierarchy == true)
                                    {
                                        slider.value = 0;
                                    }
                                    Debug.LogError("<color='yellow'>" + miniGame.activeInHierarchy + "</color>");
                                    if (miniGame.activeInHierarchy == true) return;

                                    //handling.OnDownLoadTxt(urlTxt);
                                    //CheckNewUpdateData();
                                    StartCoroutine(_EnterLobby("lobbyvn", "Lobby"));
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/phom.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/maubinh.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/tlmn.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/poker.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/xocdia.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/phom.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/maubinh.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/tlmn.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/poker.dlc");
                                    //ListUrlDownLoadDLC.Add("https://tungahihi.imfast.io/xocdia.dlc");

                                });

                            });
                        });

                    }));
                }));
            }
        }
        private void ReLoadGetData()
        {
            if (countryCode != "") return;
            TaskUtil.Delay(this, delegate
            {
                GetDataServer();
            }, 2f);
        }


        //private void OnApplicationPause(bool pause)
        //{
        //    if(pause)
        //    {
        //        App.ws.getWS().Close();
        //    }
        //    else
        //    {
        //        App.ws.getWS().Connect();
        //    }
        //}
        private void OnApplicationQuit()
        {
            App.ws.getWS().Close();
        }

        private void Start()
        {

        }
        
        public void OnStart()
        {
            //FakeVersion();
            //SceneManager.LoadScene("Lobby");
            //OnClose();
            StartCoroutine(_EnterGameByLoadData("lobby" + countryCode.ToLower(), "Lobby"));
        }

        public void OnMiniGame()
        {
            loading.gameObject.SetActive(false);
            miniGame.gameObject.SetActive(true);
        }

        public void OnClose()
        {
            this.gameObject.SetActive(false);
        }

        public void OnLoadDLC()
        {
            OnClose();
        }


        #region DOMAIN
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
        #endregion


        #region COMMUNICATION SERVER
        public void LoadAllDataServer()
        {
            GetDataServer();
        }

        bool isConnect;
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
                //Debug.Log(inB.readByte() + "<color='yellow'> Check Connect...</color>");

                string connectDownLoadDataInServer = inB.readString();
                Debug.Log(connectDownLoadDataInServer + "connect");
                //string connectDownLoadDataInServer = "0";
                if (string.Compare(connectDownLoadDataInServer, "1") == 0)
                {
                    loading.gameObject.SetActive(true);
                }
                else
                {
                    miniGame.gameObject.SetActive(true);
                }

                urlVersion = inB.readString();
                //Debug.Log(urlVersion + "version");
                int countDlc = inB.readInt();
                //Debug.Log(countDlc + "count");
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
                        //------
                        WebRequest req8 = HttpWebRequest.Create(urlDownLoadDlc);
                        req8.Method = "HEAD";
                        using (System.Net.WebResponse resp8 = req8.GetResponse())
                        {
                            DateTime time = Convert.ToDateTime(resp8.Headers.Get(4).ToString());
                            timeList.Add(time.Ticks.ToString());
                        }
                    }
                }
                callback.Invoke();
            });
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
        public void getCountryCodeURL(System.Action<string> callback = null)
        {
            var req_ccurl = new OutBounMessage("IP_LOOKUP.URL");
            req_ccurl.addHead();
            req_ccurl.writeString(App.getProvider());
            App.ws.send(req_ccurl.getReq(), delegate (InBoundMessage res_ccurl)
            {
                string url = res_ccurl.readString();

                string urlIpUser = res_ccurl.readString();
                StartCoroutine(GetIPUser(urlIpUser, (_success, _text) =>
                {

                    if (_success)
                    {
                        LoadingControl.instance.IpUser = _text.Trim();
                    }
                    else
                    {
                        StartCoroutine(GetIPUser("", (_successs, _textt) =>
                        {
                            LoadingControl.instance.IpUser = _textt.Trim();
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
                        LoadingControl.instance.CountryC = countryCode;
                        if (callback != null)
                        {
                            callback.Invoke(countryCode);
                        }
                    }
                    else
                    {
                        StartCoroutine(GetCountryCode("https://ipinfo.io/country", (_successs, _textt) =>
                        {

                            if (cC.Trim().Length < 1)
                            {
                                LoadingControl.instance.CountryC = countryCode;

                            }

                            countryCode = cC.Trim();
                            LoadingControl.instance.CountryC = countryCode;
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


        #endregion


        #region VERSION
        private void CheckNewUpdateData()
        {
            StartCoroutine(CheckVersionLoadData(() =>
            {

                bool checkDlcInFolder = CheckDLCInFolder();

                if (checkDlcInFolder == true)
                {
                    Debug.Log("<color='yellow'>File da ton tai ! Load Scene...</color>");
                    slider.DOValue(1, 1.5f).OnComplete(delegate { OnStart(); });
                }
                else
                {
                    Debug.Log("<color='yellow'>Chua Down " + TempListNameDLC.Count + " DLC! Start Download...</color>");

                    DownLoadAllDLCFile();
                }
            }));


        }
        private IEnumerator CheckVersionLoadData(Action callback = null)
        {

            while (string.Compare(urlVersion, ".") == 0)
                yield return 0;
            //URL check Version
            string urlCheckVersion = urlVersion + "?" + DateTime.Now.ToString();


            WWW www = new WWW(urlCheckVersion);
            Debug.Log("<color='red'>" + urlCheckVersion + "</color>");
            yield return www;

            if (www.error != null)
            {
                Debug.Log("Load text check data error.....");
            }
            else if (www.isDone)
            {
                // Format File .txt: TenGame:Version:Kich Thuoc
                string[] textVersion = www.text.Split(',');
                for (int i = 0; i < textVersion.Length; i++)
                {
                    Debug.Log("<color='red'>" + textVersion[i] + "</color>");

                    if (textVersion[i].Contains("Chest_" + App.languageCode.ToLower()))
                    {

                        string vsChest = textVersion[i];
                        string[] _vsChest = vsChest.Split(':');
                        versionChest = Int32.Parse(_vsChest[1].Trim());
                        sizeChest = Int32.Parse(_vsChest[2].Trim());
                        if (PlayerPrefs.GetInt("VersionChest", 0) < versionChest)
                        {
                            DeleteOldData("chest");
                        }
                    }
                    if (textVersion[i].Contains("Fish_" + App.languageCode.ToLower()))
                    {
                        string vsFish = textVersion[i];
                        string[] _vsFish = vsFish.Split(':');
                        versionFish = Int32.Parse(_vsFish[1].Trim());
                        sizeFish = Int32.Parse(_vsFish[2].Trim());
                        if (PlayerPrefs.GetInt("VersionFish", 0) < versionFish)
                        {
                            DeleteOldData("fish");
                        }
                    }
                    if (textVersion[i].Contains("Bird_" + App.languageCode.ToLower()))
                    {
                        string vsBird = textVersion[i];

                        string[] _vsBird = vsBird.Split(':');

                        versionBird = Int32.Parse(_vsBird[1].Trim());
                        sizeBird = Int32.Parse(_vsBird[2].Trim());

                        if (PlayerPrefs.GetInt("VersionBird", 0) < versionBird)
                        {
                            DeleteOldData("bird");
                        }
                    }
                    if (textVersion[i].Contains("Lobby_" + App.languageCode.ToLower()))
                    {
                        string vsLobby = textVersion[i];

                        string[] _vsLobby = vsLobby.Split(':');

                        versionLobby = Int32.Parse(_vsLobby[1].Trim());
                        sizeLobby = Int32.Parse(_vsLobby[2].Trim());

                        if (PlayerPrefs.GetInt("VersionLobby", 0) < versionLobby)
                        {
                            DeleteOldData("lobby");
                        }
                    }
                }
                SUMBytesDownLoad = (float)(sizeBird + sizeChest + sizeFish + sizeLobby);
                callback.Invoke();
            }
        }


        private void SaveVersionDLCFileByName(string name)
        {
            switch (name)
            {
                case "Chest":
                    PlayerPrefs.SetInt("VersionChest", versionChest);
                    break;
                case "Fish":
                    PlayerPrefs.SetInt("VersionFish", versionFish);
                    break;
                case "Bird":
                    PlayerPrefs.SetInt("VersionBird", versionBird);
                    break;
                case "Lobby":
                    PlayerPrefs.SetInt("VersionLobby", versionLobby);
                    break;

                default:
                    break;
            }

        }

        #endregion


        int countDLC = 0;

        #region DOWN LOAD DATA
        public void DownLoadAllDLCFile()
        {
            downloadDLC = StartCoroutine(DownLoadData(TempListNameDLC[countDLC]));
        }

        private IEnumerator DownLoadData(string DLCName)
        {
            while (SUMBytesDownLoad == 0)
                yield return 0;

            Debug.Log(sizeBird + "--" + sizeChest + "--" + sizeFish + "--" + sizeLobby);


            int lastBytesDownLoad = 0, currentBytesDownLoad = 0;

            float maxTimeOut = 10f, timeOut = 0f;

            Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                    + "/DLC/");

            Debug.Log("<color='yellow'>Down load " + DLCName + ".dlc </color>");

            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                + "/DLC/" + App.GetDLCName(DLCName.ToLower() + countryCode.ToLower());

            string url = "";
            for (int i = 0; i < ListNameDLC.Count; i++)
            {
                if (string.Compare(DLCName, ListNameDLC[i]) == 0)
                {
                    url = ListUrlDownLoadDLC[i];
                    break;
                }
            }
            //string url = App.mainUrl + App.GetDLCName(DLCName);

            Debug.LogError(url);

            WWW www = new WWW(url);

            while (!www.isDone && string.IsNullOrEmpty(www.error))
            {
                currentBytesDownLoad = www.bytesDownloaded;

                if (lastBytesDownLoad < currentBytesDownLoad)
                {
                    valueBytesDownLoad += currentBytesDownLoad - lastBytesDownLoad;

                    lastBytesDownLoad = currentBytesDownLoad;
                    slider.DOValue((valueBytesDownLoad / SUMBytesDownLoad), 0.2f);

                    Debug.Log((valueBytesDownLoad / SUMBytesDownLoad) * 100 + DLCName);

                    timeOut = 0f;
                }

                timeOut += Time.deltaTime;

                if (timeOut >= maxTimeOut)
                {
                    //Chua check duoc
                    Debug.LogError("Đang tải lại DLC " + DLCName + "....");

                    StopCoroutine(downloadDLC);

                    downloadDLC = StartCoroutine(DownLoadData(DLCName));

                }
                yield return null;
            }

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);


                //App.showErr("Đã xảy ra lỗi từ máy chủ.");
                App.showErr(App.listKeyText["WARN_SERVER_ERROR"]);
                Application.Quit();

                yield break;
            }

#if !UNITY_WEBPLAYER
            if (www.isDone)
            {
                Debug.Log("Đã tải xong." + DLCName);
                SaveVersionDLCFileByName(DLCName);
                File.WriteAllBytes(path, www.bytes);
                countDLC++;
                if (countDLC < TempListNameDLC.Count)
                {
                    DownLoadAllDLCFile();
                }
                else if (countDLC >= TempListNameDLC.Count)
                {
                    slider.DOValue(1, 0.2f);
                    OnStart();
                }
            }
#endif
        }


        #endregion

        #region LOAD DATA

        private IEnumerator _EnterGameByLoadData(string gameName, string sceneName)
        {
            yield return new WaitForSeconds(0.5f);
            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                + "/DLC/" + App.GetDLCName(gameName);

            Caching.ClearCache();
            var listScene = AssetBundle.LoadFromFileAsync(path);
            yield return listScene;
            //string[] scenes = listScene.assetBundle.GetAllScenePaths();

            LoadingControl.instance.asbs[0] = listScene.assetBundle;
            SceneManager.LoadScene(sceneName);
        }

        #endregion

        #region MAINPULATION WITH FODER


        private bool CheckDLCInFolder()
        {

            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                + "/DLC/";
            Debug.Log("<color='red'> " + path + " </color>");
            for (int i = 0; i < ListNameDLC.Count; i++)
            {
                Debug.Log("<color='red'>" + path + App.GetDLCName(ListNameDLC[i].ToLower() + App.languageCode.ToLower()) + "</color>");
                if (!File.Exists(path + App.GetDLCName(ListNameDLC[i].ToLower() + App.languageCode.ToLower())))
                {
                    TempListNameDLC.Add(ListNameDLC[i]);
                }
            }

            if (TempListNameDLC.Count > 0)
            {
                return false;
            }

            return true;
        }
        private void DeleteOldData(string gameName)
        {
            switch (gameName)
            {
                case "chest":
                    DeleteFile(pathChest);
                    break;
                case "fish":
                    DeleteFile(pathFish);
                    break;
                case "bird":
                    DeleteFile(pathBird);
                    break;
                case "lobby":
                    DeleteFile(pathLobby);
                    break;
            }

        }
        private void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }



        #endregion


        private IEnumerator _EnterLobby(string gameName, string sceneName)
        {
            yield return new WaitForSeconds(0.5f);
            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                + "/DLC/" + App.GetDLCName(gameName);

            Caching.ClearCache();
            var listScene = AssetBundle.LoadFromFileAsync(path);

            yield return listScene;
            var scenes = listScene.assetBundle.GetAllScenePaths();
            SceneManager.LoadScene(scenes[0]);
        }



        

    }
      
}
