using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
	public class NetworkOptionsScreen : UFEScreen
	{
		public Text inputDelayText;
		public Slider inputDelaySlider;
		public Toggle rollbackToggle;

        public override void OnShow()
		{
			base.OnShow();
			
			if (inputDelaySlider != null)
			{
				inputDelaySlider.value = UFE.config.networkInputDelay;
			}
			
			if (rollbackToggle != null)
			{
				rollbackToggle.isOn = UFE.config.rollbackEnabled;
			}
		}

		public virtual void GoToRandomMatchScreen()
		{
			UFE.StartSearchMatchScreen();
		}

		public virtual void GoToRoomMatchScreen()
		{
			UFE.StartRoomMatchScreen();
		}

		public virtual void SetNetworkInputDelay(Slider slider)
		{
			int inputDelay = Mathf.RoundToInt(slider.value);
			if (inputDelayText != null)
				inputDelayText.text = "Input Delay:" + inputDelay;

			UFE.SetNetworkInputDelay(inputDelay);
		}

		public virtual void ToggleNetworkRollback()
		{
			if (rollbackToggle != null)
				UFE.SetNetworkRollback(rollbackToggle.isOn);
		}

		public virtual void Disconnect()
		{
			UFE.MultiplayerAPI.Disconnect();
			UFE.StartNetworkConnectionScreen();
		}

		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}

		public virtual string GetIp()
		{
			string hostName = System.Net.Dns.GetHostName();
			IPHostEntry ipHostEntry = System.Net.Dns.GetHostEntry(hostName);
			IPAddress[] ipAddresses = ipHostEntry.AddressList;

			return ipAddresses[^1].ToString();
		}
	}
}