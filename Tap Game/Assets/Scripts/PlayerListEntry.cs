using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;

public class PlayerListEntry : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Text _playerNameText;
    [SerializeField] private Button _readyButton;
    [SerializeField] private Color _notReadyButtonColor;
    [SerializeField] private Color _readyButtonColor;

    private PhotonManager _photonManager;
    private Button _leaveButton;
    private int _ownerId;
    private bool _isPlayerReady;

    private void Awake()
    {
        _photonManager = FindAnyObjectByType<PhotonManager>();
        _leaveButton = GameObject.FindGameObjectWithTag("LeaveBtn").GetComponent<Button>();

        if (_photonManager != null && _leaveButton != null)
        {
            _leaveButton.onClick.AddListener(Leave);
        }
    }

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == _ownerId)
        {
            Hashtable initialProps = new Hashtable() { { GameManager.PLAYER_READY, _isPlayerReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            PhotonNetwork.LocalPlayer.SetScore(0);

            _readyButton.onClick.AddListener(() =>
            {
                _isPlayerReady = !_isPlayerReady;
                SetPlayerReady(_isPlayerReady);

                Hashtable props = new Hashtable() { { GameManager.PLAYER_READY, _isPlayerReady } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                if (PhotonNetwork.IsMasterClient)
                {
                    FindObjectOfType<PhotonManager>().LocalPlayerPropertiesUpdated();
                }
            });
        }
        else
        {
            _readyButton.interactable = false;
        }
    }

    public void Initialize(int playerId, string playerName)
    {
        _ownerId = playerId;
        _playerNameText.text = playerName;
    }

    public void SetPlayerReady(bool playerReady)
    {
        _readyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready!" : "Not Ready";
    }

    public void Leave()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == _ownerId)
        {
            _photonManager.OnLeaveGameButtonClicked();
        }
    }
}
