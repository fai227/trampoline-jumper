using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Instance Setting
    public static GameManager Instance;
    private void Awake()
    {
        Instance = this;
    } 
    #endregion

    [Header("Player")]
    public GameObject playerObject;
    public string playerName;
    private float previosPosition;

    [Header("Score")]
    public int distance;
    public int coin;

    public enum GameStatus
    {
        Title,
        Game,
        Ranking
    }
    public GameStatus gameStatus;

    void Start()
    {
        //音量設定
        InitializeAudio();

        //パネルを移動
        SetGamePanels(GameStatus.Title);
    }

    #region Audio
    [Header("Audio")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    private void InitializeAudio()
    {
        //キーがない場合は登録
        if (!PlayerPrefs.HasKey("BGM"))
        {
            PlayerPrefs.SetFloat("BGM", 0.5f);
        }
        if (!PlayerPrefs.HasKey("SE"))
        {
            PlayerPrefs.SetFloat("SE", 1f);
        }

        //スライダー反映
        bgmSlider.value = PlayerPrefs.GetFloat("BGM");
        seSlider.value = PlayerPrefs.GetFloat("SE");

        //サウンド反映
        SoundManager.Initialize();
    }
    public void SetBGMVolume(float value)
    {
        PlayerPrefs.SetFloat("BGM", value);
        SoundManager.Initialize();
    }
    public void SetSEVolume(float value)
    {
        PlayerPrefs.SetFloat("SE", value);
        SoundManager.Initialize();
    } 
    #endregion

    void Update()
    {
        if(gameStatus == GameStatus.Game)
        {
            //トランポリン処理
            GenerateTrampoline();

            //カメラの位置から生成するステージを計算
            if(playerObject.transform.position.x > previosPosition)
            {
                previosPosition += 10f;
                CreateStage(previosPosition + 10f);
            }
        }
    }

    #region Trampoline
    [Header("Trampoline")]
    [SerializeField] private GameObject trampolinePart;
    [SerializeField] private GameObject trampolineBar;
    private GameObject partObject;
    private void GenerateTrampoline()
    {
        //トランポリン生成開始（パーツ生成）
        if (Input.GetMouseButtonDown(0))
        {
            //パーツ生成
            Vector2 startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            partObject = Instantiate(trampolinePart, startPos, Quaternion.identity);
        }

        //トランポリン生成
        if (Input.GetMouseButtonUp(0) && partObject != null)
        {
            //プレビューの位置に
            //最初の計算
            Vector2 startPos = partObject.transform.position;
            Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 midPos = (startPos + endPos) / 2f;

            //完成パーツ生成
            GameObject part = Instantiate(trampolinePart, endPos, Quaternion.identity);

            //右から左向きに置かれたときは、左右を逆転
            if (startPos.x >= midPos.x)
            {
                startPos = endPos;
            }

            //トランポリンを設置
            GameObject trampolineObject = Instantiate(trampolineBar);
            //位置を設定
            trampolineObject.transform.position = midPos;
            //角度を設定
            Vector2 difVec = midPos - startPos;
            float rotZ = Mathf.Atan2(difVec.y, difVec.x) * Mathf.Rad2Deg;
            trampolineObject.transform.Rotate(new Vector3(0f, 0f, rotZ));
            //大きさを設定
            Vector3 tmpScale = trampolineObject.transform.localScale;
            tmpScale.x = difVec.magnitude * 2f;
            trampolineObject.transform.localScale = tmpScale;

            //トランポリン作成
            GameObject trampoline = new GameObject("Trampoline");
            trampolineObject.transform.SetParent(trampoline.transform);
            partObject.transform.SetParent(trampoline.transform);
            part.transform.SetParent(trampoline.transform);

            //トランポリンゲージを使用
            if(trampolineObject.transform.localScale.x < 5f)
            {
                trampolinePoint -= 1;
            }
            else if(trampolineObject.transform.localScale.x < 10f)
            {
                trampolinePoint -= 2;
            }
            else
            {
                trampolinePoint -= 3;
            }
            //ゲージ表示
            DisplayTrampolinePoint();

            //終了
            partObject = null;
        }
    }

    [SerializeField] private Image[] trampolinePointImages;
    public int trampolinePoint = 10;
    public void DisplayTrampolinePoint()
    {
        for(int i = 0; i < trampolinePointImages.Length; i++)
        {
            Color originalColor = trampolinePointImages[i].color;
            originalColor.a = i > trampolinePoint ? 0f : 1f;
            trampolinePointImages[i].color = originalColor;
        }
    }
    #endregion

    #region Title
    [Header("Title")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private InputField playerNameInputField;
    [SerializeField] private Text playerNameText;
    [SerializeField] private GameObject playerPrefab;
    

    public void StartGame()
    {
        //プレイヤー名未入力の時はゲーム開始しない
        if(playerNameInputField.text == "")
        {
            playerNameText.rectTransform.DOShakePosition(0.5f, vibrato:20);
            Color originalColor = playerNameText.color;
            playerNameText.DOColor(Color.red, 0.25f).OnComplete(() =>
            {
                playerNameText.DOColor(originalColor, 0.25f);
            });
            return;
        }
        playerName = playerNameInputField.text;

        //UIを左に動かす
        SetGamePanels(GameStatus.Game);
        

        //初期設置
        //プレイヤー設置
        playerObject = Instantiate(playerPrefab, new Vector2(-10, 0), Quaternion.identity);
        playerObject.GetComponent<Rigidbody2D>().AddForce((Vector2.right + Vector2.up) * 5f, ForceMode2D.Impulse);

        //開始時のトランポリン生成
        for(int i = 1; i < 3; i++)
        {
            float spawnX = -4f + (i - 1) * 3f;
            GameObject firstTrampoline = new GameObject("FirstTrampoline");
            Instantiate(trampolinePart, new Vector2(spawnX - 1f, -1f), Quaternion.identity, firstTrampoline.transform);
            Instantiate(trampolinePart, new Vector2(spawnX + 1f, -1f), Quaternion.identity, firstTrampoline.transform);
            GameObject bar = Instantiate(trampolineBar, new Vector2(spawnX, -1f), Quaternion.identity, firstTrampoline.transform);
            Vector3 scale = bar.transform.localScale;
            scale.x = 2f;
            bar.transform.localScale = scale;
        }



        //落石開始
        GetComponent<RockManager>().StartRock();

        //ゲーム開始
        DOVirtual.DelayedCall(0.5f, () =>
        {
            gameStatus = GameStatus.Game;
        });
    }
    #endregion

    #region Stage Generation
    [Header("Stage")]
    [SerializeField] private GameObject[] rockPrefab;

    [Header("Heal")]
    [SerializeField] private GameObject healPrefab;

    private void CreateStage(float posX)
    {
        GameObject stage = new GameObject(posX.ToString("0"));
        
        //岩設置
        int time = Random.Range(1, 3);
        for(int i = 0; i < time; i++)
        {
            float scale = Random.Range(0.5f, 0.8f);
            float pos = 10f / time * i;
            pos += pos * Random.Range(-0.1f, 0.1f); //誤差10％ずらす
            GameObject rock = Instantiate(rockPrefab[Random.Range(0, rockPrefab.Length)], new Vector2(posX + pos, Random.Range(-5f, -2.5f)), Quaternion.identity, stage.transform);
            rock.transform.localScale = new Vector2(scale, scale);
        }

        //置ける場所を探してヒールを置く
        Vector2 healPos = new Vector2(posX, Random.Range(-2.5f, -3.5f));
        while (true)
        {
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(healPos, healPrefab.transform.localScale, 0f);

            //置けるので置く
            if (hitColliders.Length == 0)
            {
                Instantiate(healPrefab, healPos, Quaternion.identity);
                break;
            }
            //置けないので次を探す
            else
            {
                healPos.x += 1f;
            }
        }

    }
    #endregion

    #region End Game
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip gameOverClip;
    public void EndGame()
    {
        //ゲームの状態変更
        gameStatus = GameStatus.Ranking;

        //石が降ってくるのを止める
        GetComponent<RockManager>().StopRock();

        //SE再生
        SoundManager.PlayOneShot(hitClip);
        DOVirtual.DelayedCall(hitClip.length, () =>
        {
            SoundManager.PlayOneShot(gameOverClip);
        });

        //ランキング計算
        ShowRanking();

        //死亡演出待ち
        DOVirtual.DelayedCall(5f, () =>
        {
            SetGamePanels(GameStatus.Ranking);
        });
    } 
    #endregion

    #region Score
    [Header("Score")]
    [SerializeField] private Text distanceText;
    [SerializeField] private Text coinText;
    public void SetScore(float distance = -1f, bool coin = false)
    {
        //ゲーム中でないときは早期リターン
        if (gameStatus != GameStatus.Game)
        {
            return;
        }

        //スコア反映
        if (distance != -1f)
        {
            this.distance = (int)distance;
        }
        if (coin)
        {
            this.coin++;
        }

        //スコア表示
        distanceText.text = this.distance.ToString("0") + "M";
        coinText.text = this.coin.ToString();

    }
    #endregion

    #region Panel Animation
    private static float panelMoveDuration = 2f;
    private GameObject nowPanel;

    [Header("PanelTrampolines")]
    [SerializeField] private GameObject firstTrampolines;
    [SerializeField] private GameObject rankingTrampoline;
    public void SetGamePanels(GameStatus status)
    {
        //一つ前のパネルを移動＆触れなくする
        if (nowPanel != null)
        {
            nowPanel.GetComponent<CanvasGroup>().interactable = false;
            nowPanel.transform.DOLocalMoveX(-800f, panelMoveDuration);
        }

        //表示するパネルを設定
        switch (status)
        {
            case GameStatus.Title:
                nowPanel = GameObject.Find("TitlePanel");
                firstTrampolines.transform.DOMoveX(0f, panelMoveDuration);
                break;

            case GameStatus.Game:
                nowPanel = GameObject.Find("GamePanel");
                firstTrampolines.transform.DOMoveX(-20f, panelMoveDuration).OnComplete(() =>
                {
                    Destroy(firstTrampolines);
                });
                break;

            case GameStatus.Ranking:
                rankingTrampoline.transform.localPosition = new Vector2(Camera.main.transform.position.x + 12f, -4f);
                rankingTrampoline.transform.DOMoveX(Camera.main.transform.position.x, panelMoveDuration);
                nowPanel = GameObject.Find("RankingPanel");

                //石とヒールを消す
                foreach(GameObject rock in GameObject.FindGameObjectsWithTag("Rock"))
                {
                    rock.GetComponent<SpriteRenderer>()?.DOFade(0f, panelMoveDuration);                    
                }
                foreach(GameObject heal in GameObject.FindGameObjectsWithTag("Heal"))
                {
                    heal.transform.DOScale(Vector2.zero, panelMoveDuration);
                }

                //エフェクトを止める
                foreach(GameObject effect in GameObject.FindGameObjectsWithTag("Effect"))
                {
                    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                    if(ps != null)
                    {
                        ps.Stop(true,  ParticleSystemStopBehavior.StopEmitting);
                    }
                }
                break;
        }

        //表示するパネルを移動
        nowPanel.transform.DOLocalMoveX(0f, panelMoveDuration).OnComplete(() =>
        {
            //触れるようにする
            nowPanel.GetComponent<CanvasGroup>().interactable = true;
        });
    }
    #endregion

    #region Ranking
    [SerializeField] private Text firstText;
    [SerializeField] private Text secondText;
    [SerializeField] private Text thirdText;
    [SerializeField] private Text laterText;
    private void ShowRanking()
    {
        //ランキング取得
        int[] distances = new int[11];
        string[] players = new string[11];
        for(int i = 0; i < 10; i++)
        {
            distances[i] = PlayerPrefs.GetInt("Rank" + i.ToString(), 0);
            players[i] = PlayerPrefs.GetString("Player" + i.ToString(), "");
        }
        distances[10] = distance;
        players[10] = playerName;

        //ランキング計算
        for(int i = 0; i < 11; i++)
        {
            //最大値検索
            int maxValue = distances[i];
            int maxNum = i;
            for(int x = i; x < 11; x++)
            {
                if(maxValue < distances[x])
                {
                    maxValue = distances[x];
                    maxNum = x;
                }
            }

            //最大値を先頭へ
            //得点を移動
            int tmpNum = distances[i];
            distances[i] = distances[maxNum];
            distances[maxNum] = tmpNum;
            //名前を移動
            string tmpName = players[i];
            players[i] = players[maxNum];
            players[maxNum] = tmpName;
        }

        //ランキング表示
        if (players[0] != "")
        {
            firstText.text = "1. " + distances[0].ToString("0000") + "M " + players[0].PadLeft(10);
        }
        if(players[1] != "")
        {
            secondText.text = "2. " + distances[1].ToString("0000") + "M " + players[1].PadLeft(10);
        }
        if (players[2] != "")
        {
            thirdText.text = "3. " + distances[2].ToString("0000") + "M " + players[2].PadLeft(10);
        }

        string result = "";
        for (int i = 3; i < 1; i++)
        {
            if (players[i] == "")
            {
                break;
            }

            result += (i + 1).ToString() + ". " + distances[i].ToString("0000") + "M " + players[i] + "\n";
        }
        laterText.text = result;

        //ランキング登録
        for(int i = 0; i < 10; i++)
        {
            if (players[i] == "")
                break;

            PlayerPrefs.SetInt("Rank" + i.ToString(), distances[i]);
            PlayerPrefs.SetString("Player" + i.ToString(), players[i]);
        }
    }
    #endregion

    public void ResetGame()
    {
        nowPanel.GetComponent<CanvasGroup>().interactable = false;
        nowPanel.transform.DOLocalMoveX(-800f, panelMoveDuration);
        rankingTrampoline.transform.DOMoveX(-20f, panelMoveDuration).OnComplete(() =>
        {
            SceneManager.LoadScene("Game");
        });
    }
}
