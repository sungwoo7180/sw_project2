using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
	public class MainMenuScreen : UFEScreen
	{
		public Button buttonNetwork;

		public override void OnShow()
		{
			base.OnShow();

			if (buttonNetwork != null)
			{
				buttonNetwork.interactable = UFE.IsNetworkAddonInstalled || UFE.IsBluetoothAddonInstalled;
			}
		}

		public virtual void Quit()
		{
			UFE.Quit();
		}

		public virtual void GoToStoryModeScreen()
		{
			UFE.StartStoryMode();
		}

		public virtual void GoToVersusModeScreen()
		{
			UFE.StartVersusModeScreen();
		}

		public virtual void GoToTrainingModeScreen()
		{
			UFE.StartTrainingMode();
		}

		public virtual void GoToChallengeModeScreen()
		{
			UFE.StartChallengeModeScreen();
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

		public virtual void GoToOptionsScreen()
		{
			UFE.StartOptionsScreen();
		}

		public virtual void GoToCreditsScreen()
		{
			UFE.StartCreditsScreen();
		}
	}
}