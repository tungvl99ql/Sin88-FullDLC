using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class TLMNControler : MonoBehaviour {

    //public bool hasDiscardPlayerAction = false;
    private UnityEvent discardPlayerActionCallback = new UnityEvent();
    private bool _isAutoDiscard = true;
    private bool _isAnimationFinish = true;
    private bool IsPlayActionFinished {
        get {
            return _isAnimationFinish;
        }
    }

    /// <summary>
    /// 0-3: HandCards|4: backCards|5: trans|6: firedCard00|7: firedCard10|8: firedCard20
    /// |9-12: userInfo|13-16: owner|17-20: timeLeap|21: help|22-25: showCards|26: noti
    /// </summary>
    public GameObject[] tlmnObjList;
    /// <summary>
    /// 0-3: avar|4: BackBtn|5-8: HandCard|9: firedCard00|10: cardFx|11: backBtn
    /// </summary>
    public Image[] tlmnImgList;
    //public Sprite[] cardFaces;
    public Sprite[] cardFaces;
    /// <summary>
    /// 0: TableName|1-4: balanceTxt|5-8: userName|9: Countdown|10-12: cardCount|
    /// 13-16: Earn Text|17: AttText|18-21: playerAtt|22: Noti
    /// </summary>
    public Text[] tlmnTxtList;
    /// <summary>
    /// 0: Bỏ chọn|1: Đánh|2: bỏ lượt
    /// </summary>
    public Button[] tlmnBtnList;
    public Sprite addPlayerIcon;
    /// <summary>
    /// 0: Trắng|1: Vàng
    /// </summary>
    public Font[] tlmnFontList;

    public Sprite[] tableBackground;

    private List<PlayerCard> playerCardList = new List<PlayerCard>();
    private List<int> myCardIdList = new List<int>();
    private Dictionary<int, Player> playerList = new Dictionary<int, Player>();
    private int mySlotId = -1, currOwnerId = -1;
    private bool isPlaying = false;
    private bool[] exitsSlotList = { false, false, false, false };
    private IEnumerator preCoroutine = null;
    private List<GameObject> installedCardList = new List<GameObject>();

    [HideInInspector]
    public static TLMNControler instance;

    [HideInInspector]
    public bool regToQuit = false;

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        getInstance();
    }
    private void getInstance()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }

    }

    [Header("Vua bai dep")]
    private bool isBeautiful = false; //co dc bai dep hay ko

    // Use this for initialization
    void Start() {
        //StartCoroutine(divideCards(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, false)); //Chia bài cho mình
        //return;
        CPlayer.baiDep = "TLMN.BAIDEP";
        StartCoroutine(LoadingControl.instance._start());


        //set background cua ban choi theo tung muc cuoc
        int bgIndex = UnityEngine.Random.Range(0, tableBackground.Length - 1);

        //if (CPlayer.betAmtId > 3 && CPlayer.betAmtId < 8)
        //{
        //    bgIndex = 1;
        //}
        //else if (CPlayer.betAmtId >= 8)
        //{
        //    bgIndex = 2;
        //}

        tlmnImgList[12].sprite = tableBackground[bgIndex];



        if (CPlayer.betAmtOfTableToGo.Contains('-'))
        {
            int amtId = (-1) * int.Parse(CPlayer.betAmtOfTableToGo);




            var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
            req_getBetAmtList.addHead();

            App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res)
            {

                int count = res.readByte();
                for (int i = 0; i < count; i++)
                {
                    int a = res.readInt();

                    if (i == amtId)
                    {
                        CPlayer.betAmtOfTableToGo = App.formatMoney(a.ToString());
                        string tempString = App.listKeyText["TLMN_NAME"].ToUpper();
                        tlmnTxtList[0].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " " + App.listKeyText["CURRENCY"];//"TLMN - " + CPlayer.betAmtOfTableToGo + " Gold";
                        break;
                    }


                }
            });
        }
        else
        {
            string tempString = App.listKeyText["TLMN_NAME"].ToUpper();
            tlmnTxtList[0].text = tempString + " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"]; //"TLMN - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
        }

        registerHandler();
        getTableDataEx();
    }

    #region //CHIA BÀI

    private void ForceFinishPlayerAction(UnityEvent onFinish = null) {
        // Not has any card is dragging.
        if (!isDragging)
        {
            if (onFinish != null)
            {
                //onFinish.Invoke();
                //Debug.LogError("GOI ĐÂY 1 ======= ");
                StartCoroutine(ASysInvoke(onFinish));


            }
        }
        else
        {
            //Debug.LogError("GOI ĐÂY 2 ======= ");
            for (int i = 0; i < playerCardList.Count; i++)
            {
                playerCardList[i].Rtf.GetComponent<TLMNCardCtrl>().EndProcess();
            }
        }

    }

    IEnumerator ASysInvoke(UnityEvent a) {

        //Debug.LogError("***** IsPlayActionFinished ****** " + IsPlayActionFinished);

        while (!IsPlayActionFinished)
        {
            //Debug.LogError("***** IsPlayActionFinished ****** " + IsPlayActionFinished);
            yield return null;

        }
        //Debug.LogError("888888888 IsPlayActionFinished 888888888 " + IsPlayActionFinished);
        //Debug.LogError("99999999999999  IsPlayActionFinished 999999999999999  " + IsPlayActionFinished);
        a.Invoke();
    }


    private IEnumerator divideCards(List<int> ids, bool isReconnect = true)
    {

        tlmnObjList[5].SetActive(true);
        if (playerCardList.Count > 0)
        {
            for (int i = 0; i < playerCardList.Count; i++)
            {
                DestroyImmediate(playerCardList[i].Rtf.gameObject);
            }
        }
        myCardIdList.Clear();
        playerCardList.Clear();

        if (isReconnect)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                Image img = Instantiate(tlmnImgList[5], tlmnImgList[5].transform.parent, false);
                img.overrideSprite = cardFaces[ids[i]];
                RectTransform rtf = img.GetComponent<RectTransform>();
                Vector2 vec = new Vector2(90, 0);
                Vector2 mPos = vec * (i);
                rtf.anchoredPosition = mPos;
                playerCardList.Add(new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
                myCardIdList.Add(int.Parse(cardFaces[ids[i]].name));
                img.gameObject.SetActive(true);
                installedCardList.Add(img.gameObject);
            }
            yield return new WaitForSeconds(.5f);
            tlmnObjList[5].SetActive(false);
        }
        else
        {
            Vector2 startPost = new Vector2(700, 550);
            for (int i = 0; i < ids.Count; i++)
            {
                Image img = Instantiate(tlmnImgList[5], tlmnImgList[5].transform.parent.parent, false);
                img.overrideSprite = cardFaces[52];
                RectTransform rtf = img.GetComponent<RectTransform>();
                rtf.anchoredPosition = startPost;
                img.gameObject.SetActive(true);
                Vector2 vec = new Vector2(90, 0);
                Vector2 mPos = vec * (i);
                int tmp = i;
                playerCardList.Add(new PlayerCard(rtf, img, img.transform.GetChild(0).gameObject));
                myCardIdList.Add(int.Parse(cardFaces[ids[tmp]].name));
                rtf.SetParent(tlmnImgList[5].transform.parent);
                rtf.SetAsLastSibling();
                installedCardList.Add(img.gameObject);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos,  .05f).OnComplete(() => { //time 0.5 + tmp * 0.05f

                    rtf.DORotate(new Vector3(0, 90, 0), .125f + tmp * .01f).OnComplete(() =>
                    {
                        try
                        {
                            img.overrideSprite = cardFaces[ids[tmp]];

                        }
                        catch
                        {
                            //App.trace("FACK DIU = " + ids[tmp]);
                        }
                        rtf.DORotate(new Vector3(0, 0, 0), .125f).OnComplete(() => {
                            if (tmp == ids.Count - 1)
                            {
                                tlmnObjList[5].SetActive(false);
                            }
                        });

                    });
                });
                yield return new WaitForSeconds(.125f);
            }
        }
    }
    private IEnumerator divideCards2(int slotId, int cardCount, bool isReconnect = true)
    {
        if(isReconnect == false)
        {
            Vector2 mPivot = tlmnObjList[slotId].GetComponent<RectTransform>().pivot;
            Vector3 endPos = tlmnObjList[slotId].transform.position;
            tlmnTxtList[9 + slotId].text = cardCount.ToString();
            //App.trace("==============> !isReconnected ON HAND CARD = " + cardCount.ToString(), "red");
            for (int i = 0; i < 9; i++)
            {
                GameObject img = Instantiate(tlmnObjList[4], tlmnObjList[4].transform.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                img.SetActive(true);
                DOTween.To(() => rtf.pivot, x => rtf.pivot = x, mPivot, .25f);
                img.transform.DOMove(endPos, .25f, false).OnComplete(() => {
                    Destroy(img);
                });
                yield return new WaitForSeconds(.125f);
            }
            tlmnObjList[slotId].SetActive(true);    //Hiện bài úp của thằng khác
            //App.trace("==============> !isReconnected ON HAND CARD = " + cardCount.ToString(), "red");
            //tlmnTxtList[9 + slotId].text = cardCount.ToString();
            tlmnTxtList[9 + slotId].transform.parent.gameObject.SetActive(true);
        }
        else
        {
            //App.trace("==============> isReconnected ON HAND CARD = " + cardCount.ToString(), "red");
            tlmnObjList[slotId].SetActive(true);    //Hiện bài úp của thằng khác
            tlmnTxtList[9 + slotId].text = cardCount.ToString();
            tlmnTxtList[9 + slotId].transform.parent.gameObject.SetActive(true);
        }
    }
    #endregion

    public class PlayerCard
    {
        private RectTransform rtf;
        private Image img;
        private GameObject trans;
        public PlayerCard(RectTransform rtf, Image img, GameObject trans)
        {
            this.Rtf = rtf;
            this.Img = img;
            this.Trans = trans;
        }

        public RectTransform Rtf
        {
            get
            {
                return rtf;
            }

            set
            {
                rtf = value;
            }
        }

        public Image Img
        {
            get
            {
                return img;
            }

            set
            {
                img = value;
            }
        }

        public GameObject Trans
        {
            get
            {
                return trans;
            }

            set
            {
                trans = value;
            }
        }
    }

    private List<RectTransform> firedCard01List = new List<RectTransform>();
    private List<RectTransform> firedCard02List = new List<RectTransform>();
    private void moveCard(List<int> ids, int slotId, string act = "")   //SlotId là slotId trong bàn chơi
    {
        if(act == "show")
        {
            Vector2 idstance = new Vector2(55, 0);
            if (slotId == 1 || slotId == 3)
                idstance = new Vector2(0, 55);
            for (int i = 0; i < ids.Count; i++)
            {
                int tmp = i;
                Image img = Instantiate(tlmnImgList[5 + slotId], tlmnObjList[22 + slotId].transform, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                img.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                //rtf.anchoredPosition = Vector2.zero;
                img.transform.position = tlmnImgList[5 + slotId].transform.position;
                img.overrideSprite = cardFaces[ids[tmp]];

                img.gameObject.SetActive(true);

                Vector2 mPos = idstance * tmp;
                mPos -= idstance * ids.Count / 2;

                installedCardList.Add(img.gameObject);

                rtf.localScale = Vector2.one * .75f;
                if (slotId == 1 || slotId == 3)
                    rtf.SetAsFirstSibling();
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f);

            }
            return;
        }

        {
            #region CHUYỂN FIREDCARD01 SANG FIREDCARD02
            //tlmnObjList[5].SetActive(true);
            for (int i = 0; i < firedCard01List.Count; i++)
            {
                firedCard01List[i].parent = tlmnObjList[8].transform.parent;
                int tmp = i;
                RectTransform rtf = firedCard01List[tmp];
                Vector2 end = new Vector2(rtf.anchoredPosition.x, rtf.anchoredPosition.y + 90);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, end,.25f).OnComplete(()=> {
                    rtf.parent = tlmnObjList[8].transform.parent;
                    firedCard02List.Add(rtf);
                    if (tmp == firedCard01List.Count - 1)
                    {
                        firedCard01List.Clear();
                    }
                });
            }

            #endregion

            Vector2 mPivotStart = tlmnObjList[slotId].GetComponent<RectTransform>().pivot;
            Vector2 mPivotEnd = Vector2.zero;
            Vector2 firedPos = tlmnObjList[6].GetComponent<RectTransform>().anchoredPosition;
            Vector2 idstance = new Vector2(65, 0);
            for (int i = 0; i < ids.Count; i++)
            {
                int tmp = i;
                Image img = Instantiate(tlmnImgList[9], tlmnImgList[5 + slotId].transform.parent.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                rtf.localScale = Vector2.one * 1.3333f;
                rtf.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(-10, 10)));
                rtf.pivot = mPivotStart;
                img.transform.localPosition = tlmnImgList[5 + slotId].transform.localPosition;

                img.overrideSprite = cardFaces[ids[tmp]];
                img.gameObject.SetActive(true);
                //firedCardList.Add(img.gameObject);
                Vector2 mPos = firedPos + idstance * tmp;
                mPos -= idstance * ids.Count / 2;
                rtf.SetParent(tlmnObjList[6].transform.parent);
                installedCardList.Add(img.gameObject);
                DOTween.To(() => rtf.pivot, x => rtf.pivot = x, mPivotEnd, .5f);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f).OnComplete(()=> {
                    if(tmp == 0)
                    {
                        tlmnImgList[10].transform.localPosition = new Vector2(mPos.x - 150, tlmnImgList[10].transform.localPosition.y);
                        tlmnImgList[10].gameObject.SetActive(true);
                        SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
                        tlmnImgList[10].DOFade(0, .5f).SetEase(Ease.InQuint).OnComplete(() => {
                            tlmnImgList[10].gameObject.SetActive(false);
                            tlmnImgList[10].color = Color.white;
                        });
                    }
                    rtf.DOScale(1f, .05f).OnComplete(()=> {
                        rtf.parent = tlmnObjList[7].transform.parent;
                        firedCard01List.Add(rtf);
                        if (tmp ==ids.Count - 1)
                        {
                            //tlmnObjList[5].SetActive(false);
                        }
                    });
                });
            }
        }
    }

    private void myCard(List<int> ids, string act)
    {
        IsAutoDiscard = true;
        if(act == "show")
        {
            Vector2 idstance = new Vector2(65, 0);
            int _i = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                _i = i;
                for (int j = 0; j < myCardIdList.Count; j++)
                {
                    int tmp = j;
                    if (myCardIdList[j] - 1 == ids[i])
                    {
                        RectTransform rtf = playerCardList[tmp].Rtf;
                        Vector2 mPos = idstance * (i+ 1);
                        mPos -= idstance * ids.Count / 2;
                        rtf.SetParent(tlmnObjList[6].transform.parent);

                        playerCardList.RemoveAt(tmp);
                        myCardIdList.RemoveAt(tmp);
                        rtf.parent = tlmnObjList[22].transform;
                        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f).OnComplete(() => {
                            rtf.DOScale(.75f, .05f);
                            Destroy(rtf.gameObject.GetComponent<TLMNCardCtrl>());
                            //Debug.LogError(" i = " + _i);
                            //Debug.LogError(" ids.Count - 1 = " + (ids.Count -1));
                            //Debug.LogError(" tmp = " + tmp);
                            //Debug.LogError(" myCardIdList.Count - 1= " + (myCardIdList.Count - 1));
                            if (_i == ids.Count- 1 && tmp == myCardIdList.Count - 1)
                            {
                                //Debug.LogError("CASSSSSSDADADADSDDSSD aaaa");
                                discardPlayerActionCallback.RemoveAllListeners();
                            }
                        });
                        break;
                    }
                }
            }
            return;
        }

        if(act == "remove")
        {
            tlmnObjList[5].SetActive(true);
            #region CHUYỂN FIREDCARD01 SANG FIREDCARD02

            for (int i = 0; i < firedCard01List.Count; i++)
            {
                firedCard01List[i].parent = tlmnObjList[8].transform.parent;
                int tmp = i;
                RectTransform rtf = firedCard01List[tmp];
                Vector2 end = new Vector2(rtf.anchoredPosition.x, rtf.anchoredPosition.y + 90);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, end, .25f).OnComplete(() => {
                    rtf.parent = tlmnObjList[8].transform.parent;
                    firedCard02List.Add(rtf);
                    if (tmp == firedCard01List.Count - 1)
                    {
                        firedCard01List.Clear();
                        //discardPlayerActionCallback.RemoveAllListeners();
                    }
                });
            }

            #endregion

            Vector2 firedPos = tlmnObjList[6].GetComponent<RectTransform>().anchoredPosition;
            Vector2 idstance = new Vector2(65, 0);
            bool isEnd = false;
            for (int i = 0; i < ids.Count; i++)
            {
                int tempI = i;
                if (i == ids.Count - 1)
                    isEnd = true;
                for (int j = 0; j < myCardIdList.Count; j++)
                {

                    isEnd = true;
                    int tmp = j;
                    if (myCardIdList[j] - 1 == ids[i])
                    {
                        RectTransform rtf = playerCardList[tmp].Rtf;
                        Vector2 mPos = firedPos + idstance * i;
                        mPos -= idstance * ids.Count / 2;
                        rtf.SetParent(tlmnObjList[6].transform.parent);

                        playerCardList.RemoveAt(tmp);
                        myCardIdList.RemoveAt(tmp);

                        rtf.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-10, 10)), .5f);
                        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f).OnComplete(() => {
                            if (tempI == 0)
                            {
                                tlmnImgList[10].transform.localPosition = new Vector2(mPos.x - 150, tlmnImgList[10].transform.localPosition.y);
                                tlmnImgList[10].gameObject.SetActive(true);
                                SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
                                tlmnImgList[10].DOFade(0, .5f).SetEase(Ease.InQuint).OnComplete(() => {
                                    tlmnImgList[10].gameObject.SetActive(false);
                                    tlmnImgList[10].color = Color.white;
                                });
                            }
                            rtf.DOScale(.75f, .05f).OnComplete(() => {
                                rtf.parent = tlmnObjList[7].transform.parent;
                                firedCard01List.Add(rtf);
                            });
                            Destroy(rtf.gameObject.GetComponent<TLMNCardCtrl>());
                            if (isEnd)
                            {
                                if (playerCardList.Count > 0)
                                {
                                    for (int k = 0; k < playerCardList.Count; k++)
                                    {
                                        int temp = k;
                                        DOTween.To(() => playerCardList[temp].Rtf.anchoredPosition, x => playerCardList[temp].Rtf.anchoredPosition = x, new Vector2(90 * temp, 0), .25f).OnComplete(()=> {
                                            if (tlmnObjList[5].activeSelf)
                                            {
                                                tlmnObjList[5].SetActive(false);
                                            }
                                        });
                                    }
                                }
                                else
                                {
                                    tlmnObjList[5].SetActive(false);
                                }
                            }
                            discardPlayerActionCallback.RemoveAllListeners();
                        });
                        break;
                    }
                }
            }
        }
    }

    #region //LIEN QUAN CARDCTRL
    public bool isDragging = false;
    private List<CardPrepare> cardPrepareList = new List<CardPrepare>();
    private class CardPrepare
    {
        private int id, slib;
        private RectTransform rtf;
        private Transform parrentRtf;
        private Vector2 pos;
        private GameObject border;
        private string name;
        private bool isClicked;
        public CardPrepare(int id, RectTransform rtf, Transform parrentRtf, int slib, Vector2 pos, GameObject border, string name, bool isClicked)
        {
            this.Slib = slib;
            this.Id = id;
            this.Rtf = rtf;
            this.ParrentRtf = parrentRtf;
            this.Pos = pos;
            this.Border = border;
            this.Name = name;
            this.IsClicked = isClicked;
        }

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }
        public bool IsClicked
        {
            get
            {
                return isClicked;
            }

            set
            {
                isClicked = value;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public GameObject Border
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

        public Vector2 Pos
        {
            get
            {
                return pos;
            }

            set
            {
                pos = value;
            }
        }
        public Transform ParrentRtf
        {
            get
            {
                return parrentRtf;
            }

            set
            {
                parrentRtf = value;
            }
        }

        public RectTransform Rtf
        {
            get
            {
                return rtf;
            }

            set
            {
                rtf = value;
            }
        }

        public int Slib
        {
            get
            {
                return slib;
            }

            set
            {
                slib = value;
            }
        }
    }
    public void removeCardPrepare()
    {
        if (cardPrepareList.Count > 1)
        {
            cardPrepareList.RemoveAt(1);
        }
    }
    public void addCardPrepare(int id, RectTransform rtf, Transform parrentRtf, int slib, Vector2 pos, GameObject bor, string name, bool isClicked)
    {
        //tlmnObjList[27].SetActive(true);
        if (id == 0)
        {
            cardPrepareList.Clear();
        }
        CardPrepare cP = new CardPrepare(id, rtf, parrentRtf, slib, pos, bor, name, isClicked);
        cardPrepareList.Add(cP);
        //App.trace("CARD " + id + " ADDED WITH x = " + cP.Pos.x + "|y = " + cP.Pos.y + "|sibId = " + slib);
    }

    Tween tween1 = null;
    Tween tween2 = null;

    public void swapPrepareCard()
    {
        _isAnimationFinish = false;
        if (cardPrepareList.Count < 2)
        {
            //Debug.Log("CASSSSSSSSSSSSSSSSEEE 1");

            //App.trace("MUHA " + cardPrepareList[0].Pos.x + "|" + cardPrepareList[0].Pos.y);
            cardPrepareList[0].Rtf.parent = cardPrepareList[0].ParrentRtf;
            cardPrepareList[0].Rtf.anchoredPosition = cardPrepareList[0].Pos;
            cardPrepareList[0].Rtf.SetSiblingIndex(cardPrepareList[0].Slib);
            cardPrepareList.Clear();

            _isAnimationFinish = true;
            if (discardPlayerActionCallback != null)
            {
                //Debug.Log("CALL IN CALLBACK");
                discardPlayerActionCallback.Invoke();
            }
            //tlmnObjList[27].SetActive(true);
            return;
        }

        //Debug.Log("CASSSSSSSSSSSSSSSSEEE 2");
        int a = int.Parse(cardPrepareList[0].Name);
        int b = int.Parse(cardPrepareList[1].Name);
        int idTmpA = myCardIdList.IndexOf(a);
        int idTmpB = myCardIdList.IndexOf(b);
        myCardIdList[idTmpA] = b;
        myCardIdList[idTmpB] = a;

        PlayerCard mPlayerCard = playerCardList[idTmpA];
        playerCardList[idTmpA] = playerCardList[idTmpB];
        playerCardList[idTmpB] = mPlayerCard;

        tlmnObjList[5].SetActive(true);
        cardPrepareList[0].Rtf.SetParent(cardPrepareList[1].ParrentRtf);
        cardPrepareList[0].Rtf.SetSiblingIndex(cardPrepareList[1].Slib);

        if (tween1 != null)
        {
            tween1.Kill(true);
        }

        tween1 = DOTween.To(() => cardPrepareList[0].Rtf.anchoredPosition, x => cardPrepareList[0].Rtf.anchoredPosition = x, cardPrepareList[1].Pos, .25f).OnComplete(() => {
            cardPrepareList[0].Border.SetActive(false);
            if (cardPrepareList[0].IsClicked)
                cardPrepareList[0].Rtf.anchoredPosition = new Vector2(cardPrepareList[0].Rtf.anchoredPosition.x, 65);
            else
            {
                cardPrepareList[0].Rtf.anchoredPosition = new Vector2(cardPrepareList[0].Rtf.anchoredPosition.x, 0);
            }
        });

        cardPrepareList[1].Rtf.SetAsLastSibling();
        if (tween2 != null)
        {
            tween2.Kill(true);
        }
        tween2 = DOTween.To(() => cardPrepareList[1].Rtf.anchoredPosition, x => cardPrepareList[1].Rtf.anchoredPosition = x, cardPrepareList[0].Pos, .25f).OnComplete(() => {
            cardPrepareList[1].Rtf.SetSiblingIndex(cardPrepareList[0].Slib);
            cardPrepareList[1].Border.SetActive(false);
            if (cardPrepareList[1].IsClicked)
                cardPrepareList[1].Rtf.anchoredPosition = new Vector2(cardPrepareList[1].Rtf.anchoredPosition.x, 65);
            else
            {
                cardPrepareList[1].Rtf.anchoredPosition = new Vector2(cardPrepareList[1].Rtf.anchoredPosition.x, 0);
            }
            cardPrepareList.Clear();
            tlmnObjList[5].SetActive(false);
            _isAnimationFinish = true;

            if (discardPlayerActionCallback != null)
            {
                //Debug.Log("CALLBACK CALL 2");
                discardPlayerActionCallback.Invoke();
            }

        });

        //App.trace("ĐỔI " + cardPrepareList[0].Pos.x + "|" + cardPrepareList[0].Pos.y);
        //App.trace("VỚI " + cardPrepareList[1].Pos.x + "|" + cardPrepareList[1].Pos.y);


    }
    #endregion

    public void showHelpPanel(bool isShow)
    {
        if (isShow)
        {
            tlmnObjList[21].SetActive(true);
            return;
        }
        tlmnObjList[21].SetActive(false);
    }

    private List<string> handelerCommand = new List<string>();  //Lưu các handler đã đăng ký
    private void registerHandler()
    {
        #region //SLOT_IN_TABLE_CHANGED
        var req_SLOT_IN_TABLE_CHANGED = new OutBounMessage("SLOT_IN_TABLE_CHANGED");
        req_SLOT_IN_TABLE_CHANGED.addHead();
        handelerCommand.Add("SLOT_IN_TABLE_CHANGED");
        App.ws.sendHandler(req_SLOT_IN_TABLE_CHANGED.getReq(), delegate (InBoundMessage res)
        {
            var nickName = res.readAscii();
            var slotId = res.readByte();
            var chipBalance = res.readLong();
            var score = res.readLong();
            var level = res.readByte();
            var avatarId = res.readShort();
            var avatar = res.readAscii();
            var isMale = res.readByte() == 1;
            var isOwner = res.readByte() == 1;
            var playerId = res.readLong();
            var starBalance = res.readLong();
            //App.trace("RECV [SLOT_IN_TABLE_CHANGED] slot = " + slotId + "nick = " + nickName);

            if (nickName == CPlayer.nickName)
            {
                mySlotId = slotId;
                //return;
            }
            if (isOwner)
            {
                currOwnerId = slotId;
            }

            Player player = new Player(detecSlotIdBySvrId(slotId), slotId, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

            slotId = detecSlotIdBySvrId(slotId);

            if (nickName.Length == 0)    //Có thằng thoát khỏi bàn chơi
            {
                // comment out
                SoundManager.instance.PlayUISound(SoundFX.CARD_EXIT_TABLE);
                playerList.Remove(slotId);
                tlmnObjList[9 + slotId].SetActive(false);
                tlmnImgList[slotId].sprite = addPlayerIcon;
                tlmnImgList[slotId].overrideSprite = addPlayerIcon;
                tlmnImgList[slotId].material = null;
                exitsSlotList[slotId] = false;
                //App.trace("==============> Có thằng thoát khỏi bàn chơi ON HAND CARD = 13 fix cung", "red");
                tlmnTxtList[9 + slotId].transform.parent.gameObject.SetActive(false);
                tlmnTxtList[9 + slotId].text = "13";
                if (slotId != 0)
                {
                    tlmnObjList[13 + slotId].SetActive(false);   //Xóa owner của thằng thoát
                    tlmnObjList[slotId].SetActive(false);   //Ẩn backCard
                }
                if (playerList.Count < 2)   //Khi bàn chỉ còn 1 thằng thì k đếm ngược nữa
                {
                    if (preCoroutine != null)
                    {
                        tlmnTxtList[9].gameObject.SetActive(false);
                        StopCoroutine(preCoroutine);

                    }

                }
                return;
            }
            // comment out
            SoundManager.instance.PlayUISound(SoundFX.CARD_JOIN_TABLE);
            if (playerList.ContainsKey(slotId)) //Có thằng thay thế
            {
                playerList[slotId] = player;
                exitsSlotList[slotId] = true;
                setInfo(player, tlmnImgList[player.SlotId], tlmnObjList[9 + player.SlotId], tlmnTxtList[1 + player.SlotId], tlmnTxtList[5 + player.SlotId], tlmnObjList[13 + player.SlotId]);
                return;
            }
            //Thêm bình thường
            playerList.Add(slotId, player);
            exitsSlotList[slotId] = true;
            setInfo(player, tlmnImgList[player.SlotId], tlmnObjList[9 + player.SlotId], tlmnTxtList[1 + player.SlotId], tlmnTxtList[5 + player.SlotId], tlmnObjList[13 + player.SlotId]);

        });
        #endregion

        #region [SET_TURN]
        var req_SET_TURN = new OutBounMessage("SET_TURN");
        req_SET_TURN.addHead();
        handelerCommand.Add("SET_TURN");
        App.ws.sendHandler(req_SET_TURN.getReq(), delegate (InBoundMessage res_SET_TURN)
        {
            int slotId = res_SET_TURN.readByte();
            int turnTimeOut = res_SET_TURN.readShort(); //Thời gian chờ
            int playerRemainDuration = res_SET_TURN.readShort();    //Thời gian chờ còn lại

            //App.trace("RECV [SET_TURN]");
            //App.trace("slotId = " + slotId + "| turnTimeOut = " + turnTimeOut + "|playerRemainDuration  = " + playerRemainDuration);
            if (slotId == -2)    //Khi bắt đầu ván mới
            {
                if (turnTimeOut > 0)
                {
                    preCoroutine = _showCountDOwn(turnTimeOut);
                    StartCoroutine(preCoroutine);
                }
                else
                {
                    StopCoroutine(preCoroutine);
                }

                return;
            }


            if (turnTimeOut > 0)
            {
                tlmnObjList[17 + preTimeLeapId].SetActive(false);
                slotId = getSlotIdBySvrId(slotId);
                preTimeLeapId = slotId;
                time = turnTimeOut;
                curr = playerRemainDuration;
                tlmnObjList[17 + preTimeLeapId].SetActive(true);
                preTimeLeapImage = tlmnObjList[17 + preTimeLeapId].GetComponent<Image>();
                preTimeLeapImage.fillAmount = 1;
                run = true;
            }

        });
        #endregion

        #region [START_MATCH]
        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        handelerCommand.Add("START_MATCH");
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res_START_MATCH) {
            // comment out
            SoundManager.instance.PlayUISound(SoundFX.CARD_START);

            //App.trace("============= START_MATCH================","blue");

            isPlaying = true;
            for (int i = 0; i < installedCardList.Count; i++)    //Xóa các quân bài ván trước
            {
                try
                {
                    Destroy(installedCardList[i]);
                }
                catch
                {

                }
                if (i == installedCardList.Count - 1)
                    installedCardList.Clear();
            }

            for(int i = 0; i < 4; i++)  //Ẩn kết quả ván trước
            {
                tlmnTxtList[18 + i].text = "";
                tlmnTxtList[18 + i].gameObject.SetActive(false);
                tlmnTxtList[13 + i].gameObject.SetActive(false);
            }

            firedCard01List.Clear();
            firedCard02List.Clear();

            isPlaying = true;
            loadPlayerMatchPoint(res_START_MATCH);
            loadBoardData(res_START_MATCH, false);
        });
        #endregion

        #region [MOVE]
        var req_MOVE = new OutBounMessage("MOVE");
        req_MOVE.addHead();
        handelerCommand.Add("MOVE");
        App.ws.sendHandler(req_MOVE.getReq(), delegate (InBoundMessage res) {
            List<int> ids = new List<int>();
            ids = res.readBytes();
            //CardUtils.svrIdsToIds(ids);
            // comment out
            SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
            int sourceSlotId = res.readByte();
            int sourceLineId = res.readByte() - 1;
            int targetSlotId = res.readByte();
            int targetLineId = res.readByte() - 1;
            int targetIndex = res.readByte();

            //App.trace(string.Format("RECV [MOVE] IdsCount = {0} | sourceSlotId = {1} | sourceLineId = {2}| targetSlotId = {3}| targetLineId = {4} | targetIndex = {5}| CON DATA =\n",
            //ids.Count, sourceSlotId, sourceLineId, targetSlotId, targetLineId, targetIndex));

            if (mySlotId == sourceSlotId)    //Mình đánh
            {
                //Debug.LogError("1111111 ====================>");
                //Debug.LogError("IsAutoDiscard = " + IsAutoDiscard);
                //Debug.LogError("cardPrepareList.Count = " + cardPrepareList.Count);
                //Debug.LogError("isDragging = " + isDragging);
                //Debug.LogError("_isAnimationFinish = " + _isAnimationFinish);
                //Debug.LogError("<==================== 222222222");

                if (IsAutoDiscard)
                {
                    // TODO:
                    //1 Finish inprogressing action
                    //2 Discard
                    discardPlayerActionCallback.RemoveAllListeners();
                    discardPlayerActionCallback.AddListener(
                        () =>
                        {
                            myCard(ids, "remove");
                        }
                    );
                    ForceFinishPlayerAction(discardPlayerActionCallback);

                }
                else
                {
                    myCard(ids, "remove");
                }
                return;
            }
            //Thằng khác đánh
            int slotId = getSlotIdBySvrId(sourceSlotId);
            moveCard(ids, slotId);
            tlmnTxtList[9 + slotId].text = (int.Parse(tlmnTxtList[9 + slotId].text) - ids.Count).ToString();
            //App.trace("====++==========> Danh bai = " + tlmnTxtList[9 + slotId].text, "blue");
        });
        #endregion

        #region [ENTER_STATE]
        var req_ENTER_STATE = new OutBounMessage("ENTER_STATE");
        req_ENTER_STATE.addHead();
        handelerCommand.Add("ENTER_STATE");
        App.ws.sendHandler(req_ENTER_STATE.getReq(), delegate (InBoundMessage res_ENTER_STATE)
        {
            App.trace("RECV [ENTER_STATE]");
            int stateId = res_ENTER_STATE.readByte();
            enterState(stateById[stateId]);
            enterState2(stateById[stateId]);
        });
        #endregion

        #region [CLEAR_CARD]
        var req_CLEAR_CARDS = new OutBounMessage("CLEAR_CARDS");
        req_CLEAR_CARDS.addHead();
        handelerCommand.Add("CLEAR_CARDS");
        App.ws.sendHandler(req_CLEAR_CARDS.getReq(), delegate (InBoundMessage res) {
            App.trace("RECV [CLEAR_CARDS]");
            try
            {
                for (int i = 0; i < firedCard02List.Count; i++)
                {
                    Destroy(firedCard02List[i].gameObject);
                    if (i == firedCard02List.Count - 1)
                        firedCard02List.Clear();
                }
                for (int i = 0; i < firedCard01List.Count; i++)
                {
                    Destroy(firedCard01List[i].gameObject);
                    if (i == firedCard01List.Count - 1)
                        firedCard01List.Clear();
                }
            }
            catch
            {

            }
        });
        #endregion

        #region [GAMEOVER]
        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        handelerCommand.Add("GAMEOVER");
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res)
        {

            //App.trace("RECV [GAMEOVER]");
            run = false;    //K chạy timeLeap nữa
            preTimeLeapImage.gameObject.SetActive(false);   //Ẩn time leap

            List<int> winLs = new List<int>(), loseLs = new List<int>();
            //gui bai dep
            // if(isBeautiful) {

            //     StartCoroutine(_showTheLe());

            // }

            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int slotId = res.readByte();
                int grade = res.readByte();
                long earnValue = res.readLong();
                //App.trace("slotId = " + slotId + "|grade =" + grade + "|earvalue = " + earnValue);

                //string title = "HÒA";
                //switch (grade)
                //{
                //    case 1:
                //        title = "NHẤT";
                //        break;
                //    case 4:
                //        title = "BÉT";
                //        break;

                //}

                int fontId = 0;

                if (earnValue > 0)
                {
                    fontId = 1;
                    winLs.Add(slotId);
                }
                else if (earnValue < 0)
                {
                    loseLs.Add(slotId);
                }
                slotId = getSlotIdBySvrId(slotId);
                //App.trace("slotId = " + slotId + "|grade =" + grade + "|earvalue = " + earnValue, "green");
                string title = GetResultString(grade);
                if (slotId == 0)
                {
                    // comment out
                    if (earnValue > 0)
                    {
                        SoundManager.instance.PlayEffectSound(SoundFX.CARD_PHOM_WIN);
                        //SoundManager.instance.PlayUISound(SoundFX.CARD_PHOM_WIN);
                    }
                    else
                    {
                       SoundManager.instance.PlayEffectSound(SoundFX.CARD_LOSE);
                    }
                    if (tlmnTxtList[18].text == "")
                    {
                        tlmnTxtList[18].text = title;
                    }

                }
                else
                {
                    //App.trace("************** ========== Game over car on hand = 13 fiixixixi", "yellow");
                    tlmnTxtList[9 + slotId].transform.parent.gameObject.SetActive(false);
                    tlmnTxtList[9 + slotId].text = "13";

                    if (tlmnTxtList[18 + slotId].text == "")
                    {
                        tlmnTxtList[18 + slotId].text = title;
                    }

                }

                tlmnTxtList[18 + slotId].font = tlmnFontList[fontId];
                tlmnTxtList[18 + slotId].gameObject.SetActive(true);
                tlmnTxtList[13 + slotId].font = tlmnFontList[fontId];
                if(earnValue > -1)
                {
                    tlmnTxtList[13 + slotId].text = App.formatMoney(earnValue.ToString());
                    tlmnTxtList[13 + slotId].text = "+" + tlmnTxtList[13 + slotId].text;
                }
                else
                {
                    tlmnTxtList[13 + slotId].text = App.formatMoney(Math.Abs(earnValue).ToString());
                    tlmnTxtList[13 + slotId].text = "-" + tlmnTxtList[13 + slotId].text;
                }
                tlmnTxtList[13 + slotId].gameObject.SetActive(true);
            }
            StartCoroutine(_flyCoins(winLs, loseLs));
            StartCoroutine(_ClearCardWhenGameOver());


        });
        #endregion

        #region [SHOW_PLAYER_CARD]
        var req_SHOW_PLAYER_CARD = new OutBounMessage("SHOW_PLAYER_CARD");
        req_SHOW_PLAYER_CARD.addHead();
        handelerCommand.Add("SHOW_PLAYER_CARD");
        App.ws.sendHandler(req_SHOW_PLAYER_CARD.getReq(), delegate (InBoundMessage res_SHOW_PLAYER_CARD)
        {
            int slotId = res_SHOW_PLAYER_CARD.readByte();
            List<int> ids = res_SHOW_PLAYER_CARD.readBytes();

            if(slotId == mySlotId)  //Show bài của mình
            {
                //Debug.LogError("cardPrepareList.Count = " + cardPrepareList.Count);
                //Debug.LogError("isDragging = " + isDragging);
                discardPlayerActionCallback.RemoveAllListeners();
                if (cardPrepareList.Count > 0 && isDragging)
                {
                    discardPlayerActionCallback.AddListener(
                        () =>
                        {
                            myCard(ids, "show");
                        }
                    );
                    ForceFinishPlayerAction(discardPlayerActionCallback);
                }
                else
                {
                    myCard(ids, "show");
                }
                return;
            }

            slotId = getSlotIdBySvrId(slotId);
            moveCard(ids, slotId, "show");
            tlmnObjList[slotId].SetActive(false);
        });
        #endregion

        #region [SET_PLAYER_ATT]
        var req_SET_PLAYER_ATTR = new OutBounMessage("SET_PLAYER_ATTR");
        req_SET_PLAYER_ATTR.addHead();
        handelerCommand.Add("SET_PLAYER_ATTR");
        App.ws.sendHandler(req_SET_PLAYER_ATTR.getReq(), delegate (InBoundMessage res)
        {
            //App.trace("RECV [SET_PLAYER_ATTR]");
            int slotId = res.readByte();
            string icon = res.readAscii();
            string content = res.readAscii();
            int action = res.readByte();
            //App.trace("????? icon = " + icon + "|action = " + action + "|content" + content);
            if (icon == "pass_turn"|| icon == "bet" || icon == "")
                return;

            //la bai dep game tien len

            // if(icon.Equals("white_win") && slotId == mySlotId) {
            //     isBeautiful = true;
            //     VuaBaiDepController.instance.canSent = true;
            //     VuaBaiDepController.instance.PlayCanSentAnim();

            // }

            slotId = getSlotIdBySvrId(slotId);
            if(action == 2 || action == 0) //Show value
            {
                if (icon == "freeze")
                {
                    tlmnTxtList[18 + slotId].text = getAttByIcon(icon);
                    tlmnTxtList[18 + slotId].gameObject.SetActive(true);
                }
                else
                {
                    // comment out
                    SoundManager.instance.PlayUISound(SoundFX.CARD_START_MATCH);
                    tlmnTxtList[17].text = getAttByIcon(icon);
                    tlmnTxtList[17].gameObject.SetActive(true);
                    StartCoroutine(hideAtt());
                }
                return;
            }
            if(action == -1)
            {
                return;
            }
        });
        #endregion
    }

    private string GetResultString(int grade)
    {

        string title = App.listKeyText["GAME_DRAW"];//"HÒA";
        switch (grade)
        {
            case 1:
                title = App.listKeyText["BOARD_FIRST"];//"NHẤT";
                break;
            case 4:
                title = App.listKeyText["BOARD_LAST"];//"BÉT";
                break;

        }

        return title;
    }

    // IEnumerator _showTheLe() {
    //     yield return new WaitForSeconds(5f);
    //    VuaBaiDepController.instance.canSent = false;
    //     isBeautiful = false;
    //    VuaBaiDepController.instance.PlayCanSentAnim();
    // }

    IEnumerator _ClearCardWhenGameOver()
    {
        if (isPlaying)
            yield return null;
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < installedCardList.Count; i++)    //Xóa các quân bài ván trước
        {
            try
            {
                Destroy(installedCardList[i]);
            }
            catch
            {

            }
            if (i == installedCardList.Count - 1)
                installedCardList.Clear();
        }

        for (int i = 0; i < 4; i++)  //Ẩn kết quả ván trước
        {
            tlmnTxtList[18 + i].text = "";
            tlmnTxtList[18 + i].gameObject.SetActive(false);
            tlmnTxtList[13 + i].gameObject.SetActive(false);
        }

        firedCard01List.Clear();
        firedCard02List.Clear();


    }

    private void delAllHandle()
    {

        foreach (string t in handelerCommand)
        {
            //App.trace(t);
            var req = new OutBounMessage(t);
            req.addHead();
            App.ws.delHandler(req.getReq());
        }
    }

    private void getTableDataEx()
    {
        var req_GET_TABLE_DATA_EX = new OutBounMessage("GET_TABLE_DATA_EX");
        req_GET_TABLE_DATA_EX.addHead();
        req_GET_TABLE_DATA_EX.writeAcii("");
        App.ws.send(req_GET_TABLE_DATA_EX.getReq(), delegate (InBoundMessage res)
        {
            //App.trace("RECV [GET_TABLE_DATA_EX]");

            loadStateData(res);
            loadTableData(res);
            loadPlayerMatchPoint(res);
            loadBoardData(res);

        });
    }

    #region //STATE COMMAND
    private Dictionary<int, State> stateById;
    private Dictionary<string, State> stateByCode;
    private int beginStateId;
    private void loadStateData(InBoundMessage res)
    {
        stateByCode = new Dictionary<string, State>();
        stateById = new Dictionary<int, State>();

        int count = res.readByte();
        App.trace("loadstateData " + count);
        for (int i = 0; i < count; i++)
        {
            //App.trace(res.readByte() + "|" + res.readAscii() + "|" + res.readByte());
            State mstate = new State(res.readByte(), res.readAscii(), res.readByte());

            int commandCount = res.readByte();
            for (int j = 0; j < commandCount; j++)
            {
                //App.trace(res.readByte() + "|" + res.readAscii() + "|" + res.readString() + "|" + res.readByte() + "|" + res.readByte());
                StateCommand command = new StateCommand(res.readByte(), res.readAscii(), res.readString());
                command.waitResult = true;
                command.fillBoardState = res.readByte() == 1;
                command.takeConfirmation = res.readByte() == 1;

                mstate.commands.Add(command);
            }
            stateById.Add(mstate.Id, mstate);
            stateByCode.Add(mstate.Code, mstate);
        }
        beginStateId = res.readByte();

        State stateStart = new State(0, "waitStart", 1);
        StateCommand command_START = new StateCommand(0, "START", "Bắt đầu");
        command_START.action = delegate (int s) {
            App.trace("START NÈ");
        };
        stateStart.commands.Add(command_START);
        stateById.Add(stateStart.Id, stateStart);
        stateByCode.Add(stateStart.Code, stateStart);

        var state_viewTable = new State(1, "viewTable", 1);
        stateById.Add(state_viewTable.Id, state_viewTable);
        stateByCode.Add(state_viewTable.Code, state_viewTable);

        foreach (State tmpState in stateById.Values)
        {
            tmpState.commandsByPosition = new Dictionary<int, List<StateCommand>>();
            for (int i = 0; i < tmpState.commands.Count; i++)
            {
                StateCommand command = tmpState.commands[i];
                List<StateCommand> commands = null;
                if (tmpState.commandsByPosition.ContainsKey(command.Position))
                {
                    commands = tmpState.commandsByPosition[command.Position];
                }
                if (commands == null)
                {
                    commands = new List<StateCommand>();
                    tmpState.commandsByPosition[command.Position] = commands;
                }
                commands.Add(command);
            }
        }
    }

    public class State
    {
        private int id, mode;
        private string code;
        public List<StateCommand> commands;
        public Dictionary<int, List<StateCommand>> commandsByPosition;

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public int Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
            }
        }

        public string Code
        {
            get
            {
                return code;
            }

            set
            {
                code = value;
            }
        }

        public State(int id, string code, int mode)
        {
            this.Id = id;
            this.Code = code;
            this.Mode = mode;
            commands = new List<StateCommand>();
            commandsByPosition = new Dictionary<int, List<StateCommand>>();
        }
    }

    public class StateCommand
    {
        private int position;
        private string code, name;
        public bool fillBoardState, takeConfirmation, waitResult;
        public Action<int> action;

        public int Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public string Code
        {
            get
            {
                return code;
            }

            set
            {
                code = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public StateCommand(int position, string code, string name)
        {
            this.Position = position;
            this.Code = code;
            this.Name = name;
        }
        public void printStateCommand()
        {
            App.trace("STATE: pos = " + Position + "|code =" + Code + "|name = " + Name);
        }
    }

    private Dictionary<string, Button> stateButtonByCommandCode;
    private Dictionary<int, Button> stateButtonByPosition;
    private State state;
    private void enterState(State state, InBoundMessage res = null)
    {
        if (stateButtonByCommandCode != null)
            stateButtonByCommandCode.Clear();
        else
            stateButtonByCommandCode = new Dictionary<string, Button>();

        if (stateButtonByPosition != null)
            stateButtonByPosition.Clear();
        else
            stateButtonByPosition = new Dictionary<int, Button>();
        this.state = state;

        foreach (int position in state.commandsByPosition.Keys)
        {
            List<StateCommand> commands = state.commandsByPosition[position];
            stateButtonByPosition.Add(position, createStateButton(commands));
        }
        /*
        for(position in state.commandsByPosition.keys())
        {
            var commands = state.commandsByPosition[position];
            stateButtonByPosition.set(position, createStateButton(commands));

        }
         * */



    }
    private Button createStateButton(List<StateCommand> commands)
    {
        /*
        if(commands.Count == 1) //Center btns
        {

        }else
        {   //Bot btns
            string t = "";
            foreach (StateCommand stc in commands)
            {

                t += "[" + stc.Code + "|name = " + stc.Name + "|pos = " + stc.Position + "]|";
            }
            App.trace(t);
        }*/

        Button btn = null;
        return btn;
    }
    private string currentState = "";
    private void enterState2(State state, InBoundMessage res = null)
    {
        if (state == null)
            return;
        currentState = state.Code;

        foreach (Button btn in tlmnBtnList)
        {
            btn.gameObject.SetActive(false);
        }

        App.trace("ENTER STATE = " + state.Code);

        switch (state.Code)
        {
            case "turn":
                // commen out
                SoundManager.instance.PlayUISound(SoundFX.CARD_SET_TURN);
                tlmnBtnList[0].gameObject.SetActive(true);
                tlmnBtnList[1].gameObject.SetActive(true);
                tlmnBtnList[2].gameObject.SetActive(true);
                break;
            case "":

                break;
        }
    }
    #endregion

    private void loadPlayerMatchPoint(InBoundMessage res)
    {
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            int point = res.readInt();
        }
    }

    #region //TABLE DATA
    private void loadTableData(InBoundMessage res)
    {
        playerList = new Dictionary<int, Player>();

        mySlotId = res.readByte();

        isPlaying = res.readByte() == 1;
        App.trace("MY SLOT ID = " + mySlotId + "|isPlaying = " + isPlaying);

        int count = res.readByte();

        for (int i = 0; i < count; i++)
        {

            int slotId = res.readByte();
            long playerId = res.readLong();
            string nickName = res.readAscii();
            int avatarId = res.readShort();
            string avatar = res.readAscii();
            bool isMale = res.readByte() == 1;
            long chipBalance = res.readLong();
            long starBalance = res.readLong();
            long score = res.readLong();
            int level = res.readByte();
            bool isOwner = res.readByte() == 1;
            //App.trace("slotId = " + slotId + "|nickName = " + nickName + "|isOwner = " + isOwner + "|MySlotId = " + mySlotId);


            //App.trace("== Player " + i + " = " + nickName + "|slotId = " + slotId);
            if (isOwner)
            {
                currOwnerId = slotId;
                App.trace("CURR OWNER = " + currOwnerId);
            }

            //slotId = detecSlotIdBySvrId(slotId,isOwner);

            int tmp = slotId;
            if (slotId > -1)
            {
                Player player = new Player(detecSlotIdBySvrId(slotId), tmp, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

                //App.trace("PLAYER SLOT " + player.SlotId + "ADDED!" + player.SvSlotId + "|isOwner = " + isOwner);
                if (!playerList.ContainsKey(player.SlotId))
                {
                    exitsSlotList[player.SlotId] = true;
                    playerList.Add(player.SlotId, player);
                }
                try
                {
                    //App.trace("SET PLAYER INFO " + player.SlotId + "|SVID = " + player.SvSlotId);
                    setInfo(player, tlmnImgList[player.SlotId], tlmnObjList[9 + player.SlotId], tlmnTxtList[1 + player.SlotId], tlmnTxtList[5 + player.SlotId], tlmnObjList[13 + player.SlotId]);
                }
                catch
                {
                    App.trace("sai ID = " + slotId);
                }
            }
        }

        int currentTurnSlotId = res.readByte();
        int currTimeOut = res.readShort();
        //showCountDown(currTimeOut);

        int slotRemainDuration = res.readShort();
        var currentState = res.readByte();

        //App.trace("currentTurnSlotId = " + currentTurnSlotId + "|currTimeOut = " + currTimeOut + "|slotRemainDuration = " + slotRemainDuration + "|");

        enterState(stateById[currentState]);

        if (currTimeOut > 0 && currentTurnSlotId > -1)
        {
            preTimeLeapId = getSlotIdBySvrId(currentTurnSlotId);
            time = currTimeOut;
            curr = slotRemainDuration;
            tlmnObjList[17 + preTimeLeapId].SetActive(true);
            preTimeLeapImage = tlmnObjList[17 + preTimeLeapId].GetComponent<Image>();
            preTimeLeapImage.fillAmount = 1;
            run = true;
        }
        if (mySlotId > -1)
            enterState2(stateById[currentState]);
    }

    /// <summary>
    /// Xác định vị trí theo Player luôn ở vị trí 0
    /// </summary>
    /// <param name="slotId"></param>
    /// <returns></returns>
    private int detecSlotIdBySvrId(int slotId)
    {
        int temp = slotId;
        if (mySlotId >= 0)
        {
            temp = temp - mySlotId;
            temp = temp < 0 ? (temp + 4) : temp;
        }
        return temp;
    }

    private int detecSvrBySlotId(int slotId)
    {
        foreach (Player pl in playerList.Values.ToList())
        {
            if (pl.SlotId == slotId)
                return pl.SvSlotId;
        }
        return 0;
    }

    /// <summary>
    /// Xác định slotId bằng SV Id
    /// </summary>
    /// <param name="slotId"></param>
    /// <returns></returns>
    private int getSlotIdBySvrId(int slotId)
    {
        foreach (Player pl in playerList.Values.ToList())
        {
            if (pl.SvSlotId == slotId)
                return pl.SlotId;
        }
        return 0;
    }

    private long[] chipList = { 0, 0, 0, 0 };
    private void setInfo(Player player, Image im, GameObject infoObj, Text balanceText, Text nickNamText, GameObject ownerImg)
    {
        //im.gameObject.transform.localScale = Vector3.one;
        StartCoroutine(App.loadImg(im, App.getAvatarLink2(player.Avatar, (int)player.PlayerId), player.SvSlotId == mySlotId));
        if (infoObj != null)
            infoObj.SetActive(true);
        chipList[player.SlotId] = player.ChipBalance;
        balanceText.text = "100.0 K";
        balanceText.text = " " + (player.SlotId == 0 ? App.formatMoney(player.ChipBalance.ToString()) : App.formatMoneyAuto(player.ChipBalance));
        nickNamText.text = App.formatNickName(player.NickName, 10);
        ownerImg.SetActive(player.IsOwner);

    }

    public class Player
    {
        string nickName, avatar;
        int slotId, svSlotId, avatarId, level;
        bool isMale, isOwner;
        long playerId, chipBalance, starBalance, score;

        public int SlotId
        {
            get
            {
                return slotId;
            }

            set
            {
                slotId = value;
            }
        }

        public int SvSlotId
        {
            get
            {
                return svSlotId;
            }

            set
            {
                svSlotId = value;
            }
        }

        public string NickName
        {
            get
            {
                return nickName;
            }

            set
            {
                nickName = value;
            }
        }

        public int AvatarId
        {
            get
            {
                return avatarId;
            }

            set
            {
                avatarId = value;
            }
        }

        public string Avatar
        {
            get
            {
                return avatar;
            }

            set
            {
                avatar = value;
            }
        }

        public int Level
        {
            get
            {
                return level;
            }

            set
            {
                level = value;
            }
        }

        public bool IsMale
        {
            get
            {
                return isMale;
            }

            set
            {
                isMale = value;
            }
        }

        public bool IsOwner
        {
            get
            {
                return isOwner;
            }

            set
            {
                isOwner = value;
            }
        }

        public long PlayerId
        {
            get
            {
                return playerId;
            }

            set
            {
                playerId = value;
            }
        }

        public long ChipBalance
        {
            get
            {
                return chipBalance;
            }

            set
            {
                chipBalance = value;
            }
        }

        public long StarBalance
        {
            get
            {
                return starBalance;
            }

            set
            {
                starBalance = value;
            }
        }

        public long Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;
            }
        }

        public Player(int slotId, int svSlotId, long playerId, string nickName, int avatarId, string avatar, bool isMale, long chipBalance, long starBalance, long score, int level, bool isOwner)
        {
            this.SlotId = slotId;
            this.SvSlotId = svSlotId;
            this.PlayerId = playerId;
            this.NickName = nickName;
            this.AvatarId = avatarId;
            this.IsMale = isMale;
            this.ChipBalance = chipBalance;
            this.StarBalance = starBalance;
            this.Score = score;
            this.Level = level;
            this.isOwner = isOwner;
            this.avatar = avatar;
            /*
             * var slotId = res.readByte();
            var playerId = res.readLong();
            var nickName = res.readAscii();
            var avatarId = res.readShort();
            var avatar = res.readAscii();
            var isMale = res.readByte() == 1;
            var chipBalance = res.readLong();
            var starBalance = res.readLong();
            var score = res.readLong();
            var level = res.readByte();
            var isOwner = res.readByte() == 1;
             * */
        }
    }
    #endregion

    private void loadBoardData(InBoundMessage res, bool isReconnect = true)
    {
        if (isPlaying == false)
            return;
        int slotCount = res.readByte();
        //App.trace("slot count = " + slotCount);
        for (int i = 0; i < slotCount; i++)
        {
            int slotId = res.readByte();
            if(slotId > -1)
                slotId = getSlotIdBySvrId(slotId);
            int lineCount = res.readByte();
            //App.trace("slotId = " + slotId + "|BOARD DATA lineCount = " + lineCount);
            for (int j = 0; j < lineCount; j++)
            {
                int cardLineId = res.readByte() - 1;
                int cardCount = res.readByte();

                if (cardCount < 0)   //
                {

                    List<int> ids = new List<int>();
                    ids = res.readBytes();
                    //App.trace("[[[[[[[[[[[mySlotId = " + mySlotId + "slotId = " + slotId + "|length = " + ids.Count + "|cardLineId = " + cardLineId);
                    //CardUtils.svrIdsToIds(ids);
                    //if (slotId == mySlotId)
                    //myCardIdList.Add(cardLineId, ids);
                    if (slotId == 0 && cardLineId == 0) //Bài của mình
                    {
                        StartCoroutine(divideCards(ids, isReconnect));
                        continue;
                    }

                    if (slotId == -1)   //Bài đã nhả trên bàn
                    {
                        Vector2 idstance = new Vector2(65, 0);
                        for (int k = 0; k < ids.Count; k++)
                        {
                            int tmp = k;
                            Image img = Instantiate(tlmnImgList[9], tlmnImgList[9].transform.parent, false);
                            RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                            rtf.Rotate(new Vector3(0, 0, UnityEngine.Random.Range(-10, 10)));
                            Vector2 mPos = idstance * tmp -  idstance * (ids.Count / 2);
                            img.overrideSprite = cardFaces[ids[tmp]];
                            img.gameObject.SetActive(true);
                            rtf.parent = tlmnObjList[7].transform.parent;
                            firedCard01List.Add(rtf);
                        }
                    }
                }
                else
                {
                    //App.trace("((((((((((((((((slotId = " + slotId + "CARD COUNT = " + cardCount);
                    if(slotId > -1)
                    {
                        StartCoroutine(divideCards2(slotId, cardCount, isReconnect));
                    }
                }
            }

        }
    }

    private float time = 1, curr = 0;
    private bool run = false;
    private int preTimeLeapId = -1;
    private Image preTimeLeapImage;
    private void Update()
    {
        curr += Time.deltaTime;
        if (run)
            preTimeLeapImage.fillAmount = (time - curr) / time;
    }

    private IEnumerator _showCountDOwn(int timeOut)
    {
        float mTime = timeOut - 2;

        tlmnTxtList[9].text = App.listKeyText["GAME_PREPARE"];//"CHUẨN BỊ";
        tlmnTxtList[9].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        tlmnTxtList[9].gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            tlmnTxtList[9].text = mTime.ToString();
            tlmnTxtList[9].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            tlmnTxtList[9].gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
        }

        tlmnTxtList[9].text = App.listKeyText["GAME_START"]; //"BẮT ĐẦU";
        tlmnTxtList[9].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        tlmnTxtList[9].gameObject.SetActive(false);
    }

    public void balanceChanged(int slotId,long chipBalance,long starBalance)
    {
        int mSl = getSlotIdBySvrId(slotId);
        //Debug.Log("=================================================" + mSl);
        if (mSl > -1)
        {
            //Debug.Log(" ===================================================== > -1");
            //StartCoroutine(_balanceChanged(chipList[mSl], chipBalance, mbTextList[13 + mSl]));
            chipList[mSl] = chipBalance;
            playerList[mSl].ChipBalance = chipBalance;
            playerList[mSl].StarBalance = starBalance;
            tlmnTxtList[1 + mSl].text = mSl == 0 ? App.formatMoney(chipBalance.ToString()) : App.formatMoneyAuto(chipBalance);
        }
    }

    public void showSettingPanel()
    {
        LoadingControl.instance.openSettingPanel();
    }

    #region //CHAT
    [Header("=====CHAT=====")]
    public GameObject[] chatPanels;
    public Image[] chatImoIco;
    public void showChatBox()
    {
        LoadingControl.instance.showChatBox(LoadingControl.CHANNEL_TABLE);
    }

    public void showChatPanels(string sender, string content, string emo, Sprite emoSprite = null)
    {
        foreach (Player pl in playerList.Values.ToList())
        {
            if (pl.NickName == sender && pl.SlotId > -1)
            {
                if (emoSprite == null)
                {
                    chatPanels[pl.SlotId].GetComponentInChildren<Text>().text = content;
                    if (!chatPanels[pl.SlotId].activeSelf)
                        StartCoroutine(_showChatPanels(chatPanels[pl.SlotId]));
                    return;
                }
                //App.trace("emo = " + emo);
                chatImoIco[pl.SlotId].sprite = emoSprite;
                if (!chatImoIco[pl.SlotId].gameObject.activeSelf)
                {
                    chatImoIco[pl.SlotId].gameObject.SetActive(true);
                    chatImoIco[pl.SlotId].transform.DOScale(1.2f, 4f).SetEase(Ease.OutBounce).OnComplete(() => {
                        chatImoIco[pl.SlotId].gameObject.SetActive(false);
                        chatImoIco[pl.SlotId].transform.localScale = Vector3.one;
                    });
                    return;
                }
                chatImoIco[pl.SlotId].transform.DORestart();
                return;
            }
        }
    }
    IEnumerator _showChatPanels(GameObject goj)
    {
        goj.SetActive(true);
        yield return new WaitForSeconds(3);
        goj.SetActive(false);
        yield break;
    }
    #endregion

    public void stateBtnsAction(string act)
    {
        OutBounMessage req = null;
        switch (act)
        {
            case "fire":    //đánh bài
                req = new OutBounMessage("FIRE_CARD");
                break;
            case "pass":    //bỏ lượt
                req = new OutBounMessage("PASS_TURN");
                break;
            case "unselect":    //bỏ chọn
                for (int i = 0; i < myCardIdList.Count; i++)
                {
                    if (playerCardList[i].Rtf.anchoredPosition.y < 80 && 50 < playerCardList[i].Rtf.anchoredPosition.y)
                    {
                        playerCardList[i].Rtf.anchoredPosition -= new Vector2(0, playerCardList[i].Rtf.anchoredPosition.y);
                    }

                }
                break;
        }
        if(req != null)
        {
            req.addHead();
            if(act == "fire")
            {
                List<int> ids = new List<int>();
                for (int i = 0; i < myCardIdList.Count; i++)
                {
                    if (playerCardList[i].Rtf.anchoredPosition.y < 80 && 50 < playerCardList[i].Rtf.anchoredPosition.y)
                    {
                        ids.Add(myCardIdList[i] - 1);
                        //App.trace(i + "|card = " + myCardIdList[i]);
                    }

                }
                if (ids.Count == 0)
                {
                    //App.showErr("Bạn chưa chọn bài");
                    App.showErr(App.listKeyText["CARD_NOT_SELECTED"]);
                    return;
                }
                if (state.Mode == 1)
                {
                    req.writeByte(ids[0]);
                }
                else
                {
                    req.writeBytes(ids);
                }
            }
            App.ws.send(req.getReq(), (res)=> {
                if (act == "fire")
                {
                    IsAutoDiscard = false;
                }
            }, true, 0);
        }
    }

    #region PLAYER INFO

    public void showPlayerInfo(int slotIdToShow)
    {

        if (exitsSlotList[slotIdToShow] == false)
        {
            LoadingControl.instance.sendInvite();
            return;
        }
        Player pl = null;
        string typeShowInfo = "";
        if (slotIdToShow == 0)
        {
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, tlmnImgList[slotIdToShow].overrideSprite, "me");
            //ProfileController.instance.Show();
            return;
        }

        foreach (Player mpl in playerList.Values.ToList())
        {
            if (mpl.SlotId == slotIdToShow)
            {
                pl = mpl;
                //LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, playerAvatarList[slotIdToShow].sprite,typeShowInfo);
                var req_info = new OutBounMessage("PLAYER_PROFILE");
                //Debug.Log("WRITE LONG = " + CPlayer.id);
                //App.trace("PPLAYER ID = " + CPlayer.id);
                req_info.addHead();
                req_info.writeLong(pl.PlayerId);
                req_info.writeByte(0x0f);
                req_info.writeAcii("");

                App.ws.send(req_info.getReq(), delegate (InBoundMessage res) {
                    var nickName = res.readAscii();
                    var fullName = res.readString();
                    var avatar = res.readAscii();
                    var isMale = res.readByte() == 1;
                    //App.trace("isMale = " + isMale);
                    var dateOfBirth = res.readAscii();
                    var message = res.readString();
                    var chipBalance = res.readLong();
                    var starBalance = res.readLong();
                    var phone = res.readAscii();
                    var email = res.readAscii();
                    var address = res.readAscii();
                    var cmnd = res.readAscii();
                    var isLocalPlayer = res.readByte() == 1;
                    var clubCode = res.readAscii();
                    var clubName = res.readString();
                    var path = res.readAscii();
                    var pathName = res.readString();
                    var clubId = res.readLong();
                    var attSize = res.readByte();
                    for (int i = 0; i < attSize; i++)
                    {
                        var code = res.readAscii();
                        var value = res.readAscii();
                    }
                    var itemSize = res.readByte();
                    for (int i = 0; i < itemSize; i++)
                    {
                        string itemImage = res.readAscii();
                        string itemName = res.readString();
                        string itemDesc = res.readString();
                        string itemPosition = res.readAscii();
                        string itemTour = res.readString();
                        string itemCreated = res.readAscii();
                    }

                    var count = res.readByte();
                    //App.trace("Số bản ghi " + count);
                    for (int i = 0; i < count; i++)
                    {
                        string gameId = res.readAscii();
                        string zoneName = res.readString();
                        string levelName = res.readString();
                        int win = res.readInt();
                        int lose = res.readInt();
                        int draw = res.readInt();
                        int level = res.readShort();
                        long score = res.readLong();
                        long previousScore = res.readLong();
                        long nextScore = res.readLong();

                    }

                    var isCPlayerFriend = res.readByte() == 1; // 0 : not friend 1: friend
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, tlmnImgList[slotIdToShow].overrideSprite, typeShowInfo);
                });
                return;
            }
        }

    }
    #endregion

    private string getAttByIcon(string icon)
    {
        switch (icon)
        {
            case "freeze":
                return App.listKeyText["BAND_CONG"];//"CÓNG";
            case "3_sequence_pair":
                return App.listKeyText["BAND_3DOITHONG"]; //"3 đôi thông";
            case "4_sequence_pair":
                return App.listKeyText["BAND_4DOITHONG"]; //"4 đôi thông";
            case "finish_by_3_of_spade_fail":
                return App.listKeyText["GAME_BLOCK_3B_FINISH"];//"Chặn kết 3 bích";
            case "finish_by_3_of_spade_success":
                return App.listKeyText["GAME_3BICH_FINISH"];//"Kết 3 bích";
            case "quad_pair":
                return App.listKeyText["BAND_TU_QUY"];//"Tứ quý";
            case "white_win":
                return App.listKeyText["WHITE_WIN"];//"THẮNG TRẮNG";
        }
        return "";
    }

    private IEnumerator hideAtt()
    {
        yield return new WaitForSeconds(3f);
        tlmnTxtList[17].gameObject.SetActive(false);
    }

    public void backToTableList()
    {
        if (isPlaying == false || playerList.Count < 2 || regToQuit || myCardIdList.Count < 1)
        {
            //LoadingControl.instance.blackPanel.SetActive(true);
            DOTween.PauseAll();
            LoadingControl.instance.delCoins();
            delAllHandle();

            EnterParentPlace(delegate () {
                if (LoadingControl.instance.chatBox.activeSelf)
                {
                    LoadingControl.instance.chatBox.SetActive(true);

                }
                StartCoroutine(openTable());
            });
        }
    }

    private bool exited = false;
    IEnumerator openTable()
    {
        exited = true;
        CPlayer.preScene = "TLMN";
        //LoadingControl.instance.loadingScene.SetActive(true);
        LoadingUIPanel.Show();
        SceneManager.LoadScene("TableList");
        yield return new WaitForSeconds(0.05f);
    }
    private void EnterParentPlace(Action callback)
    {
        CPlayer.clientCurrentMode = 0; // modeview
        CPlayer.clientTargetMode = 0;//mode view
        //LoadingControl.instance.loadingScene.SetActive(true);
        LoadingUIPanel.Show();
        var req = new OutBounMessage("ENTER_PARENT_PLACE");
        req.addHead();
        req.writeString("");
        req.writeByte(0);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            if (callback != null)
                callback();
        });
    }

    private IEnumerator notiCoroitine = null;
    public void showNoti()
    {
        if (playerList.Count < 2 || myCardIdList.Count < 1 || isPlaying == false)
        {
            this.regToQuit = true;
            LoadingControl.instance.delCoins();
            backToTableList();
            return;
        }
        regToQuit = !regToQuit;
        tlmnTxtList[22].GetComponentInChildren<Text>().text = !regToQuit ? App.listKeyText["GAME_BOARD_EXIT_CANCEL"] : App.listKeyText["GAME_BOARD_EXIT"]; //"Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        tlmnImgList[11].transform.localScale = new Vector2(regToQuit ? -1 : 1, 1);
        tlmnObjList[26].SetActive(true);

        if(notiCoroitine != null)
        {
            StopCoroutine(notiCoroitine);
        }
        notiCoroitine = _showNoti();
        StartCoroutine(notiCoroitine);

    }

    private IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        tlmnObjList[26].SetActive(false);
    }

    private Vector2[] coordinatesList = { new Vector2(90, 180), new Vector2(1576, 542), new Vector2(793, 818), new Vector2(28, 547) };

    public bool IsAutoDiscard
    {
        get
        {
            return _isAutoDiscard;
        }

        set
        {
            _isAutoDiscard = value;
        }
    }

    private IEnumerator _flyCoins(List<int> winLs, List<int> loseLs)
    {

        if(regToQuit == false) {
            //App.trace("WIN = " + winLs.Count + "loseLS " + loseLs.Count);
            Vector2 end = coordinatesList[getSlotIdBySvrId(winLs[0])];
            //Comment out
            SoundManager.instance.PlayUISound(SoundFX.CARD_TAI_XIU_BET);
            for (int j = 0; j < loseLs.Count; j++)
            {
                Vector2 start = coordinatesList[getSlotIdBySvrId(loseLs[j])];

                LoadingControl.instance.flyCoins("line", 10, start, end,0,0,tlmnObjList[5].transform);
            }
            yield return new WaitForSeconds(1f);
            isPlaying = false;
        }
        else
        {
            yield return new WaitForSeconds(2f);
            regToQuit = true;
            backToTableList();
        }

    }

    public void test(int id)
    {
        /*
        for (int i = 1; i < 4; i++) //Chia bài cho 3 thằng kia
        {
            StartCoroutine(divideCards2(i));
        }
        */
        //StartCoroutine(divideCards(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, false)); //Chia bài cho mình

        /*
        if(id == 0)
            moveCard(new List<int>() { 1, 2, 3,7,5,3,2,3 }, 3); //Thằng khác nhả bài
        if(id == 1)
            myCard(new List<int>() {2, 7, 13 });    //Mình nhả bài
        if(id == 2)
            myCard(new List<int>() { 1, 6, 8,9 });
            */

        /*
        moveCard(new List<int>() { 1, 2, 3, 7, 5, 3, 2, 3,10,21,17,23,34 }, 2, "show"); //Show card của chúng nó
        moveCard(new List<int>() { 1, 2, 3, 7, 5, 3, 2, 3, 10, 21, 17, 23, 34 }, 1, "show");
        moveCard(new List<int>() { 1, 2, 3, 7, 5, 3, 2, 3, 10, 21, 17, 23, 34 }, 3, "show");

        myCard(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, "show");  //Show bài mình
        */

        //preCoroutine = _showCountDOwn(20);
        //StartCoroutine(preCoroutine);

        //LoadingControl.instance.flyCoins("line", 10, coordinatesList[0], coordinatesList[2]);
    }
}
