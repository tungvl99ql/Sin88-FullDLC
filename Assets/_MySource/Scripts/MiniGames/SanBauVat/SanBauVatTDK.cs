using Core.Server.Api;
using Slot.Games.Fish;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SanBauVatTDK : MonoBehaviour
{

    public static SanBauVatTDK instance;

    public TDKInformation tdkDropInfo;
    public SanBauVatGamePlay sanBauVat;
    [SerializeField]
    FishGameControllUI fishGameControllUI;
    [SerializeField]
    SanBauVatGamePlay ControllUI;
    [SerializeField]
    GameObject MiniGamePanel;

    public UnityEvent onDropEndEvent;
    public const string GAME_CODE = "dropball";
    public const string GET_INFO_COMMAND = "DROPBALL.GET_INFO";
    public const string START_COMMAND = "DROPBALL.START";
    public const string SHOWUP_COMMAND = "DROPBALL.SHOWUP";

    private int numFreeSpin;
    public int NumFreeSpin
    {
        get
        {
            return numFreeSpin;
        }
        set
        {
            if (value >= 0)
            {
                numFreeSpin = value;
                //txtNumFree.text = value.ToString();
                ControllUI.ChangeText(ControllUI.txtNumFree, value);
            }
        }
    }
    //data get frome Big Game
    public bool isTrial = false;
    public int currbet = 10000;
    //
    public List<int> listItemRate;
    public bool isSpin=false;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        fishGameControllUI.callBallGame.AddListener(OnCallBallStart);
    }
    private void OnCallBallStart(int numberBallDrop, int currBet, bool isTrial)
    {
        tdkDropInfo = new TDKInformation(isTrial, currBet, numberBallDrop);
        MiniGamePanel.SetActive(true);
        Init();
        InitEvent();
    }
    private void OnEnable()
    {
        //Init();
        InitEvent();
    }
    private void Init()
    {
        isSpin = false;
        SendAndGetInfo();
        if (tdkDropInfo != null)
        {
            NumFreeSpin = tdkDropInfo.numFreeDrop;
            isTrial = tdkDropInfo.isTrial;
            currbet = tdkDropInfo.currBet;
        }
        listItemRate = new List<int>();
    }
    private void InitEvent()
    {
        onDropEndEvent.AddListener(onDropBallEnd);
    }

    private void OnDisable()
    {
        onDropEndEvent.RemoveListener(onDropBallEnd);
    }


    private void Spin()
    {
        sanBauVat.DisableButton();
        sanBauVat.StopAuto();
        SendAndGetStartSpin();
    }

    private void onDropBallEnd()
    {
        isSpin = false;
        if (NumFreeSpin <= 0)
        {
            ControllUI.HandleFreeTurnEqualZero();
            return;
        }
        if (sanBauVat.isAuto)
        {
            sanBauVat.AutoSpin();
            return;
        }
        sanBauVat.AllowSpin();
    }
    private void SendAndGetInfo()
    {
        var joinGameRequest = new OutBounMessage(GET_INFO_COMMAND);
        joinGameRequest.addHead();
        joinGameRequest.writeInt(tdkDropInfo.numFreeDrop);
        App.ws.send(joinGameRequest.getReq(), JoinGameHandler);
    }
    private void JoinGameHandler(InBoundMessage obj)
    {
        int num = obj.readByte();
        for (int i = 0; i < num; i++)
        {
            listItemRate.Add(obj.readInt() * currbet);
        }
        ControllUI.HandleJoinGame(listItemRate);
    }

    public void SendAndGetStartSpin()
    {
        sanBauVat.DisableButton();
        if (NumFreeSpin <= 0)
            return;
        if (isSpin)
            return;
        NumFreeSpin--;
        isSpin = true;
        //Debug.Log("NumFreeSpin => " + NumFreeSpin);
        var startDropBallRequest = new OutBounMessage(START_COMMAND);
        startDropBallRequest.addHead();
        startDropBallRequest.writeInt(currbet);
        startDropBallRequest.writeString(GAME_CODE);
        startDropBallRequest.writeByte(isTrial ? 1 : 0);
        App.ws.send(startDropBallRequest.getReq(), StartSpinHandler);
    }

    private void StartSpinHandler(InBoundMessage obj)
    {
        int totalPrize = obj.readInt();

        int itemRate = obj.readInt();
        //da luu roi khong can nua
        bool isTest = obj.readByte() == 1 ? true : false;
        //  Debug.Log("totalPrize = " + totalPrize);
        //  Debug.Log("itemRate = " + itemRate);
        //  Debug.Log("isTest = " + isTest);
        sanBauVat.ShowResult(totalPrize);
        //sanBauVat.ShowResult(999);

    }
    public void ShowUp()
    {
        sanBauVat.DisableButton();
        var ShowUpRequest = new OutBounMessage(SHOWUP_COMMAND);
        ShowUpRequest.addHead();
        ShowUpRequest.writeInt(fishGameControllUI.numberBallGame);
        ShowUpRequest.writeInt(tdkDropInfo.currBet);
        ShowUpRequest.writeByte(tdkDropInfo.isTrial ? 1 : 0);
        //Debug.Log("Muc Cuoc " + tdkDropInfo.currBet + "XstartMini " + tdkDropInfo.numFreeDrop);
        App.ws.send(ShowUpRequest.getReq(), delegate (InBoundMessage showupResult)
        {
            int totalMoney = showupResult.readInt();
            sanBauVat.ShowUpResult(totalMoney);
           // Debug.Log(totalMoney);
          //  Debug.Log("OK");
          
        });
    }

    public class TDKInformation
    {
        public bool isTrial;
        public int currBet;
        public int numFreeDrop;

        public TDKInformation(bool isTrial, int currBet, int numFreeDrop)
        {
            this.isTrial = isTrial;
            this.currBet = currBet;
            this.numFreeDrop = numFreeDrop;
        }
    }
}