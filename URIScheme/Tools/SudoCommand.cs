using System;
using System.Security;
using System.Threading;

namespace URIScheme.Tools
{
	public class SudoCommand : Command
	{
		public SudoCommand(string file, string command) : base("sudo", $"-A -E {file} {command}")
		{
			process.StartInfo.RedirectStandardInput = true;
		}
		public override Command Start()
		{
			base.Start();
			return this;
		}
	}
}
