function __awaiter(thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
}

function __web_atoms_create_promise() {
    let r;
    let e;
    var promise = new Promise((resolve, reject) => {
        r = resolve;
        e = reject;
    });
    return { r, e, promise };
}

global.assemblyCache = {};
global.typeCache = {};

// lets bind clr.resolve to give assembly...
global.clr.resolve = function (name) {
    return global.assemblyCache[name] ||= new Proxy({}, {
        get(n) {
            const fullName = `${n}, ${name}`;
            return global.typeCache[fullName] ||= global.clr.resolveType(fullName);
        }
    });
}
