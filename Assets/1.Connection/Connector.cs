using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// accommodation for net activities that are async by nature
using System.Threading.Tasks;

// unity networking-related code
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;

public class Connector : MonoBehaviour // this is NOT a NetworkBehaviour
{
    /**
     * INTRO====================================================================
     *   The following methods and structs are provided by Unity Relay and 
     * NetCode for managing connections over the  internet across different
     * instances of gameplay.
     *   The connection is P2P, meaning that no game instance is ran on servers.
     * Rather, one of the players' computers act as the server, and the actual 
     * server (provided by Unity Relay) only act as a transmitter between the 
     * server player and the client player.
     * 
     * [referencing https://docs-multiplayer.unity3d.com/docs/relay/relay]
     * =========================================================================
     */

    /**
     * SCENE====================================================================
     *   Before proceeding to the code, please go over the scene [1.Connection]
     *   Under the [Net] object, find the NetworkManager component and
     * UnityTransport component. These two components are responsible for
     * executing the following instructions. Unity Relay is designed to rely on
     * UnityTransport. Without Relay+UnityTransport, multiplayer is only
     * available under the same network (ex. connected to the same wifi).
     *   The current script [Connector] is attached to the same object for the
     * sake of simplicity. But it is not necessary.
     * =========================================================================
     */

    /**
     * NOTE=====================================================================
     *   If any of the below are marked warning by the IDE you are using, please
     * go to Project Preferences > External Tools and regenerate project files.
     *   After that, your IDE should reload the project on its own.
     * =========================================================================
     */

    RelayHostData hostData;
    RelayJoinData joinData;
    public InputField JoinCode;
    public Button Host, Client;

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

    private void Start()
    {
        // due to the asynchronous methods not recognized by Unity Inspector,
        // we add button events through listeners
        Host.onClick.AddListener(async delegate
        {
            await StartHost();
        });

        Client.onClick.AddListener(async delegate
        {
            await StartClient();
        });
    }

    /// <summary>
    /// Allocate an internet connection on Unity Relay and return relevant info
    /// It is uncommon to start a server without it also be a host, so we will
    /// start host directly instead of dealing with servers.s
    /// Hosts are clients who are also servers. This means that their computer is
    /// responsible for hosting the game, while they themselves can also participate
    /// in that hosted game as clients.
    /// </summary>
    public async Task StartHost()
    {
        await Authorize();
        Allocation allocation = await Relay.Instance.CreateAllocationAsync(8);
        //Populate the hosting data
        RelayHostData data = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };

        //Retrieve the Relay join code for our clients to join our party
        data.JoinCode = await Relay.Instance.GetJoinCodeAsync(data.AllocationID);
        hostData = data;

        Transport.SetHostRelayData(
            data.IPv4Address,
            data.Port,
            data.AllocationIDBytes,
            data.Key,
            data.ConnectionData);

        if (NetworkManager.Singleton.StartHost())
        {
            // see Logger (custom class) for implementation
            Logger.Log($"host started at {data.IPv4Address}:{data.Port} with join code {data.JoinCode}");
        } else
        {
            Debug.LogError("could not start host");
        }
        
    }

    /// <summary>
    /// Clients are connected to the game but has limited access to
    ///     1. the list of connected clients
    ///     2. writing to network variables
    /// </summary>
    public async Task StartClient()
    {
        await Authorize();

        //Ask Unity Services for allocation data based on a join code
        JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(JoinCode.text);

        //Populate the joining data
        RelayJoinData data = new RelayJoinData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            IPv4Address = allocation.RelayServer.IpV4
        };
        joinData = data;

        Transport.SetClientRelayData(
            data.IPv4Address,
            data.Port,
            data.AllocationIDBytes,
            data.Key,
            data.ConnectionData,
            data.HostConnectionData);

        if (NetworkManager.Singleton.StartClient())
        {
            // see Logger (custom class) for implementation
            Logger.Log($"client started at {data.IPv4Address}:{data.Port} with join code {data.JoinCode}");
        }
        else
        {
            Debug.LogError("could not start client");
        }
    }

    public async Task Authorize()
    {
        //Initialize the Unity Services engine
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    /// <summary>
    /// RelayHostData represents the necessary informations
    /// for a Host to host a game on a Relay
    /// </summary>
    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    /// <summary>
    /// RelayHostData represents the necessary informations
    /// for a Host to host a game on a Relay
    /// </summary>
    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }

}