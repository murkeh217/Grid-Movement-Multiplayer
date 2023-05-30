using UnityEngine;
using Photon.Pun;
using System.Collections;
using Photon.Realtime;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public PlayerModel Model { get; set; }
    private Rigidbody rb;
    public PlayerView View { get; set; }
    private bool isMyTurn = false;
    private bool isLocalPlayer = false;
    public bool IsLocalPlayer { get { return isLocalPlayer; } }
    private PhotonView photonView; // Reference to the PhotonView component
    public int PlayerIndex { get; private set; }


   private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Model = new PlayerModel();
        View = GetComponent<PlayerView>();
        photonView = GetComponent<PhotonView>(); // Assign the PhotonView component

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

         // Set the PlayerIndex based on the ActorNumber of the Photon player
        PlayerIndex = player.ActorNumber;
    }

    public void SetTurn(bool isTurn)
    {
        // Update player visuals or behavior based on the turn state
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
