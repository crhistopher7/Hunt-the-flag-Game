using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;

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
    private SimulationController simulationController;

    public MapGenerator mapGeneratorPrefab;
    private bool debugMode = true;
    private string playerControllerTag = Config.TAG_TEAM_1;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        culture.NumberFormat.NumberDecimalSeparator = ".";
    }

    public void SearchSimulationController()
    {
        simulationController = GameObject.Find("SimulationController").GetComponent<SimulationController>();
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

    // Sending message to the server
    public void Send(string data)
    {
        Debug.Log("Send to Server:  " + data);
        if (debugMode)
        {
            OnIncomingData(data);
            return;
        }

        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    // Read messages from the server
    private void OnIncomingData(string data)
    {
        string[] aData = data.Split('|');
        Debug.Log("Received from server: " + data);

        switch (aData[0])
        {
            // requisição de autentificação
            case "WhoAreYou":
                Send("Iam|" + clientName + "|" + password);
                break;

            // foi autenticado, carrega a cena
            case "Authenticated":
                StartSimulation(aData[1]);
                break;

            case "Moves":
                string[] movesData = data.Split('#');
                string[] moveData;
                int objectiveX, objectiveY, deceptiveX, deceptiveY;
                PathType pathType;
               

                foreach (string move in movesData)
                {
                    moveData = move.Split('|');
                    string action = moveData[0];
                    string team = moveData[1];
                    string agent = moveData[2];
                    Enum.TryParse(moveData[3], out pathType);
                    Int32.TryParse(moveData[4], out objectiveX);
                    Int32.TryParse(moveData[5], out objectiveY);
                    Int32.TryParse(moveData[6], out deceptiveX);
                    Int32.TryParse(moveData[7], out deceptiveY);
                    Vector3Int objectivePosition = new Vector3Int(objectiveX, objectiveY, 0);
                    Vector3Int deceptivePosition = new Vector3Int(deceptiveX, deceptiveY, 0);

                    simulationController.ReceiveMove(agent, team, objectivePosition, deceptivePosition, pathType);
                }

                break;
            case "Restart":
                break;
            default:
                Debug.Log("Unrecognizable command received");
                break;
        }
    }

    private void StartSimulation(string strSeed)
    {
        var mapGenerator = Instantiate(mapGeneratorPrefab);
        var seed = int.Parse(strSeed);
        this.seed = seed;

        mapGenerator.GenerateRealMap("C:/100x100.png");
        //mapGenerator.GenerateMap(7);
        mapGenerator.name = "Map Generator";

        SceneManager.LoadScene("MainSystem");
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
        if(debugMode)
        {
            StartSimulation("7");
        }
        else
        {
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

    public void ChangeDebugMode()
    {
        debugMode = !debugMode;
    }

    public void ChangePlayerControllerTag(string tag)
    {
        playerControllerTag = tag;
    }

    public string GetPlayerControllerTag()
    {
        return playerControllerTag;
    }
}

