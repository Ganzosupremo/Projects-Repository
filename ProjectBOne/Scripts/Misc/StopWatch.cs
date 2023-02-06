using System;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class StopWatch
{
    private bool isRunning = false;
    private float currentTime;

    private TimeSpan time;

    public StopWatch()
    {
        currentTime = 0;
    }

    public void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            UpdateTimer();
        }
    }

    public void UpdateTimer()
    {
        if (!isRunning) return;
            
        currentTime += Time.deltaTime;
        time = TimeSpan.FromSeconds(currentTime);
    }

    public void StopTimer()
    {
        if (isRunning)
        {
            isRunning = false;
        }
    }

    public TimeSpan GetCurrentTime()
    {
        return time;
    }
}
