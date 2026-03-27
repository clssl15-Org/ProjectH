namespace Actors.Monsters.Actions
{
    public enum InterruptPriority
    {
        /// <summary>
        /// 자신이 종료될 때 모든 컴포넌트를 종료합니다.
        /// </summary>
        High,
        Default,
        /// <summary>
        /// 다른 컴포넌트가 종료되면 자신도 자동으로 종료됩니다.
        /// </summary>
        Low,
    }
}
