﻿// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

namespace SixLabors.ImageSharp.Processing.Processors.Filters;

/// <summary>
/// Applies a black and white filter matrix to the image.
/// </summary>
public sealed class BlackWhiteProcessor : FilterProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlackWhiteProcessor"/> class.
    /// </summary>
    public BlackWhiteProcessor()
        : base(KnownFilterMatrices.BlackWhiteFilter)
    {
    }
}
