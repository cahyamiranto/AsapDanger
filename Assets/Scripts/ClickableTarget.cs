using UnityEngine;

public class ClickableTarget : MonoBehaviour
{
    public enum TargetType
    {
        BushPadamkan,
        NpcLapor
    }

    [Header("Konfigurasi Target")]
    [SerializeField] private TargetType tipeTarget;

    private void OnMouseDown()
    {
        if (tipeTarget == TargetType.BushPadamkan)
        {
            // 1. Cek kecukupan score
            if (ScoreManager.Instance != null)
            {
                if (!ScoreManager.Instance.BisaPadamkanBush())
                {
                    // Score tidak cukup: jangan kurangi quest dan JANGAN nonaktifkan object
                    return; 
                }

                ScoreManager.Instance.OnBushBerhasilDipadamkan();
            }

            // 2. Progres quest
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.KurangiPadamkan();
            }

            // 3. Nonaktifkan parent dari objek ini
            HideParent();
        }
        else if (tipeTarget == TargetType.NpcLapor)
        {
            // 1. Progres quest
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.KurangiLapor();
            }

            // 2. Tambah score
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnNpcClicked();
            }

            // 3. Nonaktifkan parent dari objek ini
            gameObject.SetActive(false);
        }
    }

    private void HideParent()
    {
        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}