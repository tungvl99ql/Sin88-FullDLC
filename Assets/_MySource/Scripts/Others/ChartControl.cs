using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartControl : MonoBehaviour {

    /// <summary>
    /// 0: header|1: left|2: right|3: back|
    /// </summary>
    public RectTransform[] rtfs;

    /// <summary>
    /// 0: ele
    /// </summary>
    public GameObject[] gojs;

    /// <summary>
    /// 0: blue|1: nor|
    /// </summary>
    public Font[] fonts;

    public Text[] txts;

    /// <summary>
    /// 0-2: day-week-month
    /// </summary>
    public Button[] btns;

    /// <summary>
    /// 0-1: chart-type
    /// </summary>
    public Sprite[] sprts;

    private string[] strings = { "slotmachine2", "slotmachine", "slotmachine4", "slotmachine3", "daptrung", "minipoker", "caothap", "xengfull", "iquay", "vongquay" };

    /// <summary>
    /// 0: curr char type
    /// </summary>
    private int[] numList = new int[1];

    private string currGameName = "";

    private void OnEnable()
    {
        LoadingControl.instance.loadingGojList[21].SetActive(false);
        DOTween.To(() => rtfs[3].anchoredPosition, x => rtfs[3].anchoredPosition = x, new Vector2(0, 0), .35f);
        DOTween.To(() => rtfs[0].anchoredPosition, x => rtfs[0].anchoredPosition = x, new Vector2(0, 0), .35f);
        //DOTween.To(() => rtfs[3].anchoredPosition, x => rtfs[3].anchoredPosition = x, new Vector2(0, 0), .35f);

        numList[0] = 0;
        currGameName = "minipoker";
        ChangeChartType(2);
        //LoadData("minipoker");

    }
    
    public void LoadData(string type)
    {
        for (int i = 0; i < 10; i++)
        {
            if(type == strings[i])
                txts[i].font = fonts[0];
            else
                txts[i].font = fonts[1];
            txts[i].fontSize = 30;
        }

        currGameName = type;

        //SET DATA FOR THE LEFT
        DOTween.To(() => rtfs[1].anchoredPosition, x => rtfs[1].anchoredPosition = x, new Vector2(12, 19), .35f);

        //===SET DATA FOR THE RIGHT
        foreach (Transform rtf in gojs[0].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != gojs[0].name)
            {
                Destroy(rtf.gameObject);
            }
        }

        var req_TOP = new OutBounMessage("UTIL.TOP_GAME_PLAYER");
        req_TOP.addHead();
        req_TOP.writeString(type);
        req_TOP.writeByte(numList[0]);           //0: day|1: week|2: month
        //App.trace("type = " + type + "|" + numList[0]);
        App.ws.send(req_TOP.getReq(), delegate (InBoundMessage res_TOP)
        {
            int count = res_TOP.readByte();
            for (int i = 0; i < count; i++)
            {
                long playerId = res_TOP.readLong();
                string name = res_TOP.readString();
                string ava = res_TOP.readString();
                long score = res_TOP.readLong();
                App.trace("id = " + playerId + "|nanme = " + name + "|ava = " + ava + "|score = " + score, "yellow");

                GameObject goj = Instantiate(gojs[0], gojs[0].transform.parent, false);
                Text[] txtArr = goj.transform.GetComponentsInChildren<Text>();
                txtArr[0].text = (i + 1).ToString();
                txtArr[1].text = name;
                txtArr[2].text = App.FormatMoney(score);
                Image img = goj.GetComponentInChildren<Image>();
                goj.SetActive(true);
                goj.transform.DOShakeScale(.2f);
                StartCoroutine(App.loadImg(img, App.getAvatarLink2(ava, (int)playerId)));
            }
            DOTween.To(() => rtfs[2].anchoredPosition, x => rtfs[2].anchoredPosition = x, new Vector2(190, 19), .35f);
        });
    }

    public void ChangeChartType(int type)
    {
        if (type == numList[0])
            return;

        for (int i = 0; i < 3; i++)
        {
            if(i != type)
            {
                btns[i].interactable = true;
                btns[i].image.sprite = sprts[1];
            }
            else
            {
                btns[i].interactable = false;
                btns[i].image.sprite = sprts[0];
            }
        }

        numList[0] = type;
        LoadData(currGameName);
    }

    public void Close()
    {

        DOTween.To(() => rtfs[0].anchoredPosition, x => rtfs[0].anchoredPosition = x, new Vector2(0, 160), .35f);
        DOTween.To(() => rtfs[1].anchoredPosition, x => rtfs[1].anchoredPosition = x, new Vector2(-500, 19), .35f);
        DOTween.To(() => rtfs[2].anchoredPosition, x => rtfs[2].anchoredPosition = x, new Vector2(1500, 19), .35f);
        DOTween.To(() => rtfs[3].anchoredPosition, x => rtfs[3].anchoredPosition = x, new Vector2(0, 160), .35f).OnComplete(() => {
            gameObject.SetActive(false);
            LobbyControl.instance.OpenChart(false);
            LoadingControl.instance.loadingGojList[21].SetActive(true);
        });
    }
}
