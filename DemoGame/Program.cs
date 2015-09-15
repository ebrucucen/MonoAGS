﻿using System;
using AGS.Engine;
using System.Threading.Tasks;
using AGS.API;
using System.Drawing;
using System.Diagnostics;

namespace DemoGame
{
	class MainClass
	{
		[STAThread]
		public static void Main()
		{
			IGame game = AGSGame.CreateEmpty();

			game.Events.OnLoad.Subscribe((sender, e) =>
			{
				EmptyStreet emptyStreet = new EmptyStreet (game.State.Player, new AGSViewport(), game.Events);
				Rooms.EmptyStreet = emptyStreet.Load(game.Factory.Graphics);

				BrokenCurbStreet brokenCurbStreet = new BrokenCurbStreet(game.State.Player, new AGSViewport(), game.Events);
				Rooms.BrokenCurbStreet = brokenCurbStreet.Load(game.Factory.Graphics);

				TrashcanStreet trashcanStreet = new TrashcanStreet(game.State.Player, new AGSViewport(), game.Events);
				Rooms.TrashcanStreet = trashcanStreet.Load(game.Factory.Graphics);

				DarsStreet darsStreet = new DarsStreet(game.State.Player, new AGSViewport(), game.Events);
				Rooms.DarsStreet = darsStreet.Load(game.Factory.Graphics);

				game.State.Rooms.Add(Rooms.EmptyStreet);
				game.State.Rooms.Add(Rooms.BrokenCurbStreet);
				game.State.Rooms.Add(Rooms.DarsStreet);

				Cris cris = new Cris ();
				ICharacter character = cris.Load(game.Factory.Graphics);

				game.State.Player.Character = character;
				character.ChangeRoom(Rooms.EmptyStreet, 50, 30);

				TwoButtonsInputScheme inputScheme = new TwoButtonsInputScheme(game.State, game.Input, null);
				inputScheme.Start();

				addDebugLabels(game);
			});

			game.Start("Demo Game", 320, 200);
		}

		[Conditional("DEBUG")]
		private static void addDebugLabels(IGame game)
		{
			FPSCounter fps = new FPSCounter(game.Events, game.Factory.GetLabel("", 30, 15, 0, 20));
			fps.Start();

			MousePositionLabel label = new MousePositionLabel(game.Input, game.Factory.GetLabel("", 30, 15, 0, 0));
			label.Start();
		}
	}
}
