using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Pengaturan Nilai Score (Editable)")]
    [Tooltip("Biaya/poin saat Bush diklik (isi angka positif atau negatif)")]
    [SerializeField] private int biayaPadamkanBush = 5; // Nilai biaya pemadaman

    [Tooltip("Poin yang didapat saat NPC diklik")]
    [SerializeField] private int poinNpc = 5;

    [Header("Pengaturan Blink Merah")]
    [SerializeField] private Color warnaPeringatan = Color.red;
    [SerializeField] private int jumlahKedipan = 3;
    [SerializeField] private float durasiKedip = 0.12f;

    [Header("Referensi UI")]
    [SerializeField] private TextMeshProUGUI textScore;

    [Header("Data Score Saat Ini")]
    [SerializeField] private int currentScore = 0;

    private Color warnaAsliText;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (textScore != null)
        {
            warnaAsliText = textScore.color;
        }
        UpdateScoreUI();
    }

    /// <summary>
    /// Mengecek apakah skor cukup untuk memadamkan bush.
    /// Jika tidak cukup, otomatis memicu blink merah.
    /// </summary>
    public bool BisaPadamkanBush()
    {
        int biaya = Mathf.Abs(biayaPadamkanBush);

        // Jika score 0 atau kurang dari biaya yang dibutuhkan
        if (currentScore <= 0 || currentScore < biaya)
        {
            TriggerBlinkMerah();
            return false;
        }

        return true;
    }

    public void OnBushBerhasilDipadamkan()
    {
        // Kurangi score sebesar biaya
        TambahScore(-Mathf.Abs(biayaPadamkanBush));
    }

    public void OnNpcClicked()
    {
        TambahScore(poinNpc);
    }

    public void TambahScore(int amount)
    {
        currentScore += amount;
        if (currentScore < 0) currentScore = 0;

        UpdateScoreUI();
    }

    public void TriggerBlinkMerah()
    {
        if (textScore == null) return;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        for (int i = 0; i < jumlahKedipan; i++)
        {
            textScore.color = warnaPeringatan;
            yield return new WaitForSeconds(durasiKedip);
            textScore.color = warnaAsliText;
            yield return new WaitForSeconds(durasiKedip);
        }

        textScore.color = warnaAsliText;
        blinkCoroutine = null;
    }

    private void UpdateScoreUI()
    {
        if (textScore != null)
        {
            textScore.text = $"Score: {currentScore}";
        }
    }

    public int GetCurrentScore() => currentScore;
}