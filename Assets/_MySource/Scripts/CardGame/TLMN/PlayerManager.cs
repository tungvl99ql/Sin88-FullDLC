using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Core.Server.Api;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager instance;

    void Awake()
    {
        getInstance();
    }

    public void getInstance() {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
        }
    }
    
    //public Image[] avatarImage;
    public void setInfo(BoardManager.Player player, Image im,GameObject infoObj, Text balanceText, Text nickNamText, GameObject ownerImg,bool isMine) {
        //im.gameObject.transform.localScale = Vector3.one;
        StartCoroutine(App.loadImg(im, App.getAvatarLink2(player.Avatar, (int)player.PlayerId), isMine));
        if(infoObj != null)
            infoObj.SetActive(true);
        balanceText.text = player.SlotId == 0 ? App.formatMoney(player.ChipBalance.ToString()) : App.formatMoneyAuto(player.ChipBalance);
        nickNamText.text = App.formatNickName(player.NickName,10);
        ownerImg.SetActive(player.IsOwner);
        
    }
    
    public void setTurn()
    {

    }
}
