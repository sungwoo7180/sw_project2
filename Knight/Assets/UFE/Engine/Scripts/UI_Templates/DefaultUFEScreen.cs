using System.Collections.Generic;
using UFE3D;

public class DefaultUFEScreen : UFEScreen
{
    public int OnShowFrameDelay = 3;

    // OnShow is called when this screen is instantiated
    public override void OnShow()
    {
        UFE.DelayLocalAction(OnShowDelayed, OnShowFrameDelay);
    }

    private void OnShowDelayed()
    {
        base.OnShow();
    } 

    #region public override methods
    // DoFixedUpdate is called every frame
    public override void DoFixedUpdate(
        IDictionary<InputReferences, InputEvents> player1PreviousInputs,
        IDictionary<InputReferences, InputEvents> player1CurrentInputs,
        IDictionary<InputReferences, InputEvents> player2PreviousInputs,
        IDictionary<InputReferences, InputEvents> player2CurrentInputs
    )
    {
        base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
    }

    // OnHide is called when the screen is destroyed or replaced
    public override void OnHide() 
    {
        base.OnHide();
    }

    // GoToCustomScreen loads a custom interface from Global Editor -> GUI
    public override void GoToCustomScreen(int screenId)
    {
        base.GoToCustomScreen(screenId);
    }
    #endregion
}
