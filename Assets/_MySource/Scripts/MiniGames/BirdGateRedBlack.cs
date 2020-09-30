using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BirdGateRedBlack : MonoBehaviour {
    Coroutine Wait;
    [SerializeField]
    RectTransform ScreenGameRedBlack;
    [SerializeField]
    Text txtTimeCD;
    [SerializeField]
    public GameObject blackRed;
    // Use this for initialization
    void OnEnable()
    {
        Wait = StartCoroutine(CDAutoOpen());
    }
    IEnumerator CDAutoOpen(int time = 0)
    {
        for (int i = 0; i < time; i++)
        {
            string txt = DictionaryText.instance.TryGetValue(4);
            //txtTimeCD.text = string.Concat(txt, (time - i).ToString());
            yield return new WaitForSeconds(1f);
        }
        if (!ScreenGameRedBlack.gameObject.activeInHierarchy)
        {
           
            blackRed.SetActive(true);
        }
    }
    
}
