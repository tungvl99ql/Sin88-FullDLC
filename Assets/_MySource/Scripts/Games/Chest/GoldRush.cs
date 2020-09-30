using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Server.Api
{



    public class GoldRush : MonoBehaviour
    {
        //value x Begin Game
        //public int rateBegin = 1;



        bool isLag = true;
        bool isTrial;
        int countItem = 0;
        int autoTime = 15;
        int totalCoin = 0;
        bool isTakeItem = true;
        bool isPlayerBreakAuto = false;
        bool isAnimatingItem = false;
        bool isAuto = true;
        List<int> rateList = new List<int>();
        List<int> rdList = new List<int>();
        List<GameObject> itemList = new List<GameObject>();
        Coroutine corouCDTimeAuto;
        Coroutine corouPickupItemAuto;
        Coroutine corouCDZoomItem;
        Coroutine corouCDEndGoldRush;
        Coroutine corouRotate;
        /// <summary>
        /// 0: de lam gi
        /// </summary>
        GameObject currCloneItem;

        delegate void MyDelegate();
        /// <summary>
        /// 0: La bai up | 1: La bai ngua
        /// </summary>
        public Sprite[] laBaiStates;
        //UI
        [SerializeField]
        List<GameObject> goList = new List<GameObject>();
        [SerializeField]
        List<Text> txtList = new List<Text>();
        [SerializeField]
        List<RectTransform> rtfList = new List<RectTransform>();
        [SerializeField]
        List<Image> imgList = new List<Image>();
        //
        DictionaryText dictionaryText = DictionaryText.GetInstance();
        public GameObject[] listButton;
        void Start()
        {
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        }
        void OnEnable()
        {
            buttonShowUp.GetComponent<Button>().interactable = true;
            panelDis.SetActive(false);
            isTrial = ChestControl.instance.isTrial;
            isMiss = false;
            txtMoney.text = "0";
            for (int i = 0; i < rtfList[3].childCount; i++)
            {

                GameObject item = rtfList[3].GetChild(i).gameObject;
                item.SetActive(true);
                //    Debug.Log(item.transform.childCount);
                item.transform.GetChild(1).GetComponent<Text>().enabled = false;
                item.transform.GetChild(0).GetComponent<Text>().enabled = false;
                item.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                item.transform.GetChild(0).GetComponent<Text>().text = "";
                //item.GetComponent<Image>().overrideSprite = laBaiStates[0];
                item.GetComponent<Button>().enabled = true;
                item.GetComponent<Button>().interactable = true;
                item.name = i + "";
            }

        }
        private List<string> button = new List<string>();
        private void ResetGoldRush()
        {

            if (currCloneItem != null)
            {
                Destroy(currCloneItem);
                currCloneItem = null;
            }
            rtfList[0].gameObject.SetActive(false);
            rtfList[4].gameObject.SetActive(false);
            rtfList[5].gameObject.SetActive(false);
            //rtfList[6].gameObject.SetActive(false);
            rtfList[7].gameObject.SetActive(false);
            //rateBegin = 1;
            totalCoin = 0;
            countItem = 0;
            autoTime = 15;
            isAnimatingItem = false;
            isTakeItem = true;
            isPlayerBreakAuto = false;
            isAuto = true;

            rateList = new List<int>();
            rdList = new List<int>();
            itemList = new List<GameObject>();
            button.Clear();
            StopAllCoroutines();

            corouCDTimeAuto = null;

            corouPickupItemAuto = null;
            corouCDZoomItem = null;
            corouCDEndGoldRush = null;
            corouRotate = null;
        }

        private void ControllButton(bool isStates)
        {
           /* for (int i = 0; i < rtfList[3].childCount; i++)
            {
                for (int j = 0; j < button.Count; j++)
                {
                    Debug.Log(rtfList[3].GetChild(i).GetComponent<Button>().name);

                    rtfList[3].GetChild(i).GetComponent<Button>().enabled = isStates;
                    if (rtfList[3].GetChild(i).GetComponent<Button>().name.Equals(button[j]))
                    {
                        rtfList[3].GetChild(i).GetComponent<Button>().enabled = false;
                    }


                }

            }*/
        }

        public GameObject buttonShowUp;
        public GameObject panelDis;
        public void Button_ShowUp()
        {

            buttonShowUp.GetComponent<Button>().interactable = false;
            panelDis.SetActive(true);
            var ShowUpRequest = new OutBounMessage("XBOX.SHOWUP");
            ShowUpRequest.addHead();
            ShowUpRequest.writeInt(ChestControl.instance.currentXGoldRush);
            ShowUpRequest.writeInt(ChestControl.instance.currentBet);
            ShowUpRequest.writeByte(isTrial ? 1 : 0);
            Debug.Log("Muc Cuoc " + ChestControl.instance.currentBet + "XstartMini " + ChestControl.instance.currentXGoldRush);
            App.ws.send(ShowUpRequest.getReq(), delegate (InBoundMessage shoupResult)
            {
                Debug.Log("OK");
                int totalMoney = shoupResult.readInt();
                Debug.Log("Money = " + totalMoney);
                totalCoin = totalMoney;
                txtList[4].text = App.formatMoney(totalCoin.ToString());
                txtMoney.text= App.formatMoney(totalCoin.ToString());
                //stop auto pickup
                SetActivePanelNoticeAuto(false);
                StopCoroutine(corouCDTimeAuto);
                corouCDEndGoldRush = StartCoroutine(EndGoldRush());
            });

        }

        public void OpenGoldRush()
        {
            // txtList[2].gameObject.SetActive(false);

            goList[0].transform.DOScale(0f, 0.2f).OnComplete(() =>
            {
                goList[0].SetActive(false);
                goList[0].transform.localScale = Vector3.one;
            });

            SetActivePanelGoldRush(true);
            goList[1].transform.localScale = Vector3.zero;
            goList[1].transform.DOScale(1f, 0.2f);

            var req_GetInfo = new OutBounMessage("XBOX.GET_INFO");
            req_GetInfo.addHead();

            App.ws.send(req_GetInfo.getReq(), delegate (InBoundMessage res)
            {
                Debug.Log("On Start !!!");
            });
            countItem = 30;
            for (int i = 0; i < countItem; i++)
            {
                rateList.Add(0);
            }

            SerializeAppearItem();
            corouCDTimeAuto = StartCoroutine(ColdDownTimeAuto());
        }

        private IEnumerator ColdDownTimeAuto()
        {
            int currAutoTime = autoTime;
            //delay time before CD Timt Auto
            yield return new WaitForSeconds(3f);
            //show notice CD Time
            SetActivePanelNoticeAuto(true);

            while (currAutoTime > 0)
            {
                string txt = dictionaryText.TryGetValue(1);

                txtList[0].text = string.Concat(txt, currAutoTime, " s");
                yield return new WaitForSeconds(1f);
                currAutoTime--;

            }

            corouPickupItemAuto = StartCoroutine(BeginAutoPickItem());
        }
        private void StopColdDownTime()
        {
            if (corouCDTimeAuto != null)
            {
                StopCoroutine(corouCDTimeAuto);
            }
            SetActivePanelNoticeAuto(false);

        }
        private void StopAutoPickupItem()
        {
            if (corouPickupItemAuto != null)
            {
                StopCoroutine(corouPickupItemAuto);
            }
            SetActivePanelNoticeAuto(false);
        }

        private void SetActivePanelNoticeAuto(bool isActive = false)
        {
            if (isActive)
            {
                rtfList[0].gameObject.SetActive(isActive);
                rtfList[0].localScale = Vector3.zero;
                rtfList[0].DOScale(1, 0.3f);
            }
            else
            {
                rtfList[0].DOScale(0, 0.3f).OnComplete(() =>
                {
                    rtfList[0].gameObject.SetActive(isActive);
                    rtfList[0].localScale = Vector3.one;
                });
            }
        }
        private IEnumerator BeginAutoPickItem()
        {
            buttonShowUp.GetComponent<Button>().interactable = false;
            string txt = dictionaryText.TryGetValue(2);
            txtList[0].text = txt;

            MyDelegate myDelegate = new MyDelegate(PickupItem);

            for (int i = 0; i < rateList.Count; i++)
            {
                myDelegate();
                yield return new WaitForSeconds(6f);
                if (corouRotate != null)
                {
                    StopCoroutine(corouRotate);
                }
            }
        }

        private IEnumerator CDZoomItem(GameObject cloneItem, float timeCDZoom = 2f)
        {

            cloneItem.transform.SetParent(rtfList[4]);
            //setParent 2 Text into itemSelected
            AppearTextOnItemSelected(cloneItem);
            //remove onclick current on item then add new listener
            cloneItem.GetComponent<Button>().onClick.AddListener(delegate
            {
                //    Destroy(cloneItem);
                //    rtfList[4].gameObject.SetActive(false);
                //    StopCoroutine(corouCDZoomItem);
                //    StopCoroutine(corouRotate);
                ControllButton(true);
            });
            //add listener for panel
            rtfList[4].GetComponent<Button>().onClick.AddListener(delegate
            {
                //rtfList[4].gameObject.SetActive(false);
                //   if (cloneItem != null)
                //   {
                //      cloneItem.transform.SetParent(rtfList[4]);
                //  }

                // StopCoroutine(corouCDZoomItem);
                // StopCoroutine(corouRotate);
                // Destroy(cloneItem);
                ControllButton(true);
            });
            //show panel 
            rtfList[4].gameObject.SetActive(true);
            rtfList[4].DOScale(0.25f, 0.05f);

            //if player don't click on screen/panel, that auto hide after 2s
            yield return new WaitForSeconds(timeCDZoom);

            //rtfList[4].DOScale(1, 0.2f).OnComplete(() => {
            rtfList[4].gameObject.SetActive(false);
            Destroy(cloneItem);
            //});

        }
        private IEnumerator Rotate(GameObject Item)
        {
            yield return new WaitForSeconds(0);

            Item.GetComponent<Image>().transform.DOLocalRotate(new Vector3(0, 180, 0), 0.5f).OnComplete(() =>
            {
               // Item.GetComponent<Image>().overrideSprite = laBaiStates[1].Sprite;
                Item.GetComponent<Image>().transform.localRotation = Quaternion.Euler(0, 0, 0);

            });
        }

        public Text txtMoney;
        private int moneypre = 0;
        private GameObject tempCloneItem;
        private GameObject HandleItemPicked(GameObject itemPicked, string typePickup)
        {
            if (tempCloneItem != null)
            {
                tempCloneItem.gameObject.SetActive(false); 
            }
            //clone item for animating . ItemPicked static
            GameObject cloneItem = Instantiate(itemPicked, itemPicked.transform.parent);
            tempCloneItem = cloneItem;
            cloneItem.transform.SetAsLastSibling();
            Destroy(cloneItem.GetComponent<Sprite>());
            cloneItem.GetComponent<Button>().onClick.RemoveAllListeners();
            cloneItem.transform.GetChild(0).GetComponent<Text>().enabled = false;
            cloneItem.transform.GetComponent<Button>().enabled = false;
            cloneItem.GetComponent<Image>().SetNativeSize();
            // cloneItem.GetComponent<Button>().enabled=false;
            //  itemPicked.transform.GetComponent<Button>().interactable = false;
            //  itemPicked.GetComponent<Button>().interactable = false;
            //  itemPicked.GetComponent<Button>().enabled = false;

            itemPicked.transform.GetChild(0).GetComponent<Text>().enabled = false;
            //itemPicked.transform.GetChild(0).GetComponent<Text>().fontSize = 13;
            // itemPicked.transform.GetChild(0).GetComponent<Text>().text = App.formatMoney(revertMoney.ToString());
            //itemPicked.SetActive(false);
            if (currCloneItem != null)
            {
                currCloneItem = null;
            }
            currCloneItem = cloneItem;

            cloneItem.GetComponent<RectTransform>().DOLocalMove(rtfList[2].anchoredPosition, 0.4f).OnComplete(() =>
            {


            });
           
            cloneItem.transform.GetComponent<Button>().interactable = false;
            itemPicked.GetComponent<Button>().enabled = false;
            itemPicked.GetComponent<Button>().onClick.RemoveAllListeners();
            itemPicked.GetComponent<Image>().color = new Color32(255, 255, 255, 100);
            itemPicked.transform.GetChild(1).GetComponent<Text>().enabled = false;
            cloneItem.transform.GetChild(1).GetComponent<Text>().enabled = false;

            cloneItem.GetComponent<RectTransform>().transform.DOLocalRotate(new Vector3(0, 180, 0), 0.6f);
            cloneItem.GetComponent<RectTransform>().DOScale(2.5f, 0.6f).OnComplete(() =>
            {

                //StartCoroutine(Rotate(itemPicked));
                cloneItem.transform.rotation = Quaternion.identity;
                //cloneItem.GetComponent<Image>().overrideSprite = laBaiStates[1].Sprite;
                itemPicked.transform.GetComponent<Button>().interactable = false;
                setInteractable(true);

                //cloneItem.GetComponent<RectTransform>().transform.DOLocalRotate(new Vector3(0, 90, 0), 0.5f).OnComplete(() =>
                //  {


                // cloneItem.GetComponent<Image>().overrideSprite = laBaiStates[1];
                // cloneItem.GetComponent<RectTransform>().transform.localRotation = Quaternion.Euler(0, 0, 0);

                //cloneItem.transform.GetChild(0).GetComponent<Text>().fontSize = 13;
                //  cloneItem.transform.GetChild(0).GetComponent<Text>().text = App.formatMoney(revertMoney.ToString());


                if (isMiss)
                {
                    // itemPicked.transform.GetChild(1).GetComponent<Text>().enabled = true;
                    cloneItem.transform.GetChild(1).GetComponent<Text>().enabled = true;
                }
                else
                {
                    cloneItem.transform.GetChild(0).DOScale(2.5f, 0.1f);
                    cloneItem.transform.GetChild(1).DOScale(2.5f, 0.1f);
                    cloneItem.transform.GetChild(0).GetComponent<Text>().enabled = true;
                    cloneItem.transform.GetChild(2).GetComponent<Text>().enabled = true;
                }
                //   itemPicked.transform.GetComponent<Button>().interactable = false;
                // itemPicked.transform.GetChild(0).GetComponent<Text>().enabled = true;
                //cloneItem.transform.GetChild(0).GetComponent<Text>().enabled = true;
                //cloneItem.transform.GetChild(2).GetComponent<Text>().enabled = true;
                // setInteractable(true);
                cloneItem.transform.GetComponent<Button>().interactable = true;
                cloneItem.transform.GetComponent<Button>().enabled = true;
                cloneItem.GetComponent<Button>().onClick.AddListener(delegate
            {
                Destroy(cloneItem);
                rtfList[4].gameObject.SetActive(false);
                StopCoroutine(corouCDZoomItem);
                StopCoroutine(corouRotate);

            });
                //add listener for panel
                rtfList[4].GetComponent<Button>().onClick.AddListener(delegate
                {
                    /* Destroy(cloneItem);
                     rtfList[4].gameObject.SetActive(false);
                      if (cloneItem != null)
                        {
                          cloneItem.transform.SetParent(rtfList[4]);
                      }

                     StopCoroutine(corouCDZoomItem);
                   StopCoroutine(corouRotate);*/

                    // });

                });

                var text1 = cloneItem.transform.GetChild(0).GetComponent<Text>();
                text1.text = App.formatMoney(revertMoney.ToString());
                text1.gameObject.SetActive(true);
                var text2 = cloneItem.transform.GetChild(2).GetComponent<Text>();
                text2.text = ChestControl.instance.currentXGoldRush.ToString();
                text2.gameObject.SetActive(true);
                cloneItem.GetComponent<Button>().enabled = true;

                txtMoney.text = string.Format("{0:0,0}", totalCoin);
                StartCoroutine(TweenNum(txtMoney, moneypre, totalCoin, 1f, 1f));
                txtList[4].text = App.formatMoney(totalCoin.ToString());
                moneypre = totalCoin;
            });




            // Debug.Log(isMiss);

            //   setInteractable(false);
            isPick = false;

            isAnimatingItem = false;
            if (typePickup == TypePickup.auto.ToString())
            {
                corouCDZoomItem = StartCoroutine(CDZoomItem(cloneItem));
            }
            else
            {
                corouCDZoomItem = StartCoroutine(CDZoomItem(cloneItem, 999f));
            }

            return cloneItem;
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
        private void PickupItem()
        {
            buttonShowUp.GetComponent<Button>().interactable = false;
            if (isTakeItem)
            {
                setInteractable(false);
                isTakeItem = false;
                // setInteractable(true);
            }
            else
            {
                return;
            }
            GameObject itemPicked = RemoveItemOnScreen();

            SendRequestAndGetResponse(itemPicked);
        }
        private int revertMoney = 0;
        private bool isMiss = false;
        private void SendRequestAndGetResponse(GameObject item, TypePickup typePickup = TypePickup.auto)
        {

            // txtList[2].gameObject.SetActive(true);
            ///txtList[2].text = "0";
            var req = new OutBounMessage("XBOX.START");
            req.addHead();
            //muc cuoc 1000
            int currBet = ChestControl.instance.currentBet;
            req.writeInt(ChestControl.instance.currentBet);
            //Game Code
            req.writeString("xbox");
            //gia tri nhan ban dau
            int xStart = ChestControl.instance.currentXGoldRush;
            req.writeInt(xStart);
            req.writeByte(isTrial ? 1 : 0);
            //ten Item
            //int rate = GetRateForItem();

            //req.writeString(String.Concat("X", rate));
            //ty le x
            //req.writeInt(rate);

            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                //if (isLag)
                //{
                //    return;
                //}
                //  Debug.Log("On Respond");
                int totalMoney = res.readInt();
                //  Debug.Log(totalMoney);
                revertMoney = 0;
                //
                revertMoney = totalMoney;
                totalCoin += totalMoney;
                if (totalMoney <= currBet * xStart)
                {
                    isMiss = true;
                }
                setInteractable(false);
                //Hien thi tong tien sau khi nhan tu server
                HandleItemPicked(item, typePickup.ToString());
                //
                ChangeTextItemSelected(null, 0, totalMoney);
                //before add money by take item . check rate
                if (totalMoney <= currBet * xStart)
                {
                    isTakeItem = false;
                    //update Miss Panel
                    isMiss = true;
                    rtfList[7].gameObject.SetActive(true);

                    if (typePickup == TypePickup.auto)
                    {
                        corouCDEndGoldRush = StartCoroutine(EndGoldRush());
                    }
                    else
                    {
                        corouCDEndGoldRush = StartCoroutine(EndGoldRush());
                    }
                }
                else
                {
                    isTakeItem = true;
                }
            });
        }
        private GameObject RemoveItemOnScreen()
        {
            buttonShowUp.GetComponent<Button>().interactable = false;
            //list index active in hierachy
            List<int> idItemActiveList = new List<int>();
            int countItem = itemList.Count;
            for (int i = 0; i < countItem; i++)
            {
                if (itemList[i].activeInHierarchy)
                {
                    idItemActiveList.Add(i);
                }
            }
            //random
            int rd = UnityEngine.Random.Range(0, idItemActiveList.Count);
            return itemList[idItemActiveList[rd]];
        }

        private void ChangeTextItemSelected(GameObject itemSelected, int rate, int totalAddCoin)
        {
            corouRotate = StartCoroutine(RotateGameobject());
            StartCoroutine(AnimationNumber(totalAddCoin, txtList[2]));
            //txtList[2].text = String.Format("{0:0,0,0}", totalAddCoin);
        }
        private IEnumerator RotateGameobject()
        {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < 5000; i++)
            {
                yield return new WaitForEndOfFrame();
                //  rtfList[2].Rotate(new Vector3(rtfList[2].rotation.x, rtfList[2].rotation.y, rtfList[2].rotation.z + 5f));
            }
        }

        private void AppearTextOnItemSelected(GameObject cloneItem)
        {
            txtList[1].transform.parent.SetParent(cloneItem.transform.parent);
            txtList[2].transform.parent.SetParent(cloneItem.transform.parent);
        }
        public GameObject panel;
        private void setInteractable(bool states)
        {
            //  panel.SetActive(states);
            for (int i = 0; i < listButton.Length; i++)
            {

                listButton[i].GetComponent<Button>().interactable = states;
                // Debug.Log(rtfList[3].GetChild(i).gameObject.GetComponent<Button>().enabled);
            }

        }
        private bool isPick = false;
        private void SerializeAppearItem()
        {
            isPick = false;
            //  setInteractable(false);
            if (rtfList[3].childCount < countItem)
            {
                return;
            }
            setInteractable(true);
            for (int i = 0; i < rtfList[3].childCount; i++)
            {

                GameObject item = rtfList[3].GetChild(i).gameObject;
                if (!item.activeInHierarchy)
                {
                    item.SetActive(true);
                    //item.GetComponent<Image>().overrideSprite = laBaiStates[0].Sprite;
                    item.GetComponent<Button>().interactable = true;

                }
                item.name = i.ToString();
                itemList.Add(item);
                item.GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    buttonShowUp.GetComponent<Button>().interactable = false;
                    //if (!isPick)
                    //  {
                    //    isPick = true;


                    if (!isTakeItem)
                    {
                        return;
                    }

                    //check animating first
                    if (isAnimatingItem)
                    {
                        return;
                    }
                    else
                    {
                        isAnimatingItem = true;

                        //below stop when player pickup item
                        //if (!isTakeItem)
                        //{
                        //    isTakeItem = true;
                        //stop auto pickup when player pickup
                        StopColdDownTime();
                        StopAutoPickupItem();
                        //}
                        //else
                        //{
                        //    return;
                        //}
                        ControllButton(false);
                        button.Add(item.ToString());
                        SendRequestAndGetResponse(item, TypePickup.manual);
                    }
                    // }
                });

            }
        }

        private IEnumerator EndGoldRush(float timeWait = 7f)
        {
            //stop auto pickup
            if (corouPickupItemAuto != null)
            {
                StopColdDownTime();
                StopAutoPickupItem();
            }
            string txt;

            for (int i = (int)timeWait; i > 0; i--)
            {
                if (i == 4)
                {
                    rtfList[4].gameObject.SetActive(false);
                    SetActivePanelEnd(true);
                }

                rtfList[5].gameObject.SetActive(true);
                txt = DictionaryText.instance.TryGetValue(3);
                txtList[3].text = string.Concat(txt, i, " s");
                yield return new WaitForSeconds(1f);
            }
            rtfList[0].gameObject.SetActive(true);

            ResetGoldRush();
            SetActivePanelGoldRush();
        }
        private void SetActivePanelEnd(bool isActive = true)
        {
            //rtfList[6].gameObject.SetActive(isActive);
            if (isActive)
            {
                StartCoroutine(AnimationNumber(totalCoin, txtList[4]));
            }
        }
        private IEnumerator AnimationNumber(int number, Text txt)
        {
            yield return new WaitForEndOfFrame();
            /* for (int i = 25; i > 1; i--)
             {
                 int newInt = number / i;
                 txt.text = String.Format("{0:0,0,0}", newInt);
                 yield return new WaitForEndOfFrame();
             }
             txt.text = String.Format("{0:0,0,0}", number);*/
        }
        private void SetActivePanelGoldRush(bool isActive = false)
        {
            ChestControl.instance.AllowSpin();
            if (isActive == false && ChestControl.instance.currentAutoSpin)
            {
                Invoke("Spin", 1f);
            }
            goList[1].SetActive(isActive);
            //active mini Game
            LoadingControl.instance.loadingGojList[21].SetActive(!isActive);

        }

        private void Spin()
        {
            ChestControl.instance.boolLs[2] = true;
            ChestControl.instance.currentAutoSpin = false;
            ChestControl.instance.NewSpin();
        }
        private int GetRateForItem()
        {
            int rd = UnityEngine.Random.Range(0, rateList.Count);
            int rate = rateList[rd];
            rateList.RemoveAt(rd);
            //return rate x 
            return rate;
        }

        enum TypePickup
        {
            auto,
            manual
        }

    }
}