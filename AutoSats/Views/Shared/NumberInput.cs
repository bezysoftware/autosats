using AntDesign;
using Microsoft.AspNetCore.Components.Rendering;

namespace AutoSats.Views.Shared;

/// <summary>
/// Force a number input to be on en-US culture (dot as decimal separator).
/// </summary>
public class NumberInput<TValue> : Input<TValue?>
{
    public NumberInput()
    {
        CultureInfo = new System.Globalization.CultureInfo("en-US");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // temporarily switch to en-US culture to enforce dot as decimal separator when building the render tree
        var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo;
        base.BuildRenderTree(builder);
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
    }
}
