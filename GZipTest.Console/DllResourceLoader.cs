using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GZipTest
{
    class DllResourceLoader
    {
        public Assembly AssemblyResolveFromEmbeddedResource(object sender, ResolveEventArgs args)
        {
            //Console.WriteLine(args.Name);

            string resourceName = new AssemblyName(args.Name).Name + ".dll";

            string resourceToLoad = null;

            foreach (var resource in this.GetType().Assembly.GetManifestResourceNames())
            {
                if (resource.EndsWith(resourceName))
                {
                    resourceToLoad = resource;
                    break;
                }
            }

            
            if (resourceToLoad != null)
            {

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GZipTest.lib.GZipTest.Library.dll"))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            }
            else
            {
                return null;
            }
            
        }
        
    }
}

