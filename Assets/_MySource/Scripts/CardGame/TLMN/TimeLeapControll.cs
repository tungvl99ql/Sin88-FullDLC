using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeLeapControll : MonoBehaviour {

    public static bool run = false;
    public static int time = 30;
    private Image img;
    void Awake()
    {
        img = GetComponent<Image>();
    }

	// Update is called once per frame
	void Update () {
        if (run)
        {
            img.fillAmount = Time.deltaTime / time;
        }
	}
}
