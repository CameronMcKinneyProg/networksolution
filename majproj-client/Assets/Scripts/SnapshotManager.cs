using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputSnapshotMove
{
    public long sequenceNum { get; set; }
    public bool[] inputs { get; set; }
}

public class SnapshotManager : MonoBehaviour
{
    public static SnapshotManager instance;
    private static List<InputSnapshotMove> inputSnapshotMoveBuffer = new List<InputSnapshotMove>();
    private static long nextSequenceNum = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("SnapshotManager instance already exists, destroying object!");
            Destroy(this);
        }
    }
    
    public InputSnapshotMove NewInputSnapshotMove(bool[] _inputs)
    {
        InputSnapshotMove _snapshot = new InputSnapshotMove();
        _snapshot.sequenceNum = nextSequenceNum;

        nextSequenceNum++;

        _snapshot.inputs = _inputs;

        inputSnapshotMoveBuffer.Add(_snapshot);

        return _snapshot;
    }

    public void RemoveSnapshotFromBuffer()
    {

    }
}
