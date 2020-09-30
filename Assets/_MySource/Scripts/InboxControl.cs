using DG.Tweening;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InboxControl : MonoBehaviour {

    /// <summary>
    /// 0: |1: Main0|2: Main-Search|3: search-ele|4: Main-Pm|5-6: Main-pm-ele
    /// </summary>
    public GameObject[] gojs;

    /// <summary>
    /// 0: searchPlayer
    /// </summary>
    public InputField[] ipfs;

    /// <summary>
    /// 0: onl|1: off|2: admin
    /// </summary>
    public Sprite[] sprts;

    /// <summary>
    /// 0:main-pm-header|
    /// </summary>
    public Text[] txts;

    /// <summary>
    /// 0: sender-ava
    /// </summary>
    public Image[] imgs;

    /// <summary>
    /// 0: all|1: sys|2: support|3: personal
    /// </summary>
    public Toggle[] togs;

    public Color[] colors;

    public ScrollRect scr;
    /// <summary>
    /// 0: nextPage|1: lastPage
    /// </summary>
    public Button[] buttons;

    /// <summary>
    /// 0: system
    /// </summary>
    public ScrollRect[] scrollRects;

    /// <summary>
    /// 0: currTargetId
    /// </summary>
    private string[] txtData = new string[1];

    /// <summary>
    /// 0: currMessType
    /// </summary>
    private int[] numLs = new int[2];


    private void OnEnable()
    {
        gojs[1].SetActive(true);
        gojs[2].SetActive(false);
        gojs[4].SetActive(false);

        transform.DOScale(1, .1f).SetEase(Ease.OutBack);
        numLs[0] = -1;
        ChangeType(1);
        togs[1].isOn = true;

        //Set mess noti to false
        LobbyControl.instance.lobbyTxtList[2].transform.parent.gameObject.SetActive(false);

        var reqPM = new OutBounMessage("PM.SUBS");
        reqPM.addHead();
        App.ws.send(reqPM.getReq(), delegate (InBoundMessage res)
        {

        },false);
        
        OutBounMessage req_PM = new OutBounMessage("PM.CREATE");
        req_PM.addHead();
        App.ws.delHandler(req_PM.getReq());
        App.ws.sendHandler(req_PM.getReq(), delegate (InBoundMessage res_PM)
        {
            long id = res_PM.readLong();
            string nickName = res_PM.readAscii();
            string content = res_PM.readStrings(true);
            App.trace("RECV [PM.CREATE] nick = " + nickName + "|content = " + content, "green");

            if (LobbyControl.instance.lobbyGojList[1].activeSelf && numLs[1] == id)
            {
                if (nickName == CPlayer.nickName)
                {
                    GameObject goj = Instantiate(gojs[6], gojs[6].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();

                    txtArr[0].text = txtArr[1].text = content;
                    ////App.trace("Phắc " + txtArr[0].preferredWidth,"green");
                    float w = txtArr[0].preferredWidth;
                    float h = txtArr[0].preferredHeight;
                    if (w < 600)
                        txtArr[1].GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                    //goj.transform.SetAsFirstSibling();
                    goj.SetActive(true);
                }
                else
                {
                    GameObject goj = Instantiate(gojs[5], gojs[5].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();
                    txtArr[0].text = content;
                    float w = txtArr[0].preferredWidth;
                    float h = txtArr[0].preferredHeight;
                    if (w < 600)
                        txtArr[0].GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                    //goj.transform.SetAsFirstSibling();
                    goj.SetActive(true);
                    ////App.trace("Phắc " + txtArr[0].preferredWidth, "yellow");

                }
                scr.DOVerticalNormalizedPos(0, .2f);
            }
        });
    }

    public void ChangeType(int id)
    {
        if(numLs[0] != id)
            LoadMessFrom(id);
        numLs[0] = id;
        togs[id].interactable = false;
        //togs[id].isOn = true;
        for(int i = 0; i < 4; i++)
        {
            if(i != id)
            {
                togs[i].interactable = true;
            }
        }
        
        
    }

    private void LoadData()
    {
        /*
        var req_PM_LIST = new OutBounMessage("PM.LIST");
        req_PM_LIST.addHead();
        req_PM_LIST.writeByte(0);               //type
        req_PM_LIST.writeShort(0);              //start index
        App.ws.send(req_PM_LIST.getReq(), delegate (InBoundMessage res_PM_LIST)
        {
            int count = res_PM_LIST.readByte();
            for (int i = 0; i < count; i++)
            {
                long index = res_PM_LIST.readLong();
                long targetId = res_PM_LIST.readLong();
                string target = res_PM_LIST.readAscii();
                string content = res_PM_LIST.readString();
                long time = res_PM_LIST.readLong();
                bool isRead = res_PM_LIST.readByte() == 1;
                bool isAdmin = res_PM_LIST.readByte() == 1;

            }

            int prePageId = res_PM_LIST.readShort();
            int nextPageId = res_PM_LIST.readShort();

        });*/
        /*
        var req = new OutBounMessage("PM.LIST");
        req.addHead();
        req.writeByte(3);   //Load mess targetId sent for me
        req.writeLong(1);   //targetId
        req.writeByte(5);    //lineCount
        req.writeByte(20);    //limit
        return;*/

        
    }

    private int lastPageInbox;
    private int nextPageInbox;

    private void LoadMessFrom(int type)
    {
        switch (type)
        {
            #region //ALL
            case 0:         
                var req_PM_LIST = new OutBounMessage("PM.LIST");
                req_PM_LIST.addHead();
                req_PM_LIST.writeByte(2);               //type with last mess
                req_PM_LIST.writeByte(0);              //skip
                req_PM_LIST.writeByte(10);              //limit
                App.ws.send(req_PM_LIST.getReq(), delegate (InBoundMessage res_PM_LIST)
                {
                    foreach (Transform rtf in gojs[0].transform.parent)
                    {
                        if (rtf.gameObject.name != gojs[0].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }



                    int count = res_PM_LIST.readByte();
                    App.trace("[RECV] PM.LIST | count = " + count, "red");
                    for (int i = 0; i < count; i++)
                    {
                        long time = res_PM_LIST.readLong();
                        long senderId = res_PM_LIST.readLong();
                        string senderName = res_PM_LIST.readAscii();
                        int avaId = res_PM_LIST.readShort();
                        string avaLink = res_PM_LIST.readAscii();
                        long logOutTime = res_PM_LIST.readLong();
                        string content = res_PM_LIST.readString();
                        bool isRead = res_PM_LIST.readByte() == 1;
                        bool isOnline = res_PM_LIST.readByte() == 1;

                        //App.trace("time = " + time + "|sender = " + senderName + "|ava = " + avaId + "|link = " + avaLink + "|logOutTime = " + logOutTime + "|content = " + content + "|isRead = " + isRead + "|isOnl = " + isOnline);
                        GameObject goj = Instantiate(gojs[0], gojs[0].transform.parent, false);
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = senderName;
                        txtArr[1].text = App.formatNickName(content, 100);
                        txtArr[2].text = new DateTime(1970, 1, 1).AddMilliseconds(time).ToLocalTime().ToString("dd-MM-yyyy\n HH:mm:ss");

                        Image[] imgArr = goj.GetComponentsInChildren<Image>();
                        if (senderName.ToLower().StartsWith("admin"))
                            imgArr[1].sprite = sprts[2];
                        else
                            StartCoroutine(App.loadImg(imgArr[1], App.getAvatarLink2(avaLink, avaId)));
                        Button btn = goj.GetComponent<Button>();
                        btn.onClick.AddListener(() =>
                        {
                            txtData[0] = senderName;
                            txts[0].text = senderName;
                            gojs[1].SetActive(false);
                            gojs[4].SetActive(true);
                            numLs[1] = (int)senderId;
                            LoadPmMess(senderId, imgArr[1].overrideSprite);
                        });
                        goj.SetActive(true);
                    }
                });
                break;
            #endregion

            #region //Sys
            case 1:
                var req_PM_SYS = new OutBounMessage("PM.LIST");
                req_PM_SYS.addHead();
                req_PM_SYS.writeByte(4);               //sys mess
                req_PM_SYS.writeShort(1);              //start
                App.ws.send(req_PM_SYS.getReq(), delegate (InBoundMessage res_PM_SYS)
                {
                    foreach (Transform rtf in gojs[0].transform.parent)
                    {
                        if (rtf.gameObject.name != gojs[0].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }
                    int count = res_PM_SYS.readByte();
                    App.trace("[RECV] PM.LIST SYS| count = " + count, "red");
                    for (int i = 0; i < count; i++)
                    {
                        var pmId = res_PM_SYS.readLong();
                        var targetId = res_PM_SYS.readLong();
                        var target = res_PM_SYS.readAscii();
                        var content = res_PM_SYS.readString();
                        var time = res_PM_SYS.readLong();
                        //var date = DateTools.format(Date.fromTime(time), "%H:%M %d/%m");
                        //var day = DateTools.format(Date.fromTime(time), "%d/%m/%Y");
                        //var hour = DateTools.format(Date.fromTime(time), "%H:%M");
                        var read = res_PM_SYS.readByte() == 1;
                        var targetIsAdmin = res_PM_SYS.readByte() == 1;
                        //App.trace("pmId = " + pmId + "|targetId = " + targetId + "|target = " + target + "|content = " + content + "|time = " + time);
                        GameObject goj = Instantiate(gojs[7], gojs[7].transform.parent, false);
                        //LinkUGUIText contentTxt = goj.GetComponentInChildren<LinkUGUIText>();
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        //contentTxt.onClicked.AddListener((url) => {
                        //    Debug.Log("________________________________>>>>>>>>>>>>>");
                        //    Application.OpenURL(url);
                        //});
                        txtArr[0].text = "Admin";
                        //contentTxt.fontSize = 26;
                        //if (!read)
                        //contentTxt.color = colors[0];
                        //contentTxt.SetText(content);
                        txtArr[1].text = content;
                        RectTransform rtf = goj.GetComponent<RectTransform>();
                        txtArr[2].text = new DateTime(1970, 1, 1).AddMilliseconds(time).ToLocalTime().ToString("dd-MM-yyyy\n HH:mm:ss");
                        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, 295);

                        //StartCoroutine(changeSize(rtf.sizeDelta, contentTxt, (callback) =>
                        //{
                        //    rtf.sizeDelta = callback;
                        goj.SetActive(true);
                        //}));
                    }
                    int previousIndex = res_PM_SYS.readShort();
                    //App.trace("start in:" + previousIndex, "yellow");
                    int nextIndex = res_PM_SYS.readShort();
                    //App.trace("next in:" + nextIndex, "yellow");
                    lastPageInbox = previousIndex;
                    nextPageInbox = nextIndex;
                    buttons[1].interactable = false;
                    buttons[0].interactable = false;
                });
                break;
            #endregion

            #region //SUPORT
            case 2:
                var req_PM_SUP = new OutBounMessage("PM.LIST");
                req_PM_SUP.addHead();
                req_PM_SUP.writeByte(5);               //type with last mess
                req_PM_SUP.writeByte(0);              //skip
                req_PM_SUP.writeByte(10);              //limit
                App.ws.send(req_PM_SUP.getReq(), delegate (InBoundMessage res_PM_SUB)
                {
                    foreach (Transform rtf in gojs[0].transform.parent)
                    {
                        if (rtf.gameObject.name != gojs[0].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }



                    int count = res_PM_SUB.readByte();
                    App.trace("[RECV] PM.LIST SUB| count = " + count, "red");
                    for (int i = 0; i < count; i++)
                    {
                        long time = res_PM_SUB.readLong();
                        long senderId = res_PM_SUB.readLong();
                        string senderName = res_PM_SUB.readAscii();
                        int avaId = res_PM_SUB.readShort();
                        string avaLink = res_PM_SUB.readAscii();
                        long logOutTime = res_PM_SUB.readLong();
                        string content = res_PM_SUB.readString();
                        bool isRead = res_PM_SUB.readByte() == 1;
                        bool isOnline = res_PM_SUB.readByte() == 1;

                        //App.trace("time = " + time + "|sender = " + senderName + "|ava = " + avaId + "|link = " + avaLink + "|logOutTime = " + logOutTime + "|content = " + content + "|isRead = " + isRead + "|isOnl = " + isOnline);
                        GameObject goj = Instantiate(gojs[7], gojs[7].transform.parent, false);
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = senderName;
                        txtArr[1].text = App.formatNickName(content, 100);
                        txtArr[2].text = new DateTime(1970, 1, 1).AddMilliseconds(time).ToLocalTime().ToString("dd-MM-yyyy\n HH:mm:ss");

                        Image[] imgArr = goj.GetComponentsInChildren<Image>();
                        //StartCoroutine(App.loadImg(imgArr[1], App.getAvatarLink2(avaLink, avaId)));
                        //imgArr[1].sprite = sprts[2];
                        Button btn = goj.GetComponent<Button>();
                        btn.onClick.AddListener(() =>
                        {
                            txtData[0] = senderName;
                            txts[0].text = senderName;
                            gojs[1].SetActive(false);
                            gojs[4].SetActive(true);
                            LoadPmMess(senderId, imgArr[1].overrideSprite);
                        });
                        goj.SetActive(true);
                    }
                });
                break;
            #endregion

            #region //PERSONAL
            case 3:
                var req_PM_PER = new OutBounMessage("PM.LIST");
                req_PM_PER.addHead();
                req_PM_PER.writeByte(0);               //type with last mess
                req_PM_PER.writeByte(0);              //skip
                req_PM_PER.writeByte(10);              //limit
                App.ws.send(req_PM_PER.getReq(), delegate (InBoundMessage res_PM_PER)
                {
                    foreach (Transform rtf in gojs[0].transform.parent)
                    {
                        if (rtf.gameObject.name != gojs[0].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }



                    int count = res_PM_PER.readByte();
                   Debug.Log("[RECV] PM.LIST PER| count = " + count);
                    for (int i = 0; i < count; i++)
                    {
                        long time = res_PM_PER.readLong();
                        long senderId = res_PM_PER.readLong();
                        string senderName = res_PM_PER.readAscii();
                        int avaId = res_PM_PER.readShort();
                        string avaLink = res_PM_PER.readAscii();
                        long logOutTime = res_PM_PER.readLong();
                        string content = res_PM_PER.readString();
                        bool isRead = res_PM_PER.readByte() == 1;
                        bool isOnline = res_PM_PER.readByte() == 1;

                        Debug.Log("time = " + time + "|sender = " + senderName + "|ava = " + avaId + "|link = " + avaLink + "|logOutTime = " + logOutTime + "|content = " + content + "|isRead = " + isRead + "|isOnl = " + isOnline);
                        GameObject goj = Instantiate(gojs[0], gojs[0].transform.parent, false);
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = senderName;
                        txtArr[1].text = App.formatNickName(content, 100);
                        txtArr[2].text = new DateTime(1970, 1, 1).AddMilliseconds(time).ToLocalTime().ToString("dd-MM-yyyy\n HH:mm:ss");

                        Image[] imgArr = goj.GetComponentsInChildren<Image>();
                        StartCoroutine(App.loadImg(imgArr[1], App.getAvatarLink2(avaLink, avaId)));
                        Button btn = goj.GetComponent<Button>();
                        btn.onClick.AddListener(() =>
                        {
                            txtData[0] = senderName;
                            txts[0].text = senderName;
                            gojs[1].SetActive(false);
                            gojs[4].SetActive(true);
                            LoadPmMess(senderId, imgArr[1].overrideSprite);
                        });
                        goj.SetActive(true);
                    }
                });
                break;
                #endregion
        }
    }

    public void LoadMessSysFromPage(int next)
    {
                var req_PM_SYS = new OutBounMessage("PM.LIST");
                req_PM_SYS.addHead();
                req_PM_SYS.writeByte(4);               //sys mess
                req_PM_SYS.writeShort(Convert.ToInt16(next == 1 ? nextPageInbox : lastPageInbox));              //start
                App.ws.send(req_PM_SYS.getReq(), delegate (InBoundMessage res_PM_SYS)
                {
                    foreach (Transform rtf in gojs[0].transform.parent)
                    {
                        if (rtf.gameObject.name != gojs[0].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }
                    scrollRects[0].DOVerticalNormalizedPos(1, .2f);
                    int count = res_PM_SYS.readByte();
                    App.trace("[RECV] PM.LIST SYS| count = " + count, "red");
                    for (int i = 0; i < count; i++)
                    {
                        var pmId = res_PM_SYS.readLong();
                        var targetId = res_PM_SYS.readLong();
                        var target = res_PM_SYS.readAscii();
                        var content = res_PM_SYS.readStrings();
                        var time = res_PM_SYS.readLong();
                        var read = res_PM_SYS.readByte() == 1;
                        var targetIsAdmin = res_PM_SYS.readByte() == 1;
                        GameObject goj = Instantiate(gojs[7], gojs[7].transform.parent, false);
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = "Admin";
                        txtArr[1].fontSize = 30;
                        if (!read)
                            txtArr[1].color = colors[0];
                        txtArr[1].text = content;
                        RectTransform rtf = txtArr[1].GetComponent<RectTransform>();
                        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, txtArr[1].preferredHeight);
                        txtArr[2].text = new DateTime(1970, 1, 1).AddMilliseconds(time).ToLocalTime().ToString("dd-MM-yyyy\n HH:mm:ss");
                        rtf = goj.GetComponent<RectTransform>();
                        rtf.sizeDelta = new Vector2(rtf.sizeDelta.x, 150 + txtArr[1].preferredHeight);
                        goj.SetActive(true);
                    }
                    int previousIndex = res_PM_SYS.readShort();
                    //App.trace("start in:" + previousIndex, "blue");
                    int nextIndex = res_PM_SYS.readShort();
                    //App.trace("next in:" + nextIndex, "blue");
                    lastPageInbox = previousIndex;
                    nextPageInbox = nextIndex;

                    Debug.Log(lastPageInbox + "xx");
                    if (nextPageInbox < 0)
                        buttons[0].interactable = false;
                    else
                        buttons[0].interactable = true;
                    if (lastPageInbox < 0)
                        buttons[1].interactable = false;
                    else
                        buttons[1].interactable = true;
                });

    }

    public void Close()
    {
        numLs[1] = 0;
        if (gojs[1].activeSelf == false)
        {
            CloseFaking();
            return;
        }
        transform.DOScale(.5f, .1f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
            LoadingControl.instance.loadingGojList[21].SetActive(true);
            if(CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(true);
        });
    }

    public void CloseFaking()
    {
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void DoAction(string act)
    {
        switch (act)
        {
            case "NewMess":
                ipfs[0].text = "";
                gojs[1].SetActive(false);
                gojs[2].SetActive(true);
                break;

            #region SEARCH PLAYER
            case "SearchPlayer":
                string t = ipfs[0].text;
                if (t.Length == 0)
                    return;
                var req_PLAYER_LIST = new OutBounMessage("FRIEND.LIST");
                req_PLAYER_LIST.addHead();          ////0: Thêm bạn|1: Bạn bè|2: Lời mời
                req_PLAYER_LIST.writeByte(0);
                req_PLAYER_LIST.writeString(t);
                req_PLAYER_LIST.writeShort(1);      //start from 1 (page)
                App.ws.send(req_PLAYER_LIST.getReq(), delegate (InBoundMessage res_player_list)
                {

                    //App.trace("[RECV] FRIEND.LIST");
                    foreach (Transform rtf in gojs[3].transform.parent)       //Delete exits element before
                    {
                        if (rtf.gameObject.name != gojs[3].name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }

                    int count = res_player_list.readByte();
                    for (int i = 0; i < count; i++)
                    {
                        long playerId = res_player_list.readLong();
                        string nickName = res_player_list.readAscii();
                        int gender = res_player_list.readByte();
                        int avatarId = res_player_list.readShort();
                        string avatar = res_player_list.readAscii();
                        long chipBalance = res_player_list.readLong();
                        long starBalance = res_player_list.readLong();
                        var isOnline = res_player_list.readByte() == 1;
                        //App.trace("playerId = " + playerId + "|nick = " + nickName + "|gender = " + gender + "|");
                        if (i > 8)
                            continue;
                        GameObject goj = Instantiate(gojs[3], gojs[3].transform.parent, false);
                        Image[] imgArr = goj.GetComponentsInChildren<Image>();
                        imgArr[3].sprite = isOnline ? sprts[0] : sprts[1];
                        StartCoroutine(App.loadImg(imgArr[1], App.getAvatarLink2(avatar, avatarId)));
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = App.formatNickName(nickName, 15);
                        txtArr[1].text = isOnline ? "Online" : "Offline";

                        Button btn = goj.GetComponent<Button>();
                        btn.onClick.AddListener(() =>
                        {
                            txtData[0] = nickName;
                            txts[0].text = nickName;
                            gojs[2].SetActive(false);
                            gojs[4].SetActive(true);
                            LoadPmMess(playerId, imgArr[1].overrideSprite);
                        });

                        goj.SetActive(true);
                        
                    }

                    int previousIndex = res_player_list.readShort();
                    int nextIndex = res_player_list.readShort();
                });
                break;

            #endregion


            #region //SEND MESS
            case "Send":
                string content = ipfs[1].text;
                if (content.Length == 0)
                    return;
                var req_SEND = new OutBounMessage("PM.CREATE");
                req_SEND.addHead();
                ////App.trace("This is the man i want to send mess to: " + txtData[0],"red");
                req_SEND.writeAcii(txtData[0]);
                req_SEND.writeString(content);
                App.ws.send(req_SEND.getReq(), delegate (InBoundMessage res_SEND)
                {
                    App.trace("[RECV] PM.CREATE | FROM ME");
                    
                    ipfs[1].text = "";

                    GameObject goj = Instantiate(gojs[6], gojs[6].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();

                    txtArr[0].text = txtArr[1].text = content;
                    ////App.trace("Phắc " + txtArr[0].preferredWidth, "green");
                    float w = txtArr[0].preferredWidth;
                    float h = txtArr[0].preferredHeight;
                    if (w < 600)
                        txtArr[1].GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                    goj.SetActive(true);
                    scr.DOVerticalNormalizedPos(0, .2f);
                });
                break;
            #endregion
            default:
                break;
        }
    }

    private void LoadPmMess(long targetId, Sprite img)
    {
        foreach (Transform rtf in gojs[5].transform.parent)
        {
            if (rtf.gameObject.name != gojs[5].name && rtf.gameObject.name != gojs[6].name)
            {
                Destroy(rtf.gameObject);
            }
        }

        imgs[0].sprite = img;

        var req = new OutBounMessage("PM.LIST");
        req.addHead();
        req.writeByte(3);   //Load mess targetId sent for me
        req.writeLong(targetId);   //targetId
        req.writeByte(0);    //lineCount
        req.writeByte(10);    //limit
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            int count = res.readByte();
            App.trace("[RECV] PM.LIST PMMESS "+ count,"red");
            for (int i = 0; i < count; i++)
            {
                long senderId = res.readLong();
                string senderName = res.readAscii();
                string content = res.readStrings();
                //App.trace("senderId = " + senderId + "|senderName = " + senderName + "|content = " + content);

                //IF it's my mess
                if(senderName == CPlayer.nickName)
                {
                    GameObject goj = Instantiate(gojs[6], gojs[6].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();
                    
                    txtArr[0].text = txtArr[1].text = content;
                    ////App.trace("Phắc " + txtArr[0].preferredWidth,"green");
                    float w = txtArr[0].preferredWidth;
                    float h = txtArr[0].preferredHeight;
                    if(w < 600)
                        txtArr[1].GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                    goj.transform.SetAsFirstSibling();
                    goj.SetActive(true);
                }
                else
                {
                    GameObject goj = Instantiate(gojs[5], gojs[5].transform.parent, false);
                    Text[] txtArr = goj.GetComponentsInChildren<Text>();
                    txtArr[0].text = content;
                    float w = txtArr[0].preferredWidth;
                    float h = txtArr[0].preferredHeight;
                    if (w < 600)
                        txtArr[0].GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                    goj.transform.SetAsFirstSibling();
                    goj.SetActive(true);
                    ////App.trace("Phắc " + txtArr[0].preferredWidth, "yellow");

                }

            }
            scr.DOVerticalNormalizedPos(0, .2f);
        });
    }

    public void Test()
    {
        var req = new OutBounMessage("PM.LIST");
        req.addHead();
        req.writeByte(1);   //mess type: 0:personal |4: system|5: support
        req.writeShort(1);          //Start page
        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            int count = res.readByte();
            for (int i = 0; i < count; i++)
            {
                long pmId = res.readLong();
                long senderId = res.readLong();
                string senderName = res.readAscii();
                string content = res.readStrings();
                long time = res.readLong();
                bool read = res.readByte() == 1;
                //App.trace("[RECV] private sms|senderId = " + senderId + "|senderName = " + senderName + "content = " + content);
            }
        });
    }
}
