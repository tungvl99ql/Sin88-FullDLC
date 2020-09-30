using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LinkUGUIText : TextMeshProUGUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler
{

//public string stakeholderLink = @"<#40A0FF><link="id_01"><i>Insert link text here</i></link></color>";

    public OnLinkClickEvent onClicked = new OnLinkClickEvent();

    private TMP_Text m_TextComponent;
    TextMeshProUGUI m_TextMeshPro;

    private new void  Awake()
    {
        base.Awake();
        m_TextComponent = gameObject.GetComponent<TMP_Text>();
        m_TextMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public new void SetText(string content) {
        var regex = new Regex(@"\b(?:https?://|www\.)\S+\b");
        MatchCollection matchColl = regex.Matches(content);

        foreach (Match matched in matchColl)
        {
            string stakeholderLink = string.Format("<#40A0FF><u><link={0}><i>{1}</i></u></link></color>", matched.Value, matched.Value);
            content = content.Replace(matched.Value, stakeholderLink);
        }        
        base.SetText(content);
        ForceMeshUpdate();

    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(" ================ Link text clicked ================== !!!");

        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, Input.mousePosition, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];

            Debug.Log(" Link = " + linkInfo.GetLinkID());
            onClicked.Invoke(linkInfo.GetLinkID());
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("OnPointerEnter()");
        //isHoveringObject = true;
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("OnPointerExit()");
        //isHoveringObject = false;
    }


    protected override void OnDestroy()
    {
        onClicked.RemoveAllListeners();
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("OnPointerUp()");
    }

}


public class OnLinkClickEvent: UnityEvent<string> {    
    
}
