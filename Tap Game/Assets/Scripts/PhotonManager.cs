using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using Photon.Pun.UtilityScripts;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _gameModes;
    [SerializeField] private GameObject _loading;
    [SerializeField] private GameObject _lobby;

    [SerializeField] private InputField _usernameInput;

    [Header("Lobby Panel")]
    [SerializeField] private RectTransform _playersContainer;
    [SerializeField] private GameObject _playerPrefab;

    private Dictionary<string, RoomInfo> _cachedRoomList;
    private Dictionary<string, GameObject> _roomListEntries;
    private Dictionary<int, GameObject> _playerListEntries;

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        _cachedRoomList = new Dictionary<string, RoomInfo>();
        _roomListEntries = new Dictionary<string, GameObject>();
      
        _usernameInput.text = "Player " + Random.Range(1000, 10000);
        SetActivePanel(_menu.name);

        PhotonNetwork.ConnectUsingSettings();
    }

    #region Callbacks

    public override void OnConnectedToMaster()
    {
        //this.SetActivePanel(_gameModes.name);
    }

    public override void OnJoinedLobby()
    {
        // Whenever this joins a new lobby, clear any previous room lists
        _cachedRoomList.Clear();
        ClearRoomListView();
    }

    // When a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
    public override void OnLeftLobby()
    {
        _cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(_gameModes.name);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(_gameModes.name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room " + Random.Range(1000, 10000);
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        // Joining (or entering) a room invalidates any cached lobby room list (even if LeaveLobby was not called due to just joining a room)
        _cachedRoomList.Clear();
        SetActivePanel(_lobby.name);
        _playerListEntries ??= new Dictionary<int, GameObject>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(_playerPrefab);
            entry.transform.SetParent(_playersContainer);
            entry.transform.localScale = Vector3.one;
            entry.transform.SetAsLastSibling();
            entry.GetComponent<PlayerListEntry>().Initialize(player.ActorNumber, player.NickName);

            if (player.CustomProperties.TryGetValue(GameManager.PLAYER_READY, out object isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
            _playerListEntries.Add(player.ActorNumber, entry);
        }
        CheckPlayersReady();

        Hashtable props = new Hashtable {{ GameManager.PLAYER_LOADED_LEVEL, false }};
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnLeftRoom()
    {
        SetActivePanel(_gameModes.name);

        foreach (GameObject entry in _playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        _playerListEntries.Clear();
        _playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(_playerPrefab);
        entry.transform.SetParent(_playersContainer);
        entry.transform.localScale = Vector3.one;
        entry.transform.SetAsLastSibling();
        entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);
        _playerListEntries.Add(newPlayer.ActorNumber, entry);
        CheckPlayersReady();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(_playerListEntries[otherPlayer.ActorNumber].gameObject);
        _playerListEntries.Remove(otherPlayer.ActorNumber);
        CheckPlayersReady();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            CheckPlayersReady();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (_playerListEntries == null)
        {
            _playerListEntries = new Dictionary<int, GameObject>();
        }

        if (_playerListEntries.TryGetValue(targetPlayer.ActorNumber, out GameObject entry))
        {
            if (changedProps.TryGetValue(GameManager.PLAYER_READY, out object isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
        }
        CheckPlayersReady();
    }

    #endregion

    #region UI Callbacks

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        SetActivePanel(_gameModes.name);
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        SetActivePanel(_loading.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnLoginButtonClicked()
    {
        string playerName = _usernameInput.text;

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            this.SetActivePanel(_gameModes.name);
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel("Game");
    }

    #endregion

    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.PlayerList.Length != 2)
        {
            return false;
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue(GameManager.PLAYER_READY, out object isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        StartCoroutine(StartGame());
        return true;
    }

    private IEnumerator StartGame()
    {
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            Timer.canStartTimer = true;
            yield return new WaitForSeconds(5);
            PhotonNetwork.LoadLevel("Game");
        }
        Timer.canStartTimer = false;
    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in _roomListEntries.Values)
        {
            Destroy(entry);
        }
        _roomListEntries.Clear();
    }

    public void LocalPlayerPropertiesUpdated()
    {
        CheckPlayersReady();
    }

    private void SetActivePanel(string activePanel)
    {
        _menu.SetActive(activePanel.Equals(_menu.name));
        _gameModes.SetActive(activePanel.Equals(_gameModes.name));
        _loading.SetActive(activePanel.Equals(_loading.name));
        _lobby.SetActive(activePanel.Equals(_lobby.name));
    }
}
