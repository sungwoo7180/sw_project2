using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFE3D;

public class DefaultChallengeModeGUI : ChallengeMode
{
    public void OnGUI()
    {

        if (!complete && !UFE.config.lockInputs && !UFE.config.lockMovements)
        {
            GUI.Box(new Rect(20, 100, 250, 400), UFE.GetChallenge(currentChallenge).challengeName);
            GUI.BeginGroup(new Rect(30, 130, 300, 400));
            {
                if (UFE.GetChallenge(currentChallenge).description == "%list%")
                {
                    string newDesc = "";
                    int currAction = 0;
                    foreach (ActionSequence actionSeq in challengeActions)
                    {
                        string moveName = actionSeq.specialMove.moveName;
                        if (currentAction > currAction) moveName += " (DONE)";

                        newDesc += moveName + "\n";
                        currAction++;
                    }
                    GUILayout.Label(newDesc);
                }
                else
                {
                    GUILayout.Label(UFE.GetChallenge(currentChallenge).description);
                }
            }
            GUI.EndGroup();

            if (GUI.Button(new Rect(Screen.width - 120, 50, 70, 30), "Skip"))
            {
                currentAction = challengeActions.Count;
                testChallenge();
            }
        }
    }

    /*
    protected override void startNextChallenge()
    {
        int selectedStage = 0;
        if (currentChallenge == 0) selectedStage = 1; // First challenge = 0
        UFE.SetStage(UFE.config.stages[selectedStage]);
        base.startNextChallenge();
    }*/
}
