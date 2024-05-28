namespace UFE3D
{
	public class ChallengeModeScreen : UFEScreen
	{
		public virtual void SelectChallenge(int selection)
		{
			UFE.StartChallengeMode(selection - 1);
		}

		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}
	}
}