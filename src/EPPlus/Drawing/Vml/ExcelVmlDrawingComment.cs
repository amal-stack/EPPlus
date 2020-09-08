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
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Drawing;

namespace OfficeOpenXml.Drawing.Vml
{
    /// <summary>
    /// Drawing object used for comments
    /// </summary>
    public class ExcelVmlDrawingComment : ExcelVmlDrawingBase, IRangeID
    {
        internal ExcelVmlDrawingComment(XmlNode topNode, ExcelRangeBase range, XmlNamespaceManager ns) :
            base(topNode, ns)
        {
            Range = range;
            SchemaNodeOrder = new string[] { "fill", "stroke", "shadow", "path", "textbox", "ClientData", "MoveWithCells", "SizeWithCells", "Anchor", "Locked", "AutoFill", "LockText", "TextHAlign", "TextVAlign", "Row", "Column", "Visible" };
        }   
        internal ExcelRangeBase Range { get; set; }

        /// <summary>
        /// Address in the worksheet
        /// </summary>
        public string Address
        {
            get
            {
                return Range.Address;
            }
        }

        const string VERTICAL_ALIGNMENT_PATH="x:ClientData/x:TextVAlign";
        /// <summary>
        /// Vertical alignment for text
        /// </summary>
        public eTextAlignVerticalVml VerticalAlignment
        {
            get
            {
                switch (GetXmlNodeString(VERTICAL_ALIGNMENT_PATH))
                {
                    case "Center":
                        return eTextAlignVerticalVml.Center;
                    case "Bottom":
                        return eTextAlignVerticalVml.Bottom;
                    default:
                        return eTextAlignVerticalVml.Top;
                }
            }
            set
            {
                switch (value)
                {
                    case eTextAlignVerticalVml.Center:
                        SetXmlNodeString(VERTICAL_ALIGNMENT_PATH, "Center");
                        break;
                    case eTextAlignVerticalVml.Bottom:
                        SetXmlNodeString(VERTICAL_ALIGNMENT_PATH, "Bottom");
                        break;
                    default:
                        DeleteNode(VERTICAL_ALIGNMENT_PATH);
                        break;
                }
            }
        }
        const string HORIZONTAL_ALIGNMENT_PATH="x:ClientData/x:TextHAlign";
        /// <summary>
        /// Horizontal alignment for text
        /// </summary>
        public eTextAlignHorizontalVml HorizontalAlignment
        {
            get
            {
                switch (GetXmlNodeString(HORIZONTAL_ALIGNMENT_PATH))
                {
                    case "Center":
                        return eTextAlignHorizontalVml.Center;
                    case "Right":
                        return eTextAlignHorizontalVml.Right;
                    default:
                        return eTextAlignHorizontalVml.Left;
                }
            }
            set
            {
                switch (value)
                {
                    case eTextAlignHorizontalVml.Center:
                        SetXmlNodeString(HORIZONTAL_ALIGNMENT_PATH, "Center");
                        break;
                    case eTextAlignHorizontalVml.Right:
                        SetXmlNodeString(HORIZONTAL_ALIGNMENT_PATH, "Right");
                        break;
                    default:
                        DeleteNode(HORIZONTAL_ALIGNMENT_PATH);
                        break;
                }
            }
        }
        const string VISIBLE_PATH = "x:ClientData/x:Visible";
        /// <summary>
        /// If the drawing object is visible.
        /// </summary>
        public bool Visible 
        { 
            get
            {
                return (TopNode.SelectSingleNode(VISIBLE_PATH, NameSpaceManager)!=null);
            }
            set
            {
                if (value)
                {
                    CreateNode(VISIBLE_PATH);
                    Style = SetStyle(Style,"visibility", "visible");
                }
                else
                {
                    DeleteNode(VISIBLE_PATH);
                    Style = SetStyle(Style,"visibility", "hidden");
                }                
            }
        }

        const string BACKGROUNDCOLOR_PATH = "@fillcolor";
        const string BACKGROUNDCOLOR2_PATH = "v:fill/@color2";
        /// <summary>
        /// Background color
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                string col = GetXmlNodeString(BACKGROUNDCOLOR_PATH);
                if (col == "")
                {
                    return Color.FromArgb(0xff, 0xff, 0xe1);
                }
                else
                {
                    if(col.StartsWith("#")) col=col.Substring(1,col.Length-1);
                    int res;
                    if (int.TryParse(col,System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out res))
                    {
                        return Color.FromArgb(res);
                    }
                    else
                    {
                        return Color.Empty;
                    }
                }
            }
            set
            {
                string color = "#" + value.ToArgb().ToString("X").Substring(2, 6);
                SetXmlNodeString(BACKGROUNDCOLOR_PATH, color);
                //SetXmlNode(BACKGROUNDCOLOR2_PATH, color);
            }
        }
        const string LINESTYLE_PATH="v:stroke/@dashstyle";
        const string ENDCAP_PATH = "v:stroke/@endcap";
        /// <summary>
        /// Linestyle for border
        /// </summary>
        public eLineStyleVml LineStyle 
        { 
            get
            {
                string v=GetXmlNodeString(LINESTYLE_PATH);
                if (v == "")
                {
                    return eLineStyleVml.Solid;
                }
                else if (v == "1 1")
                {
                    v = GetXmlNodeString(ENDCAP_PATH);
                    return (eLineStyleVml)Enum.Parse(typeof(eLineStyleVml), v, true);
                }
                else
                {
                    return (eLineStyleVml)Enum.Parse(typeof(eLineStyleVml), v, true);
                }
            }
            set
            {
                if (value == eLineStyleVml.Round || value == eLineStyleVml.Square)
                {
                    SetXmlNodeString(LINESTYLE_PATH, "1 1");
                    if (value == eLineStyleVml.Round)
                    {
                        SetXmlNodeString(ENDCAP_PATH, "round");
                    }
                    else
                    {
                        DeleteNode(ENDCAP_PATH);
                    }
                }
                else
                {
                    string v = value.ToString();
                    v = v.Substring(0, 1).ToLower(CultureInfo.InvariantCulture) + v.Substring(1, v.Length - 1);
                    SetXmlNodeString(LINESTYLE_PATH, v);
                    DeleteNode(ENDCAP_PATH);
                }
            }
        }
        const string LINECOLOR_PATH="@strokecolor";
        /// <summary>
        /// Line color 
        /// </summary>
        public Color LineColor
        {
            get
            {
                string col = GetXmlNodeString(LINECOLOR_PATH);
                if (col == "")
                {
                    return Color.Black;
                }
                else
                {
                    if (col.StartsWith("#")) col = col.Substring(1, col.Length - 1);
                    int res;
                    if (int.TryParse(col, System.Globalization.NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out res))
                    {
                        return Color.FromArgb(res);
                    }
                    else
                    {
                        return Color.Empty;
                    }
                }                
            }
            set
            {
                string color = "#" + value.ToArgb().ToString("X").Substring(2, 6);
                SetXmlNodeString(LINECOLOR_PATH, color);
            }
        }
        const string LINEWIDTH_PATH="@strokeweight";
        /// <summary>
        /// Width of the border
        /// </summary>
        public Single LineWidth 
        {
            get
            {
                string wt=GetXmlNodeString(LINEWIDTH_PATH);
                if (wt == "") return (Single).75;
                if(wt.EndsWith("pt")) wt=wt.Substring(0,wt.Length-2);

                Single ret;
                if(Single.TryParse(wt,System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out ret))
                {
                    return ret;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                SetXmlNodeString(LINEWIDTH_PATH, value.ToString(CultureInfo.InvariantCulture) + "pt");
            }
        }
        const string TEXTBOX_STYLE_PATH = "v:textbox/@style";
        /// <summary>
        /// Autofits the drawingobject 
        /// </summary>
        public bool AutoFit
        {
            get
            {
                string value;
                GetStyle(GetXmlNodeString(TEXTBOX_STYLE_PATH), "mso-fit-shape-to-text", out value);
                return value=="t";
            }
            set
            {                
                SetXmlNodeString(TEXTBOX_STYLE_PATH, SetStyle(GetXmlNodeString(TEXTBOX_STYLE_PATH),"mso-fit-shape-to-text", value?"t":"")); 
            }
        }        
        const string LOCKED_PATH = "x:ClientData/x:Locked";
        /// <summary>
        /// If the object is locked when the sheet is protected
        /// </summary>
        public bool Locked 
        {
            get
            {
                return GetXmlNodeBool(LOCKED_PATH, false);
            }
            set
            {
                SetXmlNodeBool(LOCKED_PATH, value, false);                
            }
        }
        const string LOCK_TEXT_PATH = "x:ClientData/x:LockText";        
        /// <summary>
        /// Specifies that the object's text is locked
        /// </summary>
        public bool LockText
        {
            get
            {
                return GetXmlNodeBool(LOCK_TEXT_PATH, false);
            }
            set
            {
                SetXmlNodeBool(LOCK_TEXT_PATH, value, false);
            }
        }
        ExcelVmlDrawingPosition _from = null;
        /// <summary>
        /// From position. For comments only when Visible=true.
        /// </summary>
        public ExcelVmlDrawingPosition From
        {
            get
            {
                if (_from == null)
                {
                    _from = new ExcelVmlDrawingPosition(NameSpaceManager, TopNode.SelectSingleNode("x:ClientData", NameSpaceManager), 0);
                }
                return _from;
            }
        }
        ExcelVmlDrawingPosition _to = null;
        /// <summary>
        /// To position. For comments only when Visible=true.
        /// </summary>
        public ExcelVmlDrawingPosition To
        {
            get
            {
                if (_to == null)
                {
                    _to = new ExcelVmlDrawingPosition(NameSpaceManager, TopNode.SelectSingleNode("x:ClientData", NameSpaceManager), 4);
                }
                return _to;
            }
        }
        const string ROW_PATH = "x:ClientData/x:Row";
        /// <summary>
        /// Row position for a comment
        /// </summary>
        internal int Row
        {
            get
            {
                return GetXmlNodeInt(ROW_PATH);
            }
            set
            {
                SetXmlNodeString(ROW_PATH, value.ToString(CultureInfo.InvariantCulture));
            }
        }
        const string COLUMN_PATH = "x:ClientData/x:Column";
        /// <summary>
        /// Column position for a comment
        /// </summary>
        internal int Column
        {
            get
            {
                return GetXmlNodeInt(COLUMN_PATH);
            }
            set
            {
                SetXmlNodeString(COLUMN_PATH, value.ToString(CultureInfo.InvariantCulture));
            }
        }
        const string STYLE_PATH = "@style";
        internal string Style
        {
            get
            {
                return GetXmlNodeString(STYLE_PATH);
            }
            set
            {
                SetXmlNodeString(STYLE_PATH, value);
            }
        }
        #region IRangeID Members

        ulong IRangeID.RangeID
        {
            get
            {
                return ExcelCellBase.GetCellID(Range.Worksheet.SheetId, Range.Start.Row, Range.Start.Column);
            }
            set
            {
                
            }
        }

        #endregion
    }
}
