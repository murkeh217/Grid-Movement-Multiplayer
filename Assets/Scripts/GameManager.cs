using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private int currentPlayerIndex = 0;
    private Player[] players;
    private int[] playerScores;
    public Button endTurnButton;
    private PlayerController playerController;
    public GameObject[] playerPrefab; // Array of player prefabs
    public Transform[] spawnPoint; // Array of spawn points
    private const string TurnOwnerKey = "TurnOwner";
    private int turnOwner = 0; // Default value indicating no turn owner
    public TMP_Text turnText;
    private bool isCurrentPlayer;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon server...");
            PhotonNetwork.ConnectUsingSettings();
        }

        endTurnButton.interactable = false; // Disable the button initially

        // Initialize turn owner on the master client
        if (PhotonNetwork.IsMasterClient)
        {
            SetTurnOwner(PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to Photon server.");

        PhotonNetwork.JoinOrCreateRoom("Game Room", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);

        Debug.Log("No. of Players: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Check if the required number of players has joined the room
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("Starting game...");
            StartGame();
        }
        else
        {
            Debug.Log("Waiting for more players...");
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("New player entered the room: " + newPlayer.NickName);
        Debug.Log("Current room player count: " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("Starting game...");
            StartGame();
        }
        else
        {
            // Check if it's the local player's turn
            if (turnOwner == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                endTurnButton.interactable = true;
            }
        }
    }

    private void StartGame()
    {
        players = PhotonNetwork.PlayerList;
        currentPlayerIndex = 0;

        // Set the initial turn text and visibility
        SetTurnText(players[currentPlayerIndex]);

        // Set the turn owner on the master client
        if (PhotonNetwork.IsMasterClient)
        {
            SetTurnOwner(players[currentPlayerIndex].ActorNumber);
            endTurnButton.interactable = true; // Enable end turn button for the master client
        }
        else
        {
            endTurnButton.interactable = false; // Disable end turn button for other players initially
        }

        // Instantiate the player for the local client
        if (PhotonNetwork.IsConnectedAndReady)
        {
            photonView.RPC("RPC_InstantiatePlayer", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_InstantiatePlayer()
    {
        // Check if all required components are assigned
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not assigned.");
            return;
        }

        if (spawnPoint == null || spawnPoint.Length == 0)
        {
            Debug.LogError("Spawn Points are not assigned.");
            return;
        }

        // Get the spawn point index for the local player
        int spawnIndex = GetSpawnIndexForPlayer(PhotonNetwork.LocalPlayer.ActorNumber);

        // Instantiate the player prefab at the selected spawn point
        GameObject player = PhotonNetwork.Instantiate(playerPrefab[spawnIndex].name, spawnPoint[spawnIndex].position, spawnPoint[spawnIndex].rotation);

        // Set the player's position and rotation locally
        player.transform.position = spawnPoint[spawnIndex].position;
        player.transform.rotation = spawnPoint[spawnIndex].rotation;
    }

    private int GetSpawnIndexForPlayer(int actorNumber)
    {
        // Find the player with the given actor number and return the corresponding spawn point index
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].ActorNumber == actorNumber)
            {
                return i;
            }
        }

        return -1; // Player not found, return an invalid index
    }

    public void EndTurn()
    {
        // Disable the end turn button to prevent multiple clicks
        endTurnButton.interactable = false;

        // Increment the current player index and wrap around if necessary
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;

        // Inform the players about the new turn
        photonView.RPC("RPC_SetTurn", RpcTarget.All, currentPlayerIndex);
    }

    [PunRPC]
    private void RPC_SetTurn(int playerIndex)
    {
        Debug.LogFormat("Setting turn for Player {0}.", playerIndex);

        currentPlayerIndex = playerIndex;

        for (int i = 0; i < players.Length; i++)
        {
            isCurrentPlayer = (i == playerIndex);
            Player currentPlayer = players[i];

            // Display turn text for the current player
            SetTurnText(currentPlayer);
        }

        // Update turn text visibility based on the current active player
        SetTurnTextVisibility(true);

        // Enable the end turn button for the current player
        UpdateTurnButtonInteractability();
    }

    private void UpdateTurnButtonInteractability()
    {
        endTurnButton.interactable = (players[currentPlayerIndex].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private void SetTurnOwner(int playerId)
    {
        turnOwner = playerId;

        // Synchronize the turn owner across all clients
        Hashtable turnOwnerProperties = new Hashtable();
        turnOwnerProperties[TurnOwnerKey] = turnOwner;
        PhotonNetwork.CurrentRoom.SetCustomProperties(turnOwnerProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(TurnOwnerKey))
        {
            turnOwner = (int)changedProps[TurnOwnerKey];

            // Enable or disable the end turn button based on the current player's turn
            endTurnButton.interactable = (turnOwner == PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    private void SetTurnTextVisibility(bool isVisible)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == players[currentPlayerIndex].ActorNumber)
        {
            turnText.gameObject.SetActive(isVisible);
        }
        else
        {
            turnText.gameObject.SetActive(false);
        }
    }

    private void SetTurnText(Player player)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == player.ActorNumber)
        {
            turnText.text = "Your Turn";
        }
        else
        {
            turnText.text = "Opponent's Turn";
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(turnOwner);
        }
        else
        {
            turnOwner = (int)stream.ReceiveNext();
        }
    }
}
