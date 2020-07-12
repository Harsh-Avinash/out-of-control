using System;
using System.Collections.Generic;
using System.IO;
using MPTK.NAudio.Midi;
namespace MidiPlayerTK
{
    //using MonoProjectOptim;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Window editor for the setup of MPTK
    /// </summary>
    public class MidiFileSetupWindow : EditorWindow
    {

        private static MidiFileSetupWindow window;

        static Vector2 scrollPosMidiFile = Vector2.zero;
        static Vector2 scrollPosAnalyze = Vector2.zero;

        static float widthLeft;
        static float widthRight;

        static float heightList;

        static int itemHeight;
        static int buttonWidth;
        static int buttonShortWidth;
        static int buttonHeight;
        static float espace;

        static float xpostitlebox;
        static float ypostitlebox;

        static string midifile;

        static GUIStyle styleBold;
        static GUIStyle styleRichText;
        static float heightLine;

        static public CustomStyle myStyle;
        static private List<string> infoEvents;
        static public int PageToDisplay = 0;
        const int MAXLINEPAGE = 100;
        static bool withMeta = true, withNoteOn = false, withNoteOff = false, withPatchChange = true;
        static bool withControlChange = false, withAfterTouch = false, withOthers = false;

        static int indexEditItem;
        private Texture buttonIconHelp;

        // % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        [MenuItem("MPTK/Midi File Setup &M", false, 10)]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            try
            {
                window = GetWindow<MidiFileSetupWindow>(true, "Midi File Setup");
                window.minSize = new Vector2(828, 400);

                styleBold = new GUIStyle(EditorStyles.boldLabel);
                styleBold.fontStyle = FontStyle.Bold;

                styleRichText = new GUIStyle(EditorStyles.label);
                styleRichText.richText = true;
                styleRichText.alignment = TextAnchor.UpperLeft;
                heightLine = styleRichText.lineHeight * 1.2f;

                espace = 5;
                widthLeft = 500;// 415;
                itemHeight = 25;
                buttonWidth = 150;
                buttonHeight = 18;
                buttonShortWidth = 50;

                xpostitlebox = 2;
                ypostitlebox = 5;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void OnEnable()
        {
            buttonIconHelp = Resources.Load<Texture2D>("Textures/question-mark");
        }

        private void OnLostFocus()
        {
#if UNITY_2017_1_OR_NEWER
            // Trig an  error before v2017...
            if (Application.isPlaying)
            {
                window.Close();
            }
#endif
        }
        private void OnFocus()
        {
            // Load description of available soundfont
            try
            {
                MidiPlayerGlobal.InitPath();
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        void OnGUI()
        {
            try
            {
                if (window == null) Init();
                float startx = 5;
                float starty = 7;
                //Log.Write("test");

                if (myStyle == null)
                    myStyle = new CustomStyle();

                GUIContent content = new GUIContent() { text = "Setup Midi files - Version " + ToolsEditor.version, tooltip = "" };
                EditorGUI.LabelField(new Rect(startx, starty, 500, itemHeight), content, styleBold);

                if (GUI.Button(new Rect(startx + 410, starty - 5, 25, 25), buttonIconHelp)) Application.OpenURL("https://paxstellar.fr/setup-mptk-add-midi-files-v2/");

                GUI.color = ToolsEditor.ButtonColor;
                content = new GUIContent() { text = "Help & Contact", tooltip = "Get some help" };
                Rect rect = new Rect(window.position.size.x - buttonWidth - 5, starty, buttonWidth, buttonHeight);
                if (GUI.Button(rect, content))
                    PopupWindow.Show(rect, new AboutMPTK());

                starty += buttonHeight + espace;

                widthRight = window.position.size.x - widthLeft - 2 * espace - startx;
                heightList = window.position.size.y - 3 * espace - starty;

                ShowListMidiFiles(startx, starty + espace);
                ShowMidiAnalyse(startx + widthLeft + espace, starty + espace);

            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display, add, remove Midi file
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowListMidiFiles(float localstartX, float localstartY)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, widthLeft, heightList);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;

                string caption = "Midi file available";
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null || MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count == 0)
                {
                    caption = "No Midi file available yet";
                }

                GUIContent content = new GUIContent() { text = caption, tooltip = "" };
                EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);

                if (indexEditItem >= 0 && indexEditItem < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count &&
                    !string.IsNullOrEmpty(MidiPlayerGlobal.CurrentMidiSet.MidiFiles[indexEditItem]))
                {
                    string name = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[indexEditItem];
                    if (name.Length > 10) name = name.Substring(0, 10);
                    string midiselected = "'" + indexEditItem + "-" + name + "'";

                    if (GUI.Button(new Rect(localstartX + xpostitlebox + 150, localstartY + ypostitlebox, buttonWidth, buttonHeight), "Remove " + midiselected))
                    {
                        DeleteResource(MidiLoad.BuildOSPath(MidiPlayerGlobal.CurrentMidiSet.MidiFiles[indexEditItem]));
                        AssetDatabase.Refresh();
                        ToolsEditor.LoadMidiSet();
                        ToolsEditor.CheckMidiSet();
                        AssetDatabase.Refresh();
                    }
                }

                if (GUI.Button(new Rect(widthLeft - buttonWidth - espace, localstartY + ypostitlebox, buttonWidth, buttonHeight), "Add Midi file"))
                    AddMidifile();

                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null)
                {
                    Rect listVisibleRect = new Rect(localstartX, localstartY + itemHeight, widthLeft - 5, heightList - itemHeight - 5);
                    Rect listContentRect = new Rect(0, 0, widthLeft - 20, MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count * itemHeight + 5);

                    scrollPosMidiFile = GUI.BeginScrollView(listVisibleRect, scrollPosMidiFile, listContentRect);
                    float boxY = 0;

                    for (int i = 0; i < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count; i++)
                    {
                        //GUI.color = new Color(.7f, .7f, .7f, 1f);
                        float boxX = 5;
                        content = new GUIContent() { text = i.ToString() + " - " + MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i], tooltip = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i] };

                        if (indexEditItem == i)
                            GUI.color = new Color(.7f, .7f, .7f, 1f);
                        else
                            GUI.color = Color.white;

                        GUI.Box(new Rect(boxX, boxY + 5, widthLeft - 30, itemHeight), "");
                        if (GUI.Button(new Rect(boxX + 5, boxY + 9, widthLeft - 30, itemHeight), content, myStyle.BtListNormal))
                        {
                            indexEditItem = i;
                            ReadEvents();
                        }

                        boxY += itemHeight;
                    }
                    GUI.EndScrollView();
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display analyse of midifile
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowMidiAnalyse(float localstartX, float localstartY)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, widthRight, heightList);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;

                if (infoEvents != null && infoEvents.Count > 0)
                {
                    float posx = localstartX + espace;
                    float posy = localstartY + espace;
                    int toggleLargeWidth = 70;
                    int toggleSmallWidth = 55;

                    //if (GUI.Button(new Rect(posx, posy, buttonWidth, buttonHeight), "Analyse"))
                    //{
                    //    ReadEvents();
                    //}
                    //posx += buttonWidth + espace;

                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "All"))
                    {
                        withMeta = withNoteOn = withNoteOff = withControlChange = withPatchChange = withAfterTouch = withOthers = true;
                        ReadEvents();
                    }
                    posx += buttonShortWidth + espace;

                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "None"))
                    {
                        withMeta = withNoteOn = withNoteOff = withControlChange = withPatchChange = withAfterTouch = withOthers = false;
                        ReadEvents();
                    }
                    posx += buttonShortWidth + espace;

                    bool filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withMeta, "Meta");
                    if (filter != withMeta)
                    {
                        withMeta = filter;
                        ReadEvents();
                    }

                    posx += toggleSmallWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleLargeWidth, buttonHeight), withNoteOn, "Note On");
                    if (filter != withNoteOn)
                    {
                        withNoteOn = filter;
                        ReadEvents();
                    }
                    posx += toggleLargeWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleLargeWidth, buttonHeight), withNoteOff, "Note Off");
                    if (filter != withNoteOff)
                    {
                        withNoteOff = filter;
                        ReadEvents();
                    }
                    posx += toggleLargeWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleLargeWidth, buttonHeight), withControlChange, "Control");
                    if (filter != withControlChange)
                    {
                        withControlChange = filter;
                        ReadEvents();
                    }
                    posx += toggleLargeWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withPatchChange, "Patch");
                    if (filter != withPatchChange)
                    {
                        withPatchChange = filter;
                        ReadEvents();
                    }
                    posx += toggleSmallWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withAfterTouch, "Touch");
                    if (filter != withAfterTouch)
                    {
                        withAfterTouch = filter;
                        ReadEvents();
                    }
                    posx += toggleSmallWidth + espace;

                    filter = GUI.Toggle(new Rect(posx, posy, toggleSmallWidth, buttonHeight), withOthers, "Others");
                    if (filter != withOthers)
                    {
                        withOthers = filter;
                        ReadEvents();
                    }
                    posx += toggleSmallWidth + espace;

                    if (PageToDisplay < 0) PageToDisplay = 0;
                    if (PageToDisplay * MAXLINEPAGE > infoEvents.Count) PageToDisplay = infoEvents.Count / MAXLINEPAGE;

                    string infoToDisplay = "";
                    for (int i = PageToDisplay * MAXLINEPAGE; i < (PageToDisplay + 1) * MAXLINEPAGE; i++)
                        if (i < infoEvents.Count)
                            infoToDisplay += infoEvents[i] + "\n";

                    posx = localstartX + espace;
                    posy = localstartY + espace + buttonHeight + espace;

                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "<<")) PageToDisplay = 0;

                    posx += buttonShortWidth + espace;
                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), "<")) PageToDisplay--;

                    posx += buttonShortWidth + espace;
                    GUI.Label(new Rect(posx, posy, buttonWidth / 2, buttonHeight),
                        "Page " + (PageToDisplay + 1).ToString() + " / " + (infoEvents.Count / MAXLINEPAGE + 1).ToString(), myStyle.LabelCentered);

                    posx += buttonWidth / 2 + espace;
                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), ">")) PageToDisplay++;

                    posx += buttonShortWidth + espace;
                    if (GUI.Button(new Rect(posx, posy, buttonShortWidth, buttonHeight), ">>")) PageToDisplay = infoEvents.Count / MAXLINEPAGE;

                    Rect listVisibleRect = new Rect(localstartX, posy + buttonHeight + espace, widthRight, heightList - 2 * buttonHeight - 4 * espace);
                    Rect listContentRect = new Rect(0, 0, widthRight - 15, MAXLINEPAGE * heightLine + 5);

                    scrollPosAnalyze = GUI.BeginScrollView(listVisibleRect, scrollPosAnalyze, listContentRect);
                    GUILayout.Label(infoToDisplay, myStyle.TextFieldMultiLine);

                    //GUI.color = new Color(.8f, .8f, .8f, 1f);
                    //float labelY = -heightLine;
                    //foreach (string s in ScanInfo.Infos)
                    //    EditorGUI.LabelField(new Rect(0, labelY += heightLine, widthRight, heightLine), s, styleRichText);
                    //GUI.color = Color.white;

                    GUI.EndScrollView();
                }
                else
                {
                    GUIContent content = new GUIContent() { text = "No Midi file analysed", tooltip = "" };
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        static private void ReadEvents()
        {
            midifile = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[indexEditItem];
            infoEvents = MidiScan.GeneralInfo(midifile, withNoteOn, withNoteOff, withControlChange, withPatchChange, withAfterTouch, withMeta, withOthers);
            infoEvents.Insert(0, "Open midi file: " + midifile);
            infoEvents.Insert(0, "DB Midi index: " + indexEditItem);
            PageToDisplay = 0;
            scrollPosAnalyze = Vector2.zero;
        }

        /// <summary>
        /// Add a new Midi file from desktop
        /// </summary>
        private static void AddMidifile()
        {
            try
            {
                string selectedFile = EditorUtility.OpenFilePanelWithFilters("Open and import Midi file", ToolsEditor.lastDirectoryMidi, new string[] { "Midi files", "mid,midi", "Karoke files", "kar", "All", "*" });
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    ToolsEditor.lastDirectoryMidi = Path.GetDirectoryName(selectedFile);

                    // Build path to midi folder 
                    string pathMidiFile = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);
                    if (!Directory.Exists(pathMidiFile))
                        Directory.CreateDirectory(pathMidiFile);

                    try
                    {
                        MidiLoad midifile = new MidiLoad();
                        if (!midifile.MPTK_LoadFile(selectedFile))
                        {
                            EditorUtility.DisplayDialog("Midi Not Loaded", "Try to open " + selectedFile + "\nbut this file is not a valid midi file", "ok");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarningFormat("{0} {1}", selectedFile, ex.Message);
                        return;
                    }

                    string filename = Path.GetFileNameWithoutExtension(selectedFile);
                    //foreach (char c in filename) Debug.Log(string.Format("{0} {1}", c, (int)c));
                    foreach (char i in Path.GetInvalidFileNameChars())
                    {
                        filename = filename.Replace(i, '_');
                    }
                    string filenameToSave = Path.Combine(pathMidiFile, filename + MidiPlayerGlobal.ExtensionMidiFile);

                    filenameToSave = filenameToSave.Replace('(', '_');
                    filenameToSave = filenameToSave.Replace(')', '_');
                    filenameToSave = filenameToSave.Replace('#', '_');
                    filenameToSave = filenameToSave.Replace('$', '_');

                    // Create a copy of the midi file in MPTK resources
                    File.Copy(selectedFile, filenameToSave, true);

                    if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null)
                        MidiPlayerGlobal.CurrentMidiSet.MidiFiles = new List<string>();

                    // Add midi file to the list
                    string midiname = Path.GetFileNameWithoutExtension(selectedFile);
                    if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == midiname) < 0)
                    {
                        MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Add(midiname);
                        MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Sort();
                        MidiPlayerGlobal.CurrentMidiSet.Save();
                    }
                    indexEditItem = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == midiname);
                }
                AssetDatabase.Refresh();
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                AssetDatabase.Refresh();
                ReadEvents();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        static private void DeleteResource(string filepath)
        {
            try
            {
                Debug.Log("Delete " + filepath);
                File.Delete(filepath);
                // delete also meta
                string meta = filepath + ".meta";
                Debug.Log("Delete " + meta);
                File.Delete(meta);

            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }

}