using Core.Bird;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;
using DG.Tweening;

namespace Core.Bird
{
    public class ChangerBird : MonoBehaviour
    {

        public Sprite[] spriteReference;
        public BirdGamePlay gameplay;
        public GameObject[] listSew;
        public GameObject[] listBird;
        public UnityEvent StartRollEvent;
        public UnityEvent SendRequestEvent;
        public UnityEvent EndRollEvent;
        public GameObject panel;

        public RectTransform[] possition;
        public SelectBet selectBet;
        public DrawLineEffect draw;
        public GameObject Parent;
        public GameObject Children;
        private String[] states = { "active", "unactive" };
        private Material materialSkeletonSpine;

        private IEnumerator finishCoroutine;

        /*
    * 0  3  6  9  12
    * 1  4  7  10 13
    * 2  5  8  11 14
    */
        private int[,] rowArrray = {
            {1,4,7,10,13 }, //Row 1
            {0,3,6,9,12 },  //Row 2
            {2,5,8,11,14 },  //Row 3
            {1,4,6,10,13 },  //Row 4
            {1,4,8,10,13 },  //Row 5
            {0,3,7,9,12 },  //Row 6
            {2,5,7,11,14 },  //Row 7
            {0,5,6,11,12},  //Row 8
            {2,3,8,9,14 },  //Row 9
            {1,3,8,9,13 },  //Row 10
            {2,4,6,10,14 },  //Row 11
            {0,4,8,10,12 },  //Row 12
            {1,5,7,9,13 }, //Row 13
            {1,3,7,11,13 }, //Row 14
            {2,4,7,10,14}, //Row 15
            {0,4,7,10,12 }, //Row 16
            {1,5,8,11,13 }, //Row 17
            {1,3,6,9,13 }, //Row 18
            {2,5,7,9,12}, //Row 19
            {0,3,7,11,14 } }; //Row 20
        private List<int> respond = new List<int>();
        private List<int> listwin = new List<int>();
        private List<GameObject> listLight = new List<GameObject>();
        public GameObject rem;
        private void Awake()
        {
            materialSkeletonSpine = Resources.Load("Material/SkeletonGraphicDefault", typeof(Material)) as Material;

            int leght = listBird.Length;
            for (int i = 0; i < leght; i++)
            {

                //    listBird[i].GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
                // listSew[i].GetComponent<SkeletonGraphic>().material = materialSkeletonSpine;
                listSew[i].SetActive(false);
            }
        }


        private void Update()
        {
        }
        void OnEnable()
        {
            selectBet.CloseSelectBetEvent.AddListener(OnCloseSelectBet);
        }

        private void OnCloseSelectBet()
        {
            Init();
        }

        void OnDisable()
        {
            selectBet.CloseSelectBetEvent.RemoveListener(OnCloseSelectBet);
        }
        private void ControllRem()
        {

        }

        float open = 755f;
        float che = 0f;
        float time = 1f;
        public void Play()
        {


            IsPlaying = true;


            rem.transform.DOLocalMoveY(IsPlaying ? che : open, time, false).OnComplete(() => {

                for (int i = 0; i < listBird.Length; i++)
                {


                    // listSew[i].SetActive(true);
                    // listSew[i].GetComponentInChildren<SkeletonGraphic>().AnimationState.SetAnimation(0, states[0], true);
                    // listSew[i].GetComponentInChildren<SkeletonGraphic>().timeScale = 2;

                }
                for (int j = 0; j < listBird.Length; j++)
                {
                    listBird[j].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                }
                SendRequest();
            });



        }
        private void SendRequest()
        {
            if (finishCoroutine != null)
                StopCoroutine(finishCoroutine);
            DeleteLight();


            if (SendRequestEvent != null)
            {
                SendRequestEvent.Invoke();

            }
        }


        public void OnSendServerRespond(object obj)
        {

            StartCoroutine(ServerRespond(obj));

            // panel.SetActive(false);

        }


        private IEnumerator ServerRespond(object obj)
        {
            respond.Clear();
            listwin.Clear();
            Hashtable res = (Hashtable)obj;
            var countItems = (int)res["count"];
            int[] listitem = (int[])res["listItem"];
            var coutPrize = (int)res["coutPrize"];
            var listPrize = (BirdSlotPrize[])res["listPrize"];
            var xStartMiniGame = (int)res["xStartMiniGame"];
            yield return new WaitForSeconds(0.1f);
            List<int> listsew = new List<int>();
            List<int> listsew1 = new List<int>();

            for (int i = 0; i < listitem.Length; i++)
            {
                // listBird[i].transform.GetComponent<SkeletonGraphic>().enabled = true;
                //  listBird[i].GetComponent<SkeletonGraphic>().sk;

                listBird[i].GetComponent<Image>().overrideSprite = spriteReference[listitem[i] - 1];
                //
                //   listBird[i].transform.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "unactive", true);

                //listBird[i].GetComponent<SkeletonGraphic>().Initialize(true);
                //listBird[i].GetComponent<SkeletonGraphic>();
                respond.Add(i);


                // Debug.Log(listitem[i] + "  " + spriteReference[listitem[i] - 1].name);
            }
            //  Debug.Log("+++++++++++++++++++++++++++++++++++++++");

            rem.transform.DOLocalMoveY(false ? che : open, time / 4, false).OnComplete(() =>
            {

            });
            if (coutPrize > 0)
            {
                gameplay.SetPanelTranfer(true);
                foreach (BirdSlotPrize x in listPrize)
                {
                    List<int> pos = new List<int>();

                    bool ok = true;
                    for (int i = 0; i < listitem.Length; i++)
                    {
                        for (int j = 0; j < rowArrray.GetLength(1); j++)
                        {
                            if (listitem[rowArrray[x.row, j]] == x.item)
                            {
                                listBird[rowArrray[x.row, j]].GetComponent<Image>().overrideSprite = spriteReference[x.item - 1];
                                //  listBird[rowArrray[x.row, j]].transform.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "active", true);

                                //  StartCoroutine(Sew(i));
                                //   respond.Remove(i);
                                if (ok)
                                {
                                    ok = false;
                                    //    Debug.Log("Row= " + x.row + " ItemRow=   " + listitem[rowArrray[x.row, j]] + " Items=   " + listitem[i] + " Name=  " + spriteReference[listitem[i] - 1].name + " quanty= " + x.quantity + " x tiem = " + x.item);

                                }
                            }
                            pos.Add(rowArrray[x.row, j]);
                        }


                    }


                    pos = pos.Distinct().ToList();
                    //DeleteLight();

                    System.Random rd = new System.Random();
                    id = rd.Next(0, 8);
                    while (id == idColor)
                        id = rd.Next(0, 8);
                    idColor = id;

                    if (pos.Count > 0)
                    {
                        GameObject gobj = Instantiate(Children, Parent.transform);
                        gobj.name = x.row.ToString() + " " + x.item;
                        if (x.item == listitem[pos[0]])
                        {
                            listwin.Add(pos[0]);
                            respond.Remove(pos[0]);
                        }
                        for (int i = 1; i < pos.Count; i++)
                        {
                            if (x.item == listitem[pos[i]])
                            {
                                listwin.Add(pos[i]);
                                respond.Remove(pos[i]);
                            }
                            // if(!gameplay.gameInfo.isTurnOnAuto)
                            draw.DrawLine(possition[pos[i - 1]], possition[pos[i]], gobj, RamdomColor(id));
                        }
                        gobj.SetActive(false);
                        //if (gameplay.gameInfo.isTurnOnAuto)
                        // gobj.SetActive(true);
                        listLight.Add(gobj);
                    }



                }

                listwin = listwin.Distinct().ToList();

                //   StartCoroutine(BigSew(listwin));




                if (gameplay.gameInfo.isTurnOnAuto)
                {
                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    yield return new WaitForSeconds(0.3f);
                }

                // StartCoroutine(BigSew(respond));
                /*  rem.transform.DOLocalMoveY(false ? che : open, time / 5, false).OnComplete(() =>
                  {

                  });*/
                if (!gameplay.gameInfo.isTurnOnAuto)
                {
                    finishCoroutine = ShowFinish(listitem);
                    StartCoroutine(finishCoroutine);
                }
                else
                {
                    finishCoroutine = ShowFinish1(listitem);
                    StartCoroutine(finishCoroutine);
                }

            }
            else
            {

                /* for (int i = 0; i < listitem.Length; i++)
                 {
                     StartCoroutine(Sew(i));
                 }*/

            }

            if (gameplay.gameInfo.xStartMiniGame > 0)
            {
                yield return new WaitForSeconds(3f);
            }
            IsPlaying = false;



        }
        private Color32 RamdomColor(int id = 0)
        {

            switch (id)
            {
                case 0: return new Color32(255, 30, 30, 255);
                case 1: return new Color32(30, 78, 255, 255);
                case 2: return new Color32(0, 246, 2, 255);
                case 3: return new Color32(241, 255, 0, 255);
                case 4: return new Color32(241, 0, 255, 255);
                case 5: return new Color32(0, 255, 255, 255);
                case 6: return new Color32(0, 0, 255, 255);
                case 7: return new Color32(255, 255, 255, 255);
                default:
                    return new Color32(255, 30, 30, 255);
            }
        }
        private int idColor = 0, id = 0;
        public IEnumerator ShowFinish1(params int[] listitem)
        {
            for (int i = 0; i < Parent.transform.childCount; i++)
            {
                Parent.transform.GetChild(i).gameObject.SetActive(true);
                yield return new WaitForSeconds(0.01f);
            }




        }
        public IEnumerator ShowFinish(params int[] listitem)
        {
            for (int i = 0; i < Parent.transform.childCount; i++)
            {
                Parent.transform.GetChild(i).gameObject.SetActive(false);
            }



            yield return new WaitForSeconds(0.3f);
            for (int i = 0; i < Parent.transform.childCount; i++)
            {
                for (int j = 0; j < listBird.Length; j++)
                {
                    listBird[j].GetComponent<Image>().color = new Color32(255, 255, 255, 100);
                }
                int row = int.Parse(Parent.transform.GetChild(i).gameObject.name.ToString().Split(' ')[0]);
                int item = int.Parse(Parent.transform.GetChild(i).gameObject.name.ToString().Split(' ')[1]);

                if (i > 0)
                {
                    int row1 = int.Parse(Parent.transform.GetChild(i - 1).gameObject.name.ToString().Split(' ')[0]);

                    for (int j = 0; j < Parent.transform.childCount; j++)
                    {
                        Parent.transform.GetChild(j).gameObject.SetActive(false);
                    }
                    yield return new WaitForSeconds(0.1f);

                }
                //  if(rowArrray[row, 0]==listitem[rowArrray[row, 0]]) 
                SoundManager.instance.PlayEffectSound(SoundFX.WIN_LINE_ONCE + "_" + UnityEngine.Random.Range(1, 6));
                //SoundManager.instance.PlayUISound(SoundFX.WIN_LINE_ONCE);

                try
                {

                    //Debug.Log(row + " " + item + " => " + rowArrray[row, 0] + " " + rowArrray[row, 1] + " " + rowArrray[row, 2] + " " + rowArrray[row, 3] + " " + rowArrray[row, 4] + " = " + listitem[rowArrray[row, 0]] + " " + listitem[rowArrray[row, 1]] + " " + listitem[rowArrray[row, 2]] + " " + listitem[rowArrray[row, 3]] + " " + listitem[rowArrray[row, 4]]);

                    if (listitem[rowArrray[row, 0]] == (item))
                    {
                        listBird[rowArrray[row, 0]].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    }

                    if (listitem[rowArrray[row, 1]] == (item))
                    {
                        listBird[rowArrray[row, 1]].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    }

                    if (listitem[rowArrray[row, 2]] == (item))
                    {
                        listBird[rowArrray[row, 2]].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    }

                    if (listitem[rowArrray[row, 3]] == (item))
                    {
                        listBird[rowArrray[row, 3]].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    }

                    if (listitem[rowArrray[row, 4]] == (item))
                    {
                        listBird[rowArrray[row, 4]].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    }




                    Parent.transform.GetChild(i).gameObject.SetActive(true);
                }
                catch (Exception)
                {
                    Debug.Break();
                    throw;
                }


                yield return new WaitForSeconds(0.9f);


            }
            gameplay.SetPanelTranfer(false);
        }



        private IEnumerator BigSew(List<int> listdata)
        {
            if (listdata.Count > 0)
            {
                for (int i = 0; i < listdata.Count; i++)
                {
                    try
                    {
                        //  listBird[listdata[i]].transform.GetComponentsInChildren<SkeletonGraphic>()[0].AnimationState.SetAnimation(0, states[1], false);
                    }
                    catch
                    {

                    }
                }
                yield return new WaitForSeconds(0.5f);
                for (int i = 0; i < listdata.Count; i++)
                {
                    listSew[listdata[i]].SetActive(false);
                }
            }
        }
        private IEnumerator Sew(int i)
        {
            //  listBird[i].transform.GetComponentsInChildren<SkeletonGraphic>()[0].AnimationState.SetAnimation(0, states[1], false);
            //  listBird[i].transform.GetComponentsInChildren<SkeletonGraphic>()[0].timeScale = 1;

            yield return new WaitForSeconds(0.5f);
            //  listSew[i].SetActive(false);
        }
        private void Start()
        {
            Init();
        }

        private bool isPlaying = false;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (value)
                {
                    if (StartRollEvent != null)
                    {
                        StartRollEvent.Invoke();
                        //Debug.Log("Start");
                        //panel.SetActive(true);
                    }
                }
                else
                {
                    if (EndRollEvent != null)
                    {
                        EndRollEvent.Invoke();
                        //Debug.Log("End");
                    }
                }


                isPlaying = value;
            }
        }
        private void DeleteLight()
        {

            for (int i = 0; i < Parent.transform.childCount; i++)
            {
                Destroy(Parent.transform.GetChild(i).gameObject);
            }
            listLight.Clear();
        }
        void Init()
        {
            DeleteLight();

            System.Random rd = new System.Random();
            int idpre = 0;
            for (int i = 0; i < listBird.Length; i++)
            {
                int id = rd.Next(0, 7);
                while (id == idpre)
                    id = rd.Next(0, 7);
                idpre = id;
                listBird[i].GetComponent<Image>().overrideSprite = spriteReference[id];
                listBird[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                //  listBird[i].transform.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "unactive", true);
                listSew[i].SetActive(true);
                // listBird[i].transform.GetComponentsInChildren<SkeletonGraphic>()[1].GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "khoino", false);
                // listBird[i].transform.GetComponentsInChildren<SkeletonGraphic>()[1].GetComponent<SkeletonGraphic>().timeScale = 1;
                listSew[i].SetActive(false);

            }
        }

    }
}