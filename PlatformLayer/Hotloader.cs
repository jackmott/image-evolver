using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace ImageEvolver
{
    public class Hotloader
    {
        dynamic logic;
        public dynamic state;

        // For gamelogic hotloading
        Assembly asm; // The current loaded gamelogic assembly
        DateTime lastUpdateDLL; // Last time the gamelogic dll file was updated        
        string solutionPath;
        string executionPath;

        //For Shader Hotloading
#if DEBUG
        ContentManager tempContent;
        DateTime lastUpdateShaders;
        //Location of mgcb executable, may be different on your system.
        string mgcbPathExe = @"C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\MGCB.exe";
#endif
        public Dictionary<string, Effect> shaders;
        ContentManager content;
        GraphicsDevice device;
        GameWindow window;
        
        public Hotloader(ContentManager content, GameWindow window, GraphicsDevice device)
        {
            //This gets the execution directory, 5 folders deep from solution
            //Adjust as necessary for your project structure
            executionPath = AppDomain.CurrentDomain.BaseDirectory;
            solutionPath = executionPath + @"..\..\..\..";
            this.content = content;
            this.device = device;
            this.window = window;

            //Load gamelogic dll
            LoadDLL();

            //Setup shader hotloading
            
            shaders = new Dictionary<string, Effect>();
#if DEBUG
            tempContent = new ContentManager(content.ServiceProvider, content.RootDirectory);
            lastUpdateShaders = DateTime.Now;
#endif
        }

        public void LoadDLL()
        {
#if DEBUG
            var path = solutionPath + @"\GameLogic\bin\x64\Debug\GameLogic.dll";
#else
            var path = solutionPath + @"\GameLogic\bin\x64\Release\GameLogic.dll";
#endif
            lastUpdateDLL = File.GetLastWriteTime(path);

            for (int i = 0; i < 10; i++) 
            {
                try
                {
                    asm = Assembly.Load(File.ReadAllBytes(path));
                    break;
                }
                catch (Exception)
                {
                    //try again
                    Thread.Sleep(100);
                }
            }
            // Find out gamelogic class in the loaded dll
            foreach (Type type in asm.GetExportedTypes())
            {
                if (type.FullName == "GameLogic.GameLogic")
                {
                    // We found our gamelogic type, set our dynamic types logic, and state
                    logic = Activator.CreateInstance(type);
                    Init();
                    // Don't set state if it already exists, we are going to keep that state                   
                    break;
                }

            }
        }

        public void OnResize()
        {
            logic.OnResize();
        }

        public void Init()
        {
            var s = logic.Init(device, window, content);
            if (state == null)
            {          
                state = s;                
            } else SetState();

        }

        public void Update(GameTime gameTime)
        {
            logic.Update(gameTime);
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            logic.Draw(batch, gameTime);
        }

             
        public void SetState()
        {

            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                var bf = new DataContractSerializer(state.GetType());
                bf.WriteObject(memoryStream, state);
                memoryStream.Position = 0;
                dynamic newState = logic.SetState(reader.ReadToEnd());

                newState.g = state.g;
                newState.w = state.w;
                newState.content = state.content;
                newState.inputState.keyboardState = state.inputState.keyboardState;
                newState.inputState.prevKeyboardState = state.inputState.prevKeyboardState;
                newState.inputState.mouseState = state.inputState.mouseState;
                newState.inputState.prevMouseState = state.inputState.prevMouseState;

                newState.evolveButton.tex = state.evolveButton.tex;
                newState.reRollButton.tex = state.reRollButton.tex;

                for (int i = 0; i < state.pictures.Count; i++)
                {
                    var newPic = newState.pictures[i];
                    var pic = state.pictures[i];

                    newPic.button.tex = pic.button.tex;
                    newPic.inject.tex = pic.inject.tex;
                    newPic.equation.tex = pic.equation.tex;

                    newPic.textBox.font = pic.textBox.font;
                    newPic.textBox.window = pic.textBox.window;
                    newPic.textBox.cursor.tex = pic.textBox.cursor.tex;
                    newPic.textBox.border.tex = pic.textBox.border.tex;

                }

                state = newState;
            }
        }

        public void CheckDLL()
        {
#if DEBUG
            var path = solutionPath + @"\GameLogic\bin\x64\Debug\GameLogic.dll";
#else
            var path = solutionPath + @"\GameLogic\bin\x64\Release\GameLogic.dll";
#endif
            var update = File.GetLastWriteTime(path);
            if (update > lastUpdateDLL)
            {
                asm = null;
                LoadDLL();                                
            }
        }



#if DEBUG
        public void CheckShaders()
        {

            var files = Directory.GetFiles(solutionPath + @"/PlatformLayer/Content", "*.fx");
            foreach (var file in files)
            {
                var t = File.GetLastWriteTime(file);
                if (t > lastUpdateShaders)
                {
                    ShaderChanged(file);
                    lastUpdateShaders = t;
                }
            }

        }

        public void ShaderChanged(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            Process pProcess = new Process
            {
                StartInfo =
                       {
                            FileName = mgcbPathExe,
                            Arguments = "/platform:Windows /config: /profile:Reach /compress:False /importer:EffectImporter /processor:EffectProcessor /processorParam:DebugMode=Auto /build:"+name+".fx",
                            CreateNoWindow = true,
                            WorkingDirectory = solutionPath+@"\PlatformLayer\Content",
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        }
            };

            //Get program output
            string stdError = null;
            StringBuilder stdOutput = new StringBuilder();
            pProcess.OutputDataReceived += (sender, args) => stdOutput.Append(args.Data);


            pProcess.Start();
            pProcess.BeginOutputReadLine();
            stdError = pProcess.StandardError.ReadToEnd();
            pProcess.WaitForExit();

            string builtPath = solutionPath + @"\PlatformLayer\Content\" + name + ".xnb";
            string movePath = executionPath + "/Content/" + name + ".xnb";
            File.Copy(builtPath, movePath, true);

            ContentManager newTemp = new ContentManager(tempContent.ServiceProvider, tempContent.RootDirectory);
            var newShaders = new Dictionary<string, Effect>();
            foreach (var shaderName in shaders.Keys)
            {
                var effect = newTemp.Load<Effect>(shaderName);
                newShaders.Add(shaderName.ToLower(), effect);
            }

            tempContent.Unload();
            tempContent.Dispose();
            tempContent = newTemp;
            shaders = newShaders;

        }
#endif

        public Effect GetShader(string name)
        {
            return shaders[name.ToLower()];
        }

        public void AddShader(string name)
        {
            if (!shaders.ContainsKey(name.ToLower()))
            {
                var shader = content.Load<Effect>(name.ToLower());
                shaders.Add(name.ToLower(), shader);
            }
        }




    }



}
