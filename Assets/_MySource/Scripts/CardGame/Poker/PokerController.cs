using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PokerController : MonoBehaviour {

    /// <summary>
    /// 0-8: Info|9-17: Owner|18: Help|19: Trans|20-28: TimeLeap|29: FoldCard|30: chipToClone|31: chipAll|32-40: chip
    /// |41: To|42: Noti|43: backBtn
    /// </summary>
    public GameObject[] pokerGojList;
    /// <summary>
    /// 0-8: ava|9-17: handCard|18: Card000|19-27: sbd
    /// </summary>
    public Image[] pokerImageList;
    /// <summary>
    /// 0-8: userName|9-17: balance|18: tableName|19: countDown|20-28: grade|29-37: earn|38-46: chip|47: chipAll
    /// |48: sliderText|49: ChiText|50: MyText
    /// </summary>
    public Text[] pokerTextList;
    /// <summary>
    /// 0-52: face|53: addImage|54: SmallBlind |55: BigBlind|56: Dealer
    /// </summary>
    public Sprite[] cardFaces;
    /// <summary>
    /// 0: To|1: Theo|2: Up||3: Mo|4: Xem|
    /// </summary>
    public Button[] stateBtns;
    /// <summary>
    /// 0: Trắng|1: Vàng|2: Xanh lá|3: xanh biển
    /// </summary>
    public Font[] pokerFontList;
    /// <summary>
    /// 0: MyCard|1-9: chip|10: chipAll
    /// </summary>
    public RectTransform[] pokerRtfList;
    public Slider pokerSlider;

    [HideInInspector]
    public static PokerController instance;
    [HideInInspector]
    public bool regQuit = false;
    /// <summary>
    /// 0-8: grade|9: foldCard
    /// </summary>
    private Vector2[] coordinatesList = new Vector2[12];
    private bool isPlaying = false, isHaveCards = false;
    private Dictionary<int, List<RectTransform>> firedCardDic = new Dictionary<int, List<RectTransform>>();
    private List<GameObject> firedCards = new List<GameObject>();
    private int maxBetAmount = 0, firstRoundBetAmount = 0, rate = 0,availableAmount = 0, lastBetAmount = 0;
    private Dictionary<int, GameObject> midTableCardDic = new Dictionary<int, GameObject>();
    private void Awake()
    {
        getInstance();

        //===Lấy tọa độ để bay tiền===
        for (int i = 0; i < 9; i++)
        {
            RectTransform rtf = pokerTextList[20 + i].gameObject.GetComponent<RectTransform>();
            coordinatesList[i] = new Vector2(30 + rtf.anchoredPosition.x + (rtf.pivot.x * 1600) + (i == 0 ? 50 : 0), rtf.anchoredPosition.y + (rtf.pivot.y * 850) + (i == 0 ? 50 : 0));
        }
        coordinatesList[9] = new Vector2(0, 50);
        coordinatesList[11] = new Vector2(56, -48);
        coordinatesList[10] = new Vector2(-56, -48);
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

    // Use this for initialization
    void Start() {

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
                        string tempString = App.listKeyText["POKER_NAME"].ToUpper();
                        pokerTextList[18].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " " + App.listKeyText["CURRENCY"];//"POKER - " + CPlayer.betAmtOfTableToGo + " Gold";
                        Debug.Log("Text Room1 : " + pokerTextList[18].text);
                        break;
                    }


                }
            });
        }
        else
        {
            string tempString = App.listKeyText["POKER_NAME"].ToUpper();
            pokerTextList[18].text = tempString + " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"]; // "POKER - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
            Debug.Log("Text Room2 : " + pokerTextList[18].text);
        }

        registerHandler();
        getTableDataEx();
    }

    private float time = 1, curr = 0;
    private bool run = false;
    private int preTimeLeapId = -1;
    private Image preTimeLeapImage;
    // Update is called once per frame
    void Update() {
        curr += Time.deltaTime;
        if (run)
        {
            preTimeLeapImage.fillAmount = (time - curr) / time;
            if (time - curr < 1)
            {
                run = false;
                preTimeLeapImage.gameObject.SetActive(false);
            }
        }
    }

    #region //TABLE DATA EX
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

    #region STATE CMD
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
    private bool[] exitsSlotList = { false, false, false, false, false, false, false, false, false };

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
                    setInfo(player, pokerImageList[player.SlotId], pokerGojList[player.SlotId], pokerTextList[9 + player.SlotId], pokerTextList[player.SlotId], pokerGojList[9 + player.SlotId]);
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
            pokerGojList[20 + preTimeLeapId].SetActive(true);
            preTimeLeapImage = pokerGojList[20 + preTimeLeapId].GetComponent<Image>();
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
        if (mySlotId >= -1)
        {
            temp = temp - mySlotId;
            temp = temp < 0 ? (temp + 9) : temp;
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

    private long[] chipList = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    /// <summary>
    /// Set thông tin người chơi
    /// </summary>
    /// <param name="player">PLAYER</param>
    /// <param name="im">AvaImg</param>
    /// <param name="infoObj">InfoGoj</param>
    /// <param name="balanceText">balanceTxt</param>
    /// <param name="nickNamText">nickNameTxt</param>
    /// <param name="ownerImg">ownerGoj</param>
    private void setInfo(Player player, Image im, GameObject infoObj, Text balanceText, Text nickNamText, GameObject ownerImg)
    {
        //im.gameObject.transform.localScale = Vector3.one;
        StartCoroutine(App.loadImg(im, App.getAvatarLink2(player.Avatar, (int)player.PlayerId), player.SvSlotId == mySlotId));
        if (infoObj != null)
            infoObj.SetActive(true);
        chipList[player.SlotId] = player.ChipBalance;
        balanceText.text = "1000.0 K";
        balanceText.text = " " + (player.SlotId == 0 ? App.formatMoney(player.ChipBalance.ToString()) : App.formatMoneyAuto(player.ChipBalance));
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
            case "callTurn":
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[1].gameObject.SetActive(true);
                stateBtns[2].gameObject.SetActive(true);
                break;
            case "checkTurn":
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[4].gameObject.SetActive(true);
                break;
            case "lastTurn":
                stateBtns[3].gameObject.SetActive(true);
                stateBtns[2].gameObject.SetActive(true);
                break;
            case "limitedTurn":
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[1].gameObject.SetActive(true);
                stateBtns[2].gameObject.SetActive(true);
                break;
        }

    }
    #endregion

    #region MATCH POINT
    private void loadPlayerMatchPoint(InBoundMessage res)
    {
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            int point = res.readInt();
        }
    }
    #endregion

    #region BOARDDATA
    private void loadBoardData(InBoundMessage res, bool isFly = false)
    {
        if (isPlaying == false)
            return;
        firstRoundBetAmount = res.readInt();
        //maxBetAmount = firstRoundBetAmount;
        int slotCount = res.readByte();
        int pot = 0;
        for (int i = 0; i < slotCount; i++)
        {
            int slotIdOrg = res.readByte();
            //App.trace("SLOT ID ========= " + slotIdOrg);
            int slotId = getSlotIdBySvrId(slotIdOrg);
            //App.trace("SLOT ID ========= " + slotId);
            int lineCount = res.readByte();
            //App.trace("lineCount = " + lineCount);
            if (lineCount > 0)
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
                        if(slotIdOrg == -1)
                        {
                            moveCard(ids, "divide", -1, isFly);
                            continue;
                        }

                        if (slotId == 0 && cardLineId == 0) //Bài của mình
                        {
                            //isHaveCards = true; //Mình có cầm bài trong tay
                            //myCard(ids, "divide", isFly);  //CHIA BÀI CÙA MÌNH
                            moveCard(ids, "divide", 0, isFly);
                            isHaveCards = true;
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
                        //App.trace("slotId = " + slotId + "CARD COUNT = " + cardCount);
                        /*
                        for(int k = 0; k < cardCount; k++)
                        {

                        }*/
                    }
                }
            if(slotIdOrg > -1)
            {
                    int avail = res.readInt();
                if (slotIdOrg == mySlotId)
                    availableAmount = avail;
                int betAmount = res.readInt();
                int potAmount = res.readInt();
                pot += potAmount;
                if (betAmount > 0 && potAmount == 0)
                {
                    pokerTextList[38 + slotId].text = App.formatMoney(betAmount.ToString());
                    pokerRtfList[1 + slotId].gameObject.SetActive(true);
                }
                if (maxBetAmount < betAmount)
                    maxBetAmount = betAmount;
            }
        }
        /*
        int smallBlindId = res.readByte(); //-1
        int bigBlindId = res.readByte(); //-1
        int dealId = res.readByte(); //-1
        */
        for(int i = 0; i < 3; i++)
        {
            setDBS(res.readByte(), i);
        }
        if(pot > 0)
        {
            //App.trace("POT  = " + pot);
            pokerTextList[47].text = App.formatMoney(pot.ToString());
            pokerRtfList[10].gameObject.SetActive(true);
        }
    }
    #endregion

    #endregion
    private List<string> handelerCommand = new List<string>();  //Lưu các handler đã đăng ký
    private void registerHandler()
    {
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
                pokerGojList[20 + preTimeLeapId].SetActive(false);
                slotId = getSlotIdBySvrId(slotId);
                preTimeLeapId = slotId;
                time = turnTimeOut;
                curr = playerRemainDuration;
                pokerGojList[20 + preTimeLeapId].SetActive(true);
                preTimeLeapImage = pokerGojList[20 + preTimeLeapId].GetComponent<Image>();
                preTimeLeapImage.fillAmount = 1;
                run = true;
            }
            else
            {
                run = false;
                pokerGojList[20 + preTimeLeapId].SetActive(false);
            }

        });
        #endregion

        #region [SLOT_IN_TABLE_CHANGED]
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


            if (nickName == CPlayer.nickName)
            {
                mySlotId = slotId;
                //return;
            }
            App.trace("SLOT_IN_TABLE_CHANGED slot = " + slotId + ",myslot = " + mySlotId + ",nick = " + nickName, "red");
            if (isOwner)
            {
                currOwnerId = slotId;
            }

            if((slotId == mySlotId) && nickName.Equals(""))
            {
                //minh bi out
                App.trace("HIHI", "yellow");
                regQuit = true;
                //backToTableList();
            }

            Player player = new Player(detecSlotIdBySvrId(slotId), slotId, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

            slotId = detecSlotIdBySvrId(slotId);

            if (nickName.Length == 0)    //Có thằng thoát khỏi bàn chơi
            {
                SoundManager.instance.PlayUISound(SoundFX.CARD_EXIT_TABLE);
                playerList.Remove(slotId);
                pokerGojList[slotId].SetActive(false);  //Ẩn info
                pokerImageList[slotId].sprite = cardFaces[53];   //Thay ava bằng +
                pokerImageList[slotId].overrideSprite = cardFaces[53];   //Thay ava bằng +
                pokerImageList[slotId].material = null;
                exitsSlotList[slotId] = false;
                if (slotId != 0)
                {
                    pokerGojList[9 + slotId].SetActive(false);   //Xóa owner của thằng thoát
                    //phomObjList[7 + slotId].SetActive(false);   //Ẩn backCard
                    pokerImageList[19 + slotId].gameObject.SetActive(false);    //Ẩn SBD
                }
                if (playerList.Count < 2)   //Khi bàn chỉ còn 1 thằng thì k đếm ngược nữa
                {
                    if (preCoroutine != null)
                    {
                        pokerTextList[19].gameObject.SetActive(false);
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
                setInfo(player, pokerImageList[player.SlotId], pokerGojList[player.SlotId], pokerTextList[9 + player.SlotId], pokerTextList[player.SlotId], pokerGojList[9 + player.SlotId]);
                return;
            }
            //Thêm bình thường
            playerList.Add(slotId, player);
            exitsSlotList[slotId] = true;
            setInfo(player, pokerImageList[player.SlotId], pokerGojList[player.SlotId], pokerTextList[9 + player.SlotId], pokerTextList[player.SlotId], pokerGojList[9 + player.SlotId]);

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
            /*
            if (slotId == mySlotId)
            {
                xiToTextList[22].transform.parent.gameObject.SetActive(false);
            }*/
            slotId = getSlotIdBySvrId(slotId);
            moveCard(null, "fold", slotId);
            pokerTextList[20 + slotId].text = App.listKeyText["POKER_FOLD"].ToUpper();  // "ÚP";
            pokerTextList[20 + slotId].font = pokerFontList[0];
            pokerTextList[20 + slotId].gameObject.SetActive(true);
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
            App.trace("slotId = " + slotId + "|type = " + type + "|betAmount = " + betAmount +"|maxBet = " + maxBetAmount + "|potAmount = " + potAmount);
            slotId = getSlotIdBySvrId(slotId);
            //if (type != "call")
            //{
                if (betAmount > maxBetAmount)
                {
                    maxBetAmount = betAmount;
                    pokerTextList[20 + slotId].text = App.listKeyText["POKER_RAISE"].ToUpper();//"TỐ";
                pokerTextList[20 + slotId].font = pokerFontList[1];
                }
                else
                {
                    pokerTextList[20 + slotId].text = App.listKeyText["POKER_CALL"].ToUpper(); //"THEO";
                pokerTextList[20 + slotId].font = pokerFontList[3];
                }
                if(type == "call" || type == "raise")
                    pokerTextList[20 + slotId].gameObject.SetActive(true);
            //}
            SoundManager.instance.PlayUISound(SoundFX.CARD_RAISE);
            pokerTextList[38 + slotId].text = App.formatMoney((betAmount - potAmount).ToString());
            pokerTextList[38 + slotId].transform.parent.gameObject.SetActive(true);
            GameObject goj = Instantiate(pokerGojList[30], pokerGojList[30].transform.parent, false);
            RectTransform rtf = goj.GetComponent<RectTransform>();
            rtf.anchoredPosition = coordinatesList[slotId];
            goj.SetActive(true);
            rtf.parent = pokerGojList[32 + slotId].transform.parent;
            pokerGojList[32 + slotId].transform.parent.gameObject.SetActive(true);
            rtf.DOLocalMove(pokerGojList[32 + slotId].transform.localPosition, .25f).OnComplete(()=> {
                Destroy(goj);
            });

            showToPanel(false); //Ẩn panel Tố
        });
        #endregion

        #region [DEAL]
        var req_DEAL = new OutBounMessage("DEAL");
        req_DEAL.addHead();
        handelerCommand.Add("DEAL");
        App.ws.sendHandler(req_DEAL.getReq(), delegate (InBoundMessage res_DEAL)
        {
            App.trace("RECV [DEAL]", "red");
            List<int> ids = res_DEAL.readBytes();
            int pot = res_DEAL.readInt();
            for(int i = 0; i < 9; i++)
            {
                if(pokerRtfList[1 + i].gameObject.activeSelf)
                {
                    collectChip(-1, i);
                }
                else
                {
                    //pokerTextList[20 + i].gameObject.SetActive(false);
                }
            }
            moveCard(ids, "divide", -1);
        });
        #endregion

        #region [START_MATCH]
        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        handelerCommand.Add("START_MATCH");
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res_START_MATCH) {
            App.trace("RECV [START_MATCH]");
            SoundManager.instance.PlayUISound(SoundFX.CARD_START);
            DOTween.KillAll();
            for (int i = 0; i < 9; i++)
            {
                pokerTextList[20 + i].gameObject.SetActive(false);   //Ẩn thứ hạng
                pokerTextList[29 + i].gameObject.SetActive(false);   //Ẩn tiền ăn được
                //xiToTextList[22 + i].transform.parent.gameObject.SetActive(false);  //Ẩn chi
                pokerRtfList[1 + i].gameObject.SetActive(false); //Ẩn chip
                pokerTextList[38 + i].text = "0";   //Đặt lại chip
                pokerImageList[19 + i].gameObject.SetActive(false); //Ẩn SBD trong bàn
            }
            pokerTextList[47].text = "0";   //Đặt lại chip All
            maxBetAmount = 0;   //Đặt lại mức cược lớn nhất

            delFiredCards();
            firedCardDic.Clear();   //Xóa các quân bài úp
            midTableCardDic.Clear();    //Xóa danh sách quân bài giữa bàn
            pokerRtfList[10].gameObject.SetActive(false);  //Ẩn chipAll
            pokerTextList[49].transform.parent.gameObject.SetActive(false);  //Ẩn chi All
            pokerTextList[50].transform.parent.gameObject.SetActive(false);  //Ẩn my chi
            isPlaying = true;
            coinFlyed = false;

            loadPlayerMatchPoint(res_START_MATCH);
            loadBoardData(res_START_MATCH, true);
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
            pokerTextList[20 + slotId].text = App.listKeyText["POKER_CHECK"];//"Xem";
            pokerTextList[20 + slotId].font = pokerFontList[2];
            pokerTextList[20 + slotId].gameObject.SetActive(true);
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
            for (int i = 0; i < slotCount; i++)
            {
                int slotId = res_DIVIDE_CHIP.readByte();
                int betAmount = res_DIVIDE_CHIP.readInt();
                int earnAmount = res_DIVIDE_CHIP.readInt();
                App.trace("slotId = " + slotId + "|betAmount = " + betAmount + "earnAmount = " + earnAmount);
                if (earnAmount > 0)
                {
                    mSlotId = getSlotIdBySvrId(slotId);
                    collectChip(earnAmount + betAmount, getSlotIdBySvrId(slotId));
                }
                else
                {
                    collectChip(-1, getSlotIdBySvrId(slotId));
                }
            }
            int pot = res_DIVIDE_CHIP.readInt();
            StartCoroutine(clearChip(pot, mSlotId));
            //collectChip(pot);
            App.trace("RECV [DIVIDE_CHIP] pot = " + pot);
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
                        SoundManager.instance.PlayEffectSound(SoundFX.CARD_PHOM_WIN);
                        //SoundManager.instance.PlayUISound(SoundFX.CARD_PHOM_WIN);
                        Debug.Log("Win");
                    }
                    else
                    {
                        SoundManager.instance.PlayEffectSound(SoundFX.CARD_LOSE);
                        Debug.Log("Lose");
                       //SoundManager.instance.PlayUISound(SoundFX.CARD_LOSE);
                    }

                }else if(grade == 10)
                {
                    title = App.listKeyText["GAME_DRAW"]; //"Hòa";
                    fontId = 2;
                }


                slotId = getSlotIdBySvrId(slotId);
                pokerTextList[20 + slotId].font = pokerFontList[fontId];
                pokerTextList[20 + slotId].text = title;
                pokerTextList[20 + slotId].gameObject.SetActive(true);

                pokerTextList[20 + slotId].transform.DOScale(1.2f, 1f).SetLoops(5).OnComplete(() => {
                    pokerTextList[20 + slotId].transform.localScale = Vector3.one;
                });


                if (earnValue > 0)
                {
                    pokerTextList[29 + slotId].font = pokerFontList[1];
                    pokerTextList[29 + slotId].text = "+" + App.formatMoney(earnValue.ToString());
                    winLs.Add(slotId);
                }
                else if(earnValue < 0)
                {
                    pokerTextList[29 + slotId].font = pokerFontList[0];
                    pokerTextList[29 + slotId].text = "-" + App.formatMoney(Math.Abs(earnValue).ToString());
                    loseLs.Add(slotId);

                }
                else
                {
                    pokerTextList[29 + slotId].font = pokerFontList[2];
                    pokerTextList[29 + slotId].text = "+" + App.formatMoney(0.ToString());
                }
                pokerTextList[29 + slotId].transform.localPosition -= new Vector3(0, 100, 0);
                pokerTextList[29 + slotId].gameObject.SetActive(true);
                pokerTextList[29 + slotId].transform.DOLocalMoveY(pokerTextList[29 + slotId].transform.localPosition.y + 100, 1f);
            }
            string matchResult = res.readStrings();
            StartCoroutine(_quit(winLs, loseLs));
            StartCoroutine(_ClearCardWhenGameOver());
            pokerImageList[9].gameObject.SetActive(false);
        });
        #endregion

        #region [SHOW_PLAYER_CARD]
        var req_SHOW_PLAYER_CARD = new OutBounMessage("SHOW_PLAYER_CARD");
        req_SHOW_PLAYER_CARD.addHead();
        handelerCommand.Add("SHOW_PLAYER_CARD");
        App.ws.sendHandler(req_SHOW_PLAYER_CARD.getReq(), delegate (InBoundMessage res_SHOW_PLAYER_CARD)
        {
            App.trace("RECV [SHOW_PLAYER_CARD]");
            int bestSlotID = res_SHOW_PLAYER_CARD.readByte();
            string bestBandName = res_SHOW_PLAYER_CARD.readString();
            int slotCount = res_SHOW_PLAYER_CARD.readByte();

            bestSlotID = getSlotIdBySvrId(bestSlotID);
            if(bestBandName != "")
            {
                pokerTextList[49].text = bestBandName;
                pokerTextList[49].transform.parent.gameObject.SetActive(true);
            }
            App.trace("bestSlotId = " + bestSlotID + "|bestBandName = " + bestBandName);

            for (int i = 0; i < slotCount; i++)
            {
                int slotId = res_SHOW_PLAYER_CARD.readByte();
                List<int> ids = res_SHOW_PLAYER_CARD.readBytes();
                List<int> highlightIds = res_SHOW_PLAYER_CARD.readBytes();
                string bandName = res_SHOW_PLAYER_CARD.readString();
                //bool isHighlight = bestSlotID == slotId;
                slotId = getSlotIdBySvrId(slotId);

                moveCard(ids, "show", slotId, true, slotId == bestSlotID ? highlightIds : null);

                if (slotId == 0 && bandName != "")
                {
                    pokerTextList[50].text = bandName;
                    pokerTextList[50].transform.parent.gameObject.SetActive(true);
                }
                App.trace("- slotId = " + slotId + "|bandName = " + bandName);
                CardUtils.svrIdsToIds(highlightIds);
            }

        });
        #endregion

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
    }

    IEnumerator _ClearCardWhenGameOver()
    {
        if (isPlaying)
            yield return null;
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < 9; i++)
        {
            pokerTextList[20 + i].gameObject.SetActive(false);   //Ẩn thứ hạng
            pokerTextList[29 + i].gameObject.SetActive(false);   //Ẩn tiền ăn được
                                                                 //xiToTextList[22 + i].transform.parent.gameObject.SetActive(false);  //Ẩn chi
            pokerRtfList[1 + i].gameObject.SetActive(false); //Ẩn chip
            pokerTextList[38 + i].text = "0";   //Đặt lại chip
            pokerImageList[19 + i].gameObject.SetActive(false); //Ẩn SBD trong bàn
        }
        pokerTextList[47].text = "0";   //Đặt lại chip All
        maxBetAmount = 0;   //Đặt lại mức cược lớn nhất

        delFiredCards();
        firedCardDic.Clear();   //Xóa các quân bài úp
        midTableCardDic.Clear();    //Xóa danh sách quân bài giữa bàn
        pokerRtfList[10].gameObject.SetActive(false);  //Ẩn chipAll
        pokerTextList[49].transform.parent.gameObject.SetActive(false);  //Ẩn chi All
        pokerTextList[50].transform.parent.gameObject.SetActive(false);  //Ẩn my chi
    }


    private IEnumerator preCoroutine = null;
    private IEnumerator _showCountDOwn(int timeOut)
    {
        /*
        if (regQuit)

            backToTableList();
            yield break;
        }*/
        float mTime = timeOut - 2;

        pokerTextList[19].text = App.listKeyText["GAME_PREPARE"].ToUpper(); //"CHUẨN BỊ";
        pokerTextList[19].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        pokerTextList[19].gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            pokerTextList[19].text = mTime.ToString();
            pokerTextList[19].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            pokerTextList[19].gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
        }

        pokerTextList[19].text = App.listKeyText["GAME_START"].ToUpper();//"BẮT ĐẦU";
        pokerTextList[19].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        pokerTextList[19].gameObject.SetActive(false);
    }

    public void openSetting()
    {
        LoadingControl.instance.openSettingPanel();
    }

    public void showHelpPanel(bool isShow)
    {
        if (isShow)
        {
            pokerGojList[18].SetActive(true);
            return;
        }
        pokerGojList[18].SetActive(false);
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
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, pokerImageList[slotIdToShow].overrideSprite, "me");
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
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, pokerImageList[slotIdToShow].overrideSprite, typeShowInfo);
                });
                return;
            }
        }

    }
    #endregion

    public void balanceChanged(int slotId, long chipBalance, long starBalance)
    {
        int mSl = getSlotIdBySvrId(slotId);
        if (mSl > -1)
        {
            //StartCoroutine(_balanceChanged(chipList[mSl], chipBalance, mbTextList[13 + mSl]));
            chipList[mSl] = chipBalance;
            playerList[mSl].ChipBalance = chipBalance;
            playerList[mSl].StarBalance = starBalance;
            pokerTextList[9 + mSl].text = mSl == 0 ? App.formatMoney(chipBalance.ToString()) : App.formatMoneyAuto(chipBalance);
        }

    }

    private void moveCard(List<int> ids, string act, int slotId, bool fly = true,List<int> highlightIds = null) {

        if (act == "fold")
        {
            List<RectTransform> rtfList = firedCardDic[slotId];

            for (int i = 0; i < rtfList.Count; i++)
            {
                RectTransform rtf = rtfList[i];
                rtf.parent = pokerGojList[29].transform;
                rtf.DOAnchorMax(Vector2.one * .5f, .25f);
                rtf.DOAnchorMin(Vector2.one * .5f, .25f);
                rtf.DOPivot(Vector2.one * .5f, .25f);
                if (slotId == 0)
                {
                    rtf.DOScale(1f, .25f);
                }
                rtf.DOScale(.1f, .25f);
                rtf.DORotate(new Vector3(0, 0, UnityEngine.Random.RandomRange(80, 100)), .25f);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, coordinatesList[9], .25f).OnComplete(()=> {
                    rtf.gameObject.SetActive(false);
                });

            }

        }

        #region [DIVIDE]
        if (act == "divide")
        {
            if (slotId == -1)
                slotId = 19;
            //Vector2 mPivot = pokerImageList[9].GetComponent<RectTransform>().pivot;
            int childCout = pokerImageList[9 + slotId].transform.parent.childCount - 1;
            Vector2 vec = new Vector2(65, 0);
            if (slotId == 19)
                vec = vec * 1.2f;
            if (fly)
            {
                pokerGojList[19].SetActive(true);
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(pokerImageList[18], pokerImageList[18].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                    img.gameObject.SetActive(true);

                    Vector2 mPos = vec * (i + childCout);

                    if (slotId!= 0)
                    {
                        rtf.parent = pokerImageList[9 + slotId].transform.parent;
                        rtf.DOScale(1f, .05f);
                        rtf.DOAnchorMax(Vector2.zero, .25f);
                        rtf.DOAnchorMin(Vector2.zero, .25f);
                        rtf.DOPivot(Vector2.zero, .25f);
                    }
                    else
                    {
                        rtf.parent = pokerImageList[9].transform.parent.parent;
                        rtf.DOScale(1.5f, .5f).OnComplete(()=> {
                            rtf.parent = pokerImageList[9].transform.parent;
                        });
                        rtf.DOAnchorMax(pokerRtfList[0].anchorMax, .25f);
                        rtf.DOAnchorMin(pokerRtfList[0].anchorMin, .25f);
                        rtf.DOPivot(pokerRtfList[0].pivot, .25f);
                        if (tmp == 0)
                            mPos = coordinatesList[10];
                        else
                            mPos = coordinatesList[11];
                    }


                    /*if (slotId < 3)
                    {
                        mPos = new Vector2(260 - mPos.x, 0);
                        rtf.SetAsFirstSibling();
                    }*/
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
                        int rotateZ = 0;
                        if(slotId == 0)
                        {
                            if (tmp == 0)
                                rotateZ = 10;
                            else
                                rotateZ = -10;
                        }
                        rtf.DORotate(new Vector3(0, 0, rotateZ), .125f).OnComplete(() => {
                            if (tmp == ids.Count - 1)
                            {
                                pokerGojList[19].SetActive(false);
                            }
                        });

                    });
                    if(slotId == 19)
                    {
                        mPos = Vector2.zero;
                        midTableCardDic.Add(ids[tmp], img.gameObject.transform.GetChild(0).gameObject);
                        //App.trace("====THE FUCK GOING ON" + ids[tmp]);
                    }
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f + tmp * .05f).OnComplete(()=> {
                        if (slotId == 19 && tmp == ids.Count - 1)
                        {
                            int listCardOrglength = Mathf.FloorToInt(firedCardDic[19].Count/2);
                            for (int k = 0; k < firedCardDic[19].Count; k++)
                            {
                                int mpt = k;    //bien tam
                                RectTransform mRtf = firedCardDic[19][mpt];
                                float coorX = (mpt - listCardOrglength) * 130;
                                //.DOMoveX(coor - listCardOrglength * 130 * .5f, .25f);
                                //App.trace("MOVE " + mpt);
                                DOTween.To(() => mRtf.anchoredPosition, x => mRtf.anchoredPosition = x, new Vector2(coorX, 0), .25f + mpt * .05f);
                            }
                        }
                    });
                }
            }
            else
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(pokerImageList[18], pokerImageList[18].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();


                    rtf.parent = pokerImageList[9 + slotId].transform.parent;
                    rtf.localScale = Vector3.one;
                    rtf.anchorMax = Vector2.zero;
                    rtf.anchorMin = Vector2.zero;
                    rtf.pivot = Vector2.zero;
                    Vector2 mPos = vec * (i + childCout);
                    /*if (slotId < 3)
                    {
                        mPos = new Vector2(260 - mPos.x, 0);
                        rtf.SetAsFirstSibling();
                    }*/
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
                    if(slotId == 0)
                    {
                        if (tmp == 0)
                            mPos = coordinatesList[10];
                        else
                            mPos = coordinatesList[11];
                        rtf.DOScale(1.5f, .05f);
                        rtf.DOAnchorMax(pokerRtfList[0].anchorMax, .05f);
                        rtf.DOAnchorMin(pokerRtfList[0].anchorMin, .05f);
                        rtf.DOPivot(pokerRtfList[0].pivot, .05f);
                    }

                    if (slotId == 19)
                    {
                        mPos = Vector2.zero;
                        midTableCardDic.Add(ids[tmp], img.gameObject.transform.GetChild(0).gameObject);
                        //App.trace("====THE FUCK GOING ON" + ids[tmp]);
                    }

                    rtf.anchoredPosition = mPos;
                    int rotateZ = 0;
                    if (slotId == 0)
                    {
                        if (tmp == 0)
                            rotateZ = 10;
                        else
                            rotateZ = -10;
                    }
                    rtf.DORotate(new Vector3(0, 0, rotateZ), .05f);


                    if (highlightIds != null)
                    {
                        if (highlightIds.Contains(ids[tmp]))
                        {
                            img.color = Color.white;
                        }
                        else
                        {
                            img.color = Color.grey;
                        }
                    }

                    img.gameObject.SetActive(true);

                    if (slotId == 19 && tmp == ids.Count - 1)
                    {
                        int listCardOrglength = Mathf.FloorToInt(firedCardDic[19].Count / 2);
                        for (int k = 0; k < firedCardDic[19].Count; k++)
                        {
                            int mpt = k;    //bien tam
                            RectTransform mRtf = firedCardDic[19][mpt];
                            float coorX = (mpt - listCardOrglength) * 130;
                            //.DOMoveX(coor - listCardOrglength * 130 * .5f, .25f);
                            //App.trace("MOVE " + mpt);
                            DOTween.To(() => mRtf.anchoredPosition, x => mRtf.anchoredPosition = x, new Vector2(coorX, 0), .05f);
                        }
                    }
                }
            }
            return;
        }
        #endregion

        #region [SHOW]
        if (act == "show")
        {
            delFiredCards(slotId);
            if (highlightIds != null)
            {
                for (int i = 0; i < highlightIds.Count; i++)
                {
                    if (midTableCardDic.ContainsKey(highlightIds[i]))
                    {

                        midTableCardDic[highlightIds[i]].SetActive(true);
                    }
                }
            }
            /*
            if (slotId == 0)
            {
                moveCard(ids, "divide", 0, false, highlightIds);
                return;
            }*/
            if(highlightIds != null)
            {
                CardUtils.svrIdsToIds(highlightIds);
                App.trace("===WHAT THE FUCK===");
                CardUtils.svrIdsToIds(midTableCardDic.Keys.ToList());
            }
            Vector2 vec = new Vector2(65, 0);
            pokerGojList[19].SetActive(true);
            for (int i = 0; i < ids.Count; i++)
            {
                int tmp = i;
                Image img = Instantiate(pokerImageList[9 + slotId], pokerImageList[9 + slotId].transform.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();

                Vector2 mPos = vec * (i);

                img.overrideSprite = cardFaces[ids[tmp]];


                if (highlightIds != null) {
                    if (highlightIds.Contains(ids[tmp]))
                    {
                        img.color = Color.white;
                    }
                }
                else
                {
                    img.color = Color.grey;
                }


                if (slotId == 0)
                {
                    if (tmp == 0)
                        mPos = coordinatesList[10];
                    else
                        mPos = coordinatesList[11];
                    //rtf.DOScale(1.5f, .05f);
                    rtf.DOAnchorMax(pokerRtfList[0].anchorMax, .05f);
                    rtf.DOAnchorMin(pokerRtfList[0].anchorMin, .05f);
                    rtf.DOPivot(pokerRtfList[0].pivot, .05f);
                    int rotateZ = 0;
                    if (tmp == 0)
                        rotateZ = 10;
                    else
                        rotateZ = -10;
                    rtf.DORotate(new Vector3(0, 0, rotateZ), .05f);
                }

                img.gameObject.SetActive(true);

                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f + +tmp * .05f).OnComplete(() => {
                    firedCards.Add(img.gameObject);
                    if (tmp == ids.Count - 1)
                        pokerGojList[19].SetActive(false);

                });


            }

            return;
        }
        #endregion
    }

    private void myCard(string act, bool isFly = true)
    {
        if(act == "divide")
        {

        }
    }

    private void delFiredCards(int slotId = -10)
    {
        if(slotId!= -10)
        {
            if(firedCardDic.ContainsKey(slotId))
            foreach(RectTransform goj in firedCardDic[slotId])
            {
                try
                {
                    DestroyImmediate(goj.gameObject);
                }
                catch
                {
                    App.trace("DESTROY FIRED CARD FAILD!");
                }
            }
            return;
        }

        foreach (GameObject goj in firedCards)   //XÓA ALL QUÂN BÀI TRÊN BÀN CHƠI
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

    /// <summary>
    /// set Dealer|Big|Small
    /// </summary>
    private void setDBS(int slotId, int type)
    {
        slotId = getSlotIdBySvrId(slotId);
        pokerImageList[19 + slotId].overrideSprite = cardFaces[54 + type];
        pokerImageList[19 + slotId].gameObject.SetActive(true);
    }

    private void collectChip(int pot, int slotId)
    {
        //App.trace("<<<<<<<<<<<< pot" + pot);
        for (int i = 0; i < 9; i++)
        {
            if (pokerTextList[20 + i].text != "ÚP")
            {
                //pokerTextList[20 + i].gameObject.SetActive(false);
            }

            else
            {
                if (pokerRtfList[1 + i].gameObject.activeSelf)
                {
                    GameObject goj1 = Instantiate(pokerTextList[38 + slotId].transform.parent.gameObject, pokerTextList[38 + slotId].transform.parent.parent, false);
                    RectTransform rtf1 = goj1.GetComponent<RectTransform>();
                    //xiToRtfList[1 + i].gameObject.SetActive(false);
                    DOTween.To(() => rtf1.anchoredPosition, x => rtf1.anchoredPosition = x, pokerRtfList[10].anchoredPosition, 0.5f).OnComplete(() => {
                        Destroy(goj1);
                    });
                    rtf1.DOPivot(pokerRtfList[10].pivot, .5f);
                    rtf1.DOAnchorMax(pokerRtfList[10].anchorMax, .5f);
                    rtf1.DOAnchorMin(pokerRtfList[10].anchorMin, .5f);
                }
            }
        }

        GameObject goj = Instantiate(pokerTextList[38 + slotId].transform.parent.gameObject, pokerTextList[38 + slotId].transform.parent.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        pokerTextList[38 + slotId].transform.parent.gameObject.SetActive(false);
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, pokerRtfList[10].anchoredPosition, 0.5f).OnComplete(() => {

            pokerTextList[47].text = App.formatMoney((App.formatMoneyBack(pokerTextList[38 + slotId].text) + App.formatMoneyBack(pokerTextList[47].text)).ToString());
            pokerTextList[47].transform.parent.gameObject.SetActive(true);

            Destroy(goj);
        });
        rtf.DOPivot(pokerRtfList[10].pivot, .5f);
        rtf.DOAnchorMax(pokerRtfList[10].anchorMax, .5f);
        rtf.DOAnchorMin(pokerRtfList[10].anchorMin, .5f);



    }

    private IEnumerator clearChip(int pot, int slotId)
    {
        yield return new WaitForSeconds(1f);
        GameObject goj = Instantiate(pokerGojList[31].transform.parent.gameObject, pokerGojList[31].transform.parent.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        pokerGojList[31].transform.parent.gameObject.SetActive(false);
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, pokerRtfList[1 + slotId].anchoredPosition, 0.5f).OnComplete(() => {
            pokerTextList[38 + slotId].text = App.formatMoney(pot.ToString());
            pokerRtfList[1 + slotId].gameObject.SetActive(true);
            Destroy(goj);
        });
        rtf.DOPivot(pokerRtfList[1 + slotId].pivot, .5f);
        rtf.DOAnchorMax(pokerRtfList[1 + slotId].anchorMax, .5f);
        rtf.DOAnchorMin(pokerRtfList[1 + slotId].anchorMin, .5f);
    }

    public void showToPanel(bool isShow)
    {
        if (isShow == false)
        {
            pokerGojList[41].SetActive(false);
            return;
        }

        rate = int.Parse(CPlayer.betAmtOfTableToGo);
        int maxValue = CPlayer.tableMaxBet * rate;
        if (maxValue == 0)
        {
            maxValue = 10 * int.Parse(CPlayer.betAmtOfTableToGo);
        }
        if (lastBetAmount > CPlayer.chipBalance)
            lastBetAmount = (int)CPlayer.chipBalance;
        if (maxValue > CPlayer.chipBalance)
            maxValue = (int)CPlayer.chipBalance;

        if (lastBetAmount > maxValue)
            lastBetAmount = maxValue;
        pokerSlider.maxValue = maxValue/rate;
        App.trace("MAX VAL = " + maxValue + "|rate = " + rate);
        int val = lastBetAmount / rate;
        if (val == 0)
            val = 1;
        pokerSlider.value = val;
        pokerGojList[41].SetActive(true);
        sliderChanged();
    }

    public void sliderChanged()
    {
        pokerTextList[48].text = App.formatMoney(Mathf.FloorToInt(rate * pokerSlider.value).ToString());
    }

    public void sliderChaned2(string type)
    {
        if (type == "cong")
        {
            pokerSlider.value++;
            return;
        }
        pokerSlider.value--;
    }

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

                        LoadingControl.instance.flyCoins("line", 10, start, end, 0, 0, pokerGojList[19].transform);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
            coinFlyed = true;
            isPlaying = false;
            isHaveCards = false; //Bài trong tay mình đã bị xóa

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
        pokerGojList[42].GetComponentInChildren<Text>().text = !regQuit ? "Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        pokerGojList[43].transform.localScale = new Vector2(regQuit ? -1 : 1, 1);
        pokerGojList[42].SetActive(true);
        StopCoroutine("_showNoti");
        StartCoroutine("_showNoti");
    }

    private IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        pokerGojList[42].SetActive(false);
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
        if (req != null)
        {
            req.addHead();
            if (act == "raise")
            {
                req.writeInt((int)pokerSlider.value * rate);
                showToPanel(false);
                lastBetAmount = (int)pokerSlider.value * rate;
            }

            App.ws.send(req.getReq(), null, true, 0);
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
                        App.trace(cardHigh + cardTypes[cardType]);
                        break;

                }
            }
        }
    }

    public void test(int id)
    {
        /*
        if (id < 10)
            moveCard(new List<int>() {1, 12, 24 }, "divide", id);
        else if (id < 20)
            moveCard(null, "fold", id - 10);
        else
            moveCard(new List<int>() { 12, 24 }, "show", id - 20);
            */
        /*
        if(id == -1)
        {
            moveCard(new List<int>() { 1, 12, 24, 46 }, "divide", id);
        }
        else
        {
            moveCard(new List<int>() { 35}, "divide", -1);
        }*/

        /*
        if(id == -1)
        {
            for (int i = 0; i < 9; i++)
            {
                if (pokerRtfList[1 + i].gameObject.activeSelf)
                {
                    collectChip(20, i);
                }
            }

        }
        GameObject goj = Instantiate(pokerGojList[30], pokerGojList[30].transform.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        rtf.anchoredPosition = coordinatesList[id];
        goj.SetActive(true);
        rtf.parent = pokerGojList[32 + id].transform.parent;
        pokerGojList[32 + id].transform.parent.gameObject.SetActive(true);
        rtf.DOLocalMove(pokerGojList[32 + id].transform.localPosition, .25f);
        */

        moveCard(new List<int>() { 12, 24 }, "show", id);
    }
}
