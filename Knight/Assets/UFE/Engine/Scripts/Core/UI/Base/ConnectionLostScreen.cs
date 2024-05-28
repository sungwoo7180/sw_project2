namespace UFE3D
{
	public class ConnectionLostScreen : UFEScreen
	{
		public override void OnShow()
		{
			base.OnShow();
		}

		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}

		public virtual void GoToNetworkOptionsScreen()
		{
			if (UFE.MultiplayerAPI.IsInsideLobby())
            {
				UFE.StartNetworkOptionsScreen();
			}
            else
            {
				UFE.StartNetworkConnectionScreen();
            }
		}
	}
}