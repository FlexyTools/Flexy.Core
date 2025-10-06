using System.Linq;
using UnityEngine.Serialization;

namespace Flexy.Core.Actions
{
	[Serializable]
	public class ActionSet: FlexyActionAsync
	{
		[SerializeField]		ERunType		Run;
		[SerializeReference]	FlexyAction[]?	Actions;
		
		public override async UniTask DoAsync	( ActionCtx ctx )
		{
			if (Actions == null)
				return;
			
			switch (Run)
			{
				case ERunType.Sequential:
				{
					foreach ( var act in Actions )
						try						{ await act.DoAsync( ctx ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
					
					break;
				}

				case ERunType.Simultanously:
				{
					try						{ await UniTask.WhenAll( Enumerable.Select(Actions, a => a.DoAsync( ctx ) ).ToArray( ) ); }
					catch ( Exception ex )	{ Debug.LogException( ex ); }
					break;
				}

				default: throw new ArgumentOutOfRangeException();
			}
		}

		private enum ERunType 
		{
			Sequential,
			Simultanously,
		}
	}
}