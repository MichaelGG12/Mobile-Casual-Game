using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBtn : MonoBehaviour
{
    private PhotonView _view;

    public Player _owner { get; private set; }

    public void Initializer(Player owner)
    {
        _owner = owner;
    }

    private void Start()
    {
        _view = GetComponent<PhotonView>();
        Initializer(_view.Owner);
    }

    public void AddPlayerScore()
    {
        if (_view.IsMine)
        {
            _owner.AddScore(1);
        }
    }
}
