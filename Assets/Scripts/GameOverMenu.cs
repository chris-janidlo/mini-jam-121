using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeLivedText, killCountText;
    [SerializeField] private FloatVariable timeLived;
    [SerializeField] private IntVariable killCount;

    private void Start()
    {
        timeLivedText.text += $"{timeLived.Value:F2} seconds";
        killCountText.text += killCount.Value.ToString();
    }
}