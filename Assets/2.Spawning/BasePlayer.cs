using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BasePlayer : NetworkBehaviour // This is a NetworkBehaviour!
{
    /**
     * INTRO====================================================================
     *   This component can be found in the [Player] prefab, which acts as the
     * default network object that is spawned once the player is connected as a
     * client (if you read the last demo, it is mentioned that the host is 
     * technically also just a client). However, if you connect as a server and
     * server only, there will be no spawns.
     * 
     *   Spawning is different from instantiating. If Player A spawns something,
     * Players B, C, ... can all see the spawned object. If Player A
     * instantiates something, other players cannot see it. Spawning is
     * generally only done when there is a NetworkObject component on the prefab
     * , and that the prefab is registered in the NetworkManager component 
     * somewhere in your scene.
     * 
     *   By convention, if you add a prefab as PlayerPrefab under NetworkManager
     * it will automatically instantiate it upon connection. Other prefabs must
     * be spawned manually.
     * 
     *   Moreover, spawning does not keep non-networked components in sync
     * between different Player views. That is, if there is a normal
     * MonoBehaviour attached to the prefab along with this NetworkBehaviour,
     * that MonoBehaviour's variables, coroutines, etc. will operate SEPARATELY
     * on each player's [game instance].
     * 
     *   To keep these non-networked components in sync, they must rely on
     * networked variables as well as function calls. This will be the focus of
     * the upcoming demos.
     * 
     * [* a game instance is essentially "the player's machine", but there can
     *    be multiple instances of the game running on the same machine, so we
     *    are using the "instance" phrasing. In actual game play, they are
     *    basically interchangeable. ]
     * =========================================================================
     */

    /**
     * BASICS===================================================================
     *   Basic Unity event handlers work for NetworkBehaviours, this is because
     * NetworkBehaviours *inherits* from the MonoBehaviour class.
     * 
     *   However, please know that these only happen in the local view of the
     * player.
     * =========================================================================
     */

    void Start()
    {
        // start works similarly as non-networked objects
        // called before the first frame update
        Logger.Log("Start Called");
    }

    void Update()
    {
        // update works similarly as non-networked objects
        // called once per frame
    }

    /**
     * Net Specific=============================================================
     *   On top of basic events, NetworkBehaviours have events that are specific
     * to networked flow.
     * 
     * [referencing https://docs-multiplayer.unity3d.com/docs/tutorials/helloworld/helloworldtwo]
     * [referending https://docs-multiplayer.unity3d.com/docs/develop/api/Unity.Netcode.NetworkBehaviour/index.html]
     * =========================================================================
     */
    public override void OnGainedOwnership()
    {
        base.OnGainedOwnership();

        Logger.Log($"NetworkBehaviour with id {NetworkBehaviourId} gained ownership");
    }

    public override void OnLostOwnership()
    {
        base.OnLostOwnership();

        Logger.Log($"NetworkBehaviour with id {NetworkBehaviourId} lost ownership");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        transform.position = new Vector3(Random.value * 6 - 3, 0, 0);
        // notice the actual spawned position on each end
        GetComponent<SpriteRenderer>().color = new Color(
            Random.value, 
            Random.value,
            Random.value);

        PrintNetworkedProperties();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        Logger.Log($"NetworkBehaviour with id {NetworkBehaviourId} despawned");
    }

    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        base.OnNetworkObjectParentChanged(parentNetworkObject);
    }

    /// <summary>
    /// Along with events, there are also specific properties related to network
    /// </summary>
    private void PrintNetworkedProperties()
    {
        Logger.Log(
            $"For network behaviour {NetworkBehaviourId} is spawned with properties...\n" + 
            $"  Is Client {IsClient}\n" +
            $"  Is Host {IsHost}\n" +
            $"  Is Server {IsServer}\n" +
            $"  Is running on owner {IsOwner}\n" +
            $"  Is running locally {IsLocalPlayer}\n" +
            $"  Is owned by server {IsOwnedByServer}\n" +
            $"  Is owned by client with id {OwnerClientId}\n" +
            $"  Is also spawned after instantiation {IsSpawned}"
            );
    }

    /**
     * Net Specific=============================================================
     *   Spawning can also be done manually by the methods below
     * =========================================================================
     */


}
