using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Casino.Core;
using UnityEngine.Events;

namespace Casino.Games.BxB {

    public delegate void BetLevelChangeEvent(int selectedIdx);

	public class GameplayPanel : MonoBehaviour {

		public CustomToggleGroup betLevelTogglesGroup;
		public Toggle autoRunModeToggle;
		public Toggle speedModeToggle;
		public SlotsPanel slotPanel;
		public Button closeBtn;
        public GameObject totalChipGO;
        public GameObject bigGiftGO;
		public GameObject potGiftGO;

        public BetLevelChangeEvent betLevelChangeEvent;
		public UnityEvent requestResultEvent;

		public Selectable[] controllers;

        public Selectable exitButton;

		private bool isFastMode;
		private bool isAutoRun;
		private bool isBigWin;
		private bool isJackpot;

		private bool isWinMoney;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Casino.Games.BxB.GameplayPanel"/> speed mode.
		/// True: indicates mormal speed mode.
		/// False: indicates fast speed mode.
		/// </summary>
		/// <value><c>true</c> if speed mode; otherwise, <c>false</c>.</value>
		public bool SpeedMode {
			get;
			set;
		}
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Casino.Games.BxB.GameplayPanel"/> auto run mode.
		/// True: dipict that is auto run mode.
		/// False: dipict that is run manually.
		/// </summary>
		/// <value><c>true</c> if auto run mode; otherwise, <c>false</c>.</value>
		public bool AutoRunMode {
			get;
			set;
		}

		private bool isPlaying;

		public bool IsPlaying {
			get { 
				return isPlaying;
			}
			set {
				isPlaying = value;
				setController (!isPlaying);
				closeBtn.interactable = !isPlaying;
			}
		}

		void OnEnable() {

            autoRunModeToggle.interactable = true;
            IsPlaying = false;
			slotPanel.finishEffectEvent.AddListener (OnFinishEffect);
			slotPanel.spinnerBeginEvent.AddListener (OnRequestResult);
			betLevelTogglesGroup.OnChangeSelect += OnBetLevelChange;
			speedModeToggle.onValueChanged.AddListener (OnSpinSpeedModeChange);
			autoRunModeToggle.onValueChanged.AddListener (OnSpinAutoRunModeChange);

			speedModeToggle.onValueChanged.Invoke (speedModeToggle.isOn);

		}

		void OnDisable() {
			slotPanel.finishEffectEvent.RemoveListener(OnFinishEffect);
			slotPanel.spinnerBeginEvent.RemoveListener (OnRequestResult);
			betLevelTogglesGroup.OnChangeSelect -= OnBetLevelChange;
			speedModeToggle.onValueChanged.RemoveListener (OnSpinSpeedModeChange);
			autoRunModeToggle.onValueChanged.RemoveListener (OnSpinAutoRunModeChange);
		}



		public void SetBetLevel() {
			// TODO: update Current Bet Data.
		}


        public void Spin() {
			IsPlaying = true;
			disableGotMoney ();
            slotPanel.Spin (SpeedMode);
            totalChipGO.SetActive(false);
		}

public GameObject SpinSpeedMode,SpinAutoRunMode;
		public void OnSpinSpeedModeChange(bool isNormalMode) {
			
			SpeedMode = speedModeToggle.isOn;
			SpinSpeedMode.SetActive(SpeedMode);
		}

		public void OnSpinAutoRunModeChange(bool isRunManually) {
			AutoRunMode = autoRunModeToggle.isOn;
				SpinAutoRunMode.SetActive(AutoRunMode);
			if (!IsPlaying) {
                Spin ();
			}
		}

		private void OnBetLevelChange(int selectIdx) {
            if (betLevelChangeEvent != null)
            {
                betLevelChangeEvent.Invoke(selectIdx);
            }
		}		

		public void OnReceicveResult(bool hasResult, bool bg, bool jp, ref int[] resultSlot, ref List<int[]> resultSlotsData) {

			isBigWin = bg;
			isJackpot = jp;
			if (resultSlotsData.Count > 0) {
				isWinMoney = true;
			} else {
				isWinMoney = false;
			}
			slotPanel.OnReceiveResult (hasResult, ref resultSlot, ref resultSlotsData);
		}

		public void OnRequestResult() {
			if (requestResultEvent != null) {
				requestResultEvent.Invoke ();
			}
		}

		public void OnFinishEffect() {

			StartCoroutine (ieShowFinish());

//			if (AutoRunMode) {
//				StartCoroutine (waitForNewTurn (1f));
//			}
//			disableGotMoney ();
//			IsPlaying = false;

		}

		IEnumerator ieShowFinish() {
			//yield return new WaitForSeconds (2f);
			showGotMoney ();
			yield return new WaitForSeconds (1f);
			if (AutoRunMode && !isJackpot) {
				Spin ();
			} else {
				IsPlaying = false;
			}
//			disableGotMoney ();
			//IsPlaying = false;
		}


		private void showGotMoney() {
			
			if (isJackpot) {
				potGiftGO.SetActive (true);
//				bigGiftGO.SetActive (false);
//				totalChipGO.SetActive (false);

			} else if (isBigWin) {
				bigGiftGO.SetActive (true);
                SoundManager.instance.PlayUISound(SoundFX.BIG_WIN + "_" + UnityEngine.Random.Range(1,4));
//				potGiftGO.SetActive (false);
//				totalChipGO.SetActive (false);
			}
			totalChipGO.SetActive (isWinMoney);
		}

		private void disableGotMoney() {
			totalChipGO.SetActive (false);
			potGiftGO.SetActive (false);
			bigGiftGO.SetActive (false);
		}

		public void setController(bool isAvaiable) {
			for (int i = 0; i < controllers.Length; i++) {
				controllers [i].interactable = isAvaiable;
			}
		}

        public void ForceExitSetupControl(bool isEnable) {
            exitButton.interactable = isEnable;
            autoRunModeToggle.interactable = !isEnable;
        }

        public void Reset()
        {
            slotPanel.ForceStop();
        }



        IEnumerator waitForNewTurn(float t) {
			yield return new WaitForSeconds (t);
            Spin ();
		}

	}

	public enum SpinState {
		STOP,
		RUNNING
	}

}

