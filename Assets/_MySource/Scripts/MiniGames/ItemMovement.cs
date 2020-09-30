using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void ReturnNewLoop(ItemMovement item);

public class ItemMovement : MonoBehaviour {

//	public float boundDistance = 1000;
	public Vector2 direction = Vector2.down;
	public Vector2 speed = new Vector2(0f, 100f);
	public float movedDistance = 0f;
	public bool isMoving = false;
	private Vector3 startPos;

//	public ReturnNewLoop returnNewLoop;

	void Start() {
		startPos = transform.position;
	}


	void Update() {

		if (isMoving) {			
			var movement = new Vector3 (
				speed.x * direction.x,
				speed.y * direction.y,
				0);
			movement *= Time.deltaTime;
			transform.Translate (movement);
			movedDistance += Mathf.Abs(movement.y);
			
//			if (movedDistance >= boundDistance) {
//				transform.position = startPos;
//				movedDistance = 0;
//				returnNewLoop.Invoke (this);
//			}
		}



	}

}
