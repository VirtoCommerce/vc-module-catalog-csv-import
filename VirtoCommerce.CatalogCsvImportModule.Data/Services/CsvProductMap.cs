using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogCsvImportModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public sealed class CsvProductMap : CsvClassMap<CsvProduct>
    {
        public CsvProductMap(CsvProductMappingConfiguration mappingCfg)
        {
            //Dynamical map scalar product fields use by manual mapping information
            foreach (var mappingItem in mappingCfg.PropertyMaps.Where(x => !string.IsNullOrEmpty(x.CsvColumnName) || !string.IsNullOrEmpty(x.CustomValue)))
            {
                var propertyInfo = typeof(CsvProduct).GetProperty(mappingItem.EntityColumnName);
                if (propertyInfo != null)
                {
                    var newMap = new CsvPropertyMap(propertyInfo);
                    newMap.TypeConverterOption(CultureInfo.InvariantCulture);
                    newMap.TypeConverterOption(NumberStyles.Any);
                    newMap.TypeConverterOption(true, "yes", "true");
                    newMap.TypeConverterOption(false, "false", "no");
                    if (!string.IsNullOrEmpty(mappingItem.CsvColumnName))
                    {
                        //Map fields if mapping specified
                        newMap.Name(mappingItem.CsvColumnName);
                    }
                    //And default values if it specified
                    else if (mappingItem.CustomValue != null)
                    {
                        var typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                        newMap.ConvertUsing(row => typeConverter.ConvertFromString(mappingItem.CustomValue));
                        newMap.Default(mappingItem.CustomValue);
                    }
                    PropertyMaps.Add(newMap);
                }
            }

            //Map properties
            if (mappingCfg.PropertyCsvColumns != null && mappingCfg.PropertyCsvColumns.Any())
            {
                // Exporting multiple csv fields from the same property (which is a collection)
                foreach (var propertyCsvColumn in mappingCfg.PropertyCsvColumns)
                {
                    // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                    // to export multiple entries for the same property
                    var csvPropertyMap = new CsvPropertyMap(typeof(CsvProduct).GetProperty("PropertyValues"));
                    csvPropertyMap.Name(propertyCsvColumn);

                    // create custom converter instance which will get the required record from the collection
                    csvPropertyMap.UsingExpression<ICollection<coreModel.PropertyValue>>(null, propValues =>
                         {
                             var multiValueProperty = propValues.Where(x => x.PropertyName == propertyCsvColumn).ToList();
                             if (multiValueProperty.Count == 1)
                             {
                                 var propValue = multiValueProperty.First();
                                 return propValue.Value?.ToString() ?? string.Empty;
                             }

                             if (multiValueProperty.Count > 1)
                             {
                                 var props = multiValueProperty.Where(x => x.Value != null).Select(x => x.Value.ToString());
                                 var result = string.Join(mappingCfg.Delimiter, props);
                                 return result;
                             }

                             return string.Empty;
                         });

                    PropertyMaps.Add(csvPropertyMap);
                }

                var newPropMap = new CsvPropertyMap(typeof(CsvProduct).GetProperty("PropertyValues"));
                newPropMap.UsingExpression<ICollection<coreModel.PropertyValue>>(null, null).ConvertUsing(x => mappingCfg.PropertyCsvColumns.Select(column => new coreModel.PropertyValue { PropertyName = column, Value = x.GetField<string>(column) }).ToList());
                PropertyMaps.Add(newPropMap);
            }
        }
    }
}
