﻿using System;
namespace AGS.API
{
	[RequiredComponent(typeof(IWalkBehavior))]
	[RequiredComponent(typeof(IHasRoom))]
	[RequiredComponent(typeof(ITranslateComponent))]
	public interface IFollowBehavior : IComponent
	{
		void Follow(IObject obj, IFollowSettings settings = null); 
	}
}

