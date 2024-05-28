using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLibrary;
using UFE3D;

public class MoveSetScript : MonoBehaviour
{
    public List<MoveSetData> loadedMoveSets;
    public BasicMoves basicMoves;
    public MoveInfo[] attackMoves;
    public MoveInfo[] moves;

    #region trackable definitions
    public MecanimControl MecanimControl { get { return this.mecanimControl; } set { mecanimControl = value; } }
    public LegacyControl LegacyControl { get { return this.legacyControl; } set { legacyControl = value; } }
    public SpriteRenderer SpriteRenderer { get { return this.spriteRenderer; } set { spriteRenderer = value; } }
    public int totalAirMoves;
    public bool animationPaused;
    public Fix64 overrideNextBlendingValue = -1;
    public Fix64 lastTimePress;
    public CombatStances currentCombatStance;
    public List<ButtonSequenceRecord> lastButtonPresses = new List<ButtonSequenceRecord>();
    public Dictionary<string, long> lastMovesPlayed = new Dictionary<string, long>();
    #endregion


    public ControlsScript controlsScript;
    public HitBoxesScript hitBoxesScript;
    private MecanimControl mecanimControl;
    private LegacyControl legacyControl;
    private SpriteRenderer spriteRenderer;

    public Dictionary<string, AnimationMap[]> dicAnimationMap = new Dictionary<string, AnimationMap[]>();

    void Awake()
    {
        controlsScript = transform.parent.gameObject.GetComponent<ControlsScript>();
        hitBoxesScript = GetComponent<HitBoxesScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Force UFE Animation Engine For Online Games
        if ((UFE.IsConnected || UFE.config.debugOptions.emulateNetwork)
            && UFE.config.networkOptions.forceAnimationControl)
        {
            controlsScript.myInfo.animationFlow = AnimationFlow.UFEEngine;
        }

        // Assign Animation Components
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            if (gameObject.GetComponent<Animation>() == null)
                gameObject.AddComponent<Animation>();

            legacyControl = gameObject.AddComponent<LegacyControl>();
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) legacyControl.overrideAnimatorUpdate = true;

        }
        else
        {
            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
                animator = gameObject.AddComponent<Animator>();
            animator.avatar = controlsScript.myInfo.avatar;

            mecanimControl = gameObject.AddComponent<MecanimControl>();
            mecanimControl.ApplyBuiltinRootMotion();
            mecanimControl.defaultTransitionDuration = controlsScript.myInfo._blendingTime;
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) mecanimControl.overrideAnimatorUpdate = true;
            mecanimControl.normalizeFrames = controlsScript.myInfo.normalizeAnimationFrames;
        }

        // Load All Animations And Move Sets Into Memory
        loadedMoveSets = new List<MoveSetData>();
        foreach (MoveSetData moveSetData in controlsScript.myInfo.moves)
        {
            loadedMoveSets.Add(moveSetData);
        }
        foreach (string path in controlsScript.myInfo.stanceResourcePath)
        {
            loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
        }

        foreach (MoveSetData moveSetData in loadedMoveSets)
        {
            if (moveSetData.combatStance == CombatStances.Stance1) basicMoves = moveSetData.basicMoves;
            FillMoves(moveSetData);

            if (controlsScript.myInfo.useAnimationMaps)
            {
                foreach (AnimationMaps animMaps in moveSetData.AM_File.animationMaps)
                {
                    if (dicAnimationMap.ContainsKey(animMaps.id)) continue;
                    dicAnimationMap.Add(animMaps.id, animMaps.animationMaps);
                }
            }
        }

        // Set Initial Stance
        ChangeMoveStances(CombatStances.Stance1, true);
    }

    public void ChangeMoveStances(CombatStances newStance, bool forceChange = false)
    {
        if (!forceChange && currentCombatStance == newStance) return;

        foreach (MoveSetData moveSetData in loadedMoveSets)
        {
            if (moveSetData.combatStance == newStance)
            {
                basicMoves = moveSetData.basicMoves;
                attackMoves = moveSetData.attackMoves;
                moves = attackMoves;

                currentCombatStance = newStance;

                return;
            }
        }
    }

    private void FillMoves(MoveSetData moveSetData)
    {
        // Feed dictionary if data comes from previous versions
        moveSetData.basicMoves.UpdateDictionary();

        foreach(BasicMoveInfo basicMoveInfo in moveSetData.basicMoves.basicMoveDictionary.Keys)
        {
            SetBasicMoveAnimation(basicMoveInfo);
        }

        foreach (MoveInfo move in moveSetData.attackMoves)
        {
            if (move == null)
            {
                Debug.LogError("You have empty entries in your move list (" + moveSetData.combatStance + "). Check your special moves under Character Editor.");
                continue;
            }

            AttachAnimation(move.animData.clip, move.id, move._animationSpeed, move.wrapMode, move.animData.length);
        }

        foreach (MoveInfo move1 in moveSetData.attackMoves)
        {
            if (move1.defaultInputs.chargeMove && move1.defaultInputs._chargeTiming <= controlsScript.myInfo._executionTiming)
            {
                Debug.LogWarning("Warning: " + move1.name + " (" + move1.moveName + ") charge timing must be higher then the character's execution timing.");
            }

            foreach (MoveInfo move2 in moveSetData.attackMoves)
            {
                if (move2 == null) Debug.LogError("Error: You have an empty move field under " + controlsScript.myInfo.characterName + "'s move set");
                if (move1.name != move2.name && move1.moveName == move2.moveName)
                {
                    Debug.LogWarning("Warning: " + move1.name + " (" + move1.moveName + ") has the same name as " + move2.name + " (" + move2.moveName + ")");
                }
            }
        }

        System.Array.Sort(moveSetData.attackMoves, delegate (MoveInfo move1, MoveInfo move2)
        {
            return move1.defaultInputs.buttonExecution.Length.CompareTo(move2.defaultInputs.buttonExecution.Length);
        });

        System.Array.Sort(moveSetData.attackMoves, delegate (MoveInfo move1, MoveInfo move2)
        {
            if (move1.defaultInputs.buttonExecution.Length > 1 && move1.defaultInputs.buttonExecution.Contains(ButtonPress.Back)) return 0;
            if (move1.defaultInputs.buttonExecution.Length > 1 && move1.defaultInputs.buttonExecution.Contains(ButtonPress.Forward)) return 0;
            if (move1.defaultInputs.buttonExecution.Length > 1) return 1;
            return 0;
        });

        System.Array.Sort(moveSetData.attackMoves, delegate (MoveInfo move1, MoveInfo move2)
        {
            return move1.selfConditions.basicMoveLimitation.Length.CompareTo(move2.selfConditions.basicMoveLimitation.Length);
        });

        System.Array.Sort(moveSetData.attackMoves, delegate (MoveInfo move1, MoveInfo move2)
        {
            return move1.opponentConditions.basicMoveLimitation.Length.CompareTo(move2.opponentConditions.basicMoveLimitation.Length);
        });

        System.Array.Sort(moveSetData.attackMoves, delegate (MoveInfo move1, MoveInfo move2)
        {
            return move1.opponentConditions.possibleMoveStates.Length.CompareTo(move2.opponentConditions.possibleMoveStates.Length);
        });

        System.Array.Sort(moveSetData.attackMoves, delegate (MoveInfo move1, MoveInfo move2)
        {
            return move1.previousMoves.Length.CompareTo(move2.previousMoves.Length);
        });

        System.Array.Sort(moveSetData.attackMoves, delegate (MoveInfo move1, MoveInfo move2)
        {
            return move1.defaultInputs.buttonSequence.Length.CompareTo(move2.defaultInputs.buttonSequence.Length);
        });

        System.Array.Reverse(moveSetData.attackMoves);
    }

    private void SetBasicMoveAnimation(BasicMoveInfo basicMove)
    {
        if (basicMove.useMoveFile && basicMove.moveInfo != null)
        {
            AttachAnimation(basicMove.moveInfo.animData.clip, basicMove.moveInfo.id, basicMove.moveInfo._animationSpeed, basicMove.moveInfo.wrapMode, basicMove.moveInfo.animData.length);
            return;
        }

        int currentClip = 0;
        foreach (SerializedAnimationData animData in basicMove.animData)
        {
            if (animData.clip != null)
            {
                WrapMode newWrapMode = basicMove.wrapMode;
                if (currentClip > 0)
                {
                    if (basicMove.reference == BasicMoveReference.Idle)
                    {
                        newWrapMode = WrapMode.Once;
                    }
                    else if ((basicMove.reference == BasicMoveReference.FallDownDefault ||
                          basicMove.reference == BasicMoveReference.FallDownFromAirJuggle ||
                          basicMove.reference == BasicMoveReference.FallDownFromGroundBounce) && basicMove.loopDownClip)
                    {
                        newWrapMode = WrapMode.Loop;
                    }
                }

                AttachAnimation(animData.clip, basicMove.id + "_" + currentClip, basicMove._animationSpeed, newWrapMode, animData.length);
            }
            currentClip++;
        }
    }

    private void AttachAnimation(AnimationClip clip, string animName, Fix64 speed, WrapMode wrapMode, Fix64 length)
    {
        if (clip == null) return;
        if (AnimationExists(animName)) return;

        if (!controlsScript.myInfo.useAnimationMaps) length = clip.length;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.AddClip(clip, animName, speed, wrapMode, length);
        }
        else
        {
            mecanimControl.AddClip(clip, animName, speed, wrapMode, length);
        }
    }

    public BasicMoveInfo GetBasicAnimationInfo(BasicMoveReference reference)
    {
        foreach (var basicMove in basicMoves.basicMoveDictionary)
        {
            if (basicMove.Value == reference) return basicMove.Key;
        }
        return null;
    }

    public string GetAnimationString(BasicMoveReference basicMoveRef, int clipNum = 0)
    {
        return GetAnimationString(basicMoves.GetBasicMoveInfo(basicMoveRef), clipNum);
    }

    public string GetAnimationString(BasicMoveInfo basicMove, int clipNum = 0)
    {
        if (basicMove.useMoveFile && basicMove.moveInfo != null) return basicMove.moveInfo.id;

        if (clipNum > 0 && !AnimationExists(basicMove, clipNum))
            clipNum = 0;

        return basicMove.id + "_" + clipNum;
    }

    public bool IsBasicMovePlaying(BasicMoveReference basicMoveRef, int clipNum = -1)
    {
        return IsBasicMovePlaying(basicMoves.GetBasicMoveInfo(basicMoveRef), clipNum);
    }

    public bool IsBasicMovePlaying(BasicMoveInfo basicMove, int clipNum = -1)
    {
        if (basicMove == null) return false;

        if (basicMove.useMoveFile && basicMove.moveInfo != null && IsAnimationPlaying(basicMove.moveInfo.id)) return true;

        if (clipNum != -1)
        {
            if (basicMove.animData[clipNum].clip != null && IsAnimationPlaying(basicMove.id + "_" + clipNum)) return true;
        }
        else
        {
            int currentClip = 0;
            foreach(SerializedAnimationData animData in basicMove.animData)
            {
                if (animData.clip != null && IsAnimationPlaying(basicMove.id + "_" + currentClip)) return true;
                currentClip++;
            }
        }

        return false;
    }

    public bool IsAnimationPlaying(string id)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.IsPlaying(id);
        }
        else
        {
            return mecanimControl.IsPlaying(id);
        }
    }

    public int AnimationTimesPlayed(BasicMoveReference basicMoveRef, int clipNum)
    {
        return AnimationTimesPlayed(GetAnimationString(basicMoveRef, clipNum));
    }

    public int AnimationTimesPlayed(string id)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetTimesPlayed(id);
        }
        else
        {
            return mecanimControl.GetTimesPlayed(id);
        }
    }

    public void OverrideWrapMode(WrapMode wrap)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.OverrideCurrentWrapMode(wrap);
        }
        else
        {
            mecanimControl.OverrideCurrentWrapMode(wrap);
        }
    }

    public Fix64 GetAnimationLength(BasicMoveReference basicMoveRef, int clipNum)
    {
        return GetAnimationLength(GetAnimationString(basicMoveRef, clipNum));
    }

    public Fix64 GetAnimationLength(string id)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetAnimationData(id).length;
        }
        else
        {
            return mecanimControl.GetAnimationData(id).length;
        }
    }

    public bool AnimationExists(BasicMoveReference basicMoveRef, int clipNum = 0)
    {
        return AnimationExists(basicMoves.GetBasicMoveInfo(basicMoveRef), clipNum);
    }

    public bool AnimationExists(BasicMoveInfo basicMove, int clipNum)
    {
        return AnimationExists(basicMove.id + "_" + clipNum);
    }

    public bool AnimationExists(string id)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetAnimationData(id) != null;
        }
        else
        {
            return mecanimControl.GetAnimationData(id) != null;
        }
    }

    public void PlayAnimation(string id, Fix64 blendingTime)
    {
        PlayAnimation(id, blendingTime, 0);
    }


    public void PlayAnimation(string id, Fix64 blendingTime, Fix64 normalizedTime)
    {
        if ((UFE.IsConnected || UFE.config.debugOptions.emulateNetwork) &&
            UFE.config.networkOptions.disableBlending) blendingTime = 0;

        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.Play(id, blendingTime, normalizedTime);
        }
        else if (controlsScript.myInfo.animationType == AnimationType.Mecanim2D)
        {
            mecanimControl.Play(id, 0, normalizedTime, false);
        }
        else
        {
            mecanimControl.Play(id, blendingTime, normalizedTime, controlsScript.mirror > 0 && UFE.config.characterRotationOptions.autoMirror);
        }
    }

    public void StopAnimation(string id)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.Stop(id);
        }
        else
        {
            mecanimControl.Stop();
        }
    }

    public void SetAnimationSpeed(Fix64 speed)
    {
        if (speed < 1) animationPaused = true;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetSpeed(speed);
        }
        else
        {
            mecanimControl.SetSpeed(speed);
        }
    }

    public void SetAnimationSpeed(string id, Fix64 speed)
    {
        string animName = id;

        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetSpeed(animName, speed);
        }
        else
        {
            mecanimControl.SetSpeed(animName, speed);
        }
    }

    public void SetAnimationNormalizedSpeed(string id, Fix64 normalizedSpeed)
    {
        string animName = id;

        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetNormalizedSpeed(animName, normalizedSpeed);
        }
        else
        {
            mecanimControl.SetNormalizedSpeed(animName, normalizedSpeed);
        }
    }

    public Fix64 GetAnimationSpeed()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetSpeed();
        }
        else
        {
            return mecanimControl.GetSpeed();
        }
    }
    public Fix64 GetNormalizedSpeed()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetNormalizedSpeed();
        }
        else
        {
            return mecanimControl.GetNormalizedSpeed();
        }
    }

    public Fix64 GetAnimationSpeed(string id)
    {
        string animName = id;

        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetSpeed(animName);
        }
        else
        {
            return mecanimControl.GetSpeed(animName);
        }
    }

    public Fix64 GetOriginalAnimationSpeed(string id)
    {
        return mecanimControl.GetOriginalSpeed(id);
    }

    public void RestoreAnimationSpeed()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.RestoreSpeed();
        }
        else
        {
            mecanimControl.RestoreSpeed();
        }
        animationPaused = false;
    }

    public void PlayBasicMove(BasicMoveReference basicMoveReference, bool replay)
    {
        PlayBasicMove(basicMoves.GetBasicMoveInfo(basicMoveReference), 0, replay);
    }

    public void PlayBasicMove(BasicMoveReference basicMoveReference, int clipNum = 0, bool replay = true)
    {
        PlayBasicMove(basicMoves.GetBasicMoveInfo(basicMoveReference), clipNum, replay);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, bool replay)
    {
        PlayBasicMove(basicMove, 0, replay);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, int clipNum = 0, bool replay = true)
    {
        if (overrideNextBlendingValue > -1)
        {
            PlayBasicMove(basicMove, clipNum, overrideNextBlendingValue);
            overrideNextBlendingValue = -1;
        }
        else if (basicMove.overrideBlendingIn)
        {
            PlayBasicMove(basicMove, clipNum, basicMove._blendingIn, replay, basicMove.invincible);
        }
        else
        {
            PlayBasicMove(basicMove, clipNum, controlsScript.myInfo._blendingTime, replay, basicMove.invincible);
        }

        if (basicMove.overrideBlendingOut) overrideNextBlendingValue = basicMove._blendingOut;
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, int clipNum, Fix64 blendingTime)
    {
        PlayBasicMove(basicMove, clipNum, blendingTime, true, basicMove.invincible);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, int clipNum, Fix64 blendingTime, bool replay)
    {
        PlayBasicMove(basicMove, clipNum, blendingTime, replay, basicMove.invincible);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, int clipNum, Fix64 blendingTime, bool replay, bool hideHitBoxes)
    {
        // Set basic move reference
        controlsScript.currentBasicMoveReference = basicMove.reference;

        // Play move instead if using move file
        if (basicMove.useMoveFile)
        {
            controlsScript.CastMove(basicMove.moveInfo, true);
            return;
        }

        if (clipNum > 0 && !AnimationExists(basicMove, clipNum))
            clipNum = 0;

        if (!AnimationExists(basicMove, clipNum)) return;

        // Set clip id
        string animName = basicMove.id + "_" + clipNum;

        // Make sure its not playing already
        if (IsAnimationPlaying(animName) && !replay) return;

        // Play animation
        PlayAnimation(animName, blendingTime);

        // Set root motion options
        controlsScript.applyRootMotion = basicMove.applyRootMotion;
        controlsScript.lockXMotion = basicMove.lockXMotion;
        controlsScript.lockYMotion = basicMove.lockYMotion;
        controlsScript.lockZMotion = basicMove.lockZMotion;

        // Toggle head look
        controlsScript.ToggleHeadLook(!basicMove.disableHeadLook);

        // Play sound effects
        UFE.PlaySound(basicMove.soundEffects);

        // Set hit boxes visibility
        hitBoxesScript.HideHitBoxes(hideHitBoxes);

        // Set visibility to nested game objects
        HitBoxesScript hitBoxes = controlsScript.character.GetComponent<HitBoxesScript>();
        if (hitBoxes != null)
        {
            foreach (HitBox hitBox in hitBoxes.hitBoxes)
            {
                if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null)
                {
                    hitBox.position.gameObject.SetActive(hitBox.defaultVisibility);
                }
            }
        }

        // Play particle effects
        if (basicMove.particleEffect.prefab != null)
        {
            Vector3 newPosition = hitBoxesScript.GetPosition(basicMove.particleEffect.bodyPart).ToVector();
            newPosition.x += basicMove.particleEffect.positionOffSet.x * -controlsScript.mirror;
            newPosition.y += basicMove.particleEffect.positionOffSet.y;
            newPosition.z += basicMove.particleEffect.positionOffSet.z;

            string uniqueId = basicMove.particleEffect.prefab.name + controlsScript.playerNum.ToString() + UFE.currentFrame;
            GameObject pTemp = UFE.SpawnGameObject(basicMove.particleEffect.prefab, newPosition, Quaternion.identity, Mathf.RoundToInt(basicMove.particleEffect.duration * UFE.config.fps), false, uniqueId);

            if (basicMove.particleEffect.mirrorOn2PSide && controlsScript.mirror > 0)
            {
                pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
            }
            if (basicMove.particleEffect.stick) pTemp.transform.parent = transform;
        }

        // Set animation maps
        if (basicMove.animData[clipNum].hitBoxDefinitionType == HitBoxDefinitionType.AutoMap)
        {
            hitBoxesScript.customHitBoxes = null;
            hitBoxesScript.animationMaps = null;
            hitBoxesScript.bakeSpeed = !basicMove.autoSpeed && basicMove.animData[clipNum].bakeSpeed;

            if (controlsScript.myInfo.useAnimationMaps)
            {
                if (!dicAnimationMap.ContainsKey(animName))
                {
                    Debug.LogError("Animation Maps for " + basicMove.reference.ToString() + " (" + currentCombatStance + ") not found!");
                }
                else
                {
                    hitBoxesScript.animationMaps = dicAnimationMap[animName];
                }
            }
        }
        else
        {
            hitBoxesScript.customHitBoxes = basicMove.animData[clipNum].customHitBoxDefinition;
        }

        // Fire basic move event
        UFE.FireBasicMove(controlsScript.currentBasicMoveReference, controlsScript);
    }

    public void SetAnimationPosition(Fix64 normalizedTime)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetCurrentClipPosition(normalizedTime);
        }
        else
        {
            mecanimControl.SetCurrentClipPosition(normalizedTime);
        }
    }

    public Vector3 GetDeltaDisplacement()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetDeltaDisplacement();
        }
        else
        {
            return mecanimControl.GetDeltaDisplacement();
        }
    }

    public Vector3 GetDeltaPosition()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetDeltaPosition();
        }
        else
        {
            return mecanimControl.GetDeltaPosition();
        }
    }

    public string GetCurrentClipName()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetCurrentClipName();
        }
        else
        {
            return mecanimControl.GetCurrentClipName();
        }
    }

    public Fix64 GetCurrentClipPosition()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetCurrentClipPosition();
        }
        else
        {
            return mecanimControl.GetCurrentClipPosition();
        }
    }

    public Fix64 GetCurrentClipNormalizedTime()
    {
        return mecanimControl.GetCurrentClipNormalizedTime();
    }

    public int GetCurrentClipFrame(bool bakeSpeed = false)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetCurrentClipFrame(bakeSpeed);
        }
        else
        {
            return mecanimControl.GetCurrentClipFrame(bakeSpeed);
        }
    }

    public Fix64 GetAnimationNormalizedTime(int animFrame, MoveInfo move)
    {
        if (move == null) return 0;
        if (move._animationSpeed < 0)
        {
            return (animFrame / (Fix64)move.totalFrames) + 1;
        }
        else
        {
            return animFrame / (Fix64)move.totalFrames;
        }
    }

    public void SetMecanimMirror(bool toggle)
    {
        mecanimControl.SetMirror(toggle, UFE.config.characterRotationOptions._mirrorBlending, true);
    }

    public bool CompareBlockButtons(ButtonPress button)
    {
        if (button == ButtonPress.Button1 && UFE.config.blockOptions.blockType == BlockType.HoldButton1) return true;
        if (button == ButtonPress.Button2 && UFE.config.blockOptions.blockType == BlockType.HoldButton2) return true;
        if (button == ButtonPress.Button3 && UFE.config.blockOptions.blockType == BlockType.HoldButton3) return true;
        if (button == ButtonPress.Button4 && UFE.config.blockOptions.blockType == BlockType.HoldButton4) return true;
        if (button == ButtonPress.Button5 && UFE.config.blockOptions.blockType == BlockType.HoldButton5) return true;
        if (button == ButtonPress.Button6 && UFE.config.blockOptions.blockType == BlockType.HoldButton6) return true;
        if (button == ButtonPress.Button7 && UFE.config.blockOptions.blockType == BlockType.HoldButton7) return true;
        if (button == ButtonPress.Button8 && UFE.config.blockOptions.blockType == BlockType.HoldButton8) return true;
        if (button == ButtonPress.Button9 && UFE.config.blockOptions.blockType == BlockType.HoldButton9) return true;
        if (button == ButtonPress.Button10 && UFE.config.blockOptions.blockType == BlockType.HoldButton10) return true;
        if (button == ButtonPress.Button11 && UFE.config.blockOptions.blockType == BlockType.HoldButton11) return true;
        if (button == ButtonPress.Button12 && UFE.config.blockOptions.blockType == BlockType.HoldButton12) return true;
        return false;
    }

    public bool CompareParryButtons(ButtonPress button)
    {
        if (button == ButtonPress.Button1 && UFE.config.blockOptions.parryType == ParryType.TapButton1) return true;
        if (button == ButtonPress.Button2 && UFE.config.blockOptions.parryType == ParryType.TapButton2) return true;
        if (button == ButtonPress.Button3 && UFE.config.blockOptions.parryType == ParryType.TapButton3) return true;
        if (button == ButtonPress.Button4 && UFE.config.blockOptions.parryType == ParryType.TapButton4) return true;
        if (button == ButtonPress.Button5 && UFE.config.blockOptions.parryType == ParryType.TapButton5) return true;
        if (button == ButtonPress.Button6 && UFE.config.blockOptions.parryType == ParryType.TapButton6) return true;
        if (button == ButtonPress.Button7 && UFE.config.blockOptions.parryType == ParryType.TapButton7) return true;
        if (button == ButtonPress.Button8 && UFE.config.blockOptions.parryType == ParryType.TapButton8) return true;
        if (button == ButtonPress.Button9 && UFE.config.blockOptions.parryType == ParryType.TapButton9) return true;
        if (button == ButtonPress.Button10 && UFE.config.blockOptions.parryType == ParryType.TapButton10) return true;
        if (button == ButtonPress.Button11 && UFE.config.blockOptions.parryType == ParryType.TapButton11) return true;
        if (button == ButtonPress.Button12 && UFE.config.blockOptions.parryType == ParryType.TapButton12) return true;
        return false;
    }

    private bool hasEnoughGauge(Fix64 gaugeNeeded, int targetGauge)
    {
        if (!UFE.config.gameGUI.hasGauge) return true;
        if (controlsScript.currentGaugesPoints[targetGauge] < (controlsScript.myInfo.maxGaugePoints * (gaugeNeeded / 100))) return false;
        return true;
    }

    public MoveInfo InstantiateMove(MoveInfo move)
    {
        if (move == null) return null;
        MoveInfo newMove = Instantiate(move);
        return newMove;
    }

    public void GetNextMove(MoveInfo currentMove, ref MoveInfo storedMove)
    {
        if (currentMove.frameLinks.Length == 0) return;

        foreach (FrameLink frameLink in currentMove.frameLinks)
        {
            if (frameLink.cancelable)
            {
                if (frameLink.useCharacterMoves && frameLink.characterSpecificMoves.Length > 0)
                {
                    foreach (CharacterSpecificMoves csMove in frameLink.characterSpecificMoves)
                    {
                        if (controlsScript.myInfo.characterName == csMove.characterName && CheckMoveLinkRequirements(csMove.move, frameLink))
                        {
                            storedMove = InstantiateMove(csMove.move);
                        }
                    }
                }
                else if (frameLink.linkableMoves.Length > 0)
                {
                    foreach (MoveInfo move in frameLink.linkableMoves)
                    {
                        if (CheckMoveLinkRequirements(move, frameLink))
                        {
                            storedMove = InstantiateMove(move);
                        }
                    }
                }
            }
        }
    }

    private bool CheckMoveLinkRequirements(MoveInfo move, FrameLink frameLink)
    {
        if (move == null) return false;

        bool gaugePass = true;
        if (!frameLink.ignoreGauge)
            foreach (GaugeInfo gaugeInfo in move.gauges)
                if (!hasEnoughGauge(gaugeInfo._gaugeRequired, (int)gaugeInfo.targetGauge)) gaugePass = false;

        if (gaugePass &&
            (move.defaultInputs.buttonExecution.Length == 0 || frameLink.ignoreInputs ||
            (move.defaultInputs.onReleaseExecution && !move.defaultInputs.requireButtonPress && controlsScript.inputHeldDown[move.defaultInputs.buttonExecution[0]] == 0)))
            return true;

        return false;
    }

    public void ClearLastButtonSequence()
    {
        lastButtonPresses.Clear();
        lastTimePress = 0;
    }

    private bool checkExecutionState(ButtonPress[] buttonPress, bool inputUp)
    {
        if (inputUp
            && lastButtonPresses.Count > 0
            && buttonPress[0].Equals(lastButtonPresses.ToArray()[lastButtonPresses.Count - 1])) return false;

        return true;
    }

    public MoveInfo GetMove(ButtonPress[] buttonPress, Fix64 charge, MoveInfo currentMove, bool inputUp)
    {
        return GetMove(buttonPress, charge, currentMove, inputUp, false);
    }

    public MoveInfo GetMove(ButtonPress[] buttonPress, Fix64 charge, MoveInfo currentMove, bool inputUp, bool forceExecution)
    {
        if (buttonPress.Length > 0 &&
            (UFE.currentFrame / (Fix64)UFE.config.fps) - lastTimePress <= controlsScript.myInfo._executionTiming)
        {

            // Attempt first execution
            foreach (MoveInfo move in moves)
            {
                if (move == null) continue;
                MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, true);
                if (newMove != null) return newMove;
            }
        }

        // If buttons were pressed, add it to last button presses
        if (buttonPress.Length > 0)
        {
            if (!forceExecution)
            {
                // If button down event happened on the same frame as last input, merge inputs
                if (!inputUp && charge == 0 && lastButtonPresses.Count > 0 && lastButtonPresses[^1].chargeTime == 0 && lastTimePress == (UFE.currentFrame / (Fix64)UFE.config.fps))
                {
                    lastButtonPresses[^1].buttonPresses = lastButtonPresses[^1].buttonPresses.Concat(buttonPress).ToArray();
                }
                // Else, add to sequence list
                else
                {
                    lastButtonPresses.Add(new ButtonSequenceRecord(buttonPress, charge));
                    lastTimePress = UFE.currentFrame / (Fix64)UFE.config.fps;
                }

                if (controlsScript.debugInfo.buttonSequence)
                {
                    string allbp = "";
                    foreach (ButtonSequenceRecord bpr in lastButtonPresses)
                    {
                        allbp += bpr.chargeTime > 0 ? " (up)" : " (down)";
                        foreach (ButtonPress bp in bpr.buttonPresses)
                        {
                            allbp += " " + bp.ToString();
                        }
                        allbp += " | ";
                    }
                    Debug.Log(allbp);
                }
            }

            // If input sequence failed to cast a move, attempt second execution with current inputs
            foreach (MoveInfo move in moves)
            {
                MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, false, forceExecution);
                if (newMove != null) return newMove;
            }
        }

        return null;
    }

    private bool SearchMoveBuffer(string uniqueId, FrameLink[] frameLinks, int currentFrame)
    {
        foreach (FrameLink frameLink in frameLinks)
        {
            if ((currentFrame >= frameLink.activeFramesBegins && currentFrame <= frameLink.activeFramesEnds)
                || currentFrame >= (frameLink.activeFramesBegins - UFE.config.executionBufferTime)
                && currentFrame <= frameLink.activeFramesEnds && frameLink.allowBuffer)
            {
                if (frameLink.useCharacterMoves)
                {
                    foreach (CharacterSpecificMoves csMove in frameLink.characterSpecificMoves)
                    {
                        if (csMove.move != null && uniqueId == csMove.move.id && csMove.characterName == controlsScript.myInfo.characterName) return true;
                    }
                }
                else
                {
                    foreach (MoveInfo move in frameLink.linkableMoves)
                    {
                        if (move != null && uniqueId == move.id) return true;
                    }
                }
            }
        }

        return false;
    }

    public bool SearchMove(string uniqueId, FrameLink[] frameLinks, bool ignoreConditions = false)
    {
        foreach (FrameLink frameLink in frameLinks)
        {
            if (frameLink.cancelable)
            {
                if (ignoreConditions && !frameLink.ignorePlayerConditions) continue;

                if (frameLink.useCharacterMoves)
                {
                    foreach (CharacterSpecificMoves csMove in frameLink.characterSpecificMoves)
                    {
                        if (csMove.move != null && uniqueId == csMove.move.id && csMove.characterName == controlsScript.myInfo.characterName) return true;
                    }
                }
                else
                {
                    foreach (MoveInfo move in frameLink.linkableMoves)
                    {
                        if (move != null && uniqueId == move.id) return true;
                    }
                }
            }
        }

        return false;
    }

    private bool searchMove(string uniqueId, MoveInfo[] moves)
    {
        foreach (MoveInfo move in moves)
        {
            if (move == null) continue;
            if (uniqueId == move.id) return true;
        }

        return false;
    }

    public bool HasMove(string uniqueId)
    {
        foreach (MoveInfo move in this.moves)
            if (uniqueId == move.id) return true;

        return false;
    }


    public bool ValidateMoveExecution(MoveInfo move)
    {
        if (!searchMove(move.id, attackMoves)) return false;
        if (!ValidateMoveStances(move.selfConditions, controlsScript, true)) return false;
        if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return false;
        if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return false;
        if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return false;

        foreach (GaugeInfo gaugeInfo in move.gauges)
        {
            if (!hasEnoughGauge(gaugeInfo._gaugeRequired, (int)gaugeInfo.targetGauge)) return false;
        }

        if (move.previousMoves.Length > 0 && controlsScript.currentMove == null) return false;
        if (move.previousMoves.Length > 0 && !searchMove(controlsScript.currentMove.id, move.previousMoves)) return false;

        if (controlsScript.currentMove != null && controlsScript.currentMove.frameLinks.Length == 0) return false;
        if (controlsScript.currentMove != null && !SearchMove(move.id, controlsScript.currentMove.frameLinks)) return false;
        return true;
    }

    public bool ValidateMoveStances(PlayerConditions conditions, ControlsScript cScript, bool bypassCrouchStance = false)
    {
        bool stateCheck = conditions.possibleMoveStates.Length == 0;
        foreach (PossibleMoveStates possibleMoveState in conditions.possibleMoveStates)
        {
            if (possibleMoveState.possibleState != cScript.currentState
                && (!bypassCrouchStance || (bypassCrouchStance && cScript.currentState != PossibleStates.Stand))) continue;

            if (cScript.normalizedDistance < (Fix64)possibleMoveState.proximityRangeBegins / 100) continue;
            if (cScript.normalizedDistance > (Fix64)possibleMoveState.proximityRangeEnds / 100) continue;

            if (cScript.currentState == PossibleStates.Stand)
            {
                //if (cScript.Physics.isTakingOff) continue;
                if (!possibleMoveState.standBy && cScript.currentSubState == SubStates.Resting) continue;
                if (!possibleMoveState.movingBack && cScript.currentSubState == SubStates.MovingBack) continue;
                if (!possibleMoveState.movingForward && cScript.currentSubState == SubStates.MovingForward) continue;

            }
            else if (cScript.currentState == PossibleStates.NeutralJump
                    || cScript.currentState == PossibleStates.ForwardJump
                    || cScript.currentState == PossibleStates.BackJump)
            {

                if (cScript.normalizedJumpArc < (Fix64)possibleMoveState.jumpArcBegins / 100) continue;
                if (cScript.normalizedJumpArc > (Fix64)possibleMoveState.jumpArcEnds / 100) continue;
            }

            if (possibleMoveState.possibleState != PossibleStates.Down)
            {
                //Reminder: If condition is true we can't preform the move
                if (UFE.config.blockOptions.allowMoveCancel)
                {
                    //When Allow Move Cancel is true we can ignore the Blocking player condition

                    //All True AND False Combinations
                    //Block Stunned == True AND Stunned = True
                    /*if (possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {

                    }*/

                    //Block Stunned == False AND Stunned = False
                    if (!possibleMoveState.blockStunned && !possibleMoveState.stunned)
                    {
                        //if (cScript.blockStunned || cScript.stunTime > 0) continue;
                        if (cScript.blockStunned || cScript.currentSubState == SubStates.Stunned) continue;
                    }

                    //Block Stunned Combinations
                    //Block Stunned = True AND Stunned = False
                    if (possibleMoveState.blockStunned && !possibleMoveState.stunned)
                    {
                        if (cScript.currentSubState == SubStates.Stunned) continue;
                    }

                    //Stunned Combinations
                    //Block Stunned = False AND Stunned = True
                    if (!possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {
                        if (cScript.blockStunned) continue;
                    }
                }
                else
                {
                    //All True AND False Combinations
                    //Blocking == True AND Block Stunned == True AND Stunned = True
                    /*if (possibleMoveState.blocking && possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {

                    }*/

                    //Blocking == False AND Block Stunned == False AND Stunned = False
                    if (!possibleMoveState.blocking && !possibleMoveState.blockStunned && !possibleMoveState.stunned)
                    {
                        //if (cScript.isBlocking || cScript.blockStunned || cScript.stunTime > 0) continue;
                        if (cScript.isBlocking || cScript.blockStunned || cScript.currentSubState == SubStates.Stunned) continue;
                    }

                    //Blocking Combinations
                    //Blocking == True AND Block Stunned == False AND Stunned = False
                    if (possibleMoveState.blocking && !possibleMoveState.blockStunned && !possibleMoveState.stunned)
                    {
                        if (cScript.blockStunned || cScript.currentSubState == SubStates.Stunned) continue;
                    }

                    //Blocking == True AND Block Stunned == True AND Stunned = False
                    if (possibleMoveState.blocking && possibleMoveState.blockStunned && !possibleMoveState.stunned)
                    {
                        if (cScript.currentSubState == SubStates.Stunned) continue;
                    }

                    //Blocking == True AND Block Stunned == False AND Stunned = True
                    if (possibleMoveState.blocking && !possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {
                        if (cScript.blockStunned) continue;
                    }

                    //Block Stunned Combinations
                    //Blocking = False AND Block Stunned = True AND Stunned = False
                    if (!possibleMoveState.blocking && possibleMoveState.blockStunned && !possibleMoveState.stunned)
                    {
                        if (cScript.isBlocking && !cScript.blockStunned || cScript.currentSubState == SubStates.Stunned) continue;
                    }

                    //Blocking = True AND Block Stunned = True AND Stunned = False
                    if (possibleMoveState.blocking && possibleMoveState.blockStunned && !possibleMoveState.stunned)
                    {
                        if (cScript.currentSubState == SubStates.Stunned) continue;
                    }

                    //Blocking = False AND Block Stunned = True AND Stunned = True
                    if (!possibleMoveState.blocking && possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {
                        if (cScript.isBlocking && !cScript.blockStunned) continue;
                    }

                    //Stunned Combinations
                    //Blocking = False AND Block Stunned = False AND Stunned = True
                    if (!possibleMoveState.blocking && !possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {
                        if (cScript.isBlocking || cScript.blockStunned) continue;
                    }

                    //Blocking = True AND Block Stunned = False AND Stunned = True
                    if (possibleMoveState.blocking && !possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {
                        if (cScript.blockStunned) continue;
                    }

                    //Blocking = False AND Block Stunned = True AND Stunned = True
                    if (!possibleMoveState.blocking && possibleMoveState.blockStunned && possibleMoveState.stunned)
                    {
                        if (cScript.isBlocking && !cScript.blockStunned) continue;
                    }
                }
            }

            stateCheck = true;
        }
        return stateCheck;
    }

    public bool ValidadeBasicMove(PlayerConditions conditions, ControlsScript cScript)
    {
        if (conditions.basicMoveLimitation.Length == 0) return true;
        if (Array.IndexOf(conditions.basicMoveLimitation, cScript.currentBasicMoveReference) >= 0) return true;
        return false;
    }

    public bool CanPlink(MoveInfo currentMove, MoveInfo tempMove)
    {
        // ignore plink candidate if
        if (currentMove == null) return false; // current move doesnt exist
        if (currentMove.currentFrame > UFE.config.plinkingDelay) return false; // current frame is outside plinking window
        if (currentMove.defaultInputs.buttonExecution.Length == 0) return false; // current move has no button execution
        if (currentMove.defaultInputs.onReleaseExecution) return false; // current move has a release button execution
        if (currentMove.previousMoves.Length > 0) return false; // current move is coming from a previous move chain
        if (tempMove.defaultInputs.buttonSequence.Length < currentMove.defaultInputs.buttonSequence.Length) return false; // plink candidate has less button sequences than current move
        if (tempMove.defaultInputs.buttonExecution.Length <= currentMove.defaultInputs.buttonExecution.Length) return false; // plink candidate has less (or equal) number of button execution than current move
        ButtonPress[] compareExecution = ArrayIntersect<ButtonPress>(currentMove.defaultInputs.buttonExecution, tempMove.defaultInputs.buttonExecution);
        if (!ArraysEqual<ButtonPress>(compareExecution, currentMove.defaultInputs.buttonExecution)) return false; // current move's button executions do not match the button executions from plinking candidate

        return true;
    }

    private MoveInfo TestMoveExecution(MoveInfo move, MoveInfo currentMove, ButtonPress[] buttonPress, bool inputUp, bool fromSequence = false, bool forceExecution = false)
    {
        foreach (GaugeInfo gaugeInfo in move.gauges)
        {
            if (!hasEnoughGauge(gaugeInfo._gaugeRequired, (int)gaugeInfo.targetGauge)) return null;
        }
        if (move.previousMoves.Length > 0 && currentMove == null) return null;
        if (move.previousMoves.Length > 0 && !searchMove(currentMove.id, move.previousMoves)) return null;
        if (controlsScript.isAirRecovering && controlsScript.airRecoveryType == AirRecoveryType.CantMove) return null;
        if (move.cooldown && lastMovesPlayed.ContainsKey(move.id) && UFE.currentFrame - lastMovesPlayed[move.id] <= move.cooldownFrames) return null;


        // Look for Projectiles On Screen
        if (move.projectiles.Length > 0 && controlsScript.projectiles.Count > 0)
        {
            int totalOnScreen = 0;
            foreach (ProjectileMoveScript pScript in controlsScript.projectiles)
            {
                if (pScript == null || !pScript.IsActive()) continue;
                if (pScript.data.limitMultiCasting)
                {
                    if (pScript.isActiveAndEnabled && pScript.onView) totalOnScreen++;
                    foreach (Projectile proj in move.projectiles)
                    {
                        if (proj.limitMultiCasting && ((!proj.limitOnlyThis && totalOnScreen >= proj.onScreenLimit) ||
                            (pScript.data.uniqueId == move.id && totalOnScreen >= proj.onScreenLimit))) return null;
                    }
                }
            }

        }

        // Look for Assists on Screen
        if (move.characterAssist.Length > 0)
        {
            foreach (ControlsScript cScript in controlsScript.assists)
            {
                if (!cScript.GetActive()) continue;
                foreach (CharacterAssist cAssist in move.characterAssist)
                {
                    if (cScript.myInfo.characterName == cAssist.characterInfo.characterName) return null;
                }
            }

        }

        if (currentMove == null || (currentMove != null && !SearchMove(move.id, currentMove.frameLinks, true)))
        {
            if (!ValidateMoveStances(move.selfConditions, controlsScript)) return null;
            if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return null;
            if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return null;
            if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return null;
        }

        if (!CompareSequence(move.defaultInputs, buttonPress, inputUp, fromSequence, true)
            && !CompareSequence(move.altInputs, buttonPress, inputUp, fromSequence, false)) return null;


        if (controlsScript.storedMove != null && move.id == controlsScript.storedMove.id)
            return controlsScript.storedMove;

        if (controlsScript.debugInfo.buttonSequence)
        {
            string allbp4 = "";
            foreach (ButtonPress bp in buttonPress) allbp4 += " " + bp.ToString();
            Debug.Log(move.moveName + ": Button Execution: " + allbp4);
        }

        if (currentMove == null || forceExecution || SearchMoveBuffer(move.id, currentMove.frameLinks, currentMove.currentFrame) || UFE.config.executionBufferType == ExecutionBufferType.AnyMove)
        {
            if ((controlsScript.currentState == PossibleStates.NeutralJump ||
                controlsScript.currentState == PossibleStates.ForwardJump ||
                controlsScript.currentState == PossibleStates.BackJump) &&
                totalAirMoves >= controlsScript.myInfo.possibleAirMoves) return null;

            MoveInfo newMove = InstantiateMove(move);
            return newMove;
        }

        return null;
    }


    private bool CompareSequence(MoveInputs moveInputs, ButtonPress[] buttonPress, bool inputUp, bool fromSequence, bool allowEmptyExecution)
    {
        if (!allowEmptyExecution && moveInputs.buttonExecution.Length == 0) return false;
        Array.Sort(buttonPress);
        Array.Sort(moveInputs.buttonExecution);

        if (fromSequence)
        {
            if (moveInputs.buttonSequence.Length == 0) return false;
            if (moveInputs.chargeMove)
            {
                bool charged = false;
                foreach (ButtonSequenceRecord bsr in lastButtonPresses)
                {
                    if (Array.IndexOf(bsr.buttonPresses, moveInputs.buttonSequence[0]) >= 0 && bsr.chargeTime >= moveInputs._chargeTiming)
                    {
                        charged = true;
                    }
                }

                if (!charged) return false;
            }

            List<ButtonPress[]> buttonPressesList = new List<ButtonPress[]>();
            foreach (ButtonSequenceRecord bsr in lastButtonPresses)
            {
                if (bsr.chargeTime == 0 || (moveInputs.allowNegativeEdge && Array.IndexOf(bsr.buttonPresses, moveInputs.buttonSequence[0]) >= 0))
                {
                    if (moveInputs.forceAxisPrecision)
                    {
                        List<ButtonPress> filteredBtp = new List<ButtonPress>(bsr.buttonPresses);
                        if (filteredBtp.Contains(ButtonPress.DownBack)
                            || filteredBtp.Contains(ButtonPress.DownForward)
                            || filteredBtp.Contains(ButtonPress.UpBack)
                            || filteredBtp.Contains(ButtonPress.UpForward))
                        {
                            filteredBtp.RemoveAll(item => (int)item <= 3);
                        }

                        buttonPressesList.Add(filteredBtp.ToArray());
                    }
                    else
                    {
                        buttonPressesList.Add(bsr.buttonPresses);
                    }
                }
            }

            if (buttonPressesList.Count >= moveInputs.buttonSequence.Length)
            {
                int compareRange = buttonPressesList.Count - moveInputs.buttonSequence.Length;

                if (moveInputs.allowInputLeniency) compareRange -= moveInputs.leniencyBuffer;
                if (compareRange < 0) compareRange = 0;

                ButtonPress[][] buttonPressesListArray = buttonPressesList.GetRange(compareRange, buttonPressesList.Count - compareRange).ToArray();
                ButtonPress[] compareSequence = ArrayIntersect<ButtonPress>(moveInputs.buttonSequence, buttonPressesListArray);

                if (!ArraysEqual<ButtonPress>(compareSequence, moveInputs.buttonSequence)) return false;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (moveInputs.buttonSequence.Length > 0) return false;
        }

        if (!inputUp && !moveInputs.onPressExecution) return false;
        if (inputUp && !moveInputs.onReleaseExecution) return false;
        if (!ArraysEqual<ButtonPress>(buttonPress, moveInputs.buttonExecution)) return false;

        return true;
    }

    private T[] ArrayIntersect<T>(T[] a1, T[] a2)
    {
        if (a1 == null || a2 == null) return null;

        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        List<T> intersection = new List<T>();
        int nextStartingPoint = 0;
        for (int i = 0; i < a1.Length; i++)
        { // button sequence
            bool added = false;
            for (int k = nextStartingPoint; k < a2.Length; k++)
            { // button presses
                if (comparer.Equals(a1[i], a2[k]))
                {
                    intersection.Add(a2[k]);
                    nextStartingPoint = k;
                    added = true;
                    break;
                }
            }
            if (!added) return null;
        }

        return intersection.ToArray();
    }

    private T[] ArrayIntersect<T>(T[] a1, T[][] a2)
    {
        if (a1 == null || a2 == null) return null;

        List<T> intersection = new List<T>();
        int sCount = 0;
        for (int i = 0; i < a2.Length; i++)
        {
            if (sCount < a1.Length && a2[i].Contains(a1[sCount]))
            {
                intersection.Add(a1[sCount]);
                sCount++;
            }
        }

        return intersection.ToArray();
    }

    private bool ArraysEqual<T>(T[] a1, T[] a2)
    {
        if (ReferenceEquals(a1, a2)) return true;
        if (a1 == null || a2 == null) return false;
        if (a1.Length != a2.Length) return false;
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < a1.Length; i++)
        {
            if (!comparer.Equals(a1[i], a2[i])) return false;
        }
        return true;
    }
}
