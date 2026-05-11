using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// Este script va en el prefab del jugador
public class TugOfWarLogic : NetworkBehaviour
{
    private const float SPAWN_OFFSET = 5f;
    private bool gameOver = false;

    public override void OnNetworkSpawn()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        bool iAmHostPlayer = OwnerClientId == 0;

        sr.color = iAmHostPlayer ? Color.blue : Color.red;

        // Posicion inicial
        ApplyPosition(0f);

        // Escuchar cambios de la UNA ropePosition del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ropePosition.OnValueChanged += OnRopeChanged;
            // Aplicar valor actual por si el cliente se conecto tarde
            ApplyPosition(GameManager.Instance.ropePosition.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ropePosition.OnValueChanged -= OnRopeChanged;
    }

    private void OnRopeChanged(float oldVal, float newVal)
    {
        ApplyPosition(newVal);
    }

    private void ApplyPosition(float rope)
    {
        bool iAmHostPlayer = OwnerClientId == 0;
        float offset = iAmHostPlayer ? -SPAWN_OFFSET : SPAWN_OFFSET;
        transform.position = new Vector3(rope + offset, transform.position.y, 0f);
    }

    void Update()
    {
        if (gameOver || !IsOwner) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PullServerRpc(OwnerClientId == 0);
        }
    }
}