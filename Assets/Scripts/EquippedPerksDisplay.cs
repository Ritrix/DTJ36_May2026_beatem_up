using UnityEngine;
using TMPro;

public class EquippedPerksDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text perkText;

    private void Awake()
    {
        if (perkText == null)
            perkText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (perkText == null) return;

        if (GameManager.Instance == null)
        {
            perkText.text = "";
            return;
        }

        perkText.text = GameManager.Instance.GetEquippedPerkNamesText();
    }
}