using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine.Unity;
namespace Casino.Games.BxB {

	public class SlotsPanel : MonoBehaviour {

		public SlotColumn column1;

		public SlotColumn column2;

		public SlotColumn column3;

		public UnityEvent finishEffectEvent;

		public List<int[]> betLines = new List<int[]> {
			// line 0
			new int[] {0, 3, 6},
			// line 1
			new int[] {1, 4, 7},
			// line 2
			new int[] {2, 5, 8},
			// line 3
			new int[] {0, 5, 6},
			// line 4
			new int[] {2, 3, 8},
			// line 5
			new int[] {0, 4, 6},
			// line 6
			new int[] {0, 4, 8},
			// line 7
			new int[] {2, 4, 6},
			// line 8
			new int[] {1, 5, 7},
			// line 9
			new int[] {1, 3, 7},
			// line 10
			new int[] {2, 4, 8},
			// line 11
			new int[] {0, 3, 7},
			// line 12
			new int[] {1, 4, 8},
			// line 13
			new int[] {1, 4, 6},
			// line 14
			new int[] {2, 5, 7},
			// line 15
			new int[] {1, 3, 6},
			// line 16
			new int[] {2, 4, 7},
			// line 17
			new int[] {0, 4, 7},
			// line 18
			new int[] {1, 5, 8},
			// line 19
			new int[] {0, 5, 7},

		};

		public BXBCharContainer[] fixedSlots;

		public LineEffectDrawer effectDrawer;

		public Sprite[] spritesReference;
       // public Sprite[] spritesReferenceActive;

        public UnityEvent spinnerBeginEvent;

		private List<int[]> resultSlotData;
		private int[] resultSlot;
		private bool hasResult = false;

		Tween tween1;
		Tween tween2;
		Tween tween3;

		private void Start()
		{
//            drawLineEffects();
		}

		void OnEnable() {
			initColumn ();
			column1.SendRequestEvent.AddListener (OnLaunchSpinners);
			// the slowest spin will send the end spin event. so let addlestener to that spinner.
			column3.EndRollEvent.AddListener(OnDrawEffet);
		}

		void OnDisable() {
			column1.SendRequestEvent.RemoveListener (OnLaunchSpinners);
			column3.EndRollEvent.RemoveListener(OnDrawEffet);
		}

		void Update() {
			if (hasResult) {
				//drawLineEffects();

				column1.willStopNextLoop = true;
				column2.willStopNextLoop = true;
				column3.willStopNextLoop = true;

				hasResult = false;
			}
		}

		private void initColumn() {

			for (int i = 0; i < column1.columnImgs.Length; i++) {
				var ranIdx1 = UnityEngine.Random.Range (0, 6);
				var ranIdx2 = UnityEngine.Random.Range (0, 6);
				var ranIdx3 = UnityEngine.Random.Range (0, 6);
                
				column1.UpdateImageSprite (i, spritesReference [ranIdx1]);
				column2.UpdateImageSprite (i, spritesReference [ranIdx2]);
				column3.UpdateImageSprite (i, spritesReference [ranIdx3]);
			}
		}

        public void Spin(bool isFastMode) {
			effectDrawer.EarseAllLineEffect ();

			for (int i = 0; i < fixedSlots.Length; i++) {
				try {
                    //	fixedSlots [i].character.AnimationState.SetAnimation (0, "unactive", false);
                    var ranIdx1 = UnityEngine.Random.Range(0, 6);
                    fixedSlots[i].GetComponent<Image>().overrideSprite = spritesReference[ranIdx1];
                } catch (Exception ex) {
					
				}
			}


			//StartCoroutine(launchSpinners());
			//StartCoroutine (waitForResult());
            column1.Spin(isFastMode);
            column2.Spin(isFastMode);
            column3.Spin(isFastMode);
		}

		private void OnLaunchSpinners() {
			if (spinnerBeginEvent != null) {
				spinnerBeginEvent.Invoke ();
			}
		}


        public void OnReceiveResult(bool hasResult, ref int[] resultSlot, ref List<int[]> resultSlotData) {
            this.hasResult = hasResult;
            this.resultSlotData = resultSlotData;
			this.resultSlot = resultSlot;
            // TODO: fill Image sprite to actual result
            fillResultData(ref resultSlot);
			//Debug.LogError ("============================> hasResult = " + hasResult);
		}

        private void fillResultData(ref int[] resultSlot) {

            for (int i = 0; i < fixedSlots.Length; i++)
            {
                //  Debug.Log(resultSlot[i]);
                /*fixedSlots[i].transform.localScale = new Vector3(1, 1, 1);
               if (resultSlot[i]==4|| resultSlot[i]==5)
                 {
                     fixedSlots[i].transform.localScale = new Vector3(1.2f, 1.2f, 1);
                 }*/
                //Debug.Log(" "+(resultSlot[i] - 1) + " Sprite "+spritesReference[resultSlot[i] - 1].name);

				fixedSlots[i].Replace(spritesReference[resultSlot[i] - 1]);
            }
        }

        private void drawLineEffects() {
			foreach (var intArr in resultSlotData) {
				int lineNum = intArr[0];
				int charIdx = intArr[1];
				int charNum = intArr[2];

				// determine which pos in specify line will be draw. 

				var localPosArr = DetermineNewLocalPos (lineNum);
//				var localPosArr = DetermineNewLocalPos (lineNum, charIdx, charNum);
				effectDrawer.DrawLine (charIdx, localPosArr);
				//setAnimationActive (true, lineNum);
			
            setAnimationActive(lineNum, charIdx, charNum);
			}

			if (finishEffectEvent != null) {
				finishEffectEvent.Invoke ();
			}

        }

		private void setAnimationActive(bool active, int lineId) {

		/*	var indexes = betLines [lineId];
			for (int i = 0; i < indexes.Length; i++) {
				if (active) {
					try {
                        //fixedSlots [indexes[i]].character.AnimationState.SetAnimation (0, "active", true);
                    
                      //  fixedSlots[indexes[i]].GetComponent<Image>().overrideSprite=spritesReferenceActive[indexes[i]];
                    } catch (Exception ex) {
						
					}
				}
			}*/
		}


		private void setAnimationActive(int lineId, int charId, int charNum) {

			var indexes = betLines [lineId];

			for (int i = 0; i < indexes.Length; i++) {

				if (resultSlot[indexes[i]] == charId || resultSlot[indexes[i]] == 1) {
				try {
                        //	fixedSlots [indexes[i]].character.AnimationState.SetAnimation (0, "active", true);
                         // Debug.Log("indexes = " + indexes[i]+ " resultSlot = " + resultSlot[indexes[i]] + " charId = " + charId);
                        //  fixedSlots[indexes[i]].GetComponent<Image>().overrideSprite = spritesReferenceActive[charId-1];
                        //fixedSlots[indexes[i]].GetComponent<Image>().overrideSprite = spritesReference[charId - 1];
                    } catch (Exception ex) {
				}
				}
			}

		}

		void OnDrawEffet() {
			StartCoroutine (waitForDrawEffect (0.1f));
		}

		IEnumerator waitForDrawEffect(float t) {
			yield return new WaitForSeconds (t);
			drawLineEffects ();
		}



        private void drawLine(Vector3 starPos, Vector3 endPos) {
          




        }

        private Vector2[] DetermineNewLocalPos(int lineID) {

            Vector2[] localPos = new Vector2[3];

            for (int i = 0; i < betLines[lineID].Length; i++)
            {
                GameObject temp = null;
                switch (i)
                {
                    
                    case 0:
                        temp = column1.gameObject;
                        break;
                    case 1:
                        temp = column2.gameObject;
                        break;
                    case 2:
                        temp = column3.gameObject;
                        break;

                    default:
                        break;
                }

                localPos[i] = localPosConverter(fixedSlots[betLines[lineID][i]].transform.localPosition + (Vector3)fixedSlots[betLines[lineID][i]].gameObject.GetComponent<RectTransform>().rect.center, temp.transform.localPosition);


            }


			return localPos;
        }


		private Vector2[] DetermineNewLocalPos(int lineID,int charID, int numOfItem) {

			int[] id = new int[numOfItem];

//			for (int i = 0; i < betLines[lineID].Length; i++) {
//				int count = 0;
//				for (int j = 0; j < 3; j++) {
//
//					if (resultSlot[betLines[lineID][j]] == charID) {
//						id [count] = j;
//						count++;
//					}
//
//				}
//			}


			int count = 0;
			for (int j = 0; j < 3; j++) {

				if (resultSlot[betLines[lineID][j]] == charID || resultSlot[betLines[lineID][j]] == 1) {
					id [count] = betLines[lineID][j];
					count++;
				}

			}

			Vector2[] localPos = new Vector2[numOfItem];

			for (int k = 0; k < id.Length; k++) {

				switch (id[k]) {

				case 0:
				case 1:
				case 2:
					localPos [k] = localPosConverter(fixedSlots [id[k]].transform.localPosition + (Vector3)fixedSlots [id[k]].GetComponent<RectTransform>().rect.center, column1.transform.localPosition);
					break;

				case 3:
				case 4:
				case 5:
					localPos [k] = localPosConverter(fixedSlots [id[k]].transform.localPosition +  (Vector3)fixedSlots [id[k]].GetComponent<RectTransform>().rect.center, column2.transform.localPosition);
					break;

				case 6:
				case 7:
				case 8:
					localPos [k] = localPosConverter(fixedSlots [id[k]].transform.localPosition +  (Vector3)fixedSlots [id[k]].GetComponent<RectTransform>().rect.center, column3.transform.localPosition);
					break;


				default:
					break;
				}

				//localPos [k] = fixedSlots [id[k]].rectTransform.localPosition;
			}

			return localPos;

		}

		private Vector2 localPosConverter (Vector2 oldCoordinate, Vector2 oldLocalPos) {
			return oldCoordinate + oldLocalPos;
		}



        public void ForceStop() {

            column1.ForceStop();
            column2.ForceStop();
            column3.ForceStop();

        }

	}

}
