using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Core.Server.Api;

namespace Core.Bird
{
    public class BirdGamePlay : MonoBehaviour
    {
        int myMoney = 0;
        public BirdInfo gameInfo = new BirdInfo();

        [SerializeField]
        BlackRed birdHitEgg;
        [SerializeField]
        BirdManager birdManager;
        [SerializeField]
        ChangerBird birdChange;
        [SerializeField]
        BirdGamePlay birdGamePlay;
        [SerializeField]
        History birdHistory;
        // [SerializeField] BirdGlory birdGlory;
        [SerializeField]
        SelectNumLine birdSelectNumLine;
        [SerializeField]
        SelectBet birdSelectBet;

        [SerializeField]
        Button btnPlay;
        [SerializeField]
        Button btnAutoPlay;
        [SerializeField]
        Button btnSelectLine;
        [SerializeField]
        Button btnGuide;
        [SerializeField]
        Button btnHistory;
        [SerializeField]
        Button btnBack;
        public GameObject gameAutoPlay;

        [SerializeField]
        Text txtBalance;
        [SerializeField]
        Text txtpot;
        [SerializeField]
        Text txtWinturn;
        [SerializeField]
        Text txtBigWin;
        [SerializeField]
        Text txtBrickPot;
        // [SerializeField] Text txtAutoPlay;
        //[SerializeField] Text txtSelectNumLine;
        [SerializeField]
        Text txtFreeSpin;
        [SerializeField]
        Text txtBetSelect;
        [SerializeField]
        Text txtTotalBet;
        [SerializeField]
        Text txtLastWinPrize;

        [SerializeField]
        GameObject pnlNormalWin;
        [SerializeField]
        GameObject pnlBigWin;
        [SerializeField]
        GameObject pnlBrickPot;
        [SerializeField]
        GameObject pnlThongBao;
        //Choose Skin
        //[SerializeField] GameObject goSkinPoor;
        // [SerializeField] GameObject goSkinRich;
        //  [SerializeField] Image skinBtnPlay;
        // [SerializeField] Image skinBtnAutoPlay;
        // [SerializeField] GameObject transferencePoor;
        // [SerializeField] GameObject transferenceRick;
        //   [SerializeField] Text txtLine;
        //[SerializeField] Text txtAuto;

        [SerializeField]
        Sprite[] spritesSkinBtn;

        private bool lastStateAutoPlay;

        public float timeCDAutoSelect = 1;
        //const string STR_STOP_AUTO = "Dừng Tự Quay";
        //const string STR_BEGIN_AUTO = "Tự Quay";
        string STR_FREE = "";//"Bạn Còn : ";
        //const string STR_SELECTNUM_1 = "Chọn ";
        //const string STR_SELECTNUM_2 = " Dòng";
        private int lastPot = 0;
        private bool isShow = false;

        public bool IsPlaying
        {
            get
            {
                return gameInfo.isPlaying;
            }
            set
            {
                gameInfo.isPlaying = value;
                ChangeStateBtnPlay(!value);
            }
        }
        public bool IsTurnOnAutoPlay
        {
            get
            {
                return gameInfo.isTurnOnAuto;
            }
            set
            {
                gameInfo.isTurnOnAuto = value;
                ChangeStateBtnAutoPlay(value);

                if (value)
                {
                    //ChangeText(txtAutoPlay, STR_STOP_AUTO);
                    //  skinBtnAutoPlay.color = Color.red;
                    if (!IsPlaying)
                    {
                        Play();
                    }
                }
                else
                {
                    //   skinBtnAutoPlay.color = Color.white;
                    //ChangeText(txtAutoPlay, STR_BEGIN_AUTO);

                }
            }
        }
        public bool IsTurnUpSpeed
        {
            get
            {
                return gameInfo.isTurnUpSpeed;
            }
            set
            {
                gameInfo.isTurnUpSpeed = value;
                ChangeStateBtnMegaSpeed(value);
            }
        }

        void Start()
        {
        }
        void Init()
        {
            txtLastWinPrize.text = "0";
            SoundManager.instance.PlayBackgroundSound(SoundFX.LOBBY_BG_MUSIC_2);
            gameInfo = new BirdInfo();
            IsPlaying = false;
            IsTurnOnAutoPlay = false;
            IsTurnUpSpeed = false;
            // txtBalance.text = string.Format("{0:0,0}", CPlayer.chipBalance);
        }
        void OnEnable()
        {
            myMoney = (int)CPlayer.chipBalance;
            Init();
            //changeBird.StartRollEvent.AddListener(OnStartPlay);
            birdChange.SendRequestEvent.AddListener(OnSendRequestEvent);
            birdChange.EndRollEvent.AddListener(OnEndPlayEvent);
            //birdHistory.OpenHistoryEvent.AddListener(OnShowHistory);
            birdHitEgg.EndGameBound.AddListener(OnCloseHitEgg);
            //birdHitEgg.StartGameBound.AddListener(OnOpenHitEgg);
            birdSelectNumLine.CloseSelectNumLineEvent.AddListener(OnCloseSelectNumLine);
            birdSelectBet.CloseSelectBetEvent.AddListener(OnCloseSelectBet);


            CPlayer.changed += CPlayer_changed;
            CPlayer.potchanged += CPlayer_potchanged;
        }

        private void CPlayer_changed(string type)
        {
            if (type == "chip")
            {
                if (CPlayer.preChipBalance != CPlayer.chipBalance)
                {
                    myMoney = (int)CPlayer.chipBalance;
                }
            }
        }

        void OnDisable()
        {
            HidePnlPotBrick();
            birdChange.SendRequestEvent.RemoveListener(OnSendRequestEvent);
            birdChange.EndRollEvent.RemoveListener(OnEndPlayEvent);
            //birdHistory.OpenHistoryEvent.RemoveListener(OnShowHistory);
            birdHitEgg.EndGameBound.RemoveListener(OnCloseHitEgg);
            //birdHitEgg.StartGameBound.RemoveListener(OnOpenHitEgg);
            birdSelectNumLine.CloseSelectNumLineEvent.RemoveListener(OnCloseSelectNumLine);
            birdSelectBet.CloseSelectBetEvent.RemoveListener(OnCloseSelectBet);
            CPlayer.changed -= CPlayer_changed;
            CPlayer.potchanged -= CPlayer_potchanged;

        }
        private void UpdateUIBet()
        {
            myMoney = (int)CPlayer.chipBalance;
            if (gameInfo.listLine.Length > 0)
            {
                btnAutoPlay.interactable = true;
                btnPlay.interactable = true;
            }
            else
            {
                MessageNumLine();
                btnAutoPlay.interactable = false;
                btnPlay.interactable = false;
            }
            if (gameInfo.isTrial)
            {
                btnAutoPlay.interactable = false;
                btnSelectLine.interactable = false;
            }
            else
            {
                if (gameInfo.listLine.Length <= 0)
                {
                    btnAutoPlay.interactable = false;
                }
                else
                {
                    btnAutoPlay.interactable = true;
                }
                btnSelectLine.interactable = true;
            }
            txtBetSelect.text = string.Format("{0:0,0}", gameInfo.betSelected);
            txtTotalBet.text = string.Format("{0:0,0}", gameInfo.CalculateTotalBet());
        }
        #region OnEvent
        private void OnCloseSelectBet()
        {
            gameInfo.betSelected = birdSelectBet.betSelected;

            gameInfo.isTrial = birdSelectBet.isTrial;
            //btnAutoPlay.interactable = !gameInfo.isTrial;
            ActiveSkin(gameInfo.betSelected);
            UpdateUIBet();
            GetInfoDetail();
            myMoney = (int)CPlayer.chipBalance;

        }
        private void ActiveSkin(int bet)
        {
            // bool isRich = bet >= 1000;
            // if (isRich)
            // {
            // txtLine.gameObject.SetActive(false);
            // txtAuto.gameObject.SetActive(false);
            //   goSkinRich.SetActive(true);
            //  goSkinPoor.SetActive(false);
            //  transferencePoor.SetActive(false);
            //  transferenceRick.SetActive(false);
            //    skinBtnPlay.overrideSprite = spritesSkinBtn[1];
            // skinBtnSelectLine.overrideSprite = spritesSkinBtn[0];
            //  skinBtnAutoPlay.overrideSprite = spritesSkinBtn[2];

            // }
            //  else
            //   {
            /*     txtLine.gameObject.SetActive(true);
                 txtAuto.gameObject.SetActive(true);
                 goSkinRich.SetActive(false);
                 goSkinPoor.SetActive(true);
                 transferencePoor.SetActive(false);
                 transferenceRick.SetActive(false);
                 skinBtnPlay.overrideSprite = spritesSkinBtn[3];
                 skinBtnSelectLine.overrideSprite = spritesSkinBtn[4];
                 skinBtnAutoPlay.overrideSprite = spritesSkinBtn[4];
             }*/
        }
        private void GetInfoDetail()
        {
            birdManager.GetInforDetail();
        }
        public void HandleRespondInfoDetail()
        {
            //if player have free Spin
            CheckFreeSpin();
        }
        private void CheckFreeSpin()
        {

            ChangeTextFreeSpin();
            if (gameInfo.freeSpin > 0)
            {
                SetInteractiveButtonSelectNumLine(false);
            }
            else
            {
                if (gameInfo.isTrial)
                {
                    SetInteractiveButtonSelectNumLine(false);
                }
                else
                {
                    SetInteractiveButtonSelectNumLine(true);
                }
            }
        }
        private void ChangeTextFreeSpin()
        {
             STR_FREE = App.listKeyText["FREE_SPIN_TURN"];
            string new1 = STR_FREE.Replace("#1", gameInfo.freeSpin.ToString());
            txtFreeSpin.text = new1; //string.Concat(STR_FREE, gameInfo.freeSpin.ToString());
            txtFreeSpin.transform.parent.gameObject.SetActive(gameInfo.freeSpin > 0 ? true : false);
        }
        private void SetInteractiveButtonSelectNumLine(bool isActive)
        {
            btnSelectLine.interactable = isActive;
        }
        private void MessageNumLine()
        {
            pnlThongBao.SetActive(true);
            pnlThongBao.transform.GetChild(0).GetComponent<Text>().DOFade(0, 2f).OnComplete(() =>
            {
                pnlThongBao.SetActive(false);
                pnlThongBao.transform.GetChild(0).GetComponent<Text>().DOFade(1, 0f);
            });
        }

        private void OnCloseSelectNumLine()
        {
            //cap nhat sau  khi chon line
            if (birdSelectNumLine.isSave)
            {
                gameInfo.listLine = birdSelectNumLine.listLine;
                if (IsTurnOnAutoPlay)
                {
                    IsTurnOnAutoPlay = gameInfo.listLine.Length == 0 ? false : true;
                }
                UpdateUIBet();
            }
        }

        private void OnOpenHitEgg()
        {
            //save state of auto play
            lastStateAutoPlay = IsTurnOnAutoPlay;
            IsTurnOnAutoPlay = false;
        }

        private void OnCloseHitEgg()
        {
            //begin Play if Auto
            if (lastStateAutoPlay)
            {
                IsTurnOnAutoPlay = true;
            }
        }

        private void OnSendRequestEvent()
        {
            birdManager.FinishGameTurn();
        }

        private void OnEndPlayEvent()
        {

            CheckFreeSpin();

            if (gameInfo.xStartMiniGame > 0)
            {
                birdHitEgg.gameObject.SetActive(true);
                OnOpenHitEgg();
                //birdHitEgg.StartGameBound.Invoke(gameInfo.isTrial, gameInfo.xStartMiniGame, gameInfo.betSelected);
            }
            if (IsPlaying)
            {
                StartCoroutine(WaitEndTurn(timeCDAutoSelect));
            }

        }
        private IEnumerator WaitEndTurn(float timeWait = 1)
        {
            yield return new WaitForSeconds(timeWait);
            IsPlaying = false;
            if (gameInfo.listLine.Length <= 0)
            {
                UpdateUIBet();
            }
            if (IsTurnOnAutoPlay)
            {
                Play();
            }
        }
        private IEnumerator ShowWinPrize(GameObject pnlToShow, float timeWait = 1)
        {
            yield return new WaitForSeconds(0.3f);
            pnlToShow.SetActive(true);
            yield return new WaitForSeconds(timeWait);
            pnlToShow.SetActive(false);
        }
        private void HidePnlPotBrick(bool isActive = false)
        {
            pnlBrickPot.SetActive(isActive);
        }
        public void ChangeLastWinPrize(int totalWin)
        {
            StartCoroutine(ShowChangeLastWinPrize(totalWin));
        }
        private IEnumerator ShowChangeLastWinPrize(int totalWin)
        {
            yield return new WaitForSeconds(0.3f);
            txtLastWinPrize.text = string.Format("{0:0,0}", totalWin);
        }
        public void HandleNormalWin(int winPrize)
        {
            if (winPrize == 0)
            {
                txtWinturn.text = 0.ToString();
            }
            else
            {
                txtWinturn.text = string.Format("{0:0,0}", winPrize);
            }
            StartCoroutine(ShowWinPrize(pnlNormalWin, timeCDAutoSelect));
        }
        public void HandleBigWin(int winPrize)
        {
            txtBigWin.text = string.Format("{0:0,0}", winPrize);
            StartCoroutine(ShowWinPrize(pnlBigWin, timeCDAutoSelect));
        }
        public void HandleBreakPot(int winPrize)
        {
            IsTurnOnAutoPlay = false;
            txtBrickPot.text = string.Format("{0:0,0}", winPrize);
            StartCoroutine(ShowWinPrize(pnlBrickPot, 999f));
        }
        private void CPlayer_potchanged()
        {
            InBoundMessage res = CPlayer.res_pot;
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                string gameId = res.readString();
                if (gameId == birdManager.GameCode)
                {
                    int count0 = res.readByte();
                    for (int k = 0; k < count0; k++)
                    {
                        int bet = res.readInt();
                        int currPot = res.readInt();
                        if (bet == gameInfo.betSelected)
                        {
                            //txtpot.text = string.Format("{0:0,0}", value);
                            MoneyManager.instance.OnPotChange(txtpot, lastPot, currPot);
                            lastPot = currPot;
                        }
                    }
                    break;
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

        //private void CPlayer_changed(string type)
        //{
        //    if (type == "chip")
        //    {
        //        if (CPlayer.preChipBalance != CPlayer.chipBalance)
        //        {
        //            //txtBalance.text = string.Format("{0:0,0}", CPlayer.chipBalance);
        //            StartCoroutine(App.CorouChangeNumber(txtBalance, (int)CPlayer.preChipBalance, (int)CPlayer.chipBalance, 1f, 1.1f, 0));
        //        }
        //    }
        //}
        //private IEnumerator TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
        //{
        //    if (delay > 0)
        //        yield return new WaitForSeconds(delay);
        //    float i = 0.0f;
        //    float rate = 2.0f / tweenTime;
        //    txt.transform.DOScale(scaleNum, tweenTime);
        //    while (i < tweenTime)
        //    {
        //        i += Time.deltaTime * rate;
        //        float a = Mathf.Lerp(fromNum, toNum, i);

        //        txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
        //        if (a == toNum)
        //        {
        //            i = tweenTime;
        //        }
        //        yield return null;
        //    }
        //    //txt.transform.localScale = Vector2.one;
        //    yield return new WaitForSeconds(.05f);
        //}

        #endregion
        public void OnServerRespond(object obj)
        {
            Hashtable hash = (Hashtable)obj;
            var coutPrize = (int)hash["coutPrize"];


            birdChange.OnSendServerRespond(hash);
            /*if (coutPrize>0)
            {
                if (gameInfo.betSelected >= 1000)
                {
                    transferenceRick.SetActive(true);
                }
                else
                {
                    transferencePoor.SetActive(true);
                }
            }*/

            /* if (gameInfo.xStartMiniGame > 0)
             {
                 birdHitEgg.gameObject.SetActive(true);
                 OnOpenHitEgg();
                 //birdHitEgg.StartGameBound.Invoke(gameInfo.isTrial, gameInfo.xStartMiniGame, gameInfo.betSelected);
             }
             */
            CheckFreeSpin();
        }
        public void SetPanelTranfer(bool isStates)
        {
            /*  if (gameInfo.betSelected >= 1000)
              {
                  transferenceRick.SetActive(isStates);
              }
              else
              {
                  transferencePoor.SetActive(isStates);
              }*/
        }
        public void AutoPlay()
        {
            IsTurnOnAutoPlay = !IsTurnOnAutoPlay;
        }
        public void OnButtonClickSetting()
        {
            if (!LoadingControl.instance.loadingGojList[17].activeInHierarchy)
            {
                LoadingControl.instance.loadingGojList[17].SetActive(true);
                isShow = true;
            }
            else
            {
                LoadingControl.instance.loadingGojList[17].SetActive(false);
                isShow = false;
            }
        }
        public void Play()
        {
            myMoney = (int)CPlayer.chipBalance;
            if (myMoney < gameInfo.betSelected * gameInfo.listLine.Length && !gameInfo.isTrial)
            {
                //App.showErr("Không đủ tiền cược.");
                App.showErr(App.listKeyText["WARN_NOT_ENOUGH_GOLD"]);
                IsTurnOnAutoPlay = false;
                return;
            }
            SetPanelTranfer(false);//Set background Den
            if (IsPlaying)
            {
                return;
            }
            else
            {
                //If showing Panel Brick Pot . Hide It
                if (pnlBrickPot.gameObject.activeInHierarchy)
                {
                    HidePnlPotBrick();
                }
                IsPlaying = true;
            }
            birdChange.Play();
        }
        public void ShowGlory()
        {
            birdManager.ShowGloryBoard();
        }
        public void SelectNumberLine()
        {
            birdSelectNumLine.listLine = gameInfo.listLine;
            birdManager.SetActivePanel(BirdPanel.SelectNumLine, true);
        }
        public void Guide()
        {
            birdManager.SetActivePanel(BirdPanel.GameGuide, true);
        }
        public void History()
        {

            birdManager.ShowHistory();
            //  birdManager.SetActivePanel(BirdPanel.GameHistory, true);
        }
        public void HitEgg()
        {
            birdManager.SetActivePanel(BirdPanel.HitEgg, true);
        }
        public void Back()
        {
            if (IsPlaying)
            {
                return;
            }
            birdManager.SetActivePanel(BirdPanel.SelectBet, true);
        }


        private void ChangeStateBtnPlay(bool isActive)
        {
            btnPlay.interactable = isActive;
        }
        private void ChangeStateBtnAutoPlay(bool isActive)
        {
            gameAutoPlay.SetActive(isActive);
        }
        private void ChangeStateBtnMegaSpeed(bool isActive)
        {
        }
        private void ChangeText(Text txt, string str)
        {
            txt.text = str;
        }
        private void Update()
        {
            txtBalance.text = MoneyManager.instance.FakeCoin;
        }
    }
}