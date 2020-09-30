using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameTRexController : MonoBehaviour {


	public GameObject BG1;
	public Transform portStart, portEnd;

	public Text Score;

	private float speed = 6f;
	private bool onStartMiniOffline = false;
	private int scoreInt = 0;

	public TRexController trexController;
	public CheckPoin checkPoin;

	public void OnOpen()
    {
		this.gameObject.SetActive(true);
		onStartMiniOffline = true;
	}
	public void OnStart()
    {
		OnOpen();
	}
	
	// Update is called once per frame
	void Update () 
	{
		scoreInt = checkPoin.tempPoin;
		//if (onStartMiniOffline != true) return;
		if (trexController.onDie)
        {
			if(Input.GetMouseButtonDown(0))
            {
				trexController.onDie = false;
				checkPoin.tempPoin = 0;
				RestartGame();
            }
			return;
		}
		Score.text = scoreInt.ToString();
        MoveBG();
	}

	public void RestartGame()
    {
		BG1.transform.position = portStart.position;
	}
	public void MoveBG()
    {
		if (BG1.transform.position.x <= portEnd.position.x)
		{
			BG1.transform.position = portStart.position;
		}
		BG1.transform.position = new Vector3(BG1.transform.position.x - speed, BG1.transform.position.y, 0);
		
    }
}
