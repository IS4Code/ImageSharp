// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Formats.Jpeg.Components;

internal abstract partial class JpegColorConverterBase
{
    internal sealed class GrayScaleVector : JpegColorConverterVector
    {
        public GrayScaleVector(int precision)
            : base(JpegColorSpace.Grayscale, precision)
        {
        }

        /// <inheritdoc/>
        protected override void ConvertToRgbInplaceVectorized(in ComponentValues values)
        {
            ref Vector<float> cBase =
                ref Unsafe.As<float, Vector<float>>(ref MemoryMarshal.GetReference(values.Component0));

            var scale = new Vector<float>(1 / this.MaximumValue);

            nuint n = values.Component0.VectorCount<float>();
            for (nuint i = 0; i < n; i++)
            {
                ref Vector<float> c0 = ref Extensions.UnsafeAdd(ref cBase, i);
                c0 *= scale;
            }
        }

        /// <inheritdoc/>
        protected override void ConvertToRgbInplaceScalarRemainder(in ComponentValues values)
            => GrayscaleScalar.ConvertToRgbInplace(values.Component0, this.MaximumValue);

        /// <inheritdoc/>
        protected override void ConvertFromRgbVectorized(in ComponentValues values, Span<float> rLane, Span<float> gLane, Span<float> bLane)
        {
            ref Vector<float> destLuma =
                ref Unsafe.As<float, Vector<float>>(ref MemoryMarshal.GetReference(values.Component0));

            ref Vector<float> srcR =
                ref Unsafe.As<float, Vector<float>>(ref MemoryMarshal.GetReference(rLane));
            ref Vector<float> srcG =
                ref Unsafe.As<float, Vector<float>>(ref MemoryMarshal.GetReference(gLane));
            ref Vector<float> srcB =
                ref Unsafe.As<float, Vector<float>>(ref MemoryMarshal.GetReference(bLane));

            var rMult = new Vector<float>(0.299f);
            var gMult = new Vector<float>(0.587f);
            var bMult = new Vector<float>(0.114f);

            nuint n = values.Component0.VectorCount<float>();
            for (nuint i = 0; i < n; i++)
            {
                Vector<float> r = Extensions.UnsafeAdd(ref srcR, i);
                Vector<float> g = Extensions.UnsafeAdd(ref srcR, i);
                Vector<float> b = Extensions.UnsafeAdd(ref srcR, i);

                // luminocity = (0.299 * r) + (0.587 * g) + (0.114 * b)
                Extensions.UnsafeAdd(ref destLuma, i) = (rMult * r) + (gMult * g) + (bMult * b);
            }
        }

        /// <inheritdoc/>
        protected override void ConvertFromRgbScalarRemainder(in ComponentValues values, Span<float> r, Span<float> g, Span<float> b)
            => GrayscaleScalar.ConvertCoreInplaceFromRgb(values, r, g, b);
    }
}
