using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using ProjectExtensions.Azure.ServiceBus;
using System.Threading.Tasks;
using NLog.Config;
using NLog.Targets;
using NLog;
using System.Diagnostics;
using System.Transactions;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Xml;

namespace PubSubUsingConfiguration {

    class Program {

        static LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(10);
        static TaskFactory taskFactory = new TaskFactory(lcts);

        static void Main(string[] args) {

            SetupLogging();

            //you can either call .ReadFromConfigFile() or set the ServiceSub Methods in code.
            //
            //<add key="ServiceBusIssuerKey" value="base64hash" />
            //<add key="ServiceBusIssuerName" value="owner" />
            //https://addresshere.servicebus.windows.net/
            //<add key="ServiceBusNamespace" value="namespace set up in service bus (addresshere) portion" />

            ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
                .ReadFromConfigFile()
                .ServiceBusApplicationId("AppName")
                //.ServiceBusIssuerKey("[sb password]")
                //.ServiceBusIssuerName("owner")
                //.ServiceBusNamespace("[addresshere]")
                .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
                .DefaultSerializer(new GZipXmlSerializer())
                .Configure();

            SetupClients(20, 25);

            Console.WriteLine("You must wait for the messages to be processed");
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }

        public static void SetupClients(int count, int messageCount) {

            for (int threadCount = 0; threadCount < count; threadCount++) {
                //var t = new Thread(
                Action action = () => {
                    var client = Guid.NewGuid();

                    for (int i = 0; i < messageCount; i++) {
                        //var sw = new Stopwatch();
                        //sw.Start();
                        var message1 = new TestMessage() {
                            Value = i,
                            Time = DateTime.Now,
                            MessageId = client.ToString()
                        };
                        BusConfiguration.Instance.Bus.PublishAsync(message1, (result) => {
                            Console.WriteLine(result.TimeSpent);
                        }, null);
                        //BusConfiguration.Instance.Bus.Publish(message1, null);
                    }
                };
                taskFactory.StartNew(action);
            }
        }

        private static void SetupLogging() {
            // Step 1. Create configuration object 
            LoggingConfiguration config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);
            fileTarget.FileName = "${basedir}/file.txt";

            // Step 3. Set target properties 
            consoleTarget.Layout = "${date:format=HH\\:mm\\:ss} ${logger} ${message}";
            fileTarget.Layout = "${date:format=HH\\:mm\\:ss} ${logger} ${message}";

            // Step 4. Define rules
            LoggingRule rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule);

            rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }
    }

    public class GZipXmlSerializer : ServiceBusSerializerBase {

        static Logger logger = LogManager.GetCurrentClassLogger();
        MemoryStream serializedStream;

        public override IServiceBusSerializer Create() {
            return new GZipXmlSerializer();
        }

        public override Stream Serialize(object obj) {
            var serial = new XmlSerializer(obj.GetType());
            serializedStream = new MemoryStream();

            using (var zipStream = new GZipStream(serializedStream, CompressionMode.Compress, true)) {
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(zipStream, null, null, false)) {
                    serial.Serialize(writer, obj);
                }
            }

            serializedStream.Position = 0; //make sure you always set the stream position to where you want to serialize.
            logger.Log(LogLevel.Info, "Serialize {0} at Bytes={1}", obj.GetType(), serializedStream.Length);
            return serializedStream;
        }

        public override object Deserialize(Stream stream, Type type) {
            var serial = new XmlSerializer(type);
            logger.Log(LogLevel.Info, "Deserialize {0} at {1} bytes", type, stream.Length);
            using (var zipStream = new GZipStream(stream, CompressionMode.Decompress, true)) {
                using (var reader = XmlDictionaryReader.CreateBinaryReader(zipStream, XmlDictionaryReaderQuotas.Max)) {
                    return serial.Deserialize(reader);
                }
            }
        }

        public override void Dispose() {
            if (serializedStream != null) {
                serializedStream.Dispose();
                serializedStream = null;
            }
        }
    }

    /// <summary>
    /// Provides a task scheduler that ensures a maximum concurrency level while
    /// running on top of the ThreadPool.
    /// </summary>
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler {

        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Whether the current thread is processing work items.</summary>
        [ThreadStatic]
        static bool _currentThreadIsProcessingItems;
        /// <summary>The list of tasks to be executed.</summary>
        readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)
        /// <summary>The maximum concurrency level allowed by this scheduler.</summary>
        readonly int _maxDegreeOfParallelism;
        /// <summary>Whether the scheduler is currently processing work items.</summary>
        int _delegatesQueuedOrRunning = 0; // protected by lock(_tasks)

        public int TasksQueued {
            get {
                int retVal = 0;
                lock (_tasks) {
                    retVal = _tasks.Count;
                }
                return retVal;
            }
        }

        /// <summary>
        /// Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the
        /// specified degree of parallelism.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism provided by this scheduler.</param>
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism) {
            if (maxDegreeOfParallelism < 1)
                throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>Queues a task to the scheduler.</summary>
        /// <param name="task">The task to be queued.</param>
        protected sealed override void QueueTask(Task task) {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks) {
                logger.Log(LogLevel.Info, "QueueTask {0}", task.Id);
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism) {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                    logger.Log(LogLevel.Debug, "QueueTask NotifyThreadPoolOfPendingWork {0}", _delegatesQueuedOrRunning);
                }
            }
        }

        /// <summary>
        /// Informs the ThreadPool that there's work to be executed for this scheduler.
        /// </summary>
        void NotifyThreadPoolOfPendingWork() {
            ThreadPool.UnsafeQueueUserWorkItem(_ => {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try {
                    // Process all available items in the queue.
                    while (true) {
                        Task item;
                        lock (_tasks) {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0) {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally {
                    _currentThreadIsProcessingItems = false;
                }
            }, null);
        }

        /// <summary>Attempts to execute the specified task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued"></param>
        /// <returns>Whether the task could be executed on the current thread.</returns>
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems)
                return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                TryDequeue(task);

            // Try to run the task.
            return base.TryExecuteTask(task);
        }

        /// <summary>Attempts to remove a previously scheduled task from the scheduler.</summary>
        /// <param name="task">The task to be removed.</param>
        /// <returns>Whether the task could be found and removed.</returns>
        protected sealed override bool TryDequeue(Task task) {
            lock (_tasks)
                return _tasks.Remove(task);
        }

        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public sealed override int MaximumConcurrencyLevel {
            get {
                return _maxDegreeOfParallelism;
            }
        }

        /// <summary>Gets an enumerable of the tasks currently scheduled on this scheduler.</summary>
        /// <returns>An enumerable of the tasks currently scheduled.</returns>
        protected sealed override IEnumerable<Task> GetScheduledTasks() {
            bool lockTaken = false;
            try {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken)
                    return _tasks.ToArray();
                else
                    throw new NotSupportedException();
            }
            finally {
                if (lockTaken)
                    Monitor.Exit(_tasks);
            }
        }
    }
}
