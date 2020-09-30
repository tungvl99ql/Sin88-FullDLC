using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Server.Api;
using UnityEngine.UI;
using System;

public class PanelAgency : MonoBehaviour
{
    public ReChargeControl reChargeControl;
    public GameObject itemObj;


    private List<Agency> listAgency = new List<Agency>();


    private void OnEnable()
    {
        setDataToTable();
    }
    

    public void setDataToTable()
    {
        listAgency.Clear();
        GetValueAgency(()=> {

            foreach (Transform rtf in itemObj.transform.parent)       //Delete exits element before
            {
                if (rtf.gameObject.name != itemObj.name)
                {
                    Destroy(rtf.gameObject);
                }
            }

            for (int i = 0; i < listAgency.Count; i++)
            {
                GameObject cloneItem = Instantiate(itemObj, itemObj.transform.parent);
                Text[] arrText = cloneItem.gameObject.GetComponentsInChildren<Text>();

                arrText[0].text = listAgency[i]._fullName;
                arrText[1].text = listAgency[i]._nickName;
                arrText[2].text = listAgency[i]._phone;
                arrText[3].text = listAgency[i]._address;
                Button[] arrButton = cloneItem.gameObject.GetComponentsInChildren<Button>();
                if (string.Compare(listAgency[i]._domain, "") == 0)
                {
                    arrButton[0].gameObject.SetActive(false);
                }

                Debug.Log("Link FB : " + listAgency[i]._domain);
                string tempLink = listAgency[i]._domain;
                arrButton[0].onClick.AddListener(()=>OpenUrl(tempLink));

                arrButton[1].onClick.AddListener(()=>LobbyControl.instance.OpenMoneyTransfer(true));
                arrButton[1].onClick.AddListener(()=>reChargeControl.showRecharge(false));


                cloneItem.SetActive(true);
            }
        });
    }


    public void OpenUrl(string tempUrl)
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR
        Application.OpenURL(tempUrl);
#else
        App.openNewTabWindow(urlNotiData);
#endif
    }

    #region AGENCY

    public void GetValueAgency(Action callback = null)
    {
        OutBounMessage req = new OutBounMessage("AGENCY.GET_LIST");
        req.addHead();
        req.writeString(App.getProvider());
        req.writeString(App.languageCode);
        App.ws.send(req.getReq(), (InBoundMessage res) =>
        {
            int sizeListAgency = res.readInt();
            Debug.LogError(sizeListAgency + " size List");

            for (int i = 0; i < sizeListAgency; i++)
            {
                //Agency tempAgency = new Agency(res.readString(), res.readString(), res.readString(), res.readString(), res.readString());

                Agency tempAgency = new Agency();
                tempAgency._fullName = res.readString();
                tempAgency._nickName = res.readString();
                tempAgency._phone = res.readString();
                tempAgency._address = res.readString();
                tempAgency._domain = res.readString();

                listAgency.Add(tempAgency);
            }

            callback.Invoke();
        });
    }

    #endregion
}
