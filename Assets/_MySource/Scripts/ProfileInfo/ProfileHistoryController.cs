using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI.Dates;
using Core.Server.Api;
public class ProfileHistoryController : MonoBehaviour
{
    public static ProfileHistoryController instance;

    public ScrollRect scrollRect;
    public GameObject conten;
    public GameObject item;
    public Button buttonNextPage, buttonPrePage;
    public Text txtPageValue;
    public Dropdown dropdownGame;
    public InputField inputDate;

    public DatePicker datetime;
    public Color32[] colors;
    private string gameNameToSearch = "all";
    private int page = 1;
    private string date;
    private List<string> lstgame = new List<string>();
    private List<string> lstgameCode = new List<string>();
    private bool isFish = true;
    private float time = 5;
    private void Awake()
    {
        if (instance != null)
            DestroyImmediate(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        isFish = true;
        state = true;
        // inputDate.text = DateTime.Now.ToString("dd-MM-yyyy");
        page = 1;
        //  inputDate.text = DateTime.Now.ToString("dd-MM-yyyy");
        datetime.DayButtonClicked(DateTime.Now);
        LoadDataHistory(gameNameToSearch, page, DateTime.Now, DateTime.Now);

        inputDate.onValueChanged.RemoveAllListeners();
        inputDate.onValueChanged.AddListener(onValueChangedDate);

    }
    private void OnDisable()
    {
        datetime.DayButtonClicked(DateTime.Now);
        inputDate.onValueChanged.RemoveAllListeners();
    }
    void Start()
    {

    }


    void Update()
    {

    }
    private void onValueChangedDate(string data)
    {
        // page = 1;
        date = data;
        //LoadDataHistory(gameNameToSearch, page,data,data);
    }
    private static string[] gameNameLabel = { "Tất cả", "Mini Poker", "Bầu Cua", "Tài Xỉu" };
    private static string[] gameID = { "all","minipoker", "baucua", "taixiu" };

    private void LoadDataHistory(string gameToSearch, int pg, string dateStart, string dateEnd)
    {
        gameNameToSearch = gameToSearch;
        var req_his = new OutBounMessage("MATCH.HISTORY");
        req_his.addHead();
        req_his.writeString(gameToSearch);                                     //GET ALL HIS
        req_his.writeByte(pg);                                           //Page num
        string t1 = dateStart;
        string t2 = dateEnd;
        date = t1;
        //  Debug.Log(t1);
        // Debug.Log(gameToSearch);
        req_his.writeString(t1);                                         //Start date to get
        req_his.writeString(t2);                                         //End date to get

        App.ws.send(req_his.getReq(), delegate (InBoundMessage res_info)
        {
            foreach (Transform rtf in conten.transform)       //Delete exits element before
            {
                Destroy(rtf.gameObject);
            }
            int count = res_info.readByte();
            Debug.Log(count);
            if (count == 0)
            {
                if (page > 1)
                    page--;
                buttonNextPage.interactable = false;
                string[] gameNameLabeltmp1 = res_info.readStrings().Split(',');
                string[] gameIDtmp1 = res_info.readStrings().Split(',');
                int pos1 = dropdownGame.value;
                if (gameIDtmp1.Length > gameID.Length)
                {
                    gameID = gameIDtmp1;
                    gameNameLabel = gameNameLabeltmp1;


                }
                lstgameCode.Clear();
                lstgame.Clear();
                dropdownGame.ClearOptions();
                for (int j = 0; j < gameNameLabel.Length; j++)
                {
                    lstgame.Add(gameNameLabel[j]);
                    lstgameCode.Add(gameID[j]);
                }
                dropdownGame.AddOptions(lstgame);
                dropdownGame.value = pos1;
                return;
            }
            else
            {
                buttonNextPage.interactable = true;
            }

            for (int i = 0; i < count; i++)
            {
                //int index = res_info.readInt();     //index
                long id = res_info.readLong();
                string time = res_info.readStrings();
                string gameName = res_info.readStrings();
                string betTotal = res_info.readStrings();
                string arise = res_info.readStrings();
                string money = res_info.readStrings();


                GameObject goj = Instantiate(item, conten.transform, false);
                Text[] arr = goj.GetComponentsInChildren<Text>();
                arr[0].text = id.ToString();
                arr[1].text = time;
                arr[2].text = gameName;
                arr[3].text = arise;
                arr[4].text = money;
                foreach (Text t in arr)
                {
                    t.color = colors[0];
                }
                goj.SetActive(true);

            }
            string [] gameNameLabeltmp = res_info.readStrings().Split(',');
            string[]  gameIDtmp = res_info.readStrings().Split(',');
            int pos = dropdownGame.value;
            if (gameIDtmp.Length>gameID.Length)
            {
                gameID = gameIDtmp;
                gameNameLabel = gameNameLabeltmp;

               
            }
            lstgameCode.Clear();
            lstgame.Clear();
            dropdownGame.ClearOptions();
            for (int j = 0; j < gameNameLabel.Length; j++)
            {
                lstgame.Add(gameNameLabel[j]);
                lstgameCode.Add(gameID[j]);
            }
            dropdownGame.AddOptions(lstgame);
            dropdownGame.value = pos;
        });
      
          
    
        txtPageValue.text = page.ToString();
    }

    private void LoadDataHistory(string gameToSearch, int pg, DateTime dateStart, DateTime dateEnd)
    {
           gameNameToSearch = gameToSearch;
        var req_his = new OutBounMessage("MATCH.HISTORY");
        req_his.addHead();
        req_his.writeString(gameToSearch);                                     //GET ALL HIS
        req_his.writeByte(pg);                                           //Page num
        string t1 = dateStart.ToString("dd-MM-yyyy");
        string t2 = dateEnd.ToString("dd-MM-yyyy");
        date = t1;
        // Debug.Log(t1);
        // Debug.Log(gameToSearch);
        req_his.writeString(t1);                                         //Start date to get
        req_his.writeString(t2);                                         //End date to get

        App.ws.send(req_his.getReq(), delegate (InBoundMessage res_info)
        {


            foreach (Transform rtf in conten.transform)       //Delete exits element before
            {
                Destroy(rtf.gameObject);
            }
            int count = res_info.readByte();

            if (count == 0)
            {
                if (page > 1)
                    page--;
                buttonNextPage.interactable = false;
                string[] gameNameLabeltmp1 = res_info.readStrings().Split(',');
                string[] gameIDtmp1 = res_info.readStrings().Split(',');
                int pos1 = dropdownGame.value;
                if (gameIDtmp1.Length > gameID.Length)
                {
                    gameID = gameIDtmp1;
                    gameNameLabel = gameNameLabeltmp1;


                }
                lstgameCode.Clear();
                lstgame.Clear();
                dropdownGame.ClearOptions();
                for (int j = 0; j < gameNameLabel.Length; j++)
                {
                    lstgame.Add(gameNameLabel[j]);
                    lstgameCode.Add(gameID[j]);
                }
                dropdownGame.AddOptions(lstgame);
                dropdownGame.value = pos1;
                return;
            }
            else
            {
                buttonNextPage.interactable = true;
            }

            for (int i = 0; i < count; i++)
            {
                //int index = res_info.readInt();     //index
                long id = res_info.readLong();
                string time = res_info.readStrings();
                string gameName = res_info.readStrings();
                string betTotal = res_info.readStrings();
                string arise = res_info.readStrings();
                string money = res_info.readStrings();


                GameObject goj = Instantiate(item, conten.transform, false);
                Text[] arr = goj.GetComponentsInChildren<Text>();
                arr[0].text = id.ToString();
                arr[1].text = time;
                arr[2].text = gameName;
                arr[3].text = arise;
                arr[4].text = money;
                foreach (Text t in arr)
                {
                    t.color = colors[0];
                }
                goj.SetActive(true);

            }

            string[] gameNameLabeltmp = res_info.readStrings().Split(',');
            string[] gameIDtmp = res_info.readStrings().Split(',');
            int pos = dropdownGame.value;
            if (gameIDtmp.Length > gameID.Length)
            {
                gameID = gameIDtmp;
                gameNameLabel = gameNameLabeltmp;


            }
            lstgameCode.Clear();
            lstgame.Clear();
            dropdownGame.ClearOptions();
            for (int j = 0; j < gameNameLabel.Length; j++)
            {
                lstgame.Add(gameNameLabel[j]);
                lstgameCode.Add(gameID[j]);
            }
            dropdownGame.AddOptions(lstgame);
            dropdownGame.value = pos;
        });
      
        txtPageValue.text = page.ToString();
    }

    public void onValueChanged()
    {
        // page = 1;
        // Debug.Log(lstgameCode[dropdownGame.value]);
        //LoadDataHistory(lstgameCode[dropdownGame.value], page, date, date);
        // Debug.Log(dropdownGame.value);
        gameNameToSearch = lstgameCode[dropdownGame.value];
    }

    public void ChangePage(int side)
    {
        // Debug.Log("OK " + side);
        if (side == 0 && page == 1)
            return;
        if (side == 0)
        {
            page--;
            if (page <= 1)
                page = 1;
        }
        else
        {
            page++;
        }
        //Debug.Log(page);
        LoadDataHistory(gameNameToSearch, page, date, date);
    }
    public void buttonSearch()
    {

        if (!state)
            return;
        state = false;
        StartCoroutine(CountDown());
        page = 1;
        LoadDataHistory(gameNameToSearch, page, date, date);
    }
    private bool state = true;
    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(time);
        state = true;
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
}
