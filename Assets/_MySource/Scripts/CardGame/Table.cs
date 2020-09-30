using UnityEngine;
using System.Collections;

public class Table : AbstractTable {

    public CardSlot centerCardSlot;
    void Start()
    {

        centerCardSlot = createCardSlot(1);
    }

    public CardSlot createCardSlot(int lineCount)
    {
        return new CardSlot(lineCount);
    }
}
