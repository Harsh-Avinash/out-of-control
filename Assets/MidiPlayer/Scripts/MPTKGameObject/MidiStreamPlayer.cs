//#define DEBUGPERF
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using MEC;
using UnityEngine.UI;

namespace MidiPlayerTK
{
    /// <summary>
    /// Play generated notes. 
    /// Any Midi file is necessary rather create music from your own algorithm with MPTK_PlayEvent().
    /// Duration can be set in the MPTKEvent, but a note can also be stopped with MPTK_StopEvent().
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [HelpURL("https://paxstellar.fr/midi-file-player-detailed-view-2-2/")]
    public partial class MidiStreamPlayer : MidiSynth
    {
        new void Awake()
        {
            base.Awake();
        }

        new void Start()
        {
            try
            {
                MPTK_InitSynth();
                base.Start();
                // Always enabled for midi stream
                MPTK_EnablePresetDrum = true;
                ThreadDestroyAllVoice();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play one midi event with a thread so the call return immediately.
        ///! @snippet MusicView.cs Example PlayNote
        /// </summary>
        public void MPTK_PlayEvent(MPTKEvent evnt)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    if (!MPTK_CorePlayer)
                        Timing.RunCoroutine(TheadPlay(evnt));
                    else
                    {
                        lock (this) //V2.83
                        {
                            QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = evnt });
                        }
                    }

                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, Midi Event cannot be processed Code:{0} Channel:{1}", evnt.Command, evnt.Channel);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play a list of midi events with a thread so the call return immediately.
        /// @snippet TestMidiStream.cs Example MPTK_PlayEvent
        /// </summary>
        public void MPTK_PlayEvent(List<MPTKEvent> events)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    if (!MPTK_CorePlayer)
                        Timing.RunCoroutine(TheadPlay(events));
                    else
                    {
                        lock (this) //V2.83
                        {
                            foreach (MPTKEvent evnt in events)
                                QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = evnt });
                        }
                    }
                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, Midi Events cannot be processed");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private IEnumerator<float> TheadPlay(MPTKEvent evnt)
        {
            if (evnt != null)
            {
                try
                {
                    //TBR if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                    {
#if DEBUGPERF
                        DebugPerf("-----> Init perf:", 0);
#endif
                        PlayEvent(evnt);
#if DEBUGPERF
                        DebugPerf("<---- ClosePerf perf:", 2);
#endif
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;
        }

        private IEnumerator<float> TheadPlay(List<MPTKEvent> events)
        {
            if (events != null && events.Count > 0)
            {
                try
                {
                    try
                    {
                        //TBR if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                        {
                            PlayEvents(events, true);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;

        }

        /// <summary>
        /// Stop playing the note. All waves associated to the note are stop by sending a noteoff.
        /// </summary>
        /// <param name="pnote"></param>
        public void MPTK_StopEvent(MPTKEvent pnote)
        {
            if (!MPTK_CorePlayer)
                StopEvent(pnote);
            else
            {
                lock (this) //V2.83
                {
                    QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StopEvent, MidiEvent = pnote });
                }
            }

            //try
            //{
            //    if (pnote != null && pnote.Voices != null)
            //    {
            //        foreach (fluid_voice voice in pnote.Voices)
            //            if (voice.volenv_section != fluid_voice_envelope_index.FLUID_VOICE_ENVRELEASE &&
            //                voice.status != fluid_voice_status.FLUID_VOICE_OFF)
            //                voice.fluid_voice_noteoff();
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    MidiPlayerGlobal.ErrorDetail(ex);
            //}
        }
    }
}

