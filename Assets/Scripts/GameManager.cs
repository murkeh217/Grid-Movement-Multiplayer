using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    private bool isGameStarted = false;
    private int currentPlayerIndex = 0;
    private Player[] players;
    private int[] playerScores;
    private Button endTurnButton;

    public GameObject[] playerPrefabs; // Array of player prefabs to spawn

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Connecting to Photon server...");
            PhotonNetwork.ConnectUsingSettings();
        }
        
        endTurnButton = GameObject.Find("End Turn Button").GetComponent<Button>();
        endTurnButton.onClick.AddListener(EndTurn);
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

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        isGameStarted = true;

        players = PhotonNetwork.PlayerList;
        playerScores = new int[players.Length];

        // Initialize player scores
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = 0;
        }

        // Determine the starting player
        currentPlayerIndex = Random.Range(0, players.Length);

        // Inform the players about the starting player
        photonView.RPC("RPC_SetTurn", RpcTarget.All, currentPlayerIndex);

        // Spawn players
        for (int i = 0; i < players.Length; i++)
        {
            SpawnPlayer(i);
        }
    }

    private void SpawnPlayer(int playerIndex)
    {
        Vector3 spawnPosition = GetPlayerSpawnPosition();

        // Instantiate the player prefab based on player index
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefabs[playerIndex].name, spawnPosition, Quaternion.identity);

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
