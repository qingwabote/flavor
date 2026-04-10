if (typeof wx !== 'undefined') {
    Module['flavor_fs_op'] = {
        stat(path) {
            try {
                WXFS.fs.accessSync(path + '.txt');
            } catch (e) {
                return false;
            }
            return true;
        },
        read(path) {
            return new Uint8Array(WXFS.fs.readFileSync(path + '.txt'));
        }
    }
}