using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance { get; private set; }

    [SerializeField] Image suffocationImage;
    [SerializeField] Gradient suffocationGradient;


    private void Awake() {
        Instance = this;
    }


    public void SetSuffocation(float suffocation01) => suffocationImage.color = suffocationGradient.Evaluate(suffocation01);


}
