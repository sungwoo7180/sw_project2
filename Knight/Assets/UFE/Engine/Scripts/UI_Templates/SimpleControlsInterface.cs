using UnityEngine;
using UFE3D;

[RequireComponent(typeof(GUIControlsInterface))]
public class SimpleControlsInterface : MonoBehaviour
{
    [Tooltip("If disabled, will only be displayed when the match is in progress and the game is not paused")]
    /// <summary>
    /// If disabled, will only be displayed when the match is in progress and the game is not paused
    /// </summary>
    public bool alwaysShow;

    [Tooltip("If enabled, start the controller hidden (must press show controls button)")]
    /// <summary>
    /// If enabled, start the controller hidden (must press show controls button)
    /// </summary>
    public bool startHidden;

    [Tooltip("If enabled, spawn controls for both players")]
    /// <summary>
    /// If enabled, spawn controls for both players
    /// </summary>
    public bool enablePlayer2;

    public CustomInputInfo ShowControlsButton = new CustomInputInfo();

    public TouchControlsInterface player1TouchInterface = new TouchControlsInterface();
    public TouchControlsInterface player2TouchInterface = new TouchControlsInterface();
    private AbstractInputController backUpController1;
    private AbstractInputController backUpController2;
    private GUIControlsInterface uiInterface1;
    private GUIControlsInterface uiInterface2;

    private bool hidden;

    void Start()
    {
        SetupGUIInterface(1, ref uiInterface1, ref backUpController1, player1TouchInterface);

        if (enablePlayer2)
        {
            SetupGUIInterface(2, ref uiInterface2, ref backUpController2, player2TouchInterface, true);
        }

        if (startHidden) 
            HideControls();
        else
            ShowControls();
    }

    void SetupGUIInterface(int playerNum, ref GUIControlsInterface uiInterface, ref AbstractInputController backupController, TouchControlsInterface touchControlsInterface, bool addComponent = false)
    {
        if (addComponent)
            uiInterface = gameObject.AddComponent<GUIControlsInterface>();
        else
            uiInterface = gameObject.GetComponent<GUIControlsInterface>();

        uiInterface.touchControlsInterface = touchControlsInterface;
        uiInterface.playerNum = playerNum;

        uiInterface.alwaysShow = alwaysShow;
        uiInterface.hideControls = startHidden;

        backupController = UFE.GetController(playerNum).humanController;
    }

    void HideControls()
    {
        hidden = true;
        uiInterface1.hideControls = true;
        UFE.GetController(1).humanController = backUpController1;

        if (uiInterface2 == null) return;
        uiInterface2.hideControls = true;
        UFE.GetController(2).humanController = backUpController2;
    }

    void ShowControls()
    {
        hidden = false;
        uiInterface1.hideControls = false;
        UFE.GetController(1).humanController = uiInterface1;

        if (uiInterface2 == null) return;
        uiInterface2.hideControls = false;
        UFE.GetController(2).humanController = uiInterface2;
    }

    // Displays the GUI elements that triggers the inputs
    private void OnGUI()
    {
        if (!alwaysShow && (!UFE.gameRunning || UFE.IsPaused())) return;
        if (ShowControlsButton.buttonImage == null || uiInterface1 == null) return;

        if (GUI.Button(uiInterface1.GetRect(ShowControlsButton), ShowControlsButton.buttonImage))
        {
            if (hidden)
            {
                ShowControls();
            }
            else
            {
                hidden = true;
                HideControls();
            }
        }
    }
}
