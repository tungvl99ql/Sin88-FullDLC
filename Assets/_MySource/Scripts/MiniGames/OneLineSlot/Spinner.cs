using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Casino.Games.OneLineSlot {
	
	public class Spinner : MonoBehaviour {

		public SpinnerChild column1;
		public SpinnerChild column2;
		public SpinnerChild column3;

		public Sprite[] spritesReference;
		public CharacterSwapper[] swapSpos;

		public UnityEvent spinnerStartEvent;
		public UnityEvent spinnerSendRequetEvent;
		public UnityEvent spinnerFinishEvent;

		public bool AutoSpin {

			get;
			set;

		}

		void OnEnable() {
			//init ();
			column1.StartRollEvent.AddListener (onChildSpinerLaunch);
			column1.SendRequestEvent.AddListener (onChildSpinerSendRequest);
			column3.EndRollEvent.AddListener (onChildSpinerStop);

		}

		void Start() {
			init ();
		}

		void OnDisable() {
			column1.StartRollEvent.RemoveListener (onChildSpinerLaunch);
			column1.SendRequestEvent.RemoveListener (onChildSpinerSendRequest);
			column3.EndRollEvent.RemoveListener (onChildSpinerStop);
		}


		private void init() {

			for (int i = 0; i < column1.columnImgs.Length; i++) {
				var ranIdx1 = UnityEngine.Random.Range (0, 8);
				var ranIdx2 = UnityEngine.Random.Range (0, 8);
				var ranIdx3 = UnityEngine.Random.Range (0, 8);
				column1.UpdateImageSprite (i, spritesReference [ranIdx1]);
				column2.UpdateImageSprite (i, spritesReference [ranIdx2]);
				column3.UpdateImageSprite (i, spritesReference [ranIdx3]);
			}
            swapSpos[0].Replace(spritesReference[UnityEngine.Random.Range(0, 3)]);
            swapSpos[1].Replace(spritesReference[UnityEngine.Random.Range(4, 6)]);
            swapSpos[2].Replace(spritesReference[UnityEngine.Random.Range(6, 8)]);

        }


		public void Spin() {

            swapSpos[0].Normal();
            swapSpos[1].Normal();
            swapSpos[2].Normal();
            // test
            Spin(false);


		}
        public void ForceStop()
        {
            column1.ForceStop();
            column2.ForceStop();
            column3.ForceStop();
        }


        public void Spin(bool isFastMode) {
			column1.Spin(isFastMode);
			column2.Spin(isFastMode);
			column3.Spin(isFastMode);
		}

		public void ReceiveServerResult(Hashtable data) {
			column1.willStopNextLoop = true;
			column2.willStopNextLoop = true;
			column3.willStopNextLoop = true;

			string winItems = (string)data["item"];
          
            string[] p = winItems.Split ('-');
			int sptIdx1 = lookTable (p [0]);
			int sptIdx2 = lookTable (p [1]);
			int sptIdx3 = lookTable (p [2]);

			swapSpos [0].Replace (spritesReference [sptIdx1]);
			swapSpos [1].Replace (spritesReference [sptIdx2]);
			swapSpos [2].Replace (spritesReference [sptIdx3]);

            string pos = (string)data["positionWin"];  
            var   postion = pos.Split('-');
            
            int pos0 = -1, pos1 = -1, pos2 = -1;

            if (pos.Length > 1)
            {
                if (postion.Length == 2)
                {
                    pos0 = int.Parse(postion[0]);
                    pos1 = int.Parse(postion[1]);
                }
                else
                {
                    pos0 = int.Parse(postion[0]);
                    pos1 = int.Parse(postion[1]);
                    pos2 = int.Parse(postion[2]);
                }
            }
            else if(pos.Length==1)
            {
                pos0 = int.Parse(pos);
            }
            swapSpos[0].UnNormal();
            swapSpos[1].UnNormal();
            swapSpos[2].UnNormal();
            if (pos0 >= 0)
            {
                swapSpos[pos0].Normal();
            }
            if (pos1 >= 0)
            {
                swapSpos[pos1].Normal();
            }
            if (pos2 >= 0)
            {
                swapSpos[pos2].Normal();
            }
        }


		private void onChildSpinerLaunch() {

			if (spinnerStartEvent != null) {
				spinnerStartEvent.Invoke ();
			}

		}

		private void onChildSpinerSendRequest() {
			if (spinnerSendRequetEvent != null) {
				spinnerSendRequetEvent.Invoke ();
			}
		}

		private void onChildSpinerStop() {
			if (spinnerFinishEvent != null) {
				spinnerFinishEvent.Invoke ();
			}
		}

		public int lookTable(string letter) {
			int id = 0;
			char[] chars = letter.ToLower ().ToCharArray(0, 1);
			switch (chars[0]) {

			case 'a':
				id = 0;
				break;
			case 'b':
				id = 1;
				break;
			case 'c':
				id = 2;
				break;
			case 'd':
				id = 3;
				break;
			case 'e':
				id = 4;
				break;
			case 'f':
				id = 5;
				break;
			case 'g':
				id = 6;
				break;
			case 'h':
				id = 7;
				break;

			default:
				break;
			}

			return id;
		}
		
	}
}

