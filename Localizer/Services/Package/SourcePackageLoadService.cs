using System.IO;
using System.Linq;
using Localizer.DataModel;
using Localizer.Services.File;
using Terraria.ModLoader;
using File = System.IO.File;

namespace Localizer.Package
{
    public sealed class SourcePackageLoadService<T> : IPackageLoadService<T> where T : IPackage
    {
        private readonly string _packageMainFileName = "Package.json";

        public IPackage Load(string path, IFileLoadService fileLoadService)
        {
            Utils.LogDebug($"Loading package from {path}");

            if (!Directory.Exists(path))
            {
                Utils.LogError("Package directory doesn't exist!");
                return null;
            }

            var packDir = new DirectoryInfo(path);
            var packageFilePath = Path.Combine(packDir.FullName, _packageMainFileName);
            if (!File.Exists(packageFilePath))
            {
                Utils.LogError("Package main file doesn't exist!");
                return null;
            }

            IPackage package = Utils.ReadFileAndDeserializeJson<T>(packageFilePath);
            if (package == null)
            {
                Utils.LogError("Package main file deserialization failed!");
                return null;
            }

            InitPackage(package);
            foreach (var fileTypeName in package.FileList)
            {
                Utils.LogDebug($"Loading file [{fileTypeName}]");

                var filePath = Path.Combine(packDir.FullName, fileTypeName + ".json");
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    var file = fileLoadService.Load(fs, fileTypeName);

                    package.AddFile(file);
                }
            }

            Utils.LogDebug($"Package [{package.Name} loaded.]");

            return package;
        }

        private void InitPackage(IPackage package)
        {
            package.Mod = new ModWrapper(ModLoader.Mods.FirstOrDefault(m => m.Name == package.ModName));
        }
    }
}
