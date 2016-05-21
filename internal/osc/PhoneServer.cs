using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class PhoneServer : MonoBehaviour
{
	public int listenPort = 9000;
	UdpClient udpClient;
	IPEndPoint endPoint;
	Osc.Parser osc = new Osc.Parser ();
	public GameObject player;

	void Start ()
	{
		endPoint = new IPEndPoint (IPAddress.Any, listenPort);
		udpClient = new UdpClient (endPoint);
	}

	void Update ()
	{

		while (udpClient.Available > 0) {
			osc.FeedData (udpClient.Receive (ref endPoint));
		}

		while (osc.MessageCount > 0) {
			var msg = osc.PopMessage ();
			if(msg.path=="/touchpad")
			{
				float[] position = new float[2];
				position[0]=(float)msg.data [0];
				position[1]=(float)msg.data [1];
				player.SendMessage ("OnPositionMessage", position);
			}
			if(msg.path=="/accxyz")
			{
//				player.SendMessage ("OnTiltMessage", msg.data);
			}

		//	Debug.Log (msg);
		}
	}
}

