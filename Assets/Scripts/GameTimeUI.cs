using UnityEngine;
using TMPro;
using System.Collections;

public class GameTimeUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI timeDisplayText;
    
    [Header("Time Settings")]
    public int currentDay = 1;
    public int maxDay = 5;
    public int currentHour = 8;
    public int currentMinute = 0;
    public bool use24HourFormat = true;
    
    [Header("Day Progression")]
    [System.Obsolete("Auto progression removed - bed controls day advancement")]
    public float dayDuration = 15f; // No longer used
    [System.Obsolete("Auto progression removed - bed controls day advancement")]
    public bool autoProgressDays = false; // No longer used
    
    [Header("Display Settings")]
    public string dayPrefix = "Day: ";
    public string timePrefix = "Time: ";
    public string timeSeparator = " | ";
    
    void Start()
    {
        // Validate component
        if (timeDisplayText == null)
        {
            Debug.LogError("GameTimeUI: Please assign a TextMeshProUGUI component in the inspector!");
            return;
        }
        
        // Initial display update
        UpdateTimeDisplay();
        
        // Auto progression removed - only bed controls day advancement
        Debug.Log("Day progression is now controlled by bed interaction only");
    }
    
    void UpdateTimeDisplay()
    {
        if (timeDisplayText == null) return;
        
        string timeString = FormatTime(currentHour, currentMinute);
        string displayText = $"{dayPrefix}{currentDay}{timeSeparator}{timePrefix}{timeString}";
        
        timeDisplayText.text = displayText;
    }
    
    string FormatTime(int hour, int minute)
    {
        if (use24HourFormat)
        {
            return $"{hour:D2}:{minute:D2}";
        }
        else
        {
            // 12-hour format with AM/PM
            string period = hour >= 12 ? "PM" : "AM";
            int displayHour = hour == 0 ? 12 : (hour > 12 ? hour - 12 : hour);
            return $"{displayHour}:{minute:D2} {period}";
        }
    }
    
    // Public methods for external time management
    public void SetDay(int day)
    {
        currentDay = day;
        UpdateTimeDisplay();
    }
    
    public void SetTime(int hour, int minute)
    {
        currentHour = Mathf.Clamp(hour, 0, 23);
        currentMinute = Mathf.Clamp(minute, 0, 59);
        UpdateTimeDisplay();
    }
    
    public void SetDateTime(int day, int hour, int minute)
    {
        currentDay = day;
        SetTime(hour, minute);
    }
    
    // Methods for advancing time (to be called by time management system)
    public void AdvanceMinute()
    {
        currentMinute++;
        if (currentMinute >= 60)
        {
            currentMinute = 0;
            AdvanceHour();
        }
        UpdateTimeDisplay();
    }
    
    public void AdvanceHour()
    {
        currentHour++;
        if (currentHour >= 24)
        {
            currentHour = 0;
            AdvanceDay();
        }
        UpdateTimeDisplay();
    }
    
    public void AdvanceDay()
    {
        if (currentDay < maxDay)
        {
            currentDay++;
            UpdateTimeDisplay();
            
            // Notify all PatientNPC objects about day change
            NotifyPatientsOfDayChange();
            
            Debug.Log($"Day advanced to: {currentDay}");
        }
        else
        {
            Debug.Log($"Maximum day ({maxDay}) reached!");
        }
    }
    
    [System.Obsolete("Auto progression removed - bed controls day advancement")]
    IEnumerator DayProgressionCoroutine()
    {
        // This method is no longer used
        yield break;
    }
    
    void NotifyPatientsOfDayChange()
    {
        // Find all PatientNPC objects and update their speed
        PatientNPC[] patients = FindObjectsByType<PatientNPC>(FindObjectsSortMode.None);
        foreach (PatientNPC patient in patients)
        {
            patient.OnDayChanged(currentDay);
        }
    }
    
    // Utility methods
    public string GetCurrentTimeString()
    {
        return FormatTime(currentHour, currentMinute);
    }
    
    public string GetCurrentDayString()
    {
        return currentDay.ToString();
    }
    
    public float GetTimeAsFloat()
    {
        return currentHour + (currentMinute / 60f);
    }
    
    // For testing in inspector
    [ContextMenu("Test Update Display")]
    void TestUpdateDisplay()
    {
        UpdateTimeDisplay();
    }
}
