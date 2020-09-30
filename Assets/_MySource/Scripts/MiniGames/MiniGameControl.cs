using Core.Server.Api;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class MiniGameControl : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public static MiniGameControl instance;
    public static GameObject itemBeginDragged;
    public GameObject blackPanel;
    public Sprite[] spriteList;
    public Image graphics;
    private Vector3 startPos;
    public GameObject gojTaiXiu;
    public Transform[] minList;
    /// <summary>
    /// 0: white|1: trans
    /// </summary>
    public Color[] colorList;
    private List<string> handlers;
    private void Awake()
    {
        if (instance != null)
            DestroyImmediate(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    private void OnEnable()
    {
        if (gojTaiXiu.activeInHierarchy)
            return;
        ClearData();
        Loaddata();
    }
    private void OnDisable()
    {
        //  DelHandler();
        if (gojTaiXiu.activeInHierarchy)
            return;
        ClearData();

    }
    public void OnEnableTX()
    {
        ClearData();
    }
    public void OnDisableTX()
    {
        Loaddata();
        // RegHandler();
    }
    private void ClearData()
    {
        DelHandler();
        txtCountDown.text = "";
        txtCountDown2.text = "";
        timeSpinFX.SetActive(false);
        panelCountDown2.SetActive(false);
        panelCountDown.SetActive(false);
    }

    #region Tai Xiu
    private void Loaddata()
    {
        if (!CPlayer.logedIn)
            return;
        threads = new IEnumerator[5];
        handlers = new List<string>();
        panelCountDown.SetActive(true);
        panelCountDown2.SetActive(true);
        var req_TX_ENTER = new OutBounMessage("TAIXIU.ENTER");
        req_TX_ENTER.addHead();
        App.ws.send(req_TX_ENTER.getReq(), delegate (InBoundMessage res_TX_ENTER)
        {
            Debug.Log("ENTER");
            string currState = res_TX_ENTER.readString();  //currTimeCountDown;
            int seconds = res_TX_ENTER.readByte();
            res_TX_ENTER.readInt().ToString(); //Số ng đặt tài
            res_TX_ENTER.readInt().ToString(); //Số ng đặt xỉu
            res_TX_ENTER.readLong().ToString();    //Tiền cửa tài
            res_TX_ENTER.readLong().ToString();    //Tiền cửa xỉu
            res_TX_ENTER.readLong().ToString();  //Tiền mình đặt tài
            res_TX_ENTER.readLong().ToString();  //Tiền mình đặt xỉu


            int count = res_TX_ENTER.readByte();
            for (int i = 0; i < count; i++)
            {
                int gateId = res_TX_ENTER.readByte();   //0: tai|1: xiu
            }
            res_TX_ENTER.readInt().ToString();

            CountDown(seconds);
            EnterState(currState);

            //RegHandler();
        });

        RegHandler();
    }
    private void RegHandler()
    {
        if (!CPlayer.logedIn)
            return;

        //handlers.Clear();
        var req_PREPARE = new OutBounMessage("TAIXIU.PREPARE");
        req_PREPARE.addHead();
        handlers.Add("TAIXIU.PREPARE");
        App.ws.sendHandler(req_PREPARE.getReq(), delegate (InBoundMessage res_PREPARE)
        {
            //Debug.Log("PREPARE");
            res_PREPARE.readByte();
            EnterState("prepare");
            CountDown(res_PREPARE.readByte());
        });

        var req_START = new OutBounMessage("TAIXIU.START");
        req_START.addHead();
        handlers.Add("TAIXIU.START");
        App.ws.sendHandler(req_START.getReq(), delegate (InBoundMessage res_START)
        {
          //  Debug.Log("START");
            res_START.readByte();
            CountDown(res_START.readByte());
            res_START.readInt().ToString(); //Update curr thread
            EnterState("bet");
        });

        var req_SELL_GATE = new OutBounMessage("TAIXIU.SELL_GATE");
        req_SELL_GATE.addHead();
        handlers.Add("TAIXIU.SELL_GATE");
        App.ws.sendHandler(req_SELL_GATE.getReq(), delegate (InBoundMessage res_SELL_GATE)
        {
            res_SELL_GATE.readByte();
            CountDown(res_SELL_GATE.readByte());
            EnterState("sellGate");
        });

        var req_SHOW_RS = new OutBounMessage("TAIXIU.SHOW_RESULT");
        req_SHOW_RS.addHead();
        handlers.Add("TAIXIU.SHOW_RESULT");
        App.ws.sendHandler(req_SHOW_RS.getReq(), delegate (InBoundMessage res_SHOW_REULT)
        {
          //  Debug.Log("SHOW_RESULT");
            res_SHOW_REULT.readByte();

            int[] rs = new int[3];
            rs[0] = res_SHOW_REULT.readByte();
            rs[1] = res_SHOW_REULT.readByte();
            rs[2] = res_SHOW_REULT.readByte();
            int gateId = res_SHOW_REULT.readByte();
            int time = res_SHOW_REULT.readInt();
            string content = gateId == 0 ? "TÀI" : "XỈU";
            if (threads[0] != null)
                StopCoroutine(threads[0]);
            txtCountDown.text = content;
            txtCountDown2.text = content;
            if (threads_SpinDices != null)
                StopCoroutine(threads_SpinDices);

            threads_SpinDices = SpinDices(rs, gateId);
            StartCoroutine(threads_SpinDices);
            EnterState("showResult", content);
            // CountDown(time);
        });



    }
    private IEnumerator threads_SpinDices;
    public Text txtCountDown, txtCountDown2;
    private IEnumerator[] threads;
    public GameObject panelCountDown, panelCountDown2;
    private void CountDown(int time)
    {
      //  Debug.Log("time   => " + time);
        if (threads[0] != null)
            StopCoroutine(threads[0]);
        threads[0] = CountDown(time, txtCountDown);
      if(gameObject.activeInHierarchy)
            StartCoroutine(threads[0]);
    }
    private IEnumerator CountDown(int time, Text txt)
    {
        int count = time;

       // Debug.Log(" count = " + count);
        while (count > -1)
        {
            // if(count>0)
            // txt.text =  string.Format("{0:0,0}", count);
            //txtCountDown2.text= string.Format("{0:0,0}", count);
            txt.text = count.ToString();
            txtCountDown2.text = count.ToString();
            time_CountDown = count;
            yield return new WaitForSeconds(1f);
            count--;
        }
    }
    private void DelHandler()
    {
        if (!CPlayer.logedIn)
            return;
        if (handlers == null)
            return;
        for (int i = 0; i < handlers.Count; i++)
        {
            var req = new OutBounMessage(handlers[i]);
            req.addHead();
            App.ws.delHandler(req.getReq());
        }


    }
    private void EnterState(string state, string content = "")
    {
        txtCountDown.color = TXControl.colorTextTime[0];
        txtCountDown2.color = TXControl.colorTextTime[0];
        isBet = false;
        switch (state)
        {
            case "bet":     //Đặt cửa
                isBet = true;
                if (spinFX != null)
                    StopCoroutine(spinFX);
                timeSpinFX.transform.rotation = new Quaternion(0, 0, 0, 0);
                timeSpinFX.SetActive(true);
                spinFX = Spin_Time_FX();
                StartCoroutine(spinFX);

                break;
            case "prepare":     //Chuẩn bị
                if (threads_SpinDices != null)
                    StopCoroutine(threads_SpinDices);
                DOTween.Kill("idtweensl");
                panelCountDown.transform.localScale = new Vector3(1f, 1f, 0);
                timeSpinFX.SetActive(false);
                break;
            case "sellGate":     //Hệ thống cân cửa
                timeSpinFX.SetActive(false);
                break;
            case "showResult":     //Kết quả
                timeSpinFX.SetActive(false);
                break;
            default:        //Vui lòng chờ
                timeSpinFX.SetActive(false);
                break;
        }
    }
    private IEnumerator SpinDices(int[] ids, int gateId)
    {
        //App.trace(ids[0] + "|" + ids[1] + "|" + ids[2], "red");
        yield return new WaitForSeconds(0f);

        panelCountDown.transform.DOScale(1.2f, .125f).SetLoops(1400, LoopType.Yoyo).OnComplete(() =>
        {
            panelCountDown.transform.localScale = new Vector3(1f, 1f, 0);
        }).SetId("idtweensl");


    }
    #endregion

    #region Time FX
    private int speedMove = 700;
    private float waittingTime = 0.1f;
    private int time_CountDown = 0;
    private bool isBet = false;
    public GameObject timeSpinFX;
    private IEnumerator spinFX = null;
    private IEnumerator Spin_Time_FX()
    {

        while (timeSpinFX.activeInHierarchy)
        {
            yield return new WaitForSeconds(waittingTime);

            if (time_CountDown > 20)
            {
                speedMove = 700;
                txtCountDown.color = TXControl.colorTextTime[0];
                txtCountDown2.color = TXControl.colorTextTime[0];
            }
            else if (time_CountDown > 10)
            {
                speedMove = 1500;
                txtCountDown.color = TXControl.colorTextTime[1];
                txtCountDown2.color = TXControl.colorTextTime[1];
            }
            else
            {

                if (isBet)
                {
                speedMove = 2500;
                    txtCountDown.color = TXControl.colorTextTime[2];
                    txtCountDown2.color = TXControl.colorTextTime[2];
                }
            }
            if (timeSpinFX.activeInHierarchy)
            {
                timeSpinFX.transform.Rotate(Vector3.back * Time.deltaTime * speedMove);
            }
        }
        // StartCoroutine(Spin_Time_FX());
    }
    #endregion

    #region Button Mini Game
    public void OnBeginDrag(PointerEventData eventData)
    {
        itemBeginDragged = gameObject;
        startPos = transform.position;
        //App.trace("AGUGU");
        isDragging = true;
        //graphics.color = colorList[1];
    }

    public void OnDrag(PointerEventData eventData)
    {
        close();
        if (isShowing)
        {
            OnEndDrag(eventData);
            return;
        }
        transform.position = Input.mousePosition;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        //graphics.color = colorList[0];
        itemBeginDragged = null;
        float y = transform.position.y;
        float x = transform.position.x;
        RectTransform mTransform = gameObject.GetComponent<RectTransform>();
        if (y >= Screen.height - mTransform.rect.width / 2)
        {
            y = Screen.height - .5f * mTransform.rect.width;

        }
        if (y <= mTransform.rect.width / 2)
        {
            y = .5f * mTransform.rect.width;
        }
        if (x > (Screen.width - mTransform.rect.width / 2) / 2)
        {
            x = Screen.width - .5f * mTransform.rect.width;
            isFlip = false;
        }
        else
        {
            x = mTransform.rect.width / 2;
            isFlip = true;
        }
      //  Debug.Log("Y = " + y + "|X" + x);
        //App.trace("Y = " + y + "|X" + x);
        Vector3 mPos = new Vector3(x, y);
        transform.position = mPos;
        
        isDragging = false;
    }

    public GameObject gamesPanel;
    private bool isDragging = false, isShowing = false, isFlip = false;
    public void show()
    {
        if (isDragging)
            return;
        if (isShowing)
        {
            graphics.sprite = spriteList[0];
            close();
            return;
        }
        //blackPanel.SetActive(true);

        gamesPanel.SetActive(true);

        transform.localPosition = Vector2.zero;
        /*
        foreach (Transform trf in minList)
        {
            trf.localScale = new Vector3((isFlip ? -1 : 1), 1, 1);
        }
        gamesPanel.transform.DOScale(new Vector2(!isFlip ? 1 : -1, 1), .5f).SetEase(Ease.OutBack).OnComplete(() => {

        });
        */
        foreach (Transform trf in minList)
        {
            // trf.localScale = new Vector3(1f, 1f, 1f);
        }
        gamesPanel.transform.DOScale(new Vector2(1.4f, 1.4f), .5f).SetEase(Ease.OutBack).OnComplete(() =>
        {

        });
        //LoadingControl.instance.showTaiXiu(true);

        isShowing = true;
    }
    private void close()
    {

        RectTransform mTransform = gameObject.GetComponent<RectTransform>();
        float x = Screen.width - .5f * mTransform.rect.width;
        float y = Screen.height / 2 + .5f * mTransform.rect.height;
        Vector3 mPos = new Vector3(x, y);
        transform.position = mPos;

        gamesPanel.transform.DOScale(new Vector2(!isFlip ? .01f : -.01f, .1f), .2f).OnComplete(() =>
        {
            gamesPanel.SetActive(false);
        });
        //blackPanel.SetActive(false);
        isShowing = false;

    }
    #endregion
}
