using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Lobby {
    public class TopJarUIItem : MonoBehaviour {

        public Text gameNameTxt;
        public Image jarIconImg;
        public Text currentJarTxt;

        public void UpdateInfo(string gamename, Sprite gameIcon, string jarValue) {

            gameNameTxt.text = gamename;
            jarIconImg.overrideSprite = gameIcon;
            currentJarTxt.text = jarValue;

        }

    }

}
