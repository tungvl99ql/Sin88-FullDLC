using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Core.Server.Api;

namespace Slot.Games.Fish
{
    public class FishGameControllLine : MonoBehaviour
    {
        [Header("SELECT LINE")]
        public RectTransform rtfLineSelect;
        public GameObject lineElement;
        public GameObject[] linePointBtn;
        public Text txtLineCount;
        public Text txtTotalBet;
        public Sprite[] sprtLineSelect;
        private List<LineSelect> lineSelectList;
        public FishGameController fishGameController;
        private int totalBetValue = 0;
        public void SetUpLineSelect()
        {
            ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet = 20;
            txtLineCount.text = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet.ToString();
            totalBetValue = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel;
            txtTotalBet.text = App.formatMoney(totalBetValue.ToString());
            lineSelectList = new List<LineSelect>();
            for (int i = 0; i < 20; i++)
            {
                lineSelectList.Add(new LineSelect(i, null, linePointBtn[i].GetComponent<Image>(), true));
            }
        }
        public class LineSelect
        {
            private bool isSlect;
            private Image btnCirlce;                        //Line select in main body
            private Image btnRect;                          //Line select in pop up
            private int index;

            public LineSelect(int index, Image rect, Image circle, bool isSelect)
            {
                this.IsSlect = isSelect;
                this.BtnCirlce = circle;
                this.BtnRect = rect;
                this.IsSlect = isSelect;
            }

            public bool IsSlect
            {
                get
                {
                    return isSlect;
                }

                set
                {
                    isSlect = value;
                }
            }

            public Image BtnCirlce
            {
                get
                {
                    return btnCirlce;
                }

                set
                {
                    btnCirlce = value;
                }
            }

            public Image BtnRect
            {
                get
                {
                    return btnRect;
                }

                set
                {
                    btnRect = value;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }

                set
                {
                    index = value;
                }
            }
        }

        public void OpenLineSelect(bool isShow)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            if (!isShow)
            {
                int posValue = 0;
                ((FishCurrLine)((FishSlotInfo)(fishGameController.GameInfo)).NumberLine).currentLine = new int[((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet];
                for (int i = 0; i < 20; i++)
                {
                    if (lineSelectList[i].IsSlect == true)
                    {
                        ((FishCurrLine)((FishSlotInfo)(fishGameController.GameInfo)).NumberLine).currentLine[posValue] = i;
                        posValue++;
                    }
                }
                rtfLineSelect.DOLocalMoveY(100, .1f).OnComplete(() =>
                {
                    rtfLineSelect.gameObject.SetActive(false);
                });
                return;
            }
            rtfLineSelect.gameObject.SetActive(true);
            rtfLineSelect.DOLocalMoveY(0, .1f);

            foreach (Transform rtf in lineElement.transform.parent)
            {
                if (rtf.gameObject.name != lineElement.name)
                {
                    Destroy(rtf.gameObject);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                int tmp = i;
                GameObject goj = Instantiate(lineElement, lineElement.transform.parent, false);
                Image img = goj.GetComponent<Image>();
                img.sprite = lineSelectList[tmp].IsSlect ? sprtLineSelect[tmp] : sprtLineSelect[20 + tmp];
                Button btn = goj.GetComponent<Button>();
                lineSelectList[tmp].BtnRect = img;
                btn.onClick.AddListener(() =>
                {
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    bool isSelected = !lineSelectList[tmp].IsSlect;
                    lineSelectList[tmp].BtnRect.sprite = isSelected ? sprtLineSelect[tmp] : sprtLineSelect[20 + tmp];
                    lineSelectList[tmp].IsSlect = isSelected;

                    ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet + (isSelected ? 1 : -1);
                    txtLineCount.text = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet.ToString();
                    totalBetValue = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel;
                    txtTotalBet.text = App.formatMoney(totalBetValue.ToString());

                });
                ; goj.SetActive(true);
            }
        }
        public void DoAction(string type)
        {
            switch (type)
            {
                case "selectAllLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet = 20;
                    txtLineCount.text = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet.ToString();
                    totalBetValue = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel;
                    txtTotalBet.text = App.formatMoney(totalBetValue.ToString());

                    for (int i = 0; i < 20; i++)
                    {
                        if (rtfLineSelect.gameObject.activeSelf == true)
                        {
                            lineSelectList[i].BtnRect.sprite = sprtLineSelect[i];
                        }
                        lineSelectList[i].IsSlect = true;
                    }
                    break;
                case "deSelectAllLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet = 0;
                    txtLineCount.text = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet.ToString();
                    totalBetValue = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel;
                    txtTotalBet.text = App.formatMoney(totalBetValue.ToString());

                    for (int i = 0; i < 20; i++)
                    {
                        lineSelectList[i].BtnRect.sprite = sprtLineSelect[20 + i];
                        lineSelectList[i].IsSlect = false;

                    }
                    break;
                case "selectEvenLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet = 10;
                    txtLineCount.text = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet.ToString();
                    totalBetValue = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel;
                    txtTotalBet.text = App.formatMoney(totalBetValue.ToString());

                    for (int i = 0; i < 20; i++)
                    {
                        if (i % 2 == 1)
                        {
                            lineSelectList[i].BtnRect.sprite = sprtLineSelect[i];
                            lineSelectList[i].IsSlect = true;
                        }
                        else
                        {
                            lineSelectList[i].BtnRect.sprite = sprtLineSelect[20 + i];
                            lineSelectList[i].IsSlect = false;
                        }
                    }
                    break;
                case "selectOddLine":
                    SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                    ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet = 10;
                    txtLineCount.text = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet.ToString();
                    totalBetValue = ((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel;
                    txtTotalBet.text = App.formatMoney(totalBetValue.ToString());
                    for (int i = 0; i < 20; i++)
                    {
                        if (i % 2 == 0)
                        {
                            lineSelectList[i].BtnRect.sprite = sprtLineSelect[i];
                            lineSelectList[i].IsSlect = true;
                        }
                        else
                        {
                            lineSelectList[i].BtnRect.sprite = sprtLineSelect[20 + i];
                            lineSelectList[i].IsSlect = false;
                        }
                    }
                    break;
            }
        }
    }
}
