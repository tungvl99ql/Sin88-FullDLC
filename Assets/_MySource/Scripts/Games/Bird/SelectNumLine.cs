using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Core.Bird
{
    public class SelectNumLine : MonoBehaviour
    {

        public UnityEvent CloseSelectNumLineEvent;
        public int[] listLine;
        public bool isSave = false;
        [SerializeField] Transform tfBtn;
        [SerializeField] BirdManager birdManager;
        [SerializeField] Button[] btns;
        [SerializeField] Image[] spriteRenderers;

        [SerializeField] Text[] txtOfBtns;
        [SerializeField] Sprite[] spritesActive;
        [SerializeField] Sprite[] spritesUnactive;

        void Awake()
        {

            btns = tfBtn.GetComponentsInChildren<Button>();
            txtOfBtns = tfBtn.GetComponentsInChildren<Text>();
            spriteRenderers = tfBtn.GetComponentsInChildren<Image>();
            //add text based on btns 
            //start new game btns 
            for (int i = 0; i < btns.Length; i++)
            {
                int id = i;
                btns[id].onClick.AddListener(() =>
                {
                    string txt = txtOfBtns[id].text;
                    spriteRenderers[id].overrideSprite = txt.Equals("Selected") ? spritesUnactive[id] : spritesActive[id];
                    txtOfBtns[id].text = txt.Equals("Selected") ? "Deselect" : "Selected";
                }); 
            }
        }
        void OnEnable()
        {

            for (int i = 0; i < btns.Length; i++)
            {
                bool isSelect = false;
                for (int j = 0; j < listLine.Length; j++)
                {
                    if (btns[i].name.Equals(listLine[j].ToString()))
                    {
                        isSelect = true;
                        continue;
                    }
                }
                spriteRenderers[i].overrideSprite = isSelect ? spritesActive[i] : spritesUnactive[i];
                txtOfBtns[i].text = isSelect ? "Selected" : "Deselect";
            }
        }
        public void SelectAll()
        {
            for (int i = 0; i < btns.Length; i++)
            {
                spriteRenderers[i].overrideSprite = spritesActive[i];
                txtOfBtns[i].text = "Selected";
            }
        }
        public void SelectOddNumber()
        {
            for (int i = 0; i < btns.Length; i++)
            {
                if ((i+1)%2 == 0)
                {
                    spriteRenderers[i].overrideSprite = spritesUnactive[i];
                    txtOfBtns[i].text = "Deselect";
                }
                else
                {
                    spriteRenderers[i].overrideSprite = spritesActive[i];
                    txtOfBtns[i].text = "Selected";
                }
            }
        }
        public void SelectEvenNumber()
        {
            for (int i = 0; i < btns.Length; i++)
            {
                if ((i + 1) % 2 == 0)
                {
                    spriteRenderers[i].overrideSprite = spritesActive[i];
                    txtOfBtns[i].text = "Selected";
                }
                else
                {
                    spriteRenderers[i].overrideSprite = spritesUnactive[i];
                    txtOfBtns[i].text = "Deselect";
                }
            }
        }
        public void DeselectAll()
        {
            for (int i = 0; i < btns.Length; i++)
            {
                spriteRenderers[i].overrideSprite = spritesUnactive[i];
                txtOfBtns[i].text = "Deselect";
            }
        }
        public void SaveAndClose()
        {
			isSave = true;
			SaveLineData();
            birdManager.SetActivePanel(BirdPanel.SelectNumLine, false);
        }
        public void OnClickCancel()
        {
            isSave = false;
            birdManager.SetActivePanel(BirdPanel.SelectNumLine, false);
        }
        private void SaveLineData()
        {
            List<int> cache = new List<int>();
            for (int i = 0; i < txtOfBtns.Length; i++)
            {
                if (txtOfBtns[i].text.Equals("Selected"))
                {
                    cache.Add(int.Parse(txtOfBtns[i].transform.parent.name));
                }
            }
            listLine = cache.ToArray();
        }
        void OnDisable()
        {
            if (CloseSelectNumLineEvent != null)
            {
                CloseSelectNumLineEvent.Invoke();
            }
        }
    }
}