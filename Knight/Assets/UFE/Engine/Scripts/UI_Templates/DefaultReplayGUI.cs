using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFE3D;

public class DefaultReplayGUI : ReplayMode
{
    private void OnGUI()
    {
        if (enableControls && UFE.gameRunning)
        {
            if (enableStateTrackerControls)
            {
                if (GUI.Button(new Rect(10, 10, 160, 40), "Save State"))
                    SaveState();

                if (GUI.Button(new Rect(10, 60, 160, 40), "Load State"))
                    LoadState();
            }

            if (enableRecordingControls)
            {
                Rect headerLabel = new Rect(10, Screen.height - 130, 140, 25);
                Rect line0Label1 = new Rect(10, Screen.height - 110, 60, 25);
                Rect line0Label2 = new Rect(80, Screen.height - 110, 60, 25);
                Rect line1Label = new Rect(10, Screen.height - 80, 80, 25);
                Rect line1Input = new Rect(100, Screen.height - 80, 70, 25);
                Rect line2Btn1 = new Rect(10, Screen.height - 50, 50, 40);
                Rect line2Btn2 = new Rect(70, Screen.height - 50, 50, 40);
                Rect line2Btn3 = new Rect(130, Screen.height - 50, 50, 40);
                Rect line2Btn4 = new Rect(190, Screen.height - 50, 50, 40);
                Rect line2Slider = new Rect(250, Screen.height - 25, Screen.width - 280, 40);

                if (isRecording)
                {
                    if (GUI.Button(line2Btn1, "Stop"))
                        StopRecording();
                }
                else
                {
                    if (enablePlayback && isPlayback)
                    {
                        if (UFE.config.networkOptions.synchronizationAction == NetworkSynchronizationAction.PlaybackTool 
                            && UFE.config.networkOptions.generateVariableLog 
                            && GUI.Button(headerLabel, "Generate Log"))
                            UFE.FluxCapacitor.CreateLogFile(UFE.currentFrame);

                        if (track2.Count > 0)
                        {
                            if (GUI.Button(line0Label1, "Rec 1"))
                                SetTrack(1);

                            if (GUI.Button(line0Label2, "Rec 2"))
                                SetTrack(2);
                        }

                        GUI.Box(line1Label, "Frame:");
                        GUI.Box(line1Input, (currentStartingFrame + currentRecFrame).ToString());

                        if (enablePlayerControl && GUI.Button(line2Btn1, "Back"))
                            Stop();

                        if (isPlaying)
                        {
                            if (GUI.Button(line2Btn2, "Pause"))
                                Pause();
                        }
                        else
                        {
                            if (GUI.Button(line2Btn2, "Play"))
                                Play();

                            if (GUI.Button(line2Btn3, "<<"))
                                MoveBack();

                            if (GUI.Button(line2Btn4, ">>"))
                                MoveForward();
                        }

                        GUI.enabled = !isPlaying;
                        int newFrame = currentRecFrame;
                        newFrame = (int)GUI.HorizontalSlider(line2Slider, currentRecFrame, 0, recordedFrames.Count - 1);
                        if (newFrame != currentRecFrame)
                        {
                            SetFrame(newFrame);
                        }

                        GUI.enabled = true;
                    }
                    else
                    {
                        if (enableRecording)
                        {
                            string maxBufferText = maxBuffer.ToString();
                            GUI.Box(line1Label, "Buffer Size");
                            maxBufferText = GUI.TextField(new Rect(95, Screen.height - 80, 55, 25), maxBufferText, 6);
                            maxBufferText = System.Text.RegularExpressions.Regex.Replace(maxBufferText, "[^0-9]", "");
                            int.TryParse(maxBufferText, out maxBuffer);

                            if (GUI.Button(line2Btn1, "Rec"))
                                StartRecording();
                        }
                        if (enablePlayback && recordedFrames.Count > 0 && GUI.Button(line2Btn2, "Replay"))
                        {
                            Play(true);
                        }
                    }
                }
            }
        }
    }
}
