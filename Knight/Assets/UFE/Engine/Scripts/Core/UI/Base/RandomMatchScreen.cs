using UnityEngine;

namespace UFE3D
{
    public class RandomMatchScreen : UFEScreen
    {
        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            // Set Multiplayer Mode to "Online" (through a network service)
            UFE.MultiplayerMode = MultiplayerMode.Online;

            // Network events 
            UFE.MultiplayerAPI.OnJoined += this.OnJoined;
            UFE.MultiplayerAPI.OnPlayerConnectedToMatch += this.OnPlayerConnectedToMatch;

            if (!UFE.MultiplayerAPI.IsInsideLobby())
            {
                UFE.MultiplayerAPI.OnJoinedLobby += this.OnJoinedLobby;
            }
            else
            {
                UFE.MultiplayerAPI.JoinRandomOrCreateRoom();
            }
        }

        public override void OnHide()
        {
            base.OnHide();
            UFE.MultiplayerAPI.OnJoined -= this.OnJoined;
            UFE.MultiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
            UFE.MultiplayerAPI.OnJoinedLobby -= this.OnJoinedLobby;
        }
        #endregion

        #region public instance methods
        public virtual void GoToMainMenuScreen()
        {
            UFE.MultiplayerAPI.LeaveMatch();
            UFE.StartMainMenuScreen();
        }

        public virtual void GoToNetworkOptionsScreen()
        {
            UFE.MultiplayerAPI.LeaveMatch();
            UFE.StartNetworkOptionsScreen();
        }
        #endregion

        #region protected instance methods
        protected virtual void OnJoined()
        {
            if (UFE.config.debugOptions.connectionLog) Debug.Log("(OnJoined) Match Starting...");
            UFE.StartNetworkGame(false, false);
        }

        protected virtual void OnJoinedLobby()
        {
            if (UFE.config.debugOptions.connectionLog) Debug.Log("(Lobby Joined.");
            UFE.MultiplayerAPI.JoinRandomOrCreateRoom();
        }

        protected virtual void OnPlayerConnectedToMatch(string playerName)
        {
            if (UFE.config.debugOptions.connectionLog) Debug.Log("(OnPlayerConnectedToMatch) Match Starting...");
            UFE.StartNetworkGame(true, false);
        }
        #endregion
    }
}