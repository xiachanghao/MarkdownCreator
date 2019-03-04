using HeyRed.MarkdownSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MarkdownCreator.Core
{
    public class Creator : IDisposable
    {
        private const string DEFFAULT_BODY_PLACEHOLDER = "<bodyPlaceholder />";
        private const SearchOption DEFFAULT_SEARCH_OPTION = SearchOption.AllDirectories;

        private Markdown _markdown;
        private string _templatePath;
        private string _htmlTemplate;
        private string _bodyPlaceholder;
        private FileSystemWatcher _templateWatcher;
        private bool _watchAllDirectories;
        private Dictionary<string,FileSystemWatcher> _fileWatcherPool = new Dictionary<string, FileSystemWatcher>();

        public Creator()
        {
            Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template", "template.html"), DEFFAULT_BODY_PLACEHOLDER, DEFFAULT_SEARCH_OPTION);
        }

        public Creator(SearchOption searchOption)
        {
            Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template", "template.html"), DEFFAULT_BODY_PLACEHOLDER,searchOption);
        }

        public Creator(string templatePath)
        {
            Init(templatePath, DEFFAULT_BODY_PLACEHOLDER, DEFFAULT_SEARCH_OPTION);
        }

        public Creator(string templatePath, SearchOption searchOption)
        {
            Init(templatePath, DEFFAULT_BODY_PLACEHOLDER, searchOption);
        }

        public Creator(string templatePath, string bodyPlaceholder)
        {
            Init(templatePath, bodyPlaceholder, DEFFAULT_SEARCH_OPTION);
        }

        public Creator(string templatePath, string bodyPlaceholder, SearchOption searchOption)
        {
            Init(templatePath, bodyPlaceholder, searchOption);
        }

        private void Init(string templatePath, string bodyPlaceholder, SearchOption searchOption)
        {
            _watchAllDirectories = SearchOption.AllDirectories.Equals(searchOption);
            var option = new MarkdownOptions()
            {
                DisableImages = false,
                AllowEmptyLinkText = true,
                AllowTargetBlank = false
            };
            _markdown = new Markdown(option);
            _templatePath = templatePath;
            _htmlTemplate = File.ReadAllText(templatePath);
            _bodyPlaceholder = bodyPlaceholder;

            _templateWatcher = new FileSystemWatcher(Path.GetDirectoryName(templatePath));
            _templateWatcher.Changed += TemplateChanged;
            _templateWatcher.EnableRaisingEvents = true;
        }

        private void TemplateChanged(object sender, FileSystemEventArgs e)
        {
            _htmlTemplate = File.ReadAllText(_templatePath);
        }

        public string Transform(string text)
        {
            var htmlContent = _markdown.Transform(text);
            return _htmlTemplate.Replace(_bodyPlaceholder, htmlContent);
        }

        public void AutoTransform()
        {
            AutoTransform(AppDomain.CurrentDomain.BaseDirectory);
        }

        public void AutoTransform(string watchPath)
        {
            var watcherKey = watchPath.ToLower();
            if (_fileWatcherPool.ContainsKey(watcherKey))
            {
                DisposeMarkdownFileWatch(watcherKey);
            }
            if (!Directory.Exists(watchPath))
            {
                return;
            }
            try
            {
                var markdownFilePaths = Directory.GetFileSystemEntries(watchPath, "*.md");
                foreach (var markdownFilePath in markdownFilePaths)
                {
                    TransformFile(markdownFilePath);
                }
                if (_watchAllDirectories)
                {
                    var directoryList = Directory.GetDirectories(watchPath);
                    if (directoryList != null && directoryList.Any())
                    {
                        foreach (var dir in directoryList)
                        {
                            AutoTransform(dir);
                        }
                    }
                }

                var _fileWatcher = new FileSystemWatcher(watchPath);
                _fileWatcher.Changed += markdownFileChanged;
                _fileWatcher.Created += markdownFileChanged;
                _fileWatcher.Renamed += markdownFileChanged;
                _fileWatcher.EnableRaisingEvents = true;
                _fileWatcherPool.Add(watcherKey, _fileWatcher);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void markdownFileChanged(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath) && ".md".Equals(Path.GetExtension(e.FullPath), StringComparison.CurrentCultureIgnoreCase))
            {
                TransformFile(e.FullPath);
            }
            else if (Directory.Exists(e.FullPath) && _watchAllDirectories)
            {
                AutoTransform(e.FullPath);
            }
        }
        
        private void TransformFile(string filePath)
        {
            try
            {
                var path = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var reader = new StreamReader(fileStream);
                var fileContext = reader.ReadToEnd();
                reader.Dispose();
                fileStream.Close();
                fileStream.Dispose();
                var content = Transform(fileContext);
                File.WriteAllText(Path.Combine(path, string.Format($"{fileName}.html")), content, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Dispose()
        {
            _templateWatcher.Changed -= TemplateChanged;
            DisposeAllMarkdownFileWatch();
        }

        private void DisposeMarkdownFileWatch(string watcherKey)
        {
            if (_fileWatcherPool.ContainsKey(watcherKey))
            {
                var _fileWatcher = _fileWatcherPool[watcherKey];
                if (_fileWatcher != null)
                {
                    _fileWatcher.Changed -= markdownFileChanged;
                    _fileWatcher.Created -= markdownFileChanged;
                    _fileWatcher.EnableRaisingEvents = false;
                    _fileWatcher.Dispose();
                }
                _fileWatcherPool.Remove(watcherKey);
            }
        }

        private void DisposeAllMarkdownFileWatch()
        {
            foreach (var key in _fileWatcherPool.Keys)
            {
                DisposeMarkdownFileWatch(key);
            }
        }
    }
}
