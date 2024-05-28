using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UFE3D;

public class DefaultNetworkConnectionScreen : NetworkConnectionScreen
{
    public Button buttonConnect;
    public Button buttonBluetooth;
    public Text connectionInfo;
    public Dropdown dropDown;

    private readonly Dictionary<string, string> regionDictionary = new Dictionary<string, string>
        {
            {"Best", ""},
            {"Australia", "asia"},
            {"Brazil", "au"},
            {"Canada", "cae"},
            {"China", "cn"},
            {"India", "eu"},
            {"Japan", "in"},
            {"Netherlands", "jp"},
            {"Singapore", "za"},
            {"South Africa", "sa"},
            {"South Korea", "kr"},
            {"Turkey", "tr"},
            {"USA, Central", "us"},
            {"USA, East", "usw"},
            {"USA, West", "ussc"}
        };

    #region public override methods
    public override void OnShow()
    {
        base.OnShow();

        connectionInfo.text = "";

        if (buttonConnect != null)
        {
            buttonConnect.interactable = UFE.IsNetworkAddonInstalled;
        }

        if (buttonBluetooth != null)
        {
            buttonBluetooth.interactable = UFE.IsBluetoothAddonInstalled;
        }
    }

    public override void ConnectToServer()
    {
        UFE.MultiplayerAPI.SetRegion(regionDictionary[dropDown.options[dropDown.value].text]);
        base.ConnectToServer();
        buttonConnect.interactable = false;
        connectionInfo.text = "Connecting...";
    }
    public override void OnInitializationSuccessful()
    {
        base.OnInitializationSuccessful();
        connectionInfo.text = "Connected";
        UFE.StartNetworkOptionsScreen();
    }

    public override void OnInitializationError()
    {
        base.OnInitializationError();
        connectionInfo.text = "Connection Error";
    }
    #endregion
}
