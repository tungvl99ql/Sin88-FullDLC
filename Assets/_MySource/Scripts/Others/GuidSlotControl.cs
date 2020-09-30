using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GuidSlotControl : MonoBehaviour {

    public GameObject[] gojs;

    private string sceneName;

    private void OnEnable()
    {
        sceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < gojs.Length; i++)
        {
            gojs[i].SetActive(false);
        }
        ShowFirstBtn();
    }

    private void ShowFirstBtn()
    {
        gojs[0].SetActive(true);
    }

    public void OpenChangeBet()
    {
       
        gojs[0].SetActive(false);
        gojs[1].SetActive(true);
    }
    
    public void ChangeBet()
    {
        
        gojs[1].SetActive(false);
        gojs[2].SetActive(true);
    }

    public void OpenLineSellect()
    {

        gojs[2].SetActive(false);
        gojs[3].SetActive(true);
    }

    public void SellectAllLines()
    {
       
        gojs[3].SetActive(false);
        gojs[4].SetActive(true);
    }

    public void Spin()
    {
       
        gameObject.SetActive(false);
        gojs[4].SetActive(false);
        PlayerPrefs.SetInt("needGuide", 2);
    }

}
