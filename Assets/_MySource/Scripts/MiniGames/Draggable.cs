using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino.Core {
	
	public class Draggable : MonoBehaviour {
		public bool canDrag = false;
		private RectTransform mTransform;
		private void Awake()
		{
			mTransform = gameObject.GetComponent<RectTransform>();
		}
		// Update is called once per frame
		void Update()
		{
			if (canDrag)
			{
				transform.position = Input.mousePosition;
				float y = transform.position.y;
				float x = transform.position.x;

				if (y >= Screen.height)
				{
					y = Screen.height - 1f * mTransform.rect.width;

				}
				if (y <= 0)
				{
					y = 1f * mTransform.rect.width;
				}
				if (x > Screen.width)
				{
					x = Screen.width - 1f * mTransform.rect.width;
				}
				if (x < 0)
				{
					x = 1f * mTransform.rect.width;
				}
				Vector3 mPos = new Vector3(x, y);
				transform.position = mPos;
			}
		}
	}
}

