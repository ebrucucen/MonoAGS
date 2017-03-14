﻿using System.Diagnostics;
using System.Threading.Tasks;
using AGS.API;

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

		public IObject GetObject(string id, string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
            Debug.WriteLine("Getting object: " + id ?? "null");
			TypedParameter idParam = new TypedParameter (typeof(string), id);
			IObject obj = _resolver.Resolve<IObject>(idParam);

            subscribeSentences(sayWhenLook, obj.Interactions.OnInteract(AGSInteractions.LOOK));
            subscribeSentences(sayWhenInteract, obj.Interactions.OnInteract(AGSInteractions.INTERACT));

			return obj;
		}

		public ICharacter GetCharacter(string id, IOutfit outfit, string[] sayWhenLook = null, string[] sayWhenInteract = null)
		{
			ICharacter character = GetCharacter(id, outfit, _resolver.Resolve<IAnimationContainer>());

            subscribeSentences(sayWhenLook, character.Interactions.OnInteract(AGSInteractions.LOOK));
            subscribeSentences(sayWhenInteract, character.Interactions.OnInteract(AGSInteractions.INTERACT));

			return character;
		}

		public ICharacter GetCharacter(string id, IOutfit outfit, IAnimationContainer container)
		{
			TypedParameter outfitParam = new TypedParameter (typeof(IOutfit), outfit);
			TypedParameter idParam = new TypedParameter (typeof(string), id);
			TypedParameter animationParam = new TypedParameter (typeof(IAnimationContainer), container);
			ICharacter character = _resolver.Resolve<ICharacter>(outfitParam, idParam, animationParam);
			return character;
		}

		public IObject GetHotspot(string maskPath, string hotspot, string[] sayWhenLook = null, 
			string[] sayWhenInteract = null, string id = null)
		{
			IMaskLoader maskLoader = _resolver.Resolve<IMaskLoader>();
			IMask mask = maskLoader.Load(maskPath, debugDrawColor:  Colors.White, id: id ?? hotspot);
            if (mask == null) return new AGSObject(id ?? hotspot, _resolver.Resolve<Resolver>());
			setMask (mask, hotspot, sayWhenLook, sayWhenInteract);
			return mask.DebugDraw;
		}

		public async Task<IObject> GetHotspotAsync(string maskPath, string hotspot, string [] sayWhenLook = null,
			string [] sayWhenInteract = null, string id = null)
		{
			IMaskLoader maskLoader = _resolver.Resolve<IMaskLoader> ();
			IMask mask = await maskLoader.LoadAsync(maskPath, debugDrawColor: Colors.White, id: id ?? hotspot);
            if (mask == null) return new AGSObject(id ?? hotspot, _resolver.Resolve<Resolver>());
			setMask (mask, hotspot, sayWhenLook, sayWhenInteract);
			return mask.DebugDraw;
		}

		private void setMask (IMask mask, string hotspot, string [] sayWhenLook = null,
			string [] sayWhenInteract = null)
		{
			mask.DebugDraw.PixelPerfect (true);
			mask.DebugDraw.Hotspot = hotspot;
			mask.DebugDraw.Opacity = 0;
			mask.DebugDraw.Z = mask.MinY;

            subscribeSentences (sayWhenLook, mask.DebugDraw.Interactions.OnInteract(AGSInteractions.LOOK));
            subscribeSentences (sayWhenInteract, mask.DebugDraw.Interactions.OnInteract(AGSInteractions.INTERACT));
		}

		private void subscribeSentences(string[] sentences, IEvent<ObjectEventArgs> e)
		{
			if (sentences == null || e == null) return;

			e.SubscribeToAsync(async (sender, args) =>
			{
				foreach (string sentence in sentences)
				{
					await _gameState.Player.SayAsync(sentence);
				}
			});
		}
	}
}

