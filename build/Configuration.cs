using System;
using System.ComponentModel;
using System.Linq;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    public static Configuration Debug = new Configuration { Value = nameof(Debug) };
    public static Configuration Release = new Configuration { Value = nameof(Release) };
    public static Configuration Revit2021 = new Configuration { Value = "2021" };
    public static Configuration Revit2022 = new Configuration { Value = "2022" };
    public static Configuration Revit2023 = new Configuration { Value = "2023" };

    public static implicit operator string(Configuration configuration)
    {
        return configuration.Value;
    }
}
