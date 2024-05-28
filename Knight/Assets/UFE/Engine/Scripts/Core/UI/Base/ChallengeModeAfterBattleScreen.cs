namespace UFE3D
{
	public class ChallengeModeAfterBattleScreen : UFEScreen
	{
		#region public instance methods
		public virtual void GoToMainMenu()
		{
			UFE.EndGame();
			UFE.StartMainMenuScreen();
			UFE.PauseGame(false);
		}

		public virtual void RepeatChallenge()
		{
			UFE.StartLoadingBattleScreen();
		}

		public virtual void NextChallenge()
		{
			UFE.NextChallenge();
			UFE.SetChallengeVariables();
			UFE.StartLoadingBattleScreen();
		}
		#endregion
	}
}