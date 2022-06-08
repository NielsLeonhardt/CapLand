using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefabA;
    [SerializeField] GameObject playerPrefabB;
    [SerializeField] GameObject myPrefab;

    [ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn

    public void SpawnPlayerServerRpc (ulong clientId, int prefabId){
        GameObject newPlayer;
        if(prefabId == 0)
            newPlayer = (GameObject)Instantiate(playerPrefabA);
        else
            newPlayer = (GameObject)Instantiate(playerPrefabB);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true); //true stands for "destroy with scene"
        // SpawnTestObject();
    }

    public void MP_HostAGame(){
        MP_Host();
    }

    public void MP_JoinAGame(){
        MP_Client();
    }

    public void MP_Host(){
        print("HOST A GAME PRESSED");
        NetworkManager.Singleton.StartHost();
    }

    public void MP_Client(){
        print("JOIN A GAME PRESSED");
        NetworkManager.Singleton.StartClient();
    }

    public override void OnNetworkSpawn(){
        SpawnTestObject();

        if(IsServer)
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 0);

        else if(IsClient)
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 1);
        
        else
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 2);
    }


    public void SpawnTestObject(){
        if(IsServer){
            GameObject go = Instantiate(myPrefab, new Vector3(0,0,0), Quaternion.identity);
            // go.GetComponent<NetworkObject>().RemoveOwnership();
            go.GetComponent<NetworkObject>().Spawn();
            // go.GetComponent<NetworkObject>().RemoveOwnership();
        }
    }

}