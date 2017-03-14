﻿using AGS.Engine;
using System.Threading.Tasks;
using AGS.API;
using System.Diagnostics;
using DemoQuest;

namespace DemoGame
{
	public class DemoStarter
	{
		public static void Run()
		{
			IGame game = AGSGame.CreateEmpty();

            //Rendering the text at a 4 time higher resolution than the actual game, so it will still look sharp when maximizing the window.
            GLText.TextResolutionFactorX = 4;
            GLText.TextResolutionFactorY = 4;

			game.Events.OnLoad.Subscribe(async (sender, e) =>
            {
                game.Factory.Fonts.InstallFonts("../../Assets/Fonts/pf_ronda_seven.ttf", "../../Assets/Fonts/Pixel_Berry_08_84_Ltd.Edition.TTF");
                AGSGameSettings.DefaultSpeechFont = game.Factory.Fonts.LoadFontFromPath("../../Assets/Fonts/pf_ronda_seven.ttf", 14f, FontStyle.Regular);
                AGSGameSettings.DefaultTextFont = game.Factory.Fonts.LoadFontFromPath("../../Assets/Fonts/Pixel_Berry_08_84_Ltd.Edition.TTF", 14f, FontStyle.Regular);
                AGSGameSettings.CurrentSkin = null;
                game.State.RoomTransitions.Transition = AGSRoomTransitions.Fade();
                setKeyboardEvents(game);
                Shaders.SetStandardShader();

                addDebugLabels(game);
                Debug.WriteLine("Startup: Loading Assets");
                await loadPlayerCharacter(game);
                Debug.WriteLine("Startup: Loaded Player Character");
                await loadSplashScreen(game);
            });

            game.Start(new AGSGameSettings("Demo Game", new AGS.API.Size(320, 200), 
				windowSize: new AGS.API.Size(640, 400), windowState: WindowState.Normal));
		}

        private static void setKeyboardEvents(IGame game)
        {
            game.State.Cutscene.SkipTrigger = SkipCutsceneTrigger.AnyKey;
            game.Input.KeyDown.Subscribe((sender, args) =>
            {
                if (args.Key == Key.Enter && (game.Input.IsKeyDown(Key.AltLeft) || game.Input.IsKeyDown(Key.AltRight)))
                {
                    if (game.Settings.WindowState == WindowState.FullScreen ||
                        game.Settings.WindowState == WindowState.Maximized)
                    {
                        game.Settings.WindowState = WindowState.Normal;
                        game.Settings.WindowBorder = WindowBorder.Resizable;
                    }
                    else
                    {
                        game.Settings.WindowBorder = WindowBorder.Hidden;
                        game.Settings.WindowState = WindowState.Maximized;
                    }
                }
                else if (args.Key == Key.Escape)
                {
                    if (game.State.Cutscene.IsRunning) return;
                    game.Quit();
                }
            });
        }

		private static async Task<IPanel> loadUi(IGame game)
		{
			MouseCursors cursors = new MouseCursors();
			await cursors.LoadAsync(game);
            Debug.WriteLine("Startup: Loaded Cursors");

			InventoryPanel inventory = new InventoryPanel (cursors.Scheme);
			await inventory.LoadAsync(game);
            Debug.WriteLine("Startup: Loaded Inventory Panel");

			OptionsPanel options = new OptionsPanel (cursors.Scheme);
			await options.LoadAsync(game);
            Debug.WriteLine("Startup: Loaded Options Panel");

			TopBar topBar = new TopBar(cursors.Scheme, inventory, options);
			var topPanel = await topBar.LoadAsync(game);
            Debug.WriteLine("Startup: Loaded Top Bar");

			return topPanel;
		}

        private static async Task loadPlayerCharacter(IGame game)
        { 
            Cris cris = new Cris();
            ICharacter character = await cris.LoadAsync(game);

            game.State.Player = character;
        }

		private static async Task loadCharacters(IGame game)
		{
            ICharacter character = game.State.Player;
			KeyboardMovement movement = new KeyboardMovement (character, game.Input, 
                                                              game.State.FocusedUI, KeyboardMovementMode.Pressing);
			movement.AddArrows();
			movement.AddWASD();

            InventoryItems items = new InventoryItems();
            await items.LoadAsync(game.Factory);

            Beman beman = new Beman ();
			character = await beman.LoadAsync(game);
			var room = await Rooms.BrokenCurbStreet;
			await character.ChangeRoomAsync(room, 100, 110);

			Characters.Init (game);
		}

        private static async Task loadSplashScreen(IGame game)
        { 
            AGSSplashScreen splashScreen = new AGSSplashScreen();
            Rooms.SplashScreen = splashScreen.Load(game);
            game.State.Rooms.Add(Rooms.SplashScreen);
            Rooms.SplashScreen.Events.OnAfterFadeIn.SubscribeToAsync(async (object sender, AGSEventArgs args) => 
            { 
                await loadRooms(game);
                Debug.WriteLine("Startup: Loaded Rooms");
                Task charactersLoaded = loadCharacters(game);
                var topPanel = await loadUi(game);
                Debug.WriteLine("Startup: Loaded UI");
                DefaultInteractions defaults = new DefaultInteractions(game, game.Events);
                defaults.Load();
                await charactersLoaded;
                Debug.WriteLine("Startup: Loaded Characters");
                await game.State.Player.ChangeRoomAsync(Rooms.EmptyStreet.Result, 50, 30);
                topPanel.Visible = true;
            });
            await game.State.ChangeRoomAsync(Rooms.SplashScreen);
            Debug.WriteLine("Startup: Loaded splash screen");
        }

		private static async Task loadRooms(IGame game)
		{
            Debug.WriteLine("Startup: Loading Rooms");
			EmptyStreet emptyStreet = new EmptyStreet (game.State.Player);
			Rooms.EmptyStreet = emptyStreet.LoadAsync(game);
            await waitForRoom(game, Rooms.EmptyStreet);
			//addRoomWhenLoaded(game, Rooms.EmptyStreet);
            Debug.WriteLine("Startup: Loaded empty street");

			BrokenCurbStreet brokenCurbStreet = new BrokenCurbStreet();
			Rooms.BrokenCurbStreet = brokenCurbStreet.LoadAsync(game);
            await waitForRoom(game, Rooms.BrokenCurbStreet);
			//addRoomWhenLoaded(game, Rooms.BrokenCurbStreet);
            Debug.WriteLine("Startup: Loaded broken curb street");

			TrashcanStreet trashcanStreet = new TrashcanStreet();
			Rooms.TrashcanStreet = trashcanStreet.LoadAsync(game);
            await waitForRoom(game, Rooms.TrashcanStreet);
			//addRoomWhenLoaded (game, Rooms.TrashcanStreet);
            Debug.WriteLine("Startup: Loaded trashcan street");

			DarsStreet darsStreet = new DarsStreet();
			Rooms.DarsStreet = darsStreet.LoadAsync(game);
            await waitForRoom(game, Rooms.DarsStreet);
			//addRoomWhenLoaded(game, Rooms.DarsStreet);
            Debug.WriteLine("Startup: Loaded Dars street");

			Rooms.Init(game);
            Debug.WriteLine("Startup: Initialized rooms");

			//await Rooms.DarsStreet;
		}

		private static void addRoomWhenLoaded (IGame game, Task<IRoom> task)
		{
			task.ContinueWith(room => game.State.Rooms.Add (room.Result));
		}

        private static async Task waitForRoom(IGame game, Task<IRoom> task)
        {
            var room = await task;
            game.State.Rooms.Add(room);
        }

		[Conditional("DEBUG")]
		private static void addDebugLabels(IGame game)
		{
			ILabel fpsLabel = game.Factory.UI.GetLabel("FPS Label", "", 30, 25, 320, 25, config: new AGSTextConfig(alignment: Alignment.TopLeft,
				autoFit: AutoFit.LabelShouldFitText));
			fpsLabel.Anchor = new AGS.API.PointF (1f, 0f);
			fpsLabel.ScaleBy(0.7f, 0.7f);
            fpsLabel.RenderLayer = new AGSRenderLayer(-99999);
            var red = Colors.IndianRed;
            fpsLabel.Tint = Color.FromRgba(red.R, red.G, red.B, 125);
			FPSCounter fps = new FPSCounter(game, fpsLabel);
			fps.Start();

			ILabel label = game.Factory.UI.GetLabel("Mouse Position Label", "", 30, 25, 320, 5, config: new AGSTextConfig(alignment: Alignment.TopRight,
				autoFit: AutoFit.LabelShouldFitText));
            var blue = Colors.SlateBlue;
            label.Tint = Color.FromRgba(blue.R, blue.G, blue.B, 125);
			label.Anchor = new AGS.API.PointF (1f, 0f);
			label.ScaleBy(0.7f, 0.7f);
            label.RenderLayer = new AGSRenderLayer(-99999);
            MousePositionLabel mouseLabel = new MousePositionLabel(game, label);
			mouseLabel.Start();
		}
	}
}
