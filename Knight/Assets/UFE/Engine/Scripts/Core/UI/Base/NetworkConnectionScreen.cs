using System.Net;

namespace UFE3D
{
	public class NetworkConnectionScreen : UFEScreen
	{
		public override void OnShow()
		{
			base.OnShow();

			UFE.MultiplayerMode = MultiplayerMode.Online;
		}

		public virtual void ConnectToServer()
		{
			UFE.MultiplayerAPI.OnInitializationSuccessful += this.OnInitializationSuccessful;
			UFE.MultiplayerAPI.OnInitializationError += this.OnInitializationError;

			UFE.MultiplayerAPI.Connect();
        }

		public virtual void OnInitializationSuccessful()
		{
			UFE.MultiplayerAPI.OnInitializationSuccessful -= this.OnInitializationSuccessful;
			UFE.MultiplayerAPI.OnInitializationError -= this.OnInitializationError;
		}

		public virtual void OnInitializationError()
		{
			UFE.MultiplayerAPI.OnInitializationSuccessful -= this.OnInitializationSuccessful;
			UFE.MultiplayerAPI.OnInitializationError -= this.OnInitializationError;
		}

		public virtual void GoToDirectMatchScreen()
		{
			// TODO: Add direct IP connection
		}

		public virtual void GoToBluetoothScreen()
		{
			UFE.StartBluetoothGameScreen();
		}

		public virtual void GoToMainMenu()
        {
			UFE.StartMainMenuScreen();
        }
	}
}