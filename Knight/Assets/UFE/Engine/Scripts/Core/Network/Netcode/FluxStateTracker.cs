using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UFENetcode;
using FPLibrary;
using UFE3D;

public class FluxStateTracker {

    public static FluxGameHistory LoadGameState(FluxGameHistory history, long frame) {
        FluxStates gameState;

        if (history.TryGetState(frame, out gameState)) {
            LoadGameState(gameState);
        } else {
            throw new ArgumentOutOfRangeException(
                "frame"
                ,
                frame
                ,
                string.Format(
                    "The frame value should be between {0} and {1}.",
                    history.FirstStoredFrame,
                    history.LastStoredFrame
                )
            );
        }

        return history;
    }

    public static void LoadGameState(FluxStates gameState) {
        LoadGameState(gameState, UFE.config.networkOptions.ufeTrackers);
    }

    public static void LoadGameState(FluxStates gameState, bool loadTrackers) {
        // Static Variables
        UFE.currentFrame = gameState.NetworkFrame;
        UFE.freeCamera = gameState.global.freeCamera;
        UFE.freezePhysics = gameState.global.freezePhysics;
        UFE.newRoundCasted = gameState.global.newRoundCasted;
        UFE.normalizedCam = gameState.global.normalizedCam;
        UFE.pauseTimer = gameState.global.pauseTimer;
        UFE.timer = gameState.global.timer;
        UFE.timeScale = gameState.global.timeScale;


        // Delayed Synchornized Actions
        UFE.delayedSynchronizedActions = new List<DelayedAction>();
        foreach (DelayedAction dAction in gameState.global.delayedActions) {
            UFE.delayedSynchronizedActions.Add(new DelayedAction(dAction.action, dAction.steps));
        }


        // Instantiated Objects
        foreach (FluxStates.InstantiatedGameObjectState state in gameState.global.instantiatedObjects)
        {
            bool objFound = false;
            foreach (InstantiatedGameObject iGameObject in UFE.instantiatedObjects)
            {
                if (iGameObject.gameObject == null) continue;
                if (state.id == iGameObject.id && state.gameObject != null)
                {
                    objFound = true;
                    iGameObject.creationFrame = state.creationFrame;
                    iGameObject.destructionFrame = state.destructionFrame;
                    iGameObject.gameObject.transform.localPosition = state.transformState.localPosition;
                    iGameObject.gameObject.transform.localRotation = state.transformState.localRotation;
                    iGameObject.gameObject.transform.localScale = state.transformState.localScale;
                    iGameObject.gameObject.transform.position = state.transformState.position;
                    iGameObject.gameObject.transform.rotation = state.transformState.rotation;
                    iGameObject.gameObject.SetActive(state.transformState.active);
                    if (iGameObject.mrFusion != null)
                        iGameObject.mrFusion.LoadState(UFE.currentFrame);
                    break;
                }
            }

            if (!objFound)
            {
                // If current object listed is not instantiated, create a new one
                /*GameObject goInstance = UFE.SpawnGameObject(state.gameObject, state.transformState.position, state.transformState.rotation, state.creationFrame - state.destructionFrame, state.mrFusion != null, state.id);

                MrFusion mrFusion = (MrFusion)goInstance.GetComponent(typeof(MrFusion));

                if (mrFusion != null && goInstance.gameObject.activeSelf)
                    mrFusion.LoadState(UFE.currentFrame);*/
            }
        }

        foreach (InstantiatedGameObject iGameObject in UFE.instantiatedObjects)
        {
            if (iGameObject.gameObject == null) continue;
            bool objFound = false;

            foreach (FluxStates.InstantiatedGameObjectState state in gameState.global.instantiatedObjects)
            {
                if (state.id == iGameObject.id && state.gameObject != null)
                {
                    objFound = true;
                }
            }

            if (!objFound)
            {
                // If current instantiated object is not listed in this state, set creation and destruction frame to 0 so it doesn't appear in this "reality"
                iGameObject.creationFrame = 0;
                iGameObject.destructionFrame = 0;
                iGameObject.gameObject.SetActive(false);
            }
        }


        // UFE Config Instance
        UFE.config.currentRound = gameState.global.currentRound;
        UFE.config.lockInputs = gameState.global.lockInputs;
        UFE.config.lockMovements = gameState.global.lockMovements;


        // Camera
        Camera.main.enabled = gameState.camera.enabled;
        Camera.main.fieldOfView = gameState.camera.fieldOfView;
        Camera.main.transform.localPosition = gameState.camera.localPosition;
        Camera.main.transform.localRotation = gameState.camera.localRotation;
        Camera.main.transform.position = gameState.camera.position;
        Camera.main.transform.rotation = gameState.camera.rotation;

        if (gameState.camera.cameraScript && UFE.CameraScript == null && UFE.GameEngine != null) {
            UFE.CameraScript = UFE.GameEngine.AddComponent<CameraScript>();
        }

        if (UFE.CameraScript != null) {
            if (gameState.camera.cameraScript) {
                UFE.CameraScript.cinematicFreeze = gameState.camera.cinematicFreeze;
                UFE.CameraScript.currentLookAtPosition = gameState.camera.currentLookAtPosition;
                UFE.CameraScript.freeCameraSpeed = gameState.camera.freeCameraSpeed;
                UFE.CameraScript.currentOwner = gameState.camera.lastOwner;
                UFE.CameraScript.killCamMove = gameState.camera.killCamMove;
                UFE.CameraScript.movementSpeed = gameState.camera.movementSpeed;
                UFE.CameraScript.rotationSpeed = gameState.camera.rotationSpeed;
                UFE.CameraScript.defaultDistance = gameState.camera.defaultDistance;
                UFE.CameraScript.standardGroundHeight = gameState.camera.standardGroundHeight;
                UFE.CameraScript.targetPosition = gameState.camera.targetPosition;
                UFE.CameraScript.targetRotation = gameState.camera.targetRotation;
                UFE.CameraScript.targetFieldOfView = gameState.camera.targetFieldOfView;
            } else {
                GameObject.Destroy(UFE.CameraScript);
            }
        }
        
        
        // Characters
        List<ControlsScript> allScripts = UFE.GetAllControlsScripts();
        for (int i = 0; i < gameState.allCharacterStates.Count; ++i)
        {
            LoadCharacterState(gameState.allCharacterStates[i], allScripts[i]);
        }


        // Battle UI
        if (UFE.battleGUI != null)
        {
            (UFE.battleGUI as BattleGUI).RefreshInputs(gameState.battleGUI.player1InputIcons, 1);
            (UFE.battleGUI as BattleGUI).RefreshInputs(gameState.battleGUI.player2InputIcons, 2);
        }


        // Load every variable through the auto tracker
        if (loadTrackers) UFE.UFEInstance = RecordVar.LoadStateTrackers(UFE.UFEInstance, gameState.tracker) as UFE;
    }

    public static FluxStates SaveGameState(long frame) {
        return SaveGameState(frame, false);
    }

    public static FluxStates SaveGameState(long frame, bool saveTrackers) {
        FluxStates gameState = new FluxStates();
        gameState.NetworkFrame = frame;

        // Global
        gameState.global.freeCamera = UFE.freeCamera;
        gameState.global.freezePhysics = UFE.freezePhysics;
        gameState.global.newRoundCasted = UFE.newRoundCasted;
        gameState.global.normalizedCam = UFE.normalizedCam;
        gameState.global.pauseTimer = UFE.pauseTimer;
        gameState.global.timer = UFE.timer;
        gameState.global.timeScale = UFE.timeScale;

        gameState.global.delayedActions = new List<DelayedAction>();
        foreach (DelayedAction dAction in UFE.delayedSynchronizedActions) {
            gameState.global.delayedActions.Add(new DelayedAction(dAction.action, dAction.steps));
        }

        gameState.global.instantiatedObjects = new List<FluxStates.InstantiatedGameObjectState>();
        foreach (InstantiatedGameObject entry in UFE.instantiatedObjects) {
            FluxStates.InstantiatedGameObjectState goState = new FluxStates.InstantiatedGameObjectState();
            if (entry.gameObject == null) continue;
            goState.id = entry.id;
            goState.gameObject = entry.gameObject;
            goState.mrFusion = entry.mrFusion;
            
            goState.creationFrame = entry.creationFrame;
            goState.destructionFrame = entry.destructionFrame;
            goState.transformState = new FluxStates.TransformState();
            goState.transformState.localScale = entry.gameObject.transform.localScale;
            goState.transformState.position = entry.gameObject.transform.position;
            goState.transformState.rotation = entry.gameObject.transform.rotation;
            goState.transformState.active = entry.gameObject.activeSelf;
            if (goState.mrFusion != null)
                goState.mrFusion.SaveState(frame);

            gameState.global.instantiatedObjects.Add(goState);
        }

        gameState.global.currentRound = UFE.config.currentRound;
        gameState.global.lockInputs = UFE.config.lockInputs;
        gameState.global.lockMovements = UFE.config.lockMovements;

        
        // Camera
        if (Camera.main != null) {
            gameState.camera.enabled = Camera.main.enabled;
            gameState.camera.fieldOfView = Camera.main.fieldOfView;
            gameState.camera.localPosition = Camera.main.transform.localPosition;
            gameState.camera.localRotation = Camera.main.transform.localRotation;
            gameState.camera.position = Camera.main.transform.position;
            gameState.camera.rotation = Camera.main.transform.rotation;

            gameState.camera.cameraScript = UFE.CameraScript != null;
            if (gameState.camera.cameraScript) {
                gameState.camera.cinematicFreeze = UFE.CameraScript.cinematicFreeze;
                gameState.camera.currentLookAtPosition = UFE.CameraScript.currentLookAtPosition;
                gameState.camera.freeCameraSpeed = UFE.CameraScript.freeCameraSpeed;
                gameState.camera.lastOwner = UFE.CameraScript.currentOwner;
                gameState.camera.killCamMove = UFE.CameraScript.killCamMove;
                gameState.camera.movementSpeed = UFE.CameraScript.movementSpeed;
                gameState.camera.rotationSpeed = UFE.CameraScript.rotationSpeed;
                gameState.camera.defaultDistance = UFE.CameraScript.defaultDistance;
                gameState.camera.standardGroundHeight = UFE.CameraScript.standardGroundHeight;
                gameState.camera.targetPosition = UFE.CameraScript.targetPosition;
                gameState.camera.targetRotation = UFE.CameraScript.targetRotation;
                gameState.camera.targetFieldOfView = UFE.CameraScript.targetFieldOfView;
            }
        }


        // Characters
        List<ControlsScript> allScripts = UFE.GetAllControlsScripts();
        gameState.allCharacterStates = new List<FluxStates.CharacterState>();
        foreach(ControlsScript cScript in allScripts)
        {
            gameState.allCharacterStates.Add(SaveCharacterState(cScript));
        }


        // Battle UI
        if (UFE.battleGUI != null)
        {
            gameState.battleGUI.player1InputIcons = CopyUIInputs((UFE.battleGUI as BattleGUI).player1Inputs);
            gameState.battleGUI.player2InputIcons = CopyUIInputs((UFE.battleGUI as BattleGUI).player2Inputs);
        }


        // Save every RecordVar attribute under UFEInterfaces to be used on auto tracker
        if (saveTrackers) gameState.tracker = RecordVar.SaveStateTrackers(UFE.UFEInstance, new Dictionary<System.Reflection.MemberInfo, object>());


        return gameState;
    }

    private static List<FluxStates.InputIconGroupState> CopyUIInputs(List<BattleGUI.InputIconGroup> targetInputGroup)
    {
        List<FluxStates.InputIconGroupState> newInputGroup = new List<FluxStates.InputIconGroupState>();
        foreach (BattleGUI.InputIconGroup inputIcons in targetInputGroup)
        {
            List<FluxStates.InputIconState> newInputIconList = new List<FluxStates.InputIconState>();
            foreach (BattleGUI.InputIcon inputIcon in inputIcons.inputs)
            {
                FluxStates.InputIconState newInput = new FluxStates.InputIconState();
                newInput.isActive = inputIcon.isActive;
                newInput.image = inputIcon.image;
                newInput.sprite = inputIcon.sprite;
                newInputIconList.Add(newInput);
            }

            FluxStates.InputIconGroupState newInputIcons = new FluxStates.InputIconGroupState();
            newInputIcons.inputs = newInputIconList;
            newInputIcons.isActive = inputIcons.isActive;

            newInputGroup.Add(newInputIcons);
        }

        return newInputGroup;
    }

    private static FluxStates.CharacterState.MoveState CopyMove(FluxStates.CharacterState.MoveState moveState, MoveInfo targetMove) {
        moveState.move = targetMove;
        if (targetMove == null) return moveState;

        moveState.kill = targetMove.kill;
        moveState.armorHits = targetMove.armorOptions.hitsTaken;
        moveState.currentFrame = targetMove.currentFrame;
        moveState.overrideStartupFrame = targetMove.overrideStartupFrame;
        moveState.currentTick = targetMove.currentTick;
        moveState.hitConfirmOnBlock = targetMove.hitConfirmOnBlock;
        moveState.hitConfirmOnParry = targetMove.hitConfirmOnParry;
        moveState.hitConfirmOnStrike = targetMove.hitConfirmOnStrike;
        moveState.hitAnimationOverride = targetMove.hitAnimationOverride;
        moveState.standUpOptions = targetMove.standUpOptions;
        moveState.currentFrameData = targetMove.currentFrameData;


        moveState.hitStates = new FluxStates.CharacterState.HitState[targetMove.hits.Length];
        for (int i = 0; i < targetMove.hits.Length; ++i) {
            moveState.hitStates[i].impactList = (targetMove.hits[i].impactList != null) ? targetMove.hits[i].impactList.ToArray() : new ControlsScript[0];
        }
        moveState.frameLinkStates = new bool[targetMove.frameLinks.Length];
        for (int i = 0; i < targetMove.frameLinks.Length; ++i) {
            moveState.frameLinkStates[i] = targetMove.frameLinks[i].cancelable;
        }
        moveState.castedBodyPartVisibilityChange = new bool[targetMove.bodyPartVisibilityChanges.Length];
        for (int i = 0; i < targetMove.bodyPartVisibilityChanges.Length; ++i) {
            moveState.castedBodyPartVisibilityChange[i] = targetMove.bodyPartVisibilityChanges[i].casted;
        }
        moveState.castedProjectile = new bool[targetMove.projectiles.Length];
        for (int i = 0; i < targetMove.projectiles.Length; ++i) {
            moveState.castedProjectile[i] = targetMove.projectiles[i].casted;
        }
        moveState.castedAppliedForce = new bool[targetMove.appliedForces.Length];
        for (int i = 0; i < targetMove.appliedForces.Length; ++i) {
            moveState.castedAppliedForce[i] = targetMove.appliedForces[i].casted;
        }
        moveState.castedOpAppliedForce = new bool[targetMove.opAppliedForces.Length];
        for (int i = 0; i < targetMove.opAppliedForces.Length; ++i) {
            moveState.castedOpAppliedForce[i] = targetMove.opAppliedForces[i].casted;
        }
        moveState.castedGauge = new bool[targetMove.gauges.Length];
        for (int i = 0; i < targetMove.gauges.Length; ++i) {
            moveState.castedGauge[i] = targetMove.gauges[i].casted;
        }
        moveState.castedCharacterAssist = new bool[targetMove.characterAssist.Length];
        for (int i = 0; i < targetMove.characterAssist.Length; ++i) {
            moveState.castedCharacterAssist[i] = targetMove.characterAssist[i].casted;
        }
        moveState.castedMoveParticleEffect = new bool[targetMove.particleEffects.Length];
        for (int i = 0; i < targetMove.particleEffects.Length; ++i) {
            moveState.castedMoveParticleEffect[i] = targetMove.particleEffects[i].casted;
        }
        moveState.castedSlowMoEffect = new bool[targetMove.slowMoEffects.Length];
        for (int i = 0; i < targetMove.slowMoEffects.Length; ++i) {
            moveState.castedSlowMoEffect[i] = targetMove.slowMoEffects[i].casted;
        }
        moveState.castedSoundEffect = new bool[targetMove.soundEffects.Length];
        for (int i = 0; i < targetMove.soundEffects.Length; ++i) {
            moveState.castedSoundEffect[i] = targetMove.soundEffects[i].casted;
        }
        moveState.castedInGameAlert = new bool[targetMove.inGameAlert.Length];
        for (int i = 0; i < targetMove.inGameAlert.Length; ++i) {
            moveState.castedInGameAlert[i] = targetMove.inGameAlert[i].casted;
        }
        moveState.castedStanceChange = new bool[targetMove.stanceChanges.Length];
        for (int i = 0; i < targetMove.stanceChanges.Length; ++i) {
            moveState.castedStanceChange[i] = targetMove.stanceChanges[i].casted;
        }
        moveState.castedCameraMovement = new bool[targetMove.cameraMovements.Length];
        for (int i = 0; i < targetMove.cameraMovements.Length; ++i) {
            moveState.castedCameraMovement[i] = targetMove.cameraMovements[i].casted;
        }
        moveState.cameraOver = new bool[targetMove.cameraMovements.Length];
        for (int i = 0; i < targetMove.cameraMovements.Length; ++i) {
            moveState.cameraOver[i] = targetMove.cameraMovements[i].over;
        }
        moveState.cameraTime = new FPLibrary.Fix64[targetMove.cameraMovements.Length];
        for (int i = 0; i < targetMove.cameraMovements.Length; ++i) {
            moveState.cameraTime[i] = targetMove.cameraMovements[i].time;
        }
        moveState.castedOpponentOverride = new bool[targetMove.opponentOverride.Length];
        for (int i = 0; i < targetMove.opponentOverride.Length; ++i) {
            moveState.castedOpponentOverride[i] = targetMove.opponentOverride[i].casted;
        }

        return moveState;
    }

    private static void CopyMove(ref MoveInfo targetMove, FluxStates.CharacterState.MoveState moveState) {
        targetMove = moveState.move;
        if (targetMove == null) return;

        targetMove.kill = moveState.move.kill;
        targetMove.armorOptions.hitsTaken = moveState.armorHits;
        targetMove.currentFrame = moveState.currentFrame;
        targetMove.overrideStartupFrame = moveState.overrideStartupFrame;
        targetMove.currentTick = moveState.currentTick;
        targetMove.hitConfirmOnBlock = moveState.hitConfirmOnBlock;
        targetMove.hitConfirmOnParry = moveState.hitConfirmOnParry;
        targetMove.hitConfirmOnStrike = moveState.hitConfirmOnStrike;
        targetMove.hitAnimationOverride = moveState.hitAnimationOverride;
        targetMove.standUpOptions = moveState.standUpOptions;
        targetMove.currentFrameData = moveState.currentFrameData;

        for (int i = 0; i < moveState.hitStates.Length; ++i) {
            targetMove.hits[i].impactList = new List<ControlsScript>(moveState.hitStates[i].impactList);
        }
        for (int i = 0; i < moveState.frameLinkStates.Length; ++i) {
            targetMove.frameLinks[i].cancelable = moveState.frameLinkStates[i];
        }
        for (int i = 0; i < moveState.castedBodyPartVisibilityChange.Length; ++i) {
            targetMove.bodyPartVisibilityChanges[i].casted = moveState.castedBodyPartVisibilityChange[i];
        }
        for (int i = 0; i < moveState.castedProjectile.Length; ++i) {
            targetMove.projectiles[i].casted = moveState.castedProjectile[i];
        }
        for (int i = 0; i < moveState.castedAppliedForce.Length; ++i) {
            targetMove.appliedForces[i].casted = moveState.castedAppliedForce[i];
        }
        for (int i = 0; i < moveState.castedOpAppliedForce.Length; ++i) {
            targetMove.opAppliedForces[i].casted = moveState.castedOpAppliedForce[i];
        }
        for (int i = 0; i < moveState.castedGauge.Length; ++i) {
            targetMove.gauges[i].casted = moveState.castedGauge[i];
        }
        for (int i = 0; i < moveState.castedCharacterAssist.Length; ++i) {
            targetMove.characterAssist[i].casted = moveState.castedCharacterAssist[i];
        }
        for (int i = 0; i < moveState.castedMoveParticleEffect.Length; ++i) {
            targetMove.particleEffects[i].casted = moveState.castedMoveParticleEffect[i];
        }
        for (int i = 0; i < moveState.castedSlowMoEffect.Length; ++i) {
            targetMove.slowMoEffects[i].casted = moveState.castedSlowMoEffect[i];
        }
        for (int i = 0; i < moveState.castedSoundEffect.Length; ++i) {
            targetMove.soundEffects[i].casted = moveState.castedSoundEffect[i];
        }
        for (int i = 0; i < moveState.castedInGameAlert.Length; ++i) {
            targetMove.inGameAlert[i].casted = moveState.castedInGameAlert[i];
        }
        for (int i = 0; i < moveState.castedStanceChange.Length; ++i) {
            targetMove.stanceChanges[i].casted = moveState.castedStanceChange[i];
        }
        for (int i = 0; i < moveState.castedCameraMovement.Length; ++i) {
            targetMove.cameraMovements[i].casted = moveState.castedCameraMovement[i];
        }
        for (int i = 0; i < moveState.cameraOver.Length; ++i) {
            targetMove.cameraMovements[i].over = moveState.cameraOver[i];
        }
        for (int i = 0; i < moveState.cameraTime.Length; ++i) {
            targetMove.cameraMovements[i].time = moveState.cameraTime[i];
        }
        for (int i = 0; i < moveState.castedOpponentOverride.Length; ++i) {
            targetMove.opponentOverride[i].casted = moveState.castedOpponentOverride[i];
        }
    }


    protected static void LoadCharacterState(FluxStates.CharacterState state, ControlsScript controlsScript) {
        if (controlsScript != null) {
            if (state.controlsScript) {
                // Character Shell FP Transform
                controlsScript.worldTransform.position = state.shellTransform.fpPosition;
                controlsScript.worldTransform.rotation = state.shellTransform.fpRotation;
                controlsScript.localTransform.position = state.characterTransform.fpPosition;
                controlsScript.localTransform.rotation = state.characterTransform.fpRotation;

                // Character Shell Transform
                controlsScript.transform.position = state.shellTransform.position;
                controlsScript.transform.rotation = state.shellTransform.rotation;

                // Character Transform
                controlsScript.character.transform.localPosition = state.characterTransform.localPosition;
                controlsScript.character.transform.rotation = state.characterTransform.rotation;
                controlsScript.character.transform.localScale = state.characterTransform.localScale;


                // Meters
                controlsScript.currentLifePoints = state.life;
                for (int i = 0; i < state.gauges.Length; ++i) {
                    controlsScript.currentGaugesPoints[i] = state.gauges[i];
                }


                // Control
                controlsScript.SetActive(state.active);
                controlsScript.playerNum = state.playerNum;
                controlsScript.afkTimer = state.afkTimer;
                controlsScript.airJuggleHits = state.airJuggleHits;
                controlsScript.airRecoveryType = state.airRecoveryType;
                controlsScript.applyRootMotion = state.applyRootMotion;
                controlsScript.blockStunned = state.blockStunned;
                controlsScript.comboDamage = state.comboDamage;
                controlsScript.comboHitDamage = state.comboHitDamage;
                controlsScript.comboHits = state.comboHits;
                controlsScript.consecutiveCrumple = state.consecutiveCrumple;
                controlsScript.currentBasicMoveReference = state.currentBasicMove;
                controlsScript.currentDrained = state.currentDrained;
                controlsScript.currentHit = state.currentHit;
                controlsScript.currentHitAnimation = state.currentHitAnimation;
                controlsScript.currentState = state.currentState;
                controlsScript.currentSubState = state.currentSubState;
                controlsScript.DCStance = state.DCStance;
                controlsScript.firstHit = state.firstHit;
                controlsScript.gaugeDPS = state.gaugeDPS;
                controlsScript.gaugeDrainId = state.gaugeDrainId;
                controlsScript.hitDetected = state.hitDetected;
                controlsScript.hitAnimationSpeed = state.hitAnimationSpeed;
                controlsScript.inhibitGainWhileDraining = state.inhibitGainWhileDraining;
                controlsScript.isAirRecovering = state.isAirRecovering;
                controlsScript.isBlocking = state.isBlocking;
                controlsScript.isCrouching = state.isCrouching;
                controlsScript.isDead = state.isDead;
                controlsScript.ignoreCollisionMass = state.ignoreCollisionMass;
                controlsScript.lit = state.lit;
                controlsScript.target = state.lockOnTarget;
                controlsScript.lockXMotion = state.lockXMotion;
                controlsScript.lockYMotion = state.lockYMotion;
                controlsScript.lockZMotion = state.lockZMotion;
                controlsScript.mirror = state.mirror;
                controlsScript.normalizedDistance = state.normalizedDistance;
                controlsScript.normalizedJumpArc = state.normalizedJumpArc;
                controlsScript.outroPlayed = state.outroPlayed;
                controlsScript.potentialBlock = state.potentialBlock;
                controlsScript.potentialParry = state.potentialParry;
                controlsScript.roundMsgCasted = state.roundMsgCasted;
                controlsScript.roundsWon = state.roundsWon;
                controlsScript.shakeCamera = state.shakeCamera;
                controlsScript.shakeCharacter = state.shakeCharacter;
                controlsScript.shakeDensity = state.shakeDensity;
                controlsScript.shakeCameraDensity = state.shakeCameraDensity;
                controlsScript.standUpOverride = state.standUpOverride;
                controlsScript.standardYRotation = state.standardYRotation;
                controlsScript.storedMoveTime = state.storedMoveTime;
                controlsScript.stunTime = state.stunTime;
                controlsScript.totalDrain = state.totalDrain;


                // Sprite Renderer (If Any)
                if (controlsScript.mySpriteRenderer != null)
                    if (controlsScript.mySpriteRenderer.flipX != state.spriteRendererFlipX) controlsScript.mySpriteRenderer.flipX = state.spriteRendererFlipX;


                // Active PullIn
                controlsScript.activePullIn = state.activePullIn.pullIn;
                if (controlsScript.activePullIn != null)
                    controlsScript.activePullIn.position = state.activePullIn.position;


                // Moves
                CopyMove(ref controlsScript.currentMove, state.currentMove);
                CopyMove(ref controlsScript.enterMove, state.enterMove);
                CopyMove(ref controlsScript.exitMove, state.exitMove);
                CopyMove(ref controlsScript.DCMove, state.DCMove);
                CopyMove(ref controlsScript.storedMove, state.storedMove);


                // Physics
                controlsScript.Physics.activeForces = state.physics.activeForces;
                controlsScript.Physics.airTime = state.physics.airTime;
                controlsScript.Physics.angularDirection = state.physics.angularDirection;
                controlsScript.Physics.appliedGravity = state.physics.appliedGravity;
                controlsScript.Physics.currentAirJumps = state.physics.currentAirJumps;
                controlsScript.Physics.freeze = state.physics.freeze;
                controlsScript.Physics.groundBounceTimes = state.physics.groundBounceTimes;
                controlsScript.Physics.horizontalJumpForce = state.physics.horizontalJumpForce;
                controlsScript.Physics.isGroundBouncing = state.physics.isGroundBouncing;
                controlsScript.Physics.isLanding = state.physics.isLanding;
                controlsScript.Physics.isTakingOff = state.physics.isTakingOff;
                controlsScript.Physics.isWallBouncing = state.physics.isWallBouncing;
                controlsScript.Physics.moveDirection = state.physics.moveDirection;
                controlsScript.Physics.overrideAirAnimation = state.physics.overrideAirAnimation;
                controlsScript.Physics.overrideStunAnimation = state.physics.overrideStunAnimation;
                controlsScript.Physics.verticalTotalForce = state.physics.verticalTotalForce;
                controlsScript.Physics.wallBounceTimes = state.physics.wallBounceTimes;


                // Move Set
                controlsScript.MoveSet.ChangeMoveStances(state.moveSet.combatStance);
                controlsScript.MoveSet.totalAirMoves = state.moveSet.totalAirMoves;
                controlsScript.MoveSet.animationPaused = state.moveSet.animationPaused;
                controlsScript.MoveSet.overrideNextBlendingValue = state.moveSet.overrideNextBlendingValue;
                controlsScript.MoveSet.lastTimePress = state.moveSet.lastTimePress;


                // Inputs being held down (charges)
                controlsScript.inputHeldDown = state.inputHeldDown.ToDictionary(entry => entry.Key, entry => entry.Value);


                // Projectiles
                controlsScript.projectiles.Clear();
                foreach (ProjectileMoveScript projectile in state.projectiles) {
                    controlsScript.projectiles.Add(projectile);
                }


                // Buttons Pressed
                controlsScript.MoveSet.lastButtonPresses.Clear();
                foreach (FluxStates.CharacterState.ButtonSequenceState btnRecord in state.moveSet.lastButtonPresses) {
                    controlsScript.MoveSet.lastButtonPresses.Add(new ButtonSequenceRecord(btnRecord.buttonPresses, btnRecord.chargeTime));
                }

                // Cooldown Timers
                controlsScript.MoveSet.lastMovesPlayed = state.moveSet.lastMovesPlayed.ToDictionary(entry => entry.Key, entry => entry.Value);


                // Hit Boxes State
                controlsScript.HitBoxes.isHit = state.hitBoxes.isHit;
                controlsScript.HitBoxes.hitConfirmType = state.hitBoxes.hitConfirmType;
                controlsScript.HitBoxes.collisionBoxSize = state.hitBoxes.collisionBoxSize;
                controlsScript.HitBoxes.inverted = state.hitBoxes.inverted;
                

                // Hit Boxes State - Custom Hit Boxes
                if (state.hitBoxes.customHitBoxes.customHitBoxes != null) {
                    controlsScript.HitBoxes.customHitBoxes = ScriptableObject.CreateInstance<CustomHitBoxesInfo>();
                    controlsScript.HitBoxes.customHitBoxes.clip = state.hitBoxes.customHitBoxes.clip;
                    controlsScript.HitBoxes.customHitBoxes.speed = state.hitBoxes.customHitBoxes.speed;
                    controlsScript.HitBoxes.customHitBoxes.totalFrames = state.hitBoxes.customHitBoxes.totalFrames;
                    controlsScript.HitBoxes.customHitBoxes.customHitBoxes = state.hitBoxes.customHitBoxes.customHitBoxes;
                } else {
                    controlsScript.HitBoxes.customHitBoxes = null;
                }


                bool sizeFits = false;
                // Hit Boxes State - Hit Boxes
                if (controlsScript.HitBoxes.hitBoxes.Length == state.hitBoxes.hitBoxes.Length)
                    sizeFits = true;
                if (!sizeFits)
                    controlsScript.HitBoxes.hitBoxes = new HitBox[state.hitBoxes.hitBoxes.Length];

                for (int i = 0; i < state.hitBoxes.hitBoxes.Length; ++i) {
                    if (!sizeFits)
                    {
                        controlsScript.HitBoxes.hitBoxes[i] = new HitBox();
                        controlsScript.HitBoxes.hitBoxes[i].position = controlsScript.transform;
                    }
                    controlsScript.HitBoxes.hitBoxes[i].hitState = state.hitBoxes.hitBoxes[i].state;
                    controlsScript.HitBoxes.hitBoxes[i].bodyPart = state.hitBoxes.hitBoxes[i].bodyPart;
                    controlsScript.HitBoxes.hitBoxes[i].hide = state.hitBoxes.hitBoxes[i].hide;
                    controlsScript.HitBoxes.hitBoxes[i].defaultVisibility = state.hitBoxes.hitBoxes[i].visibility;
                    controlsScript.HitBoxes.hitBoxes[i].mappedPosition = state.hitBoxes.hitBoxes[i].mappedPosition;
                    controlsScript.HitBoxes.hitBoxes[i].localPosition = state.hitBoxes.hitBoxes[i].localPosition;
                    controlsScript.HitBoxes.hitBoxes[i].shape = state.hitBoxes.hitBoxes[i].shape;
                    controlsScript.HitBoxes.hitBoxes[i].collisionType = state.hitBoxes.hitBoxes[i].collisionType;
                    controlsScript.HitBoxes.hitBoxes[i].type = state.hitBoxes.hitBoxes[i].type;
                    controlsScript.HitBoxes.hitBoxes[i]._rect = state.hitBoxes.hitBoxes[i].rect;
                    controlsScript.HitBoxes.hitBoxes[i]._radius = state.hitBoxes.hitBoxes[i].radius;
                    controlsScript.HitBoxes.hitBoxes[i]._offSet = state.hitBoxes.hitBoxes[i].offSet;
                }


                // Hit Boxes State - Hurt Boxes
                sizeFits = false;
                if (state.hitBoxes.activeHurtBoxes != null) {
                    //if (controlsScript.HitBoxes.activeHurtBoxes != null && controlsScript.HitBoxes.activeHurtBoxes.Length == state.hitBoxes.activeHurtBoxes.Length) 
                    //    sizeFits = true;
                    
                    if (!sizeFits) 
                        controlsScript.HitBoxes.activeHurtBoxes = new HurtBox[state.hitBoxes.activeHurtBoxes.Length];

                    for (int i = 0; i < state.hitBoxes.activeHurtBoxes.Length; ++i) {
                        if (!sizeFits) 
                            controlsScript.HitBoxes.activeHurtBoxes[i] = new HurtBox();
                        controlsScript.HitBoxes.activeHurtBoxes[i].bodyPart = state.hitBoxes.activeHurtBoxes[i].bodyPart;
                        controlsScript.HitBoxes.activeHurtBoxes[i].followXBounds = state.hitBoxes.activeHurtBoxes[i].followXBounds;
                        controlsScript.HitBoxes.activeHurtBoxes[i].followYBounds = state.hitBoxes.activeHurtBoxes[i].followYBounds;
                        controlsScript.HitBoxes.activeHurtBoxes[i].hitBoxDefinitionIndex = state.hitBoxes.activeHurtBoxes[i].hitBoxDefinitionIndex;
                        controlsScript.HitBoxes.activeHurtBoxes[i].isBlock = state.hitBoxes.activeHurtBoxes[i].isBlock;
                        controlsScript.HitBoxes.activeHurtBoxes[i]._offSet = state.hitBoxes.activeHurtBoxes[i].offSet;
                        controlsScript.HitBoxes.activeHurtBoxes[i].position = state.hitBoxes.activeHurtBoxes[i].position;
                        controlsScript.HitBoxes.activeHurtBoxes[i]._radius = state.hitBoxes.activeHurtBoxes[i].radius;
                        controlsScript.HitBoxes.activeHurtBoxes[i]._rect = state.hitBoxes.activeHurtBoxes[i].rect;
                        controlsScript.HitBoxes.activeHurtBoxes[i].rendererBounds = state.hitBoxes.activeHurtBoxes[i].rendererBounds;
                        controlsScript.HitBoxes.activeHurtBoxes[i].shape = state.hitBoxes.activeHurtBoxes[i].shape;
                        controlsScript.HitBoxes.activeHurtBoxes[i].type = state.hitBoxes.activeHurtBoxes[i].type;
                    }
                } else {
                    controlsScript.HitBoxes.activeHurtBoxes = null;
                }


                // Hit Boxes State - Animation Maps
                controlsScript.HitBoxes.bakeSpeed = state.hitBoxes.bakeSpeed;
                controlsScript.HitBoxes.deltaPosition = state.hitBoxes.deltaPosition;
                controlsScript.HitBoxes.animationMaps = new AnimationMap[state.hitBoxes.animationMaps.Length];
                for (int i = 0; i < controlsScript.HitBoxes.animationMaps.Length; ++i) {
                    controlsScript.HitBoxes.animationMaps[i] = state.hitBoxes.animationMaps[i];
                }


                // Hit Boxes State - Block Area
                controlsScript.HitBoxes.blockableArea = state.hitBoxes.blockableArea.blockArea;
                if (controlsScript.HitBoxes.blockableArea != null)
                    controlsScript.HitBoxes.blockableArea.position = state.hitBoxes.blockableArea.position;
                

                // Animator
                if (controlsScript.myInfo.animationType == AnimationType.Mecanim3D || controlsScript.myInfo.animationType == AnimationType.Mecanim2D) {
                    controlsScript.MoveSet.MecanimControl.SetMirror(state.moveSet.animator.currentMirror);

                    controlsScript.MoveSet.MecanimControl.currentAnimationData = controlsScript.MoveSet.MecanimControl.animations[state.moveSet.animator.currentAnimationData.mecanimAnimationIndex];
                    if (controlsScript.MoveSet.MecanimControl.currentAnimationData != null)
                    {
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.normalizedSpeed = state.moveSet.animator.currentAnimationData.normalizedSpeed;
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.normalizedTime = state.moveSet.animator.currentAnimationData.normalizedTime;
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.secondsPlayed = state.moveSet.animator.currentAnimationData.secondsPlayed;
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.ticksPlayed = state.moveSet.animator.currentAnimationData.ticksPlayed;
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.framesPlayed = state.moveSet.animator.currentAnimationData.framesPlayed;
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.realFramesPlayed = state.moveSet.animator.currentAnimationData.realFramesPlayed;
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.timesPlayed = state.moveSet.animator.currentAnimationData.timesPlayed;
                        controlsScript.MoveSet.MecanimControl.currentAnimationData.speed = state.moveSet.animator.currentAnimationData.speed;
                    }

                    // Mecanim Control 3.0 (MC3)
                    /*
                    controlsScript.MoveSet.MecanimControl.mc3Animator.currentInput = state.moveSet.animator.currentInput;
                    controlsScript.MoveSet.MecanimControl.mc3Animator.transitionDuration = state.moveSet.animator.transitionDuration;
                    controlsScript.MoveSet.MecanimControl.mc3Animator.transitionTime = state.moveSet.animator.transitionTime;

                    controlsScript.MoveSet.MecanimControl.mc3Animator.SetWeights(state.moveSet.animator.weightList);
                    controlsScript.MoveSet.MecanimControl.mc3Animator.speedArray = (float[]) state.moveSet.animator.speedList.Clone();
                    controlsScript.MoveSet.MecanimControl.mc3Animator.timeArray = (float[]) state.moveSet.animator.timeList.Clone();
                    controlsScript.MoveSet.MecanimControl.mc3Animator.SetController(state.moveSet.animator.currentInput);
                    controlsScript.MoveSet.MecanimControl.mc3Animator.Update(0);
                    */

                    // Mecanim Control 1.0
                    controlsScript.MoveSet.MecanimControl.currentState = state.moveSet.animator.currentState;
                    controlsScript.MoveSet.MecanimControl.currentSpeed = state.moveSet.animator.currentSpeed;

                    AnimatorOverrideController overrideController = new AnimatorOverrideController();
                    overrideController.runtimeAnimatorController = controlsScript.MoveSet.MecanimControl.controller;
                    overrideController[state.moveSet.animator.currentState] = controlsScript.MoveSet.MecanimControl.currentAnimationData.clip;

                    controlsScript.MoveSet.MecanimControl.animator.runtimeAnimatorController = overrideController;
                    controlsScript.MoveSet.MecanimControl.animator.Play(state.moveSet.animator.currentState, 0, (float)state.moveSet.animator.currentAnimationData.normalizedTime);
                    controlsScript.MoveSet.MecanimControl.animator.applyRootMotion = controlsScript.MoveSet.MecanimControl.currentAnimationData.applyRootMotion;
                    controlsScript.MoveSet.MecanimControl.animator.Update((float)UFE.fixedDeltaTime);
                    controlsScript.MoveSet.MecanimControl.SetSpeed(state.moveSet.animator.currentSpeed);

                } else {
                    controlsScript.MoveSet.LegacyControl.currentMirror = state.moveSet.animator.currentMirror;
                    controlsScript.MoveSet.LegacyControl.globalSpeed = state.moveSet.animator.globalSpeed;
                    controlsScript.MoveSet.LegacyControl.lastPosition = state.moveSet.animator.lastPosition;

                    controlsScript.MoveSet.LegacyControl.currentAnimationData = state.moveSet.animator.currentAnimationData.legacyAnimationData;
                    if (controlsScript.MoveSet.LegacyControl.currentAnimationData != null) {
                        controlsScript.MoveSet.LegacyControl.animator.Play(controlsScript.MoveSet.LegacyControl.currentAnimationData.clipName);

                        controlsScript.MoveSet.LegacyControl.currentAnimationData.animState.time = (float)state.moveSet.animator.currentAnimationData.secondsPlayed;

                        controlsScript.MoveSet.LegacyControl.currentAnimationData.normalizedSpeed = state.moveSet.animator.currentAnimationData.normalizedSpeed;
                        controlsScript.MoveSet.LegacyControl.currentAnimationData.normalizedTime = state.moveSet.animator.currentAnimationData.normalizedTime;
                        controlsScript.MoveSet.LegacyControl.currentAnimationData.secondsPlayed = state.moveSet.animator.currentAnimationData.secondsPlayed;
                        controlsScript.MoveSet.LegacyControl.currentAnimationData.ticksPlayed = state.moveSet.animator.currentAnimationData.ticksPlayed;
                        controlsScript.MoveSet.LegacyControl.currentAnimationData.framesPlayed = state.moveSet.animator.currentAnimationData.framesPlayed;
                        controlsScript.MoveSet.LegacyControl.currentAnimationData.realFramesPlayed = state.moveSet.animator.currentAnimationData.realFramesPlayed;
                        controlsScript.MoveSet.LegacyControl.currentAnimationData.timesPlayed = state.moveSet.animator.currentAnimationData.timesPlayed;
                        controlsScript.MoveSet.LegacyControl.currentAnimationData.speed = state.moveSet.animator.currentAnimationData.speed;
                    }
                    controlsScript.MoveSet.LegacyControl.animator.Sample();
                }


                // HitBoxes & MoveSet - Update Animation Map
                //controlsScript.HitBoxes.UpdateMap(controlsScript.MoveSet.GetCurrentClipFrame(controlsScript.HitBoxes.bakeSpeed));
            }
            else {
                Debug.LogWarning("We don't have the player state for this frame.");
            }
        }
    }

    protected static FluxStates.CharacterState SaveCharacterState(ControlsScript controlsScript) {
        FluxStates.CharacterState state = new FluxStates.CharacterState();

        state.controlsScript = controlsScript != null;
        if (state.controlsScript) {
            // Character Shell FP Transform
            state.shellTransform.fpPosition = controlsScript.worldTransform.position;
            state.shellTransform.fpRotation = controlsScript.worldTransform.rotation;
            state.characterTransform.fpPosition = controlsScript.localTransform.position;
            state.characterTransform.fpRotation = controlsScript.localTransform.rotation;

            // Character Shell Transform
            state.shellTransform.position = controlsScript.transform.position;
            state.shellTransform.rotation = controlsScript.transform.rotation;

            // Character Transform
            state.characterTransform.localPosition = controlsScript.character.transform.localPosition;
            state.characterTransform.rotation = controlsScript.character.transform.rotation;
            state.characterTransform.localScale = controlsScript.character.transform.localScale;


            // Meters
            state.life = controlsScript.currentLifePoints;
            state.gauges = new Fix64[controlsScript.currentGaugesPoints.Length];
            for (int i = 0; i < state.gauges.Length; ++i) {
                state.gauges[i] = controlsScript.currentGaugesPoints[i];
            }


            // Control
            state.active = controlsScript.GetActive();
            state.playerNum = controlsScript.playerNum;
            state.afkTimer = controlsScript.afkTimer;
            state.airJuggleHits = controlsScript.airJuggleHits;
            state.airRecoveryType = controlsScript.airRecoveryType;
            state.applyRootMotion = controlsScript.applyRootMotion;
            state.blockStunned = controlsScript.blockStunned;
            state.comboDamage = controlsScript.comboDamage;
            state.comboHitDamage = controlsScript.comboHitDamage;
            state.comboHits = controlsScript.comboHits;
            state.consecutiveCrumple = controlsScript.consecutiveCrumple;
            state.currentBasicMove = controlsScript.currentBasicMoveReference;
            state.currentDrained = controlsScript.currentDrained;
            state.currentHit = controlsScript.currentHit;
            state.currentHitAnimation = controlsScript.currentHitAnimation;
            state.currentState = controlsScript.currentState;
            state.currentSubState = controlsScript.currentSubState;
            state.DCStance = controlsScript.DCStance;
            state.firstHit = controlsScript.firstHit;
            state.gaugeDPS = controlsScript.gaugeDPS;
            state.gaugeDrainId = controlsScript.gaugeDrainId;
            state.hitDetected = controlsScript.hitDetected;
            state.hitAnimationSpeed = controlsScript.hitAnimationSpeed;
            state.inhibitGainWhileDraining = controlsScript.inhibitGainWhileDraining;
            state.isAirRecovering = controlsScript.isAirRecovering;
            state.isBlocking = controlsScript.isBlocking;
            state.isCrouching = controlsScript.isCrouching;
            state.isDead = controlsScript.isDead;
            state.ignoreCollisionMass = controlsScript.ignoreCollisionMass;
            state.lit = controlsScript.lit;
            state.lockOnTarget = controlsScript.target;
            state.lockXMotion = controlsScript.lockXMotion;
            state.lockYMotion = controlsScript.lockYMotion;
            state.lockZMotion = controlsScript.lockZMotion;
            state.mirror = controlsScript.mirror;
            state.normalizedDistance = controlsScript.normalizedDistance;
            state.normalizedJumpArc = controlsScript.normalizedJumpArc;
            state.outroPlayed = controlsScript.outroPlayed;
            state.potentialBlock = controlsScript.potentialBlock;
            state.potentialParry = controlsScript.potentialParry;
            state.roundMsgCasted = controlsScript.roundMsgCasted;
            state.roundsWon = controlsScript.roundsWon;
            state.shakeCamera = controlsScript.shakeCamera;
            state.shakeCharacter = controlsScript.shakeCharacter;
            state.shakeDensity = controlsScript.shakeDensity;
            state.shakeCameraDensity = controlsScript.shakeCameraDensity;
            state.standUpOverride = controlsScript.standUpOverride;
            state.standardYRotation = controlsScript.standardYRotation;
            state.storedMoveTime = controlsScript.storedMoveTime;
            state.stunTime = controlsScript.stunTime;
            state.totalDrain = controlsScript.totalDrain;


            // Sprite Renderer (If Any)
            if (controlsScript.mySpriteRenderer != null)
                state.spriteRendererFlipX = controlsScript.mySpriteRenderer.flipX;


            // Active PullIn
            state.activePullIn.pullIn = controlsScript.activePullIn;
            if (controlsScript.activePullIn != null)
                state.activePullIn.position = controlsScript.activePullIn.position;


            // Moves
            state.currentMove = CopyMove(state.currentMove, controlsScript.currentMove);
            state.enterMove = CopyMove(state.enterMove, controlsScript.enterMove);
            state.exitMove = CopyMove(state.exitMove, controlsScript.exitMove);
            state.DCMove = CopyMove(state.DCMove, controlsScript.DCMove);
            state.storedMove = CopyMove(state.storedMove, controlsScript.storedMove);


            // Physics
            state.physics.activeForces = controlsScript.Physics.activeForces;
            state.physics.airTime = controlsScript.Physics.airTime;
            state.physics.angularDirection = controlsScript.Physics.angularDirection;
            state.physics.appliedGravity = controlsScript.Physics.appliedGravity;
            state.physics.currentAirJumps = controlsScript.Physics.currentAirJumps;
            state.physics.freeze = controlsScript.Physics.freeze;
            state.physics.groundBounceTimes = controlsScript.Physics.groundBounceTimes;
            state.physics.horizontalJumpForce = controlsScript.Physics.horizontalJumpForce;
            state.physics.isGroundBouncing = controlsScript.Physics.isGroundBouncing;
            state.physics.isLanding = controlsScript.Physics.isLanding;
            state.physics.isTakingOff = controlsScript.Physics.isTakingOff;
            state.physics.isWallBouncing = controlsScript.Physics.isWallBouncing;
            state.physics.moveDirection = controlsScript.Physics.moveDirection;
            state.physics.overrideAirAnimation = controlsScript.Physics.overrideAirAnimation;
            state.physics.overrideStunAnimation = controlsScript.Physics.overrideStunAnimation;
            state.physics.verticalTotalForce = controlsScript.Physics.verticalTotalForce;
            state.physics.wallBounceTimes = controlsScript.Physics.wallBounceTimes;


            // Move Set
            state.moveSet.combatStance = controlsScript.MoveSet.currentCombatStance;
            state.moveSet.totalAirMoves = controlsScript.MoveSet.totalAirMoves;
            state.moveSet.animationPaused = controlsScript.MoveSet.animationPaused;
            state.moveSet.overrideNextBlendingValue = controlsScript.MoveSet.overrideNextBlendingValue;
            state.moveSet.lastTimePress = controlsScript.MoveSet.lastTimePress;


            // Inputs being held down (charges)
            state.inputHeldDown = controlsScript.inputHeldDown.ToDictionary(entry => entry.Key, entry => entry.Value);


            // Projectiles
            state.projectiles = new List<ProjectileMoveScript>();
            foreach (ProjectileMoveScript projectile in controlsScript.projectiles) {
                state.projectiles.Add(projectile);
            }


            // Buttons Pressed
            state.moveSet.lastButtonPresses = new List<FluxStates.CharacterState.ButtonSequenceState>();
            foreach (ButtonSequenceRecord btnRecord in controlsScript.MoveSet.lastButtonPresses) {
                FluxStates.CharacterState.ButtonSequenceState buttonSeqState = new FluxStates.CharacterState.ButtonSequenceState();
                buttonSeqState.buttonPresses = btnRecord.buttonPresses.ToArray();
                buttonSeqState.chargeTime = btnRecord.chargeTime;
                state.moveSet.lastButtonPresses.Add(buttonSeqState);
            }


            // Cooldown Timers
            state.moveSet.lastMovesPlayed = controlsScript.MoveSet.lastMovesPlayed.ToDictionary(entry => entry.Key, entry => entry.Value);


            // Hit Boxes State
            state.hitBoxes.isHit = controlsScript.HitBoxes.isHit;
            state.hitBoxes.hitConfirmType = controlsScript.HitBoxes.hitConfirmType;
            state.hitBoxes.collisionBoxSize = controlsScript.HitBoxes.collisionBoxSize;
            state.hitBoxes.inverted = controlsScript.HitBoxes.inverted;


            // Hit Boxes State - Custom Hit Boxes
            if (controlsScript.HitBoxes.customHitBoxes != null) {
                state.hitBoxes.customHitBoxes = new FluxStates.CharacterState.CustomHitBoxesState();
                state.hitBoxes.customHitBoxes.clip = controlsScript.HitBoxes.customHitBoxes.clip;
                state.hitBoxes.customHitBoxes.speed = controlsScript.HitBoxes.customHitBoxes.speed;
                state.hitBoxes.customHitBoxes.totalFrames = controlsScript.HitBoxes.customHitBoxes.totalFrames;
                state.hitBoxes.customHitBoxes.customHitBoxes = controlsScript.HitBoxes.customHitBoxes.customHitBoxes;
            }


            // Hit Boxes State - Hit Boxes
            state.hitBoxes.hitBoxes = new FluxStates.CharacterState.HitBoxState[controlsScript.HitBoxes.hitBoxes.Length];
            for (int i = 0; i < controlsScript.HitBoxes.hitBoxes.Length; ++i) {
                state.hitBoxes.hitBoxes[i].bodyPart = controlsScript.HitBoxes.hitBoxes[i].bodyPart;
                state.hitBoxes.hitBoxes[i].state = controlsScript.HitBoxes.hitBoxes[i].hitState;
                state.hitBoxes.hitBoxes[i].hide = controlsScript.HitBoxes.hitBoxes[i].hide;
                state.hitBoxes.hitBoxes[i].visibility = controlsScript.HitBoxes.hitBoxes[i].defaultVisibility;
                state.hitBoxes.hitBoxes[i].collisionType = controlsScript.HitBoxes.hitBoxes[i].collisionType;
                state.hitBoxes.hitBoxes[i].type = controlsScript.HitBoxes.hitBoxes[i].type;
                state.hitBoxes.hitBoxes[i].shape = controlsScript.HitBoxes.hitBoxes[i].shape;
                state.hitBoxes.hitBoxes[i].mappedPosition = controlsScript.HitBoxes.hitBoxes[i].mappedPosition;
                state.hitBoxes.hitBoxes[i].localPosition = controlsScript.HitBoxes.hitBoxes[i].localPosition;
                state.hitBoxes.hitBoxes[i].radius = controlsScript.HitBoxes.hitBoxes[i]._radius;
                state.hitBoxes.hitBoxes[i].rect = controlsScript.HitBoxes.hitBoxes[i]._rect;
                state.hitBoxes.hitBoxes[i].offSet = controlsScript.HitBoxes.hitBoxes[i]._offSet;
            }


            // Hit Boxes State - Hurt Boxes
            if (controlsScript.HitBoxes.activeHurtBoxes != null) {
                state.hitBoxes.activeHurtBoxes = new FluxStates.CharacterState.HurtBoxState[controlsScript.HitBoxes.activeHurtBoxes.Length];
                for (int i = 0; i < controlsScript.HitBoxes.activeHurtBoxes.Length; ++i) {
                    state.hitBoxes.activeHurtBoxes[i].bodyPart = controlsScript.HitBoxes.activeHurtBoxes[i].bodyPart;
                    state.hitBoxes.activeHurtBoxes[i].followXBounds = controlsScript.HitBoxes.activeHurtBoxes[i].followXBounds;
                    state.hitBoxes.activeHurtBoxes[i].followYBounds = controlsScript.HitBoxes.activeHurtBoxes[i].followYBounds;
                    state.hitBoxes.activeHurtBoxes[i].hitBoxDefinitionIndex = controlsScript.HitBoxes.activeHurtBoxes[i].hitBoxDefinitionIndex;
                    state.hitBoxes.activeHurtBoxes[i].isBlock = controlsScript.HitBoxes.activeHurtBoxes[i].isBlock;
                    state.hitBoxes.activeHurtBoxes[i].offSet = controlsScript.HitBoxes.activeHurtBoxes[i]._offSet;
                    state.hitBoxes.activeHurtBoxes[i].position = controlsScript.HitBoxes.activeHurtBoxes[i].position;
                    state.hitBoxes.activeHurtBoxes[i].radius = controlsScript.HitBoxes.activeHurtBoxes[i]._radius;
                    state.hitBoxes.activeHurtBoxes[i].rect = controlsScript.HitBoxes.activeHurtBoxes[i]._rect;
                    state.hitBoxes.activeHurtBoxes[i].rendererBounds = controlsScript.HitBoxes.activeHurtBoxes[i].rendererBounds;
                    state.hitBoxes.activeHurtBoxes[i].shape = controlsScript.HitBoxes.activeHurtBoxes[i].shape;
                    state.hitBoxes.activeHurtBoxes[i].type = controlsScript.HitBoxes.activeHurtBoxes[i].type;
                }
            } else {
                state.hitBoxes.activeHurtBoxes = null;
            }


            // Hit Boxes State - Block Area
            state.hitBoxes.blockableArea.blockArea = controlsScript.HitBoxes.blockableArea;
            if (controlsScript.HitBoxes.blockableArea != null)
                state.hitBoxes.blockableArea.position = controlsScript.HitBoxes.blockableArea.position;


            // Hit Boxes State - Animation Maps
            int animMapLengh = controlsScript.HitBoxes.animationMaps != null ? controlsScript.HitBoxes.animationMaps.Length : 0;
            state.hitBoxes.animationMaps = new AnimationMap[animMapLengh];
            state.hitBoxes.bakeSpeed = controlsScript.HitBoxes.bakeSpeed;
            state.hitBoxes.deltaPosition = controlsScript.HitBoxes.deltaPosition;
            for (int i = 0; i < animMapLengh; ++i) {
                state.hitBoxes.animationMaps[i] = controlsScript.HitBoxes.animationMaps[i];
            }


            // Animator
            if (controlsScript.myInfo.animationType == AnimationType.Mecanim3D || controlsScript.myInfo.animationType == AnimationType.Mecanim2D) {
                state.moveSet.animator.currentMirror = controlsScript.MoveSet.MecanimControl.currentMirror;

                state.moveSet.animator.currentAnimationData.mecanimAnimationIndex = controlsScript.MoveSet.MecanimControl.GetCurrentAnimationIndex();
                if (controlsScript.MoveSet.MecanimControl.currentAnimationData != null)
                {
                    state.moveSet.animator.currentAnimationData.normalizedSpeed = controlsScript.MoveSet.MecanimControl.currentAnimationData.normalizedSpeed;
                    state.moveSet.animator.currentAnimationData.normalizedTime = controlsScript.MoveSet.MecanimControl.currentAnimationData.normalizedTime;
                    state.moveSet.animator.currentAnimationData.secondsPlayed = controlsScript.MoveSet.MecanimControl.currentAnimationData.secondsPlayed;
                    state.moveSet.animator.currentAnimationData.ticksPlayed = controlsScript.MoveSet.MecanimControl.currentAnimationData.ticksPlayed;
                    state.moveSet.animator.currentAnimationData.framesPlayed = controlsScript.MoveSet.MecanimControl.currentAnimationData.framesPlayed;
                    state.moveSet.animator.currentAnimationData.realFramesPlayed = controlsScript.MoveSet.MecanimControl.currentAnimationData.realFramesPlayed;
                    state.moveSet.animator.currentAnimationData.timesPlayed = controlsScript.MoveSet.MecanimControl.currentAnimationData.timesPlayed;
                    state.moveSet.animator.currentAnimationData.speed = controlsScript.MoveSet.MecanimControl.currentAnimationData.speed;
                }

                // Mecanim Control 3.0
                /*
	            state.moveSet.animator.currentInput = controlsScript.MoveSet.MecanimControl.mc3Animator.currentInput;
	            state.moveSet.animator.transitionDuration = controlsScript.MoveSet.MecanimControl.mc3Animator.transitionDuration;
	            state.moveSet.animator.transitionTime = controlsScript.MoveSet.MecanimControl.mc3Animator.transitionTime;

	            state.moveSet.animator.weightList = controlsScript.MoveSet.MecanimControl.mc3Animator.GetWeights();
                state.moveSet.animator.speedList = (float[]) controlsScript.MoveSet.MecanimControl.mc3Animator.speedArray.Clone();
                state.moveSet.animator.timeList = (float[])controlsScript.MoveSet.MecanimControl.mc3Animator.timeArray.Clone();
                */

                // Mecanim Control 1.0
                state.moveSet.animator.currentState = controlsScript.MoveSet.MecanimControl.currentState;
                state.moveSet.animator.currentSpeed = controlsScript.MoveSet.MecanimControl.currentSpeed;
                state.moveSet.animator.overrideController = controlsScript.MoveSet.MecanimControl.animator.runtimeAnimatorController;

            } else {
                state.moveSet.animator.currentMirror = controlsScript.MoveSet.LegacyControl.currentMirror;
                state.moveSet.animator.globalSpeed = controlsScript.MoveSet.LegacyControl.globalSpeed;
                state.moveSet.animator.lastPosition = controlsScript.MoveSet.LegacyControl.lastPosition;

                state.moveSet.animator.currentAnimationData.legacyAnimationData = controlsScript.MoveSet.LegacyControl.currentAnimationData;
                if (controlsScript.MoveSet.LegacyControl.currentAnimationData != null)
                {
                    state.moveSet.animator.currentAnimationData.normalizedSpeed = controlsScript.MoveSet.LegacyControl.currentAnimationData.normalizedSpeed;
                    state.moveSet.animator.currentAnimationData.normalizedTime = controlsScript.MoveSet.LegacyControl.currentAnimationData.normalizedTime;
                    state.moveSet.animator.currentAnimationData.secondsPlayed = controlsScript.MoveSet.LegacyControl.currentAnimationData.secondsPlayed;
                    state.moveSet.animator.currentAnimationData.ticksPlayed = controlsScript.MoveSet.LegacyControl.currentAnimationData.ticksPlayed;
                    state.moveSet.animator.currentAnimationData.framesPlayed = controlsScript.MoveSet.LegacyControl.currentAnimationData.framesPlayed;
                    state.moveSet.animator.currentAnimationData.realFramesPlayed = controlsScript.MoveSet.LegacyControl.currentAnimationData.realFramesPlayed;
                    state.moveSet.animator.currentAnimationData.timesPlayed = controlsScript.MoveSet.LegacyControl.currentAnimationData.timesPlayed;
                    state.moveSet.animator.currentAnimationData.speed = controlsScript.MoveSet.LegacyControl.currentAnimationData.speed;
                }
            }
        }

        return state;
    }
}
