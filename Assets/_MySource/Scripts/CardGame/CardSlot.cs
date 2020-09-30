using UnityEngine;
using System.Collections;

public class CardSlot {

    public CardSlot(int lineCount)
    {
        for(int i = 0; i < lineCount; i++)
        {
            addCardLine(new CardLine());
        }
    }

    public void addCardLine(CardLine cardLine)
    {

    }
}
