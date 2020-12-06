using System.Collections;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class CVConnect: MonoBehaviour
{
	Thread receiveThread;
	UdpClient client;
	int port;

	public GameObject Player;
    public Slider HRSlider;
    public GameObject EmotionWord;
	double heartRate;
    private float timer = 0.0f;


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

				heartRate = Convert.ToDouble(Encoding.UTF8.GetString(data));
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
        print(Player.GetComponent<PlayerController>().maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1)
        {
            HRSlider.GetComponent<Slider>().value = (float)heartRate;
	    if (heartRate == 1) {
		    Player.GetComponent<PlayerController>().Stress();
		    print(Player.GetComponent<PlayerController>().maxSpeed);
	    }
	    else {
		    Player.GetComponent<PlayerController>().Destress();
	    }
            timer = 0.0f;
        }
    }
}
