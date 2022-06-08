using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Troop : NetworkBehaviour
{
    public MultiplayerLand2 multiplayerLand;

    public MultiplayerLand2 goal;

    float speed = 2f;

    CircleCollider2D thisCircleCollider2D;

    Vector3 oldPosition;
    Vector3 oldRotation;

    Vector3 currentPosition;
    Vector3 currentRotation;

    [SerializeField] NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    [SerializeField] NetworkVariable<Vector3> networkRotation = new NetworkVariable<Vector3>();

    private void Start() {
        thisCircleCollider2D = GetComponent<CircleCollider2D>();
        StartCoroutine(EnableColliderAfterTime());
    }

    public void SetTroop(MultiplayerLand2 _multiplayerLand, MultiplayerLand2 goalLand, Material mat){
        multiplayerLand = _multiplayerLand;
        goal = goalLand;
        mat = GetComponent<SpriteRenderer>().material;
    }

    void Update(){
        transform.localPosition += transform.up * speed * Time.deltaTime;


        if(oldPosition != currentPosition || oldRotation != currentRotation){
            oldPosition = currentPosition;
            oldRotation = currentRotation;
            UpdateClientPositionAndRotationStateServerRpc(currentPosition, currentRotation);
        }
    }

    [ServerRpc]
    public void UpdateClientPositionAndRotationStateServerRpc(Vector3 isPositionState, Vector3 isRotationState){
        networkPosition.Value = isPositionState;
        networkRotation.Value = isRotationState;
    }

    void OnTriggerEnter2D(Collider2D other){
        MultiplayerLand2 l = other.GetComponent<MultiplayerLand2>();
        if(l != goal){
            // l.HandleIncomingTroop();
            // Destroy(gameObject);
        }
    }

    IEnumerator EnableColliderAfterTime(){
        yield return new WaitForSeconds(1f);
        thisCircleCollider2D.enabled = true;
    }
}