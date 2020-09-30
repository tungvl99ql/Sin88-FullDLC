using DG.Tweening;
using Facebook.Unity;
using Core.Server.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreeChipControl : MonoBehaviour {

    /// <summary>
    /// 0-2: exe
    /// </summary>
    public Button[] btns;
    /// <summary>
    /// 0: btn-yel|1: btn-grey
    /// </summary>
    public Sprite[] sprts;
    
    /// <summary>
    /// 0: haeder|1: body|2: back
    /// </summary>
    public RectTransform[] rtfs;

    private string fbUesrId = "";

    public void Close()
    {
        fbUesrId = "";
        DOTween.To(() => rtfs[1].anchoredPosition, x => rtfs[1].anchoredPosition = x, new Vector2(1500, 37), .35f);
        DOTween.To(() => rtfs[0].anchoredPosition, x => rtfs[0].anchoredPosition = x, new Vector2(0, 160), .35f);
        DOTween.To(() => rtfs[2].anchoredPosition, x => rtfs[2].anchoredPosition = x, new Vector2(0, 160), .35f).OnComplete(()=>{
            gameObject.SetActive(false);
            LobbyControl.instance.OpenFreeChip(false);
        });
    }

    private void OnEnable()
    {
        DOTween.To(() => rtfs[0].anchoredPosition, x => rtfs[0].anchoredPosition = x, new Vector2(0, -45), .35f);
        DOTween.To(() => rtfs[1].anchoredPosition, x => rtfs[1].anchoredPosition = x, new Vector2(0, 37), .35f);
        DOTween.To(() => rtfs[2].anchoredPosition, x => rtfs[2].anchoredPosition = x, new Vector2(0, 0), .35f);


        if (CPlayer.phoneNum.Length > 6)
        {
            btns[0].gameObject.SetActive(false);
        }
        else
        {
            btns[0].gameObject.SetActive(true);
            btns[0].image.sprite = sprts[0];
        }
    }

    public void DoAuthen()
    {
        LoadingControl.instance.DoAuthen();
        /*
        App.ShowConfirm("- Xác thực tài khoản để kích hoạt tính năng đổi thưởng.\n- Bảo mật tài khoản và lấy lại mật khẩu.\n- Tặng ngay 1, 000 Gold khi xác thực thành công.\n(*) Phí: 1000 vnđ", delegate() {
            string url = "sms:7069?body=CGV XT " + CPlayer.id;
            Application.OpenURL(url);
        });
        */
    }


    #region //SHARE FB

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
            fbUesrId = result.AccessToken.UserId;
            ShareFacebook();
        }
        else
        {
            Debug.Log("User cancelled login");
            App.trace("Bạn chưa đăng nhập FB");
        }
    }

    private void AuthCallback2(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
            fbUesrId = result.AccessToken.UserId;
            InviteFacebook();
        }
        else
        {
            Debug.Log("User cancelled login");
            App.trace("Bạn chưa đăng nhập FB");
        }
    }

    public void ShareFacebook()
    {
        if (!FB.IsLoggedIn)
        {
            FB.LogOut();

            var perms = new List<string>() { "public_profile", "email", "user_friends" };
            FB.LogInWithReadPermissions(perms, AuthCallback);
            return;
        }

        var req_SHARE = new OutBounMessage("SOCIAL.VALIDATE_FB_SHARING");
        req_SHARE.addHead();
        req_SHARE.writeAcii(App.getProvider());
        App.ws.send(req_SHARE.getReq(), delegate (InBoundMessage res_SHARE)
        {
            string link = res_SHARE.readString();
            string title = res_SHARE.readString();
            string image = res_SHARE.readString();
            string des = res_SHARE.readString();
            App.trace("link  =" + link + "|title = " + title + "|image = " + image + "|des = " + des);

            _ShareFacebook(link, title, des, image);

        });
    }

    private void _ShareFacebook(string link, string title, string des, string photoUrl)
    {
        FB.ShareLink(
            new Uri(link)
            , title, des, new Uri(photoUrl),
            callback: ShareCallback
        );
    }

    private void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
        {
            App.trace("ShareLink Error: " + result.Error, "red");

            
            //App.showErr("Share không thành công! ");
            App.showErr(App.listKeyText["SHARE_NOT_SUCCESS"]);
        }
        else if (!String.IsNullOrEmpty(result.PostId))
        {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
            //App.showErr("Share không thành công! ");
        }
        else
        {
            // Share succeeded without postID
            App.trace("ShareLink success!");
            var req = new OutBounMessage("SOCIAL.COMPLETE_FB_SHARING");
            req.addHead();
            req.writeString(this.fbUesrId);
            App.ws.send(req.getReq(), delegate (InBoundMessage res)
            {
                App.showErr(res.readString());
                //LoadingControl.instance.loadingScene.SetActive(false);
            });
        }
    }

    #endregion

    #region //INVITE FB
    public void InviteFacebook()
    {
        //App.showErr("Đang phát triển");
        App.showErr(App.listKeyText["IN_DEVELOPING"]);

        /*
        if (!FB.IsLoggedIn)
        {
            FB.LogOut();

            var perms = new List<string>() { "public_profile", "email", "user_friends" };
            FB.LogInWithReadPermissions(perms, AuthCallback2);
            return;
        }
        */

    }
    #endregion
}
