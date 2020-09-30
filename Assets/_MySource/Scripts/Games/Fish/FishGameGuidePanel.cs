using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using System.Linq;
using UnityEngine.UI;

namespace Slot.Games.Fish {
    public class FishGameGuidePanel : UIPanel
    {
        public Dictionary<int, List<ArrayList>> achiveData = new Dictionary<int, List<ArrayList>>();
        public GameObject guidePrefabs;
        public GameObject guidePotBreak;
        public Transform guideParent;
        public Sprite[] sprts;
        public GameObject content;
        public FishGameController fishGameController;
        public override void Show()
        {
            for(int i=0;i<content.transform.childCount;i++)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            foreach (int itemIndex in achiveData.Keys.ToList())
            {


                List<ArrayList> ls = achiveData[itemIndex];

                if ((string)ls[0][1] == "Nổ hũ")
                {
                    Text[] txtArr = guidePotBreak.GetComponentsInChildren<Text>();
                    Image[] imgArr = guidePotBreak.GetComponentsInChildren<Image>();

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
                  //  rsPiecesControll[i].transform.GetChild(0).GetComponent<Image>().overrideSprite = hu[idBet];
                
                    for (int i = 0; i < ls.Count; i++)
                    {
                       
                       // Debug.Log("No hu");
                        txtArr[i].text = ls[i][0] + " = " + ls[i][1];
                        //imgArr[i].sprite = sprts[itemIndex - 1];
                       // Debug.Log(txtArr[i].text + "   " + sprts[itemIndex - 1].name);
                    }
                    continue;
                }

                GameObject goj = Instantiate(guidePrefabs, guideParent, false);
                Text txt = goj.GetComponentInChildren<Text>();
                string tmp = "";
                for (int i = 0; i < ls.Count; i++)
                {
                    tmp += ls[i][0] + " = " + ls[i][1];
                    if (i != ls.Count - 1)
                        tmp += "\n";
                }
                txt.text = tmp;
                Image img = goj.GetComponentInChildren<Image>();
                img.sprite = sprts[itemIndex - 1];
                //Debug.Log(txt.text + "    " + sprts[itemIndex - 1].name);
                //Debug.Log("itemIndex= "+ itemIndex);
                goj.SetActive(true);
            }
        }
    }
}
