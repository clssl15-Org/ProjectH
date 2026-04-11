public enum CooldownType
{
    None,
    Skill,
    Dash,
}
public interface ISkillCoolDownTimer
{
    public CooldownType CooldownType { get; set; }
}