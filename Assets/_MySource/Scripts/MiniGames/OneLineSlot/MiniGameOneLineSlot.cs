using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;
using DG.Tweening;
using Core.Server.Api;

namespace Casino.Games.OneLineSlot
{
    public class MiniGameOneLineSlot : MiniGame
    {

        public override string GameCode
        {
            get
            {
                return GameCodeApp.gameCode6;
            }
        }
        public override string JoinGameCommand
        {
            get
            {
                return "GUMMY.GET_INFO";
            }
        }
        public override string PlayGameCommand
        {
            get
            {
                return "GUMMY.START";
            }


        }
        public override string ShowHistoryCommand
        {
            get
            {
                return "GUMMY.GET_HISTORY";
            }
        }
        public override string ShowGloryBoardCommand
        {
            get
            {
                return "GUMMY.GLORY_BOARD";
            }
        }
        public Text txtGiaTriHu;
        public SlotOneLineGameplayPanel gameplayPanel;
        public SlotOneLineGloryPanel gloryPanel;
        public SlotOneLineHistoryPanel historyPanel;
        public SlotOneLineGuidePanel guidePanel;
        private int lastPotValue;

        public override void OnEnable()
        {
            InitGame();
            base.OnEnable();
            CPlayer.potChangedOneLineSlotMiniGame += OnPotChanged;
            CPlayer.forceStopGameEvent += OnForceStopGame;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            CPlayer.potChangedOneLineSlotMiniGame -= OnPotChanged;
            CPlayer.forceStopGameEvent -= OnForceStopGame;
        }
        private void OnForceStopGame(string gameCode, int gameState)
        {

            if (gameCode == GameCode)
            {
                gameplayPanel.TurnExitButton(true);
            }

        }
        public override void OnQuitGame()
        {
            gameplayPanel.Reset();
            gameObject.SetActive(false);
        }

        public override void OnPotChanged()
        {
            base.OnPotChanged();
            InBoundMessage res = CPlayer.res_potMiniGameOneLineSlot;
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

                        if (bet == ((OnLineSlotBetData)((OneLineSlotInfo)(GameInfo)).BetData).selectedBetLevel)
                        {
                            txtGiaTriHu.text = string.Format("{0:0,0}", value);
                            StartCoroutine(TweenNum(txtGiaTriHu, lastPotValue, value, 1f, 1f));
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
        private IEnumerator TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
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
        public override void InitGame()
        {
            if (GameInfo == null)
            {
                GameInfo = new OneLineSlotInfo();
                GameInfo.BetData = new OnLineSlotBetData();
            }
   
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
       
            ((OneLineSlotInfo)(GameInfo)).betLevels = level;
            gameplayPanel.UpdateBetLevel();
        }

        public override void FillBetData(ref OutBounMessage request)
        {
            var betLevel = ((OnLineSlotBetData)((OneLineSlotInfo)(GameInfo)).BetData).selectedBetLevel;
            request.writeInt(betLevel);
            request.writeString(GameCode);

        }

        public override void OnResult(InBoundMessage result)
        {

          //  Debug.Log("=========================");

            var item = result.readString();
           // Debug.Log("item => " + item);
            var totalMoney = result.readInt();
           // Debug.Log("totalMoney => " + totalMoney);
            var positionWin = result.readString();
           // Debug.Log("positionWin => " + positionWin);
            var bigWin = result.readByte() == 1 ? true : false;
           // Debug.Log("bigWin => " + bigWin);
            var jackpot = result.readByte() == 1 ? true : false;
           // Debug.Log("jackpot => " + jackpot);
          //  Debug.Log("=========================");



            if (receiveResultEvent != null)
            {
                Hashtable data = new Hashtable();
                data.Add("item", item);
                data.Add("totalMoney", totalMoney);
                data.Add("positionWin", positionWin);
                data.Add("bigWin", bigWin);
                data.Add("jackpot", jackpot);
                receiveResultEvent.Invoke(data);
            }
           
            


        }
        public override void OnShowGloryBoard(InBoundMessage gloryboardRespond)
        {

         //   Debug.Log("=========================");
            var count = gloryboardRespond.readByte();
            // count = 20;
            GrolyRowData[] data = new GrolyRowData[count];
        //    Debug.Log("count = " + count);
            for (int i = 0; i < count; i++)
            {
                var content = gloryboardRespond.readString();
             //  Debug.Log("content = " + content);


                /*06/07/2018 - 09:45|tuananhsn95|100|500,002|true(no hu)|true*/
                //   var content = "06/07/2018 - 09:45|tuananhsn95|100|500,002|true|true";
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


            gloryPanel.panelLines = data;

            if (!gloryPanel.gameObject.activeSelf)
            {
                gloryPanel.gameObject.SetActive(true);
            }
        }

        public override void FillHistoryRequestData(ref OutBounMessage request)
        {
            request.writeString(GameCode);//game code
            request.writeByte(1);// postion start
            request.writeString("");// time start
            request.writeString("");// time end
        }
        public override void ShowHistory()
        {
            var showHistoryRequest = new OutBounMessage("MATCH.HISTORY");
            showHistoryRequest.addHead();
            FillHistoryRequestData(ref showHistoryRequest);
            App.ws.send(showHistoryRequest.getReq(), OnShowHistory);
        }
      
        public override void OnShowHistory(InBoundMessage result)
        {
            var count = result.readByte();
            HistoryRowData[] data = new HistoryRowData[count];
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
            historyPanel.panelLines = data;

            if (!historyPanel.gameObject.activeSelf)
            {
                historyPanel.gameObject.SetActive(true);
            }
        }
        public override void ShowTutorial()
        {
           if (!guidePanel.gameObject.activeSelf)
            {
                guidePanel.gameObject.SetActive(true);
            }
        }

    }

    public class OnLineSlotBetData : BetData
    {
        public int selectedBetLevel;

        public OnLineSlotBetData(int _selectBetLevel = 100)
        {
            this.selectedBetLevel = _selectBetLevel;
        }
    }

    public class OneLineSlotInfo : GameInfo
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

        public OneLineSlotInfo()
        {
            betLevels = new int[3];
        }

    }

}