//	(c) Jean Fabre, 2011-2013 All rights reserved.
//	http://www.fabrejean.net

// INSTRUCTIONS
// Drop a PlayMakerArrayList script onto a GameObject, and define a unique name for reference if several PlayMakerArrayList coexists on that GameObject.
// In this Action interface, link that GameObject in "arrayListObject" and input the reference name if defined. 
// Note: You can directly reference that GameObject or store it in an Fsm variable or global Fsm variable

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("ArrayMaker/HashTable")]
	[Tooltip("Gets the type from a key on a PlayMaker HashTable Proxy component")]
	public class HashTableGetType : HashTableActions
	{
		[ActionSection("Set up")]
		
		[RequiredField]
		[Tooltip("The gameObject with the PlayMaker HashTable Proxy component")]
		[CheckForComponent(typeof(PlayMakerHashTableProxy))]
		public FsmOwnerDefault gameObject;
		
		[Tooltip("Author defined Reference of the PlayMaker HashTable Proxy component ( necessary if several component coexists on the same GameObject")]
		public FsmString reference;
		
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("The Key value for that hash set")]
		public FsmString key;
		
		[ActionSection("Result")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[ObjectType(typeof(PlayMakerCollectionProxy.VariableEnum))]
		public FsmEnum type;
		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is found")]
		public FsmEvent KeyFoundEvent;
		
		[UIHint(UIHint.FsmEvent)]
		[Tooltip("The event to trigger when key is not found")]
		public FsmEvent KeyNotFoundEvent;
		

		public override void Reset()
		{
			
			gameObject = null;
			key = null;
			
			KeyFoundEvent = null;
			KeyNotFoundEvent = null;
			
			type = null;
			
			
		}

		public override void OnEnter()
		{
			if ( SetUpHashTableProxyPointer(Fsm.GetOwnerDefaultTarget(gameObject),reference.Value) )
				DoGetType();

			Finish();
		}
		
		public void DoGetType(){
			
			if (! isProxyValid())
			{
				return;
			}
			
			if (!proxy.hashTable.ContainsKey(key.Value))
			{
				Fsm.Event(KeyNotFoundEvent);
				return;
			}
			
			if (proxy.hashTable[key.Value] == null)
			{
				type.Value = proxy.preFillType;
			}else{
				type.Value = PlayMakerCollectionProxy.GetObjectVariableType(proxy.hashTable[key.Value]);
			}

			Fsm.Event(KeyFoundEvent);

		}
	}
}