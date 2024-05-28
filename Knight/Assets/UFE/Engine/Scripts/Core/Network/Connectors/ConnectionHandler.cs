using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFE3D
{
    public class ConnectionHandler : MonoBehaviour
	{
		protected List<byte[]> _receivedNetworkMessages = new List<byte[]>();

		public bool HasStarted;

		void Start()
		{
			UFE.FluxCapacitor.Initialize();

			UFE.MultiplayerAPI.OnMessageReceived += this.OnMessageReceived;
			UFE.MultiplayerAPI.OnDisconnection += this.OnDisconnection;
			UFE.MultiplayerAPI.OnPlayerDisconnectedFromMatch += this.OnPlayerDisconnectedFromMatch;

			HasStarted = true;
		}

        private void OnDestroy()
        {
			UFE.MultiplayerAPI.OnMessageReceived -= this.OnMessageReceived;
			UFE.MultiplayerAPI.OnDisconnection -= this.OnDisconnection;
			UFE.MultiplayerAPI.OnPlayerDisconnectedFromMatch -= this.OnPlayerDisconnectedFromMatch;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This method is invoked remotely when inputs are received.
		/// </summary>
		/// <param name="bytes">Message info.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		protected virtual void OnMessageReceived(byte[] bytes)
		{
			this._receivedNetworkMessages.Add(bytes);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This method is invoked when remote player leaves the game.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private void OnPlayerDisconnectedFromMatch()
		{
			UFE.MultiplayerAPI.LeaveMatch();
			OnDisconnection();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This method is invoked when local player disconnects from the game.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private void OnDisconnection()
		{
			if (UFE.gameRunning || !(UFE.currentScreen is OnlineModeAfterBattleScreen))
			{
				UFE.EndGame();
				UFE.StartConnectionLostScreen();
			}

			Destroy(this);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Processes the pending network messages.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void ProcessReceivedNetworkMessages()
		{
			foreach (byte[] serializedMessage in this._receivedNetworkMessages)
			{
				if (serializedMessage != null && serializedMessage.Length > 0)
				{
					NetworkMessageType messageType = (NetworkMessageType)serializedMessage[0];
					if (messageType == NetworkMessageType.InputBuffer)
					{
						UFE.FluxCapacitor.ProcessInputBufferMessage(new InputBufferMessage(serializedMessage));
					}
					else if (messageType == NetworkMessageType.Syncronization && UFE.config.networkOptions.synchronizationAction != NetworkSynchronizationAction.Disabled)
					{
						UFE.FluxCapacitor.ProcessSynchronizationMessage(new SynchronizationMessage(serializedMessage));
					}
				}
			}
			this._receivedNetworkMessages.Clear();
		}
	}
}
