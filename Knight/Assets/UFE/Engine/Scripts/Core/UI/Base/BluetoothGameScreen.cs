namespace UFE3D
{
	public class BluetoothGameScreen : UFEScreen
	{
		public virtual void GoToNetworkOptions()
		{
			UFE.StartNetworkOptionsScreen();
		}

		public virtual void HostGame()
		{
			UFE.StartBluetoothHostGameScreen();
		}

		public virtual void JoinGame()
		{
			UFE.StartBluetoothJoinGameScreen();
		}
	}
}