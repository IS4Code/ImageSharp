// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Buffers;
using System.Numerics;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Formats.Tiff.PhotometricInterpretation;

/// <summary>
/// Implements decoding pixel data with photometric interpretation of type 'CieLab' with the planar configuration.
/// </summary>
internal class CieLabPlanarTiffColor<TPixel> : TiffBasePlanarColorDecoder<TPixel>
    where TPixel : unmanaged, IPixel<TPixel>
{
    private static readonly ColorSpaceConverter ColorSpaceConverter = new();

    private const float Inv255 = 1.0f / 255.0f;

    /// <inheritdoc/>
    public override void Decode(IMemoryOwner<byte>[] data, Buffer2D<TPixel> pixels, int left, int top, int width, int height)
    {
        Span<byte> l = data[0].GetSpan();
        Span<byte> a = data[1].GetSpan();
        Span<byte> b = data[2].GetSpan();

        TPixel color = default;
        int offset = 0;
        for (int y = top; y < top + height; y++)
        {
            Span<TPixel> pixelRow = pixels.DangerousGetRowSpan(y).Slice(left, width);
            for (int x = 0; x < pixelRow.Length; x++)
            {
                CieLab lab = new((l[offset] & 0xFF) * 100f * Inv255, (sbyte)a[offset], (sbyte)b[offset]);
                Rgb rgb = ColorSpaceConverter.ToRgb(lab);

                color.FromScaledVector4(new Vector4(rgb.R, rgb.G, rgb.B, 1.0f));
                pixelRow[x] = color;

                offset++;
            }
        }
    }
}
