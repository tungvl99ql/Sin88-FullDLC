using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slot.Core;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Core.Server.Api;

namespace Slot.Games.Fish {
    public class FishGameController : SlotGame
    {

        public Text txtGiaTriHu;
        public Text txtBalance;
        public Button btnBackGame;
        public FishGameHistoryPanel fishHistory;
        public FishGameGuidePanel fishGuide;
        public FishGameGloryPanel fishGameGlory;
        private int lastPotValue;
        public IEnumerator[] thread = new IEnumerator[1];
        #region SetBase
        public override string GameCode { get  { return GameCodeApp.gameCode3; }  }
        public override string GetInfoDetailCommand { get { return "69C.GET_INFO_DETAIL"; } }
        public override string JoinGameCommand { get { return "69C.GET_INFO"; } }
        public override string PlayGameCommand { get { return "69C.START"; } }
        public override string ShowHistoryCommand { get { return "69C.HISTORY"; } }
        public override string ShowGloryBoardCommand { get { return "69C.GLORY_BOARD"; } }
        #endregion

        #region History
        public override void ShowHistory()
        {

            var showHistoryRequest = new OutBounMessage("MATCH.HISTORY");
            showHistoryRequest.addHead();
            FillHistoryRequestData(ref showHistoryRequest);
            App.ws.send(showHistoryRequest.getReq(), OnShowHistory);
            //  base.ShowHistory();
        }
        public override void FillHistoryRequestData(ref OutBounMessage request)
        {
         
            request.writeString(GameCode);         //game name           
            request.writeByte(1);                       //page index
            request.writeString("");                    //from date
            request.writeString("");

           
        }
        public override void OnShowHistory(InBoundMessage result)
        {

            int count = result.readByte();
            HistoryRowData[] historyData = new HistoryRowData[count];
        
            for (int i = 0; i < count; i++)
            {

                long index = result.readLong();
                string time = result.readString();
                string game = result.readString();
                string bet = result.readString();
                string change = result.readString();
                string balance = result.readString();
                historyData[i] = new HistoryRowData(index.ToString(), time, bet, change, balance);
                fishHistory.panelLines = historyData;
            
            }
      
            if (!fishHistory.gameObject.activeSelf)
                fishHistory.gameObject.SetActive(true);
        }

        #endregion

        #region BetData
        public override void OnJoinGame(string joinGameCommand)
        {
            base.OnJoinGame(joinGameCommand);
        }

        public override void JoinGameHandler(InBoundMessage joinGameRespond)
        {
            int[] level;

            int betLevel = joinGameRespond.readByte();


            level = new int[betLevel];
            for (int i = 0; i < betLevel; i++)
            {
                var bet = joinGameRespond.readInt();
                level[i] = bet;
            }
            ((FishSlotInfo)(GameInfo)).betLevels = level;
            int count = joinGameRespond.readByte();                                    //prize
            for (int i = 0; i < count; i++)
            {
                int index = joinGameRespond.readByte();                            //piece index
                int prizeNum = joinGameRespond.readByte();                         //num of piece to get prize
                string amount = joinGameRespond.readString();                      //achive
                //App.trace("index = " + index + "|prizeNum = " + prizeNum + "|amount = " + amount,"yellow");
            }
        }
        #endregion

        #region Glory
        public override void OnShowGloryBoard(InBoundMessage gloryboardRespond)
        {
            var count = gloryboardRespond.readByte();
            //Debug.Log("count " + count);
            GrolyRowData[] data = new GrolyRowData[count];
            for (int i = 0; i < count; i++)
            {
                var content = gloryboardRespond.readString();
                //Debug.Log("content " + content);
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

                data[i] = new GrolyRowData(user, time, mucCuoc, thang, bigWin, jackpost);
            }

            // Debug.Log("=========================");


            fishGameGlory.panelLines = data;

            if (!fishGameGlory.gameObject.activeSelf)
            {
                fishGameGlory.gameObject.SetActive(true);
            }
        }
        public override void ShowGloryBoard()
        {
            base.ShowGloryBoard();
        }
        #endregion

        #region Info Detail
        public override void FillGetInfoDetail(ref OutBounMessage request)
        {
            base.FillGetInfoDetail(ref request);
        }
        public override void GetInfoDetail()
        {
            base.GetInfoDetail();
        }
        public override void OnGetInfoDetail(InBoundMessage respond)
        {
            base.OnGetInfoDetail(respond);
        }
        #endregion

        #region Guide
        public override void ShowTutorial()
        {
            base.ShowTutorial();
        }
        public override void OnShowTutorial(InBoundMessage result)
        {
            fishGuide.achiveData.Clear();

            int betLevel = result.readByte();
            for (int i = 0; i < betLevel; i++)
            {
                var bet = result.readInt();
            }
            int count = result.readByte();                                    //prize
            for (int i = 0; i < count; i++)
            {
                int index = result.readByte();                            //piece index
                int prizeNum = result.readByte();                         //num of piece to get prize
                string amount = result.readString();                      //achive
                //App.trace("index = " + index + "|prizeNum = " + prizeNum + "|amount = " + amount,"yellow");
                //Debug.Log("index = " + index + "|prizeNum = " + prizeNum + "|amount = " + amount);
                if (fishGuide.achiveData.ContainsKey(index))
                    fishGuide.achiveData[index].Add(new ArrayList() { prizeNum, amount });
                else
                    fishGuide.achiveData.Add(index, new List<ArrayList>() { new ArrayList() { prizeNum, amount } });
            }
            if (!fishGuide.gameObject.activeSelf)
                fishGuide.gameObject.SetActive(true);
        }
        #endregion

        #region Spin
        public override void OnResult(InBoundMessage result)
        {
            int count = result.readByte();                        //pieces id
            int[] ids = new int[count];
            for (int i = 0; i < count; i++)
            {
                int index = result.readByte();
                ids[i] = index;
                //App.trace("per piece : " + index, "yellow");
            }
            int countPrize = result.readByte();                                //prize count
            int[] lineIndexs = new int[countPrize];
            int[] pieceIndexs = new int[countPrize];
            int[] pieceNums = new int[countPrize];
            for (int i = 0; i < countPrize; i++)
            {
                int lineIndex = result.readByte();                    //won line (real-= 1)
                lineIndexs[i] = lineIndex;
                int pieceIndex = result.readByte();                   //won piece index
                pieceIndexs[i] = pieceIndex;
                int pieceNum = result.readByte();                     //pieces num in won line
                pieceNums[i] = pieceNum;
                //App.trace(lineIndex + "|" + pieceIndex + "|" + pieceNum, "yellow");
            }

            int wonBalance = result.readInt();
            int freeSpinNum = result.readByte();
            //totalFreeSpin = freeSpinNum;
            int remainSpint = result.readByte();
            bool isBigWin = result.readByte() == 1;
            bool isPotBreak = result.readByte() == 1;
            int totalFreeSpin = result.readInt();
            bool isTrial = result.readByte() == 1;
            if (receiveResultEvent != null)
            {
                Hashtable data = new Hashtable();
                data.Add("totalPiece", count);
                data.Add("perPieces", ids);
                data.Add("totalPrize", countPrize);
                data.Add("perLinePrizeIndexs",lineIndexs);
                data.Add("piecePrizeIndexs", pieceIndexs);
                data.Add("piecePrizeNums", pieceNums);
                data.Add("totalMoneyWin", wonBalance);
                data.Add("freeSpinNum", freeSpinNum);
                data.Add("remainSpin",remainSpint);
                data.Add("isBigWin",isBigWin);
                data.Add("isPotBreak",isPotBreak);
                data.Add("totalFreeSpin", totalFreeSpin);
                data.Add("isTrial", isTrial);
                receiveResultEvent.Invoke(data);
            }
        }
       
        public override void FillBetData(ref OutBounMessage request)
        {
            var betLevel = ((FishSlotBetData)((FishSlotInfo)(GameInfo)).BetData).selectedBetLevel;
            var totalLine = ((FishCurrLine)((FishSlotInfo)(GameInfo)).NumberLine).currentLine.Length;
            request.writeInt(betLevel);
            request.writeByte(totalLine);
            for(int i = 0; i < totalLine; i++)
            {
                request.writeByte(((FishCurrLine)((FishSlotInfo)(GameInfo)).NumberLine).currentLine[i]);
                //App.trace("Picked Line " + ((FishCurrLine)((FishSlotInfo)(GameInfo)).NumberLine).currentLine[i],"blue");
            }
            request.writeByte(((FishSlotInfo)(GameInfo)).playReal?0:1);
            request.writeString(GameCode);                        //game id
            request.writeByte(1);
            //App.trace("bet level = "+betLevel+" | total line = "+totalLine+" | play real ? = "+((FishSlotInfo)(GameInfo)).playReal+" | game code = "+GameCode,"red");
        }
        #endregion

        public override void InitGame()
        {
            int[] startNumberLine = new int[20] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19};
            CPlayer.potchanged += OnPotChanged;
            CPlayer.changed += OnChipBalanceChanged;
            txtBalance.text = App.formatMoney(CPlayer.chipBalance.ToString());
            GameInfo = new FishSlotInfo();
            GameInfo.BetData = new FishSlotBetData();
            GameInfo.NumberLine = new FishCurrLine(startNumberLine);
        }
        private void OnEnable()
        {
            CPlayer.forceStopGameEvent += OnForceStopGame;

        }
        private void OnDisable()
        {
            CPlayer.forceStopGameEvent -= OnForceStopGame;
        }
        private void OnForceStopGame(string gameCode, int gameState)
        {
            Debug.Log(1 + "Fish : " + gameCode + " === " + GameCode);

            if (gameCode == GameCode)
            {
                btnBackGame.interactable = true;
            }

        }

        public IEnumerator TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            float i = 0.0f;
            float rate = 2.0f / tweenTime;
            txt.transform.DOScale(scaleNum, tweenTime / 2);
            while (i < tweenTime)
            {
                i += Time.deltaTime * rate;
                float a = Mathf.Lerp(fromNum, toNum, i);

                txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                if (a == toNum)
                {
                    i = tweenTime;
                }
                yield return null;
            }
            txt.transform.localScale = Vector2.one;
            yield return new WaitForSeconds(.05f);
        }
        public override void OnPotChanged()
        {
            InBoundMessage res = CPlayer.res_pot;
            // number of games.
            int count = res.readByte();

            for (int i = 0; i < count; i++)
            {
                // game name
                string gameId = res.readString();
                if (gameId == GameCode)
                {
                    int count0 = res.readByte();
                    for (int j = 0; j < count0; j++)
                    {
                        int bet = res.readInt();
                        int value = res.readInt();

                        if (bet == ((FishSlotBetData)((FishSlotInfo)(GameInfo)).BetData).selectedBetLevel)
                        {
                            txtGiaTriHu.text = string.Format("{0:0,0}", value);
                            if (thread[0] != null)
                                StopCoroutine(thread[0]);
                            thread[0] = TweenNum(txtGiaTriHu, lastPotValue, value, 1f, 1f);
                            StartCoroutine(thread[0]);
                            lastPotValue = value;
                            return;
                        }

                    }
                }
                else
                {
                    int count1 = res.readByte();
                    for (int j = 0; j < count1; j++)
                    {
                        int bet = res.readInt();
                        int value = res.readInt();
                    }
                }
            }
        }

        public override void OnQuitGame(string SceneName = null)
        {
            LoadingControl.instance.loadingGojList[30].SetActive(true);
            CPlayer.potchanged -= OnPotChanged;
            CPlayer.changed -= OnChipBalanceChanged;
            if (thread[0] != null)
                StopCoroutine(thread[0]);
            StopAllCoroutines();
            if (SceneName != null)
                SceneManager.LoadScene(SceneName);
            else
            {
                try {
                    LoadingControl.instance.asbs[0].Unload(true);
                } catch
                {

                }
                SceneManager.LoadScene("Lobby");
            }
        }

        public override void OnChipBalanceChanged(string type)
        {
            if (type == "chip")
            {
                if (CPlayer.preChipBalance >= CPlayer.chipBalance)
                {
                    txtBalance.text = string.Format("{0:0,0}", CPlayer.chipBalance);
                }
            }
        }
       
    }
    public class FishSlotBetData : BetData
    {
        public int selectedBetLevel;

        public FishSlotBetData(int _selectBetLevel = 100)
        {
            this.selectedBetLevel = _selectBetLevel;
        }
    }
    public class FishCurrLine : NumberLine
    {
        public int[] currentLine;
        public FishCurrLine(int[] _currLine)
        {
            this.currentLine = _currLine;
        }
    }
    public class FishSlotInfo : GameInfo
    {
        public int pot
        {
            get;
            set;
        }
        public int[] betLevels
        {
            get;
            set;
        }
        public bool isAutoRun
        {
            get;
            set;
        }
        public bool isRunning
        {
            get;
            set;
        }
        public BetData currentBetData
        {
            get;
            set;
        }
        public NumberLine currentLine
        {
            get;
            set;
        }
        public bool playReal
        {
            get;
            set;
        }
        public int totalLineBet
        {
            get;
            set;
        }
        public FishSlotInfo()
        {

        }

    }
}
