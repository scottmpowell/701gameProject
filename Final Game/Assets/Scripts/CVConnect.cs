using System.Collections;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using System.Diagnostics;

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
		/*
		Process proc = new Process();
		proc.StartInfo.FileName = "/usr/bin/python";
		proc.StartInfo.WorkingDirectory = "~/701gameProject/";
		proc.StartInfo.UseShellExecute = false;
		proc.StartInfo.Arguments = "hr.py";
		proc.Start();
		*/
	
		while (true)
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
				byte[] data = client.Receive(ref anyIP);

				double text = Convert.ToDouble(Encoding.UTF8.GetString(data));
				if (text > 80) {
					print("stressed");
					BroadcastMessage("heartrate", text);
				}
				else if (text < 80) {
					print("not stressed");
					//Player.maxSpeed = 6;
					BroadcastMessage("heartrate", text);
				}
				print(">>> " + text);
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
		ReceiveData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
