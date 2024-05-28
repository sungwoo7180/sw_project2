using System.Collections.Generic;
using UnityEngine;
using UFE3D;
using System;
using FPLibrary;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public partial class UFE
{
	#region GUI definitions
	public static UFEScreen currentScreen { get; set; }
	public static UFEScreen battleGUI { get; set; }
	public static Canvas canvas { get; protected set; }
	public static CanvasGroup canvasGroup { get; protected set; }
	public static EventSystem eventSystem { get; protected set; }
	public static GraphicRaycaster graphicRaycaster { get; protected set; }
	public static StandaloneInputModule standaloneInputModule { get; protected set; }
	protected static readonly string MusicEnabledKey = "MusicEnabled";
	protected static readonly string MusicVolumeKey = "MusicVolume";
	protected static readonly string SoundsEnabledKey = "SoundsEnabled";
	protected static readonly string SoundsVolumeKey = "SoundsVolume";
	protected static readonly string DifficultyLevelKey = "DifficultyLevel";
	protected static readonly string DebugModeKey = "DebugMode";
	#endregion

	#region Story mode definitions
	//-----------------------------------------------------------------------------------------------------------------
	// Required for the Story Mode: if the player lost its previous battle, 
	// he needs to fight the same opponent again, not the next opponent.
	//-----------------------------------------------------------------------------------------------------------------
	private static StoryModeInfo storyMode = new StoryModeInfo();
	private static List<string> unlockedCharactersInStoryMode = new List<string>();
	private static List<string> unlockedCharactersInVersusMode = new List<string>();
	private static bool player1WonLastBattle;
	private static int lastStageIndex;
	#endregion

	#region Debug propeties
	public static bool debug = true;
	public static Text debugger1;
	public static Text debugger2;

	public static bool autoSaveAssets;
	#endregion

	#region GUI related methods
	public static BattleGUI GetBattleGUI()
	{
		return UFE.config.gameGUI.battleGUI;
	}

	public static BluetoothGameScreen GetBluetoothGameScreen()
	{
		return UFE.config.gameGUI.bluetoothGameScreen;
	}

	public static CharacterSelectionScreen GetCharacterSelectionScreen()
	{
		return UFE.config.gameGUI.characterSelectionScreen;
	}

	public static ConnectionLostScreen GetConnectionLostScreen()
	{
		return UFE.config.gameGUI.connectionLostScreen;
	}

	public static CreditsScreen GetCreditsScreen()
	{
		return UFE.config.gameGUI.creditsScreen;
	}

	public static HostGameScreen GetHostGameScreen()
	{
		return UFE.config.gameGUI.hostGameScreen;
	}

	public static IntroScreen GetIntroScreen()
	{
		return UFE.config.gameGUI.introScreen;
	}

	public static JoinGameScreen GetJoinGameScreen()
	{
		return UFE.config.gameGUI.joinGameScreen;
	}

	public static LoadingBattleScreen GetLoadingBattleScreen()
	{
		return UFE.config.gameGUI.loadingBattleScreen;
	}

	public static MainMenuScreen GetMainMenuScreen()
	{
		return UFE.config.gameGUI.mainMenuScreen;
	}

	public static NetworkRoomMatchScreen GetNetworkGameScreen()
	{
		return UFE.config.gameGUI.roomMatchScreen;
	}

	public static OptionsScreen GetOptionsScreen()
	{
		return UFE.config.gameGUI.optionsScreen;
	}

	public static StageSelectionScreen GetStageSelectionScreen()
	{
		return UFE.config.gameGUI.stageSelectionScreen;
	}

	public static StoryModeScreen GetStoryModeCongratulationsScreen()
	{
		return UFE.config.gameGUI.storyModeCongratulationsScreen;
	}

	public static StoryModeContinueScreen GetStoryModeContinueScreen()
	{
		return UFE.config.gameGUI.storyModeContinueScreen;
	}

	public static StoryModeScreen GetStoryModeGameOverScreen()
	{
		return UFE.config.gameGUI.storyModeGameOverScreen;
	}

	public static VersusModeAfterBattleScreen GetVersusModeAfterBattleScreen()
	{
		return UFE.config.gameGUI.versusModeAfterBattleScreen;
	}

	public static VersusModeScreen GetVersusModeScreen()
	{
		return UFE.config.gameGUI.versusModeScreen;
	}

	public static void HideScreen(UFEScreen screen)
	{
		if (screen != null)
		{
			screen.OnHide();
			GameObject.Destroy(screen.gameObject);
			if (!gameRunning && GameEngine != null) UFE.EndGame();
		}
	}

	public static void ShowScreen(UFEScreen screen, Action nextScreenAction = null)
	{
		if (screen != null)
		{
			if (UFE.OnScreenChanged != null)
			{
				UFE.OnScreenChanged(UFE.currentScreen, screen);
			}

			UFE.currentScreen = GameObject.Instantiate(screen);
			UFE.currentScreen.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);

			StoryModeScreen storyModeScreen = UFE.currentScreen as StoryModeScreen;
			if (storyModeScreen != null)
			{
				storyModeScreen.nextScreenAction = nextScreenAction;
			}

			UFE.currentScreen.OnShow();
		}
	}

	public static void Quit()
	{
		Application.Quit();
	}

	public static void StartBluetoothGameScreen()
	{
		UFE.StartBluetoothGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartBluetoothGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartBluetoothGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartBluetoothGameScreen(fadeTime / 2f);
		}
	}
	public static void StartBluetoothHostGameScreen()
	{
		UFE.StartBluetoothHostGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartBluetoothHostGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartBluetoothHostGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartBluetoothHostGameScreen(fadeTime / 2f);
		}
	}

	public static void StartBluetoothJoinGameScreen()
	{
		UFE.StartBluetoothJoinGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartBluetoothJoinGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartBluetoothJoinGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartBluetoothJoinGameScreen(fadeTime / 2f);
		}
	}

	public static void StartCharacterSelectionScreen()
	{
		UFE.StartCharacterSelectionScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartCharacterSelectionScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartCharacterSelectionScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartCharacterSelectionScreen(fadeTime / 2f);
		}
	}

	public static void StartCpuVersusCpu()
	{
		UFE.StartCpuVersusCpu((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartCpuVersusCpu(float fadeTime)
	{
		UFE.SetCPU(1, true);
		UFE.SetCPU(2, true);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartConnectionLostScreen()
	{
		UFE.StartConnectionLostScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartConnectionLostScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartConnectionLostScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartConnectionLostScreen(fadeTime / 2f);
		}
	}

	public static void StartCreditsScreen()
	{
		UFE.StartCreditsScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartCreditsScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartCreditsScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartCreditsScreen(fadeTime / 2f);
		}
	}

	public static void StartGame()
	{
		UFE.StartGame((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartGame(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.gameFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartGame(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartGame(fadeTime / 2f);
		}
	}

	public static void StartHostGameScreen()
	{
		UFE.StartHostGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartHostGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartHostGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartHostGameScreen(fadeTime / 2f);
		}
	}

	public static void StartIntroScreen()
	{
		UFE.StartIntroScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartIntroScreen(float fadeTime)
	{
		if (UFE.currentScreen != null && UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartIntroScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartIntroScreen(fadeTime / 2f);
		}
	}

	public static void StartJoinGameScreen()
	{
		UFE.StartJoinGameScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartJoinGameScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartJoinGameScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartJoinGameScreen(fadeTime / 2f);
		}
	}

	public static void StartLoadingBattleScreen()
	{
		UFE.StartLoadingBattleScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartLoadingBattleScreen(float fadeTime)
	{
		if (UFE.currentScreen != null && UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartLoadingBattleScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartLoadingBattleScreen(fadeTime / 2f);
		}
	}

	public static void StartMainMenuScreen()
	{
		UFE.StartMainMenuScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartMainMenuScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartMainMenuScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartMainMenuScreen(fadeTime / 2f);
		}
	}

	public static void StartSearchMatchScreen()
	{
		UFE.StartSearchMatchScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartSearchMatchScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartSearchMatchScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartSearchMatchScreen(fadeTime / 2f);
		}
	}

	public static void StartNetworkConnectionScreen()
	{
		UFE.StartNetworkConnectionScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartNetworkConnectionScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartNetworkConnectionScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartNetworkConnectionScreen(fadeTime / 2f);
		}
	}

	public static void StartNetworkOptionsScreen()
	{
		UFE.StartNetworkOptionsScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartNetworkOptionsScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartNetworkOptionsScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartNetworkOptionsScreen(fadeTime / 2f);
		}
	}

	public static void StartRoomMatchScreen()
	{
		UFE.StartRoomMatchScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartRoomMatchScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartRoomMatchScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartRoomMatchScreen(fadeTime / 2f);
		}
	}

	public static void StartOptionsScreen()
	{
		UFE.StartOptionsScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartOptionsScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartOptionsScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartOptionsScreen(fadeTime / 2f);
		}
	}

	public static void StartPlayerVersusPlayer()
	{
		UFE.StartPlayerVersusPlayer((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartPlayerVersusPlayer(float fadeTime)
	{
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartPlayerVersusCpu()
	{
		UFE.StartPlayerVersusCpu((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartPlayerVersusCpu(float fadeTime)
	{
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, true);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartNetworkGame(bool isMasterClient, bool startImmediately)
	{
		if (UFE.config.debugOptions.connectionLog)
		{
			Debug.Log(
				"\n\n\n----------------------------------" +
				"\nSTART NETWORK GAME" +
				"\nStart Immediately = " + startImmediately +
				"\n----------------------------------\n\n\n"
			);
		}

		Application.runInBackground = true;

		UFE.localPlayerController.Initialize(UFE.p1Controller.inputReferences);
		UFE.localPlayerController.humanController = UFE.p1Controller.humanController;
		UFE.localPlayerController.cpuController = UFE.p1Controller.cpuController;
		UFE.remotePlayerController.Initialize(UFE.p2Controller.inputReferences);

		if (isMasterClient)
		{
			UFE.localPlayerController.player = 1;
			UFE.remotePlayerController.player = 2;
		}
		else
		{
			UFE.localPlayerController.player = 2;
			UFE.remotePlayerController.player = 1;
		}

		//UFE.FluxCapacitor.Initialize();
		UFE.ConnectionHandler = UFE.NetworkManager.AddComponent<ConnectionHandler>();
		UFE.gameMode = GameMode.NetworkGame;

		UFE.SetCPU(1, UFE.config.networkOptions.player1AI && isMasterClient);
		UFE.SetCPU(2, UFE.config.networkOptions.player2AI && !isMasterClient);

		if (startImmediately)
		{
			UFE.StartLoadingBattleScreen();
		}
		else
		{
			UFE.StartCharacterSelectionScreen();
		}
	}

	public static void StartStageSelectionScreen()
	{
		UFE.StartStageSelectionScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStageSelectionScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStageSelectionScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStageSelectionScreen(fadeTime / 2f);
		}
	}

	public static void StartStoryMode()
	{
		UFE.StartStoryMode((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryMode(float fadeTime)
	{
		//-------------------------------------------------------------------------------------------------------------
		// Required for loading the first combat correctly.
		UFE.player1WonLastBattle = true;
		//-------------------------------------------------------------------------------------------------------------
		UFE.gameMode = GameMode.StoryMode;

		UFE.SetCPU(1, false);
		UFE.SetCPU(2, true);
		UFE.storyMode.characterStory = null;
		UFE.storyMode.canFightAgainstHimself = UFE.config.storyMode.canCharactersFightAgainstThemselves;
		UFE.storyMode.currentGroup = -1;
		UFE.storyMode.currentBattle = -1;
		UFE.storyMode.currentBattleInformation = null;
		UFE.storyMode.defeatedOpponents.Clear();
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartStoryModeBattle()
	{
		UFE.StartStoryModeBattle((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeBattle(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeBattle(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeBattle(fadeTime / 2f);
		}
	}

	public static void StartStoryModeCongratulationsScreen()
	{
		UFE.StartStoryModeCongratulationsScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeCongratulationsScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeCongratulationsScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeCongratulationsScreen(fadeTime / 2f);
		}
	}

	public static void StartStoryModeContinueScreen()
	{
		UFE.StartStoryModeContinueScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeContinueScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeContinueScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeContinueScreen(fadeTime / 2f);
		}
	}

	public static void StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen)
	{
		UFE.StartStoryModeConversationAfterBattleScreen(conversationScreen, (float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeConversationAfterBattleScreen(conversationScreen, fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeConversationAfterBattleScreen(conversationScreen, fadeTime / 2f);
		}
	}

	public static void StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen)
	{
		UFE.StartStoryModeConversationBeforeBattleScreen(conversationScreen, (float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeConversationBeforeBattleScreen(conversationScreen, fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeConversationBeforeBattleScreen(conversationScreen, fadeTime / 2f);
		}
	}

	public static void StartStoryModeEndingScreen()
	{
		UFE.StartStoryModeEndingScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeEndingScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeEndingScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeEndingScreen(fadeTime / 2f);
		}
	}

	public static void StartStoryModeGameOverScreen()
	{
		UFE.StartStoryModeGameOverScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeGameOverScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeGameOverScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeGameOverScreen(fadeTime / 2f);
		}
	}

	public static void StartStoryModeOpeningScreen()
	{
		UFE.StartStoryModeOpeningScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartStoryModeOpeningScreen(float fadeTime)
	{
		// First, retrieve the character story, so we can find the opening associated to this player
		UFE.storyMode.characterStory = UFE.GetCharacterStory(UFE.GetPlayer1());

		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartStoryModeOpeningScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartStoryModeOpeningScreen(fadeTime / 2f);
		}
	}

	public static void StartTrainingMode()
	{
		UFE.StartTrainingMode((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartTrainingMode(float fadeTime)
	{
		UFE.gameMode = GameMode.TrainingRoom;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartChallengeMode(int selectedChallenge = 0)
	{
		UFE.StartChallengeMode((float)UFE.config.gameGUI.screenFadeDuration, selectedChallenge);
	}

	public static void StartChallengeMode(float fadeTime, int selectedChallenge = 0)
	{
		if (UFE.GetChallenge(selectedChallenge) == null) return;

		UFE.gameMode = GameMode.ChallengeMode;
		currentChallenge = selectedChallenge;
		UFE.SetChallengeVariables(currentChallenge);
		UFE.StartLoadingBattleScreen(fadeTime);
	}

	public static void StartVersusModeAfterBattleScreen()
	{
		UFE.StartVersusModeAfterBattleScreen(0f);
	}

	public static void StartVersusModeAfterBattleScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartVersusModeAfterBattleScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartVersusModeAfterBattleScreen(fadeTime / 2f);
		}
	}

	public static void StartChallengeModeAfterBattleScreen()
	{
		UFE.StartChallengeModeAfterBattleScreen(0f);
	}

	public static void StartChallengeModeAfterBattleScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartChallengeModeAfterBattleScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartChallengeModeAfterBattleScreen(fadeTime / 2f);
		}
	}

	public static void StartOnlineModeAfterBattleScreen()
	{
		UFE.StartOnlineModeAfterBattleScreen(0f);
	}

	public static void StartOnlineModeAfterBattleScreen(float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartOnlineModeAfterBattleScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartOnlineModeAfterBattleScreen(fadeTime / 2f);
		}
	}

	public static void StartVersusModeScreen()
	{
		UFE.StartVersusModeScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartVersusModeScreen(float fadeTime)
	{
		UFE.gameMode = GameMode.VersusMode;

		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartVersusModeScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartVersusModeScreen(fadeTime / 2f);
		}
	}

	public static void StartChallengeModeScreen()
	{
		UFE.StartChallengeModeScreen((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartChallengeModeScreen(float fadeTime)
	{
		UFE.gameMode = GameMode.VersusMode;

		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartChallengeModeScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartChallengeModeScreen(fadeTime / 2f);
		}
	}

	public static void StartTeamModeScreen()
	{
		UFE.StartTeamModeScreen(UFE.config.selectedTeamMode);
	}

	public static void StartTeamModeScreen(int teamMode)
	{
		UFE.StartTeamModeScreen((float)UFE.config.gameGUI.screenFadeDuration, teamMode);
	}

	public static void StartTeamModeScreen(float fadeTime, int teamMode)
	{
		UFE.gameMode = GameMode.VersusMode;
		UFE.config.selectedTeamMode = teamMode;

		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartVersusModeScreen(fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartVersusModeScreen(fadeTime / 2f);
		}
	}

	public static void WonStoryModeBattle()
	{
		UFE.WonStoryModeBattle((float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void WonStoryModeBattle(float fadeTime)
	{
		UFE.storyMode.defeatedOpponents.Add(UFE.storyMode.currentBattleInformation.opponentCharacterIndex);
		UFE.StartStoryModeConversationAfterBattleScreen(UFE.storyMode.currentBattleInformation.conversationAfterBattle, fadeTime);
	}

	public static void StartCustomScreen(int screenId)
	{
		UFE.StartCustomScreen(screenId, (float)UFE.config.gameGUI.screenFadeDuration);
	}

	public static void StartCustomScreen(int screenId, float fadeTime)
	{
		if (UFE.currentScreen.hasFadeOut)
		{
			UFE.eventSystem.enabled = false;
			CameraFade.StartAlphaFade(
				UFE.config.gameGUI.screenFadeColor,
				false,
				fadeTime / 2f,
				0f
			);
			UFE.DelayLocalAction(() => { UFE.eventSystem.enabled = true; UFE._StartCustomScreen(screenId, fadeTime / 2f); }, (Fix64)fadeTime / 2);
		}
		else
		{
			UFE._StartCustomScreen(screenId, fadeTime / 2f);
		}
	}

	private static void _StartBluetoothGameScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.bluetoothGameScreen == null)
		{
			Debug.LogError("Bluetooth Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.bluetoothGameScreen);
			if (!UFE.config.gameGUI.bluetoothGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
		}
	}
	private static void _StartBluetoothHostGameScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.bluetoothHostGameScreen == null)
		{
			Debug.LogError("Host Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.bluetoothHostGameScreen);
			if (!UFE.config.gameGUI.bluetoothHostGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartBluetoothJoinGameScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.bluetoothJoinGameScreen == null)
		{
			Debug.LogError("Join To Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.bluetoothJoinGameScreen);
			if (!UFE.config.gameGUI.bluetoothJoinGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartCharacterSelectionScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		CharacterSelectionScreen charSelScreen = (UFE.config.selectedMatchType != MatchType.Singles) ? UFE.config.gameGUI.teamSelectionScreen : UFE.config.gameGUI.characterSelectionScreen;

		if (charSelScreen == null)
		{
			Debug.LogError("Character Selection Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(charSelScreen);
			if (!charSelScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartIntroScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.introScreen == null)
		{
			//Debug.Log("Intro Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.introScreen);
			if (!UFE.config.gameGUI.introScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartMainMenuScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.mainMenuScreen == null)
		{
			Debug.LogError("Main Menu Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.mainMenuScreen);
			if (!UFE.config.gameGUI.mainMenuScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartStageSelectionScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.stageSelectionScreen == null)
		{
			Debug.LogError("Stage Selection Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.stageSelectionScreen);
			if (!UFE.config.gameGUI.stageSelectionScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartCreditsScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.creditsScreen == null)
		{
			Debug.Log("Credits screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.creditsScreen);
			if (!UFE.config.gameGUI.creditsScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartConnectionLostScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.connectionLostScreen == null)
		{
			Debug.LogError("Connection Lost Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.connectionLostScreen);
			if (!UFE.config.gameGUI.connectionLostScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartHostGameScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.hostGameScreen == null)
		{
			Debug.LogError("Host Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.hostGameScreen);
			if (!UFE.config.gameGUI.hostGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartJoinGameScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.joinGameScreen == null)
		{
			Debug.LogError("Join To Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.joinGameScreen);
			if (!UFE.config.gameGUI.joinGameScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartLoadingBattleScreen(float fadeTime)
	{
		UFE.config.lockInputs = true;

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.loadingBattleScreen == null)
		{
			Debug.Log("Loading Battle Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartGame((float)UFE.config.gameGUI.gameFadeDuration);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.loadingBattleScreen);
			if (!UFE.config.gameGUI.loadingBattleScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartNetworkConnectionScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.networkConnectionScreen == null)
		{
			Debug.LogError("Network Connection Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled || UFE.IsBluetoothAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.networkConnectionScreen);
			if (!UFE.config.gameGUI.networkConnectionScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartNetworkOptionsScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.networkOptionsScreen == null)
		{
			Debug.LogError("Network Options Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled || UFE.IsBluetoothAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.networkOptionsScreen);
			if (!UFE.config.gameGUI.networkOptionsScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartSearchMatchScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.randomMatchScreen == null)
		{
			Debug.LogError("Random Match Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			//UFE.AddNetworkEventListeners();
			UFE.ShowScreen(UFE.config.gameGUI.randomMatchScreen);
			if (!UFE.config.gameGUI.randomMatchScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartRoomMatchScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.roomMatchScreen == null)
		{
			Debug.LogError("Room Match Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else if (UFE.IsNetworkAddonInstalled)
		{
			UFE.ShowScreen(UFE.config.gameGUI.roomMatchScreen);
			if (!UFE.config.gameGUI.roomMatchScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartOptionsScreen(float fadeTime)
	{

		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.optionsScreen == null)
		{
			Debug.LogError("Options Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.optionsScreen);
			if (!UFE.config.gameGUI.optionsScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	public static void _StartStoryModeBattle(int groupNumber, float fadeTime = 0)
	{
		UFE.storyMode.currentGroup = groupNumber;
		_StartStoryModeBattle(fadeTime);
	}

	public static void _StartStoryModeBattle(float fadeTime)
	{
		// If the player 1 won the last battle, load the information of the next battle. 
		// Otherwise, repeat the last battle...
		UFE3D.CharacterInfo character = UFE.GetPlayer(1);

		if (UFE.player1WonLastBattle)
		{
			// If the player 1 won the last battle...
			if (UFE.storyMode.currentGroup < 0)
			{
				// If we haven't fought any battle, raise the "Story Mode Started" event...
				if (UFE.OnStoryModeStarted != null)
				{
					UFE.OnStoryModeStarted(character);
				}

				// And start with the first battle of the first group
				UFE.storyMode.currentGroup = 0;
				UFE.storyMode.currentBattle = 0;
			}
			else if (UFE.storyMode.currentGroup >= 0 && UFE.storyMode.currentGroup < UFE.storyMode.characterStory.fightsGroups.Length)
			{
				// Otherwise, check if there are more remaining battles in the current group
				FightsGroup currentGroup = UFE.storyMode.characterStory.fightsGroups[UFE.storyMode.currentGroup];
				int numberOfFights = currentGroup.maxFights;

				if (currentGroup.mode != FightsGroupMode.FightAgainstSeveralOpponentsInTheGroupInRandomOrder)
				{
					numberOfFights = currentGroup.opponents.Length;
				}

				if (UFE.storyMode.currentBattle < numberOfFights - 1)
				{
					// If there are more battles in the current group, go to the next battle...
					++UFE.storyMode.currentBattle;
				}
				else
				{
					// Otherwise, go to the next group of battles...
					++UFE.storyMode.currentGroup;
					UFE.storyMode.currentBattle = 0;
					UFE.storyMode.defeatedOpponents.Clear();
				}
			}

			// If the player hasn't finished the game...
			UFE.storyMode.currentBattleInformation = null;
			while (
				UFE.storyMode.currentBattleInformation == null &&
				UFE.storyMode.currentGroup >= 0 &&
				UFE.storyMode.currentGroup < UFE.storyMode.characterStory.fightsGroups.Length
			)
			{
				// Try to retrieve the information of the next battle
				FightsGroup currentGroup = UFE.storyMode.characterStory.fightsGroups[UFE.storyMode.currentGroup];
				UFE.storyMode.currentBattleInformation = null;

				if (currentGroup.mode == FightsGroupMode.FightAgainstAllOpponentsInTheGroupInTheDefinedOrder)
				{
					StoryModeBattle b = currentGroup.opponents[UFE.storyMode.currentBattle];
					UFE3D.CharacterInfo opponent = UFE.config.characters[b.opponentCharacterIndex];

					if (UFE.storyMode.canFightAgainstHimself || !character.characterName.Equals(opponent.characterName))
					{
						UFE.storyMode.currentBattleInformation = b;
					}
					else
					{
						// Otherwise, check if there are more remaining battles in the current group
						int numberOfFights = currentGroup.maxFights;

						if (currentGroup.mode != FightsGroupMode.FightAgainstSeveralOpponentsInTheGroupInRandomOrder)
						{
							numberOfFights = currentGroup.opponents.Length;
						}

						if (UFE.storyMode.currentBattle < numberOfFights - 1)
						{
							// If there are more battles in the current group, go to the next battle...
							++UFE.storyMode.currentBattle;
						}
						else
						{
							// Otherwise, go to the next group of battles...
							++UFE.storyMode.currentGroup;
							UFE.storyMode.currentBattle = 0;
							UFE.storyMode.defeatedOpponents.Clear();
						}
					}
				}
				else
				{
					List<StoryModeBattle> possibleBattles = new List<StoryModeBattle>();

					foreach (StoryModeBattle b in currentGroup.opponents)
					{
						if (!UFE.storyMode.defeatedOpponents.Contains(b.opponentCharacterIndex))
						{
							UFE3D.CharacterInfo opponent = UFE.config.characters[b.opponentCharacterIndex];

							if (UFE.storyMode.canFightAgainstHimself || !character.characterName.Equals(opponent.characterName))
							{
								possibleBattles.Add(b);
							}
						}
					}

					if (possibleBattles.Count > 0)
					{
						int index = UnityEngine.Random.Range(0, possibleBattles.Count);
						UFE.storyMode.currentBattleInformation = possibleBattles[index];
					}
					else
					{
						// If we can't find a valid battle in this group, try moving to the next group
						++UFE.storyMode.currentGroup;
					}
				}
			}
		}

		if (UFE.storyMode.currentBattleInformation != null)
		{
			// If we could retrieve the battle information, load the opponent and the stage
			int characterIndex = UFE.storyMode.currentBattleInformation.opponentCharacterIndex;
			UFE.SetPlayer2(UFE.config.characters[characterIndex]);

			if (UFE.player1WonLastBattle)
			{
				UFE.lastStageIndex = UnityEngine.Random.Range(0, UFE.storyMode.currentBattleInformation.possibleStagesIndexes.Count);
			}

			// Finally, check if we should display any "Conversation Screen" before the battle
			UFE._StartStoryModeConversationBeforeBattleScreen(UFE.storyMode.currentBattleInformation.conversationBeforeBattle, fadeTime);

			UFE.SetStage(UFE.config.stages[UFE.storyMode.currentBattleInformation.possibleStagesIndexes[UFE.lastStageIndex]]);

		}
		else
		{
			// Otherwise, show the "Congratulations" Screen
			if (UFE.OnStoryModeCompleted != null)
			{
				UFE.OnStoryModeCompleted(character);
			}

			UFE._StartStoryModeCongratulationsScreen(fadeTime);
		}
	}

	private static void _StartStoryModeCongratulationsScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeCongratulationsScreen == null)
		{
			Debug.Log("Congratulations Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartStoryModeEndingScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeCongratulationsScreen, delegate () { UFE.StartStoryModeEndingScreen(fadeTime); });
			if (!UFE.config.gameGUI.storyModeCongratulationsScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartStoryModeContinueScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeContinueScreen == null)
		{
			Debug.Log("Continue Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeContinueScreen);
			if (!UFE.config.gameGUI.storyModeContinueScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (conversationScreen != null)
		{
			UFE.ShowScreen(conversationScreen, delegate () { UFE.StartStoryModeBattle(fadeTime); });
			if (!conversationScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			UFE._StartStoryModeBattle(fadeTime);
		}
	}

	private static void _StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen, float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (conversationScreen != null)
		{
			UFE.ShowScreen(conversationScreen, delegate () { UFE.StartLoadingBattleScreen(fadeTime); });
			if (!conversationScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			UFE._StartLoadingBattleScreen(fadeTime);
		}
	}

	private static void _StartStoryModeEndingScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.storyMode.characterStory.ending == null)
		{
			Debug.Log("Ending Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartCreditsScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.storyMode.characterStory.ending, delegate () { UFE.StartCreditsScreen(fadeTime); });
			if (!UFE.storyMode.characterStory.ending.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartStoryModeGameOverScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeGameOverScreen == null)
		{
			Debug.Log("Game Over Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeGameOverScreen, delegate () { UFE.StartMainMenuScreen(fadeTime); });
			if (!UFE.config.gameGUI.storyModeGameOverScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartStoryModeOpeningScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.storyMode.characterStory.opening == null)
		{
			Debug.Log("Opening Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartStoryModeBattle(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.storyMode.characterStory.opening, delegate () { UFE.StartStoryModeBattle(fadeTime); });
			if (!UFE.storyMode.characterStory.opening.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartVersusModeScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.versusModeScreen == null)
		{
			Debug.Log("Versus Mode Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE.StartPlayerVersusPlayer(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.versusModeScreen);
			if (!UFE.config.gameGUI.versusModeScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartChallengeModeScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.challengeModeScreen == null)
		{
			Debug.Log("Challenge Mode Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			//UFE.StartChallengeMode();
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.challengeModeScreen);
			if (!UFE.config.gameGUI.versusModeScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartVersusModeAfterBattleScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.versusModeAfterBattleScreen == null)
		{
			Debug.Log("Versus Mode \"After Battle\" Screen not found! Make sure you have set the prefab correctly in the Global Editor");

			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.versusModeAfterBattleScreen);
			if (!UFE.config.gameGUI.versusModeAfterBattleScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartChallengeModeAfterBattleScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.challengeModeAfterBattleScreen == null)
		{
			Debug.Log("Versus Mode \"After Battle\" Screen not found! Make sure you have set the prefab correctly in the Global Editor");

			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.challengeModeAfterBattleScreen);
			if (!UFE.config.gameGUI.challengeModeAfterBattleScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartOnlineModeAfterBattleScreen(float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.onlineModeAfterBattleScreen == null)
		{
			Debug.Log("Versus Mode \"After Battle\" Screen not found! Make sure you have set the prefab correctly in the Global Editor");

			UFE._StartMainMenuScreen(fadeTime);
		}
		else
		{
			UFE.ShowScreen(UFE.config.gameGUI.onlineModeAfterBattleScreen);
			if (!UFE.config.gameGUI.onlineModeAfterBattleScreen.hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
	}

	private static void _StartCustomScreen(int screenId, float fadeTime)
	{
		UFE.HideScreen(UFE.currentScreen);
		if (screenId <= UFE.config.gameGUI.customScreens.Length && UFE.config.gameGUI.customScreens[screenId] != null)
		{
			UFE.ShowScreen(UFE.config.gameGUI.customScreens[screenId]);
			if (!UFE.config.gameGUI.customScreens[screenId].hasFadeIn) fadeTime = 0;
			CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, fadeTime);
		}
		else
		{
			Debug.Log("Custom screen not found! Make sure you have set the prefab correctly under Global Editor -> GUI Options -> Custom Interfaces");
		}
	}


	public static CharacterStory GetCharacterStory(UFE3D.CharacterInfo character)
	{
		if (!UFE.config.storyMode.useSameStoryForAllCharacters)
		{
			StoryMode storyMode = UFE.config.storyMode;

			for (int i = 0; i < UFE.config.characters.Length; ++i)
			{
				if (UFE.config.characters[i] == character && storyMode.selectableCharactersInStoryMode.Contains(i))
				{
					CharacterStory characterStory = null;

					if (storyMode.characterStories.TryGetValue(i, out characterStory) && characterStory != null)
					{
						return characterStory;
					}
				}
			}
		}

		return UFE.config.storyMode.defaultStory;
	}
	#endregion

	#region Character selection methods
	public static UFE3D.CharacterInfo GetPlayer(int player)
	{
		if (player == 1)
		{
			return UFE.GetPlayer1();
		}
		else if (player == 2)
		{
			return UFE.GetPlayer2();
		}
		return null;
	}

	public static UFE3D.CharacterInfo GetPlayer1()
	{
		return config.player1Character;
	}

	public static UFE3D.CharacterInfo GetPlayer2()
	{
		return config.player2Character;
	}

	public static UFE3D.CharacterInfo[] GetStoryModeSelectableCharacters()
	{
		List<UFE3D.CharacterInfo> characters = new List<UFE3D.CharacterInfo>();

		for (int i = 0; i < UFE.config.characters.Length; ++i)
		{
			if (
				UFE.config.characters[i] != null
				&&
				(
					UFE.config.storyMode.selectableCharactersInStoryMode.Contains(i) ||
					UFE.unlockedCharactersInStoryMode.Contains(UFE.config.characters[i].characterName)
				)
			)
			{
				characters.Add(UFE.config.characters[i]);
			}
		}

		return characters.ToArray();
	}

	public static UFE3D.CharacterInfo[] GetTrainingRoomSelectableCharacters()
	{
		List<UFE3D.CharacterInfo> characters = new List<UFE3D.CharacterInfo>();

		for (int i = 0; i < UFE.config.characters.Length; ++i)
		{
			// If the character is selectable on Story Mode or Versus Mode,
			// then the character should be selectable on Training Room...
			if (
				UFE.config.characters[i] != null
				&&
				(
					UFE.config.storyMode.selectableCharactersInStoryMode.Contains(i) ||
					UFE.config.storyMode.selectableCharactersInVersusMode.Contains(i) ||
					UFE.unlockedCharactersInStoryMode.Contains(UFE.config.characters[i].characterName) ||
					UFE.unlockedCharactersInVersusMode.Contains(UFE.config.characters[i].characterName)
				)
			)
			{
				characters.Add(UFE.config.characters[i]);
			}
		}

		return characters.ToArray();
	}

	public static UFE3D.CharacterInfo[] GetVersusModeSelectableCharacters()
	{
		List<UFE3D.CharacterInfo> characters = new List<UFE3D.CharacterInfo>();

		for (int i = 0; i < UFE.config.characters.Length; ++i)
		{
			if (
				UFE.config.characters[i] != null &&
				(
					UFE.config.storyMode.selectableCharactersInVersusMode.Contains(i) ||
					UFE.unlockedCharactersInVersusMode.Contains(UFE.config.characters[i].characterName)
				)
			)
			{
				characters.Add(UFE.config.characters[i]);
			}
		}

		return characters.ToArray();
	}

	public static void SetPlayer(int player, UFE3D.CharacterInfo info)
	{
		if (player == 1)
		{
			config.player1Character = info;
		}
		else if (player == 2)
		{
			config.player2Character = info;
		}
	}

	public static void SetTeamCharacter(int player, int position, UFE3D.CharacterInfo info)
	{
		if (player == 1)
		{
			config.player1Team[position] = info;
		}
		else if (player == 2)
		{
			config.player2Team[position] = info;
		}
	}

	public static UFE3D.CharacterInfo GetTeamCharacter(int player, int position)
	{
		if (player == 1)
		{
			return config.player1Team[position];
		}
		else if (player == 2)
		{
			return config.player2Team[position];
		}
		return null;
	}

	public static void SetPlayer1(UFE3D.CharacterInfo player1)
	{
		config.player1Character = player1;
	}

	public static void SetPlayer2(UFE3D.CharacterInfo player2)
	{
		config.player2Character = player2;
	}

	public static void LoadUnlockedCharacters()
	{
		UFE.unlockedCharactersInStoryMode.Clear();
		string value = PlayerPrefs.GetString("UCSM", null);

		if (!string.IsNullOrEmpty(value))
		{
			string[] characters = value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string character in characters)
			{
				unlockedCharactersInStoryMode.Add(character);
			}
		}


		UFE.unlockedCharactersInVersusMode.Clear();
		value = PlayerPrefs.GetString("UCVM", null);

		if (!string.IsNullOrEmpty(value))
		{
			string[] characters = value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string character in characters)
			{
				unlockedCharactersInVersusMode.Add(character);
			}
		}
	}

	public static void SaveUnlockedCharacters()
	{
		StringBuilder sb = new StringBuilder();
		foreach (string characterName in UFE.unlockedCharactersInStoryMode)
		{
			if (!string.IsNullOrEmpty(characterName))
			{
				if (sb.Length > 0)
				{
					sb.AppendLine();
				}
				sb.Append(characterName);
			}
		}
		PlayerPrefs.SetString("UCSM", sb.ToString());

		sb = new StringBuilder();
		foreach (string characterName in UFE.unlockedCharactersInVersusMode)
		{
			if (!string.IsNullOrEmpty(characterName))
			{
				if (sb.Length > 0)
				{
					sb.AppendLine();
				}
				sb.Append(characterName);
			}
		}
		PlayerPrefs.SetString("UCVM", sb.ToString());
		PlayerPrefs.Save();
	}

	public static void RemoveUnlockedCharacterInStoryMode(UFE3D.CharacterInfo character)
	{
		if (character != null && !string.IsNullOrEmpty(character.characterName))
		{
			UFE.unlockedCharactersInStoryMode.Remove(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}

	public static void RemoveUnlockedCharacterInVersusMode(UFE3D.CharacterInfo character)
	{
		if (character != null && !string.IsNullOrEmpty(character.characterName))
		{
			UFE.unlockedCharactersInVersusMode.Remove(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}

	public static void RemoveUnlockedCharactersInStoryMode()
	{
		UFE.unlockedCharactersInStoryMode.Clear();
		UFE.SaveUnlockedCharacters();
	}

	public static void RemoveUnlockedCharactersInVersusMode()
	{
		UFE.unlockedCharactersInVersusMode.Clear();
		UFE.SaveUnlockedCharacters();
	}

	public static void UnlockCharacterInStoryMode(UFE3D.CharacterInfo character)
	{
		if (
			character != null &&
			!string.IsNullOrEmpty(character.characterName) &&
			!UFE.unlockedCharactersInStoryMode.Contains(character.characterName)
		)
		{
			UFE.unlockedCharactersInStoryMode.Add(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}

	public static void UnlockCharacterInVersusMode(UFE3D.CharacterInfo character)
	{
		if (
			character != null &&
			!string.IsNullOrEmpty(character.characterName) &&
			!UFE.unlockedCharactersInVersusMode.Contains(character.characterName)
		)
		{
			UFE.unlockedCharactersInVersusMode.Add(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}
	#endregion

	#region Audio related methods
	public static bool GetMusic()
	{
		return config.music;
	}

	public static AudioClip GetMusicClip()
	{
		return UFE.musicAudioSource.clip;
	}

	public static bool GetSoundFX()
	{
		return config.soundfx;
	}

	public static float GetMusicVolume()
	{
		if (UFE.config != null) return config.musicVolume;
		return 1f;
	}

	public static float GetSoundFXVolume()
	{
		if (UFE.config != null) return UFE.config.soundfxVolume;
		return 1f;
	}

	public static void InitializeAudioSystem()
	{
		Camera cam = Camera.main;

		// Create the AudioSources required for the music and sound effects
		UFE.musicAudioSource = cam.GetComponent<AudioSource>();
		if (UFE.musicAudioSource == null)
		{
			UFE.musicAudioSource = cam.gameObject.AddComponent<AudioSource>();
		}

		UFE.musicAudioSource.loop = true;
		UFE.musicAudioSource.playOnAwake = false;
		UFE.musicAudioSource.volume = config.musicVolume;


		UFE.soundsAudioSource = cam.gameObject.AddComponent<AudioSource>();
		UFE.soundsAudioSource.loop = false;
		UFE.soundsAudioSource.playOnAwake = false;
		UFE.soundsAudioSource.volume = 1f;
	}

	public static bool IsPlayingMusic()
	{
		if (UFE.musicAudioSource.clip != null) return UFE.musicAudioSource.isPlaying;
		return false;
	}

	public static bool IsMusicLooped()
	{
		return UFE.musicAudioSource.loop;
	}

	public static bool IsPlayingSoundFX()
	{
		return false;
	}

	public static void LoopMusic(bool loop)
	{
		UFE.musicAudioSource.loop = loop;
	}

	public static void PlayMusic()
	{
		if (config.music && !UFE.IsPlayingMusic() && UFE.musicAudioSource.clip != null)
		{
			UFE.musicAudioSource.Play();
		}
	}

	public static void PlayMusic(AudioClip music)
	{
		if (music != null)
		{
			AudioClip oldMusic = UFE.GetMusicClip();

			if (music != oldMusic)
			{
				UFE.musicAudioSource.clip = music;
			}

			if (config.music && (music != oldMusic || !UFE.IsPlayingMusic()))
			{
				UFE.musicAudioSource.Play();
			}
		}
	}

	public static void PlaySound(IList<AudioClip> sounds)
	{
		if (sounds.Count > 0)
		{
			UFE.PlaySound(sounds[UnityEngine.Random.Range(0, sounds.Count)]);
		}
	}

	public static void PlaySound(AudioClip soundFX)
	{
		UFE.PlaySound(soundFX, UFE.GetSoundFXVolume());
	}

	public static void PlaySound(AudioClip soundFX, float volume)
	{
		if (config.soundfx && soundFX != null && UFE.soundsAudioSource != null)
		{
			UFE.soundsAudioSource.PlayOneShot(soundFX, volume);
		}
	}

	public static void SetMusic(bool on)
	{
		bool isPlayingMusic = UFE.IsPlayingMusic();
		UFE.config.music = on;

		if (on && !isPlayingMusic) UFE.PlayMusic();
		else if (!on && isPlayingMusic) UFE.StopMusic();

		PlayerPrefs.SetInt(UFE.MusicEnabledKey, on ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void SetSoundFX(bool on)
	{
		UFE.config.soundfx = on;
		PlayerPrefs.SetInt(UFE.SoundsEnabledKey, on ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void SetMusicVolume(float volume)
	{
		if (UFE.config != null) UFE.config.musicVolume = volume;
		if (UFE.musicAudioSource != null) UFE.musicAudioSource.volume = volume;

		PlayerPrefs.SetFloat(UFE.MusicVolumeKey, volume);
		PlayerPrefs.Save();
	}

	public static void SetSoundFXVolume(float volume)
	{
		if (UFE.config != null) UFE.config.soundfxVolume = volume;
		PlayerPrefs.SetFloat(UFE.SoundsVolumeKey, volume);
		PlayerPrefs.Save();
	}

	public static void StopMusic()
	{
		if (UFE.musicAudioSource.clip != null) UFE.musicAudioSource.Stop();
	}

	public static void StopSounds()
	{
		UFE.soundsAudioSource.Stop();
	}
	#endregion

	#region Language methods
	public static void SetLanguage()
	{
		foreach (LanguageOptions languageOption in config.languages)
		{
			if (languageOption.defaultSelection)
			{
				config.selectedLanguage = languageOption;
				return;
			}
		}
	}

	public static void SetLanguage(string language)
	{
		foreach (LanguageOptions languageOption in config.languages)
		{
			if (language == languageOption.languageName)
			{
				config.selectedLanguage = languageOption;
				return;
			}
		}
	}
    #endregion

    #region Debug methods
    public static void SetDebugMode(bool flag)
	{
		UFE.config.debugOptions.debugMode = flag;
		if (debugger1 != null) debugger1.enabled = flag;
		if (debugger2 != null) debugger2.enabled = flag;
	}

	public static Text DebuggerText(GameObject debugGO, string dName, string dText, Vector2 position, TextAnchor alignment)
	{
		if (debugGO == null) return null;

		Transform debuggerTransform = UFE.canvas.transform.Find(dName);
		GameObject debugger = debuggerTransform != null ? debuggerTransform.gameObject : null;
		if (debugger == null)
			debugger = GameObject.Instantiate(debugGO);


		Text debuggerText = debugger.GetComponent<Text>();
		RectTransform rectTransform = debugger.GetComponent<RectTransform>();

		if (rectTransform == null)
		{
			Debug.LogError("Debug Error: RectTransform Component Not Found.");
			return null;
		}
		if (debugger == null)
		{
			Debug.LogError("Debug Error: Text Component Not Found.");
			return null;
		}

		debugger.transform.SetParent(UFE.canvas.transform);
		debuggerText.text = dText;
		debuggerText.alignment = alignment;
		rectTransform.anchoredPosition = position;
		rectTransform.localScale = new Vector3(1, 1, 1);

		return debuggerText;
	}
    #endregion
}