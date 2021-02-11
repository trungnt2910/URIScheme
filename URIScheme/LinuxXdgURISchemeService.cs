using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using URIScheme.Enums;
using URIScheme.Tools;

namespace URIScheme
{
	public class LinuxXdgURISchemeService : IURISchemeSerivce
	{
		private readonly string scheme;
		private readonly string name;
		private readonly string exec;
		private readonly string desktopFileName;
		private readonly RegisterType registerType;

		private static readonly string UserDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "applications");
		private static readonly string DefaultDir = Environment.GetEnvironmentVariable("DESKTOP_FILE_INSTALL_DIR") ?? "/usr/share/applications";

		public LinuxXdgURISchemeService(string key, string description, string runPath, RegisterType type = RegisterType.CurrentUser)
		{
			scheme = key;
			name = description;
			desktopFileName = "{name}.desktop";
			exec = $"{runPath} %u";
			registerType = type;
		}
		public bool Check()
		{
			switch (registerType)
			{
				case RegisterType.CurrentUser:
					var checkCommand = new Command("xdg-settings", $"get default-url-scheme-handler {scheme}");
					checkCommand.Start();
					string response = checkCommand.Output.Trim();

					return response == $"{scheme}.desktop";
				case RegisterType.LocalMachine:
					var appsList = new MimeAppsList(RegisterType.LocalMachine);
					return appsList.GetURISchemeHandlers(scheme).FirstOrDefault() == $"{scheme}.desktop";
			}
			return false;
		}
		public bool CheckAny()
		{
			switch (registerType)
			{
				case RegisterType.CurrentUser:
					var checkCommand = new Command("xdg-settings", $"get default-url-scheme-handler {scheme}");
					checkCommand.Start();
					string response = checkCommand.Output.Trim();

					return !string.IsNullOrEmpty(response);
				case RegisterType.LocalMachine:
					var appsList = new MimeAppsList(RegisterType.LocalMachine);
					return appsList.GetURISchemeHandlers(scheme).Count != 0;
			}
			return false;
		}
		public void Set()
		{
			string tmpFolder = Path.GetTempFileName();
			File.Delete(tmpFolder);
			Directory.CreateDirectory(tmpFolder);
			try
			{
				string xmlFileName = Path.Combine(tmpFolder, $"{scheme}.xml");
				using (var tempXMLFile = File.CreateText(xmlFileName))
				{
					GenerateXMLFile(tempXMLFile);
				}

				switch (registerType)
				{
					case RegisterType.CurrentUser:
					{
						var installCommand = new Command("xdg-mime", $"install {xmlFileName} --novendor").Start().ThrowOnError();
					}
					break;
					case RegisterType.LocalMachine:
					{
						var installCommand = new SudoCommand("xdg-mime", $"install {xmlFileName} --mode system --novendor").Start().ThrowOnError();
					}
					break;
				}

				using (var tempDesktopFile = File.CreateText(desktopFileName))
				{
					//				[Desktop Entry]
					//				Name=LMAO
					//				Exec=/home/trung/lmao %u
					//				Type=Application
					//				NoDisplay=true
					//				Categories=Utility;
					//				MimeType=x-scheme-handler/lmao;
					tempDesktopFile.WriteLine("[Desktop Entry]");
					tempDesktopFile.WriteLine($"Name={name}");
					tempDesktopFile.WriteLine($"Exec={exec}");
					tempDesktopFile.WriteLine($"Type=Application");
					tempDesktopFile.WriteLine("NoDisplay=true");
					tempDesktopFile.WriteLine("Categories=Utility");
					tempDesktopFile.WriteLine($"MimeType=x-scheme-handler/{scheme}");
				}

				switch (registerType)
				{
					case RegisterType.CurrentUser:
					{
						var desktopFileCommand = new Command("desktop-file-install", $"{desktopFileName} --dir={UserDir}").Start().ThrowOnError();
						var setDefaultCommand = new Command("xdg-settings", $"set default-url-scheme-handler {scheme} {scheme}.desktop")
													.MapReturnValue(2, "runPath does not exist.")
													.Start()
													.ThrowOnError();
					}
					break;
					case RegisterType.LocalMachine:
					{
						var desktopFileCommand = new SudoCommand("desktop-file-install", $"{desktopFileName}").Start().ThrowOnError();
						var mimeapps = new MimeAppsList(RegisterType.LocalMachine);
						var list = mimeapps.GetURISchemeHandlers(scheme);
						list.Insert(0, $"{scheme}.desktop");
						mimeapps.Save();
					}
					break;
				}
			}
			finally
			{
				Directory.Delete(tmpFolder, true);
			}
		}
		public void Delete()
		{
			string tmpFolder = Path.GetTempFileName();
			File.Delete(tmpFolder);
			Directory.CreateDirectory(tmpFolder);
			try
			{
				string xmlFileName = Path.Combine(tmpFolder, $"{scheme}.xml");
				using (var tempXMLFile = File.CreateText(xmlFileName))
				{
					GenerateXMLFile(tempXMLFile);
				}				
				switch (registerType)
				{
					case RegisterType.CurrentUser:
					{
						var uninstallCommand = new Command("xdg-mime", $"uninstall {xmlFileName} --novendor").Start().ThrowOnError();
						var deleteDesktopFileCommand = new Command("rm", $"-f {Path.Combine(UserDir, $"{scheme}.desktop")}").Start().ThrowOnError();
					}
					break;
					case RegisterType.LocalMachine:
					{
						var uninstallCommand = new SudoCommand("xdg-mime", $"uninstall {xmlFileName} --mode system --novendor").Start().ThrowOnError();
						var deleteDesktopFileCommand = new SudoCommand("rm", $"-f {Path.Combine(DefaultDir, $"{scheme}.desktop")}").Start().ThrowOnError();
						var mimeapps = new MimeAppsList(RegisterType.LocalMachine);
						var list = mimeapps.GetURISchemeHandlers(scheme);
						list.RemoveAll((s) => s == $"{scheme}.desktop");
						mimeapps.Save();
					}
					break;
				}
			}
			finally
			{
				Directory.Delete(tmpFolder, true);
			}
		}

		private void GenerateXMLFile(TextWriter output)
		{
			using (var writer = XmlWriter.Create(output))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("mime-info", "http://www.freedesktop.org/standards/shared-mime-info");

				writer.WriteStartElement("mime-type");
				writer.WriteAttributeString("type", $"x-scheme-handler/{scheme}");
				writer.WriteEndElement();

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}
	}
}
