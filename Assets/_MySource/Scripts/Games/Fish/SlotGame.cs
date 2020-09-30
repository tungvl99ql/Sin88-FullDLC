using Core.Server.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slot.Core {
    public delegate void ReceiveResultEvent(Hashtable receiveData);
    public class SlotGame : MonoBehaviour
    {

        public virtual string GameCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the info detail command
        /// </summary>
        public virtual string GetInfoDetailCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the join game command.
        /// This must be override for each subclass.
        /// </summary>
        /// <value>The join game command.</value>
        public virtual string JoinGameCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the game info.
        /// This is the current game info.
        /// </summary>
        /// <value>The game info.</value>
        public virtual GameInfo GameInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the play game command.
        /// This must be overriden in subclass.
        /// </summary>
        /// <value>The play game command.</value>
        public virtual string PlayGameCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ready command.
        /// </summary>
        /// <value>The ready command.</value>
        public virtual string FinishGameTurnCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the show history command.
        /// This must be overriden in subclass.
        /// </summary>
        /// <value>The show history command.</value>
        public virtual string ShowHistoryCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the show glory board command.
        /// This must be overriden in subclass.
        /// </summary>
        /// <value>The show glory board command.</value>
        public virtual string ShowGloryBoardCommand
        {
            get;
            set;
        }

        public ReceiveResultEvent receiveResultEvent;

        void Start()
        {
            InitGame();
            OnJoinGame(JoinGameCommand);
        }

        #region Base Mothods

        /// <summary>
        /// Initialize the game screen just before Player Join Game success.
        /// Setup the defaul value for every thing.
        /// </summary>
        public virtual void InitGame()
        {


        }

        /// <summary>
        /// Called when player join game
        /// setup player data
        /// setup game state
        /// </summary>
        public virtual void OnJoinGame(string joinGameCommand)
        {
            var joinGameRequest = new OutBounMessage(joinGameCommand);
            joinGameRequest.addHead();
            App.ws.send(joinGameRequest.getReq(), JoinGameHandler);
        }
        /// <summary>
        /// Joins the game handler.
        /// Callback from the server join game request, that will be implement in devired class
        /// </summary>
        /// <param name="joinGameRespond">Join game respond.</param>
        public virtual void JoinGameHandler(InBoundMessage joinGameRespond)
        {

        }

        /// <summary>
        /// Get info detail
        /// </summary>
        public virtual void GetInfoDetail()
        {
            var getInfoDetail = new OutBounMessage(GetInfoDetailCommand);
            getInfoDetail.addHead();
            FillGetInfoDetail(ref getInfoDetail);
            App.ws.send(getInfoDetail.getReq(),OnGetInfoDetail);

        }

        public virtual void FillGetInfoDetail(ref OutBounMessage request)
        {

        }
        /// <summary>
        /// Handle result base on the Play request
        /// </summary>
        /// <param name="respond">Result.</param>
        public virtual void OnGetInfoDetail(InBoundMessage respond)
        {

        }

        /// <summary>
        /// Called when Player pressed play game.
        /// This method will send to message to server to ask server to play a Game Turn.
        /// </summary>
        public virtual void Play()
        {
            var playGameRequest = new OutBounMessage(PlayGameCommand);
            playGameRequest.addHead();
            FillBetData(ref playGameRequest);

            App.ws.send(playGameRequest.getReq(), OnResult);
        }
        /// <summary>
        /// Fills the bet data.
        /// Call just before Player press play.
        /// Ovrride in subclass for handling detail base on what game is work on.
        /// </summary>
        /// <param name="request">Request.</param>
        public virtual void FillBetData(ref OutBounMessage request)
        {

        }
        /// <summary>
        /// Handle result base on the Play request
        /// </summary>
        /// <param name="result">Result.</param>
        public virtual void OnResult(InBoundMessage result)
        {

        }

        /// <summary>
        /// Called when gather player Bet Data that data will be send to server later with the Play message.
        /// </summary>
        public virtual void Bet()
        {

        }

        /// <summary>
        /// send request for show get glory data from server.
        /// </summary>
        public virtual void ShowGloryBoard()
        {
            var showGloryBoardRequest = new OutBounMessage(ShowGloryBoardCommand);
            showGloryBoardRequest.addHead();
            App.ws.send(showGloryBoardRequest.getReq(), OnShowGloryBoard);
        }


        public virtual void OnShowGloryBoard(InBoundMessage gloryboardRespond)
        {

        }

        /// <summary>
        /// Shows the history of Player activities.
        /// </summary>
        public virtual void ShowHistory()
        {
            var showHistoryRequest = new OutBounMessage(ShowHistoryCommand);
            showHistoryRequest.addHead();
            FillHistoryRequestData(ref showHistoryRequest);
            App.ws.send(showHistoryRequest.getReq(), OnShowHistory);
        }

        public virtual void FillHistoryRequestData(ref OutBounMessage request)
        {

        }

        public virtual void OnShowHistory(InBoundMessage result)
        {

        }


        /// <summary>
        /// Show game guide.
        /// </summary>
        public virtual void ShowTutorial()
        {
            var tutorialGameRequest = new OutBounMessage(JoinGameCommand);
            tutorialGameRequest.addHead();
            App.ws.send(tutorialGameRequest.getReq(), OnShowTutorial);
        }

        public virtual void OnShowTutorial(InBoundMessage result)
        {

        }


        public void CloseGame()
        {
            OnQuitGame();
        }

        /// <summary>
        /// Release game resource.
        /// ...
        /// </summary>
        public virtual void OnQuitGame(string SceneName = null)
        {

        }

        public virtual void OnPotChanged()
        {

        }

        public virtual void OnChipBalanceChanged(string type)
        {

        }

        #endregion
    }
    /// <summary>
    /// Bet data.
    /// that contains detail bet data.
    /// </summary>
    public class BetData
    {

    }
    /// <summary>
    /// Number line.
    /// </summary>
    public class NumberLine
    {

    }
    /// <summary>
    /// Game info.
    /// The basic information of game.
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// Gets or sets the bet data.
        /// </summary>
        /// <value>The bet data.</value>
        public virtual BetData BetData
        {
            get;
            set;
        }
        public virtual NumberLine NumberLine
        {
            get;
            set;
        }

    }

}

