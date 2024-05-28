namespace UFE3D
{
    public class PauseScreen : UFEScreen
    {
        public int BackToMenuFrameDelay = 4;

        public virtual void GoToMainMenu()
        {
            // Delay is necessary to avoid double input through the menu
            UFE.DelayLocalAction(GoToMainMenuDelayed, BackToMenuFrameDelay);
        }

        private void GoToMainMenuDelayed()
        {
            UFE.EndGame();
            UFE.StartMainMenuScreen();
            UFE.PauseGame(false);
        }

        public virtual void ResumeGame()
        {
            UFE.PauseGame(false);
        }
    }
}