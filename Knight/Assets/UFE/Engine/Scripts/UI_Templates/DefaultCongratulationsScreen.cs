using UnityEngine;
using System.Collections;
using UFE3D;

public class DefaultCongratulationsScreen : StoryModeScreen
{
    public AudioClip congratulationsSound;
    public float delayBeforeLoadingNextScreen = 3f;

    #region public override methods
    public override void OnShow()
    {
        base.OnShow();

        UFE.DelaySynchronizedAction(this.GoToNextScreen, this.delayBeforeLoadingNextScreen);
    }
    #endregion
}
