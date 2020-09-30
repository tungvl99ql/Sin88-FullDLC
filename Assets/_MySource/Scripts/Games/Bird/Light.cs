using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Core.Bird
{
    public class Light : MonoBehaviour {


        public void Draw(Vector3 position1, Vector3 position2)
        {

            Vector2 tmp = position2 - position1;
            float distance = Vector2.Distance(position1, position2);
            Debug.Log("distance = " + distance);
          //  float angle = Vector2.Angle(position1, position2);
          //  Debug.Log("angle = " + angle);
            float angle = Mathf.Atan2(-tmp.y, tmp.x);
            
            gameObject.GetComponent<RectTransform>().anchoredPosition = position1;
            gameObject.transform.rotation = Quaternion.AngleAxis(angle*Mathf.Rad2Deg, Vector3.forward);
            gameObject.GetComponent<ParticleSystem>().startRotation =angle;


            float sizeScale = distance / gameObject.GetComponent<RectTransform>().rect.width;
            Debug.Log("sizeScale = " + sizeScale);
            gameObject.GetComponent<ParticleSystem>().startSize = sizeScale;
           // gameObject.GetComponent<RectTransform>().localScale = new Vector3(sizeScale, 1, 1);

        }

     
    }
}