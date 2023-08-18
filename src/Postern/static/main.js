var editor;

require.config({ paths: { vs: '_postern/static/vs' } });

require(['vs/editor/editor.main'], function () {
    editor = monaco.editor.create(document.getElementById('editor'), {
        value: "Console.WriteLine(\"Hello World\");",
        language: 'csharp'
    });
});

function executeCode(){
    const code = editor.getValue();

    return fetch("/_postern/execute", {
        method: "POST",
        headers: {
            'content-type': 'application/json'
        },
        body: JSON.stringify({
            code
        })
    })
}