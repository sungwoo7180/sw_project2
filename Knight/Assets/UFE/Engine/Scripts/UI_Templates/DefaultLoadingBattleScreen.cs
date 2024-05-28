using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UFE3D;

public class DefaultLoadingBattleScreen : LoadingBattleScreen
{
    public float delayBeforePreload = .5f;
    public float delayAfterPreload = .5f;
    public Text namePlayer1;
    public Text namePlayer2;
    public Text nameStage;
    public Image portraitPlayer1;
    public Image portraitPlayer2;
    public Image screenshotStage;

    #region public override methods
    public override void OnShow()
    {
        base.OnShow();

        if (UFE.config.player1Character != null)
        {
            if (this.portraitPlayer1 != null)
            {
                this.portraitPlayer1.sprite = Sprite.Create(
                    UFE.config.player1Character.profilePictureBig,
                    new Rect(0f, 0f, UFE.config.player1Character.profilePictureBig.width, UFE.config.player1Character.profilePictureBig.height),
                    new Vector2(0.5f * UFE.config.player1Character.profilePictureBig.width, 0.5f * UFE.config.player1Character.profilePictureBig.height)
                );
            }

            if (this.namePlayer1 != null)
            {
                this.namePlayer1.text = UFE.config.player1Character.characterName;
            }
        }

        if (UFE.config.player2Character != null)
        {
            if (this.portraitPlayer2 != null)
            {
                this.portraitPlayer2.sprite = Sprite.Create(
                    UFE.config.player2Character.profilePictureBig,
                    new Rect(0f, 0f, UFE.config.player2Character.profilePictureBig.width, UFE.config.player2Character.profilePictureBig.height),
                    new Vector2(0.5f * UFE.config.player2Character.profilePictureBig.width, 0.5f * UFE.config.player2Character.profilePictureBig.height)
                );
            }

            if (this.namePlayer2 != null)
            {
                this.namePlayer2.text = UFE.config.player2Character.characterName;
            }
        }

        if (UFE.config.selectedStage != null)
        {
            if (this.screenshotStage != null)
            {
                this.screenshotStage.sprite = Sprite.Create(
                    UFE.config.selectedStage.screenshot,
                    new Rect(0f, 0f, UFE.config.selectedStage.screenshot.width, UFE.config.selectedStage.screenshot.height),
                    new Vector2(0.5f * UFE.config.selectedStage.screenshot.width, 0.5f * UFE.config.selectedStage.screenshot.height)
                );

                Animator anim = this.screenshotStage.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.enabled = UFE.gameMode != GameMode.StoryMode;
                }
            }

            /*if (this.nameStage != null){
				this.nameStage.text = UFE.config.selectedStage.stageName;
			}*/
        }

        UFE.DelayLocalAction(UFE.PreloadBattle, this.delayBeforePreload);
        UFE.DelayLocalAction(this.StartBattle, UFE.config._preloadingTime);
    }
    #endregion
}
