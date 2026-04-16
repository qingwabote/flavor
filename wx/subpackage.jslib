var lib = {
  $flavor_loadSubpackageCallback: null,

  flavor_loadSubpackage: function (handleId, namePtr) {
    function emit(errMsg) {
      var size = lengthBytesUTF8(errMsg) + 1;
      var errMsgPtr = _malloc(size);
      stringToUTF8(errMsg, errMsgPtr, size);
      getWasmTableEntry(flavor_loadSubpackageCallback)(handleId, errMsgPtr);
      _free(errMsgPtr);
    }

    if (typeof wx == "undefined") {
      emit("");
      return;
    }

    wx.loadSubpackage({
      name: UTF8ToString(namePtr),
      success: function () {
        emit("");
      },
      fail: function (res) {
        emit(res.errMsg);
      },
    });
  },

  flavor_registerLoadSubpackageCallback: function (callback) {
    flavor_loadSubpackageCallback = callback;
  },
};

autoAddDeps(lib, "$flavor_loadSubpackageCallback");
mergeInto(LibraryManager.library, lib);
