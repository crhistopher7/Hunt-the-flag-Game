using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;
using UnityEngine.AI;

public class Client : MonoBehaviour
{
    private string clientName;
    private int portToConnect = 6321;
    private string password;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    public InputField clientNameInputField;
    public InputField serverAddressInputField;
    public InputField passwordInputField;
    public int seed;
    private CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

    public MapGenerator mapGeneratorPrefab;

    private PlayerController pcTeam1;
    private PlayerController pcTeam2;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        culture.NumberFormat.NumberDecimalSeparator = ".";
    }

    public string getClientName()
    {
        return clientName;
    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
            return false;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error " + e.Message);
        }

        return socketReady;
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }

    public void startPlayerControllers(string a, string b)
    {
        pcTeam1 = GameObject.Find(a).GetComponent<PlayerController>();
        pcTeam2 = GameObject.Find(b).GetComponent<PlayerController>();
    }

    // Sending message to the server
    public void Send(string data)
    {
        if (!socketReady)
            return;

        //Debug.Log("Send to Server:  "+data);

        writer.WriteLine(data);
        writer.Flush();
    }

    // Read messages from the server
    private void OnIncomingData(string data)
    {
        string[] aData = data.Split('|');
        //Debug.Log("Received from server: " + data);

        switch (aData[0])
        {
            // requisição de autentificação
            case "WhoAreYou":
                Send("Iam|" + clientName + "|" + password);
                break;
            // foi autenticado, carrega a cena
            case "Authenticated":
                //pedir seed
                var mapGenerator = Instantiate(mapGeneratorPrefab);
                var seed = int.Parse(aData[1]);
                this.seed = seed;
                mapGenerator.GenerateMap(7);
                mapGenerator.name = "Map Generator";

                if (clientName.Equals("IA"))
                    SceneManager.LoadScene("MainWithAPI");
             
                else
                    SceneManager.LoadScene("SampleScene");

                // Invoke(nameof(startPlayerControllers), 3f);
                break;

            case "AgentMoved":
                //("AgentMoved|" + aData[1] + "|" + aData[2] + "|" + aData[3] + "|" + aData[4], clients)
                int x, y;
                Int32.TryParse(aData[3], out x);
                Int32.TryParse(aData[4], out y);

                if (aData[1] == "Team1")
                {
                    pcTeam1.ReceiveMove(aData[2], x, y);
                }
                else
                {
                    pcTeam2.ReceiveMove(aData[2], x, y);
                }

                break;

            case "Moves":
                //
                string[] movesData = (data.Substring(0, data.Length - 1)).Split('#');
                string[] moveData;

                
                foreach (string move in movesData)
                {
                    moveData = move.Split('|');
                    Int32.TryParse(moveData[3], out int X);
                    Int32.TryParse(moveData[4], out int Y);

                    if (moveData[1] == "Team1")
                    {
                        pcTeam1.ReceiveMove(moveData[2], X, Y);
                    }
                    else
                    {
                        pcTeam2.ReceiveMove(moveData[2], X, Y);
                    }
                }

                break;
            case "Restart":
                pcTeam1.StartAgents();
                pcTeam2.StartAgents();
                break;
            default:
                Debug.Log("Unrecognizable command received");
                break;
        }
    }


    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    private void OnDisable()
    {
        CloseSocket();
    }
    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }

    public void ConnectToServerButton()
    {
        password = passwordInputField.text;
        clientName = clientNameInputField.text;
        CloseSocket();
        try
        {
            ConnectToServer(serverAddressInputField.text, portToConnect);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}

