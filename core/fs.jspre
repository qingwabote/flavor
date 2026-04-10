(function () {
    Module['flavor_fs_op'] = Module['flavor_fs_op'] || {
        stat(path) {
            var xhr = new XMLHttpRequest();
            xhr.open('HEAD', path, false);
            try {
                xhr.send(null);
            } catch (e) {
                return false;
            }

            if (!((xhr.status >= 200 && xhr.status < 300) || xhr.status === 0)) {
                return false;
            }

            return true
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
                                mknod: flavor_fs.node_ops.mknod
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
                }

                return node;
            },

            joinUrl: function (base, name) {
                if (!base)
                    return name;
                return base.replace(/\/$/, '') + '/' + name.replace(/^\//, '');
            },

            // 尝试把 path 当作“目录”处理。
            // 纯 HTTP 下其实没有可靠目录 stat，这里只把根节点当目录。
            canTreatAsDir: function (parent, name, url) {
                if (!parent)
                    return true;
                if (parent === FS.root)
                    return true;
                if (name === 'EntityScenes' || name === 'ContentArchives' || name === 'aa')
                    return true;
                return false;
            },

            node_ops: {
                getattr: function (node) {
                    var isDir = FS.isDir(node.mode);
                    var size = isDir ? 4096 : (node.fileSize || 0);
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

                    // 对固定已知目录名，直接当目录节点建立
                    if (flavor_fs.canTreatAsDir(parent, name, childUrl)) {
                        return flavor_fs.createNode(parent, name, 16384 | 511, 0, childUrl);
                    }

                    // 否则按文件探测
                    var exist = Module['flavor_fs_op'].stat(childUrl);
                    if (!exist) {
                        throw new FS.ErrnoError(44);
                    }

                    return flavor_fs.createNode(parent, name, 32768 | 511, 0, childUrl);
                },

                mknod: function (parent, name, mode, dev) {
                    var childUrl = flavor_fs.joinUrl(parent.realUrl, name);
                    return flavor_fs.createNode(parent, name, mode, dev, childUrl, true);
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
    })
}
)();