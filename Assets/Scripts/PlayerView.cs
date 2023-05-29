using UnityEngine;
using Unity;
public class PlayerView : MonoBehaviour
{
    public PlayerModel Model { get; set; }

    private void Start()
    {
        // Assign a valid PlayerModel instance to the Model property
        Model = new PlayerModel();
    }

    void Update()
    {
        if (Model == null)
        {
            Debug.LogWarning("PlayerModel is not assigned to PlayerView");
            return;
        }

        if (Model.Falling)
        {
            Debug.Log("Player is falling");

            if (transform.position.y <= Model.TargetFallHeight)
            {
                float x = Mathf.Round(transform.position.x);
                float y = Mathf.Round(Model.TargetFallHeight);
                float z = Mathf.Round(transform.position.z);

                transform.position = new Vector3(x, y, z);

                Model.Falling = false;

                Debug.Log("Player reached target fall height: " + Model.TargetFallHeight);

                return;
            }

            transform.position += Vector3.down * Model.FallSpeed * Time.deltaTime;
            return;
        }
        else if (Model.Moving)
        {
            Debug.Log("Player is moving");

            if (Vector3.Distance(Model.StartPosition, transform.position) > 1f)
            {
                Model.Moving = false;
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, Model.TargetPosition, Model.MoveSpeed * Time.deltaTime);
            return;
        }
    }
}



