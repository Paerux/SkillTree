using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SkillTooltip : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI pointsText;

    private Skill currentSkill;

    public void SetTooltip(Skill skill)
    {
        currentSkill = skill;
    }

    private void Update()
    {
        titleText.text = currentSkill.name;
        descriptionText.text = currentSkill.description;
        if(currentSkill.CanAddPoint())
        {
            pointsText.text = currentSkill.pointsGiven + "/" + currentSkill.pointCap;
        }
        else
        {
            if(currentSkill.skillPointsRequired > currentSkill.ownerSkillTree.pointsGiven)
                pointsText.text = "Must spend " + (currentSkill.skillPointsRequired - currentSkill.ownerSkillTree.pointsGiven) +" more points.";
            else
                pointsText.text = pointsText.text = "Must unlock parent spells";
        }
    }
}
