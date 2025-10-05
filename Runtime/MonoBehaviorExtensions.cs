namespace Flexy.Core;

public static class MonoBehaviorExtensions
{
	public static		T? 			OrNull<T>	( this T obj ) where T: UnityEngine.Object => (obj) ? obj : null;
	public static		Boolean		IsAlive		( this UnityEngine.Object obj ) => obj;
	public static		void		Destroy		( this UnityEngine.Object obj ) => UnityEngine.Object.Destroy( obj );
}