//#define MPTK_PRO
using System;
using System.Collections.Generic;
using MPTK.NAudio.Midi;
namespace MidiPlayerTK
{
    //using MonoProjectOptim;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Window editor for the setup of MPTK
    /// </summary>
    public class SoundFontSetupWindow : EditorWindow
    {
        private static SoundFontSetupWindow window;

        Vector2 scrollPosBanks = Vector2.zero;
        Vector2 scrollPosSoundFont = Vector2.zero;

        static float widthLeft = 415 + 30;
        static float widthRight; // calculated

        static float heightLeftTop = 300;
        static float heightRightTop = 400;
        static float heightLeftBottom;  // calculated
        static float heightRightBottom; // calculated

        static float itemHeight = 25;
        static float titleHeight = 18; //label title above list
        static float buttonLargeWidth = 180;
        static float buttonMediumWidth = 60;
        static float buttonHeight = 18;
        static float espace = 5;

        static float xpostitlebox = 2;
        static float ypostitlebox = 5;

        static GUIStyle styleBold;
        static GUIStyle styleRed;
        static GUIStyle styleRichText;
        static GUIStyle styleLabelRight;
        static GUIStyle styleListTitle;
        static GUIStyle styleMiniButton;
        static GUIStyle styleRowListNormal;
        static GUIStyle styleRowListSelected;

        public static BuilderInfo LogInfo;
#if MPTK_PRO
        Vector2 scrollPosOptim = Vector2.zero;
#endif
        static public bool KeepAllPatchs = false;
        static public bool KeepAllZones = false;
        static public bool RemoveUnusedWaves = false;
        static public bool LogDetailSoundFont = false;

        private Texture buttonIconView;
        private Texture buttonIconHelp;
        private Texture buttonIconSave;
        private Texture buttonIconFolders;
        private Texture buttonIconDelete;

        // % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        [MenuItem("MPTK/SoundFont Setup &F", false, 20)]
        public static void Init()
        {
            //Debug.Log("init");
            // Get existing open window or if none, make a new one:
            try
            {
                window = GetWindow<SoundFontSetupWindow>(true, "SoundFont Setup");
                window.minSize = new Vector2(828 + 65, 565);

                styleBold = new GUIStyle(EditorStyles.boldLabel);
                styleBold.fontStyle = FontStyle.Bold;

                styleMiniButton = new GUIStyle(EditorStyles.miniButtonMid);
                styleMiniButton.fixedWidth = 16;
                styleMiniButton.fixedHeight = 16;

                styleListTitle = new GUIStyle("box");

                styleRed = new GUIStyle(EditorStyles.label);
                styleRed.normal.textColor = new Color(0.5f, 0, 0);
                styleRed.fontStyle = FontStyle.Bold;

                styleRichText = new GUIStyle(EditorStyles.label);
                styleRichText.richText = true;
                styleRichText.alignment = TextAnchor.UpperLeft;

                styleLabelRight = new GUIStyle(EditorStyles.label);
                styleLabelRight.alignment = TextAnchor.MiddleRight;
                //styleLabelRight.normal.background = ToolsEditor.SetColor(new Texture2D(2, 2), new Color(.6f, .8f, .6f, 1f));
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            //Debug.Log("end init");

        }

        private void OnEnable()
        {
            buttonIconView = Resources.Load<Texture2D>("Textures/eye");
            buttonIconSave = Resources.Load<Texture2D>("Textures/Save_24x24");
            buttonIconFolders = Resources.Load<Texture2D>("Textures/Folders");
            buttonIconDelete = Resources.Load<Texture2D>("Textures/Delete_32x32");
            buttonIconHelp = Resources.Load<Texture2D>("Textures/question-mark");

        }
        /// <summary>
        /// Reload data
        /// </summary>
        private void OnFocus()
        {
            // Load description of available soundfont
            try
            {
                MidiPlayerGlobal.InitPath();

                //Debug.Log(MidiPlayerGlobal.ImSFCurrent == null ? "ImSFCurrent is null" : "ImSFCurrent:" + MidiPlayerGlobal.ImSFCurrent.SoundFontName);
                //Debug.Log(MidiPlayerGlobal.CurrentMidiSet == null ? "CurrentMidiSet is null" : "CurrentMidiSet" + MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);
                //Debug.Log(MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null ? "ActiveSounFontInfo is null" : MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                // cause catch if call when playing (setup open on run mode)
                try
                {
                    if (!Application.isPlaying)
                        AssetDatabase.Refresh();
                }
                catch (Exception)
                {
                }
                // Exec after Refresh, either cause errror
                if (MidiPlayerGlobal.ImSFCurrent == null)
                    MidiPlayerGlobal.LoadCurrentSF();
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
                if (LogInfo == null) LogInfo = new BuilderInfo();

                float startx = 5;
                float starty = 7;

                if (styleRowListNormal == null) styleRowListNormal = new GUIStyle("box");
                if (styleRowListSelected == null)
                {
                    styleRowListSelected = new GUIStyle("box");
                    styleRowListSelected.normal.background = ToolsEditor.SetColor(new Texture2D(2, 2), new Color(.6f, .8f, .6f, 1f));
                }

                GUIContent content = new GUIContent() { text = "Setup SoundFont - Version " + ToolsEditor.version, tooltip = "" };
                EditorGUI.LabelField(new Rect(startx, starty, 500, itemHeight), content, styleBold);

                if (GUI.Button(new Rect(startx + 370, starty - 5, 25, 25), buttonIconHelp))
                {
                    //CreateWave createwave = new CreateWave();
                    //string path = System.IO.Path.Combine(MidiPlayerGlobal.MPTK_PathToResources, "unitySample") + ".wav";
                    ////string path = "unitySample.wav";
                    //HiSample sample = new HiSample();
                    ////sample.LoopStart = sample.LoopEnd = 0;
                    //byte[] data = new byte[10000];
                    //for (int i = 0; i < data.Length; i++) data[i] = (byte)255;
                    //sample.SampleRate = 44100;
                    //sample.End = (uint)data.Length/2;
                    //createwave.Build(path, sample, data);
                    Application.OpenURL("https://paxstellar.fr/setup-mptk-add-soundfonts-v2/");
                }
                GUI.color = ToolsEditor.ButtonColor;
                content = new GUIContent() { text = "Help & Contact", tooltip = "Get some help" };
                // Set position of the button
                Rect rect = new Rect(window.position.size.x - buttonLargeWidth - 5, starty, buttonLargeWidth, buttonHeight);
                if (GUI.Button(rect, content))
                    PopupWindow.Show(rect, new AboutMPTK());

                starty += buttonHeight + espace;

                widthRight = window.position.size.x - widthLeft - 2 * espace - startx;
                //widthRight = window.position.size.x / 2f - espace;
                //widthLeft = window.position.size.x / 2f - espace;

                heightLeftBottom = window.position.size.y - heightLeftTop - 3 * espace - starty;
                heightRightBottom = window.position.size.y - heightRightTop - 3 * espace - starty;

                // Display list of soundfont already loaded 
                ShowListSoundFonts(startx, starty, widthLeft, heightLeftTop);

                ShowListBanks(startx + widthLeft + espace, starty, widthRight, heightRightTop);

                ShowExtractOptim(startx + widthLeft + espace, starty + heightRightTop + espace, widthRight, heightRightBottom + espace);

                ShowLogOptim(startx, starty + espace + heightLeftTop, widthLeft, heightLeftBottom + espace);
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display, add, remove Soundfont
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowListSoundFonts(float startX, float startY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(startX, startY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;
                float localstartX = 0;
                float localstartY = 0;
                GUIContent content;
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count > 0)
                    content = new GUIContent() { text = "SoundFont available", tooltip = "Each SoundFonts contains a set of bank of sound. \nOnly one SoundFont can be active at the same time for the midi player" };
                else
                {
                    content = new GUIContent() { text = "No SoundFont found, click on button 'Add SoundFont'", tooltip = "See the documentation here https://paxstellar.fr/" };
                    MidiPlayerGlobal.ImSFCurrent = null;
                }
                localstartX += xpostitlebox;
                localstartY += ypostitlebox;
                EditorGUI.LabelField(new Rect(startX + localstartX + 5, startY + localstartY, width, titleHeight), content, styleBold);
                localstartY += titleHeight;
                Rect rect1 = new Rect(startX + localstartX + espace, startY + localstartY, buttonLargeWidth, buttonHeight);

#if MPTK_PRO
                if (GUI.Button(rect1, "Add SoundFont"))
                {
                    if (Application.isPlaying)
                        EditorUtility.DisplayDialog("Add a SoundFont", "This action is not possible when application is running.", "Ok");
                    else
                    {
                        //if (EditorUtility.DisplayDialog("Import SoundFont", "This action could take time, do you confirm ?", "Ok", "Cancel"))
                        {
                            this.AddSoundFont();
                            scrollPosSoundFont = Vector2.zero;
                        }
                    }
                }
#else
                if (GUI.Button(rect1, "Add a new SoundFont [PRO]"))
                    PopupWindow.Show(new Rect(startX + localstartX, startY + localstartY, buttonLargeWidth, buttonHeight), new GetFullVersion());
#endif
                rect1 = new Rect(width - buttonMediumWidth - 25, startY + localstartY, buttonMediumWidth, buttonHeight);
#if MPTK_PRO
                //rect = new Rect(width - 1 * (buttonWidth), localstartY, buttonWidth, buttonHeight);

                if (GUI.Button(rect1, "Remove"))
                {
                    if (Application.isPlaying)
                        EditorUtility.DisplayDialog("Remove a SoundFont", "This action is not possible when application is running.", "Ok");
                    else
                    {
                        this.DeleteSf();
                        if (MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count > 0)
                            this.SelectSf(0);
                        else
                            MidiPlayerGlobal.ImSFCurrent = null;
                    }
                }
#else
                if (GUI.Button(rect1, "Remove"))
                    PopupWindow.Show(rect1, new GetFullVersion());
#endif
                localstartY += buttonHeight + espace;

                // Draw title list box
                GUI.color = new Color(.6f, .6f, .6f, 1f);
                GUI.Box(new Rect(startX + localstartX + espace, startY + localstartY, width - 35, 20), "", styleListTitle);
                GUI.color = Color.white;

                // Draw text title list box
                GUI.Label(new Rect(startX + localstartX + espace, startY + localstartY, width - 25, itemHeight), " SoundFont Name                            Patch  Wave     Size");
                localstartY += itemHeight;

                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count > 0)
                {

                    Rect listVisibleRect = new Rect(startX + localstartX, startY + localstartY - 11, width - 10, height - localstartY);
                    Rect listContentRect = new Rect(0, 0, width - 25, MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count * itemHeight + 5);

                    scrollPosSoundFont = GUI.BeginScrollView(listVisibleRect, scrollPosSoundFont, listContentRect, false, true);
                    float boxY = 0;

                    // Loop on each soundfont
                    for (int i = 0; i < MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count; i++)
                    {
                        SoundFontInfo sf = MidiPlayerGlobal.CurrentMidiSet.SoundFonts[i];
                        bool selected = (MidiPlayerGlobal.ImSFCurrent != null && sf.Name == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name);

                        GUI.color = selected ? ToolsEditor.ButtonColor : new Color(.7f, .7f, .7f, 1f);
                        float boxX = espace;
                        GUI.Box(new Rect(boxX, boxY + 5, width - 35, itemHeight), "");//,  styleRowListNormal);
                        GUI.color = Color.white;
                        boxX += espace;

                        float colw = 210;
                        content = new GUIContent() { text = sf.Name, tooltip = "" };
                        EditorGUI.LabelField(new Rect(boxX, boxY + 9, colw, itemHeight - 5), content);
                        boxX = colw + espace;

                        colw = 30;
                        content = new GUIContent() { text = sf.PatchCount.ToString(), tooltip = "" };
                        EditorGUI.LabelField(new Rect(boxX, boxY + 9, colw, itemHeight - 7), content, styleLabelRight);
                        boxX += colw + espace;

                        content = new GUIContent() { text = sf.WaveCount.ToString(), tooltip = "" };
                        EditorGUI.LabelField(new Rect(boxX, boxY + 9, colw, itemHeight - 7), content, styleLabelRight);
                        boxX += colw + espace;

                        string sizew = (sf.WaveSize < 1000000) ?
                             Math.Round((double)sf.WaveSize / 1000d).ToString() + " Ko" :
                             Math.Round((double)sf.WaveSize / 1000000d).ToString() + " Mo";
                        content = new GUIContent() { text = sizew, tooltip = "" };
                        EditorGUI.LabelField(new Rect(boxX, boxY + 9, colw * 2, itemHeight - 7), content, styleLabelRight);
                        boxX += colw * 2 + espace;

                        string textselect = "Select";
                        if (MidiPlayerGlobal.ImSFCurrent != null && sf.Name == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name)
                            textselect = "Default";// GUI.color = ToolsEditor.ButtonColor;

                        // Select button right aligned
                        if (GUI.Button(new Rect(width - 35 - 1 * buttonMediumWidth, boxY + 9, buttonMediumWidth, buttonHeight), textselect))
                        {
#if MPTK_PRO
                            this.SelectSf(i);
#endif
                        }
                        boxY += itemHeight - 1;
                        GUI.color = Color.white;
                    }
                    GUI.EndScrollView();
                }
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void ShowListBanks(float startX, float startY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(startX, startY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;
                float localstartX = 0;
                float localstartY = 0;
                if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.ImSFCurrent.Banks != null)
                {
                    GUIContent content = new GUIContent() { text = "Banks available in SoundFont " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, tooltip = "Each bank contains a set of patchs (instrument).\nOnly two banks can be active at the same time : default sound (piano, ...) and drum kit (percussive)" };
                    localstartX += xpostitlebox;
                    localstartY += ypostitlebox;
                    EditorGUI.LabelField(new Rect(startX + localstartX + 5, startY + localstartY, width, titleHeight), content, styleBold);
                    localstartY += titleHeight;

                    // Save selection of banks
                    float btw = 25;
                    if (GUI.Button(new Rect(startX + localstartX + espace, startY + localstartY, btw, buttonHeight), new GUIContent(buttonIconSave, "Save banks configuration")))
                    {
#if MPTK_PRO
                        if (Application.isPlaying)
                            EditorUtility.DisplayDialog("Save Bank Configuration", "This action is not possible when application is running.", "Ok");
                        else
                            SaveBanksConfig();
#endif
                    }

                    btw = 75;
                    float buttonX = startX + localstartX + btw + 4 * espace;
                    EditorGUI.LabelField(new Rect(buttonX, startY + localstartY, btw, buttonHeight), "Keep banks:");
                    buttonX += btw;

                    if (GUI.Button(new Rect(buttonX, startY + localstartY, btw, buttonHeight), new GUIContent("All", "Select all banks to be kept in the SoundFont")))
                    {
                        if (MidiPlayerGlobal.ImSFCurrent != null) MidiPlayerGlobal.ImSFCurrent.SelectAllBanks();
                    }
                    buttonX += btw + espace;

                    if (GUI.Button(new Rect(buttonX, startY + localstartY, btw, buttonHeight), new GUIContent("None", "Unselect all banks to be kept in the SoundFont")))
                    {
                        if (MidiPlayerGlobal.ImSFCurrent != null) MidiPlayerGlobal.ImSFCurrent.UnSelectAllBanks();
                    }
                    buttonX += btw + espace;

                    if (GUI.Button(new Rect(buttonX, startY + localstartY, btw, buttonHeight), new GUIContent("Inverse", "Inverse selection of banks to be kept in the SoundFont")))
                    {
                        if (MidiPlayerGlobal.ImSFCurrent != null) MidiPlayerGlobal.ImSFCurrent.InverseSelectedBanks();
                    }
                    buttonX += btw + espace;

                    localstartY += buttonHeight + espace;

                    // Draw title list box
                    GUI.color = new Color(.6f, .6f, .6f, 1f);
                    GUI.Box(new Rect(startX + localstartX + espace, startY + localstartY, width - 35, 20), "", styleListTitle);
                    //localstartY += itemHeight;
                    GUI.color = Color.white;

                    // Draw text title list box
                    GUI.Label(new Rect(startX + localstartX + espace, startY + localstartY, width - 25, itemHeight), " Bank number                    View   Keep      Set default bank");
                    localstartY += itemHeight;

                    // Count available banks
                    int countBank = 0;
                    foreach (ImBank bank in MidiPlayerGlobal.ImSFCurrent.Banks)
                        if (bank != null) countBank++;
                    Rect listVisibleRect = new Rect(startX + localstartX, startY + localstartY - 11, width - 10, height - localstartY);
                    Rect listContentRect = new Rect(0, 0, width - 25, countBank * itemHeight + 5);

                    scrollPosBanks = GUI.BeginScrollView(listVisibleRect, scrollPosBanks, listContentRect, false, true);

                    float boxY = 0;
                    //SoundFontInfo sfi = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo;
                    if (MidiPlayerGlobal.ImSFCurrent != null)
                    {
                        foreach (ImBank bank in MidiPlayerGlobal.ImSFCurrent.Banks)
                        {
                            if (bank != null)
                            {
                                GUI.color = new Color(.7f, .7f, .7f, 1f);
                                GUI.Box(new Rect(5, boxY + 5, width - 35, itemHeight), "");

                                GUI.color = Color.white;

                                content = new GUIContent() { text = string.Format("Bank [{0,3:000}] Patch:{1,4}", bank.BankNumber, bank.PatchCount), tooltip = bank.Description };
                                GUI.Label(new Rect(10, boxY + 9, 165, itemHeight), content);
                                float btstart = 170;
                                btw = 25;

                                // View list of patchs
                                Rect btrect = new Rect(btstart, boxY + 9, btw, buttonHeight);
                                if (GUI.Button(btrect, new GUIContent(buttonIconView, "See the detail of this bank")))
                                    PopupWindow.Show(btrect, new PopupListPatchs("Patch", false, bank.GetDescription()));
                                btstart += btw + 3 * espace;

                                // Select bank to keep
                                btw = 75;
                                //Debug.Log(sfi.DefaultBankNumber );
                                Rect rect = new Rect(btstart, boxY + 9, 20, buttonHeight);
                                bool newSelect = GUI.Toggle(rect, MidiPlayerGlobal.ImSFCurrent.BankSelected[bank.BankNumber], new GUIContent("", ""));
                                btstart += 20 + 2 * espace;
                                if (newSelect != MidiPlayerGlobal.ImSFCurrent.BankSelected[bank.BankNumber])
                                {
#if MPTK_PRO
                                    MidiPlayerGlobal.ImSFCurrent.BankSelected[bank.BankNumber] = newSelect;
#else
                                    PopupWindow.Show(rect, new GetFullVersion());
#endif
                                }
                                GUI.color = Color.white;

                                // Set default bank for instrument
                                if (MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber == bank.BankNumber) GUI.color = ToolsEditor.ButtonColor;
                                if (GUI.Button(new Rect(btstart, boxY + 9, btw, buttonHeight), new GUIContent("Instrument", "Select this bank as default for playing all instruments except drum")))
                                {
                                    MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber = MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber != bank.BankNumber ? bank.BankNumber : -1;
                                }
                                btstart += btw + espace;
                                GUI.color = Color.white;

                                // Set default bank for Drum
                                if (MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber == bank.BankNumber) GUI.color = ToolsEditor.ButtonColor;
                                if (GUI.Button(new Rect(btstart, boxY + 9, btw, buttonHeight), new GUIContent("Drum", "Select this bank as default for playing drum hit")))
                                {
                                    MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber = MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber != bank.BankNumber ? bank.BankNumber : -1;
                                }
                                GUI.color = Color.white;

                                boxY += itemHeight - 1;
                            }
                        }
                    }

                    GUI.EndScrollView();
                }
                else
                    EditorGUI.LabelField(new Rect(startX + xpostitlebox, startY + ypostitlebox, 300, itemHeight), "No SoundFont selected", styleBold);
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

#if MPTK_PRO
        private bool SaveBanksConfig()
        {
            string infocheck = this.CheckAndSetBank();
            if (string.IsNullOrEmpty(infocheck))
            {
                // Save MPTK SoundFont : xml only
                this.SaveCurrentIMSF(true);
                AssetDatabase.Refresh();
                return true;
            }
            else
                EditorUtility.DisplayDialog("Save Selected Bank", infocheck, "Ok");
            return false;
        }
#endif
        /// <summary>
        /// Display optimization
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowExtractOptim(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;

                string tooltip = "Remove all banks and Presets not used in the Midi file list";

                GUIContent content;
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {

                    float xpos = localstartX + xpostitlebox + 5;
                    float ypos = localstartY + ypostitlebox;
                    content = new GUIContent() { text = "Extract Patchs & Waves from " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, tooltip = tooltip };
                    EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), content, styleBold);
                    ypos += itemHeight;// + espace;

                    float widthCheck = buttonLargeWidth;
                    /*
                    KeepAllZones = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), KeepAllZones, new GUIContent("Keep all Zones", "Keep all Waves associated with a Patch regardless of notes and velocities played in Midi files.\n Usefull if you want transpose Midi files."));
                    xpos += widthCheck + espace;
                    KeepAllPatchs = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), KeepAllPatchs, new GUIContent("Keep all Patchs", "Keep all Patchs and waves found in the SoundFont selected.\nWarning : a huge volume of files coud be created"));
                    xpos += widthCheck + +2 * espace;
                    */
                    RemoveUnusedWaves = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), RemoveUnusedWaves, new GUIContent("Remove unused waves", "If check, keep only waves used by your midi files"));
                    //xpos += widthCheck + espace;
                    ypos += itemHeight;

                    LogDetailSoundFont = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), LogDetailSoundFont, new GUIContent("Log SoundFont Detail", "If check, keep only waves used by your midi files"));
                    ypos += itemHeight;

                    // restaure X positio,
                    xpos = localstartX + xpostitlebox + 5;
                    Rect rect1 = new Rect(xpos, ypos, 210, (float)buttonHeight * 2f);
                    Rect rect2 = new Rect(xpos + 210 + 3, ypos, 210, (float)buttonHeight * 2f);
#if MPTK_PRO
                    if (GUI.Button(rect1, new GUIContent("Optimize from Midi file list", "Your list of Midi files will be scanned to identify patchs and zones useful")))
                    {
                        if (Application.isPlaying)
                            EditorUtility.DisplayDialog("Optimization", "This action is not possible when application is running.", "Ok");
                        else
                        {
                            if (SaveBanksConfig())
                            {
                                KeepAllPatchs = false;
                                KeepAllZones = false;
                                this.OptimizeSoundFont();// LogInfo, KeepAllPatchs, KeepAllZones, RemoveUnusedWaves);
                            }
                        }
                    }

                    if (GUI.Button(rect2, new GUIContent("Extract all Patchs & Waves", "All patchs and waves will be extracted from the Soundfile")))
                    {
                        if (Application.isPlaying)
                            EditorUtility.DisplayDialog("Extraction", "This action is not possible when application is running.", "Ok");
                        else
                        {
                            if (SaveBanksConfig())
                            {
                                KeepAllPatchs = true;
                                KeepAllZones = true;
                                this.OptimizeSoundFont();// (LogInfo, KeepAllPatchs, KeepAllZones, RemoveUnusedWaves);
                            }
                        }
                    }
#else
                    if (GUI.Button(rect1, new GUIContent("Optimize from Midi file list [PRO]", "You need to setup some midi files before to launch ths optimization")))
                        PopupWindow.Show(rect1, new GetFullVersion());
                    if (GUI.Button(rect2, new GUIContent("Extract all Patchs & Waves [PRO]", "")))
                        PopupWindow.Show(rect2, new GetFullVersion());

#endif
                }
                else
                {
                    content = new GUIContent() { text = "No SoundFont selected", tooltip = tooltip };
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);
                }
            }
            catch (ExitGUIException) { }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display optimization log
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowLogOptim(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;
                float posx = localstartX;
                GUI.Label(new Rect(posx + espace, localstartY + espace, 40, buttonHeight), new GUIContent("Logs:"), styleBold);
                posx += 40;
                float btw = 25f;
                if (GUI.Button(new Rect(posx + espace, localstartY + espace, btw, buttonHeight), new GUIContent(buttonIconSave, "Save Log")))
                {
                    // Save log file
                    if (LogInfo != null)
                    {
                        string filenamelog = string.Format("SoundFontSetupLog {0} {1} .txt", MidiPlayerGlobal.ImSFCurrent.SoundFontName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.IO.Path.Combine(Application.persistentDataPath, filenamelog)))
                            foreach (string line in LogInfo.Infos)
                                file.WriteLine(line);
                    }
                }

                if (GUI.Button(new Rect(posx + 2f * espace + btw, localstartY + espace, btw, buttonHeight), new GUIContent(buttonIconFolders, "Open Logs Folder")))
                {
                    Application.OpenURL(Application.persistentDataPath);
                }

                if (GUI.Button(new Rect(posx + 3f * espace + 2f * btw, localstartY + espace, btw, buttonHeight), new GUIContent(buttonIconDelete, "Clear Logs")))
                {
                    LogInfo = new BuilderInfo();
                }
#if MPTK_PRO
                if (LogInfo != null && LogInfo.Count > 0)
                {
                    float heightLine = styleRichText.lineHeight * 1.2f;

                    Rect listVisibleRect = new Rect(espace, localstartY + buttonHeight + espace, width - 5, height - 25);
                    Rect listContentRect = new Rect(0, 0, 2 * width, LogInfo.Count * heightLine + 5);

                    scrollPosOptim = GUI.BeginScrollView(listVisibleRect, scrollPosOptim, listContentRect);
                    GUI.color = Color.white;
                    float labelY = -heightLine;
                    foreach (string s in LogInfo.Infos)
                        EditorGUI.LabelField(new Rect(localstartX, labelY += heightLine, width * 2, heightLine), s, styleRichText);

                    if (MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.PatchCount == 0)
                    {
                        float xpos = 0;
                        float ypos = labelY + 2 * heightLine;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "No Patchs and Samples has been yet extracted from the Soundfont.", styleRed); ypos += itemHeight / 1f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "On the right panel:", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   Select banks you want to keep.", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   Select default bank for instruments and drums kit.", styleRed); ypos += itemHeight / 1f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "Click on button:", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   'Optimize from Midi file list' to keep only patchs required or", styleRed); ypos += itemHeight / 2f;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 450, itemHeight), "   'Extract all Patchs & Samples' to keep all patchs of selected banks.", styleRed); ypos += itemHeight;
                    }

                    GUI.EndScrollView();
                }
#endif
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}