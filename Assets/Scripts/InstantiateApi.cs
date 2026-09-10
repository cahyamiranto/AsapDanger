using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateApi : MonoBehaviour
{
    [Header("List Prefab")]
    public List<GameObject> listApi = new List<GameObject>();

    [Header("Interval Waktu Spawn (Detik)")]
    public float min = 1f;
    public float max = 3f;
    float waitTime;
    [SerializeField] ISPAlevel ispaLevel; // Referensi ke script ISPAlevel



    void Awake()
    {
        // Otomatis mencari ISPAlevel jika belum di-assign di Inspector
        if (ispaLevel == null)
        {
            ispaLevel = FindAnyObjectByType<ISPAlevel>();
        }
    }

    void Start()
    {
        // Memulai coroutine pertama kali saat Start
        StartCoroutine(SpawnApiRoutine());
    }

    // Coroutine untuk instantiate object listApi
    public IEnumerator SpawnApiRoutine()
    {
        // Memastikan list tidak kosong
        if (listApi != null && listApi.Count > 0)
        {
            GameObject selectedApi = listApi[Random.Range(0, listApi.Count)];
            if(selectedApi != null && selectedApi.activeSelf == false)
            {
                ScriptApi api = selectedApi.GetComponent<ScriptApi>();
                selectedApi.SetActive(true);

                if (api != null && api.orang != null)
                {
                    api.orang.SetActive(true);
                }

                if (ispaLevel != null)
                {
                    ispaLevel.currentSquares++; // Menambah counter currentSquares di script ISPAlevel
                    ispaLevel.ClampCounter(); // Memanggil fungsi ClampCounter() dari script ISPAlevel
                    ispaLevel.UpdateProgressBar(); // Memanggil fungsi UpdateProgressBar() dari script ISPAlevel
                }
                else
                {
                    Debug.LogWarning("ISPAlevel belum di-assign pada InstantiateApi dan tidak ditemukan di Scene!");
                }

                DelayScript delayScript = selectedApi.GetComponentInChildren<DelayScript>();
                if (delayScript != null)
                {
                    delayScript.MulaiTimer(delayScript.delayTime);
                    waitTime = Random.Range(Mathf.Min(min, max), Mathf.Max(min, max));
                }
            }
            else{
                waitTime = 0;
            }
        }

        // Tunggu beberapa detik secara acak antara min dan max
        yield return new WaitForSeconds(waitTime);

        // Di akhir coroutine panggil lagi coroutine tersebut
        StartCoroutine(SpawnApiRoutine());
    }
}
