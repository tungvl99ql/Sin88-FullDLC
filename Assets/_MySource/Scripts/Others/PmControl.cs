using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PmControl : MonoBehaviour {

    public InputField[] pmIpfs;

    private void OnEnable()
    {
        LoadingControl.instance.loadingGojList[3].SetActive(true);
        transform.DOScale(1, .25f).SetEase(Ease.OutBack);
    }

    public void Close()
    {
        pmIpfs[0].text = "";
        transform.DOScale(.5f, .25f).SetEase(Ease.InBack).OnComplete(()=> {
            gameObject.SetActive(false);
            LoadingControl.instance.loadingGojList[3].SetActive(false);
        });
    }

    public void DoSend()
    {
        string t = pmIpfs[0].text;
        if (t.Length == 0)
        {
            return;
        }
        var req_FEEDBACK = new OutBounMessage("FEEDBACK");
        req_FEEDBACK.addHead();
        req_FEEDBACK.writeString(t);
        App.ws.send(req_FEEDBACK.getReq(), delegate (InBoundMessage res_FEEDBACK)
        {
            Close();
            //App.showErr("Góp ý của bạn đã được gửi đến BQT. Cảm ơn bạn rất nhiều, chúc bạn có những phút giây vui vẻ");
            App.showErr(App.listKeyText["INFO_FEEDBACK_SUCCESS"]);
        });
    }
}
