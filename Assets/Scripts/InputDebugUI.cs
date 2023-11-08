using TMPro;
using UnityEngine;

public class InputDebugUI : MonoBehaviour {
    static public InputDebugUI Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] TextMeshProUGUI ShiftText;
    [SerializeField] TextMeshProUGUI AltText;
    [SerializeField] TextMeshProUGUI SpaceText;
    [SerializeField] TextMeshProUGUI WText;
    [SerializeField] TextMeshProUGUI RText;
    [SerializeField] TextMeshProUGUI AText;
    [SerializeField] TextMeshProUGUI SText;
    [SerializeField] TextMeshProUGUI DText;

    [Header("Mouse")]
    [SerializeField] TextMeshProUGUI mouseSensText;

    private void Awake() {
        Instance = this;
    }
    private void Update() {
        ShiftText.color = Input.GetKey(KeyCode.LeftShift) ? Color.green : Color.black;
        AltText.color = Input.GetKey(KeyCode.LeftAlt) ? Color.green : Color.black;
        SpaceText.color = Input.GetKey(KeyCode.Space) ? Color.green : Color.black;
        WText.color = Input.GetKey(KeyCode.W) ? Color.green : Color.black;
        RText.color = Input.GetKey(KeyCode.R) ? Color.green : Color.black;
        AText.color = Input.GetKey(KeyCode.A) ? Color.green : Color.black;
        SText.color = Input.GetKey(KeyCode.S) ? Color.green : Color.black;
        DText.color = Input.GetKey(KeyCode.D) ? Color.green : Color.black;
    }

    public void SetMouseSensText(float sens) {
        mouseSensText.text = sens.ToString("F3");
    }

}
