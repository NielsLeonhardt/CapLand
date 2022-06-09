using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour{
    [SerializeField] MultiplayerLand2 multiplayerLandScript;

    void Awake() {
        // multiplayerLandScript = FindObjectOfType<MultiplayerLand>();
    }

    // client catches selected land state
    MultiplayerLand2 selectedLand;
    MultiplayerLand2 landToVisit;
    MultiplayerLand2 landFromUserThatClicked;

    public void HandleLSelectedLands(MultiplayerLand2 scriptFromObjectUserClickedOn){
        //NULL ALL
        if(selectedLand == scriptFromObjectUserClickedOn || scriptFromObjectUserClickedOn == null){
            selectedLand = null;
            landToVisit = null;
            Debug.Log("Land DESELECTED");
            return;
        }
        
        //SELECT LAND AS HOST
        if(selectedLand == null && NetworkManager.Singleton.LocalClientId == 0 && IsHost && scriptFromObjectUserClickedOn.networkLandOwner.Value == MultiplayerLand2.LandOwner.PLAYER1){
            Debug.Log("HOST Land SELECTED");
            selectedLand = scriptFromObjectUserClickedOn;
            landFromUserThatClicked = scriptFromObjectUserClickedOn;
            return;
        }

        //SELECT LAND AS CLIENT
        if(selectedLand == null && NetworkManager.Singleton.LocalClientId == 2 && IsClient && scriptFromObjectUserClickedOn.networkLandOwner.Value == MultiplayerLand2.LandOwner.PLAYER2){

            Debug.Log("CLIENT Land SELECTED");
            selectedLand = scriptFromObjectUserClickedOn;
            landFromUserThatClicked = scriptFromObjectUserClickedOn;
            return;
        }

        //NEXT LAND
        if(selectedLand != scriptFromObjectUserClickedOn && selectedLand != null){
            Debug.Log("VISIT Land SELECTED");
            landToVisit = scriptFromObjectUserClickedOn;
            // landFromUserThatClicked.isAttacking = true;
            multiplayerLandScript.landToVisit = landToVisit;
            landFromUserThatClicked.SendTroopsHandler();
            // multiplayerLandScript.target = landToVisit.transform;
            // multiplayerLandScript.PlayerRotationHandler();
            // selectedLand.SendTroopsHandler(landToVisit);

            //SEND UNITS

            //DESELECT ALL NODES
            selectedLand = null;
            landToVisit = null;
            landFromUserThatClicked = null;
        }
    }
}
