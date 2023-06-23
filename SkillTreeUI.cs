using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    [Header("References")]
    public SkillTree skillTree;
    public Transform contentParent;
    public Transform lineParent;
    public GameObject skillPrefab;
    public SkillTooltip skillTooltip;
    public Sprite lineImage;

    private RectTransform rt;

    [Header("Tree Settings")]
    public float skillHorizontalSpacing = 50f;
    public float skillVerticalSpacing = 50f;

    private List<Skill> renderedSkills = new List<Skill>();

    private void Awake()
    {
        rt = gameObject.GetComponent<RectTransform>();
    }

    private void Start()
    {
        PopulateTree();
        foreach(SkillButton button1 in contentParent.GetComponentsInChildren<SkillButton>())
        {
            foreach (SkillButton button2 in contentParent.GetComponentsInChildren<SkillButton>())
            {
                if(button2.skill.parentSkills.Contains(button1.skill))
                {
                    MakeLine(button1.GetComponent<RectTransform>().anchoredPosition.x,
                             button1.GetComponent<RectTransform>().anchoredPosition.y,
                             button2.GetComponent<RectTransform>().anchoredPosition.x,
                             button2.GetComponent<RectTransform>().anchoredPosition.y,
                             Color.white);
                }
            }
        }
    }

    private void PopulateTree()
    {
        List<Skill> mainSkills = new List<Skill>();
        foreach(Skill skill in skillTree.skills)
        {
            if(skill.parentSkills.Count == 0)
                mainSkills.Add(skill);
        }

        for(int i = 0; i < mainSkills.Count; i++)
        {
            //PopulateSkill(mainSkills[i], (rt.rect.width / (mainSkills.Count + 1)) * (i+1), 0);
            PopulateSkill(mainSkills[i], (-rt.rect.width/2) + (rt.rect.width / (mainSkills.Count +1)) * (i+1), (rt.rect.height / 2) - 50f);
        }
    }

    private void PopulateSkill(Skill skill, float xPos, float yPos)
    {
        if (renderedSkills.Contains(skill))
            return;

        renderedSkills.Add(skill);
        GameObject skillUI = Instantiate(skillPrefab, contentParent);
        skillUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);
        skillUI.GetComponent<SkillButton>().InitializeIcon(skill, skillTooltip);
        
        // Calculate the total width required for child skills
        float childSkillsWidth = (skill.childSkills.Count - 1) * skillHorizontalSpacing;
        float parentSkillsWidth = skill.parentSkills.Count * skillHorizontalSpacing;

        // Calculate the starting position for child skills
        float startXPos = xPos - (childSkillsWidth * 0.5f) + (parentSkillsWidth * 0.5f);

        for (int i = 0; i < skill.childSkills.Count; i++)
        {
            // Calculate the x position for the child skill
            float childXPos = startXPos + (i * skillHorizontalSpacing);
            PopulateSkill(skill.childSkills[i], childXPos, yPos - skillVerticalSpacing);
        }
    }

    // Credits to Tom163 on unity forums
    private void MakeLine(float ax, float ay, float bx, float by, Color col)
    {
        GameObject NewObj = new GameObject();
        NewObj.name = "line from " + ax + " to " + bx;
        Image NewImage = NewObj.AddComponent<Image>();
        NewImage.sprite = lineImage;
        NewImage.color = col;
        RectTransform rect = NewObj.GetComponent<RectTransform>();
        rect.SetParent(lineParent);
        rect.localScale = Vector3.one;
        Vector3 a = new Vector3(ax, ay, 0);
        Vector3 b = new Vector3(bx, by, 0);
        rect.localPosition = (a + b) / 2;
        Vector3 dif = a - b;
        rect.sizeDelta = new Vector3(dif.magnitude, 3f);
        rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
    }
}
