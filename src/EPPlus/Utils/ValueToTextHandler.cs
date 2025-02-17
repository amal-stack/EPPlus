/*************************************************************************************************
  Required Notice: Copyright (C) EPPlus Software AB. 
  This software is licensed under PolyForm Noncommercial License 1.0.0 
  and may only be used for noncommercial purposes 
  https://polyformproject.org/licenses/noncommercial/1.0.0/

  A commercial license to use this software can be purchased at https://epplussoftware.com
 *************************************************************************************************
  Date               Author                       Change
 *************************************************************************************************
  01/27/2020         EPPlus Software AB       Initial release EPPlus 5
 *************************************************************************************************/
using OfficeOpenXml;
using OfficeOpenXml.Compatibility;
using OfficeOpenXml.Style.XmlAccess;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OfficeOpenXml.Utils
{
    internal static class ValueToTextHandler
    {
        internal static string GetFormattedText(object Value, ExcelWorkbook wb, int styleId, bool forWidthCalc, CultureInfo cultureInfo=null)
        {
            object v = Value;
            if (v == null) return "";
            var styles = wb.Styles;
            var nfID = styles.CellXfs[styleId].NumberFormatId;
            ExcelNumberFormatXml.ExcelFormatTranslator nf = null;
            for (int i = 0; i < styles.NumberFormats.Count; i++)
            {
                if (nfID == styles.NumberFormats[i].NumFmtId)
                {
                    nf = styles.NumberFormats[i].FormatTranslator;
                    break;
                }
            }
            if (nf == null)
            {
                nf = styles.NumberFormats[0].FormatTranslator;  //nf should never be null. If so set to General, Issue 173
            }

            string format, textFormat;
            if (forWidthCalc)
            {
                format = nf.NetFormatForWidth;
                textFormat = nf.NetTextFormatForWidth;
            }
            else
            {
                format = nf.NetFormat;
                textFormat = nf.NetTextFormat;
            }

            return FormatValue(v, nf, format, textFormat, cultureInfo);
        }
        internal static string FormatValue(object v, ExcelNumberFormatXml.ExcelFormatTranslator nf, string format, string textFormat, CultureInfo overrideCultureInfo)
        {
            if (v is decimal || TypeCompat.IsPrimitive(v))
            {
                double d;
                try
                {
                    d = Convert.ToDouble(v);
                }
                catch
                {
                    return "";
                }

                if (nf.DataType == ExcelNumberFormatXml.eFormatType.Number)
                {
                    if (string.IsNullOrEmpty(nf.FractionFormat))
                    {                        
                        return FormatNumber(d, format, nf, overrideCultureInfo);
                    }
                    else
                    {
                        return nf.FormatFraction(d);
                    }
                }
                else if (nf.DataType == ExcelNumberFormatXml.eFormatType.DateTime)
                {
                    var date = DateTime.FromOADate(d);
                    return GetDateText(date, format, nf, overrideCultureInfo);
                }
            }
            else if (v is DateTime)
            {
                if (nf.DataType == ExcelNumberFormatXml.eFormatType.DateTime)
                {
                    return GetDateText((DateTime)v, format, nf, overrideCultureInfo);
                }
                else
                {
                    double d = ((DateTime)v).ToOADate();
                    if (string.IsNullOrEmpty(nf.FractionFormat))
                    {
                        return d.ToString(format, nf.Culture);
                    }
                    else
                    {
                        return nf.FormatFraction(d);
                    }
                }
            }
            else if (v is TimeSpan)
            {
                if (nf.DataType == ExcelNumberFormatXml.eFormatType.DateTime)
                {
                    return GetDateText(new DateTime(((TimeSpan)v).Ticks), format, nf, overrideCultureInfo);
                }
                else
                {
                    double d = new DateTime(0).Add((TimeSpan)v).ToOADate();
                    if (string.IsNullOrEmpty(nf.FractionFormat))
                    {
                        return d.ToString(format, nf.Culture);
                    }
                    else
                    {
                        return nf.FormatFraction(d);
                    }
                }
            }
            else
            {
                if (textFormat == "")
                {
                    return v.ToString();
                }
                else
                {
                    return string.Format(textFormat, v);
                }
            }
            return v.ToString();
        }

        private static string FormatNumber(double d, string format, ExcelNumberFormatXml.ExcelFormatTranslator nf, CultureInfo overrideCultureInfo)
        {
            var split = format.Split(';');
            if (split.Length == 3)
            {                
                if(d>0)
                {
                    return FormatNumberExcel(d, split[0], overrideCultureInfo ?? nf.Culture);
                }
                else if(d<0)
                {
                    var s= FormatNumberExcel(d, split[1], overrideCultureInfo ?? nf.Culture);
                    if(s.StartsWith("--") && split[1].StartsWith("-"))
                    {
                        return s.Substring(1);
                    }
                    else if(s.StartsWith("-(", StringComparison.OrdinalIgnoreCase) && split[1].StartsWith("(", StringComparison.OrdinalIgnoreCase) && split[1].EndsWith(")", StringComparison.OrdinalIgnoreCase))
                    {
                        return s.Substring(1);
                    }
                    return s;
                }
                else
                {
                    return FormatNumberExcel(d, split[2], overrideCultureInfo ?? nf.Culture);
                }
            }
            else
            {
                var s = FormatNumberExcel(d, format, overrideCultureInfo ?? nf.Culture);
                if (s.StartsWith("-(", StringComparison.OrdinalIgnoreCase) && format.StartsWith("(", StringComparison.OrdinalIgnoreCase) && format.EndsWith(")", StringComparison.OrdinalIgnoreCase))
                {
                    return s.Substring(1);
                }
                else
                {
                    return s;
                }
            }
        }

        private static string FormatNumberExcel(double d, string format, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(format))
            {
                return null;
            }
            else
            {
                return d.ToString(format, cultureInfo);
            }
        }

        private static string GetDateText(DateTime d, string format, ExcelNumberFormatXml.ExcelFormatTranslator nf, CultureInfo overrideCultureInfo)
        {           
            if (nf.SpecialDateFormat == ExcelNumberFormatXml.ExcelFormatTranslator.eSystemDateFormat.SystemLongDate)
            {
                return d.ToLongDateString();
            }
            else if (nf.SpecialDateFormat == ExcelNumberFormatXml.ExcelFormatTranslator.eSystemDateFormat.SystemLongTime)
            {
                return d.ToLongTimeString();
            }
            else if (nf.SpecialDateFormat == ExcelNumberFormatXml.ExcelFormatTranslator.eSystemDateFormat.SystemShortDate)
            {
                return d.ToShortDateString();
            }
            if (format == "d" || format == "D")
            {
                return d.Day.ToString();
            }
            else if (format == "M")
            {
                return d.Month.ToString();
            }
            else if (format == "m")
            {
                return d.Minute.ToString();
            }
            else if (format.ToLower() == "y" || format.ToLower() == "yy")
            {
                return d.ToString("yy", overrideCultureInfo ?? nf.Culture);
            }
            else if (format.ToLower() == "yyy" || format.ToLower() == "yyyy")
            {
                return d.ToString("yyy", overrideCultureInfo ?? nf.Culture);
            }
            else
            {
                return d.ToString(format, overrideCultureInfo ?? nf.Culture);
            }

        }
    }
}
