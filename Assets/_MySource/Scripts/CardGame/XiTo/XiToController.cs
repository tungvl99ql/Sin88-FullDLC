using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class XiToController : MonoBehaviour {

    /// <summary>
    /// 0-4: Info|5-9: owner|10: Help|11: Trans|12-16: TimeLeap|17: foldCards|18: toPanel|19: notiPanel|20: back
    /// |21-25: ChipImage|26: ChipImageToClone
    /// </summary>
    public GameObject[] xiToGojList;
    /// <summary>
    /// 0-4: userName|5-9 balance|10: tableName|11: CountDOwn|12-16: grade|17-21: earnText|22-26: bandName|
    /// 27-31: chip|32: chipAll|33: sliderText
    /// </summary>
    public Text[] xiToTextList;
    /// <summary>
    /// 0-4: ava|5-9: handCard|10: card0000
    /// </summary>
    public Image[] xiToImageList;
    public Sprite[] cardFaces;
    /// <summary>
    /// 0:Trắng|1: Vàng|2: Xanh lá|3: xanh biển
    /// </summary>
    public Font[] xiToFontList;
    /// <summary>
    /// 0: chipAll|1-5: ChipRtf
    /// </summary>
    public RectTransform[] xiToRtfList;
    public Sprite addPlayerIcon;
    public Slider xiToSlider;
    //0: TỐ|1: THEO|2: ÚP|3: XEM|4: Mở
    public Button[] stateBtns;
    // Use this for initialization
    [HideInInspector]
    public bool regQuit = false;
    private bool isPlaying = false, isHaveCards = false;
    private List<GameObject> firedCards = new List<GameObject>();
    private Dictionary<int, List<RectTransform>> firedCardDic = new Dictionary<int, List<RectTransform>>();
    private int firstRoundBetAmount = 0, maxBetAmount = 0, lastBetAmount = 0, availableAmount = 0, rate = 0;
    private Vector2[] coordinatesList = new Vector2[5];
    [HideInInspector]
    public static XiToController instance;

    public Sprite[] tableBackground;

    private void Awake()
    {
        Application.runInBackground = true;
        getInstance();
        for(int i = 0; i < 5; i++)
        {
            RectTransform rtf = xiToTextList[12 + i].gameObject.GetComponent<RectTransform>();
            coordinatesList[i] = new Vector2(30 + rtf.anchoredPosition.x + (rtf.pivot.x * 1600) + (i == 0 ? 50 : 0), rtf.anchoredPosition.y + (rtf.pivot.y * 850) + (i == 0 ? 50 : 0));
        }
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

    void Start () {
        //myCard(new List<int>() { 39,26}, "divide");
        //return;

        //set background cua ban choi theo tung muc cuoc
        int bgIndex = 0;

        if (CPlayer.betAmtId > 3 && CPlayer.betAmtId < 8)
        {
            bgIndex = 1;
        }
        else if (CPlayer.betAmtId >= 8)
        {
            bgIndex = 2;
        }

        xiToImageList[11].sprite = tableBackground[bgIndex];


        StartCoroutine(LoadingControl.instance._start());
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
                        string tempString = App.listKeyText["XITO_NAME"].ToUpper();
                        xiToTextList[10].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " " + App.listKeyText["CURRENCY"];//"XÌ TỐ - " + CPlayer.betAmtOfTableToGo + " Gold";
                        break;
                    }


                }
            });
        }
        else
        {
            string tempString = App.listKeyText["XITO_NAME"].ToUpper();
            xiToTextList[10].text = tempString + " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"]; //"XÌ TỐ - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
        }

        registerHandler();
        getTableDataEx();
    }

    private float time = 1, curr = 0;
    private bool run = false;
    private int preTimeLeapId = -1;
    private Image preTimeLeapImage;
    // Update is called once per frame
    void Update () {
        curr += Time.deltaTime;
        if (run)
        {
            preTimeLeapImage.fillAmount = (time - curr) / time;

        }
    }

    public void showHelpPanel(bool isShow)
    {
        if (isShow)
        {
            xiToGojList[10].SetActive(true);
            return;
        }
        xiToGojList[10].SetActive(false);
    }

    public void moveCard(List<int> ids, string act, int slotId, bool fly = true)
    {
        if (act == "divide")
        {
            //Vector2 mPivot = xiToImageList[5].GetComponent<RectTransform>().pivot;
            int childCout = xiToImageList[5 + slotId].transform.parent.childCount - 1;
            Vector2 vec = new Vector2(65, 0);
            if (fly)
            {
                xiToGojList[11].SetActive(true);
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(xiToImageList[10], xiToImageList[10].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                    img.gameObject.SetActive(true);


                    rtf.parent = xiToImageList[5 + slotId].transform.parent;
                    rtf.DOScale(1f, .05f);
                    rtf.DOAnchorMax(Vector2.zero, .25f);
                    rtf.DOAnchorMin(Vector2.zero, .25f);
                    rtf.DOPivot(Vector2.zero, .25f);

                    Vector2 mPos = vec * (i + childCout);
                    if (slotId < 3)
                    {
                        mPos = new Vector2(260 - mPos.x, 0);
                        rtf.SetAsFirstSibling();
                    }
                    firedCards.Add(img.gameObject);
                    if (firedCardDic.ContainsKey(slotId))
                    {
                        firedCardDic[slotId].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(slotId, new List<RectTransform>() { rtf });
                    }
                    rtf.DORotate(new Vector3(0, 90, 0), .25f + tmp * .01f).OnComplete(() =>
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
                                xiToGojList[11].SetActive(false);
                            }
                        });

                    });
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f + +tmp * .05f);
                }
            }
            else
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(xiToImageList[10], xiToImageList[10].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();


                    rtf.parent = xiToImageList[5 + slotId].transform.parent;
                    rtf.localScale = Vector3.one;
                    rtf.anchorMax = Vector2.zero;
                    rtf.anchorMin = Vector2.zero;
                    rtf.pivot = Vector2.zero;
                    Vector2 mPos = vec * (i + childCout);
                    if (slotId < 3)
                    {
                        mPos = new Vector2(260 - mPos.x, 0);
                        rtf.SetAsFirstSibling();
                    }
                    firedCards.Add(img.gameObject);
                    if (firedCardDic.ContainsKey(slotId))
                    {
                        firedCardDic[slotId].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(slotId, new List<RectTransform>() { rtf });
                    }
                    try
                    {
                        img.overrideSprite = cardFaces[ids[tmp]];
                    }
                    catch
                    {
                        img.overrideSprite = cardFaces[52];
                    }
                    rtf.anchoredPosition = mPos;
                    img.gameObject.SetActive(true);
                }
            }
            return;
        }
        if(act == "show")
        {
            delFiredCards();

            Vector2 vec = new Vector2(65, 0);
            xiToGojList[11].SetActive(true);
            for (int i = 0; i < ids.Count; i++)
            {
                int tmp = i;
                Image img = Instantiate(xiToImageList[5 + slotId], xiToImageList[5 + slotId].transform.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();

                Vector2 mPos = vec * (i);

                if (slotId < 3)
                {
                    rtf.anchoredPosition = new Vector2(260, 0);
                    mPos = new Vector2(260 - mPos.x, 0);
                    rtf.SetAsFirstSibling();
                }
                img.overrideSprite = cardFaces[ids[tmp]];
                img.gameObject.SetActive(true);


                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f + +tmp * .05f).OnComplete(()=> {
                    firedCards.Add(img.gameObject);
                    if (tmp == ids.Count - 1)
                        xiToGojList[11].SetActive(false);

                });

            }
            return;
        }

        if(act == "fold")
        {
            List<RectTransform> rtfList = firedCardDic[slotId];
            Vector2 mPos = new Vector2(0,50);

            for (int i = 0; i < rtfList.Count; i++)
            {
                RectTransform rtf = rtfList[i];
                rtf.parent = xiToGojList[17].transform;
                rtf.DOAnchorMax(Vector2.one * .5f, .25f);
                rtf.DOAnchorMin(Vector2.one * .5f, .25f);
                rtf.DOPivot(Vector2.one * .5f, .25f);
                if(slotId == 0)
                {
                    rtf.DOScale(1f, .25f);
                }
                rtf.DORotate(new Vector3(0, 0, UnityEngine.Random.RandomRange(80, 100)), .25f);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f);

            }
            return;
        }
    }

    private List<int> myCardIds = new List<int>();
    private void myCard(List<int> ids, string act, bool fly = true)
    {
        if(act == "divide")
        {
            bool isSetFirstBorder = false;
            if (ids.Count > 1)
                isSetFirstBorder = true;
               //Vector2 mPivot = xiToImageList[5].GetComponent<RectTransform>().pivot;
            Vector2 vec = new Vector2(120, 0);
            int childCout = xiToImageList[5].transform.parent.childCount - 1;
            if (fly)
            {
                xiToGojList[11].SetActive(true);

                for (int i = 0; i < ids.Count; i++)
                {
                    myCardIds.Add(ids[i]);
                    int tmp = i;
                    Image img = Instantiate(xiToImageList[10], xiToImageList[10].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                    if (isSetFirstBorder && tmp == 0)
                        img.transform.GetChild(0).gameObject.SetActive(true);
                    img.gameObject.SetActive(true);
                    Vector2 mPos = vec * (i + childCout);

                    firedCards.Add(img.gameObject);
                    if (firedCardDic.ContainsKey(0))
                    {
                        firedCardDic[0].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(0, new List<RectTransform>() { rtf });
                    }

                    rtf.parent = xiToImageList[5].transform.parent;
                    rtf.DOScale(1.5f, .05f);
                    rtf.DOAnchorMax(Vector2.zero, .25f);
                    rtf.DOAnchorMin(Vector2.zero, .25f);
                    rtf.DOPivot(Vector2.zero, .25f);
                    rtf.DORotate(new Vector3(0, 90, 0), .25f + tmp * .01f).OnComplete(() =>
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
                                xiToGojList[11].SetActive(false);
                            }
                        });

                    });
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f + +tmp * .05f);
                }
            }
            else
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    myCardIds.Add(ids[i]);
                    int tmp = i;
                    Image img = Instantiate(xiToImageList[10], xiToImageList[10].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();

                    Vector2 mPos = vec * (i + childCout);

                    firedCards.Add(img.gameObject);
                    if (firedCardDic.ContainsKey(0))
                    {
                        firedCardDic[0].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(0, new List<RectTransform>() { rtf });
                    }

                    rtf.parent = xiToImageList[5].transform.parent;
                    rtf.localScale = Vector3.one * 1.5f;
                    rtf.anchorMax = Vector2.zero;
                    rtf.anchorMin = Vector2.zero;
                    rtf.pivot = Vector2.zero;
                    App.trace("FAC " + ids[tmp]);
                    try
                    {
                        img.overrideSprite = cardFaces[ids[tmp]];
                    }
                    catch
                    {
                        img.overrideSprite = cardFaces[52];
                    }
                    rtf.anchoredPosition = mPos;
                    if (isSetFirstBorder && tmp == 0)
                        img.transform.GetChild(0).gameObject.SetActive(true);
                    img.gameObject.SetActive(true);
                }
            }
            xiToTextList[22].text = getChiByIds(myCardIds);
            xiToTextList[22].transform.parent.gameObject.SetActive(true);
            return;
        }
        if (act == "show")
        {
            delFiredCards();

            Vector2 vec = new Vector2(120, 0);
            xiToGojList[11].SetActive(true);
            for (int i = 0; i < ids.Count; i++)
            {
                int tmp = i;
                Image img = Instantiate(xiToImageList[5], xiToImageList[5].transform.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                img.overrideSprite = cardFaces[ids[tmp]];
                img.gameObject.SetActive(true);
                Vector2 mPos = vec * (i);

                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f + +tmp * .05f).OnComplete(() => {
                    firedCards.Add(img.gameObject);
                    if (tmp == ids.Count - 1)
                        xiToGojList[11].SetActive(false);
                });

            }
            return;
        }
    }

    private List<string> handelerCommand = new List<string>();  //Lưu các handler đã đăng ký
    private void registerHandler()
    {
        #region [ENTER_STATE]
        var req_ENTER_STATE = new OutBounMessage("ENTER_STATE");
        req_ENTER_STATE.addHead();
        handelerCommand.Add("ENTER_STATE");
        App.ws.sendHandler(req_ENTER_STATE.getReq(), delegate (InBoundMessage res_ENTER_STATE)
        {
            int stateId = res_ENTER_STATE.readByte();
            enterState(stateById[stateId]);
            enterState2(stateById[stateId]);
        });
        #endregion

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
            App.trace("SLOT_IN_TABLE_CHANGED slot = " + slotId + "nick = " + nickName);

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
                SoundManager.instance.PlayUISound(SoundFX.CARD_EXIT_TABLE);
                playerList.Remove(slotId);

                xiToImageList[slotId].sprite = addPlayerIcon;
                xiToImageList[slotId].overrideSprite = addPlayerIcon;
                //xiToImageList[slotId].material = null;
                exitsSlotList[slotId] = false;
                if (slotId != 0)
                {
                    xiToGojList[5 + slotId].SetActive(false);   //Xóa owner của thằng thoát
                    xiToGojList[slotId].SetActive(false);   //Ẩn info thằng thoát
                }
                if (playerList.Count < 2)   //Khi bàn chỉ còn 1 thằng thì k đếm ngược nữa
                {
                    if (preCoroutine != null)
                    {
                        xiToTextList[11].gameObject.SetActive(false);
                        StopCoroutine(preCoroutine);

                    }

                }
                return;
            }
            SoundManager.instance.PlayUISound(SoundFX.CARD_JOIN_TABLE);
            if (playerList.ContainsKey(slotId)) //Có thằng thay thế
            {
                playerList[slotId] = player;
                exitsSlotList[slotId] = true;
                //setInfo(player, phomImageList[player.SlotId], phomObjList[player.SlotId], phomTextList[player.SlotId + 4], phomTextList[player.SlotId], phomObjList[player.SlotId + 4]);
                setInfo(player, xiToImageList[player.SlotId], xiToGojList[player.SlotId], xiToTextList[5 + player.SlotId], xiToTextList[player.SlotId], xiToGojList[5 + player.SlotId]);
                return;
            }
            //Thêm bình thường
            playerList.Add(slotId, player);
            exitsSlotList[slotId] = true;
            //setInfo(player, phomImageList[player.SlotId], phomObjList[player.SlotId], phomTextList[player.SlotId + 4], phomTextList[player.SlotId], phomObjList[player.SlotId + 4]);
            setInfo(player, xiToImageList[player.SlotId], xiToGojList[player.SlotId], xiToTextList[5 + player.SlotId], xiToTextList[player.SlotId], xiToGojList[5 + player.SlotId]);

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

            App.trace("RECV [SET_TURN]");
            App.trace("slotId = " + slotId + "| turnTimeOut = " + turnTimeOut + "|playerRemainDuration  = " + playerRemainDuration);
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
                xiToGojList[12 + preTimeLeapId].SetActive(false);
                slotId = getSlotIdBySvrId(slotId);
                preTimeLeapId = slotId;
                time = turnTimeOut;
                curr = playerRemainDuration;
                xiToGojList[12 + preTimeLeapId].SetActive(true);
                preTimeLeapImage = xiToGojList[12 + preTimeLeapId].GetComponent<Image>();
                preTimeLeapImage.fillAmount = 1;
                run = true;
            }
            else
            {
                run = false;
                xiToGojList[12 + preTimeLeapId].SetActive(false);

            }

        });
        #endregion

        #region [GAMEOVER]
        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        handelerCommand.Add("GAMEOVER");
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res) {
            App.trace("RECV [GAMEOVER]");
            run = false;


            if (preTimeLeapImage != null)
                preTimeLeapImage.gameObject.SetActive(false);   //Ẩn time leap

            List<int> winLs = new List<int>(), loseLs = new List<int>();

            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int slotId = res.readByte();
                int grade = res.readByte();
                long earnValue = res.readLong();
                App.trace("RS|slotId = " + slotId + "|grade = " + grade + "earnValue = " + earnValue);
                string title = "Thua";
                int fontId = 0;
                if (grade == 1)
                {
                    title = "Thắng";
                    fontId = 1;
                    if (slotId == mySlotId)
                    {
                        SoundManager.instance.PlayUISound(SoundFX.CARD_PHOM_WIN);
                    }
                    else
                    {
                        SoundManager.instance.PlayUISound(SoundFX.CARD_LOSE);
                    }
                }


                slotId = getSlotIdBySvrId(slotId);
                xiToTextList[12 + slotId].font = xiToFontList[fontId];
                xiToTextList[12 + slotId].text = title;
                xiToTextList[12 + slotId].gameObject.SetActive(true);

                xiToTextList[12 + slotId].transform.DOScale(1.2f, 1f).SetLoops(5).OnComplete(()=> {
                    xiToTextList[12 + slotId].transform.localScale = Vector3.one;
                });


                if(earnValue > -1)
                {
                    xiToTextList[17 + slotId].font = xiToFontList[1];
                    xiToTextList[17 + slotId].text = "+" + App.formatMoney(earnValue.ToString());
                    winLs.Add(slotId);
                }
                else
                {
                    xiToTextList[17 + slotId].font = xiToFontList[0];
                    xiToTextList[17 + slotId].text = "-" + App.formatMoney(Math.Abs(earnValue).ToString());
                    loseLs.Add(slotId);

                }
                xiToTextList[17 + slotId].transform.localPosition -= new Vector3(0, 100, 0);
                xiToTextList[17 + slotId].gameObject.SetActive(true);
                xiToTextList[17 + slotId].transform.DOLocalMoveY(xiToTextList[17 + slotId].transform.localPosition.y + 100, 1f);
            }
            string matchResult = res.readStrings();
            StartCoroutine(_quit(winLs, loseLs));
            StartCoroutine(_ClearCardWhenGameOver());
        });
        #endregion

        #region [START_MATCH]
        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        handelerCommand.Add("START_MATCH");
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res_START_MATCH) {
            App.trace("RECV [START_MATCH]");
            DOTween.KillAll();
            SoundManager.instance.PlayUISound(SoundFX.CARD_START);
            for (int i = 0;i < 5; i++)
            {
                xiToTextList[12 + i].gameObject.SetActive(false);   //Ẩn thứ hạng
                xiToTextList[17 + i].gameObject.SetActive(false);   //Ẩn tiền ăn được
                xiToTextList[22 + i].transform.parent.gameObject.SetActive(false);  //Ẩn chi
                xiToRtfList[1 + i].gameObject.SetActive(false); //Ẩn chip
            }

            delFiredCards();
            firedCardDic.Clear();   //Xóa các quân bài úp
            xiToTextList[32].transform.parent.gameObject.SetActive(false);  //Ẩn chipAll

            isPlaying = true;
            coinFlyed = false;

            loadPlayerMatchPoint(res_START_MATCH);
            loadBoardData(res_START_MATCH, true);
        });
        #endregion

        #region [MOVE]
        var req_MOVE = new OutBounMessage("MOVE");
        req_MOVE.addHead();
        handelerCommand.Add("MOVE");
        App.ws.sendHandler(req_MOVE.getReq(), delegate (InBoundMessage res_MOVE)
        {
            List<int> ids = new List<int>();
            ids = res_MOVE.readBytes();
            //CardUtils.svrIdsToIds(ids);
            SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
            int sourceSlotId = res_MOVE.readByte();
            int sourceLineId = res_MOVE.readByte() - 1;
            int targetSlotId = res_MOVE.readByte();
            int targetLineId = res_MOVE.readByte() - 1;
            int targetIndex = res_MOVE.readByte();

            App.trace(string.Format("RECV [MOVE] IdsCount = {0} | sourceSlotId = {1} | sourceLineId = {2}| targetSlotId = {3}| targetLineId = {4} | targetIndex = {5}| CON DATA =\n",
            ids.Count, sourceSlotId, sourceLineId, targetSlotId, targetLineId, targetIndex));
            targetSlotId = getSlotIdBySvrId(targetSlotId);
            if(targetSlotId != 0)
            {
                moveCard(ids, "divide", targetSlotId);
            }
            else
            {
                myCard(ids, "divide");
            }

            int pot = res_MOVE.readInt();
            collectChip(pot, targetSlotId);
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
            int slotIdClone = slotId;
            slotId = getSlotIdBySvrId(slotId);


            if(slotId == 0)
            {
                myCard(ids, "show");
            }
            else
            {
                moveCard(ids, "show", slotId);
            }

            string bandName = res_SHOW_PLAYER_CARD.readString();
            if(bandName.Length > 0)
            {
                xiToTextList[22 + slotId].text = bandName;
                xiToTextList[22 + slotId].transform.parent.gameObject.SetActive(true);
            }

            App.trace("RECV [SHOW_PLAYER_CARD] of slot = " + slotId + "|" + ids.Count + "|bandName = " + bandName);

        });
        #endregion

        #region [FOLD]
        var req_FOLD = new OutBounMessage("FOLD");
        req_FOLD.addHead();
        handelerCommand.Add("FOLD");
        App.ws.sendHandler(req_FOLD.getReq(), delegate (InBoundMessage res_FOLD)
        {
            App.trace("RECV [FOLD]");
            int slotId = res_FOLD.readByte();
            if (slotId == mySlotId)
            {
                xiToTextList[22].transform.parent.gameObject.SetActive(false);
            }
            slotId = getSlotIdBySvrId(slotId);
            moveCard(null, "fold", slotId);
            xiToTextList[12 + slotId].text = App.listKeyText["POKER_DOWN"].ToUpper();//"ÚP";
            xiToTextList[12 + slotId].font = xiToFontList[0];
            xiToTextList[12 + slotId].gameObject.SetActive(true);
            showToPanel(false);

        });
        #endregion

        #region [RAISE]
        var req_RAISE = new OutBounMessage("RAISE");
        req_RAISE.addHead();
        handelerCommand.Add("RAISE");
        App.ws.sendHandler(req_RAISE.getReq(), delegate (InBoundMessage res_RAISE)
        {
            App.trace("RECV [RAISE]");

            int slotId = res_RAISE.readByte();
            string type = res_RAISE.readAscii();
            int betAmount = res_RAISE.readInt();
            int potAmount = res_RAISE.readInt();
            slotId = getSlotIdBySvrId(slotId);
            if (betAmount > firstRoundBetAmount)
            {
                if(betAmount > maxBetAmount)
                {
                    maxBetAmount = betAmount;
                    xiToTextList[12 + slotId].text = App.listKeyText["POKER_TO"].ToUpper();//"TỐ";
                    xiToTextList[12 + slotId].font = xiToFontList[1];
                }
                else
                {
                    xiToTextList[12 + slotId].text = App.listKeyText["POKER_THEO"].ToUpper();//"THEO";
                    xiToTextList[12 + slotId].font = xiToFontList[3];
                }

                xiToTextList[12 + slotId].gameObject.SetActive(true);
            }
            SoundManager.instance.PlayUISound(SoundFX.CARD_RAISE);
            xiToTextList[27 + slotId].text = App.formatMoney((betAmount - potAmount).ToString());
            xiToTextList[27 + slotId].transform.parent.gameObject.SetActive(true);
            GameObject goj = Instantiate(xiToGojList[26], xiToGojList[26].transform.parent, false);
            RectTransform rtf = goj.GetComponent<RectTransform>();
            rtf.anchoredPosition = coordinatesList[slotId];
            goj.SetActive(true);
            rtf.parent = xiToGojList[21 + slotId].transform.parent;
            rtf.DOLocalMove(xiToGojList[21 + slotId].transform.localPosition, .25f);

            showToPanel(false); //Ẩn panel Tố
        });
        #endregion

        #region [CHECK]
        var req_CHECK = new OutBounMessage("CHECK");
        req_CHECK.addHead();
        handelerCommand.Add("CHECK");
        App.ws.sendHandler(req_CHECK.getReq(), delegate (InBoundMessage res_CHECK)
        {
            App.trace("RECV [CHECK]");
            var slotId = res_CHECK.readByte();
            slotId = getSlotIdBySvrId(slotId);
            xiToTextList[12 + slotId].text = App.listKeyText["GAME_VIEW"];//"Xem";
            xiToTextList[12 + slotId].font = xiToFontList[2];
            xiToTextList[12 + slotId].gameObject.SetActive(true);
            showToPanel(false);
        });
        #endregion

        #region [DIVIDE_CHIP]
        var req_DIVIDE_CHIP = new OutBounMessage("DIVIDE_CHIP");
        req_DIVIDE_CHIP.addHead();
        handelerCommand.Add("DIVIDE_CHIP");
        App.ws.sendHandler(req_DIVIDE_CHIP.getReq(), delegate (InBoundMessage res_DIVIDE_CHIP) {

            int slotCount = res_DIVIDE_CHIP.readByte();
            int mSlotId = -1;
            for(int i = 0; i < slotCount; i++)
            {
                int slotId = res_DIVIDE_CHIP.readByte();
                int betAmount = res_DIVIDE_CHIP.readInt();
                int earnAmount = res_DIVIDE_CHIP.readInt();
                App.trace("slotId = " + slotId + "|betAmount = " + betAmount + "earnAmount = " + earnAmount);
                if(earnAmount > 0)
                {
                    mSlotId = getSlotIdBySvrId(slotId);
                    collectChip(earnAmount + betAmount, getSlotIdBySvrId(slotId));
                }
                else
                {
                    collectChip( -1, getSlotIdBySvrId(slotId));
                }
            }
            int pot = res_DIVIDE_CHIP.readInt();
            StartCoroutine(clearChip(pot, mSlotId));
            //collectChip(pot);
            App.trace("RECV [DIVIDE_CHIP] pot = " + pot);
        });

        #endregion


    }

    IEnumerator _ClearCardWhenGameOver()
    {
        if (isPlaying)
            yield return null;
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < 5; i++)
        {
            xiToTextList[12 + i].gameObject.SetActive(false);   //Ẩn thứ hạng
            xiToTextList[17 + i].gameObject.SetActive(false);   //Ẩn tiền ăn được
            xiToTextList[22 + i].transform.parent.gameObject.SetActive(false);  //Ẩn chi
            xiToRtfList[1 + i].gameObject.SetActive(false); //Ẩn chip
        }

        delFiredCards();
        firedCardDic.Clear();   //Xóa các quân bài úp
        xiToTextList[32].transform.parent.gameObject.SetActive(false);  //Ẩn chipAll
    }

    private void getTableDataEx()
    {
        var req_GET_TABLE_DATA_EX = new OutBounMessage("GET_TABLE_DATA_EX");
        req_GET_TABLE_DATA_EX.addHead();
        req_GET_TABLE_DATA_EX.writeAcii("");
        //req_getTableChange.print();
        App.ws.send(req_GET_TABLE_DATA_EX.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RECV [GET_TABLE_DATA_EX]");

            loadStateData(res);
            loadTableData(res);
            loadPlayerMatchPoint(res);
            loadBoardData(res);

        });
    }

    #region //STATE CMD
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

    #endregion

    #region TABLE DATA

    private Dictionary<int, Player> playerList;
    private int mySlotId = -1;
    private int currOwnerId = -1;
    private bool[] exitsSlotList = { false, false, false, false, false };

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
                    setInfo(player, xiToImageList[player.SlotId], xiToGojList[player.SlotId], xiToTextList[5 + player.SlotId], xiToTextList[player.SlotId], xiToGojList[5 + player.SlotId]);
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

        App.trace("currentTurnSlotId = " + currentTurnSlotId + "|currTimeOut = " + currTimeOut + "|slotRemainDuration = " + slotRemainDuration + "|");

        enterState(stateById[currentState]);

        if (currTimeOut > 0 && currentTurnSlotId > -1)
        {
            preTimeLeapId = getSlotIdBySvrId(currentTurnSlotId);
            time = currTimeOut;
            curr = slotRemainDuration;
            xiToGojList[12 + preTimeLeapId].SetActive(true);
            preTimeLeapImage = xiToGojList[12 + preTimeLeapId].GetComponent<Image>();
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
            temp = temp < 0 ? (temp + 5) : temp;
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

    private long[] chipList = { 0, 0, 0, 0, 0};
    private void setInfo(Player player, Image im, GameObject infoObj, Text balanceText, Text nickNamText, GameObject ownerImg)
    {
        //im.gameObject.transform.localScale = Vector3.one;
        StartCoroutine(App.loadImg(im, App.getAvatarLink2(player.Avatar, (int)player.PlayerId), player.SvSlotId == mySlotId));
        if (infoObj != null)
            infoObj.SetActive(true);
        chipList[player.SlotId] = player.ChipBalance;
        //balanceText.text = "100.000 K";
        balanceText.text = (player.SlotId == 0 ? App.formatMoney(player.ChipBalance.ToString()) : App.formatMoneyAuto(player.ChipBalance));
        LayoutRebuilder.ForceRebuildLayoutImmediate(balanceText.gameObject.GetComponent<RectTransform>());
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
        App.trace("RECV [ENTER_STATE] " + currentState);

        foreach (Button btn in stateBtns)
        {
            btn.gameObject.SetActive(false);
        }


        switch (state.Code)
        {
            case "checkTurn":    //XEM
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[3].gameObject.SetActive(true);
                break;
            case "callTurn": //THEO
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[1].gameObject.SetActive(true);
                stateBtns[2].gameObject.SetActive(true);
                break;
            case "lastTurn":    //Mở úp bài
                stateBtns[4].gameObject.SetActive(true);
                stateBtns[2].gameObject.SetActive(true);
                break;
            case "limitedTurn": //Hết lượt tố
                stateBtns[1].gameObject.SetActive(true);
                stateBtns[2].gameObject.SetActive(true);
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

    private void loadBoardData(InBoundMessage res, bool isFly = false)
    {
        if (isPlaying == false)
            return;
        App.trace("===LOAD BOARD DATA===");
        firstRoundBetAmount = res.readInt();
        maxBetAmount = firstRoundBetAmount;
        int slotCount = res.readByte();
        int pot = 0;
        for (int i = 0; i < slotCount; i++)
        {
            int slotId = res.readByte();
            slotId = getSlotIdBySvrId(slotId);
            App.trace("SLOT ID ========= " + slotId);
            int lineCount = res.readByte();
            //App.trace("lineCount = " + lineCount);
            if(lineCount > 0)
            for (int j = 0; j < lineCount; j++)
            {
                int cardLineId = res.readByte() - 1;
                int cardCount = res.readByte();
                //App.trace("slotId = " + slotId + "|line = " + j + "|cardLineId = " + cardLineId + "|cardCound = " + cardCount);
                if (cardCount < 0)   //
                {

                    List<int> ids = new List<int>();
                    ids = res.readBytes();

                    //CardUtils.svrIdsToIds(ids);

                    if (slotId == 0 && cardLineId == 0) //Bài của mình
                    {
                        isHaveCards = true; //Mình có cầm bài trong tay
                        myCard(ids,"divide" , isFly);  //CHIA BÀI CÙA MÌNH
                        continue;
                    }

                    if (slotId != 0)
                    {
                            //phomObjList[slotId + 7].SetActive(true);    //Hiện bài úp của thằng khác
                            moveCard(ids, "divide", slotId, isFly);
                            continue;
                    }
                    if (cardLineId == 1 && ids.Count > 0)    //Bài ăn
                    {

                        continue;
                    }

                }
                else
                {
                    App.trace("slotId = " + slotId + "CARD COUNT = " + cardCount);
                    /*
                    for(int k = 0; k < cardCount; k++)
                    {

                    }*/
                }
            }
            availableAmount = res.readInt();
            int betAmount = res.readInt();
            int potAmount = res.readInt();
            pot += potAmount;
            if(betAmount > 0 && potAmount == 0)
            {
                xiToTextList[27 + slotId].text = App.formatMoney(betAmount.ToString());
                xiToRtfList[1 + slotId].gameObject.SetActive(true);
            }
            if (maxBetAmount < betAmount)
                maxBetAmount = betAmount;
            App.trace("availableAmount = " + availableAmount + "|betAmount = " + betAmount + "|potAmount = " + potAmount);
        }

        if(pot > 0)
        {
            xiToTextList[32].text = App.formatMoney(pot.ToString());
            xiToRtfList[0].gameObject.SetActive(true);
        }
    }

    public class CardUtils
    {
        private static string[] cardTypes = { "bích", "tép", "rô", "cơ" };
        /// <summary>
        /// Chuyển Id trên server về id trong sheet bài
        /// </summary>
        public static void svrIdsToIds(List<int> arr)
        {
            int cardHigh = 0; // A 2 .. 10 J Q K
            int cardType = 0; // Chất của quân bài
            int temp = 0;
            for (int i = 0; i < arr.Count; i++)
            {
                temp = arr[i];
                cardType = (int)Mathf.Floor((float)temp / 13);
                cardHigh = temp % 13 + 1;

                switch (cardHigh)
                {
                    case 1:
                        App.trace("A " + cardTypes[cardType]);
                        break;
                    case 11:
                        App.trace("J " + cardTypes[cardType]);
                        break;
                    case 12:
                        App.trace("Q " + cardTypes[cardType]);
                        break;
                    case 13:
                        App.trace("K " + cardTypes[cardType]);
                        break;
                    default:
                        //App.trace(cardHigh + cardTypes[cardType]);
                        break;

                }
            }
        }
    }

    private IEnumerator preCoroutine = null;
    private IEnumerator _showCountDOwn(int timeOut)
    {
        /*
        if (regQuit)
        {
            backToTableList();
            yield break;
        }*/
        float mTime = timeOut - 2;

        xiToTextList[11].text = App.listKeyText["GAME_PREPARE"];//"CHUẨN BỊ";
        xiToTextList[11].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        xiToTextList[11].gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            xiToTextList[11].text = mTime.ToString();
            xiToTextList[11].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            xiToTextList[11].gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
        }

        xiToTextList[11].text = App.listKeyText["GAME_START"];//"BẮT ĐẦU";
        xiToTextList[11].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        xiToTextList[11].gameObject.SetActive(false);
    }

    private void delFiredCards()
    {
        foreach (GameObject goj in firedCards)   //XÓA CÁC QUÂN BÀI TRÊN BÀN CHƠI
        {
            try
            {
                DestroyImmediate(goj);
            }
            catch
            {
                App.trace("DESTROY FIRED CARD FAILD!");
            }
        }
        firedCards.Clear();

    }

    public void balanceChanged(int slotId, long chipBalance, long starBalance)
    {
        int mSl = getSlotIdBySvrId(slotId);
        if (mSl > -1)
        {
            //StartCoroutine(_balanceChanged(chipList[mSl], chipBalance, mbTextList[13 + mSl]));
            chipList[mSl] = chipBalance;
            playerList[mSl].ChipBalance = chipBalance;
            playerList[mSl].StarBalance = starBalance;
            xiToTextList[5 + mSl].text = mSl == 0 ? App.formatMoney(chipBalance.ToString()) : App.formatMoneyAuto(chipBalance);
        }

    }

    private void collectChip(int pot, int slotId)
    {
        App.trace("<<<<<<<<<<<< pot" + pot);
        for (int i = 0; i < 5; i++)
        {
            if (xiToTextList[12 + i].text != "ÚP" || pot == -1)
            {
                xiToTextList[12 + i].gameObject.SetActive(false);
            }

            else
            {
                if(xiToRtfList[1 + i].gameObject.activeSelf)
                {
                    GameObject goj1 = Instantiate(xiToTextList[27 + slotId].transform.parent.gameObject, xiToTextList[27 + slotId].transform.parent.parent, false);
                    RectTransform rtf1 = goj1.GetComponent<RectTransform>();
                    //xiToRtfList[1 + i].gameObject.SetActive(false);
                    DOTween.To(() => rtf1.anchoredPosition, x => rtf1.anchoredPosition = x, xiToRtfList[0].anchoredPosition, 0.5f).OnComplete(() => {
                        Destroy(goj1);
                    });
                    rtf1.DOPivot(xiToRtfList[0].pivot, .5f);
                    rtf1.DOAnchorMax(xiToRtfList[0].anchorMax, .5f);
                    rtf1.DOAnchorMin(xiToRtfList[0].anchorMin, .5f);
                }
            }
        }

        GameObject goj = Instantiate(xiToTextList[27 + slotId].transform.parent.gameObject, xiToTextList[27 + slotId].transform.parent.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        xiToTextList[27 + slotId].transform.parent.gameObject.SetActive(false);
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, xiToRtfList[0].anchoredPosition, 0.5f).OnComplete(()=> {
            if(pot > -1)
            {
                xiToTextList[32].text = App.formatMoney(pot.ToString());
                xiToTextList[32].transform.parent.gameObject.SetActive(true);
            }
            Destroy(goj);
        });
        rtf.DOPivot(xiToRtfList[0].pivot, .5f);
        rtf.DOAnchorMax(xiToRtfList[0].anchorMax, .5f);
        rtf.DOAnchorMin(xiToRtfList[0].anchorMin, .5f);



    }

    private IEnumerator clearChip(int pot, int slotId)
    {
        yield return new WaitForSeconds(1f);
        GameObject goj = Instantiate(xiToTextList[32].transform.parent.gameObject, xiToTextList[32].transform.parent.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        xiToTextList[32].transform.parent.gameObject.SetActive(false);
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, xiToRtfList[1+ slotId].anchoredPosition, 0.5f).OnComplete(()=> {
            xiToTextList[27 + slotId].text = App.formatMoney(pot.ToString());
            xiToRtfList[1 + slotId].gameObject.SetActive(true);
            Destroy(goj);
        });
        rtf.DOPivot(xiToRtfList[1+ slotId].pivot, .5f);
        rtf.DOAnchorMax(xiToRtfList[1+ slotId].anchorMax, .5f);
        rtf.DOAnchorMin(xiToRtfList[1+ slotId].anchorMin, .5f);
    }

    public void sliderChanged()
    {
        xiToTextList[33].text = App.formatMoney(Mathf.FloorToInt(rate * xiToSlider.value).ToString());
    }

    public void sliderChaned2(string type)
    {
        if(type == "cong")
        {
            xiToSlider.value++;
            return;
        }
        xiToSlider.value--;
    }

    public void stateBtnAct(string act)
    {
        OutBounMessage req = null;
        switch (act)
        {
            case "fold":    //úp
                req = new OutBounMessage("FOLD");
                break;
            case "call":    //theo
                req = new OutBounMessage("CALL");
                break;
            case "check":   //xem
                req = new OutBounMessage("CHECK");
                break;
            case "raise":   //tố
                req = new OutBounMessage("RAISE");
                break;
            case "open":    //mở bài
                req = new OutBounMessage("OPEN");
                break;
        }
        if(req != null)
        {
            req.addHead();
            if (act == "raise")
            {
                req.writeInt((int)xiToSlider.value * rate);
                showToPanel(false);
            }

            App.ws.send(req.getReq(), null, true, 0);
        }
    }

    public void showToPanel(bool isShow)
    {
        if(isShow == false)
        {
            xiToGojList[18].SetActive(false);
            return;
        }
        rate = int.Parse(CPlayer.betAmtOfTableToGo);
        int maxValue = Mathf.FloorToInt((availableAmount - maxBetAmount)/rate);
        if (lastBetAmount > maxBetAmount)
            lastBetAmount = maxValue;
        xiToSlider.maxValue = maxValue;
        xiToSlider.value = 1;
        xiToGojList[18].SetActive(true);
        sliderChanged();
    }

    public void openSetting()
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
                App.trace("emo = " + emo);
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
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, xiToImageList[slotIdToShow].overrideSprite, "me");
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
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, xiToImageList[slotIdToShow].overrideSprite, typeShowInfo);
                });
                return;
            }
        }

    }
    #endregion

    private IEnumerator _quit(List<int> winLs, List<int> loseLs)
    {
        if (regQuit == false)
        {
            if (regQuit == false && CPlayer.preScene.Contains("TableList"))
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_TAI_XIU_BET);
                for (int i = 0; i < winLs.Count; i++)
                {
                    Vector2 end = coordinatesList[winLs[i]];
                    for (int j = 0; j < loseLs.Count; j++)
                    {
                        Vector2 start = coordinatesList[loseLs[j]];

                        LoadingControl.instance.flyCoins("line", 10, start, end,0,0,xiToGojList[11].transform);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
            coinFlyed = true;
            isPlaying = false;
            isHaveCards = false; //Bài trong tay mình đã bị xóa
            myCardIds.Clear();
            if (regQuit)
            {
                yield return new WaitForSeconds(.5f);
                backToTableList();
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
            backToTableList();
        }
    }

    #region EXIT TABLE
    private bool coinFlyed = false, isKicked = false;
    public void showNoti()
    {
        if (playerList.Count < 2 || isHaveCards == false || coinFlyed == true || isPlaying == false)
        {
            LoadingControl.instance.delCoins();
            backToTableList();
            return;
        }
        regQuit = !regQuit;
        xiToGojList[19].GetComponentInChildren<Text>().text = !regQuit ? "Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        xiToGojList[20].transform.localScale = new Vector2(regQuit ? -1 : 1, 1);
        xiToGojList[19].SetActive(true);
        StopCoroutine("_showNoti");
        StartCoroutine("_showNoti");
    }

    private IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        xiToGojList[19].SetActive(false);
    }

    public void backToTableList()
    {
        if (isKicked)
        {
            DOTween.PauseAll();
            LoadingControl.instance.delCoins();
            delAllHandle();
            StartCoroutine(openTable());
            return;
        }
        if (isPlaying == false || playerList.Count < 2 || regQuit || isHaveCards == false)
        {
            //LoadingControl.instance.blackPanel.SetActive(true);Cược
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
        CPlayer.preScene = "Xito";
        if (isKicked)
            CPlayer.preScene = "XitoK";
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
    #endregion

    public static string getChiByIds(List<int> arr)
    {
        int cardHigh = 0; // Độ lớn [A 2 .. 10 J Q K]
        int cardType = 0; // Chất của quân bài


        //Xác định bộ 5s
        if (arr.Count == 5)
        {
            int[] ar = new int[5];
            for (int i = 0; i < 5; i++)
            {
                ar[i] = arr[i] % 13;
                if (ar[i] == 0)
                    ar[i] = 13;
            }
            Array.Sort(ar);
            //App.trace(ar[0] + "|" + ar[1] + "|" + ar[2] + "|" + ar[3] + "|" + ar[4]);

            #region //Trường hợp 5 lá liên tiếp đồng chất | THÙNG PHÁ SẢNH
            cardType = (int)Mathf.Floor(arr[0] / 13);
            bool isThung = true;
            for (int i = 1; i < 5; i++)
            {
                if (cardType != (int)Mathf.Floor(arr[i] / 13))
                {
                    isThung = false;
                    break;
                }
            }

            bool isSanh = true;
            for (int i = 0; i < 4; i++)
            {
                if (ar[i + 1] - ar[i] != 1)
                {
                    isSanh = false;
                    break;
                }

            }

            if (ar[0] == 0 && ar[4] == 12)
                isSanh = true;

            if (isThung && isSanh)
            {
                return App.listKeyText["BAND_THUNG_PHA_SANH"]; //"Thùng phá sảnh";

            }

            #endregion

            #region //Trường hợp tứ | TỨ QUÝ
            if ((ar[1] == ar[2] && ar[2] == ar[3] && ar[3] == ar[4] && ar[4] == ar[1]))
                return App.listKeyText["BAND_TU_QUY"];//"Tứ quý";
            if (ar[0] == ar[1] && ar[1] == ar[2] && ar[2] == ar[3] && ar[3] == ar[0])
                return App.listKeyText["BAND_TU_QUY"];//"Tứ quý";
            #endregion

            #region //Trường hợp 1 bộ 3 + 1 bộ đôi | CÙ LŨ
            if (ar[0] == ar[1] && ar[1] == ar[2] && ar[2] == ar[0] && ar[3] == ar[4])
                return App.listKeyText["BAND_CU_LU"];//"Cù lũ";
            if (ar[0] == ar[1] && ar[4] == ar[2] && ar[2] == ar[3] && ar[3] == ar[4])
                return App.listKeyText["BAND_CU_LU"];//"Cù lũ";
            #endregion

            #region //Trường hợp 5 lá đồng chất | THÙNG

            if (isThung)
            {
                return App.listKeyText["BAND_THUNG"]; //"Thùng";
            }
            #endregion

            #region //Trường hợp 5 lá tạo dây | SẢNH
            isSanh = true;
            for (int i = 0; i < 4; i++)
            {
                if (ar[i + 1] - ar[i] != 1)
                {
                    isSanh = false;
                    break;
                }

            }
            if (isSanh)
                return App.listKeyText["BAND_SANH"]; //"Sảnh";
            if (ar[4] == 13 && ar[0] == 1 && ar[1] == 2 && ar[2] == 3 && ar[3] == 4)
            {
                return App.listKeyText["BAND_SANH"];//"Sảnh";
            }
            #endregion

            #region //Trường hợp 1 bộ 3 | SÁM CÔ
            bool isSamCo5 = false;
            for (int i = 1; i < 4; i++)
            {
                if (ar[i] == ar[i + 1] && ar[i - 1] == ar[i])
                {
                    isSamCo5 = true;
                    break;
                }
            }
            if (isSamCo5)
            {
                return App.listKeyText["BAND_SAM_CO"]; //"Sám cô";
            }
            #endregion

            #region //Trường hợp 2 đôi | THÚ
            if (ar[0] == ar[1] && ar[3] == ar[4])
            {
                return App.listKeyText["BAND_THU"];//"Thú";
            }
            else if (ar[1] == ar[2] && ar[3] == ar[4])
            {
                return App.listKeyText["BAND_THU"];//"Thú";
            }
            else if (ar[0] == ar[1] && ar[2] == ar[3])
            {
                return App.listKeyText["BAND_THU"];//"Thú";
            }
            /*
            if ((ar[0] == ar[1] && ar[3] == ar[4]) || (ar[1] == ar[2] && ar[3] == ar[4] || (ar[0] == ar[1] && ar[2] == ar[3])))
                return "Thú";
                */
            #endregion

            #region //Trường hợp đôi | ĐÔI
            int tmp = 0;
            bool isDoi = false;
            for (int i = 0; i < 4; i++)
            {
                if (ar[i] == ar[i + 1])
                {
                    tmp = i;
                    isDoi = true;
                    break;
                }
            }

            if (isDoi)
            {
                return App.listKeyText["BAND_DOI"];//"Đôi";
            }
            return App.listKeyText["BAND_MAU_THAU"];//"Mậu thầu";
            #endregion
        }

        {
            int[] ar = new int[arr.Count];
            for (int i = 0; i < ar.Length; i++)
            {
                ar[i] = arr[i] % 13;
                App.trace("+" + ar[i]);
                if (ar[i] == 0)
                    ar[i] = 13;
            }
            Array.Sort(ar);

            //Tứ
            if (arr.Count == 4 && ar[0] == ar[1] && ar[1] == ar[2] && ar[2] == ar[3] && ar[3] == ar[0])
            {
                return App.listKeyText["BAND_TU_QUY"]; //"Tứ";
            }
            //Bộ 3
            if(arr.Count > 2)
            {
                int countSamCo = 0;
                for(int i = 0; i < ar.Length; i++)
                {
                    if(ar[i] == ar[1])
                    {
                        countSamCo++;
                    }
                }
                if(countSamCo == 3)
                {
                    return App.listKeyText["BAND_SAM_CO"];//"Sám cô";
                }
            }
            //Đôi
            for(int i = 0; i < ar.Length; i++)
            {
                for(int j = i+1; j < ar.Length; j++)
                {
                    if (ar[i] == ar[j])
                    {
                        App.trace(ar[i] +"|" +ar[j]);
                        return App.listKeyText["BAND_DOI"]; //"Đôi";
                    }

                }
            }
        }


        return App.listKeyText["BAND_MAU_THAU"];//"Mậu thầu";
    }

    public void test(int id)
    {

        /*
        if (id == -1)
        {
            moveCard(null, "fold", 0);
            return;
        }
        myCard(new List<int>() { 1}, "divide");
        */
        /*Bay từ chip all ra chip collect
        GameObject goj = Instantiate(xiToTextList[32].transform.parent.gameObject, xiToTextList[32].transform.parent.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, xiToRtfList[id].anchoredPosition, 0.5f);
        rtf.DOPivot(xiToRtfList[id].pivot, .5f);
        rtf.DOAnchorMax(xiToRtfList[id].anchorMax, .5f);
        rtf.DOAnchorMin(xiToRtfList[id].anchorMin, .5f);
        */
        /*Bay chip từ chip collect ra chip all
        GameObject goj = Instantiate(xiToTextList[27 + id].transform.parent.gameObject, xiToTextList[27+ id].transform.parent.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, xiToRtfList[0].anchoredPosition, 0.5f);
        rtf.DOPivot(xiToRtfList[0].pivot, .5f);
        rtf.DOAnchorMax(xiToRtfList[0].anchorMax, .5f);
        rtf.DOAnchorMin(xiToRtfList[0].anchorMin, .5f);
        */
        //myCard(new List<int>() { 1, 2, 3, 4, 5}, "show");
        //moveCard(new List<int>() { 1, 2, 3, 5, 6 }, "show", id);
        // moveCard(new List<int>() { 1, 2, 3, 4, 5 }, "divide",1, false);
        //LoadingControl.instance.flyCoins("line", 10, coordinatesList[0], coordinatesList[id]);
    }
}
