using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GameLogic
{
    [DataContract]
    public struct ExternalImage
    {
        [DataMember]
        public string filename;
        [DataMember]
        public Color[] data;
        [DataMember]
        public int w;
        [DataMember]
        public int h;
    }

    public enum Screen { CHOOSE, ZOOM, EDIT,VIDEO_PLAYING,VIDEO_GENERATING };
    [DataContract]
    public class GameState
    {
        
        public GraphicsDevice g;        
        public GameWindow w;
        [DataMember]
        public bool videoMode = false;
        [DataMember]
        public InputState inputState;        
        public ContentManager content;
        [DataMember]
        public Button evolveButton;
        [DataMember]
        public Button reRollButton;
        [DataMember]
        public Button undoButton;
        [DataMember]
        public ToggleButton videoModeButton;
        [DataMember]
        public Screen screen = Screen.CHOOSE;
        [DataMember]
        public Random r;
        [DataMember]
        public int populationSize;
        [DataMember]
        public Pic[] prevPictures;
        [DataMember]
        public Pic[] pictures;
        [DataMember]
        public Pic zoomedPic;
        [DataMember]
        public static List<ExternalImage> externalImages;

        public GameState() { }

    }

    [DataContract]
    public class InputState
    {        
        [DataMember]
        public KeyboardState keyboardState;
        [DataMember]
        public KeyboardState prevKeyboardState;
        [DataMember]
        public MouseState mouseState;
        [DataMember]
        public MouseState prevMouseState;
        [DataMember]
        public int keyboardStateMillis;

        public InputState() { }
    }
      
}
