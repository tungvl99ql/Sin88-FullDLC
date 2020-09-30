using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using Casino.Games.BxB;

public class ItemMoveContainer : MonoBehaviour
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


	public Vector2 normalSpeed;
	public Vector2 fastSpeed;

	public int loopTimes;
	public int loopTimesGoal = 1;
	private int loopTimesGoalOfSpin;
	public int beforeLoopTimesGoal = 1;
	public int loopForSendRequest = 2;
	public bool willStopNextLoop = false;

	public RectTransform maskPanel;

	public Vector2 direction = Vector2.down;
	public Vector2 speed = new Vector2 (0f, 100f);

	public Vector2 offset;

	private bool isPlay;
	private List<Transform> backgroundPart;
	private int itemCount = 0;
	private Vector3[] originalPos;
    private void OnEnable()
    {
        InitPos();
    }

    void Start ()
	{
		backgroundPart = new List<Transform> ();

		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);

			// Add only the visible children
			backgroundPart.Add (child);
//			if (child.GetComponent<Image> () != null) {
				//backgroundPart.Add (child);
//			}
		}
		backgroundPart = backgroundPart.OrderBy (
			t => t.localPosition.y
		).ToList ();

		// test

        originalPos = new Vector3[backgroundPart.Count];
        for (int i = 0; i < backgroundPart.Count; i++)
        {
            BXBCharContainer t = backgroundPart[i].GetComponent<BXBCharContainer>();
            if (t != null)
            {
                t.index = i;
            }
            originalPos[i] = backgroundPart[i].localPosition;

        }

        offset = backgroundPart [1].localPosition - backgroundPart [0].localPosition;
		loopTimesGoalOfSpin = loopTimesGoal;
	}

    public void InitPos()
    {
        if (backgroundPart != null)
        {
            backgroundPart = backgroundPart.OrderBy(
                t => t.GetComponent<BXBCharContainer>().index
            ).ToList();
        }
       
    }
    void Update ()
	{

		if (IsPlaying && (loopTimes < loopTimesGoal)) {
			
			var movement = new Vector3 (
				speed.x * direction.x,
				speed.y * direction.y,
				0);
			movement *= Time.deltaTime;
			foreach (var item in backgroundPart) {
				item.Translate (movement);
			}
			
			Transform firstChild = backgroundPart.FirstOrDefault ();
			
			if (firstChild != null) {
				
				if (firstChild.localPosition.y < maskPanel.transform.localPosition.y) {
					
					if (!RectTransformUtility.RectangleContainsScreenPoint (maskPanel, Camera.main.WorldToScreenPoint (firstChild.position), Camera.main)) {

						itemCount++;
						if (itemCount >= backgroundPart.Count) {
							loopTimes++;
							itemCount = 0;

							if (!willStopNextLoop) {
								loopTimesGoal++;
							}
							
							if (loopTimes == loopTimesGoal - beforeLoopTimesGoal) {
								IsPlaying = false;
							}
							
							if (loopTimes == loopForSendRequest) {
								if (SendRequestEvent != null) {
									SendRequestEvent.Invoke ();
									//Debug.Log ("Send Server request");
								}
							}
							
						}

						Transform lastChild = backgroundPart.LastOrDefault ();
						Vector3 lastPosition = lastChild.transform.localPosition;
//						Vector3 lastSize = (lastChild.GetComponent<RectTransform> ().rect.max);
						
						firstChild.localPosition = new Vector3 (firstChild.localPosition.x, lastPosition.y + offset.y, firstChild.localPosition.z);
						
						backgroundPart.Remove (firstChild);
						backgroundPart.Add (firstChild);
					}
				}
				
			}
		}
		else {
			reset ();
			resetPos ();
		}
		

	}


	private void resetPos() {
		for (int i = 0; i < backgroundPart.Count; i++) {
			backgroundPart [i].localPosition = originalPos [i];
		}
	}

	private void reset() {
		willStopNextLoop = false;
		loopTimes = 0;
		loopTimesGoal = loopTimesGoalOfSpin;
		itemCount = 0;
	}

    public void ForceStop() {
        IsPlaying = false;
    }


	public void Spin(bool isFastMode) {

		//reset ();
		if (isFastMode)
		{
			speed = fastSpeed;
		} else
		{
			speed = normalSpeed;
		}
		IsPlaying = true;
	}

	public void Stop() {
		IsPlaying = false;
	}
}
