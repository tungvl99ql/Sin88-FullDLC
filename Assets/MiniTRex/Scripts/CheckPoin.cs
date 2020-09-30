using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CheckPoin : MonoBehaviour {


	

	public int tempPoin = 0;


	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.gameObject.CompareTag("Poin"))
		{
			tempPoin++;
		}
	}
}
