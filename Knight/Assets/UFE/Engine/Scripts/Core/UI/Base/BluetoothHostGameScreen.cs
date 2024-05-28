using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
    public class BluetoothHostGameScreen : HostGameScreen
	{
		#region public override methods
		public override void OnShow()
		{
			base.OnShow();

			// Set Multiplayer Mode to "Bluetooth"
			UFE.MultiplayerMode = MultiplayerMode.Bluetooth;
		}
		#endregion
	}
}