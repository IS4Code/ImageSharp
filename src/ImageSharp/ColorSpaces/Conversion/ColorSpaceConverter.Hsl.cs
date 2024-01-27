// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.ColorSpaces.Conversion;

/// <content>
/// Allows conversion to <see cref="Hsl"/>.
/// </content>
public partial class ColorSpaceConverter
{
    /// <summary>
    /// Converts a <see cref="CieLab"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in CieLab color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);

        return this.ToHsl(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieLab"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieLab> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieLab sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieLab sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieLch"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in CieLch color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);

        return this.ToHsl(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieLch"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieLch> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieLch sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieLch sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieLchuv"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in CieLchuv color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);

        return this.ToHsl(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieLchuv"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieLchuv> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieLchuv sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieLchuv sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieLuv"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in CieLuv color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);

        return this.ToHsl(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieLuv"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieLuv> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieLuv sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieLuv sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieXyy"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in CieXyy color)
    {
        CieXyz xyzColor = ToCieXyz(color);

        return this.ToHsl(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieXyy"/> into <see cref="Hsl"/>
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieXyy> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieXyy sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieXyy sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="CieXyz"/> into a <see cref="Hsl"/>
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in CieXyz color)
    {
        Rgb rgb = this.ToRgb(color);

        return HslAndRgbConverter.Convert(rgb);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="CieXyz"/> into <see cref="Hsl"/>
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<CieXyz> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref CieXyz sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref CieXyz sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Cmyk"/> into a <see cref="Hsl"/>
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public static Hsl ToHsl(in Cmyk color)
    {
        Rgb rgb = ToRgb(color);

        return HslAndRgbConverter.Convert(rgb);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Cmyk"/> into <see cref="Hsl"/>
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public static void Convert(ReadOnlySpan<Cmyk> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Cmyk sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Cmyk sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Hsv"/> into a <see cref="Hsl"/>
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public static Hsl ToHsl(in Hsv color)
    {
        Rgb rgb = ToRgb(color);

        return HslAndRgbConverter.Convert(rgb);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Hsv"/> into <see cref="Hsl"/>
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public static void Convert(ReadOnlySpan<Hsv> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Hsv sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Hsv sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="HunterLab"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in HunterLab color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);

        return this.ToHsl(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="HunterLab"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<HunterLab> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref HunterLab sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref HunterLab sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="LinearRgb"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public static Hsl ToHsl(in LinearRgb color)
    {
        Rgb rgb = ToRgb(color);

        return HslAndRgbConverter.Convert(rgb);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="LinearRgb"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public static void Convert(ReadOnlySpan<LinearRgb> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref LinearRgb sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref LinearRgb sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Lms"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(Lms color)
    {
        CieXyz xyzColor = this.ToCieXyz(color);

        return this.ToHsl(xyzColor);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="Lms"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<Lms> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Lms sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Lms sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="Rgb"/> into a <see cref="Hsl"/>
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public static Hsl ToHsl(in Rgb color) => HslAndRgbConverter.Convert(color);

    /// <summary>
    /// Performs the bulk conversion from <see cref="Rgb"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public static void Convert(ReadOnlySpan<Rgb> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref Rgb sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref Rgb sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = ToHsl(sp);
        }
    }

    /// <summary>
    /// Converts a <see cref="YCbCr"/> into a <see cref="Hsl"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The <see cref="Hsl"/></returns>
    public Hsl ToHsl(in YCbCr color)
    {
        Rgb rgb = this.ToRgb(color);

        return HslAndRgbConverter.Convert(rgb);
    }

    /// <summary>
    /// Performs the bulk conversion from <see cref="YCbCr"/> into <see cref="Hsl"/>.
    /// </summary>
    /// <param name="source">The span to the source colors</param>
    /// <param name="destination">The span to the destination colors</param>
    public void Convert(ReadOnlySpan<YCbCr> source, Span<Hsl> destination)
    {
        Guard.DestinationShouldNotBeTooShort(source, destination, nameof(destination));
        int count = source.Length;

        ref YCbCr sourceRef = ref MemoryMarshal.GetReference(source);
        ref Hsl destRef = ref MemoryMarshal.GetReference(destination);

        for (nuint i = 0; i < (uint)count; i++)
        {
            ref YCbCr sp = ref Extensions.UnsafeAdd(ref sourceRef, i);
            ref Hsl dp = ref Extensions.UnsafeAdd(ref destRef, i);
            dp = this.ToHsl(sp);
        }
    }
}
