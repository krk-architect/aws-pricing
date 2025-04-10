// ReSharper disable MemberCanBePrivate.Global

namespace Pricing.Visitors;

/// <summary>
///     A visitor class for displaying a directory structure.
/// </summary>
public class DirectoryInfoVisitor
{
    private StringBuilder Sb { get; } = new();

    /// <summary>
    ///     Displays the directory structure of the specified root directory.
    /// </summary>
    /// <param name="root">A <see cref="DirectoryInfo" /> object representing the root directory.</param>
    /// <returns>
    /// </returns>
    public string Tree(DirectoryInfo root)
    {
        const string indent = "";

        Sb.AppendLine(root.FullName);
        var entries = root.GetFileSystemInfos()
                          .OrderBy(static e => e is DirectoryInfo ? 0 : 1)
                          .ThenBy(static e => e.Name)
                          .ToList();

        for (var i = 0; i < entries.Count; i++)
        {
            var last  = i == entries.Count - 1;
            var entry = entries[i];

            switch (entry)
            {
                case DirectoryInfo subDir:
                    VisitDirectory(subDir, indent, last);
                    break;
                case FileInfo file:
                    VisitFile(file, indent, last);
                    break;
            }
        }

        return Sb.ToString();
    }

    /// <summary>
    ///     Visits a directory and appends its name to the StringBuilder.
    /// </summary>
    /// <param name="directory"></param>
    /// <param name="indent"></param>
    /// <param name="isLast"></param>
    public void VisitDirectory(DirectoryInfo directory, string indent, bool isLast)
    {
        Sb.AppendLine($"{indent}{(isLast ? "`-- " : "|-- ")}{directory.Name}");

        var entries = directory.GetFileSystemInfos()
                               .OrderBy(static e => e is DirectoryInfo ? 0 : 1)
                               .ThenBy(static e => e.Name)
                               .ToList();

        for (var i = 0; i < entries.Count; i++)
        {
            var last      = i == entries.Count - 1;
            var entry     = entries[i];
            var newIndent = indent + (isLast ? "    " : "|   ");

            switch (entry)
            {
                case DirectoryInfo subDir:
                    VisitDirectory(subDir, newIndent, last);
                    break;
                case FileInfo file:
                    VisitFile(file, newIndent, last);
                    break;
            }
        }
    }

    /// <summary>
    ///     Visits a file and appends its name to the StringBuilder.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="indent"></param>
    /// <param name="isLast"></param>
    public void VisitFile(FileInfo file, string indent, bool isLast)
    {
        Sb.AppendLine($"{indent}{(isLast ? "`-- " : "|-- ")}{file.Name}");
    }
}
