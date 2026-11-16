using System.Collections;
using UnityEngine;

public class PokemonTarget : MonoBehaviour
{
    [Header("Idle Floating Animation")]
    public float floatAmplitude = 0.08f;
    public float floatSpeed = 2.5f;

    [Header("Capture Animation Settings")]
    public float absorbDuration = 0.8f;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private bool isCaptured = false;
    private Collider col;

    private void Start()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!isCaptured)
        {
            // Idle 浮動與呼吸
            float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
        }
    }

    public void OnCaptureHit(Vector3 pokeballPosition)
    {
        if (isCaptured) return;
        isCaptured = true;

        if (col != null) col.enabled = false;
        StartCoroutine(AbsorbIntoBallRoutine(pokeballPosition));
    }

    private IEnumerator AbsorbIntoBallRoutine(Vector3 ballPos)
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        // 旋轉並縮小吸收進精靈球內部
        while (elapsed < absorbDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / absorbDuration);

            // 平滑插值移動到球心
            transform.position = Vector3.Lerp(startPos, ballPos, t);
            // 縮小體積
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t * t);
            // 旋轉吸入
            transform.Rotate(Vector3.up, 720f * Time.deltaTime);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void ResetPokemon()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        transform.position = initialPosition;
        transform.localScale = initialScale;
        transform.rotation = Quaternion.identity;
        isCaptured = false;
        if (col != null) col.enabled = true;
    }
}
