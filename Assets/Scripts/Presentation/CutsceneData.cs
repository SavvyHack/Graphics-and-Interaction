using System;
using System.Collections.Generic;
using UnityEngine;

public enum CutsceneTransition { HardCut, SlideIn }

/// <summary>Panels for the intro cutscene. Edit art, captions and transitions in the Inspector.</summary>
[CreateAssetMenu(fileName = "IntroCutsceneData", menuName = "Project R.A.T./Cutscene Data")]
public class CutsceneData : ScriptableObject
{
    [Serializable]
    public class Panel
    {
        [Tooltip("Picture shown in the top 75% of the screen. Author at 1920x810.")]
        public Sprite image;
        [TextArea(2, 5)] public string caption;
        [Tooltip("How the picture arrives. The caption always swaps instantly.")]
        public CutsceneTransition transition = CutsceneTransition.HardCut;
    }

    public List<Panel> panels = new List<Panel>();
}
