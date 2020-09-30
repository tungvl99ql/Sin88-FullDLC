using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Core.Server.Api;

public class PlayerInfoControl : MonoBehaviour
{

    /// <summary>
    /// 0: header|1: info|2: his|3: bot|4: back|5: changePassPanel|6: changeAvaPanel
    /// 7: dateSelect
    /// </summary>
    public RectTransform[] plRtfs;
    /// <summary>
    /// 0: NicName|1: Balance|2: err old pass|3: err new pass|4: err re pass|
    /// 5: fromDay|6: toDay|7: gameNameToSearch|8: day select|9: vip|10: process-text
    /// 11: phone|12: page
    /// 8: dateErr
    /// </summary>
    public Text[] plTxts;
    /// <summary>
    /// 0: Avatar|1: vip-process|2: vip img
    /// </summary>
    public Image[] plImgs;
    /// <summary>
    /// 0: ele|1: btnAuthen|2: black|3: avaEle|4: vip|5: btnChangePass
    /// </summary>
    public GameObject[] plGojs;
    /// <summary>
    /// 0: old|1: new|2: reNew|3: day|4: month|5: year
    /// </summary>
    public InputField[] plIpfs;

    /// <summary>
    /// 0: 255|1: 0
    /// </summary>
    public Color[] plClolors;

    /// <summary>
    /// 0-3: vip
    /// </summary>
    public Sprite[] sprts;

    private bool isFromDateSelect = true;
    private Dictionary<string, string> gameName = new Dictionary<string, string>();
    private string gameNameToSearch = "all";

    /// <summary>
    /// 0: page
    /// </summary>
    private int[] numList = new int[2];

    public Button nextBtn;
    public Button preBtn;

    private void OnEnable()
    {
        numList[0] = 1;
        nextBtn.interactable = true;
        preBtn.interactable = true;

        if (CPlayer.loginType == "fb" || CPlayer.loginType == "")
        {
            plGojs[5].SetActive(false);
        }
        else
        {
            plGojs[5].SetActive(true);
        }
        plGojs[6].SetActive(!CPlayer.hidePayment);
        string t1 = DateTime.Now.AddDays(-3).ToString("dd-MM-yyyy");
        string t2 = DateTime.Now.ToString("dd-MM-yyyy");
        plTxts[5].text = t1;
        plTxts[6].text = t2;
        plTxts[7].text = "Tất cả";
        DOTween.To(() => plRtfs[4].anchoredPosition, x => plRtfs[4].anchoredPosition = x, new Vector2(0, 0), .35f);
        DOTween.To(() => plRtfs[0].anchoredPosition, x => plRtfs[0].anchoredPosition = x, new Vector2(0, 0), .35f);
        DOTween.To(() => plRtfs[3].anchoredPosition, x => plRtfs[3].anchoredPosition = x, new Vector2(0, 0), .35f);

        loadData();
    }

    private void loadData()
    {

        var req_info = new OutBounMessage("PLAYER_PROFILE");
        //Debug.Log("WRITE LONG = " + CPlayer.id);
        //App.trace("PPLAYER ID = " + CPlayer.id);
        req_info.addHead();
        req_info.writeLong(CPlayer.id);
        req_info.writeByte(0x0f);
        req_info.writeAcii("");


        App.ws.send(req_info.getReq(), delegate (InBoundMessage res_PLAYER_INFO)
        {
            var nickName = res_PLAYER_INFO.readAscii();
            var fullName = res_PLAYER_INFO.readString();
            var avatar = res_PLAYER_INFO.readAscii();
            var isMale = res_PLAYER_INFO.readByte() == 1;
            var dateOfBirth = res_PLAYER_INFO.readAscii();
            var message = res_PLAYER_INFO.readString();
            var chipBalance = res_PLAYER_INFO.readLong();
            var starBalance = res_PLAYER_INFO.readLong();
            string phone = res_PLAYER_INFO.readAscii();
            if (CPlayer.loginType == "fb")
            {
                plTxts[0].text = nickName;
            }
            else
            {
                plTxts[0].text = App.formatNickName(nickName, 12);
            }
            //plTxts[0].text = fullName != "" ? App.formatNickName(fullName, 12) : App.formatNickName(nickName, 12);
            plTxts[1].text = App.formatMoney(chipBalance.ToString());
            plImgs[0].sprite = CPlayer.avatarSpriteToSave;
            if (phone == "")
            {
                if (CPlayer.hidePayment == false)
                    plGojs[1].SetActive(true);
                plTxts[11].gameObject.SetActive(false);

            }
            else
            {
                plGojs[1].SetActive(false);
                CPlayer.phoneNum = phone;
                plGojs[4].SetActive(true);
                plTxts[11].text = "SĐT: " + CPlayer.phoneNum;
                plTxts[11].gameObject.SetActive(true);
                GetVip();
            }
            DOTween.To(() => plRtfs[1].anchoredPosition, x => plRtfs[1].anchoredPosition = x, new Vector2(12, 19), .35f);

        });

        /*
        if (CPlayer.phoneNum == "")
        {
            plGojs[1].SetActive(true);
        }
        else
        {
            var req_VIP = new OutBounMessage("CASH_OUT.GET_PLAYER_VIP_POINT");
            req_VIP.addHead();
            req_VIP.writeString(CPlayer.nickName.ToLower());
            App.ws.send(req_VIP.getReq(), delegate (InBoundMessage res_VIP)
            {
                App.trace("RECV [CASH_OUT.GET_PLAYER_VIP_POINT]", "red");
                string tmp = res_VIP.readString();
                int point = res_VIP.readInt();
                int nextLevel = res_VIP.readInt();
                App.trace(tmp + "|point = " + point + "|nexxt = " + nextLevel, "red");

                plImgs[1].fillAmount = 1f * point / nextLevel;
                plTxts[9].text = tmp;
                plTxts[10].text = point + "/" + nextLevel;
                //tmp = "VIP 3";
                try
                {
                    int vipNum = int.Parse(tmp.Substring(4));
                    if(vipNum < 3)
                    {
                        plImgs[2].sprite = sprts[0];
                    } else if(vipNum < 6)
                    {
                        plImgs[2].sprite = sprts[1];
                    }
                    else
                    {
                        plImgs[2].sprite = sprts[2];
                    }
                }
                catch
                {

                }
            });
            plGojs[4].SetActive(true);
        }
        */
        DOTween.To(() => plRtfs[1].anchoredPosition, x => plRtfs[1].anchoredPosition = x, new Vector2(12, 19), .35f);

        LoadHis();
    }

    private void LoadHis()
    {
        var req_his = new OutBounMessage("MATCH.HISTORY");
        req_his.addHead();
        req_his.writeString(gameNameToSearch);                                     //GET ALL HIS
        req_his.writeByte(numList[0]);                                           //Page num
        string t1 = DateTime.Now.AddDays(-3).ToString("dd-MM-yyyy");
        string t2 = DateTime.Now.ToString("dd-MM-yyyy");
        req_his.writeString(t1);                                         //Start date to get
        req_his.writeString(t2);                                         //End date to get

        App.ws.send(req_his.getReq(), delegate (InBoundMessage res_info)
        {

            foreach (Transform rtf in plGojs[0].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != plGojs[0].name)
                {
                    Destroy(rtf.gameObject);
                }
            }

            foreach (Transform rtf in plGojs[7].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != plGojs[7].name)
                {
                    Destroy(rtf.gameObject);
                }
            }

            //res_info.readByte();
            App.trace("[RECV] MATCH.HISTORY");
            int count = res_info.readByte();
            //App.trace("PHAC " + count,"yellow");
            for (int i = 0; i < count; i++)
            {
                //int index = res_info.readInt();     //index
                long id = res_info.readLong();
                string time = res_info.readStrings();
                string gameName = res_info.readStrings();
                string betTotal = res_info.readStrings();
                string arise = res_info.readStrings();
                string money = res_info.readStrings();


                GameObject goj = Instantiate(plGojs[0], plGojs[0].transform.parent, false);
                Text[] arr = goj.GetComponentsInChildren<Text>();
                arr[0].text = id.ToString();
                arr[1].text = time;
                arr[2].text = gameName;
                arr[3].text = arise;
                arr[4].text = money;
                goj.SetActive(true);

            }

            string[] gameNameLabel = res_info.readStrings().Split(',');
            string[] gameID = res_info.readStrings().Split(',');
            this.gameName.Clear();


            for (int j = 0; j < gameNameLabel.Length; j++)
            {

                gameName.Add(gameID[j], gameNameLabel[j]);
                GameObject hisLabelGame = Instantiate(plGojs[7], plGojs[7].transform.parent, false);
                hisLabelGame.GetComponentInChildren<Text>().text = gameNameLabel[j];
                hisLabelGame.name = gameID[j];
                Button btn = hisLabelGame.GetComponentInChildren<Button>();
                btn.onClick.AddListener(() =>
                {
                    DoGameSelect(hisLabelGame.name);
                });
                hisLabelGame.SetActive(true);
            }


            DOTween.To(() => plRtfs[2].anchoredPosition, x => plRtfs[2].anchoredPosition = x, new Vector2(190, 19), .35f);
            plTxts[12].text = numList[0].ToString();
        });
    }

    private void GetVip()
    {
        if (CPlayer.phoneNum == "")
        {
            plGojs[1].SetActive(true);
        }
        else
        {
            var req_VIP = new OutBounMessage("CASH_OUT.GET_PLAYER_VIP_POINT");
            req_VIP.addHead();
            req_VIP.writeString(CPlayer.nickName.ToLower());
            App.ws.send(req_VIP.getReq(), delegate (InBoundMessage res_VIP)
            {
                App.trace("RECV [CASH_OUT.GET_PLAYER_VIP_POINT]", "red");
                string tmp = res_VIP.readString();
                int point = res_VIP.readInt();
                int nextLevel = res_VIP.readInt();
                App.trace(tmp + "|point = " + point + "|nexxt = " + nextLevel, "red");

                //plImgs[1].fillAmount = 1f * point / nextLevel;
                // plTxts[9].text = tmp;
                //  plTxts[10].text = point + "/" + nextLevel;
                plTxts[10].text = point.ToString();
                //tmp = "VIP 3";
                /* try
                 {
                     int vipNum = int.Parse(tmp.Substring(4));
                     if (vipNum < 3)
                     {
                         plImgs[2].sprite = sprts[0];
                     }
                     else if (vipNum < 6)
                     {
                         plImgs[2].sprite = sprts[1];
                     }
                     else
                     {
                         plImgs[2].sprite = sprts[2];
                     }
                 }
                 catch
                 {

                 }*/
            });
            plGojs[4].SetActive(true);
        }
    }

    public void Close(bool openRecharge = false)
    {
        numList[0] = 1;
        plTxts[12].text = 1.ToString();
        gameNameToSearch = "all";
        DOTween.To(() => plRtfs[4].anchoredPosition, x => plRtfs[4].anchoredPosition = x, new Vector2(0, 160), .35f);
        DOTween.To(() => plRtfs[0].anchoredPosition, x => plRtfs[0].anchoredPosition = x, new Vector2(0, 160), .35f);
        DOTween.To(() => plRtfs[1].anchoredPosition, x => plRtfs[1].anchoredPosition = x, new Vector2(-500, 19), .35f);
        DOTween.To(() => plRtfs[2].anchoredPosition, x => plRtfs[2].anchoredPosition = x, new Vector2(1500, 19), .35f);
        DOTween.To(() => plRtfs[3].anchoredPosition, x => plRtfs[3].anchoredPosition = x, new Vector2(0, -200), .35f).OnComplete(() =>
        {

            plGojs[4].SetActive(false);         //hide vip bar
            plGojs[1].SetActive(false);         //hide btn authen
            plTxts[11].gameObject.SetActive(false);         //hide phone text
            gameObject.SetActive(false);
            LobbyControl.instance.openRechareValue(openRecharge);
            LobbyControl.instance.OpenPlayerInfo(false);
        });
    }

    public void DoAction(string act)
    {
        switch (act)
        {
            case "openChangePass":
                plGojs[2].SetActive(true);

                plRtfs[5].gameObject.SetActive(true);
                plRtfs[5].DOScale(1, .35f).SetEase(Ease.OutBack);


                break;
            case "closeChangePass":
                plRtfs[5].DOScale(.5f, .35f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    plRtfs[5].gameObject.SetActive(false);
                    plGojs[2].SetActive(false);
                });
                break;
            case "doChangePass":
                #region //CHECK ERROR
                if (plIpfs[0].text.Length == 0)
                {
                    DOTween.Kill("error_old");
                    plTxts[2].color = plClolors[0];
                    plTxts[2].gameObject.SetActive(true);
                    plTxts[2].DOFade(0, 2f).OnComplete(() =>
                    {
                        plTxts[2].gameObject.SetActive(false);
                    }).SetId("error_old");
                    //App.showErr("Tên đăng nhập không được để trống");
                    return;
                }

                if (plIpfs[1].text.Length == 0)
                {
                    DOTween.Kill("error_new");
                    plTxts[3].color = plClolors[0];
                    plTxts[3].gameObject.SetActive(true);
                    plTxts[3].DOFade(0, 2f).OnComplete(() =>
                    {
                        plTxts[3].gameObject.SetActive(false);
                    }).SetId("error_new");
                    //App.showErr("Tên đăng nhập không được để trống");
                    return;
                }

                if (plIpfs[2].text.Length == 0)
                {
                    DOTween.Kill("error_re");
                    plTxts[4].color = plClolors[0];
                    plTxts[4].gameObject.SetActive(true);
                    plTxts[4].DOFade(0, 2f).OnComplete(() =>
                    {
                        plTxts[4].gameObject.SetActive(false);
                    }).SetId("error_re");
                    //App.showErr("Tên đăng nhập không được để trống");
                    return;
                }

                if (!plIpfs[1].text.Trim().Equals(plIpfs[2].text.Trim()))
                {
                    //App.showErr("Hai mật khẩu không khớp");
                    App.showErr(App.listKeyText["WARN_PASSWORD_NOT_MATCH"]);
                    return;
                }
                #endregion


                DoAction("closeChangePass");
                OutBounMessage req_CHANGE_PASS = new OutBounMessage("CHANGE_PASSWORD");
                req_CHANGE_PASS.addHead();
                req_CHANGE_PASS.writeString(plIpfs[0].text);
                req_CHANGE_PASS.writeString(plIpfs[1].text);
                App.ws.send(req_CHANGE_PASS.getReq(), delegate (InBoundMessage res_CHANGE_PASSWORD)
                {
                    string rememberPassCheck = PlayerPrefs.GetString("rememberPass");
                    if (rememberPassCheck == "true")
                    {
                        PlayerPrefs.SetString("pass", plIpfs[1].text);
                    }
                    //App.showErr("Đổi mật khẩu thành công");
                    App.showErr(App.listKeyText["INFO_PASSWORD_CHANGED_SUCCESS"]);
                });
                break;
            case "authen":
                LoadingControl.instance.DoAuthen();
                break;
            case "recharge":
                Close(true);
                break;
            case "openChangeAva":
                if (CPlayer.loginType == "fb")
                    return;
                for (int i = 0; i < 30; i++)
                {
                    int tmp = i;
                    GameObject goj = Instantiate(plGojs[3], plGojs[3].transform.parent, false);
                    goj.GetComponent<Image>().sprite = LoadingControl.instance.avaSpriteLs[i];
                    Button btn = goj.GetComponent<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        DoAction("closeChangeAva");
                        DoChangeAva(tmp);
                    });
                    goj.SetActive(true);

                }
                plGojs[2].SetActive(true);
                plRtfs[6].gameObject.SetActive(true);
                plRtfs[6].DOScale(1, .35f).SetEase(Ease.OutBack);
                break;
            case "closeChangeAva":
                plRtfs[6].DOScale(.5f, .35f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    plRtfs[6].gameObject.SetActive(false);
                    plGojs[2].SetActive(false);
                });
                break;
            case "openDateSelect":
                plTxts[8].text = App.listKeyText["DATE_REQUIRED"];// "(Vui lòng chọn ngày / tháng / năm)";
                plGojs[2].SetActive(true);
                plRtfs[7].gameObject.SetActive(true);
                plRtfs[7].DOScale(1, .35f).SetEase(Ease.OutBack);
                isFromDateSelect = true;
                break;
            case "openDateSelect2":
                plTxts[8].text = App.listKeyText["DATE_REQUIRED"]; //"(Vui lòng chọn ngày / tháng / năm)";
                plGojs[2].SetActive(true);
                plRtfs[7].gameObject.SetActive(true);
                plRtfs[7].DOScale(1, .35f).SetEase(Ease.OutBack);
                isFromDateSelect = false;
                break;
            case "closeDateSelect":
                plRtfs[7].DOScale(.5f, .35f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    plRtfs[7].gameObject.SetActive(false);
                    plGojs[2].SetActive(false);
                    plIpfs[3].text = plIpfs[4].text = plIpfs[5].text = "";
                });
                break;
            case "doDateSelect":
                if (plIpfs[3].text == "" | plIpfs[4].text == "" | plIpfs[5].text == ""
                    | plIpfs[5].text.Length < 4 || int.Parse(plIpfs[3].text) < 1 | int.Parse(plIpfs[4].text) < 1 | int.Parse(plIpfs[5].text) < 1920
                    | int.Parse(plIpfs[3].text) > 31 | int.Parse(plIpfs[4].text) > 12 | int.Parse(plIpfs[5].text) > DateTime.Now.Year)
                {
                    //App.showErr("Sai định dạng");
                    App.showErr(App.listKeyText["WARN_FORMAT_INVALID"]);
                    return;
                }

                int date = int.Parse(plIpfs[3].text);
                int month = int.Parse(plIpfs[4].text);
                int year = int.Parse(plIpfs[5].text);
                List<int> arrMonth = new List<int> { 1, 3, 5, 7, 8, 10, 12 };

                if (month == 2 && date > 29)
                {
                    //App.showErr("Sai định dạng");
                    App.showErr(App.listKeyText["WARN_FORMAT_INVALID"]);
                    return;
                }

                if (!arrMonth.Contains(month) && date > 30)
                {
                    //App.showErr("Sai định dạng");
                    App.showErr(App.listKeyText["WARN_FORMAT_INVALID"]);
                    return;
                }
                if (year % 4 != 0 && month == 2 && date > 28)
                {
                    //App.showErr("Sai định dạng");
                    App.showErr(App.listKeyText["WARN_FORMAT_INVALID"]);
                    return;
                }
                int index = isFromDateSelect ? 5 : 6;
                plTxts[index].text = string.Format("{0}-{1}-{2}", date < 10 ? "0" + date : date.ToString(), month < 10 ? "0" + month : month.ToString()
                    , year.ToString());
                numList[0] = 1;
                DoAction("closeDateSelect");
                break;
            case "openGameSelect":
                gameNameToSearch = "all";
                plGojs[2].SetActive(true);
                plRtfs[8].gameObject.SetActive(true);
                plRtfs[8].DOScale(1, .35f).SetEase(Ease.OutBack);
                break;
            case "closeGameSelect":
                numList[0] = 1;
                plRtfs[8].DOScale(.5f, .35f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    plRtfs[8].gameObject.SetActive(false);
                    plGojs[2].SetActive(false);
                });
                break;
            case "doSearch":
                numList[0] = 1;
                plTxts[12].text = 1.ToString();
                var req_his = new OutBounMessage("MATCH.HISTORY");
                req_his.addHead();
                req_his.writeString(gameNameToSearch);
                req_his.writeByte(numList[0]);                   //Page
                req_his.writeString(plTxts[5].text);     //Start date to get
                req_his.writeString(plTxts[6].text);     //End date to get
                App.ws.send(req_his.getReq(), delegate (InBoundMessage res_info)
                {

                    foreach (Transform rtf in plGojs[0].transform.parent)       //Delete exits element before
                    {
                        if (rtf.gameObject.name != plGojs[0].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }

                    //res_info.readByte();
                    App.trace("[RECV] MATCH.HISTORY");
                    int count = res_info.readByte();
                    App.trace("PHAC " + count);
                    for (int i = 0; i < count; i++)
                    {
                        //int index = res_info.readInt();     //index
                        long id = res_info.readLong();
                        string time = res_info.readStrings();
                        string gameName = res_info.readStrings();
                        string betTotal = res_info.readStrings();
                        string arise = res_info.readStrings();
                        string money = res_info.readStrings();

                        //App.trace("| id = " + id + "|time = " + time + "| gameName = " + gameName + "|arise = " + arise + "|money = " + money);

                        GameObject goj = Instantiate(plGojs[0], plGojs[0].transform.parent, false);
                        Text[] arr = goj.GetComponentsInChildren<Text>();
                        arr[0].text = id.ToString();
                        arr[1].text = time;
                        arr[2].text = gameName;
                        arr[3].text = arise;
                        arr[4].text = money;
                        goj.SetActive(true);
                        //goj.transform.DOShakeScale(1.2f, .25f);
                    }
                });
                break;
        }
    }

    private void DoChangeAva(int num)
    {
        CPlayer.avatarSpriteToSave = LoadingControl.instance.avaSpriteLs[num];
        plImgs[0].sprite = CPlayer.avatarSpriteToSave;
        LobbyControl.instance.lobbyImgList[0].overrideSprite = CPlayer.avatarSpriteToSave;
    }

    public void DoGameSelect(string t)
    {
        plTxts[7].text = gameName[t];
        gameNameToSearch = t;
        DoAction("closeGameSelect");
        LoadHis();
    }

    private void LoadHisByDate()
    {
        var req_his = new OutBounMessage("MATCH.HISTORY");
        req_his.addHead();
        req_his.writeString(gameNameToSearch);                                     //GET ALL HIS
        req_his.writeByte(numList[0]);                                           //Page num

        req_his.writeString(plTxts[5].text);                                         //Start date to get
        req_his.writeString(plTxts[6].text);                                         //End date to get

        App.ws.send(req_his.getReq(), delegate (InBoundMessage res_info)
        {



            //res_info.readByte();
            App.trace("[RECV] MATCH.HISTORY");
            int count = res_info.readByte();

            if (count == 0)
            {
                numList[0]--;
                nextBtn.interactable = false;
                return;
            }
            else
            {
                nextBtn.interactable = true;
            }
            foreach (Transform rtf in plGojs[0].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != plGojs[0].name)
                {
                    Destroy(rtf.gameObject);
                }
            }

            foreach (Transform rtf in plGojs[7].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != plGojs[7].name)
                {
                    Destroy(rtf.gameObject);
                }
            }



            //App.trace("PHAC " + count,"yellow");
            for (int i = 0; i < count; i++)
            {
                //int index = res_info.readInt();     //index
                long id = res_info.readLong();
                string time = res_info.readStrings();
                string gameName = res_info.readStrings();
                string betTotal = res_info.readStrings();
                string arise = res_info.readStrings();
                string money = res_info.readStrings();



                GameObject goj = Instantiate(plGojs[0], plGojs[0].transform.parent, false);
                Text[] arr = goj.GetComponentsInChildren<Text>();
                arr[0].text = id.ToString();
                arr[1].text = time;
                arr[2].text = gameName;
                arr[3].text = arise;
                arr[4].text = money;

                goj.SetActive(true);
            }

            string[] gameNameLabel = res_info.readStrings().Split(',');
            string[] gameID = res_info.readStrings().Split(',');
            this.gameName.Clear();
            for (int j = 0; j < gameNameLabel.Length; j++)
            {
                gameName.Add(gameID[j], gameNameLabel[j]);
                GameObject hisLabelGame = Instantiate(plGojs[7], plGojs[7].transform.parent, false);
                hisLabelGame.GetComponentInChildren<Text>().text = gameNameLabel[j];
                hisLabelGame.name = gameID[j];
                Button btn = hisLabelGame.GetComponentInChildren<Button>();
                btn.onClick.AddListener(() =>
                {
                    DoGameSelect(hisLabelGame.name);
                });
                hisLabelGame.SetActive(true);
            }


            DOTween.To(() => plRtfs[2].anchoredPosition, x => plRtfs[2].anchoredPosition = x, new Vector2(190, 19), .35f);
            plTxts[12].text = numList[0].ToString();
        });
    }
    public void ChangePage(int side)
    {
        if (side == 0 && numList[0] == 1)
            return;
        if (side == 0)
        {
            numList[0]--;
        }
        else
            numList[0]++;

        //plTxts[12].text = numList[0].ToString();
        //LoadHis();
        LoadHisByDate();
    }
}
