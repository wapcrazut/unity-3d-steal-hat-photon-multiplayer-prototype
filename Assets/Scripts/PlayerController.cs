using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    private int _playerId;
    private float _currentHatTime = 0;
    private Vector3 _moveDirection;
    private bool _tryJump;
    
    [Header("Info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private GameObject hatObject;
    
    [Header("Components")]
    private Rigidbody _rigidbody;
    private Player _photonPlayer;
    
    public int PlayerID { get => _playerId; }
    public Player PhotonPlayer { get => _photonPlayer; }
    public float CurrentHatTime { get => _currentHatTime; }

    [PunRPC]
    public void Init(Player player)
    {
        _photonPlayer = player;
        _playerId = player.ActorNumber;
        GameManager.Instance.Players[_playerId - 1] = this;
        
        if (_playerId == 1)
        {
            GameManager.Instance.GiveHatToPlayer(_playerId, true);
        }

        if (!photonView.IsMine)
        {
            _rigidbody.isKinematic = true;
        }
    }
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        //_photonPlayer = GetComponent<Player>();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (_currentHatTime >= GameManager.Instance.TimeToWin && !GameManager.Instance.HasGameEnded)
            {
                GameManager.Instance.HasGameEnded = true;
                GameManager.Instance.photonView.RPC(nameof(GameManager.GameOver), RpcTarget.All, _playerId);
            }
        }
        
        if (!photonView.IsMine)
        {
            return;
        }
        
        _moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        
        if (Input.GetButtonDown("Jump"))
        {
            _tryJump = true;
        }
        
        // If we have the hat, we need to keep track of the time
        if (hatObject.activeInHierarchy)
        {
            _currentHatTime += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        
        Move();
        
        if (_tryJump)
        {
            _tryJump = false;
            TryJump();
        }
        
        CheckOutOfBounds();
    }
    
    private void CheckOutOfBounds()
    {
        if (transform.position.y < -5)
        {
            gameObject.transform.position = GameManager.Instance.GetRandomSpawnPoint().position;
        }
    }

    private void Move()
    {
        _rigidbody.velocity = new Vector3(_moveDirection.x * moveSpeed, _rigidbody.velocity.y, _moveDirection.z * moveSpeed);
    }
    
    private void TryJump()
    {
        if (Physics.Raycast(transform.position, Vector3.down, .7f))
        {
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If we are not the local player, we don't care about collisions
        if (!photonView.IsMine)
        {
            return;
        }
        
        if (!collision.transform.CompareTag("Player"))
        {
            return;
        }
        
        if (GameManager.Instance.GetPlayer(collision.gameObject).PlayerID == GameManager.Instance.PlayerWithHat 
            && GameManager.Instance.CanPlayerGetHat())
        {
            GameManager.Instance.photonView.RPC(nameof(GameManager.GiveHatToPlayer), RpcTarget.All, _playerId, false);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // We need to sync the current hat time with all the players
        if (stream.IsWriting)
        {
            stream.SendNext(_currentHatTime);
        }
        else if (stream.IsReading)
        {
            _currentHatTime = (float) stream.ReceiveNext();
        }
    }
}
