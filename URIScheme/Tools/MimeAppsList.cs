using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using URIScheme.Enums;

namespace URIScheme.Tools
{
	public class MimeAppsList
	{
		private const string fileName = "mimeapps.list";
		private readonly bool needsSudo;
		private readonly string location;
		private enum MimeAppSection
		{
			DefaultApplications,
			AddedAssociations,
			RemovedAssociations
		}
		private static readonly Dictionary<string, MimeAppSection> SectionMap = new Dictionary<string, MimeAppSection>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ "[Default Applications]", MimeAppSection.DefaultApplications },
			{ "[Added Associations]", MimeAppSection.AddedAssociations },
			{ "[Removed Associations]", MimeAppSection.RemovedAssociations }
		};



		public readonly Dictionary<string, List<string>> DefaultApplications = new Dictionary<string, List<string>>();
		public readonly Dictionary<string, List<string>> AddedAssociations = new Dictionary<string, List<string>>();
		public readonly Dictionary<string, List<string>> RemovedAssociations = new Dictionary<string, List<string>>();

		public MimeAppsList(string location, bool needsSudo)
		{
			this.location = location;
			this.needsSudo = needsSudo;
			if (!File.Exists(location))
			{
				return;
			}
			// Contents of the file can be found here: https://specifications.freedesktop.org/mime-apps-spec/mime-apps-spec-latest.html
			MimeAppSection? currentSection = null;
			using (StreamReader reader = File.OpenText(location))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine().Trim();
					if (string.IsNullOrEmpty(line))
					{
						continue;
					}

					if (SectionMap.ContainsKey(line))
					{
						currentSection = SectionMap[line];
					}
					else
					{
						if (!currentSection.HasValue)
						{
							continue;
						}

						var key = line.Substring(0, line.IndexOf('=')).Trim();
						var values = line.Substring(line.IndexOf('=') + 1);
						Dictionary<string, List<string>> toInsert = null;
						switch (currentSection)
						{
							case MimeAppSection.DefaultApplications:
								toInsert = DefaultApplications;
								break;
							case MimeAppSection.AddedAssociations:
								toInsert = AddedAssociations;
								break;
							case MimeAppSection.RemovedAssociations:
								toInsert = RemovedAssociations;
								break;
						}
						if (toInsert.ContainsKey(key))
						{
							toInsert[key].AddRange(values.Split(';'));
						}
						else
						{
							toInsert.Add(key, values.Split(';').ToList());
						}
					}
				}
			}
		}
		public MimeAppsList(RegisterType registerType = RegisterType.CurrentUser) : 
			this(GetMimeAppsLocation(registerType), registerType == RegisterType.LocalMachine) { }

		public void Save()
		{
			string tempFile = Path.GetTempFileName();
			using (TextWriter tw = File.CreateText(tempFile))
			{
				if (DefaultApplications.Count != 0)
				{
					tw.WriteLine("[Default Applications]");
					foreach (var kvp in DefaultApplications)
					{
						if (kvp.Value.Count != 0)
						{
							tw.WriteLine($"{kvp.Key}={string.Join(";", kvp.Value)}");
						}
					}
					tw.WriteLine();
				}
				if (AddedAssociations.Count != 0)
				{
					tw.WriteLine("[Added Associations]");
					foreach (var kvp in AddedAssociations)
					{
						if (kvp.Value.Count != 0)
						{
							tw.WriteLine($"{kvp.Key}={string.Join(";", kvp.Value)}");
						}
					}
					tw.WriteLine();
				}
				if (RemovedAssociations.Count != 0)
				{
					tw.WriteLine("[Removed Associations]");
					foreach (var kvp in RemovedAssociations)
					{
						if (kvp.Value.Count != 0)
						{
							tw.WriteLine($"{kvp.Key}={string.Join(";", kvp.Value)}");
						}
					}
					tw.WriteLine();
				}
			}

			if (needsSudo)
			{
				var command = new SudoCommand("mv", $"-f {tempFile} {location}");
				command.Start().ThrowOnError();
				//Allows everyone to read the file:
				var chmod = new SudoCommand("chmod", $"644 {location}");
				chmod.Start().ThrowOnError();
			}
			else
			{
				var command = new Command("mv", $"-f {tempFile} {location}");
				command.Start().ThrowOnError();
			}
		}

		public List<string> GetURISchemeHandlers(string name)
		{
			string mimeType = $"x-scheme-handler/{name}";
			try
			{
				return DefaultApplications[mimeType];
			}
			catch (KeyNotFoundException)
			{
				var result = new List<string>();
				DefaultApplications.Add(mimeType, result);
				return result;
			}
		}


		private static string GetMimeAppsLocation(RegisterType registerType)
		{
			// About the location: https://specifications.freedesktop.org/mime-apps-spec/mime-apps-spec-latest.html#file
			// About the variable names: https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html#variables
			if (registerType == RegisterType.CurrentUser)
			{
				var location = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
				if (string.IsNullOrEmpty(location))
				{
					location = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				}
				return Path.Combine(location, fileName);
			}
			else //if (registerType == RegisterType.LocalMachine)
			{
				var path = Environment.GetEnvironmentVariable("XDG_CONFIG_DIRS");
				if (string.IsNullOrEmpty(path))
				{
					path = "/etc/xdg";
				}
				if (path.Contains(":"))
				{
					path = path.Substring(0, path.IndexOf(':'));
				}
				return Path.Combine(path, fileName);
			}
			throw new PlatformNotSupportedException("Cannot find mimeapps.list");
		}
	}
}
