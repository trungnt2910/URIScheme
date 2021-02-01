using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace URIScheme.Tools
{
	public class Command
	{
		protected readonly Process process;
		private readonly Dictionary<int, string> errorCodeMap;
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
			errorCodeMap = new Dictionary<int, string>();
		}

		public Command MapReturnValue(int value, string message)
		{
			if (errorCodeMap.ContainsKey(value))
				errorCodeMap.Remove(value);
			errorCodeMap.Add(value, message);

			return this;
		}

		public virtual Command Start()
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
				bool hasCustomMessage = errorCodeMap.TryGetValue(ReturnValue, out var message);

				if (hasCustomMessage)
				{
					throw new SystemException($"{message} Program output: {Error}");
				}

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
