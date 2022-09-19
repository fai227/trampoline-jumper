using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    private Camera mainCamera;
    private static float difX = 3f;

    [SerializeField] private GameObject lavaEffect;

    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip healClip;

    private bool isDead;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        //進んだ際にはカメラを動かす
        if(mainCamera.transform.position.x - transform.position.x < difX)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            cameraPos.x = transform.position.x + difX;
            mainCamera.transform.position = cameraPos;
            
            //スコア反映
            GameManager.Instance.SetScore(distance: mainCamera.transform.position.x);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //死んでるときは何もしない
        if (isDead)
        {
            return;
        }

        if(collision.transform.CompareTag("Lava") || collision.transform.CompareTag("Rock"))
        {
            if (collision.transform.CompareTag("Lava"))
            {
                //マグマの演出表示
                Instantiate(lavaEffect, transform.position - Vector3.down * 0.5f, Quaternion.identity);
            }

            //ゲーム終了
            if(GameManager.Instance.gameStatus == GameManager.GameStatus.Game)
            {
                GameManager.Instance.EndGame();
                isDead = true;
            }
            else
            {
                return;
            }

            //全ての石を止める
            foreach(GameObject rock in GameObject.FindGameObjectsWithTag("Rock"))
            {
                Rigidbody2D rig2D = rock.GetComponent<Rigidbody2D>();
                if(rig2D != null)
                {
                    rig2D.simulated = false;
                }
            }

            //重力停止
            Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
            rigidbody2D.simulated = false;
            rigidbody2D.velocity = Vector2.zero;

            //手前に表示
            GetComponent<SpriteRenderer>().sortingOrder = 200;

            //死亡演出
            transform.DOShakePosition(1f, strength:0.1f).OnComplete(() =>
            {
                transform.DOLocalRotate(new Vector3(0f, 0f, 180f), 1f);
                rigidbody2D.simulated = true;
                rigidbody2D.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
            });
        }
        else if (collision.transform.CompareTag("Coin"))
        {
            Destroy(collision.transform.gameObject);
            GameManager.Instance.GetCoin();
            SoundManager.PlayOneShot(coinClip);
        }
        else if (collision.transform.CompareTag("Heal"))
        {
            Destroy(collision.transform.gameObject);
            GameManager.Instance.trampolinePoint = 10;
            GameManager.Instance.DisplayTrampolinePoint();

            SoundManager.PlayOneShot(healClip);
        }
    }
}