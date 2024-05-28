#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class MapRecorder : MonoBehaviour
{

    [SerializeField]
    public UFE3D.CharacterInfo characterInfo;
    public bool bakeSpeedValues = false;
    public bool bakeGameSpeed = false;

    [HideInInspector] public HitBoxesScript hitBoxesScript;
    private MecanimControl mecanimControl;
    private LegacyControl legacyControl;
    private List<MoveSetData> loadedMoveSets = new List<MoveSetData>();

    private Animator mAnimator;
    private Animation lAnimator;
    private GameObject character;
    private BasicMoveInfo currentBasicMove;
    private MoveInfo currentMove;
    private AnimSpeedKeyFrame[] animSpeedKeyFrames;

    private int currentStanceNum;
    private int currentClipNum;
    private int currentMoveNum;
    private int currentFrame;
    private Fix64 currentSpeed;
    private int totalFrames;
    private bool recording = false;
    private bool doOnce = false;
    private int recordAllProgress = 0;
    private bool recordingSpecialMoves = false;

    private AnimationMapData AM_File;

    private string recordingFeed;
    Rect sliderLine = new Rect(100, Screen.height - 20, Screen.width - 200, 40);

    void Awake()
    {
        if (characterInfo.characterPrefabStorage == StorageMode.Prefab)
        {
            character = Instantiate(characterInfo.characterPrefab);
        }
        else
        {
            character = Instantiate(Resources.Load<GameObject>(characterInfo.prefabResourcePath));
        }
        character.transform.position = new Vector3(0, 0, 0);

        foreach (string path in characterInfo.stanceResourcePath)
        {
            loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
        }
        foreach (MoveSetData moveSetData in characterInfo.moves)
        {
            loadedMoveSets.Add(moveSetData);
        }

        foreach(MoveSetData moveSetData in loadedMoveSets)
            moveSetData.basicMoves.UpdateDictionary();

        AM_File = ScriptableObject.CreateInstance<AnimationMapData>();

        if (characterInfo.animationType == AnimationType.Legacy)
        {
            lAnimator = character.GetComponent<Animation>();
            if (lAnimator == null) lAnimator = character.AddComponent<Animation>();
            legacyControl = character.AddComponent<LegacyControl>();
            legacyControl.AddClip(loadedMoveSets[0].basicMoves.idle.animData[0].clip, "default");
            legacyControl.overrideAnimatorUpdate = true;
        }
        else
        {
            mAnimator = character.GetComponent<Animator>();
            if (mAnimator == null) mAnimator = character.AddComponent<Animator>();
            mecanimControl = character.AddComponent<MecanimControl>();
            mecanimControl.overrideAnimatorUpdate = true;
            mecanimControl.normalizeFrames = false;
            mAnimator.applyRootMotion = true;
            mAnimator.avatar = characterInfo.avatar;
        }
        hitBoxesScript = character.GetComponent<HitBoxesScript>();
        hitBoxesScript.UpdateRenderer();

        Camera.main.transform.position = new Vector3(0, 4, -40);

        UFE.fps = UFE.GetActiveConfig().fps;
    }

    void FixedUpdate()
    {
        if (recording)
        {
            if (characterInfo.animationType == AnimationType.Legacy)
            {
                legacyControl.DoFixedUpdate();
            }
            else
            {
                mecanimControl.DoFixedUpdate();
            }
            MapHitBoxes();
        }
        else
        {
            UFE.timeScale = 0;
        }
    }

    private void OnGUI()
    {
        if (recording)
        {
            GUI.HorizontalSlider(sliderLine, currentFrame, 0, totalFrames);
        }

        GUI.Box(new Rect(10, 10, 220, 150), "Animation Map Recorder");
        GUI.BeginGroup(new Rect(20, 30, 200, 150));
        {
            if (recording)
            {
                if (GUILayout.Button("Stop Recording", GUILayout.Width(200))) recording = false;

                GUILayout.Label("Current Move Set: " + (currentStanceNum + 1));

                GUILayout.Label(recordingFeed);

                GUILayout.HorizontalSlider(currentFrame, 0, totalFrames, GUILayout.Width(200));
            }
            else
            {
                if (GUILayout.Button("Record All", GUILayout.Width(200)))
                {
                    currentStanceNum = 0;
                    recordingSpecialMoves = false;
                    recordAllProgress = 1;
                    RecordAllBasicMoves();
                }

                GUILayout.Label("----------OR----------");

                GUILayout.Label("Selected Move Set: " + (currentStanceNum + 1));
                string[] selStrings = new string[loadedMoveSets.Count];
                for (int i = 0; i < loadedMoveSets.Count; i++)
                {
                    selStrings[i] = "Stance " + (i + 1);
                }
                currentStanceNum = GUILayout.SelectionGrid(currentStanceNum, selStrings, 3);
                if (GUILayout.Button("Record " + selStrings[currentStanceNum], GUILayout.Width(200)))
                {
                    recordAllProgress = 0;
                    recordingSpecialMoves = false;
                    AM_File = ScriptableObject.CreateInstance<AnimationMapData>();
                    RecordAllBasicMoves();
                }
            }
        }
        GUI.EndGroup();
    }

    public void RestoreSpeed()
    {
        UFE.timeScale = bakeGameSpeed ? UFE.GetActiveConfig()._gameSpeed : 1;
        UFE.fixedDeltaTime = 1 / (Fix64)UFE.fps;
    }

    public void RecordAllSpecialMoves()
    {
        currentBasicMove = null;
        currentMove = loadedMoveSets[currentStanceNum].attackMoves[0];
        currentMoveNum = 0;
        currentFrame = 0;
        totalFrames = 0;
        recording = true;
        doOnce = false;
    }

    public void RecordAllBasicMoves()
    {
        currentBasicMove = loadedMoveSets[currentStanceNum].basicMoves.idle;
        currentMove = null;
        currentClipNum = 0;
        currentFrame = 0;
        totalFrames = 0;
        recording = true;
        doOnce = false;
    }

    public void MapHitBoxes()
    {
        MoveSetData moveSetData = loadedMoveSets[currentStanceNum];
        bool finished = false;

        if (currentMove != null)
        {
            currentMove = MapSpecialMove(currentMove, ref finished);

            if (finished)
            {
                if (doOnce)
                {
                    doOnce = false;
                    recording = false;
                }
                else if (currentBasicMove != null && currentBasicMove.useMoveFile && currentBasicMove.moveInfo != null && currentBasicMove.moveInfo.id == currentMove.id)
                {
                    currentMove = null;
                    NextBasicMove(moveSetData);
                }
                else
                {
                    moveSetData.attackMoves[currentMoveNum] = currentMove;
                    currentMoveNum++;
                    if (currentMoveNum == moveSetData.attackMoves.Length)
                    {
                        recording = false;
                    }
                    else
                    {
                        currentMove = moveSetData.attackMoves[currentMoveNum];
                    }
                }
            }

        }
        else
        {
            currentBasicMove = MapBasicMove(currentBasicMove, currentBasicMove.reference.ToString(), ref finished);
            if (finished)
            {
                NextBasicMove(moveSetData);
            }
        }

        if (!recording)
        {
            SaveAMFile(moveSetData);
            recordingSpecialMoves = false;

            if (recordAllProgress > 0)
            {
                if (currentStanceNum >= loadedMoveSets.Count - 1)
                {
                    recordAllProgress = 0;
                }
                else
                {
                    currentStanceNum++;
                    AM_File = ScriptableObject.CreateInstance<AnimationMapData>();
                    RecordAllBasicMoves();
                }
            }
        }
    }

    private void NextBasicMove(MoveSetData moveSetData)
    {
        BasicMoveInfo prevBasicMove = currentBasicMove;
        bool next = false;
        foreach (BasicMoveInfo basicMoveInfo in moveSetData.basicMoves.basicMoveDictionary.Keys)
        {
            if (next)
            {
                currentBasicMove = basicMoveInfo;
                break;
            }
            if (basicMoveInfo == currentBasicMove) next = true;
        }

        if (prevBasicMove == currentBasicMove)
        {
            if (recordAllProgress == 1 || !recordingSpecialMoves)
            {
                recordingSpecialMoves = true;
                RecordAllSpecialMoves();
            }
        }
    }

    private void SaveAMFile(MoveSetData moveSetData)
    {
        StanceInfo reference = Resources.Load<StanceInfo>(characterInfo.stanceResourcePath[currentStanceNum]);
        string path = AssetDatabase.GetAssetPath(reference);
        if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(reference)), "");
        }

        string assetPathAndName = path + reference.name;

        if (!AssetDatabase.Contains(AM_File))
            AssetDatabase.CreateAsset(AM_File, assetPathAndName + "_maps.asset");

        moveSetData.AM_File = AM_File;
        StanceInfo newStanceInfo = moveSetData.ConvertData();
        if (!AssetDatabase.Contains(newStanceInfo)) 
            AssetDatabase.CreateAsset(newStanceInfo, assetPathAndName + ".asset");
        
        AssetDatabase.SaveAssets();
        Selection.activeObject = AM_File;

        EditorUtility.SetDirty(characterInfo);

        recordingFeed = "Animation Maps File Created: " + assetPathAndName + "_maps.asset";
        Debug.Log(recordingFeed);
    }

    private SerializedAnimationData AnimationSetup(string moveId, SerializedAnimationData sAnimData, Fix64 speed)
    {
        if (characterInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.RemoveAllClips();
            legacyControl.AddClip(sAnimData.clip, sAnimData.clip.name, speed, WrapMode.Clamp);
            legacyControl.Play(sAnimData.clip.name, 0, 0);
        }
        else
        {
            mecanimControl.SetDefaultClip(sAnimData.clip, sAnimData.clip.name, speed, WrapMode.Clamp, false);
            mecanimControl.currentAnimationData = mecanimControl.defaultAnimation;
            mecanimControl.currentAnimationData.stateName = "State1";
            mecanimControl.currentAnimationData.length = sAnimData.clip.length;
            mecanimControl.Play(mecanimControl.defaultAnimation, 0, 0, false);

            mecanimControl.animator.Update((float)UFE.fixedDeltaTime);
        }

        sAnimData.id = moveId;
        sAnimData.length = sAnimData.clip.length;
        totalFrames = (int)FPMath.Round(sAnimData.length / speed * UFE.fps);
        currentSpeed = speed;

        return sAnimData;
    }

    private AnimationMaps[] NewSerializedMaps(AnimationMaps[] animMapsArray, AnimationClip clip, string animId, ref bool finished)
    {
        bool found = false;
        foreach (AnimationMaps anim in animMapsArray)
        {
            if (anim.id == animId)
            {
                anim.animationMaps = MapFrame(anim.animationMaps, clip, ref finished);
                found = true;
                break;
            }
        }

        if (!found)
        {
            AnimationMaps newAnimMap = new AnimationMaps();
            newAnimMap.id = animId;
            newAnimMap.animationMaps = MapFrame(new AnimationMap[0], clip, ref finished);
            List<AnimationMaps> animMapsList = new List<AnimationMaps>(animMapsArray);
            animMapsList.Add(newAnimMap);
            animMapsArray = animMapsList.ToArray();
        }

        return animMapsArray;
    }

    private MoveInfo MapSpecialMove(MoveInfo moveInfo, ref bool over)
    {
        bool finished = false;
        if (moveInfo.animData.clip != null)
        {
            if (currentFrame == 0)
            {
                Fix64 speed = 1;
                if (bakeSpeedValues)
                {
                    speed = FPMath.Abs(moveInfo._animationSpeed);
                    if (!moveInfo.fixedSpeed)
                    {
                        animSpeedKeyFrames = moveInfo.animSpeedKeyFrame;
                    }
                    else
                    {
                        animSpeedKeyFrames = null;
                    }
                }

                moveInfo.animData = AnimationSetup(moveInfo.id, moveInfo.animData, speed);
                moveInfo.animData.bakeSpeed = bakeSpeedValues;

                recordingFeed = "--- Mapping " + moveInfo.moveName;
                Debug.Log(recordingFeed);
            }

            AM_File.animationMaps = NewSerializedMaps(AM_File.animationMaps, moveInfo.animData.clip, moveInfo.animData.id, ref finished);

            if (finished)
            {
                character.transform.position = Vector3.zero;
                character.transform.localPosition = Vector3.zero;
                if (characterInfo.animationType != AnimationType.Legacy)
                {
                    mAnimator.rootPosition = Vector3.zero;
                    mAnimator.bodyPosition = Vector3.zero;
                    mAnimator.WriteDefaultValues();
                    mAnimator.gameObject.SetActive(false);
                    mAnimator.gameObject.SetActive(true);
                }
                recordingFeed = "Saved";
                Debug.Log(recordingFeed);
                over = true;
            }
        }
        else
        {
            over = true;
        }

        EditorUtility.SetDirty(moveInfo);
        return moveInfo;
    }

    private BasicMoveInfo MapBasicMove(BasicMoveInfo basicMove, string basicMoveName, ref bool over)
    {
        if (currentClipNum > 8)
        {
            currentClipNum = 0;
            over = true;
        }
        else
        {
            bool finished = false;
            if (!basicMove.useMoveFile && currentClipNum < basicMove.animData.Length && basicMove.animData[currentClipNum].clip != null)
            {
                if (currentFrame == 0)
                {
                    Fix64 speed = bakeSpeedValues && !basicMove.autoSpeed ? FPMath.Abs(basicMove._animationSpeed) : 1;
                    basicMove.animData[currentClipNum] = AnimationSetup(basicMove.id + "_" + currentClipNum, basicMove.animData[currentClipNum], speed);
                    basicMove.animData[currentClipNum].bakeSpeed = bakeSpeedValues;
                    recordingFeed = "--- Mapping " + basicMoveName;
                    Debug.Log(recordingFeed);
                }

                AM_File.animationMaps = NewSerializedMaps(AM_File.animationMaps, basicMove.animData[currentClipNum].clip, basicMove.animData[currentClipNum].id, ref finished);

                if (finished)
                {
                    character.transform.position = new Vector3(0, 0, 0);
                    recordingFeed = "Saved";
                    Debug.Log(recordingFeed);
                    currentClipNum++;
                }
            }
            else if (currentClipNum == 0 && basicMove.useMoveFile && basicMove.moveInfo != null)
            {
                currentMove = basicMove.moveInfo;
            }
            else
            {
                currentClipNum++;
            }

            over = false;
        }

        return basicMove;
    }

    private AnimationMap[] MapFrame(AnimationMap[] animationMaps, AnimationClip animationClip, ref bool finished)
    {
        List<AnimationMap> _animationMaps = new List<AnimationMap>(animationMaps);

        recordingFeed = "Mapping " + animationClip.name + " (" + currentFrame + ")";
        Debug.Log(recordingFeed);

        RestoreSpeed();
        if (animSpeedKeyFrames != null)
        {
            foreach (AnimSpeedKeyFrame speedKeyFrame in animSpeedKeyFrames)
            {
                if (currentFrame >= speedKeyFrame.castingFrame)
                {
                    Debug.Log("Applying Speed Key Frames: "+ speedKeyFrame._speed);
                    UFE.timeScale = speedKeyFrame._speed * currentSpeed;
                }
            }
        }

        AnimationMap animationMap = new AnimationMap();
        animationMap.frame = currentFrame;
        animationMap.hitBoxMaps = hitBoxesScript.GetAnimationMaps();

        if (characterInfo.animationType == AnimationType.Legacy)
        {
            animationMap.deltaDisplacement = FPVector.ToFPVector(legacyControl.GetDeltaPosition());
        }
        else
        {
            animationMap.deltaDisplacement = FPVector.ToFPVector(mecanimControl.GetDeltaPosition());
        }

        _animationMaps.Add(animationMap);

        // preview
        hitBoxesScript.animationMaps = _animationMaps.ToArray();

        hitBoxesScript.UpdateMap(currentFrame);

        currentFrame++;
        if (currentFrame >= totalFrames)
        {
            currentFrame = 0;
            finished = true;
        }

        return _animationMaps.ToArray();
    }
}
#endif