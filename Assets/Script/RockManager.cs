using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockManager : MonoBehaviour
{
    [Header("Falling Rocks")]
    [SerializeField] private GameObject[] rockPrefabs;

    [Header("Coin")]
    [SerializeField] private GameObject coinPrefab;

    [Header("Wall")]
    [SerializeField] private GameObject wallObject;
    private float wallSpeed = 2f;

    //壁の落石
    private IEnumerator WallRockCoroutine()
    {
        while (true)
        {
            GameObject rockObject = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], wallObject.transform);
            
            float rPos = Random.Range(-0.1f, 0.1f);
            float rScale = Random.Range(0.2f, 1f);

            Vector3 rockPos = rockObject.transform.localPosition;
            rockPos.x += rPos;
            rockObject.transform.localPosition = rockPos;

            rockObject.transform.localScale = new Vector3(rScale, rScale, rScale);

            rockObject.transform.Rotate(0f, 0f, Random.Range(0f, 360f));

            yield return new WaitForSeconds(Random.Range(0.4f, 0.8f));
        }
    }

    private IEnumerator CeilCoroutine()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            Vector2 pos = new Vector2(GameManager.Instance.playerObject.transform.position.x + Random.Range(0f, 20f), 7f);
            
            //ランダムで選出
            if (Random.Range(0, 2) == 0) //1/2で石
            {
                GameObject fallItem = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)]);
                fallItem.transform.position = pos;
                float rScale = Random.Range(0.25f, 0.75f);
                fallItem.transform.localScale = new Vector3(rScale, rScale, rScale);
                fallItem.transform.Rotate(new Vector3(0f, 0f, Random.Range(0f, 360f)));
            }
            else //その他はコイン 
            {
                Instantiate(coinPrefab).transform.position = pos;
            }

            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        }
    }

    public void StartRock()
    {
        StartCoroutine(WallRockCoroutine());
        StartCoroutine(CeilCoroutine());
    }
    public void StopRock()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if(GameManager.Instance.gameStatus == GameManager.GameStatus.Game)
        {
            //壁を進める
            Vector2 wallObjectPosition = wallObject.transform.position;
            wallObjectPosition.x += wallSpeed * Time.deltaTime;
            wallObject.transform.position = wallObjectPosition;
        }
    }
}