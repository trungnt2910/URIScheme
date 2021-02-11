using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using URIScheme.Enums;
using URIScheme.Tools;

namespace URIScheme
{
	public static class URISchemeServiceFactory
	{
		public static IURISchemeSerivce GetURISchemeSerivce(string key, string description, string runPath, RegisterType type = RegisterType.CurrentUser)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return new WindowsURISchemeService(key, description, runPath, type);
			}
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				if (IsXDG())
				{
					return new LinuxXdgURISchemeService(key, description, runPath, type);
				}
				throw new PlatformNotSupportedException("XDG tools are required");
			}
			throw new PlatformNotSupportedException("URI schemes are not supported in this platform.");
		}

		private static bool IsXDG()
		{
			var xdgCheckCommand = new Command("xdg-settings", "--version").Start();
			return xdgCheckCommand.ReturnValue == 0;
		}
	}
}
