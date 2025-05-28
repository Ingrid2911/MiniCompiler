# Mini Language Compiler in C# using ANTLR

This project is a **mini compiler** developed in **C#** using **ANTLR** for lexical and syntactic analysis.  
It was created as a **pair programming project** by two students.

---

## 👥 Project Type

**Collaborative project (pair programming)** — implemented and documented by two authors.

---

## 📌 Features

- **Lexical analysis** using ANTLR-generated lexer
- **Syntax analysis** using a defined grammar (`miniLang.g4`)
- **Semantic validation** (e.g. undeclared variables, recursion detection)
- **Custom mini-language support**, including:
  - Variable and function declarations
  - Arithmetic, relational, and logical expressions
  - `if`, `else`, `for`, `while` control structures
  - Return statements
  - Comments (single-line and multi-line)

---

## 🧠 Language Grammar (ANTLR)

A custom grammar (`miniLang.g4`) was defined to describe the structure of the mini-language, supporting:

- Data types: `int`, `float`, `double`, `string`, `void`
- Operators: arithmetic, relational, logical, assignment, increment/decrement
- Statements: blocks, selections, iterations, jumps
- Function and variable declarations
- Function calls and expressions

---

## 📥 Input

The compiler reads a `.txt` file containing a program written in the mini-language.

Example input:
```c
double globalVariable = 15.67;
int main() {
    for (int i = 0; i < 10; i++) {
        string myString = "";
        if (i > 5) {
            // do something
        }
    }
}
```
## 📤 Output Files

The program generates **three output files** after analyzing the source file:

### 1. `output_tokens.txt`

Contains a list of tokens extracted during lexical analysis:
```c
< DOUBLE, double, 1 >
< IDENTIFIER, globalVariable, 1 >
< ASSIGNMENT_OPERATOR, =, 1 >
< NUMBER, 15.67, 1 >
< SEMICOLON, ;, 1 >
...
```

---

### 2. `functionDetails.txt`

Includes details for each function:

```c
Function: main
Return type: int
Is main: True
Is recursive: False
Parameters:
Local variables:
string myString = ""
float myFloat = divideIntegers(myThirdVariable, myFirstVariable)
Control structures:
for, line 4
if, line 9
```

---

### 3. `globalVariablesFile.txt`

Includes all global variables and their initial values:
```c
< double, globalVariable, 15.67 >
```
