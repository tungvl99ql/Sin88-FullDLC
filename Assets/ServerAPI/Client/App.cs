using UnityEngine;
using System.Collections;

using System;
using UnityEngine.UI;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using DG.Tweening;
using System.Runtime.InteropServices;
using Boo.Lang;
using System.Collections.Generic;

namespace Core.Server.Api
{

    public class App
    {
        public static Regex mRegex = new Regex("[^a-z]|[^0-9]");
        //public static App current;
        //public static WebSocket w;
        public static bool needStartSocket;
        public static MyWebsocketClient ws;
        //public static BestHTTPWSCL ws;
        //public static App current;


        public App()
        {

        }
        public static void start(string tempURI)
        {
            App.ws = new MyWebsocketClient(tempURI);
            //App.ws = new BestHTTPWSCL();
            App.needStartSocket = true;

        }

        public static void trace(object t, string color = "")
        {
            /* if(color == "")
                 Debug.Log(t.ToString());
             else
                 Debug.Log("<color=" + color + ">" + t + "</color>");*/
        }



        public static void printBytesArray(byte[] req)
        {
            String t = "";
            if (req == null || req.Length == 0)
            {
                t = "KHONG CO DATA";
            }
            else
            {
                for (int i = 0; i < req.Length; i++)
                {
                    t += req[i] + ",";
                }


                // t = System.Text.Encoding.UTF8.GetString(req);


            }
            // Debug.Log("IN RA NE" + t);
        }


        public static string getAvatarLink2(string avatar, int avatarId)
        {
            return "fake" + avatarId;
        }
        public static IEnumerator loadImg(Image img, String url, bool toSave = false, bool inScroll = false)
        {
            img.overrideSprite = LoadingControl.instance.ldSpriteList[0];
            if (url.Contains("fake"))
            {
                img.material = null;
                yield return new WaitForSeconds(.1f);
                string t = url.Substring(4);
                int a = 0;
                try
                {
                    a = int.Parse(t);
                }
                catch
                {
                    a = int.Parse(url.Substring(5));
                }
                img.overrideSprite = LoadingControl.instance.avaSpriteLs[a % 30];
                if (toSave)
                {
                    CPlayer.avatarSpriteToSave = LoadingControl.instance.avaSpriteLs[a % 30];
                    CPlayer.fakeAva = true;
                }

                yield break;
            }
            bool isReplaceMater = false;
            if (inScroll == true)
            {
                img.material = null;
                //App.trace("FACCCCCCC");
            }
            else
                isReplaceMater = true;
            WWW www = new WWW(url);
            yield return www;
            yield return new WaitForSecondsRealtime(0.2f);
            //App.trace("ANH = " + url);
            try
            {

                if (www.error == null && www.texture != null)
                {
                    //Texture2D txt2d = _circleImage(www.texture.height, www.texture.width, www.texture.height / 2, www.texture.height / 2, www.texture.height / 2, www.texture);
                    Texture2D txt2d = www.texture;
                    Sprite spr = Sprite.Create(txt2d, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
                    img.overrideSprite = spr;
                    if (toSave)
                        CPlayer.avatarSpriteToSave = spr;
                    /*
                    if(isReplaceMater)
                        img.material = LoadingControl.instance.circleMaterial;*/
                }
                else
                {

                    img.overrideSprite = LoadingControl.instance.ldSpriteList[1];

                }
            }
            catch
            {
                if (img != null)
                {
                    img.overrideSprite = LoadingControl.instance.ldSpriteList[1];
                    if (toSave)
                    {
                        CPlayer.avatarSpriteToSave = LoadingControl.instance.ldSpriteList[1];
                        CPlayer.fakeAva = true;
                    }
                    trace("[ERORRRRRRRRRRRR] Lỗi load ảnh!");
                }
            }
            //yield return img.overrideSprite
        }

        private static Texture2D _circleImage(int h, int w, float r, float cx, float cy, Texture2D sourceTex)
        {

            Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
            Texture2D b = new Texture2D(h, w);
            b.filterMode = FilterMode.Bilinear;
            for (int i = (int)(cx - r); i < cx + r; i++)
            {
                for (int j = (int)(cy - r); j < cy + r; j++)
                {
                    float dx = i - cx;
                    float dy = j - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d <= r)
                        b.SetPixel(i - (int)(cx - r), j - (int)(cy - r), sourceTex.GetPixel(i, j));
                    else
                        b.SetPixel(i - (int)(cx - r), j - (int)(cy - r), Color.clear);
                }
            }
            b.Apply();
            return b;
        }

        public static void showErr(string t, bool isReconnect = false)
        {
            LoadingControl.instance.showProcessing(false);
            if (isReconnect == true)
            {
                LoadingControl.instance.showDisconnectPanel();
                return;
            }
            /*
            CPlayer.erroShowing = true;
            LoadingControl.instance.blackPanel.SetActive(true);
            LoadingControl.instance.errorText.text = t;
            LoadingControl.instance.clientDialog.SetActive(true);
            LoadingControl.instance.cliendDialogAnim.Play("DialogAnim");
            LoadingControl.instance.loadingScene.SetActive(false);
            if (isReconnect)
            {
                PlayerPrefs.SetString("autolog", "true");
                LoadingControl.instance.isReconnect = true;
            }*/

            if (Screen.orientation == ScreenOrientation.Landscape)
                LoadingControl.instance.showError(t);
            else
            {
                LoadingControl.instance.showError2(t);
            }
        }      
                
        public static string formatMoney(string t)
        {
            if (t.Length < 4)
            {
                return t;
            }
            string rs = "";

            int count = t.Length;
            for (int i = count - 1; i > 0; i--)
            {
                if ((count - i) % 3 != 0)
                {
                    rs = t[i] + rs;
                }
                else
                {
                    rs = "," + t[i] + rs;
                }
            }
            return t[0] + rs;
        }

        public static string FormatMoney(object t, bool needSmallerThanTen = false)
        {
            if (needSmallerThanTen)
                if ((int)t < 10)
                    return t.ToString();
            return string.Format("{0:0,0}", t);
        }

        public static string formatNickName(string t, int characterNum)
        {
            string tmp = t;
            if (t.Length > characterNum)
            {
                tmp = t.Substring(0, characterNum) + "...";
            }
            return tmp;
        }

        public static long formatMoneyBack(string t)
        {
            string[] tmp = t.Split(',');
            string str = "";
            foreach (string mstr in tmp)
                str += mstr;
            return long.Parse(str);
        }

        public static string formatMoneyK(float t, bool isSpace = true)
        {
            int m = (int)t;
            if (t < 1000)
                return formatMoney(m.ToString());
            if (t < 1000000)
                return String.Format("{0:0}", t / 1000) + (isSpace ? " K" : "K");
            if (t < 1000000000)
                return String.Format("{0:0}", t / 1000000) + (isSpace ? " M" : "M");
            return "";
        }
        public static string formatMoneyAuto(float t, bool isSpace = true)
        {
            int m = (int)t;
            if (t < 1000)
                return formatMoney(m.ToString());
            if (t < 1000000)
                return String.Format("{0:0}", t / 1000) + (isSpace ? " K" : "K");
            if (t < 1000000000)
                return String.Format("{0:0}", t / 1000000) + (isSpace ? " M" : "M");
            if (t < 1000000000000)
                return String.Format("{0:0}", t / 1000000000) + (isSpace ? " B" : "B");
            return "";
        }
        public static string formatMoneyD(float money, bool isSpace = false)
        {
            int m = (int)money;
            if (money < 1000)
                return formatMoney(m.ToString());
            if (money < 1000000000)
                return String.Format("{0:0,0}", money / 1000) + (isSpace ? " K" : "K");
            //if (money < 1000000000)
            //{
            //    return String.Format("{0:0}", money / 1000000) + (isSpace ? " M" : "M");
            //}
            return "";
        }

        public static IEnumerator CorouChangeNumber(Text txt, int fromNum, int toNum, float tweenTime = 3, float scaleNum = 1.5f, float delay = 0)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            float i = 0.0f;
            float rate = 2.0f / tweenTime;
            txt.transform.DOScale(scaleNum, tweenTime);
            while (i < tweenTime)
            {
                i += Time.deltaTime * rate;
                float a = Mathf.Lerp(fromNum, toNum, i);

                txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
                if (a == toNum)
                {
                    i = tweenTime;
                }
                yield return null;
            }
            //txt.transform.localScale = Vector2.one;
            yield return new WaitForSeconds(.05f);
        }

        public static string formatMoneyD_M(float money, bool isSpace = false)
        {
            int m = (int)money;
            if (money < 1000)
                return formatMoney(m.ToString());
            if (money < 1000000)
                return String.Format("{0:0}", money / 1000) + (isSpace ? " K" : "K");
            if (money < 1000000000)
            {
                return (money / 1000000f) + (isSpace ? " M" : "M");
            }
            return "";
        }
        public static string formatToUserContent(string input)
        {
            /*
             * var br_tag = ~/<br>/gi;
            var p_tag = ~/<p.*>/gi;
            var tag = ~/<(?:.|\s)*?>/g;
            str = br_tag.replace(str, "\n");
            str = p_tag.replace(str, "\n");
            str = tag.replace(str, "");
            return str;
             * */

            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static void ShowConfirm(string content, Action callback)
        {
            LoadingControl.instance.ldTextList[11].text = content;
            LoadingControl.instance.ldBtns[1].onClick.RemoveAllListeners();
            LoadingControl.instance.ldBtns[1].onClick.AddListener(() =>
            {
                callback();
            });
            LoadingControl.instance.ldBtns[1].transform.parent.parent.gameObject.SetActive(true);
        }

        public static void showLoading(bool isShow = true)
        {

        }

        public static void showBlackPanel()
        {

        }
        
        public static string mainUrl = "https://qt.quaytay.vip/active/";


        public static void LogOut()
        {
#if UNITY_WEBGL
        Application.ExternalCall("logout");
        //  reload();
#endif
        }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    public static extern void openWindow(string url);

    [DllImport("__Internal")]
    public static extern void setCookie(string name, string value, int day);

    [DllImport("__Internal")]
    public static extern string getCookie(string name);
    [DllImport("__Internal")]
    public static extern string reload();
    [DllImport("__Internal")]
    public static extern string openNewTabWindow(string url);

#endif



        public static string GetDLCName(string gameName)
        {
            switch (gameName)
            {
#if UNITY_ANDROID
                case "event":
                    return "data-release-android-event.dlc";
                case "mnp":
                    return "data-release-android-mnp.dlc";
                case "xeng":
                    return "data-release-android-xeng.dlc";
                case "ctn":
                    return "data-release-android-avenger.dlc";
                case "zda":
                    return "data-release-android-zda.dlc";
                case "frt":
                    return "data-release-android-dragonball.dlc";
                case "sl7":
                    return "data-release-android-monkeyking.dlc";
                case "main":
                    return "data-release-android.dlc";

                case "birdsvn":
                    return "bird.dlc";
                case "chestsvn":
                    return "chest.dlc";
                case "fishsvn":
                    return "fish.dlc";
                case "lobbysvn":
                    return "lobby.dlc";


                case "birdvn":
                    return "bird_android_vn.dlc";
                case "birdla":
                    return "bird_android_la.dlc";

                case "chestvn":
                    return "chest_android_vn.dlc";
                case "chestla":
                    return "chest_android_la.dlc";

                case "fishvn":
                    return "fish_android_vn.dlc";
                case "fishla":
                    return "fish_android_la.dlc";

                case "lobbyvn":
                    return "lobby_android_vn.dlc";
                case "lobbyla":
                    return "lobby_android_la.dlc";

                case "loading":
                    return "loading_android_vn.dlc";

#elif UNITY_IOS
            case "event":
                return "data-release-ios-event.dlc";
            case "mnp":
                return "data-release-ios-mnp.dlc";
            case "xeng":
                return "data-release-ios-xeng.dlc";
            case "ctn":
                return "data-release-ios-avenger.dlc";
            case "zda":
                return "data-release-ios-zda.dlc";
            case "frt":
                return "data-release-ios-dragonball.dlc";
            case "sl7":
                return "data-release-ios-monkeyking.dlc";
            case "main":
                return "data-release-ios.dlc";
            case "chest":
                return "data-release-ios-chest.dlc";
            case "fish":
                return "data-release-ios-fish.dlc";
            case "bird":
                return "data-release-ios-bird.dlc";

                case "birdsvn":
                    return "bird.dlc";
                case "chestsvn":
                    return "chest.dlc";
                case "fishsvn":
                    return "fish.dlc";
                case "lobbysvn":
                    return "lobby.dlc";


                case "birdvn":
                    return "bird_ios_vn.dlc";
                case "birdla":
                    return "bird_ios_la.dlc";

                case "chestvn":
                    return "chest_ios_vn.dlc";
                case "chestla":
                    return "chest_ios_la.dlc";

                case "fishvn":
                    return "fish_ios_vn.dlc";
                case "fishla":
                    return "fish_ios_la.dlc";

                case "lobbyvn":
                    return "lobby_ios_vn.dlc";
                case "lobbyla":
                    return "lobby_ios_la.dlc";



#endif

            }
            return "";
        }

        //TruongCode+ New Update
        public static string languageCode = "vi-VN";
        public static string appCode ="";
        public static Dictionary<string, string> listKeyText  = new Dictionary<string, string>();
        //

        public static string getBuildTime()
        {
            return "PHIÊN BẢN " + Application.version;
        }

        public static string getDevicePlatform()
        {
#if UNITY_IOS
		return "ios";
#elif UNITY_ANDROID || UNITY_EDITOR
            return "android";
#else
        return "web";
#endif
        }

        public static string getVersion()
        {
            return Application.version;
        }

        public static string getProvider()
        {
#if UNITY_IOS
  return "SIN88_IOS";
#elif UNITY_ANDROID || UNITY_EDITOR
            //return "SIN88_ANDROID";
            return "SIN88_ANDROID_SPLIT";
#else
        return "CHIP68_WEB";
#endif
        }
    }
}
