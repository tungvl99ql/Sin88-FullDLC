using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class SoundButton : MonoBehaviour , IPointerClickHandler{
    public KindButton kindButton = KindButton.Normal;

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (kindButton)
        {
            case KindButton.Normal:
                SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                break;
            case KindButton.PlayAndAutoPlay:
                SoundManager.instance.PlayUISound(SoundFX.SPIN);
                break;
            default:
                SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
                break;
        }
    }
}
public enum KindButton
{
    Normal,
    PlayAndAutoPlay
}