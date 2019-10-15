//	(c) Jean Fabre, 2011-2013 All rights reserved.
//	http://www.fabrejean.net
//
// ArrayListGetLineRendererPositions.cs v1.0.0 by holyfingers: http://hutonggames.com/playmakerforum/index.php?topic=16079.0
//
// Based on ArrayListGetVertexPositions.cs created by LampRabbit: http://hutonggames.com/playmakerforum/index.php?topic=3982.msg18550#msg18550
//

using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/ArrayList")]
	[Tooltip("Store mesh vertex positions into an arrayList")]
	public class ArrayListGetLineRendererPositions : ArrayListActions
	{

		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker ArrayList Proxy component")]
		[CheckForComponent(typeof(PlayMakerArrayListProxy))]
		public FsmOwnerDefault gameObject;

		[Tooltip("Author defined Reference of the PlayMaker ArrayList Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;

		[Tooltip("The gameObject with the Line Renderer attached")]
		[CheckForComponent(typeof(LineRenderer))]
		public FsmGameObject lineObject;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		public override void Reset()

		{
			gameObject = null;
			reference = null;
			lineObject = null;
			everyFrame = false;
		}

		public override void OnEnter()

		{
			if ( SetUpArrayListProxyPointer(Fsm.GetOwnerDefaultTarget(gameObject),reference.Value) )
				getVertexPositions();

			if (!everyFrame)

			{
				Finish();
			}
		}

		public override void OnUpdate()

		{
			if ( SetUpArrayListProxyPointer(Fsm.GetOwnerDefaultTarget(gameObject),reference.Value) )
				getVertexPositions();

		}



		public void getVertexPositions()

		{
			if (! isProxyValid()) 
				return;

			proxy.arrayList.Clear();


			GameObject _go = lineObject.Value;

			if (_go==null)
			{
				return;
			}


			LineRenderer _lineObject = _go.GetComponent<LineRenderer>();

			if (_lineObject == null)
			{
				return;
			}

			int length = _lineObject.positionCount;
			int i=0;

			while (i < length) {

				Vector3 _point = _lineObject.GetPosition (i);

				proxy.arrayList.Add (_point);

				i++;


			}

		}

	}
}