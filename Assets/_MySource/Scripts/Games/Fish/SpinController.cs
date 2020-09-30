using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;

namespace Slot.Games.Fish
{
    public class SpinController : MonoBehaviour
    {

        /// <summary>
        /// 0-7:row1 | 8-15:row2 | 16-24:row3 | 25-32:row4 | 33-40:row5
        /// </summary>
        [SerializeField]
        private RectTransform[] rftPiecesRow1;
        [SerializeField]
        private RectTransform[] rftPiecesRow2;
        [SerializeField]
        private RectTransform[] rftPiecesRow3;
        [SerializeField]
        private RectTransform[] rftPiecesRow4;
        [SerializeField]
        private RectTransform[] rftPiecesRow5;
        public GameObject[] rsPiecesControll;
       // public SkeletonDataAsset[] skeletonDataAssets;
        public UnityEvent starSpin;
        public UnityEvent showValue;
        public bool isPlaying;
        public GameObject effectLine;
        public FishGameController fishGameController;
        /// <summary>
        /// 0: white|1: trans
        /// </summary>
        public Color[] colors;
        private int[] ids;
        private IEnumerator[] thread = new IEnumerator[1];
        private Material materialSkeletonSpine;
        public Sprite[] spriteItems;

        private void Awake()
        {
            materialSkeletonSpine = Resources.Load("Material/SkeletonGraphicDefault.mat", typeof(Material)) as Material;
           /* int leght = rftPiecesRow1.Length;
           for (int i = 0; i < leght; i++)
            {
                rftPiecesRow1[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
                rftPiecesRow2[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
                rftPiecesRow3[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
                rftPiecesRow4[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
                rftPiecesRow5[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
            }*/
        }
        public void NewSpin()
        {
            if (isPlaying || ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet <1)
                return;

            SoundManager.instance.PlayUISound(SoundFX.SPIN);
           
            effectLine.SetActive(false);
            isPlaying = true;
            if(DOTween.IsTweening(effectLine))
                DOTween.Kill(effectLine);
            for (int i = 0; i < rsPiecesControll.Length; i++)
            {
                rsPiecesControll[i].transform.GetChild(0).GetComponent<Image>().color = colors[0];
                // rsPiecesControll[i].GetComponent<Image>().enabled = false;
                //  rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "unactive", true);
              //  rsPiecesControll[i].GetComponent<Image>().enabled = true;
            }

            spin = true;
            starSpin.Invoke();
        }
        private bool stopSpin1;
        private bool stopSpin2;
        private bool stopSpin3;
        private bool stopSpin4;
        private bool stopSpin5;

        private IEnumerator _StopSpin()
        {
            for (int i = 0; i < rsPiecesControll.Length; i++)
            {
              //  rsPiecesControll[i].GetComponent<Image>().enabled = false;
            }
            yield return new WaitForSeconds(.2f);
            stopSpin1 = true;
            yield return new WaitForSeconds(.4f);
            stopSpin2 = true;
            yield return new WaitForSeconds(.3f);
            stopSpin3 = true;
            yield return new WaitForSeconds(.3f);
            stopSpin4 = true;
            yield return new WaitForSeconds(.3f);
            stopSpin5 = true;
            yield return new WaitForSeconds(.5f);
            spin = false;
            speedRow1 = 2500;
            speedRow2 = 2800;
            speedRow3 = 3100;
            speedRow4 = 3400;
            speedRow5 = 3700;
            stopSpin1 = false;
            stopSpin2 = false;
            stopSpin3 = false;
            stopSpin4 = false;
            stopSpin5 = false;
            showValue.Invoke();
        }
        private bool spin = false;
        public float speedRow1 = 2500;
        public float speedRow2 = 2800;
        public float speedRow3 = 3100;
        public float speedRow4 = 3400;
        public float speedRow5 = 3700;
        private void StopColumn(RectTransform[] rtf, int id)
        {

            if (rtf[0].anchoredPosition.y < -235)
                rtf[0].anchoredPosition = Vector3.MoveTowards(rtf[0].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[0].anchoredPosition = Vector3.MoveTowards(rtf[0].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -220), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[1].anchoredPosition.y < -5)
                rtf[1].anchoredPosition = Vector3.MoveTowards(rtf[1].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[1].anchoredPosition = Vector3.MoveTowards(rtf[1].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 0), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[2].anchoredPosition.y < 215)
                rtf[2].anchoredPosition = Vector3.MoveTowards(rtf[2].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[2].anchoredPosition = Vector3.MoveTowards(rtf[2].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 220), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[3].anchoredPosition.y < 430)
                rtf[3].anchoredPosition = Vector3.MoveTowards(rtf[3].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[3].anchoredPosition = Vector3.MoveTowards(rtf[3].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 440), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[4].anchoredPosition.y < 650)
                rtf[4].anchoredPosition = Vector3.MoveTowards(rtf[4].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[4].anchoredPosition = Vector3.MoveTowards(rtf[4].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 660), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[5].anchoredPosition.y < 870)
                rtf[5].anchoredPosition = Vector3.MoveTowards(rtf[5].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[5].anchoredPosition = Vector3.MoveTowards(rtf[5].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 880), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[6].anchoredPosition.y < 1090)
                rtf[6].anchoredPosition = Vector3.MoveTowards(rtf[6].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[6].anchoredPosition = Vector3.MoveTowards(rtf[6].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 1100), Time.deltaTime * (2500 + (id * 300)));
            if (rtf[7].anchoredPosition.y < 1310)
                rtf[7].anchoredPosition = Vector3.MoveTowards(rtf[7].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, -440), Time.deltaTime * (2500 + (id * 300)));
            else
                rtf[7].anchoredPosition = Vector3.MoveTowards(rtf[7].anchoredPosition, new Vector2(rtf[0].anchoredPosition.x, 1320), Time.deltaTime * (2500 + (id * 300)));
        }
        private void Update()
        {
            if (stopSpin1)
            {
                speedRow1 = 0;
                StopColumn(rftPiecesRow1, 0);
            }
            if (stopSpin2)
            {
                speedRow2 = 0;
                StopColumn(rftPiecesRow2, 1);
            }
            if (stopSpin3)
            {
                speedRow3 = 0;
                StopColumn(rftPiecesRow3, 2);
            }
            if (stopSpin4)
            {
                speedRow4 = 0;
                StopColumn(rftPiecesRow4, 3);
            }
            if (stopSpin5)
            {
                speedRow5 = 0;
                StopColumn(rftPiecesRow5, 4);
            }
            if (spin)
            {
                for (int j = 0; j < 8; j++)
                {
                    rftPiecesRow1[j].transform.Translate(Vector2.down * speedRow1 * Time.deltaTime);
                    rftPiecesRow2[j].transform.Translate(Vector2.down * speedRow2 * Time.deltaTime);
                    rftPiecesRow3[j].transform.Translate(Vector2.down * speedRow3 * Time.deltaTime);
                    rftPiecesRow4[j].transform.Translate(Vector2.down * speedRow4 * Time.deltaTime);
                    rftPiecesRow5[j].transform.Translate(Vector2.down * speedRow5 * Time.deltaTime);
                }

            }

            for (int i = 0; i < 8; i++)
            {
                if (rftPiecesRow1[i].anchoredPosition.y <= -440f)
                {
                    if (i == 0)
                        rftPiecesRow1[i].anchoredPosition = new Vector2(rftPiecesRow1[i].anchoredPosition.x, rftPiecesRow1[7].anchoredPosition.y + 220f);
                    else
                        rftPiecesRow1[i].anchoredPosition = new Vector2(rftPiecesRow1[i].anchoredPosition.x, rftPiecesRow1[i - 1].anchoredPosition.y + 220f);
                }
                if (rftPiecesRow2[i].anchoredPosition.y <= -440f)
                {
                    if (i == 0)
                        rftPiecesRow2[i].anchoredPosition = new Vector2(rftPiecesRow2[i].anchoredPosition.x, rftPiecesRow2[7].anchoredPosition.y + 220f);
                    else
                        rftPiecesRow2[i].anchoredPosition = new Vector2(rftPiecesRow2[i].anchoredPosition.x, rftPiecesRow2[i - 1].anchoredPosition.y + 220f);
                }
                if (rftPiecesRow3[i].anchoredPosition.y <= -440f)
                {
                    if (i == 0)
                        rftPiecesRow3[i].anchoredPosition = new Vector2(rftPiecesRow3[i].anchoredPosition.x, rftPiecesRow3[7].anchoredPosition.y + 220f);
                    else
                        rftPiecesRow3[i].anchoredPosition = new Vector2(rftPiecesRow3[i].anchoredPosition.x, rftPiecesRow3[i - 1].anchoredPosition.y + 220f);
                }
                if (rftPiecesRow4[i].anchoredPosition.y <= -440f)
                {
                    if (i == 0)
                        rftPiecesRow4[i].anchoredPosition = new Vector2(rftPiecesRow4[i].anchoredPosition.x, rftPiecesRow4[7].anchoredPosition.y + 220f);
                    else
                        rftPiecesRow4[i].anchoredPosition = new Vector2(rftPiecesRow4[i].anchoredPosition.x, rftPiecesRow4[i - 1].anchoredPosition.y + 220f);
                }
                if (rftPiecesRow5[i].anchoredPosition.y <= -440f)
                {
                    if (i == 0)
                        rftPiecesRow5[i].anchoredPosition = new Vector2(rftPiecesRow5[i].anchoredPosition.x, rftPiecesRow5[7].anchoredPosition.y + 220f);
                    else
                        rftPiecesRow5[i].anchoredPosition = new Vector2(rftPiecesRow5[i].anchoredPosition.x, rftPiecesRow5[i - 1].anchoredPosition.y + 220f);
                }
            }
        }
        public void CallBackDataSpin(Hashtable data)
        {
            int totalPiece = (int)data["totalPiece"];
            ids = (int[])data["perPieces"];
            int[] lineIndex = (int[])data["perLinePrizeIndexs"];
            for (int i = 0; i < totalPiece; i++)
            {
              //  rsPiecesControll[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().skeletonDataAsset = skeletonDataAssets[ids[i] - 1];
             //   rsPiecesControll[i].transform.GetChild(0).GetComponent<SkeletonGraphic>().Initialize(true);

                if(ids[i] - 1==1)
                {
                   int bet= ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel;
                    int idBet = 0;
                    switch(bet)
                    {
                        case 100:
                            idBet = 0;
                            break;
                        case 500:
                            idBet = 1;
                            break;
                        case 1000:
                            idBet = 2;
                            break;
                        case 10000:
                            idBet = 3;
                            break;
                    }
                }
                else
                {
                    rsPiecesControll[i].transform.GetChild(0).GetComponent<Image>().overrideSprite = spriteItems[ids[i] - 1];
                }

            }
            StartCoroutine(_StopSpin());
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
        private int idColor = 0, id = 0;

     
     
   
        public void HighLightPiece(int lineId, int targetPieceId, int targetPieceNum, List<int[]> wonLineIdList, RectTransform[] rtfPointLineDraw)
        {
            if (isPlaying)
                return;
            //moveTo = 1;
            for (int i = 0; i < rsPiecesControll.Length; i++)
            {
                //rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().color = colors[2];
                rsPiecesControll[i].transform.GetChild(0).GetComponent<Image>().color = colors[2];
                //rsPiecesControll[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "unactive", true);
            }
            int[] pieceIds = wonLineIdList[lineId];
            //effectLine.SetActive(true);
            //effectLine.GetComponent<ParticleSystem>().Play();
            //effectLine.GetComponent<RectTransform>().anchoredPosition = rtfPointLineDraw[pieceIds[0] - 1].anchoredPosition;
            //RunLineEffect(effectLine, pieceIds, rtfPointLineDraw, targetPieceId, lineId);
            for (int i = 1; i < pieceIds.Length; i++)
            {
                if (ids[pieceIds[i] - 1] == targetPieceId)
                {
                    {
                        rsPiecesControll[pieceIds[i] - 1].transform.GetChild(0).GetComponent<Image>().color = colors[0];
                        //Spine.Animation active = rsPiecesControll[pieceIds[i] - 1].GetComponentInChildren<SkeletonGraphic>().SkeletonData.FindAnimation("active");
                        //if (active != null)
                        //    rsPiecesControll[pieceIds[i] - 1].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);
                    }
                }
            }
        }
        private int moveTo = 1;
        private void RunLineEffect(GameObject effect, int[] pieceIds, RectTransform[] rtfPointLineDraw, int targetPieceId, int lineId)
        {
            if (isPlaying)
                return;
            effect.transform.DOLocalMove(rtfPointLineDraw[pieceIds[moveTo] - 1].anchoredPosition, .2f).SetId("moveLineEffect").OnComplete(() =>
            {
                if (isPlaying)
                    return;
                if (ids[pieceIds[moveTo] - 1] == targetPieceId)
                {
                    if (lineId > 9)
                    {
                       // rsPiecesControll[pieceIds[moveTo] - 1].transform.GetChild(2).gameObject.SetActive(true);
                      //  rsPiecesControll[pieceIds[moveTo] - 1].transform.GetChild(2).gameObject.GetComponent<ParticleSystem>().Simulate(0f, false, true);
                       // StartCoroutine(HideEffect(rsPiecesControll[pieceIds[moveTo] - 1].transform.GetChild(2).gameObject));
                    }
                    else
                    {
                       // rsPiecesControll[pieceIds[moveTo] - 1].transform.GetChild(1).gameObject.SetActive(true);
                       // rsPiecesControll[pieceIds[moveTo] - 1].transform.GetChild(1).gameObject.GetComponent<ParticleSystem>().Simulate(0f, false, true);
                       // StartCoroutine(HideEffect(rsPiecesControll[pieceIds[moveTo] - 1].transform.GetChild(1).gameObject));
                    }
                    
                    //rsPiecesControll[pieceIds[moveTo] - 1].GetComponentInChildren<SkeletonGraphic>().color = colors[0];
                   // rsPiecesControll[pieceIds[moveTo] - 1].GetComponent<Image>().enabled = true;
                    //Spine.Animation active = rsPiecesControll[pieceIds[moveTo] - 1].GetComponentInChildren<SkeletonGraphic>().SkeletonData.FindAnimation("active");
                    //if (active != null)
                    //    rsPiecesControll[pieceIds[moveTo] - 1].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);
                }
                moveTo++;               
                if (moveTo < pieceIds.Length)
                    RunLineEffect(effect, pieceIds, rtfPointLineDraw, targetPieceId, lineId);
                else
                {
                    effect.SetActive(false);
                    effectLine.GetComponent<ParticleSystem>().Stop();
                }
            });
        }

        private IEnumerator HideEffect(GameObject effect)
        {
            yield return new WaitForSeconds(1f);
            effect.SetActive(false);
        }
    }
}
