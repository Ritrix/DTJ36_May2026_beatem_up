using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    private VisualElement m_HealthBar;
    private VisualElement m_EnemyHealthBar;
    private Label m_waveLabel;
    public static UIHandler instance { get; private set; }

    private void Awake()
    {
        instance = this;
        CacheUIReferences();

        Debug.Log($"HealthBar: {m_HealthBar}, EnemyHealthBar: {m_EnemyHealthBar}");


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        SetHealthValue(1.0f);
        SetEnemyHealthValue(1.0f);
        CacheUIReferences();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHealthValue(float percentage)
    {
        if (m_HealthBar == null)
            CacheUIReferences();

        if (m_HealthBar == null)
            return;

        m_HealthBar.style.width = Length.Percent(100 * percentage);
    }

    public void SetEnemyHealthValue(float percentage)
    {
        if (m_EnemyHealthBar == null)
            CacheUIReferences();

        if (m_EnemyHealthBar == null)
            return;

        m_EnemyHealthBar.style.width = Length.Percent(100 * percentage);
    }

    private void CacheUIReferences()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();

        m_HealthBar = uiDocument.rootVisualElement.Q<VisualElement>("HealthBar");
        m_EnemyHealthBar = uiDocument.rootVisualElement.Q<VisualElement>("EnemyHealthBar");
    }

    public void SetWaveLabelText(string text)
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        m_waveLabel = uiDocument.rootVisualElement.Q<Label>("WaveLabel");
        m_waveLabel.text = text;
    }

    public void SetEnemiesRemainingLabelText(string text)
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        m_waveLabel = uiDocument.rootVisualElement.Q<Label>("enemiesRemainingLabel");
        m_waveLabel.text = text;
    }

    public void SetEnemiesNameLabelText(string text)
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        m_waveLabel = uiDocument.rootVisualElement.Q<Label>("EnemyNameLabel");
        m_waveLabel.text = text;
    }

    public void SetGoldLabelText(string text)
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        m_waveLabel = uiDocument.rootVisualElement.Q<Label>("GoldLabel");
        m_waveLabel.text = text;
    }
}
