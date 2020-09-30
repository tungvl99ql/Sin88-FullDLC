using DG.Tweening;
using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using Core.Server.Api;
using System.Net;
using UnityEngine.Networking;

namespace Core.Server.Api
{
    public partial class LobbyControl : MonoBehaviour
    {

        public GameObject settingBtnGO;
        public GameObject imageTopBar;

        public PanelCheckPassLV2 panelCheckPassLV2;

        #region Difference

        public static LobbyControl instance;

        //public Text[] luckyWheelText;
        //public Text manText;
        //public Text chipText;
        //public GameObject LobbyScene;

        /// <summary>
        /// 0: Nạp tiền|1: Đổi thưởng | 2 freechip khi chua xac thuc | 3 freechip
        /// </summary>
        //public Button[] btnList;

        //private void setBalance(string type)
        //{
        //    if (type == "man" && LobbyControl.instance != null)
        //        luckyWheelText[4].text = manText.text = CPlayer.manBalance >= 0 ? App.formatMoney(CPlayer.manBalance.ToString()) : "0";
        //    if (type == "chip" && LobbyControl.instance != null)
        //    {
        //        luckyWheelText[3].text = chipText.text = CPlayer.chipBalance >= 0 ? App.formatMoney(CPlayer.chipBalance.ToString()) : "0";

        //    }

        //}

        public void doAuthen()
        {

            LoadingControl.instance.btnDoConfirm.onClick.RemoveAllListeners();
            LoadingControl.instance.btnDoConfirm.onClick.AddListener(() =>
            {
                LoadingControl.instance.closeConfirmDialog();
                CPlayer.needRetryToLoadPhone = true; //khi nhan tin xac thuc thi can reload lai game khi nguoi choi an nut back
                //btnList[2].transform.gameObject.SetActive(false); //khi click vao man hinh chuyen sang tin nhan thi ko hien nut free chip xac thuc nua
                string url = "sms:7069?body=CGV XT " + CPlayer.id;
#if UNITY_IOS
            url = string.Format("sms:{0}?&body={1}", 7069, System.Uri.EscapeDataString("CGV XT " + CPlayer.id));
#endif
                Application.OpenURL(url);
            });
            //LoadingControl.instance.confirmText.text = PlayerPrefs.GetString("confirmText3"); //"- Xác thực số điện thoại để bảo mật tài khoản và kích hoạt VIP\n- Tặng ngay 2000 Chip sau khi xác thực thành công\n- Nhận miễn phí VQMM hàng ngày\n(*)Phí: 1000 vnđ";
            LoadingControl.instance.blackPanel.SetActive(true);
            LoadingControl.instance.confirmDialogAnim.Play("DialogAnim");
        }

        #endregion

        //public Text txt;
        //public RectTransform rtf;

        /// <summary>
        /// 0-1:eventSlide|2-3: topSlide
        /// </summary>
        //public Sprite[] lobbySprtList;

        /// <summary>
        /// 0: ava|1-2:ldtMnp|3-4: ldXeng|5-6: ldCtn|7-8: ldZda|9-10: ldFrt|11-12: ldSl7|13: eventSlide|14: topSlide
        /// </summary>
        public Image[] lobbyImgList;

        /// <summary>
        /// 0: userName|1: money|2: MessNum|3: HeadLine|4-19: potvalue|20: noti-num
        /// |21: honor|22:ldtMnp|23: ldtXeng|24: ldtCtn|25: ldtZda|26: ldtFrt|27: ldtSl7
        /// </summary>
        public Text[] lobbyTxtList;

        /// <summary>
        /// 0: log|
        /// </summary>
        //public Button[] lobbyBtnList;
        /// <summary>
        /// 0: btnNoti + btnSetting|1: Free + Shop|2: UserInfo|3: Zones
        /// </summary>
        public RectTransform[] lobbyRtfListl;

        /// <summary>
        /// Setting|1 :inbox|2: freeChip|3: chat|4: shop|5: recharge|6: high-light|7: eventSlide|8: topSlide||9-11: topSlideButton|12: topSlideElement
        /// </summary>
        public GameObject[] lobbyGojList;

        /// <summary>
        /// Event icon 
        /// </summary>
        public GameObject[] lobbyGojEventList;
        public GameObject[] lobbyGojEventLightList;

        /// <summary>
        /// Pot Text Value
        /// </summary>
        public Text[] lobbyPotText;

        /// <summary>
        /// Image Top Pot Slide
        /// </summary>
        //public Sprite[] lobbSprtsTopSlide;
        [Space]
        [Header("----------TOP HU---------- ")]
        public Sprite game1Icon;
        public Sprite game2Icon;
        public Sprite game3Icon;
        public Sprite game4Icon;
        public Sprite game5Icon;
        public Sprite game6Icon;
        public Sprite game7Icon;
        [Space]

        public Material[] mts;
        public ParticleSystemRenderer bgfx;

        [HideInInspector]
        //public static LobbyControl instance;

        private IEnumerator secondHalf;
        private int[] potValues = new int[24];

        private bool openRechare = false;


        public LoopText loopText;

        /// <summary>
        /// 0: fake-pot-change | 1: fake-top-pot | 2: text num fake top pot
        /// </summary>
        private IEnumerator[] threads = new IEnumerator[4];

        private int[] fakePotData = { 535051, 5578442, 9249616, 80071503, 550025, 4149930, 9810709, 98206844, 777215, 4755536, 6377172, 117320110, 824931, 6207840, 93067324, 667659, 8171007, 108177596, 965979, 8435642, 70763592, 987358, 5463971, 75903864 };//Top Hu khung giua
        private int[] fakePot100 = { 535051, 550025, 777215, 824931, 667659, 965979, 987358 };//Top 100
        private int[] fakePot500 = { 5578442, 4149930, 4755536 };//Top 500
        private int[] fakePot1K = { 9249616, 9810709, 6377172, 6207840, 8171007, 8435642, 5463971 };//Top 1000
        private int[] fakePot10K = { 80071503, 98206844, 117320110, 93067324, 108177596, 70763592, 75903864 };//Top 500

        private List<int[]> fakePot = new List<int[]>();
        private int fakePotId = 0;
        private int fakeTopPotId = 0;
        private int fakeTopPot500Id = 1;
        public Text[] lobbyFakeTopPotTxts = new Text[7];

        private List<string> honored_ls = new List<string>();

        private int[] intNum = new int[2];
        private Text[] lobbyPotTextSlide = new Text[24];
        private string[] gameCodeXpot;
        public string countryCode;
        public string forgetPassURL;
        private string cC; //countryCode để get

        public InputField passwordLV2;

        void getInstance()
        {
            if (instance != null)
                Destroy(gameObject);
            else
            {
                instance = this;
                //DontDestroyOnLoad(gameObject);
            }

        }

        private void Awake()
        {

            if (!App.needStartSocket)
            {
                //App.start();
                StartCoroutine(App.ws.getWS().Connect(() =>
                {
                    getCountryCodeURL();
                    getForgetPassURL();
                }));
            }
            StartCoroutine(checkVer());
            //checkShowButton();           
            getXpot();


           // pathChest = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
           // + "/DLC/" + App.GetDLCName("chest");
           // pathFish = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
           //+ "/DLC/" + App.GetDLCName("fish");
           // pathBird = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
           //+ "/DLC/" + App.GetDLCName("bird");
            getInstance();

            //url = App.versionCheck + (Application.platform == RuntimePlatform.IPhonePlayer ? "version_ios.txt" : "version.txt?time="+Time.time.ToString());
            //Debug.LogError(url);

            fbToken = "";
            GetFBId();
            
#if UNITY_WEBGL
        Application.ExternalCall("loginLoaded");
#endif
        }


        public void GetFBId()
        {
            OutBounMessage req = new OutBounMessage("FACEBOOK.GET_INFO");

            req.addHead();
            req.writeString(App.getDevicePlatform());
            req.writeString(LoadingControl.instance.CountryC);
            req.writeString(App.getProvider());

            App.ws.send(req.getReq(), (InBoundMessage res) =>
            {
                if (!FB.IsInitialized)
                {
                    // Initialize the Facebook SDK
                    //FB.Init(InitCallback, OnHideUnity);

                    string tempIdFb = res.readString();
                    Debug.Log("Facebook ID : " + tempIdFb);
                    FB.Init(tempIdFb);

                }
                else
                {
                    // Already initialized, signal an app activation App Event
                    FB.ActivateApp();

                }
            });
        }

        private void OnEnable()
        {
            SoundManager.instance.PlayBackgroundSound(SoundFX.LOBBY_BG_MUSIC);
        }
        public GameObject button_PlayNow, button_Reg, button_Loggin;
        private void Start()
        {
            foreach (var item in App.listKeyText)
            {
                Debug.Log("<color=red>[List Key]</color> Key : " + item.Key + " - Value : " + item.Value);
            }
            //imageTopBar.SetActive(false);
            //ButtonTopHu.instance.Hiden();

#if UNITY_ANDROID
            lobbyGojList[13].SetActive(true);
#elif UNITY_IOS
        lobbyGojList[13].SetActive(true);
#else
        button_PlayNow.SetActive(false);
        button_Reg.transform.localPosition = new Vector3(-200,-10,0);
#endif
            //PlayerPrefs.SetInt("VersionAvenger", 0);
            //PlayerPrefs.SetInt("VersionDragonBall", 0);
            //PlayerPrefs.SetInt("VersionMonkeyKing", 0);
            //if (LoadingControl.instance.loadingGojList[31].activeSelf)
            //    LoadingControl.instance.loadingGojList[31].SetActive(false);
            if (LoadDLC.instance.gameObject.activeSelf)
                LoadDLC.instance.OnClose();


            fakePot.Add(fakePot100);
            fakePot.Add(fakePot500);
            fakePot.Add(fakePot1K);
            fakePot.Add(fakePot10K);

            //fakePotId = UnityEngine.Random.Range(0, 3);
            //ChangeTopSlide(3);
            lobbyTxtList[21].text = "";

            bgfx.material = mts[0];
            if (LoadingControl.instance.loadingGojList[30].activeSelf)
                LoadingControl.instance.loadingGojList[30].SetActive(false);
            //CPlayer.changed += BalanceChanged;
            settingBtnGO.SetActive(CPlayer.logedIn);

            //CheckNewUpdateData();
            _getInfo();

            ChangeTopSlide(0);

            if (CPlayer.logedIn == false)
            {

                OpenButtonIsLogin(CPlayer.logedIn);
                
                threads[0] = FakePotChange();
                StartCoroutine(threads[0]);

                //#if UNITY_ANDROID || UNITY_IOS
            }
            else
            {
                if (CPlayer.showEvent)
                {
                    if (!CPlayer.hadShowEvent)
                        LoadingControl.instance.loadingGojList[29].SetActive(false);
                    else
                        LoadingControl.instance.loadingGojList[29].SetActive(true);
                }
                button_Reg.transform.parent.gameObject.SetActive(false);

                
            }

        }
        
        private string url = ""; // <-- enter your url here
        private string pathChest = "";
        private string pathFish = "";
        private string pathBird = "";
        private int versionChest = 0;
        private int versionFish = 0;
        private int versionBird = 0;
        private int sizeChest = 0;
        private int sizeFish = 0;
        private int sizeBird = 0;
        private int sizeFile = 0;
        private void CheckNewUpdateData()
        {
            // url = App.versionCheck;
            StartCoroutine(CheckVersionLoadData());
        }

        private IEnumerator CheckVersionLoadData()
        {
            //Debug.Log(url + " " + App.versionCheck);

            WWW www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                Debug.Log("Load text check data error.....");
            }
            else
            {
                // Line: TenGame | Version | Kich Thuoc,
                string[] textVersion = www.text.Split(',');
                for (int i = 0; i < textVersion.Length; i++)
                {

                    Debug.Log("<color='red'>" + textVersion[i] + "</color>");
                    if (textVersion[i].Contains("Chest"))
                    {

                        string vsChest = textVersion[i];
                        string[] _vsChest = vsChest.Split(':');
                        versionChest = Int32.Parse(_vsChest[1].Trim());
                        sizeChest = Int32.Parse(_vsChest[2].Trim());
                        //  Debug.Log("versionChest " + versionChest + "  Int32.Parse(_vsChest[1].Trim()) " + Int32.Parse(_vsChest[2].Trim()));
                        if (PlayerPrefs.GetInt("VersionChest", 0) < versionChest)
                        {
                            PlayerPrefs.SetInt("VersionBird", versionBird);
                            DeleteOldData("chest");
                        }
                    }
                    if (textVersion[i].Contains("Fish"))
                    {
                        string vsFish = textVersion[i];
                        string[] _vsFish = vsFish.Split(':');
                        versionFish = Int32.Parse(_vsFish[1].Trim());
                        sizeFish = Int32.Parse(_vsFish[2].Trim());
                        // Debug.Log("VersionFish " + versionFish + "  Int32.Parse(_vsChest[1].Trim()) " + Int32.Parse(_vsFish[2].Trim()));
                        if (PlayerPrefs.GetInt("VersionFish", 0) < versionFish)
                        {
                            PlayerPrefs.SetInt("VersionBird", versionBird);
                            DeleteOldData("fish");
                        }
                    }
                    if (textVersion[i].Contains("Bird"))
                    {
                        string vsBird = textVersion[i];
                        
                        string[] _vsBird = vsBird.Split(':');

                        versionBird = Int32.Parse(_vsBird[1].Trim());
                        sizeBird = Int32.Parse(_vsBird[2].Trim());
                        //  Debug.Log("VersionBird " + versionBird + "  Int32.Parse(_vsChest[1].Trim()) " + Int32.Parse(_vsBird[2].Trim()));
                        if (PlayerPrefs.GetInt("VersionBird", 0) < versionBird)
                        {
                            PlayerPrefs.SetInt("VersionBird", versionBird);
                            DeleteOldData("bird");
                        }
                    }
                }

            }
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
            }

        }
        private void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void showLogPanel(string type)
        {
            //LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.RemoveAllListeners();
            //LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.AddListener(() => LoadingControl.instance.showErrorPanel());
            //LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.RemoveAllListeners();
            //LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.AddListener(() => LoadingControl.instance.showErrorPanel2());
            getCountryCodeURL();
            getForgetPassURL();         
            if (type == "reg")
                LoadingControl.instance.showRegPanel();
            else if (type == "log")
                LoadingControl.instance.showLogPanel();
        }

        /// <summary>
        /// Set user info after success log
        /// </summary>
        private void setLogInfo()
        {
            //string name = CPlayer.fullName.Length == 0 ? CPlayer.nickName : CPlayer.fullName;
            string name = CPlayer.nickName;
          //  name = App.formatNickName(name, 9);
            lobbyTxtList[0].text = name;
            //lobbyTxtList[1].text = App.formatMoney(CPlayer.chipBalance.ToString());
            StartCoroutine(App.loadImg(lobbyImgList[0], App.getAvatarLink2(CPlayer.avatar + "", (int)CPlayer.id), true));
            lobbyImgList[0].transform.parent.parent.gameObject.SetActive(true);
        }

        private IEnumerator _login(string loginType, string nick = "", string pass = "", bool toRemember = false)
        {
            LoadingControl.instance.showProcessing(true);
            CPlayer.loginType = loginType;

            string t0 = App.appCode;
            string t2 = App.languageCode;
            string t3 = "";
            string t4 = "";

            var reqqq = new OutBounMessage(loginType == "fb" ? "LOGIN_NEW" : "LOGIN_EX");
            reqqq.addHead();
            reqqq.writeAcii(t0);
            reqqq.writeAcii(t2);
            
            //t3 = user.text;
            //t4 = pass.text;

            //App.start();

            switch (loginType)
            {
                case "log":
                    t3 = nick.ToLower();
                    t4 = pass;
                    reqqq.writeByte(0);
                    reqqq.writeAcii(t3);
                    reqqq.writeString(t4);
                    reqqq.writeString(App.getProvider());
                    reqqq.writeString(countryCode);
                    break;
                case "fb":
                    reqqq.writeByte(12);
#if UNITY_ANDROID
                    reqqq.writeString(SystemInfo.deviceUniqueIdentifier.ToLower());//device id
                    reqqq.writeLongAcii(fbToken);
                    reqqq.writeAcii(App.getProvider());
                    reqqq.writeString(countryCode);
#elif UNITY_IOS
        /*string deviceId = DeviceIDManager.GetDeviceID();
        reqqq.writeString(deviceId.ToLower());//device id*/
                 string deviceId = DeviceIDManager.GetDeviceID();
        reqqq.writeString(deviceId.ToLower());//device id
                 reqqq.writeLongAcii(fbToken);
                reqqq.writeAcii(App.getProvider());
                    reqqq.writeString(countryCode);

#elif UNITY_WEBGL
                reqqq.writeString("");
                reqqq.writeLongAcii(fbToken);
                reqqq.writeAcii(App.getProvider());
#endif

                    //CPlayer.fbToken = fbToken;
                    break;
                default:
                    break;
            }
            reqqq.writeString(LoadingControl.instance.userID);
            reqqq.writeByte(1);

            if (!App.ws.getWS().isConnecting())
            {
                yield return StartCoroutine(App.ws.getWS().Connect());
            }

            Debug.Log(LoadingControl.instance.IpUser +"IPPPPP");
            reqqq.writeString(LoadingControl.instance.IpUser);

            App.ws.send(reqqq.getReq(), delegate (InBoundMessage res)
            {
                App.trace("LOG", "red");
                LoadingControl.instance.loadingGojList[21].SetActive(true);


                CPlayer.logedIn = true;
                settingBtnGO.SetActive(true);
                button_Reg.transform.parent.gameObject.SetActive(false);

                PlayerPrefs.SetString("user", nick);

                if (toRemember)
                {
                    PlayerPrefs.SetString("rememberPass", "true");
                    PlayerPrefs.SetString("pass", pass);
                }
                else
                {
                    PlayerPrefs.SetString("rememberPass", "false");
                    PlayerPrefs.SetString("pass", "");
                }

                App.trace("[RECV] LOGIN");
                CPlayer.id = res.readLong();
                CPlayer.cdn = res.readAscii();
                CPlayer.currPath = res.readString();
                CPlayer.currPass = res.readString();
                res.readByte();
                res.readByte();
                res.readByte();
                var unRead = res.readInt();
                LoadingControl.instance.showProcessing(false);
                OpenTopSlide();
                _getInfo();
                OpenTaiXiu();
                activeUnreadMail(unRead);
            });
        }
        public GameObject button_DoiThuong, button_MoneyTransfer;
        private void OpenButtonIsLogin(bool active)
        {
            button_DoiThuong.SetActive(active);
            button_MoneyTransfer.SetActive(active);
        }
        private void _getInfo()
        {
            LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.RemoveAllListeners();
            LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.RemoveAllListeners();
            LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.AddListener(() => LoadingControl.instance.showErrorPanel());
            LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.AddListener(() => LoadingControl.instance.showErrorPanel2());
            setHandler();
#if UNITY_WEBGL
        Application.ExternalCall("loginCompleted");
#endif
            if (threads[0] != null)
                StopCoroutine(threads[0]);
            if (threads[1] != null)
                StopCoroutine(threads[1]);
#if UNITY_ANDROID || UNITY_IOS
            //CheckLoadData("chest");
            //CheckLoadData("fish");
            //CheckLoadData("bird");
#endif
            //Debug.Log("START GET USER INFO");
            var req_info = new OutBounMessage("PLAYER_PROFILE");
            //Debug.Log("WRITE LONG = " + CPlayer.id);
            //App.trace("PLAYER ID = " + CPlayer.id);
            req_info.addHead();
            req_info.writeLong(CPlayer.id);
            req_info.writeByte(0x0f);
            req_info.writeAcii("");


            App.ws.send(req_info.getReq(), delegate (InBoundMessage res)
            {
                string userName = res.readAscii();
                CPlayer.nickName = userName;
                //App.trace("userName = " + userName);
                CPlayer.fullName = res.readString();

                //App.trace("FullName = " + res.readString());
                string avatar = res.readAscii();
                CPlayer.avatar = avatar;


                



                res.readByte();
                res.readAscii();            //date of birth
                res.readString();           //message - status
                long chip = res.readLong();
                //App.trace("chip = " + chip);
                CPlayer.chipBalance = chip;
                MoneyManager.instance.OnLoginUpdateCurrCoin();
                long man = res.readLong();
                //App.trace("man = " + man);
                //Debug.Log("chip = " + chip);
                //Debug.Log("man = " + man);
                CPlayer.manBalance = man;
                CPlayer.phoneNum = res.readAscii();
                //LoadingControl.instance.blackkkkkk.SetActive(true);
                //StartCoroutine(openLobby());

                //Other
                res.readAscii();
                res.readAscii();
                res.readAscii();
                res.readByte();
                res.readAscii();
                res.readString();
                res.readAscii();
                res.readString();
                res.readLong();

                checkShowButton();

                LoadingControl.instance.bandInvitation = PlayerPrefs.GetInt("invite", 0) == 1;
                //imageTopBar.SetActive(true);

                ButtonTopHu.instance.Show();

                setLogInfo();

                GetHeadLine();

                GetGameInfo();

                getXpot();

                //GetGameInfo2();
                GetNotiCount();
                //SoundManager.instance.PlayUISound(SoundFX.BG_LOBBY);
                //ChangeTopSlide(1);

                //CheckTypeUser 0: thuong , 1:Dai ly
                CPlayer.typeUser = res.readInt();
                Debug.Log(CPlayer.typeUser+" - Type User");

            });
        }
        public void checkShowButton()
        {
#if UNITY_ANDROID
            var buttonStates = new OutBounMessage("BONUS_BUTTON.SHOW");
            buttonStates.addHead();
            buttonStates.writeString(App.getProvider());            
            buttonStates.writeString(App.getVersion());
            buttonStates.writeString(LoadingControl.instance.CountryC);
            App.ws.send(buttonStates.getReq(), delegate (InBoundMessage button_rev)
            {
                int b1 = button_rev.readInt();//Nút chuyển tiền 0:Ẩn | 1:Hiện
                int b2 = button_rev.readInt();//Nút đổi thưởng 0:Ẩn | 1:Hiện

                Debug.Log("B1 : " + b1 + " -- b2 " + b2);
                if (b1 == 0 && b2 == 0)
                {
                    //button_TaiXiu.SetActive(true);
                    button_MoneyTransfer.SetActive(false);
                    button_DoiThuong.SetActive(false);
                }
                if (b1 == 1 && b2 == 1)
                {
                    //button_TaiXiu.SetActive(false);
                    button_MoneyTransfer.SetActive(true);
                    button_DoiThuong.SetActive(true);
                }
                if( b1 == 0 && b2 == 1)
                {
                    button_MoneyTransfer.SetActive(false);
                    button_DoiThuong.SetActive(true);
                }
                if ( b1 == 1 && b2 == 0)
                {
                    button_MoneyTransfer.SetActive(true);
                    button_DoiThuong.SetActive(false);
                }


            });
#else
           // button_TaiXiu.SetActive(false);
            button_MoneyTransfer.SetActive(true);
            button_DoiThuong.SetActive(true);
#endif
        }
        private void activeUnreadMail(int num)
        {
            if (num > 0)
            {
                lobbyTxtList[2].transform.parent.gameObject.SetActive(true);
                lobbyTxtList[2].text = "1+";
            }
            else
            {
                lobbyTxtList[2].transform.parent.gameObject.SetActive(false);
            }
        }
        private void GetNotiCount()
        {
            App.trace("[SEND] PM.READ", "red");
            var req_NotiMess = new OutBounMessage("PM.UNREAD");
            req_NotiMess.addHead();
            App.ws.delHandler(req_NotiMess.getReq());
            //App.trace("[SEND] PM.UNREAD", "red");
            App.ws.sendHandler(req_NotiMess.getReq(), delegate (InBoundMessage res_NotiMess)
            {

                int num = res_NotiMess.readInt();
                App.trace("[RECV] PM.UNREAD " + num, "red");
                activeUnreadMail(num);

            });

            OutBounMessage req_EVENT = new OutBounMessage("EVENT.LIST");
            req_EVENT.addHead();
            App.ws.send(req_EVENT.getReq(), delegate (InBoundMessage res_EVENT)
            {
                int count = res_EVENT.readByte();
                if (count > 0)
                {
                    lobbyTxtList[20].text = count.ToString();
                    lobbyTxtList[20].transform.parent.gameObject.SetActive(true);
                }
            });
        }

        private void GetHeadLine()
        {
            if (CPlayer.hadShowEvent == false)
                Invoke("OpenEvents", .25f);
            lobbyGojList[6].SetActive(true);

            var req_HONOUR = new OutBounMessage("HONOUR");
            req_HONOUR.addHead();

            App.ws.send(req_HONOUR.getReq(), delegate (InBoundMessage res_HONOUR)
            {
                List<string> arr = res_HONOUR.readStringArray();
                App.trace("[RECV] HONOUR " + arr.Count, "green");
                foreach (var item in arr)
                {
                //App.trace("- " + item);
                // giu lai
                honored_ls.Add(item);
                }

            });

            var req_headLine = new OutBounMessage("UTIL.GET_HEADLINES");
            //Debug.Log("WRITE LONG = " + CPlayer.id);
            req_headLine.addHead();

            App.ws.send(req_headLine.getReq(), delegate (InBoundMessage res)
            {
            //int first = res.readByte();
            //App.trace("FIRST = " + );
            string head_line_txt = "";

                int count = res.readByte();
            //res.print();
            for (int i = 0; i < count; i++)
                {
                    head_line_txt += res.readStrings() + "\t\t\t\t\t\t\t\t\t\t";
                }

                if (head_line_txt.Length == 0)
                {
                    head_line_txt = App.listKeyText["GAME_GREETING"]; //"Chúc bạn chơi game vui vẻ và gặp nhiều may mắn!";
                }

            // giu lai
            honored_ls.Add(head_line_txt);
                honored_ls.Reverse();
                loopText.OnLoginSuccess(honored_ls.ToArray());


            });

            var req_STORE = new OutBounMessage("UTIL.STORE_REVIEW_BYPASS_EX");
            req_STORE.addHead();
            req_STORE.writeAcii(App.getDevicePlatform());
            req_STORE.writeAcii(App.getVersion());
            req_STORE.writeAcii(App.getProvider());
            App.ws.send(req_STORE.getReq(), delegate (InBoundMessage res_STORE)
            {
                CPlayer.hidePayment = res_STORE.readByte() == 1 ? true : false;
            //CPlayer.hidePayment = false;
            //App.trace(CPlayer.hidePayment, "yellow");
            lobbyGojList[4].SetActive(!CPlayer.hidePayment);
                lobbyGojList[5].SetActive(!CPlayer.hidePayment);
            //lobbyGojList[13].SetActive(!CPlayer.hidePayment);
        });
        }

        /// <summary>
        /// Get pot info
        /// </summary>
        private void GetGameInfo()
        {
            OutBounMessage req = new OutBounMessage("MINIGAME.GET_POT_ALL");
            req.addHead();
            App.trace("[SEND] SLOT_MACHINE.GET_POT_ALL");
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                int mCount = -1;
                int count = res.readByte();
                App.trace("[RECV] SLOT_MACHINE.GET_POT_ALL " + count.ToString());
                for (int i = 0; i < count; i++)
                {
                    string gameId = res.readString();
                    int count1 = res.readByte();
                // Debug.Log("gameID = " + gameId);

                for (int j = 0; j < count1; j++)
                    {
                        int bet = res.readInt();
                        int value = res.readInt();
                        int postion = 0;
                        switch (gameId)
                        {
                            case GameCodeApp.gameCode1:
                                if (bet == 100)
                                    postion = 0;
                                else if (bet == 500)
                                    postion = 1;
                                else if (bet == 1000)
                                    postion = 2;
                                else if (bet == 10000)
                                    postion = 3;
                                break;
                            case GameCodeApp.gameCode2:

                                if (bet == 100)
                                    postion = 4;
                                else if (bet == 500)
                                    postion = 5;
                                else if (bet == 1000)
                                    postion = 6;
                                else if (bet == 10000)
                                    postion = 7;
                                break;
                            case GameCodeApp.gameCode3:
                                if (bet == 100)
                                    postion = 8;
                                else if (bet == 500)
                                    postion = 9;
                                else if (bet == 1000)
                                    postion = 10;
                                else if (bet == 10000)
                                    postion = 11;
                                break;

                            case GameCodeApp.gameCode4:
                                if (bet == 100)
                                    postion = 12;
                                else if (bet == 1000)
                                    postion = 14;
                                else if (bet == 10000)
                                    postion = 15;
                                break;
                            case GameCodeApp.gameCode5:
                                if (bet == 100)
                                    postion = 16;
                                else if (bet == 1000)
                                    postion = 17;
                                else if (bet == 10000)
                                    postion = 18;
                                break;
                            case GameCodeApp.gameCode6:
                                if (bet == 100)
                                    postion = 19;
                                else if (bet == 1000)
                                    postion = 19;
                                else if (bet == 10000)
                                    postion = 20;
                                break;
                            case "minipoker":
                                if (bet == 100)
                                    postion = 21;
                                else if (bet == 1000)
                                    postion = 22;
                                else if (bet == 10000)
                                    postion = 23;
                                break;
                        }
                        lobbyPotText[postion].text = App.formatMoney(value.ToString());
                        potValues[postion] = value;
                    }



                }

            });
        }

        private void GetGameInfo2()
        {
            App.trace("[SEND] ENTER PLACE", "green");
            var req_enterChessRoom = new OutBounMessage("ENTER_PLACE");
            req_enterChessRoom.addHead();
            req_enterChessRoom.writeAcii("Lobby.0.1");
            req_enterChessRoom.writeString(""); //Mật khẩu của phòng chơi
            req_enterChessRoom.writeByte(0); // 0: Vào xem | 1: Vào chơi
            App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res)
            {
            });

        }

        public void DoLogin(string nick, string pass, bool toRemember)
        {
            StartCoroutine(_login("log", nick, pass, toRemember));
        }

        public void DoReg(string name, string pass)
        {
            StartCoroutine(_reg(name, pass));
        }

        private string useNameTemp = "";
        private string passTemp = "";
        private IEnumerator _reg(string name, string pass)
        {

            LoadingControl.instance.showProcessing(true);
            yield return new WaitForSeconds(0.5f);

            //App.start();
            //if (!App.ws.getWS().isConnecting())
            //{
            //    yield return StartCoroutine(App.ws.getWS().Connect());
            //    //App.ws.listen();
            //    /*
            //    Thread th = new Thread(App.ws.listen);
            //    th.IsBackground = true;
            //    th.Start();
            //    */
            //}

            var reqqq = new OutBounMessage("REGISTER_ACCOUNT_EX");
            reqqq.addHead();
            string provider = App.getProvider();
            reqqq.writeAcii(provider);
            reqqq.writeAcii(name.ToLower()); // ten tai khoan
            reqqq.writeAcii(pass); // mat khau
            reqqq.writeAcii("0"); //ref code
                                  //App.trace("DEVIDE ID = " + SystemInfo.deviceUniqueIdentifier, "green");
            reqqq.writeString(SystemInfo.deviceUniqueIdentifier);
            Debug.Log("CountryCode : " + LoadingControl.instance.CountryC + "/nIP : " + LoadingControl.instance.IpUser);
            reqqq.writeString(LoadingControl.instance.CountryC);
            reqqq.writeString(LoadingControl.instance.IpUser);
            //reqqq.writeString("DayLaDeviceFake");
            //int reqTime = PlayerPrefs.GetInt("reqTime", 0);
            //reqTime++;
            //reqqq.writeInt(reqTime); //reg time
            //reqqq.writeAcii(""); //sdt
            //PlayerPrefs.SetInt("reqTime", reqTime);
            App.ws.send(reqqq.getReq(), delegate (InBoundMessage res)
            {
                LoadingControl.instance.showRegPanel(false);
                LoadingControl.instance.showProcessing(true);
                useNameTemp = name;
                passTemp = pass;
                DoLogin(name, pass, true);
            });
            /*
            yield return new WaitForSeconds(0.5f);
            if (useNameTemp.Length > 4)
            {
                //StartCoroutine(_login("req"));
                login("req");
            }*/
        }

        public void OpenRecharge(bool isShow = true)
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            if (isShow == false)
            {
                LoadingControl.instance.loadingGojList[21].SetActive(true);
                if (CPlayer.showEvent)
                    LoadingControl.instance.loadingGojList[29].SetActive(true);
                //lobbyGojList[3].SetActive(true);
                DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, -10), .5f);
                DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, 0), .5f);
                DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 0), .5f);
                //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
                DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(195, 0), .5f);
                return;
            }
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(false);
            //lobbyGojList[3].SetActive(false);
            DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
            DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
            DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 220), .5f);
            //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
            DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 0), .5f).OnComplete(() =>
            {
                LoadingControl.instance.loadingGojList[7].SetActive(true);
            });
        }

        public void OpenExchange(bool isShow = true)
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            //OpenRecharge();

            //if (CPlayer.phoneNum.Length == 0)
            //{
            //    LoadingControl.instance.showAuthen(true);
            //    return;
            //}
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(false);
            //lobbyGojList[3].SetActive(false);
            DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
            DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
            DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 220), .5f);
            //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
            DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 60), .5f).OnComplete(() =>
            {
                LoadingControl.instance.loadingGojList[12].SetActive(true);

            });


        }


        public void OpenChart(bool isShow)
        {
            if (isShow == false)
            {
                LoadingControl.instance.loadingGojList[21].SetActive(true);
                if (CPlayer.showEvent)
                    LoadingControl.instance.loadingGojList[29].SetActive(true);
                //lobbyGojList[3].SetActive(true);
                DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, -10), .5f);
                DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, 0), .5f);
                DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 0), .5f);
                //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
                DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-0, 0), .5f);
                return;
            }
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(false);
            //lobbyGojList[3].SetActive(false);
            //LoadingControl.instance.showRecharge();
            DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
            DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
            DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 220), .5f);
            //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
            DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 0), .5f).OnComplete(() =>
            {
                LoadingControl.instance.loadingGojList[24].SetActive(true);
            });
        }

        public void ShowSettingPanel(bool isShow)
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            if (isShow)
                LoadingControl.instance.loadingGojList[17].SetActive(true);
        }

        private IEnumerator TweenPotNum(Text txt, int fromNum, int toNum, int id, bool fake = false)
        {
            if (fake)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(.5f, 1f));
            }
            else
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));
            }
            float i = 0.0f;
            float rate = 1.0f / .5f;
            txt.transform.localScale = 1.2f * Vector2.one;
            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                float a = Mathf.Lerp(fromNum, toNum, i);
                txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                yield return null;
            }
            potValues[id] = toNum;
            txt.transform.localScale = Vector2.one;
            yield return new WaitForSeconds(.05f);
        }

        public void OpenPlayerInfo(bool isShow)
        {
            /* if (isShow == false)
             {
                 LoadingControl.instance.loadingGojList[21].SetActive(true);
                 if(CPlayer.showEvent)
                     LoadingControl.instance.loadingGojList[29].SetActive(true);
                 //lobbyGojList[3].SetActive(true);
                 lobbyTxtList[3].gameObject.SetActive(true);
                 DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, -10), .5f);
                 DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, 0), .5f);
                 DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 0), .5f);
                 //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
                 DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-0, 60), .5f);
                 if (openRechare)
                 {
                     openRechare = false;
                     OpenRecharge();
                 }
                 return;
             }
             LoadingControl.instance.loadingGojList[21].SetActive(false);
             if (CPlayer.showEvent)
                 LoadingControl.instance.loadingGojList[29].SetActive(false);
             //lobbyGojList[3].SetActive(false);
             //LoadingControl.instance.showRecharge();
             lobbyTxtList[3].gameObject.SetActive(false);
             DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
             DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
             DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 180), .5f);
             //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
             DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 0), .5f).OnComplete(() =>
             {
                 LoadingControl.instance.loadingGojList[13].SetActive(true);
             });*/
            ProfileController.instance.Show();
        }

        public void openRechareValue(bool value)
        {
            openRechare = value;
        }

        public void OpenNoti()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(false);
            //lobbyGojList[3].SetActive(false);
            LoadingControl.instance.OpenNoti(true);
        }

        public void OpenInbox()
        {
            /*
            App.showErr("Tính năng đang phát triển");
            return;
            */
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            //if(CPlayer.showEvent)
            //LoadingControl.instance.loadingGojList[29].SetActive(false);
            lobbyGojList[1].SetActive(true);
        }


        public void EnterGame(string gameName)
        {
            if (gameName == "FACE")
            {
                //App.showErr("Tính năng đang phát triển.");
                App.showErr(App.listKeyText["IN_DEVELOPING"]);

                return;
            }
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }

            int gameId = GetDlOverImgId(gameName);

            switch (gameName)
            {
                case "chest":
                    //CPlayer.changed -= BalanceChanged;
                    DOTween.Kill("texttween");
                    DOTween.Kill("noti");
                    StopAllCoroutines();
                    LoadingControl.instance.ldTextList[10].text = "CHEST";

#if UNITY_IOS || UNITY_ANDROID
                    //StartCoroutine(_EnterGame("Chest"));
                    StartCoroutine(_EnterGameByLoadData("chest", "Chest"));
#else
                StartCoroutine(_EnterGame("Chest"));
#endif
                    break;
                case "bird":
                    //CPlayer.changed -= BalanceChanged;
                    DOTween.Kill("texttween");
                    DOTween.Kill("noti");
                    StopAllCoroutines();    
                    LoadingControl.instance.ldTextList[10].text = "BIRD";
#if UNITY_IOS || UNITY_ANDROID
                    //StartCoroutine(_EnterGame("Bird"));
                    StartCoroutine(_EnterGameByLoadData("bird", "Bird"));
#else
                StartCoroutine(_EnterGame("Bird"));
#endif

                    break;
                case "fish":
                    //CPlayer.changed -= BalanceChanged;
                    DOTween.Kill("texttween");
                    DOTween.Kill("noti");
                    StopAllCoroutines();
                    LoadingControl.instance.ldTextList[10].text = "FISH";
#if UNITY_IOS || UNITY_ANDROID
                    //StartCoroutine(_EnterGame("Fish"));
                    StartCoroutine(_EnterGameByLoadData("fish", "Fish"));
#else
                StartCoroutine(_EnterGame("Fish"));
#endif
                    break;
                default:
                    App.showErr(App.listKeyText["IN_DEVELOPING"]);
                    break;
            }
        }
        private IEnumerator _EnterGame(string t)
        {
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(t);
        }

        private IEnumerator _EnterGameByLoadData(string gameName, string sceneName)
        {
            yield return new WaitForSeconds(0f);
            Debug.Log(countryCode+"--xx");
            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                + "/DLC/" + App.GetDLCName(gameName + LoadingControl.instance.CountryC.ToLower());
            Debug.Log(path + "Lobby");
            //var listScene = AssetBundle.LoadFromFileAsync(path);

            
            AssetBundleCreateRequest rqAB = new AssetBundleCreateRequest();
            Caching.ClearCache();
            rqAB = AssetBundle.LoadFromFileAsync(path);
            
            yield return rqAB;

            LoadingControl.instance.asbs[0] = rqAB.assetBundle;
            SceneManager.LoadScene(sceneName);
        }

        private void setHandler()
        {
            OutBounMessage req_BALANCE_CHANGED = new OutBounMessage("BALANCE_CHANGED");
            req_BALANCE_CHANGED.addHead();
            App.ws.delHandler(req_BALANCE_CHANGED.getReq());
            App.ws.sendHandler(req_BALANCE_CHANGED.getReq(), delegate (InBoundMessage res_BALANCE_CHANGED)
             {

                 int slotId = res_BALANCE_CHANGED.readByte();
                 long chipBalance = res_BALANCE_CHANGED.readLong();
                 long starBalance = res_BALANCE_CHANGED.readLong();
                 Debug.Log("slotId = " + slotId + "  " + "chipBalance = " + chipBalance);
                 /*Debug.Log("Start BALANCE_CHANGED");
                 Debug.Log("slotId = " + slotId);
                 Debug.Log("chipBalance = " + chipBalance);
                 Debug.Log("starBalance = " + starBalance);
                 Debug.Log("gameName = " + CPlayer.gameName);
                 Debug.Log("End BALANCE_CHANGED");*/
                 if (slotId < 0)              //is me
                 {
                     //CPlayer.chipBalance = chipBalance;
                     //CPlayer.manBalance = starBalance;
                     CPlayer.change("chip", chipBalance);
                     CPlayer.change("man", starBalance);
                     if (ProfileController.instance != null)
                         ProfileController.instance.balanceChanged(chipBalance, starBalance);
                 }

                 switch (CPlayer.gameName)
                 {
                     case "1":
                         //BoardManager.instance.balanceChanged(slotId, chipBalance, starBalance);
                         if (TLMNControler.instance != null)
                             TLMNControler.instance.balanceChanged(slotId, chipBalance, starBalance);
                         break;
                     case "xocdia":
                         if (XocDiaControler.instance != null)
                             XocDiaControler.instance.balanceChanged(slotId, chipBalance, starBalance);
                         break;
                     case "maubinh":
                         if (MauBinhController.instance != null)
                         {
                             MauBinhController.instance.balanceChanged(slotId, chipBalance, starBalance);
                         }
                         break;
                     case "0":
                         if (PhomController.instance != null)
                         {
                             PhomController.instance.balanceChanged(slotId, chipBalance, starBalance);
                         }
                         break;
                     case "xito":
                         if (XiToController.instance != null)
                         {
                             XiToController.instance.balanceChanged(slotId, chipBalance, starBalance);
                         }
                         break;
                     case "blackjack":
                         if (XiDachController.instance != null)
                         {
                             XiDachController.instance.balanceChanged(slotId, chipBalance, starBalance);
                         }
                         break;
                     case "chan":
                         if (ChanController.instance != null)
                         {
                             ChanController.instance.balanceChanged(slotId, chipBalance, starBalance);
                         }
                         break;
                     case "poker":
                         if (PokerController.instance != null)
                         {
                             PokerController.instance.balanceChanged(slotId, chipBalance, starBalance);
                         }
                         break;

                 }


             });

            // Not enough money for the game
            OutBounMessage notEnoughMoneyRequest = new OutBounMessage("GAME.EXIT");
            notEnoughMoneyRequest.addHead();
            App.ws.delHandler(notEnoughMoneyRequest.getReq());
            App.ws.sendHandler(notEnoughMoneyRequest.getReq(), delegate (InBoundMessage notEnoughMoneyRespond)
             {
                 notEnoughMoneyRespond.readByte();
                 var gameState = notEnoughMoneyRespond.readLong(); //Giá trị xác định dừng lượt chơi ; value -1 mean stop that game
                 var gameCode = notEnoughMoneyRespond.readString(); //gamecode: ruongbau,skull,poro,minipoker

                 CPlayer.StopGame(gameCode, (int)gameState);

             });


            var req_POT_CHANGED = new OutBounMessage("MINIGAME.POT_CHANGED");
            req_POT_CHANGED.addHead();
            App.ws.delHandler(req_POT_CHANGED.getReq());
            App.ws.sendHandler(req_POT_CHANGED.getReq(), delegate (InBoundMessage res)
            {
                App.trace("[RECV] SLOT_MACHINE.POT_CHANGED " + res.readByte());

                InBoundMessage potChanged = new InBoundMessage(res.getData());
                InBoundMessage potChangedCT = new InBoundMessage(res.getData());
                InBoundMessage potChangedDT = new InBoundMessage(res.getData());
                InBoundMessage potChangedTX = new InBoundMessage(res.getData());
                InBoundMessage potChangedVQMM = new InBoundMessage(res.getData());
                InBoundMessage potChangedMNPK = new InBoundMessage(res.getData());
                InBoundMessage potChangedZBS = new InBoundMessage(res.getData());
                InBoundMessage potChanged3X3 = new InBoundMessage(res.getData());
                InBoundMessage potChangedOneLineSlot = new InBoundMessage(res.getData());

                CPlayer.ChangePot1(potChanged);
                CPlayer.ChangePot(potChanged);
                CPlayer.ChangePotCaoThap(potChangedCT);
                CPlayer.ChangePotDapTrung(potChangedDT);
                CPlayer.ChangePotTx(potChangedTX);
                CPlayer.ChangePotVQMM(potChangedVQMM);
                CPlayer.ChangePotMiniPocker(potChangedMNPK);
                CPlayer.ChangePotZombieSlot(potChangedZBS);
                CPlayer.ChangePot3X3Slot(potChanged3X3);
                CPlayer.ChangePotOneLineSlot(potChangedOneLineSlot);

                int count = res.readByte();
                //potValues = new int[count * 3];
                int m = -1;
                switch (SceneManager.GetActiveScene().name)
                {
                    case "Lobby":
                        for (int i = 0; i < count; i++)
                        {
                            string gameId = res.readString();
                            int count1 = res.readByte();

                            for (int j = 0; j < count1; j++)
                            {
                                int bet = res.readInt();
                                int value = res.readInt();
                                m++;
                                int postion = -1;
                                switch (gameId)
                                {
                                    case GameCodeApp.gameCode1:
                                        if (bet == 100)
                                            postion = 0;
                                        else if (bet == 500)
                                            postion = 1;
                                        else if (bet == 1000)
                                            postion = 2;
                                        else if (bet == 10000)
                                            postion = 3;
                                        break;
                                    case GameCodeApp.gameCode2:
                                        if (bet == 100)
                                            postion = 4;
                                        else if (bet == 500)
                                            postion = 5;
                                        else if (bet == 1000)
                                            postion = 6;
                                        else if (bet == 10000)
                                            postion = 7;
                                        break;
                                    case GameCodeApp.gameCode3:
                                        if (bet == 100)
                                            postion = 8;
                                        else if (bet == 500)
                                            postion = 9;
                                        else if (bet == 1000)
                                            postion = 10;
                                        else if (bet == 10000)
                                            postion = 11;
                                        break;
                                    case GameCodeApp.gameCode4:
                                        if (bet == 100)
                                            postion = 12;
                                        else if (bet == 1000)
                                            postion = 13;
                                        else if (bet == 10000)
                                            postion = 14;
                                        break;
                                    case GameCodeApp.gameCode5:
                                        if (bet == 100)
                                            postion = 15;
                                        else if (bet == 1000)
                                            postion = 16;
                                        else if (bet == 10000)
                                            postion = 17;
                                        break;
                                    case GameCodeApp.gameCode6:
                                        if (bet == 100)
                                            postion = 18;
                                        else if (bet == 1000)
                                            postion = 19;
                                        else if (bet == 10000)
                                            postion = 20;
                                        break;
                                    case "minipoker":
                                        if (bet == 100)
                                            postion = 21;
                                        else if (bet == 1000)
                                            postion = 22;
                                        else if (bet == 10000)
                                            postion = 23;
                                        break;

                                }
                                if (postion > -1)
                                {
                                    if (lobbyPotTextSlide[postion] != null)
                                    {
                                        if (value > potValues[postion])
                                        {
                                            StartCoroutine(TweenTopPotNum(lobbyPotTextSlide[postion], potValues[postion], value));
                                        }
                                        else
                                        {
                                            lobbyPotTextSlide[postion].text = string.Format("{0:0,0}", value);
                                        }
                                    }
                                    if (lobbyPotText[postion] != null)
                                    {
                                        if (value > potValues[postion])
                                        {
                                            StartCoroutine(TweenPotNum(lobbyPotText[postion], potValues[postion], value, postion));
                                        }
                                        else
                                        {
                                            lobbyPotText[postion].text = string.Format("{0:0,0}", value);
                                            potValues[postion] = value;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            });

            var req_NOTI = new OutBounMessage("NOTIFY_ALL");
            req_NOTI.addHead();
            App.ws.delHandler(req_NOTI.getReq());
            App.ws.sendHandler(req_NOTI.getReq(), delegate (InBoundMessage res_NOTI)
            {
                App.trace("[RECV] NOTIFY_ALL");
                res_NOTI.readByte();
                string t0 = res_NOTI.readStrings();
                string t1 = res_NOTI.readStrings();
                string t2 = res_NOTI.readStrings();
                string t3 = res_NOTI.readStrings();
                //LoadingControl.instance.showNotifyAll(res_NOTI.readStrings(), res_NOTI.readStrings(),res_NOTI.readStrings(), res_NOTI.readStrings());
                LoadingControl.instance.showNotifyAll(t0, t3, t1.ToString(), t2.ToString());
            });

            var reqChat = new OutBounMessage("CHAT.MSG");
            reqChat.addHead();
            //reqChat.print();
            App.ws.delHandler(reqChat.getReq());
            App.ws.sendHandler(reqChat.getReq(), delegate (InBoundMessage resChat)
            {
                //App.trace("RECV [CHAT.MSG]");
                int type = resChat.readByte();
                string nickName = resChat.readAscii();
                string content = resChat.readString();
                App.trace("RECV [CHAT.MESG]: type=" + type + "|nick=" + nickName + "|content=" + content, "yellow");
                LoadingControl.instance.chatMSGRecv(LoadingControl.CHANNEL_GENERAL, nickName, content);
            });

            var req_Alert = new OutBounMessage("ALERT");
            req_Alert.addHead();
            App.ws.delHandler(req_Alert.getReq());
            App.ws.sendHandler(req_Alert.getReq(), delegate (InBoundMessage res_Alert)
            {

                string content = res_Alert.readString();
                var type = res_Alert.readByte();
                /*
                if (content.Contains("không được vào chơi do không đủ gold"))
                {
                    LoadingControl.instance.notEnoughChip(App.formatToUserContent(content));
                }
                else
                {
                    App.showErr(App.formatToUserContent(content));
                }
                */
                App.showErr(App.formatToUserContent(content));

            }, "ALERT");

            var req_invite = new OutBounMessage("INVITE");
            req_invite.addHead();
            App.ws.sendHandler(req_invite.getReq(), delegate (InBoundMessage res_invite)
            {

                try
                {
                    string nickName = res_invite.readAscii();
                    int roomId = res_invite.readByte();
                    int tableId = res_invite.readShort();
                    string password = res_invite.readString();
                    int tableType = res_invite.readByte();
                    int betAmtId = res_invite.readByte();

                    if (TableList.instance != null && LoadingControl.instance.bandInvitation == false)
                    {
                        TableList.instance.beInvited(betAmtId, roomId, tableId, password);
                    }
                }
                catch
                {

                }
            });
        }

        private IEnumerator TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0f)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            float i = 0.0f;
            float rate = 2.0f / tweenTime;
            txt.transform.DOScale(scaleNum, tweenTime / 2).SetId("texttween");
            while (i < tweenTime)
            {
                i += Time.deltaTime * rate;
                float a = Mathf.Lerp(fromNum, toNum, i);

                txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                if (a == toNum)
                {
                    i = tweenTime;
                }
                yield return null;
            }
            //App.trace(i.ToString());
            txt.transform.localScale = Vector2.one;
            yield return new WaitForSeconds(.05f);
        }

        //private void BalanceChanged(string type)
        //{
        //    if(type == "chip")
        //    {
        //        if(CPlayer.preChipBalance < CPlayer.chipBalance)
        //        StartCoroutine(TweenNum(lobbyTxtList[1],(int)CPlayer.preChipBalance,(int)CPlayer.chipBalance,3f,1.1f));
        //        //lobbyTxtList[1].text = App.FormatMoney(CPlayer.chipBalance);
        //        else
        //            lobbyTxtList[1].text = App.FormatMoney(CPlayer.chipBalance);
        //    }
        //}

        public void OpenChat()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenChat();
        }

        #region LOGIN WITH FACEBOOK
        public void loginFacebook()
        {
            PlayerPrefs.SetString("clickFB", "true");
            //   App.trace("FROM FB: " + fbToken);

            if (fbToken != "")
            {
                //LoadingControl.instance.loadingScene.SetActive(true);

                StartCoroutine(_login("fb"));
            }
            else
            {
#if UNITY_WEBGL


            string appId = "2261114970871006";
            string redirectUrl = "https://sungaming.win/response.html";

            string scope = "public_profile,email";
            string url = "https://www.facebook.com/dialog/oauth?client_id=" + appId + "&redirect_uri=" + redirectUrl + "&response_type=token,granted_scopes&scope=" + scope + "&display=popup";
            //Application.ExternalEval("window.open('" + url + "');");
            //Application.ExternalEval(url);
            //Application.ExternalEval("window.open(\"" + url + "\")");

            App.openWindow(url);





#else
                //FB.LogOut();user_friends,,email,public_profile

                var perms = new List<string>() { "public_profile", "email" };
                //var perms = new List<string>() { "public_profile", "email", "user_friends" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
#endif

            }

        }
        public void CallbackFromWebGL(string access_token)
        {
            if (access_token == null || access_token.Length == 0)
            {
                //App.showErr("Có lỗi khi đăng nhập Facebook");
                App.showErr(App.listKeyText["FACEBOOK_LOGIN_ERROR"]);

            }
            else
            {
                fbToken = access_token;
                // LoadingControl.instance.loadingScene.SetActive(true);
                StartCoroutine(_login("fb"));
            }
        }

        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...
                fbToken = FB.ClientToken;
#if UNITY_IOS
            if (FB.IsLoggedIn && fbToken == "")
            {
                FB.Mobile.RefreshCurrentAccessToken(RefreshCallback);
            }
#endif
            }
            else
            {
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
        }

        private void RefreshCallback(IAccessTokenRefreshResult result)
        {
            fbToken = result.AccessToken.TokenString;
        }
        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        private string fbToken = "";
        private void AuthCallback(ILoginResult result)
        {
            /*
            if (FB.IsLoggedIn)
            {
                App.showErr("FB LOGED IN");
                return;
            }*/



            if (result.Cancelled)
            {
                //App.showErr("Bạn chưa đăng nhập Facebook");
                App.showErr(App.listKeyText["FACEBOOK_LOGIN_NOT_YET"]);

                return;
            }
            if (FB.IsLoggedIn)
            {

                // AccessToken class will have session details
                var aToken = AccessToken.CurrentAccessToken;
                // Print current access token's User ID
                //Debug.Log(aToken.UserId);
                // Print current access token's granted permissions
                /*
                foreach (string perm in aToken.Permissions)
                {
                    //Debug.Log(perm);
                }
                */
                //App.trace(aToken.TokenString);
                //App.trace(Facebook.Unity.AccessToken.CurrentAccessToken.TokenString);

                //fbToken = aToken.TokenString;
                fbToken = AccessToken.CurrentAccessToken.TokenString.ToString();
                //LoadingControl.instance.loadingScene.SetActive(true);

                StartCoroutine(_login("fb"));
            }
            /*  else
              {
                  App.showErr("Bạn chưa đăng nhập Facebook");
                  //Debug.Log("User cancelled login");
              }*/
        }
        #endregion

        public void OpenFreeChip(bool isShow)
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }

            if (isShow == false)
            {
                LoadingControl.instance.loadingGojList[21].SetActive(true);
                if (CPlayer.showEvent)
                    LoadingControl.instance.loadingGojList[29].SetActive(true);
                //lobbyGojList[3].SetActive(true);
                //lobbyTxtList[3].gameObject.SetActive(true);
                DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, -10), .5f);
                DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, 0), .5f);
                DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 0), .5f);
                //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
                DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-0, 0), .5f);

                return;
            }
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(false);
            //lobbyGojList[3].SetActive(false);
            //LoadingControl.instance.showRecharge();
            //lobbyTxtList[3].gameObject.SetActive(false);
            DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
            DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
            DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 220), .5f);
            //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
            DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 0), .5f).OnComplete(() =>
            {
                lobbyGojList[2].SetActive(true);
            });
        }

        private IEnumerator FakePotChange()
        {
            for (int i = 0; i < fakePotData.Length; i++)
            {
                lobbyPotText[i].text = App.FormatMoney(fakePotData[i]);
            }
            while (true)
            {
                yield return new WaitForSeconds(1f);
                for (int i = 0; i < fakePotData.Length; i++)
                {
                    StartCoroutine(TweenPotNum(lobbyPotText[i], fakePotData[i], fakePotData[i] + getStepPotFake(betCurrent), i, true));
                    fakePotData[i] = fakePotData[i] + getStepPotFake(betCurrent);
                }
            }
        }

        private int getStepPotFake(int betCurrent)
        {
            switch (betCurrent)
            {
                case 0: return 10;
                case 1: return 100;
                case 2: return 1000;
                case 3: return 50;
                default: return 100;
            }
        }


        private int GetDlOverImgId(string gameName)
        {
            switch (gameName)
            {
                case "chest":
                    sizeFile = sizeChest;
                    return 15;
                case "fish":
                    sizeFile = sizeFish;
                    return 17;
                case "bird":
                    sizeFile = sizeBird;
                    return 19;
            }
            return -1;
        }

//        private IEnumerator DownLoadData(string gameName, bool isLoadScene = false)
//        {
//            Debug.Log("<color='yelow'>Down load DLC...</color>");

//            float lastRememberProgress = 0f;
//            float progressDistance = 2;
//            float maxTimeOut = 10f;
//            float timeOut = 0f;
//            sizeFile = 0;
//            int imgId = GetDlOverImgId(gameName);
//            lobbyImgList[imgId].fillAmount = 0;
//            //lobbyTxtList[22 + (imgId - 1) / 2].text = "Đang tải...";
//            lobbyImgList[imgId].transform.parent.gameObject.SetActive(true);
//            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
//                + "/DLC/" + App.GetDLCName(gameName);

//            Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
//                + "/DLC/");


//            string url = App.mainUrl + App.GetDLCName(gameName);
//            Debug.LogError(url);

//            WWW www = new WWW(url);

//            Debug.LogError(www.isDone);
            
//            while (!www.isDone && string.IsNullOrEmpty(www.error))
//            {
//                lobbyImgList[imgId].fillAmount = www.progress;
//                if (100 * www.progress > (lastRememberProgress + progressDistance))
//                {
//                    lastRememberProgress = www.progress;
//                    timeOut = 0;
//                }
//                timeOut += Time.deltaTime;
//#if !UNITY_WEBGL
//                if (timeOut > maxTimeOut)
//                {
//                    StartCoroutine(LoadAgain(gameName));
//                    yield break;
//                }
//#endif
//                yield return null;
//            }
//            if (!string.IsNullOrEmpty(www.error))
//            {
//                Debug.LogError(www.error);
//                App.showErr("Đã xảy ra lỗi từ máy chủ.");
//                //Debug.LogError(www.error);
//                yield break;
//            }

//            lobbyImgList[imgId].fillAmount = 1;
//#if !UNITY_WEBPLAYER
//            if (www.isDone)
//            {
//                Debug.Log("Đã tải xong.");
//                //lobbyTxtList[22 + (imgId - 1) / 2].text = "Đã tải xong.";
//                File.WriteAllBytes(path, www.bytes);
//                yield return new WaitForSeconds(.5f);
//                // 15 17 19
//                lobbyImgList[imgId + 1].transform.parent.gameObject.SetActive(false);
//                intNum[0] = 0;
//                //switch (gameName)
//                //{
//                //    case "chest":
//                //        PlayerPrefs.SetInt("VersionChest", versionChest);
//                //        if (isLoadScene)
//                //            StartCoroutine(_EnterGameByLoadData("chest", "Chest"));
//                //        break;
//                //    case "fish":
//                //        PlayerPrefs.SetInt("VersionFish", versionFish);
//                //        if (isLoadScene)
//                //            StartCoroutine(_EnterGameByLoadData("fish", "Fish"));
//                //        break;
//                //    case "bird":
//                //        PlayerPrefs.SetInt("VersionBird", versionBird);
//                //        if (isLoadScene)
//                //StartCoroutine(_EnterGameByLoadData("bird", "Bird"));
//                //        break;
//                //}

//            }
//#endif
//        }

//        private IEnumerator LoadAgain(string gameName, bool isLoadScene = false)
//        {
//            sizeFile = 0;
//            int imgId = GetDlOverImgId(gameName);
//            lobbyImgList[imgId].transform.parent.gameObject.SetActive(true);
//            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
//                + "/DLC/" + App.GetDLCName(gameName);
//            WWW www = new WWW(App.mainUrl + App.GetDLCName(gameName));

//            while (!www.isDone && string.IsNullOrEmpty(www.error))
//            {
//                lobbyImgList[imgId].fillAmount = (float)((www.bytesDownloaded * 1.0f) / sizeFile);
//                lobbyImgList[imgId].fillAmount = www.progress;
//                yield return null;
//            }

//            if (!string.IsNullOrEmpty(www.error))
//            {
//                Debug.LogError(2);

//                App.showErr("Đã xảy ra lỗi từ máy chủ.");
//                //Debug.LogError(www.error);
//                yield break;
//            }

//            lobbyImgList[imgId].fillAmount = 1;
//#if !UNITY_WEBPLAYER
//            Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
//        + "/Resources/DLC/");

//            if (www.isDone)
//            {
//                //lobbyTxtList[22 + (imgId - 1)/2].text = "Đã tải xong.";
//                File.WriteAllBytes(path, www.bytes);
//                yield return new WaitForSeconds(.5f);
//                lobbyImgList[imgId + 1].transform.parent.gameObject.SetActive(false);
//                intNum[0] = 0;
//                switch (gameName)
//                {
//                    case "chest":
//                        PlayerPrefs.SetInt("VersionChest", versionChest);
//                        if (isLoadScene)
//                            StartCoroutine(_EnterGameByLoadData("chest", "Chest"));
//                        break;
//                    case "fish":
//                        PlayerPrefs.SetInt("VersionFish", versionFish);
//                        if (isLoadScene)
//                            StartCoroutine(_EnterGameByLoadData("fish", "Fish"));
//                        break;
//                    case "bird":
//                        PlayerPrefs.SetInt("VersionBird", versionBird);
//                        if (isLoadScene)
//                            StartCoroutine(_EnterGameByLoadData("bird", "Bird"));
//                        break;
//                }

//            }

//#endif
//        }

        //private void CheckLoadData(string gameName)
        //{
        //    string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
        //           + "/DLC/" + App.GetDLCName(gameName);

        //    lobbyImgList[GetDlOverImgId(gameName) + 1].transform.parent.gameObject.SetActive(!File.Exists(path));

        //    if (!File.Exists(path))
        //    {
        //        StartCoroutine(DownLoadData(gameName));
        //        Debug.LogError(path);
        //    }
        //    switch (gameName)
        //    {
        //        case "chest":
        //            if (PlayerPrefs.GetInt("VersionChest", 0) == 0)
        //            {//Dowload
        //                lobbyImgList[GetDlOverImgId("chest") + 1].transform.parent.GetComponentsInChildren<Image>()[3].gameObject.SetActive(true);
        //                lobbyImgList[GetDlOverImgId("chest") + 1].transform.parent.GetComponentsInChildren<Image>()[4].gameObject.SetActive(false);
        //            }
        //            else
        //            {
        //                if (PlayerPrefs.GetInt("VersionChest", 0) < versionChest)//cu = 1< moi = 2
        //                {//Update

        //                    lobbyImgList[GetDlOverImgId("chest") + 1].transform.parent.GetComponentsInChildren<Image>()[4].gameObject.SetActive(true);
        //                    lobbyImgList[GetDlOverImgId("chest") + 1].transform.parent.GetComponentsInChildren<Image>()[3].gameObject.SetActive(false);
        //                }
        //            }
        //            break;
        //        case "fish":
        //            if (PlayerPrefs.GetInt("VersionFish", 0) == 0)
        //            {
        //                lobbyImgList[GetDlOverImgId("fish") + 1].transform.parent.GetComponentsInChildren<Image>()[3].gameObject.SetActive(true);
        //                lobbyImgList[GetDlOverImgId("fish") + 1].transform.parent.GetComponentsInChildren<Image>()[4].gameObject.SetActive(false);
        //            }
        //            else
        //            {
        //                if (PlayerPrefs.GetInt("VersionFish", 0) < versionFish)
        //                {
        //                    lobbyImgList[GetDlOverImgId("fish") + 1].transform.parent.GetComponentsInChildren<Image>()[4].gameObject.SetActive(true);
        //                    lobbyImgList[GetDlOverImgId("fish") + 1].transform.parent.GetComponentsInChildren<Image>()[3].gameObject.SetActive(false);
        //                }

        //            }
        //            break;
        //        case "bird":
        //            if (PlayerPrefs.GetInt("VersionBird", 0) == 0)
        //            {
        //                lobbyImgList[GetDlOverImgId("bird") + 1].transform.parent.GetComponentsInChildren<Image>()[3].gameObject.SetActive(true);
        //                lobbyImgList[GetDlOverImgId("bird") + 1].transform.parent.GetComponentsInChildren<Image>()[4].gameObject.SetActive(false);
        //            }
        //            else
        //            {
        //                if (PlayerPrefs.GetInt("VersionBird", 0) < versionBird)
        //                {
        //                    lobbyImgList[GetDlOverImgId("bird") + 1].transform.parent.GetComponentsInChildren<Image>()[4].gameObject.SetActive(true);
        //                    lobbyImgList[GetDlOverImgId("bird") + 1].transform.parent.GetComponentsInChildren<Image>()[3].gameObject.SetActive(false);
        //                }
        //            }
        //            break;
        //    }
        //    //App.trace("Phac paht = " + path + "|" + lobbyImgList[GetDlOverImgId(gameName)].transform.parent.parent.gameObject.name, "red");
        //}

        public void LoadData(string t)
        {
            /*
            switch (t)
            {
                case "mnp":
                    lobbyImgList[2].raycastTarget = false;
                    break;
                case "xeng":
                    lobbyImgList[4].raycastTarget = false;
                    break;
                case "ctn":
                    lobbyImgList[6].raycastTarget = false;
                    break;
                case "zda":
                    lobbyImgList[8].raycastTarget = false;
                    break;
                case "frt":
                    lobbyImgList[10].raycastTarget = false;
                    break;
                case "sl7":
                    lobbyImgList[12].raycastTarget = false;
                    break;
            }*/
            if (intNum[0] == 1)
            {
                return;
            }
            intNum[0] = 1;
            //StartCoroutine(DownLoadData(t));
        }

        //private void LoadDataOpenScene(string t)
        //{
        //    if (intNum[0] == 1)
        //    {
        //        return;
        //    }
        //    intNum[0] = 1;
        //    StartCoroutine(DownLoadData(t, true));
        //}

        void OpenEvents()
        {
            //App.trace("LUCKY GAME ","yellow");
            var req = new OutBounMessage("MINIGAME.LUCKY_STATUS");
            req.addHead();
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
            //App.trace("LUCKY GAME REV", "yellow");
            //res.readByte();
            bool open = res.readByte() == 1 ? true : false; //1: bat || 0: tat
            int numbercount = res.readInt();
                string title = res.readString();
                string content = res.readString();
                bool openButton = res.readInt() == 1 ? true : false;
                CPlayer.titleEvent = title;
                CPlayer.contentEvent = content;
                CPlayer.openButtonEvent = openButton;
                CPlayer.showEvent = open;
                CPlayer.hadShowEvent = true;
                App.trace("Open Events " + open + " || So luot con lai " + numbercount.ToString() + " || Tieu de " + title + " || Noi dung text " + content + " || Open Button " + openButton, "yellow");
                if (open)
                {
                    LoadingControl.instance.ldTextList[14].text = "QUAY : " + numbercount.ToString();
                }

            });
        }

        public void CheckPassLV2()
        {
            string pwlv2 = passwordLV2.text.ToString();
            passwordLV2.text = "";
            var req = new OutBounMessage("AGENCY_PASSWORD_LV2");
            req.addHead();
            req.writeString(pwlv2);
            App.ws.send(req.getReq(), (InBoundMessage res) =>
            {
                Debug.LogError(passwordLV2.text);
                
                Debug.LogError(passwordLV2.text);

                int reqCheckPassLV2 = res.readInt();
                if(reqCheckPassLV2 == 1)
                {
                    panelCheckPassLV2.OnClose();
                    LoadingControl.instance.loadingGojList[28].SetActive(true);
                    
                }
                else if (reqCheckPassLV2 == 0)
                {
                    App.showErr(App.listKeyText["WRONG_PASSWORD"]);
                }
            });
        }

        public void OpenMoneyTransfer(bool isShow)
        {
            if (CPlayer.typeUser == 1)
            {
                panelCheckPassLV2.OnOpen();
            }
            else if(CPlayer.typeUser == 0)
            {
                if (CPlayer.logedIn == false)
                {
                    showLogPanel("log");
                    return;
                }


                //if (CPlayer.phoneNum.Length == 0)
                //{
                //    LoadingControl.instance.showAuthen(true);
                //    return;
                //}

                if (isShow == false)
                {
                    //imageTopBar.SetActive(false);
                    LoadingControl.instance.loadingGojList[21].SetActive(true);
                    if (CPlayer.showEvent)
                        LoadingControl.instance.loadingGojList[29].SetActive(true);
                    //lobbyGojList[3].SetActive(true);
                    //lobbyTxtList[3].gameObject.SetActive(true);
                    DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, -10), .5f);
                    DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, 0), .5f);
                    DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 0), .5f);
                    //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
                    DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(160, 0), .5f);
                    return;
                }
                LoadingControl.instance.loadingGojList[21].SetActive(false);
                //imageTopBar.SetActive(false);

                if (CPlayer.showEvent)
                    LoadingControl.instance.loadingGojList[29].SetActive(false);
                //lobbyGojList[3].SetActive(false);
                //LoadingControl.instance.showRecharge();
                //lobbyTxtList[3].gameObject.SetActive(false);
                DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
                DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
                DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 220), .5f);
                //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
                DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 0), .5f).OnComplete(() =>
                {

                    LoadingControl.instance.loadingGojList[28].SetActive(true);

                    //imageTopBar.SetActive(false);

                });
            }
        }

        public void OpenFanPage()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            Application.OpenURL("https://www.facebook.com/vipgame.fanpage");
        }

        public void OpenLuckyWheel()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenLuckyWheel();
        }

        public void OpenReply()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.showPmPanel("admin", App.listKeyText["COMMENT"] + " , " + App.listKeyText["FEEDBACK"] /*"PHẢN HỒI, GÓP Ý"*/);
        }

        public void OpenTaiXiu()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenTaiXiu();
        }

        public void OpenMiniPoker()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenMiniPocker();
        }
        public void OpenSlotZombie()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenZombieSlot();
        }
        public void OpenSlotOneLine()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenSlotOneLine();
        }
        public void OpenSlot3x3()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenMiniGame3X3();
        }

        public void OpenBauCua()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.OpenFeast();
        }

        public void OpenEventSlide()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            //lobbyImgList[13].overrideSprite = lobbySprtList[0];
            //lobbyImgList[14].overrideSprite = lobbySprtList[3];
            lobbyGojList[7].SetActive(true);
            lobbyGojList[8].SetActive(false);
            lobbyImgList[13].transform.GetComponent<Button>().interactable = false;
            lobbyImgList[14].transform.GetComponent<Button>().interactable = true;
        }

        public void OpenTopSlide()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            //settingBtnGO.SetActive(true);
            //lobbyImgList[13].overrideSprite = lobbySprtList[1];
            //lobbyImgList[14].overrideSprite = lobbySprtList[2];
            lobbyGojList[7].SetActive(false);
            lobbyGojList[8].SetActive(true);
            lobbyImgList[14].transform.GetComponent<Button>().interactable = false;
            lobbyImgList[13].transform.GetComponent<Button>().interactable = true;
            ChangeTopSlide(3);
        }

        private IEnumerator LoopTopPotSlide()
        {
            yield return new WaitForSeconds(10f);
            switch (currentTopSlide)
            {
                case 0:
                    currentTopSlide = 1;
                    break;
                case 1:
                    currentTopSlide = 2;
                    break;
                case 2:
                    currentTopSlide = 3;
                    break;
                case 3:
                    currentTopSlide = 0;
                    break;
            }
            ChangeTopSlide(currentTopSlide);
        }
        private int betCurrent = 3;
        private int currentTopSlide = 0;
        public void ChangeTopSlide(int id)
        {
            currentTopSlide = id;
            if (threads[3] != null)
                StopCoroutine(threads[3]);
            threads[3] = LoopTopPotSlide();
            StartCoroutine(threads[3]);
            lobbyGojList[9].transform.GetComponent<Button>().interactable = true;
            lobbyGojList[9].transform.GetChild(0).gameObject.SetActive(false);
            lobbyGojList[10].transform.GetComponent<Button>().interactable = true;
            lobbyGojList[10].transform.GetChild(0).gameObject.SetActive(false);
            lobbyGojList[11].transform.GetComponent<Button>().interactable = true;
            lobbyGojList[11].transform.GetChild(0).gameObject.SetActive(false);
            lobbyGojList[14].transform.GetComponent<Button>().interactable = true;
            lobbyGojList[14].transform.GetChild(0).gameObject.SetActive(false);
            switch (id)
            {
                case 0:
                    lobbyGojList[id + 9].transform.GetComponent<Button>().interactable = false;
                    lobbyGojList[id + 9].transform.GetChild(0).gameObject.SetActive(true);
                    lobbyGojList[id + 9].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                case 1:
                    lobbyGojList[14].transform.GetComponent<Button>().interactable = false;
                    lobbyGojList[14].transform.GetChild(0).gameObject.SetActive(true);
                    lobbyGojList[14].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                case 2:
                case 3:
                    lobbyGojList[id + 8].transform.GetComponent<Button>().interactable = false;
                    lobbyGojList[id + 8].transform.GetChild(0).gameObject.SetActive(true);
                    lobbyGojList[id + 8].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                default: break;
            }

            try
            {
                GetValueTopPot(id);
            }
            catch (Exception ex)
            {
            }
        }
        class TopPot
        {

            public string GameId = "";
            public int[] bet = { 100, 500, 1000, 10000 };
            public int[] value = { 0, 1, 2, 3 };
            public int count = 0;
            public TopPot()
            {
                GameId = "";
            }
        }

        private IEnumerator FakeTopPotChange(int id)
        {
            int eleCount = 3;
            if (id != 1)
                eleCount = 7;

            while (true)
            {
                yield return new WaitForSeconds(1f);
                for (int i = 0; i < eleCount; i++)
                {
                    threads[2] = TweenTopPotNum(
                    lobbyFakeTopPotTxts[i], fakePot[id][i], fakePot[id][i] + getStepPotFake(id), true);
                    StartCoroutine(threads[2]);
                    fakePot[id][i] = fakePot[id][i] + getStepPotFake(id);
                }
            }
        }

        private IEnumerator FakeTopPot500Change(int id)
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                for (int i = 0; i < 3; i++)
                {
                    threads[2] = TweenTopPotNum(lobbyFakeTopPotTxts[i], fakePot[fakeTopPot500Id][i], fakePot[fakeTopPot500Id][i] + 50, true);
                    StartCoroutine(threads[2]);
                    fakePot[fakeTopPot500Id][i] = fakePot[fakeTopPot500Id][i] + 50;
                }
            }
        }

        private IEnumerator TweenTopPotNum(Text txt, int fromNum, int toNum, bool fake = false)
        {
            if (fake)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(.5f, 1f));
            }
            else
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));
            }
            float i = 0.0f;
            float rate = 1.0f / .5f;
            try
            {
                txt.transform.localScale = 1.2f * Vector2.one;
            }
            catch { }

            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                float a = Mathf.Lerp(fromNum, toNum, i);

                try
                {
                    txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                }
                catch { }
                yield return null;
            }

            try
            {
                txt.transform.localScale = Vector2.one;
            }
            catch { }
            yield return new WaitForSeconds(.05f);
        }
        private List<GameObject> topCloneArr = new List<GameObject>();

        private void CloneFakeTopPot(int id)
        {
            int index = id;
            betCurrent = index;
            if (threads[1] != null)
                StopCoroutine(threads[1]);
            if (threads[2] != null)
                StopCoroutine(threads[2]);
            for (int i = lobbyGojList[12].transform.parent.childCount - 1; i > 0; i--)
            {
                Destroy(lobbyGojList[12].transform.parent.GetChild(i).gameObject);
            }
            topCloneArr.Clear();
            int rowCount = 3;
            if (id != 1)
                rowCount = 7;

            for (int i = 0; i < rowCount; i++)
            {
                GameObject topSlideClone = Instantiate(lobbyGojList[12], lobbyGojList[12].transform.parent, false) as GameObject;
                topCloneArr.Add(topSlideClone);
                switch (i)
                {
                    case 0:
                        topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName1;
                        topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game1Icon;
                        topSlideClone.SetActive(true);
                        break;
                    case 1:
                        topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName2;
                        topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game5Icon;
                        topSlideClone.SetActive(true);
                        break;
                    case 2:
                        topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName3;
                        topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game6Icon;
                        topSlideClone.SetActive(true);
                        break;
                    case 3:
                        topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName4;
                        topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game7Icon;
                        topSlideClone.SetActive(true);
                        break;
                    case 4:
                        topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName5;
                        topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game4Icon;
                        topSlideClone.SetActive(true);
                        break;
                    case 5:
                        topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName6;
                        topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game3Icon;
                        topSlideClone.SetActive(true);
                        break;
                    case 6:
                        topSlideClone.GetComponentsInChildren<Text>()[0].text = "Minipoker";
                        topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game2Icon;
                        topSlideClone.SetActive(true);
                        break;

                }


                topSlideClone.GetComponentsInChildren<Text>()[1].text = App.formatMoney(fakePot[id][i].ToString());

                //topSlideClone.GetComponentsInChildren<Text>()[1].text = App.formatMoney(fakePot[fakeTopPotId][i * 3 + index - 1].ToString());
                lobbyFakeTopPotTxts[i] = topSlideClone.GetComponentsInChildren<Text>()[1];

            }
            switch (id)
            {
                case 0:
                    topCloneArr[6].transform.SetSiblingIndex(1);
                    topCloneArr[5].transform.SetSiblingIndex(2);
                    topCloneArr[3].transform.SetSiblingIndex(3);
                    topCloneArr[2].transform.SetSiblingIndex(4);
                    topCloneArr[4].transform.SetSiblingIndex(5);
                    topCloneArr[1].transform.SetSiblingIndex(6);
                    topCloneArr[0].transform.SetSiblingIndex(7);
                    break;
                case 1:
                    topCloneArr[0].transform.SetSiblingIndex(1);
                    topCloneArr[2].transform.SetSiblingIndex(2);
                    topCloneArr[1].transform.SetSiblingIndex(3);
                    break;
                case 2:
                    topCloneArr[1].transform.SetSiblingIndex(1);
                    topCloneArr[0].transform.SetSiblingIndex(2);
                    topCloneArr[5].transform.SetSiblingIndex(3);
                    topCloneArr[4].transform.SetSiblingIndex(4);
                    topCloneArr[2].transform.SetSiblingIndex(5);
                    topCloneArr[3].transform.SetSiblingIndex(6);
                    topCloneArr[6].transform.SetSiblingIndex(7);
                    break;
                case 3:
                    topCloneArr[2].transform.SetSiblingIndex(1);
                    topCloneArr[4].transform.SetSiblingIndex(2);
                    topCloneArr[1].transform.SetSiblingIndex(3);
                    topCloneArr[3].transform.SetSiblingIndex(4);
                    topCloneArr[0].transform.SetSiblingIndex(5);
                    topCloneArr[6].transform.SetSiblingIndex(6);
                    topCloneArr[5].transform.SetSiblingIndex(7);
                    break;
            }

            threads[1] = FakeTopPotChange(index);
            StartCoroutine(threads[1]);

        }

        public void GetValueTopPot(int id)
        {
            if (CPlayer.logedIn)
            {
                topCloneArr.Clear();
                TopPot[] gameObj;
                betCurrent = id;
                for (int i = lobbyGojList[12].transform.parent.childCount - 1; i > 0; i--)
                {
                    Destroy(lobbyGojList[12].transform.parent.GetChild(i).gameObject);
                }
                OutBounMessage req = new OutBounMessage("MINIGAME.GET_POT_ALL");
                req.addHead();
                App.ws.send(req.getReq(), delegate (InBoundMessage res)
                {

                    int count = res.readByte();
                    gameObj = new TopPot[count];
                //Debug.Log("count = " + count);
                for (int i = 0; i < count; i++)
                    {

                        gameObj[i] = new TopPot();
                        string gameId = res.readString();
                    //Debug.Log("gameId " + gameId);

                    gameObj[i].GameId = gameId;
                        int count1 = res.readByte();
                        gameObj[i].count = count1;

                        for (int j = 0; j < count1; j++)
                        {
                            int bet = res.readInt();

                            int value = res.readInt();
                        //   Debug.Log("=> gameId " + gameId+" bet "+bet+" value "+value);

                        switch (id)
                            {
                                case 0:
                                    if (bet == 100)
                                        gameObj[i].value[0] = value;
                                    break;
                                case 1:
                                    if (bet == 500)
                                        gameObj[i].value[1] = value;
                                    break;
                                case 2:
                                    if (bet == 1000)
                                        gameObj[i].value[2] = value;
                                    break;

                                case 3:
                                    if (bet == 10000)
                                        gameObj[i].value[3] = value;
                                    break;

                            }




                        }
                    }

                    SortMang(ref gameObj, id);
                    for (int i = 0; i < gameObj.Length; i++)
                    {

                        if (gameObj[i].value[id] > 1)
                        {
                            GameObject topSlideClone = Instantiate(lobbyGojList[12], lobbyGojList[12].transform.parent, false) as GameObject;

                            switch (gameObj[i].GameId)
                            {
                                case GameCodeApp.gameCode1:
                                    topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName1;
                                    topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game1Icon;
                                    topSlideClone.GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        /*  if (File.Exists(pathChest))
                                              StartCoroutine(_EnterGameByLoadData("chest", "Chest"));
                                          else
                                              LoadDataOpenScene("chest");*/
                                        SceneManager.LoadScene("Chest");

                                    });

                                    if (id == 0)
                                        lobbyPotTextSlide[0] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 1)
                                        lobbyPotTextSlide[1] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 2)
                                        lobbyPotTextSlide[2] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 3)
                                        lobbyPotTextSlide[3] = topSlideClone.GetComponentsInChildren<Text>()[1];


                                    topSlideClone.SetActive(true);
                                    break;
                                case GameCodeApp.gameCode2:
                                    topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName2;
                                    topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game5Icon;
                                    topSlideClone.GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        /*if (File.Exists(pathBird))
                                            StartCoroutine(_EnterGameByLoadData("bird", "Bird"));
                                        else
                                            LoadDataOpenScene("bird");*/
                                        SceneManager.LoadScene("Bird");
                                    });
                                    if (id == 0)
                                        lobbyPotTextSlide[4] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 1)
                                        lobbyPotTextSlide[5] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 2)
                                        lobbyPotTextSlide[6] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 3)
                                        lobbyPotTextSlide[7] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    topSlideClone.SetActive(true);
                                    break;
                                case GameCodeApp.gameCode3:
                                    topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName3;
                                    topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game6Icon;
                                    topSlideClone.GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        /* if (File.Exists(pathFish))
                                             StartCoroutine(_EnterGameByLoadData("fish", "Fish"));
                                         else
                                             LoadDataOpenScene("fish");*/
                                        SceneManager.LoadScene("Fish");
                                    });
                                    if (id == 0)
                                        lobbyPotTextSlide[8] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 1)
                                        lobbyPotTextSlide[9] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 2)
                                        lobbyPotTextSlide[10] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 3)
                                        lobbyPotTextSlide[11] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    topSlideClone.SetActive(true);
                                    break;

                                case GameCodeApp.gameCode4:
                                    topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName4;
                                    topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game7Icon;
                                    topSlideClone.GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        OpenSlotZombie();
                                    });

                                    if (id == 0)
                                        lobbyPotTextSlide[12] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 2)
                                        lobbyPotTextSlide[13] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 3)
                                        lobbyPotTextSlide[14] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    topSlideClone.SetActive(true);
                                    break;
                                case GameCodeApp.gameCode5:
                                    topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName5;
                                    topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game4Icon;
                                    topSlideClone.GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        OpenSlot3x3();
                                    });
                                    if (id == 0)
                                        lobbyPotTextSlide[15] = topSlideClone.GetComponentsInChildren<Text>()[1];

                                    else if (id == 2)
                                        lobbyPotTextSlide[16] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 3)
                                        lobbyPotTextSlide[17] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    topSlideClone.SetActive(true);
                                    break;
                                case GameCodeApp.gameCode6:

                                    topSlideClone.GetComponentsInChildren<Text>()[0].text = GameCodeApp.gameName6;
                                    topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game3Icon;
                                    topSlideClone.GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        OpenSlotOneLine();
                                    });
                                    if (id == 0)
                                        lobbyPotTextSlide[18] = topSlideClone.GetComponentsInChildren<Text>()[1];

                                    else if (id == 2)
                                        lobbyPotTextSlide[19] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 3)
                                        lobbyPotTextSlide[20] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    topSlideClone.SetActive(true);
                                    break;
                                case "minipoker":
                                    topSlideClone.GetComponentsInChildren<Text>()[0].text = "Minipoker";
                                    topSlideClone.GetComponentsInChildren<Image>()[2].overrideSprite = game2Icon;
                                    topSlideClone.GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        OpenMiniPoker();
                                    });
                                    if (id == 0)
                                        lobbyPotTextSlide[21] = topSlideClone.GetComponentsInChildren<Text>()[1];

                                    else if (id == 2)
                                        lobbyPotTextSlide[22] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    else if (id == 3)
                                        lobbyPotTextSlide[23] = topSlideClone.GetComponentsInChildren<Text>()[1];
                                    topSlideClone.SetActive(true);
                                    break;

                            }
                        // Debug.Log("id ="+id +"  "  +" gamid  "+ gameObj[i].GameId + " " + App.formatMoney(gameObj[i].value[id].ToString()));
                        topSlideClone.GetComponentsInChildren<Text>()[1].text = App.formatMoney(gameObj[i].value[id].ToString());
                        }
                    }
                });
            }
            else
            {
                CloneFakeTopPot(id);
            }
        }
        private void SortMang(ref TopPot[] pot, int id)
        {
            for (int i = 0; i < pot.Length; i++)
            {
                for (int j = i + 1; j < pot.Length; j++)
                {
                    if (pot[i].value[id] < pot[j].value[id])
                    {
                        TopPot tmp = pot[i];
                        pot[i] = pot[j];
                        pot[j] = tmp;
                    }
                }
            }

        }

        public void OpenGiftCode()
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(false);
            //lobbyGojList[3].SetActive(false);
            DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
            DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
            DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 220), .5f);
            //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
            DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 60), .5f).OnComplete(() =>
            {
                CPlayer.showGifCode = true;
                LoadingControl.instance.loadingGojList[7].SetActive(true);
            });
        }

        public void OpenSlideControl(string type)
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            switch (type)
            {
                case "events":
                    if (CPlayer.showEvent)
                        LoadingControl.instance.loadingGojList[29].SetActive(false);
                    break;
                case "fanpage":
                    OpenFanPage();
                    break;
                case "authen":
                    OpenExchange(true);
                    break;
            }
        }

        public void PlayNow()
        {
            //LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.RemoveAllListeners();
            //LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.AddListener(() => LoadingControl.instance.showErrorPanel());
            //LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.RemoveAllListeners();
            //LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.AddListener(() => LoadingControl.instance.showErrorPanel2());
            getCountryCodeURL();
            getForgetPassURL();
            checkSuccess();            
            if (!App.ws.getWS().isConnecting())
            {
                StartCoroutine(App.ws.getWS().Connect(() => {
                    DoCaptcha((url) => LoadingControl.instance.openWebview(url));
                }));
            }
            else
            {
                DoCaptcha((url) => LoadingControl.instance.openWebview(url));
            }
        }
        public void DoCaptcha(System.Action<string> callback = null)
        {
            var req_getlinkCaptcha = new OutBounMessage("LOGIN_NOW.CAPTCHA");
            req_getlinkCaptcha.addHead();
            req_getlinkCaptcha.writeAcii("sungaming.win-1.0.0");//application
            req_getlinkCaptcha.writeAcii("vi-VN");
            req_getlinkCaptcha.writeString(SystemInfo.deviceUniqueIdentifier.ToLower());

            App.ws.send(req_getlinkCaptcha.getReq(), delegate (InBoundMessage res_getlinkCaptcha)
            {
                App.trace("[RECV] LOGIN_NOW.CAPTCHA");
                int check = res_getlinkCaptcha.readInt();
                string url = res_getlinkCaptcha.readString();
                Debug.Log(url);
                if (check == 0)
                {
                    getCountryCodeURL((c) =>
                    {
                        StartCoroutine(_PlayNow());
                    });
                }
                else
                {
                    if (callback != null)
                    {
                        callback.Invoke(url);
                    }
                }
#if UNITY_IOS || UNITY_ANDROID
                //openWebview(url);
#else
            //Application.OpenURL(url);
            App.openNewTabWindow(url);
#endif
            });
        }

        public void checkSuccess()
        {
            {
                var req_CAPTCHA_SUCCESS = new OutBounMessage("LOGIN_CAPTCHA.SUCCESS");
                req_CAPTCHA_SUCCESS.addHead();
                App.ws.delHandler(req_CAPTCHA_SUCCESS.getReq());
                App.ws.sendHandler(req_CAPTCHA_SUCCESS.getReq(), delegate (InBoundMessage res_CAPTCHA_SUCCESS)
                {
                    App.trace("RECV [LOGIN_CAPTCHA.SUCCESS]");
                    int success = res_CAPTCHA_SUCCESS.readByte();
                    if (success == 0)
                    {
                        LoadingControl.instance.CloseWebViewLostPass();
                        LoadingControl.instance.CloseWebViewAuthen();
                        StartCoroutine(_PlayNow());
                    }
                });
            }
        }

        IEnumerator _PlayNow()
        {
            var req = new OutBounMessage("LOGIN_NOW");
            req.addHead();
            req.writeAcii(App.appCode);//application
            req.writeAcii(App.languageCode);
            req.writeByte(0);
#if UNITY_ANDROID
            req.writeString(SystemInfo.deviceUniqueIdentifier.ToLower());//device id
#elif UNITY_IOS
        string deviceId = DeviceIDManager.GetDeviceID();
        req.writeString(deviceId.ToLower());//device id
#endif
            req.writeAcii(""); //user
            req.writeString("");//pass
            req.writeString(App.getProvider());//provider
            req.writeString(countryCode);
            req.writeString(LoadingControl.instance.userID);
            req.writeByte(1);
            req.writeString(LoadingControl.instance.IpUser);
            //open socket
            //App.start();

            //if (!App.ws.getWS().isConnecting())
            //{
            //    yield return StartCoroutine(App.ws.getWS().Connect());
            //}

            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                LoadingControl.instance.showProcessing(false);
                App.trace("BBBBBB");
                LoadingControl.instance.loadingGojList[21].SetActive(true);


                CPlayer.logedIn = true;
                settingBtnGO.SetActive(true);
                button_Reg.transform.parent.gameObject.SetActive(false);

                App.trace("[RECV] LOGIN");
                CPlayer.id = res.readLong();
                CPlayer.cdn = res.readAscii();
                CPlayer.currPath = res.readString();
                CPlayer.currPass = res.readString();
                res.readString();
                res.readByte();
            //res.readByte();
            _getInfo();
                OpenTaiXiu();

            });
            yield break;
        }

        private Tween lightRotatation;
        public void getXpot()
        {
            //get event gamecode
            var req_xpot = new OutBounMessage("XPOT.INFO");
            req_xpot.addHead();
            App.ws.send(req_xpot.getReq(), delegate (InBoundMessage res_xpot)
            {
                int rateX = res_xpot.readInt(); //0:Không chạy sự kiện | 1: Có chạy sự kiên X hũ
                int size = res_xpot.readInt(); //Số game có X hũ
                gameCodeXpot = new string[size];
                for (int i = 0; i < size; i++)
                {
                    gameCodeXpot[i] = res_xpot.readString(); //Tên gamecode hiện tại đang được X hũ
                    Debug.Log("gameCode " + gameCodeXpot);
                }
                for (int i = 0; i < gameCodeXpot.Length; i++)
                {
                    switch (gameCodeXpot[i])
                    {
                        case GameCodeApp.gameCode1:
                            lobbyGojEventList[0].SetActive(true);
                            lobbyGojEventLightList[0].SetActive(true);
                            lightRotatation = lobbyGojEventLightList[0].transform.DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
                            break;
                        case GameCodeApp.gameCode2:
                            lobbyGojEventList[1].SetActive(true);
                            lobbyGojEventLightList[1].SetActive(true);
                            lightRotatation = lobbyGojEventLightList[1].transform.DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
                            break;
                        case GameCodeApp.gameCode3:
                            lobbyGojEventList[2].SetActive(true);
                            lobbyGojEventLightList[2].SetActive(true);
                            lightRotatation = lobbyGojEventLightList[2].transform.DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
                            break;
                        case GameCodeApp.gameCode4:
                            lobbyGojEventList[3].SetActive(true);
                            lobbyGojEventLightList[3].SetActive(true);
                            lightRotatation = lobbyGojEventLightList[3].transform.DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
                            break;
                        case GameCodeApp.gameCode5:
                            lobbyGojEventList[4].SetActive(true);
                            lobbyGojEventLightList[4].SetActive(true);
                            lightRotatation = lobbyGojEventLightList[4].transform.DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
                            break;
                        case GameCodeApp.gameCode6:
                            lobbyGojEventList[5].SetActive(true);
                            lobbyGojEventLightList[5].SetActive(true);
                            lightRotatation = lobbyGojEventLightList[5].transform.DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
                            break;
                        case "minipoker":
                            lobbyGojEventList[6].SetActive(true);
                            lobbyGojEventLightList[6].SetActive(true);
                            lightRotatation = lobbyGojEventLightList[6].transform.DORotate(new Vector3(0, 0, -360), 5, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
                            break;
                    }
                }
            });
        }
        IEnumerator GetCountryCode(string url, System.Action<bool, string> callback = null)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                if (callback != null)
                {
                    callback.Invoke(false, " bi loi, khong download dc text");
                }
            }
            else
            {
                cC = www.downloadHandler.text;
                Debug.Log(www.downloadHandler.text);
                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;
                if (callback != null)
                {
                    callback.Invoke(true, www.downloadHandler.text);
                    Debug.Log("countryCode:  " + cC);
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
                Debug.Log("ilu: " + url);
                StartCoroutine(GetCountryCode(url, (_success, _text) =>
                {
                    if (_success)
                    {
                        countryCode = cC.Trim();
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
                            countryCode = cC.Trim();
                            LoadingControl.instance.CountryC = countryCode;
                            callback.Invoke(countryCode);
                        }));
                    }

                }
                ));

            });
        }

        public void getForgetPassURL()
        {
            var req_ccurl = new OutBounMessage("LOST_PWD.URL");
            req_ccurl.addHead();
            req_ccurl.writeString(App.getProvider());
            App.ws.send(req_ccurl.getReq(), delegate (InBoundMessage res_ccurl)
            {
                forgetPassURL = res_ccurl.readString();
                Debug.Log("fpu: " + forgetPassURL);

            });
        }

        private IEnumerator checkVer()
        {
            yield return new WaitForSeconds(1f);
            checkMaintain(
                        // is not mantain
                        () =>
                        {
                            checkNewUpdate(
                            // is not update
                            () =>
                            {
                            },
                            // update not required
                            (content, url) =>
                            {
                                showUpdate(content, url);                                
                            },
                            // update required
                            (content, url) =>
                            {
                               showRequiredUpdate(content, url);
                            });
                        },
                        // maintain not required
                        (content) =>
                        {
                            showMaintain(content);                            
                        },
                        // maintain required
                        (content) =>
                        {
                            showRequiredMaintain(content);
                        });
        }

        public void checkMaintain(Action notHasMaintainCallback = null, Action<string> hasMaintainCallback = null, Action<string> hasRequiredMaintainCallback = null)
        {
            var req_newup = new OutBounMessage("APP.MAINTAIN");
            req_newup.addHead();
            req_newup.writeString(App.getDevicePlatform()); // Nền tảng android| ios | web
            req_newup.writeString(App.getProvider()); //Mã phiên bản vd: GG_QUAYTAY     
            App.ws.send(req_newup.getReq(), delegate (InBoundMessage res_newup)
            {
                string maintaining = res_newup.readString(); //0:Không bảo trì|1:Đang bảo trì
                string required = res_newup.readString(); //0:Không bắt buộc|1:Bắt buộc (bắt buộc = auto disconnect)
                string content = res_newup.readString(); // nội dung thông báo
                if(maintaining == "1")
                {
                    if(required == "0")
                    {
                        
                            if (hasMaintainCallback != null)
                            {
                                hasMaintainCallback.Invoke(content);
                            }                       
                    }
                    else 
                    {
                       
                            if (hasRequiredMaintainCallback != null)
                            {
                                hasRequiredMaintainCallback.Invoke(content);
                            }                        
                    }
                }
                else
                {
                    if (notHasMaintainCallback != null)
                    {
                        notHasMaintainCallback.Invoke();
                    }
                }
            });
        }

        public void checkNewUpdate(Action notHasNewVersionCallback = null, Action<string, string> hasNewVersionCallback = null, Action<string, string> hasNewRequiredVersionCallback = null)
        {
            var req_newup = new OutBounMessage("APP.NEW_VERSION");
            req_newup.addHead();
            req_newup.writeString(App.getDevicePlatform()); // Nền tảng android| ios | web
            req_newup.writeString(App.getProvider()); //Mã phiên bản vd: GG_QUAYTAY
            req_newup.writeString(Application.identifier); //Tên package app vd: com.quaytay.nohu
            req_newup.writeString(Application.version); //Phiên bản app            
            App.ws.send(req_newup.getReq(), delegate (InBoundMessage res_newup)
            {
                string isUpdated = res_newup.readString(); //0: không có update| 1: có update
                string required = res_newup.readString(); //0: không bắt buộc|1: bắt buộc (bắt buộc là auto reload scene, yêu cầu update)
                string content = res_newup.readString(); // nội dung thông báo
                string url = res_newup.readString(); // đường dẫn update
                if(isUpdated == "1")
                {
                    if(required == "0")
                    {
                        if (hasNewVersionCallback != null)
                        {
                            hasNewVersionCallback.Invoke(content, url);
                        }
                    }
                    else
                    {
                        if (hasNewRequiredVersionCallback != null)
                        {
                            hasNewRequiredVersionCallback.Invoke(content, url);
                        }
                    }
                }
                else
                {
                    if (notHasNewVersionCallback != null)
                    {
                        notHasNewVersionCallback.Invoke();
                    }
                }
            });
        }

        public static void showMaintain(string text)
        {

            if (Screen.orientation == ScreenOrientation.Landscape)
            {
                LoadingControl.instance.showCheckVer(text);
                LoadingControl.instance.loadingGojList[40].SetActive(false);
            }
            else
            {
                LoadingControl.instance.showCheckVer2(text);
                LoadingControl.instance.loadingGojList[41].SetActive(false);
            }
        }

        public static void showRequiredMaintain(string text)
        {
            if (Screen.orientation == ScreenOrientation.Landscape)
            {
                LoadingControl.instance.showCheckVer(text);
                LoadingControl.instance.loadingGojList[40].SetActive(false);
                LoadingControl.instance.loadingGojList[44].SetActive(false);
            }
            else
            {
                LoadingControl.instance.showCheckVer2(text);
                LoadingControl.instance.loadingGojList[41].SetActive(false);
                LoadingControl.instance.loadingGojList[45].SetActive(false);
            }
        }

        public static void showUpdate(string text, string url)
        {


            if (Screen.orientation == ScreenOrientation.Landscape)
            {
                LoadingControl.instance.showCheckVer(text);
                LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(url));
            }
            else
            {
                LoadingControl.instance.showCheckVer2(text);
                LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(url));
            }
        }

        public static void showRequiredUpdate(string text, string url)
        {
            if (Screen.orientation == ScreenOrientation.Landscape)
            {
                LoadingControl.instance.showCheckVer(text);
                LoadingControl.instance.loadingGojList[40].GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(url));
                LoadingControl.instance.loadingGojList[44].SetActive(false);
            }
            else
            {
                LoadingControl.instance.showCheckVer2(text);
                LoadingControl.instance.loadingGojList[41].GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(url));
                LoadingControl.instance.loadingGojList[45].SetActive(false);
            }
        }

        private void Update()
        {
            // comment out
            lobbyTxtList[1].text = MoneyManager.instance.FakeCoin;
            // test clear assetbundle

            //if(Input.GetKeyDown(KeyCode.Space))
            //{
            //    AssetBundle[] bundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
            //    Debug.Log("number of bundles " + bundles.Length);

            //    for (int i = 0; i < bundles.Length; i++)
            //    {
            //        Debug.Log("Bundle: " + bundles[i].name);
            //    }
            //}
            //if(Input.GetKeyDown(KeyCode.S))
            //{
            //    AssetBundle[] bundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
            //    bundles[0].Unload(true);
            //    bundles[1].Unload(true);
            //    bundles[2].Unload(true);
            //    bundles[3].Unload(true);
            //}
        }
    }
}
