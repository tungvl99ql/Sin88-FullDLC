using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Other_1 : MonoBehaviour {

    public Transform[] rtfs;

    private IEnumerator[] threads;

    private int[] numList = new int[1];

    private void OnEnable()
    {
        rtfs[0].DORotate(new Vector3(-30, -3000, 0), 5f, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.OutQuad);
    }

    public void OnClick()
    {
        numList[0]++;
        DOTween.Kill("swipeChat");
        transform.parent.DOLocalMoveY(transform.parent.localPosition.y + (numList[0]%2==1 ? 300 : -300),.2f).SetId("swipeChat");
    }
    

}
