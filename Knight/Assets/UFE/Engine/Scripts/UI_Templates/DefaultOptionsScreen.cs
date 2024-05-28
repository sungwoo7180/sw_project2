using UnityEngine;
using UnityEngine.UI;
using UFE3D;

public class DefaultOptionsScreen : OptionsScreen
{
    #region public instance properties
    public Toggle musicToggle;
    public Slider musicSlider;
    public Toggle soundToggle;
    public Slider soundSlider;
    public Slider difficultySlider;
    public Text difficultyName;
    public Text aiEngineName;
    public Toggle debugModeToggle;
    public Button changeControlsButton;
    public Button cancelButton;
    public float sliderSpeed = 0.1f;
    #endregion

    #region public override methods
    public override void OnHide()
    {
        base.OnHide();
    }

    public override void OnShow()
    {
        base.OnShow();

        if (this.musicToggle != null)
        {
            this.musicToggle.isOn = UFE.config.music;
        }

        if (this.musicSlider != null)
        {
            this.musicSlider.value = UFE.config.musicVolume;
        }

        if (this.soundToggle != null)
        {
            this.soundToggle.isOn = UFE.config.soundfx;
        }

        if (this.soundSlider != null)
        {
            this.soundSlider.value = UFE.config.soundfxVolume;
        }

        int difficultySettingsLength = UFE.config.aiOptions.difficultySettings.Length;
        AIDifficultySettings difficulty = UFE.GetAIDifficulty();

        if (this.difficultySlider != null)
        {
            this.difficultySlider.minValue = 0;
            this.difficultySlider.maxValue = difficultySettingsLength - 1;
            this.difficultySlider.wholeNumbers = true;
            this.difficultySlider.value = this.GetDifficultyIndex(difficulty);
        }

        if (this.difficultyName != null)
        {
            this.difficultyName.text = difficulty.difficultyLevel.ToString();
        }

        if (this.aiEngineName != null)
        {
            AIEngine aiEngine = UFE.GetAIEngine();

            if (aiEngine == AIEngine.RandomAI)
            {
                this.aiEngineName.text = "Random";
            }
            else
            {
                this.aiEngineName.text = "Fuzzy";
            }
        }

        if (this.debugModeToggle != null)
        {
            this.debugModeToggle.isOn = UFE.config.debugOptions.debugMode;
        }


        if (this.changeControlsButton != null)
        {
            this.changeControlsButton.gameObject.SetActive(
                (UFE.IsCInputInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.cInput) ||
                (UFE.IsRewiredInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.Rewired)
            );
        }
    }
    #endregion

    #region public instance methods
    public virtual void SetAIDifficulty(Slider slider)
    {
        if (slider != null)
        {
            this.SetAIDifficulty(UFE.config.aiOptions.difficultySettings[Mathf.RoundToInt(slider.value)]);
        }
    }

    public virtual void SetMusicVolume(Slider slider)
    {
        if (slider != null)
        {
            this.SetMusicVolume(slider.value);
        }
    }

    public virtual void SetSoundFXVolume(Slider slider)
    {
        if (slider != null)
        {
            this.SetSoundFXVolume(slider.value);
        }
    }
    #endregion

    #region public override methods
    public override void SetAIDifficulty(AIDifficultySettings difficulty)
    {
        base.SetAIDifficulty(difficulty);

        if (this.difficultySlider != null)
        {
            this.difficultySlider.value = this.GetDifficultyIndex(difficulty);
        }

        if (this.difficultyName != null)
        {
            this.difficultyName.text = difficulty.difficultyLevel.ToString();
        }
    }

    public override void SetAIEngine(AIEngine aiEngine)
    {
        base.SetAIEngine(aiEngine);

        if (this.aiEngineName != null)
        {
            aiEngine = UFE.GetAIEngine();

            if (aiEngine == AIEngine.RandomAI)
            {
                this.aiEngineName.text = "Random";
            }
            else
            {
                this.aiEngineName.text = "Fuzzy";
            }
        }
    }

    public override void SetDebugMode(bool enabled)
    {
        base.SetDebugMode(enabled);

        if (this.debugModeToggle != null)
        {
            this.debugModeToggle.isOn = UFE.config.debugOptions.debugMode;
        }
    }

    public override void SetMusic(bool enabled)
    {
        base.SetMusic(enabled);

        if (this.musicToggle != null)
        {
            this.musicToggle.isOn = !this.IsMusicMuted();
        }
    }

    public override void SetSoundFX(bool enabled)
    {
        base.SetSoundFX(enabled);

        if (this.soundToggle != null)
        {
            this.soundToggle.isOn = !this.IsSoundMuted();
        }
    }

    public override void SetMusicVolume(float volume)
    {
        base.SetMusicVolume(volume);

        if (this.musicSlider != null)
        {
            this.musicSlider.value = this.GetMusicVolume();
        }
    }

    public override void SetSoundFXVolume(float volume)
    {
        base.SetSoundFXVolume(volume);

        if (this.soundSlider != null)
        {
            this.soundSlider.value = this.GetSoundFXVolume();
        }
    }

    public override void ToggleAIEngine()
    {
        base.ToggleAIEngine();
    }

    public override void ToggleDebugMode()
    {
        if (this.debugModeToggle != null)
        {
            this.SetDebugMode(this.debugModeToggle.isOn);
        }
        else
        {
            base.ToggleDebugMode();
        }
    }

    public override void ToggleMusic()
    {
        if (this.musicToggle != null)
        {
            this.SetMusic(this.musicToggle.isOn);
        }
        else
        {
            base.ToggleMusic();
        }
    }

    public override void ToggleSoundFX()
    {
        if (this.soundToggle != null)
        {
            this.SetSoundFX(this.soundToggle.isOn);
        }
        else
        {
            base.ToggleSoundFX();
        }
    }
    #endregion

    #region protected instance methods
    protected virtual int GetDifficultyIndex(AIDifficultySettings difficulty)
    {
        AIDifficultySettings[] difficultySettings = UFE.config.aiOptions.difficultySettings;
        int count = difficultySettings.Length;

        for (int i = 0; i < count; ++i)
        {
            if (difficulty == difficultySettings[i])
            {
                return i;
            }
        }

        return -1;
    }
    #endregion
}
