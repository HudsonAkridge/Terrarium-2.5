using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Terrarium.Server.Resources;

namespace Terrarium.Server.Helpers
{
    [RunInstaller(true)]
    public class WebsiteInstaller : Installer
    {
        public WebsiteInstaller()
        {
            var info = InstallerInfo.GetInstallerInfo();

            // Add a default one that we can always fall back to
            var myEventLogInstaller = new EventLogInstaller();
            myEventLogInstaller.Source = InstallerInfo.DefaultEventLogSource;
            Installers.Add(myEventLogInstaller);

            foreach (var source in info.EventLogInfos)
            {
                myEventLogInstaller = new EventLogInstaller();
                myEventLogInstaller.Source = source.Source;
                Installers.Add(myEventLogInstaller);
            }

            foreach (var performanceCounter in info.PerformanceCounterCategoryInfos)
            {
                var myCounterInstaller = new PerformanceCounterInstaller();
                myCounterInstaller.CategoryHelp = performanceCounter.CategoryHelp;
                myCounterInstaller.CategoryName = performanceCounter.CategoryName;
                var counters = new ArrayList();
                foreach (var creationDataInfo in performanceCounter.CounterCreationDataInfos)
                    counters.Add(new CounterCreationData(creationDataInfo.CounterName, creationDataInfo.CounterHelp,
                        creationDataInfo.CounterType));

                myCounterInstaller.Counters.AddRange(
                    (CounterCreationData[])counters.ToArray(typeof(CounterCreationData)));
                Installers.Add(myCounterInstaller);
            }
        }
    }

    public class InstallerInfo
    {
        private static InstallerInfo _installerInfo;

        public PerformanceCounterCategoryInfo[] PerformanceCounterCategoryInfos { get; set; }

        public EventLogInfo[] EventLogInfos { get; set; }

        public static string DefaultEventLogSource => "Website Installer";

        public static void WriteEventLog(string id, string entry)
        {
            WriteEventLog(id, entry, EventLogEntryType.Error);
        }

        public static void WriteEventLog(string id, string entry, EventLogEntryType type)
        {
            var installerInfo = GetInstallerInfo();
            var source = DefaultEventLogSource;

            foreach (var info in installerInfo.EventLogInfos)
            {
                if (info.ID.ToLower() == id.ToLower())
                {
                    source = info.Source;
                    break;
                }
            }

            var myLog = new EventLog { Source = source };
            myLog.WriteEntry(entry, type);
        }

        public static PerformanceCounter CreatePerformanceCounter(string id)
        {
            //TODO: This can be easily simplified by putting category name on the CounterCreationDataInfo, then we can just do a SelectMany on the whole collection and return a single or default result
            var installerInfo = GetInstallerInfo();

            foreach (var info in installerInfo.PerformanceCounterCategoryInfos)
            {
                var ccd = info.CounterCreationDataInfos.SingleOrDefault(x => string.Equals(x.ID, id, StringComparison.CurrentCultureIgnoreCase));
                if (ccd != null) { return new PerformanceCounter(info.CategoryName, ccd.CounterName, ccd.InstanceName, false); }
            }

            return null;
        }

        public static InstallerInfo GetInstallerInfo()
        {
            if (_installerInfo != null)
                return _installerInfo;

            var serializer = new XmlSerializer(typeof(InstallerInfo));

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(Resource.install);
            writer.Flush();
            stream.Position = 0;

            _installerInfo = (InstallerInfo)serializer.Deserialize(stream);

            stream.Close();

            //using(TextReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("install.xml"))) {
            //    installerInfo = (InstallerInfo) serializer.Deserialize(reader);
            //}

            return _installerInfo;
        }
    }

    public class EventLogInfo
    {
        public EventLogInfo()
        {
        }

        public EventLogInfo(string source)
        {
            Source = source;
        }

        [XmlAttribute("source")]
        public string Source { get; set; } = "";

        [XmlAttribute("id")]
        public string ID { get; set; } = "";
    }

    public class PerformanceCounterCategoryInfo
    {
        public PerformanceCounterCategoryInfo()
        {
        }

        public PerformanceCounterCategoryInfo(string categoryName, string categoryHelp,
            CounterCreationDataInfo[] counterCreationDataInfos)
        {
            CategoryHelp = categoryHelp;
            CategoryName = categoryName;
            CounterCreationDataInfos = counterCreationDataInfos;
        }

        [XmlAttribute("categoryHelp")]
        public string CategoryHelp { get; set; }

        [XmlAttribute("categoryName")]
        public string CategoryName { get; set; }

        public CounterCreationDataInfo[] CounterCreationDataInfos { get; set; }
    }

    public class CounterCreationDataInfo
    {
        public CounterCreationDataInfo()
        {
        }

        public CounterCreationDataInfo(string counterName, string instanceName, PerformanceCounterType counterType, string counterHelp)
        {
            CounterName = counterName;
            InstanceName = instanceName;
            CounterHelp = counterHelp;
            CounterType = counterType;
        }

        [XmlAttribute("counterHelp")]
        public string CounterHelp { get; set; }

        [XmlAttribute("counterName")]
        public string CounterName { get; set; }

        [XmlAttribute("instanceName")]
        public string InstanceName { get; set; }

        [XmlAttribute("id")]
        public string ID { get; set; }

        [XmlAttribute("counterType")]
        public PerformanceCounterType CounterType { get; set; }
    }
}