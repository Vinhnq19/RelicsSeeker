using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    RestartLevel,
    LevelCompleted,
    UndoAction,
    PlayerDied,
    GamePaused,
    GameResumed,
    PauseToggle,
    ItemCollected,
    PlayerMove,
    PlayerPush
}

public static class EventBus
{
    // Lưu trữ các sự kiện và các listener tương ứng
    private static Dictionary<GameEvent, Action> eventTable = new();
    private static Dictionary<GameEvent, Action<PushEventData>> pushEventTable = new();
    public static void Subscribe(GameEvent eventType, Action listener)
    {
        //Kiểm tra nếu sự kiện chưa có listener nào thì thêm mới, nếu đã có thì cộng dồn listener
        if (!eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = listener;
        }
        else
        {
            eventTable[eventType] += listener;
        }
    }
    public static void Unsubcribe(GameEvent eventType, Action listener)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] -= listener;
        }
    }
    // Phát sự kiện đến tất cả các listener đã đăng ký
    public static void Publish(GameEvent eventType)
    {
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType]?.Invoke();
        }
    }

    // Subscribe cho push events
    public static void SubscribePush(GameEvent eventType, Action<PushEventData> listener)
    {
        if (!pushEventTable.ContainsKey(eventType))
            pushEventTable[eventType] = listener;
        else
            pushEventTable[eventType] += listener;
    }

    // Unsubscribe push events  
    public static void UnsubscribePush(GameEvent eventType, Action<PushEventData> listener)
    {
        if (pushEventTable.ContainsKey(eventType))
            pushEventTable[eventType] -= listener;
    }

    // Publish push event với data
    public static void PublishPush(GameEvent eventType, PushEventData data)
    {
        if (pushEventTable.ContainsKey(eventType))
            pushEventTable[eventType]?.Invoke(data);
    }
}
