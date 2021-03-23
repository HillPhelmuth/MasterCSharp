if (!require.getConfig().paths.vs)  // for lte v1.2.0
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
            
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: 'http://localhost:7071/api/CompleteCode',
                    data: JSON.stringify(obj),
                    type: 'POST',
                    traditional: true,
                    contentType: 'application/json',
                    success: function (data) {
                        var availableResolvers = [];
                        if (data && data.items) {
                            for (var i = 0; i < data.items.length; i++) {
                                if (data.items[i].properties.symbolName) {
                                    var ob = {
                                        label: data.items[i].properties.symbolName,
                                        insertText: data.items[i].properties.symbolName,
                                        kind: data.items[i].properties.symbolKind,// monaco.languages.CompletionItemKind.Property,
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
                            console.log("Completions from function: " + JSON.stringify(availableResolvers));
                            var returnObj = {
                                suggestions: availableResolvers
                            }
                            resolve(returnObj);
                        }


                    },
                    error: function (error) {
                        console.log(error)
                    },
                })
            })
            
        }
    };
}
