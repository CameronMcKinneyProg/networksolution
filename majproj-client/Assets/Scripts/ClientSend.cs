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
            //_packet.Write(Time.realtimeSinceStartup);

            SendUDPData(_packet);
        }
    }

    public static void PlayerMovement(PlayerMove _move)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            // move time
            _packet.Write(_move.time);

            // move inputs
            _packet.Write(_move.input.forward);
            _packet.Write(_move.input.back);
            _packet.Write(_move.input.left);
            _packet.Write(_move.input.right);
            _packet.Write(_move.input.jump);

            // move state
            //_packet.Write(_move.state.position);
            //_packet.Write(_move.state.yVelocity);
            //_packet.Write(_move.state.isGrounded);

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
