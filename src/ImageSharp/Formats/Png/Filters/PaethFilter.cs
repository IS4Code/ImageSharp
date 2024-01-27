// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace SixLabors.ImageSharp.Formats.Png.Filters;

/// <summary>
/// The Paeth filter computes a simple linear function of the three neighboring pixels (left, above, upper left),
/// then chooses as predictor the neighboring pixel closest to the computed value.
/// This technique is due to Alan W. Paeth.
/// <see href="https://www.w3.org/TR/PNG-Filters.html"/>
/// </summary>
internal static class PaethFilter
{
    /// <summary>
    /// Decodes a scanline, which was filtered with the paeth filter.
    /// </summary>
    /// <param name="scanline">The scanline to decode.</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="bytesPerPixel">The bytes per pixel.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Decode(Span<byte> scanline, Span<byte> previousScanline, int bytesPerPixel)
    {
        DebugGuard.MustBeSameSized<byte>(scanline, previousScanline, nameof(scanline));

        // Paeth tries to predict pixel d using the pixel to the left of it, a,
        // and two pixels from the previous row, b and c:
        // prev: c b
        // row:  a d
        // The Paeth function predicts d to be whichever of a, b, or c is nearest to
        // p = a + b - c.
#if USE_SIMD_INTRINSICS
        if (Ssse3.IsSupported && bytesPerPixel is 4)
        {
            DecodeSsse3(scanline, previousScanline);
        }
        else if (AdvSimd.Arm64.IsSupported && bytesPerPixel is 4)
        {
            DecodeArm(scanline, previousScanline);
        }
        else
#endif
        {
            DecodeScalar(scanline, previousScanline, (uint)bytesPerPixel);
        }
    }

#if USE_SIMD_INTRINSICS
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeSsse3(Span<byte> scanline, Span<byte> previousScanline)
    {
        ref byte scanBaseRef = ref MemoryMarshal.GetReference(scanline);
        ref byte prevBaseRef = ref MemoryMarshal.GetReference(previousScanline);

        Vector128<byte> b = Vector128<byte>.Zero;
        Vector128<byte> d = Vector128<byte>.Zero;

        int rb = scanline.Length;
        nuint offset = 1;
        while (rb >= 4)
        {
            ref byte scanRef = ref Extensions.UnsafeAdd(ref scanBaseRef, offset);

            // It's easiest to do this math (particularly, deal with pc) with 16-bit intermediates.
            Vector128<byte> c = b;
            Vector128<byte> a = d;
            b = Sse2.UnpackLow(
                Sse2.ConvertScalarToVector128Int32(Unsafe.As<byte, int>(ref Extensions.UnsafeAdd(ref prevBaseRef, offset))).AsByte(),
                Vector128<byte>.Zero);
            d = Sse2.UnpackLow(
                Sse2.ConvertScalarToVector128Int32(Unsafe.As<byte, int>(ref scanRef)).AsByte(),
                Vector128<byte>.Zero);

            // (p-a) == (a+b-c - a) == (b-c)
            Vector128<short> pa = Sse2.Subtract(b.AsInt16(), c.AsInt16());

            // (p-b) == (a+b-c - b) == (a-c)
            Vector128<short> pb = Sse2.Subtract(a.AsInt16(), c.AsInt16());

            // (p-c) == (a+b-c - c) == (a+b-c-c) == (b-c)+(a-c)
            Vector128<short> pc = Sse2.Add(pa.AsInt16(), pb.AsInt16());

            pa = Ssse3.Abs(pa.AsInt16()).AsInt16(); /* |p-a| */
            pb = Ssse3.Abs(pb.AsInt16()).AsInt16(); /* |p-b| */
            pc = Ssse3.Abs(pc.AsInt16()).AsInt16(); /* |p-c| */

            Vector128<short> smallest = Sse2.Min(pc, Sse2.Min(pa, pb));

            // Paeth breaks ties favoring a over b over c.
            Vector128<byte> mask = SimdUtils.HwIntrinsics.BlendVariable(c, b, Sse2.CompareEqual(smallest, pb).AsByte());
            Vector128<byte> nearest = SimdUtils.HwIntrinsics.BlendVariable(mask, a, Sse2.CompareEqual(smallest, pa).AsByte());

            // Note `_epi8`: we need addition to wrap modulo 255.
            d = Sse2.Add(d, nearest);

            // Store the result.
            Unsafe.As<byte, int>(ref scanRef) = Sse2.ConvertToInt32(Sse2.PackUnsignedSaturate(d.AsInt16(), d.AsInt16()).AsInt32());

            rb -= 4;
            offset += 4;
        }
    }

    public static void DecodeArm(Span<byte> scanline, Span<byte> previousScanline)
    {
        ref byte scanBaseRef = ref MemoryMarshal.GetReference(scanline);
        ref byte prevBaseRef = ref MemoryMarshal.GetReference(previousScanline);

        Vector128<byte> b = Vector128<byte>.Zero;
        Vector128<byte> d = Vector128<byte>.Zero;

        int rb = scanline.Length;
        nuint offset = 1;
        const int bytesPerBatch = 4;
        while (rb >= bytesPerBatch)
        {
            ref byte scanRef = ref Extensions.UnsafeAdd(ref scanBaseRef, offset);
            Vector128<byte> c = b;
            Vector128<byte> a = d;
            b = AdvSimd.Arm64.ZipLow(
                Vector128.CreateScalar(Unsafe.As<byte, int>(ref Extensions.UnsafeAdd(ref prevBaseRef, offset))).AsByte(),
                Vector128<byte>.Zero).AsByte();
            d = AdvSimd.Arm64.ZipLow(
                Vector128.CreateScalar(Unsafe.As<byte, int>(ref scanRef)).AsByte(),
                Vector128<byte>.Zero).AsByte();

            // (p-a) == (a+b-c - a) == (b-c)
            Vector128<short> pa = AdvSimd.Subtract(b.AsInt16(), c.AsInt16());

            // (p-b) == (a+b-c - b) == (a-c)
            Vector128<short> pb = AdvSimd.Subtract(a.AsInt16(), c.AsInt16());

            // (p-c) == (a+b-c - c) == (a+b-c-c) == (b-c)+(a-c)
            Vector128<short> pc = AdvSimd.Add(pa.AsInt16(), pb.AsInt16());

            pa = AdvSimd.Abs(pa.AsInt16()).AsInt16(); /* |p-a| */
            pb = AdvSimd.Abs(pb.AsInt16()).AsInt16(); /* |p-b| */
            pc = AdvSimd.Abs(pc.AsInt16()).AsInt16(); /* |p-c| */

            Vector128<short> smallest = AdvSimd.Min(pc, AdvSimd.Min(pa, pb));

            // Paeth breaks ties favoring a over b over c.
            Vector128<byte> mask = SimdUtils.HwIntrinsics.BlendVariable(c, b, AdvSimd.CompareEqual(smallest, pb).AsByte());
            Vector128<byte> nearest = SimdUtils.HwIntrinsics.BlendVariable(mask, a, AdvSimd.CompareEqual(smallest, pa).AsByte());

            d = AdvSimd.Add(d, nearest);

            Vector64<byte> e = AdvSimd.ExtractNarrowingSaturateUnsignedLower(d.AsInt16());

            Unsafe.As<byte, int>(ref scanRef) = Vector128.Create(e, e).AsInt32().ToScalar();

            rb -= bytesPerBatch;
            offset += bytesPerBatch;
        }
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeScalar(Span<byte> scanline, Span<byte> previousScanline, uint bytesPerPixel)
    {
        ref byte scanBaseRef = ref MemoryMarshal.GetReference(scanline);
        ref byte prevBaseRef = ref MemoryMarshal.GetReference(previousScanline);

        // Paeth(x) + PaethPredictor(Raw(x-bpp), Prior(x), Prior(x-bpp))
        nuint offset = bytesPerPixel + 1; // Add one because x starts at one.
        nuint x = 1;
        for (; x < offset; x++)
        {
            ref byte scan = ref Extensions.UnsafeAdd(ref scanBaseRef, x);
            byte above = Extensions.UnsafeAdd(ref prevBaseRef, x);
            scan = (byte)(scan + above);
        }

        for (; x < (uint)scanline.Length; x++)
        {
            ref byte scan = ref Extensions.UnsafeAdd(ref scanBaseRef, x);
            byte left = Extensions.UnsafeAdd(ref scanBaseRef, x - bytesPerPixel);
            byte above = Extensions.UnsafeAdd(ref prevBaseRef, x);
            byte upperLeft = Extensions.UnsafeAdd(ref prevBaseRef, x - bytesPerPixel);
            scan = (byte)(scan + PaethPredictor(left, above, upperLeft));
        }
    }

    /// <summary>
    /// Encodes a scanline and applies the paeth filter.
    /// </summary>
    /// <param name="scanline">The scanline to encode</param>
    /// <param name="previousScanline">The previous scanline.</param>
    /// <param name="result">The filtered scanline result.</param>
    /// <param name="bytesPerPixel">The bytes per pixel.</param>
    /// <param name="sum">The sum of the total variance of the filtered row.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Encode(ReadOnlySpan<byte> scanline, ReadOnlySpan<byte> previousScanline, Span<byte> result, int bytesPerPixel, out int sum)
    {
        DebugGuard.MustBeSameSized(scanline, previousScanline, nameof(scanline));
        DebugGuard.MustBeSizedAtLeast(result, scanline, nameof(result));

        ref byte scanBaseRef = ref MemoryMarshal.GetReference(scanline);
        ref byte prevBaseRef = ref MemoryMarshal.GetReference(previousScanline);
        ref byte resultBaseRef = ref MemoryMarshal.GetReference(result);
        sum = 0;

        // Paeth(x) = Raw(x) - PaethPredictor(Raw(x-bpp), Prior(x), Prior(x - bpp))
        resultBaseRef = (byte)FilterType.Paeth;

        nuint x = 0;
        for (; x < (uint)bytesPerPixel; /* Note: ++x happens in the body to avoid one add operation */)
        {
            byte scan = Extensions.UnsafeAdd(ref scanBaseRef, x);
            byte above = Extensions.UnsafeAdd(ref prevBaseRef, x);
            ++x;
            ref byte res = ref Extensions.UnsafeAdd(ref resultBaseRef, x);
            res = (byte)(scan - PaethPredictor(0, above, 0));
            sum += Numerics.Abs(unchecked((sbyte)res));
        }

#if USE_SIMD_INTRINSICS
        if (Avx2.IsSupported)
        {
            Vector256<byte> zero = Vector256<byte>.Zero;
            Vector256<int> sumAccumulator = Vector256<int>.Zero;

            for (nuint xLeft = x - (uint)bytesPerPixel; (int)x <= scanline.Length - Vector256<byte>.Count; xLeft += (uint)Vector256<byte>.Count)
            {
                Vector256<byte> scan = Unsafe.As<byte, Vector256<byte>>(ref Extensions.UnsafeAdd(ref scanBaseRef, x));
                Vector256<byte> left = Unsafe.As<byte, Vector256<byte>>(ref Extensions.UnsafeAdd(ref scanBaseRef, xLeft));
                Vector256<byte> above = Unsafe.As<byte, Vector256<byte>>(ref Extensions.UnsafeAdd(ref prevBaseRef, x));
                Vector256<byte> upperLeft = Unsafe.As<byte, Vector256<byte>>(ref Extensions.UnsafeAdd(ref prevBaseRef, xLeft));

                Vector256<byte> res = Avx2.Subtract(scan, PaethPredictor(left, above, upperLeft));
                Unsafe.As<byte, Vector256<byte>>(ref Extensions.UnsafeAdd(ref resultBaseRef, x + 1)) = res; // +1 to skip filter type
                x += (uint)Vector256<byte>.Count;

                sumAccumulator = Avx2.Add(sumAccumulator, Avx2.SumAbsoluteDifferences(Avx2.Abs(res.AsSByte()), zero).AsInt32());
            }

            sum += Numerics.EvenReduceSum(sumAccumulator);
        }
        else
#endif
        if (Vector.IsHardwareAccelerated)
        {
            Vector<uint> sumAccumulator = Vector<uint>.Zero;

            for (nuint xLeft = x - (uint)bytesPerPixel; (int)x <= scanline.Length - Vector<byte>.Count; xLeft += (uint)Vector<byte>.Count)
            {
                Vector<byte> scan = Unsafe.As<byte, Vector<byte>>(ref Extensions.UnsafeAdd(ref scanBaseRef, x));
                Vector<byte> left = Unsafe.As<byte, Vector<byte>>(ref Extensions.UnsafeAdd(ref scanBaseRef, xLeft));
                Vector<byte> above = Unsafe.As<byte, Vector<byte>>(ref Extensions.UnsafeAdd(ref prevBaseRef, x));
                Vector<byte> upperLeft = Unsafe.As<byte, Vector<byte>>(ref Extensions.UnsafeAdd(ref prevBaseRef, xLeft));

                Vector<byte> res = scan - PaethPredictor(left, above, upperLeft);
                Unsafe.As<byte, Vector<byte>>(ref Extensions.UnsafeAdd(ref resultBaseRef, x + 1)) = res; // +1 to skip filter type
                x += (uint)Vector<byte>.Count;

                Numerics.Accumulate(ref sumAccumulator, Vector.AsVectorByte(Vector.Abs(Vector.AsVectorSByte(res))));
            }

            for (int i = 0; i < Vector<uint>.Count; i++)
            {
                sum += (int)sumAccumulator[i];
            }
        }

        for (nuint xLeft = x - (uint)bytesPerPixel; (int)x < scanline.Length; ++xLeft /* Note: ++x happens in the body to avoid one add operation */)
        {
            byte scan = Extensions.UnsafeAdd(ref scanBaseRef, x);
            byte left = Extensions.UnsafeAdd(ref scanBaseRef, xLeft);
            byte above = Extensions.UnsafeAdd(ref prevBaseRef, x);
            byte upperLeft = Extensions.UnsafeAdd(ref prevBaseRef, xLeft);
            ++x;
            ref byte res = ref Extensions.UnsafeAdd(ref resultBaseRef, x);
            res = (byte)(scan - PaethPredictor(left, above, upperLeft));
            sum += Numerics.Abs(unchecked((sbyte)res));
        }
    }

    /// <summary>
    /// Computes a simple linear function of the three neighboring pixels (left, above, upper left), then chooses
    /// as predictor the neighboring pixel closest to the computed value.
    /// </summary>
    /// <param name="left">The left neighbor pixel.</param>
    /// <param name="above">The above neighbor pixel.</param>
    /// <param name="upperLeft">The upper left neighbor pixel.</param>
    /// <returns>
    /// The <see cref="byte"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte PaethPredictor(byte left, byte above, byte upperLeft)
    {
        int p = left + above - upperLeft;
        int pa = Numerics.Abs(p - left);
        int pb = Numerics.Abs(p - above);
        int pc = Numerics.Abs(p - upperLeft);

        if (pa <= pb && pa <= pc)
        {
            return left;
        }

        if (pb <= pc)
        {
            return above;
        }

        return upperLeft;
    }

#if USE_SIMD_INTRINSICS
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<byte> PaethPredictor(Vector256<byte> left, Vector256<byte> above, Vector256<byte> upleft)
    {
        Vector256<byte> zero = Vector256<byte>.Zero;

        // Here, we refactor pa = abs(p - left) = abs(left + above - upleft - left)
        // to pa = abs(above - upleft).  Same deal for pb.
        // Using saturated subtraction, if the result is negative, the output is zero.
        // If we subtract in both directions and `or` the results, only one can be
        // non-zero, so we end up with the absolute value.
        Vector256<byte> sac = Avx2.SubtractSaturate(above, upleft);
        Vector256<byte> sbc = Avx2.SubtractSaturate(left, upleft);
        Vector256<byte> pa = Avx2.Or(Avx2.SubtractSaturate(upleft, above), sac);
        Vector256<byte> pb = Avx2.Or(Avx2.SubtractSaturate(upleft, left), sbc);

        // pc = abs(left + above - upleft - upleft), or abs(left - upleft + above - upleft).
        // We've already calculated left - upleft and above - upleft in `sac` and `sbc`.
        // If they are both negative or both positive, the absolute value of their
        // sum can't possibly be less than `pa` or `pb`, so we'll never use the value.
        // We make a mask that sets the value to 255 if they either both got
        // saturated to zero or both didn't.  Then we calculate the absolute value
        // of their difference using saturated subtract and `or`, same as before,
        // keeping the value only where the mask isn't set.
        Vector256<byte> pm = Avx2.CompareEqual(Avx2.CompareEqual(sac, zero), Avx2.CompareEqual(sbc, zero));
        Vector256<byte> pc = Avx2.Or(pm, Avx2.Or(Avx2.SubtractSaturate(pb, pa), Avx2.SubtractSaturate(pa, pb)));

        // Finally, blend the values together.  We start with `upleft` and overwrite on
        // tied values so that the `left`, `above`, `upleft` precedence is preserved.
        Vector256<byte> minbc = Avx2.Min(pc, pb);
        Vector256<byte> resbc = Avx2.BlendVariable(upleft, above, Avx2.CompareEqual(minbc, pb));
        return Avx2.BlendVariable(resbc, left, Avx2.CompareEqual(Avx2.Min(minbc, pa), pa));
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<byte> PaethPredictor(Vector<byte> left, Vector<byte> above, Vector<byte> upperLeft)
    {
        Vector.Widen(left, out Vector<ushort> a1, out Vector<ushort> a2);
        Vector.Widen(above, out Vector<ushort> b1, out Vector<ushort> b2);
        Vector.Widen(upperLeft, out Vector<ushort> c1, out Vector<ushort> c2);

        Vector<short> p1 = PaethPredictor(Vector.AsVectorInt16(a1), Vector.AsVectorInt16(b1), Vector.AsVectorInt16(c1));
        Vector<short> p2 = PaethPredictor(Vector.AsVectorInt16(a2), Vector.AsVectorInt16(b2), Vector.AsVectorInt16(c2));
        return Vector.AsVectorByte(Vector.Narrow(p1, p2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<short> PaethPredictor(Vector<short> left, Vector<short> above, Vector<short> upperLeft)
    {
        Vector<short> p = left + above - upperLeft;
        Vector<short> pa = Vector.Abs(p - left);
        Vector<short> pb = Vector.Abs(p - above);
        Vector<short> pc = Vector.Abs(p - upperLeft);

        Vector<short> pa_pb = Vector.LessThanOrEqual(pa, pb);
        Vector<short> pa_pc = Vector.LessThanOrEqual(pa, pc);
        Vector<short> pb_pc = Vector.LessThanOrEqual(pb, pc);

        return Vector.ConditionalSelect(
            condition: Vector.BitwiseAnd(pa_pb, pa_pc),
            left: left,
            right: Vector.ConditionalSelect(
                condition: pb_pc,
                left: above,
                right: upperLeft));
    }
}
