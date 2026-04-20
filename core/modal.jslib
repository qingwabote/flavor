var lib = {
    $flavor_showModalCallback: null,

    flavor_showModal: function (handleId, titlePtr, contentPtr, showCancel) {
        wx.showModal({
            title: UTF8ToString(titlePtr),
            content: UTF8ToString(contentPtr),
            showCancel: showCancel != 0,
            success: function (res) {
                if (res.confirm) {
                    getWasmTableEntry(flavor_showModalCallback)(handleId, 1);
                } else {
                    getWasmTableEntry(flavor_showModalCallback)(handleId, 0);
                }
            }
        });
    },

    flavor_registerShowModalCallback: function (callback) {
        flavor_showModalCallback = callback;
    },
};

autoAddDeps(lib, "$flavor_showModalCallback");
mergeInto(LibraryManager.library, lib);
