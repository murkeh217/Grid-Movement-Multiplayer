using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public PlayerModel Model { get; set; }
    private Rigidbody rb;
    public PlayerView View { get; set; }

    private void Start()
    {
        // Create an instance of PlayerModel
        Model = new PlayerModel();

        rb = GetComponent<Rigidbody>();
        Model.StartPosition = transform.position;

        // Assign the Model object to the PlayerView script
        View = GetComponent<PlayerView>();
        View.Model = Model;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, Model.WalkableMask))
            {
                Model.TargetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                Model.CameraRotator = Camera.main.transform;
                Model.CameraRotator = null;
                Model.Moving = true;
                Model.TargetFallHeight = Model.TargetPosition.y - Model.RayLength;
            }
        }

        if (Model.Moving && Vector3.Distance(transform.position, Model.TargetPosition) > 0.1f)
        {
            var moveDir = (Model.TargetPosition - transform.position).normalized;

            rb.MovePosition(transform.position + moveDir * Model.MoveSpeed);

            if (Model.CameraRotator != null)
            {
                var rotatorPosition = Model.TargetPosition - Model.CameraRotator.forward * 3f;
                Model.CameraRotator.position = Vector3.Lerp(Model.CameraRotator.position, rotatorPosition, Time.deltaTime * 3f);
            }
        }
        else if (Model.Moving && Vector3.Distance(transform.position, Model.TargetPosition) <= 0.1f)
        {
            Model.Moving = false;
            Model.CameraRotator = transform;
            Model.CameraRotator.localPosition = Vector3.zero;
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

    private IEnumerator ResetPlayer()
    {
        yield return new WaitForSeconds(0.1f);

        rb.MovePosition(Model.StartPosition);
        Model.Falling = true;
        Model.TargetPosition = Model.StartPosition;
        Model.TargetFallHeight = Model.TargetPosition.y - Model.RayLength;
        Model.CameraRotator = null;
    }
}
