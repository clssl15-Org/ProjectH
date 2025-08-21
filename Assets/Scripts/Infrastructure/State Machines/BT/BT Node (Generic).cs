using System;

namespace UniEngine.StateMachines.BT
{
    public class BTNode<TOwner, TBlackboard> : BTNode where TOwner : class where TBlackboard : class, new()
    {
        // Front
        public new TOwner Owner => base.Owner == null ? null :
            base.Owner as TOwner ?? throw new InvalidCastException(Ctx($"Owner must be of type '{typeof(TOwner)}' but was '{base.Owner?.GetType().Name}'"));

        public new TBlackboard Blackboard => base.Blackboard == null ? null :
            base.Blackboard as TBlackboard ?? throw new InvalidCastException(Ctx($"Blackboard must be of type '{typeof(TBlackboard)}' but was '{base.Blackboard?.GetType().Name}'"));


        // Content
        public BTNode(TOwner owner = null, string name = null) : base(owner, name) { }
        protected sealed override object GetBlackboard() => new TBlackboard();
    }
}
