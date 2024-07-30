using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Example.Benchmarking
{
    public class BenchmarkMetadata
    {
        public static BenchmarkMetadata Instance { get; } = new BenchmarkMetadata();

        private Dictionary<string, SortedList<int, object>> _Metadata = new Dictionary<string, SortedList<int, object>>();
        private string _DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private const string _BenchmarkDirectoryName = "Custom Benchmark Data";
        private int _Index = 0;

        public void Load(string benchmarkName)
        {
            // directory
            string directory = Path.Combine(_DocumentsPath, _BenchmarkDirectoryName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // file
            string fileName = Path.Combine(directory, $"{benchmarkName}.json");
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                _Metadata = JsonSerializer.Deserialize<Dictionary<string, SortedList<int, object>>> (json);
            }
        }
        public void Save(string benchmarkName)
        {
            // directory
            string directory = Path.Combine(_DocumentsPath, _BenchmarkDirectoryName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // file
            string fileName = Path.Combine(directory, $"{benchmarkName}.json");
            string json = JsonSerializer.Serialize(_Metadata);
            File.WriteAllText(fileName, json);
        }

        public void AddMetadata(string name, object value)
        {
            if (!_Metadata.ContainsKey(name))
                _Metadata[name] = new SortedList<int, object>();
            _Metadata[name].Add(_Index++, value);
        }

        public SortedList<int, object> GetMetdata(string name)
        {
            if (_Metadata.TryGetValue(name, out SortedList<int, object> values))
                return values;
            return null;
        }
    }
}