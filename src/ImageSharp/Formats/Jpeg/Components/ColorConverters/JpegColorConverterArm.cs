// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace SixLabors.ImageSharp.Formats.Jpeg.Components;

internal abstract partial class JpegColorConverterBase
{
#if USE_SIMD_INTRINSICS
    /// <summary>
    /// <see cref="JpegColorConverterBase"/> abstract base for implementations
    /// based on <see cref="Avx"/> instructions.
    /// </summary>
    /// <remarks>
    /// Converters of this family would expect input buffers lengths to be
    /// divisible by 8 without a remainder.
    /// This is guaranteed by real-life data as jpeg stores pixels via 8x8 blocks.
    /// DO NOT pass test data of invalid size to these converters as they
    /// potentially won't do a bound check and return a false positive result.
    /// </remarks>
    internal abstract class JpegColorConverterArm : JpegColorConverterBase
    {
        protected JpegColorConverterArm(JpegColorSpace colorSpace, int precision)
            : base(colorSpace, precision)
        {
        }

        public static bool IsSupported => AdvSimd.IsSupported;

        public sealed override bool IsAvailable => IsSupported;

        public sealed override int ElementsPerBatch => Vector128<float>.Count;
    }
#endif
}
