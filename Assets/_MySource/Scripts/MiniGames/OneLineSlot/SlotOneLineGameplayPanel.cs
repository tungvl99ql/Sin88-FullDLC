using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using Core.Server.Api;

namespace Casino.Games.OneLineSlot
{
    public class SlotOneLineGameplayPanel : MonoBehaviour
    {


        public Spinner spinner;
        public CustomToggleGroup betLevelToggleGroup;
        public Toggle autoRunModeToggle;
        public Toggle autoToggle;
        public Selectable[] controllers;
        public Text receiveMoney;
        public Text txtBigWin, txtJackPost;
        public Text[] betLevelsTxt;
        public Image[] toggleControll;
        public Toggle[] betLevelToggles;
        private bool isHot;
        private bool isBig;

        public MiniGameOneLineSlot game;
        public Button btnClose, btnRun;
        private bool isExitNoMoney=false;
      //  public Animator animator;
        public GameObject panelIsHot, panelIsBig;
        private bool isPlaying = false;
        private string money = "";

        private bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;




                if (autoRunMode)
                {
                    btnRun.GetComponentsInChildren<Image>()[1].enabled = !autoRunMode;
                    autoRunModeToggle.GetComponentsInChildren<Image>()[1].enabled = autoRunMode;
                    setController(false);
                }
                else
                {
                    autoRunModeToggle.GetComponentsInChildren<Image>()[1].enabled = autoRunMode;

                    btnRun.GetComponentsInChildren<Image>()[1].enabled = IsPlaying;
                    setController(!isPlaying);
                }

            }
        }

        private void setController(bool isAvaiable)
        {
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].interactable = isAvaiable;
            }
            autoToggle.interactable = isAvaiable;
            if (autoRunMode)
                autoToggle.interactable = autoRunMode;




        }

        private bool autoRunMode = false;


        public void setButtonClose(bool isVailable)
        {
            btnClose.interactable = isVailable;
        }
        void Start()
        {
            //game = GetComponentInParent<MiniGameOneLineSlot> ();

            receiveMoney.text = "";
            isPlaying = false;
            disablePanel();

        }

        void OnEnable()
        {
            isExitNoMoney = false;
            receiveMoney.text = "";
            txtBigWin.text = "";
            txtJackPost.text = "";
            disablePanel();
            IsPlaying = false;
            autoRunMode = false;
            game.receiveResultEvent += onReceiveResultEvent;
            spinner.spinnerSendRequetEvent.AddListener(OnSpinnerSendServerRequest);
            spinner.spinnerStartEvent.AddListener(OnSpinnerStart);
            spinner.spinnerFinishEvent.AddListener(OnSpinnerFinish);
            autoRunModeToggle.onValueChanged.AddListener(OnSpinAutoRunModeChange);
            betLevelToggleGroup.OnChangeSelect += onBetLevelChanged;

        }

        void OnDisable()
        {
            IsPlaying = false;
            autoRunMode = false;
            onBetLevelChanged(0);
            game.receiveResultEvent -= onReceiveResultEvent;
            spinner.spinnerSendRequetEvent.RemoveListener(OnSpinnerSendServerRequest);
            spinner.spinnerStartEvent.RemoveListener(OnSpinnerStart);
            spinner.spinnerFinishEvent.RemoveListener(OnSpinnerFinish);
            autoRunModeToggle.onValueChanged.RemoveListener(OnSpinAutoRunModeChange);
            betLevelToggleGroup.OnChangeSelect -= onBetLevelChanged;

        }

        public void OnSpinAutoRunModeChange(bool arg0)
        {

            autoRunMode = autoRunModeToggle.isOn;
            btnRun.interactable = !autoRunMode;
            autoRunModeToggle.GetComponentsInChildren<Image>()[1].enabled = autoRunMode;
            Spin();

        }

        public void Reset()
        {
            receiveMoney.GetComponentInParent<Image>().enabled = false;
            receiveMoney.text = "";
            spinner.ForceStop();
        }
        public void ShowGlory()
        {
            game.ShowGloryBoard();

        }

        public void ShowHistory()
        {
            game.ShowHistory();
        }


        public void ShowGuide()
        {
            game.ShowTutorial();
        }

        public void Close()
        {
            if (!IsPlaying || !autoRunMode)
                game.CloseGame();
        }

        // call Spin method of Spin Component
        public void Spin()
        {
            if (isExitNoMoney)
            {
                btnClose.interactable = true;
                return;
            }
            if (!IsPlaying )//|| autoRunMode)
            {
                disablePanel();
                IsPlaying = true;
                spinner.Spin();

            }
        }
        // call to set AutoSpin Property of Spinner Component
        public void SetAuto()
        {
        
            autoRunMode = !autoRunMode;
               if (IsPlaying)
               return;
            if (!IsPlaying)
            {
              
                Spin();
            }
        }
        // Event handler of Spinner SendServerRequest event
        public void OnSpinnerSendServerRequest()
        {
            game.Play();

        }

        // Event handler of Spinner Send
        public void OnSpinnerStart()
        {
            setControllerAvailable(false);
        }

        // Event Hanlder when spinner finish spin
        public void OnSpinnerFinish()
        {
            StartCoroutine(ShowSpinnerFinish());

        }
        private void disablePanel()
        {
          //  animator.SetBool("IsSmile", false);
            panelIsBig.SetActive(false);
            panelIsHot.SetActive(false);
            receiveMoney.GetComponentInParent<Image>().enabled = false;
            receiveMoney.text = "";
        }
        public void TurnExitButton(bool isAvailable)
        {
            IsPlaying = !isAvailable;
            autoRunMode = !isAvailable;
            btnClose.interactable = isAvailable;
            isExitNoMoney = true;
        }


        IEnumerator ShowSpinnerFinish()
        {
            if (money.Length != 0)
            {
                receiveMoney.GetComponentInParent<Image>().enabled = true;
                receiveMoney.text = money;
                txtBigWin.text = money;
                txtJackPost.text = money;
            }
       

            if (isHot)
            {
              //  animator.SetBool("IsSmile", true);
                panelIsHot.SetActive(true);
                yield return new WaitForSeconds(5f);
            }
            else
            if (isBig)
            {
               // animator.SetBool("IsSmile", true);
                panelIsBig.SetActive(true);
                SoundManager.instance.PlayUISound(SoundFX.BIG_WIN + "_" + UnityEngine.Random.Range(1,4));
                yield return new WaitForSeconds(5f);
            }
           // animator.SetBool("IsSmile", false);
            yield return new WaitForSeconds(1.5f);

            if (!isHot)
            {
                if (autoRunMode)
                {
                    setControllerAvailable(false);
                    IsPlaying = false;
                    Spin();
                }
                else
                {

                    IsPlaying = false;
                    disablePanel();

                    setControllerAvailable(true);
                }
            }
            else
            {
                btnClose.interactable = true;
                IsPlaying = false;
                
                autoRunModeToggle.GetComponentsInChildren<Image>()[1].enabled = false;

                btnRun.GetComponentsInChildren<Image>()[1].enabled = false;
            
                panelIsHot.SetActive(true);
                setControllerAvailable(true);

            }


        }
        private void setControllerAvailable(bool isAvailable)
        {
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].interactable = isAvailable;
            }
            for (int i = 0; i < betLevelToggles.Length; i++)
            {
                betLevelToggles[i].interactable = isAvailable;
            }
        }

        public void UpdateBetLevel()
        {

            for (int i = 0; i < betLevelsTxt.Length; i++)
            {
                betLevelsTxt[i].text = App.formatMoneyK(((OneLineSlotInfo)game.GameInfo).betLevels[i], false);
            }
            for (int i = 0; i < toggleControll.Length; i++)
            {

                //betLevelsTxt[i].color = new Color32(107, 153, 153, 255);//UnActive
                toggleControll[i].enabled = false;

            }
           // betLevelsTxt[0].color = new Color32(254, 235, 145, 255);//Active
            toggleControll[0].enabled = true;
        }


        private void onReceiveResultEvent(Hashtable data)
        {


            money = data["totalMoney"].ToString() == "0" ? "" : data["totalMoney"].ToString();
            if (money.Length > 0)
            {
                money = App.formatMoney(money);
            }
            isHot = (bool)data["jackpot"];
            isBig = (bool)data["bigWin"];
            if (isHot)
            {
                autoRunMode = false;
            }

            // TODO: call spinner ReceiveServerResult base on the received data


            //			data.Add ("item", item);
            //			data.Add ("totalMoney", totalMoney);
            //			data.Add ("positionWin", positionWin);
            //			data.Add ("bigWin", bigWin);
            //			data.Add ("jackpot", jackpot);


            spinner.ReceiveServerResult(data);



        }

        private void onBetLevelChanged(int idx)
        {

            isExitNoMoney = false;
            for (int i = 0; i < toggleControll.Length; i++)
            {

              //  betLevelsTxt[i].color = new Color32(107, 153, 153, 255);
                toggleControll[i].enabled = false;

            }
           // betLevelsTxt[idx].color = new Color32(254, 235, 145, 255);
            toggleControll[idx].enabled = true;
            ((OnLineSlotBetData)((OneLineSlotInfo)(game.GameInfo)).BetData).selectedBetLevel = ((OneLineSlotInfo)(game.GameInfo)).betLevels[idx];


        }


    }

}


