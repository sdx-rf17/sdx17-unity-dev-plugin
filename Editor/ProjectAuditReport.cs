using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// Counts the project's assets by type, lists the largest assets and
    /// duplicate asset names, and writes a Markdown report. The report is
    /// data only — it never deletes, moves or renames anything.
    /// </summary>
    public static class ProjectAuditReport
    {
        [MenuItem("Sdx17/Audit/Run Project Audit")]
        public static void RunAuditMenu()
        {
            AuditReport report = RunAudit();
            string path = WriteReport(report);
            Debug.Log($"[Sdx17] Project audit: {report.SummaryLine()} Report written to {path}");
        }

        /// <summary>Scans the whole project and returns the counts and findings.</summary>
        public static AuditReport RunAudit()
        {
            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            AuditReport report = new AuditReport();
            string[] guids = AssetDatabase.FindAssets(string.Empty);

            Dictionary<string, int> nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            List<AssetSizeEntry> sizedAssets = new List<AssetSizeEntry>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                report.TotalAssets++;
                string extension = Path.GetExtension(path).ToLowerInvariant();
                report.CountByExtension[extension] = report.CountByExtension.TryGetValue(extension, out int existing)
                    ? existing + 1
                    : 1;

                string fileName = Path.GetFileNameWithoutExtension(path);
                nameCounts[fileName] = nameCounts.TryGetValue(fileName, out int nameCount) ? nameCount + 1 : 1;

                if (AssetDatabase.IsValidFolder(path))
                {
                    report.FolderCount++;
                    continue;
                }

                long size = GetFileSizeBytes(path);
                report.TotalBytes += size;
                sizedAssets.Add(new AssetSizeEntry(path, size));

                if (size >= settings.LargeAssetWarningBytes)
                {
                    report.LargeAssets.Add(new AssetSizeEntry(path, size));
                }
            }

            foreach (KeyValuePair<string, int> pair in nameCounts)
            {
                if (pair.Value > 1)
                {
                    report.DuplicateNames.Add($"{pair.Key} ({pair.Value} assets share this name)");
                }
            }

            // Largest first, so the report surface the assets worth shrinking.
            sizedAssets.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));
            int take = Math.Min(10, sizedAssets.Count);
            for (int i = 0; i < take; i++)
            {
                report.LargestAssets.Add(sizedAssets[i]);
            }

            report.LargeAssetThresholdBytes = settings.LargeAssetWarningBytes;
            return report;
        }

        /// <summary>Writes the audit as Markdown to the configured path and returns that path.</summary>
        public static string WriteReport(AuditReport report)
        {
            if (report == null)
            {
                return string.Empty;
            }

            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            string path = settings != null && !string.IsNullOrEmpty(settings.AuditReportPath)
                ? settings.AuditReportPath
                : "Assets/Sdx17AuditReport.md";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Sdx17 Project Audit");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            builder.AppendLine();
            builder.AppendLine($"- Total assets: {report.TotalAssets}");
            builder.AppendLine($"- Folders: {report.FolderCount}");
            builder.AppendLine($"- Total size: {FormatBytes(report.TotalBytes)}");
            builder.AppendLine($"- Large asset threshold: {FormatBytes(report.LargeAssetThresholdBytes)}");
            builder.AppendLine();
            builder.AppendLine("## Assets by extension");
            builder.AppendLine();
            foreach (KeyValuePair<string, int> pair in report.CountByExtension)
            {
                string label = string.IsNullOrEmpty(pair.Key) ? "(no extension)" : pair.Key;
                builder.AppendLine($"- {label}: {pair.Value}");
            }

            builder.AppendLine();
            builder.AppendLine("## Largest assets");
            builder.AppendLine();
            if (report.LargestAssets.Count == 0)
            {
                builder.AppendLine("- None found.");
            }
            else
            {
                foreach (AssetSizeEntry entry in report.LargestAssets)
                {
                    builder.AppendLine($"- {entry.Path} — {FormatBytes(entry.SizeBytes)}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("## Large assets (at or above threshold)");
            builder.AppendLine();
            if (report.LargeAssets.Count == 0)
            {
                builder.AppendLine("- None found.");
            }
            else
            {
                foreach (AssetSizeEntry entry in report.LargeAssets)
                {
                    builder.AppendLine($"- {entry.Path} — {FormatBytes(entry.SizeBytes)}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("## Duplicate asset names");
            builder.AppendLine();
            if (report.DuplicateNames.Count == 0)
            {
                builder.AppendLine("- None found.");
            }
            else
            {
                foreach (string name in report.DuplicateNames)
                {
                    builder.AppendLine($"- {name}");
                }
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, builder.ToString());
            AssetDatabase.Refresh();
            return path;
        }

        /// <summary>File size in bytes, or 0 when the file cannot be read (for example a package asset).</summary>
        public static long GetFileSizeBytes(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath))
            {
                return 0;
            }

            try
            {
                return new FileInfo(projectPath).Length;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>Human-readable size, e.g. "1.5 MB".</summary>
        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }

            if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024.0:F1} KB";
            }

            if (bytes < 1024L * 1024L * 1024L)
            {
                return $"{bytes / (1024.0 * 1024.0):F1} MB";
            }

            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }

    /// <summary>One asset and its size on disk.</summary>
    public readonly struct AssetSizeEntry
    {
        public AssetSizeEntry(string path, long sizeBytes)
        {
            Path = path ?? string.Empty;
            SizeBytes = sizeBytes;
        }

        public string Path { get; }
        public long SizeBytes { get; }
    }

    /// <summary>Full result of a project audit run.</summary>
    public sealed class AuditReport
    {
        public AuditReport()
        {
            CountByExtension = new Dictionary<string, int>();
            LargestAssets = new List<AssetSizeEntry>();
            LargeAssets = new List<AssetSizeEntry>();
            DuplicateNames = new List<string>();
        }

        public int TotalAssets { get; set; }
        public int FolderCount { get; set; }
        public long TotalBytes { get; set; }
        public long LargeAssetThresholdBytes { get; set; }
        public Dictionary<string, int> CountByExtension { get; }
        public List<AssetSizeEntry> LargestAssets { get; }
        public List<AssetSizeEntry> LargeAssets { get; }
        public List<string> DuplicateNames { get; }

        public string SummaryLine()
        {
            return $"{TotalAssets} asset(s) ({ProjectAuditReport.FormatBytes(TotalBytes)}), " +
                   $"{LargestAssets.Count} largest listed, {LargeAssets.Count} large, " +
                   $"{DuplicateNames.Count} duplicate name(s).";
        }
    }
}
