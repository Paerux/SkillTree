using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Skill skill;
    public Image skillIcon;
    private SkillTooltip tooltip;
    public UnityEvent onSkillLeveled;
    public UnityEvent onSkillUnleveled;

    public Color activeColor;
    public Color inactiveColor;

    private RectTransform myTransform;
    private RectTransform tooltipTransform;

    public void InitializeIcon(Skill skill, SkillTooltip tooltip)
    {
        this.skill = skill;
        this.tooltip = tooltip;
        skillIcon.sprite = skill.icon;
        tooltipTransform = tooltip.GetComponent<RectTransform>();
        myTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if (skill.CanAddPoint())
            {
                skill.AddPoint();
                onSkillLeveled?.Invoke();
            }
        }
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            skill.RemovePoint();
            onSkillUnleveled?.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.gameObject.SetActive(true);
        tooltip.SetTooltip(skill);
        tooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(myTransform.anchoredPosition.x + tooltipTransform.rect.width / 2,
                                                                             myTransform.anchoredPosition.y);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (skill != null)
        {
            if (!skill.CanAddPoint())
            {
                skillIcon.color = inactiveColor;
            }
            else
            {
                skillIcon.color = activeColor;
            }
        }
    }   
}
