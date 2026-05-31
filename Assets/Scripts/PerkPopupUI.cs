using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkPopupUI : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private GameObject popupPanel;

    [Header("Slot Buttons")]
    [SerializeField] private Button[] slotButtons; // Size 4

    [Header("Slot Description Text")]
    [SerializeField] private TMP_Text[] slotTexts; // Size 4

    [Header("Locked Slot 4")]
    [SerializeField] private GameObject slot4LockedVisual;

    private GameManager gameManager;

    private PerkData currentPerk;

    private void Awake()
    {
        gameManager = GameManager.Instance; // or FindObjectOfType<GameManager>()
        popupPanel.SetActive(false);
    }

    public void OpenPopup(PerkData perk)
    {
        RefreshSlots();
        popupPanel.SetActive(true);
        currentPerk = perk;
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            PerkData perk = gameManager.GetEquippedPerkInSlot(i);

            string label = $"Perk slot {i + 1}:";

            if (perk != null)
                slotTexts[i].text = $"{label}\n{perk.description}";
            else
                slotTexts[i].text = $"{label}\nEmpty";

            slotButtons[i].interactable = true;
        }

        bool slot4Unlocked = gameManager.PerkSlotCount >= 4;

        slotButtons[3].interactable = slot4Unlocked;

        if (slot4LockedVisual != null)
            slot4LockedVisual.SetActive(!slot4Unlocked);
    }

    public void EquipBoughtPerkToSlot(int slotIndex)
    {
        if (currentPerk == null) return;

        GameManager.Instance.EquipPerk(currentPerk, slotIndex);

        currentPerk = null;

        ClosePopup();
    }
}