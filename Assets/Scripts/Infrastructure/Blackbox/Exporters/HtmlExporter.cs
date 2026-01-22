using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BlackboxSystem.Exporters
{
    internal static class HtmlExporter
    {
        private static readonly string[] _rainbowColors;
        private static readonly string[] _lightRainbowColors;

        static HtmlExporter()
        {
            // [Rainbow Palette] 
            _rainbowColors = new[] { "#ffd700", "#da70d6", "#179fff", "#32cd32", "#ff4500" };

            // [Light Palette]
            _lightRainbowColors = new[] { "#e6dec3", "#dec3e6", "#c3dee6", "#c3e6c6", "#e6c3c3" };
        }

        public static void Export(Blackbox blackbox, int recursionDepth, bool isCrash, bool fullExport, bool openLog)
        {
            if (string.IsNullOrWhiteSpace(Infrastructure.LogDirectory))
                throw new InvalidOperationException($"[HtmlExporter] LogDirectory is empty.");

            Directory.CreateDirectory(Infrastructure.LogDirectory);

            var sb = new StringBuilder();
            var visited = new HashSet<Blackbox>();

            // 1. HTML Header & CSS
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
            sb.AppendLine("<title>Blackbox Log Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(":root { --bg: #1e1e1e; --fg: #d4d4d4; --acc: #3794ff; --border: #333; }");
            sb.AppendLine("body { font-family: 'Consolas', 'Monaco', monospace; background: var(--bg); color: var(--fg); font-size: 13px; line-height: 1.5; margin: 0; padding: 20px; }");

            // Box Style
            sb.AppendLine(".box { border: 1px solid var(--border); margin-bottom: 30px; background: #252526; border-radius: 6px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.3); }");
            sb.AppendLine(".header { background: #333; padding: 10px 15px; font-weight: bold; color: #fff; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid var(--border); position: sticky; top: 0; z-index: 10; }");
            sb.AppendLine(".header .id-badge { background: #007acc; padding: 2px 6px; border-radius: 4px; font-size: 0.9em; margin-right: 10px; }");

            // [Fixed] Highlight Animation
            // Use 'forwards' to ensure the animation completes smoothly
            sb.AppendLine("@keyframes highlight-pulse {");
            sb.AppendLine("    0% { background-color: rgba(255, 215, 0, 0.50); }");
            sb.AppendLine("    100% { background-color: transparent; }");
            sb.AppendLine("}");
            // Disable transition during animation to prevent conflicts
            sb.AppendLine(".log-row.highlight { animation: highlight-pulse 1.4s ease-out forwards; transition: none !important; z-index: 1; position: relative; }");

            // Header Highlight Fallback
            sb.AppendLine("@keyframes highlight-header {");
            sb.AppendLine("    0% { background-color: rgba(255, 215, 0, 0.50); }");
            sb.AppendLine("    100% { background-color: #333; }");
            sb.AppendLine("}");
            sb.AppendLine(".header.highlight { animation: highlight-header 1.4s ease-out forwards; transition: none !important; }");

            // Log Row Layout
            sb.AppendLine(".log-container { padding: 10px 0; }");
            sb.AppendLine(".log-row { display: flex; padding: 2px 0; cursor: pointer; transition: background 0.1s; align-items: flex-start; }");
            sb.AppendLine(".log-row:hover { background: #2a2d2e; }");
            sb.AppendLine(".log-row.hidden { display: none; }");

            // Folded Indicator
            sb.AppendLine(".log-row.folded .content::after { content: ' ... '; background: #444; color: #fff; padding: 0 6px; border-radius: 4px; margin-left: 10px; font-size: 0.8em; display: inline-block; }");

            // Columns
            sb.AppendLine(".time { color: #666; width: 110px; min-width: 110px; padding-left: 15px; text-align: left; user-select: none; }");
            sb.AppendLine(".content { flex-grow: 1; padding-right: 15px; white-space: pre-wrap; word-break: break-all; position: relative; }");

            // Colors & Tags
            sb.AppendLine(".method { font-weight: bold; }");
            sb.AppendLine(".interaction { color: #4ec9b0; background: #1e3a3a; padding: 1px 4px; border-radius: 3px; text-decoration: none; border: 1px solid #2b5656; font-size: 0.9em; margin-left: 8px; cursor: pointer; display: inline-block; }");
            sb.AppendLine(".interaction:hover { border-color: #4ec9b0; background: #254444; }");

            sb.AppendLine("</style></head><body>");

            // 2. Recursive Generation
            BuildHtmlRecursive(blackbox, 0, recursionDepth >= 0 ? recursionDepth : int.MaxValue, fullExport, null, visited, sb);

            // 3. JavaScript
            sb.AppendLine("<script>");
            sb.AppendLine(@"
    const highlightTimers = new WeakMap();

    function setFold(openRow, fold) {
        if (!openRow) return;

        const startDepth = parseInt(openRow.getAttribute('data-depth')) || 0;

        if (fold) openRow.classList.add('folded');
        else openRow.classList.remove('folded');

        let next = openRow.nextElementSibling;
        while (next) {
            const d = parseInt(next.getAttribute('data-depth')) || 0;
            const t = next.getAttribute('data-type');

            if (d > startDepth) {
                if (fold) next.classList.add('hidden');
                else {
                    next.classList.remove('hidden');
                    next.classList.remove('folded');
                }
            }
            else if (d === startDepth && t === 'Close') {
                if (fold) next.classList.add('hidden');
                else next.classList.remove('hidden');
                break;
            }
            else break;

            next = next.nextElementSibling;
        }
    }

    function ensureVisible(row) {
        if (!row) return;

        // If the row is an Open row and currently folded, unfold it.
        if (row.classList.contains('folded') && row.getAttribute('data-type') === 'Open') {
            setFold(row, false);
        }

        let depth = parseInt(row.getAttribute('data-depth')) || 0;
        let cursor = row.previousElementSibling;

        // Unfold all ancestor scopes that are currently folded.
        // We detect ancestors by scanning backwards for Open rows with smaller depth.
        let safety = 0;
        while (cursor && safety++ < 50000) {
            const cDepth = parseInt(cursor.getAttribute('data-depth')) || 0;
            const cType = cursor.getAttribute('data-type');

            if (cType === 'Open' && cDepth < depth) {
                if (cursor.classList.contains('folded')) {
                    setFold(cursor, false);
                }
                depth = cDepth;
                if (depth <= 0) break;
            }

            cursor = cursor.previousElementSibling;
        }

        row.classList.remove('hidden');
    }

    function flash(el, cls, durationMs) {
        if (!el) return;

        const prev = highlightTimers.get(el);
        if (prev) clearTimeout(prev);

        if (el.classList && el.classList.contains('log-row')) {
            ensureVisible(el);
        }

        // Restart CSS animation reliably (remove -> reflow -> add)
        el.classList.remove(cls);
        void el.offsetWidth;
        el.classList.add(cls);

        highlightTimers.set(el, setTimeout(() => {
            el.classList.remove(cls);
            highlightTimers.delete(el);
        }, durationMs));
    }

    // Interaction Link Click Handler + Folding Logic
    document.addEventListener('click', function(e) {
        const link = e.target.closest('a.interaction');
        if (link) {
            e.preventDefault();

            const href = link.getAttribute('href');
            if (href && href.startsWith('#')) {
                const targetId = href.substring(1);
                const targetElement = document.getElementById(targetId);

                // Strategy A: Target Row Found
                if (targetElement) {
                    ensureVisible(targetElement);
                    targetElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    flash(targetElement, 'highlight', 1400);
                }
                // Strategy B: Target Row NOT Found (Fallback to Box)
                else {
                    const parts = targetId.split('_');
                    let boxId = null;

                    if (parts.length >= 3 && parts[0] === 'log') {
                        boxId = 'b' + parts[1];
                    } else if (targetId.startsWith('b')) {
                        boxId = targetId;
                    }

                    if (boxId) {
                        const boxElem = document.getElementById(boxId);
                        if (boxElem) {
                            const header = boxElem.querySelector('.header');
                            if (header) {
                                header.scrollIntoView({ behavior: 'smooth', block: 'center' });
                                flash(header, 'highlight', 1400);
                            } else {
                                boxElem.scrollIntoView({ behavior: 'smooth', block: 'start' });
                            }
                        } else {
                            console.warn('Target Box not found: ' + boxId);
                        }
                    }
                }
            }
            return;
        }

        // Folding Logic
        let row = e.target.closest('.log-row');
        if (!row) return;

        let type = row.getAttribute('data-type');
        const startDepth = parseInt(row.getAttribute('data-depth')) || 0;

        // Support clicking 'Close' tag to fold: find matching Open at same depth.
        if (type === 'Close') {
            let prev = row.previousElementSibling;
            while (prev) {
                const prevDepth = parseInt(prev.getAttribute('data-depth')) || 0;
                const prevType = prev.getAttribute('data-type');

                if (prevDepth === startDepth && prevType === 'Open') {
                    row = prev;
                    type = 'Open';
                    break;
                }
                if (prevDepth < startDepth) break;

                prev = prev.previousElementSibling;
            }
        }

        if (type !== 'Open') return;

        const nextSibling = row.nextElementSibling;
        if (!nextSibling) return;

        const nextDepth = parseInt(nextSibling.getAttribute('data-depth')) || 0;
        if (nextDepth <= startDepth) return;

        const isFolding = !row.classList.contains('folded');
        setFold(row, isFolding);
    });
");
            sb.AppendLine("</script></body></html>");

            // 4. Save
            var ext = ".html";
            var fileName = $"Blackbox {Tools.TrimSmart(blackbox.OwnerString)} ({blackbox.Id}){ext}";
            if (isCrash) fileName = "[CRASH] " + fileName;

            var fullPath = Path.Combine(Infrastructure.LogDirectory, fileName);
            File.WriteAllText(fullPath, sb.ToString());

            Infrastructure.Log($"[HtmlExporter] Exported to '{fullPath}'");

            if (openLog)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Infrastructure.Log(
                        $"[HtmlExporter] Failed to open the file automatically.\n{ex.ToString()}",
                        LogLevel.Warning);
                }
            }
        }

        private static void BuildHtmlRecursive(Blackbox current, int depth, int maxDepth, bool fullExport, Blackbox parent, HashSet<Blackbox> visited, StringBuilder sb)
        {
            if (current == null || visited.Contains(current)) return;
            visited.Add(current);

            var logs = current.GetLogs().ToList();

            sb.AppendLine($"<div class='box' id='b{current.Id}'>");
            var parentInfo = parent != null ? $" <span style='color:#888'>&larr; Called by <a href='#b{parent.Id}' style='color:#888'>#{parent.Id}</a></span>" : "";
            sb.AppendLine($"<div class='header'><div><span class='id-badge'>#{current.Id}</span> {HtmlEncode(current.OwnerString)}{parentInfo}</div></div>");
            sb.AppendLine("<div class='log-container'>");

            foreach (var log in logs)
            {
                var indentPx = log.ScopeDepth * 20;
                var contentStyle = $"padding-left: {indentPx}px";

                var scopeColor = _rainbowColors[log.ScopeDepth % _rainbowColors.Length];
                var messageColor = _lightRainbowColors[(log.ScopeDepth + _rainbowColors.Length - 1) % _rainbowColors.Length];

                var rowIdAttr = log.InteractionId >= 0 ? $"id='log_{current.Id}_{log.InteractionId}'" : "";

                sb.Append($"<div class='log-row' {rowIdAttr} data-depth='{log.ScopeDepth}' data-type='{log.ScopeType}'>");
                sb.Append($"<div class='time'>{log.Time:mm:ss.ffffff}</div>");
                sb.Append($"<div class='content' style='{contentStyle}'>");

                // Tags
                if (log.ScopeType == ScopeType.Open)
                    sb.Append($"<span style='color:{scopeColor}'>&lt;{log.MethodName}&gt;</span> ");
                else if (log.ScopeType == ScopeType.Close)
                    sb.Append($"<span style='color:{scopeColor}'>&lt;/{log.MethodName}&gt;</span> ");
                else if (!string.IsNullOrEmpty(log.MethodName))
                    sb.Append($"<span class='method' style='color:{messageColor}'>[{log.MethodName}]</span> ");

                // Message FIRST
                sb.Append($"{HtmlEncode(log.Message)}");

                // Interaction Links LATER
                if (log.ExertedBy != null)
                {
                    var peerId = log.ExertedBy.Id;
                    var peerName = HtmlEncode(log.ExertedBy.OwnerString);
                    var href = log.InteractionId >= 0 ? $"#log_{peerId}_{log.InteractionId}" : $"#b{peerId}";
                    var arrow = HtmlEncode(GetArrowText(false, log.InteractionId));
                    sb.Append($"<a href='{href}' class='interaction' title='Interaction #{log.InteractionId}'>{arrow} #{peerId}: {peerName}</a> ");
                }

                if (log.ExertingTo != null)
                {
                    var peerId = log.ExertingTo.Id;
                    var peerName = HtmlEncode(log.ExertingTo.OwnerString);
                    var href = log.InteractionId >= 0 ? $"#log_{peerId}_{log.InteractionId}" : $"#b{peerId}";
                    var arrow = HtmlEncode(GetArrowText(true, log.InteractionId));
                    sb.Append($"<a href='{href}' class='interaction' title='Interaction #{log.InteractionId}'>{arrow} #{peerId}: {peerName}</a> ");
                }

                sb.AppendLine("</div></div>");
            }

            sb.AppendLine("</div></div>");

            if (depth < maxDepth)
            {
                var peers = new HashSet<Blackbox>();

                foreach (var log in logs)
                {
                    if (log.ExertedBy != null && log.ExertedBy != current)
                        peers.Add(log.ExertedBy);
                    if (fullExport && log.ExertingTo != null && log.ExertingTo != current)
                        peers.Add(log.ExertingTo);
                }

                foreach (var peer in peers)
                    BuildHtmlRecursive(peer, depth + 1, maxDepth, fullExport, current, visited, sb);
            }
        }

        private static string HtmlEncode(string text) => System.Net.WebUtility.HtmlEncode(text);

        private static string GetArrowText(bool right, long interactionId)
        {
            if (interactionId >= 0)
                return right ? $"-[{interactionId}]->" : $"<-[{interactionId}]-";
            else
                return right ? "->" : "<-";
        }
    }
}
