﻿using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using FPLibrary;
using UFE3D;

public class VideoIntroScreen : IntroScreen {
    #region public class properties
    public static readonly string Data = "%Data%";
    public static readonly string Persistent = "%Persistent%";
    public static readonly string StreamingAssets = "%StreamingAssets%";
    public static readonly string Temp = "%Temp%";
    #endregion

    #region public instance properties
    public VideoRenderMode videoRenderMode;
    public VideoClip videoClip;
    // The name of the video file in the StreamingAssets folder
    public bool loadFromUrl = false;
    public string pathOrUrl = "file://" + VideoIntroScreen.StreamingAssets + "/video.ogv";
    public bool skippable = true;
    public float delayBeforePlayingVideo = 0.05f;
    public float delayAfterSkippingVideo = 0.05f;
    #endregion

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;

    public override void OnShow()
    {
        base.OnShow();

        this.transform.parent = null;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = Vector3.one;
        UFE.DelayLocalAction(LoadMovie, delayBeforePlayingVideo);
    }

    public void LoadMovie()
    {
        videoPlayer = this.GetComponent<VideoPlayer>();
        if (videoPlayer == null) videoPlayer = this.gameObject.AddComponent<VideoPlayer>();

        audioSource = this.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = this.gameObject.AddComponent<AudioSource>();


        if (loadFromUrl)
        {
            string url = this.pathOrUrl
                    .Replace(VideoIntroScreen.Data, Application.dataPath)
                    .Replace(VideoIntroScreen.Persistent, Application.persistentDataPath)
                    .Replace(VideoIntroScreen.StreamingAssets, Application.streamingAssetsPath)
                    .Replace(VideoIntroScreen.Temp, Application.temporaryCachePath);

            videoPlayer.url = url;
        }
        else
        {
            videoPlayer.clip = videoClip;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = videoRenderMode;
        videoPlayer.targetCamera = Camera.main;
        videoPlayer.targetMaterialRenderer = GetComponent<Renderer>();
        videoPlayer.targetMaterialProperty = "_MainTex";
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.loopPointReached -= CheckOver;
        videoPlayer.loopPointReached += CheckOver;

        if (!UFE.GetSoundFX())
            videoPlayer.SetDirectAudioMute(0, true);
    }

    void CheckOver(VideoPlayer vp)
    {
        videoPlayer.loopPointReached -= CheckOver;
        videoPlayer.Stop();
        videoPlayer = null;
        UFE.DelayLocalAction(this.GoToMainMenu, delayAfterSkippingVideo);
    }

    public void Update()
    {
        if (videoPlayer != null)
        {
            if (skippable && Input.anyKeyDown)
            {
                CheckOver(videoPlayer);
            }
            else
            {
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        CheckOver(videoPlayer);
                    }
                }
            }
        }
    }
}
