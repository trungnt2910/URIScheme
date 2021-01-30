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
		private readonly bool doSudo;

		private static readonly string UserDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "applications");
		private static readonly string DefaultDir = Environment.GetEnvironmentVariable("DESKTOP_FILE_INSTALL_DIR");

		public LinuxXdgURISchemeService(string key, string description, string runPath, RegisterType type = RegisterType.CurrentUser)
		{
			scheme = key;
			name = description;
			exec = $"{runPath} %u";
			doSudo = type == RegisterType.LocalMachine;
		}
		public bool Check()
		{
			var checkCommand = new Command("xdg-settings", $"get default-url-scheme-handler {scheme}");
			checkCommand.Start();
			string response = checkCommand.Output.Trim();
			
			return response == $"{scheme}.desktop";
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
				var installCommand = new Command("xdg-mime", $"install {xmlFileName} --novendor");
				installCommand.Start();
				if (installCommand.ReturnValue != 0)
				{
					throw new SystemException(installCommand.Error);
				}
				string desktopFileName = Path.Combine(tmpFolder, $"{scheme}.desktop");
				using (var tempDesktopFile = File.CreateText(desktopFileName))
				{
					//				[Desktop Entry]
					//				Name=LMAO
					//				Exec =/ home / trung / lmao % u
					//				Type=Application
					//				NoDisplay = true
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

				var desktopFileCommand = (new Command("desktop-file-install", $"{desktopFileName} --dir={UserDir}")).Start().ThrowOnError();
				var setDefaultCommand = (new Command("xdg-settings", $"set default-url-scheme-handler {scheme} {scheme}.desktop")).Start().ThrowOnError();
			}
			catch (Exception e)
			{
				throw e;
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
				var uninstallCommand = (new Command("xdg-mime", $"uninstall {xmlFileName} --novendor")).Start().ThrowOnError();
				var deleteDesktopFileCommand = (new Command("rm", $"-f {Path.Combine(UserDir, $"{scheme}.desktop")}")).Start().ThrowOnError();
			}
			catch (Exception e)
			{
				throw e;
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
