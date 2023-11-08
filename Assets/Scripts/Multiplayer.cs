#if UNITY_EDITOR
using ParrelSync;
#endif
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

// reference https://youtu.be/zimljd4Rxr0?si=CWL_u6np3ac-z5Gf


public enum EncryptiontionType {
    DTLS, // Datagram Transport Layer Security
    WSS, // Web Socket Secure
}
// Note: Also Udp and Ws are posible choices


public class Multiplayer : MonoBehaviour {
    static public Multiplayer Instance { get; private set; }

    [SerializeField] string lobbyName;
    [SerializeField] int maxPlayers = 4;
    [SerializeField] EncryptiontionType encryptiontion = EncryptiontionType.DTLS;

    public string playerId { get; private set; }
    public string playerName { get; private set; }

    Lobby currentLobby;

    const string keyJoinCode = "RelayJoinCode";

    const float lobbyHeartbeatInterval = 20f;
    const float lobbyPollInterval = 65f;
    HartbeatTimer heartbeatTimer = new HartbeatTimer(lobbyHeartbeatInterval);
    HartbeatTimer pollForUpdatesTimer = new HartbeatTimer(lobbyPollInterval);

    string connectionType => encryptiontion.ToString().ToLower();



    private async void Start() {
        Instance = this;
        DontDestroyOnLoad(this);

        await Authenticate(null);

        heartbeatTimer.OnHartbeat += HandleHeartbeatAsync;
        pollForUpdatesTimer.OnHartbeat += HandlePollForUpdatesAsync;


    }
    private void OnDestroy() {
        StopHeartBeatAnPolling();
    }
    void StopHeartBeatAnPolling() {
        heartbeatTimer.Stop();
        pollForUpdatesTimer.Stop();
    }

    async void HandleHeartbeatAsync() {
        try {
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            Debug.Log("Sent heartbeat ping to lobby: " + currentLobby.Name);
        }
        catch (LobbyServiceException e) {
            Debug.LogError("me. Failed to heartbeat lobby: " + e.Message);
        }
    }
    async void HandlePollForUpdatesAsync() {
        try {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            Debug.Log("Pulled for updates on lobby: " + lobby.Name);
        }
        catch (LobbyServiceException e) {
            Debug.LogError("me. Failed to pull for updates on lobby: " + e.Message);
        }
    }
    class HartbeatTimer {
        public HartbeatTimer(float seconds) {
            this.seconds = seconds;
            currentSeconds = seconds;
        }
        public HartbeatTimer WithTickTime(int tickTimeMS) {
            this.tickTimeMS = tickTimeMS;
            return this;
        }

        public Action OnHartbeat = () => { };

        float currentSeconds;
        float seconds;
        int tickTimeMS = 1000;

        CancellationTokenSource cancellationTokenSource;

        public void Start() {
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(Tick, cancellationTokenSource.Token);
        }
        async void Tick() {
            currentSeconds -= tickTimeMS / 1000;

            if (currentSeconds <= 0) {
                OnHartbeat();
                currentSeconds = seconds;
            }

            try {
                await Task.Delay(tickTimeMS, cancellationTokenSource.Token);
                Tick();
            }
            catch {
                Debug.Log("Task \"HartbeatTimer\" was cancelled!");
                return;
            }
        }
        public void Stop() => cancellationTokenSource?.Cancel();
    }


    async Task Authenticate(string playerName) {
        playerName = $"{playerName ?? "guest"}";

        if (UnityServices.State == ServicesInitializationState.Uninitialized) {
            InitializationOptions options = new InitializationOptions();

            options.SetProfile(playerName);

            await UnityServices.InitializeAsync(options);
        }

#if UNITY_EDITOR
        if (ClonesManager.IsClone()) {
            AuthenticationService.Instance.SwitchProfile(ClonesManager.GetArgument());
            playerName = ClonesManager.GetArgument();
        }
#endif
        Debug.Log("Profile: " + AuthenticationService.Instance.Profile);


        AuthenticationService.Instance.SignedIn += () =>
            Debug.Log("Signed in as: " + playerName + ". <color=green>with id:</color> " + AuthenticationService.Instance.PlayerId);

        if (AuthenticationService.Instance.IsSignedIn == false) {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerId = AuthenticationService.Instance.PlayerId;
            this.playerName = playerName;
        }
    }
    public async Task CreateLobby() {
        try {
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetReleyJoinCode(allocation);

            CreateLobbyOptions options = new CreateLobbyOptions {
                IsPrivate = false,
                Player = GetPlayer(),
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Created lobby: " + currentLobby.Name + " with code " + currentLobby.LobbyCode);

            heartbeatTimer.Start();
            pollForUpdatesTimer.Start();

            await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    {keyJoinCode, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                },
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                allocation, connectionType));

            NetworkManager.Singleton.StartHost();

            PrintPlayers();
        }
        catch (LobbyServiceException e) {
            Debug.LogError("me. Failed to allocate relay: " + e.Message);
        }
    }
    async Task<Allocation> AllocateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            return allocation;
        }
        catch (RelayServiceException e) {
            Debug.LogError("me. Failed to allocate relay: " + e.Message);
            return default;
        }
    }
    async Task<string> GetReleyJoinCode(Allocation allocation) {
        try {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e) {
            Debug.LogError("me. Failed to allocate relay: " + e.Message);
            return default;
        }
    }
    public async Task QuickJoinLobby() {
        try {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions {
                Player = GetPlayer(),
            };
            currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            pollForUpdatesTimer.Start();

            string relayJoinCode = currentLobby.Data[keyJoinCode].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                joinAllocation, connectionType));

            NetworkManager.Singleton.StartClient();

            PrintPlayers();
        }
        catch (LobbyServiceException e) {
            Debug.LogError("me. Failed to quick join lobby: " + e.Message);
        }
    }
    async Task<JoinAllocation> JoinRelay(string relayJoinCode) {
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e) {
            Debug.LogError("me. Failed to join relay: " + e.Message);
            return default;
        }
    }
    public async Task JoinLobbyByCode(string code) {
        try {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer(),
            };
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            pollForUpdatesTimer.Start();

            NetworkManager.Singleton.StartClient();

            PrintPlayers();
        }
        catch (LobbyServiceException e) {
            Debug.LogError("me. Failed to join lobby with code: " + e.Message + ". Reason: " + e.Reason);
        }
    }
    public async Task LeaveLobby() {
        try {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
            StopHeartBeatAnPolling();
            Debug.Log("Left the lobby");
        }
        catch (RelayServiceException e) {
            Debug.LogError("me. Failed to leave lobby: " + e.Message);
        }
    }
    //public async Task KickPlayer(int playerId) {
    //    try {
    //        await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
    //        Debug.Log("Left the lobby");
    //    }
    //    catch (RelayServiceException e) {
    //        Debug.LogError("me. Failed to leave lobby: " + e.Message);
    //    }
    //}

    Player GetPlayer() {
        return new Player {
            Data = new Dictionary<string, PlayerDataObject> {
                { playerId , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
            }
        };
    }
    void PrintPlayers() {
        Debug.Log("<color=green>Players in Lobby</color> " + currentLobby.Name);
        foreach (var player in currentLobby.Players) {
            Debug.Log(player.Data[player.Id].Value + " <color=green>With ID:</color> " + player.Id);
        }
    }

}
