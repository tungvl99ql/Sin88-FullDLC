using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Core.Server.CardGame;

namespace Core.Server.Api
{


    public partial class LobbyControl : MonoBehaviour
    {
        public AssetBundle[] asbs = new AssetBundle[1];
        public void enterZone(string zone)
        {
            //StartCoroutine(LoadingControl.instance._start());
            if (CPlayer.logedIn == false)
            {
                showLogPanel("log");
                return;
            }
           
            if (zone == "")
            {
                App.showErr(App.listKeyText["INFO_UPCOMING"]);
                //App.showErr("Sắp ra mắt");

                return;
            }
            if (zone == "tlmn")
            {
                CPlayer.gameName = 1.ToString();
                CPlayer.gameNameFull = App.listKeyText["TLMN_NAME_FULL"].ToUpper() ;//"TIẾN LÊN MIỀN NAM";
                //LoadingUIPanel.Show();
                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());

                return;
            }
            if (zone == "xocdia")
            {
                CPlayer.gameName = "xocdia";
                Debug.LogError(1);
                CPlayer.gameNameFull = App.listKeyText["XOCDIA_NAME"].ToUpper();//"XÓC ĐĨA";
                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());

                return;
            }
            if (zone == "maubinh")
            {
                CPlayer.gameName = "maubinh";
                CPlayer.gameNameFull = App.listKeyText["MAUBINH_NAME"].ToUpper();//"MẬU BINH";
                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());
                return;

            }

            if (zone == "phom")
            {
                Debug.Log(1);
                CPlayer.gameName = "0";
                CPlayer.gameNameFull = App.listKeyText["PHOM_NAME"].ToUpper();//"PHỎM";
                Debug.Log(2);

                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());
                return;
            }

            if (zone == "xito")
            {
                CPlayer.gameName = "xito";
                CPlayer.gameNameFull = App.listKeyText["XITO_NAME"].ToUpper();//"XÌ TỐ";
                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());
                return;
            }
            if (zone == "xidach")
            {
                CPlayer.gameName = "blackjack";
                CPlayer.gameNameFull = App.listKeyText["XIDACH_NAME"].ToUpper();//"XÌ DZÁCH";
                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());
                return;
            }

            if (zone == "chan")
            {
                CPlayer.gameName = "chan";
                CPlayer.gameNameFull = App.listKeyText["CHAN_NAME"].ToUpper();//"CHẮN";
                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());
                return;
            }

            if (zone == "poker")
            {
                CPlayer.gameName = "poker";
                CPlayer.gameNameFull = App.listKeyText["POKER_NAME"].ToUpper();//"POKER";
                LoadingUIPanel.Show();
                StartCoroutine(openTLMN());
                return;
            }

            if (zone == "slotmachine")
            {
                CPlayer.gameName = "Slot";
                CPlayer.gameNameFull = "Slot Machine";
                LoadingUIPanel.Show();
                StartCoroutine(openSlotMachine());
                return;
            }

            if (zone == "cotuong")
            {
                CPlayer.gameName = "xiangqi";
                CPlayer.gameNameFull = "Cờ Tướng";
                LoadingUIPanel.Show();
                StartCoroutine(openCoTuong());
                return;
            }
            if (zone == "coup")
            {
                CPlayer.gameName = "mystery_xiangqi";
                CPlayer.gameNameFull = "Cờ Úp";
                LoadingUIPanel.Show();
                StartCoroutine(openCoTuong());
                return;
            }
            if (zone == "covua")
            {
                CPlayer.gameName = "chess";
                CPlayer.gameNameFull = "Cờ Vua";
                LoadingUIPanel.Show();
                StartCoroutine(openCoTuong());
                return;
            }
        }



        IEnumerator openTLMN()
        {
            /*
            CPlayer.changed -= setBalance;
            CPlayer.preScene = "Lobby";
            yield return new WaitForSeconds(0.5f);

            SceneManager.LoadScene("TableList");
            yield return new WaitForSecondsRealtime(0.5f);

            slideSceneAnim.Play("SlideLobbyOut");
            Destroy(gameObject, 1f);
            yield return new WaitForSeconds(.5f);
            LoadingControl.instance.loadingScene.SetActive(false);
            */
            //LoadingControl.instance.blackkkkkk.SetActive(true);
            LoadingUIPanel.Show();
            //CPlayer.changed -= setBalance;
            CPlayer.preScene = "Lobby";
            StartCoroutine(_EnterGameByLoadTablelist("tablelist", "TableList"));
            //SceneManager.LoadScene("TableList");
            yield return new WaitForSeconds(0.05f);
        }

        IEnumerator openSlotMachine()
        {
            /*============THAY===========
            CPlayer.changed -= setBalance;
            CPlayer.preScene = "Lobby";
            yield return new WaitForSeconds(0.5f);

            SceneManager.LoadScene("SlotMachineScene");
            yield return new WaitForSecondsRealtime(0.5f);

            slideSceneAnim.Play("SlideLobbyOut");
            Destroy(gameObject, 1f);
            yield return new WaitForSeconds(.5f);
            LoadingControl.instance.loadingScene.SetActive(false);
            ===========THAY==============*/
            //LoadingControl.instance.blackkkkkk.SetActive(true);
            LoadingUIPanel.Show();
            //CPlayer.changed -= setBalance;
            CPlayer.preScene = "Lobby";
            SceneManager.LoadScene("SlotMachineScene");
            yield return new WaitForSecondsRealtime(0.05f);
        }

        IEnumerator openCoTuong()
        {
            LoadingUIPanel.Show();

            //CPlayer.changed -= setBalance;
            CPlayer.preScene = "Lobby";
            SceneManager.LoadScene("WaitChessScene");
            yield return new WaitForSecondsRealtime(0.5f);
        }

        //public void showIQuay()
        //{
        //    // Comment out
        //    //LoadingControl.instance.showIQuay(true);
        //}

        //public void showDapTrung()
        //{
        //    // Comment out
        //    //LoadingControl.instance.showDapTrung(true);
        //}

        //public void showTaiXiu()
        //{
        //    // Comment out
        //    //LoadingControl.instance.showTaiXiu(true);
        //}
        public IEnumerator _EnterGameByLoadTablelist(string gameName, string sceneName)
        {
            yield return new WaitForSeconds(0f);
            string path = ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? Application.persistentDataPath : Application.dataPath)
                + "/DLC/" + gameName+".dlc";
            //Caching.ClearCache();
            //var listScene = AssetBundle.LoadFromFileAsync(path);
            //yield return listScene;
            //var scenes = listScene.assetBundle.GetAllScenePaths();
            //SceneManager.LoadScene(scenes[0]);

            AssetBundleCreateRequest rqAB = new AssetBundleCreateRequest();
            Caching.ClearCache();
            rqAB = AssetBundle.LoadFromFileAsync(path);
            yield return rqAB;
            asbs[0] = rqAB.assetBundle;
            SceneManager.LoadScene(sceneName);
        }
    }
}