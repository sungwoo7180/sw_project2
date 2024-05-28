﻿using UnityEngine;
using System.Collections.Generic;
using UFE3D;

public class DefaultPauseScreen : PauseScreen
{
    #region public instance fields
    public UFEScreen backToMenuConfirmationDialog;
    public UFEScreen[] screens;
    #endregion

    #region protected instance fields
    protected int currentScreen = -1;
    protected bool confirmationDialogVisible = false;
    #endregion

    #region public instance methods
    public virtual void HideBackToMenuConfirmationDialog()
    {
        this.HideBackToMenuConfirmationDialog(true);
    }

    public virtual void HideBackToMenuConfirmationDialog(bool triggerOnShowScreenEvent)
    {
        if (this.backToMenuConfirmationDialog != null)
        {
            for (int i = 0; i < this.screens.Length; ++i)
            {
                if (this.screens[i] != null)
                {
                    CanvasGroup canvasGroup = this.screens[i].GetComponent<CanvasGroup>();

                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = true;
                    }
                }
            }

            this.HideScreen(this.backToMenuConfirmationDialog);
            this.confirmationDialogVisible = false;

            if (triggerOnShowScreenEvent && currentScreen >= 0)
            {
                this.ShowScreen(this.screens[this.currentScreen]);
            }
        }
    }

    public virtual void GoToScreen(int index)
    {
        for (int i = 0; i < this.screens.Length; ++i)
        {
            if (i != index)
            {
                this.HideScreen(this.screens[i]);
            }
            else
            {
                this.ShowScreen(this.screens[i]);
            }
        }

        this.currentScreen = index;
    }

    public virtual void ShowBackToMenuConfirmationDialog()
    {
        if (this.backToMenuConfirmationDialog != null)
        {
            for (int i = 0; i < this.screens.Length; ++i)
            {
                if (this.screens[i] != null)
                {
                    CanvasGroup canvasGroup = this.screens[i].GetComponent<CanvasGroup>();

                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = false;
                    }
                    else
                    {
                        this.HideScreen(this.screens[i]);
                    }
                }
            }

            this.ShowScreen(this.backToMenuConfirmationDialog);
            this.confirmationDialogVisible = true;
        }
    }
    #endregion

    #region public override methods
    public override void DoFixedUpdate(
        IDictionary<InputReferences, InputEvents> player1PreviousInputs,
        IDictionary<InputReferences, InputEvents> player1CurrentInputs,
        IDictionary<InputReferences, InputEvents> player2PreviousInputs,
        IDictionary<InputReferences, InputEvents> player2CurrentInputs
    )
    {
        if (!this.confirmationDialogVisible && currentScreen == -1)
        {
            base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
        }
    }

    public override void OnShow()
    {
        base.OnShow();

        this.confirmationDialogVisible = false;
        this.HideBackToMenuConfirmationDialog(false);
        if (this.screens.Length > 0)
        {
            this.GoToScreen(0);
        }
    }

    public override void OnHide()
    {
        base.OnHide();

        this.confirmationDialogVisible = false;
        this.HideBackToMenuConfirmationDialog(false);
        if (this.currentScreen >= 0 && this.currentScreen < this.screens.Length)
        {
            this.HideScreen(this.screens[this.currentScreen]);
        }
    }

    public override void SelectOption(int option, int player)
    {
        if (this.currentScreen >= 0 && this.currentScreen < this.screens.Length && this.screens[this.currentScreen] != null)
        {
            this.screens[this.currentScreen].SelectOption(option, player);
        }
    }
    #endregion

    #region protected instance methods
    protected virtual void HideScreen(UFEScreen screen)
    {
        if (screen != null)
        {
            screen.OnHide();
            screen.gameObject.SetActive(false);
        }
        currentScreen = -1;
    }

    protected virtual bool IsVisible(UFEScreen screen)
    {
        return screen != null && screen.IsVisible();
    }

    protected virtual void ShowScreen(UFEScreen screen)
    {
        if (screen != null)
        {
            screen.gameObject.SetActive(true);
            screen.OnShow();
        }
    }
    #endregion
}
