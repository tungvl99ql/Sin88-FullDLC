using Core.Server.Api;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour {

    public static ProfileController instance;
    public Text txtUsername,txtPointVip,txtSDT,txtMoney, txtBtnVip;

    public Image spriterAvater;
    public GameObject button_ConfirmNumber,button_ChangePass,panelChangePass , button_CreatePassLV2;
    public InputField input_passOld, input_passNew1,input_passNew2;
    private long preMoneyValues=0;
    void Awake()
    {
        if (instance != null)
            DestroyImmediate(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        gameObject.SetActive(false);
    }
    void Start()
    {

    }
    void OnEnable()
    {
        txtUsername.text = "";
        txtSDT.text = "";
        txtPointVip.text = "";
        panelChangePass.SetActive(false);
        LoadDataPlayer();
        checkButtonCreatePassLV2();
        checkBtnVip();
        GetVipPoint();
    }
  
	
	void Update () {
		
	}
    public void buttonClose()
    {
        gameObject.SetActive(false);
    }

    public void buttonChangePass()
    {
        panelChangePass.SetActive(true);
        input_passOld.text = "";
        input_passNew1.text = "";
        input_passNew2.text = "";
    }
    public void OnButtonCreatePassLV2()
    {
        LoadingControl.instance.CreatePasswordLevel2();
    }
    public void buttonConfirm()
    {
        LoadingControl.instance.DoAuthen();
    }
    public void buttonHistory()
    {
        ProfileHistoryController.instance.Show();
    }
    public void buttonAddMoney()
    {
        LobbyControl.instance.OpenRecharge(true);
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }



    private void GetVipPoint()
    {
        //if (CPlayer.phoneNum == "")
        //{
        //    txtSDT.text = "";
        //}
        //else
        //{
           
            var req_VIP = new OutBounMessage("CASH_OUT.GET_PLAYER_VIP_POINT");
            req_VIP.addHead();
            req_VIP.writeString(CPlayer.nickName.ToLower());
            App.ws.send(req_VIP.getReq(), delegate (InBoundMessage res_VIP)
            {
                string tmp = res_VIP.readString();
                int point = res_VIP.readInt();
                int nextLevel = res_VIP.readInt();
                txtPointVip.text = point.ToString();
            });
        //}
    }
    private void LoadDataPlayer()
    {
        var req_info = new OutBounMessage("PLAYER_PROFILE");
        //Debug.Log("WRITE LONG = " + CPlayer.id);
        //App.trace("PPLAYER ID = " + CPlayer.id);
        req_info.addHead();
        req_info.writeLong(CPlayer.id);
        req_info.writeByte(0x0f);
        req_info.writeAcii("");


        App.ws.send(req_info.getReq(), delegate (InBoundMessage res_PLAYER_INFO)
        {
            var nickName = res_PLAYER_INFO.readAscii();
            var fullName = res_PLAYER_INFO.readString();
            var avatar = res_PLAYER_INFO.readAscii();
            var isMale = res_PLAYER_INFO.readByte() == 1;
            var dateOfBirth = res_PLAYER_INFO.readAscii();
            var message = res_PLAYER_INFO.readString();
            var chipBalance = res_PLAYER_INFO.readLong();
            var starBalance = res_PLAYER_INFO.readLong();
            string phone = res_PLAYER_INFO.readAscii();
            if (CPlayer.loginType == "fb")
            {
                txtUsername.text = nickName;
            }
            else
            {
                //txtUsername.text = App.formatNickName(nickName, 12);
                txtUsername.text = nickName;
            }
            
            txtMoney.text = App.formatMoney(chipBalance.ToString());
            spriterAvater.sprite = CPlayer.avatarSpriteToSave;

            button_ChangePass.SetActive(true);
            CPlayer.phoneNum = phone;
            txtSDT.text = CPlayer.phoneNum;
            GetVipPoint();

            //if (phone.Trim().Length<7)
            //{
            //   // button_ConfirmNumber.SetActive(true);
            //   // button_ChangePass.SetActive(false);

            //}
            //else
            //{

            //  //  button_ConfirmNumber.SetActive(false);
                
            //}
           

        });
    }

    public void balanceChanged(long chipBalance, long starBalance)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(TweenPotNum(txtMoney, (int)preMoneyValues, (int)chipBalance));
        }

    }

    private IEnumerator TweenPotNum(Text txt, int fromNum, int toNum, bool fake = false)
    {
        if (fake)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(.5f, 1f));
        }
        else
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));
        }
        float i = 0.0f;
        float rate = 1.0f / .5f;
        txt.transform.localScale = 1.2f * Vector2.one;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            float a = Mathf.Lerp(fromNum, toNum, i);
            txt.text = a > 0 ? string.Format("{0:0,0}", a) : "0";
            yield return null;
        }
        preMoneyValues = toNum;
        txt.transform.localScale = Vector2.one;
        yield return new WaitForSeconds(.05f);
    }

    public void checkBtnVip()
    {
        var req_abs = new OutBounMessage("ACTIVE_BUTTON.SHOW");
        req_abs.addHead();
        App.ws.send(req_abs.getReq(), delegate (InBoundMessage res_abs)
        {
            int visible = res_abs.readInt(); //0:Tắt | 1: Hiển thị
            string text = res_abs.readString(); //Tên button
            txtBtnVip.text = text;
            if (visible == 0)
            {
                button_ConfirmNumber.SetActive(false);
            }
            else
            {
                button_ConfirmNumber.SetActive(true);
            }
        });
    }

    public void checkButtonCreatePassLV2()
    {
        if(CPlayer.typeUser == 1)
        {
            button_CreatePassLV2.SetActive(true);
        }
        else if(CPlayer.typeUser == 0)
        {
            button_CreatePassLV2.SetActive(false);
        }
    }

    public void button_OKChangePass()
    {

        if (input_passOld.text.Length == 0)
        {
            //App.showErr("Mật khẩu cũ không được để trống");
            App.showErr(App.listKeyText["WARN_OLD_PWD_BLANK"]);
            return;
        }

        if (input_passNew1.text.Length == 0|| input_passNew2.text.Length == 0)
        {
            //App.showErr("Mật khẩu mới không được để trống");
            App.showErr(App.listKeyText["WARN_NEW_PWD_BLANK"]);
            return;
        }
        

        if (!input_passNew1.text.Trim().Equals(input_passNew2.text.Trim()))
        {
            //App.showErr("Hai mật khẩu không khớp");
            App.showErr(App.listKeyText["WARN_PASSWORD_NOT_MATCH"]);
            return;
        }
     
        OutBounMessage req_CHANGE_PASS = new OutBounMessage("CHANGE_PASSWORD");
        req_CHANGE_PASS.addHead();
        req_CHANGE_PASS.writeString(input_passOld.text);
        req_CHANGE_PASS.writeString(input_passNew1.text);
        App.ws.send(req_CHANGE_PASS.getReq(), delegate (InBoundMessage res_CHANGE_PASSWORD)
        {
            string rememberPassCheck = PlayerPrefs.GetString("rememberPass");
            if (rememberPassCheck == "true")
            {
                PlayerPrefs.SetString("pass", input_passNew1.text);
            }
            //App.showErr("Đổi mật khẩu thành công");
            App.showErr(App.listKeyText["INFO_PASSWORD_CHANGED_SUCCESS"]);
            panelChangePass.SetActive(false);
        });
    }
    public void button_CloseChangepass()
    {
        panelChangePass.SetActive(false);
    }
}
