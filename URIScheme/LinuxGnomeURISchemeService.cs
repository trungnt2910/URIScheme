using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URIScheme.Enums;

namespace URIScheme
{
	// As we don't have a Linux machine, we may not test this right now.
	// A good reference implementation:
	// https://github.com/beothorn/URISchemeHandler/blob/master/src/main/java/uriSchemeHandler/LinuxURISchemeHandler.java
	public class LinuxGnomeURISchemeService : IURISchemeSerivce
	{
		public LinuxGnomeURISchemeService(string key, string description, string runPath, RegisterType type = RegisterType.CurrentUser)
		{

		}
		public bool Check()
		{
			throw new NotImplementedException();
		}
		public void Set()
		{
			throw new NotImplementedException();
		}
		public void Delete()
		{
			throw new NotImplementedException();
		}
	}
}
