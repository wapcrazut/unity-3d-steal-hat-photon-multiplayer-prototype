using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    
    [SerializeField] private PlayerUIContainer[] playerContainers;
    [SerializeField] private TextMeshProUGUI winnerText;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializePlayerUI();
    }
    
    private void Update()
    {
        if (GameManager.Instance.Players.Length > 0)
        {
            PlayerUIUpdate();
        }
    }
    
    private void InitializePlayerUI()
    {
        for (int i = 0; i < playerContainers.Length; i++)
        {
            if (PhotonNetwork.PlayerList.Length >= i + 1)
            {
                playerContainers[i].GameObject.SetActive(true);
                playerContainers[i].SetPlayerName(PhotonNetwork.PlayerList[i].NickName);
                playerContainers[i].SetHatTime(0);
            }
            else
            {
                playerContainers[i].GameObject.SetActive(false);
            }
        }
    }
    
    private void PlayerUIUpdate()
    {
        for (int i = 0; i < GameManager.Instance.Players.Length; i++)
        {
            if (GameManager.Instance.Players[i] != null)
            {
                playerContainers[i].SetHatTime(GameManager.Instance.Players[i].CurrentHatTime);
            }
        }
    }
    
    public void SetWinnerText(string playerName)
    {
        winnerText.gameObject.SetActive(true);
        winnerText.text = playerName + " wins!";
    }
}

[System.Serializable]
public class PlayerUIContainer
{
    [SerializeField] private GameObject gameObject;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image hatTimeImage;
    
    public GameObject GameObject { get => gameObject; }
    
    public void SetPlayerName(string name)
    {
        playerNameText.text = name;
    }
    
    public void SetHatTime(float time)
    {
        hatTimeImage.fillAmount = time / GameManager.Instance.TimeToWin;
    }
}