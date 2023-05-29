using UnityEngine;
using Photon.Pun;
using System.Collections;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public PlayerModel Model { get; set; }
    private Rigidbody rb;
    public PlayerView View { get; set; }
    private bool isMyTurn = false;
    private bool hasSpawned = false; // Flag to track if the player has already been spawned
    private bool isLocalPlayer = false;
    public bool IsLocalPlayer { get { return isLocalPlayer; } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Model = new PlayerModel();
        View = GetComponent<PlayerView>();

        if (photonView.IsMine)
        {
            isLocalPlayer = true;
            PhotonNetwork.LocalPlayer.NickName = "Player " + PhotonNetwork.LocalPlayer.ActorNumber;
        }
        else
        {
            isLocalPlayer = false;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            // Handle the case when the master client leaves the room
        }
    }

    public void InitializePlayer(Player player, bool isLocal)
    {
        Model.Name = player.NickName;

        isLocalPlayer = isLocal;

        if (isLocalPlayer)
        {
            // Additional initialization logic for the local player
        }
        else
        {
            // Additional initialization logic for remote players
        }
    }

    public void SetTurn(bool isTurn)
    {
        // Update player visuals or behavior based on the turn state
    }

    private void Start()
    {
        Model.StartPosition = transform.position;
        View.Model = Model;

        if (photonView.IsMine)
        {
            // Spawn the player only for the local player
            if (!hasSpawned)
            {
                SpawnPlayer(PhotonNetwork.LocalPlayer); // Pass the local player as an argument
                hasSpawned = true; // Set the flag to indicate that the player has been spawned
            }
        }
    }

    private void Update()
    {
        if (isMyTurn && photonView.IsMine)
        {
            // Handle player input for their turn

            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100f, Model.WalkableMask))
                {
                    Model.TargetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                    Model.Moving = true;
                    Model.TargetFallHeight = Model.TargetPosition.y - Model.RayLength;

                    Debug.Log("Clicked to move to: " + Model.TargetPosition);
                }
            }

            if (Model.Moving && Vector3.Distance(transform.position, Model.TargetPosition) > 0.1f)
            {
                var moveDir = (Model.TargetPosition - transform.position).normalized;
                rb.MovePosition(transform.position + moveDir * Model.MoveSpeed);
            }
            else if (Model.Moving && Vector3.Distance(transform.position, Model.TargetPosition) <= 0.1f)
            {
                Model.Moving = false;
                Debug.Log("Reached target position: " + Model.TargetPosition);

                // End the player's turn after reaching the target position
                SetTurn(false);
            }

            if (Model.Falling)
            {
                rb.MovePosition(transform.position + (Vector3.down * Model.FallSpeed * Time.deltaTime));
                RaycastHit hit;

                if (Physics.Raycast(transform.position, Vector3.down, out hit, Model.MaxFallCastDistance, Model.CollidableMask))
                {
                    Model.Falling = false;

                    if (Mathf.Abs(transform.position.y - Model.TargetFallHeight) > 0.1f)
                    {
                        StartCoroutine(ResetPlayer());
                    }
                }
            }
        }
    }

    private void SpawnPlayer(Player player)
    {
        if (hasSpawned)
        {
            return; // If the player has already been spawned, exit the method
        }

        Model.Name = player.NickName;

        // Instantiate the player prefab at the desired position
        GameObject playerObject = PhotonNetwork.Instantiate("Player 1", Model.StartPosition, Quaternion.identity);

        // Set up the spawned player object
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        playerController.InitializePlayer(player, true);

        // Start the turn for the local player
        SetTurn(true);

        hasSpawned = true; // Set the flag to indicate that the player has been spawned
    }

    private IEnumerator ResetPlayer()
    {
        yield return new WaitForSeconds(0.1f);

        rb.MovePosition(Model.StartPosition);
        Model.Falling = true;
        Model.TargetPosition = Model.StartPosition;
        Model.TargetFallHeight = Model.TargetPosition.y - Model.RayLength;
        Debug.Log("Resetting player position");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Model.Name);
            stream.SendNext(isMyTurn);
        }
        else
        {
            Model.Name = (string)stream.ReceiveNext();
            isMyTurn = (bool)stream.ReceiveNext();
        }
    }
}
