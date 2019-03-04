using MarkdownCreator.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownCreator.WindowsServer
{
    public partial class MDFileWatcher : ServiceBase
    {
        Creator markdownCreator;
        public MDFileWatcher()
        {
            InitializeComponent();
            var templatePath = ConfigurationManager.AppSettings["templatePath"];
            var watchOption = ConfigurationManager.AppSettings["watchOption"];
            if (string.IsNullOrEmpty(templatePath) && string.IsNullOrEmpty(watchOption))
            {
                markdownCreator = new Creator();
            }
            else if (!string.IsNullOrEmpty(templatePath) && string.IsNullOrEmpty(watchOption))
            {
                markdownCreator = new Creator(templatePath);
            }
            else if (string.IsNullOrEmpty(templatePath) && !string.IsNullOrEmpty(watchOption))
            {
                var option = (SearchOption)Enum.Parse(typeof(SearchOption), watchOption, true);
                markdownCreator = new Creator(option);
            }
            else
            {
                var option = (SearchOption)Enum.Parse(typeof(SearchOption), watchOption, true);
                markdownCreator = new Creator(templatePath, option);
            }
        }

        protected override void OnStart(string[] args)
        {
            var watchPath = ConfigurationManager.AppSettings["watchPath"];
            if (string.IsNullOrEmpty(watchPath))
            {
                watchPath = AppDomain.CurrentDomain.BaseDirectory;
            }
            markdownCreator.AutoTransform(watchPath);
        }

        protected override void OnStop()
        {
            markdownCreator.Dispose();
        }
    }
}
