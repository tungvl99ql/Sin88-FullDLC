using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatherDrag : MonoBehaviour
{
    public static FatherDrag instance;
    public bool canDrag = false;
    private void Awake()
    {
        instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        if (canDrag)
        {
            transform.position = Input.mousePosition;
            float y = transform.position.y;
            float x = transform.position.x;
            RectTransform mTransform = gameObject.GetComponent<RectTransform>();
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

            //App.trace("Y = " + y + "|X" + x);
            Vector3 mPos = new Vector3(x, y);
            transform.position = mPos;
        }
    }
}
