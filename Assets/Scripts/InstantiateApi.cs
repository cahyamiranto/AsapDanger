using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstantiateApi : MonoBehaviour
{
    // ============================================================
    // SPAWN SETTINGS
    // ============================================================

    [Header("List Prefab")]
    public List<GameObject> listApi = new List<GameObject>();

    [Header("Interval Waktu Spawn (Detik)")]
    public float min = 1f;
    public float max = 3f;

    // ============================================================
    // PROGRESS BAR (Assign from Inspector)
    // ============================================================

    [Header("Progress Bar (Assign from Inspector)")]

    [Tooltip("Komponen Slider Unity untuk progress bar. " +
             "Buat UI > Slider di Canvas, lalu drag ke sini.")]
    public Slider ispaSlider;

    [Tooltip("Image fill milik Slider untuk ubah warna gradien. " +
             "Drag child 'Fill Area > Fill' dari dalam Slider ke sini.")]
    public Image sliderFillImage;

    [Tooltip("Jumlah maksimum api yang bisa aktif sekaligus.")]
    public int maxSquares = 15;

    [Tooltip("Warna fill dan gambar saat nilai = 0 (tidak ada bahaya).")]
    public Color colorAtZero = new Color(1f, 0.20f, 0.55f, 1f);

    [Tooltip("Warna fill dan gambar saat nilai = maksimum (bahaya penuh).")]
    public Color colorAtMax = new Color(0.22f, 0.02f, 0.30f, 1f);

    // ============================================================
    // LUNG IMAGE (Assign from Inspector)
    // ============================================================

    [Header("Lung Image (Assign from Inspector)")]

    [Tooltip("Komponen Image paru-paru di Canvas.")]
    public Image lungImage;

    [Tooltip("RectTransform dari lungImage (untuk animasi pulse).")]
    public RectTransform lungRect;

    [Tooltip("Brightness gambar paru saat nilai = 0.")]
    public float lungBrightnessAtZero = 1f;

    [Tooltip("Brightness gambar paru saat nilai = maksimum.")]
    public float lungBrightnessAtMaximum = 0.35f;

    [Tooltip("Scale paru saat keadaan besar (pulse).")]
    public float lungLargeScale = 1f;

    [Tooltip("Scale paru saat keadaan kecil (pulse).")]
    public float lungSmallScale = 0.8f;

    [Tooltip("Detik antara setiap perubahan skala paru-paru.")]
    public float lungPulseInterval = 1f;

    // ============================================================
    // RUNTIME STATE
    // ============================================================

    [HideInInspector] public int currentSquares = 0;

    private float waitTime;

    public AudioSource apiSoundEffect;

    // ============================================================
    // UNITY
    // ============================================================

    void Start()
    {
        // Inisialisasi slider
        UpdateProgressBar();

        lungPulseInterval = Mathf.Max(0.05f, lungPulseInterval);
        StartCoroutine(LungPulseLoop());
        StartCoroutine(SpawnApiRoutine());
    }

    // ============================================================
    // SPAWN LOOP
    // ============================================================

    public IEnumerator SpawnApiRoutine()
    {
        if (listApi != null && listApi.Count > 0)
        {
            GameObject selectedApi =
                listApi[Random.Range(0, listApi.Count)];

            if (selectedApi != null && selectedApi.activeSelf == false)
            {
                ScriptApi api = selectedApi.GetComponent<ScriptApi>();
                selectedApi.SetActive(true);

                if (api != null && api.orang != null)
                {
                    api.orang.SetActive(true);
                }

                // Tambah counter dan perbarui UI
                ispaSlider.value++;
                UpdateProgressBar();
                apiSoundEffect.Play();

                DelayScript delayScript =
                    selectedApi.GetComponentInChildren<DelayScript>();

                if (delayScript != null)
                {
                    delayScript.MulaiTimer(delayScript.delayTime);
                    waitTime = Random.Range(
                        Mathf.Min(min, max),
                        Mathf.Max(min, max)
                    );
                }
            }
            else
            {
                waitTime = 0;
            }
        }

        // Tunggu beberapa detik secara acak antara min dan max
        yield return new WaitForSeconds(waitTime);

        // Di akhir coroutine panggil lagi coroutine tersebut
        StartCoroutine(SpawnApiRoutine());
    }

    // ============================================================
    // COUNTER
    // ============================================================

    

    // Dipanggil dari luar (misal: ketika api dipadamkan)

    // ============================================================
    // UPDATE PROGRESS BAR
    // ============================================================

    public void UpdateProgressBar()
    {
        //ClampCounter();
        // Warna interpolasi dari colorAtZero ke colorAtMax
        Color currentColor = Color.Lerp(colorAtZero, colorAtMax, ispaSlider.value / maxSquares);

        // Warna fill slider
        if (sliderFillImage != null)
        {
            sliderFillImage.color = currentColor;
        }

        // Warna + kecerahan gambar paru
        UpdateLungAppearance(currentColor, ispaSlider.value / maxSquares);
    }

    // ============================================================
    // LUNG APPEARANCE
    // ============================================================

    private void UpdateLungAppearance(Color gradientColor, float progress)
    {
        if (lungImage == null) return;

        float brightness = Mathf.Lerp(
            lungBrightnessAtZero,
            lungBrightnessAtMaximum,
            progress
        );

        lungImage.color = new Color(
            gradientColor.r * brightness,
            gradientColor.g * brightness,
            gradientColor.b * brightness,
            1f
        );
    }

    // ============================================================
    // LUNG PULSE ANIMATION
    // ============================================================

    private IEnumerator LungPulseLoop()
    {
        bool large = true;

        while (enabled)
        {
            if (lungRect != null)
            {
                lungRect.localScale =
                    Vector3.one * (large ? lungLargeScale : lungSmallScale);
            }

            large = !large;

            yield return new WaitForSeconds(lungPulseInterval);
        }
    }
}
