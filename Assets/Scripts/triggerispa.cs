// FireProgressBar.cs
// Attach to any GameObject in your scene that owns the progress bar UI.
//
// FUTURE INTEGRATION POINTS:
//   Fire spawn script  → call fireProgressBar.FireSpawned();
//   Fire stop script   → call fireProgressBar.FireStopped();
//   Final image asset  → assign it to the "Progress Bar" field in the Inspector
//
// To remove manual testing later: delete the "#region Manual Testing" block
// and the OnValidate() method. Nothing else needs to change.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FireProgressBar : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector: Visual references
    // -------------------------------------------------------------------------
    [Header("UI References")]
    [Tooltip("Image component with Type=Filled, Fill Method=Horizontal, Fill Origin=Left")]
    public Image progressBar;

    [Tooltip("Optional. Displays '5 / 15'. Leave unassigned to skip.")]
    public TMP_Text countText;

    // -------------------------------------------------------------------------
    // Inspector: Tuning
    // -------------------------------------------------------------------------
    [Header("Settings")]
    [Tooltip("Hard cap on fire count. Progress bar is full at this value.")]
    public int maxFireCount = 15;

    [Tooltip("Fire count at which the bar switches to warning color.")]
    public int warningThreshold = 10;

    [Header("Colors")]
    public Color normalColor  = Color.green;
    public Color warningColor = Color.red;

    // -------------------------------------------------------------------------
    // Inspector: Manual testing (remove this region when real triggers exist)
    // -------------------------------------------------------------------------
    [Header("─── TESTING ───────────────────────────────")]
    [Tooltip("Set this value in Play Mode to simulate fires. Range 0–maxFireCount.")]
    public int manualFireCount = 0;

    // -------------------------------------------------------------------------
    // Internal state
    // -------------------------------------------------------------------------
    private int _currentFireCount = 0;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------
    private void Start()
    {
        RefreshUI();
    }

#if UNITY_EDITOR
    // OnValidate runs in the Editor whenever an Inspector field changes,
    // giving you live preview without entering Play Mode.
    private void OnValidate()
    {
        manualFireCount = Mathf.Clamp(manualFireCount, 0, maxFireCount);
        _currentFireCount = manualFireCount;

        // RefreshUI touches UnityEngine.UI which requires the game loop in
        // builds, so guard it to Editor-only to avoid import warnings.
        if (Application.isPlaying)
            RefreshUI();
        else
            RefreshUIEditorSafe();
    }
#endif

    // Called every frame in Play Mode so the manual slider stays live.
    private void Update()
    {
        // #region Manual Testing — delete from here ...
        int clamped = Mathf.Clamp(manualFireCount, 0, maxFireCount);
        if (clamped != _currentFireCount)
        {
            _currentFireCount = clamped;
            manualFireCount   = clamped;
            RefreshUI();
        }
        // ... to here, when real fire triggers are wired up.
        // #endregion Manual Testing
    }

    // -------------------------------------------------------------------------
    // Public API — call these from your fire spawn / extinguish scripts
    // -------------------------------------------------------------------------

    /// <summary>Call this whenever an illegal fire spawns.</summary>
    public void FireSpawned()
    {
        SetFireCount(_currentFireCount + 1);
    }

    /// <summary>Call this whenever an illegal fire is stopped/extinguished.</summary>
    public void FireStopped()
    {
        SetFireCount(_currentFireCount - 1);
    }

    /// <summary>Set the fire count directly (clamped to 0–maxFireCount).</summary>
    public void SetFireCount(int count)
    {
        _currentFireCount = Mathf.Clamp(count, 0, maxFireCount);
        manualFireCount   = _currentFireCount; // keep Inspector in sync
        RefreshUI();
    }

    /// <summary>Read-only access to the current count.</summary>
    public int CurrentFireCount => _currentFireCount;

    // -------------------------------------------------------------------------
    // UI update
    // -------------------------------------------------------------------------
    private void RefreshUI()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = (float)_currentFireCount / maxFireCount;
            progressBar.color      = _currentFireCount >= warningThreshold
                                     ? warningColor
                                     : normalColor;
        }

        if (countText != null)
            countText.text = $"{_currentFireCount} / {maxFireCount}";
    }

#if UNITY_EDITOR
    // Lightweight version that only touches fields safe to set outside Play Mode.
    private void RefreshUIEditorSafe()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = (float)_currentFireCount / maxFireCount;
            progressBar.color      = _currentFireCount >= warningThreshold
                                     ? warningColor
                                     : normalColor;
        }
    }
#endif
}
