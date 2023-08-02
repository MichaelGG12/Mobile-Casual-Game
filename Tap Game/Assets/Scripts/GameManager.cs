using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public const string PLAYER_READY = "IsPlayerReady";
    public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";

    [Header("Components")]
    [SerializeField] private Text _infoTxt;
    [SerializeField] private RectTransform _gameCanvas;

    private string _winner = "";
    private int _score = -1;
    private bool _timerIsOverStartGame;

    public override void OnEnable()
    {
        base.OnEnable();
        CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
    }

    public void Start()
    {
        Hashtable props = new Hashtable
            {
                {GameManager.PLAYER_LOADED_LEVEL, true}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;
    }

    private void Update()
    {
        if (_timerIsOverStartGame == true)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player.GetScore() > _score)
                {
                    _winner = player.NickName;
                    _score = player.GetScore();
                    _infoTxt.text = $"1st {_winner}";
                }
            }
        }
    }

    private IEnumerator EndOfGame(string winner, int score)
    {
        Timer.canStartTimer = false;
        yield return new WaitForSeconds(2);
        _infoTxt.text = string.Format("{0} won with {1} taps", winner, score);
        yield return new WaitForSeconds(5);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckEndOfGame();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
        bool startTimeIsSet = CountdownTimer.TryGetStartTime(out _);

        if (changedProps.ContainsKey(GameManager.PLAYER_LOADED_LEVEL))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                if (!startTimeIsSet)
                {
                    CountdownTimer.SetStartTime();
                }
            }
            else
            {
                _infoTxt.text = "Waiting for other players...";
            }
        }
    }

    private void StartGame()
    {
        GameObject btn = PhotonNetwork.Instantiate("PlayerBtn", transform.position, Quaternion.identity);
        btn.transform.SetParent(_gameCanvas);

        if (PhotonNetwork.IsMasterClient)
        {

        }
    }

    private bool CheckAllPlayerLoadedLevel()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object playerLoadedLevel;

            if (player.CustomProperties.TryGetValue(GameManager.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
            {
                if ((bool)playerLoadedLevel)
                {
                    continue;
                }
            }
            return false;
        }
        return true;
    }

    public void CheckEndOfGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StopAllCoroutines();
        }

        string winner = "";
        int score = -1;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetScore() > score)
            {
                winner = player.NickName;
                score = player.GetScore();
                Debug.Log(winner + " " + score);
            }
        }
        StartCoroutine(EndOfGame(winner, score));
    }

    private void OnCountdownTimerIsExpired()
    {
        StartGame();
        Timer.canStartTimer = true;
        _timerIsOverStartGame = true;
    }
}