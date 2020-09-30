using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MoneyTransferControl : MonoBehaviour {

    /// <summary>
    /// 0: title|1: back|2: body|3: search
    /// </summary>
    public RectTransform[] rtfs;

    /// <summary>
    /// 0: nick|1: value|2: search|3: note
    /// </summary>
    public InputField[] ipfs;

    /// <summary>
    /// 0: search-player-ele
    /// </summary>
    public GameObject[] gojs;

    /// <summary>
    /// 0: onl - off
    /// </summary>
    public Sprite[] sprts;

    public Text[] txts;

    /// <summary>
    /// 0: pre|1: curr|2: next
    /// </summary>
    private int[] nums = new int[3];

    private void OnEnable()
    {
        nums[1] = 1;

        DOTween.To(() => rtfs[0].anchoredPosition, x => rtfs[0].anchoredPosition = x, new Vector2(0, 0), .35f);
        DOTween.To(() => rtfs[1].anchoredPosition, x => rtfs[1].anchoredPosition = x, new Vector2(0, 0), .35f);
        DOTween.To(() => rtfs[2].anchoredPosition, x => rtfs[2].anchoredPosition = x, new Vector2(0, 0), .35f);
    }

    public void Close()
    {
        DOTween.To(() => rtfs[0].anchoredPosition, x => rtfs[0].anchoredPosition = x, new Vector2(0, 160), .35f).OnComplete(() =>
        {
            ipfs[0].text = ipfs[1].text = ipfs[3].text  = "";
            gameObject.SetActive(false);
            LobbyControl.instance.OpenMoneyTransfer(false);
            //LobbyControl.instance.imageTopBar.SetActive(true);
        });
        DOTween.To(() => rtfs[1].anchoredPosition, x => rtfs[1].anchoredPosition = x, new Vector2(0, 160), .35f);
        if (rtfs[2].gameObject.activeSelf)
        {
            DOTween.To(() => rtfs[2].anchoredPosition, x => rtfs[2].anchoredPosition = x, new Vector2(1500, 0), .35f);
        }
        else
        {
            DOTween.To(() => rtfs[3].anchoredPosition, x => rtfs[3].anchoredPosition = x, new Vector2(1500, 0), .35f).OnComplete(()=> {
                rtfs[2].anchoredPosition = new Vector2(1500, 0);
                rtfs[2].gameObject.SetActive(true);

                rtfs[3].anchoredPosition = Vector2.zero;
                rtfs[3].gameObject.SetActive(false);
            });

        }

    }

    public void OpenSearch()
    {
        ipfs[2].text = "";
        foreach (Transform rtf in gojs[0].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != gojs[0].name)
            {
                Destroy(rtf.gameObject);
            }
        }
    }



    public void Transfer()
    {
        if (ipfs[0].text.Length == 0 || ipfs[1].text.Length == 0)
            return;

        var req = new OutBounMessage("TRANSFER");
        req.addHead();
        req.writeAcii(ipfs[0].text);  //Target user
        long value = long.Parse(ipfs[1].text);
        req.writeLong(value);   //Amount to transfer
        req.writeString(ipfs[3].text);

        App.ws.send(req.getReq(), delegate (InBoundMessage res)
        {
            //App.showErr("Bạn đã chuyển thành công " + App.formatMoney(value.ToString()) + " Gold cho tài khoản " + ipfs[0].text);
            //string appShowErr = App.listKeyText["TRANSFER_SU";
            //appShowErr.Replace("#1", App.formatMoney(value.ToString()));
            //appShowErr.Replace("#2", ipfs[0].text);

            //App.showErr(appShowErr);
            //ipfs[1].text = "";
        });
    }

    public void SearchPlayer(string type)
    {
        string t = ipfs[2].text;
        if (t.Length == 0)
            return;



        if (type == "pre")
        {
            if (nums[0] < 1)
                return;
            nums[1] = nums[0];
        }
        if (type == "next")
        {
            if (nums[2] < 1)
                return;
            nums[1] = nums[2];

        }

        if (type == "")
        {
            nums[1] = 1;
        }


        var req_PLAYER_LIST = new OutBounMessage("FRIEND.LIST");
        req_PLAYER_LIST.addHead();
        req_PLAYER_LIST.writeByte(0);
        req_PLAYER_LIST.writeString(t);
        req_PLAYER_LIST.writeShort((short)nums[1]);      //start from 1 (page)
        App.ws.send(req_PLAYER_LIST.getReq(), delegate (InBoundMessage res_player_list)
        {
            txts[0].text = nums[1].ToString();

            foreach (Transform rtf in gojs[0].transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != gojs[0].name)
                {
                    Destroy(rtf.gameObject);
                }
            }

            int count = res_player_list.readByte();
            for (int i = 0; i < count; i++)
            {
                if (i > 5)
                    continue;

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
                GameObject goj = Instantiate(gojs[0], gojs[0].transform.parent, false);

                /*Image[] imgArr = goj.GetComponentsInChildren<Image>();
                imgArr[3].sprite = isOnline ? sprts[0] : sprts[1];
                StartCoroutine(App.loadImg(imgArr[1], App.getAvatarLink2(avatar, avatarId)));*/
                goj.transform.GetChild(2).GetComponent<Image>().overrideSprite = isOnline ? sprts[0] : sprts[1];
                StartCoroutine(App.loadImg(goj.transform.GetChild(0).GetChild(0).GetComponent<Image>(), App.getAvatarLink2(avatar, avatarId)));


                Text[] txtArr = goj.GetComponentsInChildren<Text>();
                txtArr[0].text = App.formatNickName(nickName, 15);
                txtArr[1].text = isOnline ? "Online" : "Offline";

                Button btn = goj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    App.trace("CHOOSE " + nickName);
                    ipfs[2].text = "";
                    ipfs[0].text = nickName;
                });

                goj.SetActive(true);

            }

            nums[0] = res_player_list.readShort();  //pre
            nums[2] = res_player_list.readShort();    //next
            App.trace("[RECV] FRIEND.LIST " + nums[0] + "|" + nums[2] + "|||" + count);
        });
    }
}
