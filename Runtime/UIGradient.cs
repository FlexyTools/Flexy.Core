using Unity.Mathematics;
using UnityEngine.UI;

namespace Flexy.Core
{
	public class UIGradient : BaseMeshEffect 
	{
		[SerializeField]	Color		_topLeft 		= Color.white;
		[SerializeField]	Color		_topRight 		= Color.white;
		[SerializeField]	Color		_bottomRight 	= Color.white;
		[SerializeField]	Color		_bottomLeft 	= Color.white;
		[Range(-180f, 180f)]
		[SerializeField]	Single		_angle			= 0f;
		[SerializeField]	Boolean		_ignoreRatio	= true;

		public override		void		ModifyMesh		( VertexHelper vh )		
		{
			if (!enabled) 
				return;
			
			var rect	= graphic.rectTransform.rect;
			var dir		= RotationDir( _angle * -1 );

			if (!_ignoreRatio)
			{
				var ratio = rect.height / rect.width;
				dir.x *= ratio;
				dir = math.normalize( dir );
			}
			
			var rotationMatrix = new float2x2( dir.xy, dir.yx );

			var vertex = default(UIVertex);
			for (var i = vh.currentVertCount - 1; i >= 0; i--) 
			{
				vh.PopulateUIVertex	( ref vertex, i );

				var localPosition	= math.mul( rotationMatrix, Normalize(rect, vertex.position) );
				vertex.color		*= QuadLerp(_topLeft, _topRight, _bottomLeft, _bottomRight, localPosition / 2f + 0.5f);
				
				vh.SetUIVertex		( vertex, i );
			}
		}
		
		private static		float2		RotationDir		( Single angle )										
		{
			var angleRad = angle * Mathf.Deg2Rad;
			return new Vector2( math.cos( angleRad ), math.sin( angleRad ) );
		}
		private static		float2		Normalize		( in Rect rect, float3 vertexPos )						
		{
			var pos = vertexPos.xy;
			pos.x = (pos.x - rect.xMin)/rect.width * 2 - 1;
			pos.y = (pos.y - rect.yMin)/rect.height * 2 - 1;
			pos.y *= -1;
			
			return pos;
		}
		private static		Color		QuadLerp		( Color tl, Color tr, Color bl, Color br, Vector2 t )	
		{
			var a =	Color.LerpUnclamped( tl, tr, t.x );
			var b =	Color.LerpUnclamped( bl, br, t.x );
			return	Color.LerpUnclamped( a, b, t.y );
		}
	}
}