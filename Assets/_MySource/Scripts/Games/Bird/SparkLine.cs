using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SparkLine : MonoBehaviour {

    private RectTransform rectTransform;
   // public GameObject testGameObject;
    //public GameObject testTargetGameObject;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

    }



    private void Start()
    {
        // test
     //  Draw(testGameObject.transform.position, testTargetGameObject.transform.position);
    }


    public void Draw(Vector3 startPos, Vector3 endPos) {


        gameObject.transform.position = startPos;
        //TODO: get angle to OY
        var v = endPos - startPos;
        float angle = Vector3.Angle(v, Vector3.right) * (((startPos.y - endPos.y) > 0) ? -1 : 1 );
        

        //Debug.Log(angle);


        rectTransform.localEulerAngles = new Vector3(rectTransform.localEulerAngles.x, rectTransform.localEulerAngles.y, angle);
        //Color : 00FF61FF
        //TODO: scale rect  
        float factor = Mathf.Abs(rectTransform.rect.width);

        float correctScale = Vector3.Distance(startPos, endPos)*rectTransform.localScale.x;

        rectTransform.localScale = new Vector3( correctScale, rectTransform.localScale.y, rectTransform.localScale.z);

     
        
    }

}
