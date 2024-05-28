using UnityEngine;
using System.Collections;
using UFE3D;

public class TextureIntroScreen : IntroScreen
{
    public bool skippable = true;
    public float delayBeforeGoingToMenu = 3f;
    public float minDelayBeforeSkipping = 0.1f;

    #region public override methods
    public override void OnShow()
    {
        base.OnShow();

        this.StartCoroutine(this.ShowScreen());
    }

    public virtual IEnumerator ShowScreen()
    {
        float startTime = Time.realtimeSinceStartup;
        float time = 0f;

        while (
            time < this.delayBeforeGoingToMenu &&
            !(skippable && Input.anyKeyDown && time > this.minDelayBeforeSkipping)
        )
        {
            yield return null;
            time = Time.realtimeSinceStartup - startTime;
        }

        this.GoToMainMenu();
    }
    #endregion
}
