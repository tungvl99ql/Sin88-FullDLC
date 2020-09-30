using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using Core.Server.Api;

namespace Core.Bird
{
    public class SelectBet : MonoBehaviour
    {
        public static SelectBet instanse;
        [SerializeField] BirdManager birdManager;
        public bool isTrial = false;
        public int betSelected = 100;
        private int[] listBet;
        [SerializeField] Text txt1;
        [SerializeField] Text txt2;
        [SerializeField] Text txt3;
        [SerializeField] Text txt4;
        [SerializeField] Button btn1;
        [SerializeField] Button btn2;
        [SerializeField] Button btn3;
        [SerializeField] Button btn4;

        public UnityEvent OpenSelectBetEvent;
        public UnityEvent CloseSelectBetEvent;
        public void Awake()
        {
            if(instanse==null)
            {
                instanse = this;
            }
            else
            {
                Destroy(instanse);
            }
        }
        public void OnEnable()
        {
            //send RUONGBAU.GET_INFO;
            //birdManager.OnJoinGame(birdManager.JoinGameCommand);
            if (CloseSelectBetEvent != null)
            {
                OpenSelectBetEvent.Invoke();
            }
        }
        public void OnDisable()
        {
            if (CloseSelectBetEvent != null)
            {
                CloseSelectBetEvent.Invoke();
            }
        }
        public void GetResult(object obj)
        {
            int[] listBet = (int[])obj;

            txt1.text = App.formatMoneyK(listBet[0], false);
            txt2.text = App.formatMoneyK(listBet[1], false);
            txt3.text = App.formatMoneyK(listBet[2], false);
            txt4.text = App.formatMoneyK(listBet[3], false);

            btn1.onClick.AddListener(() =>
            {
                SelectAmountBet(listBet[0]);
            });
            btn2.onClick.AddListener(() =>
            {
                SelectAmountBet(listBet[1]);

            });
            btn3.onClick.AddListener(() =>
            {
                SelectAmountBet(listBet[2]);

            });
            btn4.onClick.AddListener(() =>
            {
                SelectAmountBet(listBet[3]);

            });
        }
        private void SelectAmountBet(int bet)
        {
            Debug.Log("---Bet---" + bet);
            betSelected = bet;
            isTrial = false;
            birdManager.SetActivePanel(BirdPanel.SelectBet, false);
        }
        public void OnClickBack()
        {
            BackToLobby();
        }
        public void OnClickTrial()
        {
            isTrial = true;
            betSelected = 10000;
            birdManager.SetActivePanel(BirdPanel.SelectBet, false);
        }
        private void BackToLobby()
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
                LoadingControl.instance.asbs[0].Unload(true);
            }
            catch (Exception)
            {

            }
            SceneManager.LoadScene("Lobby");
        }
    }
}