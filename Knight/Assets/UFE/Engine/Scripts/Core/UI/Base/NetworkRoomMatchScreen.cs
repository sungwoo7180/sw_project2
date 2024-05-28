namespace UFE3D
{
	public class NetworkRoomMatchScreen : UFEScreen
	{
		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}

		public virtual void GoToNetworkOptions()
		{
			UFE.StartNetworkOptionsScreen();
		}

		public virtual void GoToHostGameScreen()
		{
			UFE.StartHostGameScreen();
		}

		public virtual void GoToJoinGameScreen()
		{
			UFE.StartJoinGameScreen();
		}
	}
}