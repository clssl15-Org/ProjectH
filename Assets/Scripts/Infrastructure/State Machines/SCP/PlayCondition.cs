using System;
using System.Collections.Generic;

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
        public CheckType CheckType { get; init; }

        // Property
        internal IClip Clip { get; set; }

        // Content
        public PlayCondition(CTC condition)
        {
            Conditions = new[] { condition };
            CheckType = CheckType.All;
        }

        public PlayCondition(CTC[] conditions, CheckType checkType = CheckType.All)
        {
            Conditions = conditions;
            CheckType = checkType;
        }

        internal bool CanPlay(IEnumerable<ClipToken> currentTokens, out IList<ClipToken> hits)
        {
            hits = new List<ClipToken>();

            if (CheckType == CheckType.All)
            {
                foreach (var condition in Conditions)
                {
                    var succeeded = false;

                    foreach (var current in currentTokens)
                        if (condition.Correspond(current))
                        {
                            hits.Add(current);
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
                    foreach (var current in currentTokens)
                        if (condition.Correspond(current))
                        {
                            hits.Add(current);
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
