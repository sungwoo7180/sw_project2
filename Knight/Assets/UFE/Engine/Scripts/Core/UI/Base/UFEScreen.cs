using UnityEngine;
using System.Collections.Generic;

namespace UFE3D
{
    public class UFEScreen : MonoBehaviour
    {
        public bool highlightFirstOption = true;
        public bool canvasPreview = true;
        public GameObject firstSelectableGameObject = null;
        public bool hasFadeIn = true;
        public bool hasFadeOut = true;
        public bool wrapInput = true;
        public bool stopPreviousSoundEffectsOnLoad = false;
        public float delayBeforePlayingMusic = 0.1f;

        public AudioClip onLoadSound;
        public AudioClip music;
        public AudioClip selectSound;
        public AudioClip cancelSound;
        public AudioClip moveCursorSound;


        /// <summary>DoFixedUpdate is called every frame.</summary>
        public virtual void DoFixedUpdate(
            IDictionary<InputReferences, InputEvents> player1PreviousInputs,
            IDictionary<InputReferences, InputEvents> player1CurrentInputs,
            IDictionary<InputReferences, InputEvents> player2PreviousInputs,
            IDictionary<InputReferences, InputEvents> player2CurrentInputs
        )
        {
            this.DefaultNavigationSystem(
                player1PreviousInputs,
                player1CurrentInputs,
                player2PreviousInputs,
                player2CurrentInputs,
                this.moveCursorSound,
                this.selectSound,
                this.cancelSound
            );
        }


        /// <summary>Use GoToCustomScreen to load custom interfaces from Global Editor -> GUI.</summary>
        /// <param name="screenId">The screen id (starting point = 0).</param>
        public virtual void GoToCustomScreen(int screenId)
        {
            UFE.StartCustomScreen(screenId);
        }


        /// <summary>Is this screen visibile in the hierarchy?</summary>
        public virtual bool IsVisible()
        {
            return this.gameObject.activeInHierarchy;
        }


        /// <summary>OnHide is called when the screen is destroyed or replaced.</summary>
        public virtual void OnHide() { }


        /// <summary>OnShow is called when this screen is instantiated.</summary>
        public virtual void OnShow()
        {
            if (highlightFirstOption) this.HighlightOption(this.FindFirstSelectable());

            if (this.music != null)
            {
                UFE.DelayLocalAction(delegate () { UFE.PlayMusic(this.music); }, this.delayBeforePlayingMusic);
            }

            if (this.stopPreviousSoundEffectsOnLoad)
            {
                UFE.StopSounds();
            }

            if (this.onLoadSound != null)
            {
                UFE.DelayLocalAction(delegate () { UFE.PlaySound(this.onLoadSound); }, this.delayBeforePlayingMusic);
            }
        }


        /// <summary>Select a menu option.</summary>
        /// <param name="option">The option id in the grid.</param>
        /// <param name="player">The player who selected it (1 or 2).</param>
        public virtual void SelectOption(int option, int player) { }
    }
}