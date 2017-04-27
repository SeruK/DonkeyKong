using UnityEngine;
using UE = UnityEngine;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public static class GizmosHelper
{
	[Conditional("UNITY_EDITOR")]
	public static void Scope( Action action )
	{
		if(action == null) { return; }

		var oldMatrix = Gizmos.matrix;
		var color = Gizmos.color;

		action();

		Gizmos.matrix = oldMatrix;
		Gizmos.color = color;
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawRect(Rect r, float z)
	{
		DrawRect(
			bl: new Vector3(r.x, r.y, z),
			br: new Vector3(r.x + r.width, r.y, z),
			tl: new Vector3(r.x, r.y + r.height, z),
			tr: new Vector3(r.x + r.width, r.y + r.height, z)
		);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawRect(Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr)
	{
		Gizmos.DrawLine(bl, br);
		Gizmos.DrawLine(br, tr);
		Gizmos.DrawLine(tr, tl);
		Gizmos.DrawLine(tl, bl);
	}
}
