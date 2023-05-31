using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public PlayerModel Model { get; set; }
    public PlayerView View { get; set; }
    private bool isMyTurn = false;
    private bool isLocalPlayer = false;
    public bool IsLocalPlayer { get { return isLocalPlayer; } }
    private PhotonView photonView;

    private void Awake()
    {
        Model = new PlayerModel();
        View = GetComponent<PlayerView>();
        photonView = GetComponent<PhotonView>();

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

    public void InitializePlayer(Player player, bool isLocal)
    {
        Model.Name = player.NickName;
        isLocalPlayer = isLocal;
    }

    public void SetTurn(bool isTurn)
    {
        isMyTurn = isTurn;

        // Update player visuals or behavior based on the turn state
        // For example, enable/disable player input or highlight the active player
    }

    private void Update()
    {
        if (isLocalPlayer && isMyTurn)
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

            if (Model.Moving && Vector3.Distance(transform.position, Model.TargetPosition) <= 0.1f)
            {
                Model.Moving = false;
                Debug.Log("Reached target position: " + Model.TargetPosition);

                // End the player's turn after reaching the target position
                photonView.RPC("EndTurn", RpcTarget.All);
            }

            if (Model.Falling)
            {
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

        transform.position = Model.StartPosition;
        Model.Falling = true;
        Model.TargetPosition = Model.StartPosition;
        Model.TargetFallHeight = Model.TargetPosition.y - Model.RayLength;
        Debug.Log("Resetting player position");
    }

    [PunRPC]
    private void EndTurn()
    {
        // End the player's turn
        SetTurn(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isMyTurn);
        }
        else
        {
            isMyTurn = (bool)stream.ReceiveNext();
        }
    }
}
