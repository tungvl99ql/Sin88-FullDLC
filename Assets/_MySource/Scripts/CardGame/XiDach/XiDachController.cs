using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class XiDachController : MonoBehaviour {

    /// <summary>
    /// 0-5: Info|6-11: Owner|12: Help|13: Trans|14-19: TimeLeap|20: ToPanel|21: ChipToClone|22-27: ChipImage
    /// |28: noti|29: backBtn|
    /// </summary>
    public GameObject[] xiDachGojList;
    /// <summary>
    /// 0-5: avatar|6-11: handCards|12: card000|13-18: timeLeap
    /// </summary>
    public Image[] xiDachImageList;
    /// <summary>
    /// 0-5: userName|6-11: balance|12: tableName|13: countDown|14-19: grade|20-25: earnText|26: sliderText
    /// |27-32: chiText|33-38: cuocText|39: WhiteWin
    /// </summary>
    public Text[] xiDachTextList;
    /// <summary>
    /// 0: Bốc|1: Dằn|2: Cược
    /// </summary>
    public Button[] stateBtns;
    public Sprite[] cardFaces;
    public Slider xiDachSlider;
    /// <summary>
    /// 0: Trắng|1: Vàng|2: Xanh dương|3: Xanh lá|4: đỏ
    /// </summary>
    public Font[] xiDachFontList;
    /// <summary>
    /// 0-5: CượcRtf|
    /// </summary>
    public RectTransform[] xiDachRtfList;

    public Sprite[] tableBackground;

    private Vector2[] coordinatesList = new Vector2[6];
    private List<GameObject> myFiredCards = new List<GameObject>();
    private Dictionary<int, List<GameObject>> firedCards = new Dictionary<int, List<GameObject>>();
    private bool isPlaying = false;
    private int lastBetAmount = 1;


    [HideInInspector]
    public static XiDachController instance;
    [HideInInspector]
    public bool regQuit = false;

    private void Awake()
    {
        getInstance();

        //===Lấy tọa độ để bay tiền===
        for (int i = 0; i < 6; i++)
        {
            RectTransform rtf = xiDachTextList[14 + i].gameObject.GetComponent<RectTransform>();
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



    // Use this for initialization
    void Start () {
        /*
        xiDachTextList[20 ].transform.localPosition -= new Vector3(0, 100, 0);
        xiDachTextList[20 ].gameObject.SetActive(true);
        xiDachTextList[20 ].transform.DOLocalMoveY(xiDachTextList[20].transform.localPosition.y + 100, 1f);
        return;*/

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

        xiDachImageList[19].sprite = tableBackground[bgIndex];

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
                        string tempString = App.listKeyText["XIDACH_NAME"].ToUpper();

                        xiDachTextList[12].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " " + App.listKeyText["CURRENCY"]; //"XÌ DÁCH - " + CPlayer.betAmtOfTableToGo + " Gold";
                        break;
                    }


                }
            });
        }
        else
        {
            string tempString = App.listKeyText["XIDACH_NAME"].ToUpper();
            tempString.Replace("#1", App.formatMoney(CPlayer.betAmtOfTableToGo));
            xiDachTextList[12].text = tempString + " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"]; ;//"XÌ DÁCH - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
        }

        registerHandler();
        getTableDataEx();
    }

    private float time = 1, curr = 0;
    private bool run = false;
    private int currTimeLeapSlotId = -1,currOwnerIntable = -1;
    // Update is called once per frame
    void Update () {
        curr += Time.deltaTime;
        if (run)
        {
            if(currTimeLeapSlotId == -1)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (exitsSlotList[i] && i != currOwnerIntable)
                    {
                        xiDachImageList[13 + i].fillAmount = (time - curr) / time;
                    }
                }
            }
            else
            {
                xiDachImageList[13 + currOwnerIntable].fillAmount = (time - curr) / time;
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
    private bool[] exitsSlotList = { false, false, false, false, false, false};

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
                    setInfo(player, xiDachImageList[player.SlotId], xiDachGojList[player.SlotId], xiDachTextList[6 + player.SlotId], xiDachTextList[player.SlotId], xiDachGojList[6 + player.SlotId]);
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

        if (currTimeOut > 0)
        {

            currOwnerIntable = getSlotIdBySvrId(currOwnerId);
            time = currTimeOut;
            curr = slotRemainDuration;
            if (currentTurnSlotId == -1)
            {
                currTimeLeapSlotId = -1;
                for (int i = 0; i < 6; i++)
                {
                    if (exitsSlotList[i] && i != currOwnerIntable)
                    {
                        xiDachImageList[13 + i].fillAmount = 1;
                        xiDachGojList[14 + i].SetActive(true);
                    }
                }
                xiDachGojList[14 + currOwnerIntable].SetActive(false);
            }
            else
            {
                currTimeLeapSlotId = currOwnerIntable;
                xiDachImageList[13 + currOwnerIntable].fillAmount = 1;
                xiDachGojList[14 + currOwnerIntable].SetActive(true);
                for (int i = 0; i < 6; i++)
                {
                    if (i != currOwnerIntable)
                    {
                        xiDachGojList[14 + i].SetActive(false);
                    }
                }
            }
            if (mySlotId < 0 || isHaveCards == false)
                xiDachGojList[14].SetActive(false);
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
            temp = temp < 0 ? (temp + 6) : temp;
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

    private long[] chipList = { 0, 0, 0, 0, 0, 0};
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
            case "bet":    //Đặt cược
                if(mySlotId != currOwnerId)
                {
                    stateBtns[2].gameObject.SetActive(true);
                    showToPanel(true);
                }
                break;
            case "prepare": //Dằn | Bốc
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[1].gameObject.SetActive(true);
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

    #region  BOARD DATA
    private void loadBoardData(InBoundMessage res, bool isFly = false)
    {
        if (isPlaying == false)
            return;
        App.trace("===LOAD BOARD DATA===");

        int slotCount = res.readByte();
        for (int i = 0; i < slotCount; i++)
        {
            int slotId = res.readByte();
            List<int> ids = res.readBytes();
            string attrIcon = res.readAscii();
            string attrContent = res.readAscii();
            string slotIcon = res.readAscii();
            App.trace("====slotId = " + slotId + "|attrIcon = " + attrIcon + "|attrContent = " + attrContent + "|slotIcon = " + slotIcon);
            CardUtils.svrIdsToIds(ids);
            if(slotId == mySlotId)
            {
                isHaveCards = true;
                myCard(ids, "divide", isFly);
                getBo(xiDachTextList[27], 0, attrIcon, attrContent, slotIcon);
            }
            else
            {
                slotId = getSlotIdBySvrId(slotId);
                moveCard(ids, "divide", slotId, isFly);
                getBo(xiDachTextList[27 + slotId], slotId, attrIcon, attrContent, slotIcon);
            }
        }
        slotCount = res.readByte();
        for(int i = 0; i < slotCount; i++)
        {

            int slotId = res.readByte();
            int amount = res.readInt();
            App.trace("---SLOTID = " + slotId + "|amount = " + amount);
            slotId = getSlotIdBySvrId(slotId);
            collectChip(amount, slotId);
        }
    }
    public class CardUtils
    {
        private static string[] cardTypes = { "bích", "tép", "rô", "cơ","úp", "úp", "úp", "úp", "úp", "úp", "úp" };
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
                        App.trace(cardHigh + "|" + cardType);
                        break;

                }
            }
        }
    }
    #endregion

    #endregion

    private void moveCard(List<int> ids, string act, int slotId, bool fly = true)
    {
        if (ids.Count == 0)
            return;
        if (act == "divide")
        {
            int childCout = xiDachImageList[6 + slotId].transform.parent.childCount - 1;
            Vector2 vec = new Vector2(65, 0);
            if (fly)
            {
                xiDachGojList[13].SetActive(true);
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(xiDachImageList[12], xiDachImageList[12].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                    img.gameObject.SetActive(true);


                    rtf.parent = xiDachImageList[6 + slotId].transform.parent;
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

                    if (firedCards.ContainsKey(slotId))
                        firedCards[slotId].Add(img.gameObject);
                    else
                        firedCards.Add(slotId, new List<GameObject>() { img.gameObject });
                    /*
                    if (firedCardDic.ContainsKey(slotId))
                    {
                        firedCardDic[slotId].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(slotId, new List<RectTransform>() { rtf });
                    }*/
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
                        rtf.DORotate(new Vector3(0, 0, 0), .125f);

                    });
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f + +tmp * .05f).OnComplete(() => {
                        if (tmp == ids.Count - 1)
                        {
                            xiDachGojList[13].SetActive(false);
                        }
                    });
                }
            }
            else
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(xiDachImageList[12], xiDachImageList[12].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();


                    rtf.parent = xiDachImageList[6 + slotId].transform.parent;
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
                    if (firedCards.ContainsKey(slotId))
                        firedCards[slotId].Add(img.gameObject);
                    else
                        firedCards.Add(slotId, new List<GameObject>() { img.gameObject });
                    /*
                    if (firedCardDic.ContainsKey(slotId))
                    {
                        firedCardDic[slotId].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(slotId, new List<RectTransform>() { rtf });
                    }*/
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
        if (act == "show")
        {
            delFiredCards(slotId);

            Vector2 vec = new Vector2(65, 0);
            xiDachGojList[13].SetActive(true);
            for (int i = 0; i < ids.Count; i++)
            {
                int tmp = i;
                Image img = Instantiate(xiDachImageList[6 + slotId], xiDachImageList[6 + slotId].transform.parent, false);
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


                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f + +tmp * .05f).OnComplete(() => {
                    if (firedCards.ContainsKey(slotId))
                        firedCards[slotId].Add(img.gameObject);
                    else
                        firedCards.Add(slotId, new List<GameObject>() { img.gameObject });
                    if (tmp == ids.Count - 1)
                        xiDachGojList[13].SetActive(false);

                });

            }
            return;
        }
    }

    private void myCard(List<int> ids, string act, bool fly = true)
    {
        if (ids.Count == 0)
            return;
        isHaveCards = true;
        if (act == "divide")
        {
            bool isSetFirstBorder = false;
            if (ids.Count > 1)
                isSetFirstBorder = true;
            //Vector2 mPivot = xiToImageList[5].GetComponent<RectTransform>().pivot;
            Vector2 vec = new Vector2(120, 0);
            int childCout = xiDachImageList[6].transform.parent.childCount - 1;
            if (fly)
            {
                xiDachGojList[13].SetActive(true);

                for (int i = 0; i < ids.Count; i++)
                {
                    //myCardIds.Add(ids[i]);
                    int tmp = i;
                    Image img = Instantiate(xiDachImageList[12], xiDachImageList[12].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                    /*
                    if (isSetFirstBorder && tmp == 0)
                       img.transform.GetChild(0).gameObject.SetActive(true);
                       */
                    img.gameObject.SetActive(true);
                    Vector2 mPos = vec * (i + childCout);

                    myFiredCards.Add(img.gameObject);
                    /*
                    if (firedCardDic.ContainsKey(0))
                    {
                        firedCardDic[0].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(0, new List<RectTransform>() { rtf });
                    }
                    */
                    rtf.parent = xiDachImageList[6].transform.parent;
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
                                xiDachGojList[13].SetActive(false);
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
                    //myCardIds.Add(ids[i]);
                    int tmp = i;
                    Image img = Instantiate(xiDachImageList[12], xiDachImageList[12].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();

                    Vector2 mPos = vec * (i + childCout);

                    myFiredCards.Add(img.gameObject);
                    /*
                    if (firedCardDic.ContainsKey(0))
                    {
                        firedCardDic[0].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(0, new List<RectTransform>() { rtf });
                    }
                    */

                    rtf.parent = xiDachImageList[6].transform.parent;
                    rtf.localScale = Vector3.one * 1.5f;
                    rtf.anchorMax = Vector2.zero;
                    rtf.anchorMin = Vector2.zero;
                    rtf.pivot = Vector2.zero;
                    //App.trace("FAC " + ids[tmp]);
                    try
                    {
                        img.overrideSprite = cardFaces[ids[tmp]];
                    }
                    catch
                    {
                        img.overrideSprite = cardFaces[52];
                    }
                    rtf.anchoredPosition = mPos;
                    /*
                    if (isSetFirstBorder && tmp == 0)
                        img.transform.GetChild(0).gameObject.SetActive(true);
                        */
                    img.gameObject.SetActive(true);
                }
            }
            /*
            xiToTextList[22].text = getChiByIds(myCardIds);
            xiToTextList[22].transform.parent.gameObject.SetActive(true);
            */
            return;
        }
        if (act == "show")
        {
            delFiredCards(0);

            Vector2 vec = new Vector2(120, 0);
            xiDachGojList[13].SetActive(true);
            for (int i = 0; i < ids.Count; i++)
            {
                int tmp = i;
                Image img = Instantiate(xiDachImageList[6], xiDachImageList[6].transform.parent, false);
                RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                img.overrideSprite = cardFaces[ids[tmp]];
                img.gameObject.SetActive(true);
                Vector2 mPos = vec * (i);

                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .25f + +tmp * .05f).OnComplete(() => {
                    myFiredCards.Add(img.gameObject);
                    if (tmp == ids.Count - 1)
                        xiDachGojList[13].SetActive(false);
                });

            }
            return;
        }
    }

    private void delFiredCards(int slotId = -1,bool all = false)
    {
        if (slotId > -1)
        {
            if (firedCards.ContainsKey(slotId)){
                foreach (GameObject goj in firedCards[slotId])   //XÓA CÁC QUÂN BÀI CỦA MỘT THẰNG
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
            }
            return;
        }
        foreach (int key in firedCards.Keys.ToList())
        {
            foreach (GameObject goj in firedCards[key])   //XÓA CÁC QUÂN BÀI TRÊN BÀN CHƠI
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
        }
        firedCards.Clear();
        if (all)
        {
            foreach (GameObject goj in myFiredCards)   //XÓA CÁC QUÂN BÀI TRÊN BÀN CHƠI
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
            myFiredCards.Clear();
        }

    }
    private List<string> handelerCommand = new List<string>();  //Lưu các handler đã đăng ký
    private void registerHandler()
    {

        #region [OWNER_CHANGED]
        var req_OWNER_CHANGED = new OutBounMessage("OWNER_CHANGED");
        req_OWNER_CHANGED.addHead();
        handelerCommand.Add("OWNER_CHANGED");
        App.ws.sendHandler(req_OWNER_CHANGED.getReq(), delegate (InBoundMessage res)
        {
            int slotId = res.readByte();
            currOwnerId = slotId;
            slotId = getSlotIdBySvrId(slotId);
            xiDachGojList[6 + slotId].SetActive(false);
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
            if (turnTimeOut == 0)
            {
                run = false;
                for (int i = 0; i < 6; i++)
                {
                    xiDachGojList[14 + i].gameObject.SetActive(false);
                }
                return;
            }

            if (turnTimeOut > 0)
            {
                currOwnerIntable = getSlotIdBySvrId(currOwnerId);
                time = turnTimeOut;
                curr = playerRemainDuration;
                if (slotId == -1)
                {
                    currTimeLeapSlotId = -1;
                    xiDachGojList[14 + currOwnerIntable].SetActive(false);
                    for (int i = 0; i < 6; i++)
                    {
                        if (exitsSlotList[i] && i != currOwnerIntable)
                        {
                            xiDachImageList[13 + i].fillAmount = 1;
                            xiDachGojList[14 + i].SetActive(true);
                        }
                    }

                }else
                {
                    currTimeLeapSlotId = currOwnerIntable;
                    xiDachImageList[13 + currOwnerIntable].fillAmount = 1;
                    xiDachGojList[14 + currOwnerIntable].SetActive(true);
                    for (int i = 0; i < 6; i++)
                    {
                        if (i != currOwnerIntable)
                        {
                            xiDachGojList[14 + i].SetActive(false);
                        }
                    }
                }
                if (mySlotId < 0 || isHaveCards == false)
                    xiDachGojList[14].SetActive(false);
                run = true;
            }
            else
            {
                run = false;

            }
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

                xiDachImageList[slotId].sprite = cardFaces[53]; //addIcon
                xiDachImageList[slotId].overrideSprite = cardFaces[53]; //addIcon
                //xiDachImageList[slotId].material = null;
                exitsSlotList[slotId] = false;
                if (slotId != 0)
                {
                    xiDachGojList[6 + slotId].SetActive(false);   //Xóa owner của thằng thoát
                    xiDachGojList[slotId].SetActive(false);   //Ẩn info thằng thoát
                    xiDachTextList[14 + slotId].gameObject.SetActive(false);
                    xiDachTextList[20 + slotId].gameObject.SetActive(false);
                }
                if (playerList.Count < 2)   //Khi bàn chỉ còn 1 thằng thì k đếm ngược nữa
                {
                    if (preCoroutine != null)
                    {
                        xiDachTextList[13].gameObject.SetActive(false);
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
                setInfo(player, xiDachImageList[player.SlotId], xiDachGojList[player.SlotId], xiDachTextList[6 + player.SlotId], xiDachTextList[player.SlotId], xiDachGojList[6 + player.SlotId]);
                return;
            }
            //Thêm bình thường
            playerList.Add(slotId, player);
            exitsSlotList[slotId] = true;
            //setInfo(player, phomImageList[player.SlotId], phomObjList[player.SlotId], phomTextList[player.SlotId + 4], phomTextList[player.SlotId], phomObjList[player.SlotId + 4]);
            setInfo(player, xiDachImageList[player.SlotId], xiDachGojList[player.SlotId], xiDachTextList[6 + player.SlotId], xiDachTextList[player.SlotId], xiDachGojList[6 + player.SlotId]);

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

        #region [SHOW_PLAYER_CARD]
        var req_SHOW_PLAYER_CARD = new OutBounMessage("SHOW_PLAYER_CARD");
        req_SHOW_PLAYER_CARD.addHead();
        handelerCommand.Add("SHOW_PLAYER_CARD");
        App.ws.sendHandler(req_SHOW_PLAYER_CARD.getReq(), delegate (InBoundMessage res_SHOW_PLAYER_CARD)
        {
            int slotId = res_SHOW_PLAYER_CARD.readByte();
            List<int> ids = res_SHOW_PLAYER_CARD.readBytes();
            string attrIcon = res_SHOW_PLAYER_CARD.readAscii();
            string attrContent = res_SHOW_PLAYER_CARD.readAscii();
            slotId = getSlotIdBySvrId(slotId);

            if (slotId == 0)
            {
                myCard(ids, "show");
            }
            else
            {
                moveCard(ids, "show", slotId);
            }
            getBo(xiDachTextList[27 + slotId], slotId, attrIcon, attrContent);
            App.trace("RCV [SHOW_PLAYER_CARD] slotId = " + slotId + "|attrIcon = " + attrIcon + "|attrContent = " + attrContent);

        });
        #endregion

        #region [START_MATCH]
        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        handelerCommand.Add("START_MATCH");
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res_START_MATCH)
        {
            App.trace("RECV [START_MATCH]");
            SoundManager.instance.PlayUISound(SoundFX.CARD_START);

            isPlaying = true;
            isHaveCards = true;
            delFiredCards(-1,true);
            coinFlyed = false;
            for (int i = 0; i < 6; i++)
            {
                xiDachTextList[27 + i].transform.parent.gameObject.SetActive(false);    //Ẩn bộ
                xiDachTextList[14 + i].gameObject.SetActive(false); //Ẩn grade
                xiDachTextList[33 + i].text = "0";
                xiDachRtfList[i].gameObject.SetActive(false);   //Ẩn cược
                xiDachTextList[20 + i].gameObject.SetActive(false); //Ẩn earnMoney
            }

            loadPlayerMatchPoint(res_START_MATCH);
            loadBoardData(res_START_MATCH, true);
        });
        #endregion

        #region [TAKE_CARD]
        var req_TAKE_CARD = new OutBounMessage("TAKE_CARD");
        req_TAKE_CARD.addHead();
        handelerCommand.Add("TAKE_CARD");
        App.ws.sendHandler(req_TAKE_CARD.getReq(), delegate (InBoundMessage res_TAKE_CARD)
        {
            App.trace("RECV [TAKE_CARD]");
            SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
            int slotId = res_TAKE_CARD.readByte();
            int id = res_TAKE_CARD.readByte();
            if(slotId == mySlotId)
            {
                myCard(new List<int> { id }, "divide");
            }
            else
            {
                slotId = getSlotIdBySvrId(slotId);
                moveCard(new List<int>() { id }, "divide", slotId);
            }
        });
        #endregion

        #region [MOVE]
        var req_MOVE = new OutBounMessage("MOVE");
        req_MOVE.addHead();
        handelerCommand.Add("MOVE");
        App.ws.sendHandler(req_MOVE.getReq(), delegate (InBoundMessage res_MOVE) {
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
            if (targetSlotId != 0)
            {
                moveCard(ids, "divide", targetSlotId);
            }
            else
            {
                myCard(ids, "divide");
            }
        });
        #endregion

        #region [SET_PLAYER_ATTR]
        var req_SET_PLAYER_ATTR = new OutBounMessage("SET_PLAYER_ATTR");
        req_SET_PLAYER_ATTR.addHead();
        handelerCommand.Add("SET_PLAYER_ATTR");
        App.ws.sendHandler(req_SET_PLAYER_ATTR.getReq(), delegate (InBoundMessage res)
        {

            App.trace("RECV [SET_PLAYER_ATTR]");
            int slotId = res.readByte();
            string icon = res.readAscii();
            string content = res.readAscii();
            int action = res.readByte();
            App.trace("????? icon = " + icon + "|action = " + action + "|content" + content);

            if (slotId == mySlotId)
            {
                showToPanel(false);
            }

            slotId = getSlotIdBySvrId(slotId);

            if(icon == "done")
            {
                xiDachTextList[14 + slotId].text = App.listKeyText["GAME_READY"];//"XONG";
                xiDachTextList[14 + slotId].font = xiDachFontList[1];
                xiDachTextList[14 + slotId].transform.localScale = Vector3.one * 5;
                xiDachTextList[14 + slotId].gameObject.SetActive(true);
                xiDachTextList[14 + slotId].transform.DOScale(1, 1f).SetEase(Ease.OutBounce);
                return;
            }
            if(icon == "bet")
            {
                collectChip(int.Parse(content), slotId);
                return;
            }
            getBo(xiDachTextList[27 + slotId], slotId, icon,content);

        });
        #endregion

        #region DIVIDE_CHIP
        var req_DIVIDE_CHIP = new OutBounMessage("DIVIDE_CHIP");
        req_DIVIDE_CHIP.addHead();
        handelerCommand.Add("DIVIDE_CHIP");
        App.ws.sendHandler(req_DIVIDE_CHIP.getReq(), delegate (InBoundMessage res_DIVIDE_CHIP)
        {
            App.trace("RECV [DIVIDE_CHIP]");
            /*
             * 1. Chip sẽ bay từ thằng thua về cái
             * 2. Chip bay từ cái về thằng thắng
             *
             */
            int ownerSlot = res_DIVIDE_CHIP.readByte();
            ownerSlot = getSlotIdBySvrId(ownerSlot);
            int slotCount = res_DIVIDE_CHIP.readByte();
            Dictionary<int, int> dic = new Dictionary<int, int>();
            for(int i = 0; i < slotCount; i++)
            {
                int slotId = res_DIVIDE_CHIP.readByte();
                int amount = res_DIVIDE_CHIP.readInt();
                App.trace("slotId = " + slotId + "amount = " + amount);
                slotId = getSlotIdBySvrId(slotId);
                //App.trace("|||||||||" + App.formatMoneyBack(xiDachTextList[33 + slotId].text).ToString());
                if (amount < 0)
                {
                    StartCoroutine(divideChip(amount, slotId, ownerSlot));
                }
                else if(amount > 0)
                {
                    dic.Add(slotId, amount);
                }
            }
            foreach(int i in dic.Keys.ToList())
            {
                StartCoroutine(divideChip(dic[i], ownerSlot, i,true));
            }


        });
        #endregion

        #region [GAMEOVER]
        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        handelerCommand.Add("GAMEOVER");
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RECV [GAMEOVER]");
            run = false;

            List<int> winLs = new List<int>(), loseLs = new List<int>();

            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int slotId = res.readByte();
                int grade = res.readByte();
                long earnValue = res.readLong();
                App.trace("RS|slotId = " + slotId + "|grade = " + grade + "earnValue = " + earnValue);
                string title = App.listKeyText["GAME_LOSE"]; //"Thua";
                int fontId = 0;
                if (grade == 11)
                {
                    title = App.listKeyText["GAME_WIN"]; // "Thắng";
                    fontId = 1;
                    if (slotId == mySlotId)
                    {
                        SoundManager.instance.PlayUISound(SoundFX.CARD_PHOM_WIN);
                    }

                }
                else
                {
                    if (slotId == mySlotId)
                    {
                        SoundManager.instance.PlayUISound(SoundFX.CARD_LOSE);
                    }
                }

                if (grade == 10)
                {
                    title = App.listKeyText["GAME_DRAW"];  //"Hòa";
                    fontId = 3;
                }
                slotId = getSlotIdBySvrId(slotId);
                int mFontSize = xiDachTextList[20 + slotId].fontSize;
                xiDachTextList[14 + slotId].font = xiDachFontList[fontId];
                xiDachTextList[14 + slotId].text = title;
                xiDachTextList[14 + slotId].gameObject.SetActive(true);

                xiDachTextList[14 + slotId].transform.DOScale(1.2f, 1f).SetLoops(5).OnComplete(() => {
                    xiDachTextList[14 + slotId].transform.localScale = Vector3.one;
                });


                if (earnValue > 0)
                {
                    xiDachTextList[20 + slotId].font = xiDachFontList[1];
                    xiDachTextList[20 + slotId].text = "+" + App.formatMoney(earnValue.ToString());
                    winLs.Add(slotId);
                }
                else if(earnValue < 0)
                {
                    xiDachTextList[20 + slotId].font = xiDachFontList[0];
                    xiDachTextList[20 + slotId].text = "-" + App.formatMoney(Math.Abs(earnValue).ToString());
                    loseLs.Add(slotId);

                }
                else
                {
                    xiDachTextList[20 + slotId].font = xiDachFontList[1];
                    xiDachTextList[20 + slotId].text = "+" + 0.ToString();
                }
                xiDachTextList[20 + slotId].fontSize = mFontSize;
                Transform tf = xiDachTextList[20 + slotId].transform;
                tf.transform.localPosition -= new Vector3(0, 100, 0);
                tf.gameObject.SetActive(true);
                float y = tf.localPosition.y + 100;
                tf.DOLocalMoveY(y, .5f);
            }
            string matchResult = res.readStrings();
            StartCoroutine(_quit(winLs, loseLs));
            StartCoroutine(_ClearCardWhenGameOver());
        });
        #endregion
    }

    IEnumerator _ClearCardWhenGameOver()
    {
        if (isPlaying)
            yield return null;
        yield return new WaitForSeconds(5f);
        delFiredCards(-1, true);
        coinFlyed = false;
        for (int i = 0; i < 6; i++)
        {
            xiDachTextList[27 + i].transform.parent.gameObject.SetActive(false);    //Ẩn bộ
            xiDachTextList[14 + i].gameObject.SetActive(false); //Ẩn grade
            xiDachTextList[33 + i].text = "0";
            xiDachRtfList[i].gameObject.SetActive(false);   //Ẩn cược
            xiDachTextList[20 + i].gameObject.SetActive(false); //Ẩn earnMoney
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

        xiDachTextList[13].text = App.listKeyText["GAME_PREPARE"];//"CHUẨN BỊ";
        xiDachTextList[13].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        xiDachTextList[13].gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            xiDachTextList[13].text = mTime.ToString();
            xiDachTextList[13].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            xiDachTextList[13].gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
        }

        xiDachTextList[13].text = App.listKeyText["GAME_START"]; //"BẮT ĐẦU";
        xiDachTextList[13].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        xiDachTextList[13].gameObject.SetActive(false);
    }

    public void stateBtnAct(string act)
    {
        OutBounMessage req = null;
        switch (act)
        {
            case "hit":    //BỐC
                req = new OutBounMessage("HIT");
                break;
            case "stay":    //DẰN
                req = new OutBounMessage("STAY");
                break;
            case "bet":   //CƯỢC
                req = new OutBounMessage("BET");
                break;
        }
        if (req != null)
        {
            req.addHead();

            if (act == "bet")
            {
                req.writeShort((short)xiDachSlider.value);
                lastBetAmount = (int)xiDachSlider.value;
                showToPanel(false);
                stateBtns[2].gameObject.SetActive(false);
            }

            App.ws.send(req.getReq(), null, true, 0);
        }
    }


    public void showToPanel(bool isShow)
    {
        if (isShow == false)
        {
            xiDachGojList[20].SetActive(false);
            return;
        }

        int maxValue = CPlayer.tableMaxBet;
        if(maxValue == 0)
        {
            maxValue = 50 * int.Parse(CPlayer.betAmtOfTableToGo);
        }
        if (lastBetAmount > CPlayer.chipBalance)
            lastBetAmount = (int)CPlayer.chipBalance;
        if (maxValue > CPlayer.chipBalance)
            maxValue = (int)CPlayer.chipBalance;

        xiDachSlider.maxValue = maxValue;
        xiDachSlider.value = lastBetAmount;
        xiDachGojList[20].SetActive(true);
        sliderChanged();
    }
    public void sliderChanged()
    {
        xiDachTextList[26].text = App.formatMoney(Mathf.FloorToInt(xiDachSlider.value * float.Parse(CPlayer.betAmtOfTableToGo)).ToString());
    }

    public void sliderChaned2(string type)
    {
        if (type == "cong")
        {
            xiDachSlider.value++;
            return;
        }
        xiDachSlider.value--;
    }

    private void getBo(Text txt, int slotId, string icon, string content, string slotIcon = "")
    {
        if(slotIcon == "done")
        {
            xiDachTextList[14 + slotId].text = App.listKeyText["GAME_READY"]; //"XONG";
            xiDachTextList[14 + slotId].font = xiDachFontList[1];
            xiDachTextList[14 + slotId].gameObject.SetActive(true);
            return;
        }
        string t = "";
        int fontId = 0;
        
        switch (icon)
        {

            case "point0":
                string tempString1 = App.listKeyText["XIDACH_PASS_POINT"];
                string new1 = tempString1.Replace("#1", content.ToString());
                t = new1; //"Đủ - " + content + " điểm";
                fontId = 1;
                break;
            case "point-1":
                string tempString2 = App.listKeyText["XIDACH_PASS_POINT"];
                string new2 = tempString2.Replace("#1", content.ToString());
                t = new2;//"Đủ - " + content + " điểm";
                fontId = 1;
                break;
            case "point-2":
                string tempString3 = App.listKeyText["XIDACH_QUAC_POINT"];
                string new3 =  tempString3.Replace("#1", content.ToString());

                t = new3;//"Quắc - " + content + " điểm";
                fontId = 0;
                break;
            case "point-3":
                tempString3 = App.listKeyText["XIDACH_NON_POINT"];
                string new4 = tempString3.Replace("#1", content.ToString());
                t = new4; //"Non - " + content + " điểm";
                fontId = 3;
                break;
            case "point5":
                tempString3 = App.listKeyText["XIDACH_NGULINH"];
                string new5 = tempString3.Replace("#1", content.ToString());
                t = new5;//"Ngũ linh - " + content + " điểm";
                fontId = 2;
                break;
            case "point10":
                t = App.listKeyText["XIDACH_XILAT"];//" Xì lát ";
                fontId = 4;
                break;
            case "point20":
                t = App.listKeyText["XIDACH_XIBANG"];//" Xì bàng ";
                fontId = 4;
                break;
        }
        txt.text = t;
        if(t!= "")
        {
            xiDachTextList[27 + slotId].font = xiDachFontList[fontId];
            xiDachTextList[27 + slotId].text = t;
            xiDachTextList[27 + slotId].transform.parent.gameObject.SetActive(true);
            if (fontId == 4 && slotId == 0)
            {
                xiDachTextList[39].text = t;
                xiDachTextList[39].transform.localScale = Vector2.one * 5f;
                xiDachTextList[39].gameObject.SetActive(true);
                xiDachTextList[39].transform.DOScale(1f,2).SetEase(Ease.OutBounce).OnComplete(()=> {
                    xiDachTextList[39].gameObject.SetActive(false);
                });
            }
        }
    }

    private void collectChip(int pot, int slotId)
    {

        if (pot == 0)
            return;
        GameObject goj = Instantiate(xiDachGojList[21], xiDachGojList[21].transform.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        if (slotId == 0)
            rtf.localScale = Vector2.one * 1.33333f;
        rtf.anchoredPosition = coordinatesList[slotId];
        goj.SetActive(true);
        rtf.parent = xiDachGojList[22 + slotId].transform.parent;
        xiDachTextList[33 + slotId].text = App.formatMoney(pot.ToString());
        xiDachTextList[33 + slotId].transform.parent.gameObject.SetActive(true);
        rtf.DOLocalMove(xiDachGojList[22 + slotId].transform.localPosition, .25f).OnComplete(()=> {
            Destroy(goj);
        });
    }

    private IEnumerator divideChip(int pot, int slotId, int toSlotId, bool isWait = false)
    {
        if (isWait)
            yield return new WaitForSeconds(.75f);

        GameObject goj = Instantiate(xiDachTextList[33 + slotId].transform.parent.gameObject, xiDachTextList[33 + slotId].transform.parent.parent, false);
        RectTransform rtf = goj.GetComponent<RectTransform>();
        goj.GetComponentInChildren<Text>().text = App.formatMoney(Math.Abs(pot).ToString());
        goj.SetActive(true);
        xiDachRtfList[slotId].gameObject.SetActive(false);
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, xiDachRtfList[toSlotId].anchoredPosition, 0.5f).OnComplete(() => {
            long tmp = App.formatMoneyBack(xiDachTextList[33 + toSlotId].text);
            if (pot < 0)
                tmp -= pot;
            else
                tmp = pot * 2;
            xiDachTextList[33 + toSlotId].text = App.formatMoney(tmp.ToString());
            xiDachRtfList[toSlotId].gameObject.SetActive(true);
            Destroy(goj);
        });
        rtf.DOPivot(xiDachRtfList[toSlotId].pivot, .5f);
        rtf.DOAnchorMax(xiDachRtfList[toSlotId].anchorMax, .5f);
        rtf.DOAnchorMin(xiDachRtfList[toSlotId].anchorMin, .5f);
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
            xiDachTextList[6 + mSl].text = mSl == 0 ? App.formatMoney(chipBalance.ToString()) : App.formatMoneyAuto(chipBalance);
        }

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

                        LoadingControl.instance.flyCoins("line", 10, start, end,0,0,xiDachGojList[13].transform);
                    }
                }
            }
            yield return new WaitForSeconds(1.5f);
            coinFlyed = true;
            isPlaying = false;
            isHaveCards = false; //Bài trong tay mình đã bị xóa
            //myCardIds.Clear();
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
    private bool coinFlyed = false, isKicked = false, isHaveCards = false;
    public void showNoti()
    {
        App.trace("isHaveCard = " + isHaveCards + "|coinFlyed = " + coinFlyed + "|isPLaying = " + isPlaying);
        //isHaveCard = True|coinFlyed = True|isPLaying = True
        if (playerList.Count < 2 || isHaveCards == false || coinFlyed == true || isPlaying == false)
        {
            LoadingControl.instance.delCoins();
            backToTableList();
            return;
        }
        regQuit = !regQuit;
        xiDachGojList[28].GetComponentInChildren<Text>().text = !regQuit ? "Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        xiDachGojList[29].transform.localScale = new Vector2(regQuit ? -1 : 1, 1);
        xiDachGojList[28].SetActive(true);
        StopCoroutine("_showNoti");
        StartCoroutine("_showNoti");
    }

    private IEnumerator _showNoti()
    {
        yield return new WaitForSeconds(2f);
        xiDachGojList[28].SetActive(false);
    }

    public void backToTableList()
    {
        if (isKicked)
        {
            DOTween.PauseAll();
            //LoadingControl.instance.delCoins();
            delAllHandle();
            StartCoroutine(openTable());
            return;
        }
        if (isPlaying == false || playerList.Count < 2 || regQuit || isHaveCards == false)
        {
            //LoadingControl.instance.blackPanel.SetActive(true);Cược
            DOTween.PauseAll();
            //LoadingControl.instance.delCoins();
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
        LoadingControl.instance.delCoins(xiDachGojList[13].transform);
        exited = true;
        CPlayer.preScene = "Xito";
        if (isKicked)
            CPlayer.preScene = "XitoK";
        LoadingUIPanel.Show();
        UnityEngine.SceneManagement.SceneManager.LoadScene("TableList");
        yield return new WaitForSeconds(0.05f);
    }

    private void EnterParentPlace(Action callback)
    {
        CPlayer.clientCurrentMode = 0; // modeview
        CPlayer.clientTargetMode = 0;//mode view
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

    public void openSetting()
    {
        LoadingControl.instance.openSettingPanel();
    }

    public void showHelpPanel(bool isShow)
    {
        if (isShow)
        {
            xiDachGojList[12].SetActive(true);
            return;
        }
        xiDachGojList[12].SetActive(false);
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
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, xiDachImageList[slotIdToShow].overrideSprite, "me");
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
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, xiDachImageList[slotIdToShow].overrideSprite, typeShowInfo);
                });
                return;
            }
        }

    }
    #endregion

    public void test(int id)
    {
        //collectChip(3000, id);

        //moveCard(new List<int>() { 1, 2, 4, 5, 7 }, "divide", id);
        //moveCard(new List<int>() { 1, 2, 4, 5, 7 }, "show", id);
        //myCard(new List<int>() { 1, 2, 4, 5, 7 }, id == 0 ? "show" : "divide");

        /*Xếp xong
        xiDachTextList[14 + id].text = "XONG";
        xiDachTextList[14 + id].font = xiDachFontList[1];
        xiDachTextList[14 + id].transform.localScale = Vector3.one * 5;
        xiDachTextList[14 + id].gameObject.SetActive(true);
        xiDachTextList[14 + id].transform.DOScale(1, 1f).SetEase(Ease.OutBounce);
        */

        //StartCoroutine(divideChip(2530, 0, 3));

        xiDachTextList[20 + id].transform.localPosition -= new Vector3(0, 100, 0);
        xiDachTextList[20 + id].gameObject.SetActive(true);
        xiDachTextList[20 + id].transform.DOLocalMoveY(xiDachTextList[20 + id].transform.localPosition.y + 100, 1f);
        //xiDachTextList[20 + id].gameObject.SetActive(false);
    }
}
