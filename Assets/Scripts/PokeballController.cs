using System.Collections;
using UnityEngine;

public class PokeballController : MonoBehaviour
{
    private Rigidbody rb;
    private ParticleSystem hitParticles;
    private bool hasCollided = false;

    [Header("Visual References")]
    public Transform visualModel;
    public ParticleSystem successStarsParticle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hitParticles = GetComponent<ParticleSystem>();
    }

    public void Shoot(Vector3 velocity)
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocity;
            rb.angularVelocity = new Vector3(Random.Range(5f, 12f), Random.Range(3f, 8f), Random.Range(5f, 12f));
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayThrow();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // 絕對忽略與訓練家任何部位的碰撞
        if (other.gameObject.name.Contains("Trainer") || other.transform.root.name.Contains("Trainer"))
        {
            return;
        }

        if (hasCollided) return;
        hasCollided = true;

        // 1. 停用物理計算，固定在撞擊點（同簡報 6-4-6）
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 2. 播放碰撞粒子特效（同簡報 6-5-2）
        if (hitParticles != null)
        {
            hitParticles.Play();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit();
        }

        // 3. 檢查是否撞擊到寶可夢
        PokemonTarget pokemon = other.gameObject.GetComponentInParent<PokemonTarget>();
        if (pokemon != null)
        {
            // 觸發收服流程
            StartCoroutine(CaptureRoutine(pokemon));
        }
        else
        {
            // 若撞到地面或其他物件，3秒後自動清理
            Destroy(gameObject, 3.5f);
        }
    }

    private IEnumerator CaptureRoutine(PokemonTarget pokemon)
    {
        if (GameDirector.Instance != null)
        {
            GameDirector.Instance.SetStatus("命中寶可夢！收服光線啟動中...");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCapture();
        }

        // 寶可夢縮小被吸入球中
        pokemon.OnCaptureHit(transform.position);
        yield return new WaitForSeconds(0.9f);

        // 精靈球稍微落到地面草皮上準備搖晃判定 (球半徑約 0.1m)
        Vector3 groundPos = new Vector3(transform.position.x, 0.12f, transform.position.z);
        float dropTime = 0.3f;
        float elapsed = 0f;
        Vector3 startDrop = transform.position;
        while (elapsed < dropTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startDrop, groundPos, elapsed / dropTime);
            yield return null;
        }

        // 經典精靈球三次搖晃動畫 (Wobble)
        if (GameDirector.Instance != null)
        {
            GameDirector.Instance.SetStatus("精靈球晃動中... (1/3)");
        }
        for (int i = 1; i <= 3; i++)
        {
            if (GameDirector.Instance != null)
            {
                GameDirector.Instance.SetStatus($"精靈球晃動中... ({i}/3)");
            }
            yield return StartCoroutine(WobbleBallRoutine());
            yield return new WaitForSeconds(0.35f);
        }

        // 收服成功！
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySuccess();
        }

        if (successStarsParticle != null)
        {
            successStarsParticle.Play();
        }

        if (GameDirector.Instance != null)
        {
            GameDirector.Instance.SetStatus("★ 太棒了！成功收服寶可夢！ ★", true);
        }
    }

    private IEnumerator WobbleBallRoutine()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Quaternion originalRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin((elapsed / duration) * Mathf.PI * 2) * 22f;
            transform.rotation = originalRot * Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        transform.rotation = originalRot;
    }
}
