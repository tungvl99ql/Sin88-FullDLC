using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using Spine.Unity;

namespace Core.Server.Api
{

    public class ChestControl : MonoBehaviour
    {
        public bool isTrial;
        public Button[] buttonHideSpin;

        /// <summary>
        /// 0: piece|1: line select panel|2-16: dot|17: lineToInstantiate|18-37: dotbesidecircleline|38:header
        /// |39: footer|40: main|41: line-circle-btns|42: bigwin|43:spotBreak|44: goldRushWin|45: pieceLines|45-48: pieceNet
        /// </summary>
        public RectTransform[] rtfs;

        /// <summary>
        /// 0-7:row1 | 8-15:row2 | 16-24:row3 | 25-32:row4 | 33-40:row5
        /// </summary>
        public RectTransform[] rftPiecesRow1;
        public RectTransform[] rftPiecesRow2;
        public RectTransform[] rftPiecesRow3;
        public RectTransform[] rftPiecesRow4;
        public RectTransform[] rftPiecesRow5;

        public GameObject[] rsPiecesControll;

        [Space]
        [Header("----------Item sprite list----------")]
        public Sprite[] itemSpriteList;
        [Space]

        /// <summary>
        /// 0: white|1: trans
        /// </summary>
        public Color[] colors;

        public Color[] lineColors;

        /// <summary>
        /// 0: nickName|1: balance|2: potAmount|3: betAmount|4: lineCount|5: totalBetAmount|6: winAmount|7: balanceRs
        /// 8: bigwin|9: noti|10: potbreak|11-25: goldRushRs|26: goldRush-numOfPick|27: goldRushTotalWin
        /// 28-31: pot-amount-bet-pick|32: honor | 33-36: potText | 37: cdGoldRush | 38: blanceMoney | 39: freeSpin
        /// </summary>
        public Text[] txts;

        [Space]
        [Header("---------- Chose item sprite list  ----------")]

        public Sprite[] itemChoseBetSpriteList;

        [Space]
        public Sprite autoSpinSprite;
        public Sprite autoSpinInactive;
        //public ProtectedSprite choseLineActive;
        //public ProtectedSprite choseLineInactived;
        public GameObject[] gojs;
        /// <summary>
        /// 0: autoSpin|1: real-fakeBtn
        /// </summary>
        public Button[] btns;
        public Button[] btnGlory;
        public Button[] btnChoseBet;
        #region //PRIVATE VARIABLE
        private List<LineSelect> lineSelectList;
        private List<int> lineBet;
        private Dictionary<int, int> betData = new Dictionary<int, int>();
        private int totalFreeSpin = 0;

        /// <summary>
        /// 0: totalBetAmount|1: betLineCount|2: balance|3: potAmount|4: betAmount|5-8: betData|9: currHisPage|10: totalGoldRush|11: numOfGoldRushPicked|
        /// </summary>
        private int[] numList = new int[12];                      //save amount data
                                                                  /// <summary>
                                                                  /// 0: wonBalance|1: freeSpinNum|2: isBigWin|3: isPotBreak|4: remainSpin
                                                                  /// </summary>
        private ArrayList rsSpinData = new ArrayList();
        private List<int> rsPieceIdsList = new List<int>();     //save piece ids after spin (from sv)
        private List<int> rsLineIdsList = new List<int>();     //save line id after spin (from sv)
        private Dictionary<int, int[]> wonLineData = new Dictionary<int, int[]>();
        private bool isWin = false;
        /// <summary>
        /// 0: isSpinning|1: allowSping|2: autoSpin|3: play-real|4: isBigWin|5: potbreak|6: testGoldRush
        /// 7: preventSpamSpin | 8: alowGoldRush
        /// </summary>
        public bool[] boolLs = new bool[9];
        private bool enoughMoney = false;
        private long lastPot = 0;
        public int currentBet = 0;
        public int currentXGoldRush = 0;
        public bool currentAutoSpin = false;
        private Dictionary<int, List<ArrayList>> achiveData = new Dictionary<int, List<ArrayList>>();         //save data (to get prize)
                                                                                                              /// <summary>
                                                                                                              /// 0: potAmount|1: _drawMutiLine|2: bigwin|3: noti|4: balance|5-7: pot-bet-select
                                                                                                              /// </summary>
        private IEnumerator[] tweenNum = new IEnumerator[8];
        private List<string> tweenIdList = new List<string>();
        private List<List<GameObject>> drawnLineTmp = new List<List<GameObject>>();
        private List<string> honored_ls = new List<string>();
        private Material materialSkeletonSpine;
        #endregion

        #region //CONST VARIABLE
        private List<int[]> wonLineIdList = new List<int[]>();
        #endregion

        [HideInInspector]
        public static ChestControl instance;

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
            // materialSkeletonSpine = Resources.Load("Material/SkeletonGraphicDefault.mat", typeof(Material)) as Material;
            //  int leght = rftPiecesRow1.Length;
            //  for (int i = 0; i < leght; i++)
            // {
            /* rftPiecesRow1[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
             rftPiecesRow2[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
             rftPiecesRow3[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
             rftPiecesRow4[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
             rftPiecesRow5[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;*/
            //  }
            getInstance();
        }



        // Use this for initialization
        void Start()
        {

            for (int j = 0; j < btnChoseBet.Length; j++)
            {
                btnChoseBet[j].interactable = false;
            }
            LoadingControl.instance.loadingGojList[30].SetActive(false);
            lineBet = new List<int>();
            SoundManager.instance.PlayBackgroundSound(SoundFX.LOBBY_BG_MUSIC_1);
            //gojs[33].SetActive(!CPlayer.hidePayment);
            //CPlayer.changed += BalanceChanged;
            CPlayer.potchanged += PotChanged;
            CPlayer.forceStopGameEvent += EnoughMoney;
            //by default, betAmount = 100
            numList[4] = 100;
            boolLs[7] = false;

            //by default, 20 lines are selected and total bet amount gets max value white curr bet amount
            numList[1] = 20;
            txts[4].text = numList[1].ToString();
            numList[0] = numList[1] * numList[4];
            txts[5].text = App.formatMoney(numList[0].ToString());

            //by default, 20 lines, each line contain an int array which has 5 dot ids
            wonLineIdList.Clear();
            wonLineIdList.Add(new int[] { 2, 5, 8, 11, 14 });
            wonLineIdList.Add(new int[] { 1, 4, 7, 10, 13 });
            wonLineIdList.Add(new int[] { 3, 6, 9, 12, 15 });
            wonLineIdList.Add(new int[] { 2, 5, 7, 11, 14 });
            wonLineIdList.Add(new int[] { 2, 5, 9, 11, 14 });

            wonLineIdList.Add(new int[] { 1, 4, 8, 10, 13 });
            wonLineIdList.Add(new int[] { 3, 6, 8, 12, 15 });
            wonLineIdList.Add(new int[] { 1, 6, 7, 12, 13 });
            wonLineIdList.Add(new int[] { 3, 4, 9, 10, 15 });
            wonLineIdList.Add(new int[] { 2, 4, 9, 10, 14 });

            wonLineIdList.Add(new int[] { 3, 5, 7, 11, 15 });
            wonLineIdList.Add(new int[] { 1, 5, 9, 11, 13 });
            wonLineIdList.Add(new int[] { 2, 6, 8, 10, 14 });
            wonLineIdList.Add(new int[] { 2, 4, 8, 12, 14 });
            wonLineIdList.Add(new int[] { 3, 5, 8, 11, 15 });

            wonLineIdList.Add(new int[] { 1, 5, 8, 11, 13 });
            wonLineIdList.Add(new int[] { 2, 6, 9, 12, 14 });
            wonLineIdList.Add(new int[] { 2, 4, 7, 10, 14 });
            wonLineIdList.Add(new int[] { 3, 6, 8, 10, 13 });
            wonLineIdList.Add(new int[] { 1, 4, 8, 12, 15 });

            loadData();
            loadBetPotValue();
            //return;

            var req_GET_INFO = new OutBounMessage("GOBLINS.GET_INFO");
            req_GET_INFO.addHead();
            App.ws.send(req_GET_INFO.getReq(), delegate (InBoundMessage res_GET_INFO)
            {
            //  App.trace("[RECV] GOBLINS.GET_INFO");

            int count = res_GET_INFO.readByte();                                 //bet
            for (int i = 0; i < count; i++)
                {
                    int betAmount = res_GET_INFO.readInt();
                //App.trace("betAmount = " + betAmount,"yellow");
                numList[5 + i] = betAmount;
                }

                currentBet = numList[5];                                            //set default bet
            txts[3].text = App.FormatMoney(currentBet);

                achiveData.Clear();                                                 //clear data

            count = res_GET_INFO.readByte();                                    //prize
                                                                                //App.trace("Phắc cừn wao sịt " + count,"yellow");
            for (int i = 0; i < count; i++)
                {
                    int index = res_GET_INFO.readByte();                            //piece index
                int prizeNum = res_GET_INFO.readByte();                         //num of piece to get prize
                string amount = res_GET_INFO.readString();                      //achive
                                                                                //App.trace("index = " + index + "|prizeNum = " + prizeNum + "|amount = " + amount,"yellow");
                                                                                //index = 2|prizeNum = 5|amount = Nổ hũ
                if (achiveData.ContainsKey(index))
                        achiveData[index].Add(new ArrayList() { prizeNum, amount });
                    else
                        achiveData.Add(index, new List<ArrayList>() { new ArrayList() { prizeNum, amount } });
                }
                for (int j = 0; j < btnChoseBet.Length; j++)
                {
                    btnChoseBet[j].interactable = true;
                }
            //getInfoDetail();
        });


            boolLs[0] = false;
            boolLs[1] = true;

        }
        private const string Gamecode = GameCodeApp.gameCode1;
        private void loadBetPotValue()
        {
            OutBounMessage req = new OutBounMessage("MINIGAME.GET_POT_ALL");
            req.addHead();
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                int count = res.readByte();
                for (int i = 0; i < count; i++)
                {
                    string gameId = res.readString();
                    if (gameId == Gamecode)
                    {
                        int count0 = res.readByte();
                        for (int j = 0; j < count0; j++)
                        {
                            int bet = res.readInt();
                            int value = res.readInt();
                            App.trace(value);
                            txts[28 + j].text = App.formatMoney(value.ToString());
                        }
                    }
                    else
                    {
                        int count1 = res.readByte();
                        for (int j = 0; j < count1; j++)
                        {
                            int bet = res.readInt();
                            int value = res.readInt();
                        }
                    }

                }


            });
            //  gojs[21].SetActive(true);
        }

        #region //INNER CLASS
        public class LineSelect
        {
            private bool isSlect;
            private Image btnCirlce;                        //Line select in main body
            private Image btnRect;                          //Line select in pop up
            private int index;

            public LineSelect(int index, Image rect, Image circle, bool isSelect)
            {
                this.IsSlect = isSelect;
                this.BtnCirlce = circle;
                this.BtnRect = rect;
                this.IsSlect = isSelect;
            }

            public bool IsSlect
            {
                get
                {
                    return isSlect;
                }

                set
                {
                    isSlect = value;
                }
            }

            public Image BtnCirlce
            {
                get
                {
                    return btnCirlce;
                }

                set
                {
                    btnCirlce = value;
                }
            }

            public Image BtnRect
            {
                get
                {
                    return btnRect;
                }

                set
                {
                    btnRect = value;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }

                set
                {
                    index = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class RsPiece
        {
            private Image main;
            private Image overlay;
            private Image border;
            private int pieceIndex;
            private ParticleSystem fx;
            public RsPiece(Image main, Image overlay, Image border, int pieceIndex, ParticleSystem fx)
            {
                this.Main = main;
                this.Overlay = overlay;
                this.Border = border;
                this.PieceIndex = pieceIndex;
                this.Fx = fx;
            }

            public Image Main
            {
                get
                {
                    return main;
                }

                set
                {
                    main = value;
                }
            }

            public Image Overlay
            {
                get
                {
                    return overlay;
                }

                set
                {
                    overlay = value;
                }
            }

            public Image Border
            {
                get
                {
                    return border;
                }

                set
                {
                    border = value;
                }
            }

            public int PieceIndex
            {
                get
                {
                    return pieceIndex;
                }

                set
                {
                    pieceIndex = value;
                }
            }

            public ParticleSystem Fx
            {
                get
                {
                    return fx;
                }

                set
                {
                    fx = value;
                }
            }
        }
        #endregion

        //private void BalanceChanged(string type)
        //{
        //    if (type == "chip")
        //    {
        //        if (CPlayer.preChipBalance >= CPlayer.chipBalance)
        //        {
        //            //App.trace("Phắc cừn wao sịt ");
        //            txts[1].text = string.Format("{0:0,0}", CPlayer.chipBalance);
        //            txts[38].text = string.Format("{0:0,0}", CPlayer.chipBalance);
        //        }
        //    }
        //}

        private void loadData()
        {
            boolLs[0] = false;                                  //Is spinning
            boolLs[1] = true;                                   //Allow spin
            boolLs[2] = false;                                  //Auto spin
            boolLs[3] = true;                                   //By default, real play = true
            boolLs[4] = false;                                   //By default, big win = false
            boolLs[5] = false;                                   //By default, pot break = false

            //txts[0].text = CPlayer.fullName.Length == 0 ? App.formatNickName(CPlayer.nickName, 15)
            //: App.formatNickName(CPlayer.fullName, 15);     //Set nick name

            //txts[0].text = App.formatNickName(CPlayer.nickName, 15);

            //txts[1].text = App.formatMoney(CPlayer.chipBalance.ToString());     //Set balance
            //txts[38].text = App.formatMoney(CPlayer.chipBalance.ToString());
            //if (CPlayer.avatarSpriteToSave != null)
            //{
            //    imgs[0].sprite = CPlayer.avatarSpriteToSave;
            //}

            //default: 20 line are selected
            lineSelectList = new List<LineSelect>();
            for (int i = 0; i < 20; i++)
            {
                lineSelectList.Add(new LineSelect(i + 1, null, gojs[1 + i].GetComponent<Image>(), true));
            }

            var req_HONOUR = new OutBounMessage("HONOUR");
            req_HONOUR.addHead();
            App.ws.send(req_HONOUR.getReq(), delegate (InBoundMessage res_HONOUR)
            {
                List<string> arr = res_HONOUR.readStringArray();
                App.trace("[RECV] HONOUR " + arr.Count);
                if (arr.Count == 0)
                    return;
                foreach (var item in arr)
                {
                //App.trace("- " + item);
                honored_ls.Add(item);
                }
                txts[32].text = honored_ls[UnityEngine.Random.Range(0, honored_ls.Count)];
                txts[32].transform.DOLocalMoveX(450 + .5f * txts[32].preferredWidth, .01f).OnComplete(() =>
                {
                    float timeToMove = (txts[32].preferredWidth / 1000) * 10f;
                //txts[15].transform.localPosition = 
                txts[32].transform.DOLocalMoveX(-450 - .5f * txts[32].preferredWidth, timeToMove).SetId("noti").SetLoops(-1).OnStepComplete(() =>
                    {

                        if (honored_ls.Count > 0)
                        {
                            txts[32].text = honored_ls[UnityEngine.Random.Range(0, honored_ls.Count)];
                        }
                        txts[32].transform.localPosition = new Vector2(450 + .5f * txts[32].preferredWidth, 0);
                    }); ;
                });
            });
        }

        private void getInfoDetail()
        {
            OutBounMessage req_INFO_DETAIL = new OutBounMessage("GOBLINS.GET_INFO_DETAIL");
            req_INFO_DETAIL.addHead();
            req_INFO_DETAIL.writeInt(currentBet);           //Curr bet
            req_INFO_DETAIL.writeString(Gamecode);
            App.ws.send(req_INFO_DETAIL.getReq(), delegate (InBoundMessage res_INFO_DETAIL)
            {
                int freeSpinNum = res_INFO_DETAIL.readInt();                          //Free spins
            int potAmount = res_INFO_DETAIL.readInt();                          //
            string lastStage = res_INFO_DETAIL.readString();
                int lastBet = res_INFO_DETAIL.readInt();
            //App.trace("freeSpinNum " + freeSpinNum+"current Bet "+currentBet,"yellow");
            App.trace("freeCount = " + freeSpinNum + "|potAmount = " + potAmount + "|lastBet = " + lastBet + "|lastStage = " + lastStage, "yellow");
            //freeCount = 0|potAmount = 1943190|lastBet = |lastBetAmount = 0
            totalFreeSpin = freeSpinNum;
                if (freeSpinNum > 0)
                {
                    txts[39].gameObject.SetActive(true);
                    txts[27].text = freeSpinNum.ToString();
                    string[] lastLine = lastStage.Split('-');
                    for (int i = 0; i < lastLine.Length; i++)
                    {
                        lineBet.Add(Int32.Parse(lastLine[i]));
                    }
                    numList[1] = lineBet.Count;
                }
                else
                {
                    txts[39].gameObject.SetActive(false);
                }
            });
        }


        private void PreventSpamSpin()
        {

            drawnLineTmp.Clear();
            DoAction("spinn");
            boolLs[7] = false;
        }

        public void DoAction(string t)
        {
            switch (t)
            {
                #region //SPIN

                case "spin":
                    if (boolLs[7] == true)
                        return;
                    boolLs[7] = true;
                    Invoke("PreventSpamSpin", .5f);
                    break;

                case "spinn":
                    if (boolLs[1] == false)
                    {
                        //StartCoroutine(ShowNoti("Vui lòng chờ quay xong."));
                        return;
                    }
                    boolLs[0] = true;
                    boolLs[1] = false;
                    //txts[1].transform.localPosition = Vector2.one;
                    txts[6].text = "";                                //Set 0 to won amount
                    isWin = false;
                    boolLs[4] = boolLs[5] = false;                    //bigWin & potbreak = false
                    if (tweenNum[1] != null)
                        StopCoroutine(tweenNum[1]);

                    spinThreads.Clear();
                    //Spin on sv
                    OutBounMessage req_SPIN = new OutBounMessage("GOBLINS.START");
                    req_SPIN.addHead();
                    req_SPIN.writeInt(currentBet);                              //bet amount
                    req_SPIN.writeByte(numList[1]);                             //selected lines count
                    numList[0] = 0;
                    if (totalFreeSpin > 0)
                    {
                        if (lineBet.Count == 0)
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                if (lineSelectList[i].IsSlect == true)
                                {
                                    req_SPIN.writeByte(i);                              //single selected line
                                                                                        //App.trace("select " + (i + 1));
                                    numList[0] += currentBet;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < lineBet.Count; i++)
                            {
                                req_SPIN.writeByte(lineBet[i]);
                                numList[0] += currentBet;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            if (lineSelectList[i].IsSlect == true)
                            {
                                req_SPIN.writeByte(i);                              //single selected line
                                                                                    //App.trace("select " + (i + 1));
                                numList[0] += currentBet;
                            }
                        }
                    }
                    req_SPIN.writeByte(boolLs[3] ? 0 : 1);                                      //is demo spin
                    req_SPIN.writeString(Gamecode);                        //game id
                    req_SPIN.writeByte(1);                                      //0: freeSpin|1: goldRush
                    App.ws.send(req_SPIN.getReq(), delegate (InBoundMessage res_SPIN)
                    {

                        rsPieceIdsList.Clear();
                        rsLineIdsList.Clear();
                        wonLineData.Clear();

                        int count = res_SPIN.readByte();                        //pieces id
                    App.trace("[RECV] SLOT_GOLD_RUSH.START " + count);
                        int[] ids = new int[count];
                    // Debug.Log("count = " + count);
                    for (int i = 0; i < count; i++)
                        {
                        // Debug.Log("count = " + count);
                        int index = res_SPIN.readByte();
                            ids[i] = index;
                        //  Debug.Log("index = "+index.ToString());
                        rsPieceIdsList.Add(index);
                        // rsPiecesControll[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().skeletonDataAsset = skeletonDataAssets[index - 1];
                        //rsPiecesControll[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().Initialize(true);
                        rsPiecesControll[i].transform.GetChild(0).GetComponent<Image>().overrideSprite = itemSpriteList[index - 1];

                        }


                        count = res_SPIN.readByte();                                //prize count
                    Debug.Log("prize count = " + count);
                        for (int i = 0; i < count; i++)
                        {
                            int lineIndex = res_SPIN.readByte();                    //won line (real-= 1)
                        int pieceIndex = res_SPIN.readByte();                   //won piece index
                        int pieceNum = res_SPIN.readByte();                     //pieces num in won line
                                                                                //App.trace(lineIndex + "|" + pieceIndex + "|" + pieceNum,"yellosw");
                                                                                // Debug.Log(lineIndex + "|" + pieceIndex + "|" + pieceNum);
                        rsLineIdsList.Add(lineIndex);
                            wonLineData.Add(lineIndex, new int[] { pieceIndex, pieceNum });
                        }

                        int wonBalance = res_SPIN.readInt();
                        if (wonBalance > 0)
                        {
                            isWin = true;
                        }
                        int freeSpinNum = res_SPIN.readByte();
                    //totalFreeSpin = freeSpinNum;
                    //  Debug.Log("freeSpinNum = " + freeSpinNum);
                    int remainSpint = res_SPIN.readByte();
                        currentXGoldRush = remainSpint;
                        bool isBigWin = res_SPIN.readByte() == 1;
                        boolLs[4] = isBigWin;
                        bool isPotBreak = res_SPIN.readByte() == 1;
                        boolLs[5] = isPotBreak;
                        if (boolLs[5] && boolLs[2])
                        {
                            DoAction("changeAutoSpin");
                        }
                        totalFreeSpin = res_SPIN.readInt();
                        Debug.Log("totalFreeSpin = " + totalFreeSpin);
                        isTrial = res_SPIN.readByte() == 1;
                    //wonBalance = 987654;
                    //if (freeSpinNum > 0)
                    //{
                    //    txts[39].gameObject.SetActive(true);
                    //    txts[27].text = freeSpinNum.ToString();
                    //}
                    //else
                    //{
                    //    lineBet.Clear();
                    //    txts[39].gameObject.SetActive(false);
                    //}
                    rsSpinData.Clear();
                        rsSpinData.Add(wonBalance);
                        rsSpinData.Add(freeSpinNum);
                        rsSpinData.Add(isBigWin);
                        rsSpinData.Add(isPotBreak);
                        rsSpinData.Add(remainSpint);
                        App.trace("wonBalance = " + wonBalance + "|freeSpinNum = " + freeSpinNum + "|isBigWin = " + isBigWin + "|isPotBreak = " + isPotBreak + "|xgame = " + remainSpint + " |totalFreeSpin " + totalFreeSpin, "red");

                    //Do spin
                    foreach (string item in tweenIdList)
                        {
                            DOTween.Kill(item);
                        }
                        tweenIdList.Clear();
                        StartCoroutine(_StopSpin());

                    });
                    break;
                #endregion

                #region //SELECT LINE
                case "selectAllLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    numList[1] = 20;
                    txts[4].text = numList[1].ToString();
                    numList[0] = 20 * numList[4];
                    txts[5].text = App.formatMoney(numList[0].ToString());

                    for (int i = 0; i < 20; i++)
                    {
                        if (rtfs[1].gameObject.activeSelf == true)
                        {
                            //lineSelectList[i].BtnRect.sprite = sprts[7 + i];
                            lineSelectList[i].BtnRect.sprite = itemChoseBetSpriteList[i];
                            //lineSelectList[i].BtnCirlce.sprite = choseLineActive.Sprite;
                        }
                        lineSelectList[i].IsSlect = true;
                    }
                    break;
                case "deSelectAllLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    numList[1] = 0;
                    txts[4].text = 0.ToString();
                    numList[0] = 0;
                    txts[5].text = 0.ToString();

                    for (int i = 0; i < 20; i++)
                    {
                        lineSelectList[i].BtnRect.sprite = itemChoseBetSpriteList[20 + i];
                        //lineSelectList[i].BtnCirlce.sprite = choseLineInactived.Sprite;
                        lineSelectList[i].IsSlect = false;

                    }
                    break;
                case "selectEvenLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    numList[1] = 10;
                    txts[4].text = numList[1].ToString();
                    numList[0] = 10 * numList[4];
                    txts[5].text = App.formatMoney(numList[0].ToString());

                    for (int i = 0; i < 20; i++)
                    {
                        if (i % 2 == 1)
                        {
                            lineSelectList[i].BtnRect.sprite = itemChoseBetSpriteList[i];
                            //lineSelectList[i].BtnCirlce.sprite = choseLineActive.Sprite;
                            lineSelectList[i].IsSlect = true;
                        }
                        else
                        {
                            lineSelectList[i].BtnRect.sprite = itemChoseBetSpriteList[20 + i];
                            //lineSelectList[i].BtnCirlce.sprite = choseLineInactived.Sprite;
                            lineSelectList[i].IsSlect = false;
                        }
                    }
                    break;
                case "selectOddLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    numList[1] = 10;
                    txts[4].text = numList[1].ToString();
                    numList[0] = 10 * numList[4];
                    txts[5].text = App.formatMoney(numList[0].ToString());
                    for (int i = 0; i < 20; i++)
                    {
                        if (i % 2 == 0)
                        {
                            lineSelectList[i].BtnRect.sprite = itemChoseBetSpriteList[i];
                            //lineSelectList[i].BtnCirlce.sprite = choseLineActive.Sprite;
                            lineSelectList[i].IsSlect = true;
                        }
                        else
                        {
                            lineSelectList[i].BtnRect.sprite = itemChoseBetSpriteList[20 + i];
                            //lineSelectList[i].BtnCirlce.sprite = choseLineInactived.Sprite;
                            lineSelectList[i].IsSlect = false;
                        }
                    }
                    break;

                #endregion

                #region //RECHARGE
                case "openRecharge":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    if (boolLs[0] == true)
                    {
                        if (tweenNum[3] != null)
                            StopCoroutine(tweenNum[3]);
                        //tweenNum[3] = ShowNoti("Không thể sử dụng tính năng này khi đang quay.");
                        //StartCoroutine(tweenNum[3]);
                        return;
                    }
                    LoadingControl.instance.loadingGojList[21].SetActive(false);
                    if (CPlayer.showEvent)
                        LoadingControl.instance.loadingGojList[29].SetActive(false);
                    DOTween.To(() => rtfs[38].anchoredPosition, x => rtfs[38].anchoredPosition = x, new Vector2(0, 300), .35f);
                    DOTween.To(() => rtfs[39].anchoredPosition, x => rtfs[39].anchoredPosition = x, new Vector2(0, -300), .35f);
                    DOTween.To(() => rtfs[40].anchoredPosition, x => rtfs[40].anchoredPosition = x, new Vector2(-1800, 0), .35f);
                    DOTween.To(() => rtfs[41].anchoredPosition, x => rtfs[41].anchoredPosition = x, new Vector2(-1800, 0), .5f);
                    LoadingControl.instance.loadingGojList[7].SetActive(true);
                    break;
                case "closeRecharge":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    LoadingControl.instance.loadingGojList[21].SetActive(true);
                    if (CPlayer.showEvent)
                        LoadingControl.instance.loadingGojList[29].SetActive(true);
                    DOTween.To(() => rtfs[38].anchoredPosition, x => rtfs[38].anchoredPosition = x, new Vector2(0, 0), .35f);
                    DOTween.To(() => rtfs[39].anchoredPosition, x => rtfs[39].anchoredPosition = x, new Vector2(0, 0), .35f);
                    DOTween.To(() => rtfs[40].anchoredPosition, x => rtfs[40].anchoredPosition = x, new Vector2(10, 0), .35f);
                    DOTween.To(() => rtfs[41].anchoredPosition, x => rtfs[41].anchoredPosition = x, new Vector2(3, 14), .35f);
                    break;
                #endregion

                #region //CHANGE BET AMOUNT
                case "openChangeBet":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    if (enoughMoney)
                    {
                        SceneManager.LoadScene("Chest");
                        //CPlayer.changed -= BalanceChanged;
                        CPlayer.potchanged -= PotChanged;
                        StopAllCoroutines();
                        DOTween.Kill("noti");
                        DOTween.Kill("texttween");
                        DOTween.Kill("firstSpin");
                        DOTween.Kill("secondSpin");
                    }
                    if (boolLs[0])
                    {
                        if (tweenNum[3] != null)
                            StopCoroutine(tweenNum[3]);
                        //tweenNum[3] = ShowNoti("Không thể thay đổi mức cược khi đang quay.");
                        //StartCoroutine(tweenNum[3]);
                        return;
                    }
                    if (boolLs[2])
                    {
                        //tweenNum[3] = ShowNoti("Không thể thay đổi mức cược khi đang quay tự động.");
                        //StartCoroutine(tweenNum[3]);
                        return;
                    }
                    Destroy(winnerTextClone);
                    txts[7].gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
                    for (int i = 0; i < 3; i++)
                    {
                        if (betData.ContainsKey(numList[5 + i]))
                        {
                            if (tweenNum[5 + i] != null)
                                StopCoroutine(tweenNum[5 + i]);
                            tweenNum[5 + i] = TweenNum(txts[28 + i], 0, betData[numList[5 + i]], 3, 1f);
                            StartCoroutine(tweenNum[5 + i]);
                        }
                        txts[33 + i].text = App.FormatMoney(numList[5 + i]);
                    }
                    gojs[21].SetActive(true);
                    break;
                case "closeChangeBet":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    gojs[21].SetActive(false);
                    txts[32].text = "";
                    break;
                #endregion

                #region //SHOW ACHIVE TABLE
                case "openAchive":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    gojs[23].SetActive(true);
                    /*   {
                           foreach (Transform rtf in gojs[22].transform.parent)       //Delete exits element before
                           {
                               if (rtf.gameObject.name != gojs[22].name)
                               {
                                   Destroy(rtf.gameObject);
                               }
                           }

                           foreach (int itemIndex in achiveData.Keys.ToList())
                           {


                               List<ArrayList> ls = achiveData[itemIndex];

                               if ((string)ls[0][1] == "Nổ hũ")
                               {
                                   Text[] txtArr = gojs[24].GetComponentsInChildren<Text>();
                                   Image[] imgArr = gojs[24].GetComponentsInChildren<Image>();
                                   for (int i = 0; i < ls.Count; i++)
                                   {
                                       txtArr[i].text = ls[i][0] + " = " + ls[i][1];
                                       imgArr[i].sprite = sprts[itemIndex - 1];
                                   }
                                   continue;
                               }

                               GameObject goj = Instantiate(gojs[22], gojs[22].transform.parent, false);
                               Text txt = goj.GetComponentInChildren<Text>();
                               string tmp = "";
                               for (int i = 0; i < ls.Count; i++)
                               {
                                   tmp += ls[i][0] + " = " + ls[i][1];
                                   if (i != ls.Count - 1)
                                       tmp += "\n";
                               }
                               txt.text = tmp;
                               Image img = goj.GetComponentInChildren<Image>();
                               img.sprite = sprts[itemIndex - 1];
                               goj.SetActive(true);
                           }
                       }*/
                    break;
                case "closeAchive":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    gojs[23].SetActive(false);
                    break;
                #endregion

                #region //Help | chart
                case "showChart":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    numList[9] = 0;                                                 //Set currpage to 1
                    if (gojs[25].activeSelf == true)
                    {
                        gojs[25].SetActive(false);
                    }
                    else
                    {
                        gojs[25].SetActive(true);

                        foreach (Transform rtf in gojs[26].transform.parent)       //Delete exits element before
                        {
                            if (rtf.gameObject.name != gojs[26].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }

                        LoadSmt("his");
                    }
                    break;
                #endregion

                #region //SHOW BIG WIN
                case "bigWin":
                    SoundManager.instance.PlayUISound(SoundFX.BIG_WIN + "_" + UnityEngine.Random.Range(1, 4));
                    txts[8].text = 0.ToString();
                    rtfs[42].transform.localScale = 5 * Vector2.one;
                    rtfs[42].gameObject.SetActive(true);
                    rtfs[42].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() =>
                    {
                        if (tweenNum[2] != null)
                            StopCoroutine(tweenNum[2]);
                        tweenNum[2] = TweenNum(txts[8], 0, (int)rsSpinData[0]);
                        StartCoroutine(tweenNum[2]);
                        txts[6].text = string.Format("{0:0,0}", rsSpinData[0]);
                    });
                    //StartCoroutine(HideSmt(rtfs[42].gameObject, 5f));
                    Invoke("AllowSpin", 5f);
                    break;
                #endregion

                #region //SHOW POT BREAK
                case "potBreak":
                    //SoundManager.instance.PlayEffectSound(SoundFX.JACKPOT + "_" + UnityEngine.Random.Range(1, 3));
                    txts[10].text = 0.ToString();
                    rtfs[43].transform.localScale = 5 * Vector2.one;
                    rtfs[43].gameObject.SetActive(true);
                    rtfs[43].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() =>
                    {
                        if (tweenNum[2] != null)
                            StopCoroutine(tweenNum[2]);
                        tweenNum[2] = TweenNum(txts[10], 0, (int)rsSpinData[0]);
                        StartCoroutine(tweenNum[2]);
                        txts[6].text = string.Format("{0:0,0}", rsSpinData[0]);
                    });
                    //StartCoroutine(HideSmt(rtfs[43].gameObject, 7f));
                    Invoke("AllowSpin", 7f);
                    break;
                #endregion

                #region //SHOW GOLD RUSH TOTAL WIN
                case "totalGoldRush":
                    App.trace("SHOW TOTAL GOLD RUSH");
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    rtfs[44].transform.localScale = 5 * Vector2.one;
                    txts[27].text = "";
                    rtfs[44].gameObject.SetActive(true);
                    rtfs[44].DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() =>
                    {
                        if (tweenNum[2] != null)
                            StopCoroutine(tweenNum[2]);
                        tweenNum[2] = TweenNum(txts[27], 0, numList[10]);
                        StartCoroutine(tweenNum[2]);
                    });
                    StartCoroutine(HideSmt(rtfs[44].gameObject, 3f));
                    break;
                #endregion


                case "setting":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    LoadingControl.instance.loadingGojList[17].SetActive(true);
                    break;

                #region //AUTO SPIN | PLAY REAL
                case "changeAutoSpin":
                    SoundManager.instance.PlayUISound(SoundFX.SPIN);
                    if (tweenNum[3] != null)
                        StopCoroutine(tweenNum[3]);
                    SpriteState spriteState = btns[0].spriteState;

                    if (boolLs[2] == false)
                    {
                        if (boolLs[3] == false)
                        {
                            //tweenNum[3] = ShowNoti("Không thể sử dụng tính năng này khi đang chơi thử.");
                            //StartCoroutine(tweenNum[3]);
                            return;
                        }
                        boolLs[2] = true;
                        btns[0].image.sprite = autoSpinInactive;
                        spriteState.pressedSprite = autoSpinInactive;
                        //DoAction("spin");
                        NewSpin();
                    }
                    else
                    {
                        boolLs[2] = false;
                        btns[0].image.sprite = autoSpinSprite;
                        spriteState.pressedSprite = autoSpinSprite;
                        if (boolLs[0] == false && boolLs[1] == true)
                        {
                            buttonActiveSpine.enabled = true;
                            for (int i = 0; i < buttonHideSpin.Length; i++)
                            {
                                buttonHideSpin[i].interactable = true;

                            }

                            if (boolLs[3] == true)
                                btns[0].interactable = true;
                        }
                        else
                            btns[0].interactable = false;
                    }
                    txts[6].text = "";
                    btns[0].spriteState = spriteState;
                    break;
                case "changeRealPlay":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    if (tweenNum[3] != null)
                        StopCoroutine(tweenNum[3]);
                    string notiString = "Không thể dùng tính năng này khi đang quay";

                    if (boolLs[0] == true)
                    {
                        //tweenNum[3] = ShowNoti(notiString);
                        //StartCoroutine(tweenNum[3]);
                        return;
                    }
                    if (boolLs[2])
                    {
                        //tweenNum[3] = ShowNoti("Không thể thay đổi dòng cược khi đang quay tự động.");
                        //StartCoroutine(tweenNum[3]);
                        return;
                    }
                    spriteState = btns[1].spriteState;

                    /*
                    if (gojs[28].activeSelf)
                    {
                        tweenNum[3] = ShowNoti("Không thể dùng tính năng này khi còn vòng quay miễn phí.");
                        StartCoroutine(tweenNum[3]);
                        return;
                    }*/
                    if (boolLs[3] == true)          //Change to fake spin
                    {
                        boolLs[3] = false;
                        //btns[1].image.sprite = sprts[60];
                        //spriteState.pressedSprite = sprts[61];
                        notiString = "Bạn đang ở chế độ chơi thử";
                        DoAction("selectAllLine");

                        //Change btnAuto to false
                        SpriteState spriteState1 = btns[0].spriteState;
                        boolLs[2] = false;
                        btns[0].image.sprite = autoSpinSprite;
                        spriteState1.pressedSprite = autoSpinSprite;
                    }
                    else
                    {                               //change to real spin
                        boolLs[3] = true;
                        //btns[1].image.sprite = sprts[62];
                        //spriteState.pressedSprite = sprts[63];
                        notiString = "Bạn đang ở chế độ chơi thật";
                    }
                    txts[6].text = "";
                    btns[1].spriteState = spriteState;
                    //tweenNum[3] = ShowNoti(notiString);
                    //StartCoroutine(tweenNum[3]);
                    break;
                    #endregion
            }
        }

        private IEnumerator ShowNoti(string t)
        {
            txts[9].text = t;
            txts[9].transform.parent.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            txts[9].transform.parent.gameObject.SetActive(false);
        }



        private IEnumerator ShowValue()
        {

            if (totalFreeSpin > 0)
            {
                txts[39].gameObject.SetActive(true);
                txts[27].text = totalFreeSpin.ToString();
            }
            else
            {
                txts[39].gameObject.SetActive(false);
            }


            if (true)
            {
                //yield return new WaitForSeconds(5f);
                if (boolLs[4] == true && boolLs[5] == false)
                {
                    //App.trace("AHIHI");
                    DoAction("bigWin");
                }

                if (boolLs[5] == true)
                {
                    DoAction("potBreak");
                }

                if (isWin && (int)rsSpinData[0] > 0)
                {
                    if (boolLs[3] == true)
                    {
                        //if (tweenNum[4] != null)
                        //    StopCoroutine(tweenNum[4]);
                        //tweenNum[4] = TweenNum(txts[1], (int)CPlayer.preChipBalance, (int)CPlayer.chipBalance, 3f, 1f, 2);
                        //txts[38].text = string.Format("{0:0,0}", CPlayer.chipBalance);
                        //StartCoroutine(tweenNum[4]);

                    }

                    if (boolLs[5] == false && boolLs[4] == false)
                    {
                        StartCoroutine(showBalanceRs(0, (int)rsSpinData[0]));
                    }
                }
                if (!isWin && rsLineIdsList.Count > 0)
                {
                    boolLs[0] = false;
                    boolLs[1] = true;
                    if (boolLs[2] == false)
                    {
                        if (currentXGoldRush <= 0)
                        {
                            for (int j = 0; j < buttonHideSpin.Length; j++)
                            {
                                buttonHideSpin[j].interactable = true;
                            }

                            if (boolLs[3] == true)
                                btns[0].interactable = true;
                        }
                    }
                }

                if (rsLineIdsList.Count == 0)
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.LOOSE);
                    boolLs[1] = true;
                    boolLs[0] = false;
                    if (boolLs[2] == false)
                    {
                        for (int k = 0; k < buttonHideSpin.Length; k++)
                        {
                            buttonHideSpin[k].interactable = true;
                        }

                        if (boolLs[3] == true)
                            btns[0].interactable = true;
                    }
                    if (boolLs[2] == true)
                    {
                        yield return new WaitForSeconds(.5f);
                        //StopAllCoroutines();
                        //DOTween.KillAll();
                        StopTweenForNewSpin();
                        //DoAction("spin");
                        NewSpin();
                    }

                }
                else
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.WIN_1);
                    for (int k = 0; k < rsLineIdsList.Count; k++)
                    {
                        DrawLine(rsLineIdsList[k] + 1, true);
                        //DrawLineAfterSpin(rsLineIdsList[j] + 1, true, j == rsLineIdsList.Count - 1);
                    }
                    if (totalFreeSpin > 0)
                    {
                        boolLs[0] = false;
                        boolLs[1] = true;
                        if (boolLs[2] == false)
                        {
                            if (currentXGoldRush <= 0)
                            {
                                for (int j = 0; j < buttonHideSpin.Length; j++)
                                {
                                    buttonHideSpin[j].interactable = true;
                                }

                                if (boolLs[3] == true)
                                    btns[0].interactable = true;
                            }
                        }
                    }
                    if (boolLs[3] == false && boolLs[2] == true)
                    {
                        if (currentXGoldRush <= 0)
                        {
                            for (int j = 0; j < buttonHideSpin.Length; j++)
                            {
                                buttonHideSpin[j].interactable = true;
                            }

                        }
                    }
                    /*
                    tweenNum[1] = _DrawMultiLine(rsLineIdsList);
                    StartCoroutine(tweenNum[1]);
                    //boolLs[1] = true;
                    //yield return new WaitForSeconds(2f);
                    //boolLs[0] = false;
                    */

                    //int needGuide = PlayerPrefs.GetInt("needGuide", 0);
                    //if (needGuide != 2)
                    //{
                    tweenNum[1] = _DrawMultiLine(rsLineIdsList);
                    StartCoroutine(tweenNum[1]);
                    yield return new WaitForSeconds(2f);
                    //boolLs[1] = true;
                    boolLs[0] = false;
                    //}
                    //else
                    //{
                    //    yield return new WaitForSeconds(4f);
                    //    boolLs[1] = true;
                    //    boolLs[0] = false;
                    //    PlayerPrefs.SetInt("needGuide", 1);
                    //    DoAction("changeRealPlay");
                    //}

                }
                //gojs[31].SetActive(false);
            }
        }

        public void LoadSmt(string type)
        {
            switch (type)
            {
                case "his":
                    OutBounMessage req_HIS = new OutBounMessage("MATCH.HISTORY");
                    req_HIS.addHead();
                    req_HIS.writeString(Gamecode);         //game name
                    numList[9] = numList[9] + 1;
                    req_HIS.writeByte(numList[9]);                       //page index
                    req_HIS.writeString("");                    //from date
                    req_HIS.writeString("");                    //to date
                    App.ws.send(req_HIS.getReq(), delegate (InBoundMessage res_HIS)
                    {
                        int count = res_HIS.readByte();
                        App.trace("Count = " + count, "red");
                        for (int i = 0; i < count; i++)
                        {
                            long index = res_HIS.readLong();
                            string time = res_HIS.readString();
                            string game = res_HIS.readString();
                            string bet = res_HIS.readString();
                            string change = res_HIS.readString();
                            string balance = res_HIS.readString();
                            GameObject goj = Instantiate(gojs[26], gojs[26].transform.parent, false);
                            Text[] txtArr = goj.GetComponentsInChildren<Text>();
                            txtArr[0].text = index.ToString();
                            txtArr[1].text = time;
                            txtArr[2].text = App.FormatMoney(bet);
                            txtArr[3].text = App.FormatMoney(change);
                            txtArr[4].text = App.FormatMoney(balance);
                            goj.SetActive(true);
                        }
                    });
                    break;
                default:
                    break;
            }
        }

        private IEnumerator HideSmt(GameObject obj, float timeToWait)
        {
            yield return new WaitForSeconds(timeToWait);
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        private IEnumerator TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
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

        private GameObject winnerTextClone = null;
        private IEnumerator showBalanceRs(int fromNum, int toNum)
        {
            float i = 0.0f;
            float rate = 1.0f / .5f;
            Text txt = Instantiate(txts[7], txts[7].transform.parent, false);
            txt.gameObject.SetActive(true);
            txts[7].gameObject.transform.parent.GetChild(0).gameObject.SetActive(true);
            txt.transform.DOScale(2, 1f);
            while (i < 2)
            {
                i += Time.deltaTime * rate;
                float a = Mathf.Lerp(fromNum, toNum, i);
                txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                yield return null;
            }

            yield return new WaitForSeconds(.05f);
            txt.transform.parent = txts[6].transform.parent;
            //txt.transform.DOScale(1f, .5f);
            //txt.transform.DOLocalMove(txts[6].transform.localPosition, .5f).OnComplete(() => {
            txts[6].text = string.Format("{0:0,0}", toNum);
            //txts[7].gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
            winnerTextClone = txt.gameObject;
            //Destroy(txt.gameObject);
            boolLs[0] = false;
            boolLs[1] = true;
            if (boolLs[2] == false)
            {
                if (currentXGoldRush <= 0)
                {
                    for (int j = 0; j < buttonHideSpin.Length; j++)
                    {
                        buttonHideSpin[j].interactable = true;
                    }

                    if (boolLs[3] == true)
                        btns[0].interactable = true;
                }
            }
            //});
        }

        public void DrawLine(int lineId)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            DrawLine(lineId, true);
        }

        private void DrawLineAfterSpin(int lineId, bool needToDel = false, bool isLast = false)
        {
            DrawLine(lineId, needToDel, 2f, isLast);
        }

        private void DrawLine(int lineId, bool needToDel = false, float timeToDel = 2f, bool isLast = false)
        {
            lineId -= 1;


            /* foreach (Transform rtf in rtfs[17].parent)       //Delete exits element before
             {
                 if (rtf.gameObject.name != rtfs[17].gameObject.name)
                 {
                     Destroy(rtf.gameObject);
                 }
             }*/


            int[] ids = wonLineIdList[lineId];
            List<List<GameObject>> needToDelGojList = new List<List<GameObject>>();    //save gojs whic need to del
            for (int k = -1; k < ids.Length - 1; k++)
            {


                int i = 0;
                int j = k > -1 ? ids[k + 1] : (17 + lineId);
                if (k == -1)
                {
                    i = lineId < 10 ? ids[0] : ids[ids.Length - 1];
                }
                else
                    i = ids[k];
                if (lineId < 10)
                {
                    int tmp = i;
                    i = j;
                    j = tmp;
                }

                RectTransform rtf = Instantiate(rtfs[17], rtfs[17].parent, false);
                rtf.GetComponent<Image>().color = lineColors[lineId];
                rtf.SetAsFirstSibling();

                Vector3 vec1 = rtfs[1 + i].anchoredPosition;
                Vector3 vec2 = rtfs[1 + j].anchoredPosition;
                /*rtf.anchoredPosition = new Vector2(rtfs[i + 1].anchoredPosition.x + 24, rtfs[i + 1].anchoredPosition.y + (i % 3 == j % 3 ? 6 : 16));
                rtf.localScale = new Vector2(Vector3.Distance(vec1, vec2) / 32, .5f);
                rtf.right = vec2 - vec1;
                if (rtf.rotation.z < 0)
                    rtf.anchoredPosition -= new Vector2(12, 8);

                if(k == - 1)
                    if((rtf.rotation.z > -10 && rtf.rotation.z < 0) || rtf.rotation.z < -130)
                    rtf.anchoredPosition += new Vector2(12, 8);
                */

                int mY = 8;

                if (k > -1 && lineId < 10 && i % 3 != j % 3)
                {
                    mY += 16;
                }

                rtf.anchoredPosition = new Vector2(rtfs[i + 1].anchoredPosition.x + 16, rtfs[i + 1].anchoredPosition.y + mY);
                rtf.localScale = new Vector2(Vector3.Distance(vec1, vec2) / 32, .5f);
                rtf.right = vec2 - vec1;



                rtf.gameObject.SetActive(true);
                GameObject goj = Instantiate(rtfs[1 + i].gameObject, rtfs[1 + i].parent, false);
                goj.SetActive(true);
                GameObject goj1 = Instantiate(rtfs[1 + j].gameObject, rtfs[1 + j].parent, false);
                goj1.SetActive(true);

                needToDelGojList.Add(new List<GameObject>() { rtf.gameObject, goj, goj1 });
                drawnLineTmp.Add(new List<GameObject>() { rtf.gameObject, goj, goj1 });
            }

            if (needToDel)
                StartCoroutine(_DrawLine(timeToDel, needToDelGojList, isLast));
        }

        private IEnumerator _DrawLine(float timeToDelay, List<List<GameObject>> needToDelGojList, bool isLast = false)
        {
            //App.trace("Vao Day","yellow");
            yield return new WaitForSeconds(timeToDelay);
            // Debug.Log("aaaaaaaaaaaaaaa");
            for (int i = 0; i < needToDelGojList.Count; i++)
            {
                if (needToDelGojList[i] != null)
                {
                    List<GameObject> arr = needToDelGojList[i];
                    Destroy(arr[0]);
                    Destroy(arr[1]);
                    Destroy(arr[2]);
                }
            }
            needToDelGojList.Clear();
            if (isLast && boolLs[2] == true)
            {
                //DoAction("spin");
                NewSpin();
            }
        }

        private IEnumerator _DrawMultiLine(List<int> lineToDrawLs)
        {
            yield return new WaitForSeconds(2f);

            if (boolLs[2] == false)
                for (int i = 0; i < rsPiecesControll.Length; i++)
                {
                    //rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().color = colors[2];

                    rsPiecesControll[i].transform.GetChild(0).GetComponent<Image>().color = colors[2];
                }
            if (currentXGoldRush > 0)
            {
                if (boolLs[2] == true)
                {
                    boolLs[2] = false;
                    currentAutoSpin = true;
                }
                gojs[34].SetActive(true);
                boolLs[1] = true;
                boolLs[0] = false;
                //AllowSpin();
            }


            if (boolLs[4] == true || boolLs[5] == true)
            {
                boolLs[1] = true;
                boolLs[0] = false;
            }


            if (boolLs[0] == false)
                for (int i = 0; i < lineToDrawLs.Count; i++)
                {
                    if (boolLs[2] == false)
                    {
                        //DrawLine(lineToDrawLs[i] + 1, true);
                        int lineToDr = lineToDrawLs[i];
                        SoundManager.instance.PlayEffectSound(SoundFX.WIN_LINE_ONCE + "_" + UnityEngine.Random.Range(1, 6));
                        HighLightLine(lineToDr, wonLineData[lineToDr][0], wonLineData[lineToDr][1]);
                        DrawLineAfterSpin(rsLineIdsList[i] + 1, true, i == lineToDrawLs.Count - 1);
                        yield return new WaitForSeconds(2);
                    }
                }


            if (boolLs[2] == true)
            {
                if (boolLs[4] == true)
                    yield return new WaitForSeconds(3f);
                //yield return new WaitForSeconds(lineToDrawLs.Count * 2);
                yield return new WaitForSeconds(.2f);
                //StopAllCoroutines();
                //DOTween.KillAll();          
                StopTweenForNewSpin();
                //DoAction("spin");
                NewSpin();
            }
        }

        /// <summary>
        /// When a line is drew, a pieces will be highlighted if the line cross
        /// </summary>
        private void HighLightLine(int lineId, int targetPieceId, int targetPieceNum)
        {



            if (boolLs[0])
                return;
            for (int i = 0; i < rsPiecesControll.Length; i++)
            {

                rsPiecesControll[i].transform.GetChild(0).GetComponent<Image>().color = colors[2];

                //  rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "unactive", true);

                //   rsPiecesControll[i].GetComponentInChildren<Image>().color = colors[2];

            }
            int[] pieceIds = wonLineIdList[lineId];

            for (int i = 0; i < pieceIds.Length; i++)
            {
                //App.trace(pieceIds[i] + "|KEKE " + rsPieceIdsList[pieceIds[i] - 1]);

                if (rsPieceIdsList[pieceIds[i] - 1] == targetPieceId)
                {


                    //  rsPiecesControll[pieceIds[i] - 1].transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 255, 255);

                    rsPiecesControll[pieceIds[i] - 1].transform.GetChild(0).GetComponent<Image>().color = colors[0];
                    //rsPiecesControll[pieceIds[i] - 1].transform.GetChild(0).GetComponent<Image>().color = colors[0];
                    //rsPiecesControll[pieceIds[i] - 1].GetComponentInChildren<SkeletonGraphic>().color = colors[0];


                }
            }

        }

        public void Back()
        {
            if (boolLs[0] == true)
            {
                //StartCoroutine(ShowNoti("Vui lòng chờ quay xong."));
                return;
            }
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            //CPlayer.changed -= BalanceChanged;
            CPlayer.potchanged -= PotChanged;
            StopAllCoroutines();
            DOTween.Kill("noti");
            DOTween.Kill("texttween");
            DOTween.Kill("firstSpin");
            DOTween.Kill("secondSpin");
            LoadingControl.instance.loadingGojList[30].SetActive(true);
            StartCoroutine(_Back());
        }

        private IEnumerator _Back()
        {
            SoundManager.instance.PlayUISound(SoundFX.SL7_CLICK);
            yield return new WaitForSeconds(.1f);
            try
            {
                DOTween.KillAll();
                // LoadingControl.instance.asbs[0].Unload(true);
            }
            catch (Exception ex)
            {
                // Debug.Log("<color=red>Faile error:</color> " + ex.ToString());
                Debug.Log(ex);
            }
            SceneManager.LoadScene("Lobby");
        }

        public void OpenLineSelect(bool isShow)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            if (!isShow)
            {
                rtfs[1].DOLocalMoveY(100, .1f).OnComplete(() =>
                {
                    rtfs[1].gameObject.SetActive(false);
                });
                return;
            }
            if (tweenNum[3] != null)
                StopCoroutine(tweenNum[3]);
            if (boolLs[0])
            {
                //tweenNum[3] = ShowNoti("Không thể thay đổi dòng cược khi đang quay.");
                //StartCoroutine(tweenNum[3]);
                return;
            }
            if (boolLs[2])
            {
                //tweenNum[3] = ShowNoti("Không thể thay đổi dòng cược khi đang quay tự động.");
                //StartCoroutine(tweenNum[3]);
                return;
            }
            /*
            if (gojs[28].activeSelf)
            {
                tweenNum[3] = ShowNoti("Không thể thay đổi dòng cược khi còn lượt quay miễn phí.");
                StartCoroutine(tweenNum[3]);
                return;
            }
            */
            if (totalFreeSpin > 0)
            {
                return;
            }
            if (boolLs[3] == false)
            {
                if (tweenNum[3] != null)
                    StopCoroutine(tweenNum[3]);
                //tweenNum[3] = ShowNoti("Không thể thay đổi dòng cược khi đang chơi thử.");
                //StartCoroutine(tweenNum[3]);
                return;
            }
            rtfs[1].gameObject.SetActive(true);
            rtfs[1].DOLocalMoveY(0, .1f);

            foreach (Transform rtf in gojs[0].transform.parent)
            {
                if (rtf.gameObject.name != gojs[0].name)
                {
                    Destroy(rtf.gameObject);
                }
            }
            Debug.Log("CheckXXX");
            for (int i = 0; i < 20; i++)
            {
                int tmp = i;
                GameObject goj = Instantiate(gojs[0], gojs[0].transform.parent, false);
                Image img = goj.GetComponent<Image>();
                //img.sprite = lineSelectList[tmp].IsSlect ? sprts[7 + tmp] : sprts[27 + i];
                img.sprite = lineSelectList[tmp].IsSlect ? itemChoseBetSpriteList[tmp] : itemChoseBetSpriteList[20 + i];
                Button btn = goj.GetComponent<Button>();
                lineSelectList[tmp].BtnRect = img;
                btn.onClick.AddListener(() =>
                {
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    bool isSelected = !lineSelectList[tmp].IsSlect;
                    lineSelectList[tmp].BtnRect.sprite = isSelected ? itemChoseBetSpriteList[tmp] : itemChoseBetSpriteList[20 + tmp];
                    //lineSelectList[tmp].BtnCirlce.sprite = isSelected ? choseLineActive.Sprite : choseLineInactived.Sprite;
                    lineSelectList[tmp].IsSlect = isSelected;

                    numList[1] = numList[1] + (isSelected ? 1 : -1);
                    txts[4].text = numList[1].ToString();
                    numList[0] = numList[1] * numList[4];
                    txts[5].text = App.formatMoney(numList[0].ToString());

                });
                ; goj.SetActive(true);
            }
        }

        public void ChangeBet(int bet)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            currentBet = numList[5 + bet];
            //App.trace("Bet Value " + currentBet, "red");
            if (betData.ContainsKey(currentBet))
                txts[2].text = App.FormatMoney(betData[currentBet]);
            txts[3].text = App.FormatMoney(currentBet);
            txts[5].text = App.FormatMoney(currentBet * numList[1]);
            getInfoDetail();
            txts[6].text = "";
            numList[4] = currentBet;
            boolLs[3] = true;
            btns[0].interactable = true;
        }
        public void ChangeBetTrial()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            currentBet = numList[8];
            if (betData.ContainsKey(currentBet))
                txts[2].text = App.FormatMoney(betData[currentBet]);
            txts[3].text = App.FormatMoney(currentBet);
            txts[5].text = App.FormatMoney(currentBet * numList[1]);
            getInfoDetail();
            txts[6].text = "";
            numList[4] = currentBet;
            boolLs[3] = false;
            btns[0].interactable = false;
        }
        public void PotChanged()
        {

            InBoundMessage res = CPlayer.res_pot;
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                string gameId = res.readString();
                if (gameId == Gamecode)
                {
                    int count0 = res.readByte();
                    for (int k = 0; k < count0; k++)
                    {
                        int bet = res.readInt();
                        int value = res.readInt();
                        if (!betData.ContainsKey(bet))
                            betData.Add(bet, value);
                        else
                            betData[bet] = value;
                        if (currentBet == bet)
                        {
                            if (tweenNum[0] != null)
                                StopCoroutine(tweenNum[0]);
                            tweenNum[0] = TweenNum(txts[2], (int)App.formatMoneyBack(txts[2].text), value, 1f, 1f);
                            StartCoroutine(tweenNum[0]);
                        }
                    }
                    break;
                }
                else
                {
                    int count1 = res.readByte();
                    for (int j = 0; j < count1; j++)
                    {
                        int bet = res.readInt();
                        int value = res.readInt();
                    }
                }
            }

        }

        public void EnoughMoney(string gamecode, int gameState)
        {
            Debug.Log(1 + "Chest : " + gamecode + " === " + Gamecode);
            if (gamecode == Gamecode)
            {
                enoughMoney = true;
                buttonHideSpin[0].interactable = true;
            }
        }

        public void OpenChat()
        {
            LoadingControl.instance.loadingGojList[18].SetActive(true);
        }

        public void OpenGlory(bool isOpen)
        {
            if (isOpen)
            {
                gojs[35].SetActive(true);
                GetDataGlory(false);
            }
            else
            {
                SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                gojs[35].SetActive(false);
            }
        }
        public GameObject[] statesButton;
        public Image buttonActiveSpine;
        public void GetDataGlory(bool isBigWin)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            for (int i = gojs[36].transform.parent.childCount - 1; i > 0; i--)
            {
                Destroy(gojs[36].transform.parent.GetChild(i).gameObject);
            }
            if (isBigWin)
            {
                // btnGlory[0].image.color = new Color32(9, 0, 255, 255);
                //  btnGlory[1].image.color = new Color32(255, 255, 255, 255);
                statesButton[0].SetActive(false);
                statesButton[1].SetActive(true);
            }
            else
            {

                statesButton[0].SetActive(true);
                statesButton[1].SetActive(false);

                //  btnGlory[0].image.color = new Color32(255, 255, 255, 255);
                //  btnGlory[1].image.color = new Color32(9, 0, 255, 255);
            }
            OutBounMessage req_Glory = new OutBounMessage("GOBLINS.GLORY_BOARD");
            req_Glory.addHead();
            App.ws.send(req_Glory.getReq(), delegate (InBoundMessage res_Glory)
            {
                int count = res_Glory.readByte();
            //Debug.Log("count " + count);
            for (int i = 0; i < count; i++)
                {
                    var content = res_Glory.readString();
                //Debug.Log("content "+ content);
                var arr = content.Split('|');

                    string time = arr[0];
                    var times = arr[0].Split('-');
                    if (times.Length == 2)
                    {
                        time = times[0] + " " + times[1];
                    }
                    else
                    {
                        time = arr[0];
                    }

                    string user = arr[1];
                    string mucCuoc = arr[2];
                    string thang = arr[3];
                    bool jackpost = arr[4] == "true" ? true : false;
                    bool bigWin = arr[5] == "true" ? true : false;
                    if (isBigWin == true && jackpost == false && bigWin == true)
                    {
                        GameObject gloryClone = Instantiate(gojs[36], gojs[36].transform.parent, false);
                        Text[] txtArr = gloryClone.GetComponentsInChildren<Text>();
                        txtArr[0].text = user;
                        txtArr[1].text = time;
                        txtArr[2].text = mucCuoc;
                        txtArr[3].text = thang;
                        gloryClone.SetActive(true);
                    }
                    else if (isBigWin == false && jackpost == true)
                    {
                        GameObject gloryClone = Instantiate(gojs[36], gojs[36].transform.parent, false);
                        Text[] txtArr = gloryClone.GetComponentsInChildren<Text>();
                        txtArr[0].text = user;
                        txtArr[1].text = time;
                        txtArr[2].text = mucCuoc;
                        txtArr[3].text = thang;
                        gloryClone.SetActive(true);
                    }
                }
            });
        }
        public void AllowSpin()
        {
            if (boolLs[2] == false)
            {
                for (int j = 0; j < buttonHideSpin.Length; j++)
                {
                    buttonHideSpin[j].interactable = true;
                }

                if (boolLs[3] == true)
                    btns[0].interactable = true;
            }
        }

        private List<IEnumerator> spinThreads = new List<IEnumerator>();
        private void StopTweenForNewSpin()
        {
            for (int i = 0; i < tweenNum.Length; i++)
            {
                if (tweenNum[i] != null)
                    StopCoroutine(tweenNum[i]);
            }

            for (int i = 0; i < spinThreads.Count; i++)
            {
                if (spinThreads[i] != null)
                    StopCoroutine(spinThreads[i]);
            }
        }

        public void NewSpin()
        {
            if (numList[1] <= 0)
                return;
            SoundManager.instance.PlayUISound(SoundFX.SPIN);
            buttonActiveSpine.enabled = false;
            for (int i = 0; i < buttonHideSpin.Length; i++)
            {
                buttonHideSpin[i].interactable = false;
            }

            if (boolLs[2] == false && boolLs[3] == true)
                btns[0].interactable = false;
            if (winnerTextClone != null)
                Destroy(winnerTextClone);
            if (tweenNum[1] != null)
                StopCoroutine(tweenNum[1]);
            rtfs[43].gameObject.SetActive(false);
            rtfs[42].gameObject.SetActive(false);
            txts[7].gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
            for (int i = 0; i < rsPiecesControll.Length; i++)
            {

                rsPiecesControll[i].GetComponentInChildren<Image>().color = colors[0];
                // rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().color = colors[0];

                // rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "unactive", true);
            }
            /*
                 * 
                 * */
            //Clear drawn line which can't be dele when start a new spin
            for (int i = 0; i < drawnLineTmp.Count; i++)
            {
                if (drawnLineTmp[i] != null)
                {
                    List<GameObject> arr = drawnLineTmp[i];
                    Destroy(arr[0]);
                    Destroy(arr[1]);
                    Destroy(arr[2]);
                }
            }
            spin = true;
            DoAction("spin");
        }
        private bool stopSpin1;
        private bool stopSpin2;
        private bool stopSpin3;
        private bool stopSpin4;
        private bool stopSpin5;

        private IEnumerator _StopSpin()
        {
            yield return new WaitForSeconds(.2f);
            stopSpin1 = true;
            yield return new WaitForSeconds(.4f);
            stopSpin2 = true;
            yield return new WaitForSeconds(.3f);
            stopSpin3 = true;
            yield return new WaitForSeconds(.3f);
            stopSpin4 = true;
            yield return new WaitForSeconds(.3f);
            stopSpin5 = true;
            yield return new WaitForSeconds(.5f);
            spin = false;
            speedRow1 = 2500;
            speedRow2 = 2800;
            speedRow3 = 3100;
            speedRow4 = 3400;
            speedRow5 = 3700;
            stopSpin1 = false;
            stopSpin2 = false;
            stopSpin3 = false;
            stopSpin4 = false;
            stopSpin5 = false;
            StartCoroutine(ShowValue());
        }
        private bool spin = false;
        public float speedRow1 = 2500;
        public float speedRow2 = 2800;
        public float speedRow3 = 3100;
        public float speedRow4 = 3400;
        public float speedRow5 = 3700;
        private void StopColumn(RectTransform[] rtf, int id)
        {

            if (rtf[0].anchoredPosition.y < -135)
                rtf[0].anchoredPosition = Vector3.MoveTowards(rtf[0].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[0].anchoredPosition = Vector3.MoveTowards(rtf[0].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -130), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[1].anchoredPosition.y < -5)
                rtf[1].anchoredPosition = Vector3.MoveTowards(rtf[1].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[1].anchoredPosition = Vector3.MoveTowards(rtf[1].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 0), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[2].anchoredPosition.y < 125)
                rtf[2].anchoredPosition = Vector3.MoveTowards(rtf[2].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[2].anchoredPosition = Vector3.MoveTowards(rtf[2].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 130), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[3].anchoredPosition.y < 255)
                rtf[3].anchoredPosition = Vector3.MoveTowards(rtf[3].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[3].anchoredPosition = Vector3.MoveTowards(rtf[3].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 260), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[4].anchoredPosition.y < 385)
                rtf[4].anchoredPosition = Vector3.MoveTowards(rtf[4].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[4].anchoredPosition = Vector3.MoveTowards(rtf[4].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 390), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[5].anchoredPosition.y < 515)
                rtf[5].anchoredPosition = Vector3.MoveTowards(rtf[5].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[5].anchoredPosition = Vector3.MoveTowards(rtf[5].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 520), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[6].anchoredPosition.y < 645)
                rtf[6].anchoredPosition = Vector3.MoveTowards(rtf[6].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[6].anchoredPosition = Vector3.MoveTowards(rtf[6].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 650), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[7].anchoredPosition.y < 775)
                rtf[7].anchoredPosition = Vector3.MoveTowards(rtf[7].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, sizeif), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[7].anchoredPosition = Vector3.MoveTowards(rtf[7].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 780), Time.deltaTime * (2500 + (id * 300)));
        }
        private float sizeif = -340f;
        private void Update()
        {
            txts[1].text = MoneyManager.instance.FakeCoin;

            if (stopSpin1)
            {
                speedRow1 = 0;
                StopColumn(rftPiecesRow1, 0);
            }
            if (stopSpin2)
            {
                speedRow2 = 0;
                StopColumn(rftPiecesRow2, 1);
            }
            if (stopSpin3)
            {
                speedRow3 = 0;
                StopColumn(rftPiecesRow3, 2);
            }
            if (stopSpin4)
            {
                speedRow4 = 0;
                StopColumn(rftPiecesRow4, 3);
            }
            if (stopSpin5)
            {
                speedRow5 = 0;
                StopColumn(rftPiecesRow5, 4);
            }
            if (spin)
            {
                for (int j = 0; j < 8; j++)
                {
                    rftPiecesRow1[j].transform.Translate(Vector2.down * speedRow1 * Time.deltaTime);
                    rftPiecesRow2[j].transform.Translate(Vector2.down * speedRow2 * Time.deltaTime);
                    rftPiecesRow3[j].transform.Translate(Vector2.down * speedRow3 * Time.deltaTime);
                    rftPiecesRow4[j].transform.Translate(Vector2.down * speedRow4 * Time.deltaTime);
                    rftPiecesRow5[j].transform.Translate(Vector2.down * speedRow5 * Time.deltaTime);
                }

            }
            float size = 130;

            for (int i = 0; i < 8; i++)
            {
                if (rftPiecesRow1[i].anchoredPosition.y <= sizeif)
                {
                    if (i == 0)
                        rftPiecesRow1[i].anchoredPosition = new Vector2(rftPiecesRow1[i].anchoredPosition.x, rftPiecesRow1[7].anchoredPosition.y + size);
                    else
                        rftPiecesRow1[i].anchoredPosition = new Vector2(rftPiecesRow1[i].anchoredPosition.x, rftPiecesRow1[i - 1].anchoredPosition.y + size);
                }
                if (rftPiecesRow2[i].anchoredPosition.y <= sizeif)
                {
                    if (i == 0)
                        rftPiecesRow2[i].anchoredPosition = new Vector2(rftPiecesRow2[i].anchoredPosition.x, rftPiecesRow2[7].anchoredPosition.y + size);
                    else
                        rftPiecesRow2[i].anchoredPosition = new Vector2(rftPiecesRow2[i].anchoredPosition.x, rftPiecesRow2[i - 1].anchoredPosition.y + size);
                }
                if (rftPiecesRow3[i].anchoredPosition.y <= sizeif)
                {
                    if (i == 0)
                        rftPiecesRow3[i].anchoredPosition = new Vector2(rftPiecesRow3[i].anchoredPosition.x, rftPiecesRow3[7].anchoredPosition.y + size);
                    else
                        rftPiecesRow3[i].anchoredPosition = new Vector2(rftPiecesRow3[i].anchoredPosition.x, rftPiecesRow3[i - 1].anchoredPosition.y + size);
                }
                if (rftPiecesRow4[i].anchoredPosition.y <= sizeif)
                {
                    if (i == 0)
                        rftPiecesRow4[i].anchoredPosition = new Vector2(rftPiecesRow4[i].anchoredPosition.x, rftPiecesRow4[7].anchoredPosition.y + size);
                    else
                        rftPiecesRow4[i].anchoredPosition = new Vector2(rftPiecesRow4[i].anchoredPosition.x, rftPiecesRow4[i - 1].anchoredPosition.y + size);
                }
                if (rftPiecesRow5[i].anchoredPosition.y <= sizeif)
                {
                    if (i == 0)
                        rftPiecesRow5[i].anchoredPosition = new Vector2(rftPiecesRow5[i].anchoredPosition.x, rftPiecesRow5[7].anchoredPosition.y + size);
                    else
                        rftPiecesRow5[i].anchoredPosition = new Vector2(rftPiecesRow5[i].anchoredPosition.x, rftPiecesRow5[i - 1].anchoredPosition.y + size);
                }
            }
        }
    }
}