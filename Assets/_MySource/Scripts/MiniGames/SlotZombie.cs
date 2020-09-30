using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Spine.Unity;
using Core.Server.Api;
public class SlotZombie : MonoBehaviour {
    //if scale of panel game changed need change this field
    private float gameScale = 0.75f;
    /// <summary>
    /// 0:betElement | 
    /// </summary>
    public GameObject[] slotZbGojs;

    /// <summary>
    /// 0:pot
    /// </summary>
    public Text[] texts;
    private int lastPot = 0;
    /// <summary>
    ///  0:betCurrent | 1:pot | 2:curHis | 3:prizeValue | 4:totalValue
    /// </summary>
    private int[] numList = new int[5];

    /// <summary>
    /// 0:potChange
    /// </summary>
    private IEnumerator[] threads = new IEnumerator[1];

    /// <summary>
    ///  0:A | 1:B | 2:C | 3:D | 4:E | 5:W | 6:x3 | 7:x5 | 8:x10 | 9:E | 10:betActive | 11:betUnactive
    /// </summary>
    public Sprite[] sprites;

    /// <summary>
    /// 0:isSpinning | 1:isAutoSpin | 2:isHadSpinMore | 3:isBigWin | 4:isPotBreak | 5:isRunningTextTween | 6:isSuperSpeed
    /// </summary>
    public bool[] boolLs = new bool[7];

    /// <summary>
    ///  0:gloryPot | 1:gloryBigwin | 2:spin | 3:autoSpin
    /// </summary>
    public Button[] btns;
    public GameObject[] buttonState;

    /// <summary>
    /// 0:pieceElement | 1:pieceBonusElement | 2:row1 | 3:row2 | 4:row3 | 5:row4 | 6:row5 | 13:PanelItem | 14:LineItem | 14:LineItem 1 | 14:LineItem 2 | 14:LineItem 3 | 14:LineItem 4 | 19: Canvas 
    /// </summary>
    public RectTransform[] rtfs;

    /// <summary>
    /// 0-9:linePoint
    /// </summary>
    public RectTransform[] linePoints;
    
    /// <summary>
    /// 0-1:animLightingBonus 
    /// </summary>
    public Animator[] animators;

    //[SerializeField]
    //private Vector3[] posLine;
    //private Vector3[] posLine1;
    //private Vector3[] posLine2;
    //private Vector3[] posLine3;
    //private Vector3[] posLine4;
    //List<Vector3[]> posOriginItemInLines = new List<Vector3[]>();
    [SerializeField]
    private Image prefabDraw;
    private RectTransform[] rtfItemLine;
    private RectTransform[] rtfItemLine1;
    private RectTransform[] rtfItemLine2;
    private RectTransform[] rtfItemLine3;
    private RectTransform[] rtfItemLine4;

    [SerializeField] Transform tfNewPosLinesItem;
    private Vector3[] posNewItemsLine1;
    private Vector3[] posNewItemsLine2;
    private Vector3[] posNewItemsLine3;
    private Vector3[] posNewItemsLine4;
    private Vector3[] posNewItemsLine5;

    //private List<Vector3[]> posItemsAllLine = new List<Vector3[]>();
    [SerializeField]
    private Vector3[] posItemsSelected = new Vector3[9];
    public Sprite []stateButton;

    private int[] idMaxYPosItem = new int[5] { 0, 0, 0, 0, 0 };
    private float[] timeEarlyNormalSpins = new float[] { 1f, 1.25f, 1.5f, 1.75f, 2f };
    private float timeEarlySpinHighSpeed = 0.1f;
    private float timeMidSpin = 0.3f;
    private float timeLastSpin = 0.2f;
    private float timeDelayEveryAutoSpin = 1f;
    private bool isReturnData = false;
    //duc field
    int frameRate = 0;
    //1706x960
    private string[] itemsSpins;
    private string[] lineValues;
    //
    private List<RectTransform> facunwaosit = new List<RectTransform>();
    private List<RectTransform> facunwaositRow2 = new List<RectTransform>();
    private GameObject[] columnBonus = new GameObject[9];
    private GameObject[] columnXBonus = new GameObject[6];
    private RsPiece[] rsPieceList = new RsPiece[15];
    private List<string> gloryPotBreak = new List<string>();
    private List<string> gloryBigWin = new List<string>();

    private const string GAME_CODE = GameCodeApp.gameCode4;
    private const string GET_INFO_CODE = "KEOHOLO.GET_INFO";
    private const string START_CODE = "KEOHOLO.START";
    private const string HISTORY_CODE = "KEOHOLO.HISTORY";
    private const string GLORY_BOARD_CODE = "KEOHOLO.GLORY_BOARD";


    private int pMoney = (int)CPlayer.chipBalance;


    public class RsPiece
    {
        private Image main;
        private Image overlay;
        private Image border;
        private int pieceIndex;
        private ParticleSystem fx;
        public RsPiece(Image main, Image overlay, Image border, int pieceIndex, ParticleSystem fx)
        {
            this.Main = main;
            this.Overlay = overlay;
            this.Border = border;
            this.PieceIndex = pieceIndex;
            this.Fx = fx;
        }

        public Image Main
        {
            get
            {
                return main;
            }

            set
            {
                main = value;
            }
        }

        public Image Overlay
        {
            get
            {
                return overlay;
            }

            set
            {
                overlay = value;
            }
        }

        public Image Border
        {
            get
            {
                return border;
            }

            set
            {
                border = value;
            }
        }

        public int PieceIndex
        {
            get
            {
                return pieceIndex;
            }

            set
            {
                pieceIndex = value;
            }
        }

        public ParticleSystem Fx
        {
            get
            {
                return fx;
            }

            set
            {
                fx = value;
            }
        }
    }
    private void Start()
    {
        boolLs[0] = false;
        //SetDeafaultSpin();
        Init2();
        Init();
    }
    private void Awake()
    {

    }
    private void OnEnable()
    {
        checkMoney = false;
        pMoney = (int)CPlayer.chipBalance;
        CPlayer.potchangedZombieSlot += ZombieSlotPotChange;
        CPlayer.changed += CPlayer_changed;
        CPlayer.forceStopGameEvent += OnForceStopGame;

    }
    private void OnDisable()
    {
        CPlayer.forceStopGameEvent -= OnForceStopGame;
        SetBet(100);
    }
    
  
    private void CPlayer_changed(string type)
    {
        if (type == "chip")
        {
            if (CPlayer.preChipBalance != CPlayer.chipBalance)
            {

                pMoney = (int)CPlayer.chipBalance;
            }
        }
    }

    void Init()
    {
        animators[0].gameObject.SetActive(false);
        animators[1].gameObject.SetActive(false);


        for (int i = 0; i < rtfs[22].childCount; i++)
        {
            posItemsSelected[i] = rtfs[22].GetChild(i).transform.position;
        }
        //
        frameRate = (int)(1 / Time.deltaTime);
        //
        //pos item
        int len = rtfs[14].childCount;
        //posLine = new Vector3[len];
        rtfItemLine = new RectTransform[len];

        for (int i = 0; i < len; i++)
        {
            //posLine[i] = rtfs[14].GetChild(i).transform.position;
            rtfItemLine[i] = rtfs[14].GetChild(i).transform as RectTransform;
        }
        //posLine1 = new Vector3[len];
        rtfItemLine1 = new RectTransform[len];

        for (int i = 0; i < len; i++)
        {
            //posLine1[i] = rtfs[15].GetChild(i).transform.position;
            rtfItemLine1[i] = rtfs[15].GetChild(i).transform as RectTransform;

        }
        //posLine2 = new Vector3[len];
        rtfItemLine2 = new RectTransform[len];

        for (int i = 0; i < len; i++)
        {
            //posLine2[i] = rtfs[16].GetChild(i).transform.position;
            rtfItemLine2[i] = rtfs[16].GetChild(i).transform as RectTransform;

        }
        //posLine3 = new Vector3[len];
        rtfItemLine3 = new RectTransform[len];

        for (int i = 0; i < len; i++)
        {
            //posLine3[i] = rtfs[17].GetChild(i).transform.position;
            rtfItemLine3[i] = rtfs[17].GetChild(i).transform as RectTransform;

        }
        //posLine4 = new Vector3[len];
        rtfItemLine4 = new RectTransform[len];

        for (int i = 0; i < len; i++)
        {
            //posLine4[i] = rtfs[18].GetChild(i).transform.position;
            rtfItemLine4[i] = rtfs[18].GetChild(i).transform as RectTransform;
        }

        //posOriginItemInLines.Add(posLine);
        //posOriginItemInLines.Add(posLine1);
        //posOriginItemInLines.Add(posLine2);
        //posOriginItemInLines.Add(posLine3);
        //posOriginItemInLines.Add(posLine4);

    }
    private bool isMoney = true;
    private bool checkMoney = false;
    public void D_Spin()
    {
        D_HideText();
        isMoney = true;
        if (pMoney < numList[0])
        {
            //App.showErr("Không đủ tiền cược.");
            App.showErr(App.listKeyText["WARN_NOT_ENOUGH_GOLD"]);
            boolLs[1] = !boolLs[1];
            //btns[3].GetComponent<Image>().sprite = sprites[5].Sprite;
           // btns[3].GetComponent<Image>().color = Color.white;
            isMoney = false;
            return;
        }
        if (rtfs[21].childCount > 0)
        {
            for (int i = 0; i < rtfs[21].childCount; i++)
            {
                Destroy(rtfs[21].GetChild(i).gameObject);
            }
        }

        btns[2].interactable = false;
        buttonState[0].SetActive(true);
        if (!boolLs[0])
        {
            boolLs[0] = true;
        }
        if (!boolLs[6])
        {
            isReturnData = false;
            StartCoroutine(_D_EarlySpin(1, rtfs[14], rtfItemLine, timeEarlyNormalSpins[0], TYPESPIN.BigIcon));
            StartCoroutine(_D_EarlySpin(2, rtfs[15], rtfItemLine1, timeEarlyNormalSpins[1], TYPESPIN.BigIcon));
            StartCoroutine(_D_EarlySpin(3, rtfs[16], rtfItemLine2, timeEarlyNormalSpins[2], TYPESPIN.BigIcon));
            StartCoroutine(_D_EarlySpin(4, rtfs[17], rtfItemLine3, timeEarlyNormalSpins[3], TYPESPIN.MiniIcon));
            StartCoroutine(_D_EarlySpin(5, rtfs[18], rtfItemLine4, timeEarlyNormalSpins[4], TYPESPIN.MiniIcon));
        }
        else
        {
            isReturnData = false;
            StartCoroutine(_D_EarlySpin(1, rtfs[14], rtfItemLine, timeEarlySpinHighSpeed, TYPESPIN.BigIcon));
            StartCoroutine(_D_EarlySpin(2, rtfs[15], rtfItemLine1, timeEarlySpinHighSpeed, TYPESPIN.BigIcon));
            StartCoroutine(_D_EarlySpin(3, rtfs[16], rtfItemLine2, timeEarlySpinHighSpeed, TYPESPIN.BigIcon));
            StartCoroutine(_D_EarlySpin(4, rtfs[17], rtfItemLine3, timeEarlySpinHighSpeed, TYPESPIN.MiniIcon));
            StartCoroutine(_D_EarlySpin(5, rtfs[18], rtfItemLine4, timeEarlySpinHighSpeed, TYPESPIN.MiniIcon));
        }

    }
    
    IEnumerator _D_EarlySpin(int numLine,RectTransform line, RectTransform[] items,float timeSpin,TYPESPIN typeSpin, float velocity = 3000f)
    {
       
        D_HideText();
        //set all to origin state;
        D_DeactiveItem(items);
        D_SetLightingBonus(numLine,false);
        //Origin Position of LineItem , items
        Vector3 originPosLine = line.transform.position;
        Vector3[] originPosLineItem = new Vector3[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            originPosLineItem[i] = items[i].position;
        }
        //if (posItemsAllLine.Count < 5)
        //{
        //    posItemsAllLine.Add(originPosLineItem);
        //}
        //
        float theDeepestPosY;
        float spaceBetweenTwoItem;
        if (typeSpin == TYPESPIN.BigIcon)
        {
            theDeepestPosY = this.transform.GetChild(0).position.y - 860f * gameScale;
            spaceBetweenTwoItem = 5f * gameScale;
        }
        else
        {
            theDeepestPosY = this.transform.GetChild(0).position.y - 600f * gameScale;
            spaceBetweenTwoItem = 15f * gameScale;
        }
        float itemHeight = items[0].sizeDelta.y;
        float distBetweenTwoItem = (itemHeight + spaceBetweenTwoItem) * rtfs[19].localScale.y;
        Vector3 distanceBetweenTwoItem = Vector3.up *((itemHeight + spaceBetweenTwoItem) * rtfs[19].localScale.y);
        int idTheHighestItem = items.Length - 1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].position.y > items[idTheHighestItem].position.y)
            {
                idTheHighestItem = i;
            }
        }
        int totalFrameSpin = (int)(timeSpin * frameRate);
        for (int i = 0; i < totalFrameSpin; i++)
        {
            if (typeSpin == TYPESPIN.BigIcon)
            {
                theDeepestPosY = this.transform.GetChild(0).position.y - 860f * gameScale;
            }
            else
            {
                theDeepestPosY = this.transform.GetChild(0).position.y - 600f * gameScale;
            }
            line.Translate(Vector3.down * velocity * Time.deltaTime);
            for (int j = 0; j < items.Length; j++)
            {
                if (items[j].position.y < theDeepestPosY)
                {
                    items[j].position = new Vector3(items[j].position.x , items[idTheHighestItem].position.y-10f) + distanceBetweenTwoItem;
                    idTheHighestItem = j;
                }
            }
            yield return new WaitForEndOfFrame();
        }


       

        StartCoroutine(_D_MidSpin(numLine,line, items, timeMidSpin, typeSpin, velocity, theDeepestPosY, distanceBetweenTwoItem, originPosLine, originPosLineItem, distBetweenTwoItem));

    }

    IEnumerator _D_MidSpin(int numLine,RectTransform line, RectTransform[] items, float timeSpin, TYPESPIN typeSpin, float velocity, float theDeepestPosY, Vector3 distanceBetweenTwoItem, Vector3 originPosLine, Vector3[] originPosLineItem, float distBetweenTwoItem)
    {
       
        if (numLine == 1)
        {
            try
            {
                var req_SpinZombieSlot = new OutBounMessage(START_CODE);
                req_SpinZombieSlot.addHead();
                req_SpinZombieSlot.writeInt(numList[0]);
                req_SpinZombieSlot.writeString(GAME_CODE);
                App.ws.send(req_SpinZombieSlot.getReq(), delegate (InBoundMessage res_ZombieSlot)
                {
                    isReturnData = true;
                    boolLs[0] = true;
                    int count = res_ZombieSlot.readInt();
                    itemsSpins = new string[count];
                    Debug.ClearDeveloperConsole();
                    for (int i = 0; i < count; i++)
                    {
                        string item = res_ZombieSlot.readString();
                        itemsSpins[i] = item;
                        //App.trace("item " + i + " " + item,"yellow");
                      //  Debug.Log("item " + i + " " + item);
                    }
                    int prizeValue = res_ZombieSlot.readInt();
                    numList[3] = prizeValue;
                    int totalPrize = res_ZombieSlot.readInt();
                    numList[4] = totalPrize;
                    int linePrize = res_ZombieSlot.readInt();
                    if (linePrize > 0 && !itemsSpins[10].Contains("O"))
                        boolLs[2] = true;
                    else
                        boolLs[2] = false;
                    lineValues = new string[linePrize];
                    //App.trace("Number Line Prize "+linePrize,"yellow");
                    for (int j = 0; j < linePrize; j++)
                    {
                        string lineValue = res_ZombieSlot.readString();
                        lineValues[j] = lineValue;
                        //App.trace("Line Value " + j + " " + lineValue, "yellow");
                       //Debug.Log("Line Value " + j + " " + lineValue);
                    }
                    bool isBigWin = res_ZombieSlot.readByte() == 1;
                    if (isBigWin)
                    {
                        SoundManager.instance.PlayUISound(SoundFX.BIG_WIN + "_" + UnityEngine.Random.Range(1,4));
                    }

                    boolLs[3] = isBigWin;
                    bool isPotBreak = res_ZombieSlot.readByte() == 1;
                    boolLs[4] = isPotBreak;

                    //string txt = "";
                    //for (int i = 0; i < lineValues.Length; i++)
                    //{
                    //    Debug.Log(itemsSpins[i]);
                    //}
                    //for (int i = 0; i < lineValues.Length; i++)
                    //{
                    //    Debug.Log(txt + "----:" +lineValues[i]);
                    //}

                });
            }
            catch (Exception)
            {

            }

        }

        //spin mid
        //---
        //check highest item
        int idTheHighestItem = items.Length - 1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].position.y > items[idTheHighestItem].position.y)
            {
                idTheHighestItem = i;
            }
        }

        //calculate spin frame then spin
        int totalFrameSpin = (int)(timeSpin * frameRate);
        for (int i = 0; i < totalFrameSpin; i++)
        {
            if (typeSpin == TYPESPIN.BigIcon)
            {
                theDeepestPosY = this.transform.GetChild(0).position.y - 860f * gameScale;
            }
            else
            {
                theDeepestPosY = this.transform.GetChild(0).position.y - 600f * gameScale;
            }
            //Plus more Frame wait Server 
            if (!isReturnData && i > totalFrameSpin - 10)
            {
                i--;
            }
            line.Translate(Vector3.down * velocity * Time.deltaTime);
            for (int j = 0; j < items.Length; j++)
            {
                if (items[j].position.y < theDeepestPosY)
                {
                    items[j].position = new Vector3(items[j].position.x ,items[idTheHighestItem].position.y-10f) + distanceBetweenTwoItem;
                    idTheHighestItem = j;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(_D_LastSpin(numLine, line, items, timeLastSpin, typeSpin, velocity, theDeepestPosY, distanceBetweenTwoItem, originPosLine, originPosLineItem, distBetweenTwoItem, itemsSpins));
    }
    IEnumerator _D_LastSpin(int numLine,RectTransform line, RectTransform[] items, float timeSpin, TYPESPIN typeSpin, float velocity, float theDeepestPosY, Vector3 distanceBetweenTwoItem, Vector3 originPosLine, Vector3[] originPosLineItem, float distBetweenTwoItem, string[] selectedItems)
    {
        int[] slotsSelected = new int[3] { 2, 3, 4 };
        string[] thisLineItemSelected = new string[3];
        if (numLine == 1)
        {
            thisLineItemSelected[0] = selectedItems[0];
            thisLineItemSelected[1] = selectedItems[1];
            thisLineItemSelected[2] = selectedItems[2];
        }
        if (numLine == 2)
        {
            thisLineItemSelected[0] = selectedItems[3];
            thisLineItemSelected[1] = selectedItems[4];
            thisLineItemSelected[2] = selectedItems[5];
        }
        if (numLine == 3)
        {
            thisLineItemSelected[0] = selectedItems[6];
            thisLineItemSelected[1] = selectedItems[7];
            thisLineItemSelected[2] = selectedItems[8];
        }
        if (numLine == 4)
        {
            thisLineItemSelected[0] = selectedItems[9];
            thisLineItemSelected[1] = selectedItems[10];
            thisLineItemSelected[2] = selectedItems[11];
        }
        if (numLine == 5)
        {
            thisLineItemSelected[0] = selectedItems[12];
            thisLineItemSelected[1] = selectedItems[13];
            thisLineItemSelected[2] = selectedItems[14];
        }

        velocity = 0;
        //return Item, ItemPanel to origin Postion
        //originPosLineItem = new Vector3[items.Length];
        //for (int i = 0; i < items.Length; i++)
        //{
        //    originPosLineItem[i] = items[i].position;
        //}


        //for (int i = 0; i < line.childCount; i++)
        //{
        //    items[i].position = GetNewPos(numLine)[i].position;
        //}
        for (int i = 0; i < items.Length; i++)
        {
            //items[i].position = originPosLineItem[i] + distanceBetweenTwoItem;
            items[i].SetParent(rtfs[20]);
        }
        line.position = originPosLine;
        for (int i = 0; i < items.Length; i++)
        {
            items[i].SetParent(line);
        }
        //change Image on Screen
        for (int i = 0; i < items.Length; i++)
        {
            for (int j = 0; j < slotsSelected.Length; j++)
            {
                if (i == slotsSelected[j])
                {
                    if (items[i].childCount > 0)
                    {
                        for (int k = 0; k < items[i].childCount; k++)
                        {
                           // Debug.Log(items[i].GetChild(k).gameObject.name);
                            if (items[i].GetChild(k).gameObject.name.Equals(thisLineItemSelected[j]))
                            {
                                items[i].GetChild(k).gameObject.SetActive(true);

                            }
                            else
                            {
                                items[i].GetChild(k).gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        //spin slow
        //slow slice 1 item
        float slowDistance = distBetweenTwoItem;
        //velocity = slowDistance / timeSpin;
        //int totalFrames = (int)(timeSpin * frameRate);

        //for (int i = 0; i < totalFrames; i++)
        //{
        //    line.Translate(Vector3.down * velocity * Time.deltaTime);
        //    yield return new WaitForEndOfFrame();
        //}

        //khop position
        for (int i = 0; i < line.childCount; i++)
        {
            Debug.Log("- Pos : " + line.GetChild(i).gameObject.GetComponent<RectTransform>().position + "/n- Get New Pos : " + GetNewPos(numLine)[i + 1].position);
            if(typeSpin == TYPESPIN.BigIcon)
            {
                line.GetChild(i).gameObject.GetComponent<RectTransform>().position = GetNewPos(numLine)[i + 1].position;
            }
            else
            {
                line.GetChild(i).gameObject.GetComponent<RectTransform>().position = new Vector3(line.GetChild(i).gameObject.GetComponent<RectTransform>().position.x, GetNewPos(numLine)[i + 1].position.y - 25f);
            }

        }
        //draw Line
        if (numLine==3)
        {
            for (int i = 0; i < lineValues.Length; i++)
            {
                D_Draw(lineValues[i], originPosLineItem);
            }
        }
        //active animation
        //if (numLine == 3)
        //{
        //    for (int i = 0; i < lineValues.Length; i++)
        //    {
        //        D_ActiveItem(1,/* line,*/ rtfItemLine, lineValues[i]);
        //        D_ActiveItem(2,/* line,*/ rtfItemLine1, lineValues[i]);
        //        D_ActiveItem(3,/* line,*/ rtfItemLine2, lineValues[i]);

        //    }
        //}
        //else if (numLine > 3)
        //{
        //    for (int i = 0; i < lineValues.Length; i++)
        //    {
        //        D_ActiveItem(numLine,/* line,*/ items, lineValues[i]);
        //    }
        //}


        //wait player see result screen
        if(numLine == 5)
        {
            D_ShowText();
        }
        //
        if (boolLs[3] || boolLs[4])
        {
            yield return new WaitForSeconds(5f);
        }
        else
        {
            yield return new WaitForSeconds(timeDelayEveryAutoSpin);
        }
        //
        if (numLine == 5)
        {
            boolLs[0] = false;
            btns[2].interactable = true;
            buttonState[0].SetActive(false);
            if (boolLs[1])
            {
                D_Spin();
            }
        }
    }
    private RectTransform[] GetNewPos(int numLine)
    {
        RectTransform[] newTfs = new RectTransform[0];
        if (tfNewPosLinesItem != null)
        {
            newTfs = tfNewPosLinesItem.GetChild(numLine-1).GetComponentsInChildren<RectTransform>();
        }
        return newTfs;
    }
    enum TYPESPIN
    {
        BigIcon,
        MiniIcon
    }

    
    private void Init2()
    {
        transform.SetSiblingIndex(22);
        var req_ZombiePlant = new OutBounMessage(GET_INFO_CODE);
        req_ZombiePlant.addHead();
        App.ws.send(req_ZombiePlant.getReq(), delegate (InBoundMessage res_ZombiePlant)
        {
            int count = res_ZombiePlant.readByte();
            Debug.Log("Count "+count);
            for(int i = 0; i < count; i++)
            {
                int bet = res_ZombiePlant.readInt();
                Debug.Log(bet);
                //App.trace("Bet "+i+" "+bet,"yellow");
                GameObject betValue = Instantiate(slotZbGojs[0],slotZbGojs[0].transform.parent,false);
                betValue.GetComponentInChildren<Text>().text = bet % 1000 == 0 ? (bet / 1000).ToString() + "K" : App.formatMoneyK(bet);
                betValue.GetComponent<Button>().onClick.AddListener(()=>{
                    
                    if (boolLs[0])
                    {
                        return;
                    }
                    SetBet(bet);
                });
                betValue.SetActive(true);
                if (i == 0)
                    SetBet(bet);
            }
        });
    }

    public void SetBet(int betValue)
    {
        numList[0] = betValue;
        //StartCoroutine(_setBet(betValue));
        _SetBet(betValue);
    }

    private void _SetBet(int betValue)
    {
        Debug.Log("Bet "+betValue);
        Transform pnlButtonsSelectBet = slotZbGojs[0].transform.parent;
        for (int i = 0; i < pnlButtonsSelectBet.childCount; i++)
        {
          //  pnlButtonsSelectBet.GetChild(i).GetComponent<Image>().color = Color.grey;
            pnlButtonsSelectBet.GetChild(i).GetComponent<Image>().overrideSprite = stateButton[0];
        }
        boolLs[1] = false;
        //btns[3].GetComponent<Image>().sprite = sprites[4].Sprite;
        btns[3].GetComponent<Image>().color = Color.white;
        switch (betValue)
        {
            case 100:
                //slotZbGojs[0].transform.parent.GetChild(1).GetComponent<Image>().sprite = sprites[10];
               // pnlButtonsSelectBet.GetChild(1).GetComponent<Image>().color = Color.white;
                pnlButtonsSelectBet.GetChild(1).GetComponent<Image>().overrideSprite = stateButton[1];
                break;
            case 1000:
                //slotZbGojs[0].transform.parent.GetChild(2).GetComponent<Image>().sprite = sprites[10];
                //pnlButtonsSelectBet.GetChild(2).GetComponent<Image>().color = Color.white;
               pnlButtonsSelectBet.GetChild(2).GetComponent<Image>().overrideSprite = stateButton[1];
                break;
            case 10000:
                //slotZbGojs[0].transform.parent.GetChild(3).GetComponent<Image>().sprite = sprites[10];
              //  pnlButtonsSelectBet.GetChild(3).GetComponent<Image>().color = Color.white;
                pnlButtonsSelectBet.GetChild(3).GetComponent<Image>().overrideSprite = stateButton[1];
                break;
        }
    }
    public void SetSuperSpeed()
    {
        if (boolLs[0])
            return;
        if (boolLs[6])
        {
            slotZbGojs[11].SetActive(false);
        }
        else
        {
            slotZbGojs[11].SetActive(true);
        }
        boolLs[6] = !boolLs[6];
    }
    public void SetAutoSpin()
    {
        if (boolLs[1])
        {
            boolLs[1] = !boolLs[1];
            //btns[3].GetComponent<Image>().sprite = sprites[5].Sprite;
            btns[3].GetComponent<Image>().color = Color.white;
        }
        else
        {
            boolLs[1] = !boolLs[1];
           // btns[3].GetComponent<Image>().sprite = sprites[4].Sprite;
           btns[3].GetComponent<Image>().color = Color.yellow;
            if (!boolLs[0])
                D_Spin();
        }
    }

   
    private void D_ActiveItem(int numLine,/* RectTransform line,*/ RectTransform[] items, string lineContentToDraw)
    {

        int[] slotsSelected = new int[3] { 2, 3, 4 };
        string[] strLineNumber = lineContentToDraw.Split('-');
        int[] lineNumber = new int[strLineNumber.Length];
        for (int i = 0; i < strLineNumber.Length; i++)
        {
            lineNumber[i] = int.Parse(strLineNumber[i]);
        }
        //
        //itemsSpins
        //switch (numLine)
        //{
        //    case 1:
        //        for (int i = 0; i < items[slotsSelected[lineNumber[0] - 1]].childCount; i++)
        //        {
        //            if(items[slotsSelected[lineNumber[0] - 1]].GetChild(i).gameObject.activeInHierarchy)
        //            {
        //               items[slotsSelected[lineNumber[0] - 1]].GetChild(i).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);
        //                //items[slotsSelected[lineNumber[0] - 1]].GetChild(i).GetComponent<Image>().enabled=true;
        //            }
        //        }
        //        break;
        //    case 2:
        //        for (int i = 0; i < items[slotsSelected[lineNumber[1] - 4]].childCount; i++)
        //        {
        //            if (items[slotsSelected[lineNumber[1] - 4]].GetChild(i).gameObject.activeInHierarchy)
        //            {
        //                  items[slotsSelected[lineNumber[1] - 4]].GetChild(i).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);
        //               // items[slotsSelected[lineNumber[1] - 4]].GetChild(i).GetComponent<Image>().enabled = true;
        //            }
        //        }
        //        break;
        //    case 3:
        //        for (int i = 0; i < items[slotsSelected[lineNumber[2] - 7]].childCount; i++)
        //        {
        //            if (items[slotsSelected[lineNumber[2] - 7]].GetChild(i).gameObject.activeInHierarchy)
        //            {
        //                items[slotsSelected[lineNumber[2] - 7]].GetChild(i).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);
        //              //  items[slotsSelected[lineNumber[2] - 7]].GetChild(i).GetComponent<Image>().enabled = true;
        //            }
        //        }
        //        break;
        //    case 4:
        //      /*  if (itemsSpins[10].Equals("O") && itemsSpins[13] != "O")
        //        {
        //            D_SetLightingBonus(numLine, true);
        //            for (int i = 0; i < items[3].childCount; i++)
        //            {
        //                if (items[3].GetChild(i).gameObject.activeInHierarchy)
        //                {
        //                    items[3].GetChild(i).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);
        //                }
        //            }
        //        }*/
        //        break;
        //    case 5:
        //     /*   if (itemsSpins[10] != ("O") && itemsSpins[13].Equals("O"))
        //        {
        //            D_SetLightingBonus(numLine,true);
        //            for (int i = 0; i < items[3].childCount; i++)
        //            {
        //                if (items[3].GetChild(i).gameObject.activeInHierarchy)
        //                {
        //                    //update UI bug
        //                    try
        //                    {
        //                        items[3].GetChild(i).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);
        //                    }
        //                    catch
        //                    {

        //                    }
        //                }
        //            }
        //        }*/
        //        break;
        //}
    }
    private void D_DeactiveItem(RectTransform[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            for (int j = 0; j < items[i].childCount; j++)
            {
                if (items[i].GetChild(j).gameObject.activeInHierarchy)
                {
                    try
                    {
                         items[i].GetChild(j).GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "unactive", true);
                       // items[i].GetChild(j).GetComponent<Image>().enabled = true;
                    }
                    catch (Exception) { }
                }
            }
        }
    }
    private void D_Draw(string lineContent, Vector3[] originPosLineItems)
    {
        for (int i = 0; i < rtfs[22].childCount; i++)
        {
            posItemsSelected[i] = rtfs[22].GetChild(i).transform.position;
        }
        //posItemsSelected
        //int[] slotsSelected = new int[3] { 2, 3, 4 };
        string[] strLineNumber = lineContent.Split('-');
        int[] lineNumber = new int[strLineNumber.Length];
        //rtfItemLine[slotsSelected[0]]
        //rtfItemLine[slotsSelected[1]]
        //rtfItemLine[slotsSelected[2]]
        for (int i = 0; i < strLineNumber.Length; i++)
        {
            lineNumber[i] = int.Parse(strLineNumber[i]);
        }
        //Vector3[] posItemsLine = posItemsAllLine[0];
        //Vector3[] posItemsLine1 = posItemsAllLine[1];
        //Vector3[] posItemsLine2 = posItemsAllLine[2];
        System.Random rd = new System.Random();
        id = rd.Next(0, 8);
        while (id == idColor)
            id = rd.Next(0, 8);
        idColor = id;

        D_DrawLine(posItemsSelected[lineNumber[0] - 1], posItemsSelected[lineNumber[1] - 1]);
        D_DrawLine(posItemsSelected[lineNumber[1] - 1], posItemsSelected[lineNumber[2] - 1]);
    }

    private Color32 RamdomColor(int id = 0)
    {

        switch (id)
        {
            case 0: return new Color32(255, 30, 30, 255);
            case 1: return new Color32(30, 78, 255, 255);
            case 2: return new Color32(0, 246, 2, 255);
            case 3: return new Color32(241, 255, 0, 255);
            case 4: return new Color32(241, 0, 255, 255);
            case 5: return new Color32(0, 255, 255, 255);
            case 6: return new Color32(0, 0, 255, 255);
            case 7: return new Color32(255, 255, 255, 255);
            default:
                return new Color32(255, 30, 30, 255);
        }
    }
    private int idColor=0,id=0;
    private void D_DrawLine(Vector3 begin, Vector3 target)
    {
        //Debug.Log("---Draw 1 Line---" + begin.gameObject.name + " to " + target.gameObject.name);
    
        float widthLine = 2f;
        GameObject goLine = Instantiate(prefabDraw.gameObject);
        var newRtf = goLine.transform as RectTransform;
       
        goLine.GetComponent<Image>().color = RamdomColor(id);
        newRtf.gameObject.SetActive(true);
        newRtf.SetParent(rtfs[21]);
        newRtf.position = begin;
        //newRtf.pivot = begin.pivot;
        float distance = Vector3.Distance(begin, target);
        float distHeight = Vector3.Distance(new Vector3(0, begin.y), new Vector3(0, target.y));
        if (begin.y > target.y)
        {
            distHeight = -distHeight;
        }
        newRtf.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance);
        newRtf.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, widthLine);
        float valueSin = distHeight / distance;
        //Debug.Log(valueSin);
        // 1 radian = 180/ Pi (angle == degree)
        float angle = Mathf.Asin(valueSin) * (180 / Mathf.PI);
        //Debug.Log(angle);
        newRtf.rotation = Quaternion.Euler(0, 0, angle);
    }
   
    private IEnumerator _showTextValue(GameObject textObject,Vector3 sourceScale,Vector3 dirScale,int sourePrize,int dirPrize,float time,bool potBigWin=false)
    {
        boolLs[5] = true;
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            //if (potBigWin)
            //    textObject.transform.parent.GetComponent<RectTransform>().localScale = Vector3.Lerp(sourceScale, dirScale, i);
            //else
            //    textObject.GetComponent<RectTransform>().localScale = Vector3.Lerp(sourceScale,dirScale,i);
            textObject.GetComponent<Text>().text = App.formatMoney(((int)Math.Ceiling(Mathf.Lerp(sourePrize,dirPrize,i))).ToString());
            yield return null;
        }
    }

    
    private void D_SetLightingBonus(int numLine,bool isActive)
    {
        if (numLine == 4)
        {
            animators[0].gameObject.SetActive(isActive);
            animators[0].SetBool("lighting", isActive);
        }
        else
        {
            animators[1].gameObject.SetActive(isActive);
            animators[1].SetBool("lighting", isActive);
        }
    }
    private void D_ShowText()
    {
        if (numList[4] > numList[3] && !boolLs[3] && !boolLs[4] && !boolLs[6])
        {
            StartCoroutine(_showTextValue(slotZbGojs[7].GetComponentInChildren<Text>().gameObject, Vector3.one, new Vector3(1.5f, 1.5f, 1.5f), numList[3], numList[4], .5f));
        }
        if (boolLs[4] /*&& !boolLs[5]*/)
        {
            slotZbGojs[9].SetActive(true);
            StartCoroutine(_showTextValue(slotZbGojs[9].GetComponentInChildren<Text>().gameObject, new Vector3(.8f, .8f, .8f), Vector3.one, 0, numList[4], .5f, true));
            if (boolLs[1])
            {
                SetAutoSpin();
            }
        }
        else
        if (boolLs[3] /*&& !boolLs[5]*/)
        {
            //if (boolLs[1])
            //{
            //    SetAutoSpin();
            //}
            slotZbGojs[10].SetActive(true);
            StartCoroutine(_showTextValue(slotZbGojs[10].GetComponentInChildren<Text>().gameObject, new Vector3(.8f, .8f, .8f), Vector3.one, 0, numList[4], .5f, true));
        }
        if (numList[4] > 0)
        {
            slotZbGojs[7].SetActive(true);
            slotZbGojs[7].GetComponentInChildren<Text>().text = App.formatMoney(numList[4].ToString());
            slotZbGojs[7].GetComponentInChildren<Text>().gameObject.GetComponentInChildren<RectTransform>().localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }
    private void D_HideText()
    {
        slotZbGojs[7].SetActive(false);
        slotZbGojs[9].SetActive(false);
        slotZbGojs[10].SetActive(false);
    }
   

    private IEnumerator TweenNum(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        float i = 0.0f;
        float rate = 2.0f / tweenTime;
        txt.transform.DOScale(scaleNum, tweenTime / 2);
        while (i < tweenTime)
        {
            i += Time.deltaTime * rate;
            float a = Mathf.Lerp(fromNum, toNum, i);

            txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
            if (a == toNum)
            {
                i = tweenTime;
            }
            yield return null;
        }
        txt.transform.localScale = Vector2.one;
        yield return new WaitForSeconds(.05f);
    }

    public void ZombieHistory()
    {
        slotZbGojs[1].SetActive(true);
        foreach (Transform rtf in slotZbGojs[2].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != slotZbGojs[2].name)
            {
                Destroy(rtf.gameObject);
            }
        }
        OutBounMessage req_HIS = new OutBounMessage("MATCH.HISTORY");
        req_HIS.addHead();
        req_HIS.writeString(GAME_CODE);         //game name
        numList[2] = 1;
        req_HIS.writeByte(numList[2]);                       //page index
        req_HIS.writeString("");                    //from date
        req_HIS.writeString("");                    //to date
        App.ws.send(req_HIS.getReq(), delegate (InBoundMessage res_HIS)
        {
            int count = res_HIS.readByte();
            //App.trace("Count = " + count);
            for (int i = 0; i < count; i++)
            {
                long index = res_HIS.readLong();
                string time = res_HIS.readString();
                string game = res_HIS.readString();
                string bet = res_HIS.readString();
                string change = res_HIS.readString();
                string balance = res_HIS.readString();
                GameObject hisClone = Instantiate(slotZbGojs[2],slotZbGojs[2].transform.parent,false);
                Text[] txtArr = hisClone.GetComponentsInChildren<Text>();
                txtArr[0].text = index.ToString();
                txtArr[1].text = time;
                txtArr[2].text = App.FormatMoney(bet);
                txtArr[3].text = App.FormatMoney(change);
                txtArr[4].text = App.FormatMoney(balance);
                hisClone.SetActive(true);
                //App.trace("index " + index + " || time " + time + " || game " + game + " || bet " + bet + " || change " + change + " || blance " + balance, "yellow");
            }
        });
    }
    public void CloseZombieHistory()
    {
        slotZbGojs[1].SetActive(false);
    }

    public void ZombieGlory()
    {
        slotZbGojs[3].SetActive(true);
        foreach (Transform rtf in slotZbGojs[4].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != slotZbGojs[4].name)
            {
                Destroy(rtf.gameObject);
            }
        }
        gloryBigWin.Clear();
        gloryPotBreak.Clear();
        OutBounMessage req_Glory = new OutBounMessage(GLORY_BOARD_CODE);
        req_Glory.addHead();
        App.ws.send(req_Glory.getReq(), delegate (InBoundMessage resGlory) {
            int count = resGlory.readByte();
            for(int i = 0; i < count; i++)
            {
                string content = resGlory.readString();
                var st = content.Split('|');
                //App.trace("Glory "+i+" : "+ content,"yellow");
                if (st.Length > 0)
                {
                    if (st[4].Equals("true"))
                        gloryPotBreak.Add(content);
                    else
                        gloryBigWin.Add(content);
                }
            }

            ChangeTypeGlory("pot");
        });
        
    }

    public void ChangeTypeGlory(string type)
    {
        foreach (Transform rtf in slotZbGojs[4].transform.parent)       //Delete exits element before
        {
            if (rtf.gameObject.name != slotZbGojs[4].name)
            {
                Destroy(rtf.gameObject);
            }
        }
        //slotZbGojs[4].transform.parent.gameObject.SetActive(false);
        switch (type)
        {
            case "pot":
                btns[0].GetComponentInChildren<Text>().color = new Color32(255,237,0,255);
                btns[1].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
                for(int i = 0; i < gloryPotBreak.Count; i++)
                {
                    var st = gloryPotBreak[i].Split('|');
                    GameObject gloryClone = Instantiate(slotZbGojs[4],slotZbGojs[4].transform.parent,false);
                    //  gloryClone.GetComponent<Text>().text = gloryPotBreak[i];

                    Text[] txtArr = gloryClone.GetComponentsInChildren<Text>();
                //    Debug.Log("Pot " + st.Length + txtArr.Length);
                    if (st.Length > 0)
                    {
                        txtArr[0].text = st[0];
                        txtArr[1].text = st[1];
                        txtArr[2].text = st[2];
                        txtArr[3].text = st[3];
                    }
                    gloryClone.SetActive(true);
              
                    //gloryClone.GetComponent<ContentSizeFitter>().enabled = false;
                    //slotZbGojs[4].transform.parent.gameObject.SetActive(true);
                }
                break;
            case "bigwin":
                btns[1].GetComponentInChildren<Text>().color = new Color32(255, 237, 0, 255);
                btns[0].GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
                for (int i = 0; i < gloryBigWin.Count; i++)
                {
                    var st = gloryBigWin[i].Split('|');
                    GameObject gloryClone = Instantiate(slotZbGojs[4], slotZbGojs[4].transform.parent, false);
                    Text[] txtArr = gloryClone.GetComponentsInChildren<Text>();
                   // Debug.Log("Pot 1 " + st.Length);
                    if (st.Length > 0)
                    {
                        txtArr[0].text = st[0];
                        txtArr[1].text = st[1];
                        txtArr[2].text = st[2];
                        txtArr[3].text = st[3];
                    }
                    gloryClone.SetActive(true);
                    //gloryClone.GetComponent<ContentSizeFitter>().enabled = false;
                    //slotZbGojs[4].transform.parent.gameObject.SetActive(true);
                }
                break;
        }
    }

    public void CloseZombieGlory()
    {
        slotZbGojs[3].SetActive(false);
    }

    public void OpenGuideZombie(bool open)
    {
        if (open)
        {
            slotZbGojs[8].SetActive(true);
            return;
        }
        slotZbGojs[8].SetActive(false);
    }
    public void ZombieSlotPotChange()
    {
        InBoundMessage res = CPlayer.res_potMiniGameSlotZombie;
        int count = res.readByte();

        for (int i = 0; i < count; i++)
        {
            string gameId = res.readString();
            if (gameId == GAME_CODE)
            {
                int count0 = res.readByte();
                for (int j = 0; j < count0; j++)
                {
                    int bet = res.readInt();
                    int currPot = res.readInt();

                    if (bet == numList[0])
                    {
                        MoneyManager.instance.OnPotChange( texts[0], lastPot, currPot);
                        lastPot = currPot;
                    }
                }
                break;
            }
            else
            {
                int count1 = res.readByte();
                for (int j = 0; j < count1; j++)
                {
                    int bet = res.readInt();
                    int value = res.readInt();
                }
            }
        }
    }
    private void OnForceStopGame(string gameCode, int gameState)
    {

        if (gameCode == GAME_CODE)
        {
            checkMoney = true;
        }

    }
    public void Close()
    {
        if (isMoney&&!checkMoney)
        {
            if (boolLs[0] || boolLs[1])
                return;
        }
        
        StopAllCoroutines();
        CPlayer.potchangedZombieSlot -= ZombieSlotPotChange;
        CPlayer.changed -= CPlayer_changed;
        this.gameObject.SetActive(false);
    }
}
