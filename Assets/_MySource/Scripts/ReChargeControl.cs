using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ReChargeControl : MonoBehaviour
{

    /// <summary>
    /// 0: HeaderEle|1: SMS|2: SMS PLUS|3: Mid-Gift|4: smsEle|5: smsPlusTeleEle|6: smsPlusCotentEle|
    /// 7: Mid-Card|8: cardTextcontentEle|9: cardTeleEle|10: Mid-IAP|12: Agency|
    /// </summary>
    public GameObject[] rcGojList;

    /// <summary>
    /// 0: Header|1:Mid-Card|2: BtnBack|3: Mid-Gift|4: Mid-Sms|5: Mid-SmsPlus|6:mid-iap|7:Agency
    /// </summary>
    public RectTransform[] rcRtfList;
    /// <summary>
    /// 0: vtt|1: vnp|2: mobi
    /// </summary>
    public Sprite[] rcSprList;
    /// <summary>
    /// 0:
    /// </summary>
    public Text[] rcTxtList;
    /// <summary>
    /// 0: midCard-seri|1: midCard-cardcode|2: mid-giftcode
    /// </summary>
    public InputField[] rcIpfList;
    private int currContent = -1;
    private Dictionary<string, Toggle> togList = new Dictionary<string, Toggle>();
    /// <summary>
    /// CodeName of current tog
    /// </summary>
    private string currTogId = "";
    private string currSmsPlusTogId = "";        //current tog of sms plus content
    private string currCardTogCode = "";
    private Dictionary<string, ArrayList> smsPlusDataDic = new Dictionary<string, ArrayList>();
    private Dictionary<string, ArrayList> cardDataDic = new Dictionary<string, ArrayList>();
    private Dictionary<string, ArrayList> cardDataDic2 = new Dictionary<string, ArrayList>();
    private enum RechargeGojElement : int
    {
        sms = 4
    }

    private void OnEnable()
    {

        currContent = -1;
        currTogId = "";
        togList.Clear();
        getHeader();
    }

    private void getHeader()
    {
        foreach (Transform rtf in rcGojList[0].transform.parent)
        {
            if (rtf.gameObject.name != rcGojList[0].name)
            {
                Destroy(rtf.gameObject);
            }
        }

        var req = new OutBounMessage("PAYMENT.LIST_EX");
        req.addHead();
        req.writeAcii(App.getDevicePlatform());    //Platform: ios|android
        req.writeAcii(App.getVersion()); //Version: 1.4.0
        req.writeAcii(App.getProvider());   //PROVIDDER
        req.writeString(LoadingControl.instance.CountryC);
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            //App.trace("[RECV] PAYMENT.LIST_EX");
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                string code = res.readAscii();
                string name = res.readString();
                //code = name = "IAP";
                // App.trace("code = " + code + "|name = " + name);

                GameObject goj = Instantiate(rcGojList[0], rcGojList[0].transform.parent, false);
                goj.GetComponentInChildren<Text>().text = name.ToUpper();
                Toggle tog = goj.GetComponentInChildren<Toggle>();
                tog.onValueChanged.AddListener((bool isOn) =>
                {
                    if (isOn)
                    {
                        changeContent(code);
                    }
                });
                togList.Add(code, tog);
                goj.SetActive(true);
                //break;

            }

            DOTween.To(() => rcRtfList[2].anchoredPosition, x => rcRtfList[2].anchoredPosition = x, new Vector2(0, 0), .35f);
            DOTween.To(() => rcRtfList[0].anchoredPosition, x => rcRtfList[0].anchoredPosition = x, new Vector2(0, 0), .35f);
            //changeContent(togList.Keys.ToList()[0]);
            if (CPlayer.showGifCode == true)
                changeContent("GIFT");//GIFT
            else
                changeContent(togList.Keys.ToList()[0]);
        });
    }

    public void changeContent(string idToChange)
    {
        if (currTogId == idToChange)    //Prevent if user press actived tog
            return;
        CPlayer.showGifCode = false;
        LoadingControl.instance.loadingGojList[9].SetActive(true);  //Prevent touch while sliding content panel
        currTogId = idToChange;
        togList[idToChange].isOn = true;
        if (currContent == -1)
        {
            rcGojList[1].SetActive(false);
            rcGojList[2].SetActive(false);
            rcGojList[3].SetActive(false);
            rcGojList[7].SetActive(false);
            rcGojList[10].SetActive(false);
            rcGojList[12].SetActive(false);
        }
        switch (idToChange)
        {
            case "AGENCY":
                if (currContent != -1)
                    DOTween.To(() => rcRtfList[currContent].anchoredPosition, x => rcRtfList[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                    {
                        rcRtfList[currContent].gameObject.SetActive(false);

                    });
                rcRtfList[7].anchoredPosition = new Vector2(1500, 0);
                rcGojList[12].SetActive(true);
                

                DOTween.To(() => rcRtfList[7].anchoredPosition, x => rcRtfList[7].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                {
                    currContent = 7;
                    LoadingControl.instance.loadingGojList[9].SetActive(false);
                });


                break;
            case "GIFT":

                rcIpfList[2].text = "";        //reset data
                if (currContent != -1)
                    DOTween.To(() => rcRtfList[currContent].anchoredPosition, x => rcRtfList[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                    {
                        rcRtfList[currContent].gameObject.SetActive(false);

                    });
                //rcRtfList[3]
                rcRtfList[3].anchoredPosition = new Vector2(1500, 0);
                rcGojList[3].SetActive(true);
                DOTween.To(() => rcRtfList[3].anchoredPosition, x => rcRtfList[3].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                {
                    currContent = 3;
                    LoadingControl.instance.loadingGojList[9].SetActive(false);
                });

                break;
            case "SMS":

                if (currContent != -1)
                    DOTween.To(() => rcRtfList[currContent].anchoredPosition, x => rcRtfList[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                    {
                        rcRtfList[currContent].gameObject.SetActive(false);
                    });
                rcRtfList[4].anchoredPosition = new Vector2(1500, 0);
                rcGojList[1].SetActive(true);
                DOTween.To(() => rcRtfList[4].anchoredPosition, x => rcRtfList[4].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                {
                    currContent = 4;
                    LoadingControl.instance.loadingGojList[9].SetActive(false);
                    LoadRechargeData("SMS");
                });
                break;
            case "SMS+":
                currSmsPlusTogId = "";      //Reset sms data
                if (currContent != -1)
                    DOTween.To(() => rcRtfList[currContent].anchoredPosition, x => rcRtfList[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                    {
                        rcRtfList[currContent].gameObject.SetActive(false);
                    });
                rcRtfList[5].anchoredPosition = new Vector2(1500, 0);
                rcGojList[2].SetActive(true);
                DOTween.To(() => rcRtfList[5].anchoredPosition, x => rcRtfList[5].anchoredPosition = x, new Vector2(140, 0), .35f).OnComplete(() =>
                {
                    currContent = 5;
                    LoadingControl.instance.loadingGojList[9].SetActive(false);
                    LoadRechargeData("SMS+");
                });
                break;
            case "CARD":
                currCardTogCode = "";      //Reset card data
                rcIpfList[0].text = "";
                rcIpfList[1].text = "";

                if (currContent != -1)
                    DOTween.To(() => rcRtfList[currContent].anchoredPosition, x => rcRtfList[currContent].anchoredPosition = x, new Vector2(-1500, -100), .35f).OnComplete(() =>
                    {
                        rcRtfList[currContent].gameObject.SetActive(false);
                    });
                rcRtfList[1].anchoredPosition = new Vector2(1500, -50);
                rcGojList[7].SetActive(true);
                DOTween.To(() => rcRtfList[1].anchoredPosition, x => rcRtfList[1].anchoredPosition = x, new Vector2(140, -50), .35f).OnComplete(() =>
                {
                    currContent = 1;
                    LoadingControl.instance.loadingGojList[9].SetActive(false);
                    LoadRechargeData("CARD");
                });
                break;

            case "IAP":
                if (currContent != -1)
                    DOTween.To(() => rcRtfList[currContent].anchoredPosition, x => rcRtfList[currContent].anchoredPosition = x, new Vector2(-1500, 0), .35f).OnComplete(() =>
                    {
                        rcRtfList[currContent].gameObject.SetActive(false);
                    });
                rcRtfList[6].anchoredPosition = new Vector2(1500, 0);
                rcGojList[10].SetActive(true);
                DOTween.To(() => rcRtfList[6].anchoredPosition, x => rcRtfList[6].anchoredPosition = x, new Vector2(0, 0), .35f).OnComplete(() =>
                {
                    currContent = 6;
                    LoadingControl.instance.loadingGojList[9].SetActive(false);
                    LoadRechargeData("IAP");
                });
                break;
        }
    }

    public void showRecharge(bool isShow)
    {
        if (!isShow)
        {
            //App.trace("currContent = " + currContent);
            DOTween.To(() => rcRtfList[currContent].anchoredPosition, x => rcRtfList[currContent].anchoredPosition = x, new Vector2(1500, 0), .30f);
            DOTween.To(() => rcRtfList[0].anchoredPosition, x => rcRtfList[0].anchoredPosition = x, new Vector2(0, 160), .30f);
            DOTween.To(() => rcRtfList[2].anchoredPosition, x => rcRtfList[2].anchoredPosition = x, new Vector2(0, 160), .30f).OnComplete(() =>
            {
                gameObject.SetActive(false);
                LoadingControl.instance.loadingGojList[3].SetActive(false);
                switch (SceneManager.GetActiveScene().name)
                {
                    case "Lobby":
                        LobbyControl.instance.OpenRecharge(false);
                        break;
                    case "Chest":
                        ChestControl.instance.DoAction("closeRecharge");
                        break;
                    case "Fish":
                        Slot.Games.Fish.FishGameControllUI.instance.CloseRecharge();
                        break;
                }
            });
        }
    }

    public Dropdown dropListCardType;
    private List<string> cartType = new List<string>();
    public ScrollRect scrollRectNapCard;
    public GameObject contenCard;


    IEnumerator wait()
    {

        yield return new WaitForSeconds(0.1f);
        scrollRectNapCard.verticalNormalizedPosition = 1f;

    }
    private string codeCard = "";//"Chọn mệnh giá thẻ nạp";
    private string strContent = "";//"Vui lòng chọn mệnh giá thẻ nạp";
    private void LoadRechargeData(string type)
    {
        switch (type)
        {
            case "SMS":
                var req_GET_SMS_RECHARGE_DATA = new OutBounMessage("GET_SMS_RECHARGE_DATA");
                req_GET_SMS_RECHARGE_DATA.addHead();
                req_GET_SMS_RECHARGE_DATA.writeAcii(App.getProvider());
                App.ws.send(req_GET_SMS_RECHARGE_DATA.getReq(), delegate (InBoundMessage res_GET_SMS_RECHARGE_DATA)
                {
                    foreach (Transform rtf in rcGojList[(int)RechargeGojElement.sms].transform.parent)
                    {
                        if (rtf.gameObject.name != rcGojList[(int)RechargeGojElement.sms].name)
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
                        Debug.Log("content = " + content + "|serviceNumber = " + serviceNumber + "|des = " + description + "|price = " + price);
                        GameObject goj = Instantiate(rcGojList[(int)RechargeGojElement.sms], rcGojList[(int)RechargeGojElement.sms].transform.parent, false);
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = App.formatMoney(price) + " VND";
                        txtArr[2].text = App.formatMoney(description) + " Gold";

                        Button btn = goj.GetComponentInChildren<Button>();
                        btn.onClick.AddListener(() =>
                        {
                            string url = string.Format("sms:{0}?body={1}", serviceNumber, content);
#if UNITY_IOS
                            url = string.Format("sms:{0}?&body={1}", serviceNumber, System.Uri.EscapeDataString(content));
#endif
                            Application.OpenURL(url);
                        });

                        goj.SetActive(true);
                    }
                });
                break;
            case "SMS+":
                var req_SMS_PLUS_EX = new OutBounMessage("PAYMENT.SMS_PLUS_EX");
                req_SMS_PLUS_EX.addHead();
                App.ws.send(req_SMS_PLUS_EX.getReq(), delegate (InBoundMessage res_SMS_PLUS_EX)
                {
                    smsPlusDataDic.Clear();     //Clear before data of this list

                    //Delete before list telcol
                    foreach (Transform rtf in rcGojList[5].transform.parent)
                    {
                        if (rtf.gameObject.name != rcGojList[5].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }

                    string desc = res_SMS_PLUS_EX.readString();
                    int telcoCount = res_SMS_PLUS_EX.readByte();
                    for (var i = 0; i < telcoCount; i++)
                    {
                        string telcoCode = res_SMS_PLUS_EX.readAscii();
                        string telcoName = res_SMS_PLUS_EX.readString();
                        //telcoList.Add(telcoCode);
                        //setCardType(telcoCode, cardToggleImage2[i]);

                        GameObject goj = Instantiate(rcGojList[5], rcGojList[5].transform.parent, false);
                        goj.GetComponent<Image>().sprite = GetTeleSpriteByCode(telcoCode);
                        switch (telcoCode)
                        {
                            case "VTT":
                                goj.GetComponentInChildren<Text>().text = telcoName;
                                goj.GetComponentInChildren<Text>().color = new Color32(246, 124, 42, 255);
                                break;
                            case "VNP":
                                goj.GetComponentInChildren<Text>().text = telcoName;
                                goj.GetComponentInChildren<Text>().color = new Color32(31, 164, 255, 255);
                                break;
                            case "VMS":
                                goj.GetComponentInChildren<Text>().text = telcoName;
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
                                ChangeSmsPlusContent(telcoCode);
                            }
                        });

                        goj.SetActive(true);

                        int count = res_SMS_PLUS_EX.readByte();
                        //App.trace("telcoCode = " + telcoCode + "|telcoName = " + telcoName + "|count = " + count);
                        ArrayList arr = new ArrayList();
                        for (var j = 0; j < count; j++)
                        {
                            /*
                            string syntax = res.readAscii();
                            string serviceNumber = res.readAscii();
                            int money = res.readInt();
                            int amount = res.readInt();
                            */

                            arr.Add(res_SMS_PLUS_EX.readAscii());       //syntax
                            arr.Add(res_SMS_PLUS_EX.readAscii());       //number
                            arr.Add(res_SMS_PLUS_EX.readInt());         //money
                            arr.Add(res_SMS_PLUS_EX.readInt());         //amount
                            //App.trace(arr[4 * j] + "|" + arr[4 * j + 1] + "|" + arr[4 * j + 2] + "|" + arr[4 * j + 3]);
                        }

                        smsPlusDataDic.Add(telcoCode, arr);

                    }

                    ChangeSmsPlusContent(smsPlusDataDic.Keys.ToList()[0]);
                });
                break;
            case "CARD":
                cardDataDic.Clear();
                dropListCardType.ClearOptions();
                cartType.Clear();
                var req_GET_SC_RECHARGE_DATA = new OutBounMessage("GET_SC_RECHARGE_DATA");
                req_GET_SC_RECHARGE_DATA.addHead();
                req_GET_SC_RECHARGE_DATA.writeAcii(App.getProvider());    //Provider;|"PS_CGV_XANHCHIN"
                App.ws.send(req_GET_SC_RECHARGE_DATA.getReq(), delegate (InBoundMessage res_GET_SC_RECHARGE_DATA)
                {

                    //Clear denomination table content
                    foreach (Transform rtf in rcGojList[8].transform.parent)
                    {
                        if (rtf.gameObject.name != rcGojList[8].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }




                    //Denomination table
                    int count = res_GET_SC_RECHARGE_DATA.readByte();
                    //  Debug.Log("count = " + count);
                    codeCard = App.listKeyText["CARD_RECHARGE_DENOMINATION"];
                    cartType.Add(codeCard);
                    for (int i = 0; i < count; i++)
                    {
                        int denomination = res_GET_SC_RECHARGE_DATA.readInt();
                        int amount = res_GET_SC_RECHARGE_DATA.readInt();

                        cartType.Add(denomination / 1000 + "K");
                        //  Debug.Log("denomination = " + denomination + "|amount " + amount);
                        GameObject goj = Instantiate(rcGojList[8], rcGojList[8].transform.parent, false);

                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = denomination / 1000 + "K";
                        txtArr[1].text = "=\t" + App.formatMoneyK(amount) + " Gold";
                        goj.SetActive(true);

                    }
                    dropListCardType.AddOptions(cartType);
                    dropListCardType.value = 0;

                    /*  foreach (Transform rtf in rcGojList[9].transform.parent)
                      {
                          if (rtf.gameObject.name != rcGojList[9].name)
                          {
                              Destroy(rtf.gameObject);
                          }
                      }*/

                    foreach (Transform rtf in contenCard.transform)
                    {
                        if (rtf.gameObject.name != rcGojList[9].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }

                    //Seri and card code length limit
                    count = res_GET_SC_RECHARGE_DATA.readByte();
                    //Debug.Log("Seri and card code length limit " + count);
                    for (int i = 0; i < count; i++)
                    {
                        string cardTypeCode = res_GET_SC_RECHARGE_DATA.readAscii();
                        string cardTypeName = res_GET_SC_RECHARGE_DATA.readString();

                        //Debug.Log("cardTypeCode "+ cardTypeCode+ "  cardTypeName "+ cardTypeName);
                        //GameObject goj = Instantiate(rcGojList[9], rcGojList[9].transform.parent, false);
                        GameObject goj = Instantiate(rcGojList[9], contenCard.transform, false);
                        goj.GetComponent<Image>().sprite = GetTeleSpriteByCode(cardTypeCode);
                        switch (cardTypeCode)
                        {
                            case "VTT":
                                goj.GetComponentInChildren<Text>().text = cardTypeName;
                                goj.GetComponentInChildren<Text>().color = new Color32(246, 124, 42, 255);
                                break;
                            case "VNP":
                                goj.GetComponentInChildren<Text>().text = cardTypeName;
                                goj.GetComponentInChildren<Text>().color = new Color32(31, 164, 255, 255);
                                break;
                            case "VMS":
                                goj.GetComponentInChildren<Text>().text = cardTypeName;
                                goj.GetComponentInChildren<Text>().color = new Color32(232, 29, 29, 255);
                                break;

                            // case "VNG":
                            //     goj.GetComponentInChildren<Text>().text = cardTypeName;
                            //     goj.GetComponentInChildren<Text>().color = new Color32(245, 130, 80, 255);
                            //     break;

                            default:
                                System.Random rd = new System.Random();
                                int x = rd.Next(0, colors.Length);
                                while (x == idColor)
                                    x = rd.Next(0, colors.Length);
                                idColor = x;
                                goj.GetComponentInChildren<Text>().text = cardTypeName;
                                goj.GetComponentInChildren<Text>().color = colors[idColor];
                                break;

                                /*VMS MobiFone
                                cardTypeCode GATE  cardTypeName FPT GATE
                                cardTypeCode ZING cardTypeName ZING
                                cardTypeCode GRN cardTypeName GARENA*/
                        }
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
                                                                            // Debug.Log(arl[0] + "|" + arl[1] + "|" + arl[2] + "|" + arl[3]);
                        cardDataDic.Add(cardTypeCode, arl);

                    }
                    string contenAlert = res_GET_SC_RECHARGE_DATA.readString();
                    rcTxtList[0].text = contenAlert;

                    //  Debug.Log(contenAlert);
                    ChangeCardContent(cardDataDic.Keys.ToList()[0]);
                    StartCoroutine(wait());
                });
                break;

            case "IAP":
                foreach (Transform rtf in rcGojList[11].transform.parent)
                {
                    if (rtf.gameObject.name != rcGojList[11].name)
                    {
                        Destroy(rtf.gameObject);
                    }
                }
                string iapType = "ANDROID";
#if UNITY_IOS
                iapType = "IOS";
#endif

                //App.trace("provider " + App.getProvider() + " || identifier " + Application.identifier+" || iaptype "+iapType);
                var req_IAP = new OutBounMessage("RECHARGE_BY_IAP_" + iapType + "_LIST");
                req_IAP.addHead();
                //req_IAP.writeString(App.getProvider());
                req_IAP.writeString(Application.identifier);
                App.ws.send(req_IAP.getReq(), delegate (InBoundMessage res_IAP)
                {
                    int count = res_IAP.readByte();
                    LoadingControl.instance.productIdBundle = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        string productId = res_IAP.readString();
                        LoadingControl.instance.productIdBundle[i] = productId;
                        string vnd = App.formatMoney(res_IAP.readLong().ToString());
                        string usd = App.formatMoney(res_IAP.readLong().ToString());
                        GameObject iapClone = Instantiate(rcGojList[11], rcGojList[11].transform.parent, false);
                        iapClone.GetComponentsInChildren<Text>()[1].text = usd + " Gold";
                        iapClone.GetComponentsInChildren<Text>()[0].text = vnd + " VND";
                        Button btn = iapClone.GetComponent<Button>();
                        btn.onClick.AddListener(() =>
                        {
                            LoadingControl.instance.BuyChipIAP(productId);
                        });

                        iapClone.SetActive(true);
                        //App.trace("productId " + productId + " || vnd " + vnd + " || usd " + usd);
                    }
                    if (LoadingControl.m_StoreController == null)
                    {
                        LoadingControl.instance.InitializePurchasing();
                    }
                });
                break;
        }
    }


    private void ChangeSmsPlusContent(string code)
    {
        if (code == currSmsPlusTogId)
            return;

        currSmsPlusTogId = code;
        //App.trace("hey, switch = " + code);
        switch (code)
        {
            case "VTT":
                foreach (Transform rtf in rcGojList[6].transform.parent)
                {
                    if (rtf.gameObject.name != rcGojList[6].name)
                    {
                        Destroy(rtf.gameObject);
                    }
                }

                ArrayList data_VTT = smsPlusDataDic[code];
                int count = data_VTT.Count / 4;
                //App.trace(count.ToString());
                for (int i = 0; i < count; i++)
                {
                    int tmp = i;
                    GameObject goj = Instantiate(rcGojList[6], rcGojList[6].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();

                    txtArr[0].text = App.formatMoney(data_VTT[4 * i + 2].ToString()) + " VND";
                    txtArr[2].text = App.formatMoney(data_VTT[4 * i + 3].ToString()) + " Gold";

                    Button btn = goj.GetComponentInChildren<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        string url = string.Format("sms:{0}?body={1}", data_VTT[4 * tmp + 1].ToString(), data_VTT[4 * tmp].ToString());
#if UNITY_IOS
                        url = string.Format("sms:{0}?&body={1}", data_VTT[4 * tmp + 1].ToString(), System.Uri.EscapeDataString(data_VTT[4 * tmp].ToString()));
#endif
                        Application.OpenURL(url);
                    });

                    goj.SetActive(true);
                    goj.transform.DOShakeScale(.25f);
                }

                break;
            case "VNP":
                foreach (Transform rtf in rcGojList[6].transform.parent)
                {
                    if (rtf.gameObject.name != rcGojList[6].name)
                    {
                        Destroy(rtf.gameObject);
                    }
                }

                ArrayList data_VNP = smsPlusDataDic[code];
                int count_VNP = data_VNP.Count / 4;
                //App.trace(count_VNP.ToString());
                for (int i = 0; i < count_VNP; i++)
                {
                    int tmp = i;
                    GameObject goj = Instantiate(rcGojList[6], rcGojList[6].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();

                    txtArr[0].text = App.formatMoney(data_VNP[4 * i + 2].ToString());
                    txtArr[2].text = App.formatMoney(data_VNP[4 * i + 3].ToString());
                    Button btn = goj.GetComponentInChildren<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        string url = string.Format("sms:{0}?body={1}", data_VNP[4 * tmp + 1].ToString(), data_VNP[4 * tmp].ToString());
#if UNITY_IOS
                        url = string.Format("sms:{0}?&body={1}", data_VNP[4 * tmp + 1].ToString(), System.Uri.EscapeDataString(data_VNP[4 * tmp].ToString()));
#endif
                        Application.OpenURL(url);
                    });
                    goj.SetActive(true);
                    goj.transform.DOShakeScale(.25f);
                }

                break;

            case "VMS":
                foreach (Transform rtf in rcGojList[6].transform.parent)
                {
                    if (rtf.gameObject.name != rcGojList[6].name)
                    {
                        Destroy(rtf.gameObject);
                    }
                }

                ArrayList data_VMS = smsPlusDataDic[code];
                int count_VMS = data_VMS.Count / 4;
                //App.trace(data_VMS.ToString());
                for (int i = 0; i < count_VMS; i++)
                {
                    int tmp = i;
                    GameObject goj = Instantiate(rcGojList[6], rcGojList[6].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();

                    txtArr[0].text = App.formatMoney(data_VMS[4 * i + 2].ToString());
                    txtArr[2].text = App.formatMoney(data_VMS[4 * i + 3].ToString());
                    Button btn = goj.GetComponentInChildren<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        string url = string.Format("sms:{0}?body={1}", data_VMS[4 * tmp + 1].ToString(), data_VMS[4 * tmp].ToString());
#if UNITY_IOS
                        url = string.Format("sms:{0}?&body={1}", data_VMS[4 * tmp + 1].ToString(), System.Uri.EscapeDataString(data_VMS[4 * tmp].ToString()));
#endif
                        Application.OpenURL(url);
                    });
                    goj.SetActive(true);
                    goj.transform.DOShakeScale(.25f);
                }

                break;
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

        //rcIpfList[3].text = "";
        //rcIpfList[4].text = "";
    }
    int idrcSprList = 1;
    int idColor = 0;
    public Color32[] colors;
    private Sprite GetTeleSpriteByCode(string t)
    {
        System.Random rd = new System.Random();
        int x = rd.Next(0, rcSprList.Length);
        while (x == idrcSprList)
            x = rd.Next(0, rcSprList.Length);
        idrcSprList = x;
        switch (t)
        {
            case "VTT":
                return rcSprList[0];
            case "VNP":
                return rcSprList[1];
            case "VMS":
                return rcSprList[2];
            // case "VNG":
            //     return rcSprList[1].Sprite;
            default:

                return rcSprList[idrcSprList];
        }
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

                    string new1 = appShowErr.Replace("#1", minSeriLength.ToString());
                    string new2 = new1.Replace("#2", maxSeriLength.ToString());
                    App.showErr(new2);
                    return;
                }

                if (rcIpfList[1].text.Length < minPinLenght || rcIpfList[1].text.Length > maxPinLength)
                {
                    //App.showErr("Mã thẻ có độ dài trong khoảng " + minPinLenght + " - " + maxPinLength);

                    string appShowErr = App.listKeyText["WARN_CARD_PIN_LENGTH"];
                    string new1 = appShowErr.Replace("#1", minPinLenght.ToString());
                    string new2 = new1.Replace("#2", maxPinLength.ToString());

                    App.showErr(new2);
                    return;
                }

                string money = dropListCardType.captionText.text;
                //Debug.Log(money);
                if (money.Equals(codeCard))
                {
                    strContent = App.listKeyText["CARD_NOMINATION_REQUIRED"];
                    App.showErr(strContent);
                    return;
                }

                LoadingControl.instance.loadingGojList[32].SetActive(true);
                if (money.Length > 0)
                    money = money.Remove(money.Length - 1, 1) + "000";
               // Debug.Log(money);
                OutBounMessage req = new OutBounMessage("RECHARGE_BY_SC_EX");
                req.addHead();
                req.writeAcii(currCardTogCode);
                req.writeAcii(rcIpfList[1].text);       //Code
                req.writeAcii(rcIpfList[0].text);       //Seri
                req.writeAcii(money);//Meng gia


                //Debug.Log("currCardTogCode " + currCardTogCode + " Seri " + rcIpfList[0].text + " Code " + rcIpfList[1].text + " Money " + money);


                App.ws.send(req.getReq(), delegate (InBoundMessage res)
                {
                    //LoadingControl.instance.loadingGojList[32].SetActive(false);
                    //long amount = res.readLong();
                    // App.showErr(amount + " Gold đã được nạp vào tài khoản của bạn");
                });

                LoadingControl.instance.loadingGojList[32].SetActive(false);

                //App.showErr("Hệ thống đang xử lý. Thông tin sẽ được gửi vào hòm thư của bạn.");
                App.showErr(App.listKeyText["IN_PROCESSING"]);
                rcIpfList[1].text = "";
                rcIpfList[0].text = "";
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
                    //App.showErr("Mã quà tặng " + gift + "được nạp thành công. Tài khoản của bạn được tặng " + amount + " Gold");

                    string appShowErr = App.listKeyText["INFO_GIFTCODE_SUCCESS"];
                    string new1 = appShowErr.Replace("#1", gift.ToString());
                    string new2 = new1.Replace("#2", amount.ToString());

                    App.showErr(new2 + " " + App.listKeyText["CURRENCY"]);
                });
                break;
                #endregion

        }
    }

    public void DoIap()
    {
        //App.showErr("Tính năng đang phát triển");
        App.showErr(App.listKeyText["IN_DEVELOPING"]);
    }

    public void Test(string t)
    {
        changeContent(t);
    }

}
