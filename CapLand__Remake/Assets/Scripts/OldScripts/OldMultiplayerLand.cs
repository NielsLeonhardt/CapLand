// using System.Collections;
// using TMPro;
// using Unity.Netcode;
// using UnityEngine;

// public class OldMultiplayerLand : NetworkBehaviour
// {
//     public enum LandOwner{
//         NPC,        //NON ATTACKING
//         PLAYER1,    //
//         PLAYER2     //test
//     }

//     public enum LandType{
//         SMALL_COUNTRY,
//         MEDIUM_COUNTRY,
//         BIG_COUNTRY,
//         LARGE_COUNTRY  
//     }

//     public NetworkVariable<LandOwner> networkLandOwner = new NetworkVariable<LandOwner>();

//     [SerializeField] NetworkVariable<int> networkCurrentTroopAmount = new NetworkVariable<int>();
//     [SerializeField] NetworkVariable<bool> networkIsUnderAttack = new NetworkVariable<bool>();
//     [SerializeField] NetworkVariable<bool> networkIsAttacking = new NetworkVariable<bool>();
//     [SerializeField] NetworkVariable<LandType> networkLandType = new NetworkVariable<LandType>();

//     [SerializeField] Vector2 defaultInitialPosition = new Vector2(-15, 15);
//     [SerializeField] TMP_Text troopAmountText;
//     [SerializeField] LandType landType;

//     public static bool isAttacking;
//     public static bool isUnderAttack;

//     // client catches TroopAmountLimits
//     int startTroopAmount = 10;
//     int minTroopAmount = 5;
//     int maxTroopAmount;

//     [SerializeField] Material[] materials; //0 - NPC | 1 - PLAYER1 | 2 - PLAYER2
//     Material ownMaterial;

//     // client catches combat state
//     bool oldIsAttacking;
//     bool oldIsUnderAttack;

//     // client catches landowner

//     //client catches rotation
//     Vector3 oldRotation;

//     [SerializeField] GameObject troop1PlaceholderGO;
//     [SerializeField] GameObject troop2PlaceholderGO;
//     [SerializeField] GameObject troop3PlaceholderGO;
//     [SerializeField] GameObject troop4PlaceholderGO;
//     [SerializeField] GameObject troop5PlaceholderGO;

//     GameManager gameManagerScript;

//     [SerializeField] GameObject troopPrefab;

//     public MultiplayerLand landToVisit;

//     float tTimer;

//     void Awake(){
//         ownMaterial = GetComponent<Renderer>().material;
//         gameManagerScript = FindObjectOfType<GameManager>();
//     }

//     void Start(){
//         if(IsClient && IsOwner){
//             transform.position = new Vector3(Random.Range(defaultInitialPosition.x, defaultInitialPosition.y), 0,
//             Random.Range(defaultInitialPosition.x, defaultInitialPosition.y));
//         }
//         Later_StartButtonInLobbyPressed();
//         LandtypeHandler();
//     }

//     void Update(){
//         NewUpdateForHost();
//         NewUpdateForClient();
//         UpdateAmountText();
//     }

//     void FixedUpdate() {
//         TriggerResetTimer();
//         CheckForEnemyAttackHandler();
//     }

//     void Later_StartButtonInLobbyPressed(){
//         StartCoroutine(TroopIncreaseTimeHandler());
//     }

//     IEnumerator TroopIncreaseTimeHandler(){
//         while(true){
//             yield return new WaitForSeconds(.2f);
//             TroopAmountIncreaseIdleHandler();
//         }
//     }

//     void TroopAmountIncreaseIdleHandler(){
//         if(isUnderAttack || isAttacking){ return; }

//             networkCurrentTroopAmount.Value++;

//             CheckIfTroopLimitReached();
//             //UI STUFF
//             UpdateAmountText();
//     }

//      // void TroopAmountIncreaseCombatHandler(){
//     //     networkCurrentTroopAmount.Value++;
//     //     //CHECK
//     //     CheckIfTroopLimitReached();
//     //     //UI STUFF
//     //     UpdateAmountText();
//     // }

//     void TroopSpawner2(){
//         networkCurrentTroopAmount.Value++;
//         //CHECK
//         CheckIfTroopLimitReached();
//         //UI STUFF
//         UpdateAmountText();
//     }

//     void CheckIfTroopLimitReached(){
//         if(networkCurrentTroopAmount.Value > maxTroopAmount){
//             networkCurrentTroopAmount.Value = maxTroopAmount;
//         }

//         if(networkCurrentTroopAmount.Value <= minTroopAmount){
//             networkCurrentTroopAmount.Value = minTroopAmount;
//             isAttacking = false;
//         }
//     }

//     void UpdateAmountText(){
//         troopAmountText.text = "" + networkCurrentTroopAmount.Value /*+ "|" + maxTroopAmount*/;
//         // troopAmountText.text = $"Test: {Tester.CurrentTroopAmount.Value}";
//     }

//     void ClientInput(){
//         Vector3 mousePositionL1;
//         Vector3 worldMousePosL1;
//         RaycastHit2D hit;
//         if(Input.GetMouseButtonDown(0)){
//             mousePositionL1 = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
//             worldMousePosL1 = Camera.main.ScreenToWorldPoint(mousePositionL1);
//             hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

//             if (hit.collider != null){
//                 MultiplayerLand scriptFromObjectIClickedOn = hit.collider.GetComponent<MultiplayerLand>();
//                 if(scriptFromObjectIClickedOn.networkLandOwner.Value == LandOwner.PLAYER1){
//                     gameManagerScript.HandleLSelectedLands(scriptFromObjectIClickedOn);
//                     PlayerRotationHandler(hit.transform.position);
//                     return;
//                 }
//             }
//             else
//             {
//                 MultiplayerLand scriptFromObjectIClickedOn = null;
//                     gameManagerScript.HandleLSelectedLands(scriptFromObjectIClickedOn);
//                     return;
//             }
//         }

//         if(oldIsAttacking != isAttacking || oldIsUnderAttack != isUnderAttack){
//             oldIsAttacking = isAttacking;
//             oldIsUnderAttack = isUnderAttack;
//             UpdateClientCombatStateServerRpc(isAttacking, isUnderAttack);
//         }
//     }

//     void HostInput(){
//         Vector3 mousePositionL1;
//         Vector3 worldMousePosL1;
//         RaycastHit2D hit;
//         if(Input.GetMouseButtonDown(0)){
//             mousePositionL1 = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
//             worldMousePosL1 = Camera.main.ScreenToWorldPoint(mousePositionL1);
//             hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

//             if (hit.collider != null){
//                 MultiplayerLand scriptFromObjectIClickedOn = hit.collider.GetComponent<MultiplayerLand>();
//                 if(scriptFromObjectIClickedOn.networkLandOwner.Value == LandOwner.PLAYER2){
//                     gameManagerScript.HandleLSelectedLands(scriptFromObjectIClickedOn);
//                     PlayerRotationHandler(hit.transform.position);
//                     return;
//                 }
//             }
//             else
//             {
//                 MultiplayerLand scriptFromObjectIClickedOn = null;
//                     gameManagerScript.HandleLSelectedLands(scriptFromObjectIClickedOn);
//                     return;
//             }
//         }

//         if(oldIsAttacking != isAttacking || oldIsUnderAttack != isUnderAttack){
//             oldIsAttacking = isAttacking;
//             oldIsUnderAttack = isUnderAttack;
//             UpdateClientCombatStateServerRpc(isAttacking, isUnderAttack);
//         }
//     }

//     void LandtypeHandler(){
//         switch (landType){
//             case LandType.SMALL_COUNTRY:{
//                 maxTroopAmount = 20;
//             }
//                 break;
//             case LandType.MEDIUM_COUNTRY:{
//                 maxTroopAmount = 40;
//             }
//                 break;
//             case LandType.BIG_COUNTRY:{
//                 maxTroopAmount = 50;
//             }
//                 break;
//             case LandType.LARGE_COUNTRY:{
//                 maxTroopAmount = 70;
//             }
//                 break;
//         }
//     }

//     public void LandOwnerHandler(LandOwner f){
//         networkLandOwner.Value = f;
//         switch(networkLandOwner.Value){ //Land BECOMES
//             case LandOwner.PLAYER1:
//                 GetComponent<SpriteRenderer>().material = materials[1];
//                 break;
//             case LandOwner.PLAYER2:
//                 GetComponent<SpriteRenderer>().material = materials[2];
//                 break;
//         }


//     }



//     public void PlayerRotationHandler(Vector3 targetPos){
//         Vector3 targetDirection = targetPos - transform.position;

//         float angle = Mathf.Atan2(targetPos.y - transform.position.y, targetPos.x - transform.position.x) * Mathf.Rad2Deg;

//         Quaternion targetRotation = targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));

//         transform.eulerAngles = new Vector3(0, 0, targetRotation.eulerAngles.z - 90);
//     }

//     public void SendTroopsHandler(){
//         // if(isAttacking /*|| stopSendingTroops) */ ){ return; }
//         StartCoroutine(SendTroops());
//     }

//     public IEnumerator SendTroops(){
//         Vector3 spawnPosGO = new Vector3(0, 0, 0);
//         if(isAttacking){
//             for (int u = 1; u < 5; u++){
//                 switch (u){
//                     case 1: 
//                         spawnPosGO = troop1PlaceholderGO.transform.position;
//                         break;

//                     case 2:
//                         spawnPosGO = troop2PlaceholderGO.transform.position;
//                         break;

//                     case 3:
//                         spawnPosGO = troop3PlaceholderGO.transform.position;
//                         break;

//                     case 4:
//                         spawnPosGO = troop4PlaceholderGO.transform.position;
//                         break;

//                     case 5:
//                         spawnPosGO = troop5PlaceholderGO.transform.position;
//                         break;

//                     default:
//                         break;
//                 }
//                 networkCurrentTroopAmount.Value--;
//                 GameObject troop = Instantiate(troopPrefab, spawnPosGO, transform.rotation);
//                 troop.GetComponent<NetworkObject>().Spawn();
//                 troop.GetComponent<NetworkObject>().RemoveOwnership();
//                 troop.GetComponent<Troop>().SetTroop(landToVisit, this, GetComponent<SpriteRenderer>().material);
//             }
//         }
//             yield return new WaitForSeconds(0.05f);

//     }

//     void OnTriggerEnter2D(Collider2D other) {
//         Troop l = other.GetComponent<Troop>();
//         tTimer = 0;
//         HandleIncomingTroop(l.goal.networkLandOwner.Value);
//         Destroy(other.gameObject);
//     }

//     void CheckForEnemyAttackHandler(){
//         if(isUnderAttack && tTimer >= 1f){
//             isUnderAttack = false;
//         }
//     }   
//     void CheckForAttackHandler(){
//         if(isAttacking && tTimer >= 1f){
//             isAttacking = false;
//         }
//     }

//     void TriggerResetTimer(){
//         tTimer += Time.deltaTime;
//     }

//     public void HandleSendingTroop(){
//         if(networkCurrentTroopAmount.Value <= minTroopAmount/* || stopSendingTroops*/){
//             isAttacking = false;
//         }
//     }

//     public void HandleIncomingTroop(LandOwner realLandOwner){
//         if(realLandOwner == this.networkLandOwner.Value){
//             TroopSpawner2();
//             return;
//         }
//         else{
//             DestroyTroop(realLandOwner);
//             //CHECK
//         }
//     }

//     void DestroyTroop(LandOwner landOwner){
//         networkCurrentTroopAmount.Value--;
//         isUnderAttack = true;
//         UpdateAmountText();
//         if(networkCurrentTroopAmount.Value <= 0){
//             networkLandOwner.Value = landOwner;
//             LandOwnerHandler(landOwner);
//             UpdatePlayerOwnerStateServerRpc(landOwner);
//         }
//     }

//     void AutoStartAttack(){
//         if(isAttacking){
//             HandleSendingTroop();
//             SendTroopsHandler();
//         }
//     }

//     [ServerRpc]
//     public void UpdateClientCombatStateServerRpc(bool isAttackingState, bool isUnderAttackState){
//         networkIsAttacking.Value = isAttackingState;
//         networkIsUnderAttack.Value = isUnderAttackState;
//     }

//     [ServerRpc]
//         public void UpdatePlayerOwnerStateServerRpc(LandOwner landowner){

//         }
// }