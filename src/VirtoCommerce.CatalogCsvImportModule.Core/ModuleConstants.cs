using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogCsvImportModule.Core
{
    public static class ModuleConstants
    {
        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor CreateDictionaryValues { get; } = new SettingDescriptor
                {
                    Name = "CatalogCsvImport.CreateDictionaryValues",
                    GroupName = "CatalogCsvImport|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static SettingDescriptor ExportFileNameTemplate { get; } = new SettingDescriptor
                {
                    Name = "CatalogCsvImport.ExportFileNameTemplate",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "CatalogCsvImport|General",
                    DefaultValue = "products_{0:yyyy-MM-dd_HH-mm-ss}"
                };
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    yield return General.CreateDictionaryValues;
                    yield return General.ExportFileNameTemplate;
                }
            }
        }
    }
}
