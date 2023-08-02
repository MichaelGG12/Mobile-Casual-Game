using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;

public class PlayerInfoOverview : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerOverviewPrefab;
    private Dictionary<int, GameObject> _playerListEntries;

    public void Awake()
    {
        _playerListEntries = new Dictionary<int, GameObject>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(_playerOverviewPrefab);
            entry.transform.SetParent(gameObject.transform);
            entry.transform.localScale = Vector3.one;
            entry.transform.GetChild(0).GetComponent<Text>().text = string.Format("{0}\nScore: {1}", player.NickName, player.GetScore());
            _playerListEntries.Add(player.ActorNumber, entry);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (this._playerListEntries.TryGetValue(otherPlayer.ActorNumber, out _))
        {
            Destroy(_playerListEntries[otherPlayer.ActorNumber]);
            _playerListEntries.Remove(otherPlayer.ActorNumber);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (_playerListEntries.TryGetValue(targetPlayer.ActorNumber, out GameObject entry))
        {
            entry.transform.GetChild(0).GetComponent<Text>().text = string.Format("{0}\nScore: {1}", targetPlayer.NickName, targetPlayer.GetScore());
        }
    }
}
