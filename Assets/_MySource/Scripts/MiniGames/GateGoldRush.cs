using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Server.Api
{

    public class GateGoldRush : MonoBehaviour
    {
        Coroutine Wait;
        [SerializeField]
        RectTransform ScreenGameRush;
        [SerializeField]
        Text txtTimeCD;
        [SerializeField]
        GoldRush script;
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
            if (!ScreenGameRush.gameObject.activeInHierarchy)
            {
                script.OpenGoldRush();
            }
        }

    }
}