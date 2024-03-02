using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;
    
    [Header("Main Screen")]
    public Button createRoomButton;
    public Button joinRoomButton;
    
    [Header("Lobby Screen")]
    public TextMeshProUGUI playerListText;
    public Button startGameButton;
    public Button playerReadyButton;

    private void Start()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        startGameButton.interactable = false;
        playerReadyButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        createRoomButton.interactable = true;
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        joinRoomButton.interactable = true;
        Debug.Log("Failed to join room: " + message);
    }

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC(nameof(UpdateLobbyUI), RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    public void SetScreen(GameObject screen)
    {
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        
        screen.SetActive(true);
    }

    public void OnCreateRoomButton(TMP_InputField roomNameInput)
    {
        createRoomButton.interactable = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() {{"ready", true}});
        NetworkManager.Instance.CreateRoom(roomNameInput.text);
    }

    public void OnJoinRoomButton(TMP_InputField roomNameInput)
    {
        joinRoomButton.interactable = false;
        NetworkManager.Instance.JoinRoom(roomNameInput.text);
    }
    
    public void OnPlayerReadyButton()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() {{"ready", true}});
        photonView.RPC(nameof(UpdateLobbyUI), RpcTarget.All);
    }
    
    public void OnPlayerNameUpdate(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public void OnLeaveLobbyButton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.CustomProperties.Remove("ready");
            PhotonNetwork.LeaveRoom();
            SetScreen(mainScreen);
        }
    }
    
    public void OnStartGameButton()
    {
        NetworkManager.Instance.photonView.RPC(nameof(NetworkManager.ChangeScene), RpcTarget.All, "Game");
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerListText.text = "";
        
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + ": " + (player.CustomProperties.ContainsKey("ready") ? "Ready" : "Not Ready") + "\n";
        }
        
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
            playerReadyButton.gameObject.SetActive(false);
            startGameButton.interactable = true;
            
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (!player.CustomProperties.ContainsKey("ready"))
                {
                    startGameButton.interactable = false;
                }
            }
        } 
        else
        {
            startGameButton.gameObject.SetActive(false);
            playerReadyButton.gameObject.SetActive(true);
            playerReadyButton.interactable = true;
            
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("ready"))
            {
                playerReadyButton.GetComponentInParent<Image>().color = Color.green;
            }
            else
            {
                playerReadyButton.GetComponentInParent<Image>().color = Color.white;
            }
        }
    }
}
