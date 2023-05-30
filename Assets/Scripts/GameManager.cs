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
    private bool isGameStarted = false;
    private int currentPlayerIndex = 0;
    private Player[] players;
    private int[] playerScores;
    public Button endTurnButton;
    private PlayerController playerController;
    public GameObject[] playerPrefab; // Array of player prefabs
    public Transform[] spawnPoint; // Array of spawn points
    private const string TurnOwnerKey = "TurnOwner";
    private int turnOwner = -1; // Default value indicating no turn owner
    public TMP_Text turnText;

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
        Debug.Log("isGameStarted: " + isGameStarted);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && !isGameStarted)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        Debug.Log("StartGame() called");

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

        // Get a random spawn point index
        int spawnIndex = Random.Range(0, spawnPoint.Length);

        // Instantiate a random player prefab at the selected spawn point
        GameObject player = PhotonNetwork.Instantiate(playerPrefab[Random.Range(0, playerPrefab.Length)].name, spawnPoint[spawnIndex].position, spawnPoint[spawnIndex].rotation);

        // Set the player's position and rotation locally
        player.transform.position = spawnPoint[spawnIndex].position;
        player.transform.rotation = spawnPoint[spawnIndex].rotation;

        // Set the isGameStarted flag to true
        isGameStarted = true;

        // Set the initial turn text and visibility
        SetTurnText(players[currentPlayerIndex]);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !isGameStarted)
            return;

        // Check for conditions to end the turn
        // Example: Simulating the end of the player's turn after a specific time

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }

        // Update turn text visibility and content
        SetTurnTextVisibility(currentPlayerIndex == playerController.PlayerIndex);
        turnText.text = "Player " + currentPlayerIndex + "'s Turn";
    }

    private void SetTurnTextVisibility(bool isVisible)
    {
        turnText.gameObject.SetActive(isVisible);
    }

    public void EndTurn()
    {
        // Increment the current player index and wrap around if necessary
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;

        // Update the turn text visibility for the new current player
        SetTurnTextVisibility(players[currentPlayerIndex].ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        // Inform the players about the new turn
        photonView.RPC("RPC_SetTurn", RpcTarget.All, currentPlayerIndex);
    }

    [PunRPC]
    private void RPC_SetTurn(int playerIndex)
    {
        Debug.LogFormat("Setting turn for Player {0}.", playerIndex);

        for (int i = 0; i < players.Length; i++)
        {
            // Enable or disable the "End Turn" button based on the current player's turn
            endTurnButton.interactable = (i == playerIndex);

            // Display turn text for the current player
            bool isCurrentPlayer = (i == playerIndex);
            SetTurnTextVisibility(isCurrentPlayer);
        }
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
        if (changedProps.ContainsKey("nextPlayerId"))
        {
            int nextPlayerId = (int)changedProps["nextPlayerId"];
            // Use the nextPlayerId value as needed

            // Update the turn text visibility based on the current player's turn
            bool isCurrentPlayerTurn = (nextPlayerId == PhotonNetwork.LocalPlayer.ActorNumber);
            SetTurnTextVisibility(isCurrentPlayerTurn);
        }
    }

    // Example method to pass the turn to the next player
    public void PassTurnToNextPlayer()
    {
        // Determine the next player in the turn order
        int nextPlayerId = GetNextPlayerId();

        // Set the turn owner to the next player
        SetTurnOwner(nextPlayerId);
    }

    private int GetNextPlayerId()
    {
        // Implement your logic to determine the next player's ID
        // For example, you can use a list of player IDs and cycle through them

        // Return the ID of the next player
        return currentPlayerIndex;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Implement serialization logic here if needed
    }

    private void SetTurnText(Player currentPlayer)
    {
        string playerName = currentPlayer.NickName;
        turnText.text = "Turn: " + playerName;
    }
}
