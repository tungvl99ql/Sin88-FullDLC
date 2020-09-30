using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Server.Api
{

    public class CPlayer
    {
        #region Difference
        public static string gameName = "";
        public static string gameNameFull = "";
        public static string preScene = "";
        public static string currMiniGame = "";
        public static Sprite avatarSprite = null;
        public static string phoneNumber = "";
        public static string notEnouChipMess = "",  statusKick = "";
        public static int clientCurrentMode = 0, unread = 0, tableMaxBet = 0;
        public static string tableToGoId = ""; //VÀO BÀN CHƠI CÓ ID LÀ tableToGoId
        public static string betAmtOfTableToGo = "";
        public static int roomId = 0;   //KÊNH HIỆN TẠI
        public static int clientTargetMode = 0;
        public static bool erroShowing = false;
        public static int betAmtId = 0; //id muc cuoc hien tai 0 -11
        public static bool needRetryToLoadPhone = false; //co goi den server de reload lai khi nguoi choi xac thuc hay ko
        public static string baiDep = ""; //ten cua mes bai dep cho tung game
        public static string friendNicName = "";
        public static string[] defaultStatus = {"Nó đang đến, cái lạnh của mùa đông...", "Vừa đổi thưởng 50k rồi ae...", "Cao thủ Mậu Binh đây, solo đê",
                            "Solo TLMN 10K pm nhé", "Đang rảnh quẩy Xóc đĩa đi ae ơi", "Mình là girl, nhường mình Phỏm nha :v",
                            "Vừa chơi được iQuay được 50k nè. Ahhii", "Đập trúng cái hũ 20k. Hên v~ lúa", "Thắng lớn slot rồi ae ơi =))", "Kết bạn với mình nha!!!"};



        public static bool baidepActive;
        #endregion

        public static string cdn = "";                              //Server link
        public static string currMess = "";                         //Current Inbound Message Id
        public static string currPath = "";                         //Current path of user
        public static string currPass = "";                         //Current table pass
        public static string nickName = "";
        public static string fullName = "";
        public static string avatar = "";                           //avatar link
        public static string loginType = "";                        //Login type: fb, nick
        public static string phoneNum = "";                         //
        public static string titleEvent = "";                       //title Events
        public static string contentEvent = "";                     //content Events
        public static Sprite avatarSpriteToSave = null;             //User avatar

        public static bool fakeAva = false;                         //Fake avatar if not FB account
        public static bool errorShowing = false;                    //There is an error dialog showing
        public static bool lobbyBtnBackIsPressed = false;           //Is Back Btn pressed when the socket closed
        public static bool logedIn = false;                         //true if loged in
        public static bool hidePayment = false;
        public static long id = 0;                                  //Id of user
        public static long manBalance = 0;                          //man amount of user
        public static long chipBalance = 0;                         //chip amount of user
        public static long preChipBalance = 0;                      //last save chip amunt of user
        public static bool showGifCode = false;                     //show giftcode
        public static bool showEvent = false;                       //show event
        public static bool hadShowEvent = false;                    //showed event in lobby
        public static bool openButtonEvent = false;                 //open button join Event
        public static InBoundMessage res_pot = null;                       //change when recv pot change
        public static InBoundMessage res_pot1 = null;
        public static InBoundMessage res_potMiniGameTx = null;
        public static InBoundMessage res_potMiniGameVQMM = null;
        public static InBoundMessage res_potMiniGameCaoThap = null;
        public static InBoundMessage res_potMiniGameDapTrung = null;
        public static InBoundMessage res_potMiniGameMiniPocker = null;
        public static InBoundMessage res_potMiniGameSlotZombie = null;
        public static InBoundMessage res_potMiniGame3X3 = null;
        public static InBoundMessage res_potMiniGameOneLineSlot = null;

        public static string path = "";
        public static string passwordTableChess = "";
        public static int currentTurnTimeout = 0;
        public static int currentTurnDurationvalue = 0;
        public static int currentChanel = 0;
        public static string typePlay = "";
        public static string chessPreScene = "";
        public static GameObject currSceneChess = null;
        public static string reviewMatchGameId = "";
        public static long reviewMatchId;
        public static long playerReviewId;
        #region New Update
        public static int typeUser;
        #endregion

        public static void Clear()
        {
            path = "";
            passwordTableChess = "";
            currentTurnTimeout = 0;
            currentTurnDurationvalue = 0;
            currentChanel = 0;
            typePlay = "";
            chessPreScene = "";
            currSceneChess = null;
            reviewMatchGameId = "";
            reviewMatchId = -1;
            playerReviewId = -1;


            //Reset string value
            cdn = currMess = currPath = currPass = avatar = nickName = fullName = loginType
                = phoneNum = titleEvent = contentEvent = "";

            avatarSpriteToSave = null;

            //Reset bool value
            fakeAva = errorShowing = lobbyBtnBackIsPressed = logedIn = hidePayment = showGifCode = showEvent = hadShowEvent = openButtonEvent = false;

            //Reset long value
            id = chipBalance = manBalance = preChipBalance = 0;

            //Reset Handler
            changed = null;

            res_pot = null;
            res_pot1 = null;
            res_potMiniGameTx = null;
            res_potMiniGameVQMM = null;
            res_potMiniGameCaoThap = null;
            res_potMiniGameDapTrung = null;
            res_potMiniGameMiniPocker = null;
            res_potMiniGameSlotZombie = null;
        }

        public delegate void balanceChanged(string type);
        public static event balanceChanged changed;

        public static void change(string type, long balance)
        {
            if (changed != null)
            {
                if (type == "chip")
                {
                    preChipBalance = chipBalance;
                    chipBalance = balance;
                }

                if (type == "man")
                    manBalance = balance;
                changed(type);
            }
        }

        public delegate void PotChanged();
        public static event PotChanged potchanged;

        public delegate void PotChanged1();
        public static event PotChanged potchanged1;
        public static void ChangePot(InBoundMessage res)
        {
            if (potchanged != null)
            {
                res_pot = res;
                potchanged();
            }
        }
        public static void ChangePot1(InBoundMessage res)
        {
            if (potchanged1 != null)
            {
                res_pot1 = res;
                potchanged1();
            }
        }
        public delegate void PotChangedTx();
        public static event PotChangedTx potchangedTx;
        public static void ChangePotTx(InBoundMessage res)
        {
            if (potchangedTx != null)
            {
                res_potMiniGameTx = res;
                potchangedTx();
            }
        }

        public delegate void PotChangedCaoThap();
        public static event PotChangedCaoThap potchangedCaoThap;
        public static void ChangePotCaoThap(InBoundMessage res)
        {
            if (potchangedCaoThap != null)
            {
                res_potMiniGameCaoThap = res;
                potchangedCaoThap();
            }
        }

        public delegate void PotChangedVQMM();
        public static event PotChangedVQMM potchangedVQMM;
        public static void ChangePotVQMM(InBoundMessage res)
        {
            if (potchangedVQMM != null)
            {
                res_potMiniGameVQMM = res;
                potchangedVQMM();
            }
        }

        public delegate void PotChangedDapTrung();
        public static event PotChangedDapTrung potchangedDapTrung;
        public static void ChangePotDapTrung(InBoundMessage res)
        {
            if (potchangedDapTrung != null)
            {
                res_potMiniGameDapTrung = res;
                potchangedDapTrung();
            }
        }

        public delegate void PotChangedMiniPocker();
        public static event PotChangedMiniPocker potchangedMiniPocker;
        public static void ChangePotMiniPocker(InBoundMessage res)
        {
            if (potchangedMiniPocker != null)
            {
                res_potMiniGameMiniPocker = res;
                potchangedMiniPocker();
            }
        }

        public delegate void PotChangedZombieSlot();
        public static event PotChangedZombieSlot potchangedZombieSlot;
        public static void ChangePotZombieSlot(InBoundMessage res)
        {
            if (potchangedZombieSlot != null)
            {
                res_potMiniGameSlotZombie = res;
                potchangedZombieSlot();
            }
        }

        public delegate void PotChanged3X3MiniGame();
        public static event PotChanged3X3MiniGame potChanged3X3MiniGame;
        public static void ChangePot3X3Slot(InBoundMessage res)
        {
            if (potChanged3X3MiniGame != null)
            {
                res_potMiniGame3X3 = res;
                potChanged3X3MiniGame();
            }
        }


        public delegate void PotChangedOneLineSlotMiniGame();
        public static event PotChangedOneLineSlotMiniGame potChangedOneLineSlotMiniGame;
        public static void ChangePotOneLineSlot(InBoundMessage res)
        {
            if (potChangedOneLineSlotMiniGame != null)
            {
                res_potMiniGameOneLineSlot = res;
                potChangedOneLineSlotMiniGame();
            }
        }


        public delegate void ForceStopGameEvent(string gameCode, int gameState);
        public static event ForceStopGameEvent forceStopGameEvent;

        public static void StopGame(string gameCode, int gameState)
        {
            if (forceStopGameEvent != null)
            {
                forceStopGameEvent.Invoke(gameCode, gameState);
            }
        }


    }
}