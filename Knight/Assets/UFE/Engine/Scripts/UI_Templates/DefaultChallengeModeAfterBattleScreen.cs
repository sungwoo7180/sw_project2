using UnityEngine;
using System.Collections.Generic;
using UFE3D;
using UnityEngine.UI;

public class DefaultChallengeModeAfterBattleScreen : ChallengeModeAfterBattleScreen
{
    public Button nextChallengeButton;

    #region public override methods
    public override void OnShow()
    {
        base.OnShow();

        if (nextChallengeButton != null && UFE.currentChallenge + 1 >= UFE.config.challengeModeOptions.Length)
        {
            nextChallengeButton.interactable = false;
        }
    }

    // Override constructor and don't call base
    public override void DoFixedUpdate(
        IDictionary<InputReferences, InputEvents> player1PreviousInputs,
        IDictionary<InputReferences, InputEvents> player1CurrentInputs,
        IDictionary<InputReferences, InputEvents> player2PreviousInputs,
        IDictionary<InputReferences, InputEvents> player2CurrentInputs
    )
    { }
    #endregion
}
