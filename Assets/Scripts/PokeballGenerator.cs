using UnityEngine;

public class PokeballGenerator : MonoBehaviour
{
    [Header("Prefab Reference")]
    public GameObject pokeballPrefab;

    [Header("Left-To-Right Throw Configuration")]
    public Transform throwOrigin;     // 左側訓練家右手發射點
    public Transform targetPokemon;   // 右側寶可夢目標點
    public float throwArcHeight = 3.5f;
    public float throwSpeed = 1.0f;

    [Header("Auto Demonstration")]
    public bool autoThrowOnStart = true;
    public float autoThrowDelay = 1.0f;

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
            // 若點擊在 UI 上則不重複觸發，或直接依滑鼠視角發射
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ThrowTowardsRay(ray);
        }

        // 按空白鍵直接觸發標準「由左至右」弧線拋出
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowLeftToRight();
        }
    }

    /// <summary>
    /// 精靈球由左至右拋出收服寶可夢 (作業核心要求)
    /// </summary>
    public void ThrowLeftToRight()
    {
        if (pokeballPrefab == null) return;

        Vector3 startPos = throwOrigin != null ? throwOrigin.position : new Vector3(-4.0f, 1.6f, 5.0f);
        Vector3 targetPos = targetPokemon != null ? targetPokemon.position + Vector3.up * 0.7f : new Vector3(4.5f, 0.7f, 5.0f);

        GameObject ball = Instantiate(pokeballPrefab, startPos, Quaternion.identity);
        PokeballController controller = ball.GetComponent<PokeballController>();

        // 計算由左向右之拋物線初速度 (利用物理運動學公式)
        Vector3 force = CalculateBallisticVelocity(startPos, targetPos, 45f);

        if (controller != null)
        {
            controller.Shoot(force);
        }

        if (GameDirector.Instance != null)
        {
            GameDirector.Instance.SetStatus("訓練家由左至右拋出精靈球！");
        }
    }

    /// <summary>
    /// 依觸控或滑鼠射線拋球 (對應簡報 6-6-5)
    /// </summary>
    public void ThrowTowardsRay(Ray ray)
    {
        if (pokeballPrefab == null) return;

        Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : Camera.main.transform.position + Camera.main.transform.forward * 0.8f;
        GameObject ball = Instantiate(pokeballPrefab, spawnPos, Quaternion.identity);
        PokeballController controller = ball.GetComponent<PokeballController>();

        Vector3 worldDir = ray.direction;
        if (controller != null)
        {
            controller.Shoot(worldDir.normalized * 1200f);
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

        // 克服重力計算速度
        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocity = Mathf.Sqrt(dist * gravity / Mathf.Sin(2 * a));
        return velocity * dir.normalized * 50f; // 轉換為 AddForce 力道倍率
    }
}
