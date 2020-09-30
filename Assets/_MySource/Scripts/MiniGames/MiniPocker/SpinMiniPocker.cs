using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Core.Server.Api;
public delegate void OpenShowValue(InBoundMessage res);
public class SpinMiniPocker : MonoBehaviour {
    public RectTransform[] Colum1;
    public RectTransform[] Colum2;
    public RectTransform[] Colum3;
    public RectTransform[] Colum4;
    public RectTransform[] Colum5;
    public Animator spinAnimator;
    public UnityEvent startSpin;
    public OpenShowValue openShowValue;
    public Sprite[] sprts;
    public bool spin;
    public bool isPlaying;
    public float speed;
    private int i = -1;
    private List<int> index;
    private void Update()
    {
        if (spin)
        {
            MoveCardFinish();
            if (Colum1[i].anchoredPosition.y <= 0)
            {
                if (i - 2 < 0)
                {
                    Colum1[i - 2 + 5].anchoredPosition = new Vector2(Colum1[0].anchoredPosition.x, 180);
                    Colum2[i - 2 + 5].anchoredPosition = new Vector2(Colum2[0].anchoredPosition.x, 180);
                    Colum3[i - 2 + 5].anchoredPosition = new Vector2(Colum3[0].anchoredPosition.x, 180);
                    Colum4[i - 2 + 5].anchoredPosition = new Vector2(Colum4[0].anchoredPosition.x, 180);
                    Colum5[i - 2 + 5].anchoredPosition = new Vector2(Colum5[0].anchoredPosition.x, 180);
                }
                else
                {
                    Colum1[i - 2].anchoredPosition = new Vector2(Colum1[0].anchoredPosition.x, 180);
                    Colum2[i - 2].anchoredPosition = new Vector2(Colum2[0].anchoredPosition.x, 180);
                    Colum3[i - 2].anchoredPosition = new Vector2(Colum3[0].anchoredPosition.x, 180);
                    Colum4[i - 2].anchoredPosition = new Vector2(Colum4[0].anchoredPosition.x, 180);
                    Colum5[i - 2].anchoredPosition = new Vector2(Colum5[0].anchoredPosition.x, 180);
                }
                if (i >=0 && i<=5)
                    i++;               
                if (i == 5)
                    i = 0;

            }
        }
        else
        {
            if (i > -1)
            {
                MoveCardFinish();
                if (Colum1[i].anchoredPosition.y <= 0)
                {
                    if (i - 2 < 0)
                    {
                        Colum1[i - 2 + 5].anchoredPosition = new Vector2(Colum1[0].anchoredPosition.x, 180);
                        Colum2[i - 2 + 5].anchoredPosition = new Vector2(Colum2[0].anchoredPosition.x, 180);
                        Colum3[i - 2 + 5].anchoredPosition = new Vector2(Colum3[0].anchoredPosition.x, 180);
                        Colum4[i - 2 + 5].anchoredPosition = new Vector2(Colum4[0].anchoredPosition.x, 180);
                        Colum5[i - 2 + 5].anchoredPosition = new Vector2(Colum5[0].anchoredPosition.x, 180);
                    }
                    else
                    {
                        Colum1[i - 2].anchoredPosition = new Vector2(Colum1[0].anchoredPosition.x, 180);
                        Colum2[i - 2].anchoredPosition = new Vector2(Colum2[0].anchoredPosition.x, 180);
                        Colum3[i - 2].anchoredPosition = new Vector2(Colum3[0].anchoredPosition.x, 180);
                        Colum4[i - 2].anchoredPosition = new Vector2(Colum4[0].anchoredPosition.x, 180);
                        Colum5[i - 2].anchoredPosition = new Vector2(Colum5[0].anchoredPosition.x, 180);
                    }
                    if (i >= 0 && i <= 3)
                    {
                        MoveCardResponeServer(i + 1);
                    }
                    if (i == 4)
                    {
                        MoveCardResponeServer(0);
                    }
                   
                }
            }
        }
    }


    private void MoveCardFinish()
    {
        Colum1[i].SetAsLastSibling();
        Colum2[i].SetAsLastSibling();
        Colum3[i].SetAsLastSibling();
        Colum4[i].SetAsLastSibling();
        Colum5[i].SetAsLastSibling();
        Colum1[i].GetComponent<Image>().sprite = sprts[(int)Random.Range(0, 51)];
        Colum2[i].GetComponent<Image>().sprite = sprts[(int)Random.Range(0, 51)];
        Colum3[i].GetComponent<Image>().sprite = sprts[(int)Random.Range(0, 51)];
        Colum4[i].GetComponent<Image>().sprite = sprts[(int)Random.Range(0, 51)];
        Colum5[i].GetComponent<Image>().sprite = sprts[(int)Random.Range(0, 51)];
        Colum1[i].anchoredPosition = Vector3.MoveTowards(Colum1[i].anchoredPosition, new Vector2(Colum1[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum2[i].anchoredPosition = Vector3.MoveTowards(Colum2[i].anchoredPosition, new Vector2(Colum2[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum3[i].anchoredPosition = Vector3.MoveTowards(Colum3[i].anchoredPosition, new Vector2(Colum3[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum4[i].anchoredPosition = Vector3.MoveTowards(Colum4[i].anchoredPosition, new Vector2(Colum4[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum5[i].anchoredPosition = Vector3.MoveTowards(Colum5[i].anchoredPosition, new Vector2(Colum5[0].anchoredPosition.x, 0), Time.deltaTime * speed);
    }

    private void MoveCardResponeServer(int id)
    {
        Colum1[id].GetComponent<Image>().sprite = sprts[index == null ? 1 :index[0]];
        Colum2[id].GetComponent<Image>().sprite = sprts[index == null ? 20 : index[1]];
        Colum3[id].GetComponent<Image>().sprite = sprts[index == null ? 32 : index[2]];
        Colum4[id].GetComponent<Image>().sprite = sprts[index == null ? 44 : index[3]];
        Colum5[id].GetComponent<Image>().sprite = sprts[index == null ? 51 : index[4]];
        Colum1[id].SetAsLastSibling();
        Colum2[id].SetAsLastSibling();
        Colum3[id].SetAsLastSibling();
        Colum4[id].SetAsLastSibling();
        Colum5[id].SetAsLastSibling();
        Colum1[id].anchoredPosition = Vector3.MoveTowards(Colum1[id].anchoredPosition, new Vector2(Colum1[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum2[id].anchoredPosition = Vector3.MoveTowards(Colum2[id].anchoredPosition, new Vector2(Colum2[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum3[id].anchoredPosition = Vector3.MoveTowards(Colum3[id].anchoredPosition, new Vector2(Colum3[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum4[id].anchoredPosition = Vector3.MoveTowards(Colum4[id].anchoredPosition, new Vector2(Colum4[0].anchoredPosition.x, 0), Time.deltaTime * speed);
        Colum5[id].anchoredPosition = Vector3.MoveTowards(Colum5[id].anchoredPosition, new Vector2(Colum5[0].anchoredPosition.x, 0), Time.deltaTime * speed);
    }

    public void Spin()
    {
        if (isPlaying)
            return;
        isPlaying = true;
        spinAnimator.SetBool("spin", true);
        spin = true;
        if (startSpin != null)
            startSpin.Invoke();
        i++;
        if (i == 5)
            i = 0;
    }

    public void StopSpin(List<int> ids,InBoundMessage res)
    {
        index = new List<int>();
        index = ids;
        spin = false;
        StartCoroutine(ShowValue(res));
    }
    public void StopSpinEnoughMoney()
    {
        spin = false;
    }
    private IEnumerator ShowValue(InBoundMessage res)
    {
        yield return new WaitForSeconds(.5f);
        if (openShowValue != null)
            openShowValue.Invoke(res);
    }

   
}
