using DG.Tweening;
using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingControl : MonoBehaviour {

    /// <summary>
    /// 0:main| 1: sound|2: sound_bg|
    /// </summary>
    public RectTransform[] settingRtfs;
    public GameObject gojsExit;
    //0: sound|1: sound_bg|2: 
    public Image[] imgs;

    /// <summary>
    /// 0-1: sound on-off|
    /// </summary>
    public Sprite[] sprts;

    /// <summary>
    /// 0: mute sound|1: mute bg sound
    /// </summary>
    private bool[] boolLs = new bool[3];

    private void OnEnable()
    {
       // boolLs[1]=boolLs[0] = true;
        boolLs[0] = PlayerPrefs.GetInt("mute", 0) == 1;
        boolLs[1] = PlayerPrefs.GetInt("mute_bg", 0) == 1;
        LoadingControl.instance.bandInvitation = PlayerPrefs.GetInt("invite", 0) == 1;

        settingRtfs[1].anchoredPosition = new Vector2(boolLs[0] ? 27 : 81, 0);
        settingRtfs[2].anchoredPosition = new Vector2(boolLs[1] ? 27 : 81, 0);

        settingRtfs[3].anchoredPosition = new Vector2(LoadingControl.instance.bandInvitation ? 27 : 81, 0);

        imgs[0].sprite = sprts[boolLs[0] ? 1 : 0];
        imgs[1].sprite = sprts[boolLs[1] ? 1 : 0];
        imgs[2].sprite = sprts[LoadingControl.instance.bandInvitation ? 1 : 0];

        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            gojsExit.SetActive(false);
        }
        else
        {
            gojsExit.SetActive(true);

        }
        settingRtfs[0].anchoredPosition = new Vector2(520, 136);
        DOTween.To(() => settingRtfs[0].anchoredPosition, x => settingRtfs[0].anchoredPosition = x, new Vector2(0,136), .25f);

    }

    public void Close()
    {
        PlayerPrefs.SetInt("mute", boolLs[0] ? 1 : 0);
        PlayerPrefs.SetInt("mute_bg", boolLs[1] ? 1 : 0);
        PlayerPrefs.SetInt("invite", LoadingControl.instance.bandInvitation?1:0);
        SoundManager.instance.PlayUISound(SoundFX.SL7_CLICK);
        gameObject.SetActive(false);
    }

    public void DoAction(string type)
    {
        switch (type)
        {
            case "top":

                LobbyControl.instance.OpenChart(true);
                break;
            case "logout":

                AssetBundle[] bundles = Resources.FindObjectsOfTypeAll<AssetBundle>();
                for (int i = 0; i < bundles.Length; i++)
                {
                    bundles[i].Unload(true);
                }
                //AssetBundle.UnloadAllAssetBundles(true);

                DOTween.KillAll();
                StopAllCoroutines();
                if(CPlayer.showEvent)
                    LoadingControl.instance.loadingGojList[29].SetActive(false);
                LoadingControl.instance.loadingGojList[21].SetActive(false);
                CPlayer.Clear();
                App.ws.getWS().Logout();
                SceneManager.LoadScene("Main");
                Destroy(LoadingControl.instance.gameObject);
                
                break;
            case "reply":
                LoadingControl.instance.showPmPanel("admin", "PHẢN HỒI, GÓP Ý");
                break;
            case "changeSound":
                boolLs[0] = !boolLs[0];
                if (boolLs[0])
                {
                    SoundManager.instance.isEnableMusic = false;
                }
                else
                {
                    SoundManager.instance.isEnableMusic = true;
                }
                //LoadingControl.instance.ChangeSound(0, boolLs[0]);
                DOTween.To(() => settingRtfs[1].anchoredPosition, x => settingRtfs[1].anchoredPosition = x, new Vector2(boolLs[0] ? 27 : 81, 0), .1f).OnComplete(()=> {
                    imgs[0].sprite = sprts[boolLs[0] ? 1 : 0];
                });
                break;
            case "changeB_gSound":
                boolLs[1] = !boolLs[1];
                LoadingControl.instance.ChangeSound(1, boolLs[1]);
                if (boolLs[1])
                {
                    //LoadingControl.instance.PlaySound("stop");
                    SoundManager.instance.isEnableBGM = false;
                }
                else
                {
                    //LoadingControl.instance.PlaySound("play");
                    SoundManager.instance.isEnableBGM = true;
                }
                DOTween.To(() => settingRtfs[2].anchoredPosition, x => settingRtfs[2].anchoredPosition = x, new Vector2(boolLs[1] ? 27 : 81, 0), .1f).OnComplete(() => {
                    imgs[1].sprite = sprts[boolLs[1] ? 1 : 0];
                });
                break;
            case "changeInvite":
                LoadingControl.instance.bandInvitation = !LoadingControl.instance.bandInvitation;
                
                DOTween.To(() => settingRtfs[3].anchoredPosition, x => settingRtfs[3].anchoredPosition = x, new Vector2(LoadingControl.instance.bandInvitation ? 27 : 81, 0), .1f).OnComplete(() => {
                    imgs[2].sprite = sprts[LoadingControl.instance.bandInvitation ? 1 : 0];
                    PlayerPrefs.SetInt("invite", LoadingControl.instance.bandInvitation ? 1 : 0);
                });
                break;
            case "changeFx":
                boolLs[2] = !boolLs[2];
                break;
        }
        if(type != "changeSound" && type != "changeB_gSound" && type != "changeFx" && type != "changeInvite")
            Close();
    }
}
