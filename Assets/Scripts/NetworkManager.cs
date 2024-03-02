using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePhotonView();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ConnectToMasterServer();
    }
    
    private void InitializePhotonView()
    {
        PhotonView photonView = gameObject.AddComponent<PhotonView>();
        photonView.ViewID = 1;
        PhotonNetwork.RegisterPhotonView(photonView);
    }
    
    private void ConnectToMasterServer()
    {
        if (!IsConnected())
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void DisconnectFromMasterServer()
    {
        if (IsConnected())
        {
            PhotonNetwork.Disconnect();
        }
    }

    private bool IsConnected()
    {
        return PhotonNetwork.IsConnected;
    }
    
    private static bool CheckGameServerConnection()
    {
        return PhotonNetwork.Server == ServerConnection.GameServer;
    }
    
    public void CreateRoom(string roomName)
    {
        if (CheckGameServerConnection()) return;

        PhotonNetwork.CreateRoom(roomName);
        Debug.Log("Create room: " + roomName);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        Debug.Log("Join room: " + roomName);
    }

    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
