using System.Collections;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;

public class CVConnect: MonoBehaviour
{
	Thread receiveThread;
	UdpClient client;
	int port;

	public GameObject Player;

	int heartRate;


	void ReceiveData()
	{
		client = new UdpClient(port);
		while (true)
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
				byte[] data = client.Receive(ref anyIP);

				string text = Encoding.UTF8.GetString(data);
				print(">> " + text);
			}
			catch(Exception e)
			{
				print(e.ToString());
			}
		}
	}

	void InitUDP()
	{
		print("UDP Initialized");

		// Create a new thread that listens for info from the python script via UDP socket
		receiveThread = new Thread(new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}


    // Start is called before the first frame update
    void Start()
    {
		port = 5065;
		InitUDP();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
