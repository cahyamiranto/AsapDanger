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
                    // Score tidak cukup: jangan kurangi quest dan JANGAN delete object
                    return; 
                }

                ScoreManager.Instance.OnBushBerhasilDipadamkan();
            }

            // 2. Progres quest
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.KurangiPadamkan();
            }

            // 3. Hapus sprite bush dari scene
            //Destroy(gameObject);
            gameObject.SetActive(false);
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

            // 3. Hapus sprite NPC dari scene
            //Destroy(gameObject);
            gameObject.SetActive(false);
        }
    }
}