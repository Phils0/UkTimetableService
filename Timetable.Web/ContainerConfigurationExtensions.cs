using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

namespace Timetable.Web
{
    public static class ContainerConfigurationExtensions
    {
        public static ContainerConfiguration WithAssembliesInPath(this ContainerConfiguration configuration,
            string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return WithAssembliesInPath(configuration, path, null, searchOption);
        }

        public static ContainerConfiguration WithAssembliesInPath(this ContainerConfiguration configuration,
            string path, AttributedModelProvider conventions, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var assemblyFiles = Directory.GetFiles(path, "*.dll", searchOption);         
            var assemblies = assemblyFiles.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);         
            configuration = configuration.WithAssemblies(assemblies, conventions);         
            return configuration;
        }
    }
}