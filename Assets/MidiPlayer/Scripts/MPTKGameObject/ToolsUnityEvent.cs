using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace MidiPlayerTK
{

    [System.Serializable]
    public class EventMidiClass : UnityEvent<MPTKEvent>
    {
    }

    [System.Serializable]
    public class EventNotesMidiClass : UnityEvent<List<MPTKEvent>>
    {
    }

    [System.Serializable]
    public class EventSynthClass : UnityEvent<string>
    {
    }


    public enum EventEndMidiEnum
    {
        MidiEnd,
        ApiStop,
        Replay,
        Next,
        Previous
    }

    [System.Serializable]
    public class EventStartMidiClass : UnityEvent<string>
    {
    }


    [System.Serializable]
    public class EventEndMidiClass : UnityEvent<string, EventEndMidiEnum>
    {
    }

    static public class ToolsUnityEvent
    {
        
        static public bool HasEvent(this EventMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this UnityEvent evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }
        static public bool HasEvent(this EventNotesMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this EventStartMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this EventEndMidiClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

        static public bool HasEvent(this EventSynthClass evt)
        {
            if (evt != null && evt.GetPersistentEventCount() > 0 && !string.IsNullOrEmpty(evt.GetPersistentMethodName(0)))
                return true;
            else
                return false;
        }

    }
}
