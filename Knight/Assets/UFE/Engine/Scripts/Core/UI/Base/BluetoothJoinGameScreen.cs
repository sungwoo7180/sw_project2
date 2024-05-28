using UnityEngine;

namespace UFE3D
{
    public class BluetoothJoinGameScreen : JoinGameScreen
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