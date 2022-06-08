using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button startServerButton;

    [SerializeField] Button startHostButton;

    [SerializeField] Button startClientButton;

    [SerializeField] TextMeshProUGUI playersInGameText;

    void Awake() {
        Cursor.visible = true;
    }

    void Start()
    {
        startServerButton.onClick.AddListener(() => {
            if(NetworkManager.Singleton.StartServer()){
                Debug.Log("Server started...");
            }
            else{
                Debug.Log("Server could not be started...");
            }
        });

        startHostButton.onClick.AddListener(() => {
            if(NetworkManager.Singleton.StartHost()){
                Debug.Log("Host started...");
            }
            else{
                Debug.Log("Host could not be started...");
            }
        });

        startClientButton.onClick.AddListener(() => {
            if(NetworkManager.Singleton.StartClient()){
                Debug.Log("Client started...");
            }
            else{
                Debug.Log("Client could not be started...");
            }
        });
    }

    // void Update()
    // {
    //     playersInGameText.text = ("Players in game: " + PlayerManager.playersInGame);
    // }
}
