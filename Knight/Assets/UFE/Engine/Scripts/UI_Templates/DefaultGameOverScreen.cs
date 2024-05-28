using UnityEngine;
using System.Collections;
using UFE3D;

public class DefaultGameOverScreen : StoryModeScreen
{
    public float delayBeforeLoadingNextScreen = 3f;

    #region public override methods
    public override void OnShow()
    {
        base.OnShow();

        UFE.DelaySynchronizedAction(this.GoToNextScreen, delayBeforeLoadingNextScreen);
    }
    #endregion
}
