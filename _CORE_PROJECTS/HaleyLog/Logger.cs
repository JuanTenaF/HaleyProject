﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Security.AccessControl;
using Haley.Abstractions;
using Haley.Models;
using System.ComponentModel;
using Haley.Enums;

namespace Haley.Log
{
    public sealed class HLog : LoggerBase
    {
        #region ATTRIBUTES
        private const string SUBLOGKEY = "SUBLOG_PLACEHOLDER";
        private List<LogBase> memoryStore;
        private int auto_dump_count;
        private bool should_auto_dump;
        #endregion

        #region Private Build Methods
        private LogBase _buildInfo(string message, string prop_name, MessageType msg_type = MessageType.information)
        {
            LogBase _result = new LogBase();
            _result.Name = prop_name ?? string.Empty;
            _result.Message = message;
            _result.MessageType = msg_type;
            _result.TimeStamp = DateTime.UtcNow;
            return _result;
        }

        private LogBase _buildException(Exception _exception, string prop_name, string comments)
        {
            ExceptionLog _result = new ExceptionLog();
            _result.Name = prop_name ?? string.Empty;
            _result.Source = _exception.Source;
            _result.Trace = _exception.StackTrace;
            _result.ExceptionMessage = _exception.Message;
            _result.Message = comments ?? string.Empty;
            _result.TimeStamp = DateTime.UtcNow;
            _result.MessageType = MessageType.exception;
            return _result;
        }

        private LogBase _buildKVP(string key, string value, string prop_name, string comments)
        {
            DictionaryLog _result = new DictionaryLog();
            _result.Name = prop_name;
            _result.Message = comments ?? string.Empty;
            _result.Key = key;
            _result.Value = value;
            _result.TimeStamp = DateTime.UtcNow;
            _result.MessageType = MessageType.property;
            return _result;
        }
        #endregion

        #region Private Helper Methods
        private void _autoDump()
        {
            if (!should_auto_dump) return; //No further validation required.
            if (_memoryStoreCount(memoryStore) > auto_dump_count) dumpMemory();
        }

        private int _memoryStoreCount(List<LogBase> source)
        {
            int basecount = source.Count + source.Sum(p=> _memoryStoreCount(p.Children));
            return basecount; //This should give count value of recursive items.
        }

        private void _log (LogBase input, bool in_memory, bool is_sub=false)
        {
            if (in_memory) //Only if we are adding items to memory, we shoudl care about autodumping
            {
                _autoDump();
                if(!is_sub)
                {
                    memoryStore.Add(input); //Storing to the memory
                }
                else
                {
                    //Sub should always be added to last item in memory.
                    LogBase last_node;
                   if (memoryStore.Count > 0)
                    {
                        last_node = memoryStore.Last(); //Get last node
                    }
                   else
                    {
                        last_node = _buildInfo("", prop_name: SUBLOGKEY);
                        //add the newly created node to the memory store
                        memoryStore.Add(last_node);
                    }
                    last_node.Children.Add(input);
                }
            }
            else
            {
                //If it is not in memory, then we should dump whatever in memory irrespective of the count.
                if (memoryStore.Count > 0) dumpMemory();
               
                _writer.write(input,is_sub); //writing directly using the writer
            }
        }

        #endregion

        #region Overridden Methods
        /// <summary>
        /// Log the message
        /// </summary>
        /// <param name="message">String value of the message</param>
        /// <param name="msg_type">Type of message</param>
        /// <param name="property_name">Some associated property name</param>
        /// <param name="in_memory">If false, the data is written directly on to the file. If true, the date is stored in memory until dumped.</param>
        /// <returns>GUID value of the log messgae</returns>
        public override string log(string message, MessageType msg_type = MessageType.information, string property_name = null, bool in_memory = false, bool is_sub = false)
        {
            LogBase _infoLog = _buildInfo(message, property_name, msg_type);
            _log(_infoLog, in_memory,is_sub);
            return _infoLog.Id;
        }

        public override string log(Exception exception, string comments = null, string property_name = null, bool in_memory = false, bool is_sub = false)
        {
            LogBase _exceptionLog = _buildException(exception, property_name, comments);
            _log(_exceptionLog, in_memory, is_sub);
            return _exceptionLog.Id;
        }

        public override string log(string key, string value, string comments = null, string property_name = null, bool in_memory = false, bool is_sub = false)
        {
            LogBase _kvpLog = _buildKVP(key, value, property_name,comments);
            _log(_kvpLog, in_memory, is_sub);
            return _kvpLog.Id;
        }

        #endregion

        #region Public Methods

        public List<ILog> getMemoryStore()
        {
            return memoryStore.Cast<ILog>().ToList();
        }

        /// <summary>
        /// This uses the writer and converts the memory store in to its respective format and returns the object
        /// </summary>
        /// <returns></returns>
        public object getConvertedMemoryStore()
        {
            //this should use the converter to convert it to respective object and return it accordingly
            //The consumer sould take the responsibility to cast it accordingly
            return _writer.convert(memoryStore);
        }

        public void clearMemoryStore()
        {
            memoryStore.Clear();
        }

        /// <summary>
        /// Forced Memory Dump Method which dumps the memorystore data into file and then clears it
        /// </summary>
        public override void dumpMemory() //Should dump into wherever file that we write
        {
            _writer.write(memoryStore);
            clearMemoryStore();
        }
        
        #endregion

        #region Initiations
        /// <summary>
        /// Initialization logic for HLOG
        /// </summary>
        /// <param name="output_path">The location where the log file has to be stored</param>
        /// <param name="output_file_name">Name of the file</param>
        /// <param name="_type">File output type</param>
        /// <param name="auto_dump">If set to true, the data in memory is automatically dumped after every <paramref name="max_memory_count"/></param>
        /// <param name="max_memory_count">When memory data reaches this count, data in memory is dumped in to file. Minimum value should be 100 and maximum can be 5000</param>
        public HLog(string output_path,string output_file_name, OutputType _type, bool auto_dump = true, int max_memory_count = 100) :base(output_path, output_file_name, _type)
        {
            memoryStore = new List<LogBase>();
            should_auto_dump = auto_dump;
            auto_dump_count = (max_memory_count > 100 && max_memory_count < 5000) ? max_memory_count : 500;
        }

        #endregion

        #region Static Implementations
        private static HLog _ins;
        public static HLog ins { get { return _ins; }}
     
        public static void CreateInstance(HLog sourceLog)
        {
            _ins = sourceLog;
        }
        private static Dictionary<LogType, HLog> _store;
        public static HLog GetLog(LogType type)
        {
            if (_store == null) _store = new Dictionary<LogType, HLog>();
            if (!_store.ContainsKey(type)) return null;
            return _store[type];
        }
        public static bool AddLog(LogType type,HLog source)
        {
            if (_store == null) _store = new Dictionary<LogType, HLog>();
            if (_store.ContainsKey(type)) throw new ArgumentException($@"{type.ToString()} is already registered. Please assign a new key");
                _store.Add(type, source);
                return true;
        }
        #endregion
    }
}