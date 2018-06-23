﻿using System;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.Text
{
    public static class SpanExtensions
    {
        /// <summary>
        /// Returns null if Length == 0, string.Empty if value[0] == NonWidthWhitespace, otherise returns value.ToString()
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Value(this ReadOnlySpan<char> value) => value.IsEmpty 
            ? null 
            : value.Length == 1 && value[0] == TypeConstants.NonWidthWhiteSpace 
                ? ""
                : value.ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this ReadOnlySpan<char> value) => value.IsEmpty || (value.Length == 1 && value[0] == TypeConstants.NonWidthWhiteSpace);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(this ReadOnlySpan<char> value) => value.IsNullOrEmpty() || value.IsWhiteSpace();

        [Obsolete("Use value[index]")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetChar(this ReadOnlySpan<char> value, int index) => value[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Substring(this ReadOnlySpan<char> value, int pos) => value.Slice(pos, value.Length - pos).ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Substring(this ReadOnlySpan<char> value, int pos, int length) => value.Slice(pos, length).ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareIgnoreCase(this ReadOnlySpan<char> value, ReadOnlySpan<char> text) => value.Equals(text, StringComparison.OrdinalIgnoreCase);

        public static ReadOnlySpan<char> FromCsvField(this ReadOnlySpan<char> text)
        {
            //TODO replace with native Replace() when exists
            if (text.IsNullOrEmpty())
                return text;
            
            var delim = CsvConfig.ItemDelimiterString;
            if (delim.Length == 1)
            {
                if (text[0] != delim[0])
                    return text;
            }
            else if (!text.StartsWith(delim.AsSpan(), StringComparison.Ordinal))
            {
                return text;
            }
            
            var ret = text.Slice(CsvConfig.ItemDelimiterString.Length, text.Length - CsvConfig.EscapedItemDelimiterString.Length)
                .ToString().Replace(CsvConfig.EscapedItemDelimiterString, CsvConfig.ItemDelimiterString);
            
            if (ret == string.Empty)
                return TypeConstants.EmptySpan;

            return ret.AsSpan();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ParseBoolean(this ReadOnlySpan<char> value) => MemoryProvider.Instance.ParseBoolean(value);

        public static bool TryParseBoolean(this ReadOnlySpan<char> value, out bool result) =>
            MemoryProvider.Instance.TryParseBoolean(value, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseDecimal(this ReadOnlySpan<char> value, out decimal result) =>
            MemoryProvider.Instance.TryParseDecimal(value, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFloat(this ReadOnlySpan<char> value, out float result) => 
            MemoryProvider.Instance.TryParseFloat(value, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseDouble(this ReadOnlySpan<char> value, out double result) => 
            MemoryProvider.Instance.TryParseDouble(value, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal ParseDecimal(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseDecimal(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ParseFloat(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseFloat(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ParseDouble(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseDouble(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ParseSByte(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseSByte(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ParseByte(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseByte(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ParseInt16(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseInt16(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ParseUInt16(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseUInt16(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseInt32(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseInt32(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ParseUInt32(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseUInt32(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ParseInt64(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseInt64(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ParseUInt64(this ReadOnlySpan<char> value) => 
            MemoryProvider.Instance.ParseUInt64(value);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ParseGuid(this ReadOnlySpan<char> value) =>
            MemoryProvider.Instance.ParseGuid(value);
        
        public static bool TryReadLine(this ReadOnlySpan<char> text, out ReadOnlySpan<char> line, ref int startIndex)
        {
            if (startIndex >= text.Length)
            {
                line = TypeConstants.NullSpan;
                return false;
            }

            var nextLinePos = text.Slice(startIndex).IndexOfAny('\r', '\n');
            if (nextLinePos == -1)
            {
                var nextLine = text.Slice(startIndex, text.Length - startIndex);
                startIndex = text.Length;
                line = nextLine;
                return true;
            }
            else
            {
                var nextLine = text.Slice(startIndex, nextLinePos - startIndex);

                startIndex = nextLinePos + 1;

                if (text[nextLinePos] == '\r' && text.Length > nextLinePos + 1 && text[nextLinePos + 1] == '\n')
                    startIndex += 1;

                line = nextLine;
                return true;
            }
        }

        public static bool TryReadPart(this ReadOnlySpan<char> text, ReadOnlySpan<char> needle, out ReadOnlySpan<char> part, ref int startIndex)
        {
            if (startIndex >= text.Length)
            {
                part = TypeConstants.NullSpan;
                return false;
            }

            text = text.Slice(startIndex);
            var nextPartPos = text.IndexOf(needle);
            if (nextPartPos == -1)
            {
                var nextPart = text.Slice(text.Length);
                startIndex += text.Length;
                part = nextPart;
                return true;
            }
            else
            {
                var nextPart = text.Slice(nextPartPos);
                startIndex += nextPartPos + needle.Length;
                part = nextPart;
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> Advance(this ReadOnlySpan<char> text, int to) => text.Slice(to, text.Length - to);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> Subsegment(this ReadOnlySpan<char> text, int startPos) => text.Slice(startPos, text.Length - startPos);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> Subsegment(this ReadOnlySpan<char> text, int startPos, int length) => text.Slice(startPos, length);

        public static ReadOnlySpan<char> LeftPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Slice(0, pos);
        }

        public static ReadOnlySpan<char> LeftPart(this ReadOnlySpan<char> strVal, ReadOnlySpan<char> needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Subsegment(0, pos);
        }

        public static ReadOnlySpan<char> RightPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Subsegment(pos + 1);
        }

        public static ReadOnlySpan<char> RightPart(this ReadOnlySpan<char> strVal, ReadOnlySpan<char> needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Subsegment(pos + needle.Length);
        }

        public static ReadOnlySpan<char> LastLeftPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Subsegment(0, pos);
        }

        public static ReadOnlySpan<char> LastLeftPart(this ReadOnlySpan<char> strVal, ReadOnlySpan<char> needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Subsegment(0, pos);
        }

        public static ReadOnlySpan<char> LastRightPart(this ReadOnlySpan<char> strVal, char needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Slice(pos + 1);
        }

        public static ReadOnlySpan<char> LastRightPart(this ReadOnlySpan<char> strVal, ReadOnlySpan<char> needle)
        {
            if (strVal.IsEmpty) return strVal;
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? strVal
                : strVal.Subsegment(pos + needle.Length);
        }

        public static void SplitOnFirst(this ReadOnlySpan<char> strVal, char needle, out ReadOnlySpan<char> first, out ReadOnlySpan<char> last)
        {
            first = default(ReadOnlySpan<char>);
            last = default(ReadOnlySpan<char>);
            if (strVal.IsEmpty) return;
            
            var pos = strVal.IndexOf(needle);
            if (pos == -1)
            {
                first = strVal;
            }
            else
            {
                first = strVal.Slice(0, pos);
                last = strVal.Slice(pos + 1);
            }
        }

        public static void SplitOnFirst(this ReadOnlySpan<char> strVal, ReadOnlySpan<char> needle, out ReadOnlySpan<char> first, out ReadOnlySpan<char> last)
        {
            first = default(ReadOnlySpan<char>);
            last = default(ReadOnlySpan<char>);
            if (strVal.IsEmpty) return;
            
            var pos = strVal.IndexOf(needle);
            if (pos == -1)
            {
                first = strVal;
            }
            else
            {
                first = strVal.Slice(0, pos);
                last = strVal.Slice(pos + needle.Length);
            }
        }

        public static void SplitOnLast(this ReadOnlySpan<char> strVal, char needle, out ReadOnlySpan<char> first, out ReadOnlySpan<char> last)
        {
            first = default(ReadOnlySpan<char>);
            last = default(ReadOnlySpan<char>);
            if (strVal.IsEmpty) return;
            
            var pos = strVal.LastIndexOf(needle);
            if (pos == -1)
            {
                first = strVal;
            }
            else
            {
                first = strVal.Slice(0, pos);
                last = strVal.Slice(pos + 1);
            }
        }

        public static void SplitOnLast(this ReadOnlySpan<char> strVal, ReadOnlySpan<char> needle, out ReadOnlySpan<char> first, out ReadOnlySpan<char> last)
        {
            first = default(ReadOnlySpan<char>);
            last = default(ReadOnlySpan<char>);
            if (strVal.IsEmpty) return;
            
            var pos = strVal.LastIndexOf(needle);
            if (pos == -1)
            {
                first = strVal;
            }
            else
            {
                first = strVal.Slice(0, pos);
                last = strVal.Slice(pos + needle.Length);
            }
        }

        public static ReadOnlySpan<char> WithoutExtension(this ReadOnlySpan<char> filePath)
        {
            if (filePath.IsNullOrEmpty())
                return TypeConstants.NullSpan;

            var extPos = filePath.LastIndexOf('.');
            if (extPos == -1) return filePath;

            var dirPos = filePath.LastIndexOfAny(PclExport.DirSeps);
            return extPos > dirPos ? filePath.Slice(0, extPos) : filePath;
        }

        public static ReadOnlySpan<char> GetExtension(this ReadOnlySpan<char> filePath)
        {
            if (filePath.IsNullOrEmpty())
                return TypeConstants.NullSpan;

            var extPos = filePath.LastIndexOf('.');
            return extPos == -1 ? TypeConstants.NullSpan : filePath.Slice(extPos);
        }

        public static ReadOnlySpan<char> ParentDirectory(this ReadOnlySpan<char> filePath)
        {
            if (filePath.IsNullOrEmpty())
                return TypeConstants.NullSpan;

            var dirSep = filePath.IndexOf(PclExport.Instance.DirSep) != -1
                ? PclExport.Instance.DirSep
                : filePath.IndexOf(PclExport.Instance.AltDirSep) != -1
                    ? PclExport.Instance.AltDirSep
                    : (char)0;

            if (dirSep == 0)
                return TypeConstants.NullSpan;
            
            MemoryExtensions.TrimEnd(filePath, dirSep).SplitOnLast(dirSep, out var first, out _); 
            return first;
        }

        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> value, params char[] trimChars)
        {
            if (trimChars == null || trimChars.Length == 0)
                return value.TrimHelper(1);
            return value.TrimHelper(trimChars, 1);
        }

        private static ReadOnlySpan<char> TrimHelper(this ReadOnlySpan<char> value, int trimType)
        {
            int end = value.Length - 1;
            int start = 0;
            if (trimType != 1)
            {
                start = 0;
                while (start < value.Length && char.IsWhiteSpace(value[start]))
                    ++start;
            }
            if (trimType != 0)
            {
                end = value.Length - 1;
                while (end >= start && char.IsWhiteSpace(value[end]))
                    --end;
            }
            return value.CreateTrimmedString(start, end);
        }

        private static ReadOnlySpan<char> TrimHelper(this ReadOnlySpan<char> value, char[] trimChars, int trimType)
        {
            int end = value.Length - 1;
            int start = 0;
            if (trimType != 1)
            {
                for (start = 0; start < value.Length; ++start)
                {
                    char ch = value[start];
                    int index = 0;
                    while (index < trimChars.Length && (int)trimChars[index] != (int)ch)
                        ++index;
                    if (index == trimChars.Length)
                        break;
                }
            }
            if (trimType != 0)
            {
                for (end = value.Length - 1; end >= start; --end)
                {
                    char ch = value[end];
                    int index = 0;
                    while (index < trimChars.Length && (int)trimChars[index] != (int)ch)
                        ++index;
                    if (index == trimChars.Length)
                        break;
                }
            }
            return value.CreateTrimmedString(start, end);
        }

        private static ReadOnlySpan<char> CreateTrimmedString(this ReadOnlySpan<char> value, int start, int end)
        {
            int length = end - start + 1;
            if (length == value.Length)
                return value;
            if (length == 0)
                return TypeConstants.NullSpan;
            return value.Slice(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> SafeSubsegment(this ReadOnlySpan<char> value, int startIndex) => SafeSubsegment(value, startIndex, value.Length);

        public static ReadOnlySpan<char> SafeSubsegment(this ReadOnlySpan<char> value, int startIndex, int length)
        {
            if (value.IsEmpty) return TypeConstants.NullSpan;
            if (startIndex < 0) startIndex = 0;
            if (value.Length >= startIndex + length)
                return value.Slice(startIndex, length);

            return value.Length > startIndex ? value.Slice(startIndex) : TypeConstants.NullSpan;
        }

        public static string SubstringWithEllipsis(this ReadOnlySpan<char> value, int startIndex, int length)
        {
            var str = value.Slice(startIndex, length);
            return str.Length == length
                ? str.ToString() + "..."
                : str.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsIgnoreCase(this ReadOnlySpan<char> value, ReadOnlySpan<char> other) => value.Equals(other, StringComparison.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithIgnoreCase(this ReadOnlySpan<char> value, ReadOnlySpan<char> other) => value.StartsWith(other, StringComparison.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithIgnoreCase(this ReadOnlySpan<char> value, ReadOnlySpan<char> other) => value.EndsWith(other, StringComparison.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WriteAsync(this Stream stream, ReadOnlySpan<char> value, CancellationToken token = default(CancellationToken)) =>
            MemoryProvider.Instance.WriteAsync(stream, value, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<char> SafeSubstring(this ReadOnlySpan<char> value, int startIndex) => SafeSubstring(value, startIndex, value.Length);

        public static ReadOnlySpan<char> SafeSubstring(this ReadOnlySpan<char> value, int startIndex, int length)
        {
            if (value.IsEmpty) return TypeConstants.NullSpan;
            if (startIndex < 0) startIndex = 0;
            if (value.Length >= (startIndex + length))
                return value.Slice(startIndex, length);

            return value.Length > startIndex ? value.Slice(startIndex) : TypeConstants.NullSpan;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append(this StringBuilder sb, ReadOnlySpan<char> value) =>
            MemoryProvider.Instance.Append(sb, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ParseBase64(this ReadOnlySpan<char> value) => MemoryProvider.Instance.ParseBase64(value);
    }
}
