using UnityEngine;
using System.Collections;
using TMPro;

public class DelayScript : MonoBehaviour
{
    [Header("Pengaturan Timer")]
    public float delayTime = 2f; // Waktu delay dalam detik sesuai input user
    public TMP_Text timerText;   // Referensi Text (TMP) untuk menampilkan angka hitung mundur

    [Header("Status Timer")]
    public float sisaWaktu;      // Sisa waktu hitung mundur (dapat dilihat di Inspector)

    private Coroutine timerCoroutine;

    void Awake()
    {
        // Otomatis mencari Text (TMP) di objek ini atau anaknya jika belum diisi di Inspector
        if (timerText == null)
        {
            timerText = GetComponentInChildren<TMP_Text>();
        }
    }

    /// <summary>
    /// Memulai fungsi timer dengan durasi tertentu
    /// </summary>
    public void MulaiTimer(float waktu)
    {
        HentikanTimer();
        timerCoroutine = StartCoroutine(TimerRoutine(waktu));
    }

    /// <summary>
    /// Menghentikan timer yang sedang berjalan
    /// </summary>
    public void HentikanTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    // Fungsi Coroutine timer untuk menghitung mundur sisa waktu
    private IEnumerator TimerRoutine(float waktu)
    {
        sisaWaktu = waktu;

        while (sisaWaktu > 0)
        {
            // Tampilkan sisa waktu ke Text (TMP) jika tersedia
            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(sisaWaktu).ToString();
            }

            yield return null; // Update setiap frame
            sisaWaktu -= Time.deltaTime;
        }

        sisaWaktu = 0;
        if (timerText != null)
        {
            timerText.text = "0";
        }

        // Lakukan sesuatu setelah delay / timer habis
        gameObject.SetActive(false); // Mengnonaktifkan objek setelah timer selesai
    }
}

