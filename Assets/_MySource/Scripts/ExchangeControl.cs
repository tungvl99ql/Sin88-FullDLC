using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

namespace Core.Server.Api
{

    public class ExchangeControl : MonoBehaviour
    {

        /// <summary>
        /// 0: Mid-rule|1: Mid-card|2: card-telco-ele|3: card-telco-content-ele|4: Mid-His|5: Mid-his-ele|8: ele-policy
        /// |9: mid-shop
        /// </summary>
        public GameObject[] ecGojs;

        /// <summary>
        /// 0: Header|1: BtnBack|2: Mid-rule|3: Mid-card|4: Mid-His|5: Mid-List|6: Mid-Shop
        /// </summary>
        public RectTransform[] ecRtfs;
        /// <summary>
        /// 0: vtt|1: vnp|2: mobi|3: btn-grey
        /// </summary>
        public Sprite[] rcSprList;
        /// <summary>
        /// 0: policy-des
        /// </summary>
        public Text[] rcTxtList;
        /// <summary>
        /// 0: midCard-seri|1: midCard-cardcode|2: mid-giftcode
        /// </summary>
        public InputField[] rcIpfList;
        /// <summary>
        /// 0: Ex|1: His|2: list|3: rule|4: shop
        /// </summary>
        public Toggle[] ecTogs;
        private int currContent = -1;
        private Dictionary<string, Toggle> togList = new Dictionary<string, Toggle>();
        /// <summary>
        /// CodeName of current tog
        /// </summary>
        private string currTogId = "";
        private string currMidCardTogId = "";        //current tog of sms plus content
        private string currCardTogCode = "";
        private List<string> MidCardData = new List<string>();
        private Dictionary<string, ArrayList> cardDataDic = new Dictionary<string, ArrayList>();
        private List<Transform> midCardContentEleRtf = new List<Transform>();
        private Dictionary<string, string> teleFullName = new Dictionary<string, string>();
        private enum RechargeGojElement : int
        {
            sms = 4
        }

        private void OnEnable()
        {



            togList.Clear();
            togList.Add("RULE", ecTogs[3]);
            togList.Add("HIS", ecTogs[1]);
            togList.Add("LIST", ecTogs[2]);
            togList.Add("CARD", ecTogs[0]);
            togList.Add("SHOP", ecTogs[4]);

            _changeContent(ecTogs[0], "CARD");
            _changeContent(ecTogs[1], "HIS");
            _changeContent(ecTogs[2], "LIST");
            _changeContent(ecTogs[3], "RULE");
            _changeContent(ecTogs[4], "SHOP");

            currContent = -1;
            currTogId = "";
            getHeader();
        }

        private void getHeader()
        {

            DOTween.To(() => ecRtfs[1].anchoredPosition, x => ecRtfs[1].anchoredPosition = x, new Vector2(0, 0), .35f);
            DOTween.To(() => ecRtfs[0].anchoredPosition, x => ecRtfs[0].anchoredPosition = x, new Vector2(0, 0), .35f);
            changeContent("CARD");
            return;
            // OutBounMessage req_CASH_OUT = new OutBounMessage("CASH_OUT.GET_DATA");
            // req_CASH_OUT.addHead();
            // App.ws.send(req_CASH_OUT.getReq(), delegate (InBoundMessage res_CASH_OUT)
            // {
            // //App.trace("[RECV] CASH_OUT.GET_DATA");
            // var telcoCount = res_CASH_OUT.readByte();
            //     for (int i = 0; i < telcoCount; i++)
            //     {
            //         var code = res_CASH_OUT.readAscii();
            //         var name = res_CASH_OUT.readString();
            //     //App.trace("code = " + code + "name = " + name);
            // }
            //     DOTween.To(() => ecRtfs[1].anchoredPosition, x => ecRtfs[1].anchoredPosition = x, new Vector2(0, 0), .35f);
            //     DOTween.To(() => ecRtfs[0].anchoredPosition, x => ecRtfs[0].anchoredPosition = x, new Vector2(0, 0), .35f);
            //     changeContent("CARD");
            // });
        }

        /// <summary>
        /// Add event for header tog of mid-card
        /// </summary>
        /// <param name="tog"></param>
        /// <param name="act"></param>
        private void _changeContent(Toggle tog, string act)
        {
            tog.onValueChanged.AddListener(delegate (bool isOn)
            {
                if (isOn)
                {
                    changeContent(act);
                    foreach (Toggle item in togList.Values.ToList())
                    {
                        item.interactable = true;
                    }
                    tog.interactable = false;
                }
            });
        }

        public void changeContent(string idToChange)
        {
            if (currTogId == idToChange)    //Prevent if user press actived tog
                return;

            LoadingControl.instance.loadingGojList[9].SetActive(true);  //Prevent touch while sliding content panel
            currTogId = idToChange;
            //App.trace("TOG SELECATED = " + currTogId);
            togList[idToChange].isOn = true;
            togList[idToChange].transform.GetComponent<ToggleController>().ChangeFont();
            switch (idToChange)
            {
                case "RULE":
                    if (currContent != -1)
                        DOTween.To(() => ecRtfs[currContent].anchoredPosition, x => ecRtfs[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                        {
                            ecRtfs[currContent].gameObject.SetActive(false);

                        });
                    ecRtfs[2].anchoredPosition = new Vector2(1500, 0);
                    ecGojs[0].SetActive(true);
                    DOTween.To(() => ecRtfs[2].anchoredPosition, x => ecRtfs[2].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                    {
                        currContent = 2;
                        LoadingControl.instance.loadingGojList[9].SetActive(false);
                        LoadRechargeData("POLICY");
                    });

                    break;

                case "CARD":
                    currMidCardTogId = "";      //Reset sms data
                    if (currContent != -1)
                        DOTween.To(() => ecRtfs[currContent].anchoredPosition, x => ecRtfs[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                        {
                            ecRtfs[currContent].gameObject.SetActive(false);
                        });
                    ecRtfs[3].anchoredPosition = new Vector2(1500, 0);
                    ecGojs[1].SetActive(true);
                    DOTween.To(() => ecRtfs[3].anchoredPosition, x => ecRtfs[3].anchoredPosition = x, new Vector2(140, 0), .35f).OnComplete(() =>
                    {
                        currContent = 3;
                        LoadingControl.instance.loadingGojList[9].SetActive(false);
                        LoadRechargeData("CARD");
                    });
                    break;

                case "HIS":

                    if (currContent != -1)
                        DOTween.To(() => ecRtfs[currContent].anchoredPosition, x => ecRtfs[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                        {
                            ecRtfs[currContent].gameObject.SetActive(false);
                        });
                    ecRtfs[4].anchoredPosition = new Vector2(1500, 0);
                    ecGojs[4].SetActive(true);
                    DOTween.To(() => ecRtfs[4].anchoredPosition, x => ecRtfs[4].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                    {
                        currContent = 4;
                        LoadingControl.instance.loadingGojList[9].SetActive(false);
                        LoadRechargeData("HIS");
                    });
                    break;

                case "LIST":
                    currCardTogCode = "";      //Reset card data

                    if (currContent != -1)
                        DOTween.To(() => ecRtfs[currContent].anchoredPosition, x => ecRtfs[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                        {
                            ecRtfs[currContent].gameObject.SetActive(false);
                        });
                    ecRtfs[5].anchoredPosition = new Vector2(1500, 0);
                    ecGojs[6].SetActive(true);
                    DOTween.To(() => ecRtfs[5].anchoredPosition, x => ecRtfs[5].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                    {
                        currContent = 5;
                        LoadingControl.instance.loadingGojList[9].SetActive(false);
                        LoadRechargeData("LIST");
                    });
                    break;
                case "SHOP":
                    if (currContent != -1)
                        DOTween.To(() => ecRtfs[currContent].anchoredPosition, x => ecRtfs[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                        {
                            ecRtfs[currContent].gameObject.SetActive(false);
                        });
                    ecRtfs[6].anchoredPosition = new Vector2(1500, 0);
                    ecGojs[9].SetActive(true);
                    DOTween.To(() => ecRtfs[6].anchoredPosition, x => ecRtfs[6].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                    {
                        currContent = 6;
                        LoadingControl.instance.loadingGojList[9].SetActive(false);
                        LoadRechargeData("LIST");
                    });
                    break;
            }
        }

        public void showExcharge(bool isShow)
        {
            if (!isShow)
            {
                for (int i = 0; i < 4; i++)
                {
                    ecTogs[i].isOn = false;
                }
                ////App.trace("currContent = " + currContent);
                DOTween.To(() => ecRtfs[currContent].anchoredPosition, x => ecRtfs[currContent].anchoredPosition = x, new Vector2(1500, 0), .30f);
                DOTween.To(() => ecRtfs[0].anchoredPosition, x => ecRtfs[0].anchoredPosition = x, new Vector2(0, 160), .30f);
                DOTween.To(() => ecRtfs[1].anchoredPosition, x => ecRtfs[1].anchoredPosition = x, new Vector2(0, 160), .30f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    LobbyControl.instance.OpenRecharge(false);
                });
            }
        }

        private void LoadRechargeData(string type)
        {
            switch (type)
            {
                #region //CARD
                case "CARD":
                    OutBounMessage req_CASH_OUT = new OutBounMessage("CASH_OUT.GET_DATA");
                    req_CASH_OUT.addHead();
                    App.ws.send(req_CASH_OUT.getReq(), delegate (InBoundMessage res_CASH_OUT)
                    {
                        MidCardData.Clear();     //Clear before data of this list
                    midCardContentEleRtf.Clear();

                    //App.trace("[RECV] CASH_OUT.GET_DATA");
                    var telcoCount = res_CASH_OUT.readByte();

                        foreach (Transform rtf in ecGojs[2].transform.parent)       //Delete exits element before
                    {
                            if (rtf.gameObject.name != ecGojs[2].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }
                        teleFullName.Clear();
                        for (int i = 0; i < telcoCount; i++)
                        {
                            var code = res_CASH_OUT.readAscii();
                            var name = res_CASH_OUT.readString();
                        //App.trace("code = " + code + "|name = " + name);
                        teleFullName.Add(code, name);
                            MidCardData.Add(code);
                            GameObject goj = Instantiate(ecGojs[2], ecGojs[2].transform.parent, false);
                            goj.GetComponent<Image>().sprite = GetTeleSpriteByCode(code);
                            switch (code)
                            {
                                case "VTT":
                                    goj.GetComponentInChildren<Text>().text = name;
                                    goj.GetComponentInChildren<Text>().color = new Color32(246, 124, 42, 255);
                                    break;
                                case "VNP":
                                    goj.GetComponentInChildren<Text>().text = name;
                                    goj.GetComponentInChildren<Text>().color = new Color32(31, 164, 255, 255);
                                    break;
                                case "VMS":
                                    goj.GetComponentInChildren<Text>().text = name;
                                    goj.GetComponentInChildren<Text>().color = new Color32(232, 29, 29, 255);
                                    break;
                            }
                            Toggle tog = goj.GetComponent<Toggle>();
                            if (i == 0)
                                tog.isOn = true;
                            tog.onValueChanged.AddListener((bool isOn) =>
                            {
                                if (isOn)
                                {
                                    ChangeMidCardContent(code);
                                }
                            });

                            goj.SetActive(true);
                        }

                        foreach (Transform rtf in ecGojs[3].transform.parent)       //Delete exits element before
                    {
                            if (rtf.gameObject.name != ecGojs[3].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }

                        int count = res_CASH_OUT.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            int value = res_CASH_OUT.readInt();
                            int cost = res_CASH_OUT.readInt();
                        // Debug.Log("Value = "+ value+" cost = "+cost);
                        GameObject goj = Instantiate(ecGojs[3], ecGojs[3].transform.parent, false);
                            Text[] txtArr = goj.GetComponentsInChildren<Text>();

                            txtArr[0].text = App.formatMoney(cost.ToString()) + " " + App.listKeyText["CURRENCY"];
                            txtArr[1].text = App.formatMoney(value.ToString()) + " Đ";

                            Button btn = goj.GetComponent<Button>();
                            btn.onClick.AddListener(() =>
                            {
                                // Debug.Log("Show ");
                                string tempString = App.listKeyText["CASHOUT_CARD_CONFIRM"];
                                string new1 = tempString.Replace("#1", App.FormatMoney(cost).ToString());
                                string new2 = new1.Replace("#2", App.listKeyText["CURRENCY"]);
                                string new3 = new2.Replace("#3", teleFullName[currMidCardTogId]);
                                string new4 =  new3.Replace("#4", value.ToString());

                                //"Bạn có chắc chắn muốn dùng " + App.FormatMoney(cost) + " Gold đổi thẻ " + teleFullName[currMidCardTogId] + "\nmệnh giá " + value + "Đ?"
                                App.ShowConfirm(new4, delegate ()
                                    {
                                        var req_exchange = new OutBounMessage("CASH_OUT.EXCHANGE_ITEM");
                                        req_exchange.addHead();
                                        req_exchange.writeAcii(currMidCardTogId); //cardType
                                    req_exchange.writeInt(value);   //value
                                    App.ws.send(req_exchange.getReq(), delegate (InBoundMessage res_exchange)
                                        {
                                            string msg = res_exchange.readString();
                                            App.showErr(msg);
                                        });
                                    });
                            });
                            midCardContentEleRtf.Add(goj.transform);
                            goj.SetActive(true);
                        }

                        ChangeMidCardContent(MidCardData[0]);

                    });

                    break;
                #endregion

                #region //HIS
                case "HIS":
                    var req_GET_HISTORY = new OutBounMessage("CASH_OUT.GET_HISTORY");
                    req_GET_HISTORY.addHead();
                    req_GET_HISTORY.writeByte(1);
                    App.ws.send(req_GET_HISTORY.getReq(), delegate (InBoundMessage res_GET_HISTORY)
                    {
                        //App.trace("[RECV] CASH_OUT.GET_HISTORY");

                        foreach (Transform rtf in ecGojs[5].transform.parent)       //Delete exits element before
                        {
                            if (rtf.gameObject.name != ecGojs[5].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }

                        int count = res_GET_HISTORY.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            string card = res_GET_HISTORY.readString();
                            string status = res_GET_HISTORY.readString();
                            string time = res_GET_HISTORY.readString();
                            string detail = res_GET_HISTORY.readString();
                            int cardId = res_GET_HISTORY.readInt();
                            //Debug.Log("card " + card + "|status = " + status + "|time = " + time + "detail = " + detail);

                            GameObject obj = Instantiate(ecGojs[5], ecGojs[5].transform.parent, false) as GameObject;
                            Text[] txtArr = obj.GetComponentsInChildren<Text>();
                            txtArr[0].text = (i + 1).ToString();
                            txtArr[1].text = card;
                            txtArr[2].text = status;
                            txtArr[3].fontSize = 30;
                            txtArr[3].text = time;
                            //txtArr[4].text = detail;
                            Button btn = obj.GetComponentInChildren<Button>();
                            if (status.Contains("ã hủy"))
                            {
                                btn.gameObject.SetActive(false); obj.SetActive(true);
                                continue;
                            }
                            if (detail == "")
                            {
                                //btn.gameObject.SetActive(false);
                                btn.image.sprite = rcSprList[3];
                                btn.GetComponentInChildren<Text>().text = "HỦY";
                                btn.onClick.AddListener(() =>
                                {
                                    string tempString = App.listKeyText["CASHOUT_CARD_CANCEL"];
                                    string new1 = tempString.Replace("#1", card.ToString());
                                    //"Bạn có chắc chắn muốn hủy yêu cầu đổi thưởng thẻ " + card + " không?"
                                    App.ShowConfirm(new1 , delegate ()
                                    {
                                        var req_CARD_CANCEL = new OutBounMessage("CASH_OUT.CANCEL");
                                        req_CARD_CANCEL.addHead();
                                        req_CARD_CANCEL.writeInt(cardId);
                                        App.ws.send(req_CARD_CANCEL.getReq(), delegate (InBoundMessage res_CARD_CANCEL)
                                        {
                                            App.showErr(res_CARD_CANCEL.readStrings());
                                            txtArr[2].text = "Đã hủy";
                                            btn.gameObject.SetActive(false); obj.SetActive(true);
                                        });
                                    });
                                });
                            }
                            else
                            {
                                btn.onClick.AddListener(() =>
                                {
                                    App.showErr(detail);
                                });
                            }
                            obj.SetActive(true);
                        }
                    });
                    break;
                #endregion

                #region //HIS_ALL
                case "LIST":
                    //App.trace("===GET HIS _ ALL DATA===");
                    var req_GET_HISTORY_ALL = new OutBounMessage("CASH_OUT.GET_HISTORY_ALL");
                    req_GET_HISTORY_ALL.addHead();
                    req_GET_HISTORY_ALL.writeByte(1);
                    App.ws.send(req_GET_HISTORY_ALL.getReq(), delegate (InBoundMessage res_GET_HISTORY_ALL)
                    {
                        //App.trace("[RECV] CASH_OUT.GET_HISTORY");

                        foreach (Transform rtf in ecGojs[7].transform.parent)       //Delete exits element before
                        {
                            if (rtf.gameObject.name != ecGojs[7].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }

                        int count = res_GET_HISTORY_ALL.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            string name = res_GET_HISTORY_ALL.readString();
                            string status = res_GET_HISTORY_ALL.readString();
                            string detail = res_GET_HISTORY_ALL.readString();
                            string time = res_GET_HISTORY_ALL.readString();
                            //App.trace("card " + name + "|status = " + status + "|time = " + time + "detail = " + detail);

                            GameObject obj = Instantiate(ecGojs[7], ecGojs[7].transform.parent, false) as GameObject;
                            Text[] txtArr = obj.GetComponentsInChildren<Text>();
                            txtArr[0].text = (i + 1).ToString();
                            txtArr[1].text = name;
                            txtArr[2].text = status;
                            txtArr[3].text = detail;
                            txtArr[4].text = time;
                            /*
                            Button btn = obj.GetComponentInChildren<Button>();
                            btn.onClick.AddListener(() =>
                            {
                                App.showErr(detail);
                            });
                            if (detail == "")
                            {
                                btn.gameObject.SetActive(false);
                            }
                            */


                            obj.SetActive(true);
                        }
                    });
                    break;
                #endregion

                #region //SMS


                case "SMS":
                    var req_GET_SMS_RECHARGE_DATA = new OutBounMessage("GET_SMS_RECHARGE_DATA");
                    req_GET_SMS_RECHARGE_DATA.addHead();
                    req_GET_SMS_RECHARGE_DATA.writeAcii(App.getProvider());
                    App.ws.send(req_GET_SMS_RECHARGE_DATA.getReq(), delegate (InBoundMessage res_GET_SMS_RECHARGE_DATA)
                    {
                        foreach (Transform rtf in ecGojs[(int)RechargeGojElement.sms].transform.parent)
                        {
                            if (rtf.gameObject.name != ecGojs[(int)RechargeGojElement.sms].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }

                        int count = res_GET_SMS_RECHARGE_DATA.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            string content = res_GET_SMS_RECHARGE_DATA.readAscii();
                            string serviceNumber = res_GET_SMS_RECHARGE_DATA.readAscii();
                            string description = res_GET_SMS_RECHARGE_DATA.readString();
                            string price = res_GET_SMS_RECHARGE_DATA.readString();
                        // Debug.Log("content = " + content + "|serviceNumber = " + serviceNumber + "|des = " + description + "|price = " + price);
                        GameObject goj = Instantiate(ecGojs[(int)RechargeGojElement.sms], ecGojs[(int)RechargeGojElement.sms].transform.parent, false);
                            Text[] txtArr = goj.GetComponentsInChildren<Text>();
                            txtArr[0].text = App.formatMoney(price) + " Đ";
                            txtArr[2].text = App.formatMoney(description) + " " + App.listKeyText["CURRENCY"];

                            Button btn = goj.GetComponentInChildren<Button>();
                            btn.onClick.AddListener(() =>
                            {
                                Application.OpenURL(string.Format("sms:{0}?body={1}", serviceNumber, content));
                            });

                            goj.SetActive(true);
                        }
                    });
                    break;
                #endregion

                #region //MS


                case "MS":
                    cardDataDic.Clear();

                    var req_GET_SC_RECHARGE_DATA = new OutBounMessage("GET_SC_RECHARGE_DATA");
                    req_GET_SC_RECHARGE_DATA.addHead();
                    req_GET_SC_RECHARGE_DATA.writeAcii(App.getProvider());    //Provider;|"PS_CGV_XANHCHIN"
                    App.ws.send(req_GET_SC_RECHARGE_DATA.getReq(), delegate (InBoundMessage res_GET_SC_RECHARGE_DATA)
                    {

                        //Clear denomination table content
                        foreach (Transform rtf in ecGojs[8].transform.parent)
                        {
                            if (rtf.gameObject.name != ecGojs[8].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }

                        //Denomination table
                        int count = res_GET_SC_RECHARGE_DATA.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            int denomination = res_GET_SC_RECHARGE_DATA.readInt();
                            int amount = res_GET_SC_RECHARGE_DATA.readInt();
                            //App.trace("denomination = " + denomination + "|amount " + amount);
                            GameObject goj = Instantiate(ecGojs[8], ecGojs[8].transform.parent, false);
                            Text[] txtArr = goj.GetComponentsInChildren<Text>();
                            txtArr[0].text = App.formatMoneyK(denomination);
                            txtArr[1].text = "=\t" + App.formatMoney(amount.ToString()) + " Gold";
                            goj.SetActive(true);

                        }

                        foreach (Transform rtf in ecGojs[9].transform.parent)
                        {
                            if (rtf.gameObject.name != ecGojs[9].name)
                            {
                                Destroy(rtf.gameObject);
                            }
                        }

                        //Seri and card code length limit
                        count = res_GET_SC_RECHARGE_DATA.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            string cardTypeCode = res_GET_SC_RECHARGE_DATA.readAscii();
                            string cardTypeName = res_GET_SC_RECHARGE_DATA.readString();

                            GameObject goj = Instantiate(ecGojs[9], ecGojs[9].transform.parent, false);
                            goj.GetComponent<Image>().sprite = GetTeleSpriteByCode(cardTypeCode);
                            Toggle tog = goj.GetComponent<Toggle>();
                            if (i == 0)
                                tog.isOn = true;
                            tog.onValueChanged.AddListener((bool isOn) =>
                            {
                                if (isOn)
                                {
                                    ChangeCardContent(cardTypeCode);
                                }
                            });

                            goj.SetActive(true);

                            //App.trace("cardTypeCode = " + cardTypeCode + "|cardTypeName " + cardTypeName);
                            ArrayList arl = new ArrayList();
                            arl.Add(res_GET_SC_RECHARGE_DATA.readByte());       //minPinLength
                            arl.Add(res_GET_SC_RECHARGE_DATA.readByte());       //maxPinLength
                            arl.Add(res_GET_SC_RECHARGE_DATA.readByte());       //minSerialLength
                            arl.Add(res_GET_SC_RECHARGE_DATA.readByte());       //maxSerialLength
                            arl.Add(res_GET_SC_RECHARGE_DATA.readByte() == 1);  //serial required
                                                                                //App.trace(arl[0] + "|" + arl[1] + "|" + arl[2] + "|" + arl[3]);
                            cardDataDic.Add(cardTypeCode, arl);
                        }
                        ChangeCardContent(cardDataDic.Keys.ToList()[0]);
                    });
                    break;
                #endregion

                #region //POLICY
                case "POLICY":
                    foreach (Transform rtf in ecGojs[8].transform.parent)
                    {
                        if (rtf.gameObject.name != ecGojs[8].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }

                    OutBounMessage req_POLICY = new OutBounMessage("CASH_OUT.GET_POLICY");
                    req_POLICY.addHead();
                    App.ws.send(req_POLICY.getReq(), delegate (InBoundMessage res_POLICY)
                    {
                        rcTxtList[0].text = res_POLICY.readString();
                        //  Debug.Log(rcTxtList[0].text);
                        //App.trace("[RECV] CASH_OUT.POLICY");
                        int count = res_POLICY.readByte();
                        for (int i = 0; i < count; i++)
                        {
                            GameObject goj = Instantiate(ecGojs[8], ecGojs[8].transform.parent, false);
                            Text[] txtArr = goj.GetComponentsInChildren<Text>();
                            txtArr[0].text = res_POLICY.readString();
                            txtArr[1].text = res_POLICY.readString();




                            string st2 = res_POLICY.readString();

                            if (st2.Equals("Không thể đổi thưởng"))
                            {
                                txtArr[2].text = "N/A";
                            }
                            else
                            {
                                txtArr[2].text = st2;
                            }


                            string st3 = res_POLICY.readString();

                            if (st3.Equals("Không thể đổi thưởng"))
                            {
                                txtArr[3].text = "N/A";
                            }
                            else
                            {
                                txtArr[3].text = st3;
                            }

                            string st4 = res_POLICY.readString();
                            if (st4.Equals("-1"))
                            {
                                txtArr[4].text = "N/A";
                            }
                            else
                            {
                                txtArr[4].text = st4 + "%";
                            }

                            //  Debug.Log(txtArr[0].text + txtArr[1].text + txtArr[2].text + txtArr[3].text+" => " + txtArr[4].text);
                            goj.SetActive(true);
                        }
                    });
                    break;
                    #endregion
            }
        }

        private void ChangeMidCardContent(string code)
        {
            if (code == currMidCardTogId)
                return;

            currMidCardTogId = code;
            ////App.trace("hey, switch = " + code);
            foreach (Transform rtf in midCardContentEleRtf)
            {
                rtf.DOShakeScale(.25f);
            }
        }

        /// <summary>
        /// When user press card tog
        /// </summary>
        private void ChangeCardContent(string code)
        {
            if (code == currCardTogCode)
                return;

            currCardTogCode = code;
            rcIpfList[0].text = "";
            rcIpfList[1].text = "";
        }

        private Sprite GetTeleSpriteByCode(string t)
        {
            switch (t)
            {
                case "VTT":
                    return rcSprList[0];
                case "VNP":
                    return rcSprList[1];
                case "VMS":
                    return rcSprList[2];

            }
            return null;
        }

        public void doAction(string type)
        {
            switch (type)
            {
                #region //Do Recharge by card


                case "card":        //Do Recharge by card
                    ArrayList arr = cardDataDic[currCardTogCode];

                    int minPinLenght = (int)arr[0];
                    int maxPinLength = (int)arr[1];
                    int minSeriLength = (int)arr[2];
                    int maxSeriLength = (int)arr[3];

                    if (rcIpfList[0].text.Length < minSeriLength || rcIpfList[0].text.Length > maxSeriLength)
                    {
                        //App.showErr("Seri có độ dài trong khoảng " + minSeriLength + " - " + maxSeriLength);

                        string appShowErr = App.listKeyText["WARN_CARD_SERIAL_LENGTH"];
                        string new1 = appShowErr.Replace("#1", minPinLenght.ToString());
                        string new2 = new1.Replace("#2", maxSeriLength.ToString());

                        App.showErr(new2);

                        return;
                    }

                    if (rcIpfList[1].text.Length < minPinLenght || rcIpfList[1].text.Length > maxPinLength)
                    {

                        //App.showErr("Mã thẻ có độ dài trong khoảng " + minPinLenght + " - " + maxPinLength);
                        string appShowErr = App.listKeyText["WARN_CARD_PIN_LENGTH"];
                        string new1 = appShowErr.Replace("#1", minPinLenght.ToString());
                        string new2 = new1.Replace("#2", maxSeriLength.ToString());

                        App.showErr(new2);
                        return;
                    }
                    OutBounMessage req = new OutBounMessage("RECHARGE_BY_SC_EX");
                    req.addHead();
                    req.writeAcii(currCardTogCode);
                    req.writeAcii(rcIpfList[1].text);       //Code
                    req.writeAcii(rcIpfList[0].text);       //Seri
                    App.ws.send(req.getReq(), delegate (InBoundMessage res)
                    {
                        long amount = res.readLong();
                        //App.showErr(amount + " Gold đã được nạp vào tài khoản của bạn");
                        string appShowErr = App.listKeyText["INFO_CARD_RECHARGED"];
                        string new1 = appShowErr.Replace("#1", amount.ToString());
                        string new2 = new1.Replace("#2", App.listKeyText["CURRENCY"]);
                        App.showErr(new2);
                    });
                    break;
                #endregion

                #region //Do recharge by giftCode
                case "gift":
                    string gift = rcIpfList[2].text;
                    if (gift.Length == 0)
                    {
                        //App.showErr("Vui lòng nhập giftcode");


                        App.showErr(App.listKeyText["WARN_GIFTCODE_BLANK"]);
                        return;
                    }
                    OutBounMessage req_RECHARGE_BY_GIFT_CODE = new OutBounMessage("RECHARGE_BY_GIFT_CODE");
                    req_RECHARGE_BY_GIFT_CODE.addHead();
                    req_RECHARGE_BY_GIFT_CODE.writeAcii(gift);
                    App.ws.send(req_RECHARGE_BY_GIFT_CODE.getReq(), delegate (InBoundMessage res)
                    {
                        long amount = res.readLong();
                        //App.showErr("Mã quà tặng " + gift + " đã nạp thành công. Tài khoản của bạn được tặng " + amount + " Gold");

                        string appShowErr = App.listKeyText["INFO_GIFTCODE_SUCCESS"];
                        string new1 = appShowErr.Replace("#1",gift.ToString());
                        string new2 =  new1.Replace("#2",amount.ToString());

                        App.showErr(new2 + " " + App.listKeyText["CURRENCY"]);
                    });
                    break;
                    #endregion

            }
        }

        public void Test(string t)
        {
            changeContent(t);
        }
    }
}
