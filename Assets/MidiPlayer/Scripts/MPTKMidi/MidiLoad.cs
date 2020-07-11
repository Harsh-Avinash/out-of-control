#define DEBUG_LOGEVENT 
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using MPTK.NAudio.Midi;
using System;
using System.IO;
using System.Linq;

namespace MidiPlayerTK
{
    /// <summary>
    /// Internal class for loading a Midi file. 
    /// No sequencer, no synthetizer, no music playing capabilities. 
    /// Usefull to load all the Midi events from a Midi and process, transform, write them to want you want. 
    public class MidiLoad
    {
        //! @cond NODOC
        public MidiFile midifile;
        public bool EndMidiEvent;
        //public double QuarterPerMinuteValue;
        public string SequenceTrackName = "";
        public string ProgramName = "";
        public string TrackInstrumentName = "";
        public string TextEvent = "";
        public string Copyright = "";

        /// <summary>
        /// milliseconds per midi tick. depends on set-tempo
        /// </summary>
        public double MPTK_PulseLenght;

        //! @endcond

        public List<TrackMidiEvent> MPTK_MidiEvents;

        /// <summary>
        /// Initial tempo found in the Midi
        /// </summary>
        public double MPTK_InitialTempo;

        /// <summary>
        /// Initial tempo found in the Midi
        /// </summary>
        public double MPTK_CurrentTempo { get { return fluid_player_get_bpm(); } }

        /// <summary>
        /// Duration (TimeSpan) of the midi.
        /// </summary>
        public TimeSpan MPTK_Duration;

        /// <summary>
        /// Duration (milliseconds) of the midi.
        /// </summary>
        public float MPTK_DurationMS;

        private long timeLastSegment;

        /// <summary>
        /// Last tick position in Midi: Time of the last midi event in sequence expressed in number of "ticks". MPTK_TickLast / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickLast;

        /// <summary>
        /// Current tick position in Midi: Time of the current midi event expressed in number of "ticks". MPTK_TickCurrent / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickCurrent;

        /// <summary>
        /// Tick for the first note found
        /// </summary>
        public long MPTK_TickFirstNote;

        /// <summary>
        /// From TimeSignature event: The numerator counts the number of beats in a measure. For example a numerator of 4 means that each bar contains four beats. This is important to know because usually the first beat of each bar has extra emphasis.
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_NumberBeatsMeasure;

        /// <summary>
        /// From TimeSignature event: number of quarter notes in a beat. Equal 2 Power TimeSigDenominator.
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_NumberQuarterBeat;

        /// <summary>
        /// From TimeSignature event: The numerator counts the number of beats in a measure. For example a numerator of 4 means that each bar contains four beats. This is important to know because usually the first beat of each bar has extra emphasis. In MIDI the denominator value is stored in a special format. i.e. the real denominator = 2^[dd]
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_TimeSigNumerator;

        /// <summary>
        /// From TimeSignature event: The denominator specifies the number of quarter notes in a beat. 2 represents a quarter-note, 3 represents an eighth-note, etc. . 
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_TimeSigDenominator;

        /// <summary>
        /// From TimeSignature event: The standard MIDI clock ticks every 24 times every quarter note (crotchet) so a [cc] value of 24 would mean that the metronome clicks once every quarter note. A [cc] value of 6 would mean that the metronome clicks once every 1/8th of a note (quaver).
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_TicksInMetronomeClick;

        /// <summary>
        /// From TimeSignature event: This value specifies the number of 1/32nds of a note happen every MIDI quarter note. It is usually 8 which means that a quarter note happens every quarter note.
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_No32ndNotesInQuarterNote;

        /// <summary>
        /// Read from the SetTempo event: The tempo is given in micro seconds per quarter beat. 
        /// To convert this to BPM we needs to use the following equation:BPM = 60,000,000/[tt tt tt]
        /// Warning: this value can change during the playing when a change tempo event is find. 
        /// http://www.deluge.co/?q=midi-tempo-bpm
        /// </summary>
        public int MPTK_MicrosecondsPerQuarterNote;

        /// <summary>
        /// Read from Midi Header: Delta Ticks Per Quarter Note. 
        /// Represent the duration time in "ticks" which make up a quarter-note. 
        /// For instance, if 96, then a duration of an eighth-note in the file would be 48.
        /// Also named Division.
        /// </summary>
        public int MPTK_DeltaTicksPerQuarterNote;

        /// <summary>
        /// Count of track read in the Midi file
        /// </summary>
        public int MPTK_TrackCount;

        public bool LogEvents;
        public bool KeepNoteOff;
        public bool EnableChangeTempo;
        public bool ReadyToStarted;
        public bool ReadyToPlay;

        private long Quantization;
        //private long CurrentTick;
        private double Speed = 1d;
        //private double LastTimeFromStartMS;

        private int seek_ticks;           /* new position in tempo ticks (midi ticks) for seeking */
        private int next_event;
        private int start_ticks;          /* the number of tempo ticks passed at the last tempo change */
        private int cur_ticks;            /* the number of tempo ticks passed */
        //private int begin_msec;           /* the time (msec) of the beginning of the file */
        private int start_msec;           /* the start time of the last tempo change */
        private int cur_msec;             /* the current time */
        private int miditempo;            /* as indicated by MIDI SetTempo: n 24th of a usec per midi-clock. bravo! */
        //private double deltatime;         /* milliseconds per midi tick. depends on set-tempo */

        // <summary>
        /// Last position played by tracks
        /// </summary>
        //private int NextPosEvent;
        private static string[] NoteNames = new string[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public MidiLoad()
        {
            //ReadyToStarted = false;
            ReadyToPlay = false;
        }

        private void Init()
        {
            MPTK_PulseLenght = 0;
            MPTK_MidiEvents = null;
            MPTK_InitialTempo = -1;
            MPTK_Duration = TimeSpan.Zero;
            MPTK_DurationMS = 0f;
            timeLastSegment = 0;
            MPTK_TickLast = 0;
            MPTK_TickCurrent = 0;
            MPTK_TickFirstNote = -1;
            MPTK_NumberBeatsMeasure = 0;
            MPTK_NumberQuarterBeat = 0;
            MPTK_TimeSigNumerator = 0;
            MPTK_TimeSigDenominator = 0;
            MPTK_TicksInMetronomeClick = 0;
            MPTK_No32ndNotesInQuarterNote = 0;
            MPTK_MicrosecondsPerQuarterNote = 0;
            MPTK_DeltaTicksPerQuarterNote = 0;
            MPTK_TrackCount = 0;
        }

        /// <summary>
        /// Load Midi from midi MPTK referential (Unity resource). 
        /// The index of the Midi file can be found in the windo "Midi File Setup". Display with menu MPTK / Midi File Setup
        ///! @code
        /// public MidiLoad MidiLoaded;
        /// // .....
        /// MidiLoaded = new MidiLoad();
        /// MidiLoaded.MPTK_Load(14) // index for "Beattles - Michelle"
        /// Debug.Log("Duration:" + MidiLoaded.MPTK_Duration);
        ///! @endcode
        /// </summary>
        /// <param name="index"></param>
        /// <param name="strict">If true will error on non-paired note events, default:false</param>
        /// <returns>true if loaded</returns>
        public bool MPTK_Load(int index, bool strict = false)
        {
            Init();
            bool ok = true;
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    if (index >= 0 && index < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                    {
                        string midiname = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[index];
                        TextAsset mididata = Resources.Load<TextAsset>(Path.Combine(MidiPlayerGlobal.MidiFilesDB, midiname));
                        midifile = new MidiFile(mididata.bytes, strict);
                        if (midifile != null)
                            AnalyseMidi();
                    }
                    else
                    {
                        Debug.LogWarningFormat("MidiLoad - index {0} out of range ", index);
                        ok = false;
                    }
                }
                else
                {
                    Debug.LogWarningFormat("MidiLoad - index:{0} - {1}", index, MidiPlayerGlobal.ErrorNoMidiFile);
                    ok = false;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from a local file
        /// </summary>
        /// <param name="filename">Midi path and filename to load</param>
        /// <param name="strict">if true struct respect of the midi norm is checked</param>
        /// <returns></returns>
        public bool MPTK_LoadFile(string filename, bool strict = false)
        {
            bool ok = true;
            try
            {
                using (Stream sfFile = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] data = new byte[sfFile.Length];
                    sfFile.Read(data, 0, (int)sfFile.Length);
                    ok = MPTK_Load(data);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from an array of bytes
        /// </summary>
        /// <param name="datamidi">byte arry midi</param>
        /// <param name="strict">If true will error on non-paired note events, default:false</param>
        /// <returns>true if loaded</returns>
        public bool MPTK_Load(byte[] datamidi, bool strict = false)
        {
            Init();
            bool ok = true;
            try
            {
                midifile = new MidiFile(datamidi, strict);
                if (midifile != null)
                    AnalyseMidi();
                else
                    ok = false;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from a Midi file from Unity resources. The Midi file must be present in Unity MidiDB ressource folder.
        ///! @code
        /// public MidiLoad MidiLoaded;
        /// // .....
        /// MidiLoaded = new MidiLoad();
        /// MidiLoaded.MPTK_Load("Beattles - Michelle")
        /// Debug.Log("Duration:" + MidiLoaded.MPTK_Duration);
        ///! @endcode
        /// </summary>
        /// <param name="midiname">Midi file name without path and extension</param>
        /// <param name="strict">if true, check strict compliance with the Midi norm</param>
        /// <returns>true if loaded</returns>
        public bool MPTK_Load(string midiname, bool strict = false)
        {
            try
            {
                //TextAsset mididata = Resources.Load<TextAsset>(MidiPlayerGlobal.MidiFilesDB + "/" + midiname);
                TextAsset mididata = Resources.Load<TextAsset>(Path.Combine(MidiPlayerGlobal.MidiFilesDB, midiname));
                if (mididata != null && mididata.bytes != null && mididata.bytes.Length > 0)
                    return MPTK_Load(mididata.bytes, strict);
                else
                    Debug.LogWarningFormat("Midi {0} not loaded from folder {1}", midiname, MidiPlayerGlobal.MidiFilesDB);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return false;
        }

        /// <summary>
        /// Read the list of midi events available in the Midi from a ticks position to an end position.
        /// </summary>
        /// <param name="fromTicks">ticks start</param>
        /// <param name="toTicks">ticks end</param>
        /// <returns></returns>
        public List<MPTKEvent> MPTK_ReadMidiEvents(long fromTicks = 0, long toTicks = long.MaxValue)
        {
            List<MPTKEvent> midievents = new List<MPTKEvent>();
            try
            {
                if (midifile != null)
                {
                    foreach (TrackMidiEvent trackEvent in MPTK_MidiEvents)
                    {
                        if (Quantization != 0)
                            trackEvent.AbsoluteQuantize = ((trackEvent.Event.AbsoluteTime + Quantization / 2) / Quantization) * Quantization;
                        else
                            trackEvent.AbsoluteQuantize = trackEvent.Event.AbsoluteTime;

                        //Debug.Log("ReadMidiEvents - timeFromStartMS:" + Convert.ToInt32(timeFromStartMS) + " LastTimeFromStartMS:" + Convert.ToInt32(LastTimeFromStartMS) + " CurrentPulse:" + CurrentPulse + " AbsoluteQuantize:" + trackEvent.AbsoluteQuantize);

                        if (trackEvent.AbsoluteQuantize >= fromTicks && trackEvent.AbsoluteQuantize <= toTicks)
                        {
                            ConvertToEvent(midievents, trackEvent);
                        }
                        MPTK_TickCurrent = trackEvent.AbsoluteQuantize;
                        MPTK_TickLast = trackEvent.AbsoluteQuantize;

                        if (trackEvent.AbsoluteQuantize > toTicks)
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return midievents;
        }

        /// <summary>
        /// Convert the tick duration to a real time duration in millisecond regarding the current tempo.
        /// </summary>
        /// <param name="tick">duration in ticks</param>
        /// <returns>duration in milliseconds</returns>
        public double MPTK_ConvertTickToTime(long tick)
        {
            return tick * MPTK_PulseLenght;
        }

        /// <summary>
        /// Convert a real time duration in millisecond to a number of tick regarding the current tempo.
        /// </summary>
        /// <param name="time">duration in milliseconds</param>
        /// <returns>duration in ticks</returns>
        public long MPTK_ConvertTimeToTick(double time)
        {
            if (MPTK_PulseLenght != 0d)
                return Convert.ToInt64((time / MPTK_PulseLenght) + 0.5d);
            else
                return 0;
        }

        // No doc until end of file
        //! @cond NODOC

        /// <summary>
        /// Build OS path to the midi file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        static public string BuildOSPath(string filename)
        {
            try
            {
                string pathMidiFolder = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);
                string pathfilename = Path.Combine(pathMidiFolder, filename + MidiPlayerGlobal.ExtensionMidiFile);
                return pathfilename;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }

        private void AnalyseMidi()
        {
            try
            {
                MPTK_TickLast = 0;
                MPTK_TickCurrent = 0;
                SequenceTrackName = "";
                ProgramName = "";
                TrackInstrumentName = "";
                TextEvent = "";
                Copyright = "";

                // Get midi info sorted from midifile.Events
                MPTK_MidiEvents = GetMidiEvents();

                //DebugMidiSorted(MidiSorted);
                MPTK_DeltaTicksPerQuarterNote = midifile.DeltaTicksPerQuarterNote;

                if (MPTK_InitialTempo < 0)
                {
                    MPTK_InitialTempo = 120;
                    miditempo = (int)(MPTK_InitialTempo * 60000000d);
                }

                // Set initial tempo and duration 
                fluid_player_set_bpm((int)MPTK_InitialTempo);

                if (!EnableChangeTempo)
                {
                    // Calculate the real duration from the PulseLength set with MPTK_InitialTempo
                    MPTK_DurationMS = MPTK_TickLast * (float)MPTK_PulseLenght;
                }
                MPTK_Duration = TimeSpan.FromMilliseconds(MPTK_DurationMS);
                //Debug.Log("MPTK_InitialTempo:" + MPTK_InitialTempo);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private List<TrackMidiEvent> GetMidiEvents()
        {
            //Debug.Log("GetEvents");
            try
            {
                MPTK_TickLast = -1;
                int countTracks = 0;
                List<TrackMidiEvent> events = new List<TrackMidiEvent>();
                foreach (IList<MidiEvent> track in midifile.Events)
                {
                    countTracks++;
                    foreach (MidiEvent e in track)
                    {
                        try
                        {
                            //bool keepEvent = false;
                            if (e.AbsoluteTime > MPTK_TickLast) MPTK_TickLast = e.AbsoluteTime;

                            switch (e.CommandCode)
                            {
                                case MidiCommandCode.NoteOn:
                                    //Debug.Log("NoteOn "+ KeepNoteOff);
                                    if (e.AbsoluteTime < MPTK_TickFirstNote || MPTK_TickFirstNote == -1)
                                    {
                                        //Debug.Log("NoteOn MPTK_TickFirstNote" + e.AbsoluteTime);
                                        MPTK_TickFirstNote = e.AbsoluteTime;
                                    }
                                    //keepEvent = true;
                                    break;
                                //case MidiCommandCode.NoteOff:
                                //    //Debug.Log("NoteOff "+ KeepNoteOff);
                                //    //keepEvent = true;
                                //    break;
                                //case MidiCommandCode.ControlChange:
                                //    //ControlChangeEvent ctrl = (ControlChangeEvent)e;
                                //    //Debug.Log("NoteOff");
                                //    keepEvent = true;
                                //    break;
                                //case MidiCommandCode.PatchChange:
                                //    keepEvent = true;
                                //    break;

                                case MidiCommandCode.MetaEvent:
                                    MetaEvent meta = (MetaEvent)e;
                                    switch (meta.MetaEventType)
                                    {
                                        case MetaEventType.SetTempo:
                                            TempoEvent tempo = (TempoEvent)meta;
                                            // Calculate the real duration
                                            if (EnableChangeTempo)
                                            {
                                                MPTK_DurationMS += ((e.AbsoluteTime - timeLastSegment) * (float)MPTK_PulseLenght);
                                                timeLastSegment = e.AbsoluteTime;
                                            }
                                            fluid_player_set_midi_tempo(tempo.MicrosecondsPerQuarterNote);
                                            // Set the first tempo value find
                                            if (MPTK_InitialTempo < 0) MPTK_InitialTempo = tempo.Tempo;
                                            if (MPTK_MicrosecondsPerQuarterNote == 0) MPTK_MicrosecondsPerQuarterNote = tempo.MicrosecondsPerQuarterNote;
                                            //Debug.Log("Partial at: " + timeLastSegment + " " + MPTK_RealDuration + " " + Math.Round(TickLengthMs, 2) + " " + Math.Round(QuarterPerMinuteValue, 2));
                                            //Debug.Log("Tempo: " + ((TempoEvent)e).Tempo + " MPTK_InitialTempo:" + MPTK_InitialTempo);
                                            break;

                                        case MetaEventType.TimeSignature:
                                            AnalyzeTimeSignature(meta);
                                            break;
                                    }
                                    //keepEvent = true;
                                    break;
                            }

                            //if (keepEvent)
                            //{
                            TrackMidiEvent tmidi = new TrackMidiEvent()
                            {
                                IndexTrack = countTracks,
                                Event = e//.Clone()
                            };

                            events.Add(tmidi);
                            //}
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                            return null;
                        }
                    }
                }
                //DebugMidiSorted(events);

                MPTK_TrackCount = countTracks;
                List<TrackMidiEvent> midievents = events.OrderBy(o => o.Event.AbsoluteTime).ToList();
                if (midievents.Count > 0)
                {
                    long lastAbsoluteTime = midievents[midievents.Count - 1].Event.AbsoluteTime;
                    MPTK_TickLast = lastAbsoluteTime;
                    //Debug.Log("End at: " + lastAbsoluteTime + " " + MPTK_RealDuration + " " + Math.Round(TickLengthMs, 2) + " " + Math.Round(QuarterPerMinuteValue, 2));
                }
                else
                    MPTK_TickLast = 0;

                if (MPTK_TickLast > 0 && EnableChangeTempo)
                {
                    // Calculate the real duration, cumul all segments with tempo change
                    MPTK_DurationMS += ((MPTK_TickLast - timeLastSegment) * (float)MPTK_PulseLenght);
                }
                return midievents;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }

        /// <summary>
        /// Change speed to play. 1=normal speed
        /// </summary>
        /// <param name="speed"></param>
        public void ChangeSpeed(float speed)
        {
            try
            {
                if (speed > 0)
                {
                    //Debug.Log("ChangeSpeed " + speed);
                    double lastSpeed = Speed;
                    Speed = speed;
                    fluid_player_set_midi_tempo(miditempo);

                    MPTK_DurationMS = (float)((double)MPTK_DurationMS * lastSpeed / Speed);

                    MPTK_Duration = TimeSpan.FromMilliseconds(MPTK_DurationMS);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public void ChangeQuantization(int level = 4)
        {
            try
            {
                if (level <= 0)
                    Quantization = 0;
                else
                    Quantization = midifile.DeltaTicksPerQuarterNote / level;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public void StartMidi()
        {
            // Debug.Log("StartMidi core");
            //begin_msec = 0;
            start_msec = 0;
            start_ticks = 0;
            cur_ticks = 0;
            seek_ticks = -1;
            cur_msec = 0;
            next_event = 0;
            EndMidiEvent = false;
        }

        /**
         * Set the tempo of a MIDI player.
         * @param player MIDI player instance
         * @param tempo Tempo to set playback speed from param1
         * @return Always returns 0
         *
         */
        public void fluid_player_set_midi_tempo(int tempo)
        {
            if (midifile == null || midifile.DeltaTicksPerQuarterNote <= 0)
                Debug.LogWarning("Can't set tempo while midi is not loaded");
            else
            {
                miditempo = tempo;
                MPTK_PulseLenght = (double)tempo / midifile.DeltaTicksPerQuarterNote / 1000f / Speed; /* in milliseconds */
                start_msec = cur_msec;
                start_ticks = cur_ticks;
            }
        }

        /**
        * Seek in the currently playing file.
        * @param player MIDI player instance
        * @param ticks the position to seek to in the current file
        * @return #FLUID_FAILED if ticks is negative or after the latest tick of the file,
        *   #FLUID_OK otherwise
        * @since 2.0.0
        *
        * The actual seek is performed during the player_callback.
        */
        public void fluid_player_seek(int ticks)
        {
            //Debug.Log("fluid_player_seek:" + ticks);
            // Include tick un parameter
            if (ticks > 0)
                seek_ticks = ticks - 1;
            else
                seek_ticks = 0;
        }


        /**
      * Set the tempo of a MIDI player in beats per minute.
      * @param player MIDI player instance
      * @param bpm Tempo in beats per minute
      * @return Always returns #FLUID_OK
      */
        void fluid_player_set_bpm(int bpm)
        {
            fluid_player_set_midi_tempo(60000000 / bpm);
        }

        /**
         * Get the tempo of a MIDI player in beats per minute.
         * @param player MIDI player instance
         * @return MIDI player tempo in BPM
         * @since 1.1.7
         */

        int fluid_player_get_bpm()
        {
            return 60000000 / miditempo;
        }

        public List<MPTKEvent> fluid_player_callback(int msec)
        {
            List<MPTKEvent> midievents = null;
            try
            {
                if (midifile != null && next_event >= 0)
                {
                    cur_msec = msec;
                    cur_ticks = start_ticks + (int)(((double)(cur_msec - start_msec) / MPTK_PulseLenght) + 0.5d);

                    //Debug.Log("fluid_player_callback: cur_ticks:" + cur_ticks + " msec:" + cur_msec + " start_msec:" + start_msec + " start_ticks:" + start_ticks + " MPTK_PulseLenght:" + MPTK_PulseLenght + " seek_ticks:" + seek_ticks);

                    //if (seek_ticks >= 0)
                    //{
                    //    fluid_synth_all_sounds_off(synth, -1); /* avoid hanging notes */
                    //}

                    //----- midievents = fluid_send_events(midievents, cur_ticks);
                    int ticks = cur_ticks;

                    if (seek_ticks >= 0)
                    {
                        ticks = seek_ticks; /* update target ticks */
                        if (MPTK_TickCurrent > ticks)
                        {
                            // reset track if seeking backwards
                            next_event = 0;
                        }
                    }

                    // From the last position played
                    while (true)
                    {
                        if (next_event >= MPTK_MidiEvents.Count)
                        {
                            next_event = -1;
                            break;
                        }

                        TrackMidiEvent trackEvent = MPTK_MidiEvents[next_event];
                        trackEvent.AbsoluteQuantize = Quantization != 0 ?
                            ((trackEvent.Event.AbsoluteTime + Quantization / 2) / Quantization) * Quantization :
                            trackEvent.AbsoluteQuantize = trackEvent.Event.AbsoluteTime;

                        //Debug.Log("   fluid_track_send_events: Next:" + Next + " Ticks:" + Ticks + " ticks:" + ticks + " EventDeltaTime:" + trackEvent.Event.DeltaTime);

                        if (trackEvent.AbsoluteQuantize > ticks)
                        {
                            break;
                        }
                        //Debug.Log("   fluid_track_send_events: process this event " + trackEvent.Event.CommandCode + " at track.ticks:" + track.ticks);
                        //Debug.Log("   fluid_track_send_events: new at track.ticks:" + track.ticks);
                        if (seek_ticks >= 0 &&
                           (trackEvent.Event.CommandCode == MidiCommandCode.NoteOn ||
                            trackEvent.Event.CommandCode == MidiCommandCode.NoteOff ||
                            (trackEvent.Event.CommandCode == MidiCommandCode.MetaEvent && ((MetaEvent)trackEvent.Event).MetaEventType != MetaEventType.SetTempo)))
                        {
                            /* skip on/off messages */
                            //Debug.Log(BuildInfoTrack(trackEvent) + string.Format(" Skip {0} seek_ticks:{1}", trackEvent.Event.CommandCode, seek_ticks));
                        }
                        else
                        {
                            // send event to synth fluid_player_callback(evt);
                            if (midievents == null) midievents = new List<MPTKEvent>();
                            ConvertToEvent(midievents, trackEvent);
                            //Debug.Log(BuildInfoTrack(trackEvent) + string.Format(" Add {0} seek_ticks:{1}", trackEvent.Event.CommandCode, seek_ticks));

                            MPTK_TickCurrent = trackEvent.Event.AbsoluteTime;
                        }
                        next_event++;
                    }
                    //------ end fluid_send_events
                    if (seek_ticks >= 0)
                    {
                        //Debug.Log("fluid_player_callback Seek Ticks : cur_ticks:" + cur_ticks + " seek_ticks:" + seek_ticks);
                        start_ticks = seek_ticks;   /* tick position of last tempo value (which is now) */ /* the number of tempo ticks passed at the last tempo change */
                        cur_ticks = seek_ticks;     /* the number of tempo ticks passed */
                        //begin_msec = msec;      /* only used to calculate the duration of playing */
                        start_msec = msec;      /* should be the (synth)-time of the last tempo change */
                        seek_ticks = -1;        /* clear seek_ticks */
                    }

                    if (next_event < 0)
                    {
                        EndMidiEvent = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            return midievents;
        }

        /// <summary>
        /// Add a TrackMidiEvent to a list of MPTKEvent.
        /// </summary>
        /// <param name="mptkEvents">Must be alloc before the call</param>
        /// <param name="trackEvent"></param>
        /// <returns></returns>
        private void ConvertToEvent(List<MPTKEvent> mptkEvents, TrackMidiEvent trackEvent)
        {
            MPTKEvent midievent = null;
            switch (trackEvent.Event.CommandCode)
            {
                case MidiCommandCode.NoteOn:
                    //if (((NoteOnEvent)trackEvent.Event).OffEvent != null)
                    {

                        NoteOnEvent noteon = (NoteOnEvent)trackEvent.Event;
                        //Debug.Log(string.Format("Track:{0} NoteNumber:{1,3:000} AbsoluteTime:{2,6:000000} NoteLength:{3,6:000000} OffDeltaTime:{4,6:000000} ", track, noteon.NoteNumber, noteon.AbsoluteTime, noteon.NoteLength, noteon.OffEvent.DeltaTime));
                        if (noteon.OffEvent != null)
                        {
                            midievent = new MPTKEvent()
                            {
                                Track = trackEvent.IndexTrack,
                                Tick = trackEvent.AbsoluteQuantize,
                                Command = MPTKCommand.NoteOn,
                                Value = noteon.NoteNumber,
                                Channel = trackEvent.Event.Channel - 1,
                                Velocity = noteon.Velocity,
                                Duration = Convert.ToInt64(noteon.NoteLength * MPTK_PulseLenght),
                                Length = noteon.NoteLength,
                            };
                            mptkEvents.Add(midievent);
                            if (LogEvents && seek_ticks < 0)
                            {
                                string notename = (midievent.Channel != 9) ?
                                    String.Format("{0}{1}", NoteNames[midievent.Value % 12], midievent.Value / 12) : "Drum";
                                Debug.Log(BuildInfoTrack(trackEvent) + string.Format("NoteOn  {0,3:000}\t{1,-4}\tLenght:{2,5}\t{3}\tVeloc:{4,3}",
                                    midievent.Value, notename, noteon.NoteLength, NoteLength(midievent), noteon.Velocity));
                            }
                        }
                        else // It's a noteoff
                        {
                            if (KeepNoteOff)
                            {
                                midievent = new MPTKEvent()
                                {
                                    Track = trackEvent.IndexTrack,
                                    Tick = trackEvent.AbsoluteQuantize,
                                    Command = MPTKCommand.NoteOff,
                                    Value = noteon.NoteNumber,
                                    Channel = trackEvent.Event.Channel - 1,
                                    Velocity = noteon.Velocity,
                                    Duration = Convert.ToInt64(noteon.NoteLength * MPTK_PulseLenght),
                                    Length = noteon.NoteLength,
                                };
                                mptkEvents.Add(midievent);

                                if (LogEvents && seek_ticks < 0)
                                {
                                    string notename = (midievent.Channel != 9) ?
                                    String.Format("{0}{1}", NoteNames[midievent.Value % 12], midievent.Value / 12) : "Drum";
                                    Debug.Log(BuildInfoTrack(trackEvent) + string.Format("NoteOff {0,3:000}\t{1,-4}\tLenght:{2}", midievent.Value, notename, " Note Off"));
                                }
                            }
                        }
                    }
                    break;

                case MidiCommandCode.NoteOff:
                    if (KeepNoteOff)
                    {
                        NoteEvent noteoff = (NoteEvent)trackEvent.Event;
                        //Debug.Log(string.Format("Track:{0} NoteNumber:{1,3:000} AbsoluteTime:{2,6:000000} NoteLength:{3,6:000000} OffDeltaTime:{4,6:000000} ", track, noteon.NoteNumber, noteon.AbsoluteTime, noteon.NoteLength, noteon.OffEvent.DeltaTime));
                        midievent = new MPTKEvent()
                        {
                            Track = trackEvent.IndexTrack,
                            Tick = trackEvent.AbsoluteQuantize,
                            Command = MPTKCommand.NoteOff,
                            Value = noteoff.NoteNumber,
                            Channel = trackEvent.Event.Channel - 1,
                            Velocity = noteoff.Velocity,
                            Duration = 0,
                            Length = 0,
                        };

                        mptkEvents.Add(midievent);

                        if (LogEvents && seek_ticks < 0)
                        {
                            string notename = (midievent.Channel != 9) ?
                            String.Format("{0}{1}", NoteNames[midievent.Value % 12], midievent.Value / 12) : "Drum";
                            Debug.Log(BuildInfoTrack(trackEvent) + string.Format("NoteOff {0,3:000}\t{1,-4}\tLenght:{2}", midievent.Value, notename, " Note Off"));
                        }
                    }
                    break;

                case MidiCommandCode.PitchWheelChange:
                    PitchWheelChangeEvent pitch = (PitchWheelChangeEvent)trackEvent.Event;
                    midievent = new MPTKEvent()
                    {
                        Track = trackEvent.IndexTrack,
                        Tick = trackEvent.AbsoluteQuantize,
                        Command = MPTKCommand.PitchWheelChange,
                        Channel = trackEvent.Event.Channel - 1,
                        Value = pitch.Pitch,  // Pitch Wheel Value 0 is minimum, 0x2000 (8192) is default, 0x3FFF (16383) is maximum
                    };
                    mptkEvents.Add(midievent);
                    if (LogEvents && seek_ticks < 0)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("PitchWheelChange {0}", pitch.Pitch));
                    break;

                case MidiCommandCode.ControlChange:
                    ControlChangeEvent controlchange = (ControlChangeEvent)trackEvent.Event;
                    midievent = new MPTKEvent()
                    {
                        Track = trackEvent.IndexTrack,
                        Tick = trackEvent.AbsoluteQuantize,
                        Command = MPTKCommand.ControlChange,
                        Channel = trackEvent.Event.Channel - 1,
                        Controller = (MPTKController)controlchange.Controller,
                        Value = controlchange.ControllerValue,

                    };

                    //if ((MPTKController)controlchange.Controller != MPTKController.Sustain)
                    mptkEvents.Add(midievent);

                    // Other midi event
                    if (LogEvents && seek_ticks < 0)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Control {0} {1}", controlchange.Controller, controlchange.ControllerValue));

                    break;

                case MidiCommandCode.PatchChange:
                    PatchChangeEvent change = (PatchChangeEvent)trackEvent.Event;
                    midievent = new MPTKEvent()
                    {
                        Track = trackEvent.IndexTrack,
                        Tick = trackEvent.AbsoluteQuantize,
                        Command = MPTKCommand.PatchChange,
                        Channel = trackEvent.Event.Channel - 1,
                        Value = change.Patch,
                    };
                    mptkEvents.Add(midievent);
                    if (LogEvents && seek_ticks < 0)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Patch   {0,3:000} {1}", change.Patch, PatchChangeEvent.GetPatchName(change.Patch)));
                    break;

                case MidiCommandCode.MetaEvent:
                    MetaEvent meta = (MetaEvent)trackEvent.Event;
                    midievent = new MPTKEvent()
                    {
                        Track = trackEvent.IndexTrack,
                        Tick = trackEvent.AbsoluteQuantize,
                        Command = MPTKCommand.MetaEvent,
                        Channel = trackEvent.Event.Channel - 1,
                        Meta = (MPTKMeta)meta.MetaEventType,
                    };

                    switch (meta.MetaEventType)
                    {
                        case MetaEventType.EndTrack:
                            midievent.Info = "End Track";
                            break;

                        case MetaEventType.TimeSignature:
                            AnalyzeTimeSignature(meta, trackEvent);
                            break;

                        case MetaEventType.SetTempo:
                            if (EnableChangeTempo)
                            {
                                TempoEvent tempo = (TempoEvent)meta;
                                // Tempo change will be done in MidiFilePlayer
                                midievent.Duration = (long)tempo.Tempo;
                                MPTK_MicrosecondsPerQuarterNote = tempo.MicrosecondsPerQuarterNote;
                                fluid_player_set_midi_tempo(tempo.MicrosecondsPerQuarterNote);

                                // Force exit loop
                                if (LogEvents && seek_ticks < 0)
                                    Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Meta     {0,-15} Tempo:{1} MicrosecondsPerQuarterNote:{2}", meta.MetaEventType, tempo.Tempo, tempo.MicrosecondsPerQuarterNote));
                            }
                            break;

                        case MetaEventType.SequenceTrackName:
                            midievent.Info = ((TextEvent)meta).Text;
                            if (!string.IsNullOrEmpty(SequenceTrackName)) SequenceTrackName += "\n";
                            SequenceTrackName += string.Format("T{0,2:00} {1}", trackEvent.IndexTrack, midievent.Info);
                            break;

                        case MetaEventType.ProgramName:
                            midievent.Info = ((TextEvent)meta).Text;
                            ProgramName += midievent.Info + " ";
                            break;

                        case MetaEventType.TrackInstrumentName:
                            midievent.Info = ((TextEvent)meta).Text;
                            if (!string.IsNullOrEmpty(TrackInstrumentName)) TrackInstrumentName += "\n";
                            TrackInstrumentName += string.Format("T{0,2:00} {1}", trackEvent.IndexTrack, midievent.Info);
                            break;

                        case MetaEventType.TextEvent:
                            midievent.Info = ((TextEvent)meta).Text;
                            TextEvent += midievent.Info + " ";
                            break;

                        case MetaEventType.Copyright:
                            midievent.Info = ((TextEvent)meta).Text;
                            Copyright += midievent.Info + " ";
                            break;

                        case MetaEventType.Lyric: // lyric
                            midievent.Info = ((TextEvent)meta).Text;
                            TextEvent += midievent.Info + " ";
                            break;

                        case MetaEventType.Marker: // marker
                            midievent.Info = ((TextEvent)meta).Text;
                            TextEvent += midievent.Info + " ";
                            break;

                        case MetaEventType.CuePoint: // cue point
                        case MetaEventType.DeviceName:
                            break;
                    }

                    if (LogEvents && !string.IsNullOrEmpty(midievent.Info) && seek_ticks < 0)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Meta     {0,-15} '{1}'", midievent.Meta, midievent.Info));

                    mptkEvents.Add(midievent);
                    //Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Meta {0} {1}", meta.MetaEventType, meta.ToString()));
                    break;

                default:
                    // Other midi event
                    if (LogEvents && seek_ticks < 0)
                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Other    {0,-15} Not handle by MPTK", trackEvent.Event.CommandCode));
                    break;
            }
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public MPTKEvent.EnumLength NoteLength(MPTKEvent note)
        {
            if (midifile != null)
            {
                if (note.Length >= midifile.DeltaTicksPerQuarterNote * 4)
                    return MPTKEvent.EnumLength.Whole;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote * 2)
                    return MPTKEvent.EnumLength.Half;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote)
                    return MPTKEvent.EnumLength.Quarter;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote / 2)
                    return MPTKEvent.EnumLength.Eighth;
            }
            return MPTKEvent.EnumLength.Sixteenth;
        }

        private void AnalyzeTimeSignature(MetaEvent meta, TrackMidiEvent trackEvent = null)
        {
            TimeSignatureEvent timesig = (TimeSignatureEvent)meta;
            // Numerator: counts the number of beats in a measure. 
            // For example a numerator of 4 means that each bar contains four beats. 
            MPTK_TimeSigNumerator = timesig.Numerator;
            // Denominator: number of quarter notes in a beat.0=ronde, 1=blanche, 2=quarter, 3=eighth, etc. 
            MPTK_TimeSigDenominator = timesig.Denominator;
            MPTK_NumberBeatsMeasure = timesig.Numerator;
            MPTK_NumberQuarterBeat = System.Convert.ToInt32(Mathf.Pow(2f, timesig.Denominator));
            MPTK_TicksInMetronomeClick = timesig.TicksInMetronomeClick;
            MPTK_No32ndNotesInQuarterNote = timesig.No32ndNotesInQuarterNote;
            if (LogEvents && trackEvent != null)
                Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Meta     {0,-15} Numerator:{1} Denominator:{2}", meta.MetaEventType, timesig.Numerator, timesig.Denominator));
        }

        private string BuildInfoTrack(TrackMidiEvent e)
        {
#if !DEBUG_LOGEVENT
            return string.Format("[A:{0,5:00000} Q:{1,5:00000} P:{2,5:00000}] [T:{3,2:00} C:{4,2:00}] ",
                e.Event.AbsoluteTime, e.AbsoluteQuantize, cur_ticks, e.IndexTrack, e.Event.Channel);
#else
            return string.Format("[A:{0,5:00000} D:{1,4:0000} Q:{2,5:00000} CurrentTick:{3,5:00000}] [Track:{4,2:00} Channel:{5,2:00}] ",
                e.Event.AbsoluteTime, e.Event.DeltaTime, e.AbsoluteQuantize, cur_ticks, e.IndexTrack, e.Event.Channel);
#endif
        }

        public void DebugTrack()
        {
            int itrck = 0;
            foreach (IList<MidiEvent> track in midifile.Events)
            {
                itrck++;
                foreach (MidiEvent midievent in track)
                {
                    string info = string.Format("Track:{0} Channel:{1,2:00} Command:{2} AbsoluteTime:{3:0000000} ", itrck, midievent.Channel, midievent.CommandCode, midievent.AbsoluteTime);
                    if (midievent.CommandCode == MidiCommandCode.NoteOn)
                    {
                        NoteOnEvent noteon = (NoteOnEvent)midievent;
                        if (noteon.OffEvent == null)
                            info += string.Format(" OffEvent null");
                        else
                            info += string.Format(" OffEvent.DeltaTimeChannel:{0:0000.00} ", noteon.OffEvent.DeltaTime);
                    }
                    Debug.Log(info);
                }
            }
        }
        public void DebugMidiSorted(List<TrackMidiEvent> midiSorted)
        {
            foreach (TrackMidiEvent midievent in midiSorted)
            {
                string info = string.Format("Track:{0} Channel:{1,2:00} Command:{2} AbsoluteTime:{3:0000000} DeltaTime:{4:0000000} ", midievent.IndexTrack, midievent.Event.Channel, midievent.Event.CommandCode, midievent.Event.AbsoluteTime, midievent.Event.DeltaTime);
                switch (midievent.Event.CommandCode)
                {
                    case MidiCommandCode.NoteOn:
                        NoteOnEvent noteon = (NoteOnEvent)midievent.Event;
                        if (noteon.Velocity == 0)
                            info += string.Format(" Velocity 0");
                        if (noteon.OffEvent == null)
                            info += string.Format(" OffEvent null");
                        else
                            info += string.Format(" OffEvent.DeltaTimeChannel:{0:0000.00} ", noteon.OffEvent.DeltaTime);
                        break;
                }
                Debug.Log(info);
            }
        }
        //! @endcond
    }
}

