using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;

public class SkillTreeEditor : EditorWindow
{
    private static SkillTree currentSkillTree;
    private Skill mouseOverSkill;

    private GUIStyle skillStyle;
    private GUIStyle skillSelectedStyle;
    private GUIStyle skillEditPanelStyle;

    [Header("Visual Settings")]
    public float skillWidth = 150f;
    public float skillHeight = 75f;
    public int nodePadding = 0;
    public int nodeBorder = 0;
    public float lineWidth = 3f;
    public float lineArrowSize = 6f;
    public float gridLarge = 100f;
    public float gridSmall = 25f;

    #region Necessary Initial Stuff
    [MenuItem("Paerux/Skill Tree/Skill Tree Editor")]
    public static void OpenWindow()
    {
        SkillTreeEditor window = GetWindow<SkillTreeEditor>(currentSkillTree.name);
        window.minSize = new Vector2(500f, 500f);
        if (currentSkillTree != null)
        {
            window.titleContent = new GUIContent(currentSkillTree.name);
        }
    }

    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;
        skillStyle = Resources.Load<SkillResources>("SkillResources").skillNormalStyle;
        skillSelectedStyle = Resources.Load<SkillResources>("SkillResources").skillSelectedStyle;
        skillEditPanelStyle = Resources.Load<SkillResources>("SkillResources").skillEditPanelStyle;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void InspectorSelectionChanged()
    {
        SkillTree skillTree = Selection.activeObject as SkillTree;
        if (skillTree != null)
        {
            currentSkillTree = skillTree;
            titleContent = new GUIContent(currentSkillTree.name);
            GUI.changed = true;
        }
    }

    [OnOpenAsset(0)]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        SkillTree skillTree = EditorUtility.InstanceIDToObject(instanceId) as SkillTree;
        if(skillTree != null)
        {
            currentSkillTree = skillTree;
            OpenWindow();
            return true;
        }
        return false;
    }
    #endregion

    private void OnGUI()
    {
        if(currentSkillTree != null)
        {
            DrawDraggingLine();
            ProcessEvents(Event.current);
            DrawSkillConnections();
            DrawSkills();
            DrawSkillEditPanel();
        }

        if(GUI.changed)
        {
            Repaint();
        }
    }

    #region Process Events
    private void ProcessEvents(Event e)
    {
        // Update mouse over skill if we are not hovering over a skill or dragging one
        if(mouseOverSkill == null || !mouseOverSkill.dragging)
        {
            mouseOverSkill = GetSkillUnderMouse(e);
        }

        // If we arent hovering over a skill or we're not drawing a connection line, process graph events
        if(mouseOverSkill == null || currentSkillTree.connectionStartSkill != null) 
        {
            ProcessGraphEvents(e);
        }
        // Process skill events
        else
        {
            mouseOverSkill.ProcessEvents(e);
        }
    }
    private void ProcessGraphEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                MouseDown(e);
                break;
            case EventType.MouseUp:
                MouseUp(e);
                break;
            case EventType.MouseDrag:
                MouseDrag(e);
                break;
            default:
                break;
        }
    }
    private void MouseDown(Event e)
    {
        if(e.button == 0)
        {
            ClearLine();
            ClearAllSelectedSkills();
        }
        else if (e.button == 1)
        {
            ShowContextMenu(e.mousePosition);
        }
    }
    private void MouseUp(Event e)
    {
        // Releasing a drag line
        if(e.button == 1 && currentSkillTree.connectionStartSkill != null)
        {
            Skill connectionStartSkill = currentSkillTree.connectionStartSkill;
            Skill connectionEndSkill = GetSkillUnderMouse(e);
            if(connectionEndSkill != null)
            {
                connectionEndSkill.AddParent(connectionStartSkill);
                connectionStartSkill.AddChild(connectionEndSkill);
            }
            ClearLine();
        }
    }
    private void MouseDrag(Event e)
    {
        if(e.button == 0)
        {
            LeftClickDrag(e);
        }
        else if(e.button == 1)
        {
            RightClickDrag(e);
        }
    }
    private void LeftClickDrag(Event e)
    {
        foreach(Skill skill in currentSkillTree.skills)
        {
            skill.DragSkill(e);
        }
        GUI.changed = true;
    }
    private void RightClickDrag(Event e)
    {
        if(currentSkillTree.connectionStartSkill != null)
        {
            DragLine(e);
            GUI.changed = true;
        }
    }
    private void DragLine(Event e)
    {
        currentSkillTree.lineEndPosition += e.delta;
    }
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Skill"), false, CreateSkillContext, mousePosition);
        menu.AddItem(new GUIContent("Select All Skills"), false, SelectAllSkills);
        menu.AddItem(new GUIContent("Remove Links From Selected Skills"), false, RemoveSelectedSkillLinks);
        menu.AddItem(new GUIContent("Remove Selected Skills"), false, RemoveSelectedSkills);
        menu.ShowAsContext();
    }
    #endregion

    #region Skill
    private void CreateSkillContext(object mousePositionObject)
    {
        CreateSkill(mousePositionObject);
    }
    private Skill CreateSkill(object mousePositionObject)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;
        Skill skill = CreateInstance<Skill>();
        currentSkillTree.skills.Add(skill);
        skill.Initialize(new Rect(mousePosition, new Vector2(skillWidth, skillHeight)), currentSkillTree);
        AssetDatabase.AddObjectToAsset(skill, currentSkillTree);
        AssetDatabase.SaveAssets();
        return skill;
    }
    private void DrawSkills()
    {
        foreach(Skill skill in currentSkillTree.skills)
        {
            skill.Draw(skill.selected ? skillSelectedStyle : skillStyle);
        }
        GUI.changed = true;
    }
    private void DrawSkillConnections()
    {
        foreach(Skill skill in currentSkillTree.skills)
        {
            if(skill.childSkills.Count > 0)
            {
                foreach(Skill childSkill in skill.childSkills)
                {
                    DrawConnection(skill, childSkill);
                    GUI.changed = true;
                }
            }
        }
    }
    private void RemoveSelectedSkillLinks()
    {
        foreach(Skill skill in currentSkillTree.skills)
        {
            if(skill.selected && skill.childSkills.Count > 0)
            {
                for(int i = skill.childSkills.Count - 1; i >= 0; i--)
                {
                    Skill childSkill = skill.childSkills[i];
                    if(childSkill != null &&  childSkill.selected)
                    {
                        skill.childSkills.Remove(childSkill);
                        childSkill.parentSkills = null;
                    }
                }
            }
        }
        ClearAllSelectedSkills();
    }
    private void RemoveSelectedSkills()
    {
        Queue<Skill> removeQueue = new Queue<Skill>();
        foreach(Skill skill in currentSkillTree.skills)
        {
            if(skill.selected)
            {
                removeQueue.Enqueue(skill);
                foreach(Skill childSkill in skill.childSkills)
                {
                    childSkill.RemoveParent(skill);
                }

                foreach (Skill parentSkill in skill.parentSkills)
                {
                    parentSkill.RemoveChild(skill);
                }
            }
        }

        while(removeQueue.Count > 0)
        {
            Skill skillToDelete = removeQueue.Dequeue();
            currentSkillTree.skills.Remove(skillToDelete);
            DestroyImmediate(skillToDelete,true);
            AssetDatabase.SaveAssets();
        }
    }
    #endregion

    #region Misc
    private void DrawConnection(Skill parentSkill, Skill childSkill)
    {
        Vector2 startPosition = parentSkill.rect.center;
        Vector2 endPosition = childSkill.rect.center;
        Vector2 midPosition = (startPosition + endPosition) / 2f;
        Vector2 direction = (endPosition - startPosition).normalized;

        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x) * lineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x) * lineArrowSize;
        Vector2 arrowHeadPoint = midPosition + direction * lineArrowSize;

        Handles.color = Color.white;
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, lineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, lineWidth);
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, lineWidth);

        GUI.changed = true;
    }
    private void DrawDraggingLine()
    {
        if (currentSkillTree.lineEndPosition != Vector2.zero)
        {
            Vector2 startPos = currentSkillTree.connectionStartSkill.rect.center;
            Vector2 endPos = currentSkillTree.lineEndPosition;
            Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.white, null, lineWidth);
        }
    }
    private Skill GetSkillUnderMouse(Event e)
    {
        for(int i = currentSkillTree.skills.Count - 1; i >= 0; i--)
        {
            if(currentSkillTree.skills[i].rect.Contains(e.mousePosition))
            {
                return currentSkillTree.skills[i];
            }
        }
        return null;
    }
    private void SelectAllSkills()
    {
        foreach(Skill skill in currentSkillTree.skills)
        {
            skill.selected = true;
        }
        GUI.changed = true;
    }
    private void ClearLine()
    {
        currentSkillTree.connectionStartSkill = null;
        currentSkillTree.lineEndPosition = Vector2.zero;
        GUI.changed = true;
    }
    private void ClearAllSelectedSkills()
    {
        foreach(Skill skill in currentSkillTree.skills)
        {
            skill.selected = false;
        }
        GUI.changed = true;
    }

    // Credits to alexanderameye on unity forums
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
    #endregion

    #region Skill Edit Panel
    private void DrawSkillEditPanel()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(skillEditPanelStyle, GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Edit Skill", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (Skill.lastSelectedSkill != null)
        {
            Skill.lastSelectedSkill.skillName = EditorGUILayout.TextField("Skill Name", Skill.lastSelectedSkill.skillName);
            Skill.lastSelectedSkill.name = Skill.lastSelectedSkill.skillName;
            Skill.lastSelectedSkill.description = EditorGUILayout.TextField("Description", Skill.lastSelectedSkill.description);
            Skill.lastSelectedSkill.icon = (Sprite)EditorGUILayout.ObjectField("Skill Icon",Skill.lastSelectedSkill.icon, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            Skill.lastSelectedSkill.pointCap = EditorGUILayout.IntField("Skill Points Cap", Skill.lastSelectedSkill.pointCap);
            Skill.lastSelectedSkill.skillPointsRequired = EditorGUILayout.IntField("Skill Points Required", Skill.lastSelectedSkill.skillPointsRequired);
        }
        else
        {
            EditorGUILayout.HelpBox("No skill selected.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        DrawUILine(Color.white);
    }
    #endregion
}
