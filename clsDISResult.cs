using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRDocScanIDP
{
    public partial class ClassifyResult
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        [JsonProperty("contentFormat")]
        public ContentFormat ContentFormat { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("pages")]
        public Page[] Pages { get; set; }

        [JsonProperty("paragraphs")]
        public object[] Paragraphs { get; set; }

        [JsonProperty("tables")]
        public object[] Tables { get; set; }

        [JsonProperty("figures")]
        public object[] Figures { get; set; }

        [JsonProperty("sections")]
        public object[] Sections { get; set; }

        [JsonProperty("keyValuePairs")]
        public KeyValuePair[] KeyValuePairs { get; set; }

        [JsonProperty("styles")]
        public object[] Styles { get; set; }

        [JsonProperty("languages")]
        public object[] Languages { get; set; }

        [JsonProperty("documents")]
        public Document[] Documents { get; set; }

        [JsonProperty("warnings")]
        public object[] Warnings { get; set; }
    }

    public partial class Document
    {
        [JsonProperty("fields")]
        public object Fields { get; set; }

        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegion[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public object[] Spans { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class BoundingRegion
    {
        [JsonProperty("pageNumber")]
        public long PageNumber { get; set; }

        [JsonProperty("polygon")]
        public double[] Polygon { get; set; }
    }

    public partial class Page
    {
        [JsonProperty("pageNumber")]
        public long PageNumber { get; set; }

        [JsonProperty("angle")]
        public long Angle { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("unit")]
        public object Unit { get; set; }

        [JsonProperty("spans")]
        public object[] Spans { get; set; }

        [JsonProperty("words")]
        public object[] Words { get; set; }

        [JsonProperty("selectionMarks")]
        public object[] SelectionMarks { get; set; }

        [JsonProperty("lines")]
        public object[] Lines { get; set; }

        [JsonProperty("barcodes")]
        public object[] Barcodes { get; set; }

        [JsonProperty("formulas")]
        public object[] Formulas { get; set; }
    }

    public partial class KeyValuePair
    {
        [JsonProperty("key")]
        public KeyValue Key { get; set; }

        [JsonProperty("value")]
        public KeyValue Value { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class KeyValue
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegion[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("role")]
        public Role Role { get; set; }
    }

    public partial class ClassifyResult
    {
        public static ClassifyResult FromJson(string json) => JsonConvert.DeserializeObject<ClassifyResult>(json, ResultConverter.Settings);
    }

    public static class SerializeResult
    {
        public static string ToJson(this ClassifyResult self) => JsonConvert.SerializeObject(self, ResultConverter.Settings);
    }

    internal static class ResultConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff" }
            },
        };
    }

    public partial class ContentFormat
    {

    }

    public partial class AnalysisResult
    {
        [JsonProperty("apiVersion")]
        public DateTimeOffset ApiVersion { get; set; }

        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        [JsonProperty("contentFormat")]
        public ContentFormat ContentFormat { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("pages")]
        public Page[] Pages { get; set; }

        [JsonProperty("paragraphs")]
        public object[] Paragraphs { get; set; }

        [JsonProperty("tables")]
        public Table[] Tables { get; set; }

        [JsonProperty("figures")]
        public object[] Figures { get; set; }

        [JsonProperty("sections")]
        public object[] Sections { get; set; }

        [JsonProperty("keyValuePairs")]
        public KeyValuePair[] KeyValuePairs { get; set; }

        [JsonProperty("styles")]
        public object[] Styles { get; set; }

        [JsonProperty("languages")]
        public object[] Languages { get; set; }

        [JsonProperty("documents")]
        public DocumentAna[] Documents { get; set; }

        [JsonProperty("warnings")]
        public object[] Warnings { get; set; }
    }

    public partial class DocumentAna
    {
        [JsonProperty("fields")]
        public System.Collections.Generic.Dictionary<string, Field> Fields { get; set; }

        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegionAna[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("confidence")]
        public long Confidence { get; set; }
    }

    public partial class BoundingRegionAna
    {
        [JsonProperty("pageNumber")]
        public long PageNumber { get; set; }

        [JsonProperty("polygon")]
        public long[] Polygon { get; set; }
    }

    public partial class ValueDictionary
    {
        [JsonProperty("Amount")]
        public Field Amount { get; set; }

        [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
        public Field Description { get; set; }

        [JsonProperty("ProductCode", NullValueHandling = NullValueHandling.Ignore)]
        public Field ProductCode { get; set; }

        [JsonProperty("Quantity", NullValueHandling = NullValueHandling.Ignore)]
        public Field Quantity { get; set; }

        [JsonProperty("TaxRate", NullValueHandling = NullValueHandling.Ignore)]
        public Field TaxRate { get; set; }

        [JsonProperty("Unit", NullValueHandling = NullValueHandling.Ignore)]
        public Field Unit { get; set; }

        [JsonProperty("UnitPrice", NullValueHandling = NullValueHandling.Ignore)]
        public Field UnitPrice { get; set; }

        [JsonProperty("Rate", NullValueHandling = NullValueHandling.Ignore)]
        public Field Rate { get; set; }
    }

    public partial class ValueList
    {
        [JsonProperty("valueDictionary")]
        public ValueDictionary ValueDictionary { get; set; }

        [JsonProperty("fieldType")]
        public string FieldType { get; set; }

        [JsonProperty("valueString")]
        public object ValueString { get; set; }

        [JsonProperty("valueDate")]
        public object ValueDate { get; set; }

        [JsonProperty("valueTime")]
        public object ValueTime { get; set; }

        [JsonProperty("valuePhoneNumber")]
        public object ValuePhoneNumber { get; set; }

        [JsonProperty("valueDouble")]
        public object ValueDouble { get; set; }

        [JsonProperty("valueInt64")]
        public object ValueInt64 { get; set; }

        [JsonProperty("valueSelectionMark")]
        public object ValueSelectionMark { get; set; }

        [JsonProperty("valueSignature")]
        public object ValueSignature { get; set; }

        [JsonProperty("valueCountryRegion")]
        public object ValueCountryRegion { get; set; }

        [JsonProperty("valueList")]
        public object[] ValueListValueList { get; set; }

        [JsonProperty("valueCurrency")]
        public object ValueCurrency { get; set; }

        [JsonProperty("valueAddress")]
        public object ValueAddress { get; set; }

        [JsonProperty("valueBoolean")]
        public object ValueBoolean { get; set; }

        [JsonProperty("valueSelectionGroup")]
        public object[] ValueSelectionGroup { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegion[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class Field
    {
        [JsonProperty("valueDictionary")]
        public ContentFormat ValueDictionary { get; set; }

        [JsonProperty("fieldType")]
        public ContentFormat FieldType { get; set; }

        [JsonProperty("valueString")]
        public string ValueString { get; set; }

        [JsonProperty("valueDate")]
        public DateTimeOffset? ValueDate { get; set; }

        [JsonProperty("valueTime")]
        public object ValueTime { get; set; }

        [JsonProperty("valuePhoneNumber")]
        public object ValuePhoneNumber { get; set; }

        [JsonProperty("valueDouble")]
        public long? ValueDouble { get; set; }

        [JsonProperty("valueInt64")]
        public object ValueInt64 { get; set; }

        [JsonProperty("valueSelectionMark")]
        public object ValueSelectionMark { get; set; }

        [JsonProperty("valueSignature")]
        public object ValueSignature { get; set; }

        [JsonProperty("valueCountryRegion")]
        public object ValueCountryRegion { get; set; }

        [JsonProperty("valueList")]
        public ValueList[] ValueList { get; set; }

        [JsonProperty("valueCurrency")]
        public ValueCurrency ValueCurrency { get; set; }

        [JsonProperty("valueAddress")]
        public System.Collections.Generic.Dictionary<string, string> ValueAddress { get; set; }

        [JsonProperty("valueBoolean")]
        public object ValueBoolean { get; set; }

        [JsonProperty("valueSelectionGroup")]
        public object[] ValueSelectionGroup { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegionAna[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("confidence")]
        public double? Confidence { get; set; }
    }

    public partial class Span
    {
        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }
    }

    public partial class ValueCurrency
    {
        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("currencySymbol")]
        public object CurrencySymbol { get; set; }

        [JsonProperty("currencyCode")]
        public CurrencyCode CurrencyCode { get; set; }
    }

    public partial class PageAna
    {
        [JsonProperty("pageNumber")]
        public long PageNumber { get; set; }

        [JsonProperty("angle")]
        public double Angle { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("unit")]
        public ContentFormat Unit { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("words")]
        public Word[] Words { get; set; }

        [JsonProperty("selectionMarks")]
        public object[] SelectionMarks { get; set; }

        [JsonProperty("lines")]
        public Line[] Lines { get; set; }

        [JsonProperty("barcodes")]
        public object[] Barcodes { get; set; }

        [JsonProperty("formulas")]
        public object[] Formulas { get; set; }
    }

    public partial class Line
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("polygon")]
        public long[] Polygon { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }
    }

    public partial class Word
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("polygon")]
        public long[] Polygon { get; set; }

        [JsonProperty("span")]
        public Span Span { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class Table
    {
        [JsonProperty("rowCount")]
        public long RowCount { get; set; }

        [JsonProperty("columnCount")]
        public long ColumnCount { get; set; }

        [JsonProperty("cells")]
        public Cell[] Cells { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegionAna[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("caption")]
        public object Caption { get; set; }

        [JsonProperty("footnotes")]
        public object[] Footnotes { get; set; }
    }

    public partial class Cell
    {
        [JsonProperty("kind")]
        public object Kind { get; set; }

        [JsonProperty("rowIndex")]
        public long RowIndex { get; set; }

        [JsonProperty("columnIndex")]
        public long ColumnIndex { get; set; }

        [JsonProperty("rowSpan")]
        public object RowSpan { get; set; }

        [JsonProperty("columnSpan")]
        public long? ColumnSpan { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegionAna[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("elements")]
        public object[] Elements { get; set; }
    }

    public enum CurrencyCode { Gbp };

    public partial class AnalysisResult
    {
        public static AnalysisResult FromJson(string json) => JsonConvert.DeserializeObject<AnalysisResult>(json, AnalysisConverter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this AnalysisResult self) => JsonConvert.SerializeObject(self, AnalysisConverter.Settings);
    }

    internal static class AnalysisConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                CurrencyCodeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                //new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff" }
                //new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'" }
            },
        };
    }

    internal class CurrencyCodeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(CurrencyCode) || t == typeof(CurrencyCode?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "GBP")
            {
                return CurrencyCode.Gbp;
            }
            throw new Exception("Cannot unmarshal type CurrencyCode");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (CurrencyCode)untypedValue;
            if (value == CurrencyCode.Gbp)
            {
                serializer.Serialize(writer, "GBP");
                return;
            }
            throw new Exception("Cannot marshal type CurrencyCode");
        }

        public static readonly CurrencyCodeConverter Singleton = new CurrencyCodeConverter();
    }



}
