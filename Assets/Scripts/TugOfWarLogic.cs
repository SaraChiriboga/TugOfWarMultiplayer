using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TugOfWarLogic : NetworkBehaviour
{
    // Referencia estatica al host, para que RopeVisual la lea
    public static TugOfWarLogic HostInstance;

    // Posicion compartida de la soga (eje X), sincronizada por el servidor
    public NetworkVariable<float> ropePosition = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Limites: si la soga llega a -5 gana el cliente, si llega a +5 gana el host
    private const float WIN_LIMIT = 5f;  // La soga empieza en 0, si llega a +5 arrastra al host al centro, si llega a -5 arrastra al cliente

    private bool gameOver = false;

    public override void OnNetworkSpawn()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // El jugador con clientId=0 es siempre el host
        bool iAmHost = OwnerClientId == 0;

        if (iAmHost)
        {
            sr.color = Color.blue;
            transform.position = new Vector3(-5f, transform.position.y, 0f);
            HostInstance = this;
        }
        else
        {
            sr.color = Color.red;
            transform.position = new Vector3(5f, transform.position.y, 0f);
        }

        // Aplicar posicion inicial segun el valor actual de ropePosition
        float myOffset = iAmHost ? -5f : 5f;
        transform.position = new Vector3(ropePosition.Value + myOffset, transform.position.y, 0f);

        // Todos escuchan el cambio para mover su propio transform
        ropePosition.OnValueChanged += OnRopePositionChanged;
    }

    public override void OnNetworkDespawn()
    {
        ropePosition.OnValueChanged -= OnRopePositionChanged;
        if (IsHost) HostInstance = null;
    }

    // Cuando la soga se mueve, cada jugador actualiza SU PROPIA posicion
    private void OnRopePositionChanged(float oldVal, float newVal)
    {
        bool iAmHost = OwnerClientId == 0;
        float myOffset = iAmHost ? -5f : 5f;
        transform.position = new Vector3(newVal + myOffset, transform.position.y, 0f);
    }

    void Update()
    {
        if (gameOver) return;
        if (!IsOwner) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            bool iAmHost = OwnerClientId == 0;
            PullServerRpc(iAmHost);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void PullServerRpc(bool callerIsHost)
    {
        if (gameOver) return;

        float pull = callerIsHost ? -0.5f : 0.5f;
        ropePosition.Value += pull;

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
        Debug.Log($"¡GANÓ EL {winner}!");
        // Aqui puedes mostrar un panel de victoria en la UI
    }
}