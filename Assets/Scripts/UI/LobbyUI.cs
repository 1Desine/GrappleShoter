using Eflatun.SceneReference;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button createLobbyButton;
    [SerializeField] Button quickJoinButton;
    [SerializeField] Button joinWithCodeButton;
    [SerializeField] TextMeshProUGUI joinWithCodeText;
    [SerializeField] Button leaveButton;
    [SerializeField] SceneReference gameScene;


    private void Awake() {
        createLobbyButton.onClick.AddListener(CreateLobby);
        quickJoinButton.onClick.AddListener(QuickJoin);
        joinWithCodeButton.onClick.AddListener(JoinWithCode);
        leaveButton.onClick.AddListener(Leave);
    }


    async void CreateLobby() {
        await Multiplayer.Instance.CreateLobby();

        SceneLoader.LoadNetwork(gameScene);
        // The host has to change the scene
    }
    async void QuickJoin() {
        await Multiplayer.Instance.QuickJoinLobby();
    }
    async void JoinWithCode() {
        await Multiplayer.Instance.JoinLobbyByCode(joinWithCodeText.text.Trim((char)8203));
    }
    async void Leave() {
        await Multiplayer.Instance.LeaveLobby();
    }

}
