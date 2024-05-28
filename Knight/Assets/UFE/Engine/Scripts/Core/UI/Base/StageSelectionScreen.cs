using UnityEngine;

namespace UFE3D
{
	public class StageSelectionScreen : UFEScreen
	{
		public bool fadeBeforeGoingToLoadingBattleScreen = false;

		protected int stageHoverIndex = 0;

		#region public instance methods
		public virtual void GoToCharacterSelectionScreen()
		{
			if (UFE.gameMode == GameMode.NetworkGame)
			{
				UFE.config.selectedStage = null;
				this.TrySelectStage(-1);
			}
			else
			{
				this.StartLoadingCharacterSelectionScreen();
			}
		}

		public virtual void GoToLoadingBattleScreen()
		{
			this.StartLoadingBattleScreen();
		}

		public virtual void SetHoverIndex(int stageIndex)
		{
			if (stageIndex >= 0 && stageIndex < UFE.config.stages.Length)
			{
				this.stageHoverIndex = stageIndex;
			}
		}

		public void OnStageSelectionAllowed(int stageIndex)
		{
			if (stageIndex >= 0 && stageIndex < UFE.config.stages.Length)
			{
				if (this.selectSound != null) UFE.PlaySound(this.selectSound);
				this.SetHoverIndex(stageIndex);

				UFE.config.selectedStage = UFE.config.stages[stageIndex];
				this.StartLoadingBattleScreen();
			}
			else if (stageIndex < 0)
			{
				if (UFE.config.selectedStage != null)
				{
					if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
					UFE.config.selectedStage = null;
				}
				else
				{
					if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
					this.StartLoadingCharacterSelectionScreen();
				}
			}
		}

		public void TryDeselectStage()
		{
			this.TrySelectStage(-1);
		}

		public void TrySelectStage()
		{
			this.TrySelectStage(this.stageHoverIndex);
		}

		public void TrySelectStage(int stageIndex)
		{
			// Check if he was playing online or not...
			if (!UFE.IsConnected)
			{
				// If it's a local game, update the corresponding stage immediately...
				this.OnStageSelectionAllowed(stageIndex);
			}
			else
			{
				// If it's an online game, we only select the stage if it has been requested by Player 1...
				// But if player 2 wants to come back to character selection screen, we also allow that...
				int localPlayer = UFE.GetLocalPlayer();
				if (localPlayer == 1 || stageIndex < 0)
				{
					// We don't invoke the OnstageSelected() method immediately because we are using the frame-delay 
					// algorithm to keep players synchronized, so we can't invoke the OnstageSelected() method
					// until the other player has received the message with our choice.
					UFE.FluxCapacitor.RequestOptionSelection(localPlayer, (sbyte)stageIndex);
				}
			}
		}
		#endregion

		#region public override methods
		public override void OnShow()
		{
			base.OnShow();
			UFE.config.selectedStage = null;
		}

		public override void SelectOption(int option, int player)
		{
			this.OnStageSelectionAllowed(option);
		}
		#endregion

		#region protected instance method
		protected virtual void StartLoadingCharacterSelectionScreen()
		{
			UFE.StartCharacterSelectionScreen();
		}

		protected virtual void StartLoadingBattleScreen()
		{
			if (this.fadeBeforeGoingToLoadingBattleScreen)
			{
				UFE.StartLoadingBattleScreen();
			}
			else
			{
				UFE.StartLoadingBattleScreen(0f);
			}
		}
		#endregion
	}
}