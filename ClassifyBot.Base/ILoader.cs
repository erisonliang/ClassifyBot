﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClassifyBot
{
    public interface ILoader<TRecord, TFeature> where TFeature : ICloneable, IComparable, IComparable<TFeature>, IConvertible, IEquatable<TFeature> where TRecord : Record<TFeature>
    {
        StageResult Run(Dictionary<string, object> options = null);
        FileInfo InputFile { get; }
        FileInfo TrainingFile { get; }
        FileInfo TestFile { get; }
    }
}
