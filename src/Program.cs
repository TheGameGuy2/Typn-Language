using IR;
using Lexing;
using Parsing;
using System.IO;

Console.WriteLine("Hello, World!");

Tokenizer t = new(File.ReadAllText("main.tph"));


var tokens = t.MakeTokens();


foreach(Token tok in tokens)
{
    Console.WriteLine(tok);
    
}


Console.WriteLine("--- AST ---");

Parser p = new(tokens);
//p.ParseExpression().Show(0);
List<ASTNode> nodes = p.ParseModule();

foreach(ASTNode node in nodes)
{
    node.Show(0);
}

IRBuilder builder = new();

foreach(ASTNode node in nodes)
{
    node.MakeInstruction(builder);
}

Console.WriteLine("--- Stage 1 IR ---");

builder.ShowInstructions();

//Console.WriteLine(p.ParseExpression().MakeInstruction());
