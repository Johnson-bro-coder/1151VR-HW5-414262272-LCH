using UnityEngine;

public class PokeballGenerator : MonoBehaviour
{
    [Header("Prefab Reference")]
    public GameObject pokeballPrefab;

    [Header("Left-To-Right Throw Configuration")]
    public Transform throwOrigin;          // 左側訓練家右手發射點
    public Transform targetPokemon;        // 右側寶可夢目標點
    public TrainerController trainerController; // 訓練家動作控制器
    public float throwArcHeight = 3.5f;
    public float throwSpeed = 1.0f;

    [Header("Auto Demonstration")]
    public bool autoThrowOnStart = true;
    public float autoThrowDelay = 1.2f;

    private void Start()
    {
        if (autoThrowOnStart)
        {
            Invoke(nameof(ThrowLeftToRight), autoThrowDelay);
        }
    }

    private void Update()
    {
        // 點擊滑鼠左鍵或手機螢幕觸碰拋球 (同簡報 6-6-5)
        if (Input.GetMouseButtonDown(0))
        {
            // 若點擊在 UI 上由 GameDirector 處理，此處依射線或觸碰點發射
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ThrowTowardsRay(ray);
        }

        // 按空白鍵直接觸發標準「由左至右」弧線拋出 (含訓練家揮臂動作)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowLeftToRight();
        }
    }

    /// <summary>
    /// 精靈球由左至右拋出收服寶可夢 (作業核心要求：訓練家先揮臂動作，在出手點釋放球)
    /// </summary>
    public void ThrowLeftToRight()
    {
        if (pokeballPrefab == null) return;

        if (GameDirector.Instance != null)
        {
            GameDirector.Instance.SetStatus("訓練家蓄力揮臂拋出精靈球！");
        }

        // 若有綁定訓練家控制器，先播放投球動作動畫，在出手幀觸發實體拋射
        if (trainerController != null)
        {
            trainerController.PlayThrowAnimation(ExecutePhysicalThrow);
        }
        else
        {
            ExecutePhysicalThrow();
        }
    }

    private void ExecutePhysicalThrow()
    {
        Vector3 startPos = throwOrigin != null ? throwOrigin.position : new Vector3(-3.4f, 1.4f, 5.0f);
        Vector3 targetPos = targetPokemon != null ? targetPokemon.position + Vector3.up * 0.7f : new Vector3(3.8f, 0.7f, 5.0f);

        GameObject ball = Instantiate(pokeballPrefab, startPos, Quaternion.identity);
        PokeballController controller = ball.GetComponent<PokeballController>();

        // 強制忽略精靈球與訓練家身上的所有 Collider，避免出手瞬間碰撞凍結
        if (trainerController != null)
        {
            Collider ballCol = ball.GetComponent<Collider>();
            if (ballCol != null)
            {
                Collider[] tCols = trainerController.GetComponentsInChildren<Collider>();
                foreach (var tc in tCols)
                {
                    Physics.IgnoreCollision(ballCol, tc, true);
                }
            }
        }

        // 計算由左向右之精確運動學拋物線初速度
        Vector3 velocity = CalculateBallisticVelocity(startPos, targetPos, 38f);

        if (controller != null)
        {
            controller.Shoot(velocity);
        }
    }

    /// <summary>
    /// 依觸控或滑鼠射線拋球 (對應簡報 6-6-5)
    /// </summary>
    public void ThrowTowardsRay(Ray ray)
    {
        if (pokeballPrefab == null) return;

        if (trainerController != null)
        {
            trainerController.PlayThrowAnimation(() =>
            {
                Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
                GameObject ball = Instantiate(pokeballPrefab, spawnPos, Quaternion.identity);
                if (trainerController != null)
                {
                    Collider ballCol = ball.GetComponent<Collider>();
                    if (ballCol != null)
                    {
                        Collider[] tCols = trainerController.GetComponentsInChildren<Collider>();
                        foreach (var tc in tCols) Physics.IgnoreCollision(ballCol, tc, true);
                    }
                }
                PokeballController controller = ball.GetComponent<PokeballController>();
                Vector3 worldDir = ray.direction;
                if (controller != null)
                {
                    controller.Shoot(worldDir.normalized * 18f);
                }
            });
        }
        else
        {
            Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
            GameObject ball = Instantiate(pokeballPrefab, spawnPos, Quaternion.identity);
            PokeballController controller = ball.GetComponent<PokeballController>();
            Vector3 worldDir = ray.direction;
            if (controller != null)
            {
                controller.Shoot(worldDir.normalized * 18f);
            }
        }
    }

    private Vector3 CalculateBallisticVelocity(Vector3 start, Vector3 target, float angleDeg = 38f)
    {
        Vector3 planDir = target - start;
        float h = planDir.y;
        planDir.y = 0;
        float dist = planDir.magnitude;
        if (dist < 0.01f) return Vector3.forward * 10f;

        float angleRad = angleDeg * Mathf.Deg2Rad;
        float tanA = Mathf.Tan(angleRad);
        float cosA = Mathf.Cos(angleRad);
        float sinA = Mathf.Sin(angleRad);

        float g = Mathf.Abs(Physics.gravity.y);
        float denom = 2f * cosA * cosA * (dist * tanA - h);
        if (denom <= 0f)
        {
            return (target - start).normalized * 15f + Vector3.up * 4f;
        }

        float v0 = Mathf.Sqrt(g * dist * dist / denom);
        Vector3 horizDir = planDir.normalized;
        Vector3 velocity = horizDir * (v0 * cosA) + Vector3.up * (v0 * sinA);
        return velocity;
    }
}
