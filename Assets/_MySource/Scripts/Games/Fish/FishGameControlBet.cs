using Core.Server.Api;
using UnityEngine;
using UnityEngine.UI;
namespace Slot.Games.Fish {
    public class FishGameControlBet : MonoBehaviour
    {
        public Text textBet;
        public Text textTotalBet;
        public FishGameController fishGameController;
        public void ChoseBetValue(int id)
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel = ((FishSlotInfo)(fishGameController.GameInfo)).betLevels[id];
            textBet.text = App.formatMoney(((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel.ToString());
            textTotalBet.text = App.formatMoney((((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel).ToString());
            ((FishSlotInfo)(fishGameController.GameInfo)).playReal = true;
            FishGameControllUI.instance.btnAutoSpin.interactable = true;
            gameObject.SetActive(false);
        }
        public void ChoseBetValueTrial()
        {
            SoundManager.instance.PlayUISound(SoundFX.BUTTON_CLICK);
            ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel = ((FishSlotInfo)(fishGameController.GameInfo)).betLevels[((FishSlotInfo)(fishGameController.GameInfo)).betLevels.Length-1];
            textBet.text = App.formatMoney(((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel.ToString());
            textTotalBet.text = App.formatMoney((((FishSlotInfo)(fishGameController.GameInfo)).totalLineBet * ((FishSlotBetData)((FishSlotInfo)(fishGameController.GameInfo)).BetData).selectedBetLevel).ToString());
            ((FishSlotInfo)(fishGameController.GameInfo)).playReal = false;
            FishGameControllUI.instance.btnAutoSpin.interactable = false;
            gameObject.SetActive(false);
        }
    }
}
