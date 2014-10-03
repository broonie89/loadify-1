// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText(args[0]);
            var tokenStream = CHeaderLexer.Lex(text);
            var parser = new HeaderParser(tokenStream);
            var serializer = JsonSerializer.Create(new JsonSerializerSettings{Formatting=Formatting.Indented});
            serializer.Serialize(Console.Out, parser.ParseHeader());
        }
    }
}
