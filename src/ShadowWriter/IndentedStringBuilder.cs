using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ShadowWriter;

public sealed class IndentedStringBuilder {
    private readonly StringBuilder builder = new();
    private readonly string indentText;
    private int indentLevel;

    private string currentIndentation = "";
    private bool beginOfLine = true;

    public IndentedStringBuilder(string indentText, int indentLevel) {
        this.indentText = indentText;
        this.indentLevel = indentLevel;

        this.UpdateIndentation();
    }

    private void UpdateIndentation() {
        this.currentIndentation = string.Concat(Enumerable.Repeat(this.indentText, this.indentLevel));
    }

    public IndentedStringBuilder Append(string text) {
        if (text.Length == 0) return this;

        if (this.beginOfLine) {
            this.builder.Append(this.currentIndentation);
        }

        this.builder.Append(text);
        this.beginOfLine = false;

        return this;
    }

    private static IEnumerable<string> ToLines(string input) {
        using var stringReader = new StringReader(input);

        while (stringReader.ReadLine() is { } line) {
            yield return line;
        }
    }

    public IndentedStringBuilder AppendLine(string line) {
        var lines = ToLines(line);

        foreach (var l in lines) {
            this.builder.AppendLine($"{this.currentIndentation}{l}");
        }

        this.beginOfLine = true;

        return this;
    }

    public IndentedStringBuilder AppendLine() {
        this.builder.AppendLine();
        this.beginOfLine = true;
        return this;
    }

    public IDisposable BeginBlock() {
        this.indentLevel++;
        this.UpdateIndentation();

        return new ActionDisposable(this);
    }

    public override string ToString() {
        return this.builder.ToString();
    }

    private sealed class ActionDisposable : IDisposable {
        private readonly IndentedStringBuilder parent;

        public ActionDisposable(IndentedStringBuilder parent) {
            this.parent = parent;
        }

        public void Dispose() {
            this.parent.indentLevel--;
            this.parent.UpdateIndentation();
        }
    }
}