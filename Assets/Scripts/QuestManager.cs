using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [System.Serializable]
    public class LevelQuestData
    {
        public int targetLapor = 3;
        public int targetPadamkan = 3;
    }

    [Header("Pengaturan Quest")]
    [Range(1, 3)] public int currentLevel = 1;
    public List<LevelQuestData> levelConfigs = new List<LevelQuestData>
    {
        new LevelQuestData { targetLapor = 3, targetPadamkan = 3 }, // Level 1
        new LevelQuestData { targetLapor = 4, targetPadamkan = 4 }, // Level 2
        new LevelQuestData { targetLapor = 5, targetPadamkan = 5 }  // Level 3
    };

    [Header("Referensi UI Quest")]
    [SerializeField] private TextMeshProUGUI textQuestLapor;
    [SerializeField] private TextMeshProUGUI textQuestPadamkan;
    [SerializeField] private TextMeshProUGUI textLevel;

    [Header("Status Saat Ini")]
    public int sisaLapor;
    public int sisaPadamkan;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 3);
        int index = currentLevel - 1;

        sisaLapor = levelConfigs[index].targetLapor;
        sisaPadamkan = levelConfigs[index].targetPadamkan;

        UpdateUI();
    }

    public void KurangiLapor()
    {
        if (sisaLapor > 0)
        {
            sisaLapor--;
            UpdateUI();
            CheckLevelCompletion();
        }
    }

    public void KurangiPadamkan()
    {
        if (sisaPadamkan > 0)
        {
            sisaPadamkan--;
            UpdateUI();
            CheckLevelCompletion();
        }
    }

    private void UpdateUI()
    {
        if (textQuestLapor != null)
            textQuestLapor.text = $"Lapor: {sisaLapor}";

        if (textQuestPadamkan != null)
            textQuestPadamkan.text = $"Padamin Sendiri: {sisaPadamkan}";

        if (textLevel != null)
            textLevel.text = $"Level: {currentLevel}";
    }

    private void CheckLevelCompletion()
    {
        if (sisaLapor <= 0 && sisaPadamkan <= 0)
        {
            Debug.Log($"Level {currentLevel} Selesai!");
            if (currentLevel < 3)
            {
                LoadLevel(currentLevel + 1);
            }
            else
            {
                if (textQuestLapor != null) textQuestLapor.text = "Semua Misi Selesai!";
                if (textQuestPadamkan != null) textQuestPadamkan.text = string.Empty;
            }
        }
    }
}