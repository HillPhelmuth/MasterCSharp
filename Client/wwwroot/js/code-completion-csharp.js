﻿if (!require.getConfig().paths.vs)  // for lte v1.2.0
    require.config({ paths: { 'vs': '_content/BlazorMonaco/lib/monaco-editor/min/vs' } });

require(["vs/editor/editor.main"], function () {
    monaco.languages.registerCompletionItemProvider('csharp', getcsharpCompletionProvider(monaco));
});
function getcsharpCompletionProvider(monaco) {
    return {
        triggerCharacters: ['.', '(', ',', ')'],
        provideCompletionItems: function (model, position) {

            var textUntilPosition = model.getValueInRange({ startLineNumber: 1, startColumn: 1, endLineNumber: position.lineNumber, endColumn: position.column });
            var cursor = textUntilPosition.length;
            var obj = { SourceCode: model.getValue(), lineNumberOffsetFromTemplate: cursor };
            //var funcUrl = "https://codecompletionfunction.azurewebsites.net/api/CompleteCode";
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: '/api/CompleteCode', //add function url port on local
                    data: JSON.stringify(obj),
                    type: 'POST',
                    traditional: true,
                    contentType: 'application/json',
                    success: function (data) {
                        var availableResolvers = [];
                        if (data && data.items) {
                            for (var i = 0; i < data.items.length; i++) {
                                if (data.items[i].properties.SymbolName) {
                                    var ob = {
                                        label: data.items[i].properties.SymbolName,
                                        insertText: data.items[i].properties.SymbolName,
                                        kind: convertSymbolKindToMonacoEnum(data.items[i].properties.SymbolKind),// monaco.languages.CompletionItemKind.Property,
                                        detail: data.items[i].tags[0],
                                        documentation: data.items[i].tags[1]
                                    };
                                    availableResolvers.push(ob);
                                } else {
                                    var obj = {
                                        label: data.items[i].displayText,
                                        insertText: data.items[i].displayText,
                                        kind: monaco.languages.CompletionItemKind.Property,
                                        detail: data.items[i].displayText,

                                    };
                                    availableResolvers.push(obj);
                                }


                            }
                            console.log(`Completions from function: ${JSON.stringify(availableResolvers)}`);
                            var returnObj = {
                                suggestions: availableResolvers
                            };
                            resolve(returnObj);
                        }


                    },
                    error: function (error) {
                        console.log(error);
                    },
                });
            });

        }
    };
}
function convertSymbolKindToMonacoEnum(kind) {
    switch (kind) {
    case "9": return monaco.languages.CompletionItemKind.Method;
    case "15": return monaco.languages.CompletionItemKind.Property;
    case "11": return monaco.languages.CompletionItemKind.Class;
    case "6": return monaco.languages.CompletionItemKind.Field;
    case "8": return monaco.languages.CompletionItemKind.Variable;
    }
}