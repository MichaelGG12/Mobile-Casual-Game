using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Timer : MonoBehaviourPunCallbacks
{
    [Header("Timer settings")]
    [SerializeField] private bool _isInGameScene;
    [SerializeField] private float _timeInSeconds = 5f;
    [SerializeField] private Text _timerTxt;

    private PhotonManager _photonManager;

    public static bool startGame;
    public static bool canStartTimer;

    private GameManager _gameManager;
    private PhotonView _photonView;

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _photonManager = FindObjectOfType<PhotonManager>();
        _photonView = GetComponent<PhotonView>();
    }

    private void FixedUpdate()
    {
        if (canStartTimer)
        {
            _photonView.RPC("SetTimer", RpcTarget.All);
        }
    }

    [PunRPC]
    public void SetTimer()
    {
        if (_timeInSeconds > 0)
        {
            _timeInSeconds -= Time.deltaTime;
        }
        else
        {
            _timeInSeconds = 0;

            if (_isInGameScene)
            {
                _gameManager.CheckEndOfGame();
            }
        }
        DisplayTimer(_timeInSeconds);
    }

    void DisplayTimer(float remainingTime)
    {
        if (remainingTime < 0) remainingTime = 0;

        float seconds = Mathf.FloorToInt(remainingTime % 60);
        float milliSeconds = (remainingTime % 1) * 9;

        if (_isInGameScene) _timerTxt.text = string.Format("{0:0}:{1:0}", seconds, milliSeconds);
        else _timerTxt.text = $"Loading game... ({string.Format("{0:0}:{1:0}", seconds, milliSeconds)})";
    }
}