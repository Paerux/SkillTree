using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Tree", menuName = "Paerux/Skill Tree/Skill Tree")]
public class SkillTree : ScriptableObject
{
    public List<Skill> skills = new List<Skill>();
    public int pointsGiven = 0;
    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public Skill connectionStartSkill;
    [HideInInspector] public Vector2 lineEndPosition;
    public void StartConnectionFrom(Skill skill, Vector2 position)
    {
        connectionStartSkill = skill;
        lineEndPosition = position;
    }
#endif
    #endregion
}
