using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TugOfWarLogic : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (IsServer) // El Host siempre es el Servidor
        {
            sr.color = Color.blue;
            // Solo movemos si somos el dueño
            if (IsOwner) transform.position = new Vector3(-7, 0, 0);
        }
        else // El que se une es el Cliente
        {
            sr.color = Color.red;
            if (IsOwner) transform.position = new Vector3(7, 0, 0);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // Si presionas Espacio, pides al servidor que te mueva
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            MoveRequestServerRpc();
        }
    }

    [ServerRpc] // Esto viaja por el Relay hasta el Host
    void MoveRequestServerRpc()
    {
        // El Host jala hacia -infinito, el Cliente hacia +infinito
        float moveDir = IsHost ? -0.5f : 0.5f;
        transform.position += new Vector3(moveDir, 0, 0);

        // Verificar si cruzaste la meta (X=0)
        if (IsHost && transform.position.x > 0) Debug.Log("GANÓ EL HOST");
        if (!IsHost && transform.position.x < 0) Debug.Log("GANÓ EL CLIENTE");
    }
}