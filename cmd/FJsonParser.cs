namespace FJson;

public class TestObject
{
    public int Integer { get; set; }
    public float Float { get; set; }
    public string Text { get; set; }

    public List<TestObject> Objects { get; set; } = new List<TestObject>();

    public static TestObject ParseJson(string json)
    {
        var o = new TestObject();
        var action = delegate (Decoder.TSpan key, Decoder.TSpan value)
        {


        };
        Decoder.Parse(json, action);
        return o;
    }
}

public static class Decoder
{
    public enum JsonType
    {
        Object,
        Array,
        String,
        Number,
        Bool,
        Null,
        Error,
    }

    public struct TSpan
    {
        public int Begin { get; init; }
        public int End { get; set; }
        public JsonType Type { get; set; }

        public static readonly TSpan Empty = new TSpan(0, 0, JsonType.Null);

        public TSpan(int begin, int end, JsonType type)
        {
            Begin = begin;
            End = end;
            Type = type;
        }
    }

    public static void Parse(string json, Action<TSpan, TSpan> cb)
    {
        var data = new Context(json.ToCharArray().AsMemory());
        ParseValue(ref data, cb);
    }

    public static void Parse(Memory<char> json, Action<TSpan, TSpan> cb)
    {
        var data = new Context(json);
        ParseValue(ref data, cb);
    }

    #region Parse Json

    private static TSpan ParseValue(ref Context context, Action<TSpan, TSpan> cb)
    {
        SkipWhiteSpace(ref context);

        var json = context.json.Span;
        switch (json[context.index])
        {
            case '{':
                return ParseObject(ref context, cb);
            case '[':
                return ParseArray (ref context, cb);
            case '"':
                return ParseString(ref context);
            case  >= '0' and  <= '9':
            case '-':
                return ParseNumber(ref context);
            case 'f':
            case 't':
                return ParseBoolean(ref context);
            case 'n':
                return ParseNull(ref context);
        }

        return new TSpan(context.index, context.index + 1, JsonType.Error);
    }

    private static TSpan ParseBoolean(ref Context context)
    {
        var span = new TSpan() { Begin = context.index, End = context.index, Type = JsonType.Bool };

        var json = context.json.Span;
        var asTrue = "true".AsSpan();
        if (asTrue.CompareTo(json.Slice(context.index, 4), StringComparison.OrdinalIgnoreCase) == 0)
        {
            span.End = span.Begin + 4;
            context.index += 4;
            return span;
        }

        var asFalse = "false".AsSpan();
        if (asFalse.CompareTo(json.Slice(context.index, 5), StringComparison.OrdinalIgnoreCase) == 0)
        {
            span.End = span.Begin + 5;
            context.index += 5;
            return span;
        }

        span.Type = JsonType.Error;
        return span;
    }

    private static TSpan ParseNull(ref Context context)
    {
        var span = new TSpan() { Begin = context.index, End = context.index, Type = JsonType.Null };

        var json = context.json.Span;
        var asNull = "null".AsSpan();
        if (asNull.CompareTo(json.Slice(context.index, 4), StringComparison.OrdinalIgnoreCase) == 0)
        {
            span.End = span.Begin + 4;
            context.index += 4;
            return span;
        }

        span.Type = JsonType.Error;
        return span;
    }

    private static TSpan ParseObject(ref Context context, Action<TSpan, TSpan> cb)
    {
        ++context.index; // skip '{'

        var span = new TSpan(context.index, context.index, JsonType.Object);
        var json = context.json.Span;

        do
        {
            SkipWhiteSpace(ref context);

            if (json[context.index] == '}')
            {
                break;
            }

            if (json[context.index] != '"')
            {
                // should be "
                return new TSpan(context.index, context.index + 1, JsonType.Error);
            }

            var key = GetString(ref context); // get object key string

            if (!SkipWhiteSpaceUntil(ref context, ':'))
            {
                return new TSpan(context.index, context.index + 1, JsonType.Error);
            }

            // skip ':'
            ++context.index;

            var value = ParseValue(ref context, cb);

            // Json Key + Value
            cb(key, value);

            SkipWhiteSpace(ref context);

            if (json[context.index] == ',')
            {
                ++context.index;
            }
            else if (json[context.index] == '}')
            {
                break;
            }
            else
            {
                return new TSpan(context.index, context.index + 1, JsonType.Error);
            }
        }
        while (true);

        span.End = context.index++; // skip '}'
        return span;
    }


    /// <summary>
    /// Parse JsonArray.
    /// </summary>
    private static TSpan ParseArray(ref Context context, Action<TSpan, TSpan> cb)
    {
        ++context.index; // skip '['

        var span = new TSpan(context.index, context.index, JsonType.Array);
        var json = context.json.Span;

        do
        {
            SkipWhiteSpace(ref context);
            if (json[context.index] == ']')
            {
                break;
            }

            var element = ParseValue(ref context, cb);

            // Json Array Element
            cb(TSpan.Empty, element);

            SkipWhiteSpace(ref context);
            if (json[context.index] == ',')
            {
                ++context.index;
            }
            else if (json[context.index] == ']')
            {
                break;
            }
            else
            {
                return new TSpan(context.index, context.index + 1, JsonType.Error);
            }
        }
        while (true);

        span.End = context.index++; // skip ']'
        return span;
    }

    private static TSpan ParseString(ref Context context)
    {
        if (context.isEscapeString)
        {
            return GetEscapedString(ref context);
        }
        return GetString(ref context);
    }

    private static TSpan ParseNumber(ref Context context)
    {
        var span = new TSpan(context.index, context.index, JsonType.Number);
        var json = context.json.Span;
        while (true)
        {
            switch (json[++context.index])
            {
                case >= '0' and <= '9':
                case '-':
                case '+':
                case '.':
                case 'e':
                case 'E':
                    continue;
            }

            break;
        }

        span.End = context.index;
        return span;
    }

    private static void SkipWhiteSpace(ref Context context)
    {
        var json = context.json.Span;
        while (true)
        {
            switch (json[context.index])
            {
                case ' ' :
                case '\t':
                case '\n':
                case '\r':
                    ++context.index;
                    continue;
            }

            // index point to non-whitespace
            break;
        }
    }

    private static bool SkipWhiteSpaceUntil(ref Context context, char until)
    {
        var json = context.json.Span;
        while (true)
        {
            switch (json[context.index])
            {
                case ' ' :
                case '\t':
                case '\n':
                case '\r':
                    ++context.index;
                    continue;
            }

            return json[context.index] == until;
        }
    }

    private static TSpan GetString(ref Context context)
    {
        // skip '"'
        var start = ++context.index;
        var span = new TSpan(start, start, JsonType.String);

        var json = context.json.Span;
        while (true)
        {
            switch (json[context.index++])
            {
                // check end '"'
                case '"':
                    break;

                case '\\':
                    // skip escaped quotes
                    // the escape char may be '\"'，which will break while
                    ++context.index;
                    continue;

                default:
                    continue;
            }

            break;
        }

        // index after the string end '"' so -1
        //return data.json.Substring(start, data.index - start - 1);
        span.End = context.index - 1;
        return span;
    }

    private static TSpan GetEscapedString(ref Context context)
    {
        // skip '"'
        var start = ++context.index;
        var span = new TSpan(start, start, JsonType.String);

        var json = context.json.Span;
        var write = context.index;
        while (true)
        {
            switch (json[context.index])
            {
                // check string end '"'
                case '"':
                    context.index++;
                    break;

                // check escaped char
                case '\\':
                    {
                        char c;
                        context.index++;
                        switch (json[context.index++])
                        {
                            case '"':
                                c = '"';
                                break;

                            case '\\':
                                c = '\\';
                                break;

                            case '/':
                                c = '/';
                                break;

                            case '\'':
                                c = '\'';
                                break;

                            case 'b':
                                c = '\b';
                                break;

                            case 'f':
                                c = '\f';
                                break;

                            case 'n':
                                c = '\n';
                                break;

                            case 'r':
                                c = '\r';
                                break;

                            case 't':
                                c = '\t';
                                break;

                            case 'u':
                                c = GetUnicodeCodePoint(ref context);
                                break;

                            default:
                                // unsupported, just keep
                                continue;
                        }

                        json[write++] = c;
                        continue;
                    }

                default:
                    if (write < context.index)
                    {
                        json[write++] = json[context.index++];
                    }
                    continue;
            }

            break;
        }

        span.End = write;
        return span;
    }

    private static char GetUnicodeCodePoint(ref Context context)
    {
        var json = context.json.Span;
        uint unicode = 0;
        for (var i = 0; i < 4; ++i)
        {
            var c = json[context.index++];
            var cp = c switch
            {
                >= '0' and <= '9' => (byte)(c - '0'),
                >= 'A' and <= 'F' => (byte)(10 + (c - 'A')),
                >= 'a' and <= 'f' => (byte)(10 + (c - 'a')),
                _ => (0)
            };
            unicode = (uint)((unicode << 4) | (c & 0xF));
        }

        return (char)(unicode&0xFFFF);
    }

    private struct Context
    {
        public readonly Memory<char> json;
        public int index;
        public bool isEscapeString;

        public Context(Memory<char> json)
        {
            this.json = json;
            this.index = 0;
            this.isEscapeString = false;
        }
    }

    #endregion
}
