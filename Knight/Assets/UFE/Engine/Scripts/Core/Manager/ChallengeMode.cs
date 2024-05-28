using UnityEngine;
using System.Collections.Generic;
using UFE3D;
using FPLibrary;

public class ChallengeMode : UFEScreen {

    /// <summary>List of actions from current challenge.</summary>
    [HideInInspector] public List<ActionSequence> challengeActions;


    /// <summary>Current action within the challenge.</summary>
    [HideInInspector] public int currentAction = 0;


    /// <summary>True if current challenge has been completed.</summary>
    [HideInInspector] public bool complete;

    /// <summary>List of moves to execute in the combo challenge.</summary>
    [HideInInspector] public List<MoveInfo> comboSequence = new List<MoveInfo>();

    public int currentChallenge { get { return UFE.currentChallenge; } set { UFE.currentChallenge = value; } }


    private int executionOnlyMoveCount;
    private string currentMove;

    public void Start() {
        Run();
	}

    public void Run()
    {
        challengeActions = new List<ActionSequence>(UFE.GetChallenge(currentChallenge).actionSequence);

        comboSequence.Clear();
        foreach (ActionSequence actionSeq in challengeActions)
        {
            comboSequence.Add(actionSeq.specialMove);
        }

        UFE.OnMove -= this.OnMove;
        UFE.OnBasicMove -= this.OnBasicMove;
        UFE.OnButton -= this.OnButtonPress;
        UFE.OnMove += this.OnMove;
        UFE.OnBasicMove += this.OnBasicMove;
        UFE.OnButton += this.OnButtonPress;
        UFE.OnHit += this.OnHit;
        complete = false;
        currentAction = 0;
        executionOnlyMoveCount = 0;
    }

    public void Stop() {
        UFE.OnMove -= this.OnMove;
        UFE.OnBasicMove -= this.OnBasicMove;
        UFE.OnButton -= this.OnButtonPress;
        UFE.OnHit -= this.OnHit;
        currentAction = 0;
        executionOnlyMoveCount = 0;
    }
    
    protected virtual void OnMove(MoveInfo move, ControlsScript player) {
        if (move == null) return;

        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.SpecialMove
            && comboSequence[currentAction].id == move.id
            && (!UFE.GetChallenge(currentChallenge).isCombo || challengeActions[currentAction].executionOnly)) {
            currentAction++;
            executionOnlyMoveCount++;
            testChallenge();
        } else if (!UFE.GetChallenge(currentChallenge).isCombo && challengeActions[currentAction].actionType == ActionType.SpecialMove) {
            currentAction = 0;
            executionOnlyMoveCount = 0;
        }
    }

    protected virtual void OnHit(HitBox strokeHitBox, MoveInfo move, Hit hitInfo, ControlsScript player)
    {
        if (move == null) return;
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.SpecialMove
            && testCombo(move, player))
        {
            currentMove = move.id;
            currentAction++;
            testChallenge();
        }
        else if (currentMove != move.id && UFE.GetChallenge(currentChallenge).isCombo && challengeActions[currentAction].actionType == ActionType.SpecialMove)
        {
            currentAction = 0;
            executionOnlyMoveCount = 0;
        }
    }

    private bool testCombo(MoveInfo move, ControlsScript player)
    {
        ControlsScript realPlayer = player;
        if (player.isAssist)
        {
            realPlayer = player.owner;
            if (realPlayer.currentMove != null) move = realPlayer.currentMove;
        }

        if (comboSequence.Count > currentAction && comboSequence[currentAction] == null) 
            return false;

        if (comboSequence[currentAction].id == move.id && (currentAction == executionOnlyMoveCount || realPlayer.opControlsScript.stunTime > 0))
            return true;

        return false;
    }

    protected virtual void OnBasicMove(BasicMoveReference basicMove, ControlsScript player) {
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.BasicMove
            && challengeActions[currentAction].basicMove == basicMove) {
            currentAction++;
            testChallenge();
        } else if (challengeActions[currentAction].actionType == ActionType.BasicMove) {
            currentAction = 0;
        }
    }

    protected virtual void OnButtonPress(ButtonPress buttonPress, ControlsScript player) {
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.ButtonPress
            && challengeActions[currentAction].button == buttonPress) {
            currentAction++;
            testChallenge();
        } else if (challengeActions[currentAction].actionType == ActionType.ButtonPress) {
            currentAction = 0;
        }
    }

    /// <summary>Test if challenge has been successfully completed.</summary>
    protected virtual void testChallenge() 
    {
        if (!complete && currentAction == challengeActions.Count)
        {
            complete = true;

            UFE.FireAlert("Success", UFE.GetPlayer1ControlsScript());

            if (UFE.GetChallenge(currentChallenge).autoMoveNext && UFE.GetChallenge(currentChallenge).challengeSequence == ChallengeAutoSequence.MoveToNext && UFE.GetChallenge(currentChallenge + 1) != null)
            {
                UFE.DelaySynchronizedAction(startNextChallenge, UFE.GetChallenge(currentChallenge).nextChallengeDelay);
            }
            else
            {
                UFE.DelaySynchronizedAction(UFE.MatchManager.EndRound, UFE.GetChallenge(currentChallenge).nextChallengeDelay);
            }

            Stop();
        }
    }


    /// <summary>Starts next challenge immediately.</summary>
    protected virtual void startNextChallenge()
    {
        currentChallenge++;
        UFE.SetChallengeVariables(currentChallenge);

        if (UFE.GetChallenge(currentChallenge - 1).reloadMatch || 
            UFE.GetChallenge(currentChallenge - 1).character.characterName != UFE.GetChallenge(currentChallenge).character.characterName ||
            UFE.GetChallenge(currentChallenge - 1).opCharacter.characterName != UFE.GetChallenge(currentChallenge).opCharacter.characterName)
        {
            UFE.RestartMatch();
        }
        else
        {
            UFE.MatchManager.NewRound();
            UFE.DelaySynchronizedAction(Run, UFE.GetChallenge(currentChallenge).nextChallengeDelay);
        }
    }

    /// <summary>Sets characters and AI defined by current challenge.</summary>
    public static void SetChallengeVariables()
    {
        UFE.config.player1Character = UFE.GetChallenge(UFE.config.selectedChallenge).character;
        UFE.GetPlayer1Controller().isCPU = false;
        UFE.config.player2Character = UFE.GetChallenge(UFE.config.selectedChallenge).opCharacter;
        UFE.GetPlayer2Controller().isCPU = UFE.GetChallenge(UFE.config.selectedChallenge).aiOpponent;
    }
}
