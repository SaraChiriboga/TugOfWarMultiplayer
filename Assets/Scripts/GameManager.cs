using Unity.Netcode;
using UnityEngine;

// Este objeto debe existir en la escena con un NetworkObject adjunto
// NO es spawneado dinamicamente, ya esta en la escena
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public NetworkVariable<float> ropePosition = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private const float WIN_LIMIT = 5f;
    private bool gameOver = false;

    private void Awake()
    {
        Instance = this;
    }

    // Llamado por PlayerController cuando el jugador presiona espacio
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PullServerRpc(bool callerIsHost)
    {
        if (gameOver) return;

        ropePosition.Value += callerIsHost ? -0.5f : 0.5f;

        if (ropePosition.Value >= WIN_LIMIT)
        {
            gameOver = true;
            AnnounceWinnerClientRpc("CLIENTE");
        }
        else if (ropePosition.Value <= -WIN_LIMIT)
        {
            gameOver = true;
            AnnounceWinnerClientRpc("HOST");
        }
    }

    [ClientRpc]
    void AnnounceWinnerClientRpc(string winner)
    {
        gameOver = true;
        Debug.Log($"GANO EL {winner}!");
    }
}