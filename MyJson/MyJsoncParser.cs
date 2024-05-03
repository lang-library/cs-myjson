#if false
using MyJson.Parser.Jsonc;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyJson;

public class MyJsoncParser
{
    public static object Parse(string json, bool NumberAsDecimal)
    {
        if (String.IsNullOrEmpty(json)) return null;
        ParserContext context = new ParserContext(json, false);
        Rule_json_text rule = Rule_json_text.Parse(context);
        if (rule == null) throw new Exception($"{json} is illegal");
        return RuleToObject(rule, NumberAsDecimal);
    }
    public static string FullName(object x)
    {
        if (x is null) return "null";
        string fullName = ((object)x).GetType().FullName;
        return fullName.Split('`')[0];
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

    protected static List<Rule> SkipUseless(List<Rule> rules)
    {
        var result = new List<Rule>();
        foreach (var rule in rules)
        {
            if (rule is Rule_ws) continue;
            if (rule is Rule_begin_object) continue;
            if (rule is Rule_end_object) continue;
            if (rule is Rule_name_separator) continue;
            if (rule is Rule_value_separator) continue;
            if (rule is Rule_begin_array) continue;
            if (rule is Rule_end_array) continue;
            result.Add(rule);
        }
        return result;
    }
    public static object RuleToObject(Rule rule, bool NumberAsDecimal)
    {
        var rules = SkipUseless(rule.rules);
        if (rule is Rule_json_text)
        {
            foreach (var r in rules)
            {
                //Assert.Single(rules);
                return RuleToObject(rules[0], NumberAsDecimal);
            }
        }
        else if (rule is Rule_value)
        {
            //Assert.Single(rules);
            foreach (var r in rules)
            {
                return RuleToObject(rules[0], NumberAsDecimal);
            }
        }
        else if (rule is Rule_array)
        {
            var result = new List<object>();
            foreach (var r in rules)
            {
                //Assert.True(r is Rule_value);
                result.Add(RuleToObject(r, NumberAsDecimal));
            }
            return result;
        }
        else if (rule is Rule_object)
        {
            var result = new Dictionary<string, object>();
            foreach (var r in rules)
            {
                //Assert.True(r is Rule_member);
                var pair = (KeyValuePair<string, object>)RuleToObject(r, NumberAsDecimal);
                result[pair.Key] = pair.Value;
            }
            return result;
        }
        else if (rule is Rule_member)
        {
            string name = null;
            foreach (var r in rules)
            {
                if (r is Rule_string) name = (string)RuleToObject(r, NumberAsDecimal);
                if (r is Rule_member_name) name = (string)RuleToObject(r, NumberAsDecimal);
                if (r is Rule_value) return new KeyValuePair<string, object>(name, RuleToObject(r, NumberAsDecimal));
            }
        }
        else if (rule is Rule_member_name)
        {
            foreach (var r in rules)
            {
                if (r is Rule_string) return (string)RuleToObject(r, NumberAsDecimal);
                if (r is Rule_symbol) return (string)RuleToObject(r, NumberAsDecimal);
            }
        }
        else if (rule is Rule_string)
        {
            return ParseJsonString(rule.spelling);
        }
        else if (rule is Rule_symbol)
        {
            return rule.spelling;
        }
        else if (rule is Rule_number)
        {
            if (NumberAsDecimal)
                return decimal.Parse(rule.spelling);
            return double.Parse(rule.spelling);
        }
        else if (rule is Rule_true)
        {
            return true;
        }
        else if (rule is Rule_false)
        {
            return false;
        }
        else if (rule is Rule_null)
        {
            return null;
        }
        else
        {
            throw new Exception($"{FullName(rule)} is not supported");
        }
        throw new Exception($"{FullName(rule)} did not return result");
    }
}
#endif
