var lib = {
  $flavor_loadSubpackageCallback: null,

  flavor_registerLoadSubpackageCallback: function (callback) {
    flavor_loadSubpackageCallback = callback;
  },

  flavor_loadSubpackage: function (handleId, namePtr) {
    var errMsg = "";
    var size = lengthBytesUTF8(errMsg) + 1;
    var errMsgPtr = _malloc(size);
    stringToUTF8(errMsg, errMsgPtr, size);
    getWasmTableEntry(flavor_loadSubpackageCallback)(handleId, errMsgPtr);
    _free(errMsgPtr);
  },

  flavor_preDownloadSubpackage: function (namePtr) { },
};

autoAddDeps(lib, "$flavor_loadSubpackageCallback");
mergeInto(LibraryManager.library, lib);
