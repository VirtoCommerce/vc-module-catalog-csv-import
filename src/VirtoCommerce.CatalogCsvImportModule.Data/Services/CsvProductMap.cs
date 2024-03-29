using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogCsvImportModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogCsvImportModule.Data.Services
{
    public sealed class CsvProductMap : ClassMap<CsvProduct>
    {
        public CsvProductMap(CsvProductMappingConfiguration mappingCfg)
        {
            //Dynamical map scalar product fields use by manual mapping information
            var index = 0;

            foreach (var mappingItem in mappingCfg.PropertyMaps.Where(x => !string.IsNullOrEmpty(x.CsvColumnName) || !string.IsNullOrEmpty(x.CustomValue)))
            {
                var propertyInfo = typeof(CsvProduct).GetProperty(mappingItem.EntityColumnName);
                if (propertyInfo != null)
                {
                    var newMap = MemberMap.CreateGeneric(typeof(CsvProduct), propertyInfo);

                    newMap.Data.TypeConverterOptions.CultureInfo = CultureInfo.InvariantCulture;
                    newMap.Data.TypeConverterOptions.NumberStyles = NumberStyles.Any;
                    newMap.Data.TypeConverterOptions.BooleanTrueValues.AddRange(new List<string>() { "True", "Yes" });
                    newMap.Data.TypeConverterOptions.BooleanFalseValues.AddRange(new List<string>() { "False", "No" });

                    newMap.Data.Index = ++index;

                    if (!string.IsNullOrEmpty(mappingItem.CsvColumnName))
                    {
                        //Map fields if mapping specified
                        newMap.Name(mappingItem.CsvColumnName);
                    }
                    //And default values if it specified
                    else if (mappingItem.CustomValue != null)
                    {
                        var typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                        newMap.Data.ReadingConvertExpression = (Expression<Func<ConvertFromStringArgs, object>>)(x => typeConverter.ConvertFromString(mappingItem.CustomValue));
                    }
                    MemberMaps.Add(newMap);
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

                    var propertyValuesInfo = typeof(CsvProduct).GetProperty(nameof(CsvProduct.Properties));
                    var csvPropertyMap = MemberMap.CreateGeneric(typeof(CsvProduct), propertyValuesInfo);
                    csvPropertyMap.Name(propertyCsvColumn);

                    csvPropertyMap.Data.Index = ++index;

                    // create custom converter instance which will get the required record from the collection
                    csvPropertyMap.UsingExpression<ICollection<Property>>(null, properties =>
                         {
                             var property = properties.FirstOrDefault(x => x.Name == propertyCsvColumn && x.Values.Any());
                             var propertyValues = Array.Empty<string>();
                             if (property != null)
                             {
                                 if (property.Dictionary)
                                 {
                                     propertyValues = property.Values
                                         ?.Where(x => !string.IsNullOrEmpty(x.Alias))
                                         .Select(x => x.Alias)
                                         .Distinct()
                                         .ToArray();
                                 }
                                 else if(property.Multilanguage)
                                 {
                                     propertyValues = property.Values.Select(v => string.Join(null, v.LanguageCode, CsvReaderExtension.InnerDelimiter, v.Value)).ToArray();
                                 }
                                 else
                                 {
                                     propertyValues = property.Values
                                         ?.Where(x => x.Value != null || x.Alias != null)
                                         .Select(x => x.Alias ?? x.Value.ToString())
                                         .ToArray();
                                 }
                             }

                             return string.Join(mappingCfg.Delimiter, propertyValues);
                         });
                    
                    MemberMaps.Add(csvPropertyMap);
                }

                var newPropInfo = typeof(CsvProduct).GetProperty(nameof(CsvProduct.Properties));
                var newPropMap = MemberMap.CreateGeneric(typeof(CsvProduct), newPropInfo);
                newPropMap.Data.ReadingConvertExpression =
                    (Expression<Func<ConvertFromStringArgs, object>>)(x => mappingCfg.PropertyCsvColumns.Select(column =>
                        (Property)new CsvProperty
                        {
                            Name = column,
                            Values = x.Row.GetPropertiesByColumn(column).ToList()
                        }).ToList());
                newPropMap.UsingExpression<ICollection<PropertyValue>>(null, null);

                newPropMap.Data.Index = ++index;

                MemberMaps.Add(newPropMap);
                newPropMap.Ignore(true);
            }

            //map line number
            var lineNumMeber = Map(m => m.LineNumber).Convert(row => row.Row.Parser.RawRow);
            lineNumMeber.Data.Index = ++index;
            lineNumMeber.Ignore(true);
        }
    }
}
