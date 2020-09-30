/// <summary>
/// Base class for mini game, that contains server call method.
/// The Derive class will override the usful method.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Core.Server.Api;

namespace Casino.Core {

	public delegate void ReceiveResultEvent(Hashtable receiveData);
    public delegate void ShowUpResultEvent(int money);
    public class MiniGame : MonoBehaviour {

       
        public virtual string GameCode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the join game command.
		/// This must be override for each subclass.
		/// </summary>
		/// <value>The join game command.</value>
		public virtual string JoinGameCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the game info.
		/// This is the current game info.
		/// </summary>
		/// <value>The game info.</value>
		public virtual GameInfo GameInfo {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the bet data.
		/// </summary>
		/// <value>The bet data.</value>
//		public virtual BetData BetData {
//			get;
//			set;
//		}

		/// <summary>
		/// Gets or sets the play game command.
		/// This must be overriden in subclass.
		/// </summary>
		/// <value>The play game command.</value>
		public virtual string PlayGameCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the ready command.
		/// </summary>
		/// <value>The ready command.</value>
		public virtual string FinishGameTurnCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the show history command.
		/// This must be overriden in subclass.
		/// </summary>
		/// <value>The show history command.</value>
		public virtual string ShowHistoryCommand {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the show glory board command.
		/// This must be overriden in subclass.
		/// </summary>
		/// <value>The show glory board command.</value>
		public virtual string ShowGloryBoardCommand {
			get;
			set;
		}

		public ReceiveResultEvent receiveResultEvent;
        public ShowUpResultEvent showUpResultEvent;
        void Start()
        {
			InitGame ();
        }

        public virtual void OnEnable() {
			OnJoinGame (JoinGameCommand);
		}

		public virtual void OnDisable() {
			//OnQuitGame ();
		}

		#region Base Mothods

		/// <summary>
		/// Initialize the game screen just before Player Join Game success.
		/// Setup the defaul value for every thing.
		/// </summary>
		public virtual void InitGame() {


		}

		/// <summary>
		/// Called when player join game
		/// setup player data
		/// setup game state
		/// </summary>
		public virtual void OnJoinGame(string joinGameCommand) {
			var joinGameRequest = new OutBounMessage(joinGameCommand);
			joinGameRequest.addHead();
			App.ws.send(joinGameRequest.getReq(), JoinGameHandler);
		}
		/// <summary>
		/// Joins the game handler.
		/// Callback from the server join game request, that will be implement in devired class
		/// </summary>
		/// <param name="joinGameRespond">Join game respond.</param>
		public virtual void JoinGameHandler(InBoundMessage joinGameRespond) {
			
		}

		/// <summary>
		/// Finishs the game turn.
		/// Call when finish game turn, notify to server ask to update Player's money.
		/// </summary>
		public virtual void FinishGameTurn(){
			var finishTurnRequest = new OutBounMessage(FinishGameTurnCommand);
			finishTurnRequest.addHead();
			FillFinishTurnData (ref finishTurnRequest);
			App.ws.send(finishTurnRequest.getReq(), OnGameTurnFinish);
		}

		/// <summary>
		/// Called when Player pressed play game.
		/// This method will send to message to server to ask server to play a Game Turn.
		/// </summary>
		public virtual void Play() {
			var playGameRequest = new OutBounMessage(PlayGameCommand);
			playGameRequest.addHead();
			FillBetData (ref playGameRequest);

			App.ws.send(playGameRequest.getReq(), OnResult);
		}
		/// <summary>
		/// Fills the bet data.
		/// Call just before Player press play.
		/// Ovrride in subclass for handling detail base on what game is work on.
		/// </summary>
		/// <param name="request">Request.</param>
		public virtual void FillBetData(ref OutBounMessage request) {

		}

		/// <summary>
		/// Fills the finish data.
		/// </summary>
		/// <param name="request">Request.</param>
		public virtual void FillFinishTurnData(ref OutBounMessage request) {
		
		}

		/// <summary>
		/// Handle result base on the Play request
		/// </summary>
		/// <param name="result">Result.</param>
		public virtual void OnResult(InBoundMessage result) {

		}

		/// <summary>
		/// Raises the game turn finish event.
		/// 
		/// </summary>
		/// <param name="respond">Respond.</param>
		public virtual void OnGameTurnFinish(InBoundMessage respond) {
		
		}

		/// <summary>
		/// Called when gather player Bet Data that data will be send to server later with the Play message.
		/// </summary>
		public virtual void Bet() {
			
		}

		/// <summary>
		/// send request for show get glory data from server.
		/// </summary>
		public virtual void ShowGloryBoard() {
			var showGloryBoardRequest = new OutBounMessage(ShowGloryBoardCommand);
			showGloryBoardRequest.addHead();
			App.ws.send(showGloryBoardRequest.getReq(), OnShowGloryBoard);
		}


		public virtual void OnShowGloryBoard(InBoundMessage gloryboardRespond) {

		}

		/// <summary>
		/// Shows the history of Player activities.
		/// </summary>
		public virtual void ShowHistory() {
			var showHistoryRequest = new OutBounMessage(ShowHistoryCommand);
			showHistoryRequest.addHead();
            FillHistoryRequestData(ref showHistoryRequest);
            App.ws.send(showHistoryRequest.getReq(), OnShowHistory);
		}

        public virtual void FillHistoryRequestData(ref OutBounMessage request)
        {

        }

        public virtual void OnShowHistory(InBoundMessage result) {

		}

		/// <summary>
		/// Show game guide.
		/// </summary>
		public virtual void ShowTutorial() {
			
		}

		public void CloseGame() {
			OnQuitGame ();
		}

		/// <summary>
		/// Release game resource.
		/// ...
		/// </summary>
		public virtual void OnQuitGame() {
			
		}

		public virtual void OnPotChanged() {
			
		}


		#endregion
	}
	/// <summary>
	/// Bet data.
	/// that contains detail bet data.
	/// </summary>
	public class BetData {
		
	}

	/// <summary>
	/// Game info.
	/// The basic information of game.
	/// </summary>
	public class GameInfo {
		/// <summary>
		/// Gets or sets the bet data.
		/// </summary>
		/// <value>The bet data.</value>
		public virtual BetData BetData {
			get;
			set;
		}

	}
}

