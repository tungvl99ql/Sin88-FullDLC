using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Globalization;
using Core.Server.Api;

public class ChessTableVuaControll : MonoBehaviour
{
    public Text[] chessTableText;
    public GameObject chessButtonStart;
    public GameObject[] chessShowValueText;
    public GameObject[] chessGameObjectImage;
    public GameObject[] chessTimeToTurn;
    public GameObject[] chessInfoPlayer;
    public GameObject[] chessButton;
    public GameObject[] chessWaitBackButton;
    public GameObject chessControllTablePlayerMode;
    public Text[] chessTimeRemain;
    public Text[] chessPoint;
    public Sprite[] chessSprite;
    public Image[] chessTimeMainImage;
    public Sprite[] chessTuongFace;
    public GameObject[] chessPointToSlot;
    public GameObject[] chessAttackKing;
    public GameObject[] chessPlayerPopupWinner;
    public GameObject chessPopupShowValueMatch;
    public GameObject chessMediaPlayer;
    public GameObject chessPopupNotify;
    public GameObject chessPopupBack;
    public GameObject chessPopupUpdateFaceChess;
    public Image[] chessUpdateNewFaceImage;
    public GameObject[] chessEffectShowValue;
    public Text chessPopupNotifyText;
    public GameObject chessBlackPanel;
    public GameObject chessElement;
    public GameObject chessEatPlayerBotElement;
    public GameObject chessEatPlayerTopElement;
    public GameObject chessEatPlayerBotMove;
    public GameObject chessEatPlayerTopMove;
    private GameObject chessLastStep = null;
    private GameObject chessNextStep = null;
    private bool chessChangePositionPlayer;
    private GridLayoutGroup chessGrid;
    private bool chessPlayerPlaying = false;
    public string chessMyColor;
    public static ChessTableVuaControll instance;
    private GameObject chessEatPlayerBotMoveClone;
    private GameObject chessEatPlayerTopMoveClone;
    private int chessLastPointBatTotQuaDuong;
    private int chessTargetNextStep = -1;
    private long chipPlayer1, chipPlayer2;

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
    void Awake()
    {
        chessGrid = chessControllTablePlayerMode.GetComponent<GridLayoutGroup>();
        getInstance();
    }
    void Start()
    {
        CPlayer.currSceneChess = this.gameObject;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //LoadingControl.instance.miniGamePanel.SetActive(false);
        //LoadingControl.instance.blackkkkkk.SetActive(false);
        if (CPlayer.typePlay == "play")
        {
            App.trace("CPlayer.clientTargetMode " + CPlayer.clientTargetMode);
            CPlayer.clientCurrentMode = CPlayer.clientTargetMode;                    
            //chessChangeClientMode(CPlayer.clientCurrentMode);
            chessGetAtmBet();
            chessGetTableDataEx();
            registerHandler();
            //requestEnterPlace();
        }
        else if (CPlayer.typePlay == "replay")
        {
            chessButton[0].SetActive(false);
            chessButton[1].SetActive(false);
            chessButton[2].SetActive(false);
            chessButton[3].SetActive(false);
            chessButton[4].SetActive(false);
            chessReviewMatch();
            chessMediaPlayer.SetActive(true);
        }
    }
    private int[] chessBet;

    public void chessGetTableData()
    {
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
            }
            int count2 = res.readByte();
            //App.trace("Chess - count2 " + count2);
            for (int i = 0; i < count2; i++)
            {
                string attrName = res.readAscii();
                //App.trace("Chess - attrName " + attrName);
                string attrValue = res.readString();
                //App.trace("Chess - attrValue " + attrValue);
                //if (attrName == "matchDuration")
                //{
                    //chessTableText[1].text = attrValue + "'/ván";
                    //string getnumber = Regex.Match(attrValue, @"\d+").Value;
                    //int numbertext = Int32.Parse(getnumber);
                    //CPlayer.currentTurnTimeout = numbertext;
                //}
                //else if (attrName == "turnDuration")
                //{
                    //string getnumber = Regex.Match(attrValue, @"\d+").Value;
                    //int numbertext = Int32.Parse(getnumber);
                    //if (numbertext == -1 || numbertext == 1)
                    //{
                    //    chessTableText[1].text = chessTableText[1].text + "\n" + "∞" + "'/nước";
                    //}
                    //else
                    //{
                    //    chessTableText[1].text = chessTableText[1].text + "\n" + numbertext / 60 + "'/nước";
                    //}
                    //CPlayer.currentTurnDurationvalue = numbertext;
                //}
            }
            int tableType = res.readByte();
            //App.trace("Chess - tableType " + tableType);
            int betAmtId = res.readByte();
            //.trace("Chess - betAmtId " + betAmtId);
            CPlayer.betAmtOfTableToGo = chessBet[betAmtId].ToString();
            //chessTableText[0].text = CPlayer.gameNameFull + "\n" + App.formatMoney(chessBet[betAmtId].ToString()) + " Chip";
            if (CPlayer.clientCurrentMode == 0)
            {
                chessButton[1].SetActive(false);
            }
            else if (CPlayer.clientCurrentMode == 1 || CPlayer.chipBalance < float.Parse(CPlayer.betAmtOfTableToGo))
            {
                chessChangeClientMode(1);
                chessButton[1].SetActive(false);
            }
            else if (CPlayer.clientCurrentMode == 1)
            {
                chessButton[0].SetActive(false);
            }
        });
    }
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
            chessGetTableData();
        });
    }

    public void requestEnterPlace()
    {
        //string path = "Lobby." + CPlayer.gameName + "." + CPlayer.roomId + "." + CPlayer.tableToGoId;
        //CPlayer.path = path;
        var req_enterChessRoom = new OutBounMessage("ENTER_PLACE");
        req_enterChessRoom.addHead();

        req_enterChessRoom.writeAcii(CPlayer.path);
        req_enterChessRoom.writeString(CPlayer.passwordTableChess); //Mật khẩu của phòng chơi
        req_enterChessRoom.writeByte(CPlayer.clientCurrentMode); // 0: Vào xem | 1: Vào chơi
                                                                 //req_enterChessRoom.print();


        App.ws.send(req_enterChessRoom.getReq(), delegate (InBoundMessage res)
        {
            chessTableText[0].text = CPlayer.gameNameFull + "\n" + CPlayer.betAmtOfTableToGo + " Gold";
            chessTableText[1].text = CPlayer.currentTurnTimeout + "'/ván";
            if (CPlayer.currentTurnDurationvalue == -1)
            {
                chessTableText[1].text = chessTableText[1].text + "\n" + "∞" + "'/nước";
            }
            else
            {
                chessTableText[1].text = chessTableText[1].text + "\n" + CPlayer.currentTurnDurationvalue / 60 + "'/nước";
            }
            chessGetTableDataEx();
            registerHandler();
        });


    }

    private bool isOwerChess = false;
    public int mySlotID;
    private Image preTimeLeapImage;
    private int preTimeLeapId;
    public float time = 1;
    private bool run = false;
    private int curr = 0;
    private float timecd;
    void Update()
    {
        timecd += Time.deltaTime;
        if (run)
            preTimeLeapImage.fillAmount = (time - timecd) / time;
    }
    // Count Down Time
    IEnumerator chessCountDownMainTime()
    {
        while (run)
        {
            yield return new WaitForSeconds(1f);
            curr--;
            chessTimeRemain[preTimeLeapId].text = (curr / 60).ToString() + ":" + string.Format("{0:00}", (curr % 60)).ToString();
        }
    }
    // Set Step
    IEnumerator chessSetStep(int sourcePosition, int targetPosition)
    {
        yield return new WaitForSeconds(.4f);
        chessLastStep = chessPointToSlot[sourcePosition];
        chessLastStep.GetComponent<Image>().enabled = true;
        chessNextStep = chessPointToSlot[targetPosition].transform.GetChild(1).GetChild(0).gameObject;
        chessNextStep.SetActive(true);
    }
    // Handler
    public void registerHandler()
    {
        //#region //PHAN HANDLE DUNG CHUNG
        var req_SET_PLAYER_POINT = new OutBounMessage("SET_PLAYER_POINT");  //SET TỈ SỐ (CHESS)
        req_SET_PLAYER_POINT.addHead();
        App.ws.sendHandler(req_SET_PLAYER_POINT.getReq(), delegate (InBoundMessage res)
        {
            App.trace("SET_PLAYER_POINT");

        });

        var req_SET_PLAYER_STATUS = new OutBounMessage("SET_PLAYER_STATUS");    //SET TRẠNG THÁI
        req_SET_PLAYER_STATUS.addHead();
        App.ws.sendHandler(req_SET_PLAYER_STATUS.getReq(), delegate (InBoundMessage res)
        {
            App.trace("SET_PLAYER_STATUS!!!!!!!!");
        });



        var req_KICK_PLAYER = new OutBounMessage("KICK_PLAYER");    //KÍCH NG CHƠI
        req_KICK_PLAYER.addHead();
        App.ws.sendHandler(req_KICK_PLAYER.getReq(), delegate (InBoundMessage res)
        {
            App.trace("KICK_PLAYER");
            int status = res.readByte();
            string content = res.readString();
            //App.trace("status = " + status + "|content = " + content);
            if (status == -1)
            {
                App.showErr(content);
                return;
            }
            if (status == 2)
            {

            }

        });

        var req_ENTER_STATE = new OutBounMessage("ENTER_STATE");
        req_ENTER_STATE.addHead();
        App.ws.sendHandler(req_ENTER_STATE.getReq(), delegate (InBoundMessage res)
        {
            App.trace("RCV [ENTER_STATE]");
            int stateId = res.readByte();
            //App.trace("Chess - stateId " + stateId);
        });

        var req_MOVE = new OutBounMessage("MOVE");
        req_MOVE.addHead();
        App.ws.sendHandler(req_MOVE.getReq(), delegate (InBoundMessage res)
        {
            for (int z = 0; z < chessPointToSlot.Length; z++)
            {
                chessPointToSlot[z].GetComponent<Image>().enabled = false;
                if (chessPointToSlot[z].transform.childCount > 1)
                {
                    if (chessPointToSlot[z].transform.GetChild(1).childCount > 0)
                    {
                        chessPointToSlot[z].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
                        chessPointToSlot[z].transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                    }
                }
            }
            chessTableClearPointMove();
            App.trace("MOVE");
            int sourcePosition = res.readByte();
            //App.trace("Chess Move -sourcePosition " + sourcePosition);
            int targetPosition = res.readByte();
            chessCurrDirPoint = targetPosition;
            //App.trace("Chess Move -targetPosition " + targetPosition);
            if (chessNextStep != null && chessLastStep != null)
            {
                chessLastStep.GetComponent<Image>().enabled = false;
                chessNextStep.SetActive(false);
            }
            if (chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("kingred") || chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("xered") || chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("kingblack") || chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("xeblack"))
            {
                string[] newName = chessPointToSlot[sourcePosition].transform.GetChild(1).name.Split('.');
                chessPointToSlot[sourcePosition].transform.GetChild(1).name = newName[0] + "." + newName[1];
            }
            // BAT TOT QUA DUONG
            if (Mathf.Abs(chessLastPointBatTotQuaDuong) == sourcePosition)
            {
                if (chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("totred"))
                {
                    if (targetPosition - sourcePosition == 7 || targetPosition - sourcePosition == 9)
                    {
                        GameObject chessEat = chessPointToSlot[targetPosition - 8].transform.GetChild(1).gameObject;
                        GameObject chessEatBotParent = chessEatPlayerBotMoveClone.transform.parent.gameObject;
                        GameObject chessEatTopParent = chessEatPlayerTopMoveClone.transform.parent.gameObject;
                        chessEatPlayerBotMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                        chessEatPlayerTopMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                        chessEat.transform.SetParent(chessEat.transform.parent.parent.parent);
                        RectTransform rtfEat = chessEat.GetComponent<RectTransform>();
                        RectTransform rtfEatBot = chessEatPlayerBotMoveClone.GetComponent<RectTransform>();
                        RectTransform rtfEatTop = chessEatPlayerTopMoveClone.GetComponent<RectTransform>();
                        if (chessEat.name.Contains("red"))
                        {
                            DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatBot.anchoredPosition.x, rtfEatBot.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                chessEat.transform.SetParent(chessEatBotParent.transform);
                                chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                                chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                            });
                        }
                        else
                        {
                            DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatTop.anchoredPosition.x, rtfEatTop.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                chessEat.transform.SetParent(chessEatTopParent.transform);
                                chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                                chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                            });
                        }
                    }
                }
                else if (chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("totblack"))
                {
                    if (targetPosition - sourcePosition == -7 || targetPosition - sourcePosition == -9)
                    {
                        GameObject chessEat = chessPointToSlot[targetPosition + 8].transform.GetChild(1).gameObject;
                        GameObject chessEatBotParent = chessEatPlayerBotMoveClone.transform.parent.gameObject;
                        GameObject chessEatTopParent = chessEatPlayerTopMoveClone.transform.parent.gameObject;
                        chessEatPlayerBotMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                        chessEatPlayerTopMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                        chessEat.transform.SetParent(chessEat.transform.parent.parent.parent);
                        RectTransform rtfEat = chessEat.GetComponent<RectTransform>();
                        RectTransform rtfEatBot = chessEatPlayerBotMoveClone.GetComponent<RectTransform>();
                        RectTransform rtfEatTop = chessEatPlayerTopMoveClone.GetComponent<RectTransform>();
                        if (chessEat.name.Contains("red"))
                        {
                            DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatBot.anchoredPosition.x, rtfEatBot.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                chessEat.transform.SetParent(chessEatBotParent.transform);
                                chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                                chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                            });
                        }
                        else
                        {
                            DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatTop.anchoredPosition.x, rtfEatTop.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                chessEat.transform.SetParent(chessEatTopParent.transform);
                                chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                                chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                            });
                        }
                    }

                }
            }
            if (chessLastPointBatTotQuaDuong > 1)
            {
                string[] newName = chessPointToSlot[chessLastPointBatTotQuaDuong].transform.GetChild(1).name.Split('.');
                chessPointToSlot[chessLastPointBatTotQuaDuong].transform.GetChild(1).name = newName[0] + "." + newName[1];
                chessLastPointBatTotQuaDuong = -1;
                if (IsPlaying)
                {
                    PlayerPrefs.SetInt("BatTotQuaDuong", -1);
                }
            }
            if (chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("totred"))
            {
                if (Math.Abs(targetPosition - sourcePosition) == 16)
                {
                    if (chessPointToSlot[targetPosition + 1].transform.childCount > 1)
                        if (chessPointToSlot[targetPosition + 1].transform.GetChild(1).name.Contains("totblack"))
                        {
                            string[] newName = chessPointToSlot[targetPosition + 1].transform.GetChild(1).name.Split('.');
                            chessPointToSlot[targetPosition + 1].transform.GetChild(1).name = newName[0] + "." + newName[1];
                            chessPointToSlot[targetPosition + 1].transform.GetChild(1).name += ".battotquaduong+";
                            chessLastPointBatTotQuaDuong = targetPosition + 1;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("BatTotQuaDuong", targetPosition + 1);
                        }
                    if (chessPointToSlot[targetPosition - 1].transform.childCount > 1)
                        if (chessPointToSlot[targetPosition - 1].transform.GetChild(1).name.Contains("totblack"))
                        {
                            string[] newName = chessPointToSlot[targetPosition - 1].transform.GetChild(1).name.Split('.');
                            chessPointToSlot[targetPosition - 1].transform.GetChild(1).name = newName[0] + "." + newName[1];
                            chessPointToSlot[targetPosition - 1].transform.GetChild(1).name += ".battotquaduong-";
                            chessLastPointBatTotQuaDuong = targetPosition - 1;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("BatTotQuaDuong", (targetPosition - 1) * -1);
                        }
                }
                else if (Math.Abs(targetPosition - sourcePosition) == 8 || Math.Abs(targetPosition - sourcePosition) == 7 || Math.Abs(targetPosition - sourcePosition) == 9)
                {
                    string[] newName = chessPointToSlot[sourcePosition].transform.GetChild(1).name.Split('.');
                    chessPointToSlot[sourcePosition].transform.GetChild(1).name = newName[0] + "." + newName[1];
                }
            }
            else if (chessPointToSlot[sourcePosition].transform.GetChild(1).name.Contains("totblack"))
            {
                if (Math.Abs(targetPosition - sourcePosition) == 16)
                {
                    if (chessPointToSlot[targetPosition + 1].transform.childCount > 1)
                        if (chessPointToSlot[targetPosition + 1].transform.GetChild(1).name.Contains("totred"))
                        {
                            string[] newName = chessPointToSlot[targetPosition + 1].transform.GetChild(1).name.Split('.');
                            chessPointToSlot[targetPosition + 1].transform.GetChild(1).name = newName[0] + "." + newName[1];
                            chessPointToSlot[targetPosition + 1].transform.GetChild(1).name += ".battotquaduong+";
                            chessLastPointBatTotQuaDuong = targetPosition + 1;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("BatTotQuaDuong", targetPosition + 1);
                        }
                    if (chessPointToSlot[targetPosition - 1].transform.childCount > 1)
                        if (chessPointToSlot[targetPosition - 1].transform.GetChild(1).name.Contains("totred"))
                        {
                            string[] newName = chessPointToSlot[targetPosition - 1].transform.GetChild(1).name.Split('.');
                            chessPointToSlot[targetPosition - 1].transform.GetChild(1).name = newName[0] + "." + newName[1];
                            chessPointToSlot[targetPosition - 1].transform.GetChild(1).name += ".battotquaduong-";
                            chessLastPointBatTotQuaDuong = targetPosition - 1;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("BatTotQuaDuong", (targetPosition - 1) * -1);
                        }
                }
                else if (Math.Abs(targetPosition - sourcePosition) == 8 || Math.Abs(targetPosition - sourcePosition) == 7 || Math.Abs(targetPosition - sourcePosition) == 9)
                {
                    string[] newName = chessPointToSlot[sourcePosition].transform.GetChild(1).name.Split('.');
                    chessPointToSlot[sourcePosition].transform.GetChild(1).name = newName[0] + "." + newName[1];
                }
            }
            // Nhập Thành View
            if (chessPointToSlot[sourcePosition].transform.GetChild(1).gameObject.name.Contains("kingred") && sourcePosition == 4)
            {
                if (targetPosition == 6)
                {
                    RectTransform rtfNT = chessPointToSlot[7].transform.GetChild(1).gameObject.GetComponent<RectTransform>();
                    RectTransform rtfTargetNT = chessPointToSlot[5].GetComponent<RectTransform>();
                    GameObject chessNT = chessPointToSlot[7].transform.GetChild(1).gameObject;
                    chessNT.transform.SetParent(chessPointToSlot[sourcePosition].transform.parent);
                    DOTween.To(() => rtfNT.anchoredPosition, x => rtfNT.anchoredPosition = x, new Vector2(rtfTargetNT.anchoredPosition.x, rtfTargetNT.anchoredPosition.y), .2f).OnComplete(() =>
                    {
                        chessNT.transform.SetParent(chessPointToSlot[5].transform);
                    });
                }
                else if (targetPosition == 2)
                {

                    RectTransform rtfNT = chessPointToSlot[0].transform.GetChild(1).gameObject.GetComponent<RectTransform>();
                    RectTransform rtfTargetNT = chessPointToSlot[3].GetComponent<RectTransform>();
                    GameObject chessNT = chessPointToSlot[0].transform.GetChild(1).gameObject;
                    chessNT.transform.SetParent(chessPointToSlot[sourcePosition].transform.parent);
                    DOTween.To(() => rtfNT.anchoredPosition, x => rtfNT.anchoredPosition = x, new Vector2(rtfTargetNT.anchoredPosition.x, rtfTargetNT.anchoredPosition.y), .2f).OnComplete(() =>
                    {
                        chessNT.transform.SetParent(chessPointToSlot[3].transform);
                    });
                }
            }
            else if (chessPointToSlot[sourcePosition].transform.GetChild(1).gameObject.name.Contains("kingblack") && sourcePosition == 60)
            {
                if (targetPosition == 62)
                {
                    RectTransform rtfNT = chessPointToSlot[63].transform.GetChild(1).gameObject.GetComponent<RectTransform>();
                    RectTransform rtfTargetNT = chessPointToSlot[61].GetComponent<RectTransform>();
                    GameObject chessNT = chessPointToSlot[63].transform.GetChild(1).gameObject;
                    chessNT.transform.SetParent(chessPointToSlot[sourcePosition].transform.parent);
                    DOTween.To(() => rtfNT.anchoredPosition, x => rtfNT.anchoredPosition = x, new Vector2(rtfTargetNT.anchoredPosition.x, rtfTargetNT.anchoredPosition.y), .2f).OnComplete(() =>
                    {
                        chessNT.transform.SetParent(chessPointToSlot[61].transform);
                    });
                }
                else if (targetPosition == 58)
                {

                    RectTransform rtfNT = chessPointToSlot[56].transform.GetChild(1).gameObject.GetComponent<RectTransform>();
                    RectTransform rtfTargetNT = chessPointToSlot[59].GetComponent<RectTransform>();
                    GameObject chessNT = chessPointToSlot[56].transform.GetChild(1).gameObject;
                    chessNT.transform.SetParent(chessPointToSlot[sourcePosition].transform.parent);
                    DOTween.To(() => rtfNT.anchoredPosition, x => rtfNT.anchoredPosition = x, new Vector2(rtfTargetNT.anchoredPosition.x, rtfTargetNT.anchoredPosition.y), .2f).OnComplete(() =>
                    {
                        chessNT.transform.SetParent(chessPointToSlot[59].transform);
                    });
                }
            }
            
            //
            GameObject chessMove = chessPointToSlot[sourcePosition].transform.GetChild(1).gameObject;
            chessMove.transform.SetParent(chessMove.transform.parent.parent);
            RectTransform rtf = chessMove.GetComponent<RectTransform>();
            RectTransform rtfTarget = chessPointToSlot[targetPosition].GetComponent<RectTransform>();
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfTarget.anchoredPosition.x, rtfTarget.anchoredPosition.y), .25f).OnComplete(() =>
            {
                if (chessPointToSlot[targetPosition].transform.childCount > 1)
                {
                    //App.trace("TOTAL MOVE OPEN 1 " + chessTotalMove);
                    GameObject chessEat = chessPointToSlot[targetPosition].transform.GetChild(1).gameObject;
                    GameObject chessEatBotParent = chessEatPlayerBotMoveClone.transform.parent.gameObject;
                    GameObject chessEatTopParent = chessEatPlayerTopMoveClone.transform.parent.gameObject;
                    chessEatPlayerBotMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                    chessEatPlayerTopMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                    chessEat.transform.SetParent(chessEat.transform.parent.parent.parent);
                    RectTransform rtfEat = chessEat.GetComponent<RectTransform>();
                    RectTransform rtfEatBot = chessEatPlayerBotMoveClone.GetComponent<RectTransform>();
                    RectTransform rtfEatTop = chessEatPlayerTopMoveClone.GetComponent<RectTransform>();
                    if (chessEat.name.Contains("red"))
                    {
                        DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatBot.anchoredPosition.x, rtfEatBot.anchoredPosition.y), .25f).OnComplete(() =>
                        {
                            chessEat.transform.SetParent(chessEatBotParent.transform);
                            chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                            chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                        });
                    }
                    else
                    {
                        DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatTop.anchoredPosition.x, rtfEatTop.anchoredPosition.y), .25f).OnComplete(() =>
                        {
                            chessEat.transform.SetParent(chessEatTopParent.transform);
                            chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                            chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                        });
                    }
                }
                // NHAP THANH
                if (chessNhapThanhRed)
                {
                    if (sourcePosition == 4)
                    {
                        if (targetPosition < sourcePosition && targetPosition == 2)
                        {
                                chessNhapThanhRed = false;
                                if (IsPlaying)
                                    PlayerPrefs.SetInt("NhapThanhRed", 0);
                        }
                        else if (targetPosition > sourcePosition && targetPosition == 6)
                        {
                                chessNhapThanhRed = false;
                                if (IsPlaying)
                                    PlayerPrefs.SetInt("NhapThanhRed", 0);                           
                        }
                    }
                }
                if (chessNhapThanhBlack)
                {
                    if (sourcePosition == 60)
                    {
                        if (targetPosition < sourcePosition && targetPosition == 58)
                        {
                                chessNhapThanhBlack = false;
                                if (IsPlaying)
                                    PlayerPrefs.SetInt("NhapThanhBlack", 0);
                            
                        }
                        else if (targetPosition > sourcePosition && targetPosition == 62)
                        {
                                chessNhapThanhBlack = false;
                                if (IsPlaying)
                                    PlayerPrefs.SetInt("NhapThanhBlack", 0);
                            
                        }
                    }
                }
                //
                //LoadingControl.instance.playSound("step");
                chessMove.transform.SetParent(chessPointToSlot[targetPosition].transform);
                chessLastStep = chessPointToSlot[sourcePosition];
                chessLastStep.GetComponent<Image>().enabled = true;
                chessNextStep = chessPointToSlot[targetPosition].transform.GetChild(1).GetChild(0).gameObject;
                chessNextStep.SetActive(true);
                chessTargetNextStep = targetPosition;
            });
        });

        var req_HIGHLIGHT = new OutBounMessage("HIGHLIGHT");
        req_HIGHLIGHT.addHead();
        App.ws.sendHandler(req_HIGHLIGHT.getReq(), delegate (InBoundMessage res)
        {
            App.trace(" HIGHLIGHT ");
            int position = res.readByte();
            //LoadingControl.instance.playSound("highlight");
            chessAttackKing[preTimeLeapId].SetActive(true);
            StartCoroutine(chessCloseAttackKing(preTimeLeapId));
            chessPointToSlot[position].transform.GetChild(1).GetComponent<Animator>().enabled = true;
            StartCoroutine(chessCloseDangerousKing(chessPointToSlot[position].transform.GetChild(1).gameObject));
        });

        var req_GAMEOVER = new OutBounMessage("GAMEOVER");
        req_GAMEOVER.addHead();
        App.ws.sendHandler(req_GAMEOVER.getReq(), delegate (InBoundMessage res)
        {
            App.trace(" GAMEOVER ");
            IsPlaying = false;
            if (chessWaitBack)
            {
                StartCoroutine(chessCheckQuit());
            }
            //else
            //{
                if (chessPlayerPlaying)
                {
                    //App.trace("clientCurrentMode " + CPlayer.clientCurrentMode.ToString());
                    if (CPlayer.clientCurrentMode == 0)
                    {
                        chessButton[0].SetActive(true);
                    }
                    else
                    {
                        chessButton[1].SetActive(true);
                    }
                }
                run = false;
                for (int i = 0; i < chessTimeToTurn.Length; i++)
                {
                    chessTimeToTurn[i].SetActive(false);
                }
                int count = res.readByte();
                //App.trace("Chess GameOver - total " + count);
                for (int i = 0; i < count; i++)
                {
                    int slotId = res.readByte();
                    //App.trace("Chess GameOver - slotId " + slotId);
                    int grade = res.readByte();
                    //App.trace("Chess GameOver - grade " + grade);
                    int earnValue = (int)res.readLong();
                    //App.trace("Chess GameOver - earnValue " + earnValue);
                    if (mySlotID == slotId)
                    {
                        if (earnValue > 0)
                        {
                            chessEffectShowValue[1].SetActive(true);
                            chessEffectShowValue[0].SetActive(true);
                            StartCoroutine(chessCloseEffect());
                            //LoadingControl.instance.playSound("win");
                        }
                        else if (earnValue < 0)
                        {
                            chessEffectShowValue[2].SetActive(true);
                            chessEffectShowValue[0].SetActive(true);
                            StartCoroutine(chessCloseEffect());
                            //LoadingControl.instance.playSound("lost");
                        }
                        else
                        {
                            chessEffectShowValue[3].SetActive(true);
                            chessEffectShowValue[0].SetActive(true);
                            StartCoroutine(chessCloseEffect());
                            //LoadingControl.instance.playSound("draw");
                        }
                    }
                    if (slotId == 1)
                    {
                        if (earnValue > 0)
                        {
                            chessShowValueText[0].GetComponent<Text>().text = "+" + earnValue.ToString();
                            chessShowValueText[0].SetActive(true);
                            StartCoroutine(chessPlayer1Text(chipPlayer1, chipPlayer1 + earnValue, .2f));
                            StartCoroutine(_chessShowValue(chessShowValueText[0]));
                            StartCoroutine(chessCloseEffect());
                        }
                        else if (earnValue < 0)
                        {
                            chessShowValueText[1].GetComponent<Text>().text = earnValue.ToString();
                            chessShowValueText[1].SetActive(true);
                            StartCoroutine(chessPlayer1Text(chipPlayer1, chipPlayer1 + earnValue, .2f));
                            StartCoroutine(_chessShowValue(chessShowValueText[1]));
                            StartCoroutine(chessCloseEffect());
                        }
                        else
                        {
                            chessShowValueText[4].GetComponent<Text>().text = "HÒA";
                            chessShowValueText[4].SetActive(true);
                            StartCoroutine(_chessShowValue(chessShowValueText[4]));
                            StartCoroutine(chessCloseEffect());
                        }

                    }
                    else
                    {
                        if (earnValue > 0)
                        {
                            chessShowValueText[2].GetComponent<Text>().text = "+" + earnValue.ToString();
                            chessShowValueText[2].SetActive(true);
                            StartCoroutine(chessPlayer2Text(chipPlayer2, chipPlayer2 + earnValue, .2f));
                            StartCoroutine(_chessShowValue(chessShowValueText[2]));
                            StartCoroutine(chessCloseEffect());
                        }
                        else if (earnValue < 0)
                        {
                            chessShowValueText[3].GetComponent<Text>().text = earnValue.ToString();
                            chessShowValueText[3].SetActive(true);
                            StartCoroutine(chessPlayer2Text(chipPlayer2, chipPlayer2 + earnValue, .2f));
                            StartCoroutine(_chessShowValue(chessShowValueText[3]));
                            StartCoroutine(chessCloseEffect());
                        }
                        else
                        {
                            chessShowValueText[5].GetComponent<Text>().text = "HÒA";
                            chessShowValueText[5].SetActive(true);
                            StartCoroutine(_chessShowValue(chessShowValueText[5]));
                            StartCoroutine(chessCloseEffect());
                        }
                    }
                    if (earnValue > 0)
                    {
                        chessPlayerPopupWinner[slotId + 2].SetActive(true);
                        chessPlayerPopupWinner[slotId + 4].SetActive(false);
                        chessPlayerPopupWinner[0].GetComponentsInChildren<Text>()[1].text = chessNickNamePlayer[slotId];
                        chessPlayerPopupWinner[0].GetComponentsInChildren<Text>()[2].text = "+" + earnValue.ToString();
                        StartCoroutine(App.loadImg(chessPlayerPopupWinner[0].GetComponentsInChildren<Image>()[2], App.getAvatarLink2(chessAvatarPlayer[slotId] + "", chessAvartarIdPlayer[slotId])));
                    }
                    else if (earnValue < 0)
                    {
                        chessPlayerPopupWinner[slotId + 2].SetActive(true);
                        chessPlayerPopupWinner[slotId + 4].SetActive(false);
                        chessPlayerPopupWinner[1].GetComponentsInChildren<Text>()[1].text = chessNickNamePlayer[slotId];
                        chessPlayerPopupWinner[1].GetComponentsInChildren<Text>()[2].text = earnValue.ToString();
                        StartCoroutine(App.loadImg(chessPlayerPopupWinner[1].GetComponentsInChildren<Image>()[2], App.getAvatarLink2(chessAvatarPlayer[slotId] + "", chessAvartarIdPlayer[slotId])));
                    }
                    else
                    {
                        chessPlayerPopupWinner[slotId + 2].SetActive(false);
                        chessPlayerPopupWinner[slotId + 4].SetActive(true);
                        chessPlayerPopupWinner[slotId].GetComponentsInChildren<Text>()[0].text = chessNickNamePlayer[slotId];
                        chessPlayerPopupWinner[slotId].GetComponentsInChildren<Text>()[1].text = earnValue.ToString();
                        StartCoroutine(App.loadImg(chessPlayerPopupWinner[slotId].GetComponentsInChildren<Image>()[2], App.getAvatarLink2(chessAvatarPlayer[slotId] + "", chessAvartarIdPlayer[slotId])));
                    }
                }
                string matchResult = res.readString();
                //App.trace("Chess GameOver - matchResult " + matchResult);
            //}
        });

        var req_SET_TURN = new OutBounMessage("SET_TURN");
        req_SET_TURN.addHead();
        App.ws.sendHandler(req_SET_TURN.getReq(), delegate (InBoundMessage res)
        {
            App.trace(" SET_TURN ");
            int slotId = res.readByte();
            //App.trace("Chess - Slot Id " + slotId);
            int turnTimeOut = res.readShort();
            //App.trace("Chess - turnTimeOut " + turnTimeOut);
            int remainDuration = res.readShort();
            //App.trace("Chess - remainDuration " + remainDuration);
            //App.trace("chessPlayerPlaying " + chessPlayerPlaying + " || " + "CPlayer.clientCurrentMode " + CPlayer.clientCurrentMode + " || " + "mySlotID " + mySlotID);
            if (slotId >= 0)
            {
                run = true;
                preTimeLeapId = slotId;
                time = turnTimeOut;
                curr = remainDuration;
                timecd = 1;
                for (int i = 0; i < chessTimeToTurn.Length; i++)
                {
                    chessTimeToTurn[i].SetActive(false);
                    chessTimeToTurn[i].GetComponent<Image>().fillAmount = 1;
                    chessTimeRemain[i].color = new Color32(255, 255, 255, 255);
                    chessTimeMainImage[i].sprite = chessSprite[1];
                }
                chessTimeToTurn[slotId].SetActive(true);
                preTimeLeapImage = chessTimeToTurn[slotId].GetComponent<Image>();
                chessTimeRemain[slotId].color = new Color32(255, 190, 19, 255);
                chessTimeRemain[slotId].text = (curr / 60).ToString() + ":" + string.Format("{0:00}", (curr % 60)).ToString();
                chessTimeMainImage[slotId].sprite = chessSprite[0];
            }
        });

        var req_SLOT_IN_TABLE_CHANGED = new OutBounMessage("SLOT_IN_TABLE_CHANGED");
        req_SLOT_IN_TABLE_CHANGED.addHead();
        //req_getTableChange.print();
        App.ws.sendHandler(req_SLOT_IN_TABLE_CHANGED.getReq(), delegate (InBoundMessage res)
        {

            var nickName = res.readAscii();
            //App.trace("Chess Table Change -nickName " + nickName);
            var slotId = res.readByte();
            //App.trace("Chess Table Change - Slot Id " + slotId);
            var chipBalance = res.readLong();
            //App.trace("Chess Table Change - chipBalance " + chipBalance);
            var score = res.readLong();
            //App.trace("Chess Table Change - score " + score);
            var level = res.readByte();
            //App.trace("Chess Table Change - level " + level);
            var avatarId = res.readShort();
            //App.trace("Chess Table Change - avatarId " + avatarId);
            var avatar = res.readAscii();
            //App.trace("Chess Table Change - avatar " + avatar);
            var isMale = res.readByte() == 1;
            //App.trace("Chess Table Change - isMale " + isMale);
            var isOwner = res.readByte() == 1;
            //App.trace("Chess Table Change - isOwner " + isOwner);
            var playerId = res.readLong();
            //App.trace("Chess Table Change  - playerId " + playerId);
            var starBalance = res.readLong();
            //App.trace("SLOT_IN_TABLE_CHANGED slot = " + slotId + "nick = " + nickName);

            if (slotId == 1)
            {
                if (nickName == "")
                {
                    App.trace("changeeeeeeeeeee");
                    chessTableText[2].text = "";
                    chessTableText[3].text = "";
                    chessTableText[4].text = "";
                    chessGameObjectImage[0].SetActive(false);
                    chessGameObjectImage[2].SetActive(true);
                    chessGameObjectImage[6].SetActive(false);
                    chessGameObjectImage[4].SetActive(false);
                    chessGameObjectImage[8].SetActive(false);
                    if (chessChangePositionPlayer)
                    {
                        chessInfoPlayer[0].transform.SetSiblingIndex(0);
                        chessInfoPlayer[1].transform.SetSiblingIndex(2);
                        chessInfoPlayer[2].transform.SetSiblingIndex(1);
                        chessInfoPlayer[3].transform.SetSiblingIndex(3);
                        chessGrid.enabled = true;
                        chessGrid.startCorner = GridLayoutGroup.Corner.LowerLeft;
                        StartCoroutine(chessCloseGridLayout());
                        chessChangePositionPlayer = false;
                    }
                }
                else
                {
                    chessPlayeSlotId[slotId] = slotId;
                    chessPlayerName[slotId] = nickName;
                    chessNickNamePlayer[1] = nickName;
                    chessAvartarIdPlayer[1] = avatarId;
                    chessAvatarPlayer[1] = avatar;
                    chessTableText[2].text = nickName;
                    chessTableText[3].text = App.formatMoney(chipBalance.ToString());
                    chipPlayer1 = chipBalance;
                    chessTableText[4].text = "Level " + level.ToString();
                    StartCoroutine(App.loadImg(chessGameObjectImage[0].GetComponentsInChildren<Image>()[1], App.getAvatarLink2(avatar + "", avatarId)));
                    chessGameObjectImage[0].SetActive(true);
                    chessGameObjectImage[2].SetActive(false);
                    chessGameObjectImage[6].SetActive(true);
                    chessGameObjectImage[8].SetActive(true);
                    chessGameObjectImage[8].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        chessShowInfoPopUp(nickName, (int)playerId, (int)chipBalance, avatar, avatarId);
                    });
                    if (isOwner)
                    {
                        chessGameObjectImage[4].SetActive(true);
                    }
                    if (playerId == CPlayer.id)
                    {
                        if (isOwner)
                        {
                            isOwerChess = true;
                        }
                        CPlayer.clientCurrentMode = 1;
                        chessPlayerPlaying = true;
                        chessInfoPlayer[0].transform.SetSiblingIndex(2);
                        chessInfoPlayer[1].transform.SetSiblingIndex(0);
                        chessInfoPlayer[2].transform.SetSiblingIndex(3);
                        chessInfoPlayer[3].transform.SetSiblingIndex(1);
                        chessChangePositionPlayer = true;
                        chessGrid.enabled = true;
                        chessGrid.startCorner = GridLayoutGroup.Corner.UpperRight;
                        StartCoroutine(chessCloseGridLayout());
                    }
                }
            }
            else if (slotId == 0)
            {
                if (nickName == "")
                {
                    App.trace("changeeeeeeeeeee");
                    chessTableText[5].text = "";
                    chessTableText[6].text = "";
                    chessTableText[7].text = "";
                    chessGameObjectImage[1].SetActive(false);
                    chessGameObjectImage[3].SetActive(true);
                    chessGameObjectImage[7].SetActive(false);
                    chessGameObjectImage[5].SetActive(false);
                    chessGameObjectImage[9].SetActive(false);
                }
                else
                {
                    chessPlayeSlotId[slotId] = slotId;
                    chessPlayerName[slotId] = nickName;
                    chessNickNamePlayer[0] = nickName;
                    chessAvartarIdPlayer[0] = avatarId;
                    chessAvatarPlayer[0] = avatar;
                    chessTableText[5].text = nickName;
                    chessTableText[6].text = App.formatMoney(chipBalance.ToString());
                    chipPlayer2 = chipBalance;
                    chessTableText[7].text = "Level " + level.ToString();
                    StartCoroutine(App.loadImg(chessGameObjectImage[1].GetComponentsInChildren<Image>()[1], App.getAvatarLink2(avatar + "", avatarId)));
                    chessGameObjectImage[1].SetActive(true);
                    chessGameObjectImage[3].SetActive(false);
                    chessGameObjectImage[7].SetActive(true);
                    chessGameObjectImage[9].SetActive(true);
                    chessGameObjectImage[9].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        chessShowInfoPopUp(nickName, (int)playerId, (int)chipBalance, avatar, avatarId);
                    });
                    if (isOwner)
                    {
                        chessGameObjectImage[5].SetActive(true);
                    }
                    if (playerId == CPlayer.id)
                    {
                        if (isOwner)
                        {
                            isOwerChess = true;
                        }
                        CPlayer.clientCurrentMode = 1;
                        chessPlayerPlaying = true;
                    }
                }
            }
            if (chessPlayerPlaying && CPlayer.clientCurrentMode == 1 && isOwerChess && chessGameObjectImage[0].activeSelf == true && chessGameObjectImage[1].activeSelf == true)
            {
                chessButtonStart.SetActive(true);
            }
            else
            {
                chessButtonStart.SetActive(false);
            }
        });

        var req_OWNER_CHANGED = new OutBounMessage("OWNER_CHANGED");
        req_OWNER_CHANGED.addHead();
        App.ws.sendHandler(req_OWNER_CHANGED.getReq(), delegate (InBoundMessage res)
        {
            App.trace(" OWNER_CHANGED ");
            int slotId = res.readByte();
            if (slotId == mySlotID)
            {
                isOwerChess = true;
            }
            else
            {
                isOwerChess = false;
            }
            App.trace("Chess Ower - slot Ower change " + slotId);
        });


        var req_ASK_DRAW = new OutBounMessage("ASK_DRAW");
        req_ASK_DRAW.addHead();
        App.ws.sendHandler(req_ASK_DRAW.getReq(), delegate (InBoundMessage res)
        {
            App.trace("ASK_DRAW REVICE");
        });

        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        App.ws.sendHandler(req_START_MATCH.getReq(), delegate (InBoundMessage res)
        {
            for (int z = 0; z < chessPointToSlot.Length; z++)
            {
                if (chessPointToSlot[z].transform.childCount > 1)
                    chessPointToSlot[z].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
            }
            chessTableClearPointMove();
            chessTargetNextStep = -1;
            App.trace(" START_MATCH ");
            /*
             * TO DO:
             * 1. Set điểm
             * 2. Load lại Board Data
             * 3. So sánh mySlotId và 0( Lớn hơn 0 thì bđầu chơi| Nhỏ hơn 0 thì là view)
             * */
            chessNhapThanhRed = true;
            chessNhapThanhBlack = true;
            chessLastPointBatTotQuaDuong = -1;
            if (IsPlaying)
            {
                PlayerPrefs.SetInt("BatTotQuaDuong", -1);
                PlayerPrefs.SetInt("NhapThanhBlack", 1);
                PlayerPrefs.SetInt("NhapThanhRed", 1);
            }
            //LoadingControl.instance.playSound("start");
            IsPlaying = true;
            if (chessPlayerPlaying)
            {
                chessButton[0].SetActive(false);
                chessButton[1].SetActive(false);
            }
            if (chessLastStep != null && chessNextStep != null)
            {
                chessLastStep.GetComponent<Image>().enabled = false;
                chessNextStep.SetActive(false);
            }
            /*
            for (int i = 0; i < chessPointToSlot.Length; i++)
            {
                if (chessPointToSlot[i].transform.childCount > 1)
                {
                    Destroy(chessPointToSlot[i].transform.GetChild(1).gameObject);
                }
                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            for (int i = chessEatPlayerBotElement.transform.parent.GetChildCount() - 1; i > 1; i--)
            {
                DestroyImmediate(chessEatPlayerBotElement.transform.parent.GetChild(i).gameObject);
            }
            for (int i = chessEatPlayerTopElement.transform.parent.GetChildCount() - 1; i > 1; i--)
            {
                DestroyImmediate(chessEatPlayerTopElement.transform.parent.GetChild(i).gameObject);
            }
            */
            for (int i = 0; i < chessTimeRemain.Length; i++)
            {
                chessTimeRemain[i].text = "";
            }
            run = true;
            StartCoroutine(chessCountDownMainTime());
            loadPlayerMatchPoint(res);
            loadBoardData(res, true);
        });

        var req_CHANGE_PIECE = new OutBounMessage("CHANGE_PIECE");
        req_CHANGE_PIECE.addHead();
        App.ws.sendHandler(req_CHANGE_PIECE.getReq(), delegate (InBoundMessage res)
        {
            int id = res.readByte();
            //App.trace("idFace " + id);
            int newFacePiece = res.readByte();
            //App.trace("newFacePiece " + newFacePiece);
            StartCoroutine(chessChangeFace(newFacePiece, id));
        });
    }

    IEnumerator chessPlayer1Text(long source, long dir, float time)
    {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            long a = (long)Mathf.Lerp(source, dir, i);
            chessTableText[3].text = App.formatMoney(a.ToString());
            yield return null;
        }
        chipPlayer1 = dir;
    }
    IEnumerator chessPlayer2Text(long source, long dir, float time)
    {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            long a = (long)Mathf.Lerp(source, dir, i);
            chessTableText[6].text = App.formatMoney(a.ToString());
            yield return null;
        }
        chipPlayer2 = dir;
    }
    IEnumerator chessCheckQuit()
    {
        chessTimeToTurn[0].SetActive(false);
        chessTimeToTurn[1].SetActive(false);
        yield return new WaitForSeconds(3f);        
        chessOpenWaitRoom();
    }
    IEnumerator chessCloseGridLayout()
    {
        yield return new WaitForSeconds(.1f);
        chessGrid.enabled = false;
    }
    // Change Face Update
    IEnumerator chessRotationChangeFace(float time, int numberFaceChess, GameObject ico, Vector3 startRotation, Vector3 endRotation)
    {
        //App.trace("ROTATIONNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN ");
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            ico.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, i);
            yield return null;
        }
        ico.transform.eulerAngles = new Vector3(0, 0, 0);
        ico.GetComponent<Image>().sprite = chessTuongFace[numberFaceChess];
    }

    IEnumerator chessChangeFace(int face, int id)
    {
        yield return new WaitForSeconds(.4f);
        GameObject chessPieceChangeFace = chessPointToSlot[chessCurrDirPoint].transform.GetChild(1).gameObject;
        switch (face)
        {
            case 40:
            case 41:
                StartCoroutine(chessRotationChangeFace(0.5f, 8, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "mared" + "." + id + ".phongto";
                break;
            case 32:
            case 33:
                StartCoroutine(chessRotationChangeFace(0.5f, 10, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "tuongred" + "." + id + ".phongto";
                break;
            case 16:
                StartCoroutine(chessRotationChangeFace(0.5f, 6, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "haured" + "." + id + ".phongto";
                break;
            case 24:
            case 25:
                StartCoroutine(chessRotationChangeFace(0.5f, 11, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "xered" + "." + id + ".phongto";
                break;
            case -40:
            case -41:
                StartCoroutine(chessRotationChangeFace(0.5f, 2, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "mablack" + "." + id + ".phongto";
                break;
            case -32:
            case -33:
                StartCoroutine(chessRotationChangeFace(0.5f, 4, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "tuongblack" + "." + id + ".phongto";
                break;
            case -16:
                StartCoroutine(chessRotationChangeFace(0.5f, 0, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "haublack" + "." + id + ".phongto";
                break;
            case -24:
            case -25:
                StartCoroutine(chessRotationChangeFace(0.5f, 5, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "xeblack" + "." + id + ".phongto";
                break;
        }
        /*
        if (face > 0)
        {
            chessPieceChangeFace.name = chessPieceChangeFace.name + "red";
        }
        else
        {
            chessPieceChangeFace.name = chessPieceChangeFace.name + "black";
        }
        */
    }
    // Effect
    IEnumerator chessCloseEffect()
    {
        yield return new WaitForSeconds(1f);
        RectTransform rtfPopUpShowValue = chessPopupShowValueMatch.GetComponent<RectTransform>();
        chessBlackPanel.SetActive(true);
        chessPopupShowValueMatch.SetActive(true);
        DOTween.To(() => rtfPopUpShowValue.anchoredPosition, x => rtfPopUpShowValue.anchoredPosition = x, Vector2.zero, .3f).OnComplete(() =>
        {
        });
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < chessEffectShowValue.Length; i++)
        {
            chessEffectShowValue[i].SetActive(false);
        }
    }
    public void chessShowValue(GameObject Object)
    {
        RectTransform rtf = Object.GetComponent<RectTransform>();
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtf.anchoredPosition.x, rtf.anchoredPosition.y + 40), 2f).OnComplete(() =>
        {
            Object.SetActive(false);
            StartCoroutine(chessShowButtonStart());
        });
    }
    IEnumerator _chessShowValue(GameObject Object)
    {
        yield return new WaitForSeconds(1f);
        chessShowValue(Object);
    }
    IEnumerator chessShowButtonStart()
    {
        yield return new WaitForSeconds(4f);
        if (isOwerChess)
        {
            chessButtonStart.SetActive(true);
        }
    }
    IEnumerator chessCloseAttackKing(int number)
    {
        yield return new WaitForSeconds(4f);
        chessAttackKing[number].SetActive(false);
    }
    IEnumerator chessCloseDangerousKing(GameObject king)
    {
        yield return new WaitForSeconds(2f);
        king.GetComponent<Animator>().enabled = false;
        king.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }
    // Popup Show Value
    public void chessClosePopupShowValue()
    {
        RectTransform rtfPopUpShowValue = chessPopupShowValueMatch.GetComponent<RectTransform>();
        DOTween.To(() => rtfPopUpShowValue.anchoredPosition, x => rtfPopUpShowValue.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
        {
            chessPopupShowValueMatch.SetActive(false);
            chessBlackPanel.SetActive(false);
        });
    }
    public void chessGetTableDataEx()
    {
        //App.trace("===========\nSTART getTableDataEx");
        var req_GET_TABLE_DATA_EX = new OutBounMessage("GET_TABLE_DATA_EX");
        req_GET_TABLE_DATA_EX.addHead();
        req_GET_TABLE_DATA_EX.writeAcii("");
        //req_getTableChange.print();
        App.ws.send(req_GET_TABLE_DATA_EX.getReq(), delegate (InBoundMessage res)
        {
            loadStateData(res);
            loadTableData(res);
            loadPlayerMatchPoint(res);
            loadBoardData(res);

        });
    }
    public void loadStateData(InBoundMessage res)
    {
        int count = res.readByte();
        //App.trace("loadStateData - count " + count);
        for (int i = 0; i < count; i++)
        {
            int a = res.readByte();
            //App.trace("loadStateData - a " + a);
            string b = res.readAscii();
            //App.trace("loadStateData - b " + b);
            int c = res.readByte();
            //App.trace("loadStateData - c " + c);

            int commandCount = res.readByte();
            //App.trace("loadStateData - commandCount " + commandCount);
            for (int j = 0; j < commandCount; j++)
            {
                int d = res.readByte();
                //App.trace("loadStateData - d " + d);
                string e = res.readAscii();
                //App.trace("loadStateData - e " + e);
                string f = res.readString();
                //App.trace("loadStateData - f " + f);
                int g = res.readByte();
                //App.trace("loadStateData - g " + g);
                int h = res.readByte();
                //App.trace("loadStateData - h " + h);
            }
        }
        int k = res.readByte();
        //App.trace("loadStateData - k " + k);
    }
    private string[] chessNickNamePlayer = new string[2];
    private int[] chessAvartarIdPlayer = new int[2];
    private string[] chessAvatarPlayer = new string[2];
    private string[] chessPlayerName;
    private int[] chessPlayeSlotId;
    private bool IsPlaying;
    public void loadTableData(InBoundMessage res)
    {
        int mySlotId = res.readByte();
        mySlotID = mySlotId;
        //App.trace("loadTableData - mySlotId " + mySlotId);
        bool isPlaying = res.readByte() == 1;
        IsPlaying = isPlaying;
        //App.trace("loadTableData - isPlaying " + isPlaying);
        int count = res.readByte();
        chessPlayerName = new string[2];
        chessPlayeSlotId = new int[2];
        //App.trace("loadTableData - count " + count);
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            chessPlayeSlotId[i] = slotId;
            //App.trace("loadTableData - slotId " + slotId);
            long playerId = res.readLong();
            //App.trace("loadTableData - playerId " + playerId);
            string nickName = res.readAscii();
            chessPlayerName[i] = nickName;
            //App.trace("loadTableData - nickName " + nickName);
            int avatarId = res.readShort();
            //App.trace("loadTableData - avatarId " + avatarId);
            string avatar = res.readAscii();
            //App.trace("loadTableData - avatar " + avatar);
            bool isMale = res.readByte() == 1;
            //App.trace("loadTableData - isMale " + isMale);
            long chipBalance = res.readLong();
            //App.trace("loadTableData - chipBalance " + chipBalance);
            long starBalance = res.readLong();
            //App.trace("loadTableData - starBalance " + starBalance);
            long score = res.readLong();
            //App.trace("loadTableData - score " + score);
            int level = res.readByte();
            //App.trace("loadTableData - level " + level);
            bool isOwner = res.readByte() == 1;
            //App.trace("loadTableData - isOwner " + isOwner);
            if (count > 0)
            {
                if (slotId == 1)
                {
                    chessNickNamePlayer[1] = nickName;
                    chessAvartarIdPlayer[1] = avatarId;
                    chessAvatarPlayer[1] = avatar;
                    chessTableText[2].text = nickName;
                    chessTableText[3].text = App.formatMoney(chipBalance.ToString());
                    chipPlayer1 = chipBalance;
                    chessTableText[4].text = "Level " + level.ToString();
                    StartCoroutine(App.loadImg(chessGameObjectImage[0].GetComponentsInChildren<Image>()[1], App.getAvatarLink2(avatar + "", avatarId)));
                    chessGameObjectImage[0].SetActive(true);
                    chessGameObjectImage[2].SetActive(false);
                    chessGameObjectImage[6].SetActive(true);
                    chessGameObjectImage[8].SetActive(true);
                    chessGameObjectImage[8].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        chessShowInfoPopUp(nickName, (int)playerId, (int)chipBalance, avatar, avatarId);
                    });
                    if (isOwner)
                    {
                        chessGameObjectImage[4].SetActive(true);
                    }
                    if (playerId == CPlayer.id)
                    {
                        if (isOwner)
                        {
                            isOwerChess = true;
                        }
                        chessMyColor = "black";
                        CPlayer.clientCurrentMode = 1;
                        chessPlayerPlaying = true;
                        chessInfoPlayer[0].transform.SetSiblingIndex(2);
                        chessInfoPlayer[1].transform.SetSiblingIndex(0);
                        chessInfoPlayer[2].transform.SetSiblingIndex(3);
                        chessInfoPlayer[3].transform.SetSiblingIndex(1);
                        chessChangePositionPlayer = true;
                        chessGrid.enabled = true;
                        chessGrid.startCorner = GridLayoutGroup.Corner.UpperRight;
                        StartCoroutine(chessCloseGridLayout());
                    }
                }
                else if (slotId == 0)
                {
                    chessNickNamePlayer[0] = nickName;
                    chessAvartarIdPlayer[0] = avatarId;
                    chessAvatarPlayer[0] = avatar;
                    chessTableText[5].text = nickName;
                    chessTableText[6].text = App.formatMoney(chipBalance.ToString());
                    chipPlayer2 = chipBalance;
                    chessTableText[7].text = "Level " + level.ToString();
                    StartCoroutine(App.loadImg(chessGameObjectImage[1].GetComponentsInChildren<Image>()[1], App.getAvatarLink2(avatar + "", avatarId)));
                    chessGameObjectImage[1].SetActive(true);
                    chessGameObjectImage[3].SetActive(false);
                    chessGameObjectImage[7].SetActive(true);
                    chessGameObjectImage[9].SetActive(true);
                    chessGameObjectImage[9].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        chessShowInfoPopUp(nickName, (int)playerId, (int)chipBalance, avatar, avatarId);
                    });
                    if (isOwner)
                    {
                        chessGameObjectImage[5].SetActive(true);
                    }
                    if (playerId == CPlayer.id)
                    {
                        if (isOwner)
                        {
                            isOwerChess = true;
                        }
                        chessMyColor = "red";
                        chessPlayerPlaying = true;
                        CPlayer.clientCurrentMode = 1;
                    }
                }
            }

        }
        int currentTurnSlotId = res.readByte();
        //App.trace("loadTableData - currentTurnSlotId " + currentTurnSlotId);
        int currentTime = res.readShort();
        //App.trace("loadTableData - currentTime " + currentTime);
        int slotRemainDuration = res.readShort();
        //App.trace("loadTableData - slotRemainDuration " + slotRemainDuration);
        var currentState = res.readByte();
        //App.trace("loadTableData - currentState " + currentState);

        if (isPlaying)
        {
            if (chessPlayerPlaying)
            {
                chessButton[0].SetActive(false);
                chessButton[1].SetActive(false);
            }
            run = true;
            preTimeLeapId = currentTurnSlotId;
            time = currentTime;
            curr = slotRemainDuration;
            timecd = 1;
            for (int i = 0; i < chessTimeToTurn.Length; i++)
            {
                chessTimeToTurn[i].SetActive(false);
                chessTimeToTurn[i].GetComponent<Image>().fillAmount = 1;
                chessTimeRemain[i].color = new Color32(255, 255, 255, 255);
            }
            chessTimeToTurn[currentTurnSlotId].SetActive(true);
            preTimeLeapImage = chessTimeToTurn[currentTurnSlotId].GetComponent<Image>();
            chessTimeRemain[currentTurnSlotId].color = new Color32(255, 190, 19, 255);
            chessTimeRemain[currentTurnSlotId].text = (curr / 60).ToString() + ":" + string.Format("{0:00}", (curr % 60)).ToString();
            chessTimeMainImage[currentTurnSlotId].sprite = chessSprite[0];
            StartCoroutine(chessCountDownMainTime());
        }
        if (isPlaying == false && count == 2 && isOwerChess)
        {
            chessButtonStart.SetActive(true);
        }
    }
    // Point Match
    public void loadPlayerMatchPoint(InBoundMessage res)
    {
        int count = res.readByte();
        for (int i = 0; i < count; i++)
        {
            int slotId = res.readByte();
            //App.trace("loadTableData - slotId " + slotId);
            int point = res.readInt();
            //App.trace("loadTableData - point " + point);
            chessPoint[slotId].text = point.ToString();
        }
    }
    // Point To Move
    public void loadBoardData(InBoundMessage res, bool start = false)
    {
        int count = res.readByte();
        //App.trace("loadBoardData - count " + count);
        for (int i = 0; i < count; i++)
        {
            int sid = res.readByte();
            //App.trace("loadBoardData - sid " + i + "||" + sid);
            int face = res.readByte();
            //App.trace("loadBoardData - face " + i + "||" + face);
            int position = res.readByte();
            //App.trace("loadBoardData - position " + i + "||" + position);
            bool open = res.readByte() == 1;
            //App.trace("loadBoardData - open " + i + "||" + open);            
            if (position > -1)
            {
                if (start)
                {
                    for (int j = 0; j < chessPointToSlot.Length; j++)
                    {
                        if (chessPointToSlot[j].transform.childCount > 1 && chessPointToSlot[j].transform.GetChild(1).name.Contains(sid.ToString()) && chessPointToSlot[j].transform.GetChild(1).name.Contains(".phongto"))
                        {
                            if (sid > 0)
                            {
                                chessPointToSlot[j].transform.GetChild(1).GetComponent<Image>().sprite = chessTuongFace[9];
                                chessPointToSlot[j].transform.GetChild(1).name = sid + "totred" + "." + sid;
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(1).GetComponent<Image>().sprite = chessTuongFace[3];
                                chessPointToSlot[j].transform.GetChild(1).name = sid + "totblack" + "." + sid;
                            }
                        }
                        if (chessPointToSlot[j].transform.childCount > 1 && chessPointToSlot[j].transform.GetChild(1).name.Contains(sid.ToString()) && chessPointToSlot[j].transform.GetChild(1).name.Contains("." + sid.ToString()))
                        {
                            GameObject chessStart = chessPointToSlot[j].transform.GetChild(1).gameObject;
                            RectTransform rtf = chessStart.GetComponent<RectTransform>();
                            RectTransform rtfPos = chessPointToSlot[position].GetComponent<RectTransform>();
                            chessStart.transform.SetParent(chessStart.transform.parent.parent);
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                if (chessStart.name.Contains("kingred") || chessStart.name.Contains("xered") || chessStart.name.Contains("kingblack") || chessStart.name.Contains("xeblack"))
                                {
                                    chessStart.name += ".nomove";
                                }
                                chessStart.transform.SetParent(chessPointToSlot[position].transform);
                            });
                        }
                    }
                    for (int j = chessEatPlayerBotElement.transform.parent.childCount - 1; j > 1; j--)
                    {
                        if (chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains(".phongto"))
                        {
                            if (sid > 0)
                            {
                                chessEatPlayerBotElement.transform.parent.GetChild(j).GetComponent<Image>().sprite = chessTuongFace[9];
                                chessEatPlayerBotElement.transform.parent.GetChild(j).name = sid + "totred" + "." + sid;
                            }
                            else
                            {
                                chessEatPlayerBotElement.transform.parent.GetChild(j).GetComponent<Image>().sprite = chessTuongFace[3];
                                chessEatPlayerBotElement.transform.parent.GetChild(j).name = sid + "totblack" + "." + sid;
                            }
                        }
                        if (chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains("." + sid.ToString()))
                        {
                            GameObject chessStart = chessEatPlayerBotElement.transform.parent.GetChild(j).gameObject;
                            RectTransform rtf = chessStart.GetComponent<RectTransform>();
                            RectTransform rtfPos = chessPointToSlot[position].GetComponent<RectTransform>();
                            chessStart.transform.SetParent(chessPointToSlot[position].transform.parent.parent);
                            rtf.sizeDelta = new Vector2(105, 116);
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                if (chessStart.name.Contains("kingred") || chessStart.name.Contains("xered") || chessStart.name.Contains("kingblack") || chessStart.name.Contains("xeblack"))
                                {
                                    chessStart.name += ".nomove";
                                }
                                chessStart.transform.SetParent(chessPointToSlot[position].transform);
                            });
                        }
                    }
                    for (int j = chessEatPlayerTopElement.transform.parent.childCount - 1; j > 1; j--)
                    {
                        if (chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains(".phongto"))
                        {
                            if (sid > 0)
                            {
                                chessEatPlayerTopElement.transform.parent.GetChild(j).GetComponent<Image>().sprite = chessTuongFace[9];
                                chessEatPlayerTopElement.transform.parent.GetChild(j).name = sid + "totred" + "." + sid;
                            }
                            else
                            {
                                chessEatPlayerTopElement.transform.parent.GetChild(j).GetComponent<Image>().sprite = chessTuongFace[3];
                                chessEatPlayerTopElement.transform.parent.GetChild(j).name = sid + "totblack" + "." + sid;
                            }
                        }
                        if (chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains("." + sid.ToString()))
                        {
                            GameObject chessStart = chessEatPlayerTopElement.transform.parent.GetChild(j).gameObject;
                            RectTransform rtf = chessStart.GetComponent<RectTransform>();
                            RectTransform rtfPos = chessPointToSlot[position].GetComponent<RectTransform>();
                            chessStart.transform.SetParent(chessPointToSlot[position].transform.parent.parent);
                            rtf.sizeDelta = new Vector2(105, 116);
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                if (chessStart.name.Contains("kingred") || chessStart.name.Contains("xered") || chessStart.name.Contains("kingblack") || chessStart.name.Contains("xeblack"))
                                {
                                    chessStart.name += ".nomove";
                                }
                                chessStart.transform.SetParent(chessPointToSlot[position].transform);
                            });
                        }
                    }

                }
                else
                {
                    GameObject chessClone = Instantiate(chessElement, chessPointToSlot[position].transform, false);
                    switch (face)
                    {
                        case 48:
                        case 49:
                        case 50:
                        case 51:
                        case 52:
                        case 53:
                        case 54:
                        case 55:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[9];
                            chessClone.name = sid + "totred" + "." + sid;
                            break;
                        case 40:
                        case 41:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[8];
                            chessClone.name = sid + "mared" + "." + sid;
                            break;
                        case 32:
                        case 33:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[10];
                            chessClone.name = sid + "tuongred" + "." + sid;
                            break;
                        case 16:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                            chessClone.name = sid + "haured" + "." + sid;
                            break;
                        case 24:
                        case 25:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[11];
                            if (PlayerPrefs.GetInt("NhapThanhRed") == 1 && IsPlaying)
                                chessClone.name = sid + "xered" + "." + sid + ".nomove";
                            else
                                chessClone.name = sid + "xered" + "." + sid;
                            break;
                        case 8:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[7];
                            if (PlayerPrefs.GetInt("NhapThanhRed") == 1 && IsPlaying)
                                chessClone.name = sid + "kingred" + "." + sid + ".nomove";
                            else
                                chessClone.name = sid + "kingred" + "." + sid;
                            break;
                        case -48:
                        case -49:
                        case -50:
                        case -51:
                        case -52:
                        case -53:
                        case -54:
                        case -55:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[3];
                            chessClone.name = sid + "totblack" + "." + sid;
                            break;
                        case -40:
                        case -41:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[2];
                            chessClone.name = sid + "mablack" + "." + sid;
                            break;
                        case -32:
                        case -33:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[4];
                            chessClone.name = sid + "tuongblack" + "." + sid;
                            break;
                        case -16:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[0];
                            chessClone.name = sid + "haublack" + "." + sid;
                            break;
                        case -24:
                        case -25:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[5];
                            if (PlayerPrefs.GetInt("NhapThanhBlack") == 1 && IsPlaying)
                                chessClone.name = sid + "xeblack" + "." + sid + ".nomove";
                            else
                                chessClone.name = sid + "xeblack" + "." + sid;
                            break;
                        case -8:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[1];
                            if (PlayerPrefs.GetInt("NhapThanhBlack") == 1 && IsPlaying)
                                chessClone.name = sid + "kingblack" + "." + sid + ".nomove";
                            else
                                chessClone.name = sid + "kingblack" + "." + sid;
                            break;
                    }
                    if (sid != face)
                    {
                        chessClone.name = chessClone.name + ".phongto";
                    }
                    if (position == Mathf.Abs(PlayerPrefs.GetInt("BatTotQuaDuong")) && IsPlaying)
                    {
                        chessLastPointBatTotQuaDuong = Mathf.Abs(PlayerPrefs.GetInt("BatTotQuaDuong"));
                        if (PlayerPrefs.GetInt("BatTotQuaDuong") < -1)
                            chessPointToSlot[position].transform.GetChild(1).name += ".battotquaduong-";
                        else if (PlayerPrefs.GetInt("BatTotQuaDuong") > 1)
                            chessPointToSlot[position].transform.GetChild(1).name += ".battotquaduong+";
                    }
                    /*
                    if (face > 0)
                    {
                        chessClone.name = chessClone.name + "red";
                    }
                    else
                    {
                        chessClone.name = chessClone.name + "black";
                    }
                    */
                    chessClone.transform.position = chessPointToSlot[position].transform.position;
                    chessClone.SetActive(true);
                }
            }
            else
            {
                if (face > 0)
                {
                    GameObject chessClone = Instantiate(chessEatPlayerBotElement, chessEatPlayerBotElement.transform.parent, false);
                    switch (face)
                    {
                        case 48:
                        case 49:
                        case 50:
                        case 51:
                        case 52:
                        case 53:
                        case 54:
                        case 55:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[9];
                            chessClone.name = sid + "totred" + "." + sid;
                            break;
                        case 40:
                        case 41:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[8];
                            chessClone.name = sid + "mared" + "." + sid;
                            break;
                        case 32:
                        case 33:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[10];
                            chessClone.name = sid + "tuongred" + "." + sid;
                            break;
                        case 16:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                            chessClone.name = sid + "haured" + "." + sid;
                            break;
                        case 24:
                        case 25:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[11];
                            chessClone.name = sid + "xered" + "." + sid;
                            break;
                        case 8:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[7];
                            chessClone.name = sid + "kingred" + "." + sid;
                            break;
                    }
                    if (sid != face)
                    {
                        chessClone.name = chessClone.name + ".phongto";
                    }
                    chessClone.SetActive(true);

                }
                else
                {
                    GameObject chessClone = Instantiate(chessEatPlayerTopElement, chessEatPlayerTopElement.transform.parent, false);
                    switch (face)
                    {
                        case -48:
                        case -49:
                        case -50:
                        case -51:
                        case -52:
                        case -53:
                        case -54:
                        case -55:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[3];
                            chessClone.name = sid + "totblack" + "." + sid;
                            break;
                        case -40:
                        case -41:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[2];
                            chessClone.name = sid + "mablack" + "." + sid;
                            break;
                        case -32:
                        case -33:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[4];
                            chessClone.name = sid + "tuongblack" + "." + sid;
                            break;
                        case -16:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[0];
                            chessClone.name = sid + "haublack" + "." + sid;
                            break;
                        case -24:
                        case -25:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[5];
                            chessClone.name = sid + "xeblack" + "." + sid;
                            break;
                        case -8:
                            chessClone.GetComponent<Image>().sprite = chessTuongFace[1];
                            chessClone.name = sid + "kingblack" + "." + sid;
                            break;
                    }
                    if (sid != face)
                    {
                        chessClone.name = chessClone.name + ".phongto";
                    }
                    chessClone.SetActive(true);

                }

            }


        }
        if (!start)
        {
            chessEatPlayerBotMoveClone = Instantiate(chessEatPlayerBotMove, chessEatPlayerBotMove.transform.parent, false);
            chessEatPlayerBotMoveClone.SetActive(true);
            chessEatPlayerTopMoveClone = Instantiate(chessEatPlayerTopMove, chessEatPlayerTopMove.transform.parent, false);
            chessEatPlayerTopMoveClone.SetActive(true);
        }

        int lastMove = res.readByte();
        //App.trace("loadBoardData - lastMove " + lastMove);
        if (lastMove >= 0)
        {
            chessLastStep = chessPointToSlot[lastMove];
            chessLastStep.GetComponent<Image>().enabled = true;
        }
        int nextMove = res.readByte();
        if (nextMove >= 0)
        {
            chessNextStep = chessPointToSlot[nextMove].transform.GetChild(1).GetChild(0).gameObject;
            chessNextStep.SetActive(true);
            chessTargetNextStep = nextMove;
        }
        //App.trace("loadBoardData - nextMove " + nextMove);
        // app.board.setLastMove(response.readByte(), res.readByte());
    }
    private int[] chessPointChanged;
    public void chessTableClearPointMove()
    {

        if (chessPointChange != null)
        {
            App.trace("|||||||||||||||||||||||||| point change " + chessPointChange);
            chessPointChanged = chessPointChange.Split('-').Select(n => Convert.ToInt32(n)).ToArray();
            for (int i = 0; i < chessPointChanged.Length; i++)
            {
                if (chessPointToSlot[chessPointChanged[i]].transform.GetChild(0).name.Contains("red") || chessPointToSlot[chessPointChanged[i]].transform.GetChild(0).name.Contains("black"))
                {
                    chessPointToSlot[chessPointChanged[i]].transform.GetChild(0).SetAsLastSibling();
                }
            }
        }
        for (int i = 0; i < chessPointToSlot.Length; i++)
        {
            chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    private string chessPointChange;
    private bool chessNhapThanhRed = false;
    private bool chessNhapThanhBlack = false;
    private int chessLastClickPoint = -1;
    public void chessTableControllPiece(string namePiece, string pointName, string type)
    {
        //chessNhapThanh = false;
        for (int z = 0; z < chessPointToSlot.Length; z++)
        {
            if (chessPointToSlot[z].transform.childCount > 1)
                chessPointToSlot[z].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
        }
        chessSourcePointDrag = pointName;
        chessPointChange = null;
        string getnumber = Regex.Match(pointName, @"\d+").Value;
        int point = Int32.Parse(getnumber);
        //App.trace("Naaaaaaaaaaaaaaaaaaaaaameeeeeeeeeeee " + namePiece + "|| " + pointName + "|| " + type);
        // ChangeName
        if (namePiece.Contains("mared"))
        {
            namePiece = "mared";
        }
        else if (namePiece.Contains("mablack"))
        {
            namePiece = "mablack";
        }
        else if (namePiece.Contains("xered"))
        {
            namePiece = "xered";
        }
        else if (namePiece.Contains("xeblack"))
        {
            namePiece = "xeblack";
        }
        else if (namePiece.Contains("totred"))
        {
            namePiece = "totred";
        }
        else if (namePiece.Contains("totblack"))
        {
            namePiece = "totblack";
        }
        else if (namePiece.Contains("tuongred"))
        {
            namePiece = "tuongred";
        }
        else if (namePiece.Contains("tuongblack"))
        {
            namePiece = "tuongblack";
        }
        else if (namePiece.Contains("haured"))
        {
            namePiece = "haured";
        }
        else if (namePiece.Contains("haublack"))
        {
            namePiece = "haublack";
        }
        else if (namePiece.Contains("kingred"))
        {
            namePiece = "kingred";
        }
        else if (namePiece.Contains("kingblack"))
        {
            namePiece = "kingblack";
        }
        // Set Move Piece
        int i = point;
        int j = point;
        int k = point;
        int l = point;
        int m = point;
        int n = point;
        switch (namePiece)
        {
            case "totred":
                if (i < 56)
                {
                    if (i == 8 || i == 9 || i == 10 || i == 11 || i == 12 || i == 13 || i == 14 || i == 15)
                    {
                        chessMovePieceTot(i + 8);
                        if (chessPointToSlot[i + 8].transform.childCount > 1)
                        {

                        }
                        else
                        {
                            chessMovePieceTot(i + 16);
                        }
                    }
                    else
                    {
                        chessMovePieceTot(i + 8);
                    }
                    if (i != 8 && i != 16 && i != 24 && i != 32 && i != 40 && i != 48)
                        chessMovePieceTotEat(i + 7, type);
                    if (i != 15 && i != 23 && i != 31 && i != 39 && i != 47 && i != 55)
                        chessMovePieceTotEat(i + 9, type);
                    if (chessPointToSlot[point].transform.GetChild(1).name.Contains("battotquaduong+"))
                    {
                        chessMovePieceTot(i + 7);
                    }
                    if (chessPointToSlot[point].transform.GetChild(1).name.Contains("battotquaduong-"))
                    {
                        chessMovePieceTot(i + 9);
                    }
                }
                break;
            case "totblack":
                if (i > 7)
                {
                    if (i == 48 || i == 49 || i == 50 || i == 51 || i == 52 || i == 53 || i == 54 || i == 55)
                    {
                        chessMovePieceTot(i - 8);
                        if (chessPointToSlot[i - 8].transform.childCount > 1)
                        {

                        }
                        else
                        {
                            chessMovePieceTot(i - 16);
                        }
                    }
                    else
                    {
                        chessMovePieceTot(i - 8);
                    }
                    if (i != 8 && i != 16 && i != 24 && i != 32 && i != 40 && i != 48)
                        chessMovePieceTotEat(i - 9, type);
                    if (i != 15 && i != 23 && i != 31 && i != 39 && i != 47 && i != 55)
                        chessMovePieceTotEat(i - 7, type);
                    if (chessPointToSlot[point].transform.GetChild(1).name.Contains("battotquaduong+"))
                    {
                        chessMovePieceTot(i - 9);
                    }
                    if (chessPointToSlot[point].transform.GetChild(1).name.Contains("battotquaduong-"))
                    {
                        chessMovePieceTot(i - 7);
                    }
                }
                break;
            case "mared":
            case "mablack":
                if (i > 55)
                {
                    if (i != 62 && i != 63)
                        chessMovePiece(i - 6, type);
                    if (i != 56 && i != 57)
                        chessMovePiece(i - 10, type);
                    if (i != 63)
                        chessMovePiece(i - 15, type);
                    if (i != 56)
                        chessMovePiece(i - 17, type);
                }
                else if (i > 47)
                {
                    if (i != 48 && i != 49)
                    {
                        chessMovePiece(i + 6, type);
                        chessMovePiece(i - 10, type);
                    }
                    if (i != 54 && i != 55)
                    {
                        chessMovePiece(i + 10, type);
                        chessMovePiece(i - 6, type);
                    }
                    if (i != 55)
                        chessMovePiece(i - 15, type);
                    if (i != 48)
                        chessMovePiece(i - 17, type);
                }
                else if (i < 8)
                {
                    if (i != 0 && i != 1)
                        chessMovePiece(i + 6, type);
                    if (i != 6 && i != 7)
                        chessMovePiece(i + 10, type);
                    if (i != 0)
                        chessMovePiece(i + 15, type);
                    if (i != 7)
                        chessMovePiece(i + 17, type);
                }
                else if (i < 16)
                {
                    if (i != 14 && i != 15)
                    {
                        chessMovePiece(i - 6, type);
                        chessMovePiece(i + 10, type);
                    }
                    if (i != 8 && i != 9)
                    {
                        chessMovePiece(i + 6, type);
                        chessMovePiece(i - 10, type);
                    }
                    if (i != 8)
                        chessMovePiece(i + 15, type);
                    if (i != 15)
                        chessMovePiece(i + 17, type);
                }
                else
                {
                    if (i != 22 && i != 23 && i != 30 && i != 31 && i != 38 && i != 39 && i != 46 && i != 47)
                    {
                        chessMovePiece(i - 6, type);
                        chessMovePiece(i + 10, type);
                    }
                    if (i != 16 && i != 17 && i != 24 && i != 25 && i != 32 && i != 33 && i != 40 && i != 41)
                    {
                        chessMovePiece(i + 6, type);
                        chessMovePiece(i - 10, type);
                    }
                    if (i != 16 && i != 24 && i != 32 && i != 40)
                    {
                        chessMovePiece(i + 15, type);
                        chessMovePiece(i - 17, type);
                    }
                    if (i != 23 && i != 31 && i != 39 && i != 47)
                    {
                        chessMovePiece(i - 15, type);
                        chessMovePiece(i + 17, type);
                    }
                }
                break;
            case "xeblack":
            case "xered":
                if (point >= 56)
                {
                    while (i >= 0)
                    {
                        if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                        {
                            if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = i.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + i.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                            i -= 8;
                        }
                    }
                }
                else if (point <= 7)
                {
                    while (i <= 63)
                    {
                        if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                        {
                            if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = i.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + i.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                            i += 8;
                        }
                    }
                }
                else
                {
                    while (i >= 0)
                    {
                        if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                        {
                            if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = i.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + i.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                            i -= 8;
                        }
                    }
                    while (j <= 63)
                    {
                        if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                        {
                            if (chessPointToSlot[j].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[j].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = j.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + j.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                            j += 8;
                        }
                    }
                }
                chessTableControllPieceTankRow(point, type);
                break;
            case "tuongred":
            case "tuongblack":
                if (point >= 56)
                {
                    if (i == 56)
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 7 || i == 15 || i == 23 || i == 31 || i == 39 || i == 47 || i == 55 || i == 63)
                                    break;
                                i -= 7;
                            }
                        }
                    }
                    else if (i == 63)
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i -= 9;
                            }
                        }
                    }
                    else
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i -= 9;
                            }
                        }
                        while (j >= 0)
                        {
                            if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                            {
                                if (chessPointToSlot[j].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[j].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = j.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + j.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                if (j == 7 || j == 15 || j == 23 || j == 31 || j == 39 || j == 47 || j == 55 || j == 63)
                                    break;
                                j -= 7;

                            }
                        }
                    }
                }
                else if (i <= 7)
                {
                    if (i == 0)
                    {
                        while (i <= 63)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 7 || i == 15 || i == 23 || i == 31 || i == 39 || i == 47 || i == 55 || i == 63)
                                    break;
                                i += 9;
                            }
                        }
                    }
                    else if (i == 7)
                    {
                        while (i <= 63)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i += 7;
                            }
                        }
                    }
                    else
                    {
                        while (i <= 63)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 7 || i == 15 || i == 23 || i == 31 || i == 39 || i == 47 || i == 55 || i == 63)
                                    break;
                                i += 9;
                            }
                        }
                        while (j <= 63)
                        {
                            if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                            {
                                if (chessPointToSlot[j].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[j].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = j.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + j.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                if (j == 0 || j == 8 || j == 16 || j == 24 || j == 32 || j == 40 || j == 48 || j == 56)
                                    break;
                                j += 7;
                            }
                        }
                    }
                }
                else
                {
                    if (i != 8 && i != 16 && i != 24 && i != 32 && i != 40 && i != 48)
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i -= 9;
                            }
                        }
                        while (j <= 63)
                        {
                            if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                            {
                                if (chessPointToSlot[j].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[j].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = j.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + j.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                if (j == 0 || j == 8 || j == 16 || j == 24 || j == 32 || j == 40 || j == 48 || j == 56)
                                    break;
                                j += 7;
                            }
                        }
                    }
                    if (i != 15 && i != 23 && i != 31 && i != 39 && i != 47 && i != 55)
                    {
                        while (k <= 63)
                        {
                            if (chessPointToSlot[k].transform.childCount > 1 && k != point)
                            {
                                if (chessPointToSlot[k].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[k].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[k].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = k.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + k.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[k].transform.GetChild(0).gameObject.SetActive(true);
                                if (k == 7 || k == 15 || k == 23 || k == 31 || k == 39 || k == 47 || k == 55 || k == 63)
                                    break;
                                k += 9;
                            }
                        }
                        while (l >= 0)
                        {
                            if (chessPointToSlot[l].transform.childCount > 1 && l != point)
                            {
                                if (chessPointToSlot[l].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[l].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[l].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = l.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + l.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[l].transform.GetChild(0).gameObject.SetActive(true);
                                if (l == 7 || l == 15 || l == 23 || l == 31 || l == 39 || l == 47 || l == 55 || l == 63)
                                    break;
                                l -= 7;
                            }
                        }
                    }
                }
                break;
            case "kingred":
            case "kingblack":
                if (i > 55)
                {
                    if (i != 56)
                    {
                        chessMovePiece(i - 1, type);
                        chessMovePiece(i - 9, type);
                    }
                    if (i != 63)
                    {
                        chessMovePiece(i + 1, type);
                        chessMovePiece(i - 7, type);
                    }
                    chessMovePiece(i - 8, type);
                }
                else if (i < 8)
                {
                    if (i != 0)
                    {
                        chessMovePiece(i - 1, type);
                        chessMovePiece(i + 7, type);
                    }
                    if (i != 7)
                    {
                        chessMovePiece(i + 1, type);
                        chessMovePiece(i + 9, type);
                    }
                    chessMovePiece(i + 8, type);
                }
                else
                {
                    if (i != 48 && i != 40 && i != 32 && i != 24 && i != 16 && i != 8)
                    {
                        chessMovePiece(i - 1, type);
                        chessMovePiece(i + 7, type);
                        chessMovePiece(i - 9, type);
                    }
                    if (i != 15 && i != 23 && i != 31 && i != 39 && i != 47 && i != 55)
                    {
                        chessMovePiece(i + 1, type);
                        chessMovePiece(i + 9, type);
                        chessMovePiece(i - 7, type);
                    }
                    chessMovePiece(i - 8, type);
                    chessMovePiece(i + 8, type);
                }
                // CHECK NHAP THANH
                if (point == 4 && chessPointToSlot[point].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[point].transform.GetChild(1).name.Contains("kingred") && chessPointToSlot[1].transform.childCount < 2 && chessPointToSlot[2].transform.childCount < 2 && chessPointToSlot[3].transform.childCount < 2)
                {
                    if (chessPointToSlot[0].transform.childCount > 1)
                        if (chessPointToSlot[0].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[0].transform.GetChild(1).name.Contains("xered"))
                        {
                            chessMovePiece(2, type);
                            chessNhapThanhRed = true;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhRed", 1);
                        }
                        else
                        {
                            chessNhapThanhRed = false;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhRed", 0);
                        }
                }
                if (point == 4 && chessPointToSlot[point].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[point].transform.GetChild(1).name.Contains("kingred") && chessPointToSlot[5].transform.childCount < 2 && chessPointToSlot[6].transform.childCount < 2)
                {
                    if (chessPointToSlot[7].transform.childCount > 1)
                        if (chessPointToSlot[7].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[7].transform.GetChild(1).name.Contains("xered"))
                        {
                            chessMovePiece(6, type);
                            chessNhapThanhRed = true;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhRed", 1);
                        }
                        else
                        {
                            chessNhapThanhRed = false;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhRed", 0);
                        }
                }
                if (point == 60 && chessPointToSlot[point].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[point].transform.GetChild(1).name.Contains("kingblack") && chessPointToSlot[57].transform.childCount < 2 && chessPointToSlot[58].transform.childCount < 2 && chessPointToSlot[59].transform.childCount < 2)
                {
                    if (chessPointToSlot[56].transform.childCount > 1)
                        if (chessPointToSlot[56].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[56].transform.GetChild(1).name.Contains("xeblack"))
                        {
                            chessMovePiece(58, type);
                            chessNhapThanhBlack = true;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhBlack", 1);
                        }
                        else
                        {
                            chessNhapThanhBlack = false;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhBalack", 0);
                        }
                }
                if (point == 60 && chessPointToSlot[point].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[point].transform.GetChild(1).name.Contains("kingblack") && chessPointToSlot[61].transform.childCount < 2 && chessPointToSlot[62].transform.childCount < 2)
                {
                    if (chessPointToSlot[63].transform.childCount > 1)
                        if (chessPointToSlot[63].transform.GetChild(1).name.Contains("nomove") && chessPointToSlot[63].transform.GetChild(1).name.Contains("xeblack"))
                        {
                            chessMovePiece(62, type);
                            chessNhapThanhBlack = true;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhBlack", 1);
                        }
                        else
                        {
                            chessNhapThanhBlack = false;
                            if (IsPlaying)
                                PlayerPrefs.SetInt("NhapThanhBlack", 0);
                        }
                }
                break;
            case "haured":
            case "haublack":
                if (point >= 56)
                {
                    while (m >= 0)
                    {
                        if (chessPointToSlot[m].transform.childCount > 1 && m != point)
                        {
                            if (chessPointToSlot[m].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[m].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[m].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = m.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + m.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[m].transform.GetChild(0).gameObject.SetActive(true);
                            m -= 8;
                        }
                    }
                    if (i == 56)
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 7 || i == 15 || i == 23 || i == 31 || i == 39 || i == 47 || i == 55 || i == 63)
                                    break;
                                i -= 7;
                            }
                        }
                    }
                    else if (i == 63)
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i -= 9;
                            }
                        }
                    }
                    else
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i -= 9;
                            }
                        }
                        while (j >= 0)
                        {
                            if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                            {
                                if (chessPointToSlot[j].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[j].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = j.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + j.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                if (j == 7 || j == 15 || j == 23 || j == 31 || j == 39 || j == 47 || j == 55 || j == 63)
                                    break;
                                j -= 7;

                            }
                        }
                    }
                }
                else if (i <= 7)
                {
                    while (m <= 63)
                    {
                        if (chessPointToSlot[m].transform.childCount > 1 && m != point)
                        {
                            if (chessPointToSlot[m].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[m].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[m].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = m.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + m.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[m].transform.GetChild(0).gameObject.SetActive(true);
                            m += 8;
                        }
                    }
                    if (i == 0)
                    {
                        while (i <= 63)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 7 || i == 15 || i == 23 || i == 31 || i == 39 || i == 47 || i == 55 || i == 63)
                                    break;
                                i += 9;
                            }
                        }
                    }
                    else if (i == 7)
                    {
                        while (i <= 63)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i += 7;
                            }
                        }
                    }
                    else
                    {
                        while (i <= 63)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 7 || i == 15 || i == 23 || i == 31 || i == 39 || i == 47 || i == 55 || i == 63)
                                    break;
                                i += 9;
                            }
                        }
                        while (j <= 63)
                        {
                            if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                            {
                                if (chessPointToSlot[j].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[j].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = j.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + j.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                if (j == 0 || j == 8 || j == 16 || j == 24 || j == 32 || j == 40 || j == 48 || j == 56)
                                    break;
                                j += 7;
                            }
                        }
                    }
                }
                else
                {
                    while (m >= 0)
                    {
                        if (chessPointToSlot[m].transform.childCount > 1 && m != point)
                        {
                            if (chessPointToSlot[m].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[m].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[m].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = m.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + m.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[m].transform.GetChild(0).gameObject.SetActive(true);
                            m -= 8;
                        }
                    }
                    while (n <= 63)
                    {
                        if (chessPointToSlot[n].transform.childCount > 1 && n != point)
                        {
                            if (chessPointToSlot[n].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[n].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[n].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = n.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + n.ToString();
                                }
                                break;
                            }
                        }
                        else
                        {
                            chessPointToSlot[n].transform.GetChild(0).gameObject.SetActive(true);
                            n += 8;
                        }
                    }
                    if (i != 8 && i != 16 && i != 24 && i != 32 && i != 40 && i != 48)
                    {
                        while (i >= 0)
                        {
                            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                            {
                                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = i.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + i.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                                if (i == 0 || i == 8 || i == 16 || i == 24 || i == 32 || i == 40 || i == 48 || i == 56)
                                    break;
                                i -= 9;
                            }
                        }
                        while (j <= 63)
                        {
                            if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                            {
                                if (chessPointToSlot[j].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[j].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = j.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + j.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                                if (j == 0 || j == 8 || j == 16 || j == 24 || j == 32 || j == 40 || j == 48 || j == 56)
                                    break;
                                j += 7;
                            }
                        }
                    }
                    if (i != 15 && i != 23 && i != 31 && i != 39 && i != 47 && i != 55)
                    {
                        while (k <= 63)
                        {
                            if (chessPointToSlot[k].transform.childCount > 1 && k != point)
                            {
                                if (chessPointToSlot[k].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[k].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[k].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = k.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + k.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[k].transform.GetChild(0).gameObject.SetActive(true);
                                if (k == 7 || k == 15 || k == 23 || k == 31 || k == 39 || k == 47 || k == 55 || k == 63)
                                    break;
                                k += 9;
                            }
                        }
                        while (l >= 0)
                        {
                            if (chessPointToSlot[l].transform.childCount > 1 && l != point)
                            {
                                if (chessPointToSlot[l].transform.GetChild(1).name.Contains(type))
                                {
                                    break;
                                }
                                else
                                {
                                    chessPointToSlot[l].transform.GetChild(0).gameObject.SetActive(true);
                                    chessPointToSlot[l].transform.GetChild(0).SetAsLastSibling();
                                    if (chessPointChange == null)
                                    {
                                        chessPointChange = l.ToString();
                                    }
                                    else
                                    {
                                        chessPointChange = chessPointChange + "-" + l.ToString();
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                chessPointToSlot[l].transform.GetChild(0).gameObject.SetActive(true);
                                if (l == 7 || l == 15 || l == 23 || l == 31 || l == 39 || l == 47 || l == 55 || l == 63)
                                    break;
                                l -= 7;
                            }
                        }
                    }
                }
                chessTableControllPieceTankRow(point, type);
                break;
        }
        if (point != chessTargetNextStep && chessLastClickPoint != point)
        {
            chessPointToSlot[point].transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
            chessLastClickPoint = point;
        }
        else if (chessLastClickPoint == point && !dragMode)
        {
            chessPointToSlot[point].transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
            chessLastClickPoint = -1;
            chessTableClearPointMove();
        }
    }
    public bool dragMode = false;
    public void chessMovePieceTot(int point)
    {
        if (chessPointToSlot[point].transform.childCount > 1)
        {

        }
        else
        {
            chessPointToSlot[point].transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    public void chessMovePieceTotEat(int point, string type)
    {
        if (chessPointToSlot[point].transform.childCount > 1)
        {
            if (chessPointToSlot[point].transform.GetChild(1).name.Contains(type))
            {
                //break;
            }
            else
            {
                chessPointToSlot[point].transform.GetChild(0).gameObject.SetActive(true);
                chessPointToSlot[point].transform.GetChild(0).SetAsLastSibling();
                if (chessPointChange == null)
                {
                    chessPointChange = point.ToString();
                }
                else
                {
                    chessPointChange = chessPointChange + "-" + point.ToString();
                }
            }
        }
    }
    public void chessMovePiece(int point, string type)
    {
        if (chessPointToSlot[point].transform.childCount > 1)
        {
            if (chessPointToSlot[point].transform.GetChild(1).name.Contains(type))
            {
                //break;
            }
            else
            {
                chessPointToSlot[point].transform.GetChild(0).gameObject.SetActive(true);
                chessPointToSlot[point].transform.GetChild(0).SetAsLastSibling();
                if (chessPointChange == null)
                {
                    chessPointChange = point.ToString();
                }
                else
                {
                    chessPointChange = chessPointChange + "-" + point.ToString();
                }
            }
        }
        else
        {
            chessPointToSlot[point].transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    private int checkRight;
    private int checkLeft;
    public void chessTableControllPieceTankRow(int point, string type)
    {
        if (point >= 0 && point <= 7)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 7, 0, type);
        }
        else if (point >= 8 && point <= 15)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 15, 8, type);
        }
        else if (point >= 16 && point <= 23)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 23, 16, type);
        }
        else if (point >= 24 && point <= 31)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 31, 24, type);
        }
        else if (point >= 32 && point <= 39)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 39, 32, type);
        }
        else if (point >= 40 && point <= 47)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 47, 40, type);
        }
        else if (point >= 48 && point <= 55)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 55, 48, type);
        }
        else if (point >= 56 && point <= 63)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 63, 56, type);
        }
    }
    public void chessTableControllMoveAndEatPieceTankRow(int point, int max, int min, string type)
    {
        for (int i = point + 1; i <= max; i++)
        {
            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
            {
                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                {
                    break;
                }
                else
                {
                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                    if (chessPointChange == null)
                    {
                        chessPointChange = i.ToString();
                    }
                    else
                    {
                        chessPointChange = chessPointChange + "-" + i.ToString();
                    }
                    break;
                }
            }
            else
            {
                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        for (int i = point - 1; i >= min; i--)
        {
            if (chessPointToSlot[i].transform.childCount > 1 && i != point)
            {
                if (chessPointToSlot[i].transform.GetChild(1).name.Contains(type))
                {
                    break;
                }
                else
                {
                    chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                    chessPointToSlot[i].transform.GetChild(0).SetAsLastSibling();
                    if (chessPointChange == null)
                    {
                        chessPointChange = i.ToString();
                    }
                    else
                    {
                        chessPointChange = chessPointChange + "-" + i.ToString();
                    }
                    break;
                }
            }
            else
            {
                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
    // Set Mode
    public void chessChangeClientMode(int mode)
    {
        OutBounMessage req = new OutBounMessage("SET_CLIENT_MODE");
        req.addHead();
        req.writeByte(mode);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            CPlayer.clientCurrentMode = mode;
            if (mode == 0)
            {
                isOwerChess = false;
                //LoadingControl.instance.playSound("doorclose");
                chessPlayerPlaying = false;
                chessButton[0].SetActive(true);
                chessButton[1].SetActive(false);
                chessPopupNotify.SetActive(true);
                chessPopupNotifyText.text = "Bạn đã đứng dậy";
                Invoke("chessCloseNotify", 1f);
            }
            else
            {
                //LoadingControl.instance.playSound("jointable");
                chessButton[0].SetActive(false);
                chessButton[1].SetActive(true);
                chessPopupNotify.SetActive(true);
                chessPopupNotifyText.text = "Bạn đã ngồi vào bàn";
                Invoke("chessCloseNotify", 1f);
            }
            /*
            for (int i = chessEatPlayerBotElement.transform.parent.GetChildCount() - 1; i > 1; i--)
            {
                DestroyImmediate(chessEatPlayerBotElement.transform.parent.GetChild(i).gameObject);
            }
            for (int i = chessEatPlayerTopElement.transform.parent.GetChildCount() - 1; i > 1; i--)
            {
                DestroyImmediate(chessEatPlayerTopElement.transform.parent.GetChild(i).gameObject);
            }
            chessGetTableDataEx();
            */
        });
    }
    // Notify
    public void chessCloseNotify()
    {
        chessPopupNotify.SetActive(false);
        chessPopupNotifyText.text = "";
    }
    // Click
    public GameObject chessPieceMove;
    public bool chessClick;
    public int chessCurrSourePoint;
    public int chessCurrDirPoint;
    public void chessTableControllPieceClick(GameObject targetPoint)
    {
        if (chessPieceMove.transform.GetChild(1).gameObject.name.Contains(chessMyColor))
        {
            //App.trace("||||| CHESS MOVE NAME " + targetPoint.name);
            //App.trace("||||| CHESS MOVE SOURCE NAME " + chessPieceMove.name);
            string getSource = Regex.Match(chessPieceMove.name, @"\d+").Value;
            int sourcePoint = Int32.Parse(getSource);
            string getDir = Regex.Match(targetPoint.name, @"\d+").Value;
            int dirPoint = Int32.Parse(getDir);
            if (dirPoint <= 7 && chessPieceMove.transform.GetChild(1).name.Contains("tot") || dirPoint >= 56 && chessPieceMove.transform.GetChild(1).name.Contains("tot"))
            {
                chessUpdateNewFacePopUp(true);
                chessClick = true;
                chessCurrSourePoint = sourcePoint;
                chessCurrDirPoint = dirPoint;
            }
            else
            {
                chessMovePerPieceClick(sourcePoint, dirPoint, targetPoint, 0);
            }
        }
        else
        {
            chessTableClearPointMove();
        }
    }
    // Move
    public string chessSourcePointDrag;
    public void chessMovePerPieceClick(int sourcePoint, int dirPoint, GameObject targetPoint, int faceUpdateNumber)
    {
        //App.trace("CHESS MOVE TO SOURE " + sourcePoint + " || " + "CHESS MOVE TO DIR " + dirPoint +" || "+ "CHESS FACE UPDATE NUMBER "+faceUpdateNumber );     
        var req_move_per_piece = new OutBounMessage("PLAY");
        req_move_per_piece.addHead();
        req_move_per_piece.writeByte(sourcePoint); // diem bat dau
        req_move_per_piece.writeByte(dirPoint); // diem di chuyen den
        if (faceUpdateNumber > 0)
        {
            req_move_per_piece.writeByte(faceUpdateNumber); // phong cap cho tot
        }
        App.ws.send(req_move_per_piece.getReq(), delegate (InBoundMessage res)
        {
            chessTableClearPointMove();
        });

    }
    public void chessMovePerPieceDrag(int sourcePoint, int dirPoint, int faceUpdateNumber)
    {
        if (CPlayer.typePlay == "play" && sourcePoint != dirPoint)
        {
            //App.trace("CHESS MOVE TO SOURE " + sourcePoint + " || " + "CHESS MOVE TO DIR " + dirPoint + " || " + "CHESS FACE UPDATE NUMBER " + faceUpdateNumber);
            var req_move_per_piece_DRAG = new OutBounMessage("PLAY");
            req_move_per_piece_DRAG.addHead();
            req_move_per_piece_DRAG.writeByte(sourcePoint); // diem bat dau
            req_move_per_piece_DRAG.writeByte(dirPoint); // diem di chuyen den
            if (faceUpdateNumber > 0)
            {
                req_move_per_piece_DRAG.writeByte(faceUpdateNumber); // phong cap cho tot
            }
            App.ws.send(req_move_per_piece_DRAG.getReq(), delegate (InBoundMessage res)
            {
            });
        }

    }
    // Update New Face
    public void chessUpdateNewFacePopUp(bool open)
    {
        RectTransform rtf = chessPopupUpdateFaceChess.GetComponent<RectTransform>();
        if (open)
        {
            chessPopupUpdateFaceChess.SetActive(true);
            if (chessMyColor == "black")
            {
                chessUpdateNewFaceImage[0].sprite = chessTuongFace[0];
                chessUpdateNewFaceImage[1].sprite = chessTuongFace[5];
                chessUpdateNewFaceImage[2].sprite = chessTuongFace[4];
                chessUpdateNewFaceImage[3].sprite = chessTuongFace[2];
            }
            else
            {
                chessUpdateNewFaceImage[0].sprite = chessTuongFace[6];
                chessUpdateNewFaceImage[1].sprite = chessTuongFace[11];
                chessUpdateNewFaceImage[2].sprite = chessTuongFace[10];
                chessUpdateNewFaceImage[3].sprite = chessTuongFace[8];
            }
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .3f).OnComplete(() =>
            {
            });
        }
        else
        {
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
            {
                chessPopupUpdateFaceChess.SetActive(false);
            });
        }
    }
    public void chessUpdateNewFace(int faceNumber)
    {
        //App.trace("Dang Click "+chessClick);
        if (chessClick)
        {
            chessMovePerPieceClick(chessCurrSourePoint, chessCurrDirPoint, null, faceNumber);
            chessUpdateNewFacePopUp(false);
        }
        else
        {
            chessMovePerPieceDrag(chessCurrSourePoint, chessCurrDirPoint, faceNumber);
            chessUpdateNewFacePopUp(false);
        }
    }
    // SURRENDER
    public void chessSurrender()
    {
        App.trace("SURRENDER ");
        var req_SURRENDER = new OutBounMessage("SURRENDER");
        req_SURRENDER.addHead();
        App.ws.send(req_SURRENDER.getReq(), delegate (InBoundMessage res)
        {
        });
    }
    // START_MATCH
    public void chessStarMatch()
    {
        App.trace("START_MATCH AAAA");
        var req_START_MATCH = new OutBounMessage("START_MATCH");
        req_START_MATCH.addHead();
        App.ws.send(req_START_MATCH.getReq(), delegate (InBoundMessage res)
        {
            chessButtonStart.SetActive(false);
        });
    }
    // Ask Draw
    public void chessAskDraw()
    {
        App.trace("ASK_DRAW ");
        var req_ASK_DRAW = new OutBounMessage("ASK_DRAW");
        req_ASK_DRAW.addHead();
        App.ws.send(req_ASK_DRAW.getReq(), delegate (InBoundMessage res)
        {
        });
    }
    // Back
    private string[] handelerCommand = {"SET_PLAYER_POINT", "SET_PLAYER_STATUS", "KICK_PLAYER", "ENTER_STATE", "SET_TURN", "SLOT_IN_TABLE_CHANGED",
    "OWNER_CHANGED", "START_MATCH", "GAMEOVER", "SET_PLAYER_ATTR", "MOVE","HIGHLIGHT","CHANGE_PIECE"};
    private void delAllHandle()
    {
        App.trace("STOP HANDLER");
        foreach (string t in handelerCommand)
        {
            //App.trace(t);
            var req = new OutBounMessage(t);
            req.addHead();
            App.ws.delHandler(req.getReq());
        }
    }
    public void chessBackToWaitRoom(bool open)
    {
        RectTransform rtfBack = chessPopupBack.GetComponent<RectTransform>();
        //App.trace("ISSSSSSSSSSSSSSSSSSSSPLAYINGGGGGGGGGGG " + IsPlaying);
        if (CPlayer.typePlay == "play")
        {
            if (chessPlayerPlaying && IsPlaying)
            {
                if (open)
                {
                    chessPopupBack.SetActive(true);
                    DOTween.To(() => rtfBack.anchoredPosition, x => rtfBack.anchoredPosition = x, Vector2.zero, .3f).OnComplete(() =>
                    {
                    });
                }
                else
                {
                    DOTween.To(() => rtfBack.anchoredPosition, x => rtfBack.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
                    {
                        chessPopupBack.SetActive(false);
                    });
                }
            }
            else
            {
                chessOpenWaitRoom();
            }
        }
        else if (CPlayer.typePlay == "replay")
        {
            SceneManager.LoadScene(CPlayer.preScene);
            Destroy(gameObject, .2f);
        }
    }
    public void chessOpenWaitRoom()
    {
        delAllHandle();
        CPlayer.preScene = "ChessTable";
        var req_ENTER_PARENT_PLACE = new OutBounMessage("ENTER_PARENT_PLACE");
        req_ENTER_PARENT_PLACE.addHead();
        req_ENTER_PARENT_PLACE.writeString("");
        req_ENTER_PARENT_PLACE.writeByte(CPlayer.clientCurrentMode);
        App.ws.send(req_ENTER_PARENT_PLACE.getReq(), delegate (InBoundMessage res)
        {
            App.trace("ENTER_PARENT_PLACE");
            res.readByte();
            res.readByte();
            SceneManager.LoadScene("WaitChessScene");
            Destroy(gameObject, .2f);
        });
    }
    private bool chessWaitBack = false;
    public void chessBackWait(bool back)
    {
        RectTransform rtfBack = chessPopupBack.GetComponent<RectTransform>();
        chessWaitBack = back;
        if (chessWaitBack)
        {
            OutBounMessage req = new OutBounMessage("SET_CLIENT_MODE");
            req.addHead();
            req.writeByte(0);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            { });
            chessPopupNotify.SetActive(true);
            chessPopupNotifyText.text = "Bạn đã đăng ký rời bàn";
            Invoke("chessCloseNotify", 1f);
            chessWaitBackButton[0].SetActive(false);
            chessWaitBackButton[1].SetActive(true);
            DOTween.To(() => rtfBack.anchoredPosition, x => rtfBack.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
            {
                chessPopupBack.SetActive(false);
            });
        }
        else
        {
            OutBounMessage req = new OutBounMessage("SET_CLIENT_MODE");
            req.addHead();
            req.writeByte(1);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            { });
            chessPopupNotify.SetActive(true);
            chessPopupNotifyText.text = "Bạn đã hủy đăng ký rời bàn";
            Invoke("chessCloseNotify", 1f);
            chessWaitBackButton[0].SetActive(true);
            chessWaitBackButton[1].SetActive(false);
        }
    }
    // Chat
    [Header("=====CHAT=====")]
    public GameObject[] chatPanels;
    public Image[] chatImoIco;
    public void showChatBox()
    {
        LoadingControl.instance.showChatBox(LoadingControl.CHANNEL_TABLE);
    }
    public void showChatPanels(string sender, string content, string emo, Sprite emoSprite = null)
    {
        //App.trace("SHOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOWWWWWWWWWWWWWWWWWWWWWWW CHAT");
        for (int i = 0; i < chessPlayeSlotId.Length; i++)
        {
            if (chessPlayerName[i] == sender && chessPlayeSlotId[i] > -1)
            {
                if (emoSprite == null)
                {
                    chatPanels[chessPlayeSlotId[i]].GetComponentInChildren<Text>().text = content;
                    if (!chatPanels[chessPlayeSlotId[i]].activeSelf)
                        StartCoroutine(_showChatPanels(chatPanels[chessPlayeSlotId[i]]));
                    return;
                }
                //App.trace("emo = " + emo);
                chatImoIco[chessPlayeSlotId[i]].sprite = emoSprite;
                if (!chatImoIco[chessPlayeSlotId[i]].gameObject.activeSelf)
                {
                    chatImoIco[chessPlayeSlotId[i]].gameObject.SetActive(true);
                    chatImoIco[chessPlayeSlotId[i]].transform.DOScale(1.2f, 4f).SetEase(Ease.OutBounce).OnComplete(() =>
                    {
                        chatImoIco[chessPlayeSlotId[i]].gameObject.SetActive(false);
                        chatImoIco[chessPlayeSlotId[i]].transform.localScale = Vector3.one;
                    });
                    return;
                }
                chatImoIco[chessPlayeSlotId[i]].transform.DORestart();
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
    // Invite
    [Header("=====INVITE=====")]
    public GameObject chessInvitePopUp;
    public GameObject chessInviteElement;
    public void chessOpenInvitePopup(bool open)
    {
        RectTransform rtf = chessInvitePopUp.GetComponent<RectTransform>();

        if (open)
        {
            for (int i = chessInviteElement.transform.parent.childCount - 1; i > 0; i--)
            {
                DestroyImmediate(chessInviteElement.transform.parent.GetChild(i).gameObject);
            }
            chessInvitePopUp.SetActive(true);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .3f).OnComplete(() =>
            {
                //App.trace("aaaaaaaaaa");
                var req = new OutBounMessage("LIST_ZONE_PLAYER");
                req.addHead();
                App.ws.send(req.getReq(), delegate (InBoundMessage res)
                {
                    int count = res.readShort();
                    for (int i = 0; i < count; i++)
                    {
                        int playerId = (int)res.readLong();
                        //App.trace("playerId" + playerId);
                        string nickName = res.readAscii();
                        //App.trace("nickName" + nickName);
                        bool isMale = res.readByte() == 1;
                        //App.trace("isMale" + isMale);
                        int chipBalance = (int)res.readLong();
                        //App.trace("chipBalance" + chipBalance);
                        int starBalance = (int)res.readLong();
                        //App.trace("starBalance" + starBalance);
                        int score = (int)res.readLong();
                        //App.trace("score" + score);
                        int level = res.readShort();
                        //App.trace("level" + level);
                        string avatar = res.readAscii();
                        //App.trace("avatar" + avatar);
                        int avatarId = res.readShort();
                        //App.trace("avatarId" + avatarId);
                        GameObject chessInviteClone = Instantiate(chessInviteElement, chessInviteElement.transform.parent, false);
                        chessInviteClone.GetComponentsInChildren<Text>()[0].text = nickName;
                        chessInviteClone.GetComponentsInChildren<Text>()[1].text = String.Format("{0:0,0}", chipBalance);
                        StartCoroutine(App.loadImg(chessInviteClone.GetComponentsInChildren<Image>()[2], App.getAvatarLink2(avatar + "", avatarId)));
                        chessInviteClone.gameObject.name = nickName;
                        chessInviteClone.SetActive(true);
                    }
                });
            });
        }
        else
        {
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
            {
                chessInvitePopUp.SetActive(false);
            });
        }
    }
    public void chessInvitePlayer(string nickName)
    {
        var req = new OutBounMessage("INVITE");
        req.addHead();
        req.writeAcii(nickName);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
        });
    }
    [Header("=====SHOW INFO====")]
    public GameObject chessInfoPopup;
    public GameObject chessInfoStatus;
    public Text[] chessInfoPopupText;
    public GameObject chessInfoPopupElement;
    public Sprite[] chessInfoSpriteLevel;
    public Image chessInfoImageAvatar;
    public GameObject chessArchimenElement;
    public GameObject arChimentPopup;
    public GameObject arChimenPopupImage;
    public Text arChimenPopupText;
    private string[] AchmImage, AchmName, AchmDesc;
    private long[] AchmTime, AchmTransId;
    private int chessPlayerIdInfo;
    public void chessShowInfoPopUp(string nickName, int playerId, int chipBlance, string avatar, int avatarId)
    {
        for (int i = chessInfoPopupElement.transform.parent.childCount - 1; i > 1; i--)
        {
            DestroyImmediate(chessInfoPopupElement.transform.parent.GetChild(i).gameObject);
        }
        for (int i = chessArchimenElement.transform.parent.childCount - 1; i > 0; i--)
        {
            DestroyImmediate(chessArchimenElement.transform.parent.GetChild(i).gameObject);
        }
        CPlayer.friendNicName = nickName;
        //CPlayer.friendId = playerId;
        //CPlayer.playerReviewId = playerId;
        chessPlayerIdInfo = playerId;
        RectTransform rtf = chessInfoPopup.GetComponent<RectTransform>();
        chessInfoPopupText[0].text = nickName;
        chessInfoPopupText[1].text = String.Format("{0:0,0}", chipBlance);
        StartCoroutine(App.loadImg(chessInfoImageAvatar, App.getAvatarLink2(avatar + "", avatarId)));
        chessInfoPopup.SetActive(true);

        var req_info = new OutBounMessage("PLAYER_PROFILE");
        //Debug.Log("WRITE LONG = " + CPlayer.id);
        //App.trace("PPLAYER ID = " + CPlayer.id);
        req_info.addHead();
        req_info.writeLong(playerId);
        req_info.writeByte(0x0f);
        req_info.writeAcii("");
        App.ws.send(req_info.getReq(), delegate (InBoundMessage res)
        {
            var nickName1 = res.readAscii();
            var fullName = res.readString();
            var avatar1 = res.readAscii();
            var isMale = res.readByte() == 1;
            var dateOfBirth = res.readAscii();
            if (dateOfBirth.Length > 1)
                chessInfoPopupText[4].text = dateOfBirth;
            else
                chessInfoPopupText[4].text = "...";
            var message = res.readString();
            chessInfoPopupText[3].text = message;
            if (message.Length > 1)
                chessInfoStatus.SetActive(true);
            else
                chessInfoStatus.SetActive(false);
            var chipBalance = res.readLong();
            var starBalance = res.readLong();
            string phone = res.readAscii();
            var email = res.readAscii();
            var address = res.readAscii();
            var cmnd = res.readAscii();
            if (cmnd.Length > 1)
                chessInfoPopupText[5].text = cmnd;
            else
                chessInfoPopupText[5].text = "...";
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
                //App.trace("game = " + gameId + "|w " + win + "|l " + lose + "|dr " + draw + "|score " + score + "|pre " + previousScore + "|next " + nextScore);
                if (gameId == "xiangqi")
                {
                    GameObject infoClone = Instantiate(chessInfoPopupElement, chessInfoPopupElement.transform.parent, false);
                    infoClone.GetComponentsInChildren<Text>()[0].text = "Cờ Tướng";
                    infoClone.GetComponentsInChildren<Text>()[1].text = win.ToString();
                    infoClone.GetComponentsInChildren<Text>()[2].text = draw.ToString();
                    infoClone.GetComponentsInChildren<Text>()[3].text = lose.ToString();
                    infoClone.GetComponentsInChildren<Text>()[4].text = score.ToString();
                    infoClone.GetComponentsInChildren<Image>()[6].sprite = chessInfoSpriteLevel[level - 1];
                    float bar = ((float)previousScore / (float)nextScore);
                    infoClone.GetComponentsInChildren<Image>()[9].rectTransform.localScale = new Vector2(bar, 1);
                    infoClone.GetComponentsInChildren<Text>()[5].text = previousScore.ToString() + "/" + nextScore.ToString();
                    infoClone.SetActive(true);
                }
                else if (gameId == "mystery_xiangqi")
                {
                    GameObject infoClone = Instantiate(chessInfoPopupElement, chessInfoPopupElement.transform.parent, false);
                    infoClone.GetComponentsInChildren<Text>()[0].text = "Cờ Úp";
                    infoClone.GetComponentsInChildren<Text>()[1].text = win.ToString();
                    infoClone.GetComponentsInChildren<Text>()[2].text = draw.ToString();
                    infoClone.GetComponentsInChildren<Text>()[3].text = lose.ToString();
                    infoClone.GetComponentsInChildren<Text>()[4].text = score.ToString();
                    infoClone.GetComponentsInChildren<Image>()[6].sprite = chessInfoSpriteLevel[level - 1];
                    float bar = ((float)previousScore / (float)nextScore);
                    infoClone.GetComponentsInChildren<Image>()[9].rectTransform.localScale = new Vector2(bar, 1);
                    infoClone.GetComponentsInChildren<Text>()[5].text = previousScore.ToString() + "/" + nextScore.ToString();
                    infoClone.SetActive(true);
                }
                else if (gameId == "chess")
                {
                    GameObject infoClone = Instantiate(chessInfoPopupElement, chessInfoPopupElement.transform.parent, false);
                    infoClone.GetComponentsInChildren<Text>()[0].text = "Cờ Vua";
                    infoClone.GetComponentsInChildren<Text>()[1].text = win.ToString();
                    infoClone.GetComponentsInChildren<Text>()[2].text = draw.ToString();
                    infoClone.GetComponentsInChildren<Text>()[3].text = lose.ToString();
                    infoClone.GetComponentsInChildren<Text>()[4].text = score.ToString();
                    infoClone.GetComponentsInChildren<Image>()[6].sprite = chessInfoSpriteLevel[level - 1];
                    float bar = ((float)previousScore / (float)nextScore);
                    infoClone.GetComponentsInChildren<Image>()[9].rectTransform.localScale = new Vector2(bar, 1);
                    infoClone.GetComponentsInChildren<Text>()[5].text = previousScore.ToString() + "/" + nextScore.ToString();
                    infoClone.SetActive(true);
                }
            }

            var isCPlayerFriend = res.readByte() == 1; // 0 : not friend 1: friend
            isFriend = isCPlayerFriend;
            if (isFriend)
            {
                chessInfoPopupText[2].text = "Hủy Kết bạn";
            }
            else
            {
                chessInfoPopupText[2].text = "Kết bạn";
            }
            int achmSize = res.readByte();
            //App.trace("achmSize " + achmSize);
            AchmDesc = new string[achmSize];
            AchmName = new string[achmSize];
            AchmImage = new string[achmSize];
            AchmTime = new long[achmSize];
            AchmTransId = new long[achmSize];
            for (int i = 0; i < achmSize; i++)
            {
                string achmImage = res.readAscii();
                AchmImage[i] = achmImage;
                //App.trace("achmImage "+ achmImage);
                string achmName = res.readString();
                AchmName[i] = achmName;
                //App.trace("achmName " + achmName);
                string achmDesc = res.readString();
                AchmDesc[i] = achmDesc;
                //App.trace("achmDesc " + achmDesc);
                long achmTransId = res.readLong();
                AchmTransId[i] = achmTransId;
                //App.trace("achmTransId " + achmTransId);
                long achmTime = res.readLong();
                AchmTime[i] = achmTime;
                //App.trace("achmTime " + achmTime);
                string achmGameId = res.readAscii();
                //App.trace("achmGameId "+ achmGameId);
                if (achmGameId == "xiangqi" || achmGameId == "mystery_xiangqi" || achmGameId == "chess")
                {
                    GameObject playerAchimenClone = Instantiate(chessArchimenElement, chessArchimenElement.transform.parent, false);
                    StartCoroutine(App.loadImg(playerAchimenClone.GetComponent<Image>(), App.getAvatarLink2(achmImage + "", (int)achmTransId)));
                    playerAchimenClone.name = i.ToString();
                    playerAchimenClone.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        openArchiment(playerAchimenClone.name);
                    });
                    playerAchimenClone.SetActive(true);
                }
            }
        });
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .3f).OnComplete(() =>
        {
            if (CPlayer.typePlay == "replay")
            {
                chessReviewFunctionMedia.SetActive(false);
            }
        });
    }
    public void openArchiment(string name)
    {
        string getnumber = Regex.Match(name, @"\d+").Value;
        int number = Int32.Parse(getnumber);
        long time = AchmTime[number];
        DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime date = start.AddMilliseconds(time).ToLocalTime();
        string s = date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        StartCoroutine(App.loadImg(arChimenPopupImage.GetComponent<Image>(), App.getAvatarLink2(AchmImage[number] + "", (int)AchmTransId[number])));
        arChimenPopupText.text = "Thành tích: " + AchmName[number] + " \n Mô tả: " + AchmDesc[number] + " \n Ngày đạt: " + s;
        arChimentPopup.SetActive(true);
    }
    public void closeArchiment()
    {
        arChimentPopup.SetActive(false);
    }

    public void chessCloseInfoPopUp()
    {
        RectTransform rtf = chessInfoPopup.GetComponent<RectTransform>();
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
        {
            chessInfoPopup.SetActive(false);
            if (CPlayer.typePlay == "replay")
            {
                chessReviewFunctionMedia.SetActive(true);
            }
        });
    }
    public void chessOpenReportInfo()
    {
        //LoadingControl.instance.friendNickName = CPlayer.friendNicName;
        //LoadingControl.instance.friendAction(2);
    }
    private bool isFriend;
    public void chessSendMessPerPerson()
    {
        //LoadingControl.instance.friendNickName = CPlayer.friendNicName;
        //LoadingControl.instance.friendAction(0);
    }
    public void chessAddFriend()
    {
        if (isFriend)
        {
            var reqDeleteFriend = new OutBounMessage("FRIEND.DELETE");
            reqDeleteFriend.addHead();
            //reqDeleteFriend.writeLong(CPlayer.friendId);
            reqDeleteFriend.writeAcii(CPlayer.friendNicName);
            App.ws.send(reqDeleteFriend.getReq(), delegate (InBoundMessage resAddFriend)
            {
                isFriend = false;
                chessInfoPopupText[2].text = "Kết bạn";
            });
        }
        else
        {
            var reqAddFriend = new OutBounMessage("FRIEND.CREATE");
            reqAddFriend.addHead();
            //reqAddFriend.writeLong(CPlayer.friendId);
            reqAddFriend.writeAcii(CPlayer.friendNicName);
            App.ws.send(reqAddFriend.getReq(), delegate (InBoundMessage resAddFriend)
            {
                isFriend = true;
                chessInfoPopupText[2].text = "Hủy Kết bạn";
            });
        }
    }
    public void chessOpenChessReview()
    {
        delAllHandle();
        CPlayer.chessPreScene = "TableChessVuaScene";
        SceneManager.LoadScene("ReviewMatchScene");
        Destroy(gameObject, .2f);
    }

    [Header("=====REVIEW MATCH=====")]
    public GameObject chessReviewMatchPopup;
    public GameObject chessReviewMatchPopupEnd;
    public GameObject chessReviewFunctionMedia;
    public GameObject[] chessReviewButton;
    public Text[] chessReviewText;
    private string nameGame = "";
    private string[] chessReviewRowLog;
    private int[] chessReviewSourcePos = new int[0];
    private int[] chessReviewDirPos = new int[0];
    private string[] chessReviewColor = new string[0];
    private string[] chessChangeFaceIn = new string[0];
    private string chessReviewBlackPlayer;
    private string chessReviewRedPlayer;
    private string chessReviewPlayerFirstTurn;
    private string chessReviewPlayerWinner;
    private int chessReviewReason;
    private int chessReviewStep = 0;
    private bool chessReviewPause = false;
    private int chessReviewPosConvert;
    public void chessReviewMatch()
    {
        var req_GET_MATCH_DATA = new OutBounMessage("GET_MATCH_DATA");
        req_GET_MATCH_DATA.addHead();
        req_GET_MATCH_DATA.writeLong(CPlayer.reviewMatchId);
        req_GET_MATCH_DATA.writeAcii(CPlayer.reviewMatchGameId);
        App.ws.send(req_GET_MATCH_DATA.getReq(), delegate (InBoundMessage res)
        {
            chessReviewLoadBoard();
            string info = "";
            string infoMain = "";
            string infoEnd = "";
            string infoEndMain = "";
            nameGame = "Cờ Vua";
            infoEnd += nameGame;
            info += nameGame;
            int betAmount = res.readInt();
            infoEnd += " - " + betAmount + " Gold";
            info += " - " + betAmount + " Gold";
            //App.trace("Review betAmount " + betAmount);
            int competitorCount = res.readByte();
            //App.trace("Review competitorCount " + competitorCount);
            for (int i = 0; i < competitorCount; i++)
            {
                long id = res.readLong();
                //App.trace("Review id " + i + " " + id);
                string nickName = res.readAscii();
                //App.trace("Review nickName " + i + " " + nickName);
                int avatarId = res.readShort();
                //App.trace("Review avatarId " + i + " " + avatarId);
                string avatar = res.readAscii();
                //App.trace("Review avatar " + i + " " + avatar);
                bool isMale = res.readByte() == 1;
                //App.trace("Review isMale " + i + " " + isMale);
                long chipBalance = res.readLong();
                //App.trace("Review chipBalance " + i + " " + chipBalance);
                long starBalance = res.readLong();
                //App.trace("Review starBalance " + i + " " + starBalance);
                int score = res.readInt();
                //App.trace("Review score " + i + " " + score);
                int level = res.readByte();
                //App.trace("Review level " + i + " " + level);

                if (i == 1)
                {
                    chessNickNamePlayer[1] = nickName;
                    chessAvartarIdPlayer[1] = avatarId;
                    chessAvatarPlayer[1] = avatar;
                    chessTableText[2].text = nickName;
                    chessTableText[3].text = chipBalance.ToString();
                    chipPlayer1 = chipBalance;
                    chessTableText[4].text = "Level " + level.ToString();
                    StartCoroutine(App.loadImg(chessGameObjectImage[0].GetComponentsInChildren<Image>()[1], App.getAvatarLink2(avatar + "", avatarId)));
                    chessGameObjectImage[0].SetActive(true);
                    chessGameObjectImage[2].SetActive(false);
                    chessGameObjectImage[6].SetActive(true);
                    chessGameObjectImage[8].SetActive(true);
                    chessGameObjectImage[8].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        chessShowInfoPopUp(nickName, (int)id, (int)chipBalance, avatar, avatarId);
                    });
                }
                else if (i == 0)
                {
                    chessNickNamePlayer[0] = nickName;
                    chessAvartarIdPlayer[0] = avatarId;
                    chessAvatarPlayer[0] = avatar;
                    chessTableText[5].text = nickName;
                    chessTableText[6].text = chipBalance.ToString();
                    chipPlayer2 = chipBalance;
                    chessTableText[7].text = "Level " + level.ToString();
                    StartCoroutine(App.loadImg(chessGameObjectImage[1].GetComponentsInChildren<Image>()[1], App.getAvatarLink2(avatar + "", avatarId)));
                    chessGameObjectImage[1].SetActive(true);
                    chessGameObjectImage[3].SetActive(false);
                    chessGameObjectImage[7].SetActive(true);
                    chessGameObjectImage[9].SetActive(true);
                    chessGameObjectImage[9].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        chessShowInfoPopUp(nickName, (int)id, (int)chipBalance, avatar, avatarId);
                    });
                }
            }
            //info += "\n<b>"+ chessReviewNamePlayer[0] + " thắng "+chessReviewNamePlayer[1]+"</b>";
            //chessReviewText[0].text = info;          
            int logCount = res.readShort();
            //App.trace("Review logCount " + logCount);
            for (int i = 0; i < logCount; i++)
            {
                string log = res.readString();
                //App.trace("Review log " + i + " " + log);
                chessReviewRowLog = log.Split(';');
                if (chessReviewRowLog.Length >= 3)
                {
                    //App.trace("Chess Review Log " + chessReviewRowLog.Length);
                    string type = chessReviewRowLog[1];
                    if (type == "start")
                    {
                        long numberTime = Int64.Parse(chessReviewRowLog[2]);
                        string second = (((numberTime % 1000000) % 10000) / 100).ToString("00");
                        string minute = ((numberTime % 1000000) / 10000).ToString("00");
                        string day = ((numberTime % 100000000) / 1000000).ToString("00");
                        string month = ((numberTime % 10000000000) / 100000000).ToString("00");
                        string year = (numberTime / 10000000000).ToString("00");
                        infoMain += "Lúc " + minute + ":" + second + " ngày " + day + "/" + month + "/" + year;
                    }
                    else if (type == "obtainColor")
                    {
                        if (chessReviewRowLog[2] == "w")
                        {
                            chessReviewRedPlayer = chessReviewRowLog[0];
                            infoMain += "\nCầm quân trắng: " + chessReviewRowLog[0];
                        }
                        else
                        {
                            chessReviewBlackPlayer = chessReviewRowLog[0];
                            infoMain += "\nCầm quân đen: " + chessReviewRowLog[0];
                        }
                    }
                    else if (type == "duration")
                    {
                        long timeDuration = Int64.Parse(chessReviewRowLog[2]);
                        infoMain += "\nThời gian thắng: " + (timeDuration / 1000).ToString() + "s";
                        infoEndMain += "Thời gian thắng: " + (timeDuration / 1000).ToString() + "s";
                    }
                    else if (type.Length <= 0 || type == "m")
                    {
                        string sour = "";
                        string dir = "";
                        string changefaceto = "";
                        if (chessReviewRowLog[2].Length > 4)
                        {
                            sour = chessReviewRowLog[2].Substring(0, 2);
                            dir = chessReviewRowLog[2].Substring(chessReviewRowLog[2].Length - 3, 2);
                            changefaceto = chessReviewRowLog[2].Substring(chessReviewRowLog[2].Length - 1, 1);
                        }
                        else if (chessReviewRowLog[2].Length == 4)
                        {
                            sour = chessReviewRowLog[2].Substring(0, 2);
                            dir = chessReviewRowLog[2].Substring(chessReviewRowLog[2].Length - 2, 2);
                            changefaceto = "";
                        }
                        Array.Resize(ref chessReviewColor, chessReviewColor.Length + 1);
                        Array.Resize(ref chessReviewSourcePos, chessReviewSourcePos.Length + 1);
                        Array.Resize(ref chessReviewDirPos, chessReviewDirPos.Length + 1);
                        Array.Resize(ref chessChangeFaceIn, chessChangeFaceIn.Length + 1);
                        chessChangeFaceIn[chessChangeFaceIn.Length - 1] = changefaceto;
                        chessReviewChangePosition(sour);
                        chessReviewSourcePos[chessReviewSourcePos.Length - 1] = chessReviewPosConvert;
                        chessReviewChangePosition(dir);
                        chessReviewDirPos[chessReviewDirPos.Length - 1] = chessReviewPosConvert;
                        chessReviewColor[chessReviewColor.Length - 1] = chessReviewRowLog[0];
                    }
                    else if (type == "obtainFirstTurn")
                    {
                        chessReviewPlayerFirstTurn = chessReviewRowLog[0];
                    }
                    else if (type == "end")
                    {
                        chessReviewPlayerWinner = chessReviewRowLog[0];
                        chessReviewReason = Int32.Parse(chessReviewRowLog[2]);
                    }
                }
            }
            info += "\n<b>" + chessReviewPlayerFirstTurn + " tiên " + "</b>";
            if (chessReviewReason != 0)
            {
                if (chessReviewReason == 6 || chessReviewReason == 7)
                    info += "<b>" + "hòa " + "</b>";
                else
                    info += chessReviewPlayerFirstTurn != chessReviewPlayerWinner ? "<b>" + "bại " + "</b>" : "<b>" + "thắng " + "</b>";
            }
            if (chessReviewReason != 0)
            {
                if (chessReviewReason == 6)
                    infoEnd += "<b>" + "Hai đấu thủ chấp nhận hòa " + "</b>";
                else if (chessReviewReason == 7)
                    infoEnd += "<b>" + "Hòa do không thể phân thắng bại trong số nước cho phép." + "</b>";
                else
                {
                    string loser = (chessReviewPlayerWinner != chessReviewRedPlayer ? chessReviewRedPlayer : chessReviewBlackPlayer);
                    if (chessReviewReason == 5)
                        infoEnd += "<b>" + "\n" + loser + " xin hàng, " + chessReviewPlayerWinner + " giành chiến thắng" + "</b>";
                    else if (chessReviewReason == 4)
                        infoEnd += "<b>" + "\n" + loser + " hết thời gian, " + chessReviewPlayerWinner + " giành chiến thắng" + "</b>";
                    else if (chessReviewReason == 3)
                        infoEnd += "<b>" + "\n" + loser + " hết nước đi, " + chessReviewPlayerWinner + " giành chiến thắng" + "</b>";
                    else
                        infoEnd += "<b>" + "\n" + "Chiếu bí, " + chessReviewPlayerWinner + " giành chiến thắng" + "</b>";
                }
            }
            info += "<b>" + (chessReviewPlayerFirstTurn != chessReviewRedPlayer ? chessReviewRedPlayer : chessReviewBlackPlayer) + "</b>";
            infoMain += "\nSố nước đi: " + chessReviewSourcePos.Length;
            infoEndMain += "\nSố nước đi: " + chessReviewSourcePos.Length;
            chessReviewText[0].text = info;
            chessReviewText[1].text = infoMain;
            chessReviewText[2].text = infoEnd;
            chessReviewText[3].text = infoEndMain;
            StartCoroutine(_chessShowReviewMatchInfoPopup());
        });
    }
    public void chessReviewChangePosition(string pos)
    {
        switch (pos)
        {
            case "a1":
                chessReviewPosConvert = 0;
                break;
            case "a2":
                chessReviewPosConvert = 8;
                break;
            case "a3":
                chessReviewPosConvert = 16;
                break;
            case "a4":
                chessReviewPosConvert = 24;
                break;
            case "a5":
                chessReviewPosConvert = 32;
                break;
            case "a6":
                chessReviewPosConvert = 40;
                break;
            case "a7":
                chessReviewPosConvert = 48;
                break;
            case "a8":
                chessReviewPosConvert = 56;
                break;
            case "b1":
                chessReviewPosConvert = 1;
                break;
            case "b2":
                chessReviewPosConvert = 9;
                break;
            case "b3":
                chessReviewPosConvert = 17;
                break;
            case "b4":
                chessReviewPosConvert = 25;
                break;
            case "b5":
                chessReviewPosConvert = 33;
                break;
            case "b6":
                chessReviewPosConvert = 41;
                break;
            case "b7":
                chessReviewPosConvert = 49;
                break;
            case "b8":
                chessReviewPosConvert = 57;
                break;
            case "c1":
                chessReviewPosConvert = 2;
                break;
            case "c2":
                chessReviewPosConvert = 10;
                break;
            case "c3":
                chessReviewPosConvert = 18;
                break;
            case "c4":
                chessReviewPosConvert = 26;
                break;
            case "c5":
                chessReviewPosConvert = 34;
                break;
            case "c6":
                chessReviewPosConvert = 42;
                break;
            case "c7":
                chessReviewPosConvert = 50;
                break;
            case "c8":
                chessReviewPosConvert = 58;
                break;
            case "d1":
                chessReviewPosConvert = 3;
                break;
            case "d2":
                chessReviewPosConvert = 11;
                break;
            case "d3":
                chessReviewPosConvert = 19;
                break;
            case "d4":
                chessReviewPosConvert = 27;
                break;
            case "d5":
                chessReviewPosConvert = 35;
                break;
            case "d6":
                chessReviewPosConvert = 43;
                break;
            case "d7":
                chessReviewPosConvert = 51;
                break;
            case "d8":
                chessReviewPosConvert = 59;
                break;
            case "e1":
                chessReviewPosConvert = 4;
                break;
            case "e2":
                chessReviewPosConvert = 12;
                break;
            case "e3":
                chessReviewPosConvert = 20;
                break;
            case "e4":
                chessReviewPosConvert = 28;
                break;
            case "e5":
                chessReviewPosConvert = 36;
                break;
            case "e6":
                chessReviewPosConvert = 44;
                break;
            case "e7":
                chessReviewPosConvert = 52;
                break;
            case "e8":
                chessReviewPosConvert = 60;
                break;
            case "f1":
                chessReviewPosConvert = 5;
                break;
            case "f2":
                chessReviewPosConvert = 13;
                break;
            case "f3":
                chessReviewPosConvert = 21;
                break;
            case "f4":
                chessReviewPosConvert = 29;
                break;
            case "f5":
                chessReviewPosConvert = 37;
                break;
            case "f6":
                chessReviewPosConvert = 45;
                break;
            case "f7":
                chessReviewPosConvert = 53;
                break;
            case "f8":
                chessReviewPosConvert = 61;
                break;
            case "g1":
                chessReviewPosConvert = 6;
                break;
            case "g2":
                chessReviewPosConvert = 14;
                break;
            case "g3":
                chessReviewPosConvert = 22;
                break;
            case "g4":
                chessReviewPosConvert = 30;
                break;
            case "g5":
                chessReviewPosConvert = 38;
                break;
            case "g6":
                chessReviewPosConvert = 46;
                break;
            case "g7":
                chessReviewPosConvert = 54;
                break;
            case "g8":
                chessReviewPosConvert = 62;
                break;
            case "h1":
                chessReviewPosConvert = 7;
                break;
            case "h2":
                chessReviewPosConvert = 15;
                break;
            case "h3":
                chessReviewPosConvert = 23;
                break;
            case "h4":
                chessReviewPosConvert = 31;
                break;
            case "h5":
                chessReviewPosConvert = 39;
                break;
            case "h6":
                chessReviewPosConvert = 47;
                break;
            case "h7":
                chessReviewPosConvert = 55;
                break;
            case "h8":
                chessReviewPosConvert = 63;
                break;
        }
    }
    public void chessReviewLoadBoard()
    {
        chessEatPlayerBotMoveClone = Instantiate(chessEatPlayerBotMove, chessEatPlayerBotMove.transform.parent, false);
        chessEatPlayerBotMoveClone.SetActive(true);
        chessEatPlayerTopMoveClone = Instantiate(chessEatPlayerTopMove, chessEatPlayerTopMove.transform.parent, false);
        chessEatPlayerTopMoveClone.SetActive(true);
        for (int i = 0; i < 32; i++)
        {
            GameObject chessClone = Instantiate(chessElement, chessElement.transform.parent, false);
            switch (i)
            {
                case 0:
                    chessClone.transform.SetParent(chessPointToSlot[60].transform);
                    chessClone.transform.position = chessPointToSlot[60].transform.position;
                    chessReviewLoadFace(chessClone, -8, "kingblack", 1);
                    break;
                case 1:
                    chessClone.transform.SetParent(chessPointToSlot[59].transform);
                    chessClone.transform.position = chessPointToSlot[59].transform.position;
                    chessReviewLoadFace(chessClone, -16, "haublack", 0);
                    break;
                case 2:
                    chessClone.transform.SetParent(chessPointToSlot[56].transform);
                    chessClone.transform.position = chessPointToSlot[56].transform.position;
                    chessReviewLoadFace(chessClone, -24, "xeblack", 5);
                    break;
                case 3:
                    chessClone.transform.SetParent(chessPointToSlot[58].transform);
                    chessClone.transform.position = chessPointToSlot[58].transform.position;
                    chessReviewLoadFace(chessClone, -32, "tuongblack", 4);
                    break;
                case 4:
                    chessClone.transform.SetParent(chessPointToSlot[57].transform);
                    chessClone.transform.position = chessPointToSlot[57].transform.position;
                    chessReviewLoadFace(chessClone, -40, "mablack", 2);
                    break;
                case 5:
                    chessClone.transform.SetParent(chessPointToSlot[63].transform);
                    chessClone.transform.position = chessPointToSlot[63].transform.position;
                    chessReviewLoadFace(chessClone, -25, "xeblack", 5);
                    break;
                case 6:
                    chessClone.transform.SetParent(chessPointToSlot[61].transform);
                    chessClone.transform.position = chessPointToSlot[61].transform.position;
                    chessReviewLoadFace(chessClone, -33, "tuongblack", 4);
                    break;
                case 7:
                    chessClone.transform.SetParent(chessPointToSlot[62].transform);
                    chessClone.transform.position = chessPointToSlot[62].transform.position;
                    chessReviewLoadFace(chessClone, -41, "mablack", 2);
                    break;
                case 8:
                    chessClone.transform.SetParent(chessPointToSlot[48].transform);
                    chessClone.transform.position = chessPointToSlot[48].transform.position;
                    chessReviewLoadFace(chessClone, -48, "totblack", 3);
                    break;
                case 9:
                    chessClone.transform.SetParent(chessPointToSlot[49].transform);
                    chessClone.transform.position = chessPointToSlot[49].transform.position;
                    chessReviewLoadFace(chessClone, -49, "totblack", 3);
                    break;
                case 10:
                    chessClone.transform.SetParent(chessPointToSlot[50].transform);
                    chessClone.transform.position = chessPointToSlot[50].transform.position;
                    chessReviewLoadFace(chessClone, -50, "totblack", 3);
                    break;
                case 11:
                    chessClone.transform.SetParent(chessPointToSlot[51].transform);
                    chessClone.transform.position = chessPointToSlot[51].transform.position;
                    chessReviewLoadFace(chessClone, -51, "totblack", 3);
                    break;
                case 12:
                    chessClone.transform.SetParent(chessPointToSlot[52].transform);
                    chessClone.transform.position = chessPointToSlot[52].transform.position;
                    chessReviewLoadFace(chessClone, -52, "totblack", 3);
                    break;
                case 13:
                    chessClone.transform.SetParent(chessPointToSlot[53].transform);
                    chessClone.transform.position = chessPointToSlot[53].transform.position;
                    chessReviewLoadFace(chessClone, -53, "totblack", 3);
                    break;
                case 14:
                    chessClone.transform.SetParent(chessPointToSlot[54].transform);
                    chessClone.transform.position = chessPointToSlot[54].transform.position;
                    chessReviewLoadFace(chessClone, -54, "totblack", 3);
                    break;
                case 15:
                    chessClone.transform.SetParent(chessPointToSlot[55].transform);
                    chessClone.transform.position = chessPointToSlot[55].transform.position;
                    chessReviewLoadFace(chessClone, -55, "totblack", 3);
                    break;
                case 16:
                    chessClone.transform.SetParent(chessPointToSlot[4].transform);
                    chessClone.transform.position = chessPointToSlot[4].transform.position;
                    chessReviewLoadFace(chessClone, 8, "kingred", 7);
                    break;
                case 17:
                    chessClone.transform.SetParent(chessPointToSlot[3].transform);
                    chessClone.transform.position = chessPointToSlot[3].transform.position;
                    chessReviewLoadFace(chessClone, 16, "haured", 6);
                    break;
                case 18:
                    chessClone.transform.SetParent(chessPointToSlot[0].transform);
                    chessClone.transform.position = chessPointToSlot[0].transform.position;
                    chessReviewLoadFace(chessClone, 24, "xered", 11);
                    break;
                case 19:
                    chessClone.transform.SetParent(chessPointToSlot[2].transform);
                    chessClone.transform.position = chessPointToSlot[2].transform.position;
                    chessReviewLoadFace(chessClone, 32, "tuongred", 10);
                    break;
                case 20:
                    chessClone.transform.SetParent(chessPointToSlot[1].transform);
                    chessClone.transform.position = chessPointToSlot[1].transform.position;
                    chessReviewLoadFace(chessClone, 40, "mared", 8);
                    break;
                case 21:
                    chessClone.transform.SetParent(chessPointToSlot[7].transform);
                    chessClone.transform.position = chessPointToSlot[7].transform.position;
                    chessReviewLoadFace(chessClone, 25, "xered", 11);
                    break;
                case 22:
                    chessClone.transform.SetParent(chessPointToSlot[5].transform);
                    chessClone.transform.position = chessPointToSlot[5].transform.position;
                    chessReviewLoadFace(chessClone, 33, "tuongred", 10);
                    break;
                case 23:
                    chessClone.transform.SetParent(chessPointToSlot[6].transform);
                    chessClone.transform.position = chessPointToSlot[6].transform.position;
                    chessReviewLoadFace(chessClone, 41, "mared", 8);
                    break;
                case 24:
                    chessClone.transform.SetParent(chessPointToSlot[8].transform);
                    chessClone.transform.position = chessPointToSlot[8].transform.position;
                    chessReviewLoadFace(chessClone, 48, "totred", 9);
                    break;
                case 25:
                    chessClone.transform.SetParent(chessPointToSlot[9].transform);
                    chessClone.transform.position = chessPointToSlot[9].transform.position;
                    chessReviewLoadFace(chessClone, 49, "totred", 9);
                    break;
                case 26:
                    chessClone.transform.SetParent(chessPointToSlot[10].transform);
                    chessClone.transform.position = chessPointToSlot[10].transform.position;
                    chessReviewLoadFace(chessClone, 50, "totred", 9);
                    break;
                case 27:
                    chessClone.transform.SetParent(chessPointToSlot[11].transform);
                    chessClone.transform.position = chessPointToSlot[11].transform.position;
                    chessReviewLoadFace(chessClone, 51, "totred", 9);
                    break;
                case 28:
                    chessClone.transform.SetParent(chessPointToSlot[12].transform);
                    chessClone.transform.position = chessPointToSlot[12].transform.position;
                    chessReviewLoadFace(chessClone, 52, "totred", 9);
                    break;
                case 29:
                    chessClone.transform.SetParent(chessPointToSlot[13].transform);
                    chessClone.transform.position = chessPointToSlot[13].transform.position;
                    chessReviewLoadFace(chessClone, 53, "totred", 9);
                    break;
                case 30:
                    chessClone.transform.SetParent(chessPointToSlot[14].transform);
                    chessClone.transform.position = chessPointToSlot[14].transform.position;
                    chessReviewLoadFace(chessClone, 54, "totred", 9);
                    break;
                case 31:
                    chessClone.transform.SetParent(chessPointToSlot[15].transform);
                    chessClone.transform.position = chessPointToSlot[15].transform.position;
                    chessReviewLoadFace(chessClone, 55, "totred", 9);
                    break;
            }
            chessClone.SetActive(true);
        }
    }
    public void chessReviewLoadFace(GameObject chessClone, int sid, string name, int face)
    {
        chessClone.GetComponent<Image>().sprite = chessTuongFace[face];
        chessClone.name = sid + "." + name + "." + sid;
    }
    public void chessReviewMoveAnimation(string type)
    {
        switch (type)
        {
            case "info":
                chessShowReviewMatchInfoPopup(true);
                break;
            case "play":
                if (chessReviewStep == chessReviewSourcePos.Length)
                {
                    StartCoroutine(_chessShowReviewMatchInfoEndPopup());
                }
                else
                {
                    chessReviewButton[2].SetActive(false);
                    chessReviewButton[3].SetActive(true);
                    chessReviewPause = false;
                    StartCoroutine(chessReviewMediaPlay());
                }
                break;
            case "next":
                if (chessReviewStep < chessReviewSourcePos.Length)
                {
                    chessReviewMediaNext(.3f);
                }
                break;
            case "pre":
                chessReviewMediaPrevious(.3f);
                break;
            case "first":
                if (chessReviewStep - 1 == 0)
                {
                    chessReviewButton[2].SetActive(true);
                    chessReviewButton[3].SetActive(false);
                }
                else
                {
                    chessReviewButton[2].SetActive(false);
                    chessReviewButton[3].SetActive(true);
                    chessReviewPause = false;
                    StartCoroutine(chessReviewMediaFirst());
                }
                break;
            case "last":
                chessReviewButton[2].SetActive(false);
                chessReviewButton[3].SetActive(true);
                chessReviewPause = false;
                StartCoroutine(chessReviewMediaLast());
                break;
            case "pause":
                chessReviewButton[2].SetActive(true);
                chessReviewButton[3].SetActive(false);
                chessReviewPause = true;
                break;
        }
    }
    public void chessReviewMediaNext(float time)
    {
        chessTableClearPointMove();
        GameObject chessMove = chessPointToSlot[chessReviewSourcePos[chessReviewStep]].transform.GetChild(1).gameObject;
        chessMove.transform.SetParent(chessMove.transform.parent.parent);
        RectTransform rtf = chessMove.GetComponent<RectTransform>();
        RectTransform rtfTarget = chessPointToSlot[chessReviewDirPos[chessReviewStep]].GetComponent<RectTransform>();
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfTarget.anchoredPosition.x, rtfTarget.anchoredPosition.y), time).OnComplete(() =>
        {
            if (chessChangeFaceIn[chessReviewStep] != "")
            {
                App.trace("CO PHONG TOT");
                chessMove.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform);
                string[] chessId = chessMove.name.Split('.');
                int id = Int32.Parse(chessId[0]);
                string[] lastStep = chessMove.name.Split('.').Skip(3).ToArray();
                if (chessMove.name.Contains("black"))
                {
                    chessConvertNewFace(chessChangeFaceIn[chessReviewStep], true, id, chessMove, lastStep, chessReviewStep);
                }
                else
                {
                    chessConvertNewFace(chessChangeFaceIn[chessReviewStep], false, id, chessMove, lastStep, chessReviewStep);
                }
            }
            if (chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform.childCount > 1)
            {
                GameObject chessEat = chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform.GetChild(1).gameObject;
                GameObject chessEatBotParent = chessEatPlayerBotMoveClone.transform.parent.gameObject;
                GameObject chessEatTopParent = chessEatPlayerTopMoveClone.transform.parent.gameObject;
                chessEatPlayerBotMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                chessEatPlayerTopMoveClone.transform.SetParent(chessEat.transform.parent.parent.parent);
                chessEat.transform.SetParent(chessEat.transform.parent.parent.parent);
                RectTransform rtfEat = chessEat.GetComponent<RectTransform>();
                RectTransform rtfEatBot = chessEatPlayerBotMoveClone.GetComponent<RectTransform>();
                RectTransform rtfEatTop = chessEatPlayerTopMoveClone.GetComponent<RectTransform>();
                if (chessEat.name.Contains("red"))
                {
                    DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatBot.anchoredPosition.x, rtfEatBot.anchoredPosition.y), time).OnComplete(() =>
                    {
                        chessEat.transform.SetParent(chessEatBotParent.transform);
                        chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                        chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                        chessMove.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform);
                        chessEat.name += ".step" + chessReviewStep;
                        chessReviewStep += 1;
                    });
                }
                else
                {
                    DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfEatTop.anchoredPosition.x, rtfEatTop.anchoredPosition.y), time).OnComplete(() =>
                    {
                        chessEat.transform.SetParent(chessEatTopParent.transform);
                        chessEatPlayerBotMoveClone.transform.SetParent(chessEatBotParent.transform);
                        chessEatPlayerTopMoveClone.transform.SetParent(chessEatTopParent.transform);
                        chessMove.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform);
                        chessEat.name += ".step" + chessReviewStep;
                        chessReviewStep += 1;
                    });
                }
            }
            else
            {
                chessMove.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform);
                chessMove.name += ".step" + chessReviewStep;
                chessReviewStep += 1;
            }
        });
    }
    public void chessConvertNewFace(string type, bool black, int id, GameObject point, string[] lastStep, int chessReviewStep)
    {
        switch (type)
        {
            case "b":
                if (black)
                {
                    StartCoroutine(chessChangeFaceReview(-33, id, point, lastStep, chessReviewStep));
                }
                else
                {
                    StartCoroutine(chessChangeFaceReview(33, id, point, lastStep, chessReviewStep));
                }
                break;
            case "q":
                if (black)
                {
                    StartCoroutine(chessChangeFaceReview(-16, id, point, lastStep, chessReviewStep));
                }
                else
                {
                    StartCoroutine(chessChangeFaceReview(16, id, point, lastStep, chessReviewStep));
                }
                break;
            case "r":
                if (black)
                {
                    StartCoroutine(chessChangeFaceReview(-24, id, point, lastStep, chessReviewStep));
                }
                else
                {
                    StartCoroutine(chessChangeFaceReview(24, id, point, lastStep, chessReviewStep));
                }
                break;
            case "n":
                if (black)
                {
                    StartCoroutine(chessChangeFaceReview(-40, id, point, lastStep, chessReviewStep));
                }
                else
                {
                    StartCoroutine(chessChangeFaceReview(40, id, point, lastStep, chessReviewStep));
                }
                break;
        }
    }
    IEnumerator chessChangeFaceReview(int face, int id, GameObject point, string[] lastStep, int chessReviewStep)
    {
        yield return null;
        GameObject chessPieceChangeFace = point;
        switch (face)
        {
            case 40:
                StartCoroutine(chessRotationChangeFace(0.5f, 8, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "mared" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
            case 33:
                StartCoroutine(chessRotationChangeFace(0.5f, 10, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "tuongred" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
            case 16:
                StartCoroutine(chessRotationChangeFace(0.5f, 6, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "haured" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
            case 24:
                StartCoroutine(chessRotationChangeFace(0.5f, 11, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "xered" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
            case -40:
                StartCoroutine(chessRotationChangeFace(0.5f, 2, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "mablack" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
            case -33:
                StartCoroutine(chessRotationChangeFace(0.5f, 4, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "tuongblack" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
            case -16:
                StartCoroutine(chessRotationChangeFace(0.5f, 0, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "haublack" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
            case -24:
                StartCoroutine(chessRotationChangeFace(0.5f, 5, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "xeblack" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                chessPieceChangeFace.name += ".phongto" + chessReviewStep;
                break;
        }
    }
    public void chessReviewMediaPrevious(float time)
    {
        chessTableClearPointMove();
        if (chessReviewStep - 1 == 0)
        {
            chessReviewButton[2].SetActive(true);
            chessReviewButton[3].SetActive(false);
        }
        if (chessReviewStep > 0)
        {
            GameObject chessMove = chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].transform.GetChild(1).gameObject;
            chessMove.transform.SetParent(chessMove.transform.parent.parent);
            RectTransform rtf = chessMove.GetComponent<RectTransform>();
            RectTransform rtfTarget = chessPointToSlot[chessReviewSourcePos[chessReviewStep - 1]].GetComponent<RectTransform>();
            if (chessReviewColor[chessReviewStep - 1] == "w")
            {
                for (int i = chessEatPlayerTopElement.transform.parent.childCount - 1; i > 1; i--)
                {
                    if (chessEatPlayerTopElement.transform.parent.GetChild(i).name.Contains(".step" + (chessReviewStep - 1)))
                    {
                        //App.trace((chessReviewStep - 1).ToString());
                        GameObject chessStart = chessEatPlayerTopElement.transform.parent.GetChild(i).gameObject;
                        RectTransform rtfEat = chessStart.GetComponent<RectTransform>();
                        RectTransform rtfPos = chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].GetComponent<RectTransform>();
                        chessStart.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].transform.parent.parent);
                        rtfEat.sizeDelta = new Vector2(105, 116);
                        DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), time).OnComplete(() =>
                        {
                            string[] chessId = chessMove.name.Split('.');
                            int id = Int32.Parse(chessId[0]);
                            string[] lastStep = chessMove.name.Split('.').Skip(3).ToArray();
                            if (chessStart.name.Contains(".phongto" + (chessReviewStep - 1)) && chessStart.name.Contains("black"))
                            {
                                App.trace("aaaa");
                            }
                            else if (chessStart.name.Contains(".phongto" + (chessReviewStep - 1)) && chessStart.name.Contains("red"))
                            {
                                App.trace("bbbb");
                            }
                            chessStart.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].transform);
                        });
                    }
                }
            }
            else if (chessReviewColor[chessReviewStep - 1] == "b")
            {
                for (int i = chessEatPlayerBotElement.transform.parent.childCount - 1; i > 1; i--)
                {
                    if (chessEatPlayerBotElement.transform.parent.GetChild(i).name.Contains(".step" + (chessReviewStep - 1)))
                    {
                        //App.trace((chessReviewStep - 1).ToString());
                        GameObject chessStart = chessEatPlayerBotElement.transform.parent.GetChild(i).gameObject;
                        RectTransform rtfEat = chessStart.GetComponent<RectTransform>();
                        RectTransform rtfPos = chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].GetComponent<RectTransform>();
                        chessStart.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].transform.parent.parent);
                        rtfEat.sizeDelta = new Vector2(105, 116);
                        DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), time).OnComplete(() =>
                        {
                            string[] chessId = chessMove.name.Split('.');
                            int id = Int32.Parse(chessId[0]);
                            string[] lastStep = chessMove.name.Split('.').Skip(3).ToArray();
                            if (chessStart.name.Contains(".phongto" + (chessReviewStep - 1)) && chessStart.name.Contains("black"))
                            {
                                StartCoroutine(chessReviewReFace(id, -53, chessMove, lastStep));
                            }
                            else if (chessStart.name.Contains(".phongto" + (chessReviewStep - 1)) && chessStart.name.Contains("red"))
                            {
                                StartCoroutine(chessReviewReFace(id, 53, chessMove, lastStep));
                            }
                            chessStart.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].transform);
                        });
                    }
                }
            }
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfTarget.anchoredPosition.x, rtfTarget.anchoredPosition.y), time).OnComplete(() =>
            {
                string[] chessId = chessMove.name.Split('.');
                int id = Int32.Parse(chessId[0]);
                string[] lastStep = chessMove.name.Split('.').Skip(3).ToArray();
                if (chessMove.name.Contains(".phongto" + (chessReviewStep - 1)) && chessMove.name.Contains("black"))
                {
                    StartCoroutine(chessReviewReFace(-53, id, chessMove, lastStep));
                }
                else if (chessMove.name.Contains(".phongto" + (chessReviewStep - 1)) && chessMove.name.Contains("red"))
                {
                    StartCoroutine(chessReviewReFace(53, id, chessMove, lastStep));
                }
                chessMove.transform.SetParent(chessPointToSlot[chessReviewSourcePos[chessReviewStep - 1]].transform);
                chessReviewStep -= 1;
            });

        }
    }
    IEnumerator chessReviewReFace(int face, int id, GameObject point, string[] lastStep)
    {
        yield return null;
        GameObject chessPieceChangeFace = point;
        switch (face)
        {
            case 53:
                StartCoroutine(chessRotationChangeFace(0.5f, 9, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "totred" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                break;
            case -53:
                StartCoroutine(chessRotationChangeFace(0.5f, 3, chessPieceChangeFace, Vector3.zero, new Vector3(0, 90, 0)));
                chessPieceChangeFace.name = id + "." + "totblack" + "." + id;
                for (int i = 0; i < lastStep.Length; i++)
                {
                    chessPieceChangeFace.name += "." + lastStep[i];
                }
                break;
        }
    }
    IEnumerator chessReviewMediaFirst()
    {
        for (int j = chessReviewStep; j > 0; j--)
        {
            chessReviewMediaPrevious(.2f);
            yield return new WaitForSeconds(0.5f);
            if (chessReviewPause)
            {
                break;
            }
        }

    }
    IEnumerator chessReviewMediaLast()
    {
        for (int i = chessReviewStep; i < chessReviewSourcePos.Length; i++)
        {
            if (i == chessReviewSourcePos.Length - 1)
            {
                StartCoroutine(_chessShowReviewMatchInfoEndPopup());
                chessReviewButton[2].SetActive(true);
                chessReviewButton[3].SetActive(false);
            }
            chessReviewMediaNext(.2f);
            yield return new WaitForSeconds(0.5f);
            if (chessReviewPause)
            {
                break;
            }
        }
    }
    IEnumerator chessReviewMediaPlay()
    {
        for (int i = chessReviewStep; i < chessReviewSourcePos.Length; i++)
        {
            if (i == chessReviewSourcePos.Length - 1)
            {
                StartCoroutine(_chessShowReviewMatchInfoEndPopup());
                chessReviewButton[2].SetActive(true);
                chessReviewButton[3].SetActive(false);
            }
            chessReviewMediaNext(.3f);
            yield return new WaitForSeconds(1f);
            if (chessReviewPause)
            {
                break;
            }
        }
    }
    IEnumerator _chessShowReviewMatchInfoPopup()
    {
        yield return new WaitForSeconds(0.2f);
        chessShowReviewMatchInfoPopup(true);

    }
    public void chessShowReviewMatchInfoPopup(bool open)
    {
        RectTransform rtf = chessReviewMatchPopup.GetComponent<RectTransform>();
        if (open)
        {
            chessReviewMatchPopup.SetActive(true);
            chessBlackPanel.SetActive(true);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .3f).OnComplete(() =>
            {

            });
        }
        else
        {
            chessBlackPanel.SetActive(false);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
            {
                chessReviewMatchPopup.SetActive(false);
            });
        }
    }
    IEnumerator _chessShowReviewMatchInfoEndPopup()
    {
        yield return new WaitForSeconds(1.5f);
        chessShowReviewMatchInfoEndPopup(true);
    }
    public void chessShowReviewMatchInfoEndPopup(bool open)
    {
        RectTransform rtf = chessReviewMatchPopupEnd.GetComponent<RectTransform>();
        if (open)
        {
            chessReviewMatchPopupEnd.SetActive(true);
            chessBlackPanel.SetActive(true);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, Vector2.zero, .3f).OnComplete(() =>
            {

            });
        }
        else
        {
            chessBlackPanel.SetActive(false);
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(0, 960), .3f).OnComplete(() =>
            {
                chessReviewMatchPopupEnd.SetActive(false);
            });
        }
    }

    [Header("=====SETTING====")]
    private int a;
    public void chessOpenSetting()
    {
        LoadingControl.instance.settingPanel.SetActive(true);
    }
}
