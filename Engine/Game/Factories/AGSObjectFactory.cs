﻿using System;
using AGS.API;
using System.Drawing;
using Autofac;

namespace AGS.Engine
{
	public class AGSObjectFactory : IObjectFactory
	{
		private IContainer _resolver;
		private IGameState _gameState;

		public AGSObjectFactory(IContainer resolver, IGameState gameState)
		{
			_resolver = resolver;
			_gameState = gameState;
		}

		public IObject GetObject(string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			IObject obj = _resolver.Resolve<IObject>();

			subscribeSentences(sayWhenLook, obj.Interactions.OnLook);
			subscribeSentences(sayWhenInteract, obj.Interactions.OnInteract);

			return obj;
		}

		public ICharacter GetCharacter(IOutfit outfit, string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			TypedParameter outfitParam = new TypedParameter (typeof(IOutfit), outfit);
			ICharacter character = _resolver.Resolve<ICharacter>(outfitParam);

			subscribeSentences(sayWhenLook, character.Interactions.OnLook);
			subscribeSentences(sayWhenInteract, character.Interactions.OnInteract);

			return character;
		}

		public IObject GetHotspot(string maskPath, string hotspot, string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			Bitmap image = (Bitmap)Image.FromFile(maskPath); 
			return GetHotspot(image, hotspot, sayWhenLook, sayWhenInteract);
		}

		public IObject GetHotspot(Bitmap maskBitmap, string hotspot, 
			string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			IMaskLoader maskLoader = _resolver.Resolve<IMaskLoader>();
			IMask mask = maskLoader.Load(maskBitmap, debugDrawColor: Color.White);
			mask.DebugDraw.PixelPerfect(true);
			mask.DebugDraw.Hotspot = hotspot;
			mask.DebugDraw.Opacity = 0;
			mask.DebugDraw.Z = mask.MinY;

			subscribeSentences(sayWhenLook, mask.DebugDraw.Interactions.OnLook);
			subscribeSentences(sayWhenInteract, mask.DebugDraw.Interactions.OnInteract);

			return mask.DebugDraw;
		}

		private void subscribeSentences(string[] sentences, IEvent<ObjectEventArgs> e)
		{
			if (sentences == null || e == null) return;

			e.SubscribeToAsync(async (sender, args) =>
			{
				foreach (string sentence in sentences)
				{
					await _gameState.Player.Character.SayAsync(sentence);
				}
			});
		}
	}
}
