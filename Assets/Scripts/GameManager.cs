using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject panelVictoria;
    public TextMeshProUGUI textoGanador;
    public GameObject botonRevancha; // solo visible cuando ambos pidieron revancha o el host la inicia

    public NetworkVariable<float> ropePosition = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Cuantos jugadores pidieron revancha (0, 1 o 2)
    private NetworkVariable<int> revanchaVotos = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private const float WIN_LIMIT = 5f;

    // gameOver como NetworkVariable para que ambos lados lo sepan
    private NetworkVariable<bool> gameOver = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        // Escuchar cambios para mostrar/ocultar panel de revancha
        revanchaVotos.OnValueChanged += OnRevanchaVotosChanged;

        if (panelVictoria != null) panelVictoria.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        revanchaVotos.OnValueChanged -= OnRevanchaVotosChanged;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PullServerRpc(bool callerIsHost)
    {
        if (gameOver.Value) return;

        ropePosition.Value += callerIsHost ? -0.5f : 0.5f;

        if (ropePosition.Value >= WIN_LIMIT)
        {
            gameOver.Value = true;
            MostrarGanadorClientRpc("CLIENTE");
        }
        else if (ropePosition.Value <= -WIN_LIMIT)
        {
            gameOver.Value = true;
            MostrarGanadorClientRpc("HOST");
        }
    }

    [ClientRpc]
    void MostrarGanadorClientRpc(string winner)
    {
        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (textoGanador != null) textoGanador.text = $"GANO EL {winner}!";
        if (botonRevancha != null) botonRevancha.SetActive(true);
        Debug.Log($"GANO EL {winner}!");
    }

    // Llamado por el boton de revancha en la UI
    public void PedirRevancha()
    {
        PedirRevanchaServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void PedirRevanchaServerRpc()
    {
        revanchaVotos.Value += 1;

        // Si los dos jugadores pidieron revancha, reiniciar
        if (revanchaVotos.Value >= 2)
        {
            ReiniciarJuegoClientRpc();
            ropePosition.Value = 0f;
            gameOver.Value = false;
            revanchaVotos.Value = 0;
        }
    }

    private void OnRevanchaVotosChanged(int oldVal, int newVal)
    {
        // Mostrar cuantos quieren revancha (opcional)
        if (textoGanador != null && newVal == 1)
            textoGanador.text += "\n(1/2 quiere revancha)";
    }

    [ClientRpc]
    void ReiniciarJuegoClientRpc()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (botonRevancha != null) botonRevancha.SetActive(false);
    }
}