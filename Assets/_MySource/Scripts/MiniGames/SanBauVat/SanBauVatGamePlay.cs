using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Slot.Games.Fish;

public class SanBauVatGamePlay : MonoBehaviour {

    public static SanBauVatGamePlay instance;
    public FishGameControllUI fishGameControllUI;

    public Text txtNumFree;
    public Text txtResult;

    private int totalAllPrize = 0;
    int CDTimeAuto;
    int CDTimePick;
    public bool isAuto;

    public Sprite []spriterBauVatActive;
    public Sprite[] spriterBauVatUnActive;
    public Sprite spriterTranfefden;
    public Image []imageSpriter;
    public Image imageResult;
    public Button buttonSpin;
    public Button buttonShowUp;

    public GameObject panelCDtime,panelTotalMoney;
    public Text txtCDTime,txtTotalMoney;
    public Text[] txtValue;

    public const string STRING_CD_TIME_AUTO = "Hệ thống tự động mở sau: ";
    public const string STRING_CD_TIME_AUTO_ENDGAME = "Game sẽ kết thúc sau: ";
    public const string STRING_AUTO_DROPING_BALL = "Đang tự động mở....";

    private IEnumerator IEAutoTime;
    private int totalMoney=0;
    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
        }
    }
    private void OnEnable()
    {
        SanBauVatTDK.instance.isSpin = false;
        CDTimeAuto = 15;
        CDTimePick = 2;
        totalMoney = 0;
        isAuto = true;
        Init();
        if (IEAutoTime != null)
            StopCoroutine(IEAutoTime);
        IEAutoTime = CorouCDTimeAuto();
        StartCoroutine(IEAutoTime);
    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }
    void Start () {
    }

    private List<int> listId = new List<int>();
    public void HandleJoinGame(List<int> list)
    {
        listId = list;
        for (int i = 0; i < listId.Count; i++)
        {
          
            txtValue[i].text = App.formatMoneyD_M(listId[i], true);
            Debug.Log(listId[i].ToString());
           // txtSlots[i].text = App.formatMoneyD_M(list[i]);
        }
    }
    public void ChangeText(Text txtObject, object str)
    {
        txtObject.text = str.ToString();
    }
    public void Init()
    {
        txtResult.text = "";
        panelCDtime.SetActive(false);
        panelTotalMoney.SetActive(false);
        imageResult.overrideSprite = spriterTranfefden;
        buttonShowUp.interactable = true;
        buttonSpin.interactable = true;
        for(int i=0;i< imageSpriter.Length;i++)
        {
            imageSpriter[i].overrideSprite = spriterBauVatUnActive[i];
            imageSpriter[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    public void ShowResult(int money)
    {
        int pos = -1;
        StopCoroutine("ShowResultEffect");
        for (int i = 0; i < imageSpriter.Length; i++)
        {
            imageSpriter[i].overrideSprite = spriterBauVatUnActive[i];
            imageSpriter[i].transform.GetChild(0).gameObject.SetActive(false);
            if (money == listId[i])
            {
                pos = i;
            }
        }
        StartCoroutine(ShowResultEffect(money,pos));
      
        //HandleFreeTurnEqualZero();
    }
    public void ShowClear()
    {
        for (int i = 0; i < imageSpriter.Length; i++)
        {
            imageSpriter[i].overrideSprite = spriterBauVatUnActive[i];
            imageSpriter[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        txtResult.text = "";
    }
    private IEnumerator ShowResultEffect(int money,int pos)
    {
        int count = 5;
        float speed = 0f;
        for (int i = 0, j=0; i < imageSpriter.Length&&j<count; i++)
        {
            if(i==0)
            {
                imageSpriter[i].overrideSprite = spriterBauVatActive[i];
                imageSpriter[imageSpriter.Length-1].overrideSprite = spriterBauVatUnActive[imageSpriter.Length-1];
                j++;
                speed += 0.1f;
            }
            else
            {
                imageSpriter[i-1].overrideSprite = spriterBauVatUnActive[i-1];
                imageSpriter[i].overrideSprite = spriterBauVatActive[i];
            }
            if (i == imageSpriter.Length - 1)
                i = -1;
            yield return new WaitForSeconds(speed);
        }

      /*  for (int i = 0; i < imageSpriter.Length; i++)
        {
            imageSpriter[i].overrideSprite = spriterBauVatUnActive[i].Sprite;
            imageSpriter[i].transform.GetChild(0).gameObject.SetActive(false);
        }*/
       // yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < listId.Count; i++)
        {


            if (i == 0)
            {
                imageSpriter[i].overrideSprite = spriterBauVatActive[i];
                imageSpriter[imageSpriter.Length - 1].overrideSprite = spriterBauVatUnActive[imageSpriter.Length - 1];
            }
            else
            {
                imageSpriter[i - 1].overrideSprite = spriterBauVatUnActive[i - 1];
                imageSpriter[i].overrideSprite = spriterBauVatActive[i];
            }

            if (i == pos)
            {
                imageSpriter[i].overrideSprite = spriterBauVatActive[i];
                imageResult.overrideSprite= spriterBauVatActive[i];
                imageSpriter[i].transform.GetChild(0).gameObject.SetActive(true);
                txtResult.text = money.ToString();
                totalMoney += money;
                break;
            }
            yield return new WaitForSeconds(speed+0.01f);

            /*if (money == listId[i])
            {
                imageSpriter[i].overrideSprite = spriterBauVatActive[i].Sprite;
                imageSpriter[i].transform.GetChild(0).gameObject.SetActive(true);
            }*/
        }
        SanBauVatTDK.instance.onDropEndEvent.Invoke();
    }
    public void ShowUpResult(int money)
    {
        for (int i = 0; i < imageSpriter.Length; i++)
        {
            imageSpriter[i].overrideSprite = spriterBauVatUnActive[i];
            imageSpriter[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        //txtResult.text = money.ToString();
        /* for (int i = 0; i < listId.Count; i++)
         {
             if (money == listId[i])
             {
                 imageSpriter[i].overrideSprite = spriterBauVatActive[i].Sprite;
                 imageSpriter[i].transform.GetChild(0).gameObject.SetActive(true);
             }
         }*/
        if (IEAutoTime != null)
            StopCoroutine(IEAutoTime);
        panelCDtime.SetActive(false);

        txtTotalMoney.text = "";
        totalMoney = money;
        HandleFreeTurnEqualZero();
    }
    public void DisableButton()
    {
        buttonShowUp.interactable = false;
        buttonSpin.interactable = false;
        imageResult.overrideSprite = spriterTranfefden;
        ShowClear();
    }
    public void AllowSpin()
    {
        buttonSpin.interactable = true;
    }
    public void AutoSpin()
    {
        panelCDtime.SetActive(true);
        txtCDTime.text = STRING_AUTO_DROPING_BALL;
        StartCoroutine(IEAutoSpin());
    }
    private IEnumerator IEAutoSpin()
    {
        yield return new WaitForSeconds(CDTimePick);
        SanBauVatTDK.instance.SendAndGetStartSpin();
    }
    public void StopAuto()
    {
        isAuto = false;
        if (IEAutoTime != null)
            StopCoroutine(IEAutoTime);
        panelCDtime.SetActive(false);
    }
    private IEnumerator CorouCDTimeAuto()
    {
        for (int i = 0; i < CDTimeAuto; i++)
        {
            ChangeTextAuto((CDTimeAuto - i).ToString(),true);
            yield return new WaitForSeconds(1);
            if (i == CDTimeAuto - 1)
            {
                ChangeTextAuto((CDTimeAuto - i).ToString(), false);
            }
        }
        isAuto = true;
        DisableButton();
        AutoSpin();
    }
    public void ChangeTextAuto(string count = "15", bool isEndCD = false)
    {
        if (isEndCD)
        {
            panelCDtime.SetActive(true);
            txtCDTime.text = STRING_CD_TIME_AUTO + count+" giây";
        }
        else
        {
            panelCDtime.SetActive(false);
        }
    }

    public void HandleFreeTurnEqualZero()
    {
        txtTotalMoney.text = totalMoney.ToString();
        panelTotalMoney.SetActive(true);
        StartCoroutine(CorouCDTimeEnd());
    }
    private IEnumerator CorouCDTimeEnd()
    {
        int time = 5;
        txtCDTime.text =STRING_CD_TIME_AUTO_ENDGAME+ time.ToString() +" giây";
        panelCDtime.SetActive(true);
        for (int i = time; i >= 0; i--)
        {
            txtCDTime.text = STRING_CD_TIME_AUTO_ENDGAME + i.ToString() + " giây";
            yield return new WaitForSeconds(1f);

        }
       
        BackToBigGame();
    }
    private void BackToBigGame()
    {
        this.gameObject.SetActive(false);
        //Handle when end game
        if (fishGameControllUI.EndMiniGameEvent != null)
        {
            fishGameControllUI.EndMiniGameEvent.Invoke();
        }
    }
}
