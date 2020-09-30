using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;
namespace Core.Server.Api
{
    public partial class LoadingControl : MonoBehaviour, IStoreListener, IStoreCallback
    {

        public static int MAX_SIBLING_INDEX = 18;
        #region Difference
        public Button btnDoConfirm;
        public GameObject blackPanel;
        public Animator cliendDialogAnim, confirmDialogAnim;
        public string openFrom;
        private List<string> paymentList;
        public Text searchPlayerText;
        private int preClickedId = -1;
        private int currPanel = 0;
        private List<string> telcoList, cardTypeCodeList;
        private int preClickedId3 = -1;
        private int preClickedId2 = -1;
        public Text errorText, confirmText;
        public GameObject playerListPanel, playerPanel;


        public static int CHANNEL_GENERAL = 1;
        public static int CHANNEL_GROUP = 0;
        public static int CHANNEL_TABLE = 4;
        public static int CHANNEL_PRIVATE = 5;
        public static int TYPE_EMOTICON = 1;
        public static int TYPE_QUICK_MESSAGE = 2;

        public GameObject[] pmElementUser;
        private bool pmRecentMessLoadFull = false;
        private List<Toggle> pmTogList;
        private List<long> pmTogIdList;
        public Sprite[] chatSpritesList;
        private int pmPreLeftTogId = -1;
        public Text pmElementText;
        private long currTargetPmId = -1;

        [Header("==========Chat==============")]
        public Color[] colorList;
        public ScrollRect[] chatScrollView;
        private bool set3LinecallBack = true;
        private int chatType = -1;
        /// <summary>
        /// Body các tab 1: Tab chung|2: Tab riêng|3: Tab Ban`
        /// </summary>
        public GameObject[] chatBotPanelList;

        private int maxLineCount = 100;
        private List<string> linesContent, threeLinesContent;
        public GameObject lineChatContent;
        public InputField[] chatPanelIpfList;
        public GameObject[] chatEmoPanel;
        public Toggle[] chatTogList;
        public GameObject chatBox;
        [Header("==========End Chat==============")]
        private string[] quickMessStr;//= { "Xin chào", "Đánh nhanh đê", "Cứ từ từ", "Vãi xoài", "Có hàng nè", "Tha cho em", "Chạy đâu", "Tưởng thế nào" };
        private string[] xocDiaMessStr; //= { "Chẵn nè!", "Xin lỗi đời quá đen", "Mua nhanh nào", "Hôm nay hên quá", "Lẻ lẻ lẻ...", "Đỏ quá đi :v", "Yếu đừng ra gió", "Chia buồn với các chú" };
        private string[] mauBinhMessStr; //= { "Vãi xoài", "Cứ từ từ", "Ôi thôi chết rồi", "Mậu binh rồi", "Nhanh còn đọ bài", "Sập 3 chi", "Xin chào", "Đời quá đen" };
        private string[] phomMessStr; //= { "Ù này", "Xin cây chốt", "Ăn đê...", "Cứ từ từ", "Mình xin", "Cho thì ăn", "Xin chào", "Đánh nhanh đê" };
        private string[] xitoMessStr; //= { "Úp nhanh còn kịp", "Tất tay đê", "Tố to vào", "Sợ rồi à", "Mình xin", "Ngon thì zô", "Xin chào", "Đen đừng hỏi" };
        private string[] xiDachMessStr; //= { "Còn non lắm", "Lại có tiền", "Vãi xoài", "Quắc mất rồi", "Bốc nhanh đê", "Đời quá đen", "Xin chào", "Đủ 21, hốt tiền rồi" };
        private string[] chanMessStr; //= { "Bình tĩnh", "Ôi thôi chết rồi", "Chờ ù, hehe", "Ăn đê", "Xin chào", "Nhất đi nhì ù", "Mình xin", "Đánh nhanh đê" };
        private string[] pokerMessStr; //= { "Úp nhanh còn kịp", "Tất tay đê", "Tố to vào", "Sợ rồi à", "Mình xin", "Ngon thì zdô", "Xin chào", "Đen đừng hỏi" };

        private string emo = "";
        private Sprite _emo = null;
        public GameObject settingPanel;
        public bool hasSendInvite = false;
        private float curr = -1;

        /// Các nút trong popup thông tin player 1: Tố cáo|2: Thêm bạn|3: Nhắn tin|4: KICK
        /// </summary>
        public Button[] smallInfo_btnList;
        /// Text hiển thị trong popup thông tin player 1:Nick|2:Status|3:Chip|4:Gold|5-6:them-huy kb
        /// </summary>
        public Text[] smallInfoPanel_textList;
        public GameObject smallInfoPanel;
        /// Cac image can dung trong popup thong tin player 1: Avatar
        /// </summary>
        public Image[] smallInfo_imageList;
        /// <summary>
        /// List các text trong pop tố cáo 1:Tên tài khoản|2: Người nhận vs Tài khoản
        /// </summary>
        public Text[] reportPanel_textList;
        /// <summary>
        /// List các nút trong pop tố cáo 1: nút đóng|2: nút tố cáo
        /// </summary>
        public Button[] report_btnList;
        public InputField reportPanelIpf;
        public GameObject reportPanel;
        private bool isCPlayerFriend = false;
        public Text theleText;
        /// <summary>
        /// Cho phep moi choi hay khong
        /// </summary>
        public bool bandInvitation = false;
        public string CountryC; //countrycode
        public string IpUser; //IPUser
        public void closeSmallInfo(bool closeBlackPanel = true)
        {
            smallInfoPanel.transform.DOScale(Vector3.zero, .1f).OnComplete(() => {
                smallInfoPanel.SetActive(false);
                if (closeBlackPanel)
                    blackPanel.SetActive(false);
            });
        }
        public void closeChatPanel()
        {
            chatBox.SetActive(false);
        }
        public void chatScrollViewChanged()
        {
            float currY = chatScrollView[0].content.anchoredPosition.y;
            if (currY > 5)
                return;
            _loadMoreChatData();
        }

        public void chatPmScrollViewChanged()
        {
            float currY = chatScrollView[1].content.anchoredPosition.y;
            if (currY > 5)
                return;
            pmLoadMoreMesss(currTargetPmId);

        }
        public void closeReportPanel()
        {
            reportPanel.transform.DOScale(Vector3.zero, .1f).OnComplete(() =>
            {
                smallInfoPanel.SetActive(false);
                blackPanel.SetActive(false);
            });
        }
        private void doReport(long playerId, bool isReport = true, bool isFeedBack = false)
        {

            string name = CPlayer.friendNicName;
            string reason = reportPanelIpf.text;

            if (isFeedBack)
            {

                if (reason.Length == 0)
                {

                    //App.showErr("Vui lòng nhập góp ý của bạn.");
                    App.showErr(App.listKeyText["WARN_FEEDBACK_BLANK"]);

                    return;
                }
                var req_FEEDBACK = new OutBounMessage("FEEDBACK");
                req_FEEDBACK.addHead();
                req_FEEDBACK.writeString(reason);
                //loadingScene.SetActive(true);
                LoadingUIPanel.Show();
                App.ws.send(req_FEEDBACK.getReq(), delegate (InBoundMessage res_FEEDBACK)
                {
                    closeReportPanel();
                    //loadingScene.SetActive(false);
                    LoadingUIPanel.Hide();
                    //App.showErr("Góp ý của bạn đã được gửi đến BQT. Cảm ơn bạn rất nhiều, chúc bạn có những phút giây vui vẻ!");
                    App.showErr(App.listKeyText["INFO_FEEDBACK_SUCCESS"]);

                });
                return;
            }

            if (!isReport)
            {
                if (reason.Length == 0)
                {
                    //App.showErr("Vui lòng nhập tin nhắn");
                    App.showErr(App.listKeyText["WARN_PM_BLANK"]);

                    
                    return;
                }
                var reqPMCreate = new OutBounMessage("PM.CREATE");
                reqPMCreate.addHead();
                reqPMCreate.writeAcii(name);
                reqPMCreate.writeString(reason);
                //loadingScene.SetActive(true);
                LoadingUIPanel.Show();
                App.ws.send(reqPMCreate.getReq(), delegate (InBoundMessage res)
                {
                    //App.trace("MSS SENT!");
                    closeReportPanel();
                    //loadingScene.SetActive(false);
                    LoadingUIPanel.Hide();
                });
                return;
            }
            if (reason.Length == 0)
            {
                
                //App.showErr("Vui lòng nhập lý do");
                App.showErr(App.listKeyText["WARN_CONTENT_BLANK"]);

                return;
            }
            OutBounMessage req = new OutBounMessage("REPORT_ABUSE");
            req.addHead();
            req.writeLong(playerId); //Ma tkhoan
            req.writeAcii(name);
            req.writeString(reason);
            //loadingScene.SetActive(true);
            LoadingUIPanel.Show();
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                //App.trace("REPORT OK!");
                closeReportPanel();
                //loadingScene.SetActive(false);
                LoadingUIPanel.Hide();
            });

        }

        public void GetQuickChat()
        {
            string tempquickMessStr = App.listKeyText["BOTCHAT_COMMON"];
            quickMessStr = tempquickMessStr.Split('<');

            string tempxocDiaMessStr = App.listKeyText["BOTCHAT_XOCDIA"];
            xocDiaMessStr = tempxocDiaMessStr.Split('<');

            string tempmauBinhMessStr = App.listKeyText["BOTCHAT_MAUBINH"];
            mauBinhMessStr = tempmauBinhMessStr.Split('<');

            string tempphomMessStr = App.listKeyText["BOTCHAT_PHOM"];
            phomMessStr = tempphomMessStr.Split('<');

            string tempxitoMessStr = App.listKeyText["BOTCHAT_XITO"];
            xitoMessStr = tempxitoMessStr.Split('<');

            string tempxiDachMessStr = App.listKeyText["BOTCHAT_XIJACH"];
            xiDachMessStr = tempxiDachMessStr.Split('<');

            string tempchanMessStr = App.listKeyText["BOTCHAT_CHAN"];
            chanMessStr = tempchanMessStr.Split('<');

            string temppokerMessStr = App.listKeyText["BOTCHAT_POKER"];
            pokerMessStr = temppokerMessStr.Split('<');
        }

        public void chatMSGRecv(int type, string nickName, string content)
        {
            linesContent.RemoveAt(linesContent.Count - 1);
            linesContent.Insert(0, System.String.Format("{0}: {1}", nickName, content));
            if (set3LinecallBack)
                set3LineChat();
            //App.trace("type = " + type + "content = " + nickName + ": " + content + "| currChatType " + chatType);
            if (type == chatType)
            {
                addALineChatContent(System.String.Format("{0}: {1}", nickName, content), nickName == CPlayer.nickName);
            }

        }

        public void openReportPanel(long playerId, bool isReport, bool isFeedBack = false)
        {
            blackPanel.SetActive(true);
            if (isReport)
            {
                report_btnList[1].GetComponentInChildren<Text>().text = App.listKeyText["SEND_REPORT"]; //"Gửi tố cáo";
                reportPanel_textList[1].text = App.listKeyText["USERNAME"]; //"Tên tài khoản:";
            }
            else
            {
                report_btnList[1].GetComponentInChildren<Text>().text = App.listKeyText["SEND_PM"]; //"Gửi tin nhắn";
                reportPanel_textList[1].text = App.listKeyText["RECEIVER"]; //"Người nhận";
                if (isFeedBack)
                {
                    report_btnList[1].GetComponentInChildren<Text>().text = App.listKeyText["SEND_FEEDBACK"];// "Gửi góp ý";
                    reportPanel_textList[0].text = "ADMIN";
                }
            }

            report_btnList[1].onClick.RemoveAllListeners();

            report_btnList[1].onClick.AddListener(() =>
            {
                if (!isFeedBack)
                {
                    doReport(playerId, isReport);
                }
                else
                {

                    doReport(playerId, isReport, true);
                }
                //closeReportPanel();
            });

            reportPanel.transform.localScale = Vector3.zero;
            reportPanel.SetActive(true);
            reportPanel.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
        }


        public void showPlayerInfo(string nickName, long chipBalance, long starBalance, long playerId, bool isFriend, Sprite sprite, string type = "")
        {
            Debug.Log("Show Info");
            CPlayer.friendNicName = nickName;
            if (type == "me")
            {
                smallInfo_btnList[0].gameObject.SetActive(false);
                smallInfo_btnList[1].gameObject.SetActive(false);
                smallInfo_btnList[2].gameObject.SetActive(false);
                smallInfo_btnList[3].gameObject.SetActive(false);

                //blackPanel.SetActive(true);

                // //App.trace("chip = " + chipBalance + "|star = " + starBalance);
                smallInfoPanel_textList[0].text = nickName;
                smallInfoPanel_textList[1].text = CPlayer.defaultStatus[playerId % 10];
                smallInfoPanel_textList[2].text = App.formatMoney(chipBalance.ToString());
                smallInfoPanel_textList[3].text = App.formatMoney(starBalance.ToString());

                smallInfoPanel_textList[5].text = App.listKeyText["CURRENCY"].ToUpper();

                smallInfoPanel.transform.localScale = Vector3.zero;
                smallInfoPanel.SetActive(true);
                smallInfoPanel.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
                smallInfo_imageList[0].sprite = sprite;
                return;
            }
            smallInfo_btnList[0].gameObject.SetActive(true);
            smallInfo_btnList[1].gameObject.SetActive(false);
            smallInfo_btnList[2].gameObject.SetActive(false);
            //smallInfo_btnList[3].gameObject.SetActive(false);
            if (type == "kick")
            {
                //smallInfo_btnList[3].gameObject.SetActive(true);
                smallInfo_btnList[3].onClick.RemoveAllListeners();
                smallInfo_btnList[3].onClick.AddListener(() =>
                {
                    var req = new OutBounMessage("KICK_PLAYER");
                    req.addHead();
                    req.writeAcii(nickName);
                    App.ws.send(req.getReq(), delegate (InBoundMessage res)
                    {

                    });
                    blackPanel.SetActive(false);
                    smallInfoPanel.SetActive(false);
                });
            }

            blackPanel.SetActive(true);

            // //App.trace("chip = " + chipBalance + "|star = " + starBalance);
            smallInfoPanel_textList[0].text = nickName;
            smallInfoPanel_textList[1].text = CPlayer.defaultStatus[playerId % 10];
            smallInfoPanel_textList[2].text = App.formatMoney(chipBalance.ToString());
            smallInfoPanel_textList[3].text = App.formatMoney(starBalance.ToString());

            smallInfoPanel_textList[5].text = App.listKeyText["CURRENCY"].ToUpper();


            smallInfo_btnList[0].onClick.RemoveAllListeners();
            smallInfo_btnList[0].onClick.AddListener(() =>
            {

                smallInfoPanel.SetActive(false);
                blackPanel.SetActive(true);
                reportPanel_textList[0].text = App.formatNickName(nickName, 15);
                openReportPanel(playerId, true);
            });

            smallInfoPanel_textList[4].text = isFriend ? "Xóa bạn" : "Thêm bạn";
            smallInfo_btnList[1].onClick.RemoveAllListeners();
            smallInfo_btnList[1].onClick.AddListener(() =>
            {
                smallInfoPanel.SetActive(false);
                blackPanel.SetActive(false);
                if (isFriend)
                {
                    var reqAddFriend = new OutBounMessage("FRIEND.DELETE");
                    reqAddFriend.addHead();
                    reqAddFriend.writeLong(playerId);
                    reqAddFriend.writeAcii(nickName);
                    App.ws.send(reqAddFriend.getReq(), delegate (InBoundMessage resAddFriend)
                    {
                        smallInfoPanel_textList[4].text = "Thêm bạn";
                        isCPlayerFriend = false;
                    });
                }
                else
                {
                    var reqAddFriend = new OutBounMessage("FRIEND.CREATE");
                    reqAddFriend.addHead();
                    reqAddFriend.writeLong(playerId);
                    reqAddFriend.writeAcii(nickName);
                    App.ws.send(reqAddFriend.getReq(), delegate (InBoundMessage resAddFriend)
                    {
                        smallInfoPanel_textList[4].text = "Xóa bạn";
                        isCPlayerFriend = true;
                    });
                }

            });


            smallInfo_btnList[2].onClick.RemoveAllListeners();
            smallInfo_btnList[2].onClick.AddListener(() =>
            {
                smallInfoPanel.SetActive(false);
                blackPanel.SetActive(true);
                reportPanel_textList[0].text = App.formatNickName(nickName, 15);
                openReportPanel(playerId, false);
            });

            smallInfoPanel.transform.localScale = Vector3.zero;
            smallInfoPanel.SetActive(true);
            smallInfoPanel.transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack);
            smallInfo_imageList[0].sprite = sprite;


        }

        private void _invite(string nickName)
        {
            var req = new OutBounMessage("INVITE");
            req.addHead();
            req.writeAcii(nickName);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                
                //App.showErr("Gửi lời mời thành công");
                App.showErr(App.listKeyText["INFO_INVITE_SUCCESS"]);

            });
        }
        private int timeSendInven = 10;
        private IEnumerator WaittingSendInven()
        {
            yield return new WaitForSeconds(timeSendInven);
            hasSendInvite = false;
        }
        public void sendInvite()
        {
            if (hasSendInvite == true)
            {
                string appShowErr = App.listKeyText["WARN_INVITE_SPAM"];
                string new1 = appShowErr.Replace("#1", timeSendInven.ToString());
                //App.showErr("Bạn phải chờ "+timeSendInven+" giây nữa mới có thể tiếp tục mời người chơi khác.");

                App.showErr(new1);
                return;
            }
            int ranNum = UnityEngine.Random.RandomRange(0, 10);
            timeSendInven = 10;
            hasSendInvite = true;
            StartCoroutine(WaittingSendInven());
            curr = 0;
            var req = new OutBounMessage("LIST_ZONE_PLAYER");
            req.addHead();
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                int count = res.readShort();
                if (count == 0)
                {
                    //App.showErr("Gửi lời mời thành công!");
                    App.showErr(App.listKeyText["INFO_INVITE_SUCCESS"]);
                }
                //Debug.Log("count "+ count);
                for (int i = 0; i < count; i++)
                {
                    long playerId = res.readLong();
                    string nickName = res.readAscii();
                    bool isMale = res.readByte() == 1;
                    long chipBalance = res.readLong();
                    long starBalance = res.readLong();
                    long score = res.readLong();
                    int level = res.readShort();
                    string avatar = res.readAscii();
                    int avatarId = res.readShort();
                    int a = UnityEngine.Random.RandomRange(0, 1);
                    if (a % 2 == 0)
                    {
                        _invite(nickName);
                        ranNum--;
                        if (ranNum < 0)
                            break;
                    }

                }
            });
        }

        public void _showNotEnoughMoney(string mess)
        {
            Text txt = btnDoConfirm.GetComponentInChildren<Text>();
            txt.text = "OK";
            btnDoConfirm.onClick.RemoveAllListeners();

            btnDoConfirm.onClick.AddListener(() =>
            {

                if (CPlayer.phoneNumber.Equals("")) //chua xac thuc so dienj thoai
                {
                    LobbyControl.instance.doAuthen();
                }
                else //da xac thuc so dienj thoai
                {
                    //if (!CPlayer.shouldShowRefine.Equals("1")) //khong phai la choi ngay, tk binh thuong
                    //{
                    closeConfirmDialog();
                    showRechargeScene(true);
                    //}
                    //else //la tai khoan choi ngay
                    //{
                    //    LoadingControl.instance._showRefineDialog(true);
                    //}
                }


            });
            CPlayer.notEnouChipMess = "";
            confirmText.text = mess;
            blackPanel.SetActive(true);
            confirmDialogAnim.Play("DialogAnim");
        }

        public void notEnoughChip(string mess)
        {
            CPlayer.notEnouChipMess = mess;
            switch (CPlayer.gameName)
            {
                case "1":   //TLMN
                    if (BoardManager.instance != null)
                    {
                        BoardManager.instance.regQuit = true;
                    }
                    if (TLMNControler.instance != null)
                    {
                        TLMNControler.instance.regToQuit = true;
                        //App.trace("???");
                    }

                    break;
                case "maubinh":   //Mậu binh
                    if (MauBinhController.instance != null)
                    {
                        MauBinhController.instance.regQuit = true;
                    }
                    break;
                case "xocdia":   //Xóc đĩa
                    if (XocDiaControler.instance != null)
                    {
                        XocDiaControler.instance.regQuit = true;
                    }
                    break;
                case "0":
                    if (PhomController.instance != null)
                    {
                        PhomController.instance.regQuit = true;
                    }
                    break;
                case "xito":
                    if (XiToController.instance != null)
                    {
                        XiToController.instance.regQuit = true;
                    }
                    break;
                case "blackjack":
                    if (XiDachController.instance != null)
                        XiDachController.instance.regQuit = true;
                    break;
                case "chan":
                    if (ChanController.instance != null)
                        ChanController.instance.regQuit = true;
                    break;
            }
        }

        public void openSettingPanel()
        {
            loadingGojList[17].SetActive(true);
            //settingPanel.SetActive(true);
        }
        #region Chat
        private void _loadMoreChatData(bool togIsChanged = false)
        {
            int skip = lineChatContent.transform.parent.childCount - 2;
            if (togIsChanged)
            {
                skip = 0;
            }
            if (skip < maxLineCount)
            {
                var res = new OutBounMessage("CHAT.LOAD");
                res.addHead();
                res.writeByte(chatType);    //loại khung chat
                res.writeByte(skip);    //bỏ qua bao nhiêu dòng
                res.writeByte(80);   //Giới hạn số dòng 1 lần chat
                res.writeLong(0); //hardcode for chatbox in room

                App.ws.send(res.getReq(), delegate (InBoundMessage resChat)
                {
                    int numberMsg = resChat.readByte();
                    if (linesContent == null)
                    {
                        linesContent = new List<string>();
                        threeLinesContent = new List<string>();
                    }
                    else
                    {
                        linesContent.Clear();
                        //threeLinesContent.Clear();
                    }
                    for (int i = 0; i < numberMsg; i++)
                    {
                        long sender = resChat.readLong();
                        string senderName = resChat.readAscii();
                        string content = resChat.readStrings();
                        ////App.trace("-- " + i + "senderName = " + senderName + "|content = " + content);

                        if (senderName.Length == 0 && content.Length == 0)
                        {
                            continue;
                        }
                        linesContent.Add(System.String.Format("{0}: {1}", senderName, content));
                        if (i < 3)
                        {
                            threeLinesContent.Add(System.String.Format("{0}: {1}", senderName, content));
                        }

                    }
                    //loadChat(linesContent, true);
                    /*
                    if (type == "first")
                    {
                        set3LineChat();
                        setChatContent(type);
                        return;
                    }

                    setChatContent(type);
                    */
                    if (set3LinecallBack)
                    {
                        set3LineChat();

                    }
                    setChatContent(togIsChanged);
                });
            }

        }
        private int preChatTogId = 0;
        /// <summary>
        /// ĐỔI TAB CHAT BANG NUT CHAT
        /// </summary>
        public void _changeChatTog(int id)
        {
            if (preChatTogId != id && chatTogList[id].isOn)
            {
                if (preChatTogId > -1)
                {
                    chatBotPanelList[preChatTogId].SetActive(false);
                    chatTogList[preChatTogId].GetComponentInChildren<Text>().color = colorList[1];
                }
                preChatTogId = id;
                chatTogList[id].GetComponentInChildren<Text>().color = colorList[0];
                chatBotPanelList[id].SetActive(true);


                chatEmoPanel[21].SetActive(false);
                chatEmoPanel[22].SetActive(false);
                if (id == 0)
                    loadChatData(CHANNEL_GENERAL);
                if (id == 2)
                {
                    loadChatData(CHANNEL_TABLE);
                    chatEmoPanel[21].SetActive(true);
                    chatEmoPanel[22].SetActive(true);
                }
                if (id == 1)
                {
                    loadChatData(CHANNEL_PRIVATE);
                    chatType = CHANNEL_PRIVATE;
                }

            }
            //showChatBox(id);
        }
        private void changeChatTog(int togId)
        {
            for (int i = 0; i < chatTogList.Length; i++)
            {
                if (togId == i)
                {
                    chatTogList[i].isOn = true;
                    chatTogList[i].GetComponentInChildren<Text>().color = colorList[0];

                }
                else
                {
                    chatTogList[i].isOn = false;
                    chatTogList[i].GetComponentInChildren<Text>().color = colorList[1];
                }
            }
            if (!chatBox.activeSelf)
            {
                chatBox.SetActive(true);
                chatScrollView[0].normalizedPosition = Vector2.zero;
            }
        }
        public void showChatBox(int type)
        {
            ChatManager.instance.OpenChat("chatBox");
            foreach (InputField ipf in chatPanelIpfList)
            {
                ipf.text = "";
            }
            chatEmoPanel[0].SetActive(false);
            chatEmoPanel[22].SetActive(false);
            switch (type)
            {
                case 1:
                    //chatTogList[2].gameObject.SetActive(false);
                    chatTogList[2].gameObject.SetActive(true);
                    chatTogList[2].interactable = false;
                    changeChatTog(0);
                    set3LinecallBack = true;
                    break;
                case 4: //TABLE
                    chatTogList[2].gameObject.SetActive(true);
                    chatTogList[2].interactable = true;
                    changeChatTog(2);
                    set3LinecallBack = false;
                    Transform tfm = pmElementUser[2].transform.parent;
                    int count = tfm.childCount;
                    ////App.trace("==================" + count);
                    for (int i = count - 1; i > 0; i--)
                    {
                        DestroyImmediate(tfm.GetChild(i).gameObject);
                    }
                    string[] quickMessS = null;
                    switch (CPlayer.gameName)
                    {
                        case "1":
                            quickMessS = quickMessStr;
                            break;
                        case "xocdia":
                            quickMessS = xocDiaMessStr;
                            break;
                        case "maubinh":
                            quickMessS = mauBinhMessStr;
                            break;
                        case "0":
                            quickMessS = phomMessStr;
                            break;
                        case "xito":
                            quickMessS = xitoMessStr;
                            break;
                        case "blackjack":
                            quickMessS = xiDachMessStr;
                            break;
                        case "chan":
                            quickMessS = chanMessStr;
                            break;
                        case "poker":
                            quickMessS = pokerMessStr;
                            break;
                    }
                    for (int i = 0; i < quickMessS.Length; i++)
                    {
                        GameObject goj = Instantiate(pmElementUser[2], pmElementUser[2].transform.parent, false) as GameObject;
                        int temp = i;
                        goj.GetComponentInChildren<Text>().text = quickMessS[temp];
                        Button btn = goj.GetComponent<Button>();
                        btn.onClick.AddListener(() =>
                        {
                            chatPanelIpfList[0].text = quickMessS[temp];
                            chatBox.SetActive(false);
                            sendSMG(0);
                            Debug.Log(" quickMessS[temp] " + quickMessS[temp]);

                        });
                        goj.SetActive(true);
                    }

                    chatEmoPanel[22].SetActive(true);
                    break;
                case 5: //CHAT RIÊNG
                        //chatTogList[2]
                    set3LinecallBack = false;
                    changeChatTog(1);

                    break;
            }
            if (type != chatType)
                loadChatData(type);
        }
        public void showEmoTiconPanel()
        {
            if (chatEmoPanel[0].activeSelf)
            {
                chatEmoPanel[0].SetActive(false);
                return;
            }
            for (int i = 1; i < 21; i++)
            {
                chatEmoPanel[i].GetComponent<Image>().sprite = chatSpritesList[i + 1];
                chatEmoPanel[i].GetComponent<Button>().onClick.RemoveAllListeners();
                int temp = i + 1;
                chatEmoPanel[i].GetComponent<Button>().onClick.AddListener(() =>
                {
                    ////App.trace(chatSpritesList[temp].name);
                    emo = chatSpritesList[temp].name.Substring(8);
                    ////App.trace("tao kic emo " + emo);
                    _emo = chatSpritesList[temp];
                    sendSMG(100);
                    closeChatPanel();
                });
            }
            chatEmoPanel[0].SetActive(true);

        }
        public void sendSMG(int herachiType)
        {
            string t = chatPanelIpfList[0].text;
            if (herachiType != 100 && t.Length == 0)
                return;
            if (chatType == CHANNEL_TABLE)
            {
                ////App.trace("send BroadCast = " + chatType);
                Debug.Log("send BroadCast = " + chatType);
                var req_BROADCAST = new OutBounMessage("BROADCAST");
                req_BROADCAST.addHead();
                string mContent = "", mEmo = "";
                int mType = 0;
                Sprite _mEmo = null;
                if (herachiType == 100)
                {
                    mContent = emo;
                    mEmo = emo;
                    mType = TYPE_EMOTICON;
                    _mEmo = _emo;
                }
                else
                {
                    mContent = t;
                    mEmo = "";
                    mType = TYPE_QUICK_MESSAGE;
                }
                req_BROADCAST.writeStrings(mContent); //content
                req_BROADCAST.writeAcii(mEmo);   //icon
                req_BROADCAST.writeByte(mType); //channel type
                                                ////App.trace("SEND conten = " + mContent + "|emo = " + mEmo + "|type = " + mType);
                                                //req_BROADCAST.print();
                Debug.Log("SEND conten = " + mContent + "|emo = " + mEmo + "|type = " + mType);
                App.ws.send(req_BROADCAST.getReq(), null, false, 0);
                chatPanelIpfList[0].text = "";
                return;
            }
            string content = chatPanelIpfList[herachiType].text;
            var req = new OutBounMessage("CHAT.SEND");
            req.addHead();
            req.writeString(content);
            req.writeByte(chatType);
            //App.trace("chatType to send = " + chatType);
            req.writeLong(0);
            chatPanelIpfList[herachiType].text = "";
            Debug.Log("chatType to send = " + chatType);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                //chatPanelIpfList[0].text = "";
            }, false, 0);
        }


        private void setChatContent(bool togisChanged = false)
        {

            if (togisChanged)
            {

                Transform tfm = lineChatContent.transform.parent;
                int count = tfm.childCount;
                ////App.trace("==================" + count);
                for (int i = count - 1; i > 0; i--)
                {
                    DestroyImmediate(tfm.GetChild(i).gameObject);
                }
            }

            Transform tempTransform;


            for (int i = 0; i < linesContent.Count; i++)
            {

                GameObject temp = Instantiate(lineChatContent, lineChatContent.transform.parent, false) as GameObject;
                Text txt = temp.GetComponent<Text>();
                txt.text = linesContent[i] as string;
                tempTransform = temp.transform;
                ////App.trace("dài thế: " + temp.GetComponent<Text>().preferredHeight);
                /*
                 * float h = txt.preferredHeight / 2.8f;
                if (h < 60)
                    h = 60;
                txt.GetComponent<LayoutElement>().minHeight = h;
                 * */
                float h = txt.preferredHeight / 2.5f;

                if (h < 40)
                    h = 50;
                else if (h < 70)
                {
                    h = 80;
                }
                txt.GetComponent<LayoutElement>().minHeight = h;
                if (linesContent[i].Contains(CPlayer.nickName))
                {
                    txt.color = colorList[2];
                }
                else if (linesContent[i].Contains("admin"))
                {
                    txt.color = colorList[0];
                }
                temp.transform.SetSiblingIndex(lineChatContent.transform.GetSiblingIndex() + 1);
                temp.SetActive(true);
                StartCoroutine(WaitForRefresh(h));

            }

        }

        IEnumerator WaitForRefresh(float h)
        {
            yield return new WaitForEndOfFrame();
            chatScrollView[0].normalizedPosition = Vector2.zero;
            chatScrollView[0].content.anchoredPosition += new Vector2(0, h * 2);
        }

        private void set3LineChat()
        {
            TableList.instance.chat3TextList[0].text = "";
            if (TableList.instance != null)
                for (int i = 0; i < 3; i++)
                {
                    TableList.instance.chat3TextList[0].text += threeLinesContent[2 - i] + "\n";
                    ////App.trace("THU " + threeLinesContent[i]);
                }
        }

        private void pmLoadMoreMesss(long targetId)
        {
            Transform tfm = pmElementText.gameObject.transform.parent;
            int lineCount = tfm.childCount - 1;
            if (lineCount > 90)
            {
                return;
            }
            lineCount = tfm.childCount - 1;
            int limit = 15;
            OutBounMessage reqqq = new OutBounMessage("PM.LIST");
            reqqq.addHead();
            reqqq.writeByte(3);
            reqqq.writeLong(targetId);  //selected user id
            currTargetPmId = targetId;
            reqqq.writeByte(lineCount);
            reqqq.writeByte(limit);
            App.ws.send(reqqq.getReq(), delegate (InBoundMessage resss)
            {
                int messNum = resss.readByte();
                for (int i = 0; i < messNum; i++)
                {
                    var sender = resss.readLong();
                    var senderName = resss.readAscii();
                    var content = resss.readString();
                    ////App.trace(senderName + "| content = " + content);

                    Text txt = Instantiate(pmElementText, pmElementText.transform.parent, false) as Text;
                    txt.text = senderName + ": " + App.formatToUserContent(content);
                    txt.color = senderName == CPlayer.nickName ? colorList[2] : colorList[1];
                    float h = txt.preferredHeight / 2.3f;
                    if (h < 40)
                        h = 50;
                    else if (h < 70)
                        h = 80;
                    if (content.Contains("http"))
                    {
                        h += 40;
                    }
                    txt.GetComponent<LayoutElement>().minHeight = h;
                    txt.transform.SetSiblingIndex(1);
                    txt.gameObject.SetActive(true);
                }
            });
        }

        private void changePMTog(int id, long targetId)
        {
            //App.trace(id.ToString() + "|" + targetId.ToString());
            if (pmTogList[id].isOn == false)
                return;
            pmPreLeftTogId = id;
            for (int i = 0; i < pmTogList.Count; i++)
            {
                if (i == id)
                {
                    pmTogList[i].interactable = false;
                }
                else
                {
                    pmTogList[i].isOn = false;
                    pmTogList[i].interactable = true;
                }

            }

            Transform tfm = pmElementText.gameObject.transform.parent;
            int lineCount = tfm.childCount - 1;

            for (int i = lineCount; i > 0; i--)
            {
                DestroyImmediate(tfm.GetChild(i).gameObject);
            }

            pmLoadMoreMesss(targetId);
            chatScrollView[1].normalizedPosition = Vector2.zero;
        }

        private void loadRecentPMMessage(int limit = -1, int skip = -1)
        {
            if (limit < 0)
                limit = 15;
            Transform tfm = pmElementUser[0].transform.parent;
            int lineCount = tfm.childCount - 1;

            for (int i = lineCount; i > 0; i--)
            {
                DestroyImmediate(tfm.GetChild(i).gameObject);
            }

            if (skip < 0)
            {

                skip = tfm.childCount - 1;
            }

            OutBounMessage req = new OutBounMessage("PM.LIST");
            req.addHead();
            req.writeByte(2);   //LOAD KÈM TIN NHẮN CUỐI
            req.writeByte(skip);
            req.writeByte(limit);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                int mesNum = res.readByte();
                if (mesNum < limit)
                {
                    pmRecentMessLoadFull = true;
                }
                pmTogList = new List<Toggle>();
                pmTogIdList = new List<long>();
                int mCount = -1;
                for (int i = 0; i < mesNum; i++)
                {
                    long time = res.readLong();
                    long senderId = res.readLong();
                    string senderName = res.readAscii();
                    int avatarId = res.readShort();
                    string avatar = res.readAscii();
                    long logoutTime = res.readLong();
                    string content = res.readStrings();
                    bool read = res.readByte() == 1;
                    bool isOnline = res.readByte() == 1;
                    if (!pmTogIdList.Contains(senderId))
                    {
                        ////App.trace(". " + senderId + ", senderName = " + senderName + "|content = " + content);
                        GameObject goj = Instantiate(pmElementUser[0], pmElementUser[0].transform.parent, false) as GameObject;
                        Text[] txtArr = goj.GetComponentsInChildren<Text>();
                        txtArr[0].text = senderName;
                        txtArr[1].text = isOnline ? "Online" : "Offline";
                        Image[] imgList = goj.GetComponentsInChildren<Image>();
                        imgList[3].overrideSprite = isOnline ? chatSpritesList[0] : chatSpritesList[1];
                        StartCoroutine(App.loadImg(imgList[1], App.getAvatarLink2(avatar, avatarId)));


                        pmTogIdList.Add(senderId);

                        Toggle tog = goj.GetComponent<Toggle>();
                        mCount++;
                        int tmp = mCount;
                        tog.onValueChanged.AddListener((bool s) =>
                        {
                            if (s)
                                changePMTog(tmp, senderId);
                        });
                        pmTogList.Add(tog);
                        goj.SetActive(true);
                    }



                }
                if (mesNum > 0)
                {
                    pmTogList[0].isOn = true;
                    changePMTog(0, pmTogIdList[0]);
                }

            });
        }


        public void loadChatData(int channel, bool isInTableList = false)
        {
            if (channel == CHANNEL_PRIVATE)
            {
                var reqPM = new OutBounMessage("PM.SUBS");
                reqPM.addHead();
                App.ws.send(reqPM.getReq(), delegate (InBoundMessage res)
                {

                });
                loadRecentPMMessage();
                return;
            }

            ////App.trace("===LOLAD DATA " + channel);
            if (isInTableList)
            {
                set3LinecallBack = true;
            }
            if (channel == chatType)
            {
                _loadMoreChatData();
                return;
            }
            chatType = channel;

            var req = new OutBounMessage("CHAT.SUBS");
            req.addHead();
            req.writeByte(channel);
            req.writeLong(0);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                //App.trace("CHAT.SUB, type = " + chatType);
                _loadMoreChatData(true);
            });


        }
        private void addALineChatContent(string content, bool isMine = false)
        {
            if (chatBox.activeSelf == false)
                return;
            Transform tfm = lineChatContent.gameObject.transform.parent;
            int count = tfm.childCount;
            if (count < maxLineCount)
            {
                GameObject temp = Instantiate(lineChatContent, lineChatContent.transform.parent, false) as GameObject;
                temp.GetComponent<Text>().text = content;
                float h = temp.GetComponent<Text>().preferredHeight / 2.5f;
                if (h < 40)
                    h = 50;
                else if (h < 70)
                {
                    h = 80;
                }
                temp.GetComponent<LayoutElement>().minHeight = h;
                temp.transform.SetAsLastSibling();

                if (isMine)
                {
                    temp.GetComponent<Text>().color = colorList[2];
                }
                temp.SetActive(true);
                StartCoroutine(WaitForRefresh(h));
            }

        }
        public void showBroadcastMessage(int type, string sender, string content, string emoticon, int channleType)
        {
            if (channleType == chatType)
                addALineChatContent(sender + ": " + content, sender == CPlayer.nickName);
            if (type == TYPE_QUICK_MESSAGE)
            {
                switch (CPlayer.gameName)
                {
                    case "1":   //TLMN
                        if (BoardManager.instance != null)
                            BoardManager.instance.showChatPanels(sender, content, "");
                        if (TLMNControler.instance != null)
                            TLMNControler.instance.showChatPanels(sender, content, "");
                        break;
                    case "xocdia":
                        if (XocDiaControler.instance != null)
                            XocDiaControler.instance.showChatPanels(sender, content, "");
                        break;
                    case "maubinh":
                        if (MauBinhController.instance != null)
                        {
                            MauBinhController.instance.showChatPanels(sender, content, "");
                        }
                        break;
                    case "0":
                        if (PhomController.instance != null)
                            PhomController.instance.showChatPanels(sender, content, "");
                        break;
                    case "xito":
                        if (XiToController.instance != null)
                            XiToController.instance.showChatPanels(sender, content, "");
                        break;
                    case "blackjack":
                        if (XiDachController.instance != null)
                            XiDachController.instance.showChatPanels(sender, content, "");
                        break;
                    case "chan":
                        if (ChanController.instance != null)
                            ChanController.instance.showChatPanels(sender, content, "");
                        break;
                    case "poker":
                        if (PokerController.instance != null)
                            PokerController.instance.showChatPanels(sender, content, "");
                        break;
                }

                return;
            }
            if (type == TYPE_EMOTICON)
            {
                for (int i = 2; i < 22; i++)
                {
                    if (chatSpritesList[i].name == "ico_emo_" + emoticon)
                    {
                        switch (CPlayer.gameName)
                        {
                            case "1":   //TLMN
                                if (BoardManager.instance != null)
                                    BoardManager.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                if (TLMNControler.instance != null)
                                    TLMNControler.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                            case "xocdia":
                                if (XocDiaControler.instance != null)
                                    XocDiaControler.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                            case "maubinh":
                                if (MauBinhController.instance != null)
                                    MauBinhController.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                            case "0":
                                if (PhomController.instance != null)
                                    PhomController.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                            case "xito":
                                if (XiToController.instance != null)
                                    XiToController.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                            case "blackjack":
                                if (XiDachController.instance != null)
                                    XiDachController.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                            case "chan":
                                if (ChanController.instance != null)
                                    ChanController.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                            case "poker":
                                if (PokerController.instance != null)
                                    PokerController.instance.showChatPanels(sender, content, emoticon, chatSpritesList[i]);
                                break;
                        }

                        break;
                    }
                }
            }
        }
        #endregion


        public IEnumerator _start()
        {
            //blackkkkkk.SetActive(true);
            yield return new WaitForSeconds(.5f);
            //blackkkkkk.SetActive(false);
            LoadingUIPanel.Hide();
        }
        public GameObject panelProcessing;
        public Text textProcessingValue;
        public void showProcessing(bool isShow=false) {
            if (isShow)
            {
                panelProcessing.SetActive(isShow);
            }
            else
            {
                if (panelProcessing.activeInHierarchy)
                    panelProcessing.SetActive(isShow);
            }

        }
        public void closeConfirmDialog()
        {
            if (blackPanel.activeSelf)
                blackPanel.SetActive(false);
            confirmDialogAnim.Play("DialogAnimRollBack");
            Text txt = btnDoConfirm.GetComponentInChildren<Text>();
            //txt.text = "ĐỒNG Ý";

        }


        public void showRechargeScene(bool toOpen)
        {
            //LoadingControl.instance.loadingScene.SetActive(true);
            LoadingUIPanel.Show();
            //LoadingControl.instance.openFrom = "khac";
            //LoadingControl.instance.showExChangeScene(toOpen);
            LoadingControl.instance.OpenRecharge(toOpen);
        }


        #endregion

        #region [PUBLIC VARIABLE]
        /// <summary>
        /// 0: Loading|1: Ghost
        /// </summary>
        public Sprite[] ldSpriteList;
        /// <summary>
        /// List danh sach cac avatar
        /// </summary>
        public Sprite[] avaSpriteLs;
        /// <summary>
        /// Make circle avatar
        /// </summary>
        public Material circleMaterial;
        /// <summary>
        /// 0: LoadingScene|1: splashImage|2: LogPanel|3: BlackPanel|4: RegPanel|5: errorPanel|
        /// 6: LossPass|7: recharge|8: fade|9: transparentPanel|10: Disconnect|11: PmPanel|12: ExchangePanel|13: PlayerInfoPanel
        /// 12: exchange|13: playerInfoPanel|14: notiPanel|15: noti-left-ele|16: enterGamePanel|17: settingPanel
        /// |18: chatPanel|19: AuthenPanel|20: vqmm|21:mini-btn|22: caothap|23: errorPanel2|24: Charts|25: eggbreak|26: guideAll
        /// |27: txPanel|28: MoneyTransferPanel
        /// </summary>
        public GameObject[] loadingGojList;
        /// <summary>
        /// 0: ErrorText|1: userError|2: pass error|3:req name err|4: req pass eerr|5: req pass 2 err|6: wrong pass
        /// |7: passLostText|8: PmHeader|9: noti-content|10: game-name-to-enter|11: confirm-content|12: errText2
        /// 13: noti-all
        /// </summary>
        public Text[] ldTextList;
        /// <summary>
        /// 0: userName|1: pass|2: reqName|3: reqPass|4: reqPass2|5: lossPass
        /// </summary>
        public InputField[] ldIpfList;
        /// <summary>
        /// 0: loginTog
        /// </summary>
        public Toggle[] ldTogList;
        /// <summary>
        /// 0: ErrorText
        /// </summary>
        public Color[] ldColorList;
        /// <summary>
        /// 0: noti-right|1: noti-left|2: noti-all-text|3: noti-all-panel
        /// </summary>
        public RectTransform[] ldRtfs;
        /// <summary>
        /// 0: Blue|1: yel|3: nor
        /// </summary>
        public Font[] ldFonts;

        /// <summary>
        /// 0: noti-join|1: confirm-ok|2: confirm-canncel
        /// </summary>
        public Button[] ldBtns;

        /// <summary>
        /// 0: sl7-bg|1: sl7_spin|2-4: sl7_highlight|5-7:sl7_win|8: sl7_winButLost|9: sl7_miss
        /// </summary>
        #endregion

        #region HIDE VARIVABLE
        [HideInInspector]
        public float currMessTime = -1;                 //A message timeout
        [HideInInspector]
        public static LoadingControl instance;          //Loading singleton
        [HideInInspector]
        //0: sound|1: bg_sound
        public bool[] mute;

        /// <summary>
        /// 0: mnp|
        /// </summary>
        [HideInInspector]
        public AssetBundle[] asbs = new AssetBundle[5];
        #endregion


        #region [PRIVATE VARIABLE]
        private WaitForSeconds waitFor;                 //return a wait by seconds
        private Text[] preNotiLeftChosen = new Text[2];                 //save text of last tog selected in left panel of noti popup
        private Dictionary<int, string[]> notiData = new Dictionary<int, string[]>();
        /// <summary>
        /// 0: allow spin|1: isSpinning
        /// </summary>
        private bool[] spinBool;
        private IEnumerator[] ldThread = new IEnumerator[1];

        #endregion

        public static bool isDragging = false;
        void Awake()
        {
            if (instance != null)
                DestroyImmediate(gameObject);
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public string userID;
        private OSPermissionSubscriptionState status;

        private void Start()
        {
            // set invisible Vuabaidep
            //vuaBaiDepPanel.gameObject.SetActive(false);
            showProcessing(false);
            //loadingGojList[31].SetActive(true);
            //LoadDLC.instance.OnClose();
            
            mute = new bool[2];
            mute[0] = PlayerPrefs.GetInt("mute", 0) == 1;
            mute[1] = PlayerPrefs.GetInt("mute_bg", 0) == 1;
            SoundManager.instance.isEnableMusic = PlayerPrefs.GetInt("mute", 0) == 0;
            SoundManager.instance.isEnableBGM = PlayerPrefs.GetInt("mute_bg", 0) == 0;
        }

        public void GetOneSignalID()
        {
            OutBounMessage req = new OutBounMessage("ONESIGNAL.GET_INFO");
            req.addHead();

            req.writeString(App.getDevicePlatform());
            req.writeString(CountryC);
            req.writeString(App.getProvider());

            App.ws.send(req.getReq(), (InBoundMessage res) =>
            {
                string tempId = res.readString();
                Debug.Log("ID OneSignal : " + tempId);
                InitOneSignal(tempId);
            });

        }

        public void InitOneSignal(string idOneSignal)
        {
            //"895ff78e-3134-4c9f-8a33-d86d56b273ef"
            OneSignal.StartInit(idOneSignal)
            .HandleNotificationOpened(HandleNotificationOpened)
            .EndInit();
            OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.Notification;

            //SceneManager.LoadScene("Lobby");
            status = OneSignal.GetPermissionSubscriptionState();
            if (status.subscriptionStatus.userId != null)
            {
                userID = status.subscriptionStatus.userId;
                Debug.Log("userId1: " + userID);
            }
            else
            {
                userID = "null";
                Debug.Log("userId2: " + userID);
            }

            //string deviceid = SystemInfo.deviceUniqueIdentifier;
#if UNITY_ANDROID
            string deviceId = SystemInfo.deviceUniqueIdentifier.ToLower();
#elif UNITY_IOS
            string deviceId = DeviceIDManager.GetDeviceID().ToLower();
#endif


            Debug.Log("deviceID: " + deviceId);
            try
            {
                OneSignal.SetExternalUserId(deviceId);
                Debug.Log("deviceid set");
            }
            catch (Exception e)
            {
                Debug.Log("set fail: " + e);
            }
        }

        public void changeVisibility(int value)
        {
            Application.targetFrameRate = value;
            //App.trace("VISIBLE : " + value);
        }
        private static void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
        }

        public void showRegPanel(bool toShow = true)
        {
            if (toShow)
            {
                showBlackPanel();
                ldIpfList[2].text = ldIpfList[3].text = ldIpfList[4].text = "";

                loadingGojList[4].SetActive(true);
                loadingGojList[4].transform.localScale = Vector2.zero;
                loadingGojList[4].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                loadingGojList[4].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    showBlackPanel(false);
                    loadingGojList[2].SetActive(false);
                });

            }
        }

        public void showBlackPanel(bool isShow = true)
        {
            //if(isShow)
            //loadingGojList[3].SetActive(true);
            //else
            //loadingGojList[3].SetActive(false);
        }

        public void showError(string t)
        {
            ldTextList[0].text = t;
            showErrorPanel();
        }
        public void showError2(string t)
        {
            ldTextList[12].text = t;
            showErrorPanel2();
        }

        
        public void showErrorPanel(bool toShow = true)
        {
            if (toShow)
            {
                //showBlackPanel();
                loadingGojList[5].SetActive(true);
                loadingGojList[5].transform.localScale = Vector2.zero;
                loadingGojList[5].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                loadingGojList[5].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                //showBlackPanel(false);
                loadingGojList[5].SetActive(false);
                });

            }
        }

        public void showErrorPanel2(bool toShow = true)
        {
            if (toShow)
            {
                //showBlackPanel();
                loadingGojList[23].SetActive(true);
                loadingGojList[23].transform.localScale = Vector2.zero;
                loadingGojList[23].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                loadingGojList[23].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                //showBlackPanel(false);
                loadingGojList[23].SetActive(false);
                });

            }
        }

        public void showCheckVer(string t)
        {
            ldTextList[15].text = t;
            showCheckVerPanel();
        }
        public void showCheckVer2(string t)
        {
            ldTextList[16].text = t;
            showCheckVerPanel2();
        }

        public void showCheckVerPanel(bool toShow = true)
        {
            if (toShow)
            {
                //showBlackPanel();
                loadingGojList[42].SetActive(true);
                loadingGojList[42].transform.localScale = Vector2.zero;
                loadingGojList[42].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                loadingGojList[42].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    //showBlackPanel(false);
                    loadingGojList[42].SetActive(false);
                });

            }
        }

        public void showCheckVerPanel2(bool toShow = true)
        {
            if (toShow)
            {
                //showBlackPanel();
                loadingGojList[43].SetActive(true);
                loadingGojList[43].transform.localScale = Vector2.zero;
                loadingGojList[43].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                loadingGojList[43].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    //showBlackPanel(false);
                    loadingGojList[43].SetActive(false);
                });

            }
        }

        public void closeCheckVerPanel()
        {
            loadingGojList[42].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                //showBlackPanel(false);
                loadingGojList[42].SetActive(false);
            });
        }

        public void closeCheckVerPanel2()
        {
            loadingGojList[43].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                //showBlackPanel(false);
                loadingGojList[43].SetActive(false);
            });
        }

        public void showDisconnectPanel(bool toShow = true)
        {
            if (toShow && gameObject != null)
            {
                //showBlackPanel();
                loadingGojList[10].SetActive(true);
                loadingGojList[10].transform.localScale = Vector2.zero;
                loadingGojList[10].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                AssetBundle.UnloadAllAssetBundles(true);
                DOTween.KillAll();
                StopAllCoroutines();
                if (LoadingControl.instance.asbs[0] != null)
                {
                    LoadingControl.instance.asbs[0].Unload(true);
                }

                if (LoadingControl.instance.asbs[1] != null)
                {
                    LoadingControl.instance.asbs[1].Unload(true);
                }
                CPlayer.Clear();
                SceneManager.LoadScene("Main");
                DestroyImmediate(gameObject);

            }
        }

        public void showAuthen(bool toShow)
        {

            if (toShow && gameObject != null)
            {
                //showBlackPanel();
                loadingGojList[19].SetActive(true);
                loadingGojList[19].transform.localScale = Vector2.zero;
                loadingGojList[19].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                loadingGojList[19].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                //showBlackPanel(false);
                loadingGojList[19].SetActive(false);
                });

            }


        }
        public void DoAuthen()
        {
            var req_getlinkAuthen = new OutBounMessage("PHONE.ACTIVE");
            req_getlinkAuthen.addHead();
            App.ws.send(req_getlinkAuthen.getReq(), delegate (InBoundMessage res_getlinkAuthen)
            {
                App.trace("[RECV] PHONE.ACTIVE");
                string url = res_getlinkAuthen.readString();
            //  Debug.Log(url);
#if UNITY_IOS || UNITY_ANDROID
            openWebview(url);
#else
            //Application.OpenURL(url);
            App.openNewTabWindow(url);
#endif
            //App.trace("URL AUTHEN " + url);
        });
            /*
            string url = "sms:7069?body=NE XT " + CPlayer.id;
#if UNITY_IOS
                                url = string.Format("sms:{0}?&body={1}", 7069, System.Uri.EscapeDataString("NE XT " + CPlayer.id));
#endif
            Application.OpenURL(url);
            */
        }

        public void CreatePasswordLevel2()
        {
            var req = new OutBounMessage("AGENCY_UPDATE");

            req.addHead();

            App.ws.send(req.getReq(), (InBoundMessage res) =>
            {
                string url = res.readString();
                Debug.Log(url+" Link Create Pass LV2");
                openWebview(url);

            });
        }

        public void showLogPanel(bool toShow = true)
        {

            if (toShow)
            {
                string rememberPassCheck = PlayerPrefs.GetString("rememberPass", "");
                string user_pref = PlayerPrefs.GetString("user", "");
                string pass_pref = PlayerPrefs.GetString("pass", "");
                //App.trace(pass_pref);
                ldTogList[0].isOn = false;

                if (rememberPassCheck == "false")
                {
                    ldTogList[0].isOn = false;
                }
                else
                {
                    ldTogList[0].isOn = true;
                }

                ldIpfList[0].text = user_pref;
                ldIpfList[1].text = pass_pref;

                showBlackPanel();
                loadingGojList[2].SetActive(true);
                loadingGojList[2].transform.localScale = Vector2.zero;
                loadingGojList[2].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);

            }
            else
            {
                showBlackPanel(false);
                loadingGojList[2].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    loadingGojList[2].SetActive(false);
                });

            }
        }

        public void DoLogin()
        {
            //showLogPanel(false);

            if (ldIpfList[0].text.Length == 0)
            {
                DOTween.Kill("error_name");
                ldTextList[1].color = ldColorList[0];
                ldTextList[1].gameObject.SetActive(true);
                ldTextList[1].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[1].gameObject.SetActive(false);
                }).SetId("error_name");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            if (ldIpfList[1].text.Length == 0)
            {
                DOTween.Kill("error_pass");
                ldTextList[2].color = ldColorList[0];
                ldTextList[2].gameObject.SetActive(true);
                ldTextList[2].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[2].gameObject.SetActive(false);
                }).SetId("error_pass"); ;
                return;
            }
            showLogPanel(false);
            LoadingControl.instance.showProcessing(true);
            LobbyControl.instance.DoLogin(ldIpfList[0].text, ldIpfList[1].text, ldTogList[0].isOn);
        }

        public void DoReg()
        {
            if (ldIpfList[2].text.Length == 0)
            {
                DOTween.Kill("error_name_req");
                ldTextList[3].text = "Tên tài khoản không được để trống";
                ldTextList[3].color = ldColorList[0];
                ldTextList[3].gameObject.SetActive(true);
                ldTextList[3].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[3].gameObject.SetActive(false);
                }).SetId("error_name_req");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            if (ldIpfList[2].text.Length < 6)
            {
                DOTween.Kill("error_name_req");
                ldTextList[3].text = "Tên tài khoản phải dài tối thiểu 6 ký tự";
                ldTextList[3].color = ldColorList[0];
                ldTextList[3].gameObject.SetActive(true);
                ldTextList[3].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[3].gameObject.SetActive(false);
                }).SetId("error_name_req");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            if (ldIpfList[3].text.Length == 0)
            {
                DOTween.Kill("error_pass_req");
                ldTextList[4].text = "Mật khẩu không được để trống";
                ldTextList[4].color = ldColorList[0];
                ldTextList[4].gameObject.SetActive(true);
                ldTextList[4].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[4].gameObject.SetActive(false);
                }).SetId("error_pass_req");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            if (ldIpfList[3].text.Length < 6)
            {
                DOTween.Kill("error_pass_req");
                ldTextList[4].text = "Mật khẩu phải dài tối thiểu 6 ký tự";
                ldTextList[4].color = ldColorList[0];
                ldTextList[4].gameObject.SetActive(true);
                ldTextList[4].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[4].gameObject.SetActive(false);
                }).SetId("error_pass_req");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            if (ldIpfList[4].text.Length == 0)
            {
                DOTween.Kill("error_pass_req_2");
                ldTextList[5].text = "Mật khẩu không được để trống";
                ldTextList[5].color = ldColorList[0];
                ldTextList[5].gameObject.SetActive(true);
                ldTextList[5].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[5].gameObject.SetActive(false);
                }).SetId("error_pass_req_2");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            if (ldIpfList[4].text.Length < 6)
            {
                DOTween.Kill("error_pass_req_2");
                ldTextList[5].text = "Mật khẩu phải dài tối thiểu 6 ký tự";
                ldTextList[5].color = ldColorList[0];
                ldTextList[5].gameObject.SetActive(true);
                ldTextList[5].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[5].gameObject.SetActive(false);
                }).SetId("error_pass_req_2");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            if (ldIpfList[4].text != ldIpfList[3].text)
            {
                DOTween.Kill("error_wrong_pass");
                ldTextList[6].color = ldColorList[0];
                ldTextList[6].gameObject.SetActive(true);
                ldTextList[6].DOFade(0, 2f).OnComplete(() =>
                {
                    ldTextList[6].gameObject.SetActive(false);
                }).SetId("error_wrong_pass");
                //App.showErr("Tên đăng nhập không được để trống");
                return;
            }

            LobbyControl.instance.DoReg(ldIpfList[2].text, ldIpfList[3].text);
        }

        public void DoLoginFB()
        {
            LobbyControl.instance.loginFacebook();
        }

        public void showPassLostPanel(bool toShow = true)
        {
            string url = LobbyControl.instance.forgetPassURL;


#if UNITY_IOS || UNITY_ANDROID
            openWebview(url, true);
#else
        App.openNewTabWindow(url);
        // Application.OpenURL(url);
#endif

            /*
            if (toShow)
            {
                //showBlackPanel();
                showLogPanel(false);
                loadingGojList[6].SetActive(true);
                loadingGojList[6].transform.localScale = Vector2.zero;
                loadingGojList[6].transform.DOScale(1f, .25f).SetEase(Ease.OutBack);
            }
            else
            {
                loadingGojList[6].transform.DOScale(0f, .25f).SetEase(Ease.InBack).OnComplete(() => {
                    //showBlackPanel(false);
                    loadingGojList[6].SetActive(false);
                });

            }
            */
        }
        private string urlNotiData = "";

        public void OpenUrlNotiData()
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
            Application.OpenURL(urlNotiData);
#else
        App.openNewTabWindow(urlNotiData);
#endif
        }

        public void DoPassLost()
        {
            showPassLostPanel(false);

            string url = "sms:7069?body=NE MK " + ldIpfList[5].text;
#if UNITY_IOS
                            url = string.Format("sms:{0}?&body={1}", 7069, System.Uri.EscapeDataString("NE MK " + ldIpfList[5].text));
#endif
            // Application.OpenURL(url);
            // App.openNewTabWindow(url);
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
            Application.OpenURL(url);
#else
        App.openNewTabWindow(url);
#endif
        }

        public void showRecharge(bool blackPanel = false)
        {
            //if (blackPanel)
            //loadingGojList[3].SetActive(true);
            Image img = loadingGojList[8].GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            loadingGojList[8].SetActive(true);
            img.DOFade(1, .25f).OnComplete(() =>
            {
                loadingGojList[7].SetActive(true);
                img.DOFade(0, .25f);
            });


        }

        /// <summary>
        /// Show a dialog to send sms for an user
        /// </summary>
        public void showPmPanel(string toUser, string header = "")
        {
            if (header != "")
            {
                ldTextList[8].text = header;
            }

            loadingGojList[11].SetActive(true);
        }

        public void showLogPanel(string type)
        {
            if (type == "reg")
                showRegPanel();
            else if (type == "log")
                showLogPanel();
        }

        public void OpenRecharge(bool isShow = true)
        {
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
            if (isShow == false)
            {
                loadingGojList[21].SetActive(true);
                if (CPlayer.showEvent)
                    loadingGojList[29].SetActive(true);
                //lobbyGojList[3].SetActive(true);
                //DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, -10), .5f);
                //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, 0), .5f);
                //DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 0), .5f);
                //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
                //DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-0, 60), .5f);
                return;
            }
            loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                loadingGojList[29].SetActive(false);
            //lobbyGojList[3].SetActive(false);
            //DOTween.To(() => lobbyRtfListl[0].anchoredPosition, x => lobbyRtfListl[0].anchoredPosition = x, new Vector2(0, 160), .5f);
            //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(0, -160), .5f);
            //DOTween.To(() => lobbyRtfListl[2].anchoredPosition, x => lobbyRtfListl[2].anchoredPosition = x, new Vector2(0, 180), .5f);
            //DOTween.To(() => lobbyRtfListl[1].anchoredPosition, x => lobbyRtfListl[1].anchoredPosition = x, new Vector2(30, 160), .25f);
            //DOTween.To(() => lobbyRtfListl[3].anchoredPosition, x => lobbyRtfListl[3].anchoredPosition = x, new Vector2(-2700, 60), .5f).OnComplete(() =>
            //{
            //});
                loadingGojList[7].SetActive(true);
        }

        public void OpenNoti(bool isShow)
        {
            if (!isShow)
            {
                LoadingControl.instance.loadingGojList[21].SetActive(true);
                if (CPlayer.showEvent)
                    LoadingControl.instance.loadingGojList[29].SetActive(true);
                DOTween.To(() => ldRtfs[0].anchoredPosition, x => ldRtfs[0].anchoredPosition = x, new Vector2(1500, -40), .3f);
                DOTween.To(() => ldRtfs[1].anchoredPosition, x => ldRtfs[1].anchoredPosition = x, new Vector2(-500, -40), .3f).OnComplete(() =>
                {
                    loadingGojList[14].gameObject.SetActive(false);
                //loadingGojList[3].SetActive(false);
                //LobbyControl.instance.lobbyGojList[3].SetActive(true);
            });
                return;
            }
            foreach (Transform rtf in loadingGojList[15].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != loadingGojList[15].name)
                {
                    Destroy(rtf.gameObject);
                }
            }
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            if (CPlayer.showEvent)
                LoadingControl.instance.loadingGojList[29].SetActive(false);
            OutBounMessage req_EVENT = new OutBounMessage("EVENT.LIST");
            req_EVENT.addHead();
            App.ws.send(req_EVENT.getReq(), delegate (InBoundMessage res_EVENT)
            {
                notiData.Clear();

                int count = res_EVENT.readByte();
                App.trace("[RECV] EVENT.LIST " + count);
            //App.trace("Count = " + count);
            for (int i = 0; i < count; i++)
                {
                    int index = res_EVENT.readInt();
                    string title = res_EVENT.readString();
                    string des = res_EVENT.readString();
                    string link = res_EVENT.readString();
                //App.trace(index + "|title = " + title + "|des = " + des + "|link = " + link);
                notiData.Add(index, new string[] { des, link });
                    GameObject goj = Instantiate(loadingGojList[15], loadingGojList[15].transform.parent, false);
                    Text[] arr = goj.GetComponentsInChildren<Text>();
                    arr[0].text = App.formatNickName(title, 20);
                    arr[1].text = App.formatNickName(des, 45);
                    if (i == 0)
                    {
                        arr[0].font = ldFonts[1];
                        arr[0].fontSize = 35;
                        arr[1].font = ldFonts[1];
                        arr[1].fontSize = 25;
                        preNotiLeftChosen[0] = arr[0];
                        preNotiLeftChosen[1] = arr[1];
                        setNotiData(index);
                    }
                    Button btn = goj.GetComponent<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        if (preNotiLeftChosen[0] != null && preNotiLeftChosen[1] != null)
                        {
                            preNotiLeftChosen[0].font = ldFonts[0];
                            preNotiLeftChosen[0].fontSize = 35;
                            preNotiLeftChosen[1].font = ldFonts[2];
                            preNotiLeftChosen[1].fontSize = 30;
                        }
                        arr[0].font = ldFonts[1];
                        arr[0].fontSize = 35;
                        arr[1].font = ldFonts[1];
                        arr[1].fontSize = 25;
                        preNotiLeftChosen[0] = arr[0];
                        preNotiLeftChosen[1] = arr[1];

                        setNotiData(index);
                    });
                    goj.SetActive(true);
                }
            });

            //loadingGojList[3].SetActive(true);
            loadingGojList[14].SetActive(true);
            DOTween.To(() => ldRtfs[0].anchoredPosition, x => ldRtfs[0].anchoredPosition = x, new Vector2(209, -40), .3f);
            DOTween.To(() => ldRtfs[1].anchoredPosition, x => ldRtfs[1].anchoredPosition = x, new Vector2(40, -40), .3f);

        }

        private void setNotiData(int index)
        {
            string[] arr = notiData[index];
            //ldTextList[9].text = arr[0];
            if (arr[1] == "")
            {
                ldBtns[0].gameObject.SetActive(false);
            }
            else
            {
                urlNotiData = arr[1];
                ldBtns[0].onClick.RemoveAllListeners();
                ldBtns[0].onClick.AddListener(() =>
                {
                // Application.OpenURL(arr[1]);
                urlNotiData = arr[1];
                    OpenUrlNotiData();
                });
                ldBtns[0].gameObject.SetActive(true);
            }

            var req_content = new OutBounMessage("EVENT.DETAIL");
            req_content.addHead();
            req_content.writeInt(index);
            App.ws.send(req_content.getReq(), delegate (InBoundMessage res_content)
            {
                App.trace("[RECV] EVENT.DETAIL");
                res_content.readInt();          //id
            res_content.readString();           //title
            res_content.readString();           //des
            ldTextList[9].text = res_content.readString();           //content
        });


        }

        public void TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delayTime = 0)
        {
            ldThread[0] = _TweenNum(txt, fromNum, toNum, tweenTime, scaleNum, delayTime);
            StartCoroutine(ldThread[0]);
        }
        public void StopTweenNum()
        {
            StopCoroutine(ldThread[0]);
        }
        private IEnumerator _TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float deLayTime = 0)
        {
            if (deLayTime > 0)
                yield return new WaitForSeconds(deLayTime);

            float i = 0.0f;
            float rate = 1.0f / .5f;
            if (txt != null)
            {
                txt.transform.DOScale(scaleNum, tweenTime / 2).SetId("scaleText");

                while (i < tweenTime)
                {
                    i += Time.deltaTime * rate;
                    float a = Mathf.Lerp(fromNum, toNum, i);
                    txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                    yield return null;
                }
                DOTween.Kill("scaleText");
                txt.transform.localScale = Vector2.one;
                yield return new WaitForSeconds(.05f);
            }
        }

        public void OpenChat()
        {
            loadingGojList[21].SetActive(false);
            //loadingGojList[18].SetActive(true);
            //showNotifyAll("toanpt", "CityNighy", 20000,123456);
        }

        public void CloseChat()
        {
            var reqChat = new OutBounMessage("BROADCAST");
            reqChat.addHead();
            App.ws.delHandler(reqChat.getReq());
            //loadingGojList[18].SetActive(false);
            instance.loadingGojList[21].SetActive(true);
        }

        public void ChangeSound(int id, bool value)
        {
            mute[id] = value;
        }

        public void OpenLuckyWheel()
        {
            loadingGojList[20].SetActive(true);
        }
        public void OpenUpDown()
        {
            loadingGojList[22].SetActive(true);
            loadingGojList[22].transform.SetSiblingIndex(MAX_SIBLING_INDEX);
        }
        public void OpenEggBreak()
        {
            loadingGojList[25].SetActive(true);
        }

        public void OpenTaiXiu()
        {
            loadingGojList[27].transform.DOScale(0.1f, 0.05f).OnComplete(() =>
            {
                loadingGojList[27].SetActive(true);
                loadingGojList[27].transform.DOScale(0.8f, 0.3f);
            });
            loadingGojList[27].transform.SetSiblingIndex(MAX_SIBLING_INDEX);
        }

        public void OpenMiniPocker()
        {
            loadingGojList[33].SetActive(true);
            loadingGojList[33].transform.SetSiblingIndex(MAX_SIBLING_INDEX);
        }

        public void OpenZombieSlot()
        {
            loadingGojList[36].SetActive(true);
            loadingGojList[36].transform.SetSiblingIndex(MAX_SIBLING_INDEX);
        }

        public void OpenFeast()
        {
            loadingGojList[37].transform.DOScale(0.1f, 0.05f).OnComplete(() =>
            {
                loadingGojList[37].SetActive(true);
                if (loadingGojList[37].activeSelf)
                    FeastControl.instance.ControllScrollRect();
                loadingGojList[37].transform.DOScale(1f, 0.3f);
            });
            //loadingGojList[37].SetActive(true);
            loadingGojList[37].transform.SetSiblingIndex(MAX_SIBLING_INDEX);
        }

        public void OpenMiniGame3X3()
        {
            loadingGojList[38].SetActive(true);
            loadingGojList[38].transform.SetSiblingIndex(MAX_SIBLING_INDEX);
        }

        public void OpenSlotOneLine()
        {
            loadingGojList[39].SetActive(true);
            loadingGojList[39].transform.SetSiblingIndex(MAX_SIBLING_INDEX);
        }
        public void CloseConfirm()
        {
            ldBtns[1].transform.parent.parent.gameObject.SetActive(false);
        }

        public void changeMiniGamePot(string gameName, int value)
        {

        }

        public void showNotifyAll(string name, string gameName, string bet, string balance)
        {
            SoundManager.instance.PlayUISound(SoundFX.NOTIFY_OTHER_PLAY_JACKPOT);
            //If game is opening
            if (loadingGojList[16].activeSelf)
            {
                return;
            }

            string tempString = App.listKeyText["JACKPOT_GREETING"];
            string new1 = tempString.Replace("#1", name);
            string new2 = new1.Replace("#2", gameName);
            string new3 = new2.Replace("#3", App.FormatMoney(bet));
            string new4 = new3.Replace("#4", App.FormatMoney(balance));
            ldTextList[13].text = new4 + " " + App.listKeyText["CURRENCY"]; //"Chúc mừng người chơi <color=yellow>" + name + "</color> nổ hũ game <color=yellow>"+ gameName + "</color> \nmức cược <color=yellow>" + App.FormatMoney(bet) + "</color> nhận được <color=yellow>" + App.FormatMoney(balance) + "</color> Gold";

            openNotifyAll(true);
            float w = ldRtfs[2].rect.width;
            //App.trace("====== w " + w);
            DOTween.Kill("notifyAllText");
            //ldRtfs[2].anchoredPosition = new Vector2(1100, 0);
            DOTween.To(() => ldRtfs[2].anchoredPosition, x => ldRtfs[2].anchoredPosition = x, new Vector2(0, 0), 8f).SetId("notifyAllText").OnComplete(() =>
            {
                openNotifyAll(false);
            });
        }

        public void openNotifyAll(bool toShow)
        {
            DOTween.Kill("notifyAllRtf");
            if (!toShow)
            {
                //notifyAllPanel.DOMoveY(200, .5f);

                DOTween.To(() => ldRtfs[3].anchoredPosition, x => ldRtfs[3].anchoredPosition = x, (new Vector2(0, 300)), .5f).SetId("notifyAllRtf").OnComplete(() =>
                {
                    ldRtfs[3].gameObject.SetActive(false);
                });
                return;
            }
            ldRtfs[3].gameObject.SetActive(true);
            DOTween.To(() => ldRtfs[3].anchoredPosition, x => ldRtfs[3].anchoredPosition = x, (new Vector2(0, 0)), .5f).SetId("notifyAllRtf");
        }

        private void Update()
        {
            if (currMessTime > -1)
                currMessTime += Time.deltaTime;
            if (currMessTime > 300)
            {
                currMessTime = -1;
                App.needStartSocket = false;
                App.trace("CONNECT FAILED!", "red");

                //App.showErr("Mất kết nối, vui lòng thử lại.", true);
                App.showErr(App.listKeyText["WARN_SERVER_CONNECTION"], true);


            }
        }

        public void test(int s)
        {
            App.trace(UnityEngine.Random.Range(0, 3).ToString());
            return;
            // //ldTextList[9].text = "⏱ 💰💸💸💸💸💸🌍 ➡️➡️ ❗️➡️➡️➡️➡️";
            // OutBounMessage req_EVENT = new OutBounMessage("EVENT.LIST");
            // req_EVENT.addHead();
            // App.ws.send(req_EVENT.getReq(), delegate (InBoundMessage res_EVENT)
            // {
            //     int count = res_EVENT.readByte();
            //     App.trace("Count = " + count);
            //     for (int i = 0; i < count; i++)
            //     {
            //         int index = res_EVENT.readInt();
            //         string title = res_EVENT.readString();
            //         string des = res_EVENT.readString();
            //         string link = res_EVENT.readString();
            //         App.trace(index + "|title = " + title + "|des = " + des + "|link = " + link);
            //     }
            // });
        }

        public void openMiniEvents()
        {
            loadingGojList[29].SetActive(true);
        }
#if !UNITY_WEBGL
#region //Webview

        private UniWebView _webView;

        private string _errorMessage;
        public void openWebview(string url, bool lossPast = false)
        {
            if (lossPast)
                loadingGojList[35].SetActive(true);
            else
                loadingGojList[34].SetActive(true);
            _webView = GetComponent<UniWebView>();
            if (_webView == null)
            {
                _webView = gameObject.AddComponent<UniWebView>();
                _webView.OnReceivedMessage += OnReceivedMessage;
                _webView.OnLoadComplete += OnLoadComplete;
                _webView.OnWebViewShouldClose += OnWebViewShouldClose;
                _webView.OnEvalJavaScriptFinished += OnEvalJavaScriptFinished;

                _webView.InsetsForScreenOreitation += InsetsForScreenOreitation;
            }

            //3. You can set the insets of this webview by assigning an insets value simply
            //   like this:
            /*
                    int bottomInset = (int)(UniWebViewHelper.screenHeight * 0.5f);
                    _webView.insets = new UniWebViewEdgeInsets(5,5,bottomInset,5);
            */

            // Or you can also use the `InsetsForScreenOreitation` delegate to specify different
            // insets for portrait or landscape screen. If your webpage should resize on both portrait
            // and landscape, please use the delegate way. See the `InsetsForScreenOreitation` method
            // in this file for more.

            // Now, set the url you want to load.
            _webView.url = url;
            //You can read a local html file, by putting the file into /Assets/StreamingAssets folder
            //And use the url like these
            //If you are using "Split Application Binary" for Android, see the FAQ section of manual for more.
            /*
#if UNITY_EDITOR
            _webView.url = Application.streamingAssetsPath + "/index.html";
#elif UNITY_IOS
            _webView.url = Application.streamingAssetsPath + "/index.html";
#elif UNITY_ANDROID
            _webView.url = "file:///android_asset/index.html";
#elif UNITY_WP8
            _webView.url = "Data/StreamingAssets/index.html";
#endif
            */

            // You can set the spinner visibility and text of the webview.
            // This line can change the text of spinner to "Wait..." (default is  "Loading...")
            //_webView.SetSpinnerLabelText("Wait...");
            // This line will tell UniWebView to not show the spinner as well as the text when loading.
            //_webView.SetShowSpinnerWhenLoading(false);

            //4.Now, you can load the webview and waiting for OnLoadComplete event now.
            _webView.Load();

            _errorMessage = null;

            //You can also load some HTML string instead from a url or local file.
            //When loading from the HTML string, the _webView.url will take no effect.
            //_webView.LoadHTMLString("<body>I am a html string</body>",null);

            //If you want the webview show immediately, instead of the OnLoadComplete event, call Show()
            //A blank webview will appear first, then load the web page content in it
            _webView.Show();
        }
        //5. When the webView complete loading the url sucessfully, you can show it.
        //   You can also set the autoShowWhenLoadComplete of UniWebView to show it automatically when it loads finished.
        void OnReceivedMessage(UniWebView webView, UniWebViewMessage message)
        {
            //App.trace("Received a message from native");
            App.trace(message.rawMessage);
        }

        void OnLoadComplete(UniWebView webView, bool success, string errorMessage)
        {
            if (success)
            {
                webView.Show();
            }
            else
            {
                //App.trace("Something wrong in webview loading: " + errorMessage);
                _errorMessage = errorMessage;
            }
        }

        IEnumerator closeWebView(UniWebView webView)
        {
            yield return new WaitForSeconds(.5f);
            //_webView.currentUrl
            webView.Hide();
            Destroy(webView);
            webView.OnReceivedMessage -= OnReceivedMessage;
            webView.OnLoadComplete -= OnLoadComplete;
            webView.OnWebViewShouldClose -= OnWebViewShouldClose;
            webView.OnEvalJavaScriptFinished -= OnEvalJavaScriptFinished;
            webView.InsetsForScreenOreitation -= InsetsForScreenOreitation;
            _webView = null;
            loadingGojList[34].SetActive(false);
            LobbyControl.instance.checkShowButton();
            if (loadingGojList[13].activeSelf)
            {
                loadingGojList[13].SetActive(false);
                StartCoroutine(OpenPlayerInfoScene());
                ReloadInfoPlayerAfterCloseWebView();
            }
            else
            {
                ReloadInfoPlayerAfterCloseWebView();
            }
        }

        public void ShowPlayerInforPanel() {
            loadingGojList[13].SetActive(false);
            StartCoroutine(OpenPlayerInfoScene());
        }


        private IEnumerator OpenPlayerInfoScene()
        {
            loadingGojList[13].SetActive(true);
            yield return null;
        }



        //In this demo, we set the text to the return value from js.
        void OnEvalJavaScriptFinished(UniWebView webView, string result)
        {
            //Debug.Log("js result: " + result);
        }

        //10. If the user close the webview by tap back button (Android) or toolbar Done button (iOS),
        //    we should set your reference to null to release it.
        //    Then we can return true here to tell the webview to dismiss.
        bool OnWebViewShouldClose(UniWebView webView)
        {
            if (webView == _webView)
            {
                _webView = null;
                return true;
            }
            return false;
        }

        // This method will be called when the screen orientation changed. Here we returned UniWebViewEdgeInsets(5,5,bottomInset,5)
        // for both situation. Although they seem to be the same, screenHeight was changed, leading a difference between the result.
        // eg. on iPhone 5, bottomInset is 284 (568 * 0.5) in portrait mode while it is 160 (320 * 0.5) in landscape.
        UniWebViewEdgeInsets InsetsForScreenOreitation(UniWebView webView, UniWebViewOrientation orientation)
        {
            int bottomInset = (int)(UniWebViewHelper.screenHeight * .1f);
            int topInset = (int)(UniWebViewHelper.screenHeight * .1f);
            int leftInset = (int)(UniWebViewHelper.screenWidth * .1f);
            int rightInset = (int)(UniWebViewHelper.screenWidth * .1f);
            if (orientation == UniWebViewOrientation.Portrait)
            {
                return new UniWebViewEdgeInsets(topInset, leftInset, bottomInset, rightInset);
                //return new UniWebViewEdgeInsets(5, 5, bottomInset, 5);
            }
            else
            {
                return new UniWebViewEdgeInsets(topInset, leftInset, bottomInset, rightInset);
                //return new UniWebViewEdgeInsets(5, 5, bottomInset, 5);
            }
        }

        public void ReloadInfoPlayerAfterCloseWebView()
        {
            var req_info = new OutBounMessage("PLAYER_PROFILE");
            req_info.addHead();
            req_info.writeLong(CPlayer.id);
            req_info.writeByte(0x0f);
            req_info.writeAcii("");



            App.ws.send(req_info.getReq(), delegate (InBoundMessage res)
            {
                string userName = res.readAscii();
                CPlayer.nickName = userName;
                //App.trace("userName = " + userName);
                CPlayer.fullName = res.readString();

                //App.trace("FullName = " + res.readString());
                string avatar = res.readAscii();
                CPlayer.avatar = avatar;
                //App.trace("avatar = " + avatar);
                res.readByte();
                //App.trace("ismale = " + res.readByte());

                res.readAscii();            //date of birth
                res.readString();           //message - status

                long chip = res.readLong();
                //App.trace("chip = " + chip);
                CPlayer.chipBalance = chip;
                long man = res.readLong();
                //App.trace("man = " + man);
                CPlayer.manBalance = man;

                CPlayer.phoneNum = res.readAscii();

            });
        }
        public void CloseWebViewAuthen()
        {
            StartCoroutine(closeWebView(_webView));
        }
        public void CloseWebViewLostPass()
        {
            StartCoroutine(closeWebViewPassLost(_webView));
        }

        IEnumerator closeWebViewPassLost(UniWebView webView)
        {
            yield return new WaitForSeconds(.5f);
            //_webView.currentUrl
            webView.Hide();
            Destroy(webView);
            webView.OnReceivedMessage -= OnReceivedMessage;
            webView.OnLoadComplete -= OnLoadComplete;
            webView.OnWebViewShouldClose -= OnWebViewShouldClose;
            webView.OnEvalJavaScriptFinished -= OnEvalJavaScriptFinished;
            webView.InsetsForScreenOreitation -= InsetsForScreenOreitation;
            _webView = null;
            loadingGojList[35].SetActive(false);
        }
#endregion
#endif

        public string[] productIdBundle;
        public static IStoreController m_StoreController;          // The Unity Purchasing system.
        public static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

        public void InitializePurchasing()
        {
            if (IsInitialized())
            {
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var t in productIdBundle)
            {
                builder.AddProduct(t, ProductType.Consumable);
            }

            UnityPurchasing.Initialize(this, builder);
        }


        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void BuyChipIAP(string productId)
        {
            BuyProductID(productId);
        }


        void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }


        public void RestorePurchases()
        {
            if (!IsInitialized())
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result) =>
                {
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            StartCoroutine(OnDecompileReceipt(args));
            return PurchaseProcessingResult.Pending;
        }
        public IEnumerator OnDecompileReceipt(PurchaseEventArgs args)
        {
            try
            {
                var wrapper = (Dictionary<string, object>) MiniJson.JsonDecode(args.purchasedProduct.receipt);
                if (wrapper != null)
                {
                    var store = (string) wrapper["Store"];
                    var transactionId = (string) wrapper["TransactionID"];
                    var payload = (string) wrapper["Payload"];
#if UNITY_ANDROID
                    if (payload != null)
                    {
                        var payloadDetails = (Dictionary<string, object>) MiniJson.JsonDecode(payload);
                        var json = (string) payloadDetails["json"];
                        if (json != null)
                        {
                            var jsonDetails = (Dictionary<string, object>) MiniJson.JsonDecode(json);
                            var packageName = (string) jsonDetails["packageName"];
                            var productId = (string) jsonDetails["productId"];
                            var purchaseToken = (string) jsonDetails["purchaseToken"];
                            var req = new OutBounMessage("RECHARGE_BY_IAP_ANDROID");
                            req.addHead();
                            req.writeString(packageName);
                            req.writeString(productId);
                            req.writeString(purchaseToken);
                            req.writeString(transactionId);
                            App.ws.send(req.getReq(), delegate(InBoundMessage res)
                            {
                                var chipIAP = res.readLong();
                                //App.showErr("Giao dịch thành công. Tài khoản của bạn được cộng " +
                                //            App.formatMoney(chipIAP.ToString()) + " Gold. Mã giao dịch " +
                                //            transactionId);

                                string appShowErr = App.listKeyText["TRANSACTION_SUCCESS"];
                                string new1 = appShowErr.Replace("#1", App.formatMoney(chipIAP.ToString()));
                                string new2 = new1.Replace("#2", App.listKeyText["CURRENCY"]);
                                string new3 = new2.Replace("#3", transactionId.ToString());

                                App.showErr(new3);
                            });
                        }
                    }
#elif UNITY_IOS
                    if (payload != null)
                    {
                        int lenght1 = payload.Length / 100;
                        int lenght2 = payload.Length % 100;
                        int countLength = lenght1 + (lenght2 > 0 ? 1 : 0);
                        var req = new OutBounMessage("RECHARGE_BY_IAP_IOS");
                                req.addHead();
                                req.writeString(transactionId);
                                req.writeByte(countLength);
                                for (int m = 0; m < payload.Length; m++)
                                {
                                    string a;
                                    if (m + 100 > payload.Length)
                                        a = payload.Substring(m, payload.Length - m);
                                    else
                                        a = payload.Substring(m, 100);
                                    req.writeString(a);
                                    m += 99;
                                }
                                App.ws.send(req.getReq(), delegate (InBoundMessage res)
                                {
                                    var chipIAP = res.readLong();
                                    App.showErr("Giao dịch thành công. Tài khoản của bạn được cộng " + App.formatMoney(chipIAP.ToString()) + " Gold. Mã giao dịch " + transactionId);
                                });
                    }
#endif
                }

            }
            catch (Exception e)
            {
                //App.showErr("Giao dịch không thành công");
                App.showErr(App.listKeyText["TRANSACTION_FAILED"]);

                Debug.Log(e);
            }
            finally
            {
                m_StoreController.ConfirmPendingPurchase(args.purchasedProduct);
            }

            return null;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

        }

        public ProductCollection products
        {
            get
            {
                return ((IStoreCallback)instance).products;
            }
        }

        public bool useTransactionLog
        {
            get
            {
                return ((IStoreCallback)instance).useTransactionLog;
            }

            set
            {
                ((IStoreCallback)instance).useTransactionLog = true;
            }
        }

        public void OnSetupFailed(InitializationFailureReason reason)
        {
            ((IStoreCallback)instance).OnSetupFailed(reason);
        }

        public void OnProductsRetrieved(List<ProductDescription> products)
        {
            ((IStoreCallback)instance).OnProductsRetrieved(products);
        }

        public void OnPurchaseSucceeded(string storeSpecificId, string receipt, string transactionIdentifier)
        {
            ((IStoreCallback)instance).OnPurchaseSucceeded(storeSpecificId, receipt, transactionIdentifier);
        }

        public void OnPurchaseFailed(PurchaseFailureDescription desc)
        {
            ((IStoreCallback)instance).OnPurchaseFailed(desc);
        }

    }
}
