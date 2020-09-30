using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LobbyDownloadDLC : MonoBehaviour
{
    public GameObject[] btn; //0 chest , 1 bird , 2 fish , 3 phom, 4 maubinh, 5 tlmn,6 poker, 7 xocdia
    public Slider[] slider; //0 chest , 1 bird , 2 fish , 3 phom, 4 maubinh, 5 tlmn, 6 poker, 7 xoc dia

    private void Start()
    {
        //PlayerPrefs.DeleteAll();
        //check_downloadDLC();
        checkUpdate();
    }
    public void DOWNLOAD(int n)
    {
        switch(n)
        {
            case 1: // tai chest
                StartCoroutine(dowload_loading("chest", 0,0));
                break;
            case 2: // tai bird
                StartCoroutine(dowload_loading("bird", 7,1));
                break;
            case 3: // tai fish
                StartCoroutine(dowload_loading("fish", 8,2));
                break;
            case 4: // tai phom
                StartCoroutine(dowload_loading("phom", 2, 3));
                break;
            case 5: // tai maubinh
                StartCoroutine(dowload_loading("maubinh", 4,4));
                break;
            case 6: // tai tlmn
                StartCoroutine(dowload_loading("tlmn", 9, 5));
                break;
            case 7: // tai poker
                StartCoroutine(dowload_loading("poker", 1, 6));
                break;
            case 8: // tai xoc dia
                StartCoroutine(dowload_loading("xocdia", 5, 7));
                break;
        }
        
    }
    void check_downloadDLC()
    {
        string path_bird = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "bird_android_vn.dlc";
        string path_chest = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "chest_android_vn.dlc";
        string path_fish = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "fish_android_vn.dlc";
        string path_phom = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "phom_android_vn.dlc";
        string path_maubinh = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "maubinh_android_vn.dlc";
        string path_tlmn = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "tlmn_android_vn.dlc";
        string path_poker = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "poker_android_vn.dlc";
        string path_xocdia = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + "xocdia_android_vn.dlc";
        if (File.Exists(path_chest))
        {
            Debug.Log("chest da ton tai");
            btn[0].SetActive(false);
        }else
        {
            btn[0].SetActive(true);
        }
        if (File.Exists(path_bird))
        {
            Debug.Log("bird da ton tai");
            btn[1].SetActive(false);
        }else
        {
            btn[1].SetActive(true);
        }
        if (File.Exists(path_fish))
        {
            Debug.Log("fish da ton tai");
            btn[2].SetActive(false);
        }else
        {
            btn[2].SetActive(true);
        }
        if (File.Exists(path_phom))
        {
            Debug.Log("fish da ton tai");
            btn[3].SetActive(false);
        }else
        {
            btn[3].SetActive(true);
        }
        if (File.Exists(path_maubinh))
        {
            Debug.Log("fish da ton tai");
            btn[4].SetActive(false);
        }else
        {
            btn[4].SetActive(true);
        }
        if (File.Exists(path_tlmn))
        {
            Debug.Log("fish da ton tai");
            btn[5].SetActive(false);
        }else
        {
            btn[5].SetActive(true);
        }
        if (File.Exists(path_poker))
        {
            btn[6].SetActive(false);
        }
        else
        {
            btn[6].SetActive(true);
        }
        if (File.Exists(path_xocdia))
        {
            btn[7].SetActive(false);
        }else
        {
            btn[7].SetActive(true);
        }
    }
    IEnumerator dowload_loading(string scene,int index,int btn_index)
    {
        string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
               + "/DLC/" + scene +"_android_vn.dlc";

        Directory.CreateDirectory(((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
          + "/DLC/");


        string url = /*"https://tungahihi.imfast.io/loading.dlc"*/ LoadDLC.instance.ListUrlDownLoadDLC[index];
          WWW www = new WWW(url);
            while (!www.isDone)
            {
                Debug.Log(www.progress * 100);
            slider[btn_index].value = www.progress * 100;
                yield return null;
            }
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.LogError(www.error);
                yield break;
            }
#if !UNITY_WEBPLAYER
            if (www.isDone)
            {
                Debug.LogError("tai xong loading");
                File.WriteAllBytes(path, www.bytes);
                btn[btn_index].SetActive(false);
                PlayerPrefs.SetString(scene, LoadDLC.instance.timeList[index]);
                Debug.LogError(PlayerPrefs.GetString(scene) + "--" + btn_index);

        }
#endif

        //=======================================

    }

    void checkUpdate()
    {
        if(PlayerPrefs.GetString("chest") == LoadDLC.instance.timeList[0].ToString())
        {
            btn[0].SetActive(false);
        }else
        {
            btn[0].SetActive(true);
        }
        //
        if (PlayerPrefs.GetString("bird") == LoadDLC.instance.timeList[7].ToString())
        {
            btn[1].SetActive(false);
        }
        else
        {
            btn[1].SetActive(true);
        }
        //
        if (PlayerPrefs.GetString("fish") == LoadDLC.instance.timeList[8].ToString())
        {
            btn[2].SetActive(false);
        }
        else
        {
            btn[2].SetActive(true);
        }
        //
        if (PlayerPrefs.GetString("phom") == LoadDLC.instance.timeList[2].ToString())
        {
            btn[3].SetActive(false);
        }
        else
        {
            btn[3].SetActive(true);
        }
        //
        if (PlayerPrefs.GetString("maubinh") == LoadDLC.instance.timeList[4].ToString())
        {
            btn[4].SetActive(false);
        }
        else
        {
            btn[4].SetActive(true);
        }
        //
        if (PlayerPrefs.GetString("tlmn") == LoadDLC.instance.timeList[9].ToString())
        {
            btn[5].SetActive(false);
        }
        else
        {
            btn[5].SetActive(true);
        }
        //
        if (PlayerPrefs.GetString("poker") == LoadDLC.instance.timeList[1].ToString())
        {
            btn[6].SetActive(false);
        }
        else
        {
            btn[6].SetActive(true);
        }
        //
        if (PlayerPrefs.GetString("xocdia") == LoadDLC.instance.timeList[5].ToString())
        {
            btn[7].SetActive(false);
        }
        else
        {
            btn[7].SetActive(true);
        }
    }


}
