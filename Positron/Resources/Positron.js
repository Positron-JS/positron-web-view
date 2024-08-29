(function () {
    let id = 1;

    const map = new Map();

    function runCode($rid$, $code$, $args$) {
        return function () {
            try {
                let a = $args$;
                let result = ($code$).apply({ clr, evalInPage }, a);
                if (result && result.then) {
                    result.then((r) => {
                        evalInPage(`window.positron.on($rid$, ${serialize(r) || 1})`);
                    }, (e) => {
                        evalInPage(`window.positron.on($rid$, void 0, ${serialize(e.stack || e)})`);
                    });
                } else {
                    setTimeout(() =>
                        evalInPage(`window.positron.on($rid$, ${serialize(result) || 1})`),
                        1);
                }
            } catch (error) {
                setTimeout(() =>
                    evalInPage(`window.positron.on($rid$, void 0, ${serialize(error.stack || error)})`),
                    1);
            }
        }
    }

    const scriptTemplate = runCode().toString();

    window.positron = {

        on(rid, result, error) {
            try {
                const { resolve, reject } = map.get(rid);
                if (error) {
                    reject(error);
                    return;
                }
                resolve(result);
            } catch (error) {
                console.error(error);
            } finally {
                map.delete(rid);
            }
        },

        run(script, ...args) {
            return new Promise((resolve, reject) => {
                let rid = id++;
                map.set(rid, { resolve, reject });
                const inScript = script.toString();
                let sendScript = scriptTemplate;
                sendScript = sendScript
                    .replace(/\$rid\$/g, rid)
                    .replace("$args$", JSON.stringify(args));
                sendScript = sendScript
                    .replace("$code$", inScript);
                if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.mainScript) {
                    window.webkit.messageHandlers.mainScript.postMessage(`(${sendScript})();`);
                }
                if (typeof androidBridge !== "undefined") {
                    androidBridge.invokeAction(`(${sendScript})();`);
                }
            });
        }
    };
}());