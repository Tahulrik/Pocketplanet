using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction{

	public class Selectable : MonoBehaviour {
		
		protected Animator anim;
		
		protected Transform OriginalParent;
		public Transform CurrentParent{ get; set;}
		
		public Selectable SelectionMethods;
		
		protected RectTransform ObjectTransform;
		protected Vector2 OriginalAnchor;
		
		protected Vector2 SelectionAnchor;
		
		protected GameObject canvas;
		
		protected bool pickedUp;
		
		// Use this for initialization
		void Start () {
			
			
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}
		
		public virtual void Initialize()
		{
			ObjectTransform = gameObject.GetComponent<RectTransform> ();
			OriginalAnchor = ObjectTransform.pivot;
			SelectionAnchor = new Vector2 (0.5f, 0.5f);
			
			SelectionMethods = gameObject.GetComponent<Selectable> ();
			OriginalParent = gameObject.transform.parent;
			canvas = gameObject.transform.root.gameObject;
			
			CurrentParent = gameObject.transform.parent;
		}
		
		public virtual void MakeActiveObject()
		{
			
		}
		
		public virtual void DeselectObject()
		{
			EndSelection ();
		}
		
		public void StartSelection()
		{
			anim = gameObject.GetComponentInChildren<Animator> ();
			anim.SetTrigger ("Clicked");
			anim.SetBool ("Holding", true);
		}
		
		public void EndSelection()
		{
			anim = gameObject.GetComponentInChildren<Animator> ();
			
			anim.SetBool ("Holding", false);
			
		}
		
		Transform GetOriginalParent()
		{
			return OriginalParent;
		}
		
	}
	
	
}