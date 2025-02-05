// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Processing.Processors.Convolution;

/// <summary>
/// Defines a processor that uses two one-dimensional matrices to perform two-pass convolution against an image.
/// </summary>
/// <typeparam name="TPixel">The pixel format.</typeparam>
internal class Convolution2PassProcessor<TPixel> : ImageProcessor<TPixel>
    where TPixel : unmanaged, IPixel<TPixel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Convolution2PassProcessor{TPixel}"/> class.
    /// </summary>
    /// <param name="configuration">The configuration which allows altering default behaviour or extending the library.</param>
    /// <param name="kernel">The 1D convolution kernel.</param>
    /// <param name="preserveAlpha">Whether the convolution filter is applied to alpha as well as the color channels.</param>
    /// <param name="source">The source <see cref="Image{TPixel}"/> for the current processor instance.</param>
    /// <param name="sourceRectangle">The source area to process for the current processor instance.</param>
    /// <param name="borderWrapModeX">The <see cref="BorderWrappingMode"/> to use when mapping the pixels outside of the border, in X direction.</param>
    /// <param name="borderWrapModeY">The <see cref="BorderWrappingMode"/> to use when mapping the pixels outside of the border, in Y direction.</param>
    public Convolution2PassProcessor(
        Configuration configuration,
        float[] kernel,
        bool preserveAlpha,
        Image<TPixel> source,
        Rectangle sourceRectangle,
        BorderWrappingMode borderWrapModeX,
        BorderWrappingMode borderWrapModeY)
        : base(configuration, source, sourceRectangle)
    {
        this.Kernel = kernel;
        this.PreserveAlpha = preserveAlpha;
        this.BorderWrapModeX = borderWrapModeX;
        this.BorderWrapModeY = borderWrapModeY;
    }

    /// <summary>
    /// Gets the convolution kernel.
    /// </summary>
    public float[] Kernel { get; }

    /// <summary>
    /// Gets a value indicating whether the convolution filter is applied to alpha as well as the color channels.
    /// </summary>
    public bool PreserveAlpha { get; }

    /// <summary>
    /// Gets the <see cref="BorderWrappingMode"/> to use when mapping the pixels outside of the border, in X direction.
    /// </summary>
    public BorderWrappingMode BorderWrapModeX { get; }

    /// <summary>
    /// Gets the <see cref="BorderWrappingMode"/> to use when mapping the pixels outside of the border, in Y direction.
    /// </summary>
    public BorderWrappingMode BorderWrapModeY { get; }

    /// <inheritdoc/>
    protected override void OnFrameApply(ImageFrame<TPixel> source)
    {
        using Buffer2D<TPixel> firstPassPixels = this.Configuration.MemoryAllocator.Allocate2D<TPixel>(source.Size());

        var interest = Rectangle.Intersect(this.SourceRectangle, source.Bounds());

        // We can create a single sampling map with the size as if we were using the non separated 2D kernel
        // the two 1D kernels represent, and reuse it across both convolution steps, like in the bokeh blur.
        using var mapXY = new KernelSamplingMap(this.Configuration.MemoryAllocator);

        mapXY.BuildSamplingOffsetMap(this.Kernel.Length, this.Kernel.Length, interest, this.BorderWrapModeX, this.BorderWrapModeY);

        // Horizontal convolution
        var horizontalOperation = new HorizontalConvolutionRowOperation(
            interest,
            firstPassPixels,
            source.PixelBuffer,
            mapXY,
            this.Kernel,
            this.Configuration,
            this.PreserveAlpha);

        ParallelRowIterator.IterateRows<HorizontalConvolutionRowOperation, Vector4>(
            this.Configuration,
            interest,
            in horizontalOperation);

        // Vertical convolution
        var verticalOperation = new VerticalConvolutionRowOperation(
            interest,
            source.PixelBuffer,
            firstPassPixels,
            mapXY,
            this.Kernel,
            this.Configuration,
            this.PreserveAlpha);

        ParallelRowIterator.IterateRows<VerticalConvolutionRowOperation, Vector4>(
            this.Configuration,
            interest,
            in verticalOperation);
    }

    /// <summary>
    /// A <see langword="struct"/> implementing the logic for the horizontal 1D convolution.
    /// </summary>
    internal readonly struct HorizontalConvolutionRowOperation : IRowOperation<Vector4>
    {
        private readonly Rectangle bounds;
        private readonly Buffer2D<TPixel> targetPixels;
        private readonly Buffer2D<TPixel> sourcePixels;
        private readonly KernelSamplingMap map;
        private readonly float[] kernel;
        private readonly Configuration configuration;
        private readonly bool preserveAlpha;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HorizontalConvolutionRowOperation(
            Rectangle bounds,
            Buffer2D<TPixel> targetPixels,
            Buffer2D<TPixel> sourcePixels,
            KernelSamplingMap map,
            float[] kernel,
            Configuration configuration,
            bool preserveAlpha)
        {
            this.bounds = bounds;
            this.targetPixels = targetPixels;
            this.sourcePixels = sourcePixels;
            this.map = map;
            this.kernel = kernel;
            this.configuration = configuration;
            this.preserveAlpha = preserveAlpha;
        }

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public int GetRequiredBufferLength(Rectangle bounds)
            => 2 * bounds.Width;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int y, Span<Vector4> span)
        {
            if (this.preserveAlpha)
            {
                this.Convolve3(y, span);
            }
            else
            {
                this.Convolve4(y, span);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Convolve3(int y, Span<Vector4> span)
        {
            // Span is 2x bounds.
            int boundsX = this.bounds.X;
            int boundsWidth = this.bounds.Width;
            int kernelSize = this.kernel.Length;

            Span<Vector4> sourceBuffer = span[..this.bounds.Width];
            Span<Vector4> targetBuffer = span[this.bounds.Width..];

            // Clear the target buffer for each row run.
            targetBuffer.Clear();

            // Get the precalculated source sample row for this kernel row and copy to our buffer.
            Span<TPixel> sourceRow = this.sourcePixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.ToVector4(this.configuration, sourceRow, sourceBuffer);

            ref Vector4 sourceBase = ref MemoryMarshal.GetReference(sourceBuffer);
            ref Vector4 targetStart = ref MemoryMarshal.GetReference(targetBuffer);
            ref Vector4 targetEnd = ref Extensions.UnsafeAdd(ref targetStart, (uint)sourceBuffer.Length);
            ref float kernelBase = ref Extensions.GetUnsafeArrayDataReference(this.kernel);
            ref float kernelEnd = ref Extensions.UnsafeAdd(ref kernelBase, (uint)kernelSize);
            ref int sampleColumnBase = ref MemoryMarshal.GetReference(this.map.GetColumnOffsetSpan());

            while (Unsafe.IsAddressLessThan(ref targetStart, ref targetEnd))
            {
                ref float kernelStart = ref kernelBase;
                ref int sampleColumnStart = ref sampleColumnBase;

                while (Unsafe.IsAddressLessThan(ref kernelStart, ref kernelEnd))
                {
                    Vector4 sample = Extensions.UnsafeAdd(ref sourceBase, (uint)(sampleColumnStart - boundsX));

                    targetStart += kernelStart * sample;

                    kernelStart = ref Extensions.UnsafeAdd(ref kernelStart, 1);
                    sampleColumnStart = ref Extensions.UnsafeAdd(ref sampleColumnStart, 1);
                }

                targetStart = ref Extensions.UnsafeAdd(ref targetStart, 1);
                sampleColumnBase = ref Extensions.UnsafeAdd(ref sampleColumnBase, (uint)kernelSize);
            }

            // Now we need to copy the original alpha values from the source row.
            sourceRow = this.sourcePixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.ToVector4(this.configuration, sourceRow, sourceBuffer);

            targetStart = ref MemoryMarshal.GetReference(targetBuffer);

            while (Unsafe.IsAddressLessThan(ref targetStart, ref targetEnd))
            {
                targetStart.W = sourceBase.W;

                targetStart = ref Extensions.UnsafeAdd(ref targetStart, 1);
                sourceBase = ref Extensions.UnsafeAdd(ref sourceBase, 1);
            }

            Span<TPixel> targetRow = this.targetPixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.FromVector4Destructive(this.configuration, targetBuffer, targetRow);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Convolve4(int y, Span<Vector4> span)
        {
            // Span is 2x bounds.
            int boundsX = this.bounds.X;
            int boundsWidth = this.bounds.Width;
            int kernelSize = this.kernel.Length;

            Span<Vector4> sourceBuffer = span[..this.bounds.Width];
            Span<Vector4> targetBuffer = span[this.bounds.Width..];

            // Clear the target buffer for each row run.
            targetBuffer.Clear();

            // Get the precalculated source sample row for this kernel row and copy to our buffer.
            Span<TPixel> sourceRow = this.sourcePixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.ToVector4(this.configuration, sourceRow, sourceBuffer);

            Numerics.Premultiply(sourceBuffer);

            ref Vector4 sourceBase = ref MemoryMarshal.GetReference(sourceBuffer);
            ref Vector4 targetStart = ref MemoryMarshal.GetReference(targetBuffer);
            ref Vector4 targetEnd = ref Extensions.UnsafeAdd(ref targetStart, (uint)sourceBuffer.Length);
            ref float kernelBase = ref Extensions.GetUnsafeArrayDataReference(this.kernel);
            ref float kernelEnd = ref Extensions.UnsafeAdd(ref kernelBase, (uint)kernelSize);
            ref int sampleColumnBase = ref MemoryMarshal.GetReference(this.map.GetColumnOffsetSpan());

            while (Unsafe.IsAddressLessThan(ref targetStart, ref targetEnd))
            {
                ref float kernelStart = ref kernelBase;
                ref int sampleColumnStart = ref sampleColumnBase;

                while (Unsafe.IsAddressLessThan(ref kernelStart, ref kernelEnd))
                {
                    Vector4 sample = Extensions.UnsafeAdd(ref sourceBase, (uint)(sampleColumnStart - boundsX));

                    targetStart += kernelStart * sample;

                    kernelStart = ref Extensions.UnsafeAdd(ref kernelStart, 1);
                    sampleColumnStart = ref Extensions.UnsafeAdd(ref sampleColumnStart, 1);
                }

                targetStart = ref Extensions.UnsafeAdd(ref targetStart, 1);
                sampleColumnBase = ref Extensions.UnsafeAdd(ref sampleColumnBase, (uint)kernelSize);
            }

            Numerics.UnPremultiply(targetBuffer);

            Span<TPixel> targetRow = this.targetPixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.FromVector4Destructive(this.configuration, targetBuffer, targetRow);
        }
    }

    /// <summary>
    /// A <see langword="struct"/> implementing the logic for the vertical 1D convolution.
    /// </summary>
    internal readonly struct VerticalConvolutionRowOperation : IRowOperation<Vector4>
    {
        private readonly Rectangle bounds;
        private readonly Buffer2D<TPixel> targetPixels;
        private readonly Buffer2D<TPixel> sourcePixels;
        private readonly KernelSamplingMap map;
        private readonly float[] kernel;
        private readonly Configuration configuration;
        private readonly bool preserveAlpha;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VerticalConvolutionRowOperation(
            Rectangle bounds,
            Buffer2D<TPixel> targetPixels,
            Buffer2D<TPixel> sourcePixels,
            KernelSamplingMap map,
            float[] kernel,
            Configuration configuration,
            bool preserveAlpha)
        {
            this.bounds = bounds;
            this.targetPixels = targetPixels;
            this.sourcePixels = sourcePixels;
            this.map = map;
            this.kernel = kernel;
            this.configuration = configuration;
            this.preserveAlpha = preserveAlpha;
        }

        /// <inheritdoc/>
        [MethodImpl(InliningOptions.ShortMethod)]
        public int GetRequiredBufferLength(Rectangle bounds)
            => 2 * bounds.Width;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int y, Span<Vector4> span)
        {
            if (this.preserveAlpha)
            {
                this.Convolve3(y, span);
            }
            else
            {
                this.Convolve4(y, span);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Convolve3(int y, Span<Vector4> span)
        {
            // Span is 2x bounds.
            int boundsX = this.bounds.X;
            int boundsWidth = this.bounds.Width;
            int kernelSize = this.kernel.Length;

            Span<Vector4> sourceBuffer = span[..this.bounds.Width];
            Span<Vector4> targetBuffer = span[this.bounds.Width..];

            ref int sampleRowBase = ref Extensions.UnsafeAdd(ref MemoryMarshal.GetReference(this.map.GetRowOffsetSpan()), (uint)((y - this.bounds.Y) * kernelSize));

            // Clear the target buffer for each row run.
            targetBuffer.Clear();

            ref Vector4 targetBase = ref MemoryMarshal.GetReference(targetBuffer);
            ref float kernelStart = ref Extensions.GetUnsafeArrayDataReference(this.kernel);
            ref float kernelEnd = ref Extensions.UnsafeAdd(ref kernelStart, (uint)kernelSize);

            Span<TPixel> sourceRow;
            while (Unsafe.IsAddressLessThan(ref kernelStart, ref kernelEnd))
            {
                // Get the precalculated source sample row for this kernel row and copy to our buffer.
                sourceRow = this.sourcePixels.DangerousGetRowSpan(sampleRowBase).Slice(boundsX, boundsWidth);

                PixelOperations<TPixel>.Instance.ToVector4(this.configuration, sourceRow, sourceBuffer);

                ref Vector4 sourceBase = ref MemoryMarshal.GetReference(sourceBuffer);
                ref Vector4 sourceEnd = ref Extensions.UnsafeAdd(ref sourceBase, (uint)sourceBuffer.Length);
                ref Vector4 targetStart = ref targetBase;
                float factor = kernelStart;

                while (Unsafe.IsAddressLessThan(ref sourceBase, ref sourceEnd))
                {
                    targetStart += factor * sourceBase;

                    sourceBase = ref Extensions.UnsafeAdd(ref sourceBase, 1);
                    targetStart = ref Extensions.UnsafeAdd(ref targetStart, 1);
                }

                kernelStart = ref Extensions.UnsafeAdd(ref kernelStart, 1);
                sampleRowBase = ref Extensions.UnsafeAdd(ref sampleRowBase, 1);
            }

            // Now we need to copy the original alpha values from the source row.
            sourceRow = this.sourcePixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.ToVector4(this.configuration, sourceRow, sourceBuffer);
            {
                ref Vector4 sourceBase = ref MemoryMarshal.GetReference(sourceBuffer);
                ref Vector4 sourceEnd = ref Extensions.UnsafeAdd(ref sourceBase, (uint)sourceBuffer.Length);

                while (Unsafe.IsAddressLessThan(ref sourceBase, ref sourceEnd))
                {
                    targetBase.W = sourceBase.W;

                    targetBase = ref Extensions.UnsafeAdd(ref targetBase, 1);
                    sourceBase = ref Extensions.UnsafeAdd(ref sourceBase, 1);
                }
            }

            Span<TPixel> targetRow = this.targetPixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.FromVector4Destructive(this.configuration, targetBuffer, targetRow);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Convolve4(int y, Span<Vector4> span)
        {
            // Span is 2x bounds.
            int boundsX = this.bounds.X;
            int boundsWidth = this.bounds.Width;
            int kernelSize = this.kernel.Length;

            Span<Vector4> sourceBuffer = span[..this.bounds.Width];
            Span<Vector4> targetBuffer = span[this.bounds.Width..];

            ref int sampleRowBase = ref Extensions.UnsafeAdd(ref MemoryMarshal.GetReference(this.map.GetRowOffsetSpan()), (uint)((y - this.bounds.Y) * kernelSize));

            // Clear the target buffer for each row run.
            targetBuffer.Clear();

            ref Vector4 targetBase = ref MemoryMarshal.GetReference(targetBuffer);
            ref float kernelStart = ref Extensions.GetUnsafeArrayDataReference(this.kernel);
            ref float kernelEnd = ref Extensions.UnsafeAdd(ref kernelStart, (uint)kernelSize);

            Span<TPixel> sourceRow;
            while (Unsafe.IsAddressLessThan(ref kernelStart, ref kernelEnd))
            {
                // Get the precalculated source sample row for this kernel row and copy to our buffer.
                sourceRow = this.sourcePixels.DangerousGetRowSpan(sampleRowBase).Slice(boundsX, boundsWidth);

                PixelOperations<TPixel>.Instance.ToVector4(this.configuration, sourceRow, sourceBuffer);

                Numerics.Premultiply(sourceBuffer);

                ref Vector4 sourceBase = ref MemoryMarshal.GetReference(sourceBuffer);
                ref Vector4 sourceEnd = ref Extensions.UnsafeAdd(ref sourceBase, (uint)sourceBuffer.Length);
                ref Vector4 targetStart = ref targetBase;
                float factor = kernelStart;

                while (Unsafe.IsAddressLessThan(ref sourceBase, ref sourceEnd))
                {
                    targetStart += factor * sourceBase;

                    sourceBase = ref Extensions.UnsafeAdd(ref sourceBase, 1);
                    targetStart = ref Extensions.UnsafeAdd(ref targetStart, 1);
                }

                kernelStart = ref Extensions.UnsafeAdd(ref kernelStart, 1);
                sampleRowBase = ref Extensions.UnsafeAdd(ref sampleRowBase, 1);
            }

            Numerics.UnPremultiply(targetBuffer);

            Span<TPixel> targetRow = this.targetPixels.DangerousGetRowSpan(y).Slice(boundsX, boundsWidth);
            PixelOperations<TPixel>.Instance.FromVector4Destructive(this.configuration, targetBuffer, targetRow);
        }
    }
}
