using System;

namespace Infrastructure.StateMachines.Scp
{
    public class ClipToken
    {
        public IClip Clip { get; }
        public bool Completed { get; private set; }
        public object Payload { get; }
        public object Version { get; private set; }

        internal ClipToken(IClip clip, object payload = null)
        {
            Clip = clip;
            Payload = payload;
            Completed = true;
        }

        internal ClipToken With(object version, bool complete) =>
            new ClipToken(Clip, Payload)
            {
                Version = version,
                Completed = complete,
            };
    }

    public class CTC
    {
        // Front
        public IClip TargetClip { get; }

        // Internal
        private bool _completedOnly = false;
        private bool _comparePayload = false;
        private bool _compareVersion = false;

        private object _targetPayload;
        private object _targetVersion;

        private Func<object, object, bool> _payloadComparer;
        private Func<object, object, bool> _versionComparer;


        // Content
        public CTC(IClip clip) => TargetClip = clip;

        public CTC WithPayload(object payload = null, Func<object, object, bool> comparer = null)
        {
            _comparePayload = true;
            _targetPayload = payload;
            _payloadComparer = comparer ?? ((a, b) => Equals(a, b));

            return this;
        }

        public CTC WithVersion(object version = null, Func<object, object, bool> comparer = null)
        {
            _compareVersion = true;
            _targetVersion = version;
            _versionComparer = comparer ?? ((a, b) => Equals(a, b));

            return this;
        }

        public CTC CompletedOnly()
        {
            _completedOnly = true;
            return this;
        }

        public bool Correspond(ClipToken target)
        {
            if (target.Clip != TargetClip)
                return false;

            if (_completedOnly && !target.Completed)
                return false;

            if (_comparePayload && !_payloadComparer(_targetPayload, target.Payload))
                return false;

            if (_compareVersion && !_versionComparer(_targetVersion, target.Version))
                return false;

            return true;
        }
    }
}
