using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : MonoBehaviour {

    public AudioSource musicSource;
    private float dspSongTime;

    public MidiFileLoader midiFileLoader;
    private List<MPTKEvent> midiEvents;
    private int midiEventIndex = -1;

    public Rigidbody level;
    private Quaternion startRotation = Quaternion.identity;
    private Vector3 additionalRotationEuler;

    public AnimationCurve curve;

    const float minTimeBetweenRotations = 0.125f;

    void Start() {
        dspSongTime = (float)AudioSettings.dspTime;
        musicSource?.Play();
        Debug.Log("Loading " + midiFileLoader.MPTK_MidiName + " (index " + midiFileLoader.MPTK_MidiIndex + ")...");
        midiFileLoader.MPTK_Load();
        Debug.Log("Reading events...");
        midiEvents = midiFileLoader.MPTK_ReadMidiEvents();
    }

    private void PrintEvents() {
        foreach (MPTKEvent midiEvent in midiEvents) {
            Debug.Log("{channel: " + midiEvent.Channel +
                ", note: " + midiEvent.Value +
                ", velocity: " + midiEvent.Velocity +
                ", duration: " + midiEvent.Duration + " ms" +
                ", tick: " + midiEvent.Tick +
                ", command: " + midiEvent.Command + "}");
        }
    }

    void Update() {

        float minTimeBetweenRotations = curve.keys[curve.keys.Length - 1].time;

        float songPositionInSeconds = (float)(AudioSettings.dspTime - dspSongTime);
        float ticksPerSecond = midiFileLoader.MPTK_DeltaTicksPerQuarterNote / (midiFileLoader.MPTK_MicrosecondsPerQuarterNote / 1000000f);
        float songPositionInTicks = songPositionInSeconds * ticksPerSecond;

        for (int i = midiEventIndex + 1; i < midiEvents.Count; i++) {

            MPTKEvent midiEvent = midiEvents[i];

            if (midiEvent.Tick > songPositionInTicks) {

                break;

            } else if (midiEvent.Command == MPTKCommand.NoteOn && (midiEventIndex < 0 || midiEvent.Tick > midiEvents[midiEventIndex].Tick + ticksPerSecond * minTimeBetweenRotations)) {

                midiEventIndex = i;
                startRotation = Quaternion.Euler(additionalRotationEuler) * startRotation;

                switch (midiEvent.Channel % 6) {
                    case 0:
                        additionalRotationEuler = new Vector3(1, 0, 0);
                        break;
                    case 1:
                        additionalRotationEuler = new Vector3(-1, 0, 0);
                        break;
                    case 2:
                        additionalRotationEuler = new Vector3(0, 1, 0);
                        break;
                    case 3:
                        additionalRotationEuler = new Vector3(0, -1, 0);
                        break;
                    case 4:
                        additionalRotationEuler = new Vector3(0, 0, 1);
                        break;
                    case 5:
                        additionalRotationEuler = new Vector3(0, 0, -1);
                        break;
                }
            }
        }

        if (midiEventIndex > 0) {
            float timeSinceEventInTicks = songPositionInTicks - midiEvents[midiEventIndex].Tick;
            float timeSinceEventInSeconds = timeSinceEventInTicks / ticksPerSecond;
            level.MoveRotation(Quaternion.Euler(additionalRotationEuler * curve.Evaluate(timeSinceEventInSeconds)) * startRotation);
        }
    }
}