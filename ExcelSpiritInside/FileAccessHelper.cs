using System.IO;
using System.Runtime.InteropServices;

namespace ExcelSpiritInside
{
    /// <summary>
    /// Detects common file-access problems (in-use by Excel, missing, permission denied, ...)
    /// before the operation is attempted and translates raw exceptions into user-friendly text.
    /// </summary>
    internal static class FileAccessHelper
    {
        private const int ErrorSharingViolation = 32;
        private const int ErrorLockViolation = 33;

        public static void EnsureReadable(string path, string role)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new UserFacingException($"{role} is not specified.");
            }
            if (!File.Exists(path))
            {
                throw new UserFacingException($"{role} was not found:\r\n{path}");
            }

            try
            {
                using var _ = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
            }
            catch (IOException ex) when (IsSharingViolation(ex))
            {
                throw new UserFacingException(BuildInUseMessage(path, role, forWrite: false));
            }
            catch (UnauthorizedAccessException)
            {
                throw new UserFacingException(
                    $"Cannot read {role}. Access was denied. Please check the file permissions:\r\n{path}");
            }
        }

        public static void EnsureWritable(string path, string role)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new UserFacingException($"{role} path is not specified.");
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                throw new UserFacingException($"The output folder does not exist:\r\n{directory}");
            }

            try
            {
                if (File.Exists(path))
                {
                    using var _ = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                else
                {
                    using (var _ = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                    }
                    File.Delete(path);
                }
            }
            catch (IOException ex) when (IsSharingViolation(ex))
            {
                throw new UserFacingException(BuildInUseMessage(path, role, forWrite: true));
            }
            catch (UnauthorizedAccessException)
            {
                throw new UserFacingException(
                    $"Cannot write {role}. Access was denied. Please choose a different folder or run with sufficient permissions:\r\n{path}");
            }
        }

        public static string Translate(Exception ex, string operationLabel)
        {
            switch (ex)
            {
                case UserFacingException ufe:
                    return ufe.Message;
                case FileNotFoundException fnf:
                    return $"{operationLabel} failed: file not found\r\n{fnf.FileName ?? fnf.Message}";
                case DirectoryNotFoundException dnf:
                    return $"{operationLabel} failed: folder not found\r\n{dnf.Message}";
                case UnauthorizedAccessException:
                    return $"{operationLabel} failed: access was denied.\r\n{ex.Message}";
                case IOException io when IsSharingViolation(io):
                    return $"{operationLabel} failed: the file is currently open in another program (typically Excel). Please close it and try again.\r\n{io.Message}";
                case InvalidDataException:
                    return $"{operationLabel} failed: the workbook could not be parsed.\r\n{ex.Message}";
                default:
                    return $"{operationLabel} failed: {ex.Message}";
            }
        }

        public static bool IsSharingViolation(IOException ex)
        {
            var code = ex.HResult & 0xFFFF;
            return code == ErrorSharingViolation || code == ErrorLockViolation;
        }

        private static string BuildInUseMessage(string path, string role, bool forWrite)
        {
            var verb = forWrite ? "written to" : "read";
            var name = Path.GetFileName(path);
            var holder = TryGetProcessHoldingFile(path);
            var who = holder is null ? "another program (typically Excel)" : $"{holder}";
            return $"{role} '{name}' cannot be {verb} because it is currently open in {who}. Please close it and try again.";
        }

        private static string? TryGetProcessHoldingFile(string path)
        {
            // Best-effort lookup via the Restart Manager. If unavailable, we return null and use a generic message.
            try
            {
                uint sessionHandle;
                if (RmStartSession(out sessionHandle, 0, Guid.NewGuid().ToString()) != 0)
                {
                    return null;
                }
                try
                {
                    string[] resources = { path };
                    if (RmRegisterResources(sessionHandle, (uint)resources.Length, resources, 0, null!, 0, null!) != 0)
                    {
                        return null;
                    }

                    uint pnProcInfoNeeded = 0;
                    uint pnProcInfo = 0;
                    uint lpdwRebootReasons = 0;
                    int rc = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);
                    if (pnProcInfoNeeded == 0)
                    {
                        return null;
                    }

                    var processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;
                    if (RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons) == 0)
                    {
                        var names = new List<string>();
                        for (int i = 0; i < pnProcInfo; i++)
                        {
                            var appName = processInfo[i].strAppName;
                            if (!string.IsNullOrWhiteSpace(appName))
                            {
                                names.Add(appName);
                            }
                        }
                        if (names.Count > 0)
                        {
                            return string.Join(", ", names.Distinct());
                        }
                    }
                }
                finally
                {
                    RmEndSession(sessionHandle);
                }
            }
            catch
            {
                // Ignore – informational only.
            }
            return null;
        }

        // Restart Manager P/Invoke – used only to name the process that owns the lock.
        [StructLayout(LayoutKind.Sequential)]
        private struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        private const int CCH_RM_MAX_APP_NAME = 255;
        private const int CCH_RM_MAX_SVC_NAME = 63;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public string strServiceShortName;
            public uint ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames,
            uint nApplications, [In] RM_UNIQUE_PROCESS[] rgApplications, uint nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo,
            [In, Out] RM_PROCESS_INFO[]? rgAffectedApps, ref uint lpdwRebootReasons);
    }

    internal sealed class UserFacingException : Exception
    {
        public UserFacingException(string message) : base(message) { }
        public UserFacingException(string message, Exception inner) : base(message, inner) { }
    }
}
