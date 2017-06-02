using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager.Templating;

namespace DD4T.Templates.Base.Providers
{
    public class DefaultBinaryPathProvider : BaseBinaryPathProvider
    {
        public DefaultBinaryPathProvider(Engine engine, Package package) : base(engine, package)
        {
        }
    }
}
