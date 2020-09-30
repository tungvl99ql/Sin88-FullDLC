using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRexController : MonoBehaviour {

	public Rigidbody2D rigidbody;
	public Animator animator;
	public Transform port;

	public bool onDie = false;

	public bool jump = true;
	void Start () 
	{
		animator = this.gameObject.GetComponent<Animator>();
		rigidbody = this.gameObject.GetComponent<Rigidbody2D>();
		
	}

	// Update is called once per frame

	public void Jump()
    {
		var tempY = this.gameObject.transform.position.y;


		if(jump == true && onDie != true)
        {
			jump = false;
			this.transform.DOMoveY(port.position.y, 0.5f).OnComplete(()=> {
				this.transform.DOMoveY(tempY, 0.5f).OnComplete(()=> { jump = true; }
				);
				
			});
			//rigidbody.AddForce(new Vector2(0, 25000));
			
        }
    }


	void OnCollisionEnter2D(Collision2D collider)
    {
		if(collider.gameObject.CompareTag("Ground"))
        {
			jump = true;
        }
		
    }

	void OnTriggerEnter2D(Collider2D collider)
    {
		if (collider.gameObject.CompareTag("Poin"))
		{
			onDie = true;
		}
	}




}
