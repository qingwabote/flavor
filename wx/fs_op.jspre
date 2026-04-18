if (typeof wx !== 'undefined') {
    Module['flavor_fs_op'] = {
        stat(path) {
            var fs = WXFS.fs || wx.getFileSystemManager();
            try {
                fs.accessSync(path);
            } catch (e) {
                try {
                    fs.accessSync(path + '.txt');
                } catch (e) {
                    return 0;
                }
                return 0x8000;
            }
            return 0x4000;
        },
        read(path) {
            return new Uint8Array(WXFS.fs.readFileSync(path + '.txt'));
        }
    }
}