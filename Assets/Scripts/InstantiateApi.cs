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



    void Start()
    {
        // Memulai coroutine pertama kali saat Start
        StartCoroutine(SpawnApiRoutine());
    }

    // Coroutine untuk instantiate object listApi
    public IEnumerator SpawnApiRoutine()
    {
        float waitTime;
        // Memastikan list tidak kosong
        if (listApi != null && listApi.Count > 0)
        {
            GameObject selectedApi = listApi[Random.Range(0, listApi.Count)];
            if(selectedApi.activeSelf == false)
            {
                ScriptApi api = selectedApi.GetComponent<ScriptApi>();
                // Memilih objek secara acak dari listApi
                int randomIndex = Random.Range(0, listApi.Count);
                selectedApi.SetActive(true);
                api.orang.SetActive(true);
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
