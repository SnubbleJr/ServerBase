using UnityEngine;
using System.Collections;

public class MainMenuGui : MonoBehaviour {
	
    public string theGame = "Ed's Server Base";
    public string levelName = "testScene";
	public static bool localPlay = false; //Set to true when local game starts

    private bool useMaster, isRequestingHostList, refreshHosts = true;
    private HostData[] hosts;

    private int port = 25000, maxPlayers = 5, conPort = 25000;
    private string serverName = "Game 1", ip = "localhost";
	
	//GUI controls
    private bool showDediPanel, showClientPanel, showMainPanel = true;

    void OnGUI()
    {
        if (showDediPanel)
        {
            displayDediSettings();
            return; //make sure nothing else renders
        }
        if (showClientPanel)
        {
            displayClientSettings();
        }
        if (showMainPanel)
        {
            displayMainPanel();
        }
    }

    //this for initialization
	void Start () {
        Netman.levelName = levelName;
	}
	
	// Update is called once per frame
	void Update () {
        if ((refreshHosts && !isRequestingHostList))
        {
            MasterServer.RequestHostList(theGame);
            refreshHosts = false;
            isRequestingHostList = true;
        }

        if (isRequestingHostList)
        {
            if (MasterServer.PollHostList().Length > 0)
            {
                hosts = MasterServer.PollHostList();
                isRequestingHostList = false;
            }
        }
	}

    
	void OnFailedToConnect(NetworkConnectionError error)
    {
		Debug.Log("Could not connect to server: "+ error);
	}
	
	void OnServerInitialized()
    {
		Debug.Log("Server initialized and ready - loading " + Netman.levelName);
		Application.LoadLevel(Netman.levelName);
		Debug.Log("Connections: " + Network.connections.Length);
	}
	
	//////////////////////////////////////////////////////////////////
	// DISPLAY MENU FUNCTIONS
	//////////////////////////////////////////////////////////////////
	
	/**
	 * Render the menu controls for starting a dedicated server
	 */
	private void displayDediSettings()
    {
		//Port Area
		GUILayout.BeginArea(new Rect(10, 10, 100, 50));
			GUILayout.Label("Port: ");
			GUI.TextField(new Rect(0, 20, 50, 21), port + "");
		GUILayout.EndArea();
		
		
		//Max Players
		GUILayout.BeginArea(new Rect(65, 10, 100, 50));
			GUILayout.Label("Max. Players: ");
			GUI.TextField(new Rect(0, 20, 50, 21), maxPlayers+"");
		GUILayout.EndArea();
		
		//Game Name
		GUILayout.BeginArea(new Rect(155, 10, 200, 50));
			GUILayout.Label("Game Name: ");
			serverName = GUI.TextField(new Rect(0, 20, 150, 21), serverName);
		GUILayout.EndArea();
		
		Netman.levelName = levelName;
		
		useMaster = GUI.Toggle(new Rect(195, 60, 130, 19), useMaster, "Use Master Server");
		
		if (GUI.Button(new Rect(10,60,180,25), "Start a Dedicated Server"))
        {
			Network.InitializeServer(maxPlayers, port, !Network.HavePublicAddress());
			if (useMaster)
            {
				MasterServer.RegisterHost(theGame, serverName);
			}
		}
		
		if (GUI.Button(new Rect(10, 90, 100, 25), "Main Menu")) {
			showMainPanel = true;
			showClientPanel = false;
			showDediPanel = false;
		}
	}
	
	private void displayClientSettings() {
		//Local Game
		
		GUILayout.BeginArea(new Rect(10, 10, 100, 50));
			GUILayout.Label("Direct connect");
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(10, 30, 100, 50));
			GUILayout.Label("IP or Host name: ");
			ip = GUI.TextField(new Rect(0, 20, 80, 21), ip);
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(120, 30, 100, 50));
			GUILayout.Label("Game Port: ");
			GUI.TextField(new Rect(0, 20, 80, 21), conPort+"");
		GUILayout.EndArea();
		if (GUI.Button(new Rect(10, 80, 100, 24), "Connect"))
        {
			Debug.Log("Connecting to " + ip + ":" + conPort);
			Network.Connect(ip, conPort);
		}
		if (GUI.Button(new Rect(10, 110, 100, 24), "Main Menu"))
        {
			showMainPanel = true;
			showClientPanel = false;
			showDediPanel = false;
		}
		
		GUILayout.BeginArea(new Rect(250, 10, 200, Screen.height - 10));
			GUILayout.BeginArea(new Rect(5, 0, 100, 50));
				GUILayout.Label("Server List: ");
			GUILayout.EndArea();
			
			int lastPosition = 50;
			if (hosts != null)
            {
				for (int i = 0; i < hosts.Length; i++ )
                {
					if (GUI.Button(new Rect(0, 10 + (40 * i), 180, 30), hosts[i].gameName))
                    {
						Network.Connect(hosts[i]);
						lastPosition = 245 * (40 * i);
					}
				}
			}
			if (GUI.Button(new Rect(0, lastPosition, 95, 25), "Refresh Hosts"))
            {
				refreshHosts = true;
				isRequestingHostList = false;
			}
		GUILayout.EndArea();
	}
	
	private void displayMainPanel() {
		GUILayout.BeginArea(new Rect(10, 10, 200, 120));
        GUILayout.Label("Welcome to " + theGame + " (with net code provide by damagefilter at playback.net)\n\nTo host a local game start\none instance of the game\nas server, then start another\nto connect.");
		GUILayout.EndArea();
		
		if (GUI.Button(new Rect(210, 10, 100, 25), "Start a Server"))
        {
			showMainPanel = false;
			showClientPanel = false;
			showDediPanel = true;
		}
		
		if (GUI.Button(new Rect(210, 50, 150, 25), "Connect to a Server ..."))
        {
			showMainPanel = false;
			showClientPanel = true;
			showDediPanel = false;
		}
	}
}
