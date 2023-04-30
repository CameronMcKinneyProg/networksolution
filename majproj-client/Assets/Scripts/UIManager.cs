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

    public void UpdateRTTText(double _rtt)
    {
        _rtt *= 1000; // conversion from seconds to milliseconds
        rttText.text = $"RTT: {_rtt}";
    }

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }
}
