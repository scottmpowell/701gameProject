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

public class CVConnect : MonoBehaviour
{
	Thread receiveThread;
	UdpClient client;
	int port;

	public GameObject Player;
	public Slider HRSlider;
	public GameObject EmotionWord;
	public Slider EmotionSlider;
	public GameObject HRValue;
	double heartRatio;
	double heartRate;
	string playerEmotion;
	private float timer = 0.0f;
	private float emotionLockout = 0.0f;

	private bool ready;


	void ReceiveData()
	{
		client = new UdpClient(port);

		while (true)
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port);
				byte[] data = client.Receive(ref anyIP);
				string[] info = Encoding.UTF8.GetString(data).Split();
				int index = Convert.ToInt16(info[2]);
				heartRatio = Convert.ToDouble(info[0]);
				heartRate = Convert.ToDouble(info[3]);
				playerEmotion = info[1];
				if (index < 3 || index == 5)
				{
					emotionLockout = 5.0f;
				}

			}
			catch (Exception e)
			{
				print(e.ToString());
			}
		}
	}

	void InitUDP()
	{
		print("UDP Initialized");
		port = 5065;

		// Create a new thread that listens for info from the python script via UDP socket
		receiveThread = new Thread(new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}

	public void EndUDP()
    {
		receiveThread.Abort();
    }

    private void OnDestroy()
    {
		EndUDP();
    }

    // Start is called before the first frame update
    void Start()
	{
		/*ready = false;
		Time.timeScale = 0f;
		SomeUI.SetActive(true);*/
		InitUDP();
	}

	// Update is called once per frame
	void Update()
	{
		timer += Time.deltaTime;
		if (emotionLockout != 0.0)
		{
			emotionLockout -= Time.deltaTime;

			if (emotionLockout < 0.0)
			{
				emotionLockout = 0.0F;
			}
			EmotionSlider.GetComponent<Slider>().value = (float)(emotionLockout / 5.0F);
		}

		if (timer >= 1)
		{
			HRSlider.GetComponent<Slider>().value = (float)heartRatio;
			HRValue.GetComponent<Text>().text = heartRate.ToString();
			EmotionWord.GetComponent<Text>().text = playerEmotion;

			if (heartRatio == 1 || emotionLockout > 0)
			{
				Player.GetComponent<PlayerController>().Stress();
			}
			else
			{
				Player.GetComponent<PlayerController>().Destress();
			}

			timer = 0.0f;
		}
	}
}
