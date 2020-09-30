using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Core.Server.Api
{
    public class ChessControll : MonoBehaviour
    {
        [Header("====CHESS====")]
        public GameObject buttonEx;
        public GameObject[] chessListRoom;
        public GameObject chessTableClone;
        public GameObject chessPlayerClone;
        public Sprite[] chessButtonMid;
        public GameObject[] chessButton;
        public GameObject chessPopupJoinRoom;
        public GameObject chessPopupCreateRoom;
        public GameObject blackPanel;
        public Text[] chessTextPopUp;
        public InputField[] chessWaitInputField;
        public GameObject[] chessImageValueBetCreateTable;
        public Dropdown[] chessWaitDropDow;
        public GameObject[] chessObjectCreateTable;
        public InputField chessPassInputfieldPopupJoinRoom;
        public GameObject chessPassPopupJoinRoom;
        public Animator chessWaitAnimator;
        public GameObject chessScene;
        public static ChessControll instance;
        [Header("====TEXT====")]
        public Text[] chessTextBet;
        public Text userNameText;
        public Text manText, chipText, gameNameFull, gameName;
        public Text[] chat3TextList;

        [Header("===IMAGE===")]
        public Image avatar_img;
        // Use this for initialization
        void Start()
        {
            if (CPlayer.hidePayment == true)
            {
                buttonEx.SetActive(false);
            }
            //CPlayer.currSceneChess = this.gameObject;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //LoadingControl.instance.miniGamePanel.SetActive(true);
            //LoadingControl.instance.blackkkkkk.SetActive(false);
            if (CPlayer.nickName != null && CPlayer.nickName.Length > 1)
            {
                //CPlayer.currentScene = "WaitChess";
                gameName.text = CPlayer.gameNameFull;
                gameNameFull.text = CPlayer.gameNameFull;
                string name = CPlayer.fullName.Length == 0 ? CPlayer.nickName : CPlayer.fullName;
                name = App.formatNickName(name, 9);
                userNameText.text = name;
                //userNameText.text = CPlayer.fullName.Length == 0 ? CPlayer.nickName : CPlayer.fullName;

                manText.text = CPlayer.manBalance >= 0 ? App.formatMoney(CPlayer.manBalance.ToString()) : "0";
                chipText.text = CPlayer.chipBalance >= 0 ? App.formatMoney(CPlayer.chipBalance.ToString()) : "0";
                StartCoroutine(App.loadImg(avatar_img, App.getAvatarLink2(CPlayer.avatar + "", (int)CPlayer.id)));
                //CPlayer.changed += setBalance;
            }
            chessListRoom[0].GetComponent<Image>().color = new Color32(161, 161, 161, 255);
            if (CPlayer.preScene != "ChessTable")
            {
                chessTuongEnterZone(CPlayer.gameName, "", 0);
                chessTuongEnterZone("0", "", 0);
            }
            else
            {
                chessTuongListZone();
            }
            if (CPlayer.gameName == "xiangqi")
            {
                chessObjectCreateTable[0].SetActive(true);
                chessObjectCreateTable[1].SetActive(true);
                chessObjectCreateTable[2].SetActive(true);
                chessObjectCreateTable[3].SetActive(true);
            }
            else
            {
                chessObjectCreateTable[0].SetActive(false);
                chessObjectCreateTable[1].SetActive(false);
                chessObjectCreateTable[2].SetActive(false);
                chessObjectCreateTable[3].SetActive(false);
            }
            chessGetAtmBet();
            //chessTuongChoseRadio(CPlayer.currentChanel);
            //chessTuongGetTableByChanel(0);
            chessHandlerWaitRoom();
            //LoadingControl.instance.chatType = -1;
            LoadingControl.instance.loadChatData(LoadingControl.CHANNEL_GENERAL);
        }
        private void setBalance(string type)
        {
            if (type == "man")
                manText.text = CPlayer.manBalance >= 0 ? App.formatMoney(CPlayer.manBalance.ToString()) : "0";
            if (type == "chip")
            {
                chipText.text = CPlayer.chipBalance >= 0 ? App.formatMoney(CPlayer.chipBalance.ToString()) : "0";

            }

        }
        public void showChatBox()
        {
            LoadingControl.instance.showChatBox(LoadingControl.CHANNEL_GENERAL);
        }
        void Awake()
        {
            getInstance();
            chessScene.SetActive(true);
        }

        void getInstance()
        {
            if (instance != null)
                Destroy(gameObject);
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

        }
        // Invite
        [Header("=====INVITE=====")]
        public GameObject chessInvitedPopup;
        public GameObject chessElementPlayerInvited;
        public Text[] chessInvitedText;
        public void chessHandlerWaitRoom()
        {
            var req = new OutBounMessage("INVITE");
            req.addHead();
            App.ws.sendHandler(req.getReq(), delegate (InBoundMessage res)
            {
                chessShowPopUpJoinRoom(false);
                string nickName = res.readAscii();
                if (nickName.Length > 10)
                {
                    chessInvitedText[0].text = nickName.Substring(0, 9) + "...";
                }
                else
                {
                    chessInvitedText[0].text = nickName;
                }
            //App.trace("INVITE - NICK NAME " + nickName);
            int roomId = res.readByte();
            //App.trace("INVITE - ROOMID " + roomId);
            int tableId = res.readShort();
            //App.trace("INVITE - TABLEID " + tableId);
            string password = res.readString();
            //App.trace("INVITE - PASSWORD " + password);
            int tableType = res.readByte();
            //App.trace("INVITE - TABLETYPE " + tableType);
            int betAmtId = res.readByte();
                chessInvitedText[1].text = App.formatMoney(chessBet[betAmtId].ToString());
            //App.trace("INVITE - BETAMT ID " + betAmtId);

            CPlayer.passwordTableChess = password;
                CPlayer.tableToGoId = tableId.ToString();
                CPlayer.betAmtOfTableToGo = chessBet[betAmtId].ToString();
                CPlayer.roomId = roomID;
                chessShowRoomInvited(CPlayer.tableToGoId);
            });
        }
        // PopUp Invite
        public void chessShowRoomInvited(string path)
        {
            for (int i = chessElementPlayerInvited.transform.parent.childCount - 1; i >= 1; i--)
            {
                DestroyImmediate(chessElementPlayerInvited.transform.parent.GetChild(i).gameObject);
            }
            string nameTable = path;
            path = "Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + path;
            CPlayer.path = path;
            var req_get_table_data = new OutBounMessage("GET_TABLE_DATA");
            req_get_table_data.addHead();
            req_get_table_data.writeAcii(CPlayer.path);
            App.ws.send(req_get_table_data.getReq(), delegate (InBoundMessage res)
            {
                string nameOwer = res.readAscii(); // ten chu phong 
            int totalPlayer = res.readByte();
            //App.trace("total Player " + totalPlayer);
            for (int i = 0; i < totalPlayer; i++)
                {

                    int playerId = (int)res.readLong();
                    string nickName = res.readAscii();
                //App.trace("nickName " + nickName);
                string avatar = res.readAscii();
                    int avatarId = res.readShort();
                    int chipBalance = (int)res.readLong();
                    int starBalance = (int)res.readLong();
                    int score = (int)res.readLong();
                    int level = (int)res.readByte();
                    GameObject chessUserClone = Instantiate(chessElementPlayerInvited, chessElementPlayerInvited.transform.parent, false);
                    chessUserClone.GetComponentsInChildren<Text>()[0].text = nickName;
                    chessUserClone.GetComponentsInChildren<Text>()[1].text = App.formatMoney(chipBalance.ToString());
                    StartCoroutine(App.loadImg(chessUserClone.GetComponentsInChildren<Image>()[2], App.getAvatarLink2(avatar + "", avatarId)));
                    chessUserClone.SetActive(true);
                }
                int count2 = res.readByte();
                for (int i = 0; i < count2; i++)
                {
                    string attrName = res.readAscii();
                    string attrValue = res.readString();
                //App.trace("attrName " + attrName);
                //App.trace("attrValue " + attrValue);
                if (attrName == "matchDuration")
                    {
                        chessInvitedText[4].text = attrValue + "'/ván";
                        string getnumber = Regex.Match(attrValue, @"\d+").Value;
                        int numbertext = Int32.Parse(getnumber);
                        CPlayer.currentTurnTimeout = numbertext;
                    }
                    else if (attrName == "turnDuration")
                    {
                        string getnumber = Regex.Match(attrValue, @"\d+").Value;
                        int numbertext = Int32.Parse(getnumber);
                        chessInvitedText[3].text = numbertext / 60 + "'/nước";
                        CPlayer.currentTurnDurationvalue = numbertext;
                    }
                    else if (attrName == "accDuration")
                    {
                        chessTextPopUp[4].text = attrValue + "s";
                    }
                    else if (attrName == "blockSoftware")
                    {
                        if (attrValue == "0")
                        {
                            chessInvitedText[2].text = "Bình thường";
                        }
                        else if (attrValue == "1")
                        {
                            chessInvitedText[2].text = "Liệt tốt 1";
                        }
                        else if (attrValue == "2")
                        {
                            chessInvitedText[2].text = "Liệt tốt 5";
                        }
                        else if (attrValue == "3")
                        {
                            chessInvitedText[2].text = "Liệt mã 2 tốt 5";
                        }
                        else if (attrValue == "4")
                        {
                            chessInvitedText[2].text = "Nghĩ 50s";
                        }
                    }
                }
                int tableType = res.readByte();
                int betAmtId = res.readByte();
                if (totalPlayer > 0)
                    StartCoroutine(chessWaitShowInvited(true));
            });
        }
        IEnumerator chessWaitShowInvited(bool open)
        {
            yield return new WaitForSeconds(.3f);
            chessShowPopupInvited(true);
        }
        public void chessShowPopupInvited(bool open)
        {
            if (PlayerPrefs.GetInt("invite") == 0)
            {
                RectTransform rtf = chessInvitedPopup.GetComponent<RectTransform>();
                if (open)
                {
                    blackPanel.SetActive(true);
                    chessInvitedPopup.SetActive(true);
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .35f).OnComplete(() =>
                    {

                    });
                }
                else
                {
                    DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .35f).OnComplete(() =>
                    {
                        blackPanel.SetActive(false);
                        chessInvitedPopup.SetActive(false);
                    });
                }
            }
        }
        private void delAllHandle()
        {
            var req = new OutBounMessage("INVITE");
            req.addHead();
            App.ws.delHandler(req.getReq());
        }
        // Enter Room
        public void chessTuongEnterZone(string gameid, string password, int playmode)
        {
            var req_enter_zone = new OutBounMessage("ENTER_CHILD_PLACE");
            req_enter_zone.addHead();
            req_enter_zone.writeAcii(gameid);
            req_enter_zone.writeString(password); // pass
            req_enter_zone.writeByte(playmode); // 1:choi 0:view
            App.ws.send(req_enter_zone.getReq(), delegate (InBoundMessage res)
            {
                chessTuongListZone();
            });
        }

        private int roomID = 0;
        public void chessTuongListZone()
        {
            var req_list_zone = new OutBounMessage("LIST_ZONE_ROOM");
            req_list_zone.addHead();
            App.ws.send(req_list_zone.getReq(), delegate (InBoundMessage res)
            {
                int totalRoom = res.readByte();
            //App.trace("Chess - total room "+totalRoom);
            for (int i = 0; i < totalRoom; i++)
                {
                    int roomId = res.readByte();
                //App.trace("Chess - room id " + roomId);
                string roomName = res.readString();
                //App.trace("Chess - room name " + roomName);
                int numberInRoom = res.readShort();
                //App.trace("Chess - number room " + numberInRoom);
                int totalTable = res.readShort();
                //App.trace("Chess - total table " + totalTable);
                int maxTable = res.readShort();
                //App.trace("Chess - max table " + maxTable);
                if (i == 0)
                    {
                        chessListRoom[0].GetComponentInChildren<Text>().text = roomName;
                        chessListRoom[1].GetComponentsInChildren<Text>()[1].text = "(" + numberInRoom.ToString() + "/" + (maxTable * 2).ToString() + ")";
                    }
                    else if (i == 1)
                    {
                        chessListRoom[2].GetComponentInChildren<Text>().text = roomName;
                        chessListRoom[3].GetComponentsInChildren<Text>()[1].text = "(" + numberInRoom.ToString() + "/" + (maxTable * 2).ToString() + ")";
                    }
                    else
                    {
                        chessListRoom[4].GetComponentInChildren<Text>().text = roomName;
                        chessListRoom[5].GetComponentsInChildren<Text>()[1].text = "(" + numberInRoom.ToString() + "/" + (maxTable * 2).ToString() + ")";
                    }
                }
            });
        }
        // Radio
        public void chessTuongChoseRadio(int type)
        {
            roomID = type;
            for (int i = chessTableClone.transform.parent.childCount - 1; i >= 1; i--)
            {
                DestroyImmediate(chessTableClone.transform.parent.GetChild(i).gameObject);
            }
            chessListRoom[0].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            chessListRoom[2].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            chessListRoom[4].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            chessListRoom[1].SetActive(false);
            chessListRoom[3].SetActive(false);
            chessListRoom[5].SetActive(false);
            switch (type)
            {
                case 0:
                    chessListRoom[0].GetComponent<Image>().color = new Color32(161, 161, 161, 255);
                    chessListRoom[1].SetActive(true);
                    chessGetChoseRadio(type.ToString());
                    break;
                case 1:
                    chessListRoom[2].GetComponent<Image>().color = new Color32(161, 161, 161, 255);
                    chessListRoom[3].SetActive(true);
                    chessGetChoseRadio(type.ToString());
                    break;
                case 2:
                    chessListRoom[4].GetComponent<Image>().color = new Color32(161, 161, 161, 255);
                    chessListRoom[5].SetActive(true);
                    chessGetChoseRadio(type.ToString());
                    break;
            }
        }
        // Chose Radio
        public void chessGetChoseRadio(string roomId)
        {
            var req_list_chanel_table = new OutBounMessage("ENTER_SIBLING_PLACE");
            req_list_chanel_table.addHead();
            req_list_chanel_table.writeAcii(roomId);
            req_list_chanel_table.writeString("");
            req_list_chanel_table.writeByte(0);
            App.ws.send(req_list_chanel_table.getReq(), delegate (InBoundMessage res)
            {
                CPlayer.currentChanel = roomID;
                if (roomId == "0")
                {
                    chessTextBet[0].text = "1";
                    chessTextBet[1].text = "200";
                    chessTextBet[2].text = "500";
                    chessTextBet[3].text = "1000";
                    chessBetIdTableCreate = 1;
                    for (int i = 0; i < chessImageValueBetCreateTable.Length; i++)
                    {
                        chessImageValueBetCreateTable[i].SetActive(false);
                    }
                    chessImageValueBetCreateTable[1].SetActive(true);
                }
                else if (roomId == "1")
                {
                    chessTextBet[0].text = "1000";
                    chessTextBet[1].text = "2000";
                    chessTextBet[2].text = "3000";
                    chessTextBet[3].text = "5000";
                    chessBetIdTableCreate = 5;
                    for (int i = 0; i < chessImageValueBetCreateTable.Length; i++)
                    {
                        chessImageValueBetCreateTable[i].SetActive(false);
                    }
                    chessImageValueBetCreateTable[1].SetActive(true);
                }
                else if (roomId == "2")
                {
                    chessTextBet[0].text = "10K";
                    chessTextBet[1].text = "20K";
                    chessTextBet[2].text = "50K";
                    chessTextBet[3].text = "100K";
                    chessBetIdTableCreate = 9;
                    for (int i = 0; i < chessImageValueBetCreateTable.Length; i++)
                    {
                        chessImageValueBetCreateTable[i].SetActive(false);
                    }
                    chessImageValueBetCreateTable[1].SetActive(true);
                }
                chessTuongGetTableByChanel(0);
            });
        }

        private string[] chessAllTable;
        public void chessTuongGetTableByChanel(int typeTable)
        {
            for (int i = chessTableClone.transform.parent.childCount - 1; i >= 1; i--)
            {
                DestroyImmediate(chessTableClone.transform.parent.GetChild(i).gameObject);
            }
            if (typeTable == 0)
            {
                chessButton[0].GetComponent<Image>().sprite = chessButtonMid[0];
                chessButton[1].GetComponent<Image>().sprite = chessButtonMid[1];
            }
            else
            {
                chessButton[0].GetComponent<Image>().sprite = chessButtonMid[1];
                chessButton[1].GetComponent<Image>().sprite = chessButtonMid[0];
            }
            //App.trace("|||||||||||||||||||||||||||||||||||||");
            var req_list_zone_table = new OutBounMessage("LIST_ZONE_TABLE");
            req_list_zone_table.addHead();
            req_list_zone_table.writeByte(typeTable); // 1:ban con trong 0:tat ca cac ban
            req_list_zone_table.writeAcii(CPlayer.gameName);
            App.ws.send(req_list_zone_table.getReq(), delegate (InBoundMessage res)
            {
                int totalTable = res.readInt();
            //App.trace("Chess - Total Table Chanel " + totalTable);
            chessAllTable = new string[totalTable];
                for (int i = 0; i < totalTable; i++)
                {
                    GameObject tableClone = Instantiate(chessTableClone, chessTableClone.transform.parent, false);
                    int idTable = res.readShort(); //id của bàn
                chessAllTable[i] = idTable.ToString();
                //App.trace("Chess - Id table " + idTable);
                string tableName = res.readString(); // tên bàn 
                                                     //App.trace("Chess - Table name " + tableName);
                int tableType = res.readByte(); //kiểu bàn (ko dùng làm j)
                int betAmtId = res.readByte(); // id của mức cược (sau khi lấy danh sách các mức cược thì nên cho vào 1 cái array, mức tiền cược hiển thị sẽ là array[betAmtId])
                tableClone.GetComponentInChildren<Text>().text = chessBet[betAmtId].ToString();
                //App.trace("Chess - Table Bet " + betAmtId);
                int slotCount = res.readByte(); // số người chơi trong bàn
                if (slotCount == 1)
                    {
                        tableClone.GetComponentsInChildren<Image>()[1].enabled = false;
                    }
                //App.trace("Chess - Table Slot " + slotCount);
                bool waiting = res.readByte() == 1; //cái này ko cần thiết
                bool locked = res.readByte() == 1; //có đặt pass hay ko, cũng ko cần  
                tableClone.name = idTable.ToString() + locked;
                    tableClone.SetActive(true);
                    chessTableRoomChange();
                }
            });
        }

        private int[] chessBet;
        public void chessGetAtmBet()
        {
            var req_getBetAmtList = new OutBounMessage("LIST_BET_AMT");
            req_getBetAmtList.addHead();
            App.ws.send(req_getBetAmtList.getReq(), delegate (InBoundMessage res)
            {

                int count = res.readByte();
                chessBet = new int[count];
                for (int i = 0; i < count; i++)
                {
                    int a = res.readInt();
                //App.trace("Chess - Chess Bet " + a);
                chessBet[i] = a;
                }
            //App.trace("GET BET AMT DONE! BETAMT COUNT = " + count);
        });
        }
        public void chessShowPopUpCreateTable(bool open)
        {
            RectTransform rtf = chessPopupCreateRoom.GetComponent<RectTransform>();
            if (open)
            {
                chessPopupCreateRoom.SetActive(true);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .35f).OnComplete(() =>
                {

                });
            }
            else
            {
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .35f).OnComplete(() =>
                {
                    chessPopupCreateRoom.SetActive(false);
                });
            }
        }
        private string chessNameTableCreate;
        private string chessPassTableCreate;
        private int chessMatchDurationTableCreate;
        private int chessTurnDurationTableCreate;
        private int chessAccTableCreate;
        private int chessBlockSoftWareTableCreate;
        private int chessBetIdTableCreate = 1;
        private bool chessJoinRoomHadPass;
        public void chessCreateTable()
        {
            chessNameTableCreate = chessWaitInputField[0].text;
            chessPassTableCreate = chessWaitInputField[1].text;
            switch (chessWaitDropDow[0].value)
            {
                case 0:
                    chessMatchDurationTableCreate = 5;
                    break;
                case 1:
                    chessMatchDurationTableCreate = 10;
                    break;
                case 2:
                    chessMatchDurationTableCreate = 15;
                    break;
                case 3:
                    chessMatchDurationTableCreate = 20;
                    break;
                case 4:
                    chessMatchDurationTableCreate = 30;
                    break;
                case 5:
                    chessMatchDurationTableCreate = 60;
                    break;
                case 6:
                    chessMatchDurationTableCreate = 5;
                    break;
                case 7:
                    chessMatchDurationTableCreate = 10;
                    break;
            }
            switch (chessWaitDropDow[1].value)
            {
                case 0:
                    chessTurnDurationTableCreate = 30;
                    break;
                case 1:
                    chessTurnDurationTableCreate = 60;
                    break;
                case 2:
                    chessTurnDurationTableCreate = 120;
                    break;
                case 3:
                    chessTurnDurationTableCreate = 180;
                    break;
                case 4:
                    chessTurnDurationTableCreate = 300;
                    break;
                case 5:
                    chessTurnDurationTableCreate = -1;
                    break;
            }
            switch (chessWaitDropDow[2].value)
            {
                case 0:
                    chessAccTableCreate = 0;
                    break;
                case 1:
                    chessAccTableCreate = 1;
                    break;
                case 2:
                    chessAccTableCreate = 2;
                    break;
                case 3:
                    chessAccTableCreate = 3;
                    break;
            }
            switch (chessWaitDropDow[3].value)
            {
                case 0:
                    chessBlockSoftWareTableCreate = 0;
                    break;
                case 1:
                    chessBlockSoftWareTableCreate = 1;
                    break;
                case 2:
                    chessBlockSoftWareTableCreate = 2;
                    break;
                case 3:
                    chessBlockSoftWareTableCreate = 3;
                    break;
            }
            var req_CREATE_RULE = new OutBounMessage("CREATE_RULE");
            req_CREATE_RULE.addHead();
            req_CREATE_RULE.writeByte(chessBetIdTableCreate);
            req_CREATE_RULE.writeByte(6);
            req_CREATE_RULE.writeAcii("formattedName");
            req_CREATE_RULE.writeString(chessNameTableCreate);
            req_CREATE_RULE.writeAcii("password");
            req_CREATE_RULE.writeString(chessPassTableCreate);
            req_CREATE_RULE.writeAcii("matchDuration");
            req_CREATE_RULE.writeString(chessMatchDurationTableCreate.ToString());
            req_CREATE_RULE.writeAcii("turnDuration");
            req_CREATE_RULE.writeString(chessTurnDurationTableCreate.ToString());
            req_CREATE_RULE.writeAcii("accDuration");
            req_CREATE_RULE.writeString(chessAccTableCreate.ToString());
            req_CREATE_RULE.writeAcii("blockSoftware");
            req_CREATE_RULE.writeString(chessBlockSoftWareTableCreate.ToString());
            App.ws.send(req_CREATE_RULE.getReq(), delegate (InBoundMessage res)
            {
                string tableId = res.readAscii();
                CPlayer.roomId = roomID;
                CPlayer.betAmtOfTableToGo = chessBet[chessBetIdTableCreate].ToString();
                CPlayer.tableToGoId = tableId;
                CPlayer.currentTurnTimeout = chessMatchDurationTableCreate;
                CPlayer.currentTurnDurationvalue = chessTurnDurationTableCreate;
                CPlayer.passwordTableChess = chessPassTableCreate;
                CPlayer.clientTargetMode = 1;
                string path = "Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId;
                CPlayer.path = path;
                StartCoroutine(chessOpenRoom());
            });
        }
        public void chessCheckValueBetCreateTable(int number)
        {
            if (roomID == 0)
                chessBetIdTableCreate = number;
            else if (roomID == 1)
                chessBetIdTableCreate = number + 4;
            else if (roomID == 2)
                chessBetIdTableCreate = number + 8;
            for (int i = 0; i < chessImageValueBetCreateTable.Length; i++)
            {
                chessImageValueBetCreateTable[i].SetActive(false);
            }
            chessImageValueBetCreateTable[number].SetActive(true);
        }
        // Quick Play
        public void chessQuickPlay()
        {
            var req_QUICK_PLAY = new OutBounMessage("QUICK_PLAY");
            req_QUICK_PLAY.addHead();
            App.ws.send(req_QUICK_PLAY.getReq(), delegate (InBoundMessage res)
            {
                string tablePath = res.readAscii();
            //App.trace("tablePath" + tablePath);
            string tableName = res.readString();
            //App.trace("tableName" + tableName);
            int count = res.readByte();
            //App.trace("count" + count);
            for (var i = 0; i < count; i++)
                {
                    string attrName = res.readAscii();
                //App.trace("attrName" + attrName);
                string attrValue = res.readString();
                //App.trace("attrValue" + attrValue);
            }
                int tableType = res.readByte();
            //App.trace("tableType" + tableType);
            int tableBetAmtId = res.readByte();
            //App.trace("tableBetAmtId" + tableBetAmtId);
            CPlayer.path = tablePath;
                chessJoinRoomQuick(1);
            });
        }
        public void chessJoinRoomQuick(int playMode)
        {
            var req_enterChessRoom = new OutBounMessage("ENTER_PLACE");
            req_enterChessRoom.addHead();

            req_enterChessRoom.writeAcii(CPlayer.path);
            req_enterChessRoom.writeString(CPlayer.passwordTableChess); //Mật khẩu của phòng chơi
            req_enterChessRoom.writeByte(playMode); // 0: Vào xem | 1: Vào chơi
                                                    //req_enterChessRoom.print();

            App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res)
            {
                delAllHandle();
                CPlayer.clientTargetMode = playMode;
                StartCoroutine(chessOpenRoom());
            });
        }
        // Popup Join Room
        public void chessShowPopUpJoinRoom(bool open)
        {
            RectTransform rtf = chessPopupJoinRoom.GetComponent<RectTransform>();
            if (open)
            {
                blackPanel.SetActive(true);
                chessPopupJoinRoom.SetActive(true);
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .35f).OnComplete(() =>
                {

                });
            }
            else
            {
                DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .35f).OnComplete(() =>
                {
                    blackPanel.SetActive(false);
                    chessPassPopupJoinRoom.SetActive(false);
                    chessPopupJoinRoom.SetActive(false);
                });
            }
        }
        public void chessGetTableData(string path)
        {
            //App.trace("||||||||||||||||||||| Name Room " + path);
            for (int i = chessPlayerClone.transform.parent.childCount - 1; i >= 1; i--)
            {
                DestroyImmediate(chessPlayerClone.transform.parent.GetChild(i).gameObject);
            }
            if (path.Contains("True"))
            {
                chessJoinRoomHadPass = true;
                chessPassPopupJoinRoom.SetActive(true);
            }
            else
            {
                CPlayer.passwordTableChess = "";
                chessJoinRoomHadPass = false;
            }
            string getNameTable = Regex.Match(path, @"\d+").Value;
            int name = Int32.Parse(getNameTable);
            string nameTable = name.ToString();
            path = "Lobby." + CPlayer.gameName + "." + roomID.ToString() + "." + nameTable;
            CPlayer.path = path;
            var req_get_table_data = new OutBounMessage("GET_TABLE_DATA");
            req_get_table_data.addHead();
            req_get_table_data.writeAcii(CPlayer.path);
            App.ws.send(req_get_table_data.getReq(), delegate (InBoundMessage res)
            {
                string nameOwer = res.readAscii(); // ten chu phong 
                                                   //App.trace("Chess - name ower " + nameOwer);
            int totalPlayer = res.readByte();
            //App.trace("Chess - total player " + totalPlayer);
            for (int i = 0; i < totalPlayer; i++)
                {

                    int playerId = (int)res.readLong();
                //App.trace("Chess - player id " + playerId);
                string nickName = res.readAscii();
                //App.trace("Chess - nickName " + nickName);
                string avatar = res.readAscii();
                //App.trace("Chess - avatar " + avatar);
                int avatarId = res.readShort();
                //App.trace("Chess - avatarId " + avatarId);
                int chipBalance = (int)res.readLong();
                //App.trace("Chess - chipBalance " + chipBalance);
                int starBalance = (int)res.readLong();
                //App.trace("Chess - starBalance " + starBalance);
                int score = (int)res.readLong();
                //App.trace("Chess - score " + score);
                int level = (int)res.readByte();
                //App.trace("Chess - level " + level);
                GameObject chessUserClone = Instantiate(chessPlayerClone, chessPlayerClone.transform.parent, false);
                    if (nickName.Length > 10)
                    {
                        chessUserClone.GetComponentsInChildren<Text>()[0].text = nickName.Substring(0, 9) + "...";
                    }
                    else
                    {
                        chessUserClone.GetComponentsInChildren<Text>()[0].text = nickName;
                    }
                    chessUserClone.GetComponentsInChildren<Text>()[1].text = App.formatMoney(chipBalance.ToString());
                    StartCoroutine(App.loadImg(chessUserClone.GetComponentsInChildren<Image>()[2], App.getAvatarLink2(avatar + "", avatarId)));
                    chessUserClone.SetActive(true);
                }
                int count2 = res.readByte();
            //App.trace("Chess - count2 " + count2);
            for (int i = 0; i < count2; i++)
                {
                    string attrName = res.readAscii();
                //App.trace("Chess - attrName " + attrName);
                string attrValue = res.readString();
                //App.trace("Chess - attrValue " + attrValue);
                if (attrName == "matchDuration")
                    {
                        chessTextPopUp[2].text = attrValue + "'/ván";
                        string getnumber = Regex.Match(attrValue, @"\d+").Value;
                        int numbertext = Int32.Parse(getnumber);
                        CPlayer.currentTurnTimeout = numbertext;
                    }
                    else if (attrName == "turnDuration")
                    {
                        string getnumber = Regex.Match(attrValue, @"\d+").Value;
                        int numbertext = Int32.Parse(getnumber);
                        chessTextPopUp[3].text = numbertext / 60 + "'/nước";
                        CPlayer.currentTurnDurationvalue = numbertext;
                    }
                    else if (attrName == "accDuration")
                    {
                        chessTextPopUp[4].text = attrValue + "s";
                    }
                    else if (attrName == "blockSoftware")
                    {
                        if (attrValue == "0")
                        {
                            chessTextPopUp[5].text = "Bình thường";
                        }
                        else if (attrValue == "1")
                        {
                            chessTextPopUp[5].text = "Liệt tốt 1";
                        }
                        else if (attrValue == "2")
                        {
                            chessTextPopUp[5].text = "Liệt tốt 5";
                        }
                        else if (attrValue == "3")
                        {
                            chessTextPopUp[5].text = "Liệt mã 2 tốt 5";
                        }
                        else if (attrValue == "4")
                        {
                            chessTextPopUp[5].text = "Nghĩ 50s";
                        }
                    }
                }
                int tableType = res.readByte();
            //App.trace("Chess - tableType " + tableType);
            int betAmtId = res.readByte();
            //App.trace("Chess - betAmtId " + betAmtId);
            chessTextPopUp[1].text = App.formatMoney(chessBet[betAmtId].ToString());
                chessTextPopUp[0].text = "Bàn số " + nameTable;
                CPlayer.tableToGoId = nameTable;
                CPlayer.betAmtOfTableToGo = chessBet[betAmtId].ToString();
                CPlayer.roomId = roomID;
                chessShowPopUpJoinRoom(true);
            });
        }
        // Table Change
        public void chessTableRoomChange()
        {
            var req_table_in_room_change = new OutBounMessage("TABLE_IN_ROOM_CHANGED");
            req_table_in_room_change.addHead();
            App.ws.sendHandler(req_table_in_room_change.getReq(), delegate (InBoundMessage res)
            {
                int tableId = res.readShort();
                App.trace("chessTableRoomChange-tableId " + tableId);
                string tableName = res.readString();
                App.trace("chessTableRoomChange-tableName " + tableName);
                int tableType = res.readByte();
                App.trace("chessTableRoomChange-tableType " + tableType);
                int betAmtId = res.readByte();
                App.trace("chessTableRoomChange-betAmtId " + betAmtId);
                int slotCount = res.readByte();
                App.trace("chessTableRoomChange-slotCount " + slotCount);
                bool waiting = res.readByte() == 1;
                App.trace("chessTableRoomChange-waiting " + waiting);
                bool locked = res.readByte() == 1;
                App.trace("chessTableRoomChange-locked " + locked);
                bool isFull = res.readByte() == 1;
                App.trace("chessTableRoomChange-isFull " + isFull);
                for (int i = chessTableClone.transform.parent.childCount - 1; i >= 1; i--)
                {
                    if (chessTableClone.transform.parent.GetChild(i).name == tableId.ToString() + locked)
                    {
                        if (slotCount == 0)
                        {
                            Destroy(chessTableClone.transform.parent.GetChild(i).gameObject);
                        }
                        else
                        {
                            chessTableClone.transform.parent.GetChild(i).GetComponentInChildren<Text>().text = chessBet[betAmtId].ToString();
                            if (isFull)
                            {
                                chessTableClone.transform.parent.GetChild(i).GetComponentsInChildren<Image>()[0].enabled = true;
                                chessTableClone.transform.parent.GetChild(i).GetComponentsInChildren<Image>()[1].enabled = true;
                            }
                            else
                            {
                                chessTableClone.transform.parent.GetChild(i).GetComponentsInChildren<Image>()[1].enabled = false;
                            }
                        }
                    }
                }
                if (chessAllTable.Contains(tableId.ToString()) == false)
                {
                    GameObject tableClone = Instantiate(chessTableClone, chessTableClone.transform.parent, false);
                    tableClone.GetComponentInChildren<Text>().text = chessBet[betAmtId].ToString();
                    if (slotCount == 1)
                    {
                        tableClone.GetComponentsInChildren<Image>()[1].enabled = false;
                    }
                    tableClone.name = tableId.ToString() + locked;
                    tableClone.SetActive(true);
                    Array.Resize(ref chessAllTable, chessAllTable.Length + 1);
                    chessAllTable[chessAllTable.Length - 1] = tableId.ToString();
                }
            });
        }
        // Join Room
        public void chessJoinRoom(int playMode)
        {
            if (chessJoinRoomHadPass)
            {
                CPlayer.passwordTableChess = chessPassInputfieldPopupJoinRoom.text;
            }
            string path = "Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId;
            CPlayer.path = path;
            var req_enterChessRoom = new OutBounMessage("ENTER_PLACE");
            req_enterChessRoom.addHead();

            req_enterChessRoom.writeAcii(CPlayer.path);
            req_enterChessRoom.writeString(CPlayer.passwordTableChess); //Mật khẩu của phòng chơi
            req_enterChessRoom.writeByte(playMode); // 0: Vào xem | 1: Vào chơi
                                                    //req_enterChessRoom.print();

            App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res)
            {
                delAllHandle();
                CPlayer.clientTargetMode = playMode;
                StartCoroutine(chessOpenRoom());
            });
        }
        IEnumerator chessOpenRoom()
        {
            CPlayer.typePlay = "play";
            if (CPlayer.gameName == "xiangqi" || CPlayer.gameName == "mystery_xiangqi")
                SceneManager.LoadScene("TableChessTuongScene");
            else
                SceneManager.LoadScene("TableChessVuaScene");
            yield return new WaitForSecondsRealtime(0.5f);
            Destroy(gameObject);
        }
        // Open Player Info
        //public void openPlayerInfo()
        //{
        //    var req_ENTER_PARENT_PLACE = new OutBounMessage("ENTER_PARENT_PLACE");
        //    req_ENTER_PARENT_PLACE.addHead();
        //    req_ENTER_PARENT_PLACE.writeString("");
        //    req_ENTER_PARENT_PLACE.writeByte(CPlayer.clientCurrentMode);
        //    App.ws.send(req_ENTER_PARENT_PLACE.getReq(), delegate (InBoundMessage res)
        //    {
        //        var req = new OutBounMessage("TABLE_IN_ROOM_CHANGED");
        //        req.addHead();
        //        App.ws.delHandler(req.getReq());
        //        App.trace("ENTER_PARENT_PLACE");
        //        res.readByte();
        //        res.readByte();
        //    //App.trace("AAAAA");
        //    LoadingControl.instance.loadin   gScene.SetActive(true);
        //        StartCoroutine(_openPlayerInfo());
        //    });
        //}
        //IEnumerator _openPlayerInfo()
        //{
        //    var req_ENTER_PARENT_PLACE = new OutBounMessage("ENTER_PARENT_PLACE");
        //    req_ENTER_PARENT_PLACE.addHead();
        //    req_ENTER_PARENT_PLACE.writeString("");
        //    req_ENTER_PARENT_PLACE.writeByte(CPlayer.clientCurrentMode);
        //    App.ws.send(req_ENTER_PARENT_PLACE.getReq(), delegate (InBoundMessage res)
        //    {
        //        res.readByte();
        //        res.readByte();
        //    });
        //    CPlayer.preScene = "WaitChessScene";
        //    //yield return new WaitForSeconds(0.5f);
        //    SceneManager.LoadScene("PlayerScene");
        //    yield return new WaitForSecondsRealtime(0.2f);
        //    PlayerSceneControl.instance.playerScene.SetActive(true);
        //    //PlayerSceneControl.instance.playerScene.transform.SetAsLastSibling();
        //    //PlayerSceneControl.instance.slideSceneAnim.Play("PlayerSceneAnim");
        //    //yield return async; 
        //    //chessWaitAnimator.Play("WaitchessOut");
        //    Destroy(gameObject, .2f);
        //    yield return new WaitForSeconds(.2f);
        //    LoadingControl.instance.loadingScene.SetActive(false);
        //    //LobbyControll.instance.LobbyScene.SetActive(false);
        //}
        //// Show Recharge Player
        //public void showRechargeScene(bool toOpen)
        //{
        //    LoadingControl.instance.loadingScene.SetActive(true);
        //    LoadingControl.instance.openFrom = "chess";
        //    LoadingControl.instance.showExChangeScene(toOpen);
        //}
        // Back Lobby
        public void chessBackLobby()
        {
            EnterParentPlace();

            StartCoroutine(openLobby());
            Destroy(gameObject, .2f);
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
            CPlayer.preScene = "WaitChessScene";
            CPlayer.changed -= setBalance;
            SceneManager.LoadScene("Lobby");
            yield return new WaitForSeconds(0.05f);

        }
        IEnumerator _chessBackLobby()
        {
            var req_ENTER_PARENT_PLACE = new OutBounMessage("ENTER_PARENT_PLACE");
            req_ENTER_PARENT_PLACE.addHead();
            req_ENTER_PARENT_PLACE.writeString("");
            req_ENTER_PARENT_PLACE.writeByte(CPlayer.clientCurrentMode);
            App.ws.send(req_ENTER_PARENT_PLACE.getReq(), delegate (InBoundMessage res)
            {
                res.readByte();
                res.readByte();
            });
            CPlayer.preScene = "WaitChessScene";
            //yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("LobbyScene");
            //LobbyControll.instance.LobbyScene.SetActive(true);
            //chessWaitAnimator.Play("WaitchessOut");
            //yield return async; 
            Destroy(gameObject, .2f);
            //yield return new WaitForSeconds(0.5f);
            //LoadingControl.instance.loadingScene.SetActive(false);
            yield break;
        }
        public void chessOpenSetting()
        {
            LoadingControl.instance.settingPanel.SetActive(true);
        }
    }
}
