using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public PlayerModel Model { get; set; }
    void Update()
    {
        // Set the ray positions every frame
        Model.YOffset = transform.position + Vector3.up * Model.RayOffsetY;
        Model.ZOffset = Vector3.forward * Model.RayOffsetZ;
        Model.XOffset = Vector3.right * Model.RayOffsetX;

        Model.ZAxisOriginA = Model.YOffset + Model.XOffset;
        Model.ZAxisOriginB = Model.YOffset - Model.XOffset;

        Model.XAxisOriginA = Model.YOffset + Model.ZOffset;
        Model.XAxisOriginB = Model.YOffset - Model.ZOffset;

        // Draw Debug Rays
        Debug.DrawLine(
                Model.ZAxisOriginA,
                Model.ZAxisOriginA + Vector3.forward * Model.RayLength,
                Color.red,
                Time.deltaTime);

        Debug.DrawLine(
                Model.ZAxisOriginB,
                Model.ZAxisOriginB + Vector3.forward * Model.RayLength,
                Color.red,
                Time.deltaTime);

        Debug.DrawLine(
                Model.ZAxisOriginA,
                Model.ZAxisOriginA + Vector3.back * Model.RayLength,
                Color.red,
                Time.deltaTime);

        Debug.DrawLine(
                Model.ZAxisOriginB,
                Model.ZAxisOriginB + Vector3.back * Model.RayLength,
                Color.red,
                Time.deltaTime);

        Debug.DrawLine(
                Model.XAxisOriginA,
                Model.XAxisOriginA + Vector3.right * Model.RayLength,
                Color.green,
                Time.deltaTime);

        Debug.DrawLine(
                Model.XAxisOriginB,
                Model.XAxisOriginB + Vector3.right * Model.RayLength,
                Color.green,
                Time.deltaTime);

        Debug.DrawLine(
                Model.XAxisOriginA,
                Model.XAxisOriginA + Vector3.left * Model.RayLength,
                Color.green,
                Time.deltaTime);

        Debug.DrawLine(
                Model.XAxisOriginB,
                Model.XAxisOriginB + Vector3.left * Model.RayLength,
                Color.green,
                Time.deltaTime);

        if (Model.Falling) {
            if (transform.position.y <= Model.TargetFallHeight) {
                float x = Mathf.Round(transform.position.x);
                float y = Mathf.Round(Model.TargetFallHeight);
                float z = Mathf.Round(transform.position.z);

                transform.position = new Vector3(x, y, z);

                Model.Falling = false;

                return;
            }

            transform.position += Vector3.down * Model.FallSpeed * Time.deltaTime;
            return;

        } else if (Model.Moving) {
            if (Vector3.Distance(Model.StartPosition, transform.position) > 1f) {
                float x = Mathf.Round(Model.TargetPosition.x);
                float y = Mathf.Round(Model.TargetPosition.y);
                float z = Mathf.Round(Model.TargetPosition.z);

                transform.position = new Vector3(x, y, z);

                Model.Moving = false;

                return;
            }

            transform.position += (Model.TargetPosition - Model.StartPosition) * Model.MoveSpeed * Time.deltaTime;
            return;

        } else {
            RaycastHit[] hits = Physics.RaycastAll(
                    transform.position + Vector3.up * 0.5f,
                    Vector3.down,
                    Model.MaxFallCastDistance,
                    Model.WalkableMask
            );

            if (hits.Length > 0) {
                int topCollider = 0;
                for (int i = 0; i < hits.Length; i++) {
                    if (hits[topCollider].collider.bounds.max.y < hits[i].collider.bounds.max.y)
                        topCollider = i;
                }
                if (hits[topCollider].distance > 1f) {
                    Model.TargetFallHeight = transform.position.y - hits[topCollider].distance + 0.5f;
                    Model.Falling = true;
                }
            } else {
                Model.TargetFallHeight = -Mathf.Infinity;
                Model.Falling = true;
            }
        }

    }

    // Check if the player can move

    bool CanMove(Vector3 direction) {
        if (direction.z != 0) {
            if (Physics.Raycast(Model.ZAxisOriginA, direction, Model.RayLength)) return false;
            if (Physics.Raycast(Model.ZAxisOriginB, direction, Model.RayLength)) return false;
        }
        else if (direction.x != 0) {
            if (Physics.Raycast(Model.XAxisOriginA, direction, Model.RayLength)) return false;
            if (Physics.Raycast(Model.XAxisOriginB, direction, Model.RayLength)) return false;
        }
        return true;
    }

    // Check if the player can step-up

    bool CanMoveUp(Vector3 direction) {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 1f, Model.CollidableMask))
            return false;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, 1f, Model.CollidableMask))
            return false;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, 1f, Model.WalkableMask))
            return true;
        return false;
    }
}


