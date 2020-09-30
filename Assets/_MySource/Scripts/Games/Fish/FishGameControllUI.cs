using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using Core.Server.Api;

[System.Serializable]
public class CallBallGame : UnityEvent<int, int, bool>
{
}
namespace Slot.Games.Fish
{

    public class FishGameControllUI : MonoBehaviour
    {
        public CallBallGame callBallGame;
        public UnityEvent EndMiniGameEvent;
        public Text txtBalance;
        public Text txtWin;
        [Header("FREESPIN")]
        public Text txtShowFreeSpin;
        private int totalFreeSpin = 0;
        [Header("AUTOPLAY")]
        public Sprite[] sprtAutoPlay;
        public Button[] buttonHideSpin;
        public Button btnAutoSpin;
        [Header("RECHARGE")]
        public RectTransform[] rtfsMove;
        [Header("WIN")]
        public Text txtElementWin;
        public Text txtWon;
        [Header("BIGWIN")]
        public RectTransform rtfBigWin;
        public Text txtBigWIn;
        [Header("POTBREAK")]
        public RectTransform rtfPotBreak;
        public Text txtPotBreak;
        private int totalBetValue = 0;
        private bool isAutoSpin;
        [Header("LINE")]
        public Color[] lineColors;
        public RectTransform rtfLineElementClone;
        public RectTransform[] rtfPointLineDraw;
        public GameObject effectLine;
        
        [Header("REF")]
        public static FishGameControllUI instance;
        public FishGameController fishGameController;
        public SpinController spinController;
        public FishGameControlBet fishGameControlBet;
        public FishGameControllLine fishGameControllLine;

        /// <summary>
        /// private variable
        /// </summary>
        private IEnumerator tweenNum;
        private IEnumerator tweenNum2;
        private IEnumerator tweenNum3;
        private int wonValue = 0;
        private bool isBigWin;
        private bool isPotBreak;
        public int numberBallGame;
        private bool isTrial;
        private int[] rsLineIdsList;
        private bool currentAutoSpin;
        private bool enoughMoney = false;
        /// <summary>
        /// private prize line
        /// </summary>
        private List<int[]> wonLineIdList = new List<int[]>();
        private List<List<GameObject>> drawnLineTmp = new List<List<GameObject>>();
        private Dictionary<int, int[]> wonLineData = new Dictionary<int, int[]>();
        private List<List<GameObject>> needToDelGojList = new List<List<GameObject>>();    //save gojs whic need to del
        private Material materialSkeletonSpine;
        private void Awake()
        {
            fishGameControlBet.gameObject.SetActive(true);
         //  materialSkeletonSpine = Resources.Load("Material/SkeletonGraphicDefault.mat", typeof(Material)) as Material;
           // buttonHideSpin[3].GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
            if (instance != null)
                Destroy(gameObject);
            else
            {
                instance = this;
            }
        }
        
        private void Start()
        {
            LoadingControl.instance.loadingGojList[30].SetActive(false);
            SoundManager.instance.PlayBackgroundSound(SoundFX.LOBBY_BG_MUSIC_3);
            wonLineIdList.Clear();
            wonLineIdList.Add(new int[] { 17,2, 5, 8, 11, 14 });
            wonLineIdList.Add(new int[] { 18,1, 4, 7, 10, 13 });
            wonLineIdList.Add(new int[] { 19,3, 6, 9, 12, 15 });
            wonLineIdList.Add(new int[] { 20,2, 5, 7, 11, 14 });
            wonLineIdList.Add(new int[] { 21,2, 5, 9, 11, 14 });

            wonLineIdList.Add(new int[] { 22,1, 4, 8, 10, 13 });
            wonLineIdList.Add(new int[] { 23,3, 6, 8, 12, 15 });
            wonLineIdList.Add(new int[] { 24,1, 6, 7, 12, 13 });
            wonLineIdList.Add(new int[] { 25,3, 4, 9, 10, 15 });
            wonLineIdList.Add(new int[] { 26,2, 4, 9, 10, 14 });

            wonLineIdList.Add(new int[] { 27, 15, 11, 7, 5, 3 });
            wonLineIdList.Add(new int[] { 28, 13, 11, 9, 5, 1 });           
            wonLineIdList.Add(new int[] { 29, 14, 10, 8, 6, 2 });            
            wonLineIdList.Add(new int[] { 30, 14, 12, 8, 4, 2 });            
            wonLineIdList.Add(new int[] { 31, 15, 11, 8, 5, 3 });
            
            wonLineIdList.Add(new int[] { 32, 13, 11, 8, 5, 1 });           
            wonLineIdList.Add(new int[] { 33, 14, 12, 9, 6, 2 });           
            wonLineIdList.Add(new int[] { 34, 14, 10, 7, 4, 2 });           
            wonLineIdList.Add(new int[] { 35, 13, 10, 8, 6, 3 });            
            wonLineIdList.Add(new int[] { 36, 15, 12, 8, 4, 1 });            
            spinController.starSpin.AddListener(SendRequestToSever);
            spinController.showValue.AddListener(ShowValue);
            fishGameController.receiveResultEvent += OnResultSpinData;
            EndMiniGameEvent.AddListener(EndMiniGame);
            CPlayer.forceStopGameEvent += EnoughMoney;
            fishGameControllLine.SetUpLineSelect();
            txtWin.text = "0";
       
        }

        public void ShowHistory()
        {
            fishGameController.ShowHistory();
        }

        public void ShowGuilde()
        {
            fishGameController.ShowTutorial();
        }
        public void ShowGlory()
        {
            fishGameController.ShowGloryBoard();
        }

        public void Back()
        {
            DOTween.KillAll();
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            spinController.starSpin.RemoveListener(SendRequestToSever);
            spinController.showValue.RemoveListener(ShowValue);
            fishGameController.receiveResultEvent -= OnResultSpinData;
            EndMiniGameEvent.RemoveListener(EndMiniGame);
            CPlayer.forceStopGameEvent -= EnoughMoney;
            fishGameController.OnQuitGame();
        }
        public void OpenChangeBet()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            if (enoughMoney)
            {
                spinController.starSpin.RemoveListener(SendRequestToSever);
                spinController.showValue.RemoveListener(ShowValue);
                fishGameController.receiveResultEvent -= OnResultSpinData;
                EndMiniGameEvent.RemoveListener(EndMiniGame);
                CPlayer.forceStopGameEvent -= EnoughMoney;
                fishGameController.OnQuitGame("Fish");
            }
            Destroy(winnerTextClone);
            txtElementWin.text = "";
            txtWon.gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
            fishGameControlBet.gameObject.SetActive(true);
        }
        public void OpenSetting()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            LoadingControl.instance.loadingGojList[17].SetActive(true);
        }
        public void OpenRecharge()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            DOTween.To(() => rtfsMove[0].anchoredPosition, x => rtfsMove[0].anchoredPosition = x, new Vector2(0, 300), .35f);
            DOTween.To(() => rtfsMove[1].anchoredPosition, x => rtfsMove[1].anchoredPosition = x, new Vector2(0, -300), .35f);
            DOTween.To(() => rtfsMove[2].anchoredPosition, x => rtfsMove[2].anchoredPosition = x, new Vector2(-1800, 0), .35f);
            DOTween.To(() => rtfsMove[3].anchoredPosition, x => rtfsMove[3].anchoredPosition = x, new Vector2(-1800, 0), .5f);
            LoadingControl.instance.loadingGojList[7].SetActive(true);
        }
        public void CloseRecharge()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            LoadingControl.instance.loadingGojList[21].SetActive(true);
            DOTween.To(() => rtfsMove[0].anchoredPosition, x => rtfsMove[0].anchoredPosition = x, new Vector2(0, 0), .35f);
            DOTween.To(() => rtfsMove[1].anchoredPosition, x => rtfsMove[1].anchoredPosition = x, new Vector2(0, 0), .35f);
            DOTween.To(() => rtfsMove[2].anchoredPosition, x => rtfsMove[2].anchoredPosition = x, new Vector2(10, 0), .35f);
            DOTween.To(() => rtfsMove[3].anchoredPosition, x => rtfsMove[3].anchoredPosition = x, new Vector2(3, 14), .35f);
        }
        private void SendRequestToSever()
        {
            rtfPotBreak.gameObject.SetActive(false);
            rtfBigWin.gameObject.SetActive(false);
            StopTweenForNewSpin();
            wonLineData.Clear();
            drawnLineTmp.Clear();
            txtElementWin.text = "";
            if (DOTween.IsTweening(spinController.effectLine))
                DOTween.Kill(spinController.effectLine);
            for (int i = 0; i < spinController.rsPiecesControll.Length; i++)
            {
               // spinController.rsPiecesControll[i].GetComponent<Image>().enabled = false;
            }
            txtWon.gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
            if (winnerTextClone != null)
                Destroy(winnerTextClone);
            if (txtWon.transform.parent.childCount > 2)
                Destroy(txtWon.transform.parent.GetChild(txtWon.transform.parent.childCount-1));
            for (int i = 0; i < buttonHideSpin.Length; i++)
            {
                buttonHideSpin[i].interactable = false;
            }
           // buttonHideSpin[3].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", false);
            buttonHideSpin[2].transform.GetChild(0).GetComponent<Image>().enabled = false;
            if (isAutoSpin == false && ((FishSlotInfo)(fishGameController.GameInfo)).playReal == true)
                btnAutoSpin.interactable = false;           
            fishGameController.Play();
        }
        private void OnResultSpinData(Hashtable data)
        {
            rsLineIdsList = (int[])data["perLinePrizeIndexs"];
            wonValue = (int)data["totalMoneyWin"];
            isBigWin = (bool)data["isBigWin"];
            isPotBreak = (bool)data["isPotBreak"];
            totalFreeSpin = (int)data["totalFreeSpin"];
            numberBallGame = (int)data["remainSpin"];
            isTrial = (bool)data["isTrial"];
            int[] lineIndex = (int[])data["perLinePrizeIndexs"];
            int[] pieceIndex = (int[])data["piecePrizeIndexs"];
            int[] pieceNum = (int[])data["piecePrizeNums"];
            int countPrize = (int)data["totalPrize"];
            for(int i =0; i< countPrize; i++)
            {
                wonLineData.Add(lineIndex[i], new int[] { pieceIndex[i], pieceNum[i] });
            }
            spinController.CallBackDataSpin(data);
        }

        public void OpenLineSelect(bool isOpen)
        {
            if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == false || totalFreeSpin > 0)
            {
                return;
            }
            fishGameControllLine.OpenLineSelect(isOpen);
        }

        public void DoAction(string type)
        {
            switch (type)
            {
                case "bigWin":
                    SoundManager.instance.PlayUISound(SoundFX.BIG_WIN+"_"+Random.Range(1,4));
                    txtBigWIn.text = 0.ToString();
                    rtfBigWin.transform.localScale = 5 * Vector2.one;
                    rtfBigWin.gameObject.SetActive(true);
                    rtfBigWin.DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() =>
                    {
                        if (tweenNum != null)
                            StopCoroutine(tweenNum);
                        tweenNum = fishGameController.TweenNum(txtBigWIn, 0, wonValue);
                        StartCoroutine(tweenNum);
                        txtWin.text = string.Format("{0:0,0}", wonValue);
                    });
                    //StartCoroutine(HideSmt(rtfBigWin.gameObject, 5f));
                    Invoke("AllowSpin", .5f);
                    break;
                case "potBreak":
                    //SoundManager.instance.PlayEffectSound(SoundFX.JACKPOT + "_" + Random.Range(1, 3));
                    txtPotBreak.text = 0.ToString();
                    rtfPotBreak.transform.localScale = 5 * Vector2.one;
                    rtfPotBreak.gameObject.SetActive(true);
                    rtfPotBreak.DOScale(1f, .5f).SetEase(Ease.OutElastic).OnComplete(() =>
                    {
                        if (tweenNum != null)
                            StopCoroutine(tweenNum);
                        tweenNum = fishGameController.TweenNum(txtPotBreak, 0, wonValue);
                        StartCoroutine(tweenNum);
                        txtWin.text = string.Format("{0:0,0}", wonValue);
                    });
                    //StartCoroutine(HideSmt(rtfPotBreak.gameObject, 7f));
                    Invoke("AllowSpin", .5f);
                    break;
                case "changeAutoSpin":
                    SpriteState spriteState = btnAutoSpin.spriteState;

                    if (isAutoSpin == false)
                    {
                        if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == false)
                        {
                            return;
                        }
                        isAutoSpin = true;
                        btnAutoSpin.image.sprite = sprtAutoPlay[1];
                        spriteState.pressedSprite = sprtAutoPlay[1];
                        StopTweenForNewSpin();
                        spinController.NewSpin();
                    }
                    else
                    {
                        isAutoSpin = false;
                        btnAutoSpin.image.sprite = sprtAutoPlay[0];
                        spriteState.pressedSprite = sprtAutoPlay[0];
                        if (spinController.isPlaying == false)
                        {
                            for (int i = 0; i < buttonHideSpin.Length; i++)
                            {
                                buttonHideSpin[i].interactable = true;
                            }
                          //  buttonHideSpin[3].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active1", false);
                            buttonHideSpin[2].transform.GetChild(0).GetComponent<Image>().enabled = true;
                            if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == true)
                                btnAutoSpin.interactable = true;
                        }
                        else
                            btnAutoSpin.interactable = false;
                    }
                    btnAutoSpin.spriteState = spriteState;
                    break;

            }
        }
        private void AllowSpin()
        {
            if (isAutoSpin == false)
            {
                for (int j = 0; j < buttonHideSpin.Length; j++)
                {
                    buttonHideSpin[j].interactable = true;
                }
                buttonHideSpin[2].transform.GetChild(0).GetComponent<Image>().enabled = true;
                if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == true)
                    btnAutoSpin.interactable = true;
            }
        }
        private void ShowValue()
        {
            StartCoroutine(_ShowValue());
        }
        private IEnumerator _ShowValue()
        {
            Text[] txtArr = txtShowFreeSpin.GetComponentsInChildren<Text>();
            if (totalFreeSpin > 0)
            {
                txtShowFreeSpin.gameObject.SetActive(true);
                txtArr[1].text = totalFreeSpin.ToString();
            }
            else
            {
                txtShowFreeSpin.gameObject.SetActive(false);
            }


            if (true)
            {
                if (isBigWin == true && isPotBreak == false)
                {
                    DoAction("bigWin");
                }

                if (isPotBreak == true)
                {
                    if (isAutoSpin)
                    {
                        DoAction("changeAutoSpin");
                    }
                    DoAction("potBreak");
                }
                if (wonValue > 0)
                {
                    //if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == true)
                    //{
                    //    if (tweenNum2 != null)
                    //        StopCoroutine(tweenNum2);
                    //    tweenNum2 = fishGameController.TweenNum(txtBalance, (int)CPlayer.preChipBalance, (int)CPlayer.chipBalance, 3f, 1f, 2);
                    //    StartCoroutine(tweenNum2);

                    //}

                    if (isPotBreak == false && isBigWin == false)
                    {
                        StartCoroutine(showBalanceRs(0, wonValue));
                    }
                }


                if (rsLineIdsList.Length == 0)
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.LOOSE);
                    spinController.isPlaying = false;
                    if (isAutoSpin == false)
                    {
                        for (int k = 0; k < buttonHideSpin.Length; k++)
                        {
                            buttonHideSpin[k].interactable = true;
                        }
                        // buttonHideSpin[3].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active1", false);
                        buttonHideSpin[2].transform.GetChild(0).GetComponent<Image>().enabled = true;
                        if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == true)
                            btnAutoSpin.interactable = true;
                    }
                    if (isAutoSpin == true)
                    {
                        yield return new WaitForSeconds(.5f);
                        StopTweenForNewSpin();
                        spinController.NewSpin();
                    }

                }
                else
                {
                    SoundManager.instance.PlayEffectSound(SoundFX.WIN_3);
                    for (int k = 0; k < rsLineIdsList.Length; k++)
                    {
                        DrawLine(rsLineIdsList[k] + 1, true);
                    }
                    if (totalFreeSpin > 0)
                    {
                        spinController.isPlaying = false;
                        if (isAutoSpin == false )
                        {
                            if (numberBallGame <= 0)
                            {
                                for (int j = 0; j < buttonHideSpin.Length; j++)
                                {
                                    buttonHideSpin[j].interactable = true;
                                }
                                //  buttonHideSpin[3].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active1", false);
                                buttonHideSpin[2].transform.GetChild(0).GetComponent<Image>().enabled = true;
                                if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == true)
                                    btnAutoSpin.interactable = true;
                            }
                        }
                    }
                    if(((FishSlotInfo)(fishGameController.GameInfo)).playReal == false)
                    {
                        if (numberBallGame <= 0)
                        {
                            for (int j = 0; j < buttonHideSpin.Length; j++)
                            {
                                buttonHideSpin[j].interactable = true;
                            }
                        }
                    }
                    tweenNum3 = _DrawMultiLine(rsLineIdsList);
                    StartCoroutine(tweenNum3);
                    yield return new WaitForSeconds(2f);
                    spinController.isPlaying = false;

                }

            }
        }
        private GameObject winnerTextClone = null;
        private IEnumerator showBalanceRs(int fromNum, int toNum)
        {
            float i = 0.0f;
            float rate = 1.0f / .5f;
            Text txt = Instantiate(txtWon, txtWon.transform.parent, false);
            txt.gameObject.SetActive(true);
            txtWon.gameObject.transform.parent.GetChild(0).gameObject.SetActive(true);
            txt.transform.DOScale(2, 1f);
            winnerTextClone = txt.gameObject;
            while (i < 2)
            {
                i += Time.deltaTime * rate;
                float a = Mathf.Lerp(fromNum, toNum, i);
                txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                yield return null;
            }
            yield return new WaitForSeconds(.05f);
            txt.transform.parent = txtElementWin.transform.parent;
            txtElementWin.text = string.Format("{0:0,0}", toNum);
            
            spinController.isPlaying = false;
            if (isAutoSpin == false)
            {
                if (numberBallGame <= 0)
                {
                    for (int j = 0; j < buttonHideSpin.Length; j++)
                    {
                        buttonHideSpin[j].interactable = true;
                    }
                    //  buttonHideSpin[3].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active1", false);
                    buttonHideSpin[2].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    if (((FishSlotInfo)(fishGameController.GameInfo)).playReal == true)
                        btnAutoSpin.interactable = true;
                }
            }
        }


        private void StopTweenForNewSpin()
        {
            if(tweenNum != null)
                StopCoroutine(tweenNum);
            if(tweenNum2 != null)
                StopCoroutine(tweenNum2);
            if(tweenNum3 != null)
                StopCoroutine(tweenNum3);
            for (int i = 0; i < needToDelGojList.Count; i++)
            {
                if (needToDelGojList[i] != null)
                {
                    List<GameObject> arr = needToDelGojList[i];
                    Destroy(arr[0]);
                    Destroy(arr[1]);
                    //Destroy(arr[2]);
                }
            }
            needToDelGojList.Clear();
        }
        private IEnumerator HideSmt(GameObject obj, float timeToWait)
        {
            yield return new WaitForSeconds(timeToWait);
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        public void DrawLine(int lineId)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            DrawLine(lineId, true);
        }

        private void DrawLineAfterSpin(int lineId, bool needToDel = false, bool isLast = false)
        {
            DrawLine(lineId, needToDel, 1.2f, isLast);
        }

        private void DrawLine(int lineId, bool needToDel = false, float timeToDel = 2f, bool isLast = false)
        {
            lineId -= 1;
            int[] ids = wonLineIdList[lineId];           
            for (int k = 0; k < ids.Length-1; k++)
            {
                GameObject dotClone = Instantiate(rtfPointLineDraw[ids[k] - 1].gameObject, rtfPointLineDraw[ids[k] - 1].gameObject.transform.parent, false);
                dotClone.SetActive(true);
                GameObject lineElement = Instantiate(rtfLineElementClone.gameObject, rtfLineElementClone.gameObject.transform.parent, false);
                lineElement.GetComponent<Image>().color = lineColors[lineId];
                Vector2 dir = rtfPointLineDraw[ids[k+1]-1].anchoredPosition - rtfPointLineDraw[ids[k]-1].anchoredPosition;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                lineElement.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                lineElement.GetComponent<RectTransform>().localPosition = rtfPointLineDraw[ids[k]-1].localPosition;
                lineElement.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                float distance = Vector2.Distance(rtfPointLineDraw[ids[k]-1].anchoredPosition, rtfPointLineDraw[ids[k+1]-1].anchoredPosition);
                lineElement.GetComponent<RectTransform>().sizeDelta = new Vector2(distance, 22);
                lineElement.SetActive(true);

                needToDelGojList.Add(new List<GameObject>() { lineElement.gameObject, dotClone });
                drawnLineTmp.Add(new List<GameObject>() { lineElement.gameObject, dotClone });
            }

            if (needToDel)
                StartCoroutine(_DrawLine(timeToDel, needToDelGojList, isLast));
        }

        private IEnumerator _DrawLine(float timeToDelay, List<List<GameObject>> needToDelGojList, bool isLast = false)
        {
            yield return new WaitForSeconds(timeToDelay);
            for (int i = 0; i < needToDelGojList.Count; i++)
            {
                if (needToDelGojList[i] != null)
                {
                    List<GameObject> arr = needToDelGojList[i];
                    Destroy(arr[0]);
                    Destroy(arr[1]);
                    //Destroy(arr[2]);
                }
            }
            needToDelGojList.Clear();
            if (isLast && isAutoSpin == true)
            {
                StopTweenForNewSpin();
                spinController.NewSpin();
            }
        }

        private IEnumerator _DrawMultiLine(int[] lineToDrawLs)
        {
            yield return new WaitForSeconds(2.2f);
            //if (isAutoSpin == false)
            //for (int i = 0; i < rsPiecesControll.Length; i++)
            //{
            // rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().color = colors[2];
            //}
            if (isBigWin == true || isPotBreak == true)
            {
                spinController.isPlaying = false;
            }
            if (numberBallGame > 0)
            {
                Debug.Log("numberBallGame = " + numberBallGame);
                LoadingControl.instance.loadingGojList[21].SetActive(false);
                callBallGame.Invoke(numberBallGame, ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel, isTrial);
                if (isAutoSpin == true)
                {
                    isAutoSpin = false;
                    currentAutoSpin = true;
                }
                spinController.isPlaying = false;

            }
            if (spinController.isPlaying == false)
                for (int i = 0; i < lineToDrawLs.Length; i++)
                {
                    if (isAutoSpin == false)
                    {
                        //moveTo = 1;
                        int lineToDr = lineToDrawLs[i];
                        HighLightLine(lineToDr, wonLineData[lineToDr][0], wonLineData[lineToDr][1]);
                        DrawLineAfterSpin(rsLineIdsList[i] + 1, true, i == lineToDrawLs.Length - 1);
                        //DrawLineEffect(rsLineIdsList[i] + 1);
                        yield return new WaitForSeconds(1.2f);
                    }
                }
            if (isAutoSpin == true)
            {
                if (isBigWin == true)
                    yield return new WaitForSeconds(3f);
                yield return new WaitForSeconds(.2f);
                StopTweenForNewSpin();
                spinController.NewSpin();
            }
        }
        /* 
        private void DrawLineEffect(int lineId)
        {
            
            lineId -= 1;
            int[] ids = wonLineIdList[lineId];
            
            effectLine.transform.SetParent(rtfPointLineDraw[0].gameObject.transform.parent);
            effectLine.SetActive(true);
            effectLine.GetComponent<ParticleSystem>().Play();
            effectLine.GetComponent<RectTransform>().anchoredPosition = rtfPointLineDraw[ids[0]-1].anchoredPosition;
            RunLineEffect(effectLine, ids);
        }
        private int moveTo = 1;
        public void RunLineEffect(GameObject effect,int[] ids)
        {
            effect.transform.DOLocalMove(rtfPointLineDraw[ids[moveTo] - 1].anchoredPosition, .3f).OnComplete(()=> {
                moveTo++;
                if (moveTo < ids.Length)
                    RunLineEffect(effect, ids);
                else
                {
                    effect.SetActive(false);
                    effectLine.GetComponent<ParticleSystem>().Stop();
                }
            });
        }
        */
        /// <summary>
        /// When a line is drew, a pieces will be highlighted if the line cross
        /// </summary>
        private void HighLightLine(int lineId, int targetPieceId, int targetPieceNum)
        {
            if (spinController.isPlaying)
                return;
            SoundManager.instance.PlayEffectSound(SoundFX.WIN_LINE_ONCE+"_"+Random.Range(1,6));
            spinController.HighLightPiece(lineId,targetPieceId,targetPieceNum,wonLineIdList,rtfPointLineDraw);

           
                  
        }

        private void EndMiniGame()
        {
            LoadingControl.instance.loadingGojList[21].SetActive(true);
            AllowSpin();
            if (currentAutoSpin)
            {
                isAutoSpin = true;
                currentAutoSpin = false;
                StopTweenForNewSpin();
                spinController.NewSpin();
            }

        }

        public void EnoughMoney(string gamecode, int gameState)
        {
            if (gamecode == "season")
            {
                enoughMoney = true;
                buttonHideSpin[0].interactable = true;
            }
        }
        private void Update()
        {
            txtBalance.text = MoneyManager.instance.FakeCoin;
        }
    }

}
