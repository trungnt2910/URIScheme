
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URIScheme
{
	public interface IURISchemeSerivce
	{
		public bool Check();
		public void Set();
		public void Delete();
	}
}
