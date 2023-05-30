using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    private bool isGameStarted = false;
    private int currentPlayerIndex = 0;
    private Player[] players;
    private int[] playerScores;
    public Button endTurnButton;

public GameObject[] playerPrefab; // Array of player prefabs
    public Transform[] spawnPoint; // Array of spawn points

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon server...");
            PhotonNetwork.ConnectUsingSettings();
        }

        endTurnButton.interactable = false; // Disable the button initially
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
}



    private void SpawnPlayer(int playerIndex)
    {
        Vector3 spawnPosition = GetPlayerSpawnPosition();

        // Instantiate the player prefab based on player index
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab[playerIndex].name, spawnPosition, Quaternion.identity);

        Debug.Log("Player prefab instantiated: " + playerObject.name);

        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        playerController.InitializePlayer(players[playerIndex], playerIndex == 0); // Pass true for local player, false for remote players
    }

    private Vector3 GetPlayerSpawnPosition()
    {
        // Randomly select a spawn point from the available spawn points
        Transform[] spawnPoints = GameObject.Find("Spawn Points").GetComponentsInChildren<Transform>();
        int randomIndex = Random.Range(1, spawnPoints.Length); // Start from index 1 to skip the parent transform
        Vector3 spawnPosition = spawnPoints[randomIndex].position;

        return spawnPosition;
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
    }

    public void EndTurn()
    {
        // Increment the current player index and wrap around if necessary
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;

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
        }
    }
}
