// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static SixLabors.ImageSharp.SimdUtils;

namespace SixLabors.ImageSharp.Formats.Jpeg.Components;

internal abstract partial class JpegColorConverterBase
{
#if USE_SIMD_INTRINSICS
    internal sealed class GrayscaleAvx : JpegColorConverterAvx
    {
        public GrayscaleAvx(int precision)
            : base(JpegColorSpace.Grayscale, precision)
        {
        }

        /// <inheritdoc/>
        public override void ConvertToRgbInplace(in ComponentValues values)
        {
            ref Vector256<float> c0Base =
                ref Unsafe.As<float, Vector256<float>>(ref MemoryMarshal.GetReference(values.Component0));

            // Used for the color conversion
            var scale = Vector256.Create(1 / this.MaximumValue);

            nuint n = values.Component0.Vector256Count<float>();
            for (nuint i = 0; i < n; i++)
            {
                ref Vector256<float> c0 = ref Extensions.UnsafeAdd(ref c0Base, i);
                c0 = Avx.Multiply(c0, scale);
            }
        }

        /// <inheritdoc/>
        public override void ConvertFromRgb(in ComponentValues values, Span<float> rLane, Span<float> gLane, Span<float> bLane)
        {
            ref Vector256<float> destLuminance =
                ref Unsafe.As<float, Vector256<float>>(ref MemoryMarshal.GetReference(values.Component0));

            ref Vector256<float> srcRed =
                ref Unsafe.As<float, Vector256<float>>(ref MemoryMarshal.GetReference(rLane));
            ref Vector256<float> srcGreen =
                ref Unsafe.As<float, Vector256<float>>(ref MemoryMarshal.GetReference(gLane));
            ref Vector256<float> srcBlue =
                ref Unsafe.As<float, Vector256<float>>(ref MemoryMarshal.GetReference(bLane));

            // Used for the color conversion
            var f0299 = Vector256.Create(0.299f);
            var f0587 = Vector256.Create(0.587f);
            var f0114 = Vector256.Create(0.114f);

            nuint n = values.Component0.Vector256Count<float>();
            for (nuint i = 0; i < n; i++)
            {
                ref Vector256<float> r = ref Extensions.UnsafeAdd(ref srcRed, i);
                ref Vector256<float> g = ref Extensions.UnsafeAdd(ref srcGreen, i);
                ref Vector256<float> b = ref Extensions.UnsafeAdd(ref srcBlue, i);

                // luminocity = (0.299 * r) + (0.587 * g) + (0.114 * b)
                Extensions.UnsafeAdd(ref destLuminance, i) = HwIntrinsics.MultiplyAdd(HwIntrinsics.MultiplyAdd(Avx.Multiply(f0114, b), f0587, g), f0299, r);
            }
        }
    }
#endif
}
