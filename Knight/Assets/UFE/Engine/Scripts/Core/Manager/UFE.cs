using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using FPLibrary;
using UFENetcode;
using UFE3D;

public partial class UFE : MonoBehaviour, UFEInterface
{
    #region Global info instance
    public GlobalInfo UFE_Config;
    #endregion


    #region Event definitions
    public delegate void MeterHandler(float newFloat, ControlsScript player);
    public static event MeterHandler OnLifePointsChange;

    public delegate void GaugeHandler(int targetGauge, float newValue, ControlsScript character);
    public static event GaugeHandler OnGaugeUpdate;

    public delegate void IntHandler(int newInt);
    public static event IntHandler OnRoundBegins;

    public delegate void StringHandler(string newString, ControlsScript player);
    public static event StringHandler OnNewAlert;

    public delegate void HitHandler(HitBox strokeHitBox, MoveInfo move, Hit hitInfo, ControlsScript player);
    public static event HitHandler OnHit;
    public static event HitHandler OnBlock;
    public static event HitHandler OnParry;

    public delegate void MoveHandler(MoveInfo move, ControlsScript player);
    public static event MoveHandler OnMove;

    public delegate void ButtonHandler(ButtonPress button, ControlsScript player);
    public static event ButtonHandler OnButton;

    public delegate void GhostInputHandler(ButtonPress button);
    public static event GhostInputHandler OnGhostInput;

    public delegate void BasicMoveHandler(BasicMoveReference basicMove, ControlsScript player);
    public static event BasicMoveHandler OnBasicMove;

    public delegate void BodyVisibilityHandler(MoveInfo move, ControlsScript player, BodyPartVisibilityChange bodyPartVisibilityChange, HitBox hitBox);
    public static event BodyVisibilityHandler OnBodyVisibilityChange;

    public delegate void ParticleEffectsHandler(MoveInfo move, ControlsScript player, MoveParticleEffect particleEffects);
    public static event ParticleEffectsHandler OnParticleEffects;

    public delegate void SideSwitchHandler(int side, ControlsScript player);
    public static event SideSwitchHandler OnSideSwitch;

    public delegate void GameBeginHandler(ControlsScript player1, ControlsScript player2, StageOptions stage);
    public static event GameBeginHandler OnGameBegin;

    public delegate void GameEndsHandler(ControlsScript winner, ControlsScript loser);
    public static event GameEndsHandler OnGameEnds;
    public static event GameEndsHandler OnRoundEnds;

    public delegate void GamePausedHandler(bool isPaused);
    public static event GamePausedHandler OnGamePaused;

    public delegate void ScreenChangedHandler(UFEScreen previousScreen, UFEScreen newScreen);
    public static event ScreenChangedHandler OnScreenChanged;

    public delegate void StoryModeHandler(UFE3D.CharacterInfo character);
    public static event StoryModeHandler OnStoryModeStarted;
    public static event StoryModeHandler OnStoryModeCompleted;

    public delegate void TimerHandler(Fix64 time);
    public static event TimerHandler OnTimer;

    public delegate void TimeOverHandler();
    public static event TimeOverHandler OnTimeOver;

    public delegate void InputHandler(InputReferences[] inputReferences, int player);
    public static event InputHandler OnInput;
    #endregion


    #region Network definitions
    public static MultiplayerAPI MultiplayerAPI
    {
        get
        {
            if (UFE.MultiplayerMode == MultiplayerMode.Bluetooth)
            {
                return UFE.bluetoothMultiplayerAPI;
            }
            else if (UFE.MultiplayerMode == MultiplayerMode.Lan)
            {
                return UFE.lanMultiplayerAPI;
            }
            else
            {
                return UFE.onlineMultiplayerAPI;
            }
        }
    }

    public static MultiplayerMode MultiplayerMode
    {
        get
        {
            return UFE._multiplayerMode;
        }
        set
        {
            UFE._multiplayerMode = value;

            if (value == MultiplayerMode.Bluetooth)
            {
                UFE.bluetoothMultiplayerAPI.enabled = true;
                UFE.lanMultiplayerAPI.enabled = false;
                UFE.onlineMultiplayerAPI.enabled = false;
            }
            else if (value == MultiplayerMode.Lan)
            {
                UFE.bluetoothMultiplayerAPI.enabled = false;
                UFE.lanMultiplayerAPI.enabled = true;
                UFE.onlineMultiplayerAPI.enabled = false;
            }
            else
            {
                UFE.bluetoothMultiplayerAPI.enabled = false;
                UFE.lanMultiplayerAPI.enabled = false;
                UFE.onlineMultiplayerAPI.enabled = true;
            }
        }
    }

    public static bool IsConnected
    {
        get
        {
            return UFE.MultiplayerAPI != null && UFE.MultiplayerAPI.IsConnectedToGame() && UFE.MultiplayerAPI.Connections > 0;
        }
    }

    private static MultiplayerAPI bluetoothMultiplayerAPI;
    private static MultiplayerAPI lanMultiplayerAPI;
    private static MultiplayerAPI onlineMultiplayerAPI;
    private static MultiplayerMode _multiplayerMode = MultiplayerMode.Lan;

    public static ConnectionHandler ConnectionHandler;
    #endregion


    #region Global manager definitions
    public static FluxCapacitor FluxCapacitor;
    public static GameManager GameManager;
    public static MatchManager MatchManager;
    public static UIManager UIManager;
    public static GlobalInfo config;
    public static UFE UFEInstance;
    public static CameraScript CameraScript { get; set; }
    public static ReplayMode ReplayMode;
    #endregion


    #region Addons definitions
    public static bool IsAiAddonInstalled { get; set; }
    public static bool IsCInputInstalled { get; set; }
    public static bool IsControlFreakInstalled { get; set; }
    public static bool IsControlFreak1Installed { get; set; }
    public static bool IsControlFreak2Installed { get; set; }
    public static bool IsRewiredInstalled { get; set; }
    public static bool IsNetworkAddonInstalled { get; set; }
    public static bool IsPhotonInstalled { get; set; }
    public static bool IsBluetoothAddonInstalled { get; set; }

    public static InputTouchControllerBridge touchControllerBridge;
    public static GameObject controlFreakPrefab;
    #endregion


    #region Global game objects definitions
    public static GameObject GameEngine { get; protected set; }
    public static GameObject SpawnPool { get; protected set; }
    public static GameObject NetworkManager { get; protected set; }
    #endregion


    #region Trackable definitions
    public static bool freeCamera;
    public static bool freezePhysics;
    public static bool newRoundCasted;
    public static bool normalizedCam = true;
    public static bool pauseTimer;
    public static Fix64 timer;
    public static Fix64 timeScale;
    public static ControlsScript p1ControlsScript;
    public static ControlsScript p2ControlsScript;
    public static List<ControlsScript> p1TeamControlsScripts = new List<ControlsScript>();
    public static List<ControlsScript> p2TeamControlsScripts = new List<ControlsScript>();
    public static List<DelayedAction> delayedLocalActions = new List<DelayedAction>();
    public static List<DelayedAction> delayedSynchronizedActions = new List<DelayedAction>();
    public static List<InstantiatedGameObject> instantiatedObjects = new List<InstantiatedGameObject>();
    public static ChallengeMode challengeMode;
    #endregion


    #region Global variable definitions
    /// <summary>UFE's own fixed delta time.</summary>
    public static Fix64 fixedDeltaTime { get { return _fixedDeltaTime * timeScale; } set { _fixedDeltaTime = value; } }
    /// <summary>Total time in frames.</summary>
    public static int intTimer;
    /// <summary>Frames per second.</summary>
    public static int fps { get { return config != null ? config.fps : 60; } set { config.fps = value; } }
    /// <summary>Current frame.</summary>
    public static long currentFrame { get; set; }
    /// <summary>Is a match currently in progress?</summary>
    public static bool gameRunning { get; protected set; }
    /// <summary>Current challenge.</summary>
    public static int currentChallenge { get; set; }
    /// <summary>The current game mode</summary>
    public static GameMode gameMode = GameMode.None;

    public static UFEController localPlayerController;
    public static UFEController remotePlayerController;
    #endregion


    #region Private definitions
    private static Fix64 _fixedDeltaTime;
    private static AudioSource musicAudioSource;
    private static AudioSource soundsAudioSource;
    private static Scene mainScene;

    private static UFEController p1Controller;
    private static UFEController p2Controller;

    private static RandomAI p1RandomAI;
    private static RandomAI p2RandomAI;
    private static AbstractInputController p1FuzzyAI;
    private static AbstractInputController p2FuzzyAI;
    private static SimpleAI p1SimpleAI;
    private static SimpleAI p2SimpleAI;

    private static List<object> memoryDump = new List<object>();
    #endregion


    #region Delayed local and synchronized action definitions
    public static void DelayLocalAction(Action action, Fix64 seconds)
    {
        if (UFE.fixedDeltaTime > 0)
        {
            UFE.DelayLocalAction(action, (int)FPMath.Floor(seconds * config.fps / UFE.timeScale));
        }
        else
        {
            UFE.DelayLocalAction(action, 1);
        }
    }

    public static void DelayLocalAction(Action action, int steps)
    {
        UFE.DelayLocalAction(new DelayedAction(action, steps));
    }

    public static void DelayLocalAction(DelayedAction delayedAction)
    {
        UFE.delayedLocalActions.Add(delayedAction);
    }

    public static void DelaySynchronizedAction(Action action, Fix64 seconds)
    {
        if (UFE.fixedDeltaTime > 0)
        {
            UFE.DelaySynchronizedAction(action, (int)FPMath.Floor(seconds * config.fps / UFE.timeScale));
        }
        else
        {
            UFE.DelaySynchronizedAction(action, 1);
        }
    }

    public static void ClearAllActions()
    {
        UFE.delayedLocalActions.Clear();
        UFE.delayedSynchronizedActions.Clear();
    }

    public static void DelaySynchronizedAction(Action action, int steps)
    {
        UFE.DelaySynchronizedAction(new DelayedAction(action, steps));
    }

    public static void DelaySynchronizedAction(DelayedAction delayedAction)
    {
        UFE.delayedSynchronizedActions.Add(delayedAction);
    }


    public static bool FindDelaySynchronizedAction(Action action)
    {
        foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions)
        {
            if (action == delayedAction.action) return true;
        }
        return false;
    }

    public static bool FindAndUpdateDelaySynchronizedAction(Action action, Fix64 seconds)
    {
        foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions)
        {
            if (action == delayedAction.action)
            {
                delayedAction.steps = (int)FPMath.Floor(seconds * config.fps);
                return true;
            }
        }
        return false;
    }

    public static void FindAndRemoveDelaySynchronizedAction(Action action)
    {
        foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions)
        {
            if (action == delayedAction.action)
            {
                UFE.delayedSynchronizedActions.Remove(delayedAction);
                return;
            }
        }
    }

    public static void FindAndRemoveDelayLocalAction(Action action)
    {
        foreach (DelayedAction delayedAction in UFE.delayedLocalActions)
        {
            if (action == delayedAction.action)
            {
                UFE.delayedLocalActions.Remove(delayedAction);
                return;
            }
        }
    }

    public static void ExecuteLocalDelayedActions()
    {
        // Check if we need to execute any delayed "local action" (such as playing a sound or GUI)
        if (UFE.delayedLocalActions.Count == 0) return;

        for (int i = UFE.delayedLocalActions.Count - 1; i >= 0; --i)
        {
            if (UFE.delayedLocalActions.Count == 0) continue;

            DelayedAction action = UFE.delayedLocalActions[i];
            --action.steps;

            if (action.steps <= 0)
            {
                action.action();
                if (i < UFE.delayedLocalActions.Count) UFE.delayedLocalActions.RemoveAt(i);
            }
        }
    }

    public static void ExecuteSynchronizedDelayedActions()
    {
        // Check if we need to execute any delayed "synchronized action" (game actions)
        if (UFE.delayedSynchronizedActions.Count == 0) return;

        for (int i = UFE.delayedSynchronizedActions.Count - 1; i >= 0; --i)
        {
            if (UFE.delayedSynchronizedActions.Count == 0) continue;

            DelayedAction action = UFE.delayedSynchronizedActions[i];
            --action.steps;

            if (action.steps <= 0)
            {
                action.action();
                if (i < UFE.delayedSynchronizedActions.Count) UFE.delayedSynchronizedActions.RemoveAt(i);
            }
        }
    }
    #endregion


    #region AI related methods
    public static void SetAIEngine(AIEngine engine)
    {
        UFE.config.aiOptions.engine = engine;
    }

    public static AIEngine GetAIEngine()
    {
        return UFE.config.aiOptions.engine;
    }

    public static void SetAIDifficulty(AIDifficultyLevel difficulty)
    {
        foreach (AIDifficultySettings difficultySettings in UFE.config.aiOptions.difficultySettings)
        {
            if (difficultySettings.difficultyLevel == difficulty)
            {
                UFE.SetAIDifficulty(difficultySettings);
                break;
            }
        }
    }

    public static void SetAIDifficulty(AIDifficultySettings difficulty)
    {
        UFE.config.aiOptions.selectedDifficulty = difficulty;
        UFE.config.aiOptions.selectedDifficultyLevel = difficulty.difficultyLevel;

        for (int i = 0; i < UFE.config.aiOptions.difficultySettings.Length; ++i)
        {
            if (difficulty == UFE.config.aiOptions.difficultySettings[i])
            {
                PlayerPrefs.SetInt(UFE.DifficultyLevelKey, i);
                PlayerPrefs.Save();
                break;
            }
        }
    }

    public static AIDifficultySettings GetAIDifficulty()
    {
        return UFE.config.aiOptions.selectedDifficulty;
    }

    public static void SetSimpleAI(int player, SimpleAIBehaviour behaviour)
    {
        if (player == 1)
        {
            UFE.p1SimpleAI.behaviour = behaviour;
            UFE.p1Controller.cpuController = UFE.p1SimpleAI;
        }
        else if (player == 2)
        {
            UFE.p2SimpleAI.behaviour = behaviour;
            UFE.p2Controller.cpuController = UFE.p2SimpleAI;
        }
    }

    public static void SetRandomAI(int player)
    {
        if (player == 1)
        {
            UFE.p1Controller.cpuController = UFE.p1RandomAI;
        }
        else if (player == 2)
        {
            UFE.p2Controller.cpuController = UFE.p2RandomAI;
        }
    }

    public static void SetFuzzyAI(int player, UFE3D.CharacterInfo character)
    {
        UFE.SetFuzzyAI(player, character, UFE.config.aiOptions.selectedDifficulty);
    }

    public static void SetFuzzyAI(int player, UFE3D.CharacterInfo character, AIDifficultySettings difficulty)
    {
        if (UFE.IsAiAddonInstalled)
        {
            if (player == 1)
            {
                UFE.p1Controller.cpuController = UFE.p1FuzzyAI;
            }
            else if (player == 2)
            {
                UFE.p2Controller.cpuController = UFE.p2FuzzyAI;
            }

            UFEController controller = UFE.GetController(player);
            if (controller != null && controller.isCPU)
            {
                AbstractInputController cpu = controller.cpuController;

                if (cpu != null)
                {
                    MethodInfo method = cpu.GetType().GetMethod(
                        "SetAIInformation",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
                        null,
                        new Type[] { typeof(ScriptableObject) },
                        null
                    );

                    if (method != null)
                    {
                        if (character != null && character.aiInstructionsSet != null && character.aiInstructionsSet.Length > 0)
                        {
                            if (difficulty.startupBehavior == AIBehavior.Any)
                            {
                                method.Invoke(cpu, new object[] { character.aiInstructionsSet[0].aiInfo });
                            }
                            else
                            {
                                ScriptableObject selectedAIInfo = character.aiInstructionsSet[0].aiInfo;
                                foreach (AIInstructionsSet instructionSet in character.aiInstructionsSet)
                                {
                                    if (instructionSet.behavior == difficulty.startupBehavior)
                                    {
                                        selectedAIInfo = instructionSet.aiInfo;
                                        break;
                                    }
                                }
                                method.Invoke(cpu, new object[] { selectedAIInfo });
                            }
                        }
                        else
                        {
                            method.Invoke(cpu, new object[] { null });
                        }
                    }
                }
            }
        }
    }

    public static bool GetCPU(int player)
    {
        UFEController controller = UFE.GetController(player);
        if (controller != null)
        {
            return controller.isCPU;
        }
        return false;
    }
    public static void SetCPU(int player, bool cpuToggle)
    {
        UFEController controller = UFE.GetController(player);
        if (controller != null)
        {
            controller.isCPU = cpuToggle;
        }
    }
    #endregion


    #region Input related methods
    public static string GetInputReference(ButtonPress button, InputReferences[] inputReferences)
    {
        foreach (InputReferences inputReference in inputReferences)
        {
            if (inputReference.engineRelatedButton == button) return inputReference.inputButtonName;
        }
        return null;
    }

    public static string GetInputReference(InputType inputType, InputReferences[] inputReferences)
    {
        foreach (InputReferences inputReference in inputReferences)
        {
            if (inputReference.inputType == inputType) return inputReference.inputButtonName;
        }
        return null;
    }

    public static UFEController GetPlayer1Controller()
    {
        if (UFE.IsConnected)
        {
            if (UFE.MultiplayerAPI.IsServer())
            {
                return UFE.localPlayerController;
            }
            else
            {
                return UFE.remotePlayerController;
            }
        }
        return UFE.p1Controller;
    }

    public static UFEController GetPlayer2Controller()
    {
        if (UFE.IsConnected)
        {
            if (UFE.MultiplayerAPI.IsServer())
            {
                return UFE.remotePlayerController;
            }
            else
            {
                return UFE.localPlayerController;
            }
        }
        return UFE.p2Controller;
    }

    public static UFEController GetController(int player)
    {
        if (player == 1) return UFE.GetPlayer1Controller();
        else if (player == 2) return UFE.GetPlayer2Controller();
        else return null;
    }

    public static int GetLocalPlayer()
    {
        if (UFE.localPlayerController == UFE.GetPlayer1Controller()) return 1;
        else if (UFE.localPlayerController == UFE.GetPlayer2Controller()) return 2;
        else return -1;
    }

    public static int GetRemotePlayer()
    {
        if (UFE.remotePlayerController == UFE.GetPlayer1Controller()) return 1;
        else if (UFE.remotePlayerController == UFE.GetPlayer2Controller()) return 2;
        else return -1;
    }

    public static void SetNetworkInputDelay(int frameDelay)
    {
        if (UFE.config != null)
            UFE.config.networkInputDelay = frameDelay;
    }

    public static void SetNetworkRollback(bool active)
    {
        if (UFE.config != null)
            UFE.config.rollbackEnabled = active;
    }
    #endregion


    #region Stage selection related methods
    public static void SetStage(StageOptions stage)
    {
        config.selectedStage = stage;
    }

    public static void SetStage(string stageName)
    {
        foreach (StageOptions stage in config.stages)
        {
            if (stageName == stage.stageName)
            {
                UFE.SetStage(stage);
                return;
            }
        }
    }

    public static StageOptions GetStage()
    {
        return config.selectedStage;
    }
    #endregion


    #region Characters and players during the battle related methods
    public static ControlsScript GetControlsScript(int player)
    {
        if (player == 1)
        {
            return UFE.GetPlayer1ControlsScript();
        }
        else if (player == 2)
        {
            return UFE.GetPlayer2ControlsScript();
        }
        return null;
    }

    public static List<ControlsScript> GetControlsScriptTeam(int player)
    {
        if (player == 1)
        {
            return UFE.p1TeamControlsScripts;
        }
        else if (player == 2)
        {
            return UFE.p2TeamControlsScripts;
        }
        return null;
    }

    public static ControlsScript GetControlsScriptTeamMember(int player, int position)
    {
        if (player == 1)
        {
            return UFE.p1TeamControlsScripts[position];
        }
        else if (player == 2)
        {
            return UFE.p2TeamControlsScripts[position];
        }
        return null;
    }

    public static void SetMainControlsScript(int player, int position)
    {
        if (player == 1)
        {
            p1ControlsScript = UFE.p1TeamControlsScripts[position];
            UFE.CameraScript.player1 = p1ControlsScript;
        }
        else
        {
            p2ControlsScript = UFE.p2TeamControlsScripts[position];
            UFE.CameraScript.player2 = p2ControlsScript;
        }
    }

    public static List<ControlsScript> GetAllControlsScripts()
    {
        List<ControlsScript> allScripts = new List<ControlsScript>();
        allScripts.AddRange(UFE.p1TeamControlsScripts);
        foreach (ControlsScript cScript in UFE.p1TeamControlsScripts)
        {
            allScripts.AddRange(cScript.assists);
        }
        allScripts.AddRange(UFE.p2TeamControlsScripts);
        foreach (ControlsScript cScript in UFE.p2TeamControlsScripts)
        {
            allScripts.AddRange(cScript.assists);
        }
        return allScripts;
    }

    public static List<ControlsScript> GetAllControlsScriptsByPlayer(int player)
    {
        List<ControlsScript> allScripts = new List<ControlsScript>();
        List<ControlsScript> targetList = GetControlsScriptTeam(player);
        allScripts.AddRange(targetList);
        foreach (ControlsScript cScript in targetList)
        {
            allScripts.AddRange(cScript.assists);
        }

        return allScripts;
    }

    public static ControlsScript GetPlayer1ControlsScript()
    {
        return p1ControlsScript;
    }

    public static ControlsScript GetPlayer2ControlsScript()
    {
        return p2ControlsScript;
    }
    #endregion


    #region Event raising methods
    public static void FireLifePoints(Fix64 newValue, ControlsScript player)
    {
        if (UFE.OnLifePointsChange != null) UFE.OnLifePointsChange((float)newValue, player);
    }

    public static void FireGaugeChange(int targetGauge, Fix64 newValue, ControlsScript player)
    {
        OnGaugeUpdate?.Invoke(targetGauge, (float)newValue, player);
    }

    public static void FireAlert(string alertMessage, ControlsScript player)
    {
        if (UFE.OnNewAlert != null) UFE.OnNewAlert(alertMessage, player);
    }

    public static void FireHit(HitBox strokeHitBox, MoveInfo move, Hit hitInfo, ControlsScript player)
    {
        if (UFE.OnHit != null) UFE.OnHit(strokeHitBox, move, hitInfo, player);
    }

    public static void FireBlock(HitBox strokeHitBox, MoveInfo move, Hit hitInfo, ControlsScript player)
    {
        if (UFE.OnBlock != null) UFE.OnBlock(strokeHitBox, move, hitInfo, player);
    }

    public static void FireParry(HitBox strokeHitBox, MoveInfo move, Hit hitInfo, ControlsScript player)
    {
        if (UFE.OnParry != null) UFE.OnParry(strokeHitBox, move, hitInfo, player);
    }

    public static void FireMove(MoveInfo move, ControlsScript player)
    {
        if (UFE.OnMove != null) UFE.OnMove(move, player);
    }

    public static void FireButton(ButtonPress button, ControlsScript player)
    {
        if (UFE.OnButton != null) UFE.OnButton(button, player);
    }

    public static void FireGhostInput(ButtonPress button)
    {
        OnGhostInput?.Invoke(button);
    }

    public static void FireBasicMove(BasicMoveReference basicMove, ControlsScript player)
    {
        if (UFE.OnBasicMove != null) UFE.OnBasicMove(basicMove, player);
    }

    public static void FireBodyVisibilityChange(MoveInfo move, ControlsScript player, BodyPartVisibilityChange bodyPartVisibilityChange, HitBox hitBox)
    {
        if (UFE.OnBodyVisibilityChange != null) UFE.OnBodyVisibilityChange(move, player, bodyPartVisibilityChange, hitBox);
    }

    public static void FireParticleEffects(MoveInfo move, ControlsScript player, MoveParticleEffect particleEffects)
    {
        if (UFE.OnParticleEffects != null) UFE.OnParticleEffects(move, player, particleEffects);
    }

    public static void FireSideSwitch(int side, ControlsScript player)
    {
        if (UFE.OnSideSwitch != null) UFE.OnSideSwitch(side, player);
    }

    public static void FireGameBegins()
    {
        if (UFE.OnGameBegin != null)
        {
            gameRunning = true;
            UFE.OnGameBegin(GetControlsScript(1), GetControlsScript(2), config.selectedStage);
        }
    }

    public static void FireGameEnds(ControlsScript winner = null, ControlsScript loser = null)
    {
        UFE.player1WonLastBattle = winner != null && winner == UFE.GetControlsScript(1);
        if (winner != null && loser != null && UFE.OnGameEnds != null)
        {
            UFE.OnGameEnds(winner, loser);
        }
    }

    public static void FireRoundBegins(int currentRound)
    {
        if (UFE.OnRoundBegins != null) UFE.OnRoundBegins(currentRound);
    }

    public static void FireRoundEnds(ControlsScript winner, ControlsScript loser)
    {
        if (UFE.OnRoundEnds != null) UFE.OnRoundEnds(winner, loser);
    }

    public static void FireTimer(float timer)
    {
        if (UFE.OnTimer != null) UFE.OnTimer(timer);
    }

    public static void FireTimeOver()
    {
        if (UFE.OnTimeOver != null) UFE.OnTimeOver();
    }
    #endregion


    #region Match controller methods
    public static void PauseGame(bool pause)
    {
        if (pause && UFE.timeScale == 0) return;

        if (pause)
        {
            UFE.timeScale = 0;
        }
        else
        {
            UFE.timeScale = UFE.config._gameSpeed;
        }

        if (UFE.OnGamePaused != null)
        {
            UFE.OnGamePaused(pause);
        }
    }

    public static bool IsInstalled(string theClass)
    {
        return UFE.SearchClass(theClass) != null;
    }

    public static bool IsPaused()
    {
        return UFE.timeScale <= 0;
    }

    public static Fix64 GetTimer()
    {
        return timer;
    }

    public static void ResetTimer()
    {
        timer = config.roundOptions._timer;
        intTimer = (int)FPMath.Round(config.roundOptions._timer);
        if (UFE.OnTimer != null) OnTimer((float)timer);
    }

    public static Type SearchClass(string theClass)
    {
        Type type = null;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(theClass);
            if (type != null) { break; }
        }

        return type;
    }

    public static void SetTimer(Fix64 time)
    {
        timer = time;
        intTimer = (int)FPMath.Round(time);
        if (UFE.OnTimer != null) OnTimer(timer);
    }

    public static void PlayTimer()
    {
        pauseTimer = false;
    }

    public static void PauseTimer()
    {
        pauseTimer = true;
    }

    public static bool IsTimerPaused()
    {
        return pauseTimer;
    }

    public static void RestartMatch()
    {
        UFE.EndGame();
        UFE.StartLoadingBattleScreen();
        UFE.PauseGame(false);
    }

    public static void EndGame(bool killEngine = true)
    {
        UFE.timeScale = UFE.config._gameSpeed;
        UFE.gameRunning = false;
        UFE.newRoundCasted = false;
        ClearAllActions();

        if (killEngine)
        {
            if (battleGUI != null)
            {
                battleGUI.OnHide();
                GameObject.Destroy(UFE.battleGUI.gameObject);
                battleGUI = null;
            }

            if (GameEngine != null)
            {
                UFE.instantiatedObjects.Clear();

                if (UFE.config.selectedStage.stageLoadingMethod == StorageMode.SceneFile)
                {
                    SceneManager.UnloadSceneAsync(UFE.config.selectedStage.stagePath);
                    SceneManager.SetActiveScene(mainScene);
                }

                GameObject.Destroy(GameEngine);
                GameEngine = null;
                MatchManager = null;
                ReplayMode = null;
                challengeMode = null;

                Resources.UnloadUnusedAssets();
            }
        }
    }

    public static void CastNewRound()
    {
        UFE.FireRoundBegins(config.currentRound);
        UFE.DelaySynchronizedAction(StartFight, UFE.config.roundOptions._matchStartDelay);
    }

    public static void StartFight()
    {
        if (UFE.gameMode != GameMode.ChallengeMode)
            UFE.FireAlert(UFE.config.selectedLanguage.fight, null);
        UFE.config.lockInputs = false;
        UFE.config.lockMovements = false;
        UFE.PlayTimer();
    }

    public static void CastInput(InputReferences[] inputReferences, int player)
    {
        if (UFE.OnInput != null) OnInput(inputReferences, player);
    }

    public static GlobalInfo GetActiveConfig()
    {
        // Check for config
        if (UFE.config == null)
        {
            GameObject manager = GameObject.Find("UFE Manager");
            if (manager != null)
            {
                UFE ufe = manager.GetComponent<UFE>();
                if (ufe != null && ufe.UFE_Config != null)
                {
                    UFE.config = ufe.UFE_Config;
                }
                else
                {
                    UFE.config = ScriptableObject.CreateInstance<GlobalInfo>();
                }
            }
            else
            {
                UFE.config = ScriptableObject.CreateInstance<GlobalInfo>();
            }
        }
        return UFE.config;
    }

    private static void _StartGame(float fadeTime)
    {
        UFE.HideScreen(UFE.currentScreen);
        // Initialize Battle GUI
        if (UFE.config.gameGUI.battleGUI == null)
        {
            Debug.LogError("Battle GUI not found! Make sure you have set the prefab correctly in the Global Editor");
            UFE.battleGUI = new GameObject("BattleGUI").AddComponent<UFEScreen>();
        }
        else
        {
            UFE.battleGUI = Instantiate(UFE.config.gameGUI.battleGUI);
        }
        if (!UFE.battleGUI.hasFadeIn) fadeTime = 0;
        CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);

        UFE.battleGUI.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);
        UFE.battleGUI.OnShow();
        UFE.canvasGroup.alpha = 0;


        // Initialize Game Engine
        UFE.GameEngine = new GameObject("Game");
        UFE.CameraScript = UFE.GameEngine.AddComponent<CameraScript>();

        UFE.SpawnPool = new GameObject("SpawnPool");
        SpawnPool.transform.parent = GameEngine.transform;

        if (UFE.config.player1Character == null)
        {
            Debug.LogError("No character selected for player 1.");
            return;
        }
        if (UFE.config.player2Character == null)
        {
            Debug.LogError("No character selected for player 2.");
            return;
        }
        if (UFE.config.selectedStage == null)
        {
            Debug.LogError("No stage selected.");
            return;
        }

        UFE.MatchManager = UFE.GameEngine.AddComponent<MatchManager>();

        if (UFE.config.aiOptions.engine == AIEngine.FuzzyAI)
        {
            UFE.SetFuzzyAI(1, UFE.config.player1Character);
            UFE.SetFuzzyAI(2, UFE.config.player2Character);
        }
        else
        {
            UFE.SetRandomAI(1);
            UFE.SetRandomAI(2);
        }

        // Load Stage
        GameObject stageInstance = null;
        if (config.selectedStage.stageLoadingMethod == StorageMode.Prefab)
        {
            if (UFE.config.selectedStage.prefab != null)
            {
                stageInstance = Instantiate(config.selectedStage.prefab);
                stageInstance.transform.parent = GameEngine.transform;
            }
            else
            {
                Debug.LogError("Stage prefab not found! Make sure you have set the prefab correctly in the Global Editor.");
            }
        }
        else if (config.selectedStage.stageLoadingMethod == StorageMode.ResourcesFolder)
        {
            GameObject prefab = Resources.Load<GameObject>(config.selectedStage.stagePath);

            if (prefab != null)
            {
                stageInstance = Instantiate(prefab);
                stageInstance.transform.parent = GameEngine.transform;
            }
            else
            {
                Debug.LogError("Stage prefab not found! Make sure the prefab is correctly located under the Resources folder and the path is written correctly.");
            }
        }
        else
        {
            SceneManager.LoadScene(UFE.config.selectedStage.stagePath, LoadSceneMode.Additive);
            UFE.DelayLocalAction(SetActiveStageScene, 3);
        }


        UFE.config.currentRound = 1;
        UFE.config.lockInputs = true;
        UFE.SetTimer(config.roundOptions._timer);
        UFE.PauseTimer();

        ControlsScript cScript1 = null;
        ControlsScript cScript2 = null;

        // Initialize Teams
        p1TeamControlsScripts = new List<ControlsScript>();
        p2TeamControlsScripts = new List<ControlsScript>();
        if (UFE.config.selectedMatchType != MatchType.Singles)
        {
            //int maxSizePlayer1 = UFE.config.teamModes[UFE.config.selectedTeamMode].teams[0].characters.Length;
            //int maxSizePlayer2 = UFE.config.teamModes[UFE.config.selectedTeamMode].teams[1].characters.Length;

            int counter = 0;
            foreach (UFE3D.CharacterInfo character in UFE.config.player1Team)
            {
                FPVector spawnPos = UFE.config.teamModes[UFE.config.selectedTeamMode].teams[0].characters[counter].spawnPosition;
                if (counter == 0)
                {
                    cScript1 = SpawnCharacter(character, 1, -1, spawnPos, false);
                    p1TeamControlsScripts.Add(cScript1);
                    UFE.p1ControlsScript = cScript1;
                    UFE.config.player1Character = cScript1.myInfo;
                    UFE.CameraScript.player1 = cScript1;
                }
                else
                {
                    p1TeamControlsScripts.Add(SpawnCharacter(character, 1, -1, spawnPos, false));
                }
                counter++;
            }

            counter = 0;
            foreach (UFE3D.CharacterInfo character in UFE.config.player2Team)
            {
                FPVector spawnPos = UFE.config.teamModes[UFE.config.selectedTeamMode].teams[1].characters[counter].spawnPosition;
                if (counter == 0)
                {
                    cScript2 = SpawnCharacter(character, 2, -1, spawnPos, false);
                    p2TeamControlsScripts.Add(cScript2);
                    UFE.p2ControlsScript = cScript2;
                    UFE.config.player2Character = cScript2.myInfo;
                    UFE.CameraScript.player2 = cScript2;
                }
                else
                {
                    p2TeamControlsScripts.Add(SpawnCharacter(character, 2, -1, spawnPos, false));
                }
                counter++;
            }
        }
        else
        {
            // Initialize Player 1 Character
            FPVector p1Pos = UFE.config.selectedStage.position;
            p1Pos += UFE.config.roundOptions._p1XPosition;
            cScript1 = SpawnCharacter(UFE.config.player1Character, 1, -1, p1Pos, false);
            p1TeamControlsScripts.Add(cScript1);
            cScript1.debugInfo = UFE.config.debugOptions.p1DebugInfo;
            UFE.p1ControlsScript = cScript1;
            UFE.config.player1Character = cScript1.myInfo;
            UFE.CameraScript.player1 = cScript1;
            if (UFE.IsControlFreak2Installed && UFE.p1ControlsScript.myInfo.customControls.overrideControlFreak && UFE.p1ControlsScript.myInfo.customControls.controlFreak2Prefab != null)
            {
                UFE.controlFreakPrefab = Instantiate(UFE.p1ControlsScript.myInfo.customControls.controlFreak2Prefab.gameObject);
                UFE.touchControllerBridge = (UFE.controlFreakPrefab != null) ? UFE.controlFreakPrefab.GetComponent<InputTouchControllerBridge>() : null;
                UFE.touchControllerBridge.Init();
            }


            // Initialize Player 2 Character
            int altCostume = -1;
            FPVector p2Pos = UFE.config.selectedStage.position;
            p2Pos += UFE.config.roundOptions._p2XPosition;
            if (UFE.config.player1Character.characterName == UFE.config.player2Character.characterName && UFE.config.player2Character.alternativeCostumes.Length > 0) altCostume = 0;
            cScript2 = SpawnCharacter(UFE.config.player2Character, 2, 1, p2Pos, false, null, null, altCostume);
            p2TeamControlsScripts.Add(cScript2);
            cScript2.debugInfo = UFE.config.debugOptions.p2DebugInfo;
            UFE.p2ControlsScript = cScript2;
            UFE.config.player2Character = cScript2.myInfo;
            UFE.CameraScript.player2 = cScript2;
        }

        if (cScript1 != null && cScript2 != null)
        {
            // Extra Options
            if (UFE.config.roundOptions.allowMovementStart)
            {
                UFE.config.lockMovements = false;
            }
            else
            {
                UFE.config.lockMovements = true;
            }


            // Initialize Debuggers
            UFE.debugger1 = UFE.DebuggerText(UFE.config.debugOptions.p1DebugInfo.debuggerGameObject, "Debugger1", "", UFE.config.debugOptions.p1DebugInfo.textPosition, UFE.config.debugOptions.p1DebugInfo.textAlignment);
            if (UFE.debugger1 != null)
            {
                UFE.p1ControlsScript.debugger = UFE.debugger1;
                UFE.debugger1.enabled = config.debugOptions.debugMode;
            }

            UFE.debugger2 = UFE.DebuggerText(UFE.config.debugOptions.p2DebugInfo.debuggerGameObject, "Debugger2", "", UFE.config.debugOptions.p2DebugInfo.textPosition, UFE.config.debugOptions.p2DebugInfo.textAlignment);
            if (UFE.debugger2 != null)
            {
                UFE.p2ControlsScript.debugger = UFE.debugger2;
                UFE.debugger2.enabled = config.debugOptions.debugMode;
            }


            for (int i = 1; i <= 2; i++)
            {
                ControlsScript opCScript;
                UFE3D.CharacterInfo opCharInfo;
                if (i == 1)
                {
                    opCScript = cScript2;
                    opCharInfo = UFE.config.player2Character;
                }
                else
                {
                    opCScript = cScript1;
                    opCharInfo = UFE.config.player1Character;
                }

                // Set References
                foreach (ControlsScript cScript in UFE.GetAllControlsScriptsByPlayer(i))
                {
                    cScript.opControlsScript = opCScript;
                    cScript.opInfo = opCharInfo;

#if !UFE_LITE && !UFE_BASIC
                    FindAndSpawnAssist(cScript, i);
#endif
                    // Initialize Characters
                    cScript.Init();
                    foreach (ControlsScript cAssist in cScript.assists) cAssist.Init();

                    // Set Sprite Renderer for 2D characters
                    if (cScript.myInfo.animationType == AnimationType.Mecanim2D)
                    {
                        cScript.mySpriteRenderer = cScript.GetComponentInChildren<SpriteRenderer>();
                        if (UFE.config.sortCharacterOnHit && cScript.mySpriteRenderer != null)
                        {
                            cScript.mySpriteRenderer.sortingOrder = UFE.config.foregroundSortLayer;
                        }
                    }
                }
            }
        }

        // Challenge Mode
        if (UFE.gameMode == GameMode.ChallengeMode && challengeMode == null)
        {
            challengeMode = Instantiate(UFE.config.gameGUI.challengeModeOverlay);
            challengeMode.transform.parent = GameEngine.transform;
        }

        // Start Game
        UFE.FluxCapacitor.savedState = null;
        UFE.PauseGame(false);

        if (UFE.config.gameGUI.replayTools != null)
        {
            ReplayMode = Instantiate(UFE.config.gameGUI.replayTools);
            ReplayMode.transform.parent = GameEngine.transform;
            ReplayMode.name = "ReplayTools";
            if (!UFE.config.debugOptions.displayReplayInTraining || (UFE.config.debugOptions.displayReplayInTraining && UFE.gameMode == GameMode.TrainingRoom))
            {
                UFE.ReplayMode.enableStateTrackerControls = UFE.config.debugOptions.stateTrackerTest;
                UFE.ReplayMode.enableRecordingControls = UFE.config.debugOptions.recordMatchTools;
            }
            else
            {
                UFE.ReplayMode.enableStateTrackerControls = false;
                UFE.ReplayMode.enableRecordingControls = false;
            }
        }

        // Instantiate Replay Tools for network synch tests
        if (UFE.IsConnected
            && UFE.config.networkOptions.synchronizationAction == NetworkSynchronizationAction.PlaybackTool
            && UFE.ReplayMode != null)
        {
            UFE.ReplayMode.enableControls = false;
            UFE.ReplayMode.enableRecordingControls = true;
            UFE.ReplayMode.SetMaxBuffer(UFE.config.networkOptions.recordingBuffer);
            UFE.ReplayMode.StopRecording();
            UFE.ReplayMode.StartRecording();
        }

        UFE.eventSystem.enabled = true;


        Fix64 newRoundDelay = 0;
        SerializedAnimationData animData = cScript1.MoveSet.basicMoves.intro.animData[0];
        if (cScript1.MoveSet.basicMoves.intro.useMoveFile && cScript1.MoveSet.basicMoves.intro.moveInfo != null)
            animData = cScript1.MoveSet.basicMoves.intro.moveInfo.animData;

        if (animData.clip != null)
        {
            newRoundDelay = animData.length;
            UFE.DelaySynchronizedAction(cScript1.PlayIntro, 1);
        }

        SerializedAnimationData animData2 = cScript2.MoveSet.basicMoves.intro.animData[0];
        if (cScript2.MoveSet.basicMoves.intro.useMoveFile && cScript2.MoveSet.basicMoves.intro.moveInfo != null)
            animData2 = cScript2.MoveSet.basicMoves.intro.moveInfo.animData;

        if (animData2.clip != null)
        {
            if (UFE.config.roundOptions.playIntrosAtSameTime && animData2.length > newRoundDelay)
            {
                newRoundDelay = animData2.length;
            }
            else if (!UFE.config.roundOptions.playIntrosAtSameTime)
            {
                UFE.DelaySynchronizedAction(cScript2.PlayIntro, newRoundDelay);
                newRoundDelay += animData2.length;
            }
        }
        UFE.DelaySynchronizedAction(UFE.CastNewRound, newRoundDelay);

        UFE.FireGameBegins();
    }

    #endregion


    #region MonoBehaviour methods
    protected void Awake()
    {
        UFE.config = UFE_Config;
        UFE.UFEInstance = this;

        UFE.fps = UFE.config.fps;
        UFE.fixedDeltaTime = 1 / (Fix64)UFE.config.fps;
        mainScene = SceneManager.GetActiveScene();

        FPRandom.Init();

        // Check which characters have been unlocked
        UFE.LoadUnlockedCharacters();

        // Check the installed Addons and supported 3rd party products
        UFE.IsCInputInstalled = UFE.IsInstalled("cInput");
#if UFE_LITE
        UFE.isAiAddonInstalled = false;
#else
        UFE.IsAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");
#endif

#if UFE_LITE || UFE_BASIC
		UFE.isPhotonInstalled = false;
        UFE.isBluetoothAddonInstalled = false;
		UFE.isNetworkAddonInstalled = false;
#else
        UFE.IsPhotonInstalled = UFE.IsInstalled("PhotonMultiplayerAPI") && UFE.config.networkOptions.networkService != NetworkService.Disabled;
        UFE.IsBluetoothAddonInstalled = UFE.IsInstalled("BluetoothMultiplayerAPI") && UFE.config.networkOptions.networkService != NetworkService.Disabled;
        UFE.IsNetworkAddonInstalled = UFE.IsPhotonInstalled;
#endif

        UFE.IsControlFreak1Installed = UFE.IsInstalled("TouchController");
        UFE.IsControlFreak2Installed = UFE.IsInstalled("ControlFreak2.UFEBridge");
        UFE.IsControlFreakInstalled = UFE.IsControlFreak1Installed || UFE.IsControlFreak2Installed;
        UFE.IsRewiredInstalled = UFE.IsInstalled("Rewired.Integration.UniversalFightingEngine.RewiredUFEInputManager");

        // Check if we should run the application in background
        Application.runInBackground = UFE.config.runInBackground;

        // Check if cInput is installed and initialize the cInput GUI
        if (UFE.IsCInputInstalled)
        {
            Type t = UFE.SearchClass("cGUI");
            if (t != null) t.GetField("cSkin").SetValue(null, UFE.config.inputOptions.cInputSkin);
        }

        //-------------------------------------------------------------------------------------------------------------
        // Initialize the GUI
        //-------------------------------------------------------------------------------------------------------------
        GameObject goGroup = new GameObject("CanvasGroup");
        UFE.canvasGroup = goGroup.AddComponent<CanvasGroup>();

        GameObject go = new GameObject("Canvas");
        go.transform.SetParent(goGroup.transform);
        UFE.canvas = go.AddComponent<Canvas>();
        UFE.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UFE.UIManager = go.AddComponent<UIManager>();

        UFE.graphicRaycaster = go.AddComponent<GraphicRaycaster>();

        UFE.standaloneInputModule = go.AddComponent<StandaloneInputModule>();
        UFE.standaloneInputModule.verticalAxis = "Mouse Wheel";
        UFE.standaloneInputModule.horizontalAxis = "Mouse Wheel";
        //UFE.standaloneInputModule.forceModuleActive = true;

        if (UFE.config.gameGUI.useCanvasScaler)
        {
            CanvasScaler cs = go.AddComponent<CanvasScaler>();
            cs.defaultSpriteDPI = UFE.config.gameGUI.canvasScaler.defaultSpriteDPI;
            cs.fallbackScreenDPI = UFE.config.gameGUI.canvasScaler.fallbackScreenDPI;
            cs.matchWidthOrHeight = UFE.config.gameGUI.canvasScaler.matchWidthOrHeight;
            cs.physicalUnit = UFE.config.gameGUI.canvasScaler.physicalUnit;
            cs.referencePixelsPerUnit = UFE.config.gameGUI.canvasScaler.referencePixelsPerUnit;
            cs.referenceResolution = UFE.config.gameGUI.canvasScaler.referenceResolution;
            cs.scaleFactor = UFE.config.gameGUI.canvasScaler.scaleFactor;
            cs.screenMatchMode = UFE.config.gameGUI.canvasScaler.screenMatchMode;
            cs.uiScaleMode = UFE.config.gameGUI.canvasScaler.scaleMode;

            //Line commented because we use "Screen Space - Overlay" canvas and the "dynaicPixelsPerUnit" property is only used in "World Space" Canvas.
            //cs.dynamicPixelsPerUnit = UFE.config.gameGUI.canvasScaler.dynamicPixelsPerUnit; 
        }

        // Check if "Control Freak Virtual Controller" is installed and instantiate the prefab
        if (UFE.IsControlFreakInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.ControlFreak)
        {
            if (UFE.IsControlFreak2Installed && (UFE.config.inputOptions.controlFreak2Prefab != null))
            {
                // Try to instantiate Control Freak 2 rig prefab...
                UFE.controlFreakPrefab = Instantiate(UFE.config.inputOptions.controlFreak2Prefab.gameObject);
                UFE.touchControllerBridge = (UFE.controlFreakPrefab != null) ? UFE.controlFreakPrefab.GetComponent<InputTouchControllerBridge>() : null;
                UFE.touchControllerBridge.Init();

            }
            else if (UFE.IsControlFreak1Installed && (UFE.config.inputOptions.controlFreakPrefab != null))
            {
                // ...or try to instantiate Control Freak 1.x controller prefab...
                UFE.controlFreakPrefab = Instantiate(UFE.config.inputOptions.controlFreakPrefab);
            }
        }

        // Check if the "network addon" is installed
        string uuid = UFE.config.gameName ?? "UFE" /*+ "_" + Application.version*/;
        NetworkManager = new GameObject("Network Manager");
        NetworkManager.transform.SetParent(this.gameObject.transform);

        if (UFE.IsNetworkAddonInstalled)
        {
            UFE.config.networkInputDelay = UFE.config.networkOptions.defaultFrameDelay;
            UFE.config.rollbackEnabled = UFE.config.networkOptions.allowRollBacks;

            if (UFE.config.networkOptions.networkService == NetworkService.Photon && UFE.IsPhotonInstalled)
            {
                UFE.onlineMultiplayerAPI = NetworkManager.AddComponent(UFE.SearchClass("PhotonMultiplayerAPI")) as MultiplayerAPI;
                UFE.lanMultiplayerAPI = NetworkManager.AddComponent<NullMultiplayerAPI>();
            }
            else if (UFE.config.networkOptions.networkService == NetworkService.Photon && !UFE.IsPhotonInstalled)
            {
                Debug.LogError("You need 'Photon Unity Networking' installed in order to use Photon as a Network Service.");
            }
            UFE.onlineMultiplayerAPI.Initialize(uuid);

            if ((Application.platform == RuntimePlatform.Android ||
                 Application.platform == RuntimePlatform.IPhonePlayer ||
                 Application.platform == RuntimePlatform.tvOS) && UFE.IsBluetoothAddonInstalled)
            {
                UFE.bluetoothMultiplayerAPI = NetworkManager.AddComponent(UFE.SearchClass("BluetoothMultiplayerAPI")) as MultiplayerAPI;
            }
            else
            {
                UFE.bluetoothMultiplayerAPI = NetworkManager.AddComponent<NullMultiplayerAPI>();
            }

            UFE.lanMultiplayerAPI.Initialize(uuid);
            UFE.bluetoothMultiplayerAPI.Initialize(uuid);

            UFE.MultiplayerAPI.SendRate = 1 / (float)UFE.config.fps;

            UFE.localPlayerController = NetworkManager.AddComponent<UFEController>();
            UFE.remotePlayerController = NetworkManager.AddComponent<DummyInputController>();

            UFE.localPlayerController.isCPU = false;
            UFE.remotePlayerController.isCPU = false;
        }
        else
        {
            UFE.lanMultiplayerAPI = NetworkManager.AddComponent<NullMultiplayerAPI>();
            UFE.lanMultiplayerAPI.Initialize(uuid);

            UFE.onlineMultiplayerAPI = NetworkManager.AddComponent<NullMultiplayerAPI>();
            UFE.onlineMultiplayerAPI.Initialize(uuid);

            UFE.bluetoothMultiplayerAPI = NetworkManager.AddComponent<NullMultiplayerAPI>();
            UFE.bluetoothMultiplayerAPI.Initialize(uuid);
        }

        UFE.GameManager = new GameManager();
        UFE.FluxCapacitor = new FluxCapacitor(UFE.currentFrame);
        UFE._multiplayerMode = MultiplayerMode.Lan;


        // Initialize the input systems
        GameObject inputManager = new GameObject("Input Manager");
        inputManager.transform.SetParent(this.gameObject.transform);

        if (UFE.config.inputOptions.inputManagerType == InputManagerType.NativeTouchControls && UFE.config.inputOptions.nativeTouchControls != null)
        {
            GameObject touchControls = Instantiate(UFE.config.inputOptions.nativeTouchControls);
            touchControls.transform.SetParent(inputManager.transform);
        }

        // Player 1
        p1Controller = inputManager.AddComponent<UFEController>();
        if (UFE.config.inputOptions.inputManagerType == InputManagerType.ControlFreak)
        {
            p1Controller.humanController = inputManager.AddComponent<InputTouchController>();
        }
        else if (UFE.config.inputOptions.inputManagerType == InputManagerType.Rewired)
        {
            p1Controller.humanController = inputManager.AddComponent<RewiredInputController>();
            (p1Controller.humanController as RewiredInputController).rewiredPlayerId = 0;
        }
        else
        {
            p1Controller.humanController = inputManager.AddComponent<InputController>();
        }

        // Initialize AI
        p1SimpleAI = inputManager.AddComponent<SimpleAI>();
        p1SimpleAI.player = 1;

        p1RandomAI = inputManager.AddComponent<RandomAI>();
        p1RandomAI.player = 1;

        p1FuzzyAI = null;
        if (UFE.IsAiAddonInstalled && UFE.config.aiOptions.engine == AIEngine.FuzzyAI)
        {
            p1FuzzyAI = inputManager.AddComponent(UFE.SearchClass("RuleBasedAI")) as AbstractInputController;
            p1FuzzyAI.player = 1;
            p1Controller.cpuController = p1FuzzyAI;
        }
        else
        {
            p1Controller.cpuController = p1RandomAI;
        }

        p1Controller.isCPU = UFE.config.deploymentOptions.AIControlled[0];
        p1Controller.player = 1;

        // Player 2
        p2Controller = inputManager.AddComponent<UFEController>();
        if (UFE.config.inputOptions.inputManagerType == InputManagerType.Rewired)
        {
            p2Controller.humanController = inputManager.AddComponent<RewiredInputController>();
            (p2Controller.humanController as RewiredInputController).rewiredPlayerId = 1;
        }
        else
        {
            p2Controller.humanController = inputManager.AddComponent<InputController>();
        }

        // Initialize AI
        p2SimpleAI = inputManager.AddComponent<SimpleAI>();
        p2SimpleAI.player = 2;

        p2RandomAI = inputManager.AddComponent<RandomAI>();
        p2RandomAI.player = 2;

        p2FuzzyAI = null;
        if (UFE.IsAiAddonInstalled && UFE.config.aiOptions.engine == AIEngine.FuzzyAI)
        {
            p2FuzzyAI = inputManager.AddComponent(UFE.SearchClass("RuleBasedAI")) as AbstractInputController;
            p2FuzzyAI.player = 2;
            p2Controller.cpuController = p2FuzzyAI;
        }
        else
        {
            p2Controller.cpuController = p2RandomAI;
        }

        p2Controller.isCPU = UFE.config.deploymentOptions.AIControlled[1];
        p2Controller.player = 2;


        p1Controller.Initialize(config.player1_Inputs);
        p2Controller.Initialize(config.player2_Inputs);

        if (config.fps > 0)
        {
            UFE.timeScale = UFE.config._gameSpeed;
            Application.targetFrameRate = config.fps;
        }

        SetLanguage();
        UFE.InitializeAudioSystem();
        UFE.SetAIDifficulty(UFE.config.aiOptions.selectedDifficultyLevel);
        UFE.SetDebugMode(config.debugOptions.debugMode);

        // Load the player settings from disk
        UFE.SetMusic(PlayerPrefs.GetInt(UFE.MusicEnabledKey, 1) > 0);
        UFE.SetMusicVolume(PlayerPrefs.GetFloat(UFE.MusicVolumeKey, 1f));
        UFE.SetSoundFX(PlayerPrefs.GetInt(UFE.SoundsEnabledKey, 1) > 0);
        UFE.SetSoundFXVolume(PlayerPrefs.GetFloat(UFE.SoundsVolumeKey, 1f));
    }

    protected void Start()
    {
        // Check for active EventSystem and spawn one if there are none
        if (EventSystem.current != null)
        {
            UFE.eventSystem = EventSystem.current;
        }
        else
        {
            UFE.eventSystem = FindObjectOfType<EventSystem>();
            if (UFE.eventSystem == null)
                UFE.eventSystem = gameObject.AddComponent<EventSystem>();
        }

        // Load the intro screen or the combat, depending on the UFE Config settings
        if (UFE.config.deploymentOptions.deploymentType != DeploymentType.FullInterface)
        {
            if (UFE.config.deploymentOptions.deploymentType == DeploymentType.TrainingMode)
            {
                UFE.gameMode = GameMode.TrainingRoom;
            }
            else if (UFE.config.deploymentOptions.deploymentType == DeploymentType.ChallengeMode)
            {
                UFE.gameMode = GameMode.ChallengeMode;
            }
            else
            {
                UFE.gameMode = GameMode.VersusMode;
            }

            if (UFE.config.stages.Length > 0)
            {
                UFE.config.selectedStage = UFE.config.stages[0];
            }
            else
            {
                Debug.LogError("No stage found.");
            }

            if (UFE.config.selectedMatchType == MatchType.Singles)
            {
                UFE.config.player1Character = UFE.config.deploymentOptions.activeCharacters[0];
                UFE.config.player2Character = UFE.config.deploymentOptions.activeCharacters[1];
                UFE.SetCPU(1, UFE.config.deploymentOptions.AIControlled[0]);
                UFE.SetCPU(2, UFE.config.deploymentOptions.AIControlled[1]);
            }
            else
            {
                int maxSizePlayer1 = UFE.config.teamModes[UFE.config.selectedTeamMode].teams[0].characters.Length;
                int maxSizePlayer2 = UFE.config.teamModes[UFE.config.selectedTeamMode].teams[1].characters.Length;

                if (maxSizePlayer1 <= 0) Debug.LogError("Player 1 Character Slot Empty");
                if (maxSizePlayer2 <= 0) Debug.LogError("Player 2 Character Slot Empty");

                UFE.config.player1Team = new UFE3D.CharacterInfo[maxSizePlayer1];
                UFE.config.player2Team = new UFE3D.CharacterInfo[maxSizePlayer2];

                UFE.config.player1Character = UFE.config.deploymentOptions.activeCharacters[0];

                int charArrayCount = 0;
                for (int i = 0; i < maxSizePlayer1; i++)
                {
                    UFE.config.player1Team[i] = UFE.config.deploymentOptions.activeCharacters[charArrayCount];
                    charArrayCount++;
                }

                UFE.config.player2Character = UFE.config.deploymentOptions.activeCharacters[charArrayCount];

                for (int i = 0; i < maxSizePlayer2; i++)
                {
                    UFE.config.player2Team[i] = UFE.config.deploymentOptions.activeCharacters[charArrayCount];
                    charArrayCount++;
                }

                UFE.SetCPU(1, UFE.config.deploymentOptions.AIControlled[0]);
                UFE.SetCPU(2, UFE.config.deploymentOptions.AIControlled[1]);
            }

            UFE.eventSystem.enabled = false;


            if (UFE.gameMode != GameMode.ChallengeMode && UFE.config.deploymentOptions.skipLoadingScreen)
            {
                UFE._StartGame((float)UFE.config.gameGUI.gameFadeDuration);
            }
            else
            {
                if (UFE.gameMode == GameMode.ChallengeMode)
                {
                    currentChallenge = UFE.config.selectedChallenge;
                    SetChallengeVariables(currentChallenge);
                }

                UFE._StartLoadingBattleScreen((float)UFE.config.gameGUI.screenFadeDuration);
            }
        }
        else
        {
            UFE.StartIntroScreen(0f);
        }
    }

    //public List<Dictionary<System.Reflection.MemberInfo, System.Object>> dictionaryList = new List<Dictionary<System.Reflection.MemberInfo, System.Object>>();
    protected void Update()
    {
        if (ReplayMode == null || !ReplayMode.isPlayback)
        {
            UFE.GetPlayer1Controller().DoUpdate();
            UFE.GetPlayer2Controller().DoUpdate();
        }

        if (UFE.FluxCapacitor != null && UFE.gameRunning && ReplayMode != null)
            ReplayMode.UFEUpdate();
    }

    protected void FixedUpdate()
    {
        if (ReplayMode == null || !ReplayMode.isPlayback)
        {
            UFE.GameManager.DoFixedUpdate();
        }

        if (ReplayMode != null) ReplayMode.UFEFixedUpdate();
    }
    #endregion


    #region Challenge mode methods
    public static ChallengeModeOptions GetChallenge(int challengeNum = -1)
    {
        if (challengeNum == -1) challengeNum = UFE.currentChallenge;
        if (challengeNum >= UFE.config.challengeModeOptions.Length)
        {
            Debug.LogError("Challenge Not Found");
            return null;
        }
        return UFE.config.challengeModeOptions[challengeNum];
    }

    public static void NextChallenge()
    {
        if (UFE.config.challengeModeOptions.Length > currentChallenge + 1)
            currentChallenge++;
    }

    public static void SetChallengeVariables(int selection = -1)
    {
        if (selection == -1) selection = currentChallenge;
        UFE.config.player1Character = UFE.GetChallenge(selection).character;
        p1Controller.isCPU = false;
        UFE.config.player2Character = UFE.GetChallenge(selection).opCharacter;
        p2Controller.isCPU = UFE.GetChallenge(selection).aiOpponent;
    }
    #endregion


    #region Load & spawn methods
    public static void SetActiveStageScene()
    {
        Scene stageScene;
        if (UFE.config.selectedStage.stagePath.Contains(".unity"))
        {
            stageScene = SceneManager.GetSceneByPath(UFE.config.selectedStage.stagePath);
        }
        else
        {
            stageScene = SceneManager.GetSceneByName(UFE.config.selectedStage.stagePath);
        }

        SceneManager.SetActiveScene(stageScene);
    }

    public static ControlsScript SpawnCharacter(UFE3D.CharacterInfo characterInfo, int player, int mirror, FPVector location, bool isAssist, MoveInfo enterMove = null, MoveInfo exitMove = null, int altCostume = -1)
    {

        if (!isAssist && characterInfo == null)
        {
            Debug.LogError("Player " + player + " character not found! Make sure you have set the characters correctly in the Editor");
            return null;
        }
        else if (characterInfo == null)
        {
            Debug.LogError("Assist character for player " + player + " not found! Make sure you have set the character correctly in the Move Editor");
            return null;
        }

        GameObject go = null;
        ControlsScript cScript = null;
        bool isNew = false;

        if (isAssist)
        {
#if !UFE_LITE && !UFE_BASIC
            if (player == 1 && UFE.p1ControlsScript != null)
            {
                cScript = FindSpawnedAssist(UFE.p1ControlsScript, characterInfo);
            }
            else if (player == 2 && UFE.p2ControlsScript != null)
            {
                cScript = FindSpawnedAssist(UFE.p2ControlsScript, characterInfo);
            }

            if (cScript == null)
            {
                go = new GameObject("Player" + player + "_Assist");
                go.transform.parent = GameEngine.transform;
                isNew = true;
            }
#endif
        }
        else
        {

            if (UFE.config.selectedMatchType == MatchType.Singles)
            {
                if (player == 1 && UFE.p1ControlsScript != null)
                {
                    cScript = UFE.p1ControlsScript;
                }
                else if (player == 2 && UFE.p2ControlsScript != null)
                {
                    cScript = UFE.p2ControlsScript;
                }
                else
                {
                    go = new GameObject("Player" + player);
                    go.transform.parent = GameEngine.transform;
                    isNew = true;
                }
            }
            else
            {
                go = new GameObject("Player" + player + "_Character");
                GameObject teamGO = GameObject.Find("Team" + player);
                if (teamGO == null) teamGO = new GameObject("Team" + player);
                go.transform.parent = teamGO.transform;
                teamGO.transform.parent = GameEngine.transform;

                isNew = true;
            }
        }

        if (isNew)
        {
            cScript = go.AddComponent<ControlsScript>();
            cScript.worldTransform = go.AddComponent<FPTransform>();
            cScript.myInfo = Instantiate(characterInfo);
            cScript.playerNum = player;
            cScript.cameraScript = UFE.CameraScript;
            cScript.debugInfo = player == 1 ? UFE.config.debugOptions.p1DebugInfo : UFE.config.debugOptions.p2DebugInfo;

#if !UFE_LITE && !UFE_BASIC
            cScript.isAssist = isAssist;
#endif

            // Instantiate Character Prefab
            if (characterInfo.characterPrefabStorage == StorageMode.Prefab && characterInfo.characterPrefab == null)
                Debug.LogError("Character prefab for " + go.name + " not found. Make sure you have selected a prefab character in the Character Editor");

            GameObject characterPrefab;
            if (altCostume > -1)
            {
                if (characterInfo.alternativeCostumes[altCostume].characterPrefabStorage == StorageMode.Prefab)
                {
                    characterPrefab = Instantiate(characterInfo.alternativeCostumes[altCostume].prefab);
                }
                else
                {
                    characterPrefab = Instantiate(Resources.Load<GameObject>(characterInfo.alternativeCostumes[altCostume].prefabResourcePath));
                }

                cScript.isAlt = true;
                cScript.selectedCostume = altCostume;
            }
            else
            {
                if (characterInfo.characterPrefabStorage == StorageMode.Prefab)
                {
                    characterPrefab = Instantiate(characterInfo.characterPrefab);
                }
                else
                {
                    characterPrefab = Instantiate(Resources.Load<GameObject>(characterInfo.prefabResourcePath));
                }
            }

            cScript.character = characterPrefab;
            cScript.character.transform.parent = cScript.transform;
            cScript.localTransform = cScript.character.AddComponent<FPTransform>();
            if (UFE.config.gameplayType == GameplayType._2DFighter)
            {
                cScript.localTransform.rotation = characterInfo.initialRotation;
                cScript.standardYRotation = cScript.localTransform.eulerAngles.y;
            }
            else
            {
                characterPrefab.transform.rotation = Quaternion.identity;
                cScript.standardYRotation = -90;
            }

            cScript.Physics = cScript.gameObject.AddComponent<PhysicsScript>();
            cScript.MoveSet = cScript.character.AddComponent<MoveSetScript>();
            cScript.HitBoxes = cScript.character.GetComponent<HitBoxesScript>();
            cScript.HitBoxes.blockableArea = null;
            cScript.HitBoxes.activeHurtBoxes = null;

            cScript.HitBoxes.autoHitBoxes = new HitBox[cScript.HitBoxes.hitBoxes.Length];
            Array.Copy(cScript.HitBoxes.hitBoxes, cScript.HitBoxes.autoHitBoxes, cScript.HitBoxes.hitBoxes.Length);

            cScript.Physics.controlScript = cScript;
            cScript.Physics.moveSetScript = cScript.MoveSet;
            cScript.MoveSet.controlsScript = cScript;
            cScript.MoveSet.hitBoxesScript = cScript.HitBoxes;
            cScript.HitBoxes.controlsScript = cScript;
            cScript.HitBoxes.moveSetScript = cScript.MoveSet;
        }
        else
        {
            cScript.SetActive(true);
        }

        cScript.ResetData(true);
        cScript.mirror = mirror;
        cScript.worldTransform.position = location;
        cScript.transform.position = location.ToVector();
        cScript.HitBoxes.UpdateMap(0);

#if !UFE_LITE && !UFE_BASIC
        // Assist Moves
        cScript.enterMove = enterMove;
        cScript.exitMove = exitMove;
#endif

        if (!isNew) return cScript;

        cScript.currentGaugesPoints = new Fix64[10];
        if (UFE.gameMode == GameMode.TrainingRoom)
        {
            cScript.currentLifePoints = (Fix64)characterInfo.lifePoints * ((player == 1 ? UFE.config.trainingModeOptions.p1StartingLife : UFE.config.trainingModeOptions.p2StartingLife) / 100);
            for (int i = 0; i < cScript.currentGaugesPoints.Length; i++)
            {
                cScript.currentGaugesPoints[i] = (Fix64)characterInfo.maxGaugePoints * ((player == 1 ? UFE.config.trainingModeOptions.p1StartingGauge : UFE.config.trainingModeOptions.p2StartingGauge) / 100);
            }
        }
        else
        {
            cScript.currentLifePoints = characterInfo.lifePoints;
        }

#if !UFE_LITE && !UFE_BASIC
        if (isAssist)
        {
            if (player == 1)
            {
                cScript.owner = UFE.p1ControlsScript;
                UFE.p1ControlsScript.assists.Add(cScript);
            }
            else if (player == 2)
            {
                cScript.owner = UFE.p2ControlsScript;
                UFE.p2ControlsScript.assists.Add(cScript);
            }

        }
#endif
        return cScript;
    }

#if !UFE_LITE && !UFE_BASIC
    public static void FindAndSpawnAssist(ControlsScript controlsScript, int player)
    {
        List<MoveSetData> loadedMoveSets = new List<MoveSetData>();
        foreach (MoveSetData moveSetData in controlsScript.myInfo.moves)
        {
            loadedMoveSets.Add(moveSetData);
        }
        foreach (string path in controlsScript.myInfo.stanceResourcePath)
        {
            loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
        }

        foreach (MoveSetData moveSet in loadedMoveSets)
        {
            foreach (MoveInfo move in moveSet.attackMoves)
            {
                foreach (CharacterAssist charAssist in move.characterAssist)
                {
                    if (charAssist.characterInfo != null)
                    {
                        foreach (ControlsScript cAssist in controlsScript.assists)
                        {
                            if (cAssist.myInfo.characterName == charAssist.characterInfo.characterName) continue;
                        }
                        ControlsScript cScript = SpawnCharacter(charAssist.characterInfo, player, -1, new FPVector(-999, -999, 0), true);
                        cScript.opControlsScript = controlsScript.opControlsScript;
                        cScript.opInfo = controlsScript.opInfo;
                        cScript.SetActive(false);

                        if (UFE.config.debugOptions.preloadedObjects) Debug.Log(move.moveName + " - " + charAssist.characterInfo.characterName + " Assist Preloaded");
                    }
                }
            }
        }
    }

    public static ControlsScript FindSpawnedAssist(ControlsScript owner, UFE3D.CharacterInfo characterInfo)
    {
        foreach (ControlsScript csAssist in owner.assists)
        {
            if (csAssist.myInfo.characterName == characterInfo.characterName) return csAssist;
        }
        return null;
    }
#endif

    public static GameObject SpawnGameObject(GameObject gameObject)
    {
        return SpawnGameObject(gameObject, Vector3.zero, Quaternion.identity);
    }

    public static GameObject SpawnGameObject(GameObject gameObject, Vector3 position, Quaternion rotation, bool addMrFusion, Fix64 destroyTimerSeconds)
    {
        long? newDestroyTimer = null;
        if (destroyTimerSeconds != 0) newDestroyTimer = (long)(destroyTimerSeconds * UFE.fps);
        return SpawnGameObject(gameObject, position, rotation, newDestroyTimer, addMrFusion);
    }

    public static GameObject SpawnGameObject(GameObject gameObject, Vector3 position, Quaternion rotation, long? durationFrames = null, bool addMrFusion = false, string id = null)
    {
        if (gameObject == null) return null;

        GameObject goInstance = null;
        MrFusion mrFusion = null;
        long creationFrame = UFE.currentFrame;
        string uniqueId = id ?? gameObject.name + creationFrame;

        foreach (InstantiatedGameObject entry in UFE.instantiatedObjects)
        {
            if (entry.id == uniqueId)
            {
                goInstance = entry.gameObject;
                goInstance.transform.position = position;
                goInstance.transform.rotation = rotation;
                goInstance.SetActive(true);

                entry.creationFrame = creationFrame;
                entry.destructionFrame = creationFrame + durationFrames;
                if (entry.mrFusion != null)
                {
                    mrFusion = entry.mrFusion;
                }

                break;
            }
        }

        if (goInstance == null)
        {
            goInstance = UnityEngine.Object.Instantiate(gameObject, position, rotation);
            goInstance.transform.SetParent(UFE.SpawnPool.transform);
            goInstance.name = uniqueId;
            if (addMrFusion)
            {
                mrFusion = (MrFusion)goInstance.GetComponent(typeof(MrFusion));
                if (mrFusion == null) mrFusion = goInstance.AddComponent<MrFusion>();
            }

            Dictionary<ParticleSystem, float> particleStorage = null;
            if (UFE.config.networkOptions.controlParticles)
            {
                particleStorage = new Dictionary<ParticleSystem, float>();
                ParticleSystem[] particles = goInstance.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem particle in particles)
                {
                    particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    if (UFE.config.networkOptions.particleRandomSeed) particle.randomSeed = (uint)creationFrame;
                    if (UFE.config.networkOptions.particleSimulatedSpeed) particleStorage.Add(particle, particle.main.simulationSpeed);
                }
            }

            UFE.instantiatedObjects.Add(new InstantiatedGameObject(uniqueId, goInstance, mrFusion, particleStorage, creationFrame, creationFrame + durationFrames));
        }

        return goInstance;
    }

    public static void DestroyGameObject(GameObject gameObject, long? destroyTimer = null)
    {
        for (int i = 0; i < UFE.instantiatedObjects.Count; ++i)
        {
            if (UFE.instantiatedObjects[i].gameObject.name == gameObject.name)
            {
                UFE.instantiatedObjects[i].destructionFrame = destroyTimer == null ? UFE.currentFrame : destroyTimer;
                break;
            }
        }
    }
    #endregion


    #region Preload objects (pre-battle)
    //Preloader
    public static void PreloadBattle()
    {
        PreloadBattle((float)UFE.config._preloadingTime);
    }

    public static void PreloadBattle(float warmTimer)
    {
        if (UFE.config.preloadHitEffects)
        {
            SearchAndCastGameObject(UFE.config.hitOptions.weakHit, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.mediumHit, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.heavyHit, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.crumpleHit, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.customHit1, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.customHit2, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.customHit3, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.customHit4, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.customHit5, warmTimer);
            SearchAndCastGameObject(UFE.config.hitOptions.customHit6, warmTimer);

            SearchAndCastGameObject(UFE.config.groundBounceOptions, warmTimer);
            SearchAndCastGameObject(UFE.config.wallBounceOptions, warmTimer);
            SearchAndCastGameObject(UFE.config.blockOptions, warmTimer);

            SearchAndCastGameObject(UFE.GetPlayer1(), warmTimer);
            SearchAndCastGameObject(UFE.GetPlayer2(), warmTimer);

            if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Hit Effects Preloaded");
        }

        if (UFE.config.preloadStage)
        {
            SearchAndCastGameObject(UFE.config.selectedStage, warmTimer);
            if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Stage Preloaded");
        }

        if (UFE.config.warmAllShaders) Shader.WarmupAllShaders();

        memoryDump.Clear();
    }

    public static void SearchAndCastGameObject(UFE3D.CharacterInfo characterInfo, float warmTimer)
    {
        List<MoveSetData> loadedMoveSets = new List<MoveSetData>();
        foreach (MoveSetData moveSetData in characterInfo.moves)
        {
            loadedMoveSets.Add(moveSetData);
        }
        foreach (string path in characterInfo.stanceResourcePath)
        {
            loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
        }

        foreach (MoveSetData moveSet in loadedMoveSets)
        {
            foreach (MoveInfo move in moveSet.attackMoves)
            {
                foreach (MoveParticleEffect particle in move.particleEffects) SearchAndCastGameObject(particle, warmTimer);
                foreach (Projectile projectile in move.projectiles) SearchAndCastGameObject(projectile, warmTimer);
            }
        }
    }

    public static void SearchAndCastGameObject(object target, float warmTimer)
    {
        if (target != null)
        {
            Type typeSource = target.GetType();
            FieldInfo[] fields = typeSource.GetFields();

            foreach (FieldInfo field in fields)
            {
                object fieldValue = field.GetValue(target);
                if (fieldValue == null || fieldValue.Equals(null)) continue;
                if (memoryDump.Contains(fieldValue)) continue;
                memoryDump.Add(fieldValue);

                if (field.FieldType.Equals(typeof(GameObject)))
                {
                    GameObject tempGO = Instantiate((GameObject)fieldValue);
                    tempGO.transform.position = new Vector2(-999, -999);
                    Destroy(tempGO, warmTimer);
                    if (UFE.config.debugOptions.preloadedObjects) Debug.Log(fieldValue + " Preloaded");

                }
                else if (field.FieldType.IsArray && !field.FieldType.GetElementType().IsEnum)
                {
                    object[] fieldValueArray = (object[])fieldValue;
                    foreach (object obj in fieldValueArray)
                    {
                        SearchAndCastGameObject(obj, warmTimer);
                    }
                }
            }
        }
    }
    #endregion
}