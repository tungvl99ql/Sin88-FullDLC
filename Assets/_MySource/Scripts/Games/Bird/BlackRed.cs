using Casino.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using Core.Server.Api;

namespace Core.Bird
{

    public class BlackRed : MiniGame
    {
        public UnityEvent EndGameBound;

        // public BirdGamePlay bird;
        public BirdControl bird;
        public GameObject open;
        private int fird = 1;
        public bool isPlayFird
        {
            get { if (fird == 1)
                    return true;
                else
                    return false;
            }
        }
        public override string GameCode
        {
            get
            {
                return "redblack";
            }
        }
       
        public override string PlayGameCommand
        {
            get
            {
                return "REDBLACK.START";
            }


        }
     
        public override void OnEnable()
        {
            LoadingControl.instance.loadingGojList[21].SetActive(false);
            fird = 1;
          
           
        }

        public override void OnDisable()
        {
            LoadingControl.instance.loadingGojList[21].SetActive(true);
        }
        public override void OnQuitGame()
        {
            BirdControl.instance.AllowSpin();
            open.SetActive(false);
            gameObject.SetActive(false);

          
           
          
            LoadingControl.instance.loadingGojList[21].SetActive(true);
        }
        /// <summary>
        /// Open Done
        /// </summary>
        public void ShowUp()
        {

            var ShowUpRequest = new OutBounMessage("REDBLACK.SHOWUP");
            ShowUpRequest.addHead();
            ShowUpRequest.writeInt(bird.currentXGoldRush);
            ShowUpRequest.writeInt(bird.currentBet);
            if (bird.isTrial)
            {
                ShowUpRequest.writeByte(1); /*1 - choi thu | 0 - Choi that*/
            }
            else
            {
                ShowUpRequest.writeByte(0); /*1 - choi thu | 0 - Choi that*/
            }
         //  Debug.Log("Muc Cuoc " + bird.gameInfo.betSelected + "XstartMini " + bird.gameInfo.xStartMiniGame);
            App.ws.send(ShowUpRequest.getReq(), delegate (InBoundMessage showupResult)
            {
                int totalMoney = showupResult.readInt();
                if (showUpResultEvent != null)
                {
                    showUpResultEvent.Invoke(totalMoney);
                }
                Debug.Log(totalMoney);
                Debug.Log("OK");
            });
        }
        public override void FillBetData(ref OutBounMessage request)
        {
        
            request.writeInt(bird.currentBet);//Test
            request.writeString(GameCode);

            request.writeInt(bird.currentXGoldRush); //Gia tri X ban dau
            if(bird.isTrial)
            {
                request.writeByte(1); /*1 - choi thu | 0 - Choi that*/
            }
            else
            {
                request.writeByte(0); /*1 - choi thu | 0 - Choi that*/
            }
            request.writeByte(fird);//Xác định là lượt đầu tiên | 1 : Dau tien 0: Sau
      //      Debug.Log("bird.gameInfo.betSelected = " + bird.gameInfo.betSelected);
           // Debug.Log("bird.gameInfo.isTrial = " + bird.gameInfo.isTrial);
           // Debug.Log("fird = " + fird);
            fird = 0;


     
    

        }

        public override void OnResult(InBoundMessage result)
        {

   

           // Debug.Log("=========================");
            
            var goldBonus = result.readInt();//Số tiền ăn mỗi turn, vào BLACK thì = 0
            var xBonus = result.readString(); //là BLACK thì dừng, khác thì đi tiếp
            var isTrial = result.readByte() == 1 ? true : false;//Biến xác định là lượt chơi thử
         //   Debug.Log("goldBonus = " + goldBonus);
          //  Debug.Log("xBonus = " + xBonus);
          //  Debug.Log("isTrial = " + isTrial);
          //  Debug.Log("=========================");


            if (receiveResultEvent != null)
            {
                Hashtable data = new Hashtable();
                data.Add("goldBonus", goldBonus);
                data.Add("xBonus", xBonus);
                data.Add("isTrial", isTrial);
                receiveResultEvent.Invoke(data);
            }

        }


    }
   
}