using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Skill : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public string description;
    public int pointCap = 1;
    public int skillPointsRequired;
    public List<Skill> parentSkills;
    public List<Skill> childSkills;

    public int pointsGiven = 0;
    public bool maxedOut = false;

    public static List<Skill> selectedSkills = new List<Skill>();
    public static Skill lastSelectedSkill = null;
    public SkillTree ownerSkillTree;

    public bool IsMainSkill
    {
        get
        {
            return parentSkills.Count == 0;
        }
    }

    public void AddPoint()
    {
        if (CanAddPoint())
        {
            pointsGiven++;
            ownerSkillTree.pointsGiven++;
            if (pointsGiven >= pointCap)
            {
                pointsGiven = pointCap;
                maxedOut = true;
            }
        }
    }

    public bool CanAddPoint()
    {
        bool parentsMaxedOut = true;
        foreach (Skill parent in parentSkills)
        {
            if (parent.pointsGiven < parent.pointCap)
            {
                parentsMaxedOut = false;
                break;
            }
        }

        bool requiredPoints = true;

        if(ownerSkillTree.pointsGiven < skillPointsRequired)
        {
            requiredPoints = false;
        }

        return requiredPoints && parentsMaxedOut;
    }

    public void RemovePoint()
    {
        pointsGiven--;
        ownerSkillTree.pointsGiven--;
        maxedOut = false;
        if (pointsGiven <= 0)
        {
            pointsGiven = 0;
        }
        if(ownerSkillTree.pointsGiven<=0)
        {
            ownerSkillTree.pointsGiven = 0;
        }

        foreach (Skill skill in childSkills)
        {
            RemovePointFromChildren(skill);
        }
    }

    private void RemovePointFromChildren(Skill child)
    {
        child.pointsGiven = 0;
        child.maxedOut = false;
        foreach (Skill skill in child.childSkills)
        {
            RemovePointFromChildren(skill);
        }
    }


    #region Editor Code
#if UNITY_EDITOR
    public Rect rect;
    public bool selected = false;
    public bool dragging = false;

    public void Initialize(Rect rect, SkillTree ownerSkillTree)
    {
        name = "Skill";
        skillName = "New Skill";
        childSkills = new List<Skill>();
        parentSkills = new List<Skill>();
        this.rect = rect;
        this.ownerSkillTree = ownerSkillTree;
    }

    public bool AddChild(Skill child)
    {
        if(IsChildValid(child))
        {
            childSkills.Add(child);
            return true;
        }
        return false;
    }

    public bool AddParent(Skill parent)
    {
        if (!parentSkills.Contains(parent))
        {
            parentSkills.Add(parent);
            return true;
        }
        return false;
    }

    public bool RemoveChild(Skill child)
    {
        if(childSkills.Contains(child))
        {
            childSkills.Remove(child);
            return true;
        }
        return false;
    }

    public bool RemoveParent(Skill parent)
    {
        if(parentSkills.Contains(parent))
        {
            parentSkills.Remove(parent);
            return true;
        }
        return false;
    }

    private bool IsChildValid(Skill child)
    {
        if (childSkills.Contains(child))
            return false;

        if (child == this)
            return false;

        if (IsAncestor(child))
            return false;

        return true;
    }

    private bool IsAncestor(Skill skill)
    {
        foreach(Skill parent in parentSkills)
        {
            if(parent == skill || parent.IsAncestor(skill))
            {
                return true;
            }
        }
        return false;
    }

    public void Draw(GUIStyle style)
    {
        GUILayout.BeginArea(rect, style);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label(skillName, style);
        GUILayout.FlexibleSpace();
        Rect iconRect = GUILayoutUtility.GetRect(50, 50);
        if (icon != null)
        {
            GUI.DrawTexture(iconRect, icon.texture, ScaleMode.ScaleToFit);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);
        GUILayout.EndArea();
    }

    public void ProcessEvents(Event e)
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
            LeftClickDown(e);
        }
        else if(e.button == 1)
        {
            RightClickDown(e);
        }
    }

    private void LeftClickDown(Event e)
    {
        Selection.activeObject = this;
        selected = !selected;
        if(!e.shift)
        {
            selectedSkills.Clear();
            selectedSkills.Add(this);
            lastSelectedSkill = this;
        }
        else
        {
            selectedSkills.Add(this);
        }

        foreach(Skill skill in ownerSkillTree.skills)
        {
            if (!selectedSkills.Contains(skill))
                skill.selected = false;
        }
        GUI.changed = true;
    }

    private void RightClickDown(Event e)
    {
        ownerSkillTree.StartConnectionFrom(this, e.mousePosition);
    }

    private void MouseUp(Event e)
    {
        if(e.button == 0)
        {
            LeftClickUp(e);
        }
    }

    private void LeftClickUp(Event e)
    {
        if (dragging)
            dragging = false;
    }

    private void MouseDrag(Event e)
    {
        if(e.button == 0)
        {
            LeftClickDrag(e);
        }
    }

    private void LeftClickDrag(Event e)
    {
        dragging = true;
        DragSkill(e);
        GUI.changed = true;
    }

    public void DragSkill(Event e)
    {
        rect.position += e.delta;
        EditorUtility.SetDirty(this);
    }
#endif
    #endregion
}
