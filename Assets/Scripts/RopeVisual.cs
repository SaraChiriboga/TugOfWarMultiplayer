using Unity.Netcode;
using UnityEngine;

public class RopeVisual : MonoBehaviour
{
    // Distancia desde el centro hasta cada jugador (debe coincidir con TugOfWarLogic)
    private const float PLAYER_OFFSET = 5f;

    // Offset visual en X por si el sprite de la soga no tiene el pivot centrado
    [SerializeField] private float visualOffsetX = 0f;

    void Update()
    {
        if (TugOfWarLogic.HostInstance == null) return;

        float ropeX = TugOfWarLogic.HostInstance.ropePosition.Value;

        // Mover solo X, mantener Y y Z originales
        transform.position = new Vector3(ropeX + visualOffsetX, transform.position.y, transform.position.z);

        // Escalar para cubrir la distancia entre jugadores
        float distancia = PLAYER_OFFSET * 2f;
        transform.localScale = new Vector3(distancia, transform.localScale.y, transform.localScale.z);

        transform.rotation = Quaternion.identity;
    }
}