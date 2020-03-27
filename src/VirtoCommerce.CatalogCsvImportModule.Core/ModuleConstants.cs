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
                    Name = "CsvCatalogImport.CreateDictionaryValues",
                    GroupName = "CsvCatalogImport|General",
                    ValueType = SettingValueType.Boolean,
                };

                public static SettingDescriptor ExportFileNameTemplate { get; } = new SettingDescriptor
                {
                    Name = "CsvCatalogImport.ExportFileNameTemplate",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "CsvCatalogImport|General",
                    DefaultValue = "products_{0:yyyy-MM-dd_HH-mm-ss}"

                };
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return new List<SettingDescriptor>
                    {
                        General.CreateDictionaryValues,
                        General.ExportFileNameTemplate
                    };
                }
            }

        }
    }
}
