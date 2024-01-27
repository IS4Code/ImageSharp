// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.ColorSpaces.Conversion;

/// <content>
/// Allows conversion to <see cref="CieLuv"/>.
/// </content>
public partial class ColorSpaceConverter
{
    /// <summary>
    /// Converts a <see cref="CieLab"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in CieLab color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieLab"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieLab> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieLab sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieLab sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieLch"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in CieLch color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieLch"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieLch> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieLch sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieLch sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieLchuv"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLab"/></returns>
    public CieLuv ToCieLuv(in CieLchuv color)
    {
        // Conversion (preserving white point)
        CieLuv unadapted = CieLchuvToCieLuvConverter.Convert(color);

        // Adaptation
        return this.Adapt(unadapted);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieLchuv"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieLchuv> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieLchuv sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieLchuv sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieXyy"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in CieXyy color)
    {
        CieXyz xyzColor = ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieXyy"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieXyy> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieXyy sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieXyy sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieXyz"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in CieXyz color)
    {
        // Adaptation
        CieXyz adapted = this.Adapt(color, this.whitePoint, this.targetLuvWhitePoint);

        // Conversion
        return this.cieXyzToCieLuvConverter.Convert(adapted);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieXyz"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieXyz> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieXyz sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieXyz sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Cmyk"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in Cmyk color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Cmyk"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<Cmyk> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Cmyk sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Cmyk sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Hsl"/> into a <see cref="CieLuv"/>
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in Hsl color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Hsl"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<Hsl> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Hsl sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Hsl sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Hsv"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in Hsv color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Hsv"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<Hsv> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Hsv sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Hsv sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="HunterLab"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in HunterLab color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="HunterLab"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<HunterLab> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref HunterLab sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref HunterLab sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Lms"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in Lms color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Lms"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<Lms> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Lms sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Lms sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="LinearRgb"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in LinearRgb color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="LinearRgb"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<LinearRgb> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref LinearRgb sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref LinearRgb sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Rgb"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in Rgb color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Rgb"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<Rgb> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Rgb sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Rgb sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="YCbCr"/> into a <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="CieLuv"/></returns>
    public CieLuv ToCieLuv(in YCbCr color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);
        return this.ToCieLuv(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="YCbCr"/> into <see cref="CieLuv"/>.
    /// </summary>
    /// <param name="source">The span to the source colors.</param>
    /// <param name="destination">The span to the destination colors.</param>
    public void Convert(ReadOnlySpan<YCbCr> source, Span<CieLuv> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref YCbCr sourceRef = ref MemoryMarshal.GetReference(source);
        ref CieLuv destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref YCbCr sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref CieLuv dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToCieLuv(sp);
        }
    }
}
