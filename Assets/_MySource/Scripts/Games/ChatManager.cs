using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour {
    public bool isChatActive;
    [SerializeField] GameObject chatFeast;
    [SerializeField] GameObject chatTX;
    public GameObject chatBox;
    public static ChatManager instance;
    private void Awake()
    {
        getInstance();
    }
    void getInstance()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }

    }
    public void OpenChat(string gameName)
    {
        if(gameName.Equals("chatBox"))
        {
            chatFeast.SetActive(false);
            chatTX.SetActive(false);
        }
        else
        {
            if (gameName.Equals("taixiu"))
            {
                chatBox.SetActive(false);
                if (chatTX.activeInHierarchy)
                {
                    chatFeast.SetActive(false);
                    chatTX.SetActive(false);
                }
                else
                {
                    chatFeast.SetActive(false);
                    chatTX.SetActive(true);
                }
            }
            else
            {
                chatBox.SetActive(false);
                if (chatFeast.activeInHierarchy)
                {
                    chatFeast.SetActive(false);
                    chatTX.SetActive(false);
                }
                else
                {
                    chatTX.SetActive(false);
                    chatFeast.SetActive(true);
                }
            }
        }
    }
}
