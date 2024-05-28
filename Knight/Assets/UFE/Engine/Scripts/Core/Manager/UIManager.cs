using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
	public class UIManager : MonoBehaviour
	{
		public Text debugger;

		private void Start()
		{
			debugger = UFE.DebuggerText(UFE.config.debugOptions.networkDebugger, "Network Debugger", "", UFE.config.debugOptions.netDebugTextPosition, UFE.config.debugOptions.netDebugTextAlignment);
		}

		public void UpdateUIState()
		{
			this.UpdateUIState(null, null, null, null, null, null);
		}

		public void UpdateUIState(
			IDictionary<InputReferences, InputEvents> player1PreviousInputs,
			IDictionary<InputReferences, InputEvents> player1CurrentInputs,
			int? player1SelectedOptions,
			IDictionary<InputReferences, InputEvents> player2PreviousInputs,
			IDictionary<InputReferences, InputEvents> player2CurrentInputs,
			int? player2SelectedOptions
		)
		{

			if (CameraFade.instance.enabled)
			{
				CameraFade.instance.DoFixedUpdate();
			}

			if (UFE.battleGUI != null)
			{
				if (player1SelectedOptions != null)
				{
					UFE.battleGUI.SelectOption(player1SelectedOptions.Value, 1);
				}

				if (player2SelectedOptions != null)
				{
					UFE.battleGUI.SelectOption(player2SelectedOptions.Value, 2);
				}

				UFE.battleGUI.DoFixedUpdate(
					player1PreviousInputs,
					player1CurrentInputs,
					player2PreviousInputs,
					player2CurrentInputs
				);
			}

			if (UFE.IsControlFreak2Installed && UFE.touchControllerBridge != null)
			{
				UFE.touchControllerBridge.DoFixedUpdate();
			}
			else if (UFE.IsControlFreak1Installed)
			{
				if (UFE.gameRunning && UFE.controlFreakPrefab != null && !UFE.controlFreakPrefab.activeSelf)
				{
					UFE.controlFreakPrefab.SetActive(true);
				}
				else if (!UFE.gameRunning && UFE.controlFreakPrefab != null && UFE.controlFreakPrefab.activeSelf)
				{
					UFE.controlFreakPrefab.SetActive(false);
				}
			}

			if (UFE.currentScreen != null)
			{
				if (player1SelectedOptions != null)
				{
					UFE.currentScreen.SelectOption(player1SelectedOptions.Value, 1);
				}

				if (player2SelectedOptions != null)
				{
					UFE.currentScreen.SelectOption(player2SelectedOptions.Value, 2);
				}

				UFE.currentScreen.DoFixedUpdate(
					player1PreviousInputs,
					player1CurrentInputs,
					player2PreviousInputs,
					player2CurrentInputs
				);
			}

			if (UFE.canvasGroup.alpha == 0)
			{
				UFE.canvasGroup.alpha = 1;
			}
		}
	}
}
