using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ISPAlevel : MonoBehaviour
{
    // Singleton agar mudah diakses
    public static ISPAlevel Instance { get; private set; }

    // ============================================================
    // GAME SETTINGS
    // ============================================================

    [Header("Game Settings")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 2.0f;
    [SerializeField] private float squareSize = 30f;

    // Maximum number of active squares.
    private const int MAX_SQUARES = 15;

    public int currentSquares = 0;

    // ============================================================
    // PLAYABLE AREA
    // ============================================================

    [Header("Playable Area")]
    [Tooltip("Canvas used as the game area.")]
    [SerializeField] private RectTransform uiCanvas;

    [Tooltip("Distance from the edge where squares cannot spawn.")]
    [SerializeField] private float playableAreaPadding = 20f;

    // ============================================================
    // SQUARE APPEARANCE
    // ============================================================

    [Header("Square Appearance")]
    [SerializeField] private Color squareColor =
        new Color(0.1f, 0.8f, 1f, 1f);

    // ============================================================
    // PROGRESS BAR (Assign from Inspector)
    // ============================================================

    [Header("Progress Bar (Assign from Inspector)")]

    [Tooltip("Komponen Slider Unity untuk progress bar ISPA. " +
             "Buat UI > Slider di Canvas, lalu drag ke sini.")]
    [SerializeField] private Slider ispaSlider;

    [Tooltip("Image fill milik Slider untuk ubah warna gradien. " +
             "Drag child 'Fill Area > Fill' dari dalam Slider ke sini.")]
    [SerializeField] private Image sliderFillImage;

    [Tooltip("Warna fill dan gambar saat nilai = 0 (tidak ada bahaya).")]
    [SerializeField] private Color colorAtZero =
        new Color(1f, 0.20f, 0.55f, 1f);

    [Tooltip("Warna fill dan gambar saat nilai = maksimum (bahaya penuh).")]
    [SerializeField] private Color colorAtMax =
        new Color(0.22f, 0.02f, 0.30f, 1f);

    // ============================================================
    // LUNG IMAGE (Assign from Inspector)
    // ============================================================

    [Header("Lung Image (Assign from Inspector)")]

    [Tooltip("Komponen Image paru-paru di Canvas.")]
    [SerializeField] private Image lungImage;

    [Tooltip("RectTransform dari lungImage (untuk animasi pulse).")]
    [SerializeField] private RectTransform lungRect;

    [Tooltip("Brightness ketika currentSquares = 0.")]
    [SerializeField] private float lungBrightnessAtZero = 1f;

    [Tooltip("Brightness ketika currentSquares = MAX (15).")]
    [SerializeField] private float lungBrightnessAtMaximum = 0.35f;

    [Tooltip("Scale ketika paru-paru dalam keadaan besar (pulse besar).")]
    [SerializeField] private float lungLargeScale = 1f;

    [Tooltip("Scale ketika paru-paru dalam keadaan kecil (pulse kecil).")]
    [SerializeField] private float lungSmallScale = 0.8f;

    [Tooltip("Detik antara setiap perubahan skala paru-paru.")]
    [SerializeField] private float lungPulseInterval = 1f;

    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        minSpawnInterval = Mathf.Max(0.05f, minSpawnInterval);

        maxSpawnInterval = Mathf.Max(
            minSpawnInterval,
            maxSpawnInterval
        );

        squareSize = Mathf.Max(5f, squareSize);

        playableAreaPadding = Mathf.Max(0f, playableAreaPadding);

        lungPulseInterval = Mathf.Max(0.05f, lungPulseInterval);

        // --------------------------------------------------------
        // FIND CANVAS (fallback jika tidak di-assign)
        // --------------------------------------------------------

        if (uiCanvas == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();

            if (canvas != null)
            {
                uiCanvas = canvas.GetComponent<RectTransform>();
            }
        }

        if (uiCanvas == null)
        {
            Debug.LogError(
                "ISPAlevel: No Canvas found. " +
                "Please create a Canvas and assign its RectTransform " +
                "to the Ui Canvas field."
            );

            enabled = false;
            return;
        }

        // --------------------------------------------------------
        // INISIALISASI PROGRESS BAR
        // --------------------------------------------------------

        currentSquares = Mathf.Clamp(currentSquares, 0, MAX_SQUARES);

        UpdateProgressBar();

        // --------------------------------------------------------
        // START LUNG ANIMATION
        // --------------------------------------------------------

        StartCoroutine(LungPulseLoop());
    }

    // ============================================================
    // LUNG BRIGHTNESS
    // ============================================================

    private void UpdateLungBrightness(Color gradientColor)
    {
        if (lungImage == null)
        {
            return;
        }

        float progress =
            Mathf.Clamp01((float)currentSquares / MAX_SQUARES);

        // Kecerahan dari Inspector (0 = terang, MAX = gelap)
        float brightness =
            Mathf.Lerp(
                lungBrightnessAtZero,
                lungBrightnessAtMaximum,
                progress
            );

        // Kalikan warna gradien dengan kecerahan
        lungImage.color =
            new Color(
                gradientColor.r * brightness,
                gradientColor.g * brightness,
                gradientColor.b * brightness,
                1f
            );
    }

    // ============================================================
    // LUNG PULSE
    // ============================================================

    private IEnumerator LungPulseLoop()
    {
        bool large = true;

        while (enabled)
        {
            if (lungRect != null)
            {
                lungRect.localScale =
                    Vector3.one *
                    (large ? lungLargeScale : lungSmallScale);
            }

            large = !large;

            yield return new WaitForSeconds(lungPulseInterval);
        }
    }

    // ============================================================
    // METHOD PENGURANG NILAI ISPA
    // ============================================================

    public void KurangiISPA(int amount = 1)
    {
        currentSquares -= amount;
        ClampCounter();
        UpdateProgressBar();
    }

    // ============================================================
    // SQUARE CLICKED
    // ============================================================

    private void HandleSquareClicked(ClickableSquare square)
    {
        if (square == null)
        {
            return;
        }

        if (square.HasBeenClicked)
        {
            return;
        }

        square.MarkAsClicked();

        Destroy(square.gameObject);

        currentSquares--;

        ClampCounter();

        UpdateProgressBar();
    }

    // ============================================================
    // COUNTER CLAMP
    // ============================================================

    public void ClampCounter()
    {
        currentSquares =
            Mathf.Clamp(currentSquares, 0, MAX_SQUARES);
    }

    // ============================================================
    // UPDATE PROGRESS BAR
    // ============================================================

    public void UpdateProgressBar()
    {
        ClampCounter();

        float progress =
            Mathf.Clamp01((float)currentSquares / MAX_SQUARES);

        // Warna yang diinterpolasi dari colorAtZero ke colorAtMax
        Color currentColor = Color.Lerp(colorAtZero, colorAtMax, progress);

        // --------------------------------------------------------
        // SLIDER — gerakkan nilai bar
        // --------------------------------------------------------

        if (ispaSlider != null)
        {
            ispaSlider.value = progress;
        }

        // --------------------------------------------------------
        // WARNA FILL SLIDER
        // --------------------------------------------------------

        if (sliderFillImage != null)
        {
            sliderFillImage.color = currentColor;
        }

        // --------------------------------------------------------
        // LUNG — kecerahan + warna gambar mengikuti progress
        // --------------------------------------------------------

        UpdateLungBrightness(currentColor);
    }

    // ============================================================
    // PUBLIC INFORMATION
    // ============================================================

    public int GetCurrentSquareCount()
    {
        return currentSquares;
    }

    public int GetMaximumSquareCount()
    {
        return MAX_SQUARES;
    }

    // ============================================================
    // RESET GAME
    // ============================================================

    public void ResetGame()
    {
        ClickableSquare[] squares =
            FindObjectsByType<ClickableSquare>(FindObjectsSortMode.None);

        foreach (ClickableSquare square in squares)
        {
            if (square != null && square.Owner == this)
            {
                Destroy(square.gameObject);
            }
        }

        currentSquares = 0;

        ClampCounter();

        UpdateProgressBar();
    }

    // ============================================================
    // DEBUG: SPAWN ONE SQUARE
    // ============================================================

    [ContextMenu("Spawn Square Now")]
    private void SpawnSquareFromEditor()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (currentSquares < MAX_SQUARES)
        {
            //SpawnSquare();
        }
    }

    // ============================================================
    // DEBUG: RESET
    // ============================================================

    [ContextMenu("Reset Game")]
    private void ResetFromEditor()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResetGame();
    }

    // ============================================================
    // CLICKABLE SQUARE
    // ============================================================

    private class ClickableSquare :
        MonoBehaviour,
        IPointerClickHandler
    {
        private ISPAlevel owner;

        private bool hasBeenClicked = false;

        public bool HasBeenClicked
        {
            get
            {
                return hasBeenClicked;
            }
        }

        public ISPAlevel Owner
        {
            get
            {
                return owner;
            }
        }

        public void Initialize(ISPAlevel gameOwner)
        {
            owner = gameOwner;
            hasBeenClicked = false;
        }

        public void MarkAsClicked()
        {
            hasBeenClicked = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (hasBeenClicked)
            {
                return;
            }

            if (owner == null)
            {
                return;
            }

            owner.HandleSquareClicked(this);
        }
    }
}
