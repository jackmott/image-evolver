using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JM.LinqFaster;
using System.Windows.Forms;

namespace GameLogic
{

    public class ImageAdder
    {
        GameState state;
        Button addButton;
        Button closeButton;
        string assetPath;

        public ImageAdder(GameState state)
        {
            this.state = state;
            addButton = new Button("download-btn", Rectangle.Empty, state.buttons);
            closeButton = new Button("cancel-btn", Rectangle.Empty, state.buttons,Color.Red);
            assetPath = AppDomain.CurrentDomain.BaseDirectory + @"\Assets\";
            LoadExternalImages(state.g);
        }

        public void Update(GameTime gameTime)
        {
            ExternalImage deleteMe = 
                GameState.externalImages.FirstOrDefaultF(img => img.button.WasLeftClicked(state.inputState));                            
            if (deleteMe != null)
            {
                
                File.Delete(assetPath + deleteMe.filename);
                GameState.externalImages.Remove(deleteMe);
            }

            if (addButton.WasLeftClicked(state.inputState))
            {                
                

                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {                    
                    openFileDialog.Filter = "Image Files(*.JPG;*.JPEG;*.PNG)|*.JPG;*.JPEG;*.PNG";
                    openFileDialog.FilterIndex = 0;
                    openFileDialog.RestoreDirectory = false;
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                       var filePath = openFileDialog.FileName;
                       var name = filePath.Substring(filePath.LastIndexOf("\\"));
                       File.Copy(filePath, assetPath + name);
                        LoadExternalImages(state.g);                        
                    }
                }                
            }

            if (closeButton.WasLeftClicked(state.inputState))
            {
                state.screen = Screen.CHOOSE;
            }
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            int h = (int)(state.g.Viewport.Height * 0.75f);
            int w = (int)(state.g.Viewport.Width * 0.5f);
            var back = GraphUtils.GetTexture(state.g, new Color(0.0f, 0.0f, 0.0f, 0.85f));
            var bounds = GraphUtils.CenteredRect(state.g.Viewport.Bounds, w, h);
            batch.Draw(back, bounds, Color.White);
            Vector2 pos = new Vector2(bounds.X + bounds.Width * .01f, bounds.Y);
            var fontHeight = Settings.otherFont.MeasureString("A").Y;
            int yPlus = (int)(fontHeight * 1.1f);

            foreach (var img in GameState.externalImages)
            {
                img.button.bounds = GraphUtils.FRect(bounds.X + bounds.Width - fontHeight, pos.Y, fontHeight, fontHeight);
                img.button.Draw(batch, state.g, gameTime);
                batch.DrawString(Settings.otherFont, img.filename, pos, Color.White);
                pos.Y += yPlus;
            }
            addButton.bounds = GraphUtils.FRect(pos.X, pos.Y, addButton.GetWidth() / 2, addButton.GetHeight() / 2);
            addButton.Draw(batch, state.g, gameTime);

            var closeScale = 4;
            closeButton.bounds = new Rectangle(bounds.X + bounds.Width - closeButton.GetWidth() / closeScale, bounds.Y + bounds.Height - closeButton.GetHeight() / closeScale, closeButton.GetWidth() / closeScale, closeButton.GetHeight() / closeScale);
            closeButton.Draw(batch, state.g, gameTime);
        }

        public IEnumerable<FileInfo> GetAllExternalImageFiles()
        {
            DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Assets");
            return d.GetFiles("*.jpg").AsEnumerable().Concat(d.GetFiles("*.png")).Concat(d.GetFiles("*.jpeg")).OrderBy(f => f.Name);
        }

        public void LoadExternalImages(GraphicsDevice g)
        {
            
            GameState.externalImages = new List<ExternalImage>();

            foreach (var file in GetAllExternalImageFiles())
            {
                try
                {
                    Button button = new Button("cancel-btn", Rectangle.Empty, state.buttons, Color.Red);
                    var fs = new FileStream(file.FullName, FileMode.Open);
                    var tex = Texture2D.FromStream(g, fs);
                    fs.Close();
                    Color[] colors = new Color[tex.Width * tex.Height];
                    tex.GetData(colors);
                    ExternalImage img = new ExternalImage { button=button, filename = file.Name, data = colors, w = tex.Width, h = tex.Height };
                    GameState.externalImages.Add(img);
                    tex.Dispose();
                }
                catch (Exception e)
                {
                    //do something
                    throw e;
                }
            }
        }
    }
}
