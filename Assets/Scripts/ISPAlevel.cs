using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ISPAlevel : MonoBehaviour
{
    // ============================================================
    // GAME SETTINGS
    // ============================================================

    [Header("Game Settings")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 2.0f;
    [SerializeField] private float squareSize = 30f;

    // Maximum number of active squares.
    private const int MAX_SQUARES = 15;

    // The single authoritative counter.
    private int currentSquares = 0;

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
    // PROGRESS BAR
    // ============================================================

    [Header("Progress Bar")]
    [SerializeField] private float progressBarWidth = 500f;
    [SerializeField] private float progressBarHeight = 30f;
    [SerializeField] private float progressBarTopMargin = 40f;

    [SerializeField] private Color trackColor =
        new Color(0.25f, 0.25f, 0.25f, 1f);

    [SerializeField] private Color orangeFillColor =
        new Color(1f, 0.55f, 0.05f, 1f);

    [SerializeField] private Color redFillColor =
        new Color(0.95f, 0.05f, 0.05f, 1f);

    // ============================================================
    // UI REFERENCES CREATED AT RUNTIME
    // ============================================================

    private RectTransform progressBarRoot;
    private Image trackImage;
    private Image fillImage;

    // ============================================================
    // UNITY
    // ============================================================

    private void Awake()
    {
        minSpawnInterval = Mathf.Max(0.05f, minSpawnInterval);
        maxSpawnInterval = Mathf.Max(
            minSpawnInterval,
            maxSpawnInterval
        );

        squareSize = Mathf.Max(5f, squareSize);
        playableAreaPadding = Mathf.Max(
            0f,
            playableAreaPadding
        );

        progressBarWidth = Mathf.Max(
            100f,
            progressBarWidth
        );

        progressBarHeight = Mathf.Max(
            10f,
            progressBarHeight
        );

        progressBarTopMargin = Mathf.Max(
            0f,
            progressBarTopMargin
        );

        // Try to automatically find a Canvas.
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

        CreateProgressBar();

        // Make sure the initial state is correct.
        currentSquares = Mathf.Clamp(
            currentSquares,
            0,
            MAX_SQUARES
        );

        UpdateProgressBar();

        StartCoroutine(SpawnLoop());
    }

    // ============================================================
    // PROGRESS BAR CREATION
    // ============================================================

    private void CreateProgressBar()
    {
        // --------------------------------------------------------
        // PROGRESS BAR ROOT
        // --------------------------------------------------------

        GameObject rootObject = new GameObject(
            "ProgressBar",
            typeof(RectTransform)
        );

        rootObject.transform.SetParent(
            uiCanvas,
            false
        );

        progressBarRoot =
            rootObject.GetComponent<RectTransform>();

        progressBarRoot.anchorMin =
            new Vector2(0.5f, 1f);

        progressBarRoot.anchorMax =
            new Vector2(0.5f, 1f);

        progressBarRoot.pivot =
            new Vector2(0.5f, 1f);

        progressBarRoot.sizeDelta =
            new Vector2(
                progressBarWidth,
                progressBarHeight
            );

        progressBarRoot.anchoredPosition =
            new Vector2(
                0f,
                -progressBarTopMargin
            );

        // --------------------------------------------------------
        // GREY TRACK
        // --------------------------------------------------------

        GameObject trackObject = new GameObject(
            "Track",
            typeof(RectTransform),
            typeof(Image)
        );

        trackObject.transform.SetParent(
            progressBarRoot,
            false
        );

        RectTransform trackRect =
            trackObject.GetComponent<RectTransform>();

        trackImage =
            trackObject.GetComponent<Image>();

        trackRect.anchorMin =
            Vector2.zero;

        trackRect.anchorMax =
            Vector2.one;

        trackRect.offsetMin =
            Vector2.zero;

        trackRect.offsetMax =
            Vector2.zero;

        trackImage.color =
            trackColor;

        trackImage.raycastTarget =
            false;

        // --------------------------------------------------------
        // FILL
        // --------------------------------------------------------

        GameObject fillObject = new GameObject(
            "Fill",
            typeof(RectTransform),
            typeof(Image)
        );

        fillObject.transform.SetParent(
            progressBarRoot,
            false
        );

        RectTransform fillRect =
            fillObject.GetComponent<RectTransform>();

        fillImage =
            fillObject.GetComponent<Image>();

        // Anchor to the left so the width grows horizontally.
        fillRect.anchorMin =
            new Vector2(0f, 0f);

        fillRect.anchorMax =
            new Vector2(0f, 1f);

        fillRect.pivot =
            new Vector2(0f, 0.5f);

        fillRect.anchoredPosition =
            new Vector2(3f, 0f);

        // Grey track remains visible around the fill.
        fillRect.sizeDelta =
            new Vector2(
                0f,
                -6f
            );

        fillImage.color =
            orangeFillColor;

        fillImage.raycastTarget =
            false;
    }

    // ============================================================
    // SPAWN LOOP
    // ============================================================

    private IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            float waitTime = Random.Range(
                minSpawnInterval,
                maxSpawnInterval
            );

            yield return new WaitForSeconds(
                waitTime
            );

            // Never exceed 15 active squares.
            if (currentSquares >= MAX_SQUARES)
            {
                continue;
            }

            SpawnSquare();
        }
    }

    // ============================================================
    // SPAWN SQUARE
    // ============================================================

    private void SpawnSquare()
    {
        // Safety check.
        ClampCounter();

        if (currentSquares >= MAX_SQUARES)
        {
            return;
        }

        GameObject squareObject = new GameObject(
            "ClickableSquare",
            typeof(RectTransform),
            typeof(Image)
        );

        squareObject.transform.SetParent(
            uiCanvas,
            false
        );

        RectTransform squareRect =
            squareObject.GetComponent<RectTransform>();

        Image squareImage =
            squareObject.GetComponent<Image>();

        // Set square size.
        squareRect.sizeDelta =
            new Vector2(
                squareSize,
                squareSize
            );

        // Set appearance.
        squareImage.color =
            squareColor;

        squareImage.raycastTarget =
            true;

        // Put it at a random valid position.
        squareRect.anchoredPosition =
            GetRandomPlayablePosition();

        // Add the click handler.
        ClickableSquare clickableSquare =
            squareObject.AddComponent<ClickableSquare>();

        clickableSquare.Initialize(
            this
        );

        // --------------------------------------------------------
        // IMPORTANT:
        // Only now do we increment the authoritative counter.
        // --------------------------------------------------------

        currentSquares++;

        ClampCounter();

        UpdateProgressBar();
    }

    // ============================================================
    // RANDOM POSITION
    // ============================================================

    private Vector2 GetRandomPlayablePosition()
    {
        Rect canvasRect =
            uiCanvas.rect;

        float halfSquare =
            squareSize * 0.5f;

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

        // Prevent invalid ranges on very small canvases.
        if (maxX < minX)
        {
            float centerX =
                (canvasRect.xMin +
                 canvasRect.xMax) *
                0.5f;

            minX = centerX;
            maxX = centerX;
        }

        if (maxY < minY)
        {
            float centerY =
                (canvasRect.yMin +
                 canvasRect.yMax) *
                0.5f;

            minY = centerY;
            maxY = centerY;
        }

        return new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );
    }

    // ============================================================
    // SQUARE CLICKED
    // ============================================================

    private void HandleSquareClicked(
        ClickableSquare square
    )
    {
        if (square == null)
        {
            return;
        }

        // The same square must never be counted twice.
        if (square.HasBeenClicked)
        {
            return;
        }

        square.MarkAsClicked();

        // Destroy the square.
        Destroy(
            square.gameObject
        );

        // Decrease the authoritative counter exactly once.
        currentSquares--;

        // Never go below zero.
        ClampCounter();

        // Immediately update the UI.
        UpdateProgressBar();
    }

    // ============================================================
    // COUNTER CLAMP
    // ============================================================

    private void ClampCounter()
    {
        currentSquares =
            Mathf.Clamp(
                currentSquares,
                0,
                MAX_SQUARES
            );
    }

    // ============================================================
    // PROGRESS BAR UPDATE
    // ============================================================

    private void UpdateProgressBar()
    {
        if (fillImage == null)
        {
            return;
        }

        ClampCounter();

        // --------------------------------------------------------
        // PROGRESS CALCULATION
        //
        // 0 / 15  = 0%
        // 1 / 15  = 6.67%
        // 5 / 15  = 33.33%
        // 10 / 15 = 66.67%
        // 15 / 15 = 100%
        // --------------------------------------------------------

        float progress =
            (float)currentSquares /
            MAX_SQUARES;

        progress =
            Mathf.Clamp01(progress);

        // Leave a small grey border around the fill.
        float usableWidth =
            progressBarWidth - 6f;

        usableWidth =
            Mathf.Max(
                0f,
                usableWidth
            );

        float fillWidth =
            usableWidth * progress;

        RectTransform fillRect =
            fillImage.rectTransform;

        fillRect.sizeDelta =
            new Vector2(
                fillWidth,
                -6f
            );

        // --------------------------------------------------------
        // COLOR
        //
        // 0-9  = ORANGE
        // 10-15 = RED
        // --------------------------------------------------------

        if (currentSquares >= 10)
        {
            fillImage.color =
                redFillColor;
        }
        else
        {
            fillImage.color =
                orangeFillColor;
        }
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
    FindObjectsByType<ClickableSquare>();


        foreach (ClickableSquare square in squares)
        {
            if (square != null &&
                square.Owner == this)
            {
                Destroy(
                    square.gameObject
                );
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
            SpawnSquare();
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
    //
    // This is intentionally inside the same C# file.
    // No second script is required.
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

        public void Initialize(
            ISPAlevel gameOwner
        )
        {
            owner =
                gameOwner;

            hasBeenClicked =
                false;
        }

        public void MarkAsClicked()
        {
            hasBeenClicked =
                true;
        }

        public void OnPointerClick(
            PointerEventData eventData
        )
        {
            // Prevent duplicate click processing.
            if (hasBeenClicked)
            {
                return;
            }

            // Make sure the game controller still exists.
            if (owner == null)
            {
                return;
            }

            owner.HandleSquareClicked(
                this
            );
        }
    }
}
