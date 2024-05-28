using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
    public class HostGameScreen : UFEScreen
    {
        public Text connectionStatus;
        public Button createMatchButton;

        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            // Set Multiplayer Mode to "Online" (through a network service)
            UFE.MultiplayerMode = MultiplayerMode.Online;

            // Network events 
            UFE.MultiplayerAPI.OnMatchCreated += this.OnMatchCreated;
            UFE.MultiplayerAPI.OnMatchCreationError += this.OnMatchCreationError;
            UFE.MultiplayerAPI.OnPlayerConnectedToMatch += this.OnPlayerConnectedToMatch;
        }

        public override void OnHide()
        {
            base.OnHide();
            UFE.MultiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
            UFE.MultiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;
            UFE.MultiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
        }
        #endregion

        #region public interaction methods
        public void StartHostGame(Text textUI)
        {
            connectionStatus.text = "Creating match...";

            UFE.MultiplayerAPI.CreateMatch(textUI.text);

            createMatchButton.interactable = false;
        }

        public virtual void StartRoomMatchScreen()
        {
            UFE.MultiplayerAPI.LeaveMatch();

            UFE.StartRoomMatchScreen();
        }
        #endregion

        #region protected instance methods
        protected void OnMatchCreated(string matchName)
        {
            connectionStatus.text = "Waiting for players...";
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Created: " + matchName);
        }

        protected void OnMatchCreationError()
        {
            connectionStatus.text = "Error Creating Match";
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Error Creating Match.");
        }

        protected void OnPlayerConnectedToMatch(string playerName)
        {
            connectionStatus.text = "Player Connected. Starting Match...";
            if (UFE.config.debugOptions.connectionLog) Debug.Log("(Host) Match Starting...");
            UFE.StartNetworkGame(true, false);
        }
        #endregion
    }
}