﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

using Serilog;
using CommandLine;

namespace ClassifyBot
{
    public abstract class Classifier<TRecord, TFeature> : Stage, IClassifier<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        #region Constructors
        public Classifier() : base()
        {
            Contract.Requires(!TrainingFileName.Empty());
            Contract.Requires(!TestFileName.Empty());
            Contract.Requires(!ModelFileName.Empty());
        }
        #endregion

        #region Abstract members
        public abstract StageResult Train(Dictionary<string, string> options = null);
        #endregion

        #region Overidden members
        public override StageResult Run()
        {
            StageResult r;
            if ((r = Init()) != StageResult.SUCCESS)
            {
                return r;
            }
            if ((r = Train()) != StageResult.SUCCESS)
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
            Contract.Requires(TrainingFile != null && TestFile == null);
            if (!TrainingFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }
            if (!TestFile.CheckExistsAndReportError(L))
            {
                return StageResult.INPUT_ERROR;
            }
            if (TrainOp)
            {
                if (!ModelFile.Exists && !OverwriteOutputFile)
                {
                    Error("The model file {0} exists but the overwrite option was not specified.", ModelFile.FullName);
                    return StageResult.INPUT_ERROR;
                }
            }
            return StageResult.SUCCESS;
        }
        #endregion

        #region Properties
        public FileInfo TrainingFile => TrainingFileName.Empty() ? null : new FileInfo(TrainingFileName);

        public FileInfo TestFile => TestFileName.Empty() ? null : new FileInfo(TestFileName);

        public FileInfo ModelFile => ModelFileName.Empty() ? null : new FileInfo(ModelFileName);

        [Option('t', "training-file", Required = true, HelpText = "Input file name with training data for classifier")]
        public string TrainingFileName { get; set; }

        [Option('e', "test-file", Required = true, HelpText = "Input file name with test data for classifier")]
        public string TestFileName { get; set; }

        [Option('m', "model-file", Required = true, HelpText = "Output file name for classifier model.")]
        public string ModelFileName { get; set; }

        [Option("train", HelpText = "Train a classifier model using the training and test data files.", SetName = "op")]
        public bool TrainOp { get; set; }
        #endregion

        
    }
}