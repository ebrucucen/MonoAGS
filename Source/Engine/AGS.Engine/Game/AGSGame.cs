﻿using System;
using AGS.API;
using Autofac;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reflection;

namespace AGS.Engine
{
	public class AGSGame : IGame
	{
		private Resolver _resolver;
		private IRendererLoop _renderLoop;
		private int _relativeSpeed;
		private AGSEventArgs _renderEventArgs;
		private readonly IMessagePump _messagePump;
        private readonly IGraphicsBackend _graphics;
        private readonly IGLUtils _glUtils;
		public const double UPDATE_RATE = 60.0;

        public AGSGame(IGameState state, IGameEvents gameEvents, IMessagePump messagePump, 
                       IGraphicsBackend graphics, IGLUtils glUtils)
		{
			_messagePump = messagePump;
			_messagePump.SetSyncContext ();
			State = state;
			Events = gameEvents;
			_relativeSpeed = state.Speed;
			_renderEventArgs = new AGSEventArgs ();
            _graphics = graphics;
            _glUtils = glUtils;
            GLUtils = _glUtils;
		}

		public static IGameWindow GameWindow { get; private set; }

		public static IGame Game { get; private set; }

        public static IDevice Device { get; set; }

		public static IShader Shader { get; set; }

		public static Resolver Resolver { get { return ((AGSGame)Game)._resolver; } }

        public static IGLUtils GLUtils { get; private set; }

		public static IGame CreateEmpty()
		{
            if (Game != null) return Game;
			UIThreadID = Environment.CurrentManagedThreadId;

			printRuntime();
			Resolver resolver = new Resolver(Device);
			resolver.Build();
			AGSGame game = resolver.Container.Resolve<AGSGame>();
			game._resolver = resolver;
			Game = game;
			return game;
		}

        public static int UIThreadID;

		#region IGame implementation

		public IGameFactory Factory { get; private set; }

		public IGameState State { get; private set; } 

		public IGameLoop GameLoop { get; private set; } 

		public ISaveLoad SaveLoad { get; private set; } 

		public IInput Input { get; private set; } 

		public IGameEvents Events { get; private set; }

		public IAudioSettings AudioSettings { get; private set; }

        public IRuntimeSettings Settings { get; private set; }

        public void Start(IGameSettings settings)
		{
			GameLoop = _resolver.Container.Resolve<IGameLoop>(new TypedParameter (typeof(AGS.API.Size), settings.VirtualResolution));
            TypedParameter settingsParameter = new TypedParameter(typeof(IGameSettings), settings);

            try { GameWindow = Resolver.Container.Resolve<IGameWindow>(settingsParameter); }
            catch (Exception ese) 
            {
                Debug.WriteLine(ese.ToString());
                throw;
            }

            //using (GameWindow)
			{
                try
                {
                    TypedParameter gameWindowParameter = new TypedParameter(typeof(IGameWindow), GameWindow);
                    GameWindow.Load += (sender, e) =>
                    {
                        Settings = Resolver.Container.Resolve<IRuntimeSettings>(settingsParameter, gameWindowParameter);

                        _graphics.ClearColor(0f, 0f, 0f, 1f);

                        _graphics.Init();
                        _glUtils.GenBuffers();

                        Factory = Resolver.Container.Resolve<IGameFactory>();

                        TypedParameter sizeParameter = new TypedParameter(typeof(AGS.API.Size), Settings.VirtualResolution);
                        Input = _resolver.Container.Resolve<IInput>(gameWindowParameter, sizeParameter);
                        TypedParameter inputParamater = new TypedParameter(typeof(IInput), Input);
                        TypedParameter gameParameter = new TypedParameter(typeof(IGame), this);
                        _renderLoop = _resolver.Container.Resolve<IRendererLoop>(inputParamater, gameParameter);
                        updateResolver();
                        AudioSettings = _resolver.Container.Resolve<IAudioSettings>();
                        SaveLoad = _resolver.Container.Resolve<ISaveLoad>();

                        _glUtils.AdjustResolution(settings.VirtualResolution.Width, settings.VirtualResolution.Height);

                        Events.OnLoad.Invoke(sender, new AGSEventArgs());
                    };

                    GameWindow.Resize += async (sender, e) =>
                    {
                        await Task.Delay(10); //todo: For some reason on the Mac, the GL Viewport assignment is overridden without this delay (so aspect ratio is not preserved), a bug in OpenTK?
                        resize();
                        Events.OnScreenResize.Invoke(sender, new AGSEventArgs());
                    };

                    GameWindow.UpdateFrame += async (sender, e) =>
                    {
                        try
                        {
                            _messagePump.PumpMessages();
                            if (State.Paused) return;
                            adjustSpeed();
                            await GameLoop.UpdateAsync().ConfigureAwait(false);
                            AGSEventArgs args = new AGSEventArgs();

                            //Invoking repeatedly execute asynchronously, as if one subscriber is waiting on another subscriber the event will 
                            //never get to it (for example: calling ChangeRoom from within RepeatedlyExecute calls StopWalking which 
                            //waits for the walk to stop, only the walk also happens on RepeatedlyExecute and we'll hang.
                            //Since we're running asynchronously, the next UpdateFrame will call RepeatedlyExecute for the walk cycle to stop itself and we're good.
                            ///The downside of this approach is that we need to look out for re-entrancy issues.
                            await Events.OnRepeatedlyExecute.InvokeAsync(sender, args);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            throw ex;
                        }
                    };

                    GameWindow.RenderFrame += (sender, e) =>
                    {
                        if (_renderLoop == null) return;
                        try
                        {
                            // render graphics
                            _graphics.ClearScreen();
                            Events.OnBeforeRender.Invoke(sender, _renderEventArgs);

                            if (_renderLoop.Tick())
                            {
                                GameWindow.SwapBuffers();
                            }
                            if (Repeat.OnceOnly("SetFirstRestart"))
                            {
                                SaveLoad.SetRestartPoint();
                            }
                        }
                        catch (Exception ex)
    					{
    						Debug.WriteLine("Exception when rendering:");
    						Debug.WriteLine(ex.ToString());
    						throw;
    					}
    				};

				// Run the game at 60 updates per second
				GameWindow.Run(UPDATE_RATE);
                } catch (Exception exx)
                {
                    Debug.WriteLine(exx.ToString());
                    throw;
                }
			}
		}

		public void Quit()
		{
			GameWindow.Exit();
		}

		public TEntity Find<TEntity>(string id) where TEntity : class, IEntity
		{
			return State.Find<TEntity>(id);
		}

        #endregion

        private void resize()
        {
            var settings = Settings;
            if (settings != null) Settings.ResetViewport();
        }

		private void updateResolver()
		{
			var updater = new ContainerBuilder ();
			updater.RegisterInstance(Input).As<IInput>();
			updater.RegisterInstance(_renderLoop).As<IRendererLoop>();
			updater.RegisterInstance(this).As<IGame>();
            updater.RegisterInstance(Settings).As<IGameSettings>();
            updater.RegisterInstance(Settings).As<IRuntimeSettings>();

			updater.Update(_resolver.Container);
		}

		void adjustSpeed()
		{
			if (_relativeSpeed == State.Speed) return;

			_relativeSpeed = State.Speed;
			GameWindow.TargetUpdateFrequency = UPDATE_RATE * (_relativeSpeed / 100f);
		}

		private static void printRuntime()
		{
			Type type = Type.GetType("Mono.Runtime");
			if (type != null)
			{                                          
				MethodInfo getDisplayName = type.GetRuntimeMethod("GetDisplayName", new Type[]{}); 
				if (getDisplayName != null)
				{
					object displayName = getDisplayName.Invoke(null, null);
					Debug.WriteLine(string.Format("Runtime: Mono- {0}", displayName)); 
				}
			}
		}
	}
}

