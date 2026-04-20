(function () {
    Module['flavor_fs_op'] = Module['flavor_fs_op'] || {
        stat(path) {
            if (path == 'StreamingAssets/ContentArchives' ||
                path == 'StreamingAssets/EntityScenes'
            ) {
                return 0x4000
            }
            var xhr = new XMLHttpRequest();
            xhr.open('HEAD', path, false);
            try {
                xhr.send(null);
            } catch (e) {
                return 0;
            }

            if (!((xhr.status >= 200 && xhr.status < 300) || xhr.status === 0)) {
                return 0;
            }

            return 0x8000
        },
        read(path) {
            var xhr = new XMLHttpRequest();
            xhr.open('GET', path, false);
            xhr.overrideMimeType('text/plain; charset=x-user-defined');
            xhr.send();

            var text = xhr.responseText || '';
            var bytes = new Uint8Array(text.length);
            for (var i = 0; i < text.length; i++) {
                bytes[i] = text.charCodeAt(i) & 0xff;
            }

            return bytes;
        }
    };

    Module['preRun'].push(function () {
        var flavor_fs = {
            ops_table: null,

            mount: function (mount) {
                return flavor_fs.createNode(null, '/', 16384 | 511, 0, 'StreamingAssets');
            },

            createNode: function (parent, name, mode, dev, realUrl) {
                if (!flavor_fs.ops_table) {
                    flavor_fs.ops_table = {
                        dir: {
                            node: {
                                getattr: flavor_fs.node_ops.getattr,
                                lookup: flavor_fs.node_ops.lookup,
                                setattr: flavor_fs.node_ops.setattr,
                                mknod: flavor_fs.node_ops.mknod,
                                symlink: flavor_fs.node_ops.symlink
                            },
                            stream: {
                                llseek: flavor_fs.stream_ops.llseek
                            }
                        },
                        file: {
                            node: {
                                getattr: flavor_fs.node_ops.getattr,
                                setattr: flavor_fs.node_ops.setattr
                            },
                            stream: {
                                open: flavor_fs.stream_ops.open,
                                close: flavor_fs.stream_ops.close,
                                read: flavor_fs.stream_ops.read,
                                write: flavor_fs.stream_ops.write,
                                llseek: flavor_fs.stream_ops.llseek
                            }
                        },
                        link: {
                            node: {
                                getattr: flavor_fs.node_ops.getattr,
                                setattr: flavor_fs.node_ops.setattr,
                                readlink: flavor_fs.node_ops.readlink
                            },
                            stream: {}
                        }
                    };
                }

                var node = FS.createNode(parent, name, mode, dev);
                node.realUrl = realUrl;
                node.timestamp = Date.now();

                if (FS.isDir(node.mode)) {
                    node.node_ops = flavor_fs.ops_table.dir.node;
                    node.stream_ops = flavor_fs.ops_table.dir.stream;
                } else if (FS.isFile(node.mode)) {
                    node.node_ops = flavor_fs.ops_table.file.node;
                    node.stream_ops = flavor_fs.ops_table.file.stream;
                    node.usedBytes = 0;
                    node.fileSize = 0;
                } else if (FS.isLink(node.mode)) {
                    node.node_ops = flavor_fs.ops_table.link.node;
                    node.stream_ops = flavor_fs.ops_table.link.stream;
                    node.link = null;
                }

                return node;
            },

            joinUrl: function (base, name) {
                if (!base)
                    return name;
                return base.replace(/\/$/, '') + '/' + name.replace(/^\//, '');
            },

            node_ops: {
                getattr: function (node) {
                    var isDir = FS.isDir(node.mode);
                    var isLink = FS.isLink(node.mode);
                    var size = isDir ? 4096 : (isLink ? ((node.link || '').length) : (node.fileSize || 0));
                    var ts = new Date(node.timestamp);

                    return {
                        dev: 1,
                        ino: node.id,
                        mode: node.mode,
                        nlink: 1,
                        uid: 0,
                        gid: 0,
                        rdev: 0,
                        size: size,
                        atime: ts,
                        mtime: ts,
                        ctime: ts,
                        blksize: 4096,
                        blocks: Math.ceil(size / 4096)
                    };
                },

                setattr: function (node, attr) { },

                lookup: function (parent, name) {
                    var childUrl = flavor_fs.joinUrl(parent.realUrl, name);
                    var mode = Module['flavor_fs_op'].stat(childUrl);
                    if (mode == 0) {
                        throw new FS.ErrnoError(44);
                    }
                    return flavor_fs.createNode(parent, name, mode | 511, 0, childUrl);
                },

                mknod: function (parent, name, mode, dev) {
                    var childUrl = flavor_fs.joinUrl(parent.realUrl, name);
                    return flavor_fs.createNode(parent, name, mode, dev, childUrl, true);
                },

                symlink: function (parent, newname, oldpath) {
                    var childUrl = flavor_fs.joinUrl(parent.realUrl, newname);
                    var node = flavor_fs.createNode(parent, newname, 511 | 40960, 0, childUrl);
                    node.link = oldpath;
                    return node;
                },

                unlink: function (parent, name) {
                    delete parent.contents[name];
                    parent.timestamp = Date.now()
                },

                readlink: function (node) {
                    if (!FS.isLink(node.mode)) {
                        throw new FS.ErrnoError(28);
                    }
                    return node.link;
                }
            },

            stream_ops: {
                open: function (stream) {
                    stream.position = 0;
                },

                close: function (stream) {// no-op
                },

                read: function (stream, buffer, offset, length, position) {
                    var node = stream.node;
                    if (!node.contents) {
                        var bytes = Module['flavor_fs_op'].read(node.realUrl);

                        node.contents = bytes;
                        node.usedBytes = bytes.length;
                        node.fileSize = bytes.length;
                    }

                    if (position >= node.usedBytes)
                        return 0;

                    var size = Math.min(length, node.usedBytes - position);
                    if (size <= 0)
                        return 0;

                    buffer.set(node.contents.subarray(position, position + size), offset);
                    return size;
                },

                write: function (stream, buffer, offset, length, position) {
                    return 0;
                },

                llseek: function (stream, offset, whence) {
                    var position = offset;

                    if (whence === 1) {
                        position += stream.position;
                    } else if (whence === 2) {
                        position += stream.node.usedBytes || stream.node.fileSize || 0;
                    }

                    if (position < 0) {
                        throw new FS.ErrnoError(28);
                    }

                    return position;
                }
            }
        };
        var mountpoint = typeof wx !== "undefined" ? 'StreamingAssets' : 'vfs_streamingassets';
        FS.mkdir(mountpoint);
        FS.mount(flavor_fs, {}, mountpoint);
    });
}
)();