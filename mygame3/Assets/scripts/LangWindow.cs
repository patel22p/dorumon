using UnityEngine;
using System.Collections;

    public class LangWindow: WindowBase
    {
        public void Start()
        {
            size = new Vector2(100,100);
            title = "Select language";
            if (!build || OptionsWindow.secondrun) enabled = false;
        }
        protected override void OnGUI()
        {            
            base.OnGUI();            
        }
        
        public Texture2D rus;
        public Texture2D eng;
        protected override void Window(int i)
        {
            GUI.BringWindowToFront(id);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(eng))
            {
                OptionsWindow.ruslang = false;
                OptionsWindow.secondrun = true;
                enabled = false;
            }
            if (GUILayout.Button(rus))
            {
                OptionsWindow.ruslang = true;
                OptionsWindow.secondrun = true;
                enabled = false;
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
    }
