using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using URIScheme.Enums;

namespace URIScheme
{
	public class URISchemeServiceFactory
	{
		public static IURISchemeSerivce GetURISchemeSerivce(string key, string description, string runPath, RegisterType type = RegisterType.CurrentUser)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return new WindowsURISchemeService(key, description, runPath, type);
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				if (IsGnome())
					return new LinuxGnomeURISchemeService(key, description, runPath, type);
				else
					throw new PlatformNotSupportedException("URI schemes are not supported in this platform.");
			}
			throw new PlatformNotSupportedException("URI schemes are not supported in this platform.");
		}

		private static bool IsGnome()
		{
			var command = new Process();
			command.StartInfo.FileName = "gnome-shell";
			command.StartInfo.Arguments = "--version";
			command.StartInfo.UseShellExecute = false;
			command.StartInfo.RedirectStandardOutput = true;

			command.Start();

			command.WaitForExit();

			string result = command.StandardOutput.ReadToEnd().Trim();
			string error = command.StandardError.ReadToEnd().Trim();

			if (!string.IsNullOrEmpty(error))
				return false;
			else
				return !string.IsNullOrEmpty(result);
		}
	}
}
