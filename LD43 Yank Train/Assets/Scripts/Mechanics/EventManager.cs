using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private static EventManager _eventManager;
    private Dictionary<string, UnityEvent> _eventDictionary;

    /// <summary>
    /// Returns an instance of the EventManager.
    /// </summary>
    public static EventManager Instance
    {
        get
        {
            if(!_eventManager)
            {
                _eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if(!_eventManager)
                {
                    //Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    _eventManager.Init();
                }
            }

            return _eventManager;
        }
    }

    /// <summary>
    /// Initialize the Dictionary.
    /// </summary>
    private void Init()
    {
        if(_eventDictionary == null)
        {
            _eventDictionary = new Dictionary<string, UnityEvent>();
        }
    }

    /// <summary>
    /// Start listening to an event. If it's a new definition, add to dictionary.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void StartListening(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if(Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            Instance._eventDictionary.Add(eventName, thisEvent);
        }
    }

    /// <summary>
    /// Stop listening to an event if it's being listened to.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="listener"></param>
    public static void StopListening(string eventName, UnityAction listener)
    {
        if(_eventManager == null)
        {
            return;
        }

        UnityEvent thisEvent = null;
        if(Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    /// <summary>
    /// Trigger an event if it's being listened to.
    /// </summary>
    /// <param name="eventName"></param>
    public static void TriggerEvent(string eventName)
    {
        if(_eventManager == null)
        {
            return;
        }

        UnityEvent thisEvent = null;
        if(Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}