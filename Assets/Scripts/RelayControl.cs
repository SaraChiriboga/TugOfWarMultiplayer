using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;

public class RelayControl : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject panelInicio; // El que tiene Host y Join
    public GameObject panelHost;   // El que tiene el texto amarillo del código
    public GameObject panelJoin;   // El que tiene el InputField

    public TextMeshProUGUI codeText;
    public TMP_InputField inputField;

    async void Start()
    {
        // Al empezar, nos aseguramos de que solo se vea el inicio
        panelInicio.SetActive(true);
        panelHost.SetActive(false);
        panelJoin.SetActive(false);

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // ESTO LO LLAMA EL BOTÓN "HOST"
    public async void StartRelayHost()
    {
        // Cambiamos la UI antes de esperar al servidor
        panelInicio.SetActive(false);
        panelHost.SetActive(true);
        codeText.text = "Generando...";

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            codeText.text = joinCode;

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }

    // ESTO LO LLAMA EL BOTÓN "JOIN"
    public void UI_IrAPanelJoin()
    {
        panelInicio.SetActive(false);
        panelJoin.SetActive(true);
    }

    // ESTO LO LLAMA EL BOTÓN "CONECTAR" DENTRO DEL PANEL JOIN
    public async void ConfirmarUnionClient()
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputField.text);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();

            // Si conecta bien, apagamos todo el menú
            panelJoin.SetActive(false);
        }
        catch (RelayServiceException e) { Debug.Log(e); }
    }
}