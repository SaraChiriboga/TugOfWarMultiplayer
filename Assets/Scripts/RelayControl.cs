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
    public GameObject panelInicio;
    public GameObject panelHost;
    public GameObject panelJoin;
    public TextMeshProUGUI codeText;
    public TMP_InputField inputField;

    async void Start()
    {
        panelInicio.SetActive(true);
        panelHost.SetActive(false);
        panelJoin.SetActive(false);

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartRelayHost()
    {
        panelInicio.SetActive(false);
        panelHost.SetActive(true);
        codeText.text = "Generando...";

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
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

            // Ocultar panel cuando el cliente se una
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            codeText.text = "Error: " + e.Message;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == 0) return; // ignorar el host mismo
        panelHost.SetActive(false);
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public void UI_IrAPanelJoin()
    {
        panelInicio.SetActive(false);
        panelJoin.SetActive(true);
    }

    public async void ConfirmarUnionClient()
    {
        string code = inputField.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Ingresa un codigo valido.");
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

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
            panelJoin.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}