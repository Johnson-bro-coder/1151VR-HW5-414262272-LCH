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
        Vector3 startPos = throwOrigin != null ? throwOrigin.position : new Vector3(-3.8f, 1.4f, 5.0f);
        Vector3 targetPos = targetPokemon != null ? targetPokemon.position + Vector3.up * 0.7f : new Vector3(3.8f, 0.7f, 5.0f);

        GameObject ball = Instantiate(pokeballPrefab, startPos, Quaternion.identity);
        PokeballController controller = ball.GetComponent<PokeballController>();

        // 計算由左向右之拋物線初速度 (利用物理運動學公式)
        Vector3 force = CalculateBallisticVelocity(startPos, targetPos, 45f);

        if (controller != null)
        {
            controller.Shoot(force);
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
                PokeballController controller = ball.GetComponent<PokeballController>();
                Vector3 worldDir = ray.direction;
                if (controller != null)
                {
                    controller.Shoot(worldDir.normalized * 1200f);
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
                controller.Shoot(worldDir.normalized * 1200f);
            }
        }
    }

    private Vector3 CalculateBallisticVelocity(Vector3 start, Vector3 target, float angle)
    {
        Vector3 dir = target - start;
        float h = dir.y;
        dir.y = 0;
        float dist = dir.magnitude;
        float a = angle * Mathf.Deg2Rad;
        dir.y = dist * Mathf.Tan(a);
        dist += h / Mathf.Tan(a);

        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocity = Mathf.Sqrt(dist * gravity / Mathf.Sin(2 * a));
        return velocity * dir.normalized * 50f; // 轉換為 AddForce 力道倍率
    }
}
