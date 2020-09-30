using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using System;
using UnityEngine.SceneManagement;
using Core.Server.Api;

namespace Core.Bird
{
    public class BirdManager : MiniGame
    {
        [SerializeField] ChangerBird birdChange;
        [SerializeField] BirdGamePlay birdGamePlay;
        [SerializeField] History birdHistory;
        [SerializeField] BlackRedMiniGame birdHitEgg;
        [SerializeField] SelectNumLine birdSelectNumLine;
        [SerializeField] SelectBet birdSelectBet;
        [SerializeField] GameObject[] goList;
        [SerializeField] BirdGlory gloryPanel;
        [SerializeField] Guide guide;

        public void SetActivePanel(BirdPanel panel, bool isActive)
        {
            switch (panel)
            {
                case BirdPanel.GameContent:
                    goList[0].SetActive(isActive);
                    break;
                case BirdPanel.GameHistory:
                    goList[1].SetActive(isActive);
                    break;
                case BirdPanel.GameGuide:
                    goList[2].SetActive(isActive);
                    break;
                case BirdPanel.HitEgg:
                    goList[3].SetActive(isActive);
                    break;
                case BirdPanel.SelectBet:
                    goList[4].SetActive(isActive);
                    break;
                case BirdPanel.SelectNumLine:
                    goList[5].SetActive(isActive);
                    break;
                default:
                    
                    break;
            }
        }

        public override void OnEnable()
        {
            birdSelectBet.OpenSelectBetEvent.AddListener(OnOpenSelectBetEvent);
            //birdChange.StartRollEvent.AddListener(OnStartPlay);
            //birdChange.SendRequestEvent.AddListener(OnSendRequestEvent);
            //birdChange.EndRollEvent.AddListener(OnEndRollEvent);

            //base.OnEnable();
        }
        public override void OnDisable()
        {
            birdSelectBet.OpenSelectBetEvent.RemoveListener(OnOpenSelectBetEvent);
            //birdChange.StartRollEvent.RemoveListener(OnStartPlay);
            //birdChange.SendRequestEvent.RemoveListener(OnSendRequestEvent);
            //birdChange.EndRollEvent.RemoveListener(OnEndRollEvent);
        }

        //private void OnStartPlay()
        //{
        //    Debug.Log("---Start---");
        //}
        //private void OnSendRequestEvent()
        //{
        //    Debug.Log("---Push---");
        //    //FinishGameTurn();
        //}
        //private void OnEndRollEvent()
        //{
        //    Debug.Log("---End---");
        //}
        //Override


        public override string GameCode { get { return GameCodeApp.gameCode2; } }
        public override string JoinGameCommand { get { return "EGYPT.GET_INFO"; } }
        //public override GameInfo GameInfo { get => base.GameInfo; set => base.GameInfo = value; }
        //public override string PlayGameCommand { get { return "XBOX.START"; } }
        public string InfoDetailCommand { get { return "EGYPT.GET_INFO_DETAIL"; } }
        public override string FinishGameTurnCommand { get { return "EGYPT.START"; } }
        public override string ShowHistoryCommand { get { return "EGYPT.GET_HISTORY"; } }
        public override string ShowGloryBoardCommand { get { return "EGYPT.GLORY_BOARD"; } }

        public override void InitGame()
        {
            SetActivePanel(BirdPanel.SelectBet, true);
            SetActivePanel(BirdPanel.SelectNumLine, false);
        }

        private void OnOpenSelectBetEvent()
        {
            OnJoinGame(JoinGameCommand);
        }

        public override void OnJoinGame(string joinGameCommand)
        {
            base.OnJoinGame(joinGameCommand);
        }


        public override void JoinGameHandler(InBoundMessage joinGameRespond)
        {
            int count = joinGameRespond.readByte();
            int[] listBet = new int[count];
            for (int i = 0; i < count; i++)
            {
                int betAmount = joinGameRespond.readInt();
                listBet[i] = betAmount;
            }
            birdSelectBet.GetResult(listBet);
        }


        public override void FillFinishTurnData(ref OutBounMessage request)
        {
            if (birdGamePlay.gameInfo.freeSpin > 0)
            {
                request.writeInt(birdGamePlay.gameInfo.lastBet);
                request.writeByte(birdGamePlay.gameInfo.lastLineState.Length);
                for (int i = 0; i < birdGamePlay.gameInfo.lastLineState.Length; i++)
                {
                    //không cần -1 vì đây là thông tin trên server trả về.
                    request.writeByte(birdGamePlay.gameInfo.lastLineState[i]);
                }
            }
            else
            {
                request.writeInt(birdGamePlay.gameInfo.betSelected);
                request.writeByte(birdGamePlay.gameInfo.listLine.Length);
                for (int i = 0; i < birdGamePlay.gameInfo.listLine.Length; i++)
                {
                    //-1 vì listline giá trị nhỏ nhất là 1 lớn nhất 20
                    // yêu cầu 0 đến 19
                    request.writeByte(birdGamePlay.gameInfo.listLine[i]);
                }
            }

            request.writeByte(birdGamePlay.gameInfo.isTrial ? 1 : 0); // 0 choi that . 1 choi demo                                    
            request.writeString(GameCode);
            request.writeByte(1);
        }
     
        public override void OnGameTurnFinish(InBoundMessage respond)
        {
            Hashtable hashtable = new Hashtable();

            int count = respond.readByte();
            int[] listItem = new int[count];
            for (int i = 0; i < count; i++)
            {
                listItem[i] = respond.readByte();
            }
            int coutPrize = respond.readByte();
            BirdSlotPrize[] listPrize = new BirdSlotPrize[coutPrize];
            for (int i = 0; i < coutPrize; i++)
            {
                listPrize[i] = new BirdSlotPrize();
                listPrize[i].row = respond.readByte();
                listPrize[i].item = respond.readByte();
                listPrize[i].quantity = respond.readByte();
            }
            int totalChip = respond.readInt();
            int numTurnFree = respond.readByte();
            int xStartMiniGame = respond.readByte();
            bool isBigWin = respond.readByte() == 1;
            bool isPotBreak = respond.readByte() == 1;
            int totalTurnFree = respond.readInt();
            bool isTrial = respond.readByte() == 1;
            if (isPotBreak)
            {
                //SoundManager.instance.(SoundFX.JACKPOT + "_" + UnityEngine.Random.Range(1, 3));
                birdGamePlay.timeCDAutoSelect = 5;
                birdGamePlay.HandleBreakPot(totalChip);
            }else if (isBigWin)
            {
                SoundManager.instance.PlayUISound(SoundFX.BIG_WIN + "_" + UnityEngine.Random.Range(1, 4));
                birdGamePlay.timeCDAutoSelect = 5;
                birdGamePlay.HandleBigWin(totalChip);
            }else if (totalChip > 0)
            {
                SoundManager.instance.PlayEffectSound(SoundFX.WIN_2);
                birdGamePlay.timeCDAutoSelect = 2;
                birdGamePlay.HandleNormalWin(totalChip);
            }else if (totalChip == 0)
            {
                SoundManager.instance.PlayEffectSound(SoundFX.LOOSE);
            }
            birdGamePlay.ChangeLastWinPrize(totalChip);

            if (numTurnFree > 0)
            {
                birdGamePlay.gameInfo.lastBet = birdGamePlay.gameInfo.betSelected;
                birdGamePlay.gameInfo.lastLineState = birdGamePlay.gameInfo.listLine;
            }
            Debug.Log("xStartMiniGame = " + xStartMiniGame);
            birdGamePlay.gameInfo.freeSpin = totalTurnFree;
            hashtable.Add("count", count);
            hashtable.Add("listItem", listItem);
            hashtable.Add("coutPrize", coutPrize);
            hashtable.Add("listPrize", listPrize);
            hashtable.Add("totalChip", totalChip);
            hashtable.Add("numTurnFree", numTurnFree);
            hashtable.Add("xStartMiniGame", xStartMiniGame);
            hashtable.Add("isBigWin", isBigWin);
            hashtable.Add("isPotBreak", isPotBreak);
            hashtable.Add("totalTurnFree", totalTurnFree);
            hashtable.Add("isTrial", isTrial);

            birdGamePlay.gameInfo.xStartMiniGame = xStartMiniGame;
            //Debug.Log("xStartMiniGame = " + xStartMiniGame);
            birdGamePlay.OnServerRespond(hashtable);
        }



        public override void ShowHistory()
        {
            
            var showHistoryRequest = new OutBounMessage("MATCH.HISTORY");
            showHistoryRequest.addHead();
            FillHistoryRequestData(ref showHistoryRequest);
            App.ws.send(showHistoryRequest.getReq(), OnShowHistory);
        }
        public override void FillHistoryRequestData(ref OutBounMessage request)
        {
            request.writeString(GameCode);//game code
            request.writeByte(1);// postion start
            request.writeString("");// time start
            request.writeString("");// time end
        }
        public override void OnShowHistory(InBoundMessage result)
        {
            HistoryRowData[] data;

            var count = result.readByte();
            data = new HistoryRowData[count];
            for (int i = 0; i < count; i++)
            {

                long index = result.readLong();
                string time = result.readString();
                string game = result.readString();
                string bet = result.readString();
                string change = result.readString();
                string balance = result.readString();
                data[i] = new HistoryRowData(index.ToString(), time, bet, change, balance);

            }

            birdHistory.panelLines = data;
            if (!birdHistory.gameObject.activeSelf)
            {
                birdHistory.gameObject.SetActive(true);
            }

        }

        public override void OnShowGloryBoard(InBoundMessage gloryboardRespond)
        {
            //   Debug.Log("=========================");
            var count = gloryboardRespond.readByte();
             //count = 20;
            GrolyRowData[] data = new GrolyRowData[count];
           //    Debug.Log("count = " + count);
            for (int i = 0; i < count; i++)
            {
               var content = gloryboardRespond.readString();
               // Debug.Log("content = " + content);


                /*06/07/2018 - 09:45|tuananhsn95|100|500,002|true(no hu)|true*/
                //  var content = "06/07/2018 - 09:45|tuananhsn95|100|500,002|true|true";
                var arr = content.Split('|');


                string time = arr[0];
                var times = arr[0].Split('-');
                if (times.Length == 2)
                {
                    time = times[0] + " " + times[1];
                }
                else
                {
                    time = arr[0];
                }

                string user = arr[1];
                string mucCuoc = arr[2];
                string thang = arr[3];
                bool jackpost = arr[4] == "true" ? true : false;
                bool bigWin = arr[5] == "true" ? true : false;

                //Debug.Log("jackpost = "+ jackpost+ arr[4] + " bigWin = "+ bigWin+ arr[5]);
                data[i] = new GrolyRowData(user, time, mucCuoc, thang, bigWin, jackpost);
            }

            // Debug.Log("=========================");


            gloryPanel.panelLines = data;

            if (!gloryPanel.gameObject.activeSelf)
            {
                gloryPanel.gameObject.SetActive(true);
            }
        }


        public void GetInforDetail()
        {
            var showGetInforDetail = new OutBounMessage(InfoDetailCommand);
            showGetInforDetail.addHead();
            FillGetInforDetailData(ref showGetInforDetail);
            App.ws.send(showGetInforDetail.getReq(), OnGetInforDetail);
        }
        private void FillGetInforDetailData(ref OutBounMessage request)
        {
            request.writeInt(birdGamePlay.gameInfo.betSelected);
            request.writeString(GameCode);
        }

        public void OnGetInforDetail(InBoundMessage result)
        {
            birdGamePlay.gameInfo.freeSpin = result.readInt();
            int pot = result.readInt();
            string newStr = result.readString();
            //if have freeSpin split newStr
            if (birdGamePlay.gameInfo.freeSpin > 0)
            {
                string[] str = newStr.Split('-');
                int[] lastLineState;
                lastLineState = new int[str.Length];
                for (int i = 0; i < str.Length; i++)
                {
                    lastLineState[i] = int.Parse(str[i]);
                }
                birdGamePlay.gameInfo.lastLineState = lastLineState;
            }
            else
            {
                birdGamePlay.gameInfo.lastLineState = new int[0];
            }

            birdGamePlay.gameInfo.lastBet = result.readInt();
            birdGamePlay.HandleRespondInfoDetail();
        }
        public override void OnQuitGame()
        {
            SoundManager.instance.PlayUISound(SoundFX.STOP);

            StopAllCoroutines();
            LoadingControl.instance.loadingGojList[30].SetActive(true);
            StartCoroutine(CorouBack());
        }
        private IEnumerator CorouBack()
        {
            SoundManager.instance.PlayUISound(SoundFX.SL7_CLICK);
            yield return new WaitForSeconds(.1f);
            try
            {
                // LoadingControl.instance.asbs[0].Unload(true);
            }
            catch (Exception)
            {

            }
            SceneManager.LoadScene("Lobby");
        }
    }

    public enum BirdPanel
    {
        GameContent,
        GameHistory,
        GameGuide,
        HitEgg,
        SelectBet,
        SelectNumLine
    }
    public class BirdSlotPrize
    {
        public int row;
        public int item;
        public int quantity;
    }
    //public class BirdBetData : BetData
    //{
    //    public int BetSelected { get; set; }

    //}
    public class BirdInfo : GameInfo
    {
        public BirdInfo(int betSelected = 100, bool isPlaying = false, bool isTurnOnAuto = false, bool isTurnUpSpeed = false, int freeSpin = 0)
        {
            this.betSelected = betSelected;
            this.isPlaying = isPlaying;
            this.isTurnOnAuto = isTurnOnAuto;
            this.isTurnUpSpeed = isTurnUpSpeed;
            this.listLine = new int[20] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
            this.freeSpin = freeSpin;
            this.isTrial = false;
            this.xStartMiniGame = 0;
        }
        public int xStartMiniGame;
        public bool isTrial;
        public int freeSpin;
        //public int changePotMinigame;
        public int[] lastLineState;
        //last bet use for free turn;
        public int lastBet { get; set; }
        public int betSelected { get; set; }
        public int pot { get; set; }
        public int[] listLine { get; set; }
        public bool isPlaying { get; set; }
        public bool isTurnOnAuto { get; set; }
        public bool isTurnUpSpeed { get; set; }

        public int CalculateTotalBet()
        {
            return listLine.Length * betSelected;
        }
       
    }

}
public enum KindFXSound
{
    Normal,
    BigWin,
    PotBreak,
    Miss
}