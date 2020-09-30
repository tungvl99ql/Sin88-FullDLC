using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using Core.Server.Api;

public class LoopText : MonoBehaviour {

    public RectTransform maskPanel;

    public Vector2 direction = Vector2.down;
    public Vector2 speed = new Vector2(0f, 100f);

    public Vector2 offset;
    private string[] lineContents;
    private List<Transform> backgroundPart;


    void Start()
    {
        backgroundPart = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            backgroundPart.Add(child);
        }
        backgroundPart = backgroundPart.OrderBy(
            t => t.localPosition.x
        ).ToList();

      
    }


    void Update()
    {

        if (true)
        {

            var movement = new Vector3(
                speed.x * direction.x,
                speed.y * direction.y,
                0);
            movement *= Time.deltaTime;
            foreach (var item in backgroundPart)
            {
                item.Translate(movement);
            }

            Transform firstChild = backgroundPart.FirstOrDefault();

            if (firstChild != null)
            {

                if (firstChild.localPosition.x < maskPanel.transform.localPosition.x)
                {

                    if (!RectTransformUtility.RectangleContainsScreenPoint(maskPanel, Camera.main.WorldToScreenPoint(firstChild.position), Camera.main))
                    {
                                                
                        Transform lastChild = backgroundPart.LastOrDefault();
                        Vector3 lastPosition = lastChild.transform.localPosition;

                        //firstChild.localPosition = new Vector3(firstChild.localPosition.x, lastPosition.y + offset.y + lastSize.x*2, firstChild.localPosition.z);
                        if (CPlayer.logedIn)
                        {
                            var shouldChangeText = firstChild.GetComponent<Text>();
                            changeTextContent(shouldChangeText);
                        }  
                        float lastSize = (lastChild.GetComponent<RectTransform> ().rect.width);
                        firstChild.localPosition = new Vector3(lastPosition.x + offset.x + lastSize + 100, firstChild.localPosition.y, firstChild.localPosition.z);
                        backgroundPart.Remove(firstChild);
                        backgroundPart.Add(firstChild);

                    }
                }

            }
        }

    }


    public void changeTextContent(Text beChangeText) {
        var l = lineContents.Length;
        beChangeText.text = lineContents[Random.Range(0, l-1)];
    }
        

    public void OnLoginSuccess(string[] lineList)
    {
   
        
        lineContents = lineList;
    }

   
}
