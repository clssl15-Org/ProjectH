using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public interface IInputLayerHub
    {
        enum RemoveOption
        {
            None,
            RemoveIfEmpty,
            Forced,
        }

        void Add(IInputLayerSubject subject, bool blockBelows = true);
        void Add(IEnumerable<IInputLayerSubject> subjects, bool blockBelows = true);
        void AddTo(string targetLayer, IInputLayerSubject subject, bool blockBelows = true);
        void AddTo(string targetLayer, IEnumerable<IInputLayerSubject> subjects, bool blockBelows = true);
        void Remove(IInputLayerSubject subject, RemoveOption remveOption = RemoveOption.RemoveIfEmpty, bool forceUnblock = false);

        void Block(object requester);
        void Unblock(object requester);
    }

    public interface IInputLayerController
    {
        void Initialize(IInputLayerHub inputHub);
    }

    public interface IInputLayerSubject
    {
        bool AllowInput { get; set; }
        event Action Destroying;
    }
}
