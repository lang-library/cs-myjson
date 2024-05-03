#if false
using Antlr4.Runtime;
using MyJson.Parser.Json5;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyJson;

public class MyParser
{
    public static object Parse(string json, bool NumberAsDecimal)
    {
        if (String.IsNullOrEmpty(json)) return null;
        var inputStream = new AntlrInputStream(json);
        var lexer = new JSON5Lexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new JSON5Parser(commonTokenStream);
        var context = parser.json5();
        var result = JSON5ToObject(context, NumberAsDecimal);
        return result;
    }
    public static string ParseJsonString(string aJSON)
    {
        int i = 0;
        StringBuilder Token = new StringBuilder();
        bool QuoteMode = false;
        while (i < aJSON.Length)
        {
            switch (aJSON[i])
            {

                case '"':
                    QuoteMode ^= true;
                    break;

                case '\r':
                case '\n':
                    break;

                case ' ':
                case '\t':
                    if (QuoteMode)
                        Token.Append(aJSON[i]);
                    break;

                case '\\':
                    ++i;
                    if (QuoteMode)
                    {
                        char C = aJSON[i];
                        switch (C)
                        {
                            case 't':
                                Token.Append('\t');
                                break;
                            case 'r':
                                Token.Append('\r');
                                break;
                            case 'n':
                                Token.Append('\n');
                                break;
                            case 'b':
                                Token.Append('\b');
                                break;
                            case 'f':
                                Token.Append('\f');
                                break;
                            case 'u':
                                {
                                    string s = aJSON.Substring(i + 1, 4);
                                    Token.Append((char)int.Parse(
                                        s,
                                        System.Globalization.NumberStyles.AllowHexSpecifier));
                                    i += 4;
                                    break;
                                }
                            default:
                                Token.Append(C);
                                break;
                        }
                    }
                    break;

                case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
                    break;

                default:
                    Token.Append(aJSON[i]);
                    break;
            }
            ++i;
        }
        if (QuoteMode)
        {
            throw new Exception("My Parse: Quotation marks seems to be messed up.");
        }
        return Token.ToString();
    }
    private static object JSON5ToObject(ParserRuleContext x, bool NumberAsDecimal)
    {
        if (x is JSON5Parser.Json5Context)
        {
            for (int i = 0; i < x.children.Count; i++)
            {
                if (x.children[i] is Antlr4.Runtime.Tree.ErrorNodeImpl)
                {
                    return null;
                }
            }

            return JSON5ToObject((ParserRuleContext)x.children[0], NumberAsDecimal);
        }
        else if (x is JSON5Parser.ValueContext)
        {
            if (x.children[0] is Antlr4.Runtime.Tree.TerminalNodeImpl)
            {
                string t = JSON5Terminal(x.children[0])!;
                if (t.StartsWith("\""))
                {
                    return ParseJsonString(t);
                }

                if (t.StartsWith("'"))
                {
                    t = t.Substring(1, t.Length - 2).Replace("\\'", ",").Replace("\"", "\\\"");
                    t = "\"" + t + "\"";
                    return ParseJsonString(t);
                }

                switch (t)
                {
                    case "true":
                        return true;
                    case "false":
                        return false;
                    case "null":
                        return null;
                }

                throw new Exception($"Unexpected JSON5Parser+ValueContext: {t}");
            }

            return JSON5ToObject((ParserRuleContext)x.children[0], NumberAsDecimal);
        }
        else if (x is JSON5Parser.ArrContext)
        {
            var result = new List<object>();
            for (int i = 0; i < x.children.Count; i++)
            {
                if (x.children[i] is JSON5Parser.ValueContext value)
                {
                    result.Add(JSON5ToObject(value, NumberAsDecimal));
                }
            }

            return result;
        }
        else if (x is JSON5Parser.ObjContext)
        {
            var result = new Dictionary<string, object>();
            for (int i = 0; i < x.children.Count; i++)
            {
                if (x.children[i] is JSON5Parser.PairContext pair)
                {
                    var pairObj = (Dictionary<string, object>)JSON5ToObject(pair, NumberAsDecimal);
                    string key = (string)pairObj!["key"];
                    result[key] = pairObj["value"];
                }
            }

            return result;
        }
        else if (x is JSON5Parser.PairContext)
        {
            var result = new Dictionary<string, object>();
            for (int i = 0; i < x.children.Count; i++)
            {
                if (x.children[i] is JSON5Parser.KeyContext key)
                {
                    result["key"] = JSON5ToObject(key, NumberAsDecimal);
                }

                if (x.children[i] is JSON5Parser.ValueContext value)
                {
                    result["value"] = JSON5ToObject(value, NumberAsDecimal);
                }
            }

            return result;
        }
        else if (x is JSON5Parser.KeyContext)
        {
            if (x.children[0] is Antlr4.Runtime.Tree.TerminalNodeImpl)
            {
                string t = JSON5Terminal(x.children[0])!;
                if (t.StartsWith("\""))
                {
                    return ParseJsonString(t);
                }

                if (t.StartsWith("'"))
                {
                    t = t.Substring(1, t.Length - 2).Replace("\\'", ",").Replace("\"", "\\\"");
                    t = "\"" + t + "\"";
                    return ParseJsonString(t);
                }

                return t;
            }
            else
            {
                return "?";
            }
        }
        else if (x is JSON5Parser.NumberContext)
        {
            string n = JSON5Terminal(x.children[0]);
            if (n == "-" || n == "+")
            {
                string sign = n;
                n = sign + JSON5Terminal(x.children[1]);
            }
            decimal result;
            if (!decimal.TryParse(n, out result))
                result = 0;
            if (NumberAsDecimal)
                return new MyNumber(result);
            return new MyNumber(Convert.ToDouble(result));
        }
        else
        {
            throw new Exception($"Unexpected: {x.GetType().FullName}");
        }
    }

    private static string? JSON5Terminal(Antlr4.Runtime.Tree.IParseTree x)
    {
        if (x is Antlr4.Runtime.Tree.TerminalNodeImpl t)
        {
            return t.ToString();
        }

        return null;
    }
}
#endif
