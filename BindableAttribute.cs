﻿namespace Flexy.Core
{
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Method )]
	public class BindableAttribute: UnityEngine.Scripting.PreserveAttribute
	{
		public	String		Description;
		public	Boolean		IsWarning;
	}
}