﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rewrite.Structure2;
using Rewrite.ConditionParser;
using Rewrite.RuleParser;
namespace Rewrite.FileParser
{
    public static class RewriteConfigurationFileParser
    {
        private static StreamReader _reader;
        public static List<Rule> Parse(Stream input)
        {
            _reader = new StreamReader(input);
            var line = (string) null;
            List<Rule> rules = new List<Rule>();
            var currentRule = new Rule { Conditions = new List<Condition>() };
            while ((line = _reader.ReadLine()) != null) {
                // TODO handle comments
                List<string> tokens = RewriteTokenizer.TokenizeRule(line);
                if (tokens.Count < 3)
                {
                    // This means the line didn't have an appropriate format, throw format exception
                    throw new FormatException();
                }
                switch (tokens[0])
                {
                    case "RewriteCond":
                        {
                            List<ConditionTestStringSegment> matchesForCondition = ConditionTestStringParser.ParseConditionTestString(tokens[1]);
                            GeneralExpression ie = ConditionRegexParser.ParseCondition(tokens[2]);
                            List<string> flags = null;
                            if (tokens.Count == 4)
                            {
                                flags = ConditionFlagParser.TokenizeAndParseFlags(tokens[3]);
                            }
                            currentRule.Conditions.Add(new Condition { TestStringSegments = matchesForCondition, ConditionRegex = ie, Flags = flags });
                            break;
                        }
                    case "RewriteRule":
                        {
                            // parse regex
                            // then do similar logic to the condition test string replacement
                            GeneralExpression ie = RuleRegexParser.ParseRuleRegex(tokens[1]);
                            List<ConditionTestStringSegment> matchesForRule = ConditionTestStringParser.ParseConditionTestString(tokens[2]);
                            if (tokens.Count == 4)
                            {
                                currentRule.Flags = ConditionFlagParser.TokenizeAndParseFlags(tokens[3]);
                            }
                            currentRule.InitialRule = ie;
                            currentRule.OnMatch = matchesForRule;
                            rules.Add(currentRule);
                            currentRule = new Rule { Conditions = new List<Condition>() };
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
            return rules;
        }
    }
}