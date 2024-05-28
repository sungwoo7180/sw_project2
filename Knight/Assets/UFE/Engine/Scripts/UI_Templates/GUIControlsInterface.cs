using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class TouchControlsInterface
    {
        public CustomInputInfo UpArrow;
        public CustomInputInfo DownArrow;
        public CustomInputInfo LeftArrow;
        public CustomInputInfo RightArrow;
        public CustomInputInfo Button1;
        public CustomInputInfo Button2;
        public CustomInputInfo Button3;
        public CustomInputInfo Button4;
        public CustomInputInfo Button5;
        public CustomInputInfo Button6;
        public CustomInputInfo Pause;
    }

    [System.Serializable]
    public class CustomInputInfo
    {
        public Texture buttonImage;
        public SpriteAlignment alignment;
        public Vector2 positionOffSet;
        public float size = 1;
    }

    public class GUIControlsInterface : AbstractInputController
    {
        public int playerNum;
        public bool alwaysShow;
        [HideInInspector] public bool hideControls;
        [HideInInspector] public TouchControlsInterface touchControlsInterface;

        private Fix64 hAxis = 0;
        private Fix64 vAxis = 0;
        private bool b1;
        private bool b2;
        private bool b3;
        private bool b4;
        private bool b5;
        private bool b6;
        private bool pause;

        public Rect GetRect(CustomInputInfo inputInfo)
        {
            Rect rect = new Rect();
            rect.width = inputInfo.buttonImage.width * inputInfo.size;
            rect.height = inputInfo.buttonImage.height * inputInfo.size;

            switch (inputInfo.alignment)
            {
                case SpriteAlignment.TopLeft:
                    rect.x = 0;
                    rect.y = 0;
                    break;
                case SpriteAlignment.TopCenter:
                    rect.x = (Screen.width - rect.width) / 2;
                    rect.y = 0;
                    break;
                case SpriteAlignment.TopRight:
                    rect.x = Screen.width - rect.width;
                    rect.y = 0;
                    break;
                case SpriteAlignment.LeftCenter:
                    rect.x = 0;
                    rect.y = (Screen.height - rect.height) / 2;
                    break;
                case SpriteAlignment.Center:
                    rect.x = (Screen.width - rect.width) / 2;
                    rect.y = (Screen.height - rect.height) / 2;
                    break;
                case SpriteAlignment.RightCenter:
                    rect.x = Screen.width - rect.width;
                    rect.y = (Screen.height - rect.height) / 2;
                    break;
                case SpriteAlignment.BottomLeft:
                    rect.x = 0;
                    rect.y = Screen.height - rect.height;
                    break;
                case SpriteAlignment.BottomCenter:
                    rect.x = (Screen.width - rect.width) / 2;
                    rect.y = Screen.height - rect.height;
                    break;
                case SpriteAlignment.BottomRight:
                    rect.x = Screen.width - rect.width;
                    rect.y = Screen.height - rect.height;
                    break;
                default:
                    rect.x = 0;
                    rect.y = 0;
                    break;
            }

            rect.x += inputInfo.positionOffSet.x;
            rect.y += inputInfo.positionOffSet.y;

            return rect;
        }

        // Displays the GUI elements that triggers the inputs
        private void OnGUI()
        {
            if (hideControls || (!alwaysShow && (!UFE.gameRunning || UFE.IsPaused()))) return;

            if (touchControlsInterface.UpArrow.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.UpArrow), touchControlsInterface.UpArrow.buttonImage);
            if (touchControlsInterface.DownArrow.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.DownArrow), touchControlsInterface.DownArrow.buttonImage);
            if (touchControlsInterface.LeftArrow.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.LeftArrow), touchControlsInterface.LeftArrow.buttonImage);
            if (touchControlsInterface.RightArrow.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.RightArrow), touchControlsInterface.RightArrow.buttonImage);
            if (touchControlsInterface.Button1.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.Button1), touchControlsInterface.Button1.buttonImage);
            if (touchControlsInterface.Button2.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.Button2), touchControlsInterface.Button2.buttonImage);
            if (touchControlsInterface.Button3.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.Button3), touchControlsInterface.Button3.buttonImage);
            if (touchControlsInterface.Button4.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.Button4), touchControlsInterface.Button4.buttonImage);
            if (touchControlsInterface.Button5.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.Button5), touchControlsInterface.Button5.buttonImage);
            if (touchControlsInterface.Button6.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.Button6), touchControlsInterface.Button6.buttonImage);
            if (touchControlsInterface.Pause.buttonImage != null) GUI.DrawTexture(GetRect(touchControlsInterface.Pause), touchControlsInterface.Pause.buttonImage);

            bool upEvent = false; // Up Axis
            bool downEvent = false; // Down Axis
            bool leftEvent = false; // Left Axis
            bool rightEvent = false; // Right Axis
            bool button1Event = false; // Button 1
            bool button2Event = false; // Button 2
            bool button3Event = false; // Button 3
            bool button4Event = false; // Button 4
            bool button5Event = false; // Button 5
            bool button6Event = false; // Button 6
            bool pauseEvent = false; // Pause

            foreach(Touch touch in Input.touches)
            {
                Vector2 vec = touch.position;
                vec.y = Screen.height - vec.y; // You need to invert since GUI and screen have differnet coordinate system

                upEvent = touchControlsInterface.UpArrow.buttonImage != null && GetRect(touchControlsInterface.UpArrow).Contains(vec);
                downEvent = touchControlsInterface.DownArrow.buttonImage != null && GetRect(touchControlsInterface.DownArrow).Contains(vec);
                leftEvent = touchControlsInterface.LeftArrow.buttonImage != null && GetRect(touchControlsInterface.LeftArrow).Contains(vec);
                rightEvent = touchControlsInterface.RightArrow.buttonImage != null && GetRect(touchControlsInterface.RightArrow).Contains(vec);
                button1Event = touchControlsInterface.Button1.buttonImage != null && GetRect(touchControlsInterface.Button1).Contains(vec);
                button2Event = touchControlsInterface.Button2.buttonImage != null && GetRect(touchControlsInterface.Button2).Contains(vec);
                button3Event = touchControlsInterface.Button3.buttonImage != null && GetRect(touchControlsInterface.Button3).Contains(vec);
                button4Event = touchControlsInterface.Button4.buttonImage != null && GetRect(touchControlsInterface.Button4).Contains(vec);
                button5Event = touchControlsInterface.Button5.buttonImage != null && GetRect(touchControlsInterface.Button5).Contains(vec);
                button6Event = touchControlsInterface.Button6.buttonImage != null && GetRect(touchControlsInterface.Button6).Contains(vec);
                pauseEvent = touchControlsInterface.Pause.buttonImage != null && GetRect(touchControlsInterface.Pause).Contains(vec);
            }

            if (upEvent)
            {
                vAxis = 1;
            }
            else if (downEvent)
            {
                vAxis = -1;
            }
            else
            {
                vAxis = 0;
            }

            if (rightEvent)
            {
                hAxis = 1;
            }
            else if (leftEvent)
            {
                hAxis = -1;
            }
            else
            {
                hAxis = 0;
            }

            if (button1Event)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }

            if (button2Event)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }

            if (button3Event)
            {
                b3 = true;
            }
            else
            {
                b3 = false;
            }

            if (button4Event)
            {
                b4 = true;
            }
            else
            {
                b4 = false;
            }

            if (button5Event)
            {
                b5 = true;
            }
            else
            {
                b5 = false;
            }

            if (button6Event)
            {
                b6 = true;
            }
            else
            {
                b6 = false;
            }

            if (pauseEvent)
            {
                pause = true;
            }
            else
            {
                pause = false;
            }
        }

        // Override ReadInput so it can send the information back to UFE
        public override InputEvents ReadInput(InputReferences inputReference)
        {
            if (inputReference != null)
            {
                if (inputReference.inputType == InputType.HorizontalAxis)
                { // Sends hAxis value as a Horizontal Axis Input Event
                    return new InputEvents(hAxis);
                }
                else if (inputReference.inputType == InputType.VerticalAxis)
                { // Sends vAxis value as a Vertical Axis Input Event
                    return new InputEvents(vAxis);
                }
                else if (inputReference.inputType == InputType.Button)
                { 
                    if (inputReference.engineRelatedButton == ButtonPress.Button1)
                    { // Sends Button 1 Input Event
                        return new InputEvents(b1);
                    }
                    else if (inputReference.engineRelatedButton == ButtonPress.Button2)
                    { // Sends Button 2 Input Event
                        return new InputEvents(b2);
                    }
                    else if (inputReference.engineRelatedButton == ButtonPress.Button3)
                    { // Sends Button 3 Input Event
                        return new InputEvents(b3);
                    }
                    else if (inputReference.engineRelatedButton == ButtonPress.Button4)
                    { // Sends Button 4 Input Event
                        return new InputEvents(b4);
                    }
                    else if (inputReference.engineRelatedButton == ButtonPress.Button5)
                    { // Sends Button 5 Input Event
                        return new InputEvents(b5);
                    }
                    else if (inputReference.engineRelatedButton == ButtonPress.Button6)
                    { // Sends Button 6 Input Event
                        return new InputEvents(b6);
                    }
                    else if (inputReference.engineRelatedButton == ButtonPress.Start)
                    { // Sends Button 6 Input Event
                        return new InputEvents(pause);
                    }
                }
            }
            return InputEvents.Default;
        }
    }
}