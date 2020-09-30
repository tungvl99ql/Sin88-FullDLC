using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;
using Core.Server.Api;

public class TableList : MonoBehaviour {

    [Header("====TEXT====")]
    public Text userNameText;
    public Text manText, chipText, gameNameFull;

    [Header("=== BUTTON ===")]
    public Button rechargeButton;

    [Header("===IMAGE===")]
    public Image avatar_img;
    public Animator slideSceneAnim;
    [Header("===LIST===")]
    public Text[] betAmtTextList;

    public Button[] betAmtButtonList;
    public static TableList instance;
    public GameObject TabList;
    public GameObject transPanel;
    void Awake()
    {
        Debug.Log(3);

        getInstance();
        Debug.Log(4);

        TabList.SetActive(true);

    }

    void getInstance()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            //App.trace("New");
            instance = this;
            //LoadingControl.instance.blackPanel.SetActive(true);
            //DontDestroyOnLoad(gameObject);
        }

    }
    // Use this for initialization
    void Start() {
       // VuaBaiDepController.instance.canSent = false;
       // VuaBaiDepController.instance.PlayCanSentAnim();
        SoundManager.instance.PlayBackgroundSound("");


        loadData();

        // Comment out
        //LoadingControl.instance.playRandomBgSound();
        //LoadingControl.instance.playBgSound();
        //LoadingControl.instance.audioSource[1].volume = 1f;
        /*
        if (CPlayer.hidePayment || CPlayer.needRetryToLoadPhone)
            rechargeButton.gameObject.SetActive(false);

        if (!CPlayer.phoneNumber.Equals(""))
            rechargeButton.gameObject.SetActive(true);
            */
        var req_GET_CURRENT_PATH = new OutBounMessage("GET_CURRENT_PATH");
        req_GET_CURRENT_PATH.addHead();
        App.ws.send(req_GET_CURRENT_PATH.getReq(), delegate (InBoundMessage res_GET_CURRENT_PATH) {
            string path = "Lobby";
            if (CPlayer.gameName != "") path += "." + CPlayer.gameName;
            var count = res_GET_CURRENT_PATH.readByte();
            for (var i = 0; i < count; i++)
            {
                var type = res_GET_CURRENT_PATH.readAscii();
                var id = res_GET_CURRENT_PATH.readAscii();
                var name = res_GET_CURRENT_PATH.readString();
                App.trace(i + "========" + type + "|" + id + "|" + name);
                if (type == "room")
                {
                    path += "." + id;

                }
                else if (type == "table")
                {
                    path += "." + id;
                }
            }

            var avatar = res_GET_CURRENT_PATH.readAscii();
            var avatarId = res_GET_CURRENT_PATH.readInt();
            var chipBalance = res_GET_CURRENT_PATH.readLong();
            var starBalance = res_GET_CURRENT_PATH.readLong();
            var score = res_GET_CURRENT_PATH.readLong();
            var level = res_GET_CURRENT_PATH.readByte();
        });
        LoadingControl.instance.delCoins();

        if (CPlayer.nickName != null && CPlayer.nickName.Length > 1)
        {
            StartCoroutine(LoadingControl.instance._start());

            gameNameFull.text = CPlayer.gameNameFull;
            string name = CPlayer.fullName.Length == 0 ? App.formatNickName(CPlayer.nickName, 9) : App.formatNickName(CPlayer.fullName, 9);
            userNameText.text = name;
            //userNameText.text = CPlayer.fullName.Length == 0 ? CPlayer.nickName : CPlayer.fullName;

            manText.text = CPlayer.manBalance >= 0 ? App.formatMoney(CPlayer.manBalance.ToString()) + "" : "0";
            chipText.text = CPlayer.chipBalance >= 0 ? App.formatMoney(CPlayer.chipBalance.ToString()) + "" : "0";
            //StartCoroutine(App.loadImg(avatar_img, App.getAvatarLink2(CPlayer.avatar + "", (int)CPlayer.id)));
            if (CPlayer.fakeAva)
                avatar_img.material = null;
            else
                avatar_img.material = LoadingControl.instance.circleMaterial;
            //avatar_img.sprite = CPlayer.avatarSprite;
            avatar_img.sprite = CPlayer.avatarSpriteToSave;
            CPlayer.changed += setBalance;

            if(CPlayer.notEnouChipMess != "")
            {
                //App.showErr(CPlayer.notEnouChipMess);
                LoadingControl.instance.btnDoConfirm.onClick.RemoveAllListeners();

                LoadingControl.instance.btnDoConfirm.onClick.AddListener(() => {
                    LoadingControl.instance.closeConfirmDialog();
                    LoadingControl.instance.showRechargeScene(true);
                });
                CPlayer.notEnouChipMess = "";
                string tempString = App.listKeyText["WARN_NOT_ENOUGH_GOLD_PLAYING"];
                string new1  = tempString.Replace("#1", App.listKeyText["CURRENCY"]);
                LoadingControl.instance.confirmText.text = new1; //"Bạn không đủ Gold để chơi. Vui lòng nạp thêm để chơi tiếp";
                LoadingControl.instance.blackPanel.SetActive(true);
                LoadingControl.instance.confirmDialogAnim.Play("DialogAnim");
            }
            if(CPlayer.statusKick != "")
            {
                App.showErr(CPlayer.statusKick);
                CPlayer.statusKick = "";
            }
        }

	}

    private void setBalance(string type)
    {
        if (type == "man" && manText != null)
            manText.text = CPlayer.manBalance >= 0 ? App.formatMoney(CPlayer.manBalance.ToString()) : "0";
        if (type == "chip" && chipText != null)
        {
            chipText.text = CPlayer.chipBalance >= 0 ? App.formatMoney(CPlayer.chipBalance.ToString()) : "0";

        }

    }

    public void loadData()
    {
        /*=========THAY================
        switch (CPlayer.preScene)
        {
            case "MauBinh":
                //enterchild("0");//vào mức bình dân
                break;
            case "MauBinhK":
                enterchild("0");
                break;
            case "TLMN":
                break;
            case "TLMNK":
                enterchild("0");
                break;
            case "XocDia":
                break;
            case "XocDiaK":
                enterchild("0");
                break;
            default:
                if (CPlayer.preScene != "Table")
                {
                    enterchild(CPlayer.gameName); //vao GAME

                }
                enterchild("0");//vào mức bình dân
                break;
        }
        *=======ENDTHAY============*/
        LoadingControl.instance.hasSendInvite = false;

        getBetAmtList();

        //LoadingControl.instance.loadChatData(LoadingControl.CHANNEL_GENERAL,true);


    }

    public void enterchild(string id)
    {
        //App.trace("CB ENTER_CHILD_PLACE");
        var req_enterChessRoom = new OutBounMessage("ENTER_CHILD_PLACE");
        req_enterChessRoom.addHead();
        req_enterChessRoom.writeAcii(id);
        //CPlayer.roomId = int.Parse(id);
        req_enterChessRoom.writeString("");
        req_enterChessRoom.writeByte(0);
        //req_enterChessRoom.print();



        App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res)
        {
            App.trace("ENTER CHILD PLACE DONE!");

        });

    }

    private List<int> betAmtList;
    public void getBetAmtList()
    {
        /*======THAY=============
        betAmtList = new List<int>();
        var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
        req_getBetAmtList.addHead();

        App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res)
        {

            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                int a = res.readInt();
                //App.trace("BET = " + a);
                betAmtList.Add(a);
                if(betAmtTextList[i]!= null)
                    betAmtTextList[i].text = App.formatMoney(a.ToString());

            }



            //App.trace("GET BET AMT DONE! BETAMT COUNT = " + count);
        });
        ========THAY=========*/
        switch (CPlayer.preScene)
        {
            case "Lobby":
                {
                    var req_enterChessRoom = new OutBounMessage("ENTER_CHILD_PLACE");
                    req_enterChessRoom.addHead();
                    req_enterChessRoom.writeAcii(CPlayer.gameName);  //VÀO GAME
                    req_enterChessRoom.writeString("");
                    req_enterChessRoom.writeByte(0);
                    App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res1)
                    {


                        var req = new OutBounMessage("ENTER_CHILD_PLACE");
                        req.addHead();
                        req.writeAcii("0");  //VÀO KÊNH 0
                        req.writeString("");
                        req.writeByte(0);
                        App.ws.send(req.getReq(), delegate (InBoundMessage res)
                        {
                            betAmtList = new List<int>();
                            var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
                            req_getBetAmtList.addHead();
                            App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res_getBetAmtList)
                            {
                                int count = res_getBetAmtList.readByte();
                                //Debug.Log("count " + count);
                                for (int i = 0; i < count; i++)
                                {
                                    int a = res_getBetAmtList.readInt();
                                   // Debug.Log("a " + a);
                                    betAmtList.Add(a);
                                    if (betAmtTextList[i] != null)
                                        betAmtTextList[i].text = App.formatMoneyK(a);

                                }
                                //Read on/Off
                                for (int i = 0; i < count; i++)
                                {
                                    int state = res_getBetAmtList.readInt();
                                    betAmtButtonList[i].interactable = state == 1 ? true : false;
                                }
                            });

                        });

                    });
                }


                break;
            case "TLMNK":
                {
                    var req = new OutBounMessage("ENTER_CHILD_PLACE");
                    req.addHead();
                    req.writeAcii("0");  //VÀO KÊNH 0
                    req.writeString("");
                    req.writeByte(0);
                    App.ws.send(req.getReq(), delegate (InBoundMessage res)
                    {
                        betAmtList = new List<int>();
                        var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
                        req_getBetAmtList.addHead();
                        App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res_getBetAmtList)
                        {
                            int count = res_getBetAmtList.readByte();
                            for (int i = 0; i < count; i++)
                            {
                                int a = res_getBetAmtList.readInt();
                                betAmtList.Add(a);
                                if (betAmtTextList[i] != null)
                                    betAmtTextList[i].text = App.formatMoneyK(a);
                            }

                            //Read on/Off
                            for (int i = 0; i < count; i++)
                            {
                                int state = res_getBetAmtList.readInt();
                                betAmtButtonList[i].interactable = state == 1 ? true : false;
                            }

                        });

                    });
                }
                break;
            case "MauBinhK":
                {
                    var req = new OutBounMessage("ENTER_CHILD_PLACE");
                    req.addHead();
                    req.writeAcii("0");  //VÀO KÊNH 0
                    req.writeString("");
                    req.writeByte(0);
                    App.ws.send(req.getReq(), delegate (InBoundMessage res)
                    {
                        betAmtList = new List<int>();
                        var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
                        req_getBetAmtList.addHead();
                        App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res_getBetAmtList)
                        {
                            int count = res_getBetAmtList.readByte();
                            for (int i = 0; i < count; i++)
                            {
                                int a = res_getBetAmtList.readInt();
                                betAmtList.Add(a);
                                if (betAmtTextList[i] != null)
                                    betAmtTextList[i].text = App.formatMoneyK(a);
                            }

                            //Read on/Off
                            for (int i = 0; i < count; i++)
                            {
                                int state = res_getBetAmtList.readInt();
                                betAmtButtonList[i].interactable = state == 1 ? true : false;
                            }
                        });

                    });
                }
                break;
            case "XocDiaK":
                {
                    var req = new OutBounMessage("ENTER_CHILD_PLACE");
                    req.addHead();
                    req.writeAcii("0");  //VÀO KÊNH 0
                    req.writeString("");
                    req.writeByte(0);
                    App.ws.send(req.getReq(), delegate (InBoundMessage res)
                    {
                        betAmtList = new List<int>();
                        var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
                        req_getBetAmtList.addHead();
                        App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res_getBetAmtList)
                        {
                            int count = res_getBetAmtList.readByte();
                            for (int i = 0; i < count; i++)
                            {
                                int a = res_getBetAmtList.readInt();
                                betAmtList.Add(a);
                                if (betAmtTextList[i] != null)
                                    betAmtTextList[i].text = App.formatMoneyK(a);
                            }
                            //Read on/Off
                            for (int i = 0; i < count; i++)
                            {
                                int state = res_getBetAmtList.readInt();
                                betAmtButtonList[i].interactable = state == 1 ? true : false;
                            }
                        });

                    });
                }
                break;
            case "MONEY":   //KHÔNG ĐỦ TIỀN

                break;
            case "PhomK":
                {
                    var req = new OutBounMessage("ENTER_CHILD_PLACE");
                    req.addHead();
                    req.writeAcii("0");  //VÀO KÊNH 0
                    req.writeString("");
                    req.writeByte(0);
                    App.ws.send(req.getReq(), delegate (InBoundMessage res)
                    {
                        betAmtList = new List<int>();
                        var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
                        req_getBetAmtList.addHead();
                        App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res_getBetAmtList)
                        {
                            int count = res_getBetAmtList.readByte();
                            for (int i = 0; i < count; i++)
                            {
                                int a = res_getBetAmtList.readInt();
                                betAmtList.Add(a);
                                if (betAmtTextList[i] != null)
                                    betAmtTextList[i].text = App.formatMoneyK(a);
                            }
                            //Read on/Off
                            for (int i = 0; i < count; i++)
                            {
                                int state = res_getBetAmtList.readInt();
                                betAmtButtonList[i].interactable = state == 1 ? true : false;
                            }
                        });

                    });
                }
                break;
            default:
                /*
                {
                    var req = new OutBounMessage("ENTER_CHILD_PLACE");
                    req.addHead();
                    req.writeAcii("0");  //VÀO KÊNH 0
                    req.writeString("");
                    req.writeByte(0);
                    App.ws.send(req.getReq(), delegate (InBoundMessage res)
                    {
                        betAmtList = new List<int>();
                        var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
                        req_getBetAmtList.addHead();
                        App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res_getBetAmtList)
                        {
                            int count = res_getBetAmtList.readByte();
                            for (int i = 0; i < count; i++)
                            {
                                int a = res_getBetAmtList.readInt();
                                betAmtList.Add(a);
                                if (betAmtTextList[i] != null)
                                    betAmtTextList[i].text = App.formatMoney(a.ToString());
                            }
                        });

                    });
                }*/
                {
                    betAmtList = new List<int>();
                    var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
                    req_getBetAmtList.addHead();
                    App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res_getBetAmtList)
                    {
                        int count = res_getBetAmtList.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            int a = res_getBetAmtList.readInt();
                            betAmtList.Add(a);
                            if (betAmtTextList[i] != null)
                                betAmtTextList[i].text = App.formatMoneyK(a);
                        }

                        for (int i = 0; i < count; i++)
                        {
                            int state = res_getBetAmtList.readInt();
                            betAmtButtonList[i].interactable = state == 1 ? true : false;
                        }
                    });
                }
                break;
        }
    }

    #region //LOAD KHUNG CHAT
    /// <summary>
    /// 3 Dòng chat con
    /// </summary>
    public Text[] chat3TextList;
    public void showChatBox()
    {
        LoadingControl.instance.showChatBox(LoadingControl.CHANNEL_GENERAL);

    }

    #endregion

    #region //LẤY THÔNG TIN BÀN CHƠI
    public void getTableData(int bet)
    {
        //VÀO LUÔN | VAO LUON
        //requestEnterPlace("");
        //return;

        // tableAmtToCheck = betAmtList[bet];
        // switch (CPlayer.gameName)
        // {
        //     case "0":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 3)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 3 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 3).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "1":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 8)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 8 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 8).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "xocdia":
        //         if (CPlayer.chipBalance< tableAmtToCheck * 5)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 5 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 5).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "maubinh":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 9)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 9 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 9).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "xito":
        //         if (CPlayer.chipBalance < tableAmtToCheck)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 1 lần cược tương đương " + App.formatMoney((tableAmtToCheck).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "blackjack":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 3)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 3 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 3).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "chan":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 5)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 5 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 5).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "poker":
        //         if (CPlayer.chipBalance < tableAmtToCheck)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 1 lần cược tương đương " + App.formatMoney((tableAmtToCheck).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        // }
        var reqTableList = new OutBounMessage("LIST_ZONE_TABLE");
        reqTableList.addHead();
        reqTableList.writeByte(1); //filter: 1-bàn còn trống | 0-tất cả các bàn
        reqTableList.writeAcii(CPlayer.roomId.ToString()); //gameID

        App.ws.send(reqTableList.getReq(), delegate (InBoundMessage res)
        {
            App.trace("START GET LIST TABLE DONE!");
            int max = 0;
            switch (CPlayer.gameName)
            {
                case "0":   //Phỏm
                    max = 4;
                    break;
                case "1":   //TLMN
                    max = 4;
                    break;
                case "xocdia":
                    max = 7;
                    break;
                case "maubinh":
                    max = 4;
                    break;
                case "xito":
                    max = 5;
                    break;
                case "blackjack":
                    max = 6;
                    break;
                case "chan":
                    max = 4;
                    break;
                case "poker":
                    max = 9;
                    break;
            }

            int count = res.readInt();
            List<Table> arr = new List<Table>();
            for(int i = 0; i < count; i++)
            {
                int tableId = res.readShort();
                string tableName = res.readStrings();
                int tableType = res.readByte();
                int betAmtId = res.readByte();
                //Debug.Log(betAmtId + " - BetAmtID - " + bet + " - Bet") ;
                int slotCount = res.readByte();
                //Debug.Log(slotCount + " - SlotCount - " + max + " - Max");

                bool waiting = res.readByte() == 1;
                bool locked = res.readByte() == 1;
                //App.trace(i + "|tableName = " + tableName + "|slotCOunt = " + slotCount + "|betAmtId = " + betAmtId);
                if (betAmtId == bet && slotCount < max && locked == false)
                {
                    //App.trace("tableType = " + tableType + "|slotCOunt = " + slotCount);
                    //getTableData(new Table(tableId, tableName, tableType, betAmtList[betAmtId], slotCount, waiting, locked));
                    //return;
                    arr.Add(new Table(tableId, tableName, tableType, betAmtList[betAmtId], slotCount,waiting ,locked));
                }
            }
            if(arr.Count > 0)
            {
                int randNum = UnityEngine.Random.Range(0,arr.Count);
                getTableData(arr[randNum]);
                return;
            }

            /*
            LoadingControl.instance.errorText.text = "Mức cược hiện tại chưa có bàn trống. Vui lòng chọn mức cược khác!" + bet;
            LoadingControl.instance.clientDialog.SetActive(true);
            LoadingControl.instance.cliendDialogAnim.Play("DialogAnim");
            LoadingControl.instance.loadingScene.SetActive(false);
            */

            //Không có bàn nào trống thì tạo bàn mới
            // switch (CPlayer.gameName)   //Kiểm tra điều kiện tạo bàn
            // {
            //     case "0":
            //         if (CPlayer.chipBalance < tableAmtToCheck * 3)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 3 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 3).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            //     case "1":
            //         if (CPlayer.chipBalance < tableAmtToCheck * 8)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 8 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 8).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            //     case "xocdia":
            //         if (CPlayer.chipBalance< tableAmtToCheck * 100)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 100 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 100).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            //     case "maubinh":
            //         if (CPlayer.chipBalance < tableAmtToCheck * 9)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 9 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 9).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            //     case "xito":
            //         if (CPlayer.chipBalance < tableAmtToCheck)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 1 lần cược tương đương " + App.formatMoney((tableAmtToCheck).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            //     case "blackjack":
            //         if (CPlayer.chipBalance < tableAmtToCheck * 3)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 3 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 3).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            //     case "chan":
            //         if (CPlayer.chipBalance < tableAmtToCheck * 5)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 5 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 5).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            //     case "poker":
            //         if (CPlayer.chipBalance < tableAmtToCheck)
            //         {
            //             App.showErr("Bạn không đủ Gold (tối thiểu 1 lần cược tương đương " + App.formatMoney((tableAmtToCheck).ToString()) + " Gold) để tham gia bàn chơi");
            //             return;
            //         }
            //         break;
            // }
            var req_createTable = new OutBounMessage("CREATE_RULE");
            req_createTable.addHead();
            req_createTable.writeByte(bet);    //id mức cược
            if (CPlayer.gameName == "xocdia")
            {
                req_createTable.writeByte(1);
                req_createTable.writeAcii("");
                req_createTable.writeString("");
                req_createTable.writeAcii("");
                req_createTable.writeString("");

                App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
                {
                    string tableId = res_createTable.readAscii();
                    //CPlayer.tableToGoId = tableId;
                    //CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();

                    //getTableData(new Table(int.Parse(tableId), "", 0, betAmtList[bet], 1, true, false));
                    if (tableId != "")
                    {
                        if (tableId != "")
                        {
                            CPlayer.tableToGoId = tableId;
                            CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                            _enterTableAfterCreate();
                        }
                    }
                    //getTableData(new Table(int.Parse(tableId), "", 0, betAmtList[bet], 0, true, false));
                    //requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId);
                });
            }
            else if (CPlayer.gameName == "1" || CPlayer.gameName == "maubinh" || CPlayer.gameName == "0")
            {
                req_createTable.writeByte(3);
                req_createTable.writeAcii("formattedName");
                req_createTable.writeString("");
                req_createTable.writeAcii("password");
                req_createTable.writeString("");
                req_createTable.writeAcii("maxNumberOfSlot");
                req_createTable.writeString("4");
            } else if (CPlayer.gameName == "xito")
            {
                req_createTable.writeByte(3);
                req_createTable.writeAcii("formattedName");
                req_createTable.writeString("");
                req_createTable.writeAcii("maxBet");
                req_createTable.writeString(maxBetSliderValue[(int)slider[1].value].ToString());
                req_createTable.writeAcii("password");
                req_createTable.writeString("");

            } else if (CPlayer.gameName == "blackjack")
            {
                req_createTable.writeByte(2);
                req_createTable.writeAcii("formattedName");
                req_createTable.writeString("");
                req_createTable.writeAcii("password");
                req_createTable.writeString("");
            } else if(CPlayer.gameName == "chan")
            {
                req_createTable.writeByte(4);
                req_createTable.writeAcii("formattedName");
                req_createTable.writeString("");
                req_createTable.writeAcii("password");
                req_createTable.writeString("");
                req_createTable.writeAcii("pigType");
                req_createTable.writeString("1");   //Start by 1 of 2
                req_createTable.writeAcii("restriction");
                req_createTable.writeString("0");   //Start by 0 of 2
            } else if (CPlayer.gameName == "poker")
            {
                req_createTable.writeByte(3);
                req_createTable.writeAcii("formattedName");
                req_createTable.writeString("");
                req_createTable.writeAcii("maxBet");
                req_createTable.writeString(maxBetSliderValue[(int)slider[1].value].ToString());
                req_createTable.writeAcii("password");
                req_createTable.writeString("");
            }

            App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
            {
                string tableId = res_createTable.readAscii();
                //CPlayer.tableToGoId = tableId;
                //CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                if (tableId != "")
                {
                    if (tableId != "")
                    {
                        CPlayer.tableToGoId = tableId;
                        CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                        _enterTableAfterCreate();
                    }
                }
                //requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId);
            });
        });
    }
    public class Table
    {
        private int tableId, tableType, tableAmt, slotCount;
        private string tableName;
        private bool locked, waiting;

        public int TableId
        {
            get
            {
                return tableId;
            }

            set
            {
                tableId = value;
            }
        }

        public int TableType
        {
            get
            {
                return tableType;
            }

            set
            {
                tableType = value;
            }
        }

        public int TableAmt
        {
            get
            {
                return tableAmt;
            }

            set
            {
                tableAmt = value;
            }
        }

        public int SlotCount
        {
            get
            {
                return slotCount;
            }

            set
            {
                slotCount = value;
            }
        }

        public string TableName
        {
            get
            {
                return tableName;
            }

            set
            {
                tableName = value;
            }
        }

        public bool Locked
        {
            get
            {
                return locked;
            }

            set
            {
                locked = value;
            }
        }

        public bool Waiting
        {
            get
            {
                return waiting;
            }

            set
            {
                waiting = value;
            }
        }

        public Table(int tableId, string tableName, int tableType, int tableAmt, int slotCount, bool waiting, bool locked)
        {
            this.tableId = tableId;
            this.tableName = tableName;
            this.tableType = tableType;
            this.tableAmt = tableAmt;
            this.slotCount = slotCount;
            this.locked = locked;
            this.waiting = waiting;
        }
    }
    public void getTableData(Table table, string pass = "")
    {

        string path = "Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + table.TableId;
        var req = new OutBounMessage("GET_TABLE_DATA");
        req.addHead();
        req.writeAcii(path);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            string owner = res.readAscii();
            //App.trace("CHU BAN = " + owner);
            int count = res.readByte();
            //App.trace("SO THANG TRONG BAN = " + count);
            for (int i = 0; i < count; i++)
            {
                var playerId = res.readLong();
                var nickName = res.readAscii();
                var avatar = res.readAscii();
                var avatarId = res.readShort();
                var chipBalance = res.readLong();
                var starBalance = res.readLong();
                var score = res.readLong();
                var level = res.readByte();
            }

            count = res.readByte();

            for (int i = 0; i < count; i++)
            {
                var attrName = res.readAscii();
                var attrValue = res.readString();
                App.trace("ATT NAME = " + attrName + "|value = " + attrValue);
                if(attrName == "maxBet")
                {
                    CPlayer.tableMaxBet = int.Parse(attrValue);
                }
            }
            var tableType = res.readByte();
            var betAmtId = res.readByte();

            CPlayer.tableToGoId = table.TableId.ToString();
            CPlayer.betAmtOfTableToGo = table.TableAmt.ToString();
            tableAmtToCheck = table.TableAmt;
            requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId,pass);

            //requestEnterPlace("Lobby.xocdia.0.560");  Xóc đĩa
            //requestEnterPlace("Lobby.1.0.12736", "125"); //TLMN
            //requestEnterPlace("Lobby.maubinh.0.5", ""); //Mau Binh


        });

    }

    #endregion


    private int mode = 1; // 0: Vào xem | 1: Vào chơi
    private int tableAmtToCheck = 0;
    public void requestEnterPlace(string path, string pass = "")
    {
        // VÀO LUÔN BÀN CHƠI| VAO LUON
        //App.trace("PATH = " + path);
        //path = "Lobby.poker.0.11";
        //pass = "125";
        //mode = 0;


        CPlayer.clientTargetMode = mode;
        //roomId + "." + tableId
        var req_enterChessRoom = new OutBounMessage("ENTER_PLACE");
        req_enterChessRoom.addHead();
        req_enterChessRoom.writeAcii(path);
        req_enterChessRoom.writeString(pass); //Mật khẩu của phòng chơi
        req_enterChessRoom.writeByte(mode); // 0: Vào xem | 1: Vào chơi
                                            //req_enterChessRoom.print();

        CPlayer.changed -= setBalance;
        App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res)
        {
            //App.trace("ENTERED TABLE!");
            App.trace("[ENTERED TABLE]");
            //if(res.readByte())
            //if (CPlayer.erroShowing == false)
            //{
            //LoadingControl.instance.blackPanel.SetActive(true);
            // comment out
            //LoadingControl.instance.playRandomBgSound();
            //LoadingControl.instance.playBgSound();
            //LoadingControl.instance.audioSource[1].volume = 0.3f;
            switch (CPlayer.gameName)
            {
                case "0":
                    StartCoroutine(openGaneTable("phom_android_vn", "Phom"));
                    break;
                case "1":
                    //StartCoroutine(openGaneTable("Table"));
                    StartCoroutine(openGaneTable("tlmn_android_vn", "TLMN"));
                    break;
                case "xocdia":
                    StartCoroutine(openGaneTable("xocdia_android_vn", "XocDia"));
                    break;
                case "maubinh":
                    StartCoroutine(openGaneTable("maubinh_android_vn", "MauBinh"));
                    break;
                case "xito":
                    StartCoroutine(openGaneTable("XiTo", "XiTo"));
                    break;
                case "blackjack":
                    StartCoroutine(openGaneTable("XiDach", "XiTo"));
                    break;
                case "chan":
                    StartCoroutine(openGaneTable("Chan", "Chan"));
                    break;
                case "poker":
                    StartCoroutine(openGaneTable("poker_android_vn", "Poker"));
                    break;
            }
            //}
        });


    }

    public void back()
    {
        StartCoroutine(LoadingControl.instance._start());
        EnterParentPlace();
        EnterParentPlace();

        StartCoroutine(openLobby());
        CPlayer.changed -= setBalance;
    }

    private void EnterParentPlace()
    {
        CPlayer.clientCurrentMode = 0; //mode view
        CPlayer.clientTargetMode = 0; //mode view
        //LoadingControl.instance.loadingScene.SetActive(true);
        var req = new OutBounMessage("ENTER_PARENT_PLACE");
        req.addHead();
        req.writeString("");
        req.writeByte(CPlayer.clientCurrentMode);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {

        });
    }

    IEnumerator openLobby()
    {
        /*======THAY===========
        CPlayer.preScene = "TableList";

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("LobbyScene");

        //LobbyControll.instance.LobbyScene.SetActive(true);
        slideSceneAnim.Play("TableListAnimReWind");
        //yield return async;
        Destroy(gameObject, 1f);
        //yield return new WaitForSeconds(0.5f);
        LoadingControl.instance.loadingScene.SetActive(false);
        yield break;
        =========THAY====*/
        //LoadingControl.instance.loadingScene.SetActive(true);
        LoadingUIPanel.Show();
        CPlayer.preScene = "TableList";
        CPlayer.changed -= setBalance;
        SceneManager.LoadScene("Lobby");
        yield return new WaitForSeconds(0.05f);

    }

    private int betIdAfterSib = 0;  //betId
    public void enterTable(int channel)
    {
        /*========THAY================
        if (clickAlow == false || transPanel.activeSelf)
            return;
        curr = 0;
        isShow = true;
        clickAlow = false;
        transPanel.SetActive(true);
        int tmp = (int)Mathf.Floor(channel/10);
        //CPlayer.roomId = tmp;
        betIdAfterSib = 4  * tmp + channel % 10 ;
        App.trace("betIdAfterSib " + betIdAfterSib);
        if (tmp!= CPlayer.roomId)
        {
            CPlayer.roomId = tmp;
            enterSib2(tmp.ToString(), delegate(int a) {
                getTableData(betIdAfterSib);
            });

        }
        else
        {
            getTableData(betIdAfterSib);
        }
        =========THAY======================*/
        if (clickAlow == false || transPanel.activeSelf)
            return;
        curr = 0;
        isShow = true;
        clickAlow = false;
        transPanel.SetActive(true);
        int tmp = (int)Mathf.Floor(channel / 10);
        if (tmp != CPlayer.roomId)
            CPlayer.roomId= tmp;
        betIdAfterSib = 4 * tmp + channel % 10;


        var req = new OutBounMessage("ENTER_SIBLING_PLACE");
        req.addHead();
        req.writeAcii(tmp.ToString());
        req.writeString("");
        CPlayer.clientCurrentMode = 0;
        req.writeByte(0);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            getTableData(betIdAfterSib);
            CPlayer.betAmtId = betIdAfterSib;
        });


    }

    private string pathAfterSib = "";
    private void enterSib(string id, Action<string> callback = null)
    {
        var req = new OutBounMessage("ENTER_SIBLING_PLACE");
        req.addHead();
        req.writeAcii(id);
        req.writeString("");
        CPlayer.clientCurrentMode = 0;
        req.writeByte(0);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            if(callback !=null){
                callback(pathAfterSib);
            }
        });
    }
    private void enterSib2(string id, Action<int> callback = null)
    {
        var req = new OutBounMessage("ENTER_SIBLING_PLACE");
        req.addHead();
        req.writeAcii(id);
        req.writeString("");
        CPlayer.clientCurrentMode = 0;
        req.writeByte(0);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            if (callback != null)
            {
                callback(betIdAfterSib);
            }
        });
    }

    IEnumerator openGaneTable(string gameName, string scenename)
    {
        //LoadingControl.instance.blackkkkkk.SetActive(true);
        LoadingUIPanel.Show();

        CPlayer.preScene = "TableList";
        //SceneManager.LoadScene(gameName,scenename);
        StartCoroutine(_EnterGameByGamename(gameName, scenename));
        yield return new WaitForSecondsRealtime(0.5f);
    }

    public void openPlayerInfo()
    {
        //EnterParentPlace();
        //EnterParentPlace();
        //CPlayer.changed -= setBalance;
        //StartCoroutine(_openPlayerInfo());
       // LoadingControl.instance.ShowPlayerInforPanel();
    }


    private bool isShow = false;
    private float time = 2;
    private float curr = 0;
    private bool clickAlow = true;
    private void Update()
    {
        if(isShow == true)
        {
            curr += Time.deltaTime;
            if(time < curr)
            {
                clickAlow = true;
                isShow = false;
                transPanel.SetActive(false);
            }
        }

    }

    [Header("===TẠO BÀN===")]
    //0: mức cược|1: cược tối đa
    public Slider[] slider;
    public Toggle[] tog;
    public void createTalbe()
    {

        int bet = (int)slider[0].value;
        tableAmtToCheck = betAmtList[bet];
        int tmp = (int)Mathf.Floor(bet / 4);

        //CPlayer.roomId = tmp;

        // switch (CPlayer.gameName)   //Kiểm tra điều kiện tạo bàn
        // {
        //     case "0":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 3)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 3 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 3).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "1":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 8)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 8 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 8).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "xocdia":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 100)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 100 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 100).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "maubinh":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 9)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 9 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 9).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "xito":
        //         if(CPlayer.chipBalance < tableAmtToCheck * 1)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 1 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 1).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "blackjack":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 3)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 1 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 3).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "chan":
        //         if (CPlayer.chipBalance < tableAmtToCheck * 5)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 5 lần cược tương đương " + App.formatMoney((tableAmtToCheck * 5).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        //     case "poker":
        //         if (CPlayer.chipBalance < tableAmtToCheck)
        //         {
        //             App.showErr("Bạn không đủ Gold (tối thiểu 5 lần cược tương đương " + App.formatMoney((tableAmtToCheck).ToString()) + " Gold) để tham gia bàn chơi");
        //             return;
        //         }
        //         break;
        // }

        if (true)
        {

            CPlayer.roomId = tmp;
            var req = new OutBounMessage("ENTER_SIBLING_PLACE");
            req.addHead();
            req.writeAcii(tmp.ToString());
            req.writeString("");
            CPlayer.clientCurrentMode = 0;
            req.writeByte(0);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                //App.trace("EN ROOIF");
                var req_createTable = new OutBounMessage("CREATE_RULE");
                req_createTable.addHead();
                req_createTable.writeByte(bet);    //id mức cược
                if (CPlayer.gameName == "xocdia")
                {
                    req_createTable.writeByte(1);
                    req_createTable.writeAcii("");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("");
                    req_createTable.writeString("");

                    App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
                    {
                        App.trace("RECV [CREATE_RULE]");
                        string tableId = res_createTable.readAscii();
                        //CPlayer.tableToGoId = tableId;
                        //CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                        //App.trace("RECV [CREATE_RULE] tableId = " + tableId);
                        //getTableData(new Table(int.Parse(tableId), "", 0, betAmtList[bet], 1, true, false));
                        if (tableId != "")
                        {
                            CPlayer.tableToGoId = tableId;
                            CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                            _enterTableAfterCreate();
                        }

                    });
                }
                else if (CPlayer.gameName == "maubinh" || CPlayer.gameName == "0")
                {
                    req_createTable.writeByte(3);
                    req_createTable.writeAcii("formattedName");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("password");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("maxNumberOfSlot");
                    req_createTable.writeString("4");
                }
                else if (CPlayer.gameName == "1")    //Có để Solo | Truyền thống
                {
                    req_createTable.writeByte(3);
                    req_createTable.writeAcii("formattedName");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("password");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("maxNumberOfSlot");
                    if (tog[0].isOn)
                        req_createTable.writeString("4");
                    else
                        req_createTable.writeString("2");
                }else if(CPlayer.gameName == "xito")
                {
                    req_createTable.writeByte(3);
                    req_createTable.writeAcii("formattedName");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("maxBet");
                    req_createTable.writeString(maxBetSliderValue[(int)slider[1].value].ToString());
                    req_createTable.writeAcii("password");
                    req_createTable.writeString("");
                }else if(CPlayer.gameName == "blackjack")
                {
                    req_createTable.writeByte(2);
                    req_createTable.writeAcii("formattedName");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("password");
                    req_createTable.writeString("");

                }else if(CPlayer.gameName == "poker")
                {
                    CPlayer.tableMaxBet = maxBetSliderValue[(int)slider[1].value];
                    req_createTable.writeByte(3);
                    req_createTable.writeAcii("formattedName");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("maxBet");
                    req_createTable.writeString(CPlayer.tableMaxBet.ToString());
                    req_createTable.writeAcii("password");
                    req_createTable.writeString("");
                }

                App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
                {
                    string tableId = res_createTable.readAscii();
                    //CPlayer.tableToGoId = tableId;
                    //.betAmtOfTableToGo = betAmtList[bet].ToString();
                    //CPlayer.roomId = tmp;
                    App.trace("ĐÃ TẠO BÀN ID = " + tableId);
                    if (tableId != "")
                    {
                        CPlayer.tableToGoId = tableId;
                        CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                        _enterTableAfterCreate();
                    }


                });
            });
            return;

            /**
             *KHÔN DÙNG

            App.trace("PHAC DIU PHAC DIU " + tmp);
            enterSib2(tmp.ToString(), delegate (int a) {
                var req_createTable = new OutBounMessage("CREATE_RULE");
                req_createTable.addHead();
                req_createTable.writeByte(bet);    //id mức cược
                if (CPlayer.gameName == "xocdia")
                {
                    req_createTable.writeByte(1);
                    req_createTable.writeAcii("");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("");
                    req_createTable.writeString("");

                    App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
                    {
                        App.trace("RECV [CREATE_RULE]");
                        string tableId = res_createTable.readAscii();
                        //CPlayer.tableToGoId = tableId;
                        //CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                        //App.trace("RECV [CREATE_RULE] tableId = " + tableId);
                        //getTableData(new Table(int.Parse(tableId), "", 0, betAmtList[bet], 1, true, false));
                        if(tableId != "")
                        {
                            getTableData(new Table(int.Parse(tableId),"",0, betAmtList[bet], 0,false,false));
                            //new Table(tableId, tableName, tableType, betAmtList[betAmtId], slotCount,waiting ,locked)
                            //requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId);
                        }

                    });
                }
                else if (CPlayer.gameName == "maubinh")
                {
                    req_createTable.writeByte(3);
                    req_createTable.writeAcii("formattedName");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("password");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("maxNumberOfSlot");
                    req_createTable.writeString("4");
                }
                else if (CPlayer.gameName == "1" || CPlayer.gameName == "0")    //Có để Solo | Truyền thống
                {
                    req_createTable.writeByte(3);
                    req_createTable.writeAcii("formattedName");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("password");
                    req_createTable.writeString("");
                    req_createTable.writeAcii("maxNumberOfSlot");
                    if (tog[0].isOn)
                        req_createTable.writeString("4");
                    else
                        req_createTable.writeString("2");
                }

                App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
                {
                    string tableId = res_createTable.readAscii();
                    //CPlayer.tableToGoId = tableId;
                    //.betAmtOfTableToGo = betAmtList[bet].ToString();
                    //CPlayer.roomId = tmp;
                    if(tableId != "")
                        requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId);

                });
            });
            */
        }
        /*
        else
        {

            var req_createTable = new OutBounMessage("CREATE_RULE");
            req_createTable.addHead();
            req_createTable.writeByte(bet);    //id mức cược
            if (CPlayer.gameName == "xocdia")
            {
                req_createTable.writeByte(1);
                req_createTable.writeAcii("");
                req_createTable.writeString("");
                req_createTable.writeAcii("");
                req_createTable.writeString("");

                App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
                {
                    string tableId = res_createTable.readAscii();
                    //CPlayer.tableToGoId = tableId;
                    //CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();

                    //getTableData(new Table(int.Parse(tableId), "", 0, betAmtList[bet], 1, true, false));
                    if(tableId != "")
                        getTableData(new Table(int.Parse(tableId), "", 0, betAmtList[bet], 0, false, false));
                        //requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId);

                });
            }
            else if (CPlayer.gameName == "maubinh")
            {
                req_createTable.writeByte(3);
                req_createTable.writeAcii("formattedName");
                req_createTable.writeString("");
                req_createTable.writeAcii("password");
                req_createTable.writeString("");
                req_createTable.writeAcii("maxNumberOfSlot");
                req_createTable.writeString("4");
            }
            else if (CPlayer.gameName == "1" || CPlayer.gameName == "0")
            {
                req_createTable.writeByte(3);
                req_createTable.writeAcii("formattedName");
                req_createTable.writeString("");
                req_createTable.writeAcii("password");
                req_createTable.writeString("");
                req_createTable.writeAcii("maxNumberOfSlot");
                if (tog[0].isOn)
                    req_createTable.writeString("4");
                else
                    req_createTable.writeString("2");
            }
            //App.trace("FACK YA");
            App.ws.send(req_createTable.getReq(), delegate (InBoundMessage res_createTable)
            {
                string tableId = res_createTable.readAscii();
                //CPlayer.tableToGoId = tableId;
                //CPlayer.betAmtOfTableToGo = betAmtList[bet].ToString();
                //
                if(tableId != "")
                {
                    //CPlayer.roomId = tmp;
                    getTableData(new Table(int.Parse(tableId), "", 0, betAmtList[bet], 0, false, false));
                }

                //requestEnterPlace("Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId);

            });
        }
        */



    }
    /// <summary>
    /// 0: sliderDetail|1: Popup create table|2: sliderCuocToiDaDetail
    /// </summary>
    public RectTransform[] sliderRtf;
    public Text[] sliderTxt;

    private int[] maxBetSliderValue = { 10, 50, 100, 200, 500, 1000};
    public void sliderChanged(string type)
    {
        if(type == "chonmuccuoc")
        {
            int bet = (int)slider[0].value;
            sliderTxt[0].text = App.formatMoney(betAmtList[bet].ToString());
            if (slider[0].value > 6)
            {
                sliderRtf[0].localScale = new Vector2(1, 1);
                sliderTxt[0].transform.localScale = new Vector2(1, 1);
                sliderRtf[0].anchoredPosition = new Vector2(-105, 60);
            }
            else
            {
                sliderRtf[0].localScale = new Vector2(-1, 1);
                sliderTxt[0].transform.localScale = new Vector2(-1, 1);
                sliderRtf[0].anchoredPosition = new Vector2(140, 60);
            }
            return;
        }
        if(type == "cuoctoida")
        {
            int bet = (int)slider[1].value;
            sliderTxt[1].text = App.formatMoney(maxBetSliderValue[bet].ToString());
            if (slider[1].value > 2)
            {
                sliderRtf[2].localScale = new Vector2(1, 1);
                sliderTxt[1].transform.localScale = new Vector2(1, 1);
                sliderRtf[2].anchoredPosition = new Vector2(-105, 60);
            }
            else
            {
                sliderRtf[2].localScale = new Vector2(-1, 1);
                sliderTxt[1].transform.localScale = new Vector2(-1, 1);
                sliderRtf[2].anchoredPosition = new Vector2(140, 60);
            }
        }
    }

    public void showCreateTablePanels(bool isShow)
    {
        if (!isShow)
        {
            sliderRtf[1].DOAnchorPosY(1000, .5f).OnComplete(()=> {
                sliderRtf[1].transform.parent.gameObject.SetActive(false);
            });

            return;
        }
        float conditionAmout = 0;
        slider[1].transform.parent.gameObject.SetActive(false);
        switch (CPlayer.gameName)
        {
            case "0":   //PHỎM
                conditionAmout = CPlayer.chipBalance / 3;
                tog[0].transform.parent.parent.gameObject.SetActive(false); //Ẩn truyền thống, solo
                break;
            case "1":   //TLMN
                conditionAmout = CPlayer.chipBalance / 8;
                tog[0].transform.parent.parent.gameObject.SetActive(true);
                break;
            case "xocdia":
                tog[0].transform.parent.parent.gameObject.SetActive(false);
                conditionAmout = CPlayer.chipBalance / 100;
                break;
            case "maubinh":
                tog[0].transform.parent.parent.gameObject.SetActive(false);
                conditionAmout = CPlayer.chipBalance / 9;
                break;
            case "xito":
                tog[0].transform.parent.parent.gameObject.SetActive(false);
                conditionAmout = CPlayer.chipBalance / 1;
                slider[1].transform.parent.gameObject.SetActive(true);
                slider[1].value = 6;
                sliderChanged("cuoctoida");
                break;
            case "blackjack":
                tog[0].transform.parent.parent.gameObject.SetActive(false);
                conditionAmout = CPlayer.chipBalance / 3;
                break;
            case "poker":
                tog[0].transform.parent.parent.gameObject.SetActive(false);
                conditionAmout = CPlayer.chipBalance / 1;
                slider[1].transform.parent.gameObject.SetActive(true);
                slider[1].value = 6;
                sliderChanged("cuoctoida");
                break;

        }
        sliderTxt[0].text = App.formatMoney(betAmtList[0].ToString());
        slider[0].value = 0;
        for (int i = betAmtList.Count - 1; i > -1; i--)
        {
            if (conditionAmout > betAmtList[i])
            {
                sliderTxt[0].text = App.formatMoney(betAmtList[i].ToString());
                slider[0].value = i;
                break;
            }
        }
        sliderRtf[1].anchoredPosition = new Vector2(0, 1000);
        slider[0].transform.parent.parent.parent.parent.gameObject.SetActive(true);
        sliderRtf[1].DOAnchorPosY(0, .5f);
    }

    //CHỌN MỨC CƯỢC
    public void changeSliderValue(int type)
    {
        //Type: 0-Tru|1-Cong
        if (type == 0)
            slider[0].value--;
        else
            slider[0].value++;
    }

    //CƯỢC TỐI ĐA
    public void changeSliderValue2(int type)
    {
        //Type: 0-Tru|1-Cong
        if (type == 0)
            slider[1].value--;
        else
            slider[1].value++;
    }

    public void openSetting()
    {
        LoadingControl.instance.openSettingPanel();
    }

    public void beInvited(int betId, int roomId, int tableId, string pass)
    {
        if (slider[0].transform.parent.parent.parent.parent.gameObject.activeSelf || CPlayer.preScene == "TableList" || transPanel.activeSelf)
            return;
        LoadingControl.instance.btnDoConfirm.onClick.RemoveAllListeners();
        LoadingControl.instance.btnDoConfirm.onClick.AddListener(() => {
            LoadingControl.instance.closeConfirmDialog();
            CPlayer.betAmtOfTableToGo = App.formatMoney(betAmtList[betId].ToString());
            //requestEnterPlace("Lobby." + CPlayer.gameName + "." + roomId + "." + tableId, pass);
            CPlayer.roomId = roomId;
            getTableData(new Table(tableId, "", 0, betAmtList[betId], 0, true, false), pass);
        });
        string tempString = App.listKeyText["PLAY_INVITATION"];
        string new1 =  tempString.Replace("#1", App.formatMoney(betAmtList[betId].ToString()));
        string new2 = new1.Replace("#2", App.listKeyText["CURRENCY"]);
        LoadingControl.instance.confirmText.text = new2; //"Bạn nhận được một lời mời chơi với mức cược " + App.formatMoney(betAmtList[betId].ToString()) +" Gold. Bạn có muốn tham gia không?";
        LoadingControl.instance.blackPanel.SetActive(true);
        LoadingControl.instance.confirmDialogAnim.Play("DialogAnim");
    }

    public void showRecharePanel()
    {
        LoadingControl.instance.showRechargeScene(true);
    }

    private void _enterTableAfterCreate()
    {
        CPlayer.changed -= setBalance;
        //if (CPlayer.erroShowing == false)
        //{
        switch (CPlayer.gameName)
        {
            case "0":
                StartCoroutine(openGaneTable("phom_android_vn", "Phom"));
                break;
            case "1":
                StartCoroutine(openGaneTable("tlmn_android_vn", "TLMN"));
                break;
            case "xocdia":
                StartCoroutine(openGaneTable("xocdia_android_vn", "XocDia"));
                break;
            case "maubinh":
                StartCoroutine(openGaneTable("maubinh_android_vn", "MauBinh"));
                break;
            case "xito":
                StartCoroutine(openGaneTable("XiTo", "XiTo"));
                break;
            case "blackjack":
                StartCoroutine(openGaneTable("XiDach", "XiTo"));
                break;
            case "chan":
                StartCoroutine(openGaneTable("Chan", "Chan"));
                break;
            case "poker":
                StartCoroutine(openGaneTable("poker_android_vn", "Poker"));
                break;
        }
        //}
    }

    public IEnumerator _EnterGameByGamename(string gameName, string sceneName)
    {
        yield return new WaitForSeconds(0f);
        string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
            + "/DLC/" + gameName + ".dlc";
        Debug.Log(path + gameName);
        //var listScene = AssetBundle.LoadFromFileAsync(path);


        AssetBundleCreateRequest rqAB = new AssetBundleCreateRequest();
        Caching.ClearCache();
        rqAB = AssetBundle.LoadFromFileAsync(path);

        yield return rqAB;

        LoadingControl.instance.asbs[0] = rqAB.assetBundle;
        SceneManager.LoadScene(sceneName);
    }

}
