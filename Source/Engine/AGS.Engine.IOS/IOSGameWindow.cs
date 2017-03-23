﻿extern alias IOS;

using System;
using System.Diagnostics;
using AGS.API;
using Autofac;
using OpenTK;
using OpenTK.Graphics;
using IOS::UIKit;
using IOS::CoreGraphics;
using System.Threading.Tasks;

namespace AGS.Engine.IOS
{
    public class IOSGameWindow : IGameWindow
    {
        private IOSGameView _view;
        private bool _started;
        private double _updateRate;
        private Lazy<Size> _size;

        public static IOSGameWindow Instance = new IOSGameWindow();

        private IOSGameWindow()
        {
            Debug.WriteLine("IOS Game Window Constructor");
            Resolver.Override(resolver => resolver.Builder.RegisterInstance(this).As<IGameWindow>());
            Resolver.Override(resolver => resolver.Builder.RegisterType<IOSGestures>().SingleInstance());
            Resolver.Override(resolver => resolver.Builder.RegisterType<IOSInput>().SingleInstance().As<IInput>());
        }

        public IOSGameView View
        {
            get { return _view; }
            set
            {
                _view = value;
                _size = new Lazy<Size>(() => new Size((int)(View.Size.Width * View.ContentScaleFactor),
                                                      (int)(View.Size.Height * View.ContentScaleFactor)));
                var onNewView = OnNewView;
                if (onNewView != null) onNewView(this, value);
            }
        }

        public event EventHandler<IOSGameView> OnNewView;

        public Action StartGame { get; set; }

        public int ClientHeight { get { return _size.Value.Height; } }

        public int ClientWidth { get { return _size.Value.Width; } }

        public int Height { get { return _size.Value.Height; } }

        public int Width { get { return _size.Value.Width; } }

        public double TargetUpdateFrequency { get { return 60f; } set { } } //todo
        public VsyncMode Vsync { get { return VsyncMode.Off; } set { } } //todo
        public string Title { get { return ""; } set { } } //todo

        public API.WindowBorder WindowBorder { get { return (API.WindowBorder)View.WindowBorder; } set { View.WindowBorder = (OpenTK.WindowBorder)value; } }
        public API.WindowState WindowState { get { return (API.WindowState)View.WindowState; } set { View.WindowState = (OpenTK.WindowState)value; } }

        public event EventHandler<EventArgs> Load;
        public event EventHandler<FrameEventArgs> RenderFrame;
        public event EventHandler<EventArgs> Resize;
        public event EventHandler<FrameEventArgs> UpdateFrame;

        public void OnLoad(EventArgs args)
        {
            if (!_started)
            {
                _started = true;
                AGSEngineIOS.Init();
                StartGame();
                var onLoad = Load;
                if (onLoad != null)
                {
                    onLoad(this, args);
                }
            }
            else View.Run(_updateRate);
        }

        public void OnResize(CGSize size)
        {
            View.ResizeFrameBuffer();
            float width = (float)size.Width;
            float height = (float)size.Height;
            _size = new Lazy<Size>(() => new Size((int)(width * View.ContentScaleFactor),
                                                  (int)(height * View.ContentScaleFactor)));
            var onResize = Resize;
            if (onResize != null) onResize(this, new EventArgs());
        }

        public void OnRenderFrame(FrameEventArgs args)
        {
            RenderFrame(this, args);
        }

        public void OnUpdateFrame(FrameEventArgs args)
        {
            UpdateFrame(this, args);
        }

        public void Dispose()
        {
            View.Dispose();
        }

        public void Exit()
        {
            //https://developer.apple.com/library/content/qa/qa1561/_index.html
            throw new InvalidOperationException("It's against IOS guidelines to have a Quit button!!");
        }

        public void Run(double updateRate)
        {
            _updateRate = updateRate;
            View.Run(updateRate);
        }

        public void SetSize(Size size) { }

        public void SwapBuffers()
        {
            View.SwapBuffers();
        }
    }
}
