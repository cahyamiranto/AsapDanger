using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Pengaturan Nilai Score (Editable)")]
    [Tooltip("Biaya saat padam sendiri (berkurang 100.000)")]
    [SerializeField] private int biayaPadamkanBush = 100000;

    [Tooltip("Poin yang didapat saat lapor NPC (+500.000)")]
    [SerializeField] private int poinNpc = 500000;

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

    public bool BisaPadamkanBush()
    {
        int biaya = Mathf.Abs(biayaPadamkanBush);

        if (currentScore <= 0 || currentScore < biaya)
        {
            TriggerBlinkMerah();
            return false;
        }

        return true;
    }

    public void OnBushBerhasilDipadamkan()
    {
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
            textScore.text = $"{FormatScore(currentScore)}";
        }
    }

    /// <summary>
    /// Mengubah format angka ke satuan 'K' jika >= 1000.
    /// Contoh: 950 -> "950", 1000 -> "1K", 100000 -> "100K", 1500 -> "1.5K"
    /// </summary>
    private string FormatScore(int score)
    {
        if (score >= 1000)
        {
            float nilaiK = score / 1000f;
            return $"{nilaiK:0.##}K";
        }

        return score.ToString();
    }

    public int GetCurrentScore() => currentScore;
}