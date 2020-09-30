using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Casino.Games.BxB {

	public class ScrollingBackground : MonoBehaviour
	{
		public bool IsPlaying {
			get { 
				return isPlay;	
			}
			set {
				if (value) {
					if (StartRollEvent != null) {
						StartRollEvent.Invoke ();
					}
				} else {
					if (EndRollEvent != null) {
						EndRollEvent.Invoke ();
					}
				}
				isPlay = value;
			}
		}

		public UnityEvent StartRollEvent;
		public UnityEvent SendRequestEvent;
		public UnityEvent EndRollEvent;

		public int loopTimes = 0;
		
		public int loopTimesGoal = 10;
		public int beforeLoopTimesGoal = 1;
		public int loopForSendRequest = 2;

		public bool willStopNextLoop = false;
		
		Vector3[] fixedPosArr = new Vector3[3];
		
		[SerializeField] RectTransform maskPanel;

		private Vector3 originPos;
		private Vector3[] childOriginPos = new Vector3[15];
		
		public Vector2 offset = new Vector2 (10f, 10f);
		
		/// <summary>
		/// Scrolling speed
		/// </summary>
		public Vector2 speed = new Vector2 (10, 10);
		
		/// <summary>
		/// Moving direction
		/// </summary>
		public Vector2 direction = new Vector2 (-1, 0);


		public Vector3 movement;

		/// <summary>
		/// 1 - Background is infinite
		/// </summary>
		public bool isLooping = false;
		private int childCount = 0;
		private bool isPlay;
		
		/// <summary>
		/// 2 - List of children with a renderer.
		/// </summary>
		private List<Transform> backgroundPart;
		
		
		// 3 - Get all the children
		void Start ()
		{

			originPos = transform.position;
			//LerpToStop ();
			
			// For infinite background only
			if (isLooping) {
				// Get all the children of the layer with a renderer
				backgroundPart = new List<Transform> ();
				
				for (int i = 0; i < transform.childCount; i++) {
					Transform child = transform.GetChild (i);
					
					// Add only the visible children
					if (child.GetComponent<Image> () != null) {
						backgroundPart.Add (child);
					}
				}
				
				// Sort by position.
				// Note: Get the children from left to right.
				// We would need to add a few conditions to handle
				// all the possible scrolling directions.
				backgroundPart = backgroundPart.OrderBy (
					t => t.position.y
				).ToList ();
				for (int i = 0; i < childOriginPos.Length; i++) {
					childOriginPos [i] = backgroundPart [i].position;
				}
			}
			
			offset = (backgroundPart [1].position - backgroundPart [0].position);
			for (int i = 0; i < fixedPosArr.Length; i++) {
				fixedPosArr [i] = backgroundPart [i].transform.position;
			}
			
		}

		public void Update ()
		{
			if (IsPlaying && !(loopTimes == loopTimesGoal)) {
//			if (IsPlaying) {
				
				// Movement
				movement = new Vector3 (
					speed.x * direction.x,
					speed.y * direction.y,
					0);
				


				
				movement *= Time.deltaTime;
				transform.Translate (movement);
				
				// 4 - Loop
				if (isLooping) {
					// Get the first object.
					// The list is ordered from left (x position) to right.
					Transform firstChild = backgroundPart.FirstOrDefault ();
					
					if (firstChild != null) {
						// Check if the child is already (partly) before the camera.
						// We test the position first because the IsVisibleFrom
						// method is a bit heavier to execute.
						if (firstChild.position.y < maskPanel.transform.position.y) {
							
							
							
							// If the child is already on the left of the camera,
							// we test if it's completely outside and needs to be
							// recycled.
							if (!RectTransformUtility.RectangleContainsScreenPoint (maskPanel, Camera.main.WorldToScreenPoint (firstChild.position), Camera.main)) {
								childCount++;
								
								if (childCount == backgroundPart.Count) {
									childCount = 0;
									loopTimes++;
									if (!willStopNextLoop) {
										loopTimesGoal++;
									}
									if (loopTimes == loopTimesGoal - beforeLoopTimesGoal) {
										IsPlaying = false;
									}

									if (loopTimes == loopForSendRequest) {
										if (SendRequestEvent != null) {
											SendRequestEvent.Invoke ();
											Debug.Log ("Send Server request");
										}
									}

								}
								// Get the last child position.
								Transform lastChild = backgroundPart.LastOrDefault ();
								Vector3 lastPosition = lastChild.transform.position;
								Vector3 lastSize = (lastChild.GetComponent<RectTransform> ().rect.max);
								
								// Set the position of the recyled one to be AFTER
								// the last child.
								// Note: Only work for horizontal scrolling currently.
								//							firstChild.position = new Vector3 (lastPosition.x + lastSize.x + offset.x, firstChild.position.y + offset.y, firstChild.position.z);
								firstChild.position = new Vector3 (firstChild.position.x + offset.x , lastPosition.y + lastSize.y + offset.y, firstChild.position.z);
								
								// Set the recycled child to the last position
								// of the backgroundPart list.
								backgroundPart.Remove (firstChild);
								backgroundPart.Add (firstChild);
							}
						}
					}
				}
			}			
			
		}


		private void reset() {
			loopTimes = 0;
		}


        public void Spin(bool isFastMode) {

            if (isFastMode)
            {
                speed = new Vector2(0, 1000f);
            } else
            {
                speed = new Vector2(0, 500f);
            }
            IsPlaying = true;
			reset ();
		}

		public void Stop() {
			IsPlaying = false;
		}
		
		
	}
}

