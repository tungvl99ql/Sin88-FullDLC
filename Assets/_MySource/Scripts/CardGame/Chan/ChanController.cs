using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChanController : MonoBehaviour {

    /// <summary>
    /// 0-3: info|4:7: owner|8-11: TimeLeap|12: HelpPanel|13: Trans|14: Noti|15: backBtn
    /// </summary>
    public GameObject[] chanGojList;

    /// <summary>
    /// 0-3: avar|4-7: EatenCard|8-11: FiredCard|12: card000|13-16: EatenCard1
    /// </summary>
    public Image[] chanImageList;

    /// <summary>
    /// 0-3: userName|4-7: balance|8: tableName|9: CountDOwn|10-13: grade|14-17: earnText
    /// </summary>
    public Text[] chanTextList;
    public Button[] stateBtns;
    public Sprite[] cardFaces;

    private bool isPlaying = false;
    private List<String> handelerCommand = new List<string>();
    private IEnumerator preCoroutine;
    private Vector2[] coordinatesList = new Vector2[4];
    private Dictionary<int, List<RectTransform>> firedCardRtfDic = new Dictionary<int, List<RectTransform>>();
    Vector2 vecX60 = new Vector2(60, 0);

    [HideInInspector]
    public static ChanController instance;
    [HideInInspector]
    public bool regQuit = false;

    private void Awake()
    {
        getInstance();

        //===Lấy tọa độ để bay tiền===
        for (int i = 0; i < 4; i++)
        {
            RectTransform rtf = chanTextList[10 + i].gameObject.GetComponent<RectTransform>();
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
        //return;
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

                        string tempString = App.listKeyText["CHAN_NAME"].ToUpper();
                        chanTextList[8].text = tempString + " - " + CPlayer.betAmtOfTableToGo + " " + App.listKeyText["CURRENCY"]; //"CHẴN - " + CPlayer.betAmtOfTableToGo + " Gold";
                        break;
                    }


                }
            });
        }
        else
        {
            string tempString = App.listKeyText["CHAN_NAME"].ToUpper();
            chanTextList[8].text = tempString + " - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " " + App.listKeyText["CURRENCY"]; //"CHẴN - " + App.formatMoney(CPlayer.betAmtOfTableToGo) + " Gold";
        }

        registerHandler();
        getTableDataEx();
    }
    #region GET TABLE DATA EX
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
    private bool[] exitsSlotList = { false, false, false, false, false, false };

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
                    setInfo(player, chanImageList[player.SlotId], chanGojList[player.SlotId], chanTextList[4 + player.SlotId], chanTextList[player.SlotId], chanGojList[4 + player.SlotId]);
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
            chanGojList[8 + preTimeLeapId].SetActive(true);
            preTimeLeapImage = chanGojList[8 + preTimeLeapId].GetComponent<Image>();
            preTimeLeapImage.fillAmount = 1;
            run = true;
        }
        if (mySlotId > -1)
            enterState2(stateById[currentState]);

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
        if (mySlotId > -1)
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

    private long[] chipList = { 0, 0, 0, 0, 0, 0 };
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
            /*
            case "bet":    //Đặt cược
                if (mySlotId != currOwnerId)
                {
                    stateBtns[2].gameObject.SetActive(true);
                    showToPanel(true);
                }
                break;
            case "prepare": //Dằn | Bốc
                stateBtns[0].gameObject.SetActive(true);
                stateBtns[1].gameObject.SetActive(true);
                break;
                */
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

    private void loadBoardData(InBoundMessage res, bool isChiaBai = false)
    {
        int slotCount = res.readByte();
        for (int i = 0; i < slotCount; i++)
        {
            int slotId = res.readByte();
            if (slotId > -1)
                slotId = getSlotIdBySvrId(slotId);
            int lineCount = res.readByte();
            for (int j = 0; j < lineCount; j++)
            {
                int cardLineId = res.readByte() - 1;
                int cardCount = res.readByte();
                if (cardCount < 0)   //
                {

                    List<int> ids = new List<int>();
                    ids = res.readBytes();
                    App.trace(ids.Count + " cards of line " + j + " of slot " + slotId);
                    moveCard("move", ids, 0, slotId, -1, j, isChiaBai);
                }
            }
        }
    }

    #endregion

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

            App.trace("RECV [SET_TURN] slotId = " + slotId + " | turnTimeOut = " + turnTimeOut + " | playerRemainDuration = " + playerRemainDuration);
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
                chanGojList[8 + preTimeLeapId].SetActive(false);
                slotId = getSlotIdBySvrId(slotId);
                preTimeLeapId = slotId;
                time = turnTimeOut;
                curr = playerRemainDuration;
                chanGojList[8 + preTimeLeapId].SetActive(true);
                preTimeLeapImage = chanGojList[8 + preTimeLeapId].GetComponent<Image>();
                preTimeLeapImage.fillAmount = 1;
                run = true;
            }
        });
        #endregion

        #region [GAMEOVER]
        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        handelerCommand.Add("GAMEOVER");
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res) {
            App.trace("RECV [GAMEOVER]");
            run = false;    //K chạy timeLeap nữa
            preTimeLeapImage.gameObject.SetActive(false);   //Ẩn time leap

            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int slotId = res.readByte();
                int grade = res.readByte();
                long earnValue = res.readLong();
                App.trace("slotId = " + slotId + "|grade = " + grade + "|earnValue = " + earnValue);
            }
            string matchResult = res.readString();
            App.trace("mathResult = " + matchResult);
            /*
             if(myGrade == 10) {
				App.current.playSound('draw');
			}
			else if(myGrade == 11)
				App.current.playSound('chess_win');
			else if(myGrade == 12)
				App.current.playSound('chess_lose');
			else if(myGrade == 1 || myGrade > 4)
				App.current.playSound('chess_win');
			else if(myGrade < 0)
				App.current.playSound('completed');
			else
				App.current.playSound('chess_lose');
             */


        });
        #endregion

        #region [SPREAD]
        var req_SPREAD = new OutBounMessage("SPREAD");
        req_SPREAD.addHead();
        handelerCommand.Add("SPREAD");
        App.ws.sendHandler(req_SPREAD.getReq(), delegate (InBoundMessage res_SPREAD)
        {
            int slotId = res_SPREAD.readByte();
            int count = res_SPREAD.readByte();
            for (int i = 0; i < count; i++)
            {
                int id = res_SPREAD.readByte();
                int c = res_SPREAD.readByte();
            }
            string winTitle = res_SPREAD.readString();
            string loseTitle = res_SPREAD.readString();
            App.trace("RECV [SPREAD]  slotId = " + slotId + "|winTitle = " + winTitle + "|loseTitle = " + loseTitle);
            //
        });
        #endregion

        #region [SLOT_IN_TABLE_CHANGED]
        var req_SLOT_IN_TABLE_CHANGED = new OutBounMessage("SLOT_IN_TABLE_CHANGED");
        req_SLOT_IN_TABLE_CHANGED.addHead();
        handelerCommand.Add("SLOT_IN_TABLE_CHANGED");
        App.ws.sendHandler(req_SLOT_IN_TABLE_CHANGED.getReq(), delegate (InBoundMessage res) {
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

                chanImageList[slotId].sprite = cardFaces[31]; //addIcon
                chanImageList[slotId].overrideSprite = cardFaces[31]; //addIcon
                chanImageList[slotId].material = null;
                exitsSlotList[slotId] = false;
                if (slotId != 0)
                {
                    chanGojList[4 + slotId].SetActive(false);   //Xóa owner của thằng thoát
                    chanGojList[slotId].SetActive(false);   //Ẩn info thằng thoát
                    chanTextList[14 + slotId].gameObject.SetActive(false);
                    chanTextList[14 + slotId].gameObject.SetActive(false);
                }
                if (playerList.Count < 2)   //Khi bàn chỉ còn 1 thằng thì k đếm ngược nữa
                {
                    if (preCoroutine != null)
                    {
                        chanTextList[9].gameObject.SetActive(false);
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
                setInfo(player, chanImageList[player.SlotId], chanGojList[player.SlotId], chanTextList[4 + player.SlotId], chanTextList[player.SlotId], chanGojList[4 + player.SlotId]);
                return;
            }
            //Thêm bình thường
            playerList.Add(slotId, player);
            exitsSlotList[slotId] = true;
            //setInfo(player, phomImageList[player.SlotId], phomObjList[player.SlotId], phomTextList[player.SlotId + 4], phomTextList[player.SlotId], phomObjList[player.SlotId + 4]);
            setInfo(player, chanImageList[player.SlotId], chanGojList[player.SlotId], chanTextList[4 + player.SlotId], chanTextList[player.SlotId], chanGojList[4 + player.SlotId]);

        });
        #endregion

        #region [MOVE]
        var req_MOVE = new OutBounMessage("MOVE");
        req_MOVE.addHead();
        handelerCommand.Add("MOVE");
        App.ws.sendHandler(req_MOVE.getReq(), delegate (InBoundMessage res_MOVE)
        {
            List<int> ids = res_MOVE.readBytes();
            //CardUtils.svrIdsToIds(ids);
            SoundManager.instance.PlayUISound(SoundFX.CARD_DROP);
            int sourceSlotId = res_MOVE.readByte();
            int sourceLineId = res_MOVE.readByte() - 1;
            int targetSlotId = res_MOVE.readByte();
            int targetLineId = res_MOVE.readByte() - 1; //2: Nhả|1: Bộ
            int targetIndex = res_MOVE.readByte();

            App.trace(string.Format("RECV [MOVE] IdsCount = {0} | sourceSlotId = {1} | sourceLineId = {2}| targetSlotId = {3}| targetLineId = {4} | targetIndex = {5}| CON DATA =\n",
            ids.Count, sourceSlotId, sourceLineId, targetSlotId, targetLineId, targetIndex));

            //NHẢ
            //RECV [MOVE] IdsCount = 1 | sourceSlotId = 2 | sourceLineId = 0| targetSlotId = 2| targetLineId = 2 | targetIndex = -1| CON DATA =

            if (sourceSlotId == targetSlotId && targetLineId == 2)
            {
                sourceSlotId = getSlotIdBySvrId(sourceSlotId);
                moveCard("move", ids, sourceSlotId, sourceSlotId, 0, 2);
                return;
            }

            //BỐC
            //RECV[MOVE] IdsCount = 1 | sourceSlotId = -1 | sourceLineId = 0 | targetSlotId = 0 | targetLineId = 2 | targetIndex = -1 | CON DATA =
            if(sourceSlotId == -1 && targetLineId == 2)
            {
                targetSlotId = getSlotIdBySvrId(targetSlotId);
                moveCard("move", ids, -1, targetSlotId, 0, 2);
                return;
            }

            //ĂN
            if(sourceSlotId != -1 && sourceSlotId != targetSlotId && targetLineId == 1 && sourceLineId == 2)
            {
                moveCard("eat", ids, sourceLineId, targetLineId, sourceLineId, targetLineId);
                return;
            }

            //NHẢ BỘ
            if (sourceSlotId == targetSlotId && targetLineId == 2)
            {
                moveCard("eat", ids, sourceSlotId, sourceSlotId, 0, 1);
                return;
            }
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
            App.trace("RECV [SHOW_PLAYER_CARD] from slot " + slotId + " cardNum = " + ids.Count);
        });
        #endregion


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
        /*
        if (regQuit)
        {
            backToTableList();
            yield break;
        }*/
        float mTime = timeOut - 2;

        chanTextList[9].text = App.listKeyText["GAME_PREPARE"].ToUpper();//"CHUẨN BỊ";
        chanTextList[9].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        chanTextList[9].gameObject.SetActive(false);
        yield return new WaitForSeconds(.5f);
        while (mTime > 0f)
        {
            chanTextList[9].text = mTime.ToString();
            chanTextList[9].gameObject.SetActive(true);
            yield return new WaitForSeconds(.5f);
            chanTextList[9].gameObject.SetActive(false);
            yield return new WaitForSeconds(.5f);
            mTime -= 1f;
        }

        chanTextList[9].text = App.listKeyText["GAME_START"].ToUpper(); //"BẮT ĐẦU";
        chanTextList[9].gameObject.SetActive(true);
        yield return new WaitForSeconds(.5f);
        chanTextList[9].gameObject.SetActive(false);
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
        chanGojList[14].GetComponentInChildren<Text>().text = !regQuit ? App.listKeyText["GAME_BOARD_EXIT_CANCEL"] : App.listKeyText["GAME_BOARD_EXIT"];   // "Bạn đã hủy đăng ký rời bàn." : "Bạn đã đăng ký rời bàn.";
        chanGojList[15].transform.localScale = new Vector2(regQuit ? -1 : 1, 1);
        chanGojList[14].SetActive(true);
        StopCoroutine("_showNoti");
        StartCoroutine("_showNoti");
    }

    private IEnumerator _showNoti()
    {

        yield return new WaitForSeconds(2f);
        chanGojList[14].SetActive(false);
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
        LoadingControl.instance.delCoins(chanGojList[13].transform);
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

    public void openSetting()
    {
        LoadingControl.instance.openSettingPanel();
    }

    public void showHelpPanel(bool isShow)
    {
        if (isShow)
        {
            chanGojList[12].SetActive(true);
            return;
        }
        chanGojList[12].SetActive(false);
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
            LoadingControl.instance.showPlayerInfo(CPlayer.nickName, (long)CPlayer.chipBalance, (long)CPlayer.manBalance, CPlayer.id, true, chanImageList[slotIdToShow].overrideSprite, "me");
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
                    LoadingControl.instance.showPlayerInfo(pl.NickName, pl.ChipBalance, pl.StarBalance, pl.PlayerId, isCPlayerFriend, chanImageList[slotIdToShow].overrideSprite, typeShowInfo);
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
            chanTextList[4 + mSl].text = mSl == 0 ? App.formatMoney(chipBalance.ToString()) : App.formatMoneyAuto(chipBalance);
        }

    }

    private void moveCard(string act, List<int> ids, int sourceSlotId, int targetSlotId, int sourceLineId, int targetLineId, bool isFly = true)
    {
        #region DIVIDE CARD
        if(act == "move")
        {
            //targetLine: 1-an|2-nha
            int slotId = targetSlotId;
            if (targetLineId == 1)
                targetLineId = 4;
            else if (targetLineId == 2)
            {
                targetLineId = 8;
            }
            else
                targetLineId = 13;
            int childCout = chanImageList[targetLineId + slotId].transform.parent.childCount - 1;
            //int childCount2 = chanImageList[13 + slotId].transform.parent.childCount - 1;

            if (isFly)
            {
                chanGojList[13].SetActive(true);
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(chanImageList[12], chanImageList[12].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                    img.raycastTarget = false;
                    img.gameObject.SetActive(true);


                    if (targetLineId == 4)
                    {
                        rtf.parent = chanImageList[i % 2 == 0 ? targetLineId + slotId : 13 + slotId].transform.parent;
                    }
                    else
                    {
                        rtf.parent = chanImageList[targetLineId + slotId].transform.parent;
                    }

                    rtf.DOScale(.9f, .05f);
                    rtf.DOAnchorMax(Vector2.zero, .25f);
                    rtf.DOAnchorMin(Vector2.zero, .25f);
                    rtf.DOPivot(Vector2.zero, .25f);

                    Vector2 mPos = vecX60 * (i + childCout);

                    if (slotId < 2 && targetLineId == 4 || slotId == 1 || (slotId == 2 && targetLineId == 8))
                    {
                        mPos = new Vector2(-mPos.x, 0);
                        rtf.SetAsFirstSibling();
                    }
                    /*
                    if (firedCards.ContainsKey(slotId))
                        firedCards[slotId].Add(img.gameObject);
                    else
                        firedCards.Add(slotId, new List<GameObject>() { img.gameObject });
                        */
                    /*
                    if (firedCardDic.ContainsKey(slotId))
                    {
                        firedCardDic[slotId].Add(rtf);
                    }
                    else
                    {
                        firedCardDic.Add(slotId, new List<RectTransform>() { rtf });
                    }*/
                    if (targetLineId == 8)
                    {
                        if (firedCardRtfDic.ContainsKey(slotId))
                        {
                            firedCardRtfDic[slotId].Add(rtf);
                        }
                        else
                            firedCardRtfDic.Add(slotId, new List<RectTransform>() { rtf });
                    }
                    rtf.DORotate(new Vector3(0, 90, 0), .25f + tmp * .01f).OnComplete(() =>
                    {
                        try
                        {
                            img.overrideSprite = cardFaces[ids[tmp] % 30];

                        }
                        catch
                        {
                            App.trace("FACK DIU = " + ids[tmp]);
                        }
                        rtf.DORotate(Vector3.zero, .125f);

                    });
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f + +tmp * .05f).OnComplete(() => {
                        if (tmp == ids.Count - 1)
                        {
                            chanGojList[13].SetActive(false);
                        }
                    });
                }

            }
            else
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    int tmp = i;
                    Image img = Instantiate(chanImageList[12], chanImageList[12].transform.parent, false);
                    RectTransform rtf = img.gameObject.GetComponent<RectTransform>();
                    img.raycastTarget = false;

                    if (targetLineId == 4)
                    {
                        rtf.parent = chanImageList[i % 2 == 0 ? targetLineId + slotId : 13 + slotId].transform.parent;
                    }
                    else
                    {
                        rtf.parent = chanImageList[targetLineId + slotId].transform.parent;
                    }
                    rtf.localScale = .9f * Vector2.one;
                    rtf.anchorMax = Vector2.zero;
                    rtf.anchorMin = Vector2.zero;
                    rtf.pivot = Vector2.zero;
                    rtf.DORotate(Vector3.zero, .001f);
                    Vector2 mPos = Vector2.zero;
                    img.gameObject.SetActive(true);
                    if (targetLineId == 8)
                    {
                        mPos = vecX60 * (i + childCout);
                        if (firedCardRtfDic.ContainsKey(slotId))
                        {
                            firedCardRtfDic[slotId].Add(rtf);
                        }
                        else
                            firedCardRtfDic.Add(slotId, new List<RectTransform>() { rtf });
                    }else if(targetLineId == 4)
                    {
                        mPos = vecX60 * (Mathf.FloorToInt(i / 2) + childCout);
                    }

                    if (slotId < 2 && targetLineId == 4 || slotId == 1 || (slotId == 2 && targetLineId == 8))
                    {
                        mPos = new Vector2(-mPos.x, 0);
                        rtf.SetAsFirstSibling();
                    }
                    /*
                    if (firedCards.ContainsKey(slotId))
                        firedCards[slotId].Add(img.gameObject);
                    else
                        firedCards.Add(slotId, new List<GameObject>() { img.gameObject });
                        */
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
                        img.overrideSprite = cardFaces[ids[tmp] % 30];
                        App.trace("FACK DIU = " + ids[tmp]);
                    }
                    catch
                    {
                        App.trace("FACK DIU = " + ids[tmp]);
                    }
                    rtf.anchoredPosition = mPos;
                }
            }
            return;
        }
        #endregion

        #region EAT
        if(act == "eat")
        {
            chanGojList[13].SetActive(false);
            int slotId = sourceSlotId;
            int count = firedCardRtfDic[slotId].Count - 1;
            RectTransform rtf = firedCardRtfDic[slotId][count];
            firedCardRtfDic[slotId].RemoveAt(count);
            slotId = getSlotIdBySvrId(targetSlotId);
            rtf.parent = chanImageList[4 + slotId].transform.parent;

            Vector2 mPos = vecX60 * (chanImageList[targetLineId + slotId].transform.parent.childCount - 1);
            if (slotId < 2 && targetLineId == 4 || slotId == 1 || (slotId == 2 && targetLineId == 8))
            {
                mPos = new Vector2(-mPos.x, 0);
                rtf.SetAsFirstSibling();
            }
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, mPos, .5f).OnComplete(() => {
                chanGojList[13].SetActive(false);
            });
            return;
        }
        #endregion
    }

    public void test(int id)
    {
        int targetLine = id;
        if(id > 10)
        {
            targetLine = id % 10;
            id = id / 10;
        }
        //moveCard(new List<int>() {1}, -1,id,1, targetLine);
    }
}
