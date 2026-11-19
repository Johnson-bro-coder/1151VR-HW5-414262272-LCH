using System;
using System.Collections;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [Header("Joint Transforms")]
    public Transform bodyTransform;
    public Transform rightShoulderPivot;
    public Transform handBall; // 手中持有的展示用小精靈球

    [Header("Animation States")]
    private bool isThrowing = false;

    // 預設關節旋轉角度
    private Quaternion initialBodyRot;
    private Quaternion initialArmRot;
    private Vector3 initialBodyPos;

    private void Start()
    {
        if (bodyTransform != null)
        {
            initialBodyRot = bodyTransform.localRotation;
            initialBodyPos = bodyTransform.localPosition;
        }
        if (rightShoulderPivot != null)
        {
            initialArmRot = rightShoulderPivot.localRotation;
        }
    }

    private void Update()
    {
        if (!isThrowing)
        {
            // 待機自然呼吸微動 (Idle Breathing Animation)
            float breath = Mathf.Sin(Time.time * 2.5f) * 0.015f;
            if (bodyTransform != null)
            {
                bodyTransform.localPosition = initialBodyPos + new Vector3(0, breath, 0);
            }
            if (rightShoulderPivot != null)
            {
                float armSway = Mathf.Sin(Time.time * 2.0f) * 4.0f;
                rightShoulderPivot.localRotation = initialArmRot * Quaternion.Euler(armSway, 0, 0);
            }
        }
    }

    /// <summary>
    /// 執行完整棒球/投球拋擲動作 (蓄力後拉 -> 大力揮臂 -> 出手拋球 -> 隨動回正)
    /// </summary>
    public void PlayThrowAnimation(Action onRelease)
    {
        if (isThrowing) return;
        StartCoroutine(ThrowActionRoutine(onRelease));
    }

    private IEnumerator ThrowActionRoutine(Action onRelease)
    {
        isThrowing = true;
        if (handBall != null) handBall.gameObject.SetActive(true);

        // -------------------------------------------------------------
        // Phase 1: 蓄力後拉 (Wind-up), 歷時 0.35 秒
        // -------------------------------------------------------------
        float windUpDuration = 0.35f;
        float elapsed = 0f;

        Quaternion armWindUpRot = Quaternion.Euler(-65f, -30f, 25f);
        Quaternion bodyWindUpRot = initialBodyRot * Quaternion.Euler(0, -30f, -5f);

        while (elapsed < windUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / windUpDuration);

            if (rightShoulderPivot != null)
                rightShoulderPivot.localRotation = Quaternion.Slerp(initialArmRot, armWindUpRot, t);
            if (bodyTransform != null)
                bodyTransform.localRotation = Quaternion.Slerp(initialBodyRot, bodyWindUpRot, t);

            yield return null;
        }

        // 短暫凝力定格 (0.05 秒)
        yield return new WaitForSeconds(0.05f);

        // -------------------------------------------------------------
        // Phase 2: 大力揮臂向前拋出 (Pitch & Release), 歷時 0.18 秒
        // -------------------------------------------------------------
        float pitchDuration = 0.18f;
        elapsed = 0f;

        Quaternion armPitchRot = Quaternion.Euler(80f, 40f, -15f);
        Quaternion bodyPitchRot = initialBodyRot * Quaternion.Euler(6f, 35f, 0f);

        bool released = false;

        while (elapsed < pitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pitchDuration; // 線性急遽發力加速

            if (rightShoulderPivot != null)
                rightShoulderPivot.localRotation = Quaternion.Slerp(armWindUpRot, armPitchRot, t);
            if (bodyTransform != null)
                bodyTransform.localRotation = Quaternion.Slerp(bodyWindUpRot, bodyPitchRot, t);

            // 在揮臂至 70% 進度 (手臂向前伸展最高點) 出手拋球
            if (!released && t >= 0.70f)
            {
                released = true;
                if (handBall != null) handBall.gameObject.SetActive(false);
                onRelease?.Invoke(); // 觸發實體精靈球由左至右拋出
            }

            yield return null;
        }

        if (!released)
        {
            if (handBall != null) handBall.gameObject.SetActive(false);
            onRelease?.Invoke();
        }

        // -------------------------------------------------------------
        // Phase 3: 隨動收勢與平滑回正 (Follow-through & Recovery), 歷時 0.40 秒
        // -------------------------------------------------------------
        float recoveryDuration = 0.40f;
        elapsed = 0f;

        Quaternion finalArmPitch = rightShoulderPivot != null ? rightShoulderPivot.localRotation : armPitchRot;
        Quaternion finalBodyPitch = bodyTransform != null ? bodyTransform.localRotation : bodyPitchRot;

        while (elapsed < recoveryDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / recoveryDuration);

            if (rightShoulderPivot != null)
                rightShoulderPivot.localRotation = Quaternion.Slerp(finalArmPitch, initialArmRot, t);
            if (bodyTransform != null)
                bodyTransform.localRotation = Quaternion.Slerp(finalBodyPitch, initialBodyRot, t);

            yield return null;
        }

        if (rightShoulderPivot != null) rightShoulderPivot.localRotation = initialArmRot;
        if (bodyTransform != null) bodyTransform.localRotation = initialBodyRot;

        // 動作結束後 1 秒，手上重新凝聚待投精靈球
        yield return new WaitForSeconds(1.0f);
        if (handBall != null) handBall.gameObject.SetActive(true);

        isThrowing = false;
    }

    public void ResetTrainer()
    {
        StopAllCoroutines();
        isThrowing = false;
        if (bodyTransform != null)
        {
            bodyTransform.localRotation = initialBodyRot;
            bodyTransform.localPosition = initialBodyPos;
        }
        if (rightShoulderPivot != null)
        {
            rightShoulderPivot.localRotation = initialArmRot;
        }
        if (handBall != null)
        {
            handBall.gameObject.SetActive(true);
        }
    }
}
