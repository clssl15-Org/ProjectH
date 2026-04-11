public enum CooldownType
{
    None,
    RushStabbing,
    StrongAttack,
    Skill3,
    Dash,
}
public interface ISkillCoolDownTimer
{
    public CooldownType CooldownType { get; set; }
}
