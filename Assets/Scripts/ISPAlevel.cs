using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ISPAlevel : MonoBehaviour
{
    // ============================================================
    // GAME SETTINGS
    // ============================================================

    [Header("Game Settings")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 2.0f;
    [SerializeField] private float squareSize = 30f;

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
    // PROGRESS BAR
    // ============================================================

    [Header("Progress Bar")]

    [SerializeField] private float progressBarWidth = 500f;

    [SerializeField] private float progressBarHeight = 30f;

    [SerializeField] private float progressBarTopMargin = 40f;

    [Tooltip("Thickness of the grey border surrounding the fill.")]
    [SerializeField] private float progressBarBorderThickness = 7f;

    [Tooltip("Roundness of the progress bar corners.")]
    [SerializeField] private float progressBarCornerRadius = 15f;

    // Grey track
    [SerializeField] private Color trackColor =
        new Color(0.25f, 0.25f, 0.25f, 1f);

    // Pink at zero
    [SerializeField] private Color pinkFillColor =
        new Color(1f, 0.20f, 0.55f, 1f);

    // Dark purple at maximum
    [SerializeField] private Color darkPurpleFillColor =
        new Color(0.22f, 0.02f, 0.30f, 1f);

    // ============================================================
    // LUNG IMAGE
    // ============================================================

    [Header("Lung Image")]

    [Tooltip("Image displayed at the end of the progress bar.")]
    [SerializeField]
    private string lungImageURL =
        "https://static.vecteezy.com/system/resources/thumbnails/072/638/320/small/illustration-of-human-lungs-in-pink-color-122-png.png";

    [Tooltip("Size of the lung image.")]
    [SerializeField]
    private float lungImageSize = 70f;

    [Tooltip("How far the lung image sits above/beside the end of the bar.")]
    [SerializeField]
    private float lungVerticalOffset = 0f;

    [Tooltip("Brightness when there are 0 squares.")]
    [SerializeField]
    private float lungBrightnessAtZero = 1f;

    [Tooltip("Brightness when there are 15 squares.")]
    [SerializeField]
    private float lungBrightnessAtMaximum = 0.35f;

    [Tooltip("Scale when lung is in its large state.")]
    [SerializeField]
    private float lungLargeScale = 1f;

    [Tooltip("Scale when lung is in its small state.")]
    [SerializeField]
    private float lungSmallScale = 0.8f;

    [Tooltip("Seconds between each scale change.")]
    [SerializeField]
    private float lungPulseInterval = 1f;

    // ============================================================
    // UI REFERENCES
    // ============================================================

    private RectTransform progressBarRoot;

    private Image trackImage;

    private Image fillImage;

    private RectTransform fillRect;

    private Image lungImage;

    private RectTransform lungRect;

    private Sprite roundedSprite;

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

        progressBarBorderThickness = Mathf.Max(
            1f,
            progressBarBorderThickness
        );

        progressBarCornerRadius = Mathf.Max(
            1f,
            progressBarCornerRadius
        );

        lungImageSize = Mathf.Max(
            1f,
            lungImageSize
        );

        lungPulseInterval = Mathf.Max(
            0.05f,
            lungPulseInterval
        );

        // --------------------------------------------------------
        // FIND CANVAS
        // --------------------------------------------------------

        if (uiCanvas == null)
        {
            Canvas canvas =
                FindAnyObjectByType<Canvas>();

            if (canvas != null)
            {
                uiCanvas =
                    canvas.GetComponent<RectTransform>();
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
        // CREATE UI
        // --------------------------------------------------------

        CreateProgressBar();

        currentSquares =
            Mathf.Clamp(
                currentSquares,
                0,
                MAX_SQUARES
            );

        UpdateProgressBar();

        // Load lung image.
        StartCoroutine(
            LoadLungImage()
        );

        // Start lung animation.
        StartCoroutine(
            LungPulseLoop()
        );

        // Start spawning.
        StartCoroutine(
            SpawnLoop()
        );
    }

    // ============================================================
    // CREATE PROGRESS BAR
    // ============================================================

    private void CreateProgressBar()
    {
        // --------------------------------------------------------
        // CREATE ROUNDED SPRITE
        // --------------------------------------------------------

        roundedSprite =
            CreateRoundedSprite(
                128,
                128,
                progressBarCornerRadius
            );

        // --------------------------------------------------------
        // PROGRESS BAR ROOT
        // --------------------------------------------------------

        GameObject rootObject =
            new GameObject(
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

        GameObject trackObject =
            new GameObject(
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

        trackImage.sprite =
            roundedSprite;

        trackImage.type =
            Image.Type.Sliced;

        trackImage.color =
            trackColor;

        trackImage.raycastTarget =
            false;

        // --------------------------------------------------------
        // FILL
        // --------------------------------------------------------

        GameObject fillObject =
            new GameObject(
                "Fill",
                typeof(RectTransform),
                typeof(Image)
            );

        fillObject.transform.SetParent(
            progressBarRoot,
            false
        );

        fillRect =
            fillObject.GetComponent<RectTransform>();

        fillImage =
            fillObject.GetComponent<Image>();

        // The fill stays inside the grey track.
        fillRect.anchorMin =
            new Vector2(0f, 0f);

        fillRect.anchorMax =
            new Vector2(0f, 1f);

        fillRect.pivot =
            new Vector2(0f, 0.5f);

        fillRect.anchoredPosition =
            new Vector2(
                progressBarBorderThickness,
                0f
            );

        fillRect.sizeDelta =
            new Vector2(
                0f,
                -progressBarBorderThickness * 2f
            );

        fillImage.sprite =
            roundedSprite;

        fillImage.type =
            Image.Type.Sliced;

        fillImage.color =
            pinkFillColor;

        fillImage.raycastTarget =
            false;
    }

    // ============================================================
    // ROUNDED SPRITE CREATION
    // ============================================================

    private Sprite CreateRoundedSprite(
        int textureWidth,
        int textureHeight,
        float radius
    )
    {
        Texture2D texture =
            new Texture2D(
                textureWidth,
                textureHeight,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "ProceduralRoundedProgressBar";

        texture.wrapMode =
            TextureWrapMode.Clamp;

        Color32[] pixels =
            new Color32[
                textureWidth *
                textureHeight
            ];

        float r =
            Mathf.Clamp(
                radius,
                1f,
                Mathf.Min(
                    textureWidth,
                    textureHeight
                ) * 0.5f
            );

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                float dx = 0f;
                float dy = 0f;

                if (px < r)
                {
                    dx = r - px;
                }
                else if (px > textureWidth - r)
                {
                    dx = px - (textureWidth - r);
                }

                if (py < r)
                {
                    dy = r - py;
                }
                else if (py > textureHeight - r)
                {
                    dy = py - (textureHeight - r);
                }

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy
                    );

                float alpha =
                    distance <= r
                        ? 1f
                        : 0f;

                pixels[
                    y * textureWidth + x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        float border =
            textureWidth * 0.5f;

        Sprite sprite =
            Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    textureWidth,
                    textureHeight
                ),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(
                    border,
                    border,
                    border,
                    border
                )
            );

        return sprite;
    }

    // ============================================================
    // LUNG IMAGE LOADING
    // ============================================================

    private IEnumerator LoadLungImage()
    {
        using (
            UnityWebRequest request =
                UnityWebRequestTexture.GetTexture(
                    lungImageURL
                )
        )
        {
            yield return request.SendWebRequest();

            if (
                request.result !=
                UnityWebRequest.Result.Success
            )
            {
                Debug.LogError(
                    "ISPAlevel: Could not load lung image: " +
                    request.error
                );

                yield break;
            }

            Texture2D texture =
                DownloadHandlerTexture
                    .GetContent(request);

            if (texture == null)
            {
                Debug.LogError(
                    "ISPAlevel: Lung image texture is null."
                );

                yield break;
            }

            Sprite lungSprite =
                Sprite.Create(
                    texture,
                    new Rect(
                        0,
                        0,
                        texture.width,
                        texture.height
                    ),
                    new Vector2(
                        0.5f,
                        0.5f
                    ),
                    100f,
                    0,
                    SpriteMeshType.FullRect
                );

            CreateLungUI(
                lungSprite
            );
        }
    }

    // ============================================================
    // CREATE LUNG UI
    // ============================================================

    private void CreateLungUI(
        Sprite lungSprite
    )
    {
        GameObject lungObject =
            new GameObject(
                "Lung",
                typeof(RectTransform),
                typeof(Image)
            );

        lungObject.transform.SetParent(
            progressBarRoot,
            false
        );

        lungRect =
            lungObject.GetComponent<RectTransform>();

        lungImage =
            lungObject.GetComponent<Image>();

        lungRect.anchorMin =
            new Vector2(0f, 0.5f);

        lungRect.anchorMax =
            new Vector2(0f, 0.5f);

        lungRect.pivot =
            new Vector2(0.5f, 0.5f);

        lungRect.sizeDelta =
            new Vector2(
                lungImageSize,
                lungImageSize
            );

        lungImage.sprite =
            lungSprite;

        lungImage.preserveAspect =
            true;

        lungImage.raycastTarget =
            false;

        UpdateLungPosition();

        UpdateLungBrightness();
    }

    // ============================================================
    // LUNG POSITION
    // ============================================================

    private void UpdateLungPosition()
    {
        if (
            lungRect == null ||
            fillRect == null
        )
        {
            return;
        }

        float progress =
            (float)currentSquares /
            MAX_SQUARES;

        progress =
            Mathf.Clamp01(progress);

        float usableWidth =
            progressBarWidth -
            progressBarBorderThickness * 2f;

        usableWidth =
            Mathf.Max(
                0f,
                usableWidth
            );

        float fillWidth =
            usableWidth * progress;

        // The lung sits exactly at the right end
        // of the fill.
        float lungX =
            progressBarBorderThickness +
            fillWidth;

        lungRect.anchoredPosition =
            new Vector2(
                lungX,
                lungVerticalOffset
            );
    }

    // ============================================================
    // LUNG BRIGHTNESS
    // ============================================================

    private void UpdateLungBrightness()
    {
        if (lungImage == null)
        {
            return;
        }

        float progress =
            (float)currentSquares /
            MAX_SQUARES;

        progress =
            Mathf.Clamp01(progress);

        // 0 = bright
        // 15 = dark
        float brightness =
            Mathf.Lerp(
                lungBrightnessAtZero,
                lungBrightnessAtMaximum,
                progress
            );

        lungImage.color =
            new Color(
                brightness,
                brightness,
                brightness,
                1f
            );
    }

    // ============================================================
    // LUNG PULSE
    // ============================================================

    private IEnumerator LungPulseLoop()
    {
        bool large =
            true;

        while (enabled)
        {
            if (lungRect != null)
            {
                lungRect.localScale =
                    Vector3.one *
                    (
                        large
                            ? lungLargeScale
                            : lungSmallScale
                    );
            }

            large =
                !large;

            yield return new WaitForSeconds(
                lungPulseInterval
            );
        }
    }

    // ============================================================
    // SPAWN LOOP
    // ============================================================

    // private IEnumerator SpawnLoop()
    // {
    //     while (enabled)
    //     {
    //         float waitTime =
    //             Random.Range(
    //                 minSpawnInterval,
    //                 maxSpawnInterval
    //             );

    //         yield return new WaitForSeconds(
    //             waitTime
    //         );

    //         if (currentSquares >= MAX_SQUARES)
    //         {
    //             continue;
    //         }

    //         SpawnSquare();
    //     }
    // }

    // ============================================================
    // SPAWN SQUARE
    // ============================================================

    // private void SpawnSquare()
    // {
    //     ClampCounter();

    //     if (currentSquares >= MAX_SQUARES)
    //     {
    //         return;
    //     }

    //     GameObject squareObject =
    //         new GameObject(
    //             "ClickableSquare",
    //             typeof(RectTransform),
    //             typeof(Image)
    //         );

    //     squareObject.transform.SetParent(
    //         uiCanvas,
    //         false
    //     );

    //     RectTransform squareRect =
    //         squareObject.GetComponent<RectTransform>();

    //     Image squareImage =
    //         squareObject.GetComponent<Image>();

    //     squareRect.sizeDelta =
    //         new Vector2(
    //             squareSize,
    //             squareSize
    //         );

    //     squareImage.color =
    //         squareColor;

    //     squareImage.raycastTarget =
    //         true;

    //     squareRect.anchoredPosition =
    //         GetRandomPlayablePosition();

    //     ClickableSquare clickableSquare =
    //         squareObject.AddComponent<
    //             ClickableSquare
    //         >();

    //     clickableSquare.Initialize(
    //         this
    //     );

    //     currentSquares++;

    //     ClampCounter();

    //     UpdateProgressBar();
    // }

    // ============================================================
    // RANDOM POSITION
    // ============================================================

    // private Vector2 GetRandomPlayablePosition()
    // {
    //     Rect canvasRect =
    //         uiCanvas.rect;

    //     float halfSquare =
    //         squareSize * 0.5f;

    //     float minX =
    //         canvasRect.xMin +
    //         playableAreaPadding +
    //         halfSquare;

    //     float maxX =
    //         canvasRect.xMax -
    //         playableAreaPadding -
    //         halfSquare;

    //     float minY =
    //         canvasRect.yMin +
    //         playableAreaPadding +
    //         halfSquare;

    //     float maxY =
    //         canvasRect.yMax -
    //         playableAreaPadding -
    //         halfSquare;

    //     if (maxX < minX)
    //     {
    //         float centerX =
    //             (
    //                 canvasRect.xMin +
    //                 canvasRect.xMax
    //             ) * 0.5f;

    //         minX = centerX;
    //         maxX = centerX;
    //     }

    //     if (maxY < minY)
    //     {
    //         float centerY =
    //             (
    //                 canvasRect.yMin +
    //                 canvasRect.yMax
    //             ) * 0.5f;

    //         minY = centerY;
    //         maxY = centerY;
    //     }

    //     return new Vector2(
    //         Random.Range(minX, maxX),
    //         Random.Range(minY, maxY)
    //     );
    // }

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

        if (square.HasBeenClicked)
        {
            return;
        }

        square.MarkAsClicked();

        Destroy(
            square.gameObject
        );

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
            Mathf.Clamp(
                currentSquares,
                0,
                MAX_SQUARES
            );
    }

    // ============================================================
    // UPDATE PROGRESS BAR
    // ============================================================

    private void UpdateProgressBar()
    {
        if (fillImage == null)
        {
            return;
        }

        ClampCounter();

        // --------------------------------------------------------
        // PROGRESS
        // --------------------------------------------------------

        float progress =
            (float)currentSquares /
            MAX_SQUARES;

        progress =
            Mathf.Clamp01(progress);

        // --------------------------------------------------------
        // FILL WIDTH
        // --------------------------------------------------------

        float usableWidth =
            progressBarWidth -
            progressBarBorderThickness * 2f;

        usableWidth =
            Mathf.Max(
                0f,
                usableWidth
            );

        float fillWidth =
            usableWidth * progress;

        fillRect.sizeDelta =
            new Vector2(
                fillWidth,
                -progressBarBorderThickness * 2f
            );

        // --------------------------------------------------------
        // COLOR GRADIENT
        //
        // 0  = PINK
        // 15 = DARK PURPLE
        //
        // Every number in between is interpolated.
        // --------------------------------------------------------

        fillImage.color =
            Color.Lerp(
                pinkFillColor,
                darkPurpleFillColor,
                progress
            );

        // --------------------------------------------------------
        // LUNG
        // --------------------------------------------------------

        UpdateLungPosition();

        UpdateLungBrightness();
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
            FindObjectsByType<
                ClickableSquare
            >();

        foreach (
            ClickableSquare square
            in squares
        )
        {
            if (
                square != null &&
                square.Owner == this
            )
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
            if (hasBeenClicked)
            {
                return;
            }

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


