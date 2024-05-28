using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
    public class OnlineModeAfterBattleScreen : UFEScreen
    {
        public Button RepeatBattleButton;
        public Button CharacterSelectionButton;

        public Color highlightColor = Color.green;

        #region protected enum definitions
        protected enum Option
        {
            RepeatBattle = 0,
            CharacterSelectionScreen = 1,
            SearchNewMatch = 2,
            MainMenu = 3,
        }
        #endregion

        protected Option mySelection = Option.MainMenu;
        protected Option opSelection = Option.MainMenu;


        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            UFE.MultiplayerAPI.OnDisconnection += this.OnPlayerDisconnection;
        }

        public override void OnHide()
        {
            base.OnHide();
            UFE.MultiplayerAPI.OnDisconnection -= this.OnPlayerDisconnection;

        }
        #endregion

        protected void OnPlayerDisconnection()
        {
            UFE.MultiplayerAPI.OnDisconnection -= this.OnPlayerDisconnection;

            DisableMenuOption(RepeatBattleButton);
            DisableMenuOption(CharacterSelectionButton);
        }

        #region public instance methods
        public virtual void GoToCharacterSelectionScreen()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.CharacterSelectionScreen, UFE.GetLocalPlayer(), true);
        }

        public virtual void GoToMainMenu()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.MainMenu, UFE.GetLocalPlayer(), true);
        }

        public virtual void GoToSearchMatchScreen()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.SearchNewMatch, UFE.GetLocalPlayer(), true);
        }

        public virtual void RepeatBattle()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.RepeatBattle, UFE.GetLocalPlayer(), true);
        }

        public virtual void TrySelectOption(int option, int player, bool broadcast)
        {
            if (!broadcast || !UFE.IsConnected)
            {
                this.SelectOption(option, player);
            }
            else
            {
                // We don't invoke the SelectOption() method immediately because we are using the frame-delay 
                // algorithm to keep players synchronized, so we can't invoke the SelectOption() method
                // until the other player has received the message with our choice.
                UFE.FluxCapacitor.RequestOptionSelection(player, (sbyte)option);
            }
        }

        // Use this event to deactivate a button
        public virtual void DisableMenuOption(Button button)
        {
            button.GetComponent<Button>().interactable = false;
        }

        // Use this event to highlight a button
        public virtual void HighlightMenuOption(Button button)
        {
            button.GetComponent<Image>().color = highlightColor;
        }
        #endregion


        #region public override methods
        public override void SelectOption(int option, int player)
        {
            OnlineModeAfterBattleScreen.Option selectedOption = (OnlineModeAfterBattleScreen.Option)option;
            if (selectedOption == OnlineModeAfterBattleScreen.Option.CharacterSelectionScreen)
            {
                HighlightMenuOption(CharacterSelectionButton);

                if (UFE.GetLocalPlayer() == player)
                {
                    mySelection = selectedOption;
                }
                else
                {
                    opSelection = selectedOption;
                    DisableMenuOption(RepeatBattleButton);
                }

                if (UFE.GetLocalPlayer() == player)
                {
                    DisableMenuOption(RepeatBattleButton);
                    DisableMenuOption(CharacterSelectionButton);
                }

                if ((mySelection == Option.CharacterSelectionScreen && (opSelection == Option.CharacterSelectionScreen || opSelection == Option.RepeatBattle)) ||
                    (opSelection == Option.CharacterSelectionScreen && (mySelection == Option.CharacterSelectionScreen || mySelection == Option.RepeatBattle)))
                {
                    UFE.EndGame();

                    UFE.StartCharacterSelectionScreen();
                    UFE.PauseGame(false);
                }
            }
            else if (selectedOption == OnlineModeAfterBattleScreen.Option.MainMenu)
            {
                if (UFE.GetLocalPlayer() == player)
                {
                    UFE.EndGame();

                    UFE.MultiplayerAPI.LeaveMatch();

                    UFE.StartMainMenuScreen();
                    UFE.PauseGame(false);
                }
                else
                {
                    DisableMenuOption(RepeatBattleButton);
                    DisableMenuOption(CharacterSelectionButton);
                }
            }
            else if (selectedOption == OnlineModeAfterBattleScreen.Option.SearchNewMatch)
            {
                if (UFE.GetLocalPlayer() == player)
                {
                    UFE.EndGame();

                    UFE.MultiplayerAPI.LeaveMatch();

                    UFE.StartSearchMatchScreen();
                    UFE.PauseGame(false);
                }
                else
                {
                    DisableMenuOption(RepeatBattleButton);
                    DisableMenuOption(CharacterSelectionButton);
                }
            }
            else if (selectedOption == OnlineModeAfterBattleScreen.Option.RepeatBattle)
            {
                HighlightMenuOption(RepeatBattleButton);

                if (UFE.GetLocalPlayer() == player)
                {
                    mySelection = selectedOption;
                }
                else
                {
                    opSelection = selectedOption;
                }

                if (UFE.GetLocalPlayer() == player)
                {
                    DisableMenuOption(RepeatBattleButton);
                    DisableMenuOption(CharacterSelectionButton);
                }

                if ((mySelection == Option.RepeatBattle && (opSelection == Option.CharacterSelectionScreen || opSelection == Option.RepeatBattle)) ||
                    (opSelection == Option.RepeatBattle && (mySelection == Option.CharacterSelectionScreen || mySelection == Option.RepeatBattle)))
                {
                    UFE.EndGame();

                    if (opSelection == Option.CharacterSelectionScreen || mySelection == Option.CharacterSelectionScreen)
                    {
                        UFE.StartCharacterSelectionScreen();
                    }
                    else
                    {
                        UFE.StartLoadingBattleScreen();
                    }
                    UFE.PauseGame(false);
                }
            }
        }
        #endregion
    }
}