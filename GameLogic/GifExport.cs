using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace GameLogic
{
    /// <summary>
    /// Store a number of frames as <see cref="Color"/> arrays. Supports exporting single
    /// frames as a png image or multiple frames as a gif using ImageSharp.
    /// </summary>
    public class FrameStore
    {
        /// <summary>
        /// Width of the frames in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the frames in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The stored frames.
        /// </summary>
        public Color[][] Frames { get; }

        private readonly Rgba32[] _rgbaBuffer;
        private int _frameIndex;

        /// <summary>
        /// Number of frames that can be stored.
        /// </summary>
        public int FrameCapacity => Frames.Length;

        /// <summary>
        /// Number of stored frames.
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// Create a new <see cref="FrameStore"/>.
        /// </summary>
        /// <param name="capacity">Number of frames to store.</param>
        /// <param name="width">Width of each frame.</param>
        /// <param name="height">Height of each frame.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   If <paramref name="capacity"/>, <paramref name="width"/> or <paramref name="height"/>
        ///   are less than or equal to zero.
        /// </exception>
        public FrameStore(int capacity, int width, int height)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be larger than zero.");
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width should be larger than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height should be larger than zero.");
            Width = width;
            Height = height;
            Frames = new Color[capacity][];
            var frameSize = width * height;
            for (var i = 0; i < capacity; i++)
                Frames[i] = new Color[frameSize];
            _rgbaBuffer = new Rgba32[frameSize];
        }

        /// <summary>
        /// Add a frame at the end of the store.
        /// </summary>
        /// <param name="frame">The frame to add.</param>
        /// <exception cref="ArgumentException">If the frame size does not match.</exception>
        public void PushFrame(Color[] frame)
        {
            if (Frames.Length < Width * Height)
                throw new ArgumentException("Frame has less pixels than expected.", nameof(Frames));
            for (var i = 0; i < Width * Height; i++)
                Frames[_frameIndex][i] = frame[i];
            _frameIndex = (_frameIndex + 1) % FrameCapacity;
            if (FrameCount < FrameCapacity)
                FrameCount++;
        }

        /// <summary>
        /// Add a frame at the end of the store.
        /// </summary>
        /// <param name="frame">The frame to add.</param>
        public void PushFrame(Texture2D frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            frame.GetData(Frames[_frameIndex]);
            _frameIndex = (_frameIndex + 1) % FrameCapacity;
            if (FrameCount < FrameCapacity)
                FrameCount++;
        }

        /// <summary>
        /// Export a frame as a PNG image.
        /// </summary>
        /// <param name="path">Path to write the image to.</param>
        /// <param name="index">Index of the frame.</param>
        public void ExportFrame(string path, int index)
        {
            using (var stream = File.OpenWrite(path))
                ExportFrame(stream, index);
        }

        /// <summary>
        /// Export a frame as a PNG image.
        /// </summary>
        /// <param name="output">Stream to write the image to.</param>
        /// <param name="index">Index of the frame.</param>
        public void ExportFrame(Stream output, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException();
            if (index >= FrameCount)
                throw new ArgumentOutOfRangeException();

            var frameIndex = (_frameIndex + index) % FrameCapacity;
            ConvertColorData(Frames[frameIndex], _rgbaBuffer);
            using (var image = Image.LoadPixelData(_rgbaBuffer, Width, Height))
                image.SaveAsPng(output);
        }

        /// <summary>
        /// Export frames from the store as a GIF.
        /// </summary>
        /// <param name="path">Path to write the GIF to.</param>
        /// <param name="frameDelay">Delay between frames in units of 10ms.</param>
        /// <param name="start">First frame to export.</param>
        /// <param name="count">Number of frames to export.</param>
        public void ExportGif(string path, int frameDelay, int start = 0, int count = -1)
        {
            using (var stream = File.OpenWrite(path))
                ExportGif(stream, frameDelay, start, count);
        }

        /// <summary>
        /// Export frames from the store as a GIF.
        /// </summary>
        /// <param name="output">Stream to write the GIF to.</param>
        /// <param name="frameDelay">Delay between frames in units of 10ms.</param>
        /// <param name="start">First frame to export.</param>
        /// <param name="count">Number of frames to export.</param>
        public void ExportGif(Stream output, int frameDelay, int start = 0, int count = -1)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException();
            if (start + count > FrameCount)
                throw new ArgumentOutOfRangeException();

            if (count < 0)
                count = FrameCapacity;

            using (var image = new Image<Rgba32>(Width, Height))
            {
                var frames = image.Frames;
                for (var i = start + 1; i <= count; i++)
                {
                    var frameIndex = (_frameIndex + i) % FrameCapacity;
                    ConvertColorData(Frames[frameIndex], _rgbaBuffer);
                    var frame = frames.AddFrame(_rgbaBuffer);
                    frame.MetaData.FrameDelay = frameDelay;
                    Transition.SetProgress(i/4);
                }

                // remove the frame created with image creation
                frames.RemoveFrame(0);
                var encoder = new GifEncoder();                
                image.SaveAsGif(output, encoder);
            }
        }

        private static void ConvertColorData(Color[] mgBuffer, Rgba32[] isBuffer)
        {
            for (var i = 0; i < mgBuffer.Length; i++)
            {
                var c = mgBuffer[i];
                isBuffer[i] = new Rgba32(c.R, c.G, c.B, c.A);
            }
        }
    }
}