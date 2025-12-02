using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private static bool isInitialized = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Invoke(nameof(InitializeFirebase), 1);
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
               
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                isInitialized = true;
                Debug.Log("Firebase initialized successfully.");
                FirebaseManager.LogEvent("game_started");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }


    /// <summary>
    /// Logs a custom Firebase Analytics event.
    /// </summary>
    /// <param name="eventName">Name of the event.</param>
    /// <param name="parameters">Optional parameters for the event.</param>
    public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Firebase not initialized yet.");
            return;
        }

        if (parameters == null)
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
        else
        {
            var firebaseParams = new Parameter[parameters.Count];
            int i = 0;
            foreach (var param in parameters)
            {
                firebaseParams[i++] = new Parameter(param.Key, param.Value.ToString());
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        }

        Debug.Log($"Logged event: {eventName}");
    }
}
