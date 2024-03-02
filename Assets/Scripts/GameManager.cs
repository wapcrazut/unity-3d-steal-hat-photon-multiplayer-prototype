using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    
    private float _hatPickupTime;
    private bool _hasGameEnded;
    private int _playerWithHat;
    private int _playersInGame;
    private PlayerController[] _players;
    
    [Header("Game Settings")]
    [SerializeField] private float timeToWin;
    [SerializeField] private float invincibleDuration;
    
    [Header("Players")]
    [SerializeField] private string playerPrefabLocation;
    [SerializeField] private Transform[] spawnPoints;
    
    
    public bool HasGameEnded { get => _hasGameEnded; set => _hasGameEnded = value; }
    public float TimeToWin { get => timeToWin; }
    public int PlayerWithHat { get => _playerWithHat; }
    public PlayerController[] Players { get => _players; set => _players = value; }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _players = new PlayerController[PhotonNetwork.PlayerList.Length];
        
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(PlayerIsInGame), RpcTarget.AllBuffered);
        }
        else
        {
            // Only for testing in the editor
            SpawnPlayer();
        }
    }
    
    [PunRPC]
    private void PlayerIsInGame()
    {
        _playersInGame++;
        if (_playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        GameObject playerObject;
        
        if (PhotonNetwork.IsConnected)
        { 
            playerObject = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
        }
        else
        {
            playerObject = Instantiate(Resources.Load(playerPrefabLocation), GetRandomSpawnPoint().position, Quaternion.identity) as GameObject;
        }
        
        PlayerController playerScript = playerObject.GetComponent<PlayerController>();
        playerScript.photonView.RPC(nameof(PlayerController.Init), RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer(int playerId)
    {
        return _players.First(p => p.PlayerID == playerId);
    }
    
    public PlayerController GetPlayer(GameObject playerObject)
    {
        return _players.First(p => p.gameObject == playerObject);
    }

    // Called when a player hits a player with the hat
    [PunRPC]
    public void GiveHatToPlayer(int playerId, bool firstGive)
    {
        if (!firstGive)
        {
            GetPlayer(_playerWithHat).SetHat(false);
        }
        
        // Update the player with the hat
        _playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        _hatPickupTime = Time.time;
    }

    public bool CanPlayerGetHat()
    {
        return Time.time > _hatPickupTime + invincibleDuration;
    }
    
    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
    
    [PunRPC]
    public void GameOver(int playerId)
    {
        _hasGameEnded = true;
        PlayerController player = GetPlayer(playerId);
        GameUI.Instance.SetWinnerText(player.PhotonPlayer.NickName);
        Debug.Log(player.PhotonPlayer.NickName + " has won the game!");
        
        Invoke(nameof(GoToMenu), 3f);
    }
    
    private void GoToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManager.Instance.ChangeScene("Menu");
    }
}
