using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance; // singleton

    public GameObject startMenu;
    public TMP_InputField usernameField;
    public TMP_Text rttText;
    public float rttUpdatePeriod = 1.0f;

    private float nextRttUpdateTime = 0.0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("UIManager instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //private void Update()
    //{
    //    if (Time.time > nextRttUpdateTime)
    //    {
    //        nextRttUpdateTime += rttUpdatePeriod;

    //        rttText.text = $"{Client.PingAddress(hostIp, rttUpdatePeriod)} ms";
    //    }
    //}

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }
}
