namespace Actors.Monsters.Actions
{
    /// <summary>
    /// 아무 행동도 가지지 않는 컴포넌트입니다.
    /// </summary>
    /// <remarks>
    /// 이 컴포넌트는 Input 인자 수를 맞추기 위한 임시 방편입니다.
    /// </remarks>
    internal sealed class Empty : MonsterActionComponent
    {
        protected override void OnEnter(object _)
        {
            Interrupt(InterruptType.Completed);
        }
    }
}
