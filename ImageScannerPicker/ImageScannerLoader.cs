using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ImageScannerPicker
{
    public class ImageScannerLoader
    {
        private static List<Type> plugIns = new List<Type>();

        public static List<string> PlugIns => plugIns.Select(x => x.Name).ToList();

        public static IImageScannerPlugin GetPlugin(string name, ImageScannerConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Type plugInType = plugIns.FirstOrDefault(type => type.Name == name);

            IImageScannerPlugin plugIn = (IImageScannerPlugin)Activator.CreateInstance(
                plugInType ?? throw new Exception($"No plugin found with name '{name}'"), config);

            return plugIn;
        }

        public void LoadPluginAssemblies(string path)
            => Directory
            .GetFiles(path)
            .ToList()
            .Where(
                fileName =>
                Path.GetFileName(fileName).StartsWith("ImageScannerPicker.") &&
                Path.GetFileName(fileName).EndsWith(".dll"))
            .ToList()
            .Select(
                fileName =>
                Assembly.LoadFile(Path.GetFullPath(fileName)))
            .SelectMany(assembly => assembly.GetTypes())
            .Where(p => typeof(IImageScannerPlugin).IsAssignableFrom(p) && p.IsClass)
            .Where(type => !plugIns.Contains(type))
            .ToList()
            .ForEach(type => plugIns.Add(type));
    }
}
