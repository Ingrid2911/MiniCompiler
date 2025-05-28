// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using System.Reflection.Emit;
using Antlr4.Runtime;
using BasicCompiler;
partial class Program
{
    static void Main(string[] args)
    {
        string caleFisier = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Program.txt");
        string outputTokensFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output_tokens.txt");
        string globalVariablesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "globalVariablesFile.txt");
        string functionDetails= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "functionDetails.txt");

        PrelucrareProgram(caleFisier, outputTokensFile, globalVariablesFile, functionDetails);
    }

    static void PrelucrareProgram(string caleFisier, string outputTokensFile, string globalVariablesFile, string functionDetails)
    {
        try
        {
            if (!File.Exists(caleFisier))
            {
                Console.WriteLine("Eroare: fișierul specificat nu există!");
                return;
            }

            string codCpp = File.ReadAllText(caleFisier); //funcția ReadAllText pentru citirea din fișier

            var units = GetLexicalUnits(codCpp); // Obținerea unităților lexicale
            SaveLexicalUnitsToFile(units, outputTokensFile); // Salvarea unităților lexicale într-un fișier

            var globalVars = GetGlobalVariables(codCpp); // Obținerea variabilelor globale
            SaveGlobalVariablesToFile(globalVars, globalVariablesFile); // Salvarea variabilelor globale într-un fișier

            var functions = GetFunctions(codCpp);
            SaveFunctionsToFile(functions, functionDetails);

            CompilareProgram(codCpp);

        }
        catch (Exception ex)
        {
            Console.WriteLine("Eroare la citirea fișierului: " + ex.Message);
        }
    }
    static void CompilareProgram(string code)
    {
        try
        {
            // input stream - creeaza un flux de intrare pentru a analiza caracterele de intrare
            AntlrInputStream inputStream = new AntlrInputStream(code);

            // lexer - creeaza un analizator lexical pentru codul sursa, imparte codul in tokeni
            miniLangLexer lexer = new miniLangLexer(inputStream);

            // token stream - secvență liniară de token-uri, generată de lexer, care este transmisă către parser (analizatorul sintactic)
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);

            // parser - procesează fluxul de token-uri și construiește un arbore de sintaxă
            miniLangParser parser = new miniLangParser(tokenStream);

            var parseTree = parser.program();

            Console.WriteLine("Programul a fost analizat cu succes!");

            // de completat cu operațiunile de verificare
        }
        catch (Exception ex)
        {
            Console.WriteLine("Eroare la compilarea programului: " + ex.Message);
        }
    }
// Definirea clasei LexicalUnit pentru a păstra token-ul, lexema și linia
public class LexicalUnit
{
    public string Token { get; set; }
    public string Lexema { get; set; }
    public int Line { get; set; }

    public LexicalUnit(string token, string lexema, int line)
    {
        Token = token;
        Lexema = lexema;
        Line = line;
    }
}

// Funcția pentru obținerea unităților lexicale
static List<LexicalUnit> GetLexicalUnits(string code)
{
    var units = new List<LexicalUnit>();

    try
    {
        // Crearea fluxului de intrare pentru lexer
        AntlrInputStream inputStream = new AntlrInputStream(code);
        miniLangLexer lexer = new miniLangLexer(inputStream);
        CommonTokenStream tokenStream = new CommonTokenStream(lexer);

        // Inițializarea lexer-ului
        lexer.RemoveErrorListeners(); // Eliminăm eventualii erori de lexer
        tokenStream.Fill(); // Generăm toate token-urile

        foreach (var token in tokenStream.GetTokens())
        {
            if (token.Type == -1)
              continue; 

            string tokenName = lexer.Vocabulary.GetSymbolicName(token.Type);
            string lexema = token.Text; // Textul token-ului
            int line = token.Line; // Linia token-ului

            units.Add(new LexicalUnit(tokenName, lexema, line));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Eroare la obtinerea unităților lexicale: {ex.Message}");
    }

    return units;
}

// Funcția pentru salvarea unităților lexicale într-un fișier text
static void SaveLexicalUnitsToFile(List<LexicalUnit> units, string outputFile)
{
        FileInfo fileInfo = new FileInfo(outputFile);
        fileInfo.IsReadOnly = false;
        try
    {
        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            foreach (var unit in units)
            {
                writer.WriteLine($"< {unit.Token}, {unit.Lexema}, {unit.Line} >");
            }
        }
        Console.WriteLine("Unitatile lexicale au fost salvate cu succes in fisier.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Eroare la salvarea în fișier: {ex.Message}");
    }
    }


    // Clasa pentru variabilele globale
    public class GlobalVariable
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public GlobalVariable(string type, string name, string value)
        {
            Type = type;
            Name = name;
            Value = value;
        }
    }

    // Funcția pentru obținerea variabilelor globale
    static List<GlobalVariable> GetGlobalVariables(string code)
    {
        var globalVars = new List<GlobalVariable>();
        int blockDepth = 0; // Contor pentru nivelul blocurilor
        try
        {
            // Crearea fluxului de intrare pentru lexer
            AntlrInputStream inputStream = new AntlrInputStream(code);
            miniLangLexer lexer = new miniLangLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);

            // Inițializarea lexer-ului
            lexer.RemoveErrorListeners();
            tokenStream.Fill(); 

            
            
            for (int i = 0; i < tokenStream.Size; i++)
            {
                var token = tokenStream.Get(i);

                // Detectăm începutul unei funcții pentru a ignora variabilele din interiorul funcțiilor
                if (token.Text == "{" || token.Text=="(")
                {
                    blockDepth++; // Suntem în interiorul unei funcții
                }

                if (token.Text == "}" || token.Text==")" )
                {
                    blockDepth--; // Ieșim dintr-o funcție
                }

                // Verificăm dacă este o declarație de variabilă globală
                if (blockDepth==0 && (token.Text == "int" || token.Text == "double" || token.Text == "string"))
                {
                    string type = token.Text; // Tipul variabilei

                    // Căutăm următoarele token-uri pentru identificatori și valori
                    var nextToken = tokenStream.Get(i + 1);
                    if (nextToken.Type == miniLangLexer.IDENTIFIER)
                    {
                        string name = nextToken.Text; // Numele variabilei

                        // Verificăm dacă există o valoare atribuită
                        if(tokenStream.Get(i + 2).Text != "=")
                        {
                            continue;
                        }
                        var assignmentToken = tokenStream.Get(i + 2);
                        string value=null;
                        if (assignmentToken != null && assignmentToken.Text == "=")
                        {
                            var valueToken = tokenStream.Get(i + 3);
                            value = valueToken.Text; // Valoarea variabilei
                        }
                        globalVars.Add(new GlobalVariable(type, name, value));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare la obținerea variabilelor globale: {ex.Message}");
        }

        return globalVars;
    }

    // Funcția pentru salvarea variabilelor globale într-un fișier text
    static void SaveGlobalVariablesToFile(List<GlobalVariable> globalVars, string outputFile)
    {
        FileInfo fileInfo = new FileInfo(outputFile);
        fileInfo.IsReadOnly = false;
        try
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                
                foreach (var globalVar in globalVars)
                {
                    writer.WriteLine($"< {globalVar.Type}, {globalVar.Name}, {globalVar.Value} >");
                }
            }
            Console.WriteLine("Variabilele globale au fost salvate cu succes în fișier.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare la salvarea în fișier: {ex.Message}");
        }
    }
    //clasa pentru informatii despre functii
    public class FunctionInfo
    {
        public string ReturnType { get; set; }
        public string Name { get; set; }
        public List<(string Type, string Name)> Parameters { get; set; } = null;
        public List<(string Type, string Name, string Value)> LocalVariables { get; set; } = null;
        public List<(string Structure, int Line)> ControlStructures { get; set; } = null;
        public bool IsRecursive { get; set; }
        public bool IsMain { get; set; }

        public FunctionInfo(string returnType, string name)
        {
            ReturnType = returnType;
            Name = name;
            Parameters = new List<(string, string)>();
            ControlStructures = new List<(string Structure, int Line)>();
            LocalVariables = new List<(string Type, string Name, string Value)>();
        }
    }
    static List<FunctionInfo> GetFunctions(string code)
    {
        var functions = new List<FunctionInfo>();
        try
        {
            AntlrInputStream inputStream = new AntlrInputStream(code);
            miniLangLexer lexer = new miniLangLexer(inputStream);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            miniLangParser parser = new miniLangParser(tokenStream);
            var parseTree = parser.program();

            // Traversează arborele de sintaxă pentru a găsi funcțiile
            foreach (var child in parseTree.children)
            {
                if (child is miniLangParser.FunctionDeclarationContext funcCtx)
                {
                    var returnType = funcCtx.type().GetText();
                    var functionName = funcCtx.IDENTIFIER().GetText();
                    var functionInfo = new FunctionInfo(returnType, functionName);

                    // Detectează dacă este funcția `main`
                    functionInfo.IsMain = functionName == "main";

                    // Colectează parametrii
                    var paramList = funcCtx.parameterList();
                    if (paramList != null)
                    {
                        foreach (var paramCtx in paramList.parameter())
                        {
                            var paramTypeChild = paramCtx.GetChild(0); // Verifică tipul parametru
                            var paramNameNode = paramCtx.IDENTIFIER(); // Verifică numele parametru

                            if (paramTypeChild != null && paramNameNode != null)
                            {
                                var paramType = paramTypeChild.GetText();
                                var paramName = paramNameNode.GetText();
                                functionInfo.Parameters.Add((paramType, paramName));  // Adăugare parametru
                            }
                            else
                            {
                                // Logare sau tratare pentru parametri invalizi
                                Console.WriteLine("Parametru invalid: tip sau nume lipsa.");
                            }
                        }
                    }

                    // Colectează variabilele locale
                    foreach (var localVarCtx in funcCtx.block().statement())
                    {
                        if (localVarCtx.variableDeclaration() != null)
                        {
                            var lVarCtx = localVarCtx.variableDeclaration();
                            var lVarType = lVarCtx.type().GetText();
                            var lVarName = lVarCtx.IDENTIFIER().GetText();
                            string localVarValue = lVarCtx.expression()?.GetText() ?? "null";
                            functionInfo.LocalVariables.Add((lVarType, lVarName, localVarValue));
                        }
                    }

                    // Colectează structurile de control
                    foreach (var statementCtx in funcCtx.block().statement())
                    {
                        if (statementCtx.selectionStatement() != null)
                            functionInfo.ControlStructures.Add(("if", statementCtx.Start.Line));
                        if (statementCtx.iterationStatement() != null)
                            functionInfo.ControlStructures.Add(("while", statementCtx.Start.Line));
                        if (statementCtx.forStatement() != null)
                            functionInfo.ControlStructures.Add(("for", statementCtx.Start.Line));
                    }

                    // Detectează recursivitatea
                    functionInfo.IsRecursive = funcCtx.block().GetText().Contains(functionName);

                    functions.Add(functionInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare la obtinerea functiilor: {ex.Message}");
        }

        return functions;
    }
    static void SaveFunctionsToFile(List<FunctionInfo> functions, string outputFile)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                foreach (var func in functions)
                {
                    writer.WriteLine($"Functie: {func.Name}");
                    writer.WriteLine($"- Tip retur: {func.ReturnType}");
                    writer.WriteLine($"- Este main: {func.IsMain}");
                    writer.WriteLine($"- Este recursiva: {func.IsRecursive}");
                    writer.WriteLine("- Parametri:");
                    foreach (var param in func.Parameters)
                    {
                        writer.WriteLine($"  - {param.Type} {param.Name}");
                    }
                    writer.WriteLine("- Variabile locale:");
                    foreach (var localVar in func.LocalVariables)
                    {
                        writer.WriteLine($"  - {localVar.Type} {localVar.Name} = {localVar.Value}");
                    }
                    writer.WriteLine("- Structuri de control:");
                    foreach (var ctrl in func.ControlStructures)
                    {
                        writer.WriteLine($"  - {ctrl.Structure}, linia {ctrl.Line}");
                    }
                    writer.WriteLine();
                }
            }
            Console.WriteLine("Functiile au fost salvate cu succes in fisier.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Eroare la salvarea functiilor în fisier: {ex.Message}");
        }
    }
}




