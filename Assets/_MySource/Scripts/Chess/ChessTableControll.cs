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

public class ChessTableControll : MonoBehaviour
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
    public static ChessTableControll instance;
    private GameObject chessEatPlayerBotMoveClone;
    private GameObject chessEatPlayerTopMoveClone;
    private int chessTargetNextStep =-1;
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
        //LoadingControl.instance.miniGamePanel.SetActive(false);
        CPlayer.currSceneChess = this.gameObject;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //LoadingControl.instance.blackkkkkk.SetActive(false);
        if (CPlayer.typePlay == "play")
        {
            //App.trace("CPlayer.clientTargetMode " + CPlayer.clientTargetMode);
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
                if (attrName == "matchDuration")
                {
                    chessTableText[1].text = attrValue + "'/ván";
                    string getnumber = Regex.Match(attrValue, @"\d+").Value;
                    int numbertext = Int32.Parse(getnumber);
                    CPlayer.currentTurnTimeout = numbertext;
                }
                else if (attrName == "turnDuration")
                {
                    string getnumber = Regex.Match(attrValue, @"\d+").Value;
                    int numbertext = Int32.Parse(getnumber);
                    if (numbertext == -1 || numbertext == 1)
                    {
                        chessTableText[1].text = chessTableText[1].text + "\n" + "∞" + "'/nước";
                    }
                    else
                    {
                        chessTableText[1].text = chessTableText[1].text + "\n" + numbertext / 60 + "'/nước";
                    }
                    CPlayer.currentTurnDurationvalue = numbertext;
                }
            }
            int tableType = res.readByte();
            //App.trace("Chess - tableType " + tableType);
            int betAmtId = res.readByte();
            //.trace("Chess - betAmtId " + betAmtId);
            CPlayer.betAmtOfTableToGo = chessBet[betAmtId].ToString();
            chessTableText[0].text = CPlayer.gameNameFull + "\n" + App.formatMoney(chessBet[betAmtId].ToString()) + " Gold";
            if (CPlayer.clientCurrentMode == 0)
            {               
                chessButton[1].SetActive(false);
            }
            else if(CPlayer.clientCurrentMode == 1 || CPlayer.chipBalance < float.Parse(CPlayer.betAmtOfTableToGo))
            {
                chessChangeClientMode(1);
                chessButton[1].SetActive(false);                
            }else if(CPlayer.clientCurrentMode == 1)
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
    private int[] chessIdOpen;
    private int[] chessFaceOpen;
    private int chessTotalMove;
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
                /*
                isKicked = true;
                isPlaying = false;
                statusKick = content;
                backToTableList();
                */
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
            chessTableClearPointMove();
            for (int m = 0; m < chessPointToSlot.Length; m++)
            {
                chessPointToSlot[m].GetComponent<Image>().enabled = false;
                if (chessPointToSlot[m].transform.childCount > 1)
                {
                    chessPointToSlot[m].transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
                    chessPointToSlot[m].transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                }
            }
            App.trace("MOVE");
            int sourcePosition = res.readByte();
            //App.trace("Chess Move -sourcePosition " + sourcePosition);
            int targetPosition = res.readByte();
            //App.trace("Chess Move -targetPosition " + targetPosition);
            int totalMove = res.readByte();
            chessTotalMove = totalMove;
            chessIdOpen = new int[totalMove];
            chessFaceOpen = new int[totalMove];
            //App.trace("Chess Move -totalMove " + totalMove);
            for (int i = 0; i < totalMove; i++)
            {
                int id = res.readByte();
                chessIdOpen[i] = id;
                //App.trace("Chess Move -id " + id);
                int face = res.readByte();
                chessFaceOpen[i] = face;
                //App.trace("Chess Move -face " + face);              
            }
            if (chessNextStep != null && chessLastStep != null)
            {
                chessLastStep.GetComponent<Image>().enabled = false;
                chessNextStep.SetActive(false);
            }
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
                    if (!chessEat.name.Contains("open") && !chessEat.name.Contains(chessMyColor))
                    {
                        if (CPlayer.gameName == "mystery_xiangqi")
                        {
                            chessEat.transform.GetChild(1).gameObject.SetActive(true);
                        }
                    }
                    if (chessTotalMove == 1)
                    {
                        string[] nameChessEat = chessEat.name.Split('.');
                        string nameLastChessEat = nameChessEat[1];
                        string[] nameChessMove = chessMove.name.Split('.');
                        string nameLastChessMove = nameChessMove[1];
                        if (nameLastChessEat == chessIdOpen[0].ToString())
                            chessShowFaceOpen(chessFaceOpen[0], chessIdOpen[0], chessEat);
                        else if (nameLastChessMove == chessIdOpen[0].ToString())
                            chessShowFaceOpen(chessFaceOpen[0], chessIdOpen[0], chessMove);
                    }
                    else if (chessTotalMove == 2 && chessEat.name.Contains(chessIdOpen[1].ToString()))
                    {
                        chessShowFaceOpen(chessFaceOpen[1], chessIdOpen[1], chessEat);
                        chessShowFaceOpen(chessFaceOpen[0], chessIdOpen[0], chessMove);
                    }
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
                }else if (totalMove > 0 && chessMove.name.Contains(chessIdOpen[0].ToString()))
                {
                    chessShowFaceOpen(chessFaceOpen[0], chessIdOpen[0], chessMove);
                }
                chessMove.transform.SetParent(chessPointToSlot[targetPosition].transform);                
                //LoadingControl.instance.playSound("step");
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
        //req_getTableChange.print();
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
                //preTimeLeapImage.fillAmount = 1;
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
                        chessMyColor = "black";
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
                        chessMyColor = "red";
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
            /*
            if (nickName == CPlayer.nickName)
            {
                mySlotId = slotId;
            }
            if (isOwner)
            {
                currOwnerId = slotId;
            }

            Player player = new Player(detecSlotIdBySvrId(slotId), slotId, playerId, nickName, avatarId, avatar, isMale, chipBalance, starBalance, score, level, isOwner);

            slotId = detecSlotIdBySvrId(slotId);

            if (nickName.Length == 0)    //Có thằng thoát khỏi bàn chơi
            {
                playerList.Remove(slotId);
                infoObjectList[slotId].SetActive(false);
                playerAvatarList[slotId].overrideSprite = addPlayerIcon;
                exitsSlotList[slotId] = false;
                if (slotId != 0)
                {
                    backCards[slotId - 1].SetActive(false);
                    ownerList[slotId].SetActive(false);
                }

                return;
            }
            if (playerList.ContainsKey(slotId)) //Có thằng thay thế
            {
                playerList[slotId] = player;
                PlayerManager.instance.setInfo(player, playerAvatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId]);
                exitsSlotList[slotId] = true;
                return;
            }

            //Thêm bình thường
            playerList.Add(slotId, player);
            PlayerManager.instance.setInfo(player, playerAvatarList[slotId], infoObjectList[slotId], balanceTextList[slotId], nickNameTextList[slotId], ownerList[slotId]);
            exitsSlotList[slotId] = true;
            */
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
            //App.trace("Chess Ower - slot Ower change " + slotId);
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
            for (int m = 0; m < chessPointToSlot.Length; m++)
            {
                if (chessPointToSlot[m].transform.childCount > 1)
                    chessPointToSlot[m].transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
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
            //App.trace("START_MATCH");
            for (int i = 0; i < chessTimeRemain.Length; i++)
            {
                chessTimeRemain[i].text = "";
            }
            run = true;
            StartCoroutine(chessCountDownMainTime());
            loadPlayerMatchPoint(res);
            loadBoardData(res, true);
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
    public void chessShowFaceOpen(int face, int id, GameObject point)
    {
        switch (face)
        {
            case 57:
            case 58:
            case 59:
            case 60:
            case 61:
                StartCoroutine(chessRotationChangeFace(0.5f, 12, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[12];
                if (CPlayer.typePlay == "play")
                    point.name = id + "totred" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "totred" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "totred" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case 8:
                StartCoroutine(chessRotationChangeFace(0.5f, 8, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[8];
                if (CPlayer.typePlay == "play")
                    point.name = id + "kingred" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "kingred" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "kingred" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case 17:
            case 18:
                StartCoroutine(chessRotationChangeFace(0.5f, 11, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[11];
                if (CPlayer.typePlay == "play")
                    point.name = id + "sired" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "sired" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "sired" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case 25:
            case 26:
                StartCoroutine(chessRotationChangeFace(0.5f, 13, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[13];
                if (CPlayer.typePlay == "play")
                    point.name = id + "tuongred" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "tuongred" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "tuongred" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case 33:
            case 34:
                StartCoroutine(chessRotationChangeFace(0.5f, 15, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[15];
                if (CPlayer.typePlay == "play")
                    point.name = id + "xered" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "xered" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "xered" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case 41:
            case 42:
                StartCoroutine(chessRotationChangeFace(0.5f, 10, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[10];
                if (CPlayer.typePlay == "play")
                    point.name = id + "phaored" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "phaored" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "phaored" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case 49:
            case 50:
                StartCoroutine(chessRotationChangeFace(0.5f, 9, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[9];
                if (CPlayer.typePlay == "play")
                    point.name = id + "mared" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "mared" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "mared" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case -57:
            case -58:
            case -59:
            case -60:
            case -61:
                StartCoroutine(chessRotationChangeFace(0.5f, 4, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[4];
                if (CPlayer.typePlay == "play")
                    point.name = id + "totblack" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "totblack" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "totblack" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case -8:
                StartCoroutine(chessRotationChangeFace(0.5f, 0, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[0];
                if (CPlayer.typePlay == "play")
                    point.name = id + "kingblack" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "kingblack" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "kingblack" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case -17:
            case -18:
                StartCoroutine(chessRotationChangeFace(0.5f, 3, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[3];
                if (CPlayer.typePlay == "play")
                    point.name = id + "siblack" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "siblack" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "siblack" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case -25:
            case -26:
                StartCoroutine(chessRotationChangeFace(0.5f, 5, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[5];
                if (CPlayer.typePlay == "play")
                    point.name = id + "tuongblack" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "tuongblack" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "tuongblack" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case -33:
            case -34:
                StartCoroutine(chessRotationChangeFace(0.5f, 7, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[7];
                if (CPlayer.typePlay == "play")
                    point.name = id + "xeblack" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "xeblack" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "xeblack" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case -41:
            case -42:
                StartCoroutine(chessRotationChangeFace(0.5f, 2, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[2];
                if (CPlayer.typePlay == "play")
                    point.name = id + "phaoblack" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "phaoblack" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "phaoblack" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
            case -49:
            case -50:
                StartCoroutine(chessRotationChangeFace(0.5f, 1, point, Vector3.zero, new Vector3(90, 0, 0)));
                //point.GetComponent<Image>().sprite = chessTuongFace[1];
                if (CPlayer.typePlay == "play")
                    point.name = id + "mablack" + "." + id + "open";
                else
                {
                    if (point.name.Contains(".step"))
                    {
                        string[] nameChess = point.name.Split('.');
                        string step = nameChess[5];
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "mablack" + "." + id + "open" + "." + openinstep + "." + lastface + "." + step;
                    }
                    else
                    {
                        string[] nameChess = point.name.Split('.');
                        string lastface = nameChess[4];
                        string openinstep = nameChess[3];
                        point.name = id + "." + "mablack" + "." + id + "open" + "." + openinstep + "." + lastface;
                    }
                }
                break;
        }
        /*
        if (face > 0)
        {
            point.name = point.name + "red";
        }
        else
        {
            point.name = point.name + "black";
        }
        */
    }
    IEnumerator chessCloseGridLayout()
    {
        yield return new WaitForSeconds(.1f);
        chessGrid.enabled = false;
    }
    // Effect
    IEnumerator chessCloseEffect()
    {
        yield return new WaitForSeconds(1f);
        RectTransform rtfPopUpShowValue = chessPopupShowValueMatch.GetComponent<RectTransform>();
        chessBlackPanel.SetActive(true);
        chessPopupShowValueMatch.SetActive(true);
        DOTween.To(() => rtfPopUpShowValue.anchoredPosition, x => rtfPopUpShowValue.anchoredPosition = x, Vector2.zero, .25f).OnComplete(() =>
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
        DOTween.To(() => rtfPopUpShowValue.anchoredPosition, x => rtfPopUpShowValue.anchoredPosition = x, new Vector2(0, 960), .25f).OnComplete(() =>
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
            //preTimeLeapImage.fillAmount = 1;
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
    public void chessReFaceOpen(int sid, GameObject chessClone)
    {
        //App.trace("SIDDDDDDDDDDDDDDDD " + sid);
        switch (sid)
        {
            case 57:
            case 58:
            case 59:
            case 60:
            case 61:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[12];
                chessClone.name = sid + "totred" + "." + sid;
                break;
            case 8:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[8];
                chessClone.name = sid + "kingred" + "." + sid;
                break;
            case 17:
            case 18:

                //chessClone.GetComponent<Image>().sprite = chessTuongFace[11];
                chessClone.name = sid + "sired" + "." + sid;
                break;
            case 25:
            case 26:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[13];
                chessClone.name = sid + "tuongred" + "." + sid;
                break;
            case 33:
            case 34:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[15];
                chessClone.name = sid + "xered" + "." + sid;
                break;
            case 41:
            case 42:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[10];
                chessClone.name = sid + "phaored" + "." + sid;
                break;
            case 49:
            case 50:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[9];
                chessClone.name = sid + "mared" + "." + sid;
                break;
            case -57:
            case -58:
            case -59:
            case -60:
            case -61:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[4];
                chessClone.name = sid + "totblack" + "." + sid;
                break;
            case -8:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[0];
                chessClone.name = sid + "kingblack" + "." + sid;
                break;
            case -17:
            case -18:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[3];
                chessClone.name = sid + "siblack" + "." + sid;
                break;
            case -25:
            case -26:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[5];
                chessClone.name = sid + "tuongblack" + "." + sid;
                break;
            case -33:
            case -34:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[7];
                chessClone.name = sid + "xeblack" + "." + sid;
                break;
            case -41:
            case -42:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[2];
                chessClone.name = sid + "phaoblack" + "." + sid;
                break;
            case -49:
            case -50:
                //chessClone.GetComponent<Image>().sprite = chessTuongFace[1];
                chessClone.name = sid + "mablack" + "." + sid;
                break;
        }
    }
    public void loadBoardData(InBoundMessage res, bool start = false)
    {
        int count = res.readByte();
        //App.trace("loadBoardData - count " + count);
        for (int i = 0; i < count; i++)
        {
            int sid = res.readByte();
            //App.trace("loadBoardData - sid "+i +"||"+ sid);
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
                        //if (chessPointToSlot[j].transform.childCount > 1 && chessPointToSlot[j].transform.GetChild(1).name.Contains(sid.ToString()) && chessPointToSlot[j].transform.GetChild(1).name.Contains("open"))
                        //{
                        //    chessReFaceOpen(sid, chessPointToSlot[j].transform.GetChild(1).gameObject);
                        //}
                        if (chessPointToSlot[j].transform.childCount > 1 && chessPointToSlot[j].transform.GetChild(1).name.Contains(sid.ToString()) && chessPointToSlot[j].transform.GetChild(1).name.Contains("." + sid.ToString()))
                        {
                            GameObject chessStart = chessPointToSlot[j].transform.GetChild(1).gameObject;
                            if (sid > 0 && !open)
                            {
                                chessStart.GetComponent<Image>().sprite = chessTuongFace[14];
                            }
                            else if (sid < 0 && !open)
                            {
                                chessStart.GetComponent<Image>().sprite = chessTuongFace[6];
                            }
                            RectTransform rtf = chessStart.GetComponent<RectTransform>();
                            RectTransform rtfPos = chessPointToSlot[position].GetComponent<RectTransform>();
                            chessStart.transform.SetParent(chessStart.transform.parent.parent);
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                chessStart.transform.SetParent(chessPointToSlot[position].transform);
                                chessReFaceOpen(sid, chessPointToSlot[position].transform.GetChild(1).gameObject);
                            });
                        }
                    }
                    for (int j = chessEatPlayerBotElement.transform.parent.childCount - 1; j > 1; j--)
                    {
                        //if (chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains("open"))
                        //{
                        //    chessReFaceOpen(sid, chessEatPlayerBotElement.transform.parent.GetChild(j).gameObject);
                        //}
                        if (chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerBotElement.transform.parent.GetChild(j).name.Contains("." + sid.ToString()))
                        {
                            GameObject chessStart = chessEatPlayerBotElement.transform.parent.GetChild(j).gameObject;
                            if (sid > 0 && !open)
                            {
                                chessStart.GetComponent<Image>().sprite = chessTuongFace[14];
                            }
                            else if (sid < 0 && !open)
                            {
                                chessStart.GetComponent<Image>().sprite = chessTuongFace[6];
                            }
                            RectTransform rtf = chessStart.GetComponent<RectTransform>();
                            RectTransform rtfPos = chessPointToSlot[position].GetComponent<RectTransform>();
                            chessStart.transform.SetParent(chessPointToSlot[position].transform.parent.parent);
                            rtf.sizeDelta = new Vector2(80, 80);
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                chessStart.transform.GetChild(1).gameObject.SetActive(false);
                                chessStart.transform.SetParent(chessPointToSlot[position].transform);
                                chessReFaceOpen(sid, chessPointToSlot[position].transform.GetChild(1).gameObject);
                            });
                        }
                    }
                    for (int j = chessEatPlayerTopElement.transform.parent.childCount - 1; j > 1; j--)
                    {
                        //if (chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains("open"))
                        //{
                        //    chessReFaceOpen(sid, chessEatPlayerTopElement.transform.parent.GetChild(j).gameObject);
                        //}
                        if (chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains(sid.ToString()) && chessEatPlayerTopElement.transform.parent.GetChild(j).name.Contains("." + sid.ToString()))
                        {
                            GameObject chessStart = chessEatPlayerTopElement.transform.parent.GetChild(j).gameObject;
                            if (sid > 0 && !open)
                            {
                                chessStart.GetComponent<Image>().sprite = chessTuongFace[14];
                            }
                            else if (sid < 0 && !open)
                            {
                                chessStart.GetComponent<Image>().sprite = chessTuongFace[6];
                            }
                            RectTransform rtf = chessStart.GetComponent<RectTransform>();
                            RectTransform rtfPos = chessPointToSlot[position].GetComponent<RectTransform>();
                            chessStart.transform.SetParent(chessPointToSlot[position].transform.parent.parent);
                            rtf.sizeDelta = new Vector2(80, 80);
                            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), .25f).OnComplete(() =>
                            {
                                chessStart.transform.GetChild(1).gameObject.SetActive(false);
                                chessStart.transform.SetParent(chessPointToSlot[position].transform);
                                chessReFaceOpen(sid, chessPointToSlot[position].transform.GetChild(1).gameObject);
                            });
                        }
                    }
                }
                else
                {
                    GameObject chessClone = Instantiate(chessElement, chessPointToSlot[position].transform, false);
                    switch (face)
                    {
                        case 57:
                        case 58:
                        case 59:
                        case 60:
                        case 61:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[12];
                                chessClone.name = sid + "totred" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "totred" + "." + sid;
                            }
                            break;
                        case 8:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[8];
                                chessClone.name = sid + "kingred" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "kingred" + "." + sid;
                            }
                            break;
                        case 17:
                        case 18:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[11];
                                chessClone.name = sid + "sired" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "sired" + "." + sid;
                            }
                            break;
                        case 25:
                        case 26:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[13];
                                chessClone.name = sid + "tuongred" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "tuongred" + "." + sid;
                            }
                            break;
                        case 33:
                        case 34:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[15];
                                chessClone.name = sid + "xered" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "xered" + "." + sid;
                            }
                            break;
                        case 41:
                        case 42:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[10];
                                chessClone.name = sid + "phaored" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "phaored" + "." + sid;
                            }
                            break;
                        case 49:
                        case 50:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[9];
                                chessClone.name = sid + "mared" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "mared" + "." + sid;
                            }
                            break;
                        case -57:
                        case -58:
                        case -59:
                        case -60:
                        case -61:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[4];
                                chessClone.name = sid + "totblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "totblack" + "." + sid;
                            }
                            break;
                        case -8:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[0];
                                chessClone.name = sid + "kingblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "kingblack" + "." + sid;
                            }
                            break;
                        case -17:
                        case -18:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[3];
                                chessClone.name = sid + "siblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "siblack" + "." + sid;
                            }
                            break;
                        case -25:
                        case -26:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[5];
                                chessClone.name = sid + "tuongblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "tuongblack" + "." + sid;
                            }
                            break;
                        case -33:
                        case -34:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[7];
                                chessClone.name = sid + "xeblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "xeblack" + "." + sid;
                            }
                            break;
                        case -41:
                        case -42:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[2];
                                chessClone.name = sid + "phaoblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "phaoblack" + "." + sid;
                            }
                            break;
                        case -49:
                        case -50:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[1];
                                chessClone.name = sid + "mablack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "mablack" + "." + sid;
                            }
                            break;
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
                        case 57:
                        case 58:
                        case 59:
                        case 60:
                        case 61:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[12];
                                chessClone.name = sid + "totred" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "totred" + "." + sid;
                            }
                            break;
                        case 8:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[8];
                                chessClone.name = sid + "kingred" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "kingred" + "." + sid;
                            }
                            break;
                        case 17:
                        case 18:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[11];
                                chessClone.name = sid + "sired" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "sired" + "." + sid;
                            }
                            break;
                        case 25:
                        case 26:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[13];
                                chessClone.name = sid + "tuongred" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "tuongred" + "." + sid;
                            }
                            break;
                        case 33:
                        case 34:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[15];
                                chessClone.name = sid + "xered" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "xered" + "." + sid;
                            }
                            break;
                        case 41:
                        case 42:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[10];
                                chessClone.name = sid + "phaored" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "phaored" + "." + sid;
                            }
                            break;
                        case 49:
                        case 50:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[9];
                                chessClone.name = sid + "mared" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[14];
                                chessClone.name = sid + "mared" + "." + sid;
                            }
                            break;
                    }
                    chessClone.SetActive(true);
                }
                else
                {
                    GameObject chessClone = Instantiate(chessEatPlayerTopElement, chessEatPlayerTopElement.transform.parent, false);
                    switch (face)
                    {
                        case -57:
                        case -58:
                        case -59:
                        case -60:
                        case -61:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[4];
                                chessClone.name = sid + "totblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "totblack" + "." + sid;
                            }
                            break;
                        case -8:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[0];
                                chessClone.name = sid + "kingblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "kingblack" + "." + sid;
                            }
                            break;
                        case -17:
                        case -18:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[3];
                                chessClone.name = sid + "siblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "siblack" + "." + sid;
                            }
                            break;
                        case -25:
                        case -26:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[5];
                                chessClone.name = sid + "tuongblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "tuongblack" + "." + sid;
                            }
                            break;
                        case -33:
                        case -34:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[7];
                                chessClone.name = sid + "xeblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "xeblack" + "." + sid;
                            }
                            break;
                        case -41:
                        case -42:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[2];
                                chessClone.name = sid + "phaoblack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "phaoblack" + "." + sid;
                            }
                            break;
                        case -49:
                        case -50:
                            if (open)
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[1];
                                chessClone.name = sid + "mablack" + "." + sid + "open";
                            }
                            else
                            {
                                chessClone.GetComponent<Image>().sprite = chessTuongFace[6];
                                chessClone.name = sid + "mablack" + "." + sid;
                            }
                            break;
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
        int allowPassRiver = res.readByte();
        //App.trace("loadBoardData - allowPassRiver " + allowPassRiver);

        int count2 = res.readByte();
        //App.trace("loadBoardData - count2 " + count2);
        for (int i = 0; i < count2; i++)
        {
            int piece = res.readByte();
            //App.trace("loadBoardData - piece " + piece);
            for(int j = 0;j< chessEatPlayerTopElement.transform.parent.childCount - 1; j++)
            {
                if (chessEatPlayerTopElement.transform.parent.GetChild(j).gameObject.name.Contains(piece.ToString()))
                {
                    chessEatPlayerTopElement.transform.parent.GetChild(j).GetChild(1).gameObject.SetActive(true);
                }
            }
            for (int j = 0; j < chessEatPlayerBotElement.transform.parent.childCount - 1; j++)
            {
                if (chessEatPlayerBotElement.transform.parent.GetChild(j).gameObject.name.Contains(piece.ToString()))
                {
                    chessEatPlayerBotElement.transform.parent.GetChild(j).GetChild(1).gameObject.SetActive(true);
                }
            }

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
    private int chessLastClickPoint = -1;
    public void chessTableControllPiece(string namePiece, string pointName, string type)
    {
        for(int m = 0;m < chessPointToSlot.Length; m++)
        {
            if (chessPointToSlot[m].transform.childCount > 1)
                chessPointToSlot[m].transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
        }
        chessSourcePointDrag = pointName;
        chessPointChange = null;
        bool justOpen = false;
        if (namePiece.Contains("open"))
        {
            justOpen = true;
        }
        string getnumber = Regex.Match(pointName, @"\d+").Value;
        int point = Int32.Parse(getnumber);       
        App.trace("Naaaaaaaaaaaaaaaaaaaaaameeeeeeeeeeee " + namePiece + "|| " + pointName + "|| " + type);
        // ChangeName
        if (namePiece.Contains("mared"))
        {
            namePiece = "mared";
        }
        else if (namePiece.Contains("mablack"))
        {
            namePiece = "mablack";
        }
        else if (namePiece.Contains("phaored"))
        {
            namePiece = "phaored";
        }
        else if (namePiece.Contains("phaoblack"))
        {
            namePiece = "phaoblack";
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
        else if (namePiece.Contains("sired"))
        {
            namePiece = "sired";
        }
        else if (namePiece.Contains("siblack"))
        {
            namePiece = "siblack";
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
        switch (namePiece)
        {
            case "mared":
            case "mablack":
                if (i >= 81)
                {
                    if (i != 89 && i != 80 && i != 88)
                    {
                        if (chessPointToSlot[i + 1].transform.childCount < 2)
                        {
                            chessMovePiece(i - 7, type);
                        }
                    }
                    if (i != 89 && i != 80)
                    {
                        if (chessPointToSlot[i - 9].transform.childCount < 2)
                        {
                            chessMovePiece(i - 17, type);
                        }
                    }
                    if (i != 82 && i != 81)
                    {
                        if (chessPointToSlot[i - 1].transform.childCount < 2)
                        {
                            chessMovePiece(i - 11, type);
                        }
                    }
                    if (i != 81)
                    {
                        if (chessPointToSlot[i - 9].transform.childCount < 2)
                        {
                            chessMovePiece(i - 19, type);
                        }
                    }
                }
                else if (i >= 72)
                {
                    if (i != 72 && i != 73)
                    {
                        if (chessPointToSlot[i - 1].transform.childCount < 2)
                        {
                            chessMovePiece(i - 11, type);
                            chessMovePiece(i + 7, type);
                        }
                    }
                    if (i != 80)
                    {
                        if (chessPointToSlot[i - 9].transform.childCount < 2)
                        {
                            chessMovePiece(i - 17, type);
                        }
                    }
                    if (i != 72)
                    {
                        if (chessPointToSlot[i - 9].transform.childCount < 2)
                        {
                            chessMovePiece(i - 19, type);
                        }
                    }
                    if (i < 79)
                    {
                        if (chessPointToSlot[i + 1].transform.childCount < 2)
                        {
                            chessMovePiece(i + 11, type);
                            chessMovePiece(i - 7, type);
                        }
                    }
                }
                else if (i <= 8)
                {
                    if (i != 1 && i != 9 && i != 0)
                    {
                        if (chessPointToSlot[i - 1].transform.childCount < 2)
                        {
                            chessMovePiece(i + 7, type);
                        }
                    }
                    if (i != 7 && i != 8)
                    {
                        if (chessPointToSlot[i + 1].transform.childCount < 2)
                        {
                            chessMovePiece(i + 11, type);
                        }
                    }
                    if (i != 9 && i != 0)
                    {
                        chessMovePiece(i + 17, type);
                    }
                    if (i != 8)
                    {
                        chessMovePiece(i + 19, type);
                    }
                }
                else if (i <= 17)
                {
                    if (i > 7)
                    {
                        if (i != 16 && i != 8 && i != 17)
                        {
                            chessMovePiece(i - 7, type);
                        }
                    }
                    if (i > 10)
                    {
                        chessMovePiece(i - 11, type);
                    }
                    if (i != 10 && i != 9)
                    {
                        chessMovePiece(i + 7, type);
                    }
                    if (i != 7 && i != 16 && i != 8 && i != 17)
                    {
                        chessMovePiece(i + 11, type);
                    }
                    if (i != 9)
                    {
                        chessMovePiece(i + 17, type);
                    }
                    if (i != 8 && i != 17)
                    {
                        chessMovePiece(i + 19, type);
                    }
                }
                else
                {
                    if (chessPointToSlot[i + 1].transform.childCount < 2)
                    {
                        if (i != 70 && i != 61 && i != 52 && i != 43 && i != 34 && i != 25 && i != 71 && i != 35 && i != 26 && i != 53 && i != 44 && i!= 62)
                        {
                            chessMovePiece(i - 7, type);
                        }
                        if (i != 70 && i != 61 && i != 52 && i != 43 && i != 34 && i != 25 && i != 62 && i != 53 && i != 44 && i != 35 && i != 26 && i != 71)
                        {
                            chessMovePiece(i + 11, type);
                        }
                    }
                    if (chessPointToSlot[i - 1].transform.childCount < 2)
                    {
                        if (i != 64 && i != 55 && i != 46 && i != 37 && i != 28 && i != 19 && i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                        {
                            chessMovePiece(i - 11, type);
                        }
                        if (i != 64 && i != 55 && i != 46 && i != 37 && i != 28 && i != 19 && i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                        {
                            chessMovePiece(i + 7, type);
                        }
                    }
                    if (chessPointToSlot[i - 9].transform.childCount < 2)
                    {
                        if (i != 62 && i != 53 && i != 44 && i != 35 && i != 26 && i != 71)
                        {
                            chessMovePiece(i - 17, type);
                        }
                        if (i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                        {
                            chessMovePiece(i - 19, type);
                        }
                    }
                    if (chessPointToSlot[i + 9].transform.childCount < 2)
                    {
                        if (i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                        {
                            chessMovePiece(i + 17, type);
                        }
                        if (i != 62 && i != 53 && i != 44 && i != 35 && i != 26 && i != 71)
                        {
                            chessMovePiece(i + 19, type);
                        }
                    }
                }
                /*
                if (i < 89)
                {
                    if (chessPointToSlot[i + 1].transform.GetChildCount() > 1)
                    {
                        if (i < 79 && i != 7 && i != 16 && i != 8 && i != 70 && i != 61 && i != 52 && i != 43 && i != 34 && i != 25 && i != 62 && i != 53 && i != 44 && i != 35 && i != 26 && i != 71)
                            chessPointToSlot[i + 11].transform.GetChild(0).gameObject.SetActive(false);
                        if (i > 7 && i != 16 && i != 8 && i != 17 && i != 70 && i != 61 && i != 52 && i != 43 && i != 34 && i != 25 && i != 71)
                            chessPointToSlot[i - 7].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                if (i > 0)
                {
                    if (chessPointToSlot[i - 1].transform.GetChildCount() > 1)
                    {
                        if (i > 10 && i != 82 && i != 81 && i != 64 && i != 55 && i != 46 && i != 37 && i != 28 && i != 19 && i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                            chessPointToSlot[i - 11].transform.GetChild(0).gameObject.SetActive(false);
                        if (i < 81 && i != 72 && i != 73 && i != 1 && i != 9 && i != 0 && i != 10 && i != 64 && i != 55 && i != 46 && i != 37 && i != 28 && i != 19 && i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                            chessPointToSlot[i + 7].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                if (i > 8)
                {
                    if (chessPointToSlot[i - 9].transform.GetChildCount() > 1)
                    {
                        if (i > 18 && i != 81 && i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                            chessPointToSlot[i - 19].transform.GetChild(0).gameObject.SetActive(false);
                        if (i > 16 && i != 89 && i != 80 && i != 62 && i != 53 && i != 44 && i != 35 && i != 26 && i != 71)
                            chessPointToSlot[i - 17].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                if (i < 81)
                {
                    if (chessPointToSlot[i + 9].transform.GetChildCount() > 1)
                    {
                        if (i < 71 && i != 8 && i != 17 && i != 62 && i != 53 && i != 44 && i != 35 && i != 26 && i != 71)
                            chessPointToSlot[i + 19].transform.GetChild(0).gameObject.SetActive(false);
                        if (i < 73 && i != 9 && i != 0 && i != 63 && i != 54 && i != 45 && i != 36 && i != 27 && i != 18)
                            chessPointToSlot[i + 17].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                */
                break;
            case "phaored":
            case "phaoblack":
                if (point >= 81)
                {
                    int checkDown;
                    while (i >= 0)
                    {
                        if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                        {
                            break;
                        }
                        else 
                        {
                            chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                            i -= 9;
                        }
                    }
                    for (checkDown = i; checkDown >= 0; checkDown -= 9)
                    {
                        if (chessPointToSlot[checkDown].transform.childCount > 1 && i != checkDown)
                        {
                            if (chessPointToSlot[checkDown].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[checkDown].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[checkDown].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = checkDown.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + checkDown.ToString();
                                }
                                break;
                            }
                        }
                    }
                }
                else if (point <= 8)
                {
                    int checkUp;
                    while (i <= 89)
                    {
                        if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                        {
                            break;
                        }
                        else 
                        {
                            chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                            i += 9;
                        }
                    }
                    for (checkUp = i; checkUp <= 89; checkUp += 9)
                    {
                        if (chessPointToSlot[checkUp].transform.childCount > 1 && i != checkUp)
                        {
                            if (chessPointToSlot[checkUp].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[checkUp].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[checkUp].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = checkUp.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + checkUp.ToString();
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    int checkDown;
                    int checkUp;
                    while (i >= 0)
                    {
                        if (chessPointToSlot[i].transform.childCount > 1 && i != point)
                        {
                            break;
                        }
                        else 
                        {
                            chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                            i -= 9;
                        }
                    }
                    for (checkDown = i; checkDown >= 0; checkDown -= 9)
                    {
                        if (chessPointToSlot[checkDown].transform.childCount > 1 && i != checkDown)
                        {
                            if (chessPointToSlot[checkDown].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[checkDown].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[checkDown].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = checkDown.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + checkDown.ToString();
                                }
                                break;
                            }
                        }
                    }
                    while (j <= 89)
                    {
                        if (chessPointToSlot[j].transform.childCount > 1 && j != point)
                        {
                            break;
                        }
                        else 
                        {
                            chessPointToSlot[j].transform.GetChild(0).gameObject.SetActive(true);
                            j += 9;
                        }
                    }
                    for (checkUp = j; checkUp <= 89; checkUp += 9)
                    {
                        if (chessPointToSlot[checkUp].transform.childCount > 1 && j != checkUp)
                        {
                            if (chessPointToSlot[checkUp].transform.GetChild(1).name.Contains(type))
                            {
                                break;
                            }
                            else
                            {
                                chessPointToSlot[checkUp].transform.GetChild(0).gameObject.SetActive(true);
                                chessPointToSlot[checkUp].transform.GetChild(0).SetAsLastSibling();
                                if (chessPointChange == null)
                                {
                                    chessPointChange = checkUp.ToString();
                                }
                                else
                                {
                                    chessPointChange = chessPointChange + "-" + checkUp.ToString();
                                }
                                break;
                            }
                        }
                    }
                }
                chessTableControllPieceCannonRow(point, type);
                break;
            case "xeblack":
            case "xered":
                if (point >= 81)
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
                            i -= 9;
                        }
                    }
                }
                else if (point <= 8)
                {
                    while (i <= 89)
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
                            i += 9;
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
                            i -= 9;
                        }
                    }
                    while (j <= 89)
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
                            j += 9;
                        }
                    }
                }
                chessTableControllPieceTankRow(point, type);
                break;
            case "totred":
                if (i < 81)
                {
                    chessMovePiece(i + 9, type);
                    if (i >= 45)
                    {
                        chessMovePiece(i + 1, type);
                        chessMovePiece(i - 1, type);
                        if (i == 45 || i == 54 || i == 63 || i == 72 || i == 81)
                        {
                            chessPointToSlot[i - 1].transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (i == 53 || i == 62 || i == 71 || i == 80 || i == 89)
                        {
                            chessPointToSlot[i + 1].transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    chessMovePiece(i + 1, type);
                    chessMovePiece(i - 1, type);
                    if (i == 45 || i == 54 || i == 63 || i == 72 || i == 81)
                    {
                        chessPointToSlot[i - 1].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    if (i == 53 || i == 62 || i == 71 || i == 80 || i == 89)
                    {
                        chessPointToSlot[i + 1].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                break;
            case "totblack":
                if (i > 8)
                {
                    chessMovePiece(i - 9, type);
                    if (i <= 44)
                    {
                        chessMovePiece(i + 1, type);
                        chessMovePiece(i - 1, type);
                        if (i == 44 || i == 35 || i == 26 || i == 17 || i == 8)
                        {
                            chessPointToSlot[i + 1].transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (i == 36 || i == 27 || i == 18 || i == 9 || i == 0)
                        {
                            chessPointToSlot[i - 1].transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    chessMovePiece(i + 1, type);
                    chessMovePiece(i - 1, type);
                    if (i == 44 || i == 35 || i == 26 || i == 17 || i == 8)
                    {
                        chessPointToSlot[i + 1].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    if (i == 36 || i == 27 || i == 18 || i == 9 || i == 0)
                    {
                        chessPointToSlot[i - 1].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                break;
            case "tuongblack":
            case "tuongred":
                if (CPlayer.gameName == "xiangqi" || CPlayer.reviewMatchGameId == "xiangqi")
                {
                    if (point == 2)
                    {
                        chessMovePiece(18, type);
                        chessMovePiece(22, type);
                        if (chessPointToSlot[10].transform.childCount > 1)
                            chessPointToSlot[18].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[12].transform.childCount > 1)
                            chessPointToSlot[22].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 18)
                    {
                        chessMovePiece(38, type);
                        chessMovePiece(2, type);
                        if (chessPointToSlot[28].transform.childCount > 1)
                            chessPointToSlot[38].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[10].transform.childCount > 1)
                            chessPointToSlot[2].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 38)
                    {
                        chessMovePiece(18, type);
                        chessMovePiece(22, type);
                        if (chessPointToSlot[59].transform.childCount > 1)
                            chessPointToSlot[51].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[57].transform.childCount > 1)
                            chessPointToSlot[47].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 22)
                    {
                        chessMovePiece(38, type);
                        chessMovePiece(2, type);
                        chessMovePiece(6, type);
                        chessMovePiece(42, type);
                        if (chessPointToSlot[30].transform.childCount > 1)
                            chessPointToSlot[38].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[12].transform.childCount > 1)
                            chessPointToSlot[2].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[14].transform.childCount > 1)
                            chessPointToSlot[6].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[32].transform.childCount > 1)
                            chessPointToSlot[42].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 6)
                    {
                        chessMovePiece(26, type);
                        chessMovePiece(22, type);
                        if (chessPointToSlot[16].transform.childCount > 1)
                            chessPointToSlot[26].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[14].transform.childCount > 1)
                            chessPointToSlot[22].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 26)
                    {
                        chessMovePiece(6, type);
                        chessMovePiece(42, type);
                        if (chessPointToSlot[16].transform.childCount > 1)
                            chessPointToSlot[6].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[34].transform.childCount > 1)
                            chessPointToSlot[42].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 42)
                    {
                        chessMovePiece(26, type);
                        chessMovePiece(22, type);
                        if (chessPointToSlot[32].transform.childCount > 1)
                            chessPointToSlot[22].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[34].transform.childCount > 1)
                            chessPointToSlot[26].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 83)
                    {
                        chessMovePiece(63, type);
                        chessMovePiece(67, type);
                        if (chessPointToSlot[73].transform.childCount > 1)
                            chessPointToSlot[63].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[75].transform.childCount > 1)
                            chessPointToSlot[67].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 63)
                    {
                        chessMovePiece(83, type);
                        chessMovePiece(47, type);
                        if (chessPointToSlot[73].transform.childCount > 1)
                            chessPointToSlot[83].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[55].transform.childCount > 1)
                            chessPointToSlot[47].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 47)
                    {
                        chessMovePiece(63, type);
                        chessMovePiece(67, type);
                        if (chessPointToSlot[57].transform.childCount > 1)
                            chessPointToSlot[67].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[55].transform.childCount > 1)
                            chessPointToSlot[63].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 67)
                    {
                        chessMovePiece(83, type);
                        chessMovePiece(47, type);
                        chessMovePiece(87, type);
                        chessMovePiece(51, type);
                        if (chessPointToSlot[59].transform.childCount > 1)
                            chessPointToSlot[51].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[57].transform.childCount > 1)
                            chessPointToSlot[47].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[75].transform.childCount > 1)
                            chessPointToSlot[83].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[77].transform.childCount > 1)
                            chessPointToSlot[87].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 51)
                    {
                        chessMovePiece(71, type);
                        chessMovePiece(67, type);
                        if (chessPointToSlot[61].transform.childCount > 1)
                            chessPointToSlot[71].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[59].transform.childCount > 1)
                            chessPointToSlot[67].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 71)
                    {
                        chessMovePiece(51, type);
                        chessMovePiece(87, type);
                        if (chessPointToSlot[79].transform.childCount > 1)
                            chessPointToSlot[87].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[61].transform.childCount > 1)
                            chessPointToSlot[51].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (point == 87)
                    {
                        chessMovePiece(71, type);
                        chessMovePiece(67, type);
                        if (chessPointToSlot[79].transform.childCount > 1)
                            chessPointToSlot[71].transform.GetChild(0).gameObject.SetActive(false);
                        if (chessPointToSlot[77].transform.childCount > 1)
                            chessPointToSlot[67].transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
                else if (CPlayer.gameName == "mystery_xiangqi" || CPlayer.reviewMatchGameId == "mystery_xiangqi")
                {
                    if (i < 18)
                    {
                        if (i != 7 && i != 8 && i != 16 && i != 17)
                        {
                            chessMovePiece(i + 20, type);
                        }
                        if (i != 0 && i != 1 && i != 9 && i != 10)
                        {
                            chessMovePiece(i + 16, type);
                        }
                    }
                    else if (i > 71)
                    {
                        if (i != 72 && i != 73 && i != 81 && i != 82)
                        {
                            chessMovePiece(i - 20, type);
                        }
                        if (i != 79 && i != 80 && i != 88 && i != 89)
                        {
                            chessMovePiece(i - 16, type);
                        }
                    }
                    else
                    {
                        if (i != 63 && i != 64 && i != 54 && i != 55 && i != 45 && i != 46 && i != 36 && i != 37 && i != 27 && i != 28 && i != 18 && i != 19)
                        {
                            chessMovePiece(i - 20, type);
                            chessMovePiece(i + 16, type);
                        }
                        if (i != 25 && i != 26 && i != 34 && i != 35 && i != 43 && i != 44 && i != 52 && i != 53 && i != 61 && i != 62 && i != 70 && i != 71)
                        {
                            chessMovePiece(i + 20, type);
                            chessMovePiece(i - 16, type);
                        }
                    }
                    if (i < 18)
                    {
                        if (chessPointToSlot[i + 10].transform.childCount > 1)
                        {
                            if (i != 7 && i != 8 && i != 16 && i != 17)
                                chessPointToSlot[i + 20].transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (chessPointToSlot[i + 8].transform.childCount > 1)
                        {
                            if (i != 0 && i != 1 && i != 9 && i != 10)
                                chessPointToSlot[i + 16].transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                    else if (i > 71)
                    {
                        if (chessPointToSlot[i - 10].transform.childCount > 1)
                        {
                            if (i != 72 && i != 73 && i != 81 && i != 82)
                                chessPointToSlot[i - 20].transform.GetChild(0).gameObject.SetActive(false);
                        }
                        if (chessPointToSlot[i - 8].transform.childCount > 1)
                        {
                            if (i != 79 && i != 80 && i != 88 && i != 89)
                                chessPointToSlot[i - 16].transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (chessPointToSlot[i - 10].transform.childCount > 1)
                        {
                            if (i != 63 && i != 64 && i != 54 && i != 55 && i != 45 && i != 46 && i != 36 && i != 37 && i != 27 && i != 28 && i != 18 && i != 19)
                            {
                                chessPointToSlot[i - 20].transform.GetChild(0).gameObject.SetActive(false);
                            }
                        }
                        if (chessPointToSlot[i + 8].transform.childCount > 1)
                        {
                            if (i != 63 && i != 64 && i != 54 && i != 55 && i != 45 && i != 46 && i != 36 && i != 37 && i != 27 && i != 28 && i != 18 && i != 19)
                            {
                                chessPointToSlot[i + 16].transform.GetChild(0).gameObject.SetActive(false);
                            }
                        }
                        if (chessPointToSlot[i - 8].transform.childCount > 1)
                        {
                            if (i != 25 && i != 26 && i != 34 && i != 35 && i != 43 && i != 44 && i != 52 && i != 53 && i != 61 && i != 62 && i != 70 && i != 71)
                            {
                                chessPointToSlot[i - 16].transform.GetChild(0).gameObject.SetActive(false);
                            }
                        }
                        if (chessPointToSlot[i + 10].transform.childCount > 1)
                        {
                            if (i != 25 && i != 26 && i != 34 && i != 35 && i != 43 && i != 44 && i != 52 && i != 53 && i != 61 && i != 62 && i != 70 && i != 71)
                            {
                                chessPointToSlot[i + 20].transform.GetChild(0).gameObject.SetActive(false);
                            }
                        }
                    }
                }
                break;
            case "siblack":
            case "sired":
                if (CPlayer.gameName == "xiangqi" || CPlayer.reviewMatchGameId == "xiangqi")
                {
                    if (point == 3 || point == 21 || point == 23 || point == 5)
                    {
                        chessMovePiece(13, type);
                    }
                    else if (point == 13)
                    {
                        chessMovePiece(3, type);
                        chessMovePiece(5, type);
                        chessMovePiece(21, type);
                        chessMovePiece(23, type);
                    }
                    if (point == 84 || point == 86 || point == 66 || point == 68)
                    {
                        chessMovePiece(76, type);
                    }
                    else if (point == 76)
                    {
                        chessMovePiece(84, type);
                        chessMovePiece(86, type);
                        chessMovePiece(66, type);
                        chessMovePiece(68, type);
                    }
                }
                else if (CPlayer.gameName == "mystery_xiangqi" || CPlayer.reviewMatchGameId == "mystery_xiangqi")
                {
                    if (justOpen)
                    {
                        if (i < 9)
                        {
                            if (i != 8)
                                chessMovePiece(i + 10, type);
                            if (i != 0)
                                chessMovePiece(i + 8, type);
                        }
                        else if (i > 80)
                        {
                            if (i != 81)
                                chessMovePiece(i - 10, type);
                            if (i != 89)
                                chessMovePiece(i - 8, type);
                        }
                        else
                        {
                            if (i != 17 && i != 26 && i != 35 && i != 44 && i != 53 && i != 62 && i != 71 && i != 80)
                            {
                                chessMovePiece(i + 10, type);
                                chessMovePiece(i - 8, type);
                            }
                            if (i != 9 && i != 18 && i != 27 && i != 36 && i != 45 && i != 54 && i != 63 && i != 72)
                            {
                                chessMovePiece(i - 10, type);
                                chessMovePiece(i + 8, type);
                            }
                        }
                    }
                    else
                    {
                        if (point == 3 || point == 21 || point == 23 || point == 5)
                        {
                            chessMovePiece(13, type);
                        }
                        else if (point == 13)
                        {
                            chessMovePiece(3, type);
                            chessMovePiece(5, type);
                            chessMovePiece(21, type);
                            chessMovePiece(23, type);
                        }
                        if (point == 84 || point == 86 || point == 66 || point == 68)
                        {
                            chessMovePiece(76, type);
                        }
                        else if (point == 76)
                        {
                            chessMovePiece(84, type);
                            chessMovePiece(86, type);
                            chessMovePiece(66, type);
                            chessMovePiece(68, type);
                        }
                    }
                }
                break;
            case "kingblack":
            case "kingred":
                if (point == 4)
                {
                    chessMovePiece(3, type);
                    chessMovePiece(5, type);
                    chessMovePiece(13, type);
                }
                else if (point == 3)
                {
                    chessMovePiece(4, type);
                    chessMovePiece(12, type);
                }
                else if (point == 5)
                {
                    chessMovePiece(4, type);
                    chessMovePiece(14, type);
                }
                else if (point == 13)
                {
                    chessMovePiece(4, type);
                    chessMovePiece(12, type);
                    chessMovePiece(14, type);
                    chessMovePiece(22, type);
                }
                else if (point == 12)
                {
                    chessMovePiece(3, type);
                    chessMovePiece(13, type);
                    chessMovePiece(21, type);
                }
                else if (point == 14)
                {
                    chessMovePiece(5, type);
                    chessMovePiece(13, type);
                    chessMovePiece(23, type);
                }
                else if (point == 21)
                {
                    chessMovePiece(12, type);
                    chessMovePiece(22, type);
                }
                else if (point == 22)
                {
                    chessMovePiece(13, type);
                    chessMovePiece(21, type);
                    chessMovePiece(23, type);
                }
                else if (point == 23)
                {
                    chessMovePiece(22, type);
                    chessMovePiece(14, type);
                }
                else if (point == 85)
                {
                    chessMovePiece(84, type);
                    chessMovePiece(86, type);
                    chessMovePiece(76, type);
                }
                else if (point == 84)
                {
                    chessMovePiece(85, type);
                    chessMovePiece(75, type);
                }
                else if (point == 86)
                {
                    chessMovePiece(85, type);
                    chessMovePiece(77, type);
                }
                else if (point == 75)
                {
                    chessMovePiece(66, type);
                    chessMovePiece(76, type);
                    chessMovePiece(84, type);
                }
                else if (point == 76)
                {
                    chessMovePiece(67, type);
                    chessMovePiece(77, type);
                    chessMovePiece(75, type);
                    chessMovePiece(85, type);
                }
                else if (point == 77)
                {
                    chessMovePiece(86, type);
                    chessMovePiece(76, type);
                    chessMovePiece(68, type);
                }
                else if (point == 66)
                {
                    chessMovePiece(75, type);
                    chessMovePiece(67, type);
                }
                else if (point == 67)
                {
                    chessMovePiece(66, type);
                    chessMovePiece(76, type);
                    chessMovePiece(68, type);
                }
                else if (point == 68)
                {
                    chessMovePiece(67, type);
                    chessMovePiece(77, type);
                }
                break;
        }
        if (point != chessTargetNextStep && chessLastClickPoint != point)
        {
            chessPointToSlot[point].transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
            chessLastClickPoint = point;
        }
        else if(chessLastClickPoint == point && !dragMode)
        {
            chessPointToSlot[point].transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
            chessLastClickPoint = -1;
            chessTableClearPointMove();
        }
    }

    public bool dragMode = false;
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
    public void chessTableControllPieceCannonMoveRow(int point, int min, int max, string type)
    {
        for (int i = point + 1; i <= max; i++)
        {
            if (chessPointToSlot[i].transform.childCount > 1)
            {
                checkRight = i;
                break;
            }
            else 
            {
                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                checkRight = max;
            }
        }
        for (int j = checkRight; j <= max; j++)
        {
            if (chessPointToSlot[j].transform.childCount > 1 && checkRight != j)
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
        }
        for (int i = point - 1; i >= min; i--)
        {
            if (chessPointToSlot[i].transform.childCount > 1)
            {
                checkLeft = i;
                break;
            }
            else 
            {
                chessPointToSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                checkLeft = min;
            }
        }
        for (int j = checkLeft; j >= min; j--)
        {
            if (chessPointToSlot[j].transform.childCount > 1 && checkLeft != j)
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
        }
    }
    public void chessTableControllPieceCannonRow(int point, string type)
    {
        App.trace("|||||||||||||||||||||| Point " + point);
        if (point >= 0 && point <= 8)
        {
            chessTableControllPieceCannonMoveRow(point, 0, 8, type);
        }
        else if (point >= 9 && point <= 17)
        {
            chessTableControllPieceCannonMoveRow(point, 9, 17, type);
        }
        else if (point >= 18 && point <= 26)
        {
            chessTableControllPieceCannonMoveRow(point, 18, 26, type);
        }
        else if (point >= 27 && point <= 35)
        {
            chessTableControllPieceCannonMoveRow(point, 27, 35, type);
        }
        else if (point >= 36 && point <= 44)
        {
            chessTableControllPieceCannonMoveRow(point, 36, 44, type);
        }
        else if (point >= 45 && point <= 53)
        {
            chessTableControllPieceCannonMoveRow(point, 45, 53, type);
        }
        else if (point >= 54 && point <= 62)
        {
            chessTableControllPieceCannonMoveRow(point, 54, 62, type);
        }
        else if (point >= 63 && point <= 71)
        {
            chessTableControllPieceCannonMoveRow(point, 63, 71, type);
        }
        else if (point >= 72 && point <= 80)
        {
            chessTableControllPieceCannonMoveRow(point, 72, 80, type);
        }
        else if (point >= 81 && point <= 89)
        {
            chessTableControllPieceCannonMoveRow(point, 81, 89, type);
        }
    }
    public void chessTableControllPieceTankRow(int point, string type)
    {
        if (point >= 0 && point <= 8)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 8, 0, type);
        }
        else if (point >= 9 && point <= 17)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 17, 9, type);
        }
        else if (point >= 18 && point <= 26)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 26, 18, type);
        }
        else if (point >= 27 && point <= 35)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 35, 27, type);
        }
        else if (point >= 36 && point <= 44)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 44, 36, type);
        }
        else if (point >= 45 && point <= 53)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 53, 45, type);
        }
        else if (point >= 54 && point <= 62)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 62, 54, type);
        }
        else if (point >= 63 && point <= 71)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 71, 63, type);
        }
        else if (point >= 72 && point <= 80)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 80, 72, type);
        }
        else if (point >= 81 && point <= 89)
        {
            chessTableControllMoveAndEatPieceTankRow(point, 89, 81, type);
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
            chessMovePerPieceClick(sourcePoint, dirPoint, targetPoint);
        }
        else
        {
            chessTableClearPointMove();
        }
    }
    // Move
    public string chessSourcePointDrag;
    public void chessMovePerPieceClick(int sourcePoint, int dirPoint, GameObject targetPoint)
    {
        if (CPlayer.typePlay == "play")
        {
            //App.trace("CHESS MOVE TO SOURE " + sourcePoint + " || " + "CHESS MOVE TO DIR " + dirPoint);     
            var req_move_per_piece = new OutBounMessage("PLAY");
            req_move_per_piece.addHead();
            req_move_per_piece.writeByte(sourcePoint); // diem bat dau
            req_move_per_piece.writeByte(dirPoint); // diem di chuyen den
            App.ws.send(req_move_per_piece.getReq(), delegate (InBoundMessage res)
            {
                //chessTableClearPointMove();
                /*
                 GameObject chessMove = chessPieceMove.transform.GetChild(1).gameObject;
                 chessMove.transform.SetParent(chessMove.transform.parent.parent);
                 RectTransform rtf = chessMove.GetComponent<RectTransform>();
                 RectTransform rtfTarget = targetPoint.GetComponent<RectTransform>();
                 DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfTarget.anchoredPosition.x, rtfTarget.anchoredPosition.y), .35f).OnComplete(() =>
                 {
                     chessMove.transform.SetParent(targetPoint.transform);
                     chessTableClearPointMove();                   
                 });
                 */
            });
        }

    }
    public void chessMovePerPieceDrag(int sourcePoint, int dirPoint)
    {
        if (CPlayer.typePlay == "play" && sourcePoint != dirPoint)
        {
            App.trace("CHESS MOVE TO SOURE " + sourcePoint + " || " + "CHESS MOVE TO DIR " + dirPoint);
            var req_move_per_piece_DRAG = new OutBounMessage("PLAY");
            req_move_per_piece_DRAG.addHead();
            req_move_per_piece_DRAG.writeByte(sourcePoint); // diem bat dau
            req_move_per_piece_DRAG.writeByte(dirPoint); // diem di chuyen den
            App.ws.send(req_move_per_piece_DRAG.getReq(), delegate (InBoundMessage res)
            {
                //StartCoroutine(chessSetDrag(sourcePoint, dirPoint));
            });
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
        App.trace("START_MATCH ");
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
    "OWNER_CHANGED", "START_MATCH", "GAMEOVER", "SET_PLAYER_ATTR", "MOVE","HIGHLIGHT"};
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
                        /*
                        App.trace("Button "+chessInviteClone.GetComponentInChildren<Button>().gameObject.name);
                        chessInviteClone.GetComponentsInChildren<Button>()[0].onClick.AddListener(() =>
                        {
                            chessInvitePlayer(nickName);
                        });
                        */
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
    private string [] AchmImage, AchmName, AchmDesc;
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
            if(message.Length > 1)
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
                if(gameId == "xiangqi")
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
                    infoClone.GetComponentsInChildren<Text>()[5].text = previousScore.ToString() +"/"+ nextScore.ToString();
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
            AchmTime = new long [achmSize];
            AchmTransId = new long[achmSize];
            for (int i = 0;i<achmSize;i++)
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
        string s =  date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
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
        CPlayer.chessPreScene = "TableChessTuongScene";
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
    private string[] logs;
    private int[] chessReviewSourcePos = new int[0];
    private int[] chessReviewDirPos = new int[0];
    private string[] chessReviewColor = new string[0];
    private int[,] chessReviewChangeFace;
    private string chessReviewBlackPlayer;
    private string chessReviewRedPlayer;
    private string chessReviewPlayerFirstTurn;
    private string chessReviewPlayerWinner;
    private int chessReviewReason;
    private int chessReviewStep = 0;
    private bool chessReviewPause = false;
    private int chessReviewLastFace;
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
            if (CPlayer.reviewMatchGameId == "xiangqi")
            {
                nameGame = "Cờ Tướng";
            }
            else if (CPlayer.reviewMatchGameId == "mystery_xiangqi")
            {
                nameGame = "Cờ Úp";
            }
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
            logs = new string[logCount];
            chessReviewChangeFace = new int[logCount * 2, 4];
            int numberArrCf = 0;
            //App.trace("Review logCount " + logCount);
            for (int i = 0; i < logCount; i++)
            {
                string log = res.readString();
                logs[i] = log;
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
                        if (chessReviewRowLog[2] == "r")
                        {
                            chessReviewRedPlayer = chessReviewRowLog[0];
                            infoMain += "\nCầm quân đỏ: " + chessReviewRowLog[0];
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
                        string[] positons = chessReviewRowLog[2].Split('-');
                        Array.Resize(ref chessReviewColor, chessReviewColor.Length + 1);
                        Array.Resize(ref chessReviewSourcePos, chessReviewSourcePos.Length + 1);
                        Array.Resize(ref chessReviewDirPos, chessReviewDirPos.Length + 1);
                        chessReviewSourcePos[chessReviewSourcePos.Length - 1] = Int32.Parse(positons[0]);
                        chessReviewDirPos[chessReviewDirPos.Length - 1] = Int32.Parse(positons[1]);
                        chessReviewColor[chessReviewColor.Length - 1] = chessReviewRowLog[0];
                    }
                    else if (type == "c")
                    {
                        string[] changeFace = chessReviewRowLog[2].Split('-');
                        string pieceId = changeFace[0];
                        string faceId = changeFace[1];
                        char faceID = faceId[1];
                        char pieceID = pieceId[1];
                        char color = faceId[0];
                        chessReviewChangeFace[numberArrCf, 0] = chessReviewSourcePos.Length;
                        if (color == 'r')
                        {
                            chessReviewChangeFace[numberArrCf, 1] = (int)Char.GetNumericValue(pieceID) + 10;
                            chessReviewChangeFace[numberArrCf, 2] = (int)Char.GetNumericValue(faceID) + 10;
                            chessReviewChangeFace[numberArrCf, 3] = 0;
                        }
                        else
                        {
                            chessReviewChangeFace[numberArrCf, 1] = (int)Char.GetNumericValue(pieceID);
                            chessReviewChangeFace[numberArrCf, 2] = (int)Char.GetNumericValue(faceID);
                            chessReviewChangeFace[numberArrCf, 3] = 1;
                        }
                        //App.trace("Step " + chessReviewSourcePos.Length.ToString());
                        //App.trace("Face " + chessReviewChangeFace[numberArrCf, 2]);
                        //App.trace("Piece " + chessReviewChangeFace[numberArrCf, 1]);
                        numberArrCf++;
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
                    chessClone.transform.SetParent(chessPointToSlot[87].transform);
                    chessClone.transform.position = chessPointToSlot[87].transform.position;
                    chessReviewLoadFace(chessClone, -26, "tuongblack", 5, 6);
                    break;
                case 1:
                    chessClone.transform.SetParent(chessPointToSlot[83].transform);
                    chessClone.transform.position = chessPointToSlot[83].transform.position;
                    chessReviewLoadFace(chessClone, -25, "tuongblack", 5, 6);
                    break;
                case 2:
                    chessClone.transform.SetParent(chessPointToSlot[54].transform);
                    chessClone.transform.position = chessPointToSlot[54].transform.position;
                    chessReviewLoadFace(chessClone, -61, "totblack", 4, 6);
                    break;
                case 3:
                    chessClone.transform.SetParent(chessPointToSlot[33].transform);
                    chessClone.transform.position = chessPointToSlot[33].transform.position;
                    chessReviewLoadFace(chessClone, 58, "totred", 12, 14);
                    break;
                case 4:
                    chessClone.transform.SetParent(chessPointToSlot[35].transform);
                    chessClone.transform.position = chessPointToSlot[35].transform.position;
                    chessReviewLoadFace(chessClone, 57, "totred", 12, 14);
                    break;
                case 5:
                    chessClone.transform.SetParent(chessPointToSlot[25].transform);
                    chessClone.transform.position = chessPointToSlot[25].transform.position;
                    chessReviewLoadFace(chessClone, 42, "phaored", 10, 14);
                    break;
                case 6:
                    chessClone.transform.SetParent(chessPointToSlot[29].transform);
                    chessClone.transform.position = chessPointToSlot[29].transform.position;
                    chessReviewLoadFace(chessClone, 60, "totred", 12, 14);
                    break;
                case 7:
                    chessClone.transform.SetParent(chessPointToSlot[85].transform);
                    chessClone.transform.position = chessPointToSlot[85].transform.position;
                    chessClone.GetComponent<Image>().sprite = chessTuongFace[0];
                    chessClone.name = -8 + "." + "kingblack" + "." + -8 + "open";
                    break;
                case 8:
                    chessClone.transform.SetParent(chessPointToSlot[19].transform);
                    chessClone.transform.position = chessPointToSlot[19].transform.position;
                    chessReviewLoadFace(chessClone, 41, "phaored", 10, 14);
                    break;
                case 9:
                    chessClone.transform.SetParent(chessPointToSlot[31].transform);
                    chessClone.transform.position = chessPointToSlot[31].transform.position;
                    chessReviewLoadFace(chessClone, 59, "totred", 12, 14);
                    break;
                case 10:
                    chessClone.transform.SetParent(chessPointToSlot[6].transform);
                    chessClone.transform.position = chessPointToSlot[6].transform.position;
                    chessReviewLoadFace(chessClone, 26, "tuongred", 13, 14);
                    break;
                case 11:
                    chessClone.transform.SetParent(chessPointToSlot[2].transform);
                    chessClone.transform.position = chessPointToSlot[2].transform.position;
                    chessReviewLoadFace(chessClone, 25, "tuongred", 13, 14);
                    break;
                case 12:
                    chessClone.transform.SetParent(chessPointToSlot[27].transform);
                    chessClone.transform.position = chessPointToSlot[27].transform.position;
                    chessReviewLoadFace(chessClone, 61, "totred", 12, 14);
                    break;
                case 13:
                    chessClone.transform.SetParent(chessPointToSlot[82].transform);
                    chessClone.transform.position = chessPointToSlot[82].transform.position;
                    chessReviewLoadFace(chessClone, -49, "mablack", 1, 6);
                    break;
                case 14:
                    chessClone.transform.SetParent(chessPointToSlot[81].transform);
                    chessClone.transform.position = chessPointToSlot[81].transform.position;
                    chessReviewLoadFace(chessClone, -33, "xeblack", 7, 6);
                    break;
                case 15:
                    chessClone.transform.SetParent(chessPointToSlot[88].transform);
                    chessClone.transform.position = chessPointToSlot[88].transform.position;
                    chessReviewLoadFace(chessClone, -50, "mablack", 1, 6);
                    break;
                case 16:
                    chessClone.transform.SetParent(chessPointToSlot[84].transform);
                    chessClone.transform.position = chessPointToSlot[84].transform.position;
                    chessReviewLoadFace(chessClone, -17, "siblack", 3, 6);
                    break;
                case 17:
                    chessClone.transform.SetParent(chessPointToSlot[89].transform);
                    chessClone.transform.position = chessPointToSlot[89].transform.position;
                    chessReviewLoadFace(chessClone, -34, "xeblack", 7, 6);
                    break;
                case 18:
                    chessClone.transform.SetParent(chessPointToSlot[86].transform);
                    chessClone.transform.position = chessPointToSlot[86].transform.position;
                    chessReviewLoadFace(chessClone, -18, "siblack", 3, 6);
                    break;
                case 19:
                    chessClone.transform.SetParent(chessPointToSlot[1].transform);
                    chessClone.transform.position = chessPointToSlot[1].transform.position;
                    chessReviewLoadFace(chessClone, 49, "mared", 9, 14);
                    break;
                case 20:
                    chessClone.transform.SetParent(chessPointToSlot[0].transform);
                    chessClone.transform.position = chessPointToSlot[0].transform.position;
                    chessReviewLoadFace(chessClone, 33, "xered", 15, 14);
                    break;
                case 21:
                    chessClone.transform.SetParent(chessPointToSlot[7].transform);
                    chessClone.transform.position = chessPointToSlot[7].transform.position;
                    chessReviewLoadFace(chessClone, 50, "mared", 9, 14);
                    break;
                case 22:
                    chessClone.transform.SetParent(chessPointToSlot[3].transform);
                    chessClone.transform.position = chessPointToSlot[3].transform.position;
                    chessReviewLoadFace(chessClone, 17, "sired", 11, 14);
                    break;
                case 23:
                    chessClone.transform.SetParent(chessPointToSlot[8].transform);
                    chessClone.transform.position = chessPointToSlot[8].transform.position;
                    chessReviewLoadFace(chessClone, 34, "xered", 15, 14);
                    break;
                case 24:
                    chessClone.transform.SetParent(chessPointToSlot[5].transform);
                    chessClone.transform.position = chessPointToSlot[5].transform.position;
                    chessReviewLoadFace(chessClone, 18, "sired", 11, 14);
                    break;
                case 25:
                    chessClone.transform.SetParent(chessPointToSlot[60].transform);
                    chessClone.transform.position = chessPointToSlot[60].transform.position;
                    chessReviewLoadFace(chessClone, -58, "totblack", 4, 6);
                    break;
                case 26:
                    chessClone.transform.SetParent(chessPointToSlot[62].transform);
                    chessClone.transform.position = chessPointToSlot[62].transform.position;
                    chessReviewLoadFace(chessClone, -57, "totblack", 4, 6);
                    break;
                case 27:
                    chessClone.transform.SetParent(chessPointToSlot[70].transform);
                    chessClone.transform.position = chessPointToSlot[70].transform.position;
                    chessReviewLoadFace(chessClone, -42, "phaoblack", 2, 6);
                    break;
                case 28:
                    chessClone.transform.SetParent(chessPointToSlot[56].transform);
                    chessClone.transform.position = chessPointToSlot[56].transform.position;
                    chessReviewLoadFace(chessClone, -60, "totblack", 4, 6);
                    break;
                case 29:
                    chessClone.transform.SetParent(chessPointToSlot[64].transform);
                    chessClone.transform.position = chessPointToSlot[64].transform.position;
                    chessReviewLoadFace(chessClone, -41, "phaoblack", 2, 6);
                    break;
                case 30:
                    chessClone.transform.SetParent(chessPointToSlot[58].transform);
                    chessClone.transform.position = chessPointToSlot[58].transform.position;
                    chessReviewLoadFace(chessClone, -59, "totblack", 4, 6);
                    break;
                case 31:
                    chessClone.transform.SetParent(chessPointToSlot[4].transform);
                    chessClone.transform.position = chessPointToSlot[4].transform.position;
                    chessClone.GetComponent<Image>().sprite = chessTuongFace[8];
                    chessClone.name = 8 + "." + "kingred" + "." + 8 + "open";
                    break;
            }
            chessClone.SetActive(true);
        }
    }
    public void chessReviewLoadFace(GameObject chessClone, int sid, string name, int face, int faceClose)
    {
        if (CPlayer.reviewMatchGameId == "xiangqi")
        {
            chessClone.GetComponent<Image>().sprite = chessTuongFace[face];
            chessClone.name = sid + "." + name + "." + sid + "open";
        }
        else
        {
            chessClone.GetComponent<Image>().sprite = chessTuongFace[faceClose];
            chessClone.name = sid + "." + name + "." + sid;
        }
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
    public void chessReviewReLastFace(string id, int face, GameObject point)
    {
        switch (face)
        {
            case 1:
                point.name = id + "." + "kingblack" + "." + id;
                break;
            case 2:
                point.name = id + "." + "siblack" + "." + id;
                break;
            case 3:
                point.name = id + "." + "tuongblack" + "." + id;
                break;
            case 4:
                point.name = id + "." + "xeblack" + "." + id;
                break;
            case 5:
                point.name = id + "." + "phaoblack" + "." + id;
                break;
            case 6:
                point.name = id + "." + "mablack" + "." + id;
                break;
            case 7:
                point.name = id + "." + "totblack" + "." + id;
                break;
            case 11:
                point.name = id + "." + "kingred" + "." + id;
                break;
            case 12:
                point.name = id + "." + "sigred" + "." + id;
                break;
            case 13:
                point.name = id + "." + "tuongred" + "." + id;
                break;
            case 14:
                point.name = id + "." + "xered" + "." + id;
                break;
            case 15:
                point.name = id + "." + "phaored" + "." + id;
                break;
            case 16:
                point.name = id + "." + "mared" + "." + id;
                break;
            case 17:
                point.name = id + "." + "totred" + "." + id;
                break;
        }
    }
    public void chessReviewCheckOpen(int numberChange, GameObject chessMove, GameObject chessEat, int color, int chessReviewStep)
    {
        if (numberChange >= 2)
        {
            int check = 0;
            for (int i = 0; i < chessReviewChangeFace.GetLength(0); i++)
            {
                if (chessReviewChangeFace[i, 0] == chessReviewStep + 1)
                {
                    string[] chessName = chessMove.name.Split('.');
                    int id = Int32.Parse(chessName[0]);
                    chessReviewConvertLastFace(chessName[1]);
                    chessMove.name += ".openinstep" + chessReviewStep;
                    chessMove.name += ".lastface" + chessReviewLastFace;
                    chessReviewConvertFace(chessReviewChangeFace[i, 2], id, chessMove);
                    check = i;
                    break;
                }
            }
            for (int i = check + 1; i < chessReviewChangeFace.GetLength(0); i++)
            {
                if (chessReviewChangeFace[i, 0] == chessReviewStep + 1)
                {
                    string[] chessName = chessEat.name.Split('.');
                    int id = Int32.Parse(chessName[0]);
                    chessReviewConvertLastFace(chessName[1]);
                    chessEat.name += ".openinstep" + chessReviewStep;
                    chessEat.name += ".lastface" + chessReviewLastFace;
                    chessEat.name += ".step" + chessReviewStep;
                    chessReviewConvertFace(chessReviewChangeFace[i, 2], id, chessEat);
                    check = i;
                    break;
                }
            }
        }
        else if (numberChange == 1)
        {
            for (int i = 0; i < chessReviewChangeFace.GetLength(0); i++)
            {
                if (chessReviewChangeFace[i, 0] == chessReviewStep + 1)
                {
                    if (chessReviewChangeFace[i, 3] == color)
                    {
                        string[] chessName = chessEat.name.Split('.');
                        int id = Int32.Parse(chessName[0]);
                        chessReviewConvertLastFace(chessName[1]);
                        chessEat.name += ".openinstep" + chessReviewStep;
                        chessEat.name += ".lastface" + chessReviewLastFace;
                        chessEat.name += ".step" + chessReviewStep;
                        chessReviewConvertFace(chessReviewChangeFace[i, 2], id, chessEat);
                        break;
                    }
                    else
                    {
                        string[] chessName = chessMove.name.Split('.');
                        int id = Int32.Parse(chessName[0]);
                        chessReviewConvertLastFace(chessName[1]);
                        chessMove.name += ".openinstep" + chessReviewStep;
                        chessMove.name += ".lastface" + chessReviewLastFace;
                        chessEat.name += ".step" + chessReviewStep;
                        chessReviewConvertFace(chessReviewChangeFace[i, 2], id, chessMove);
                        break;
                    }
                }
            }
        }
        else
        {
            chessEat.name += ".step" + chessReviewStep;
        }
    }
    public void chessReviewConvertLastFace(string name)
    {
        switch (name)
        {
            case "kingblack":
                chessReviewLastFace = 1;
                break;
            case "siblack":
                chessReviewLastFace = 2;
                break;
            case "tuongblack":
                chessReviewLastFace = 3;
                break;
            case "xeblack":
                chessReviewLastFace = 4;
                break;
            case "phaoblack":
                chessReviewLastFace = 5;
                break;
            case "mablack":
                chessReviewLastFace = 6;
                break;
            case "totblack":
                chessReviewLastFace = 7;
                break;
            case "kingred":
                chessReviewLastFace = 11;
                break;
            case "sired":
                chessReviewLastFace = 12;
                break;
            case "tuongred":
                chessReviewLastFace = 13;
                break;
            case "xered":
                chessReviewLastFace = 14;
                break;
            case "phaored":
                chessReviewLastFace = 15;
                break;
            case "mared":
                chessReviewLastFace = 16;
                break;
            case "totred":
                chessReviewLastFace = 17;
                break;

        }
    }
    public void chessReviewConvertFace(int face, int id, GameObject point)
    {
        switch (face)
        {
            case 1:
                chessShowFaceOpen(-8, id, point);
                break;
            case 2:
                chessShowFaceOpen(-17, id, point);
                break;
            case 3:
                chessShowFaceOpen(-25, id, point);
                break;
            case 4:
                chessShowFaceOpen(-33, id, point);
                break;
            case 5:
                chessShowFaceOpen(-41, id, point);
                break;
            case 6:
                chessShowFaceOpen(-49, id, point);
                break;
            case 7:
                chessShowFaceOpen(-57, id, point);
                break;
            case 11:
                chessShowFaceOpen(8, id, point);
                break;
            case 12:
                chessShowFaceOpen(17, id, point);
                break;
            case 13:
                chessShowFaceOpen(25, id, point);
                break;
            case 14:
                chessShowFaceOpen(33, id, point);
                break;
            case 15:
                chessShowFaceOpen(41, id, point);
                break;
            case 16:
                chessShowFaceOpen(49, id, point);
                break;
            case 17:
                chessShowFaceOpen(57, id, point);
                break;
        }

    }
    public void chessReviewMediaNext(float time)
    {
        chessTableClearPointMove();
        int numberChange = 0;
        for (int j = 0; j < chessReviewChangeFace.GetLength(0); j++)
        {
            if (chessReviewChangeFace[j, 0] == chessReviewStep + 1)
            {
                numberChange++;
            }
        }
        GameObject chessMove = chessPointToSlot[chessReviewSourcePos[chessReviewStep]].transform.GetChild(1).gameObject;
        chessMove.transform.SetParent(chessMove.transform.parent.parent);
        RectTransform rtf = chessMove.GetComponent<RectTransform>();
        RectTransform rtfTarget = chessPointToSlot[chessReviewDirPos[chessReviewStep]].GetComponent<RectTransform>();
        DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfTarget.anchoredPosition.x, rtfTarget.anchoredPosition.y), time).OnComplete(() =>
        {
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
                        chessReviewCheckOpen(numberChange, chessMove, chessEat, 0, chessReviewStep);
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
                        chessReviewCheckOpen(numberChange, chessMove, chessEat, 1, chessReviewStep);
                        chessReviewStep += 1;
                    });
                }
            }
            else
            {
                if (numberChange == 1)
                {
                    for (int k = 0; k < chessReviewChangeFace.GetLength(0); k++)
                    {
                        if (chessReviewChangeFace[k, 0] == chessReviewStep + 1)
                        {
                            string[] chessName = chessMove.name.Split('.');
                            int id = Int32.Parse(chessName[0]);
                            chessReviewConvertLastFace(chessName[1]);
                            chessMove.name += ".openinstep" + chessReviewStep;
                            chessMove.name += ".lastface" + chessReviewLastFace;
                            chessReviewConvertFace(chessReviewChangeFace[k, 2], id, chessMove);
                            chessMove.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform);
                            chessReviewStep += 1;
                            break;
                        }
                    }
                }
                else
                {
                    chessMove.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep]].transform);
                    chessReviewStep += 1;
                }
            }
        });
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
            if (chessReviewColor[chessReviewStep - 1] == "r")
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
                        rtfEat.sizeDelta = new Vector2(80, 80);
                        DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), time).OnComplete(() =>
                        {
                            if (chessStart.name.Contains(".openinstep" + (chessReviewStep - 1)))
                            {
                                string[] nameChess = chessStart.name.Split('.');
                                string getnumber = Regex.Match(nameChess[4], @"\d+").Value;
                                int preFace = Int32.Parse(getnumber);
                                chessReviewReLastFace(nameChess[0], preFace, chessStart);
                                StartCoroutine(chessRotationChangeFace(0.5f, 6, chessStart, Vector3.zero, new Vector3(90, 0, 0)));
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
                        rtfEat.sizeDelta = new Vector2(80, 80);
                        DOTween.To(() => rtfEat.anchoredPosition, x => rtfEat.anchoredPosition = x, new Vector2(rtfPos.anchoredPosition.x, rtfPos.anchoredPosition.y), time).OnComplete(() =>
                        {
                            if (chessStart.name.Contains(".openinstep" + (chessReviewStep - 1)))
                            {
                                string[] nameChess = chessStart.name.Split('.');
                                string getnumber = Regex.Match(nameChess[4], @"\d+").Value;
                                int preFace = Int32.Parse(getnumber);
                                chessReviewReLastFace(nameChess[0], preFace, chessStart);
                                StartCoroutine(chessRotationChangeFace(0.5f, 14, chessStart, Vector3.zero, new Vector3(90, 0, 0)));
                            }
                            chessStart.transform.SetParent(chessPointToSlot[chessReviewDirPos[chessReviewStep - 1]].transform);
                        });
                    }
                }
            }
            DOTween.To(() => rtf.anchoredPosition, x => rtf.anchoredPosition = x, new Vector2(rtfTarget.anchoredPosition.x, rtfTarget.anchoredPosition.y), time).OnComplete(() =>
            {
                if (chessMove.name.Contains(".openinstep" + (chessReviewStep - 1)) && chessMove.name.Contains("black"))
                {
                    string[] nameChess = chessMove.name.Split('.');
                    string getnumber = Regex.Match(nameChess[4], @"\d+").Value;
                    int preFace = Int32.Parse(getnumber);
                    chessReviewReLastFace(nameChess[0], preFace, chessMove);
                    StartCoroutine(chessRotationChangeFace(0.5f, 6, chessMove, Vector3.zero, new Vector3(90, 0, 0)));
                }
                else if (chessMove.name.Contains(".openinstep" + (chessReviewStep - 1)) && chessMove.name.Contains("red"))
                {
                    string[] nameChess = chessMove.name.Split('.');
                    string getnumber = Regex.Match(nameChess[4], @"\d+").Value;
                    int preFace = Int32.Parse(getnumber);
                    chessReviewReLastFace(nameChess[0], preFace, chessMove);
                    StartCoroutine(chessRotationChangeFace(0.5f, 14, chessMove, Vector3.zero, new Vector3(90, 0, 0)));
                }
                chessMove.transform.SetParent(chessPointToSlot[chessReviewSourcePos[chessReviewStep - 1]].transform);
                chessReviewStep -= 1;
            });

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
        LoadingControl.instance.loadingGojList[17].SetActive(true);
    }
}
