using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerLand2 : NetworkBehaviour
{
    public enum LandOwner{
        NPC,        //NON ATTACKING
        PLAYER1,    //
        PLAYER2     //test
    }

    public enum LandType{
        SMALL_COUNTRY,
        MEDIUM_COUNTRY,
        BIG_COUNTRY,
        LARGE_COUNTRY  
    }
    
    public NetworkVariable<LandOwner> networkLandOwner = new NetworkVariable<LandOwner>(default, NetworkVariableBase.DefaultReadPerm, NetworkVariableWritePermission.Owner);

    [SerializeField] NetworkVariable<bool> networkIsUnderAttack = new NetworkVariable<bool>();
    [SerializeField] NetworkVariable<bool> networkIsAttacking = new NetworkVariable<bool>();
    [SerializeField] NetworkVariable<int> networkCurrentTroopAmount = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    GameManager gameManagerScript;

    public bool isAttacking;
    public bool isUnderAttack;
    public bool stopSendingTroops;

    public MultiplayerLand2 landToVisit;

    [SerializeField] TMP_Text troopAmountText;

    int startTroopAmount = 10;
    int minTroopAmount = 5;
    int maxTroopAmount;

    [SerializeField] LandType landType;

    [SerializeField] Material[] materials; //0 - NPC | 1 - PLAYER1 | 2 - PLAYER2
    Material ownMaterial;

    [SerializeField] GameObject troop1PlaceholderGO;
    [SerializeField] GameObject troop2PlaceholderGO;
    [SerializeField] GameObject troop3PlaceholderGO;
    [SerializeField] GameObject troop4PlaceholderGO;
    [SerializeField] GameObject troop5PlaceholderGO;

    [SerializeField] GameObject troopPrefab;

    [SerializeField] float tTimer;

    // client catches combat state
    bool oldIsAttacking;
    bool oldIsUnderAttack;

    // [SerializeField] Vector2 defaultInitialPosition = new Vector2(-7, 5);
 
    void Awake(){
        // ownMaterial = GetComponent<Renderer>().material;
        gameManagerScript = FindObjectOfType<GameManager>();
        networkCurrentTroopAmount.Value = startTroopAmount;
    }

    void Start(){

        
        if(IsHost && IsOwner && NetworkManager.Singleton.LocalClientId == 0 && IsLocalPlayer){
            transform.position = new Vector3(-2f, 0, -2f);
        }

        if(IsClient && IsOwner && NetworkManager.Singleton.LocalClientId == 1 && IsLocalPlayer){
            transform.position = new Vector3(2f, 0, 2f);
        }

        Later_StartButtonInLobbyPressed();
        LandtypeHandler();
    }

    void Update(){
        if(IsLocalPlayer){
            ClientInput();
        }

        TriggerResetTimer();
        CheckForEnemyAttackHandler();
        UpdateAmountText();
    }

    void FixedUpdate() {
        // TriggerResetTimer();
        CheckForEnemyAttackHandler();
    }

    void LandtypeHandler(){
        switch (landType){
            case LandType.SMALL_COUNTRY:{
                maxTroopAmount = 20;
            }
                break;
            case LandType.MEDIUM_COUNTRY:{
                maxTroopAmount = 40;
            }
                break;
            case LandType.BIG_COUNTRY:{
                maxTroopAmount = 50;
            }
                break;
            case LandType.LARGE_COUNTRY:{
                maxTroopAmount = 70;
            }
                break;
        }
    }

    void ClientInput(){
        Vector3 mousePositionL1;
        Vector3 worldMousePosL1;
        RaycastHit2D hit;

        if(Input.GetMouseButtonDown(0) && IsLocalPlayer){
            mousePositionL1 = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            worldMousePosL1 = Camera.main.ScreenToWorldPoint(mousePositionL1);
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null){
                PlayerRotationHandler(hit.transform.position);
                MultiplayerLand2 scriptFromObjectIClickedOn = hit.collider.GetComponent<MultiplayerLand2>();
                gameManagerScript.HandleLSelectedLands(scriptFromObjectIClickedOn);
                return;
            }
            else
            {
                MultiplayerLand2 scriptFromObjectIClickedOn = null;
                    gameManagerScript.HandleLSelectedLands(scriptFromObjectIClickedOn);
                    return;
            }
        }

        if(oldIsAttacking != isAttacking || oldIsUnderAttack != isUnderAttack){
            oldIsAttacking = isAttacking;
            oldIsUnderAttack = isUnderAttack;
            UpdateClientCombatStateServerRpc(isAttacking, isUnderAttack);
        }
    }

    public void PlayerRotationHandler(Vector3 targetPos){
        Vector3 targetDirection = targetPos - transform.position;

        float angle = Mathf.Atan2(targetPos.y - transform.position.y, targetPos.x - transform.position.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));

        transform.eulerAngles = new Vector3(0, 0, targetRotation.eulerAngles.z - 90);
    }

    void Later_StartButtonInLobbyPressed(){
        StartCoroutine(TroopIncreaseTimeHandler());
    }

    IEnumerator TroopIncreaseTimeHandler(){
        while(true){
            yield return new WaitForSeconds(.2f);
            TroopAmountIncreaseIdleHandler();
        }
    }

    void TroopAmountIncreaseIdleHandler(){
        if(isUnderAttack || isAttacking){ return; }
        networkCurrentTroopAmount.Value++;

        CheckIfTroopLimitReached();
        //UI STUFF
        UpdateAmountText();
    }

    void TroopAmountIncreaseCombatHandler(){
        networkCurrentTroopAmount.Value++;
        //CHECK
        CheckIfTroopLimitReached();
        //UI STUFF
        UpdateAmountText();
    }

    void CheckIfTroopLimitReached(){
        if(networkCurrentTroopAmount.Value > maxTroopAmount){
            networkCurrentTroopAmount.Value = maxTroopAmount;
        }

        if(networkCurrentTroopAmount.Value <= minTroopAmount){
            networkCurrentTroopAmount.Value = minTroopAmount;
            isAttacking = false;
        }

        if(networkCurrentTroopAmount.Value >= minTroopAmount){
            stopSendingTroops = false;
        }
    }

    void UpdateAmountText(){
        troopAmountText.text = "" + networkCurrentTroopAmount.Value /*+ "|" + maxTroopAmount*/;
        // troopAmountText.text = $"Test: {Tester.CurrentTroopAmount.Value}";
    }

    public void LandOwnerHandler(LandOwner landOwner){
        networkLandOwner.Value = landOwner;
        switch(networkLandOwner.Value){ //Land BECOMES
            case LandOwner.PLAYER1:
                GetComponent<SpriteRenderer>().material = materials[1];
                break;
            case LandOwner.PLAYER2:
                GetComponent<SpriteRenderer>().material = materials[2];
                break;
        }
    }

    public void SendTroopsHandler(){
        if(isAttacking && stopSendingTroops) { return; }
        // tTimer = 0f;
        StartCoroutine(SendTroops());
    }

    public IEnumerator SendTroops(){
        isAttacking = true;

            while (!stopSendingTroops)
            {  
                if(networkCurrentTroopAmount.Value <= minTroopAmount){
                    stopSendingTroops = true;
                    isAttacking = false;
                    break;}

                Vector3 spawnPosGO = troop1PlaceholderGO.transform.position;
                    if(isAttacking && IsServer){
                        for (int u = 0; u < 5; u++){
                            switch (u){
                                case 1: 
                                    spawnPosGO = troop1PlaceholderGO.transform.position;
                                    break;

                                case 2:
                                    spawnPosGO = troop2PlaceholderGO.transform.position;
                                    break;

                                case 3:
                                    spawnPosGO = troop3PlaceholderGO.transform.position;
                                    break;

                                case 4:
                                    spawnPosGO = troop4PlaceholderGO.transform.position;
                                    break;

                                case 5:
                                    spawnPosGO = troop5PlaceholderGO.transform.position;
                                    break;

                                default:
                                    break;
                            }
                            networkCurrentTroopAmount.Value--;
                            GameObject troop = Instantiate(troopPrefab, spawnPosGO, transform.rotation);
                            troop.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
                            // troop.GetComponent<NetworkObject>().RemoveOwnership();
                            troop.GetComponent<Troop>().SetTroop(landToVisit, this, GetComponent<SpriteRenderer>().material);
                        }
                    }
                    yield return new WaitForSecondsRealtime(.9f);
            }
            // yield return null;
    }

    void OnTriggerEnter2D(Collider2D other) {
        Troop l = other.GetComponent<Troop>();
        tTimer = 0f;
        HandleIncomingTroop(l.goal.networkLandOwner.Value);
        Destroy(other.gameObject);
    }

    void CheckForEnemyAttackHandler(){
        if(isUnderAttack && tTimer >= 1f){
            isUnderAttack = false;
        }
    }   
    void CheckForAttackHandler(){
        if(isAttacking && tTimer >= 1f){
            isAttacking = false;
        }
    }

    void TriggerResetTimer(){
        tTimer += Time.deltaTime;
    }

    public void HandleSendingTroop(){
        if(networkCurrentTroopAmount.Value <= minTroopAmount/* || stopSendingTroops*/){
            // isAttacking = false;
        }
    }

    public void HandleIncomingTroop(LandOwner realLandOwner){
        if(realLandOwner == this.networkLandOwner.Value){
            TroopAmountIncreaseCombatHandler();
            return;
        }
        else{
            DestroyTroop(realLandOwner);
            //CHECK
        }
    }

    void DestroyTroop(LandOwner landOwner){
        networkCurrentTroopAmount.Value--;
        isUnderAttack = true;
        UpdateAmountText();
        if(networkCurrentTroopAmount.Value <= 0){
            networkLandOwner.Value = landOwner;
            LandOwnerHandler(landOwner);
            // UpdatePlayerOwnerStateServerRpc(landOwner);
        }
    }

    [ServerRpc]
    public void UpdateClientCombatStateServerRpc(bool isAttackingState, bool isUnderAttackState){
        networkIsAttacking.Value = isAttackingState;
        networkIsUnderAttack.Value = isUnderAttackState;
    }
}
