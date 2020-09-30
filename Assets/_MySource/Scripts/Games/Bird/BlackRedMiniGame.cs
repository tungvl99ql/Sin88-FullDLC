using DG.Tweening;
using Core.Server.Api;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Core.Bird
{
  
    public class BlackRedMiniGame : MonoBehaviour
    {
        
        public BlackRed game;
        public GameObject[] Eggs;
        private int posstion=0;
        private bool isOpen = false;
        public Text txtAuto;
        private int total = 0;
        public Text txtTotal;
        public Text Money;
        public Button[] buttonEgg;
        public bool isPlay = false;
        private bool isFistTime = true;
        public Text[] text;
        private Material materialSkeletonSpine;
        public GameObject autoTime, totalMoneyEnd;
        /// <summary>
        /// 0: idle 1: khong xac uop   2: xac uop
        /// </summary>
        private string []statesMuni= {"xac uop idle" ,"khong xac uop","xac uop"};
        private void Awake()
        {
            
        }
        private void Start()
        {
            if (isFistTime)
            {
                autoTime.SetActive(false);
                totalMoneyEnd.SetActive(false);
                isFistTime = false;
                game.gameObject.SetActive(false);
                return;
            }
        }
        bool isStart = false;

        private int lastPotValue = 0;
        private void OnEnable()
        {
            if (isFistTime)
            {
                return;
            }

            isStart = false;
            Loaded();
            Init();

            game.receiveResultEvent += onReceiveResultEvent;
            game.showUpResultEvent += onShowUpResultEvent;

        }


        
   
        private void Init()
        {
            //  materialSkeletonSpine = Resources.Load("Material/SkeletonGraphicDefault", typeof(Material)) as Material;
            /*int leght = Eggs.Length;
            for (int i = 0; i < leght; i++)
            {
                // Eggs[i].GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
              
            }*/
            autoTime.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            int count = text.Length;
            for (int i = 0; i < count; i++)
            {
                //   Eggs[i].transform.GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
                //  Eggs[i].transform.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, statesMuni[0], false);
                text[i].text = "";
            }
            lastPotValue = 0;
            time = 15;
            total = 0;
            timeout = false;
            isOpen = false;
            txtTotal.enabled = true;
            txtTotal.text = "";
            Money.text = "0";
            txtAuto.enabled = true;
            isPlay = false;
            buttonShowup.GetComponent<Button>().interactable = true;
            StartCoroutine(UpdateTime());


        }
        private void OnDisable()
        {

            StopCoroutine(UpdateTime());
            autoTime.SetActive(false);
            totalMoneyEnd.SetActive(false);
            game.receiveResultEvent -= onReceiveResultEvent;
            game.showUpResultEvent -= onShowUpResultEvent;
        }
      
        private void UpdateMoney(string str)
        {
            // Money.text =App.formatMoney(str);
            int value = int.Parse(str);
            Money.text = string.Format("{0:0,0}", value);
            StartCoroutine(TweenNum(Money, lastPotValue, value, 1f, 1f));
            lastPotValue = value;
    
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
            txt.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(.05f);
        }

        private void onReceiveResultEvent(Hashtable receiveData)
        {
            StartCoroutine(ReceiveResult(receiveData));
        }
        private void onShowUpResultEvent(int receiveData)
        {
            StartCoroutine(ShowUpResult(receiveData));
        }
        private void SetTextTime(int time)
        {

            txtAuto.text = "Hệ thống sẽ tự động mở sau " + time+" giây";
        }
        private int time = 15;
        private bool isShowUp = false;
        private bool timeout = false;
        private void Update()
        {
            if (isFistTime)
            {
                return;
            }

            if (time == 0 && !isOpen&&timeout)
            {
                int x = UnityEngine.Random.Range(0, 4);
                while (x == posstion)
                    x = UnityEngine.Random.Range(0, 4);
                Choice(x);

            }
        }
        IEnumerator UpdateTime()
        {
            yield return new WaitForSeconds(1f);
            if(time>0&&!isOpen)
            {
                if(!isShowUp)
                autoTime.SetActive(true);
                time--;
                if (time == 0)
                    timeout = true;
                SetTextTime(time);
            }
          if(time == 0)
            {
                autoTime.SetActive(false);
                txtAuto.enabled = false;
            }
          

           
          //  Debug.Log(timeout + " " + isOpen + " " );
           
            if (game.isPlayFird)
            {
               StartCoroutine(UpdateTime());
            }
            else
            {
                txtAuto.enabled = false;
                if(timeout && !isOpen)
                {
                   int x = UnityEngine.Random.Range(0, 4);
                    while (x == posstion)
                       x = UnityEngine.Random.Range(0, 4);
                    Choice(x);
                }
               StartCoroutine(UpdateTime());
            }
           // StartCoroutine(UpdateTime());
        }


        private IEnumerator ReceiveResult(Hashtable data)
        {
            string xBonus = (string)data["xBonus"];
            int goldBonus = (int)data["goldBonus"];


            total += goldBonus;
            isOpen = true;
            time = 0;
            // Debug.Log("xBonus  => "+xBonus);
            if (!xBonus.Equals("BLACK"))
            {
                if (posstion >= 0)
                {
                    //    Eggs[posstion].transform.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, statesMuni[2], false);
                    Eggs[posstion].GetComponent<RectTransform>().transform.DOLocalRotate(new Vector3(0, 90, 0), 0.5f).OnComplete(() =>
                    {
                        text[posstion].text = App.formatMoney(goldBonus.ToString());

                    });


                }
                yield return new WaitForSeconds(1.5f);
                Eggs[posstion].GetComponent<RectTransform>().transform.DOLocalRotate(new Vector3(0, 0, 0), 0.0f).OnComplete(() =>
            {
                text[posstion].text = "";
            });
                // yield return new WaitForSeconds((Eggs[posstion].transform.GetComponent<SkeletonGraphic>().AnimationState.TimeScale * 1.333f));
                //Eggs[posstion].transform.GetComponent<SkeletonGraphic>().AnimationState.TimeScale;

                UpdateMoney(total.ToString());
                yield return new WaitForSeconds(1);
                Loaded();
            }
            else
            {

                if (posstion >= 0)
                {
                    //  Eggs[posstion].transform.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, statesMuni[1], false);

                    Eggs[posstion].GetComponent<RectTransform>().transform.DOLocalRotate(new Vector3(0, 90, 0), 0.5f).OnComplete(() =>
                    {
                        text[posstion].text = "MISS";

                    });



                    yield return new WaitForSeconds(1.5f);
                    Eggs[posstion].GetComponent<RectTransform>().transform.DOLocalRotate(new Vector3(0, 0, 0), 0.0f).OnComplete(() =>
                    {
                        text[posstion].text = "";
                    });

                }
                yield return new WaitForSeconds(0.5f);
                //UpdateMoney(total.ToString());
                // yield return new WaitForSeconds(1f);
                autoTime.SetActive(false);
                totalMoneyEnd.SetActive(true);
                txtTotal.text = "TỔNG SỐ TIỀN BẠN NHẬN: " + App.formatMoney(total.ToString());
                txtAuto.enabled = false;
                yield return new WaitForSeconds(5f);
                game.EndGameBound.Invoke();
                game.CloseGame();
                Debug.Log("End");
            }
            isPlay = false;


        }
        private void DisableButton(bool states)
        {
            for(int i = 0; i < buttonEgg.Length; i++)
            {
                buttonEgg[i].interactable=states;
            }
        }
        private void Loaded()
        {
            txtAuto.enabled = true;
            txtAuto.text = "";
      
            for (int i = 0; i < text.Length; i++)
            {
                //  Eggs[i].transform.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, statesMuni[0], false);
                text[i].text = "";
            }
           // time = 15;
            DisableButton(true);
     
        isOpen = false;
        }
        private IEnumerator ShowUpResult(int totalMoney)
        {
            //StopCoroutine(UpdateTime());
            totalMoneyEnd.SetActive(true);
            txtTotal.text = "TỔNG SỐ TIỀN BẠN NHẬN: " + App.formatMoney(totalMoney.ToString());
            txtAuto.enabled = false;
            autoTime.SetActive(false);
            autoTime.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
            yield return new WaitForSeconds(5f);
            game.EndGameBound.Invoke();
            game.CloseGame();
            Debug.Log("End");
        }
        public GameObject buttonShowup;

        public void Button_ShowUp()
        {
            //  if (!isShowUp)
            //   return;
            buttonShowup.GetComponent<Button>().interactable = false;
            DisableButton(false);
            game.ShowUp();
        }
        public void Choice(int vt)
        {
            if (isShowUp)
                return;
            buttonShowup.GetComponent<Button>().interactable = false;
            //UpdateMoney("0");
            if (!isPlay)
            {
                isPlay = true;
                SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                DisableButton(false);
                posstion = vt;
               
                game.Play();

            }
        }
    }

}