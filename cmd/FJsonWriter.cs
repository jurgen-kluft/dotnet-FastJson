using System.Text;

namespace FJson;

public class Writer
{
    private readonly StringBuilder _json = new StringBuilder();
    private const string _indentation = "                                                                                                                                ";
    private int _indent;

    public void Begin()
    {
        _json.Clear();
        InternalBegin("{");
    }

    public string End()
    {
        InternalEnd("}");
        return _json.ToString();
    }

    public void BeginObject(string key)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        if (string.IsNullOrEmpty(key))
        {
            _json.AppendLine("{");
        }
        else
        {
            _json.Append("\"");
            _json.Append(key);
            _json.AppendLine("\": {");
        }
        _indent++;
    }

    public void EndObject(bool last = false)
    {
        --_indent;
        _json.Append(_indentation.AsSpan()[.._indent]);
        if (!last)
        {
            _json.AppendLine("},");
        }
        else
        {
            _json.AppendLine("}");
        }
    }

    public void BeginArray(string key)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.Append("\"");
        _json.Append(key);
        _json.AppendLine("\": [");
        _indent++;
    }

    public void EndArray(bool last= false)
    {
        --_indent;
        _json.Append(_indentation.AsSpan()[.._indent]);
        if (!last)
        {
            _json.AppendLine("],");
        }
        else
        {
            _json.AppendLine("]");
        }
    }

    public void WriteField(string key, int value, bool last=false)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.Append("\"");
        _json.Append(key);
        _json.Append("\": ");
        _json.Append(value.ToString());
        if (last)
        {
            _json.AppendLine();
        }
        else
        {
            _json.AppendLine(",");
        }
    }

    private static string EscapeString(string str)
    {
        var _json = new StringBuilder();
        foreach(var c in str)
        {
            switch (c)
            {
                case '"':
                    _json.Append("\\\"");
                    break;
                case '\\':
                    _json.Append("\\\\");
                    break;
                case '\b':
                    _json.Append("\\b");
                    break;
                case '\f':
                    _json.Append("\\f");
                    break;
                case '\n':
                    _json.Append("\\n");
                    break;
                case '\r':
                    _json.Append("\\r");
                    break;
                case '\t':
                    _json.Append("\\t");
                    break;
                default:
                    _json.Append(c);
                    break;
            }
        }
        return _json.ToString();
    }

    public void WriteField(string key, string value, bool last=false)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.Append("\"");
        _json.Append(key);
        _json.Append("\": \"");

        _json.Append(EscapeString(value));

        if (last)
        {
            _json.AppendLine("\"");
        }
        else
        {
            _json.AppendLine("\",");
        }
    }

    public void WriteElement(string value, bool last=false)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.Append("\"");
        _json.Append(value.ToString());
        if (last)
        {
            _json.AppendLine("\"");
        }
        else
        {
            _json.AppendLine("\",");
        }
    }

    public void WriteField(string key, long value, bool last=false)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.Append("\"");
        _json.Append(key);
        _json.Append("\": ");
        _json.Append(value.ToString());
        if (last)
        {
            _json.AppendLine();
        }
        else
        {
            _json.AppendLine(",");
        }
    }

    public void WriteField(string key, float value, bool last=false)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.Append("\"");
        _json.Append(key);
        _json.Append("\": ");
        _json.Append(value.ToString());
        if (last)
        {
            _json.AppendLine();
        }
        else
        {
            _json.AppendLine(",");
        }
    }

    public void WriteField(string key, bool value, bool last=false)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.Append("\"");
        _json.Append(key);
        _json.Append("\": ");
        _json.Append((value) ? "true" : "false");
        if (last)
        {
            _json.AppendLine();
        }
        else
        {
            _json.AppendLine(",");
        }
    }

    private void InternalBegin(string c)
    {
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.AppendLine(c);
        _indent++;
    }
    private void InternalEnd(string c)
    {
        --_indent;
        _json.Append(_indentation.AsSpan()[.._indent]);
        _json.AppendLine(c);
    }

}
