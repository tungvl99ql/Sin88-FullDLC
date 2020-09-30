using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Server.Api
{

    public class ChatControl : MonoBehaviour
    {

        public InputField ipf;
        public Text txt;
        public ScrollRect scr;

        private List<GameObject> chatLs = new List<GameObject>();

        private int currSkip = 0;
        private float currPos = 0, currPoss = 0;
        private bool firsLoad = true;
        private bool isLoading = false;
        private IEnumerator[] tweens = new IEnumerator[1];

        private void OnEnable()
        {
            scr.verticalNormalizedPosition = 0;

            chatLs.Clear();
            ipf.text = "";
            currSkip = 0;
            currPos = 0;
            currPoss = 0;


            setHandler();

            LoadData(currSkip, true);


        }

        private void setHandler()
        {
            var req_BROADCAST = new OutBounMessage("BROADCAST");
            req_BROADCAST.addHead();
            App.ws.sendHandler(req_BROADCAST.getReq(), delegate (InBoundMessage res_BRAODCAST)
            {
                App.trace("RECV [BROADCAST]", "green");
                string nickName = res_BRAODCAST.readAscii();
                //res_BRAODCAST.print();
                string content = res_BRAODCAST.readStrings(true);
                string emoticon = res_BRAODCAST.readAscii();
                //res_BRAODCAST.print();
                int messageType = res_BRAODCAST.readByte();
                App.trace("BOARD CAST HANDLER nic = " + nickName + "|content = " + content + "|emoticon = " + emoticon + "|messType = " + messageType);
                Debug.Log("BOARD CAST HANDLER nic = " + nickName + "|content = " + content + "|emoticon = " + emoticon + "|messType = " + messageType);

                switch (messageType) {
                    case 0:// messenger
                        if (gameObject.activeSelf)
                        {
                            Text mText = Instantiate(txt, txt.transform.parent, false);
                            if (nickName.ToUpper() == "ADMIN")
                            {
                                mText.text = "<color=yellow>" + "[" + nickName + "]: " + content + "</color>";
                            }
                            else if (nickName == CPlayer.nickName)
                            {
                                mText.text = "<color=lime>" + " " + nickName + ": </color>" + content;
                            }
                            else
                                mText.text = "<color=aqua>" + " " + nickName + ": </color>" + content;


                            //App.trace(mText.text);
                            if (chatLs.Count == 100)
                            {
                                Destroy(chatLs[99]);
                                chatLs.RemoveAt(99);
                            }

                            chatLs.Insert(0, mText.gameObject);
                            mText.transform.SetAsLastSibling();
                            mText.gameObject.SetActive(true);
                            if (tweens[0] != null)
                            {
                                StopCoroutine(tweens[0]);
                            }
                            if (gameObject.activeInHierarchy)
                            {
                                tweens[0] = TweenScroll();
                                StartCoroutine(tweens[0]);
                            }
                        }
                        break;

                    case 1:
                        LoadingControl.instance.showBroadcastMessage(1, nickName, content, emoticon, LoadingControl.CHANNEL_TABLE);
                        break;
                    case 2:
                        LoadingControl.instance.showBroadcastMessage(2, nickName, content, emoticon, LoadingControl.CHANNEL_TABLE);
                        break;
                }
               


            });

        }

        private void LoadData(int skip, bool scroll = false)
        {
            isLoading = true;

            var req = new OutBounMessage("CHAT.LOAD");
            req.addHead();
            req.writeByte(1);    //chat type
            req.writeByte(skip);    //line skip
            currSkip += 80;
            req.writeByte(80);   //num of line per load
            req.writeLong(0); //hardcode for chatbox in room

            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {

                if (scroll)
                    foreach (Transform rtf in txt.transform.parent)       //Delete exits element before
                {
                        if (rtf.gameObject.name != txt.gameObject.name)
                        {
                            Destroy(rtf.gameObject);
                        }
                    }

                int count = res.readByte();
                App.trace("[RECV] CHAT.LOAD " + count);
                if (count == 0)
                    return;
                for (int i = 0; i < count; i++)
                {
                    res.readLong();
                    string nick = res.readAscii();
                    string content = res.readStrings();
                    App.trace(content);
                    Text mText = Instantiate(txt, txt.transform.parent, false);
                    if (nick.ToUpper() == "ADMIN")
                    {
                        mText.text = "<color=yellow>" + "[" + nick + "]: " + content + "</color>";
                    }
                    else if (nick == CPlayer.nickName)
                    {
                        mText.text = "<color=lime>" + " " + nick + ": </color>" + content;
                    }
                    else
                        mText.text = "<color=aqua>" + " " + nick + ": </color>" + content;


                //App.trace(mText.text);
                if (chatLs.Count == 100)
                    {
                        Destroy(chatLs[99]);
                        chatLs.RemoveAt(99);
                    }

                    chatLs.Insert(0, mText.gameObject);
                    currPos += mText.preferredHeight;
                    mText.transform.SetAsFirstSibling();
                    mText.gameObject.SetActive(true);
                    if (i == count - 1)
                    {
                        currPoss = currPos;
                    }
                }

                currPos = scr.content.rect.height;
            //App.trace("<color=green>curr num mess = " + chatLs.Count + "|" + currPoss + "|" + currPos + "</color>");
            if (tweens[0] != null)
                {
                    StopCoroutine(tweens[0]);
                }
                tweens[0] = TweenScroll();
                StartCoroutine(tweens[0]);

            });

        }

        public void DoAction(string t)
        {
            switch (t)
            {
                case "Close":
                    var reqChat = new OutBounMessage("BROADCAST");
                    reqChat.addHead();
                    App.ws.delHandler(reqChat.getReq());
                    gameObject.SetActive(false);
                    LoadingControl.instance.loadingGojList[21].SetActive(true);
                    if (CPlayer.showEvent)
                        LoadingControl.instance.loadingGojList[29].SetActive(true);
                    break;
                case "Send":
                    if (ipf.text.Length == 0)
                        return;
                    OutBounMessage req_SEND = new OutBounMessage("CHAT.SEND");
                    req_SEND.addHead();
                    req_SEND.writeString(ipf.text);         //CONTENT OF CHAT
                    req_SEND.writeByte(1);                  //CHAT FOR ALL
                    req_SEND.writeLong(0);
                    ipf.text = "";
                    App.ws.send(req_SEND.getReq(), delegate (InBoundMessage res_SEND)
                    {
                    //App.trace("Phắc cừn wao sịt", "yellow");
                }, false);
                    break;
            }
        }

        public void ScrollChange()
        {
            return;
            // if (firsLoad)
            //     return;
            // App.trace("<color=red>CURR POSS = </color>" + scr.verticalNormalizedPosition + "|" + scr.content.sizeDelta.y);
            // if (Mathf.Abs(1 - scr.verticalNormalizedPosition) > Mathf.Epsilon)
            //     return;
            // if (isLoading)
            //     return;
            // LoadData(currSkip);
        }

        private IEnumerator TweenScroll()
        {
            yield return new WaitForSeconds(.25f);
            if (this.gameObject.name.Equals("ChatFeast"))
            {
                if (DOTween.IsTweening(scr))
                {
                    DOTween.Complete("scrollF");
                }
                scr.DOVerticalNormalizedPos(0, .25f).SetId("scrollF");
                isLoading = firsLoad = false;
            }
            else
            {
                if (DOTween.IsTweening(scr))
                {
                    DOTween.Complete("scroll");
                }
                scr.DOVerticalNormalizedPos(0, .25f).SetId("scroll");
                isLoading = firsLoad = false;
            }


        }
    }
}