using System.Collections;
using TMPro;
using UnityEngine;

public class RecordTimer : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;

    [Header("Settings")]
    public float countdownDuration = 3f;
    public bool startOnAwake = true;

    // متغیرهای داخلی
    private float currentTime = 0f; // زمان جاری بازی (بر حسب ثانیه)
    private bool isTimerRunning = false; // وضعیت تایمر
    private bool isCountdownComplete = false;

    void Start()
    {
        if (startOnAwake)
        {
            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        if (isCountdownComplete)
        {
            ResetTimer();
        }
        StartCoroutine(CountdownRoutine());
    }

    // کوروتین شمارش معکوس
    IEnumerator CountdownRoutine()
    {
        isTimerRunning = false;
        float remaining = countdownDuration;

        while (remaining > 0)
        {
            // نمایش عدد صحیح (۳، ۲، ۱)
            timerText.text = Mathf.CeilToInt(remaining).ToString();
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        // اتمام شمارش معکوس
        timerText.text = "GO!";
        isCountdownComplete = true;

        // شروع تایمر
        currentTime = 0f;
        isTimerRunning = true;

        yield return new WaitForSeconds(0.2f);
        while (isTimerRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
            yield return null; // هر فریم بروزرسانی
        }
    }

    // بروزرسانی نمایش تایمر (فرمت دقیقه:ثانیه.صدم)
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // نمایش به صورت mm:ss.ff (مثلاً 01:23.45)
            string minutes = Mathf.Floor(currentTime / 60).ToString("00");
            string seconds = Mathf.Floor(currentTime % 60).ToString("00");
            string fraction = Mathf.Floor((currentTime % 1) * 100).ToString("00");
            timerText.text = string.Format("{0}:{1}.{2}", minutes, seconds, fraction);
        }
    }

    // متوقف کردن تایمر (مثلاً وقتی بازی تمام می‌شود)
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // دریافت زمان جاری به ثانیه (برای ذخیره در دیتابیس یا نمایش)
    public float GetCurrentTime()
    {
        return currentTime;
    }

    // ریست کامل تایمر و شمارش معکوس
    public void ResetTimer()
    {
        StopTimer();
        isCountdownComplete = false;
        currentTime = 0f;
        timerText.text = countdownDuration.ToString(); // نمایش عدد شروع
    }

    public void OnStartButtonPressed()
    {
        StartCountdown();
    }
}
