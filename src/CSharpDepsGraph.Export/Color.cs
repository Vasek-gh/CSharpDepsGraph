using System;

namespace CSharpDepsGraph.Export;

/// <summary>
/// Represents a 32-bit ARGB (Alpha, Red, Green, Blue) color structure with
/// predefined color values and format conversion capabilities.
/// </summary>
public struct Color
{
    /// <summary>
    /// Gets a predefined white color with maximum alpha (0xFFFFFFFF).
    /// </summary>
    public static readonly Color White = 0xFFFFFFFF;

    /// <summary>
    /// Gets the default color used as a base reference (0xFF626567).
    /// </summary>
    public static readonly Color Deafult = 0xFF626567;

    /// <summary>
    /// Gets the color for group elements (0xFF17202A).
    /// </summary>
    public static readonly Color Group = 0xFF17202a;

    /// <summary>
    /// Gets the color for assembly-related elements (0xFF7D6050).
    /// </summary>
    public static readonly Color Assembly = 0xFF7D6050;

    /// <summary>
    /// Gets the color for namespace elements (0xFF8E8ADE).
    /// </summary>
    public static readonly Color Namespace = 0xFF8E8ADE;

    /// <summary>
    /// Gets the color for enumeration elements (0xFF758C92).
    /// </summary>
    public static readonly Color Enum = 0xFF758C92;

    /// <summary>
    /// Gets the color for class elements (0xFF5D998B).
    /// </summary>
    public static readonly Color Class = 0xFF5D998B;

    /// <summary>
    /// Gets the color for structure elements (0xFF7FD1AE).
    /// </summary>
    public static readonly Color Structure = 0xFF7FD1AE;

    /// <summary>
    /// Gets the color for interface elements (0xFF475C6C).
    /// </summary>
    public static readonly Color Interface = 0xFF475C6C;

    /// <summary>
    /// Gets the color for constant elements (0xFFFF8652).
    /// </summary>
    public static readonly Color Const = 0xFFFF8652;

    /// <summary>
    /// Gets the color for field elements (0xFFFF8652).
    /// </summary>
    public static readonly Color Field = 0xFFFF8652;

    /// <summary>
    /// Gets the color for property elements (0xFF58508D).
    /// </summary>
    public static readonly Color Property = 0xFF58508D;

    /// <summary>
    /// Gets the color for method elements (0xFFCD8B62).
    /// </summary>
    public static readonly Color Method = 0xFFCD8B62;

    private string? _alphaFirstStr;
    private string? _alphaLastStr;

    /// <summary>
    /// Gets the alpha component value (0-255).
    /// </summary>
    public byte A { get; }

    /// <summary>
    /// Gets the red component value (0-255).
    /// </summary>
    public byte R { get; }

    /// <summary>
    /// Gets the green component value (0-255).
    /// </summary>
    public byte G { get; }

    /// <summary>
    /// Gets the blue component value (0-255).
    /// </summary>
    public byte B { get; }

    /// <summary>
    /// Gets the color string in #AARRGGBB hexadecimal format.
    /// </summary>
    public string AlphaFirstStr => _alphaFirstStr ??= $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    /// <summary>
    /// Gets the color string in #RRGGBBAA hexadecimal format.
    /// </summary>
    public string AlphaLastStr => _alphaLastStr ??= $"#{R:X2}{G:X2}{B:X2}{A:X2}";

    /// <summary>
    /// Initializes a new Color structure from a 32-bit ARGB value.
    /// </summary>
    /// <param name="value">The 32-bit ARGB value in format 0xAARRGGBB.</param>
    public Color(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        A = bytes[3];
        R = bytes[2];
        G = bytes[1];
        B = bytes[0];
    }

    /// <summary>
    /// Converts a 32-bit unsigned integer to a Color structure.
    /// </summary>
    /// <param name="value">The 32-bit ARGB value to convert.</param>
    /// <returns>A new Color structure initialized with the specified value.</returns>
    public static implicit operator Color(uint value)
    {
        return new Color(value);
    }
}