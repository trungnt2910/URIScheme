using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URIScheme
{
	public interface IURISchemeSerivce
	{
		bool Check();
		bool CheckAny();
		void Set();
		void Delete();
	}
}
