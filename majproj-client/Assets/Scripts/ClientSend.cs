using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend
{
    // TODO Create circular buffer of movement input snapshots

    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    public static void Ping()
    {
        using (Packet _packet = new Packet((int)ClientPackets.ping))
        {
            _packet.Write(Time.realtimeSinceStartup);

            SendUDPData(_packet);
        }
    }

    public static void PlayerMovement(InputSnapshotMove _snapshot)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            // snapshot sequence number
            _packet.Write(_snapshot.sequenceNum);

            // position
            _packet.Write(_snapshot.inputs.Length);
            foreach (bool _input in _snapshot.inputs)
            {
                _packet.Write(_input);
            }

            // rotation
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

            SendUDPData(_packet);
        }
    }

    public static void PlayerShoot(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
        }
    }

    public static void PlayerThrowItem(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerThrowItem))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
        }
    }
    #endregion
}
