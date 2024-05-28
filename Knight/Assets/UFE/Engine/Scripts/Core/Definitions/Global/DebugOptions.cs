using UnityEngine;

[System.Serializable]
public class DebugOptions
{
    public bool debugMode;
    public bool emulateNetwork;
    public bool trainingModeDebugger;
    public bool preloadedObjects;

    public bool inputsToggle;
    public bool networkToggle;
    public bool recordToggle;
    public bool hitboxColorsToggle;
    public bool displayCollisionMassGizmos = false;

    public GameObject networkDebugger = null;
    public TextAnchor netDebugTextAlignment;
    public Vector2 netDebugTextPosition;
    public bool connectionLog = true;
    public bool ping = true;
    public bool frameDelay = true;
    public bool currentLocalFrame = true;
    public bool currentNetworkFrame = true;
    public bool rollbackLog = false;
    public bool displayReplayInTraining = false;
    public bool stateTrackerTest = false;
    public bool recordMatchTools = false;
    public bool playbackPhysics = false;

    public bool displayInputsNetwork = false;
    public bool displayInputsVersus = false;
    public bool displayInputsTraining = false;
    public bool displayInputsStoryMode = false;
    public bool displayInputsChallengeMode = false;

    public CharacterDebugInfo p1DebugInfo;
    public CharacterDebugInfo p2DebugInfo;
}