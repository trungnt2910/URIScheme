using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace URIScheme.Tools
{
	public class Command
	{
		private readonly Process process;
		public int ReturnValue { get; private set; }
		public string Output { get; private set; }
		public string Error { get; private set; }

		public Command(string file, string args)
		{
			process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					Arguments = args,
					FileName = file,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				}
			};
			Output = null;
			Error = null;
			ReturnValue = 0;
		}

		public Command Start()
		{
			process.Start();
			process.WaitForExit();

			ReturnValue = process.ExitCode;
			Output = process.StandardOutput.ReadToEnd();
			Error = process.StandardError.ReadToEnd();
			return this;
		}

		public Command ThrowOnError()
		{
			if (ReturnValue != 0)
			{
				throw new SystemException(Error);
			}
			return this;
		}

		public Task StartAsync()
		{
			return Task.Run(() => Start());
		}
	}
}
