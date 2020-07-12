using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    public class PopupListItem
    {
        public bool Show = false;
        public string Title;
        public int Width = -1;
        public int Height = -1;
        public int ColCount;
        public int ColWidth = 200;
        public int ColHeight = 30;
        public bool KeepOpen;
        public object Tag;
        public int EspaceX = 5;
        public int EspaceY = 5;
        public int TitleHeight = 30;
        public int itemHeight = 25;
        public Color BackgroundColor = new Color(.5f, .5f, .5f, 1f);
        /// <summary>
        /// Selected item in list
        /// </summary>
        private int selectedItem;

        private CustomStyle myStyle;
        private Vector2 positionbt;
        private List<MPTKListItem> list;
        private Vector2 scrollPosSoundFont;
        private int resizedWidth;
        private int resizedHeight;
        private int calculatedColCount;
        private int countRow;

        //// the method call int+bool and retur string
        //Func<int, bool, string> myMethodName1;

        //// the method call string+int and retur bool
        //Func<string, int, bool> myMethodName;

        public Action<object, int> OnSelect;

        public int CountRow { get { return countRow; } }

        public void Draw(List<MPTKListItem> plist, int pselected, CustomStyle style)
        {
            list = plist;
            selectedItem = pselected;
            myStyle = style;
            if (Show)
            {
                // Min, one column
                if (ColCount < 1)
                    ColCount = 1;

                if (ColWidth < 20)
                    ColWidth = 20;

                if (ColHeight < 10)
                    ColHeight = 10;

                if (list.Count < ColCount)
                    calculatedColCount = list.Count;
                else
                    calculatedColCount = ColCount;

                if (calculatedColCount > 1)
                    countRow = (int)((float)list.Count / (float)calculatedColCount + 1f);
                else
                    countRow = list.Count;


                if (Width < 0)
                    // Try to fit all col without H scroll
                    resizedWidth = calculatedColCount * (ColWidth + EspaceX) + 5 * EspaceX;
                else
                    resizedWidth = Width;

                if (Height < 0)
                    // Try to fit all row without V scroll
                    resizedHeight = countRow * ColHeight + EspaceY;
                else
                    resizedHeight = Height;

                resizedHeight += TitleHeight + 2 * EspaceY;

                if (positionbt.x + resizedWidth > Screen.width)
                    // popup right out of the screen
                    positionbt.x = Screen.width - resizedWidth;

                if (positionbt.y + resizedHeight > Screen.height)
                    // popup bottom out of the screen
                    positionbt.y = Screen.height - resizedHeight;

                if (positionbt.x < 0)
                    // Popup left out of the screen
                    positionbt.x = 0;

                if (positionbt.y < 0)
                    // Popup top out of the screen
                    positionbt.y = 0;

                // Popup too big for screen, resize ...
                if (resizedWidth >= Screen.width)
                {
                    positionbt.x = 0;
                    resizedWidth = Screen.width;
                }

                if (resizedHeight >= Screen.height)
                {
                    positionbt.y = 0;
                    resizedHeight = Screen.height;
                }

                if (resizedWidth < 100)
                    resizedWidth = 100;

                if (resizedHeight < 35)
                    resizedHeight = 35;

                GUI.Window(10, new Rect(positionbt, new Vector2(resizedWidth, resizedHeight)), DrawWindow, "", myStyle.BackgPopupList);
            }
        }

        private void DrawWindow(int windowID)
        {
            int localstartX = 0;// EspaceX;
            int localstartY = 0;// EspaceY;
            int boxX = 0;
            int boxY = 0;

            Rect zone = new Rect(localstartX, localstartY, resizedWidth + EspaceX, resizedHeight + EspaceY);
            //GUI.color = BackgroundColor;
            GUI.Box(zone, "");
            //GUI.color = Color.white;

            // Draw title list box
            //GUI.color = new Color(.6f, .6f, .6f, 1f);
            GUI.Box(new Rect(localstartX, localstartY, resizedWidth - 2 * EspaceX, TitleHeight), "", new GUIStyle("box"));
            //GUI.color = Color.white;

            localstartX += EspaceX;


            // Draw text title list box
            if (resizedWidth > 250)
                GUI.Label(new Rect(localstartX, localstartY + 2, 200, TitleHeight), new GUIContent(Title), myStyle.TitleLabel2);

            //GUI.color = Color.white;

            int width = 30;
            boxX = resizedWidth - EspaceX - width;
            if (GUI.Button(new Rect(boxX, localstartY, width, TitleHeight), "X", myStyle.BtStandard))
                Show = false;

            width = 100;
            boxX = boxX - EspaceX - width;
            KeepOpen = GUI.Toggle(new Rect(boxX, localstartY + 4, width, TitleHeight), KeepOpen, new GUIContent("Keep Open"));

            localstartY += TitleHeight + EspaceY;


            Rect listVisibleRect = new Rect(localstartX, localstartY, resizedWidth - localstartX, resizedHeight - 2 * EspaceY - TitleHeight);
            Rect listContentRect = new Rect(0, 0, calculatedColCount * (ColWidth + EspaceX) + 0, countRow * ColHeight + EspaceY);

            scrollPosSoundFont = GUI.BeginScrollView(listVisibleRect, scrollPosSoundFont, listContentRect);

            boxX = 0;
            boxY = 0;

            int indexList = -1;
            foreach (MPTKListItem item in list)
            {
                if (item != null)
                {
                    indexList++;

                    GUIStyle style = myStyle.BtStandard;
                    if (item.Index == selectedItem) style = myStyle.BtSelected;

                    Rect rect = new Rect(boxX, boxY, ColWidth, ColHeight);

                    if (GUI.Button(rect, item.Label, style))
                    {
                        if (OnSelect != null)
                            OnSelect(Tag, item.Index);
                        if (!KeepOpen)
                            Show = false;
                    }
                    if (calculatedColCount <= 1 || indexList % calculatedColCount == calculatedColCount - 1)
                    {
                        // New row
                        boxY += ColHeight;
                        boxX = 0;
                    }
                    else
                        boxX += ColWidth + EspaceX;
                }
            }
            GUI.EndScrollView();
        }

        public void Position(ref Vector2 scrollerWindow)
        {
            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                // Get the position of the button to set the position popup near the button : same X and above
                Rect lastRect = GUILayoutUtility.GetLastRect();
                // Set popup above the button
                positionbt = new Vector2(lastRect.x - scrollerWindow.x, lastRect.y + lastRect.height - scrollerWindow.y);
            }
        }
    }
}
