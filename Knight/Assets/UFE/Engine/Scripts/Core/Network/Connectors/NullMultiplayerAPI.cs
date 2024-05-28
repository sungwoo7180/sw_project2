namespace UFE3D
{
	public class NullMultiplayerAPI : MultiplayerAPI
	{
		#region public override properties
		public override int Connections
		{
			get
			{
				return 0;
			}
		}

		public override float SendRate { get; set; }
		#endregion

		#region public override methods
		// Client
		public override void LeaveMatch()
		{
			this.RaiseOnDisconnection();
		}

		public override void JoinMatch(string match)
		{
			this.RaiseOnJoinError();
		}

		public override void JoinRandomOrCreateRoom()
		{
			this.RaiseOnJoinError();
		}

		// Common
		public override NetworkState GetConnectionState()
		{
			return NetworkState.Disconnected;
		}

		public override int GetLastPing()
		{
			return 0;
		}

		public override void SetRegion(string region)
		{

		}

		// Server
		public override void CreateMatch(string matchName)
		{
			this.RaiseOnMatchCreationError();
		}

		public override bool IsInsideLobby()
        {
			return false;
        }

		public override void Connect()
		{

		}

		public override void Disconnect()
		{

		}
		#endregion

		#region protected override methods
		protected override bool SendNetworkMessage(byte[] bytes)
		{
			return false;
		}
		#endregion
	}
}