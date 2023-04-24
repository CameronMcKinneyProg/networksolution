using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class UDP
{
    public IPEndPoint endPoint;

    private int id;

    public UDP(int _id)
    {
        id = _id;
    }

    public void Connect(IPEndPoint _endPoint)
    {
        endPoint = _endPoint;
    }

    public void SendData(Packet _packet)
    {
        Server.SendUDPData(endPoint, _packet);
    }

    public void HandleData(Packet _packetData)
    {
        int _packetLength = _packetData.ReadInt();
        byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

        ThreadManager.ExecuteOnMainThread(() =>
        {
            using (Packet _packet = new Packet(_packetBytes))
            {
                int _packetId = _packet.ReadInt();
                Server.packetHandlers[_packetId](id, _packet);
            }
        });
    }

    public void Disconnect()
    {
        endPoint = null;
    }
}
