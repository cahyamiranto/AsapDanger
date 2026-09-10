using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Self-contained Unity implementation of a square-clicking occupancy game.
///
/// Responsibilities:
/// - Randomly spawn clickable squares.
/// - Keep one authoritative active-square counter.
/// - Clamp the counter to 0..15.
/// - Update a grey progress-bar track and dynamic fill.
/// - Change fill color from orange to red at 10 active squares.
/// - Remove squares when clicked.
/// - Prevent duplicate click handling.
/// - Keep spawned squares fully inside the configured playable area.
///
/// Minimal setup:
/// 1. Create a Canvas in the scene.
/// 2. Create an empty GameObject and attach this script.
/// 3. Assign the Canvas RectTransform to "uiCanvas".
/// 4. Assign the Canvas's GraphicRaycaster to "graphicRaycaster".
/// 5. If the scene does not already have an EventSystem, Unity should create one
///    when using UI; otherwise create one manually.
/// 6. Press Play. The script creates the progress bar and squares automatically.
///
/// No square prefab or separate script is required.
/// </summary>
public class SquareProgressGame : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // GAME SETTINGS
    // -------------------------------------------------------------------------

    [Header("Game Capacity")]
    [SerializeField] private int maxSquares = 15;

    [Header("Square Spawning")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 2.0f;
    [SerializeField] private float squareSize = 30f;

    [Header("Playable Area")]
    [Tooltip("Padding from the edges of the Canvas where squares cannot spawn.")]
    [SerializeField] private float playableAreaPadding = 20f;

    [Header("Square Appearance")]
    [SerializeField] private Color squareColor = new Color(0.15f, 0.85f, 1f, 1f);

    [Header("Progress Bar")]
    [SerializeField] private float progressBarWidth = 500f;
    [SerializeField] private float progressBarHeight = 30f;
    [SerializeField] private float progressBarTopMargin = 40f;

    [SerializeField] private Color trackColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color orangeFillColor = new Color(1f, 0.55f, 0.05f, 1f);
    [SerializeField] private Color redFillColor = new Color(0.95f, 0.08f, 0.08f, 1f);

    [Header("Optional Fill Animation")]
    [Tooltip("Enable a small visual smoothing effect. The logical counter still updates immediately.")]
    [SerializeField] private bool animateFill = true;

    [SerializeField] private float fillAnimationSpeed = 12f;

    // -------------------------------------------------------------------------
    // AUTHORITATIVE GAME STATE
    // -------------------------------------------------------------------------

    // This is the ONLY authoritative active-square counter.
    private int currentSquares = 0;

    // -------------------------------------------------------------------------
    // UNITY/UI REFERENCES
    // -------------------------------------------------------------------------

    [Header("Scene References")]
    [Tooltip("Canvas used for the generated UI and spawned squares.")]
    [SerializeField] private RectTransform uiCanvas;

    [Tooltip("GraphicRaycaster on the Canvas. If omitted, the script will try to find one.")]
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    private RectTransform progressBarRoot;
    private Image trackImage;
    private Image fillImage;

    private Coroutine spawnCoroutine;

    // -------------------------------------------------------------------------
    // CONSTANTS
    // -------------------------------------------------------------------------

    private const int DEFAULT_MAX_SQUARES = 15;

    // -------------------------------------------------------------------------
    // COLORS
    // -------------------------------------------------------------------------

    private Color CurrentFillColor
    {
        get
        {
            return currentSquares >= 10
                ? redFillColor
                : orangeFillColor;
        }
    }

    // -------------------------------------------------------------------------
    // INITIALIZATION
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Ensure the requested maximum capacity is sensible.
        maxSquares = Mathf.Max(1, maxSquares);

        // The requested game design uses 15 as the capacity.
        // If the Inspector value was left at zero, restore it to 15.
        if (maxSquares <= 0)
        {
            maxSquares = DEFAULT_MAX_SQUARES;
        }

        // Clamp all tunable values to safe ranges.
        minSpawnInterval = Mathf.Max(0.05f, minSpawnInterval);
        maxSpawnInterval = Mathf.Max(minSpawnInterval, maxSpawnInterval);
        squareSize = Mathf.Max(5f, squareSize);
        playableAreaPadding = Mathf.Max(0f, playableAreaPadding);

        progressBarWidth = Mathf.Max(50f, progressBarWidth);
        progressBarHeight = Mathf.Max(5f, progressBarHeight);
        progressBarTopMargin = Mathf.Max(0f, progressBarTopMargin);

        if (uiCanvas == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();

            if (canvas != null)
            {
                uiCanvas = canvas.GetComponent<RectTransform>();
            }
        }

        if (uiCanvas == null)
        {
            Debug.LogError(
                "SquareProgressGame: No Canvas was found. " +
                "Create a Canvas and assign its RectTransform to uiCanvas."
            );

            enabled = false;
            return;
        }

        if (graphicRaycaster == null)
        {
            graphicRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();

            if (graphicRaycaster == null)
            {
                graphicRaycaster = uiCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Create the complete progress bar automatically.
        CreateProgressBar();

        // Start the randomized spawning loop.
        spawnCoroutine = StartCoroutine(SpawnLoop());

        // Make absolutely certain the initial UI represents the initial state.
        currentSquares = Mathf.Clamp(currentSquares, 0, maxSquares);
        UpdateProgressBar();
    }

    private void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // -------------------------------------------------------------------------
    // PROGRESS BAR CREATION
    // -------------------------------------------------------------------------

    private void CreateProgressBar()
    {
        // Root object.
        GameObject rootObject = new GameObject(
            "ProgressBar",
            typeof(RectTransform)
        );

        rootObject.transform.SetParent(uiCanvas, false);

        progressBarRoot = rootObject.GetComponent<RectTransform>();

        progressBarRoot.anchorMin = new Vector2(0.5f, 1f);
        progressBarRoot.anchorMax = new Vector2(0.5f, 1f);
        progressBarRoot.pivot = new Vector2(0.5f, 1f);

        progressBarRoot.sizeDelta = new Vector2(
            progressBarWidth,
            progressBarHeight
        );

        progressBarRoot.anchoredPosition = new Vector2(
            0f,
            -progressBarTopMargin
        );

        // ---------------------------------------------------------------------
        // GREY TRACK
        // ---------------------------------------------------------------------

        GameObject trackObject = new GameObject(
            "Track",
            typeof(RectTransform),
            typeof(Image)
        );

        trackObject.transform.SetParent(progressBarRoot, false);

        RectTransform trackRect = trackObject.GetComponent<RectTransform>();
        trackImage = trackObject.GetComponent<Image>();

        trackRect.anchorMin = new Vector2(0f, 0f);
        trackRect.anchorMax = new Vector2(1f, 1f);
        trackRect.offsetMin = Vector2.zero;
        trackRect.offsetMax = Vector2.zero;

        trackImage.color = trackColor;
        trackImage.raycastTarget = false;

        // Add a rounded-looking sprite when Unity's built-in UI sprite exists.
        trackImage.sprite = CreateRoundedSprite();

        // ---------------------------------------------------------------------
        // DYNAMIC FILL
        // ---------------------------------------------------------------------

        GameObject fillObject = new GameObject(
            "Fill",
            typeof(RectTransform),
            typeof(Image)
        );

        fillObject.transform.SetParent(progressBarRoot, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillImage = fillObject.GetComponent<Image>();

        // Fill grows from left to right.
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);

        fillRect.anchoredPosition = Vector2.zero;

        fillRect.sizeDelta = new Vector2(
            0f,
            -6f
        );

        fillImage.color = orangeFillColor;
        fillImage.raycastTarget = false;

        fillImage.sprite = CreateRoundedSprite();

        // Make the fill a little smaller than the track so grey remains visible
        // as an outer border.
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(0f, -3f);
    }

    /// <summary>
    /// Uses Unity's built-in UI sprite if available.
    /// This keeps the implementation self-contained without requiring an
    /// external texture asset.
    /// </summary>
    private Sprite CreateRoundedSprite()
    {
        Sprite defaultSprite = Resources.GetBuiltinResource<Sprite>(
            "UI/Skin/UISprite.psd"
        );

        return defaultSprite;
    }

    // -------------------------------------------------------------------------
    // SPAWNING
    // -------------------------------------------------------------------------

    private IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            float delay = Random.Range(
                minSpawnInterval,
                maxSpawnInterval
            );

            yield return new WaitForSeconds(delay);

            // Never create a 16th square.
            if (currentSquares >= maxSquares)
            {
                continue;
            }

            SpawnSquare();
        }
    }

    private void SpawnSquare()
    {
        // Defensive check in case this method is called from elsewhere.
        ClampCounter();

        if (currentSquares >= maxSquares)
        {
            return;
        }

        GameObject squareObject = new GameObject(
            "ClickableSquare",
            typeof(RectTransform),
            typeof(Image)
        );

        squareObject.transform.SetParent(uiCanvas, false);

        RectTransform squareRect =
            squareObject.GetComponent<RectTransform>();

        Image squareImage =
            squareObject.GetComponent<Image>();

        squareRect.sizeDelta = new Vector2(
            squareSize,
            squareSize
        );

        squareImage.color = squareColor;
        squareImage.raycastTarget = true;

        squareImage.sprite = CreateRoundedSprite();

        // Give each square a slightly different visual tint while retaining
        // the configured base color.
        Color randomColor = Color.Lerp(
            squareColor,
            Color.white,
            Random.Range(0f, 0.25f)
        );

        squareImage.color = randomColor;

        // Find a random valid position that keeps the complete square visible.
        squareRect.anchoredPosition = GetRandomPlayablePosition();

        // Add click handling directly to this square.
        ClickableSquare clickableSquare =
            squareObject.AddComponent<ClickableSquare>();

        clickableSquare.Initialize(this);

        // IMPORTANT:
        // The counter is incremented only after the square has been
        // successfully created and initialized.
        currentSquares++;

        ClampCounter();
        UpdateProgressBar();
    }

    private Vector2 GetRandomPlayablePosition()
    {
        Rect canvasRect = uiCanvas.rect;

        float halfSquare = squareSize * 0.5f;

        // Leave enough room for the entire square to remain visible.
        float minX =
            canvasRect.xMin +
            playableAreaPadding +
            halfSquare;

        float maxX =
            canvasRect.xMax -
            playableAreaPadding -
            halfSquare;

        float minY =
            canvasRect.yMin +
            playableAreaPadding +
            halfSquare;

        float maxY =
            canvasRect.yMax -
            playableAreaPadding -
            halfSquare;

        // Prevent invalid Random.Range ranges on extremely small canvases.
        if (maxX < minX)
        {
            float centerX = (canvasRect.xMin + canvasRect.xMax) * 0.5f;
            minX = centerX;
            maxX = centerX;
        }

        if (maxY < minY)
        {
            float centerY = (canvasRect.yMin + canvasRect.yMax) * 0.5f;
            minY = centerY;
            maxY = centerY;
        }

        return new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );
    }

    // -------------------------------------------------------------------------
    // SQUARE CLICK RESPONSE
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by ClickableSquare when a square is clicked.
    /// </summary>
    public void HandleSquareClicked(ClickableSquare square)
    {
        if (square == null)
        {
            return;
        }

        // A square can only be processed once.
        if (square.HasBeenClicked)
        {
            return;
        }

        square.MarkAsClicked();

        // Destroy first so the object can no longer receive another normal
        // click. The counter is updated exactly once.
        Destroy(square.gameObject);

        currentSquares--;

        ClampCounter();
        UpdateProgressBar();
    }

    // -------------------------------------------------------------------------
    // COUNTER MANAGEMENT
    // -------------------------------------------------------------------------

    private void ClampCounter()
    {
        currentSquares = Mathf.Clamp(
            currentSquares,
            0,
            maxSquares
        );
    }

    /// <summary>
    /// Optional public read-only access for other game systems.
    /// </summary>
    public int GetCurrentSquareCount()
    {
        return currentSquares;
    }

    /// <summary>
    /// Returns the configured maximum capacity.
    /// </summary>
    public int GetMaxSquareCount()
    {
        return maxSquares;
    }

    // -------------------------------------------------------------------------
    // PROGRESS BAR UPDATE
    // -------------------------------------------------------------------------

    private void UpdateProgressBar()
    {
        if (fillImage == null || progressBarRoot == null)
        {
            return;
        }

        ClampCounter();

        // Exact logical progress:
        //
        // 0 / 15 = 0%
        // 1 / 15 = 6.67%
        // 5 / 15 = 33.33%
        // 10 / 15 = 66.67%
        // 15 / 15 = 100%
        //
        float progress = (float)currentSquares / maxSquares;

        progress = Mathf.Clamp01(progress);

        RectTransform fillRect = fillImage.rectTransform;

        float availableWidth =
            progressBarWidth - 6f;

        availableWidth = Mathf.Max(
            0f,
            availableWidth
        );

        float targetWidth =
            availableWidth * progress;

        if (!animateFill)
        {
            fillRect.sizeDelta = new Vector2(
                targetWidth,
                -6f
            );
        }
        else
        {
            // The fill animation is visual only. The logical counter has
            // already been changed immediately above.
            StartCoroutine(AnimateFillWidth(targetWidth));
        }

        // Color always follows the authoritative counter immediately.
        fillImage.color = CurrentFillColor;
    }

    private IEnumerator AnimateFillWidth(float targetWidth)
    {
        if (fillImage == null)
        {
            yield break;
        }

        RectTransform fillRect = fillImage.rectTransform;

        float startingWidth = fillRect.sizeDelta.x;

        // If the target is already reached, avoid unnecessary animation.
        if (Mathf.Approximately(startingWidth, targetWidth))
        {
            fillRect.sizeDelta = new Vector2(
                targetWidth,
                -6f
            );

            yield break;
        }

        while (fillImage != null &&
               fillRect != null &&
               !Mathf.Approximately(fillRect.sizeDelta.x, targetWidth))
        {
            float newWidth = Mathf.MoveTowards(
                fillRect.sizeDelta.x,
                targetWidth,
                fillAnimationSpeed *
                progressBarWidth *
                Time.unscaledDeltaTime
            );

            fillRect.sizeDelta = new Vector2(
                newWidth,
                -6f
            );

            yield return null;
        }

        if (fillRect != null)
        {
            fillRect.sizeDelta = new Vector2(
                targetWidth,
                -6f
            );
        }
    }

    // -------------------------------------------------------------------------
    // OPTIONAL RESET
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resets the game to zero active squares.
    /// Useful for a restart button or hackathon testing.
    /// </summary>
    public void ResetGame()
    {
        ClickableSquare[] squares =
            FindObjectsByType<ClickableSquare>(
                FindObjectsSortMode.None
            );

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

    // -------------------------------------------------------------------------
    // DEBUG / TEST HELPERS
    // -------------------------------------------------------------------------

    [ContextMenu("Spawn Square Now")]
    private void DebugSpawnSquare()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (currentSquares < maxSquares)
        {
            SpawnSquare();
        }
    }

    [ContextMenu("Reset Game")]
    private void DebugResetGame()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ResetGame();
    }

    // =========================================================================
    // NESTED CLICKABLE SQUARE COMPONENT
    // =========================================================================
    //
    // This is deliberately nested inside the same file so no second C# file is
    // required.
    //
    // Each square has exactly one ClickableSquare component. It owns a
    // "hasBeenClicked" flag, which prevents rapid/double click events from
    // decrementing the authoritative counter more than once.
    // =========================================================================

    private class ClickableSquare :
        MonoBehaviour,
        IPointerClickHandler
    {
        private SquareProgressGame owner;
        private bool hasBeenClicked;

        public bool HasBeenClicked
        {
            get { return hasBeenClicked; }
        }

        public SquareProgressGame Owner
        {
            get { return owner; }
        }

        public void Initialize(SquareProgressGame gameOwner)
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
