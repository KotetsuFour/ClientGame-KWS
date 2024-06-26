using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class LobbyScript : MonoBehaviour
{
    public Lobby lobby;
    public Lobby joinedLobby;
    public List<Button> joinButtons;
    [SerializeField] private Button joinButtonPrefab;

    [SerializeField] private float heartbeatCount;
    private float heartbeatTimer;
    private float lobbyUpdateNameTimer;

    private static string KEY_START_GAME = "KeyStartGame";
    private static bool waiting;

    // Start is called before the first frame update
    async void Start()
    {
        InitializationOptions options = new InitializationOptions();
        string profile = Guid.NewGuid().ToString().Substring(0, 8);
        options.SetProfile(profile);

        await UnityServices.InitializeAsync(options);
//        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void createLobby(int playersPerTeam, string lobbyName)
    {
        if (StaticData.playerName == null || StaticData.playerName == "")
        {
            return;
        }
        try
        {
            heartbeatTimer = heartbeatCount;
            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, playersPerTeam);
            Debug.Log("Created Lobby");
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
    public async void searchJoins(Transform menusHolder)
    {
        try
        {
            QueryLobbiesOptions qlo = new QueryLobbiesOptions
            {
                Count = 20,
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse qr = await Lobbies.Instance.QueryLobbiesAsync(qlo);
            joinButtons = new List<Button>();
            int count = 0;
            foreach (Lobby lob in qr.Results)
            {
                count++;
                Button join = Instantiate(joinButtonPrefab);
                updateLobbyName(lob, join);
                Button.ButtonClickedEvent happen = new Button.ButtonClickedEvent();
                happen.AddListener(delegate { joinLobby(lob, menusHolder); });
                join.onClick = happen;
                joinButtons.Add(join);
                join.transform.SetParent(StaticData.findDeepChild(menusHolder, "JoinList"));
            }
            Debug.Log($"Buttonized {count} Results");
        } catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
    public void discardJoinButtons()
    {
        if (joinButtons != null)
        {
            foreach (Button btn in joinButtons)
            {
                Destroy(btn);
            }
            joinButtons.Clear();
        }
    }
    private void updateLobbyName(Lobby lob, Button btn)
    {
        StaticData.findDeepChild(btn.transform, "LobbyName").GetComponent<TextMeshProUGUI>()
            .text = $"{lob.Name} ({lob.MaxPlayers - lob.AvailableSlots}/{lob.MaxPlayers})";
    }
    public async void joinLobby(Lobby lob, Transform menusHolder)
    {
        Debug.Log("Join");
        joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lob.Id);
        for (int q = 0; q < menusHolder.childCount; q++)
        {
            menusHolder.GetChild(q).gameObject.SetActive(false);
        }
        StaticData.findDeepChild(menusHolder, "JoinedLobbyName").GetComponent<TextMeshProUGUI>()
            .text = $"{lob.Name} ({lob.MaxPlayers - lob.AvailableSlots}/{lob.MaxPlayers})";
        StaticData.findDeepChild(menusHolder, "JoinedLobby").gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        heartbeat();
    }
    private async void heartbeat()
    {
        if (lobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                heartbeatTimer = heartbeatCount;
                await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                Debug.Log("Sent heartbeat");
            }
        }
    }
    public async void updateLobbyNameWhileWaiting(TextMeshProUGUI lobLabel, TextMeshProUGUI joinLobLabel)
    {
        try
        {
            if (lobbyUpdateNameTimer <= 0)
            {
                lobbyUpdateNameTimer = 1;
                bool host = lobby != null;
                Lobby lob = host ? lobby : joinedLobby;

                lob = await Lobbies.Instance.GetLobbyAsync(lob.Id);
                string msg = $"{lob.Name} ({lob.MaxPlayers - lob.AvailableSlots}/{lob.MaxPlayers})";
                lobLabel.text = msg;
                joinLobLabel.text = msg;

                if (host)
                {
                    lobby = lob;
                    StaticData.findDeepChild(GameObject.Find("NewBattle").transform, "StartGame")
                        .gameObject.SetActive(lob.AvailableSlots == 0);
                    if (waiting)
                    {
                        Debug.Log(NetworkManager.Singleton.ConnectedClients.Count + " clients");
                    }
                    if (waiting && NetworkManager.Singleton.ConnectedClientsList.Count
                        >= lob.MaxPlayers)
                    {
                        NetworkManager.Singleton.SceneManager.LoadScene("Lobby",
                            LoadSceneMode.Single);
//                        SceneManager.LoadScene("Lobby");
                    }
                }
                else if (!waiting)
                {
                    joinedLobby = lob;
                    if (lob.Data != null
                        && lob.Data.ContainsKey(KEY_START_GAME)
                        && lob.Data[KEY_START_GAME].Value != "0"
                        && lobby == null)
                    {
                        Debug.Log("Joined");
                        joinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                        joinedLobby = null;
                        waiting = true;
                    }
                }
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex);
        }
        lobbyUpdateNameTimer -= Time.deltaTime;
    }

    public async void switchToNetcode()
    {
        if (lobby != null)
        {
            try
            {
                string relayJoinCode = await createRelayCode();
                Lobby lob = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject> {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                });
                lobby = lob;
                waiting = true;
                lobbyUpdateNameTimer = 3;
            }
            catch (LobbyServiceException ex)
            {
                Debug.Log(ex);
            }
        }
    }

    private async Task<string> createRelayCode()
    {
        Allocation all = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(all.AllocationId);
        Debug.Log($"Join Code: {joinCode}");
        Debug.Log(NetworkManager.Singleton);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
            all.RelayServer.IpV4,
            (ushort)all.RelayServer.Port,
            all.AllocationIdBytes,
            all.Key,
            all.ConnectionData
        );
        NetworkManager.Singleton.StartHost();
        StaticData.playerId = 0;
        return joinCode;
    }

    private async void joinRelay(string joinCode)
    {
        JoinAllocation all = await RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
            all.RelayServer.IpV4,
            (ushort)all.RelayServer.Port,
            all.AllocationIdBytes,
            all.Key,
            all.ConnectionData,
            all.HostConnectionData
        );
        NetworkManager.Singleton.StartClient();
        StaticData.playerId = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
