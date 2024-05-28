using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
    public class JoinGameScreen : UFEScreen
    {
        public Text connectionStatus;

        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            // Set Multiplayer Mode to "Online" (through a network service)
            UFE.MultiplayerMode = MultiplayerMode.Online;

            // Network events 
            UFE.MultiplayerAPI.OnJoined += this.OnJoined;
            UFE.MultiplayerAPI.OnJoinError += this.OnJoinError;
        }

        public override void OnHide()
        {
            base.OnHide();
            UFE.MultiplayerAPI.OnJoined -= this.OnJoined;
            UFE.MultiplayerAPI.OnJoinError -= this.OnJoinError;
        }
        #endregion

        #region public interaction methods
        public void JoinGame(Text textUI)
        {
            connectionStatus.text = "Joining match...";

            UFE.MultiplayerAPI.JoinMatch(textUI.text);
        }

        public virtual void GoToRoomMatchScreen()
        {
            UFE.StartRoomMatchScreen();
        }

        public virtual void GoToMainMenu()
        {
            UFE.StartMainMenuScreen();
        }
        #endregion


        #region protected instance methods
        protected virtual void OnJoined()
        {
            connectionStatus.text = "Match Found! Starting...";
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Starting...");
            UFE.StartNetworkGame(false, false);
        }

        protected virtual void OnJoinError()
        {
            connectionStatus.text = "Could not join match.";
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Error Joining");
        }
        #endregion
    }
}