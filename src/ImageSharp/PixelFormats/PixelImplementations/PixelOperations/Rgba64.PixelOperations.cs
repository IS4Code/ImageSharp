// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Formats;

namespace SixLabors.ImageSharp.PixelFormats;

/// <content>
/// Provides optimized overrides for bulk operations.
/// </content>
public partial struct Rgba64
{
    /// <summary>
    /// Provides optimized overrides for bulk operations.
    /// </summary>
    internal partial class PixelOperations : PixelOperations<Rgba64>;
}
