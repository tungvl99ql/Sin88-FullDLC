using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core.Server.CardGame;
using Core.Server.Api;

public partial class BoardManager: MonoBehaviour {

    public class CardUtils
    {
        private static string[] cardTypes = { "bích", "tép", "rô", "cơ" };
        /// <summary>
        /// Chuyển Id trên server về id trong sheet bài
        /// </summary>
        public static void svrIdsToIds(List<int> arr)
        {
            int cardHigh = 0; // A 2 .. 10 J Q K
            int cardType = 0; // Chất của quân bài
            int temp = 0;
            for(int i = 0;i < arr.Count; i++)
            {
                temp = arr[i];
                cardType = (int)Mathf.Floor((float)temp / 13);
                cardHigh = temp % 13 + 1;

                switch (cardHigh)
                {
                    case 1:
                        App.trace("A " + cardTypes[cardType]);
                        break;
                    case 11:
                        App.trace("J " + cardTypes[cardType]);
                        break;
                    case 12:
                        App.trace("Q " + cardTypes[cardType]);
                        break;
                    case 13:
                        App.trace("K " + cardTypes[cardType]);
                        break;
                    default:
                        App.trace(cardHigh + cardTypes[cardType]);
                        break;
                        
                }
            }
        }
    }
    
}
