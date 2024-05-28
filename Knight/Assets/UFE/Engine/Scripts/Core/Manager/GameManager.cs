using System;
using System.Collections.Generic;

namespace UFE3D
{
    public class GameManager
    {
		public void DoFixedUpdate()
        {
			if (UFE.ConnectionHandler != null && UFE.ConnectionHandler.HasStarted)
            {
				UFE.FluxCapacitor.DoFixedUpdate();
            }
			else
            {
                UFE.FluxCapacitor.ReadInputs(GetInputDelay(), false);
                UpdateGameState(UFE.currentFrame);
				UFE.FluxCapacitor.ClearBuffer(UFE.currentFrame);
			}
        }

        private static int GetInputDelay()
        {
            int frameDelay = 0;
            if (UFE.IsNetworkAddonInstalled && UFE.config.networkOptions.applyFrameDelayOffline)
            {
                if (UFE.config.networkOptions.frameDelayType == NetworkFrameDelay.Auto)
                {
                    frameDelay = UFE.config.networkOptions.minFrameDelay;
                }
                else
                {
                    frameDelay = UFE.config.networkInputDelay;
                }
            }
			return frameDelay;
        }

        public void UpdateGameState(long currentFrame)
		{
			//-------------------------------------------------------------------------------------------------------------
			// Retrieve the player 1 input in the previous frame
			//-------------------------------------------------------------------------------------------------------------
			UFEController player1Controller = UFE.FluxCapacitor.PlayerManager.player1.inputController;

			FrameInput? player1PreviousFrameInput;
			bool foundPlayer1PreviousFrameInput =
				UFE.FluxCapacitor.PlayerManager.TryGetInput(1, currentFrame - 1, out player1PreviousFrameInput) &&
				player1PreviousFrameInput != null;

			if (!foundPlayer1PreviousFrameInput) player1PreviousFrameInput = new FrameInput(FrameInput.NullSelectedOption);

			Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player1PreviousTuple =
				player1Controller.inputReferences.GetInputEvents(player1PreviousFrameInput.Value);

			IDictionary<InputReferences, InputEvents> player1PreviousInputs = player1PreviousTuple.Item1;
			sbyte? player1PreviousSelectedOption = player1PreviousTuple.Item2;


			//-------------------------------------------------------------------------------------------------------------
			// Retrieve the player 1 input in the current frame
			//-------------------------------------------------------------------------------------------------------------
			FrameInput? player1CurrentFrameInput;
			bool foundPlayer1CurrentFrameInput =
				UFE.FluxCapacitor.PlayerManager.TryGetInput(1, currentFrame, out player1CurrentFrameInput) &&
				player1CurrentFrameInput != null;

			if (!foundPlayer1CurrentFrameInput) player1CurrentFrameInput = new FrameInput(FrameInput.NullSelectedOption);

			Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player1CurrentTuple =
				player1Controller.inputReferences.GetInputEvents(player1CurrentFrameInput.Value);

			IDictionary<InputReferences, InputEvents> player1CurrentInputs = player1CurrentTuple.Item1;
			sbyte? player1CurrentSelectedOption = player1CurrentTuple.Item2;

			int? player1SelectedOptions = null;
			if (player1CurrentSelectedOption != null && player1CurrentSelectedOption != player1PreviousSelectedOption)
			{
				player1SelectedOptions = player1CurrentSelectedOption;
			}


			//-------------------------------------------------------------------------------------------------------------
			// Retrieve the player 2 input in the previous frame
			//-------------------------------------------------------------------------------------------------------------
			UFEController player2Controller = UFE.FluxCapacitor.PlayerManager.player2.inputController;

			FrameInput? player2PreviousFrameInput;
			bool foundPlayer2PreviousFrameInput =
				UFE.FluxCapacitor.PlayerManager.TryGetInput(2, currentFrame - 1, out player2PreviousFrameInput) &&
				player2PreviousFrameInput != null;

			if (!foundPlayer2PreviousFrameInput) player2PreviousFrameInput = new FrameInput(FrameInput.NullSelectedOption);

			Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player2PreviousTuple =
				player2Controller.inputReferences.GetInputEvents(player2PreviousFrameInput.Value);

			IDictionary<InputReferences, InputEvents> player2PreviousInputs = player2PreviousTuple.Item1;
			sbyte? player2PreviousSelectedOption = player2PreviousTuple.Item2;


			//-------------------------------------------------------------------------------------------------------------
			// Retrieve the player 2 input in the current frame
			//-------------------------------------------------------------------------------------------------------------
			FrameInput? player2CurrentFrameInput;
			bool foundPlayer2CurrentFrameInput =
				UFE.FluxCapacitor.PlayerManager.TryGetInput(2, currentFrame, out player2CurrentFrameInput) &&
				player2CurrentFrameInput != null;

			if (!foundPlayer2CurrentFrameInput) player2CurrentFrameInput = new FrameInput(FrameInput.NullSelectedOption);

			Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> player2CurrentTuple =
				player2Controller.inputReferences.GetInputEvents(player2CurrentFrameInput.Value);

			IDictionary<InputReferences, InputEvents> player2CurrentInputs = player2CurrentTuple.Item1;
			sbyte? player2CurrentSelectedOption = player2CurrentTuple.Item2;

			int? player2SelectedOptions = null;
			if (player2CurrentSelectedOption != null && player2CurrentSelectedOption != player2PreviousSelectedOption)
			{
				player2SelectedOptions = player2CurrentSelectedOption;
			}


			//-------------------------------------------------------------------------------------------------------------
			// Set the Random Seed
			//-------------------------------------------------------------------------------------------------------------
			UnityEngine.Random.InitState((int)currentFrame);


			//-------------------------------------------------------------------------------------------------------------
			// Before updating the state of the game, save the current state and the input that will be applied 
			// to reach the next frame state
			//-------------------------------------------------------------------------------------------------------------
			FluxStates currentState = FluxStateTracker.SaveGameState(currentFrame);
			UFE.FluxCapacitor.History.TrySetState(
				currentState,
				new FluxFrameInput(
					player1PreviousFrameInput.Value,
					player1CurrentFrameInput.Value,
					player2PreviousFrameInput.Value,
					player2CurrentFrameInput.Value
				)
			);


			//-------------------------------------------------------------------------------------------------------------
			// Update Game State and Synch Delayed Actions
			//-------------------------------------------------------------------------------------------------------------
			if (!UFE.IsPaused())
			{
				if (UFE.MatchManager != null)
					UFE.MatchManager.UpdateMatchState(currentFrame, player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

				UFE.ExecuteSynchronizedDelayedActions();
			}


			//-------------------------------------------------------------------------------------------------------------
			// Execute Local Delayed Actions
			//-------------------------------------------------------------------------------------------------------------
			UFE.ExecuteLocalDelayedActions();


			//-------------------------------------------------------------------------------------------------------------
			// Update UI State
			//-------------------------------------------------------------------------------------------------------------
			UFE.UIManager.UpdateUIState(
				player1PreviousInputs,
				player1CurrentInputs,
				player1SelectedOptions,
				player2PreviousInputs,
				player2CurrentInputs,
				player2SelectedOptions
			);


			//-------------------------------------------------------------------------------------------------------------
			// Update Inputs
			//-------------------------------------------------------------------------------------------------------------
			UFE.FluxCapacitor.PlayerManager.player1.inputController.DoFixedUpdate();
			UFE.FluxCapacitor.PlayerManager.player2.inputController.DoFixedUpdate();


			//-------------------------------------------------------------------------------------------------------------
			// Finally, increment the frame count
			//-------------------------------------------------------------------------------------------------------------
			UFE.currentFrame = currentFrame + 1;
		}
    }
}
