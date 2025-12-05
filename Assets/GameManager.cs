using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance { get; private set; }

    [Header("UI (Set these in inspector per client)")]
    [Tooltip("These panels should exist on each client's scene (deactivated by default).")]
    public GameObject localVictoryPanel;
    public GameObject localDefeatPanel;

    private bool gameEnded = false;
    private int winnerActorNumber = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    #region Public API (called by players / items)
    // Player requests to be declared winner (e.g. collected final item)
    public void RequestPlayerWin(int actorNumber)
    {
        if (gameEnded) return;

        // If this client is master, perform the end directly
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_EndGame), RpcTarget.AllBuffered, actorNumber);
        }
        else
        {
            // ask master client to end the game with this winner
            photonView.RPC(nameof(RPC_RequestWinOnMaster), RpcTarget.MasterClient, actorNumber);
        }
    }

    // Player requests to be declared loser (e.g. died / fell / health 0)
    public void RequestPlayerLose(int actorNumber)
    {
        if (gameEnded) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // Master will pick a winner among remaining players
            HandleLoseOnMaster(actorNumber);
        }
        else
        {
            photonView.RPC(nameof(RPC_RequestLoseOnMaster), RpcTarget.MasterClient, actorNumber);
        }
    }
    #endregion

    #region RPCs (Master and All)
    [PunRPC]
    private void RPC_RequestWinOnMaster(int actorNumber, PhotonMessageInfo info)
    {
        // Only MasterClient runs this
        if (!PhotonNetwork.IsMasterClient) return;
        if (gameEnded) return;

        // Validate (optional): ensure actorNumber is actually in the room
        Photon.Realtime.Player p = FindPlayerByActor(actorNumber);

        if (p == null)
        {
            Debug.LogWarning($"GameManager: RequestWinOnMaster - actor {actorNumber} not found.");
            return;
        }

        photonView.RPC(nameof(RPC_EndGame), RpcTarget.AllBuffered, actorNumber);
    }

    [PunRPC]
    private void RPC_RequestLoseOnMaster(int actorNumber, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (gameEnded) return;

        HandleLoseOnMaster(actorNumber);
    }

    // This RPC runs on ALL clients and finalizes the result locally
    [PunRPC]
    private void RPC_EndGame(int actorNumber, PhotonMessageInfo info)
    {
        if (gameEnded) return;

        gameEnded = true;
        winnerActorNumber = actorNumber;
        OnGameEnded_Local(winnerActorNumber);
    }
    #endregion

    #region Master helpers
    private void HandleLoseOnMaster(int loserActorNumber)
    {
        // mark game ended and pick a winner among remaining players (anyone except loser)
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        int winner = -1;
        foreach (var p in players)
        {
            if (p.ActorNumber != loserActorNumber)
            {
                winner = p.ActorNumber;
                break;
            }
        }

        if (winner == -1)
        {
            Debug.LogWarning("GameManager: No other players found. Ending with no winner.");
            // still broadcast loser as winner fallback
            photonView.RPC(nameof(RPC_EndGame), RpcTarget.AllBuffered, loserActorNumber);
            return;
        }

        photonView.RPC(nameof(RPC_EndGame), RpcTarget.AllBuffered, winner);
    }

    private Photon.Realtime.Player FindPlayerByActor(int actor)
    {
        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == actor) return p;
        }
        return null;
    }   

    #endregion

    #region Local UI handling
    private void OnGameEnded_Local(int winnerActor)
    {
        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;

        bool iAmWinner = (myActor == winnerActor);

        if (iAmWinner)
        {
            ShowVictoryUI();
        }
        else
        {
            // 🔥 نابود کردن بازیکن بازنده
            KillLocalPlayer();

            ShowDefeatUI();
        }
    }

    private void KillLocalPlayer()
    {
        Player localPlayer = FindFirstObjectByType<Player>();
        if (localPlayer != null)
        {
            localPlayer.Die();
        }
    }

    private void ShowVictoryUI()
    {
        if (localVictoryPanel != null) localVictoryPanel.SetActive(true);
        else Debug.LogWarning("GameManager: localVictoryPanel not assigned for this client.");
    }

    private void ShowDefeatUI()
    {
        if (localDefeatPanel != null) localDefeatPanel.SetActive(true);
        else Debug.LogWarning("GameManager: localDefeatPanel not assigned for this client.");
    }
    #endregion
}
