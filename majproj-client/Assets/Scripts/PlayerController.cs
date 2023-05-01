using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camTransform.forward); // RPC
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ClientSend.PlayerThrowItem(camTransform.forward); // RPC
        }
    }

    private void FixedUpdate()
    {
        InputSnapshotMove _snapshot = SnapshotManager.instance.NewInputSnapshotMove(GetInput());

        SendInputSnapshotToServer(_snapshot);

    }

    private bool[] GetInput()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
        };

        return _inputs;
    }

    private void SendInputSnapshotToServer(InputSnapshotMove _snapshot)
    {
        ClientSend.PlayerMovement(_snapshot);
    }

    private void PredictMovement()
    {

    }
}
