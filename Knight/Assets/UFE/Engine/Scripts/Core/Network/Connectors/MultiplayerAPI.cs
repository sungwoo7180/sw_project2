using UnityEngine;
using System;
using System.Collections.Generic;

namespace UFE3D
{
	public abstract class MultiplayerAPI : MonoBehaviour
	{
		#region public delegate definitions: Common Delegates
		public delegate void OnInitializationErrorDelegate();
		public delegate void OnInitializationSuccessfulDelegate();
		public delegate void OnMessageReceivedDelegate(byte[] bytes);
		#endregion

		#region public delegate definitions: Client Delegates
		public delegate void OnDisconnectionDelegate();
		public delegate void OnJoinedDelegate();
		public delegate void OnJoinErrorDelegate();
		#endregion

		#region public delegate definitions: Server Delegates
		public delegate void OnJoinedLobbyDelegate();
		public delegate void OnMatchCreatedDelegate(string matchName);
		public delegate void OnMatchCreationErrorDelegate();
		public delegate void OnMatchDestroyedDelegate();
		public delegate void OnPlayerConnectedToMatchDelegate(string playerName);
		public delegate void OnPlayerDisconnectedFromMatchDelegate();
		#endregion

		#region public event definitions: Common Events
		public event OnInitializationErrorDelegate OnInitializationError;
		public event OnInitializationSuccessfulDelegate OnInitializationSuccessful;
		public event OnMessageReceivedDelegate OnMessageReceived;
		#endregion

		#region public class event definitions: Client Events
		public event OnDisconnectionDelegate OnDisconnection;
		public event OnJoinedDelegate OnJoined;
		public event OnJoinErrorDelegate OnJoinError;
		#endregion

		#region public event definitions: Server Events
		public event OnJoinedLobbyDelegate OnJoinedLobby;
		public event OnMatchCreatedDelegate OnMatchCreated;
		public event OnMatchCreationErrorDelegate OnMatchCreationError;
		public event OnMatchDestroyedDelegate OnMatchDestroyed;
		public event OnPlayerConnectedToMatchDelegate OnPlayerConnectedToMatch;
		public event OnPlayerDisconnectedFromMatchDelegate OnPlayerDisconnectedFromMatch;
		#endregion

		#region public abstract properties
		public abstract int Connections { get; }
		public abstract float SendRate { get; set; }
		#endregion

		#region private instance fields
		protected string _uuid = null;
		#endregion

		#region public instance methods
		public virtual void Initialize(string uuid)
		{
			if (uuid != null)
			{
				this._uuid = uuid;
				this.RaiseOnInitializationSuccessful();
			}
			else
			{
				this.RaiseOnInitializationError();
			}
		}
		#endregion

		#region public abstract methods
		// Client
		public abstract void LeaveMatch();
		public abstract void JoinMatch(string match);
		public abstract void JoinRandomOrCreateRoom();
		public abstract void CreateMatch(string match);

		// Common
		public abstract NetworkState GetConnectionState();
		public abstract int GetLastPing();
		public abstract void SetRegion(string region);
		public abstract bool IsInsideLobby();
		public abstract void Connect();

		// Server
		public abstract void Disconnect();
		#endregion

		#region public instance methods
		public bool IsClient()
		{
			return this.GetConnectionState() == NetworkState.Client;
		}


		public bool IsConnectedToGame()
		{
			return this.GetConnectionState() != NetworkState.Disconnected;
		}

		public bool IsServer()
		{
			return this.GetConnectionState() == NetworkState.Server;
		}

		public bool SendNetworkMessage<T>(NetworkMessage<T> message)
		{
			return this.SendNetworkMessage(message.Serialize());
		}
		#endregion

		#region protected abstract methods
		protected abstract bool SendNetworkMessage(byte[] bytes);

		protected virtual void RaiseOnInitializationError()
		{
			this.OnInitializationError?.Invoke();
		}

		protected virtual void RaiseOnInitializationSuccessful()
		{
			this.OnInitializationSuccessful?.Invoke();
		}

		protected virtual void RaiseOnMessageReceived(byte[] bytes)
		{
			this.OnMessageReceived?.Invoke(bytes);
		}
		#endregion

		#region protected instance methods: Client Events
		protected virtual void RaiseOnDisconnection()
		{
			this.OnDisconnection?.Invoke();
		}

		protected virtual void RaiseOnJoined()
		{
			this.OnJoined?.Invoke();
		}

		protected virtual void RaiseOnJoinError()
		{
			this.OnJoinError?.Invoke();
		}
		#endregion

		#region protected instance methods: Server Events
		protected virtual void RaiseOnJoinedLobby()
		{
			this.OnJoinedLobby?.Invoke();
		}

		protected virtual void RaiseOnMatchCreated(string match)
		{
			this.OnMatchCreated?.Invoke(match);
		}

		protected virtual void RaiseOnMatchCreationError()
		{
			this.OnMatchCreationError?.Invoke();
		}

		protected virtual void RaiseOnMatchDestroyed()
		{
			this.OnMatchDestroyed?.Invoke();
		}

		protected virtual void RaiseOnPlayerConnectedToMatch(string playerName)
		{
			if (UFE.config.debugOptions.connectionLog) Debug.Log("MultiplayerAPI.RaiseOnPlayerConnectedToMatch");
			this.OnPlayerConnectedToMatch?.Invoke(playerName);
		}

		protected virtual void RaiseOnPlayerDisconnectedFromMatch()
		{
			if (UFE.config.debugOptions.connectionLog) Debug.Log("MultiplayerAPI.RaiseOnPlayerDisconnectedFromMatch");
			this.OnPlayerDisconnectedFromMatch?.Invoke();
		}
		#endregion
	}
}