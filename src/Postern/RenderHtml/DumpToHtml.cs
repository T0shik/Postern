using HyperTextExpression;
using static HyperTextExpression.HtmlExp;

namespace Postern.RenderHtml;

public class DumpToHtml
{
    public static HtmlEl Value(object? val, bool recursive = false, DateTime? stamp = null)
    {
        if (val == null)
        {
            return Div(Attrs("class", "value", "null"));
        }
    
        var type = val.GetType();
        if (val is string || type.IsPrimitive)
        {
            return Div($"{val}");
        }
    
        if (recursive)
        {
            DumpContext.SetCacheEntry(val);
            return Div(
                ("a",
                    Attrs("class", "link",
                        "hx-get", $"/_debug/obj/{val.GetHashCode()}",
                        "hx-swap", "outerHTML",
                        "hx-target", "this"
                    ),
                    type.Name
                )
            );
        }
    
        return
            HtmlEl("div",
                Attrs("class", "obj"),
                ("header", $"{type.Name}"),
                HtmlEl("table",
                    ("tbody",
                        type.GetFields()
                            .Select(field =>
                                HtmlEl("tr",
                                    Attrs("class", "member"),
                                    ("td", field.Name),
                                    HtmlEl("td", Value(field.GetValue(val), recursive: true))
                                )
                            )
                            .Concat(type.GetProperties()
                                .Select(property =>
                                    HtmlEl("tr",
                                        Attrs("class", "member"),
                                        ("td", property.Name),
                                        HtmlEl("td", Value(property.GetValue(val), recursive: true))
                                    )
                                )
                            )
                    )
                )
            );
    }

}