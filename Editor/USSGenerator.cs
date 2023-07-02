using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class CSSProperty
{
    public string Name { get; private set; }
    public string ShortName { get; private set; }

    public bool directional = false;

    protected CSSProperty(string name, string shortName)
    {
        Name = name;
        ShortName = shortName;
    }

    public abstract IEnumerable<string[]> GetRules();

    public static readonly string[][] directionalValues =
    {
        new string[] {"t","top"},
        new string[] {"l","left"},
        new string[] {"r","right"},
        new string[] {"b","bottom"},
    };
}

public class GenericProperties : CSSProperty
{
    public GenericProperties() : base("", "") { }

    public IEnumerable<string[]> GenerateRules;

    public override IEnumerable<string[]> GetRules()
    {
        return GenerateRules;
    }
}

public class OpacityProperty : CSSProperty
{

    /*
    opacity-0	opacity: 0;
    opacity-5	opacity: 0.05;
    opacity-10	opacity: 0.1;
    opacity-20	opacity: 0.2;
    opacity-25	opacity: 0.25;
    opacity-30	opacity: 0.3;
    opacity-40	opacity: 0.4;
    opacity-50	opacity: 0.5;
    opacity-60	opacity: 0.6;
    opacity-70	opacity: 0.7;
    opacity-75	opacity: 0.75;
    opacity-80	opacity: 0.8;
    opacity-90	opacity: 0.9;
    opacity-95	opacity: 0.95;
    opacity-100	opacity: 1;
    */

    public OpacityProperty() : base("opacity", "opacity") { }

    public override IEnumerable<string[]> GetRules()
    {
        // auto fill in opacity with opacity-0 to opacity-100 with reference to the value above
        for (int i = 0; i <= 100; i += 5)
        {
            yield return new string[] {
                $"{ShortName}-{i}",
                $"{Name}: {i / 100f};",
            };
        }
    }
}

public class CursorProperty : GenericProperties
{
    public CursorProperty() : base()
    {
        GenerateRules = CursorRules;
    }

    /* cursor: 
    [ 
        [ <resource> | <url> ] 
        [ <integer> <integer>]? , ] 
        [ arrow | text | resize-vertical | resize-horizontal | link | slide-arrow | resize-up-right | resize-up-left | move-arrow | rotate-arrow | scale-arrow | arrow-plus | arrow-minus | pan | orbit | zoom | fps | split-resize-up-down | split-resize-left-right ] 
    ]
    */

    public string[][] CursorRules = {
        new string[] { "cursor-arrow", "cursor: arrow;" },
        new string[] { "cursor-text", "cursor: text;" },
        new string[] { "cursor-resize-vertical", "cursor: resize-vertical;" },
        new string[] { "cursor-resize-horizontal", "cursor: resize-horizontal;" },
        new string[] { "cursor-link", "cursor: link;" },
        new string[] { "cursor-slide-arrow", "cursor: slide-arrow;" },
        new string[] { "cursor-resize-up-right", "cursor: resize-up-right;" },
        new string[] { "cursor-resize-up-left", "cursor: resize-up-left;" },
        new string[] { "cursor-move-arrow", "cursor: move-arrow;" },
        new string[] { "cursor-rotate-arrow", "cursor: rotate-arrow;" },
        new string[] { "cursor-scale-arrow", "cursor: scale-arrow;" },
        new string[] { "cursor-arrow-plus", "cursor: arrow-plus;" },
        new string[] { "cursor-arrow-minus", "cursor: arrow-minus;" },
        new string[] { "cursor-pan", "cursor: pan;" },
        new string[] { "cursor-orbit", "cursor: orbit;" },
        new string[] { "cursor-zoom", "cursor: zoom;" },
        new string[] { "cursor-fps", "cursor: fps;" },
        new string[] { "cursor-split-resize-up-down", "cursor: split-resize-up-down;" },
        new string[] { "cursor-split-resize-left-right", "cursor: split-resize-left-right;" },
    };
}

public class FontSizeProperty : CSSProperty
{
    // text-xs	font-size: 0.75rem; /* 12px */
    // line-height: 1rem; /* 16px */
    // text-sm	font-size: 0.875rem; /* 14px */
    // line-height: 1.25rem; /* 20px */
    // text-base	font-size: 1rem; /* 16px */
    // line-height: 1.5rem; /* 24px */
    // text-lg	font-size: 1.125rem; /* 18px */
    // line-height: 1.75rem; /* 28px */
    // text-xl	font-size: 1.25rem; /* 20px */
    // line-height: 1.75rem; /* 28px */
    // text-2xl	font-size: 1.5rem; /* 24px */
    // line-height: 2rem; /* 32px */
    // text-3xl	font-size: 1.875rem; /* 30px */
    // line-height: 2.25rem; /* 36px */
    // text-4xl	font-size: 2.25rem; /* 36px */
    // line-height: 2.5rem; /* 40px */
    // text-5xl	font-size: 3rem; /* 48px */
    // line-height: 1;
    // text-6xl	font-size: 3.75rem; /* 60px */
    // line-height: 1;
    // text-7xl	font-size: 4.5rem; /* 72px */
    // line-height: 1;
    // text-8xl	font-size: 6rem; /* 96px */
    // line-height: 1;
    // text-9xl	font-size: 8rem; /* 128px */
    // line-height: 1;

    private static string[][] GetTailwindWidthSize()
    {
        var values = new List<string[]>
        {
            new string[] {"xs","12px"},
            new string[] {"sm","14px"},
            new string[] {"base","16px"},
            new string[] {"lg","18px"},
            new string[] {"xl","20px"},
            new string[] {"2xl","24px"},
            new string[] {"3xl","30px"},
            new string[] {"4xl","36px"},
            new string[] {"5xl","48px"},
            new string[] {"6xl","60px"},
            new string[] {"7xl","72px"},
            new string[] {"8xl","96px"},
            new string[] {"9xl","128px"},
        };
        return values.ToArray();
    }

    private static readonly string[][] values = GetTailwindWidthSize();

    public FontSizeProperty() : base("font-size", "text") { }

    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                $"{ShortName}-{value[0]}",
                $"{Name}: {value[1]};",
            };
        }
    }

}

public class SizeProperty : DimensionProperty
{
    // w-0	width: 0px;
    // w-px	width: 1px;
    // w-0.5	width: 0.125rem; /* 2px */
    // w-1	width: 0.25rem; /* 4px */
    // w-1.5	width: 0.375rem; /* 6px */
    // w-2	width: 0.5rem; /* 8px */
    // w-2.5	width: 0.625rem; /* 10px */
    // w-3	width: 0.75rem; /* 12px */
    // w-3.5	width: 0.875rem; /* 14px */
    // w-4	width: 1rem; /* 16px */
    // w-5	width: 1.25rem; /* 20px */
    // w-6	width: 1.5rem; /* 24px */
    // w-7	width: 1.75rem; /* 28px */
    // w-8	width: 2rem; /* 32px */
    // w-9	width: 2.25rem; /* 36px */
    // w-10	width: 2.5rem; /* 40px */
    // w-11	width: 2.75rem; /* 44px */
    // w-12	width: 3rem; /* 48px */
    // w-14	width: 3.5rem; /* 56px */
    // w-16	width: 4rem; /* 64px */
    // w-20	width: 5rem; /* 80px */
    // w-24	width: 6rem; /* 96px */
    // w-28	width: 7rem; /* 112px */
    // w-32	width: 8rem; /* 128px */
    // w-36	width: 9rem; /* 144px */
    // w-40	width: 10rem; /* 160px */
    // w-44	width: 11rem; /* 176px */
    // w-48	width: 12rem; /* 192px */
    // w-52	width: 13rem; /* 208px */
    // w-56	width: 14rem; /* 224px */
    // w-60	width: 15rem; /* 240px */
    // w-64	width: 16rem; /* 256px */
    // w-72	width: 18rem; /* 288px */
    // w-80	width: 20rem; /* 320px */
    // w-96	width: 24rem; /* 384px */

    private static string[][] GetTailwindWidthSize()
    {
        // from w-1 to w-96
        // from w-full to w-11/12

        var values = new List<string[]>
        {
            new string[] { $"0", $"0px" },
            new string[] { $"px", $"1px" },
            // use px as unit

            new string[] { $"0.5", $"2px" },
            new string[] { $"1", $"4px" },
            new string[] { $"1.5", $"6px" },
            new string[] { $"2", $"8px" },
            new string[] { $"2.5", $"10px" },
            new string[] { $"3", $"12px" },
            new string[] { $"3.5", $"14px" },
            new string[] { $"4", $"16px" },
            new string[] { $"5", $"20px" },
            new string[] { $"6", $"24px" },
            new string[] { $"7", $"28px" },
            new string[] { $"8", $"32px" },
            new string[] { $"9", $"36px" },
            new string[] { $"10", $"40px" },
            new string[] { $"11", $"44px" },
            new string[] { $"12", $"48px" },
            new string[] { $"14", $"56px" },
            new string[] { $"16", $"64px" },
            new string[] { $"20", $"80px" },
            new string[] { $"24", $"96px" },
            new string[] { $"28", $"112px" },
            new string[] { $"32", $"128px" },
            new string[] { $"36", $"144px" },
            new string[] { $"40", $"160px" },
            new string[] { $"44", $"176px" },
            new string[] { $"48", $"192px" },
            new string[] { $"52", $"208px" },
            new string[] { $"56", $"224px" },
            new string[] { $"60", $"240px" },
            new string[] { $"64", $"256px" },
            new string[] { $"72", $"288px" },
            new string[] { $"80", $"320px" },
            new string[] { $"96", $"384px" },


            new string[] { $"1/2", $"50%" },
            new string[] { $"1/3", $"33.333333%" },
            new string[] { $"2/3", $"66.666667%" },
            new string[] { $"1/4", $"25%" },
            new string[] { $"2/4", $"50%" },
            new string[] { $"3/4", $"75%" },
            new string[] { $"1/5", $"20%" },
            new string[] { $"2/5", $"40%" },
            new string[] { $"3/5", $"60%" },
            new string[] { $"4/5", $"80%" },
            new string[] { $"1/6", $"16.666667%" },
            new string[] { $"2/6", $"33.333333%" },
            new string[] { $"3/6", $"50%" },
            new string[] { $"4/6", $"66.666667%" },
            new string[] { $"5/6", $"83.333333%" },
            new string[] { $"1/12", $"8.333333%" },
            new string[] { $"2/12", $"16.666667%" },
            new string[] { $"3/12", $"25%" },
            new string[] { $"4/12", $"33.333333%" },
            new string[] { $"5/12", $"41.666667%" },
            new string[] { $"6/12", $"50%" },
            new string[] { $"7/12", $"58.333333%" },
            new string[] { $"8/12", $"66.666667%" },
            new string[] { $"9/12", $"75%" },
            new string[] { $"10/12", $"83.333333%" },
            new string[] { $"11/12", $"91.666667%" },

            new string[] { $"full", $"100%" }
        };
        // values.Add(new string[] { $"auto", $"auto" });

        return values.ToArray();
    }

    private static readonly string[][] values = GetTailwindWidthSize();

    public SizeProperty(string name, string shortName) : base(name, shortName) { }

    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                $"{ShortName}-{value[0]}",
                $"{Name}: {value[1]};",
            };
        }
    }

}

public class DimensionProperty : CSSProperty
{
    private static readonly string[][] values =
    {
        new string[] {"0","0"},
        // new string[] {"auto","auto"},
        new string[] {"1","4px"},
        new string[] {"2","8px"},
        new string[] {"4","16px"},
    };

    public DimensionProperty(string name, string shortName) : base(name, shortName) { }

    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                $"{ShortName}-{value[0]}",
                $"{Name}: {value[1]};",
            };
        }

        if (directional)
        {
            foreach (var direction in directionalValues)
            {
                foreach (var value in values)
                {
                    yield return new string[] {
                        $"{ShortName}{direction[0]}-{value[0]}",
                        $"{Name}-{direction[1]}: {value[1]};",
                    };
                }
            }
        }
    }
}

[Serializable]
public class Palette
{
    public string paletteName;
    public Swatch[] swatches;
}

[Serializable]
public class ArrayOfPalette
{
    public Palette[] colors;
}

[Serializable]
public class Swatch
{
    public string name;
    public string color;
}

public class ColorProperty : CSSProperty
{
    private static readonly string[] values = { "red", "green", "blue", "black", "white", "transparent" };

    private static readonly string[][] colorsMap = GetColorsMap();
    // read and parse the json file to get the colors

    //     [
    //   {
    //     "paletteName": "gray",
    //     "swatches": [
    //       {
    //         "name": "100",
    //         "color": "#f7fafc"
    //       },
    //       {
    //         "name": "200",
    //         "color": "#edf2f7"
    //       },
    //       {
    //         "name": "300",
    //         "color": "#e2e8f0"
    //       },
    //       {
    //         "name": "400",
    //         "color": "#cbd5e0"
    //       },
    //       {
    //         "name": "500",
    //         "color": "#a0aec0"
    //       },
    //       {
    //         "name": "600",
    //         "color": "#718096"
    //       },
    //       {
    //         "name": "700",
    //         "color": "#4a5568"
    //       },
    //       {
    //         "name": "800",
    //         "color": "#2d3748"
    //       },
    //       {
    //         "name": "900",
    //         "color": "#1a202c"
    //       }
    //     ]
    //   },
    //     ]

    public static string[][] GetColorsMap()
    {
        // The json file tailwind-palettte-app.json

        var json = File.ReadAllText("Packages/unity-tailwindcss/Editor/tailwind-palettte-app.json");

        var colorsMap = new List<string[]>();

        // var palettes = JsonConvert.DeserializeObject<Palette[]>(json);
        ArrayOfPalette palettes = JsonUtility.FromJson<ArrayOfPalette>(json);

        // Debug.Log(palettes.palettes);

        foreach (var palette in palettes.colors)
        {
            foreach (var swatch in palette.swatches)
            {
                colorsMap.Add(new string[] { $"{palette.paletteName}-{swatch.name}", $"{swatch.color}" });
            }
        }

        return colorsMap.ToArray();
    }

    public ColorProperty(string name, string shortName) : base(name, shortName) { }

    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                $"{ShortName}-{value}",
                $"{Name}: {value};",
            };
        }

        foreach (var value in colorsMap)
        {
            yield return new string[] {
                $"{ShortName}-{value[0]}",
                $"{Name}: {value[1]};",
            };
        }
    }
}

public class BorderRadius : CSSProperty
{
    // rounded-none	border-radius: 0px;
    // rounded-sm	border-radius: 0.125rem; /* 2px */
    // rounded	border-radius: 0.25rem; /* 4px */
    // rounded-md	border-radius: 0.375rem; /* 6px */
    // rounded-lg	border-radius: 0.5rem; /* 8px */
    // rounded-xl	border-radius: 0.75rem; /* 12px */
    // rounded-2xl	border-radius: 1rem; /* 16px */
    // rounded-3xl	border-radius: 1.5rem; /* 24px */
    // rounded-full	border-radius: 9999px;

    private static readonly string[][] values =
    {
        new string[] {"none","0px"},
        new string[] {"sm","2px"},
        new string[] {"","4px"},
        new string[] {"md","6px"},
        new string[] {"lg","8px"},
        new string[] {"xl","12px"},
        new string[] {"2xl","16px"},
        new string[] {"3xl","24px"},
        new string[] {"full","9999px"},
    };

    public BorderRadius() : base("border-radius", "rounded") { }


    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                value[0].Length > 0 ? $"{ShortName}-{value[0]}" : $"{ShortName}",
                $"{Name}: {value[1]};",
            };
        }
    }
}

public class BorderWidth : CSSProperty
{
    // border-0	border-width: 0px;
    // border-2	border-width: 2px;
    // border-4	border-width: 4px;
    // border-8	border-width: 8px;
    // border	border-width: 1px;
    // border-x-0	border-left-width: 0px;
    // border-right-width: 0px;
    // border-x-2	border-left-width: 2px;
    // border-right-width: 2px;
    // border-x-4	border-left-width: 4px;
    // border-right-width: 4px;
    // border-x-8	border-left-width: 8px;
    // border-right-width: 8px;
    // border-x	border-left-width: 1px;
    // border-right-width: 1px;
    // border-y-0	border-top-width: 0px;
    // border-bottom-width: 0px;
    // border-y-2	border-top-width: 2px;
    // border-bottom-width: 2px;
    // border-y-4	border-top-width: 4px;
    // border-bottom-width: 4px;
    // border-y-8	border-top-width: 8px;
    // border-bottom-width: 8px;
    // border-y	border-top-width: 1px;
    // border-bottom-width: 1px;
    // border-s-0	border-inline-start-width: 0px;
    // border-s-2	border-inline-start-width: 2px;
    // border-s-4	border-inline-start-width: 4px;
    // border-s-8	border-inline-start-width: 8px;
    // border-s	border-inline-start-width: 1px;
    // border-e-0	border-inline-end-width: 0px;
    // border-e-2	border-inline-end-width: 2px;
    // border-e-4	border-inline-end-width: 4px;
    // border-e-8	border-inline-end-width: 8px;
    // border-e	border-inline-end-width: 1px;
    // border-t-0	border-top-width: 0px;
    // border-t-2	border-top-width: 2px;
    // border-t-4	border-top-width: 4px;
    // border-t-8	border-top-width: 8px;
    // border-t	border-top-width: 1px;
    // border-r-0	border-right-width: 0px;
    // border-r-2	border-right-width: 2px;
    // border-r-4	border-right-width: 4px;
    // border-r-8	border-right-width: 8px;
    // border-r	border-right-width: 1px;
    // border-b-0	border-bottom-width: 0px;
    // border-b-2	border-bottom-width: 2px;
    // border-b-4	border-bottom-width: 4px;
    // border-b-8	border-bottom-width: 8px;
    // border-b	border-bottom-width: 1px;
    // border-l-0	border-left-width: 0px;
    // border-l-2	border-left-width: 2px;
    // border-l-4	border-left-width: 4px;
    // border-l-8	border-left-width: 8px;
    // border-l	border-left-width: 1px;

    private static readonly string[][] values =
    {
        new string[] {"0","0px"},
        new string[] {"2","2px"},
        new string[] {"4","4px"},
        new string[] {"8","8px"},
        new string[] {"","1px"},
    };

    public BorderWidth() : base("border-width", "border") { }

    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                value[0].Length > 0 ? $"{ShortName}-{value[0]}" : $"{ShortName}",
                $"{Name}: {value[1]};",
            };
        }
    }
}


public class MinMaxProperty : CSSProperty
{
    // max-w-0	max-width: 0rem; /* 0px */
    // max-w-none	max-width: none;
    // max-w-xs	max-width: 20rem; /* 320px */
    // max-w-sm	max-width: 24rem; /* 384px */
    // max-w-md	max-width: 28rem; /* 448px */
    // max-w-lg	max-width: 32rem; /* 512px */
    // max-w-xl	max-width: 36rem; /* 576px */
    // max-w-2xl	max-width: 42rem; /* 672px */
    // max-w-3xl	max-width: 48rem; /* 768px */
    // max-w-4xl	max-width: 56rem; /* 896px */
    // max-w-5xl	max-width: 64rem; /* 1024px */
    // max-w-6xl	max-width: 72rem; /* 1152px */
    // max-w-7xl	max-width: 80rem; /* 1280px */
    // max-w-full	max-width: 100%;
    // max-w-min	max-width: min-content;
    // max-w-max	max-width: max-content;
    // max-w-fit	max-width: fit-content;
    // max-w-prose	max-width: 65ch;
    // max-w-screen-sm	max-width: 640px;
    // max-w-screen-md	max-width: 768px;
    // max-w-screen-lg	max-width: 1024px;
    // max-w-screen-xl	max-width: 1280px;
    // max-w-screen-2xl	max-width: 1536px;

    private static readonly string[][] values =
    {
        new string[] {"0","0px"},
        // new string[] {"none","none"},x
        new string[] {"xs","320px"},
        new string[] {"sm","384px"},
        new string[] {"md","448px"},
        new string[] {"lg","512px"},
        new string[] {"xl","576px"},
        new string[] {"2xl","672px"},
        new string[] {"3xl","768px"},
        new string[] {"4xl","896px"},
        new string[] {"5xl","1024px"},
        new string[] {"6xl","1152px"},
        new string[] {"7xl","1280px"},
        new string[] {"full","100%"},

        new string[] {"screen-sm","640px"},
        new string[] {"screen-md","768px"},
        new string[] {"screen-lg","1024px"},
        new string[] {"screen-xl","1280px"},
        new string[] {"screen-2xl","1536px"},
    };

    public MinMaxProperty(string name, string shortName) : base(name, shortName) { }

    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                $"{ShortName}-{value[0]}",
                $"{Name}: {value[1]};",
            };
        }
    }
}

public class TextAlign : CSSProperty
{
    // text-left	text-align: left;
    // text-center	text-align: center;
    // text-right	text-align: right;
    // text-justify	text-align: justify;
    // text-start	text-align: start;
    // text-end	text-align: end;

    private static readonly string[][] values =
    {
        new string[] {"left","middle-left"},
        new string[] {"center","middle-center"},
        new string[] {"right","middle-right"},
    };

    public TextAlign() : base("-unity-text-align", "text") { }

    public override IEnumerable<string[]> GetRules()
    {
        foreach (var value in values)
        {
            yield return new string[] {
                $"{ShortName}-{value[0]}",
                $"{Name}: {value[1]};",
            };
        }
    }
}


public class TransitionProperty : CSSProperty
{
    public TransitionProperty() : base("transition", "transition") { }

    public override IEnumerable<string[]> GetRules()
    {
        yield return new string[] { "duration-0", "transition-duration: 0s;" };
        yield return new string[] { "duration-75", "transition-duration: 75ms;" };
        yield return new string[] { "duration-100", "transition-duration: 100ms;" };
        yield return new string[] { "duration-150", "transition-duration: 150ms;" };
        yield return new string[] { "duration-200", "transition-duration: 200ms;" };
        yield return new string[] { "duration-300", "transition-duration: 300ms;" };
        yield return new string[] { "duration-500", "transition-duration: 500ms;" };
        yield return new string[] { "duration-700", "transition-duration: 700ms;" };
        yield return new string[] { "duration-1000", "transition-duration: 1000ms;" };

        yield return new string[] { "transition-none", "transition-property: none;" };
        yield return new string[] { "transition-all", "transition-property: all; transition-timing-function: ease-in-out-cubic; transition-duration: 150ms;" };
        yield return new string[] { "transition", "transition-property: background-color, border-color, color, fill, stroke, opacity, box-shadow, transform; transition-timing-function: ease-in-out-cubic; transition-duration: 150ms;" };
        yield return new string[] { "transition-colors", "transition-property: background-color, border-color, color, fill, stroke; transition-timing-function: ease-in-out-cubic; transition-duration: 150ms;" };
        yield return new string[] { "transition-opacity", "transition-property: opacity; transition-timing-function: ease-in-out-cubic; transition-duration: 150ms;" };
        yield return new string[] { "transition-shadow", "transition-property: box-shadow; transition-timing-function: ease-in-out-cubic; transition-duration: 150ms;" };
        yield return new string[] { "transition-transform", "transition-property: transform; transition-timing-function: ease-in-out-cubic; transition-duration: 150ms;" };

        yield return new string[] { "ease-linear", "transition-timing-function: linear;" };
        yield return new string[] { "ease-in", "transition-timing-function: ease-in-out-cubic;" };
        yield return new string[] { "ease-out", "transition-timing-function: ease-in-out-cubic;" };
        yield return new string[] { "ease-in-out", "transition-timing-function: ease-in-out-cubic;" };

        // delay
        yield return new string[] { "delay-0", "transition-delay: 0s;" };
        yield return new string[] { "delay-75", "transition-delay: 75ms;" };
        yield return new string[] { "delay-100", "transition-delay: 100ms;" };
        yield return new string[] { "delay-150", "transition-delay: 150ms;" };
        yield return new string[] { "delay-200", "transition-delay: 200ms;" };
        yield return new string[] { "delay-300", "transition-delay: 300ms;" };
        yield return new string[] { "delay-500", "transition-delay: 500ms;" };
        yield return new string[] { "delay-700", "transition-delay: 700ms;" };
        yield return new string[] { "delay-1000", "transition-delay: 1000ms;" };

    }
}

public class TransformProperty : CSSProperty
{
    public TransformProperty() : base("transform", "transform") { }

    public override IEnumerable<string[]> GetRules()
    {
        // transform origin
        yield return new string[] { "origin-center", "transform-origin: center;" };
        yield return new string[] { "origin-top", "transform-origin: top;" };
        yield return new string[] { "origin-top-right", "transform-origin: top right;" };
        yield return new string[] { "origin-right", "transform-origin: right;" };
        yield return new string[] { "origin-bottom-right", "transform-origin: bottom right;" };
        yield return new string[] { "origin-bottom", "transform-origin: bottom;" };
        yield return new string[] { "origin-bottom-left", "transform-origin: bottom left;" };
        yield return new string[] { "origin-left", "transform-origin: left;" };
        yield return new string[] { "origin-top-left", "transform-origin: top left;" };

        // handle for scale and x y procedurally generated
        yield return new string[] { $"scale-0", $"scale: 0 0;" };
        yield return new string[] { $"scale-50", $"scale: 0.5 0.5;" };
        yield return new string[] { $"scale-75", $"scale: 0.75 0.75;" };
        yield return new string[] { $"scale-90", $"scale: 0.9 0.9;" };
        yield return new string[] { $"scale-95", $"scale: 0.95 0.95;" };
        yield return new string[] { $"scale-100", $"scale: 1 1;" };
        yield return new string[] { $"scale-105", $"scale: 1.05 1.05;" };
        yield return new string[] { $"scale-110", $"scale: 1.1 1.1;" };
        yield return new string[] { $"scale-125", $"scale: 1.25 1.25;" };
        yield return new string[] { $"scale-150", $"scale: 1.5 1.5;" };

        yield return new string[] { $"scale-x-0", $"scale: 0 0;" };
        yield return new string[] { $"scale-x-50", $"scale: 0.5 0;" };
        yield return new string[] { $"scale-x-75", $"scale: 0.75 0;" };
        yield return new string[] { $"scale-x-90", $"scale: 0.9 0;" };
        yield return new string[] { $"scale-x-95", $"scale: 0.95 0;" };
        yield return new string[] { $"scale-x-100", $"scale: 1 0;" };
        yield return new string[] { $"scale-x-105", $"scale: 1.05 0;" };
        yield return new string[] { $"scale-x-110", $"scale: 1.1 0;" };
        yield return new string[] { $"scale-x-125", $"scale: 1.25 0;" };
        yield return new string[] { $"scale-x-150", $"scale: 1.5 0;" };

        yield return new string[] { $"scale-y-0", $"scale: 0 0;" };
        yield return new string[] { $"scale-y-50", $"scale: 0 0.5;" };
        yield return new string[] { $"scale-y-75", $"scale: 0 0.75;" };
        yield return new string[] { $"scale-y-90", $"scale: 0 0.9;" };
        yield return new string[] { $"scale-y-95", $"scale: 0 0.95;" };
        yield return new string[] { $"scale-y-100", $"scale: 0 1;" };
        yield return new string[] { $"scale-y-105", $"scale: 0 1.05;" };
        yield return new string[] { $"scale-y-110", $"scale: 0 1.1;" };
        yield return new string[] { $"scale-y-125", $"scale: 0 1.25;" };
        yield return new string[] { $"scale-y-150", $"scale: 0 1.5;" };

        // handle for rotate procedurally generated
        yield return new string[] { $"rotate-0", $"rotate: 0deg;" };
        yield return new string[] { $"rotate-45", $"rotate: 45deg;" };
        yield return new string[] { $"rotate-90", $"rotate: 90deg;" };
        yield return new string[] { $"rotate-135", $"rotate: 135deg;" };
        yield return new string[] { $"rotate-180", $"rotate: 180deg;" };
    }
}

public class USSGenerator
{
    private static readonly CSSProperty[] properties =
    {
        new DimensionProperty("width", "w"),
        new DimensionProperty("height", "h"),
        new ColorProperty("background-color", "bg"),
        new ColorProperty("border-color", "border"),
        new ColorProperty("color", "text"),

        new DimensionProperty("margin", "m") {
            directional = true,
        },
        new DimensionProperty("padding", "p")  {
            directional = true,
        },
        new DimensionProperty("top", "top"),
        new DimensionProperty("bottom", "bottom"),
        new DimensionProperty("left", "left"),
        new DimensionProperty("right", "right"),

        new SizeProperty("width", "w"),
        new SizeProperty("height", "h"),

        new MinMaxProperty("max-width", "max-w"),
        new MinMaxProperty("max-height", "max-h"),
        new MinMaxProperty("min-width", "min-w"),
        new MinMaxProperty("min-height", "min-h"),

        new BorderWidth(),

        new OpacityProperty(),

        new CursorProperty(),

        new FontSizeProperty(),

        new BorderRadius(),

        new TextAlign(),

        new TransformProperty(),

        new TransitionProperty(),

        new GenericProperties() {
            GenerateRules = new string[][] {
                new string[] {"visible", "visibility: visible;"},
                new string[] {"hidden", "visibility: hidden;"},

                new string[] {"overflow-hidden", "overflow: hidden;"},
                new string[] {"overflow-visible", "overflow: visible;"},
        }},

        new GenericProperties() {
            GenerateRules = new string[][] {
                new string[] {"font-bold", "-unity-font-style: bold;"},
                new string[] {"font-normal", "-unity-font-style: normal;"},
                new string[] {"italic", "-unity-font-style: italic;"},
        }},


        new GenericProperties() {
            GenerateRules = new string[][] {
                new string[] {"tracking-tighter", "letter-spacing: -5px"},
                new string[] {"tracking-tight", "letter-spacing: -2.5px"},
                new string[] {"tracking-normal", "letter-spacing: 0px"},
                new string[] {"tracking-wide", "letter-spacing: 2.5px"},
                new string[] {"tracking-wider", "letter-spacing: 5px"},
                new string[] {"tracking-widest", "letter-spacing: 10px"},
        }},

/*
Items
flex-grow: <number>
flex-shrink: <number>
flex-basis: <length> | auto
flex: none | [ <'flex-grow'> <'flex-shrink'>? || <'flex-basis'> ]
align-self: auto | flex-start | flex-end | center | stretch

Containers 
flex-direction: row | row-reverse | column | column-reverse
flex-wrap: nowrap | wrap | wrap-reverse
align-content: flex-start | flex-end | center | stretch

The default value is `stretch`.
`auto` sets `align-items` to `flex-end`.
align-items: auto | flex-start | flex-end | center | stretch 

justify-content: flex-start | flex-end | center | space-between | space-around
*/
        new GenericProperties() {
            GenerateRules = new string[][] {
                new string[] {"absolute", "position: absolute;"},
                new string[] {"relative", "position: relative;"},
                new string[] {"hidden", "display: none;"},

                new string[] {"flex", "display: flex;"},

                new string[] {"flex-row", "flex-direction: row;"},
                new string[] {"flex-col", "flex-direction: column;"},
                new string[] {"flex-row-reverse", "flex-direction: row-reverse;"},
                new string[] {"flex-col-reverse", "flex-direction: column-reverse;"},

                new string[] {"flex-wrap", "flex-wrap: wrap;"},
                new string[] {"flex-nowrap", "flex-wrap: nowrap;"},
                new string[] {"flex-wrap-reverse", "flex-wrap: wrap-reverse;"},

                new string[] {"grow", "flex-grow: 1;"},
                new string[] {"grow-0", "flex-grow: 0;"},

                new string[] {"shrink", "flex-shrink: 1;"},
                new string[] {"shrink-0", "flex-shrink: 0;"},

                new string[] {"justify-start", "justify-content: flex-start;"},
                new string[] {"justify-end", "justify-content: flex-end;"},
                new string[] {"justify-center", "justify-content: center;"},
                new string[] {"justify-between", "justify-content: space-between;"},
                new string[] {"justify-around", "justify-content: space-around;"},

                new string[] {"items-auto", "align-items: auto;"},
                new string[] {"items-start", "align-items: flex-start;"},
                new string[] {"items-end", "align-items: flex-end;"},
                new string[] {"items-center", "align-items: center;"},
                new string[] {"items-stretch", "align-items: stretch;"},
            }
        },
        // add more properties here...
    };

    public static void Generate(string ussFilePath)
    {
        using StreamWriter writer = new StreamWriter(ussFilePath, false);

        foreach (CSSProperty property in properties)
        {
            foreach (string[] rule in property.GetRules())
            {
                writer.WriteLine($".{rule[0]} {{ {rule[1]} }}");
            }
        }

        // hover:
        foreach (CSSProperty property in properties)
            foreach (string[] rule in property.GetRules())
                writer.WriteLine($".hover-{rule[0]}:hover {{ {rule[1]} }}");

        // active
        foreach (CSSProperty property in properties)
            foreach (string[] rule in property.GetRules())
                writer.WriteLine($".active-{rule[0]}:active {{ {rule[1]} }}");

        // inactive
        foreach (CSSProperty property in properties)
            foreach (string[] rule in property.GetRules())
                writer.WriteLine($".inactive-{rule[0]}:inactive {{ {rule[1]} }}");
    }
}
