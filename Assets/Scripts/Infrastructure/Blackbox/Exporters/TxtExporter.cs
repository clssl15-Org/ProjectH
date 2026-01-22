using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BlackboxSystem.Exporters
{
    internal static class TxtExporter
    {
        public static void Export(Blackbox blackbox, int recursionDepth, bool isCrash, bool fullExport, bool openLog)
        {
            if (string.IsNullOrWhiteSpace(Infrastructure.LogDirectory))
                throw new InvalidOperationException($"[TxtExporter] LogDirectory is empty.");

            Directory.CreateDirectory(Infrastructure.LogDirectory);

            var fileName = $"Blackbox {Tools.TrimSmart(blackbox.OwnerString)} ({blackbox.Id}).txt";
            if (isCrash) fileName = "[CRASH] " + fileName;

            var sb = new StringBuilder();
            var visited = new HashSet<Blackbox>();

            BuildTextRecursive(blackbox, 0, recursionDepth >= 0 ? recursionDepth : int.MaxValue, fullExport, null, visited, sb);

            var result = sb.ToString();

            var fullPath = Path.Combine(Infrastructure.LogDirectory, fileName);
            File.WriteAllText(fullPath, result);

            Infrastructure.Log($"[TxtExporter] Log successfully exported to '{fullPath}'");

            if (openLog)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Infrastructure.Log(
                        $"[TxtExporter] Failed to open the file automatically.\n{ex.ToString()}",
                        LogLevel.Warning);
                }
            }
        }

        private static void BuildTextRecursive(Blackbox current, int depth, int maxDepth, bool fullExport, Blackbox parent, HashSet<Blackbox> visited, StringBuilder sb)
        {
            if (current == null || visited.Contains(current)) return;
            visited.Add(current);

            var logs = current.GetLogs().ToList();

            var fromInfo = parent != null ? $" | From = #{parent.Id}: {parent.OwnerString}" : "";
            var header = $"========= #{current.Id}: {current.OwnerString} (Depth = {depth}{fromInfo}) =========";

            sb.AppendLine(header);
            sb.AppendLine(new string('-', 40));

            var peers = new HashSet<Blackbox>();

            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];

                if (i > 0 && log.ScopeDepth == 0 && log.ScopeType == ScopeType.Open)
                    sb.AppendLine(new string('-', 40));

                if (log.ExertedBy != null && log.ExertedBy != current)
                    peers.Add(log.ExertedBy);
                if (fullExport && log.ExertingTo != null && log.ExertingTo != current)
                    peers.Add(log.ExertingTo);

                sb.AppendLine(log.ToString());
            }

            sb.AppendLine();
            sb.AppendLine();

            if (depth < maxDepth)
            {
                foreach (var peer in peers)
                    BuildTextRecursive(peer, depth + 1, maxDepth, fullExport, current, visited, sb);
            }
        }
    }
}
