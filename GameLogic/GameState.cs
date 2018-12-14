﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace GameLogic
{
    [DataContract]
    public struct ExternalImage
    {
        [DataMember]
        public Color[] data;
        [DataMember]
        public int w;
        [DataMember]
        public int h;
    }

    public enum Screen { CHOOSE, ZOOM };
    [DataContract]
    public class GameState
    {
        
        public GraphicsDevice g;        
        public GameWindow w;
        [DataMember]
        public InputState inputState;        
        public ContentManager content;
        [DataMember]
        public Button evolveButton;
        [DataMember]
        public Button reRollButton;
        [DataMember]
        public Screen screen = Screen.CHOOSE;
        [DataMember]
        public Random r;
        [DataMember]
        public int populationSize;
        [DataMember]
        public List<Pic> pictures;
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