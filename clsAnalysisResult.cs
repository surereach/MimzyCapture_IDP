using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRDocScanIDP;

namespace SRDocScanIDPForm
{
    //Typically used by Form document IDP.
    public partial class FormAnalysisResult
    {
        [JsonProperty("serviceVersion")]
        public DateTimeOffset ServiceVersion { get; set; }

        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("pages")]
        public Page[] Pages { get; set; }

        [JsonProperty("paragraphs")]
        public Paragraph[] Paragraphs { get; set; }

        [JsonProperty("tables")]
        public Table[] Tables { get; set; }

        [JsonProperty("keyValuePairs")]
        public KeyValuePair[] KeyValuePairs { get; set; }

        [JsonProperty("styles")]
        public Style[] Styles { get; set; }

        [JsonProperty("languages")]
        public object[] Languages { get; set; }

        [JsonProperty("documents")]
        public Document[] Documents { get; set; }
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

    public partial class KeyValuePair
    {
        [JsonProperty("key")]
        public Paragraph Key { get; set; }

        [JsonProperty("value")]
        public Paragraph Value { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class Paragraph
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

    public partial class BoundingRegion
    {
        [JsonProperty("pageNumber")]
        public long PageNumber { get; set; }

        [JsonProperty("boundingPolygon")]
        public BoundingPolygon[] BoundingPolygon { get; set; }
    }

    public partial class BoundingPolygon
    {
        [JsonProperty("isEmpty")]
        public bool IsEmpty { get; set; }

        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public partial class Role
    {
    }

    public partial class Span
    {
        [JsonProperty("index")]
        public long Index { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }
    }

    public partial class Document
    {
        [JsonProperty("fields")]
        public System.Collections.Generic.Dictionary<string, Field> Fields { get; set; }

        [JsonProperty("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("boundingRegions")]
        public BoundingRegion[] BoundingRegions { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("confidence")]
        public long Confidence { get; set; }
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

    public partial class Page
    {
        [JsonProperty("unit")]
        public long Unit { get; set; }

        [JsonProperty("pageNumber")]
        public long PageNumber { get; set; }

        [JsonProperty("angle")]
        public double Angle { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("words")]
        public SelectionMark[] Words { get; set; }

        [JsonProperty("selectionMarks")]
        public SelectionMark[] SelectionMarks { get; set; }

        [JsonProperty("lines")]
        public Line[] Lines { get; set; }

        [JsonProperty("barcodes")]
        public object[] Barcodes { get; set; }

        [JsonProperty("formulas")]
        public object[] Formulas { get; set; }
    }

    public partial class Line
    {
        [JsonProperty("boundingPolygon")]
        public BoundingPolygon[] BoundingPolygon { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }
    }

    public partial class SelectionMark
    {
        [JsonProperty("boundingPolygon")]
        public BoundingPolygon[] BoundingPolygon { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public long? State { get; set; }

        [JsonProperty("span")]
        public Span Span { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }
    }

    public partial class Style
    {
        [JsonProperty("isHandwritten")]
        public bool IsHandwritten { get; set; }

        [JsonProperty("similarFontFamily")]
        public object SimilarFontFamily { get; set; }

        [JsonProperty("fontStyle")]
        public object FontStyle { get; set; }

        [JsonProperty("fontWeight")]
        public object FontWeight { get; set; }

        [JsonProperty("color")]
        public object Color { get; set; }

        [JsonProperty("backgroundColor")]
        public object BackgroundColor { get; set; }

        [JsonProperty("spans")]
        public Span[] Spans { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public partial class FormAnalysisResult
    {
        public static FormAnalysisResult FromJson(string json) => JsonConvert.DeserializeObject<FormAnalysisResult>(json, FormConverter.Settings);
    }

    public static class SerializeForm
    {
        public static string ToJson(this FormAnalysisResult self) => JsonConvert.SerializeObject(self, FormConverter.Settings);
    }

    public static class SerializeRegion
    {
        public static string ToJson(this BoundingRegion[] self) => JsonConvert.SerializeObject(self, FormConverter.Settings);
    }

    internal static class FormConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }



}
