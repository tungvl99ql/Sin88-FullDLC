using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;
using Casino.Games.BxB;
using DG.Tweening;
using Core.Server.Api;

namespace Casino.Games.BxB
{

	public delegate void OnReceiveResult(bool hasResult, bool bw, bool jp, ref int[] resultSlot, ref List<int[]> resultSlotData);

    public class MiniGame3X3 : MiniGame
    {

        public GuidePanel guidePanel;
        public HistoryPanel historyPanel;
        public GloryPanel gloryPanel;
        public ChooseLinesPanel chooseLinesPanel;
        public GameplayPanel gameplayPanel;

        public int[] betLevelList = new int[] { 100, 1000, 10000 };

        public Text bigRewardTxt;
        public Text totalChipTxt;

        public Text betLevel1Txt;
        public Text betLevel2Txt;
        public Text betLevel3Txt;

        private int totalChip;

        private OnReceiveResult onReceiveResult;
		private int lastPotValue;
        private int[] resultSlots = new int[9];

        private const string gameCode= GameCodeApp.gameCode5;
        public override string GameCode
        {
            get
            {
                return gameCode;
            }
        }

        public override string JoinGameCommand
        {
            get
            {
                return "PIGS.GET_INFO";
            }
        }

        public override string PlayGameCommand
        {
            get
            {
                return "PIGS.START";
            }
        }

        public override string FinishGameTurnCommand
        {
            get
            {
                return "PIGS.COMPLETED";
            }

        }

        public override string ShowHistoryCommand
        {
            get
            {
                return "PIGS.GET_HISTORY";
            }

        }

        public override string ShowGloryBoardCommand
        {
            get
            {
                return "PIGS.GLORY_BOARD";
            }
        }

		public override void OnEnable ()
		{
            transform.SetSiblingIndex(22);
            base.OnEnable ();
			CPlayer.potChanged3X3MiniGame += OnPotChanged;
            CPlayer.forceStopGameEvent += OnForceStopGame;
		}

		public override void OnDisable ()
		{
			base.OnDisable ();
			CPlayer.potChanged3X3MiniGame -= OnPotChanged;
            CPlayer.forceStopGameEvent -= OnForceStopGame;
            //gameplayPanel.requestResultEvent.RemoveListener(Play);
        }

		public override void OnPotChanged ()
		{
			base.OnPotChanged ();

			InBoundMessage res = CPlayer.res_potMiniGame3X3;
			// number of games.
			int count = res.readByte();

			for (int i = 0; i < count; i++)
			{
				// game name
				string gameId = res.readString();
				if (gameId == GameCode)
				{
					// bet levels.
					int count0 = res.readByte();
					for (int j = 0; j < count0; j++)
					{
						int bet = res.readInt();
						int value = res.readInt();

						if (bet == ((BXBBetData)GameInfo.BetData).betAmount) {							
							bigRewardTxt.text = string.Format("{0:0,0}", value);
							StartCoroutine(TweenNum(bigRewardTxt, lastPotValue, value, 1f, 1f));
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

        public override void InitGame()
        {
            gameplayPanel.betLevelChangeEvent = OnBetLevelChange;
            onReceiveResult = gameplayPanel.OnReceicveResult;
            gameplayPanel.requestResultEvent.AddListener(Play);
            GameInfo = new BXBGameInfo();
            GameInfo.BetData = new BXBBetData(100, 20, new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 });
            chooseLinesPanel.CurrentBetData = (BXBBetData)GameInfo.BetData;
        }

        public override void JoinGameHandler(InBoundMessage joinGameRespond)
        {
            // handle bet levels
            int betLevel = joinGameRespond.readByte();
            var bet1 = joinGameRespond.readInt();
            var bet2 = joinGameRespond.readInt();
            var bet3 = joinGameRespond.readInt();
            setBetLevel(bet1, bet2, bet3);

            // handle bet lines
            int betLines = joinGameRespond.readByte();
            GuideLineData[] lineDatas = new GuideLineData[betLines];
            for (int i = 0; i < betLines; i++)
            {
                var item = joinGameRespond.readByte();
                var quantity = joinGameRespond.readByte();
                string reward = joinGameRespond.readString();
                lineDatas[i] = new GuideLineData(item, quantity, reward);
                string formatText = string.Format(" price item = {0} | quantity = {1} | value = {2}", item, quantity, reward);
                //Debug.Log("======");
                //Debug.Log(formatText);
            }
            guidePanel.panelLines = lineDatas;
        }

        public override void FillBetData(ref OutBounMessage request)
        {
            request.writeInt(((BXBBetData)GameInfo.BetData).betAmount);
			request.writeByte(((BXBBetData)GameInfo.BetData).lineIdxes.Count);
            foreach (var idx in ((BXBBetData)GameInfo.BetData).lineIdxes)
            {
                request.writeByte(idx);
            }

            request.writeString(GameCode);
        }

        public override void FillFinishTurnData(ref OutBounMessage request)
        {
            request.writeInt(totalChip);
        }

        IEnumerator ieWaitForFinishTurn(float t)
        {
            yield return new WaitForSeconds(t);
            FinishGameTurn();
        }


        public override void OnGameTurnFinish(InBoundMessage respond)
        {

        }

        public override void OnResult(InBoundMessage result)
        {
            var lineNum = result.readByte();
            for (int i = 0; i < lineNum; i++)
            {
                var itemIdx = result.readByte();

                resultSlots[i] = itemIdx;
				//Debug.Log ("=================> " + itemIdx);

            }
            List<int[]> resultSlotData = new List<int[]>();
            var slotSize = result.readByte();

            for (int i = 0; i < slotSize; i++)
            {
                var row = result.readByte();
                var item = result.readByte();
                var quality = result.readByte();
                resultSlotData.Add(new int[] { row, item, quality });
                var s = string.Format(" row {0} item {1} quality {2} ", row, item, quality);

                //Debug.Log(s);
            }

            totalChip = result.readInt();
			var totalSpin = result.readByte ();
			var bigWin = result.readByte () == 1? true: false;
			var jackpot = result.readByte () == 1? true: false;
			totalChipTxt.text = string.Format("{0:0,0}", totalChip);

            if (onReceiveResult != null)
            {
				onReceiveResult(true, bigWin, jackpot , ref resultSlots, ref resultSlotData);
            }
            //StartCoroutine (ieWaitForFinishTurn (10));

        }



        private void setBetLevel(int bet1, int bet2, int bet3)
        {
//            betLevel1Txt.text = bet1.ToString();
//            betLevel2Txt.text = bet2.ToString();
//            betLevel3Txt.text = bet3.ToString();
			betLevel1Txt.text = "100";
			betLevel2Txt.text =	"1k";
			betLevel3Txt.text = "10k";
        }

        public override void ShowTutorial()
        {
            if (!guidePanel.gameObject.activeSelf)
            {
                guidePanel.gameObject.SetActive(true);
            }
        }

        public override void OnShowHistory(InBoundMessage result)
        {
            //int whatIsThis = result.readByte ();
            int historyLineLength = result.readByte();
            HistoryLineData[] lineData = new HistoryLineData[historyLineLength];

            for (int i = 0; i < historyLineLength; i++)
            {

                int idx = result.readInt();
                string timeStamp = result.readString();
                string room = result.readString();
                string win = result.readString();
                string bigWin = result.readString();
				lineData[i] = new HistoryLineData(idx.ToString(), timeStamp, room, win, bigWin);

            }

            historyPanel.panelLines = lineData;

            if (!historyPanel.gameObject.activeSelf)
            {
                historyPanel.gameObject.SetActive(true);
            }

        }

        public override void OnShowGloryBoard(InBoundMessage gloryboardRespond)
        {
            int gloryLineLength = gloryboardRespond.readByte();
            GloryLineData[] lineData = new GloryLineData[gloryLineLength];

            for (int i = 0; i < gloryLineLength; i++)
            {	

				var s = gloryboardRespond.readString ();
				var parsedData = s.Split ('|');

                if (parsedData.Length > 0)
                {
                    string timeStamp = parsedData[0];
                    string playerName = parsedData[1];
                    string betLevel = parsedData[2];
                    string win = parsedData[3];

                    bool isPotWin = false;
                    bool.TryParse(parsedData[4], out isPotWin);
                    bool isBigWin = false;
                    bool.TryParse(parsedData[5], out isBigWin);

                    lineData[i] = new GloryLineData(playerName, timeStamp, betLevel, win, isPotWin, isBigWin);
                }

                }

            gloryPanel.panelLines = lineData;

            if (!gloryPanel.gameObject.activeSelf)
            {
                gloryPanel.gameObject.SetActive(true);
            }
        }

        public void OnBetLevelChange(int selectedBetLevelIdx)
        {
            ((BXBBetData)(GameInfo.BetData)).betAmount = betLevelList[selectedBetLevelIdx];
            //Debug.Log("BetLevel = " + ((BXBBetData)(GameInfo.BetData)).betAmount.ToString());
        }

        public void ShowChooseLinePanel() {
            chooseLinesPanel.gameObject.SetActive(true);
        }

		public override void OnQuitGame()
		{
            gameObject.SetActive(false);
            gameplayPanel.Reset();
		}


        private void OnForceStopGame(string gameCode, int gameState) {

            if (gameCode == GameCode)
            {
                gameplayPanel.ForceExitSetupControl(true);
            }

        }



        // Thao function.
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

	}

    public class BXBBetData : BetData
    {

        public int betAmount;
        public int lineNum;
        public List<int> lineIdxes;

        public BXBBetData(int betAmount = 0, int lineNum = 0, List<int> lineIdxes = null)
        {
            this.betAmount = betAmount;
            this.lineNum = lineNum;
            this.lineIdxes = lineIdxes;
        }

        public BXBBetData(int betAmount = 0, int lineNum = 0)
        {
            this.betAmount = betAmount;
            this.lineNum = lineNum;
        }
    }

    public class BXBGameInfo : GameInfo
    {

        public int hu;
        public bool isQuickMode;
        public bool isAutoPlay;

        public BXBGameInfo(int hu = 0, bool isQuickMode = false, bool isAutoPlay = false)
        {
            this.hu = hu;
            this.isQuickMode = isQuickMode;
            this.isAutoPlay = isAutoPlay;
        }

        public BXBGameInfo(BXBBetData betData, int hu = 0, bool isQuickMode = false, bool isAutoPlay = false)
        {
            this.BetData = betData;
            this.hu = hu;
            this.isQuickMode = isQuickMode;
            this.isAutoPlay = isAutoPlay;
        }


    }
}

