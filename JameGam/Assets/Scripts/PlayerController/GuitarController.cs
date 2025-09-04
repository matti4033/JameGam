using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class GuitarController : MonoBehaviour
{
    // Events
    public static Action<NoteEvent> Strummed;
    
    // Bindings & settings
    [Header("Bindings (1–6 = strings)")]
    public List<NoteBinding> notes = new List<NoteBinding>(); // 6 items: E2..E4 mapped to Alpha1..Alpha6

    [Header("Input")] public KeyCode strumKey = KeyCode.Space; // hold notes, press Space to strum

    [Header("Volumes")] [Range(0.05f, 1f)] public float baseVolume = 0.7f;

    [Tooltip("Extra loudness for bigger chords (per extra note).")] [Range(0f, 0.5f)]
    public float chordLoudnessBoostPerNote = 0.18f;

    [Header("QoL")]
    [Tooltip("If true, hitting a note key while holding Space will include it in the current strum right away.")]
    public bool allowPressWhileStrumming = true;

    AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;

        // Auto-fill defaults (top number row 1–6)
        if (notes == null || notes.Count == 0)
        {
            notes = new List<NoteBinding>
            {
                new NoteBinding { noteName = "E2", key = KeyCode.Alpha1, frequencyHz = 82.41f },
                new NoteBinding { noteName = "A2", key = KeyCode.Alpha2, frequencyHz = 110.00f },
                new NoteBinding { noteName = "D3", key = KeyCode.Alpha3, frequencyHz = 146.83f },
                new NoteBinding { noteName = "G3", key = KeyCode.Alpha4, frequencyHz = 196.00f },
                new NoteBinding { noteName = "B3", key = KeyCode.Alpha5, frequencyHz = 246.94f },
                new NoteBinding { noteName = "E4", key = KeyCode.Alpha6, frequencyHz = 329.63f },
            };
        }
    }

    void Update()
    {
        // Single-note pluck on key down (when Space is NOT held)
        foreach (var n in notes)
        {
            if (Input.GetKeyDown(n.key) && !Input.GetKey(strumKey))
            {
                PlayAndBroadcast(new[] { n });
                return;
            }
        }

        // Chord strum when Space is pressed
        if (Input.GetKeyDown(strumKey))
        {
            var held = notes.Where(n => Input.GetKey(n.key)).ToArray();
            if (held.Length > 0) PlayAndBroadcast(held);
        }

        // Keep strumming if new keys are pressed while holding Space (optional)
        if (allowPressWhileStrumming && Input.GetKey(strumKey))
        {
            if (notes.Any(n => Input.GetKeyDown(n.key)))
            {
                var held = notes.Where(n => Input.GetKey(n.key)).ToArray();
                if (held.Length > 0) PlayAndBroadcast(held);
            }
        }
    }

    void PlayAndBroadcast(NoteBinding[] bindings)
    {
        int count = Mathf.Max(1, bindings.Length);
        float amp = Mathf.Clamp01(baseVolume * (1f + chordLoudnessBoostPerNote * (count - 1)));

        // Play all clips
        foreach (var b in bindings)
        {
            if (b.clip != null)
                _src.PlayOneShot(b.clip, amp / count);
        }

        // Fire a NoteEvent
        var evt = new NoteEvent
        {
            origin = transform.position,
            amplitude = amp,
            time = Time.time,
            frequencies = bindings.Select(b => b.frequencyHz).ToArray(),
            noteNames = bindings.Select(b => string.IsNullOrEmpty(b.noteName) ? $"Hz{b.frequencyHz:F0}" : b.noteName)
                .ToArray()
        };

        Strummed?.Invoke(evt);
    }
}

#region Data types

[Serializable]
public struct NoteBinding
{
    public string noteName; // e.g., "E2"
    public KeyCode key; // KeyCode.Alpha1..Alpha6 etc.
    public float frequencyHz; // e.g., 82.41
    public AudioClip clip; // one-shot sample
}

[Serializable]
public struct NoteEvent
{
    public Vector2 origin; // Where the strum happened (player pos)
    public float amplitude; // 0..1 loudness
    public float time; // Time.time when fired
    public float[] frequencies; // Hz (one or more if chord)
    public string[] noteNames; // Same order as frequencies
}

// Environment objects can implement this to react to NoteEvent
public interface ISoundReactive
{
    void React(NoteEvent note);
}

#endregion