using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.StateMachines.Scp
{
    public enum CheckType
    {
        All,
        Any,
    }

    public class PlayCondition
    {
        // Front
        public CTC[] Conditions { get; }
        public CheckType CheckType { get; private set; } = CheckType.All;

        // Property
        internal IClip Clip { get; set; }

        // Content
        public PlayCondition(params IClip[] clips)
        {
            Conditions = clips
                .Select(c => new CTC(c))
                .ToArray();
        }
        public PlayCondition(params CTC[] conditions)
        {
            Conditions = conditions;
        }

        public PlayCondition SetCheckType(CheckType checkType)
        {
            CheckType = checkType;
            return this;
        }

        internal bool CanPlay(ClipToken currentToken, IEnumerable<ClipToken> accumulatedTokens, out IList<ClipToken> hits)
        {
            if (!Conditions.Any(cond => cond.TargetClip == currentToken.Clip))
            {
                hits = Array.Empty<ClipToken>();
                return false;
            }

            hits = new List<ClipToken>();

            if (CheckType == CheckType.All)
            {
                foreach (var condition in Conditions)
                {
                    var succeeded = false;

                    foreach (var token in accumulatedTokens)
                        if (condition.Correspond(token))
                        {
                            hits.Add(token);
                            succeeded = true;
                            break;
                        }

                    if (!succeeded)
                    {
                        hits.Clear();
                        return false;
                    }
                }

                return true;
            }
            else if (CheckType == CheckType.Any)
            {
                foreach (var condition in Conditions)
                {
                    foreach (var token in accumulatedTokens)
                        if (condition.Correspond(token))
                        {
                            hits.Add(token);
                            return true;
                        }
                }

                return false;
            }
            else
                throw new InvalidOperationException($"Unknown CheckType '{CheckType}' detected.");
        }
    }
}
