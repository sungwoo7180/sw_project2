using UnityEngine;
using System.Collections;
using UFE3D;

public class StoryModeTextureScreen : StoryModeScreen
{
    public bool skippable = true;
    public float delayBeforeGoingToNextScreen = 3f;
    public float minDelayBeforeSkipping = 0.1f;

    #region public override methods
    public override void OnShow()
    {
        base.OnShow();

        this.StartCoroutine(this.ShowScreen());
    }
    #endregion

    public virtual IEnumerator ShowScreen()
    {
        float startTime = Time.realtimeSinceStartup;
        float time = 0f;

        while (
            time < this.delayBeforeGoingToNextScreen &&
            !(skippable && Input.anyKeyDown && time > this.minDelayBeforeSkipping)
        )
        {
            yield return null;
            time = Time.realtimeSinceStartup - startTime;
        }

        this.GoToNextScreen();
    }
}
