using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class UnityMainThreadDispatcher : MonoBehaviour
{
    static readonly Queue<Action> queue = new Queue<Action>();
    static UnityMainThreadDispatcher instance;

    void Awake()
    {
        if (instance == null) { instance = this; /*DontDestroyOnLoad(gameObject);*/ }
        //else if (instance != this) Destroy(gameObject);
    }

    void Update()
    {
        // Drain queue once per frame
        if (queue.Count == 0) return;
        lock (queue)
        {
            while (queue.Count > 0)
            {
                try { queue.Dequeue().Invoke(); } catch (Exception ex) { Debug.LogException(ex); }
            }
        }
    }

    public static void Enqueue(Action a)
    {
        if (a == null) return;
        lock (queue) queue.Enqueue(a);
    }
}

