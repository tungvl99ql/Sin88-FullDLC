using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Casino.Core;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

namespace Casino.Games.BxB {	
	public class ChooseLinesPanel : MonoBehaviour {
		
        public LineToggle[] lineBtns;
		public CustomToggleGroup controlGroup;
		public CanvasGroup suggestion;
		public List<int> EncodedData {
			get { 
				List<int> tempList = new List<int> ();
				for (int i = 0; i < chooseList.Length; i++) {
					if (chooseList[i] == true) {
						tempList.Add (i);
					}
				}
				return tempList;
			}
		}

		public BXBBetData CurrentBetData {
			get;
			set;
		}



		private bool[] chooseList = new bool[20];

		void OnEnable() {
			setListener (lineBtns, true);
			controlGroup.OnChangeSelect = changeControlBtn;
            fetchData();
			updateUI ();
		}

		void OnDisable() {
			setListener (lineBtns, false);
			controlGroup.OnChangeSelect -= changeControlBtn;
			suggestion.gameObject.SetActive (false);
		}

		private void updateUI() {
            for (int i = 0; i < chooseList.Length; i++)
            {
                lineBtns[i].isOn = chooseList[i];               
            }
        }

        private void fetchData() {
            for (int i = 0; i < chooseList.Length; i++)
            {
                chooseList[i] = false;
            }

            foreach (var item in CurrentBetData.lineIdxes)
            {
                chooseList[item] = true;
            }
        }


		private void changeControlBtn(int idx) {
			switch (idx) {
			case 0:
				ShooseOddLines ();
				break;
			case 1:
				ChooseEvenLines ();
				break;
			case 2:
				SelectAll ();
				break;
			case 3:
				Discard ();
				break;
			default:
				break;
			}

		}

        private void setListener (LineToggle[] btns, bool isAddingListeners) {

			for (int i = 0; i < btns.Length; i++) {
				if (isAddingListeners) {
                    btns[i].id = i;
                    btns [i].changeValueEvent.AddListener (SetSelectLine);
				} else {
                    btns [i].changeValueEvent.RemoveListener (SetSelectLine);
				}

			}
		}

        private void SetSelectLine(int lineIdx, bool isSelected) {
            chooseList [lineIdx] = isSelected;
            CurrentBetData.lineIdxes = EncodedData;
		}
		
		public void ChooseEvenLines() {
			for (int i = 0; i < chooseList.Length; i++) {
				if (i%2 == 0) {
					chooseList [i] = true;
				} else {
					chooseList [i] = false;
				}
			}
            CurrentBetData.lineIdxes = EncodedData;
            updateUI();
		}

		public void ShooseOddLines() {
			for (int i = 0; i < chooseList.Length; i++) {
				if (i%2 == 0) {
					chooseList [i] = false;
				} else {
					chooseList [i] = true;
				}
			}
            CurrentBetData.lineIdxes = EncodedData;
            updateUI();
		}

		public void SelectAll() {
			for (int i = 0; i < chooseList.Length; i++) {
				chooseList [i] = true;
			}
            CurrentBetData.lineIdxes = EncodedData;
            updateUI();
		}

		public void Discard() {
			for (int i = 0; i < chooseList.Length; i++) {
				chooseList [i] = false;
			}
            CurrentBetData.lineIdxes = EncodedData;
            updateUI();
		}

		public void Close() {

			bool isAvailable = false;

			for (int i = 0; i < chooseList.Length; i++) {
				if (chooseList[i] == true) {
					isAvailable = true;
				}
			}

			if (isAvailable) {
				gameObject.SetActive (false);
			} else {
				//suggestion.gameObject.SetActive (true);
				StartCoroutine(ieShowSuggestion(2f));
			}
		}

		IEnumerator ieShowSuggestion(float duration) {
			suggestion.gameObject.SetActive (true);
			suggestion.alpha = 1f;
			while (suggestion.alpha > 0) {
				suggestion.alpha -= 0.01f;
				yield return new WaitForSeconds (duration/100f);
			}
			yield return null;
		}

	}
}

