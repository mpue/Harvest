using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// Editor Window for managing AudioMixerGroups across all AudioSources
/// Provides utilities to find and assign mixer groups
/// </summary>
public class AudioMixerAssignmentWindow : EditorWindow
{
    private AudioMixerGroup targetMixerGroup;
    private AudioManager.AudioCategory selectedCategory = AudioManager.AudioCategory.SFX;
    private Vector2 scrollPosition;
    private List<AudioSource> audioSourcesWithoutGroup;
    private List<AudioSource> allAudioSources;
    private bool showSourcesWithoutGroup = true;
    private bool showAllSources = false;
    private GUIStyle headerStyle;
    private GUIStyle warningStyle;
    private bool needsRefresh = true;

    [MenuItem("Tools/Audio Mixer Assignment")]
    public static void ShowWindow()
{
        var window = GetWindow<AudioMixerAssignmentWindow>("Audio Mixer Assignment");
        window.minSize = new Vector2(450, 500);
        window.Show();
    }

    void OnEnable()
  {
  RefreshAudioSources();
    }

    void OnFocus()
    {
        RefreshAudioSources();
    }

    void InitStyles()
    {
        if (headerStyle == null)
     {
  headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        }

        if (warningStyle == null)
   {
            warningStyle = new GUIStyle(EditorStyles.helpBox);
     warningStyle.normal.textColor = new Color(1f, 0.7f, 0f);
        }
    }

    void OnGUI()
    {
 InitStyles();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("AUDIO MIXER ASSIGNMENT", headerStyle);
     GUILayout.Space(10);

    DrawSeparator();

 // Quick Actions Section
        DrawQuickActionsSection();

        DrawSeparator();

  // Assignment Section
        DrawAssignmentSection();

        DrawSeparator();

        // AudioSources Overview
     DrawAudioSourcesOverview();

      EditorGUILayout.EndScrollView();
    }

    void DrawQuickActionsSection()
    {
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

  // Refresh button
        if (GUILayout.Button("Refresh AudioSources List", GUILayout.Height(30)))
   {
    RefreshAudioSources();
        }

        GUILayout.Space(5);

        // Info display
 if (audioSourcesWithoutGroup != null && allAudioSources != null)
        {
            if (audioSourcesWithoutGroup.Count > 0)
      {
      EditorGUILayout.HelpBox(
    $"Found {audioSourcesWithoutGroup.Count} AudioSource(s) without MixerGroup assignment.\n" +
    $"Total AudioSources in scene: {allAudioSources.Count}",
      MessageType.Warning);
       }
            else
            {
        EditorGUILayout.HelpBox(
      $"All {allAudioSources.Count} AudioSource(s) have MixerGroup assignments.",
         MessageType.Info);
            }
        }

  EditorGUILayout.EndVertical();
    }

    void DrawAssignmentSection()
    {
        EditorGUILayout.LabelField("Assign MixerGroups", EditorStyles.boldLabel);
  EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Method 1: Use AudioManager categories
        EditorGUILayout.LabelField("Method 1: Use AudioManager Categories", EditorStyles.miniBoldLabel);
        
        if (AudioManager.Instance == null)
        {
       EditorGUILayout.HelpBox(
     "No AudioManager found in scene. Create one to use category-based assignment.",
           MessageType.Warning);
            
            if (GUILayout.Button("Create AudioManager", GUILayout.Height(25)))
   {
        CreateAudioManager();
    }
        }
        else
     {
        selectedCategory = (AudioManager.AudioCategory)EditorGUILayout.EnumPopup("Category", selectedCategory);

 GUILayout.BeginHorizontal();
      
            if (GUILayout.Button("Assign to Sources Without Group", GUILayout.Height(25)))
      {
      AssignCategoryToSourcesWithoutGroup();
    }

     if (GUILayout.Button("Assign to ALL Sources", GUILayout.Height(25)))
    {
     if (EditorUtility.DisplayDialog("Assign to All", 
                    "This will assign the selected category to ALL AudioSources, overwriting existing assignments. Continue?", 
     "Yes", "Cancel"))
    {
           AssignCategoryToAllSources();
            }
       }

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        // Method 2: Direct mixer group assignment
 EditorGUILayout.LabelField("Method 2: Direct MixerGroup Assignment", EditorStyles.miniBoldLabel);
        targetMixerGroup = (AudioMixerGroup)EditorGUILayout.ObjectField("MixerGroup", targetMixerGroup, typeof(AudioMixerGroup), false);

    if (targetMixerGroup != null)
        {
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Assign to Sources Without Group", GUILayout.Height(25)))
            {
       AssignMixerGroupToSourcesWithoutGroup();
  }

  if (GUILayout.Button("Assign to ALL Sources", GUILayout.Height(25)))
            {
  if (EditorUtility.DisplayDialog("Assign to All", 
          $"This will assign '{targetMixerGroup.name}' to ALL AudioSources, overwriting existing assignments. Continue?", 
          "Yes", "Cancel"))
           {
         AssignMixerGroupToAllSources();
     }
      }

          GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a MixerGroup to enable assignment.", MessageType.Info);
    }

        EditorGUILayout.EndVertical();
    }

    void DrawAudioSourcesOverview()
    {
   EditorGUILayout.LabelField("AudioSources Overview", EditorStyles.boldLabel);

        // Sources without group
        showSourcesWithoutGroup = EditorGUILayout.Foldout(showSourcesWithoutGroup, 
 $"AudioSources Without MixerGroup ({(audioSourcesWithoutGroup != null ? audioSourcesWithoutGroup.Count : 0)})", true);
        
        if (showSourcesWithoutGroup && audioSourcesWithoutGroup != null && audioSourcesWithoutGroup.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
     
            foreach (AudioSource source in audioSourcesWithoutGroup)
          {
     if (source == null) continue;

         EditorGUILayout.BeginHorizontal();
  
       EditorGUILayout.ObjectField(source.gameObject, typeof(GameObject), true);
         
       if (GUILayout.Button("Select", GUILayout.Width(60)))
    {
   Selection.activeGameObject = source.gameObject;
    EditorGUIUtility.PingObject(source.gameObject);
    }

    if (targetMixerGroup != null)
    {
         if (GUILayout.Button("Assign", GUILayout.Width(60)))
    {
    Undo.RecordObject(source, "Assign MixerGroup");
            source.outputAudioMixerGroup = targetMixerGroup;
         EditorUtility.SetDirty(source);
  RefreshAudioSources();
   }
       }

         EditorGUILayout.EndHorizontal();
            }

         EditorGUILayout.EndVertical();
        }

    GUILayout.Space(5);

        // All sources
     showAllSources = EditorGUILayout.Foldout(showAllSources, 
   $"All AudioSources ({(allAudioSources != null ? allAudioSources.Count : 0)})", true);
    
   if (showAllSources && allAudioSources != null && allAudioSources.Count > 0)
        {
  EditorGUILayout.BeginVertical(EditorStyles.helpBox);
  
          foreach (AudioSource source in allAudioSources)
       {
                if (source == null) continue;

EditorGUILayout.BeginHorizontal();
   
     EditorGUILayout.ObjectField(source.gameObject, typeof(GameObject), true);
    
   string groupName = source.outputAudioMixerGroup != null ? source.outputAudioMixerGroup.name : "None";
                EditorGUILayout.LabelField(groupName, GUILayout.Width(100));
         
      if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
Selection.activeGameObject = source.gameObject;
     EditorGUIUtility.PingObject(source.gameObject);
             }

      EditorGUILayout.EndHorizontal();
            }

    EditorGUILayout.EndVertical();
        }
    }

    void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);
    }

    void RefreshAudioSources()
    {
        allAudioSources = new List<AudioSource>(FindObjectsOfType<AudioSource>());
     audioSourcesWithoutGroup = new List<AudioSource>();

        foreach (AudioSource source in allAudioSources)
        {
   if (source != null && source.outputAudioMixerGroup == null)
      {
audioSourcesWithoutGroup.Add(source);
  }
        }

        needsRefresh = false;
        Repaint();
    }

    void CreateAudioManager()
    {
   GameObject audioManagerObj = new GameObject("AudioManager");
        audioManagerObj.AddComponent<AudioManager>();
        Selection.activeGameObject = audioManagerObj;
     EditorGUIUtility.PingObject(audioManagerObj);
        
        EditorUtility.DisplayDialog("AudioManager Created", 
       "AudioManager has been created. Please assign AudioMixer and MixerGroups in the Inspector.", 
            "OK");
    }

  void AssignCategoryToSourcesWithoutGroup()
    {
      if (AudioManager.Instance == null)
        {
      EditorUtility.DisplayDialog("Error", "No AudioManager found in scene.", "OK");
       return;
        }

        AudioMixerGroup mixerGroup = AudioManager.Instance.GetMixerGroup(selectedCategory);
        
        if (mixerGroup == null)
        {
    EditorUtility.DisplayDialog("Error", 
 $"No MixerGroup assigned for category '{selectedCategory}' in AudioManager.", 
          "OK");
            return;
   }

        int count = 0;
        foreach (AudioSource source in audioSourcesWithoutGroup)
        {
            if (source != null)
            {
      Undo.RecordObject(source, "Assign MixerGroup");
 source.outputAudioMixerGroup = mixerGroup;
     EditorUtility.SetDirty(source);
 count++;
    }
        }

        RefreshAudioSources();
        EditorUtility.DisplayDialog("Assignment Complete", 
            $"Assigned '{mixerGroup.name}' to {count} AudioSource(s).", 
            "OK");
    }

    void AssignCategoryToAllSources()
    {
        if (AudioManager.Instance == null) return;

        AudioMixerGroup mixerGroup = AudioManager.Instance.GetMixerGroup(selectedCategory);
        if (mixerGroup == null) return;

        int count = 0;
        foreach (AudioSource source in allAudioSources)
   {
   if (source != null)
     {
          Undo.RecordObject(source, "Assign MixerGroup");
  source.outputAudioMixerGroup = mixerGroup;
                EditorUtility.SetDirty(source);
   count++;
 }
 }

    RefreshAudioSources();
        EditorUtility.DisplayDialog("Assignment Complete", 
            $"Assigned '{mixerGroup.name}' to {count} AudioSource(s).", 
     "OK");
    }

    void AssignMixerGroupToSourcesWithoutGroup()
    {
        if (targetMixerGroup == null) return;

        int count = 0;
     foreach (AudioSource source in audioSourcesWithoutGroup)
        {
 if (source != null)
            {
          Undo.RecordObject(source, "Assign MixerGroup");
       source.outputAudioMixerGroup = targetMixerGroup;
EditorUtility.SetDirty(source);
     count++;
  }
        }

        RefreshAudioSources();
        EditorUtility.DisplayDialog("Assignment Complete", 
            $"Assigned '{targetMixerGroup.name}' to {count} AudioSource(s).", 
        "OK");
    }

    void AssignMixerGroupToAllSources()
    {
        if (targetMixerGroup == null) return;

        int count = 0;
        foreach (AudioSource source in allAudioSources)
        {
            if (source != null)
        {
          Undo.RecordObject(source, "Assign MixerGroup");
    source.outputAudioMixerGroup = targetMixerGroup;
                EditorUtility.SetDirty(source);
                count++;
            }
        }

        RefreshAudioSources();
        EditorUtility.DisplayDialog("Assignment Complete", 
        $"Assigned '{targetMixerGroup.name}' to {count} AudioSource(s).", 
 "OK");
    }
}
