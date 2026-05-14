namespace MAACO.Tools.Tools;

internal static class ToolPathSafety
{
    public static bool IsWithinWorkspace(string workspacePath, string targetPath)
    {
        var workspaceFull = Path.GetFullPath(workspacePath);
        var targetFull = Path.GetFullPath(targetPath);

        if (string.Equals(workspaceFull, targetFull, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!workspaceFull.EndsWith(Path.DirectorySeparatorChar))
        {
            workspaceFull += Path.DirectorySeparatorChar;
        }

        return targetFull.StartsWith(workspaceFull, StringComparison.OrdinalIgnoreCase);
    }
}
