using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class QuitLevelInfo : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Panel atau Canvas popup Quit yang akan dimunculkan")]
    [SerializeField] private GameObject quitPanel;

    private bool isPaused = false;

    private void Start()
    {
        // Pastikan panel tersembunyi dan game berjalan normal di awal scene
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Tekan tombol Q pada keyboard
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Membuka atau menutup panel Quit serta mengatur pause/unpause
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (quitPanel != null)
        {
            quitPanel.SetActive(isPaused);
        }

        // Hentikan waktu jika pause, jalankan kembali jika tidak
        Time.timeScale = isPaused ? 0f : 1f;
    }

    /// <summary>
    /// Dipanggil melalui OnClick event Button "Try Again"
    /// </summary>
    public void TryAgain()
    {
        // Kembalikan timeScale ke normal sebelum reload scene
        Time.timeScale = 1f;

        // Muat ulang scene yang sedang aktif saat ini
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    /// <summary>
    /// (Opsional) Fungsi jika kamu ingin menambahkan button Resume/Batal
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }
}