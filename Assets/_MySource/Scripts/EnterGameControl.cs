using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Core.Server.Api
{

    public class EnterGameControl : MonoBehaviour
    {

        /// <summary>
        /// 0: blackPanel
        /// </summary>
        public Image[] etgImgs;

        /// <summary>
        /// 0: Black|1: trans|2: 
        /// </summary>
        public Color[] etgColors;

        /// <summary>
        /// 0: gameName
        /// </summary>
        public RectTransform[] etgRtfs;

        /// <summary>
        /// 0: tooltips
        /// </summary>
        public Text[] txts;


        private void Start()
        {
            Debug.LogError("ENTER GAME CONTROLLLLLLL");
        }

        private string[] t = {"Người chơi quay được Nổ Hũ sẽ thông báo tới toàn server.",
"Người chơi thắng lớn sẽ hiển thị trong phần vinh danh.",
"Minigame có thể chơi cùng lúc với các game chính.",
"Người chơi có thể xem lịch sử chơi của mình trong phần thông tin cá nhân."};
        //private void OnEnable()
        //{
        //    int random = UnityEngine.Random.Range(0, t.Length);
        //  //  etgImgs[0].sprite = imgs[UnityEngine.Random.Range(0, 4)];
        //    etgImgs[0].DOColor(etgColors[2], .5f).OnComplete(() =>
        //    {
        //        txts[0].text = "<color=red>Chú ý: </color>" + t[random];
        //        txts[0].gameObject.SetActive(true);
        //        etgRtfs[0].gameObject.SetActive(true);
        //        etgRtfs[0].DOScale(1.5f, 2f);
        //        DOTween.To(() => etgRtfs[0].anchoredPosition, x => etgRtfs[0].anchoredPosition = x, new Vector2(0, 250), 2f).OnComplete(() =>
        //        {
        //            gameObject.SetActive(false);
        //            etgRtfs[0].anchoredPosition = new Vector2(-207, 250);
        //            etgImgs[0].color = etgColors[1];
        //            etgRtfs[0].localScale = Vector2.one;
        //            etgRtfs[0].gameObject.SetActive(false);
        //            txts[0].gameObject.SetActive(false);
        //            if (SceneManager.GetActiveScene().name != "Lobby")
        //            {
        //                bool needGuide = PlayerPrefs.GetInt("needGuide", 0) == 0;
        //                //needGuide = true;
        //                if (needGuide && SceneManager.GetActiveScene().name != "MiniPoker" && SceneManager.GetActiveScene().name != "Xeng")
        //                {
        //                    LoadingControl.instance.loadingGojList[26].SetActive(true);
        //                }
        //            }
        //        });
        //    });
        //}

    }
}