using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ImageScannerPicker
{
    public class ImageScannerLoader
    {
        public static List<IImageScannerPlugin> Plugins { get; set; }

        public static IImageScannerPlugin GetPlugin(string name, Delegates delegates, string license = "")
        {
            IImageScannerPlugin plugin = Plugins.Where(p => p.Name == name).FirstOrDefault();

            plugin?.SetDelegates(delegates);

            return plugin ?? throw new Exception($"No plugin found with name '{name}'");
        }

        public void LoadPlugins(string path)
        {
            Plugins = new List<IImageScannerPlugin>();

            LoadPluginAssemblyFile(path);

            LoadPluginInstance();
        }

        void LoadPluginAssemblyFile(string path) =>
            Directory.GetFiles(path).ToList()
                .Where(file =>
                {
                    string fileName = Path.GetFileName(file);
                    return fileName.StartsWith("ImageScannerPicker.") && fileName.EndsWith("Adaptor.dll");
                })
                .ToList()
                .ForEach(file => Assembly.LoadFile(Path.GetFullPath(file)));

        private void LoadPluginInstance() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => typeof(IImageScannerPlugin).IsAssignableFrom(p) && p.IsClass)
                .ToList()
                .ForEach(type => Plugins.Add((IImageScannerPlugin)Activator.CreateInstance(type)));
    }
}
