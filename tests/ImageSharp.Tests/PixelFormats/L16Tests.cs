// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Tests.PixelFormats;

[Trait("Category", "PixelFormats")]
public class L16Tests
{
    [Fact]
    public void AreEqual()
    {
        L16 color1 = new(3000);
        L16 color2 = new(3000);

        Assert.Equal(color1, color2);
    }

    [Fact]
    public void AreNotEqual()
    {
        L16 color1 = new(12345);
        L16 color2 = new(54321);

        Assert.NotEqual(color1, color2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65535)]
    [InlineData(32767)]
    [InlineData(42)]
    public void L16_PackedValue_EqualsInput(ushort input)
        => Assert.Equal(input, new L16(input).PackedValue);

    [Fact]
    public void L16_FromScaledVector4()
    {
        // Arrange
        L16 gray = default;
        const ushort expected = 32767;
        Vector4 scaled = new L16(expected).ToScaledVector4();

        // Act
        gray.FromScaledVector4(scaled);
        ushort actual = gray.PackedValue;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65535)]
    [InlineData(32767)]
    public void L16_ToScaledVector4(ushort input)
    {
        // Arrange
        L16 gray = new(input);

        // Act
        Vector4 actual = gray.ToScaledVector4();

        // Assert
        float vectorInput = input / 65535F;
        Assert.Equal(vectorInput, actual.X);
        Assert.Equal(vectorInput, actual.Y);
        Assert.Equal(vectorInput, actual.Z);
        Assert.Equal(1F, actual.W);
    }

    [Fact]
    public void L16_FromVector4()
    {
        // Arrange
        L16 gray = default;
        const ushort expected = 32767;
        Vector4 vector = new L16(expected).ToVector4();

        // Act
        gray.FromVector4(vector);
        ushort actual = gray.PackedValue;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65535)]
    [InlineData(32767)]
    public void L16_ToVector4(ushort input)
    {
        // Arrange
        L16 gray = new(input);

        // Act
        Vector4 actual = gray.ToVector4();

        // Assert
        float vectorInput = input / 65535F;
        Assert.Equal(vectorInput, actual.X);
        Assert.Equal(vectorInput, actual.Y);
        Assert.Equal(vectorInput, actual.Z);
        Assert.Equal(1F, actual.W);
    }

    [Fact]
    public void L16_FromRgba32()
    {
        // Arrange
        L16 gray = default;
        const byte rgb = 128;
        ushort scaledRgb = ColorNumerics.UpscaleFrom8BitTo16Bit(rgb);
        ushort expected = ColorNumerics.Get16BitBT709Luminance(scaledRgb, scaledRgb, scaledRgb);

        // Act
        gray.FromRgba32(new Rgba32(rgb, rgb, rgb));
        ushort actual = gray.PackedValue;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65535)]
    [InlineData(8100)]
    public void L16_ToRgba32(ushort input)
    {
        // Arrange
        ushort expected = ColorNumerics.DownScaleFrom16BitTo8Bit(input);
        L16 gray = new(input);

        // Act
        Rgba32 actual = default;
        gray.ToRgba32(ref actual);

        // Assert
        Assert.Equal(expected, actual.R);
        Assert.Equal(expected, actual.G);
        Assert.Equal(expected, actual.B);
        Assert.Equal(byte.MaxValue, actual.A);
    }

    [Fact]
    public void L16_FromBgra5551()
    {
        // arrange
        L16 gray = default;
        ushort expected = ushort.MaxValue;

        // act
        gray.FromBgra5551(new Bgra5551(1.0f, 1.0f, 1.0f, 1.0f));

        // assert
        Assert.Equal(expected, gray.PackedValue);
    }

    [Fact]
    public void L16_PixelInformation()
    {
        PixelTypeInfo info = L16.GetPixelTypeInfo();
        Assert.Equal(Unsafe.SizeOf<L16>() * 8, info.BitsPerPixel);
        Assert.Equal(PixelAlphaRepresentation.None, info.AlphaRepresentation);
        Assert.Equal(PixelColorType.Grayscale, info.ColorType);

        PixelComponentInfo componentInfo = info.ComponentInfo.Value;
        Assert.Equal(1, componentInfo.ComponentCount);
        Assert.Equal(0, componentInfo.Padding);
        Assert.Equal(16, componentInfo.GetComponentPrecision(0));
        Assert.Equal(16, componentInfo.GetMaximumComponentPrecision());
    }
}
