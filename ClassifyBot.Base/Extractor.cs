﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CommandLine;
using Newtonsoft.Json;
using Serilog;

namespace ClassifyBot
{
    public abstract class Extractor<TRecord, TFeature> : Stage, IExtractor<TRecord, TFeature> 
        where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public Extractor()
        {
            Contract.Requires(!OutputFileName.Empty());
            
        }
        #endregion

        #region Overriden members
        public override FileInfo OutputFile => OutputFileName.Empty() ? null : new FileInfo(OutputFileName);

        public override StageResult Run()
        {
            StageResult r;
            if ((r = Init()) != StageResult.SUCCESS)
            {
                return r; 
            }
            if ((r = Extract()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Save()) != StageResult.SUCCESS)
            {
                return r;
            }
            Cleanup();
            return StageResult.SUCCESS;
        }

        protected override StageResult Init()
        {
            if (!InputFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }
            else
            {
                return StageResult.SUCCESS;
            }
        }
        #endregion

        #region Abstract methods
        public abstract StageResult Extract(int? recordBatchSize = null, int? recordLimit = null, Dictionary<string, string> options = null);
        #endregion

        #region Properties
        public List<TRecord> ExtractedRecords { get; protected set; } = new List<TRecord>();
        #endregion

        #region Methods
        protected override StageResult Save()
        {
            Contract.Requires(ExtractedRecords != null);
            if (ExtractedRecords.Count == 0)
            {
                Warn("0 records extracted from file {0}. Not writing to output file.", InputFile.FullName);
                return StageResult.SUCCESS;
            }
            Contract.Requires(OutputFile != null && OutputFile.Exists);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            if (!CompressOutputFile)
            {
                using (StreamWriter sw = new StreamWriter(OutputFile.FullName, false, Encoding.UTF8))
                {
                    serializer.Serialize(sw, ExtractedRecords);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(OutputFile.FullName, FileMode.Create))
                using (GZipStream gzs = new GZipStream(fs, CompressionMode.Compress))
                using (StreamWriter sw = new StreamWriter(gzs, Encoding.UTF8))
                {
                    serializer.Serialize(sw, ExtractedRecords);
                }
            }
            Info("Wrote {0} records to {file}", ExtractedRecords.Count, OutputFile.FullName);
            return StageResult.SUCCESS;
        }
        #endregion
    }
}
